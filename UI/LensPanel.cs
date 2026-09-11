using System.Collections;
using OttoLens.Model;
using OttoLens.Readers;
using TMPro;
using UnityEngine.UI;

namespace OttoLens.UI;

/// The ledger panel from panel-design.md. Built once under the Hud root, pooled rows, two
/// refresh paths: a rebuild when the target or its content shape changes, a cheap tick at
/// RefreshHz that writes only values. Hidden whenever there is no report.
internal sealed class LensPanel : MonoBehaviour
{
    // Design section 2 metrics, canvas units at 1080p.
    private const int ItemRowsPerBlock = 16;
    private const int SecondaryRows = 2;
    private const float ItemRowHeight = 32f;
    private const float ItemRowStride = 33f;
    private const float MeterWidth = 120f;
    private const float MeterFillWidth = 118f;

    private static readonly Dictionary<string, Sprite?> SpriteCache = new(StringComparer.Ordinal);
    private static readonly string[] PlateNames = { "woodpanel_trophys", "woodpanel_settings", "panel_bkg_128", "darken_blob" };
    private static readonly string[] RuleNames = { "panel_separator", "divider" };
    private static readonly string[] TileNames = { "item_background", "inventory_slot" };
    private static bool _probed;
    private static bool _rebuildRequested;
    private static bool _parentWarned;

    internal static LensPanel? Instance;

    private RectTransform _root = null!;
    private CanvasGroup _group = null!;
    private Image _plate = null!;
    private TextSlot _title = null!;
    private TextSlot _headline = null!;
    private GameObject _statusBlock = null!;
    private StatusRow[] _statusRows = null!;
    private ItemBlockView[] _blocks = null!;
    private GameObject _overflowBlock = null!;
    private TextSlot _overflow = null!;
    private TextSlot _footer = null!;

    private Component? _target;
    private ILensReader? _reader;
    private GameObject? _hover;
    private string? _signature;
    private bool _visible;
    private Coroutine? _tick;
    private Coroutine? _fade;
    private int _tickHz;
    private WaitForSeconds? _tickWait;
    private float _nextRetry;
    // A reader that throws for one target throws for it every frame. Remember the pair that
    // failed and skip Read until the target or the reader changes.
    private Component? _failedTarget;
    private ILensReader? _failedReader;

    private static readonly HashSet<(Type Reader, Type Exception)> LoggedReaderFailures = new();

    // Static entry points

    internal static LensPanel? Get()
    {
        if (Instance != null)
        {
            return Instance;
        }

        Hud? hud = Hud.instance;
        if (hud == null || hud.m_rootObject == null || hud.m_hoverName == null)
        {
            return null;
        }

        Transform parent = hud.m_rootObject.transform;
        if (parent == null)
        {
            if (!_parentWarned)
            {
                _parentWarned = true;
                OttoLensPlugin.Log.LogWarning("Hud root has no transform; the lens panel is disabled for this session.");
            }

            return null;
        }

        ProbeSprites();
        var go = new GameObject("OttoLensPanel", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        LensPanel panel = go.AddComponent<LensPanel>();
        panel.Build(hud.m_hoverName.font);
        Instance = panel;
        return panel;
    }

    internal static void RequestRebuild() => _rebuildRequested = true;

    /// Design section 7: a master switch flip destroys the root and releases nothing else.
    /// The probed sprites are vanilla-owned and outlive the panel, so keeping the cache means
    /// a toggle back on rebuilds the tree without re-running the whole-heap sprite scan.
    internal static void Destroy()
    {
        LensPanel? panel = Instance;
        Instance = null;
        if (panel != null)
        {
            // Rows die with the root; no other references are held past this point.
            UnityEngine.Object.Destroy(panel.gameObject);
        }
    }

    /// World unload: the probed sprites die with the scene, so the cache goes with them.
    internal static void Release()
    {
        Destroy();
        SpriteCache.Clear();
        _probed = false;
    }

    // Per frame entry. Cheap on a still hover: one reference compare.

    internal void SetTarget(Component? target, ILensReader? reader, GameObject? hover)
    {
        if (target == null || reader == null || hover == null)
        {
            if (_visible)
            {
                BeginHide();
            }

            _target = null;
            _reader = null;
            _hover = null;
            _failedTarget = null;
            _failedReader = null;
            return;
        }

        if (ReferenceEquals(target, _target))
        {
            bool hoverChanged = !ReferenceEquals(hover, _hover);
            _hover = hover;
            _reader = reader;
            // Hidden for lack of content: retry at the tick rate, not every frame.
            if (_visible || Time.unscaledTime < _nextRetry)
            {
                // Hover collider changed on the same target (e.g. different MineRock5 hit area):
                // update values immediately so the next frame shows the right health rather than
                // waiting for the next tick.
                if (hoverChanged && _visible)
                {
                    Refresh();
                }
                return;
            }

            _nextRetry = Time.unscaledTime + 1f / Mathf.Clamp(OttoLensPlugin.RefreshHz.Value, 1, 10);
            Refresh();
            return;
        }

        _target = target;
        _reader = reader;
        _hover = hover;
        _signature = null;
        _nextRetry = 0f;
        _failedTarget = null;
        _failedReader = null;
        Refresh();
    }

    // Refresh paths

    private void Refresh()
    {
        if (_target == null || _reader == null || _hover == null)
        {
            BeginHide();
            return;
        }

        // Already known to throw for this target: SetTarget retries at RefreshHz for as long as
        // the crosshair rests here, and re-entering the throw would log on every retry.
        if (ReferenceEquals(_target, _failedTarget) && ReferenceEquals(_reader, _failedReader))
        {
            BeginHide();
            return;
        }

        ILensReader reader = _reader;
        LensReport? report;
        try
        {
            report = reader.Read(_target, _hover);
        }
        catch (Exception ex)
        {
            _failedTarget = _target;
            _failedReader = reader;
            LogReaderFailure(reader, ex);
            report = null;
        }

        if (report == null || !report.HasContent)
        {
            BeginHide();
            return;
        }

        string signature = report.Signature();
        if (_rebuildRequested || _signature == null || !string.Equals(signature, _signature, StringComparison.Ordinal))
        {
            _rebuildRequested = false;
            _signature = signature;
            Rebuild(report);
        }
        else
        {
            Tick(report);
        }

        if (!_visible)
        {
            BeginShow();
        }
    }

    private void Rebuild(LensReport report)
    {
        ApplyGeometry();

        bool showNames = OttoLensPlugin.ShowNames.Value;
        _title.SetText(LensFormat.Upper(report.Title));
        _headline.SetText(report.Headline ?? "");
        _headline.SetColor(Palette.Get(report.HeadlineColor));
        _headline.Go.SetActive(!string.IsNullOrEmpty(report.Headline));

        int statusCount = 0;
        statusCount += _statusRows[0].Apply(report.PrimaryMeter, true) ? 1 : 0;
        statusCount += _statusRows[1].Apply(report.Secondary1, false) ? 1 : 0;
        statusCount += _statusRows[2].Apply(report.Secondary2, false) ? 1 : 0;
        _statusBlock.SetActive(statusCount > 0);

        int blocks = (report.Block0 != null && report.Block0.Items.Count > 0 ? 1 : 0)
                     + (report.Block1 != null && report.Block1.Items.Count > 0 ? 1 : 0);
        int totalTypes = (report.Block0?.Items.Count ?? 0) + (report.Block1?.Items.Count ?? 0);
        int rowBudget = RowBudget(statusCount, blocks, totalTypes, out bool overflowPossible, out int rowsFit);

        // Design section 7: maxRows is a per-block cap, so block 0 gets the whole budget.
        // Block 1 then gets what is left of the screen budget, never less than one row: an
        // even split let a one-row output block cost the contents block half the screen.
        int hidden = 0;
        int used = _blocks[0].Apply(report.Block0, rowBudget, showNames, ref hidden);
        int rest = Mathf.Max(1, Mathf.Min(rowBudget, rowsFit - used));
        _blocks[1].Apply(report.Block1, rest, showNames, ref hidden);

        bool showOverflow = hidden > 0 && overflowPossible;
        bool showFooter = !string.IsNullOrEmpty(report.Footer);
        _overflow.SetText(showOverflow ? $"and {hidden} more" : "");
        _overflow.Go.SetActive(showOverflow);
        _footer.SetText(report.Footer ?? "");
        _footer.Go.SetActive(showFooter);
        _overflowBlock.SetActive(showOverflow || showFooter);

        LayoutRebuilder.ForceRebuildLayoutImmediate(_root);
    }

    private void Tick(LensReport report)
    {
        _headline.SetText(report.Headline ?? "");
        _headline.SetColor(Palette.Get(report.HeadlineColor));
        _statusRows[0].TickValues(report.PrimaryMeter, true);
        _statusRows[1].TickValues(report.Secondary1, false);
        _statusRows[2].TickValues(report.Secondary2, false);
        _blocks[0].Tick(report.Block0);
        _blocks[1].Tick(report.Block1);
        _footer.SetText(report.Footer ?? "");
    }

    /// Design section 2: rows shown per block = min(maxRows, rowsFit); the overflow line is
    /// dropped when the types count is exactly maxRows + 1, because the line costs a row
    /// anyway. maxRows is a per-block cap and rowsFit is the screen cap shared by both
    /// blocks, so the caller hands out rowsFit by demand rather than splitting it evenly.
    private int RowBudget(int statusCount, int blocks, int totalTypes, out bool overflowPossible, out int rowsFit)
    {
        int maxRows = Mathf.Clamp(OttoLensPlugin.MaxRows.Value, 1, ItemRowsPerBlock);
        float fixedHeight = 16f + 22f + 2f + 2f + 2f;
        if (statusCount > 0)
        {
            fixedHeight += 4f + 22f + 18f * (statusCount - 1) + 2f * statusCount + 2f;
        }

        fixedHeight += blocks * 5f;
        fixedHeight += 2f + 18f + 18f + 4f;

        float canvasHeight = 1080f;
        if (_root.parent is RectTransform parentRect && parentRect.rect.height > 0f)
        {
            canvasHeight = parentRect.rect.height;
        }

        float offsetY = Mathf.Min(OttoLensPlugin.OffsetY.Value, -16);
        float scale = Mathf.Clamp(OttoLensPlugin.GuiScale.Value, 0.75f, 1.6f);
        rowsFit = Mathf.FloorToInt((offsetY + canvasHeight / 2f - 24f - fixedHeight * scale) / (ItemRowStride * scale));
        int shown = Mathf.Max(1, Mathf.Min(maxRows, rowsFit));

        // Skip the overflow line only when the extra type actually fits without hitting
        // the per-block row cap. When totalTypes would exceed ItemRowsPerBlock, a block
        // silently clamps to 16 rows, hiding one item with no footer; fall through so
        // overflowPossible stays true and the "and N more" line is shown correctly.
        if (totalTypes == maxRows + 1 && rowsFit >= totalTypes && totalTypes <= ItemRowsPerBlock)
        {
            overflowPossible = false;
            return totalTypes;
        }

        overflowPossible = true;
        return shown;
    }

    private void ApplyGeometry()
    {
        float offsetX = Mathf.Max(OttoLensPlugin.OffsetX.Value, 96);
        float offsetY = Mathf.Min(OttoLensPlugin.OffsetY.Value, -16);
        float width = Mathf.Clamp(OttoLensPlugin.PanelWidth.Value, 240, 420);
        float scale = Mathf.Clamp(OttoLensPlugin.GuiScale.Value, 0.75f, 1.6f);

        // Rebuild only: pushes right past a wide vanilla hover line, never left.
        if (OttoLensPlugin.AvoidHoverText.Value && Hud.instance != null && Hud.instance.m_hoverName != null)
        {
            float hoverWidth = Hud.instance.m_hoverName.preferredWidth;
            float pushed = hoverWidth / 2f + 16f;
            if (pushed > offsetX)
            {
                offsetX = pushed;
            }
        }

        _root.anchoredPosition = new Vector2(offsetX, offsetY);
        _root.sizeDelta = new Vector2(width, _root.sizeDelta.y);
        _root.localScale = new Vector3(scale, scale, 1f);

        Color plate = Palette.Panel;
        plate.a = Mathf.Clamp(OttoLensPlugin.BackdropAlpha.Value, 0.5f, 1f);
        if (_plate.color != plate)
        {
            _plate.color = plate;
        }
    }

    // Show, hide, tick, fade

    private void BeginShow()
    {
        _visible = true;
        gameObject.SetActive(true);
        StartTick();
        StartFade(1f, false);
    }

    private void BeginHide()
    {
        if (!_visible)
        {
            return;
        }

        _visible = false;
        _signature = null;
        StopTick();
        if (gameObject.activeSelf)
        {
            StartFade(0f, true);
        }
    }

    private void StartTick()
    {
        StopTick();
        int hz = Mathf.Clamp(OttoLensPlugin.RefreshHz.Value, 1, 10);
        if (_tickWait == null || hz != _tickHz)
        {
            _tickHz = hz;
            _tickWait = new WaitForSeconds(1f / hz);
        }

        _tick = StartCoroutine(TickLoop());
    }

    private void StopTick()
    {
        if (_tick != null)
        {
            StopCoroutine(_tick);
            _tick = null;
        }
    }

    private IEnumerator TickLoop()
    {
        while (true)
        {
            yield return _tickWait;
            // The target can be destroyed between ticks; Unity fake null catches that.
            if (_target == null)
            {
                BeginHide();
                yield break;
            }

            Refresh();
        }
    }

    private void StartFade(float to, bool deactivateAtEnd)
    {
        if (_fade != null)
        {
            StopCoroutine(_fade);
            _fade = null;
        }

        float seconds = Mathf.Clamp(OttoLensPlugin.FadeSeconds.Value, 0f, 0.5f);
        if (seconds <= 0f)
        {
            _group.alpha = to;
            if (deactivateAtEnd)
            {
                gameObject.SetActive(false);
            }

            return;
        }

        _fade = StartCoroutine(FadeTo(to, seconds, deactivateAtEnd));
    }

    private IEnumerator FadeTo(float to, float seconds, bool deactivateAtEnd)
    {
        float from = _group.alpha;
        float t = 0f;
        while (t < seconds)
        {
            t += Time.unscaledDeltaTime;
            _group.alpha = Mathf.Lerp(from, to, t / seconds);
            yield return null;
        }

        _group.alpha = to;
        _fade = null;
        if (deactivateAtEnd)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (ReferenceEquals(Instance, this))
        {
            Instance = null;
        }
    }

    // Build, once

    private void Build(TMP_FontAsset font)
    {
        _root = (RectTransform)transform;
        _root.anchorMin = new Vector2(0.5f, 0.5f);
        _root.anchorMax = new Vector2(0.5f, 0.5f);
        _root.pivot = new Vector2(0f, 1f);
        _root.sizeDelta = new Vector2(300f, 0f);

        Image outline = gameObject.AddComponent<Image>();
        outline.sprite = null;
        outline.color = Palette.Outline;
        outline.raycastTarget = false;

        _group = gameObject.AddComponent<CanvasGroup>();
        _group.alpha = 0f;
        _group.blocksRaycasts = false;
        _group.interactable = false;

        VerticalLayoutGroup vlg = gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(10, 10, 8, 8);
        vlg.spacing = 2f;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childAlignment = TextAnchor.UpperLeft;

        ContentSizeFitter fitter = gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        // Plate first so it draws under everything. ignoreLayout keeps it out of the column.
        RectTransform plateRect = NewRect("Plate", _root);
        plateRect.anchorMin = Vector2.zero;
        plateRect.anchorMax = Vector2.one;
        plateRect.offsetMin = new Vector2(1f, 1f);
        plateRect.offsetMax = new Vector2(-1f, -1f);
        _plate = AddImage(plateRect, Probe(PlateNames), Palette.Panel);
        plateRect.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;

        RectTransform edge = NewRect("GoldEdge", _root);
        edge.anchorMin = new Vector2(0f, 0f);
        edge.anchorMax = new Vector2(0f, 1f);
        edge.pivot = new Vector2(0f, 0.5f);
        edge.sizeDelta = new Vector2(3f, 0f);
        edge.anchoredPosition = Vector2.zero;
        AddImage(edge, null, Palette.EdgeGold);
        edge.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;

        RectTransform titleRow = NewRect("TitleRow", _root);
        SetPreferredHeight(titleRow, 22f);
        AddRowLayout(titleRow, 6f);
        _title = NewText("Title", titleRow, font, 17f, TextAlignmentOptions.MidlineLeft, Palette.TitleGold, ellipsis: true);
        SetFlexibleName(_title.Go);
        _headline = NewText("Headline", titleRow, font, 15f, TextAlignmentOptions.MidlineRight, Palette.TitleGold, ellipsis: false);
        LayoutElement headlineLe = _headline.Go.AddComponent<LayoutElement>();
        headlineLe.minWidth = 0f;

        RectTransform titleRule = NewRect("TitleRule", _root);
        SetPreferredHeight(titleRule, 2f);
        AddImage(titleRule, Probe(RuleNames), Palette.RuleGold);

        RectTransform statusBlock = NewRect("StatusBlock", _root);
        _statusBlock = statusBlock.gameObject;
        AddColumnLayout(statusBlock, 2f, 2, 2);
        _statusRows = new StatusRow[1 + SecondaryRows];
        for (int i = 0; i < _statusRows.Length; i++)
        {
            _statusRows[i] = new StatusRow(statusBlock, font, i == 0);
        }

        _blocks = new ItemBlockView[2];
        for (int b = 0; b < _blocks.Length; b++)
        {
            _blocks[b] = new ItemBlockView(_root, font, b);
        }

        RectTransform overflowBlock = NewRect("OverflowBlock", _root);
        _overflowBlock = overflowBlock.gameObject;
        AddColumnLayout(overflowBlock, 1f, 0, 0);
        RectTransform overflowRule = NewRect("BlockRule", overflowBlock);
        SetPreferredHeight(overflowRule, 2f);
        AddImage(overflowRule, Probe(RuleNames), Palette.RuleDim);
        _overflow = NewText("Overflow", overflowBlock, font, 14f, TextAlignmentOptions.MidlineLeft, Palette.Dim, ellipsis: true);
        _overflow.Tmp.fontStyle = FontStyles.Italic;
        SetPreferredHeight((RectTransform)_overflow.Go.transform, 18f);
        _footer = NewText("Footer", overflowBlock, font, 14f, TextAlignmentOptions.MidlineLeft, Palette.Dim, ellipsis: true);
        SetPreferredHeight((RectTransform)_footer.Go.transform, 18f);

        _statusBlock.SetActive(false);
        _overflowBlock.SetActive(false);
        gameObject.SetActive(false);
    }

    // Sprite probe with fallback: a miss changes the texture only, never the geometry.

    private static void ProbeSprites()
    {
        if (_probed)
        {
            return;
        }

        _probed = true;
        var wanted = new HashSet<string>(StringComparer.Ordinal);
        foreach (string n in PlateNames) wanted.Add(n);
        foreach (string n in RuleNames) wanted.Add(n);
        foreach (string n in TileNames) wanted.Add(n);

        try
        {
            Sprite[] all = Resources.FindObjectsOfTypeAll<Sprite>();
            for (int i = 0; i < all.Length; i++)
            {
                Sprite s = all[i];
                if (s != null && wanted.Contains(s.name) && !SpriteCache.ContainsKey(s.name))
                {
                    SpriteCache[s.name] = s;
                }
            }
        }
        catch (Exception ex)
        {
            OttoLensPlugin.Log.LogDebug($"Sprite probe failed: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private static Sprite? Probe(string[] names)
    {
        for (int i = 0; i < names.Length; i++)
        {
            if (SpriteCache.TryGetValue(names[i], out Sprite? s) && s != null)
            {
                return s;
            }
        }

        OttoLensPlugin.Log.LogDebug($"No vanilla sprite found for {string.Join(", ", names)}; drawing a plain quad.");
        return null;
    }

    // uGUI helpers

    private static RectTransform NewRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rect = (RectTransform)go.transform;
        rect.SetParent(parent, false);
        return rect;
    }

    private static Image AddImage(RectTransform rect, Sprite? sprite, Color color)
    {
        Image image = rect.gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.type = sprite != null && sprite.border != Vector4.zero ? Image.Type.Sliced : Image.Type.Simple;
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static void SetPreferredHeight(RectTransform rect, float height)
    {
        LayoutElement le = rect.gameObject.GetComponent<LayoutElement>() ?? rect.gameObject.AddComponent<LayoutElement>();
        le.preferredHeight = height;
    }

    private static HorizontalLayoutGroup AddRowLayout(RectTransform rect, float spacing)
    {
        HorizontalLayoutGroup hlg = rect.gameObject.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = spacing;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childAlignment = TextAnchor.MiddleLeft;
        return hlg;
    }

    private static VerticalLayoutGroup AddColumnLayout(RectTransform rect, float spacing, int padTop, int padBottom)
    {
        VerticalLayoutGroup vlg = rect.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(0, 0, padTop, padBottom);
        vlg.spacing = spacing;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childAlignment = TextAnchor.UpperLeft;
        return vlg;
    }

    private static void SetFlexibleName(GameObject go)
    {
        LayoutElement le = go.GetComponent<LayoutElement>() ?? go.AddComponent<LayoutElement>();
        le.minWidth = 0f;
        le.preferredWidth = 0f;
        le.flexibleWidth = 1f;
    }

    private static TextSlot NewText(string name, Transform parent, TMP_FontAsset font, float size, TextAlignmentOptions align, Color color, bool ellipsis)
    {
        RectTransform rect = NewRect(name, parent);
        // Add TMP while inactive: its Awake falls back to TMP_Settings' LiberationSans SDF,
        // which Valheim does not ship, and warns once per label unless the font is set first.
        rect.gameObject.SetActive(false);
        TextMeshProUGUI tmp = rect.gameObject.AddComponent<TextMeshProUGUI>();
        // Font only. The shared material belongs to the vanilla hover label.
        if (font != null)
        {
            tmp.font = font;
        }

        rect.gameObject.SetActive(true);

        tmp.fontSize = size;
        tmp.alignment = align;
        tmp.color = color;
        tmp.richText = false;
        tmp.raycastTarget = false;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.overflowMode = ellipsis ? TextOverflowModes.Ellipsis : TextOverflowModes.Overflow;
        tmp.text = "";
        return new TextSlot(tmp, color);
    }

    // Pooled pieces. Each keeps its last value and early-returns on equality.

    private sealed class TextSlot
    {
        public readonly TextMeshProUGUI Tmp;
        public readonly GameObject Go;
        private string _last = "";
        private Color _lastColor;

        public TextSlot(TextMeshProUGUI tmp, Color color)
        {
            Tmp = tmp;
            Go = tmp.gameObject;
            _lastColor = color;
        }

        public void SetText(string text)
        {
            if (string.Equals(text, _last, StringComparison.Ordinal))
            {
                return;
            }

            _last = text;
            Tmp.text = text;
        }

        public void SetColor(Color color)
        {
            if (color == _lastColor)
            {
                return;
            }

            _lastColor = color;
            Tmp.color = color;
        }
    }

    private sealed class StatusRow
    {
        private readonly GameObject _go;
        private readonly TextSlot _label;
        private readonly GameObject _meter;
        private readonly RectTransform _fill;
        private readonly Image _fillImage;
        private readonly TextSlot _value;
        private float _lastFill = -1f;
        private Color _lastFillColor;

        public StatusRow(Transform parent, TMP_FontAsset font, bool primary)
        {
            RectTransform row = NewRect(primary ? "StatusRow_0" : "StatusRow_n", parent);
            _go = row.gameObject;
            SetPreferredHeight(row, primary ? 22f : 18f);
            AddRowLayout(row, 6f);

            _label = NewText("Label", row, font, 15f, TextAlignmentOptions.MidlineLeft, Palette.Body, ellipsis: true);
            LayoutElement labelLe = _label.Go.AddComponent<LayoutElement>();
            labelLe.minWidth = 64f;
            labelLe.preferredWidth = 64f;

            RectTransform meter = NewRect("Meter", row);
            _meter = meter.gameObject;
            LayoutElement meterLe = meter.gameObject.AddComponent<LayoutElement>();
            meterLe.preferredWidth = MeterWidth;
            meterLe.preferredHeight = primary ? 10f : 6f;
            AddImage(meter, null, primary ? Palette.MeterTrack : Palette.RuleDim);

            _fill = NewRect("Fill", meter);
            _fill.anchorMin = new Vector2(0f, 0f);
            _fill.anchorMax = new Vector2(0f, 1f);
            _fill.pivot = new Vector2(0f, 0.5f);
            _fill.anchoredPosition = new Vector2(1f, 0f);
            _fill.sizeDelta = new Vector2(0f, -2f);
            _fillImage = AddImage(_fill, null, Palette.MeterGold);
            _lastFillColor = Palette.MeterGold;

            _value = NewText("Value", row, font, 15f, TextAlignmentOptions.MidlineRight, Palette.Count, ellipsis: false);
            LayoutElement valueLe = _value.Go.AddComponent<LayoutElement>();
            valueLe.minWidth = 0f;
            valueLe.flexibleWidth = 1f;
            _go.SetActive(false);
        }

        /// Rebuild path: presence, label, bar presence. Returns true when the row shows.
        public bool Apply(LensMeter? meter, bool primary)
        {
            if (!meter.HasValue)
            {
                _go.SetActive(false);
                return false;
            }

            LensMeter m = meter.Value;
            _label.SetText(m.Label);
            _meter.SetActive(m.Fraction.HasValue);
            _go.SetActive(true);
            TickValues(meter, primary);
            return true;
        }

        /// Tick path: value text, fill width and colours only.
        public void TickValues(LensMeter? meter, bool primary)
        {
            if (!meter.HasValue)
            {
                return;
            }

            LensMeter m = meter.Value;
            _value.SetText(m.ValueText);
            Color role = Palette.Get(m.Color);
            _value.SetColor(primary ? role : Palette.Count);
            if (!m.Fraction.HasValue)
            {
                return;
            }

            float width = Mathf.Clamp01(m.Fraction.Value) * MeterFillWidth;
            if (!Mathf.Approximately(width, _lastFill))
            {
                _lastFill = width;
                _fill.sizeDelta = new Vector2(width, -2f);
            }

            if (role != _lastFillColor)
            {
                _lastFillColor = role;
                _fillImage.color = role;
            }
        }
    }

    private sealed class ItemRow
    {
        public readonly GameObject Go;
        private readonly Image _art;
        private readonly TextSlot _name;
        private readonly TextSlot _count;
        private readonly LayoutElement _countLe;
        private Sprite? _lastSprite;
        private bool _artEnabled;

        public ItemRow(Transform parent, TMP_FontAsset font, Sprite? tile)
        {
            RectTransform row = NewRect("ItemRow", parent);
            Go = row.gameObject;
            SetPreferredHeight(row, ItemRowHeight);
            AddRowLayout(row, 8f);

            RectTransform icon = NewRect("Icon", row);
            LayoutElement iconLe = icon.gameObject.AddComponent<LayoutElement>();
            iconLe.preferredWidth = 30f;
            iconLe.preferredHeight = 30f;
            AddImage(icon, tile, Palette.Slot);

            RectTransform art = NewRect("Art", icon);
            art.anchorMin = Vector2.zero;
            art.anchorMax = Vector2.one;
            art.offsetMin = new Vector2(1f, 1f);
            art.offsetMax = new Vector2(-1f, -1f);
            _art = AddImage(art, null, Color.white);
            _art.preserveAspect = true;
            _art.enabled = false;

            _name = NewText("Name", row, font, 15f, TextAlignmentOptions.MidlineLeft, Palette.Body, ellipsis: true);
            SetFlexibleName(_name.Go);

            _count = NewText("Count", row, font, 15f, TextAlignmentOptions.MidlineRight, Palette.Count, ellipsis: false);
            _countLe = _count.Go.AddComponent<LayoutElement>();
            _countLe.minWidth = 48f;
            _countLe.preferredWidth = 48f;
            Go.SetActive(false);
        }

        public void Apply(LensItem item, bool showNames, bool output)
        {
            if (!ReferenceEquals(item.Sprite, _lastSprite))
            {
                _lastSprite = item.Sprite;
                _art.sprite = item.Sprite;
            }

            bool hasArt = item.Sprite != null;
            if (hasArt != _artEnabled)
            {
                _artEnabled = hasArt;
                _art.enabled = hasArt;
            }

            _name.SetText(item.Name);
            _name.Go.SetActive(showNames);
            _countLe.flexibleWidth = showNames ? 0f : 1f;
            _count.SetColor(output ? Palette.MeterGood : Palette.Count);
            TickCount(item);
            Go.SetActive(true);
        }

        public void TickCount(LensItem item)
        {
            _count.SetText(item.Count > 0 ? item.Count.ToString(System.Globalization.CultureInfo.InvariantCulture) : "");
        }
    }

    private sealed class ItemBlockView
    {
        private readonly GameObject _go;
        private readonly ItemRow[] _rows;
        private readonly List<int> _order = new(ItemRowsPerBlock);
        private int _shown;

        public ItemBlockView(Transform parent, TMP_FontAsset font, int index)
        {
            RectTransform block = NewRect($"ItemBlock_{index}", parent);
            _go = block.gameObject;
            AddColumnLayout(block, 1f, 0, 0);
            RectTransform rule = NewRect("BlockRule", block);
            SetPreferredHeight(rule, 2f);
            AddImage(rule, Probe(RuleNames), Palette.RuleDim);

            Sprite? tile = Probe(TileNames);
            _rows = new ItemRow[ItemRowsPerBlock];
            for (int i = 0; i < _rows.Length; i++)
            {
                _rows[i] = new ItemRow(block, font, tile);
            }

            _go.SetActive(false);
        }

        /// Rebuild path. Returns rows used; adds the rows it could not show to hidden.
        public int Apply(LensItemBlock? block, int budget, bool showNames, ref int hidden)
        {
            if (block == null || block.Items.Count == 0 || budget <= 0)
            {
                if (block != null)
                {
                    hidden += block.Items.Count;
                }

                _shown = 0;
                _go.SetActive(false);
                return 0;
            }

            List<LensItem> items = block.Items;
            _order.Clear();
            for (int i = 0; i < items.Count; i++)
            {
                _order.Add(i);
            }

            // Row order is fixed at rebuild so rows do not jump between ticks. A block whose
            // reader already ordered it (spec 3.14 equipment order, slot order) keeps that order:
            // every row there has Count 1, so the count sort would only alphabetize it.
            if (!block.PreserveOrder && OttoLensPlugin.SortRowsBy.Value == OttoLensPlugin.SortRows.Count)
            {
                _order.Sort((a, b) =>
                {
                    int byCount = items[b].Count.CompareTo(items[a].Count);
                    return byCount != 0 ? byCount : string.CompareOrdinal(items[a].Name, items[b].Name);
                });
            }

            _shown = Mathf.Min(items.Count, Mathf.Min(budget, ItemRowsPerBlock));
            for (int r = 0; r < _rows.Length; r++)
            {
                if (r < _shown)
                {
                    _rows[r].Apply(items[_order[r]], showNames, block.IsOutput);
                }
                else if (_rows[r].Go.activeSelf)
                {
                    _rows[r].Go.SetActive(false);
                }
            }

            hidden += items.Count - _shown;
            _go.SetActive(true);
            return _shown;
        }

        /// Tick path: counts only, through the order fixed at rebuild.
        public void Tick(LensItemBlock? block)
        {
            if (block == null || _shown == 0)
            {
                return;
            }

            List<LensItem> items = block.Items;
            for (int r = 0; r < _shown && r < _order.Count; r++)
            {
                int i = _order[r];
                if (i < items.Count)
                {
                    _rows[r].TickCount(items[i]);
                }
            }
        }
    }

    /// One warning per reader and exception type, matching Patches.LogOnce: a reader that
    /// throws deterministically would otherwise write a line on every retry.
    private static void LogReaderFailure(ILensReader reader, Exception ex)
    {
        Type readerType = reader.GetType();
        if (LoggedReaderFailures.Add((readerType, ex.GetType())))
        {
            OttoLensPlugin.Log.LogWarning($"{readerType.Name} threw {ex.GetType().Name}: {ex.Message}. Further {ex.GetType().Name} reports from {readerType.Name} are suppressed.\n{ex.StackTrace}");
        }
    }

    /// Design section 3 palette.
    private static class Palette
    {
        public static readonly Color Panel = Hex(0x17110C, 0.88f);
        public static readonly Color Outline = Hex(0x0B0806, 0.70f);
        public static readonly Color EdgeGold = Hex(0xC9A54A, 1f);
        public static readonly Color TitleGold = Hex(0xD9B45B, 1f);
        public static readonly Color RuleGold = Hex(0xC9A227, 0.85f);
        public static readonly Color RuleDim = Hex(0x6B5A3E, 0.55f);
        public static readonly Color Body = Hex(0xE4D5B7, 1f);
        public static readonly Color Count = Hex(0xF0E6CF, 1f);
        public static readonly Color Dim = Hex(0x9C8C6E, 1f);
        public static readonly Color Slot = Hex(0x0F0C0A, 0.55f);
        public static readonly Color MeterTrack = Hex(0x2A211A, 0.90f);
        public static readonly Color MeterGood = Hex(0x6E8B3D, 1f);
        public static readonly Color MeterGold = Hex(0xC9A227, 1f);
        public static readonly Color MeterWarn = Hex(0xC87A28, 1f);
        public static readonly Color MeterBad = Hex(0xA8443A, 1f);

        public static Color Get(LensColor role) => role switch
        {
            LensColor.Good => MeterGood,
            LensColor.Warn => MeterWarn,
            LensColor.Bad => MeterBad,
            LensColor.Dim => Dim,
            _ => MeterGold,
        };

        private static Color Hex(int rgb, float alpha)
            => new(((rgb >> 16) & 0xFF) / 255f, ((rgb >> 8) & 0xFF) / 255f, (rgb & 0xFF) / 255f, alpha);
    }
}
