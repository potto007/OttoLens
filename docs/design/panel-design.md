# OttoLens hover panel, final design: the ledger, edged

Base: design B (vertical ledger). Grafts: A's status strip discipline (health
and support as a stacked pair, content level going amber, `Free N` footer,
degraded-frame spec, no Update while hidden), C's rebuild-vs-tick split, gold
left edge plus 1 px outline as the no-sprite identity, one-primary-meter rule,
absolute time strings, fixed secondary order, `raycastTarget = false`
everywhere, never-throw on a missing sprite.

A narrow column to the right of the vanilla hover text. One entry per line:
icon left, name middle, count right-aligned on a fixed guide. The identity
comes from alignment, type and a 3 px gold edge, not from a frame sprite.

Every judge weakness is listed in the "Weakness ledger" at the end of section 6
with fixed or accepted and why.

## 1. ASCII mockups (1080p, 1 char = about 8 px)

Chest, 11 item types, default `MaxRows = 10`. Eleven types is exactly
`MaxRows + 1`, so the overflow line is replaced by the eleventh row (the line
costs a row either way).

```
                    +          <- crosshair at screen centre, never covered
      Reinforced chest
      [E] Open                 <- vanilla hover text, untouched
                             ||  REINFORCED CHEST                 ||
                             ||  ================================ ||   gold rule
                             ||  Slots   [#########-------]  11/24||   primary meter, 10 px
                             ||  -------------------------------- ||   dim rule
                             || [##] Wood                     72  ||
                             || [##] Fine wood                40  ||
                             || [##] Iron nails              120  ||
                             || [##] Iron                     18  ||
                             || [##] Deer hide                16  ||
                             || [##] Carrot seeds              9  ||
                             || [##] Thistle                   5  ||
                             || [##] Resin                     4  ||
                             || [##] Honey                     3  ||
                             || [##] Frostner                  1  ||
                             || [##] Bronze                    1  ||
                             ||  -------------------------------- ||
                             ||  Free 13                         ||   footer, dim
                             ^ 3 px gold edge, full height
```

Chest with 30 item types, rows sorted by count descending so the tail is the
least interesting:

```
                             || [##] Bronze                    1  ||
                             ||  -------------------------------- ||
                             ||  and 20 more                     ||   italic, dim
                             ||  Free 0                          ||
```

Smelter: fuel is the primary meter, smelt progress is secondary, ore queue and
finished output are item blocks. The state word sits in the title row.

```
                    +
      Smelter
      [E] Add ore            ||  SMELTER                  SMELTING ||   headline, amber
                             ||  ================================ ||
                             ||  Coal    [############-----] 12/20||   primary, 10 px, amber
                             ||  Next    [#########--------]  0:41||   secondary, 6 px, gold
                             ||  -------------------------------- ||
                             || [##] Copper ore                7  ||
                             || [##] Tin ore                   3  ||
                             ||  -------------------------------- ||
                             || [##] Copper                    2  ||   count in green: ready
```

Build piece (stone wall). Health and support stack as a pair on the same guides:

```
                             ||  STONE WALL                   87% ||
                             ||  ================================ ||
                             ||  Health  [##############---]   87%||   primary
                             ||  Support [##########-------]   61%||   secondary
```

The rule under the title, the rule at the top of each block, and the gold edge
are the only chrome. Every icon, name, meter and count sits on the same
vertical guides, so a smelter and a chest are the same panel with different
rows.

## 2. uGUI hierarchy (px at 1080p)

Parent: the Hud canvas (`Hud.instance.m_rootObject.transform`), which carries
the vanilla CanvasScaler, so 1440p needs no extra work. `GuiScale` (config,
default 1.0) multiplies the root `localScale` only; `anchoredPosition` is in
canvas units and does not scale, so the left edge stays put.

```
OttoLensPanel                 RectTransform anchor (0.5,0.5) pivot (0,1)
                              anchoredPosition (offsetX=150, offsetY=-32)
                              sizeDelta (300, auto)
  + Image                     sprite null, color Outline    (the 1 px outline IS the root)
  + CanvasGroup               alpha 0..1, blocksRaycasts=false, interactable=false
  + VerticalLayoutGroup       padding L10 R10 T8 B8, spacing 2,
                              childForceExpandWidth=true, childForceExpandHeight=false,
                              childControlWidth=true, childControlHeight=true
  + ContentSizeFitter         vertical PreferredSize, horizontal Unconstrained
  |                           (the ONLY ContentSizeFitter in the tree)
  |
  +-- Plate                   RectTransform stretch, offsets 1,1,-1,-1
  |     + Image               backdrop sprite (probe) or null, type Sliced|Simple, color Panel
  |     + LayoutElement       ignoreLayout=true          (first sibling: draws under content)
  |
  +-- GoldEdge                anchorMin (0,0) anchorMax (0,1) pivot (0,0.5)
  |     + Image               sprite null, color EdgeGold, sizeDelta (3,0), anchoredPosition (0,0)
  |     + LayoutElement       ignoreLayout=true
  |
  +-- TitleRow                LayoutElement preferredHeight 22
  |     + HorizontalLayoutGroup spacing 6, childForceExpandWidth=false, childAlignment MiddleLeft
  |     +-- Title             TMP 17, Left, TitleGold, uppercase via text,
  |     |                     LayoutElement minWidth 0, preferredWidth 0, flexibleWidth 1
  |     |                     overflow Ellipsis, wordWrap off
  |     +-- Headline          TMP 15, Right, color = state colour, LayoutElement minWidth 0
  |                           (hidden when no state word)
  |
  +-- TitleRule               LayoutElement preferredHeight 2
  |     + Image               rule sprite (probe) or null, color RuleGold
  |
  +-- StatusBlock             VerticalLayoutGroup spacing 2, padding T2 B2  (SetActive false if empty)
  |     +-- StatusRow_0 (primary)     LayoutElement preferredHeight 22
  |     |     + HorizontalLayoutGroup spacing 6, childAlignment MiddleLeft
  |     |     +-- Label       TMP 15, Body, LayoutElement minWidth 64 preferredWidth 64
  |     |     +-- Meter       LayoutElement preferredWidth 120, preferredHeight 10
  |     |     |     + Image   sprite null, color MeterTrack
  |     |     |     +-- Fill  anchorMin (0,0) anchorMax (0,1) pivot (0,0.5)
  |     |     |               offsetMin (1,1) offsetMax (0,-1), sizeDelta.x = f*118
  |     |     |               + Image sprite null, color MeterGood|Warn|Bad|Gold
  |     |     +-- Value       TMP 15, Right, color = meter colour, LayoutElement flexibleWidth 1
  |     +-- StatusRow_1..2 (secondary, pooled)   LayoutElement preferredHeight 18
  |           same children; Meter preferredHeight 6, Fill x = f*118; Value color Count
  |
  +-- ItemBlock_0..1          VerticalLayoutGroup spacing 1, padding T0 B0 (pooled 2: contents, output)
  |     +-- BlockRule         LayoutElement preferredHeight 2, Image rule sprite|null, color RuleDim
  |     +-- ItemRow_0..15 (pooled)   LayoutElement preferredHeight 32
  |           + HorizontalLayoutGroup spacing 8, childForceExpandWidth=false, childAlignment MiddleLeft
  |           +-- Icon        LayoutElement preferredWidth 30 preferredHeight 30
  |           |     + Image   slot sprite (probe) or null, color Slot
  |           |     +-- Art   stretch, offsets 1; Image sprite = item icon, preserveAspect,
  |           |                 raycastTarget=false; disabled when icon is null
  |           +-- Name        TMP 15, Left, Body, overflow Ellipsis, wordWrap off,
  |           |               LayoutElement minWidth 0, preferredWidth 0, flexibleWidth 1
  |           +-- Count       TMP 15, Right, Count (or MeterGood for output rows),
  |                           LayoutElement minWidth 48, preferredWidth 48
  |
  +-- OverflowBlock           VerticalLayoutGroup spacing 1 (SetActive false when nothing to say)
        +-- BlockRule         LayoutElement preferredHeight 2, Image, color RuleDim
        +-- Overflow          TMP 14 italic, Dim, "and N more"  (hidden when N = 0)
        +-- Footer            TMP 14, Dim, "Free 13"           (hidden when target has no capacity)
```

Every `Image` and `TextMeshProUGUI` has `raycastTarget = false`. Every
`TextMeshProUGUI` has `richText = false` so item names cannot inject tags.

Sub-blocks carry no ContentSizeFitter. A VerticalLayoutGroup is itself an
`ILayoutElement` and reports its preferred height to the parent, so the single
root fitter is enough and there is no nested-fitter conflict. Block heights
therefore need no hand computation.

Rules belong to the block they head, so one `SetActive` on the block shows or
hides its rule with it. There is no separate rule object to keep in step.

Width budget at 300 px: padding 20, icon 30, gap 8, name (flexible, about
186), gap 8, count 48. Status: label 64, gap 6, meter 120, gap 6, value 84.
The name column is the only flexible element in any row, and it has
`minWidth 0`, so long names shrink to the ellipsis instead of widening the row.
`childForceExpandWidth` is off on every row HLG for the same reason.

### Placement and the crosshair

Pivot (0,1) at `(offsetX, offsetY)` from screen centre means the panel occupies
`x in [offsetX, offsetX + 300]`, `y in [offsetY - height, offsetY]`. With
`OffsetX >= 96` and `OffsetY <= -16` (config clamps, both enforced in code) the
crosshair box at centre is never inside that rectangle, at any row count and
at any `GuiScale`. Growth is downward from a fixed top-left corner, so the aim
point is never approached as content grows.

`AvoidHoverText` (default true): on rebuild only, read
`Hud.instance.m_hoverName.preferredWidth`, and if `width/2 + 16 > offsetX`
place the panel at `width/2 + 16` for the life of that target. It only ever
pushes right, never left, and never runs on the tick path. With it off the
left edge is fixed at `OffsetX`.

Vertical clamp: at rebuild, `rowsFit = floor((offsetY + canvasHeight/2 - 24 -
fixedHeight) / 33)` where `fixedHeight` is header, status block, rules and
overflow block. Rows shown = `min(maxRows, rowsFit)`. The panel bottom never
passes 24 px above the screen edge at any resolution or scale.

## 3. Colors, fonts, metrics

| Role | Hex | Alpha | Use |
| --- | --- | --- | --- |
| Panel | `#17110C` | 0.88 | plate fill (raised from 0.82 so dark biomes keep the edge) |
| Outline | `#0B0806` | 0.70 | 1 px outline (the root Image) |
| EdgeGold | `#C9A54A` | 1.0 | 3 px left edge |
| TitleGold | `#D9B45B` | 1.0 | title |
| RuleGold | `#C9A227` | 0.85 | rule under the title |
| RuleDim | `#6B5A3E` | 0.55 | block rules, secondary meter track |
| Body | `#E4D5B7` | 1.0 | names, labels |
| Count | `#F0E6CF` | 1.0 | counts, secondary values |
| Dim | `#9C8C6E` | 1.0 | overflow, footer |
| Slot | `#0F0C0A` | 0.55 | icon tile behind the sprite |
| MeterTrack | `#2A211A` | 0.90 | primary meter track |
| MeterGood | `#6E8B3D` | 1.0 | muted green |
| MeterGold | `#C9A227` | 1.0 | in-progress time, neutral |
| MeterWarn | `#C87A28` | 1.0 | amber (low fuel, nearly full) |
| MeterBad | `#A8443A` | 1.0 | muted red |

Font: the `TMP_FontAsset` taken at runtime from `Hud.instance.m_hoverName`.
Copy `font` only. Do not touch `fontMaterial` or `fontSharedMaterial`, and do
not enable outline: writing an outline on the shared material outlines every
vanilla hover label, and the 0.88 backdrop already carries legibility over
snow and sand. No font asset ships with the mod.

Sizes: title 17, headline 15, rows 15, labels and values 15, overflow and
footer 14. Icon tile 30 with a 28 px sprite inside. Row height 32, primary
status row 22, secondary 18, title row 22. Meter 120 wide; 10 px tall primary,
6 px secondary; fill inset 1 px on all sides so the fill width is `f * 118`.
Panel padding 10 sides, 8 top and bottom. At 1440p the CanvasScaler lifts 15 px
rows to about 20 px; `GuiScale 1.15` is the recommended value there.

## 4. Vanilla sprites and fallback

Probed once at first build with `Resources.FindObjectsOfTypeAll<Sprite>()`,
cached in a static `Dictionary<string, Sprite>`. First name that exists wins:

- Plate: `woodpanel_trophys`, `woodpanel_settings`, `panel_bkg_128`,
  `darken_blob`. `type = Sliced` when `sprite.border != Vector4.zero`, else
  `Simple`. Tinted `Panel`.
- Rule: `panel_separator`, `divider`. Tinted per rule role.
- Icon tile: `item_background`, `inventory_slot`. Tinted `Slot`.
- Meter track and fill: no probe. `sprite = null` renders a plain quad.
- Item art: `ItemDrop.ItemData.GetIcon()` inside try/catch; on null or throw,
  `Art.enabled = false`. The tile and name still render, so an item with a
  broken icon still reads.

Fallback contract for every probed role: `sprite = null`, `type = Simple`,
same `color`, same `RectTransform`. A miss changes the texture only, never the
geometry. The fully degraded panel is a flat 0.88 plate with a 1 px outline and
a 3 px gold edge, which is still a designed object, not an accident.

No `Image.type = Filled` anywhere in the tree, so the "Filled with null sprite
ignores fillAmount" trap cannot occur. If a later feature needs a Filled
image, use `Sprite.Create(Texture2D.whiteTexture, new Rect(0,0,4,4),
Vector2.one * 0.5f)` as its sprite, never null.

No embedded PNG, no asset bundle. If one PNG is ever added, spend it on the
plate only (32x32 9-sliced parchment edge). Nothing else needs art.

## 5. Status encoding

One meter widget, one primary per target, at most two secondaries. The meter
is a `sizeDelta.x` write on the Fill child: `clamp01(f) * 118`. Value text
carries the exact number; the bar carries the shape; the title headline carries
a state word when one exists.

Primary per target (C's table, one row each, always first):

| Target | Primary label, fraction | Colour | Value | Headline |
| --- | --- | --- | --- | --- |
| Fire, smelter, kiln, oven, sap collector | fuel name, `fuel / maxFuel` | Good > 0.5, Warn 0.15..0.5, Bad < 0.15 | `12/20` | `SMELTING`, `NO FUEL` |
| Fermenter, cooking station, oven step | `Next`, `remaining / total` (drains) | Gold busy, Good at zero | `m:ss`, `Ready` | `READY` when done |
| Plant, beehive | `Grown`, progress | Gold, Good when done, Bad if unhealthy | `2m 10s`, `Ready` | `READY` |
| Container, cart, ship cargo | `Slots`, `used / capacity` | Dim < 0.9, Warn >= 0.9, Bad at 1.0 | `11/24` | `FULL` at 1.0 |
| Build piece | `Health`, `hp / maxHp` | lerp Good to Bad | `87%` | `87%` |
| Tree, rock | `Health`, `hp / maxHp` | lerp Good to Bad | `40%` | none |
| Tamed pet | `Tame` or `Hunger` | Good, Warn, Bad | `62%`, `Fed` | `2 STARS` |

Secondary rows, fixed order, only when the target supplies the data:

1. `Next`: process time remaining, drains, Gold; `Ready` in Good at zero.
2. `Support` (build pieces only): `support / maxSupport`, ramp inverted so high
   is Good; sits directly under `Health` so the pair compares at one glance.
   Full-health pieces hide `Health` unless `ShowFullHealth` is on, and then
   `Support` becomes the primary.

Item blocks: block 0 is contents or queue (chest items, ore queue, pet
inventory); block 1 is finished output (smelter bars, cooked food, fermenter
mead) with counts in `MeterGood`. Footer shows `Free N` for anything with a
slot capacity.

Rules that keep it honest: a value with no maximum renders no meter, and the
number right-aligns into the value column. A value that is exactly full renders
a full bar, never an empty one. Time is always absolute short form (`0:41`,
`2m 10s`, `1h 04m`), never a bare percent of an unknown total.

## 6. Why this is not an icon row, and the weakness ledger

- **The count is a number in a column, not a badge on a sprite.** Counts line
  up on a guide, so 120 against 9 compares by eye position.
- **Names are present.** The player reads `Iron nails` instead of decoding a
  sprite. A broken icon does not break the row.
- **Status and contents share one grammar.** Fuel, time, health and the ore
  queue are label, meter, value on the same guides. Every target is the same
  panel with different rows.
- **One primary meter.** The first row is taller and its value takes the meter
  colour; the headline repeats the state word. The eye lands on one thing.
- **Growth is vertical, from a fixed corner, away from the crosshair.**
- **The chrome is a plate, an outline, a gold edge and rules**, all plain
  quads. It ages cleanly to 1440p because type and alignment carry it.

Weakness ledger (every judge point on B):

| Judge point | Disposition |
| --- | --- |
| Column floats 150 px right, looks detached | Fixed in part: gold edge and 0.88 plate give it a body; `AvoidHoverText` closes the gap only when the vanilla line is wide. Accepted remainder: a fixed left edge is the price of not depending on the vanilla text height, which the build lens ranked higher. |
| Low identity, debug-overlay look, backdrop vanishes in dark biomes | Fixed: 3 px gold edge, 1 px outline, Panel alpha 0.88, gold title rule. |
| 24 px icons too small | Fixed: 30 px tile, 28 px art, 32 px rows. |
| Inline meters jar against the serif ledger | Fixed in part: secondary tracks use the rule colour so they read as a rule that lights up; primary is the only heavy bar. Accepted remainder: a bar is the only pre-attentive shape available without a sprite. |
| Row cap 8 hides a busy chest | Fixed: default 10 rows, `MaxRows + 1` shows all, rows sort by count desc so the hidden tail is the small stuff, screen-height clamp keeps the raised cap on screen. Accepted: 30 types still says `and 20 more`; a two-column grid would reintroduce badges over art. |
| Vertical list scans slower than a 2-row grid | Accepted: names remove the recognition step, which is the bigger cost for a moving player; count-desc sort puts the answer in the first three rows. |
| Fixed anchor lets a wide vanilla line run under the panel | Fixed: `AvoidHoverText` width probe on rebuild only. |
| No dominant signal on a smelter | Fixed: primary row rule (10 px meter, coloured value), state word headline, at most two secondaries. |
| Drifts into play view at 1440p with GuiScale 1.15 | Fixed: `anchoredPosition` does not scale with `localScale`, so the left edge stays; vertical clamp keeps the bottom on screen. |
| Many TMPs regenerate meshes at 4 Hz | Fixed: `SetTextIfChanged` string cache on every TMP; colour and sizeDelta writes are also dirty-checked. |
| TMP ellipsis in an HLG can widen the row | Fixed: Name and Title have `minWidth 0`, `preferredWidth 0`, `flexibleWidth 1`, wrap off; every row HLG has `childForceExpandWidth = false`; Count and Label have fixed widths. |
| Localization dependency | Accepted: `Localization.instance.Localize(m_shared.m_name)` cached per token in a static dictionary; on throw, show the token with `$` stripped. |
| Raised cap runs off screen | Fixed: `rowsFit` clamp. |
| BlockRule and OverflowRule must be toggled in step | Fixed: rules are children of their blocks; one `SetActive` per block. |

## 7. Implementation checklist: `LensPanel`

Build once

- [ ] `LensPanel.Get()` lazily builds under `Hud.instance.m_rootObject` when
      `Hud.instance != null`; returns null otherwise and the caller skips the
      frame. Never build in `Awake` of the plugin.
- [ ] Probe sprites once into a static dictionary; every probe miss logs once
      at Debug level and falls back to `sprite = null`. No throw path.
- [ ] Take `font` from `Hud.instance.m_hoverName`; do not copy or modify
      materials.
- [ ] Build the tree in section 2 exactly once. Set `raycastTarget = false`
      and `richText = false` on every element as it is created.
- [ ] Clamp config at build: `OffsetX >= 96`, `OffsetY <= -16`,
      `PanelWidth in [240, 420]`, `MaxRows in [1, 16]`, `GuiScale in [0.75, 1.6]`,
      `RefreshHz in [1, 10]`.
- [ ] Root starts `SetActive(false)` with `CanvasGroup.alpha = 0`.

Pool

- [ ] Item rows: 16 (the hard `MaxRows` max) per item block, 2 blocks, built
      once, never destroyed; toggled with `SetActive`.
- [ ] Status rows: 3 (1 primary, 2 secondary). Overflow and footer: 1 each.
- [ ] Each pooled row keeps its last string, colour and fill width; setters
      early-return on equality.

Refresh rules

- [ ] Two paths. **Rebuild** runs when the hovered target changes (ZDO id or
      instance id) or the content signature changes (row count, item token
      list, block presence, headline text). Rebuild sets sprites, names,
      row `SetActive`, block `SetActive`, reads the hover-text width, applies
      the `rowsFit` clamp, then calls
      `LayoutRebuilder.ForceRebuildLayoutImmediate(root)` once.
- [ ] **Tick** runs on a coroutine at `RefreshHz` (default 4) while visible and
      writes only counts, values, headline, meter `sizeDelta.x` and colours.
      No `SetActive`, no `LayoutRebuilder` call, no allocation beyond the
      formatted strings.
- [ ] The tick compares a cheap signature (row count plus item token hash)
      and promotes itself to a rebuild when it differs.
- [ ] Row order: sort by count descending, then name, computed at rebuild
      only, so rows do not jump between ticks. Config `SortRows = Slot`
      keeps container slot order.
- [ ] `rows shown = types` when `types == maxRows + 1`; otherwise
      `min(maxRows, rowsFit)` plus `and N more`.
- [ ] Time strings: `m:ss` under 10 minutes, `Xm Ys` under an hour, `Xh Ym`
      above. `Ready` at zero. Never a percent for time.

Hide and destroy

- [ ] On hover loss: stop the tick coroutine, fade `CanvasGroup.alpha` to 0
      over `FadeSeconds` (default 0.08), then `root.SetActive(false)`. No
      `Update` and no coroutine runs while hidden.
- [ ] On hover gain: `root.SetActive(true)`, rebuild, start the tick, fade in.
- [ ] Hud destroyed or scene unloaded: the panel dies with its parent; the
      static reference is checked with `== null` (Unity fake-null) before use
      and rebuilt lazily.
- [ ] Config `Enabled` flipped off at runtime: destroy root, null the static,
      release nothing else (sprites are vanilla-owned).
- [ ] Targets without any data (no container, no fuel, no health, no items):
      panel stays hidden. Show nothing rather than an empty plate.

Config knobs (BepInEx `ConfigEntry`, section `OttoLens`)

| Key | Default | Range | Effect |
| --- | --- | --- | --- |
| `Enabled` | true | | master switch |
| `OffsetX` | 150 | 96..600 | left edge, px right of screen centre |
| `OffsetY` | -32 | -400..-16 | top edge, px below screen centre |
| `PanelWidth` | 300 | 240..420 | fixed panel width |
| `GuiScale` | 1.0 | 0.75..1.6 | root `localScale` |
| `MaxRows` | 10 | 1..16 | item rows per block before `and N more`, screen-clamped |
| `SortRows` | Count | Count, Slot | row order |
| `ShowNames` | true | | false hides the Name column (icon and count only) |
| `ShowFullHealth` | false | | show Health row at 100 percent |
| `AvoidHoverText` | true | | push right past a wide vanilla hover line |
| `RefreshHz` | 4 | 1..10 | tick rate while visible |
| `FadeSeconds` | 0.08 | 0..0.5 | show and hide fade |
| `BackdropAlpha` | 0.88 | 0.5..1.0 | Panel colour alpha |

Config changes to `OffsetX`, `OffsetY`, `PanelWidth`, `GuiScale`,
`BackdropAlpha` apply on the next rebuild without a restart; `MaxRows` and
`ShowNames` force a rebuild on change.
