using OttoLens.Model;

namespace OttoLens.Readers;

/// Mineable rock, one instance per vanilla type. MineRock5 keeps per area health in its loaded
/// hit area list (refreshed by CheckForUpdate on data revision change); the legacy MineRock
/// keeps one float per area under "Health<i>". The hover object is the area collider itself,
/// so the primary row is the health of the area under the crosshair with no extra raycast.
internal sealed class MineRockReader : ILensReader
{
    private readonly LensReport _report = new();
    private readonly bool _legacy;

    // Legacy MineRock area keys, built once per index (spec 3.0: cache built hashes).
    private static readonly List<int> AreaKeys = new();

    /// legacy = false reads MineRock5, legacy = true reads MineRock.
    public MineRockReader(bool legacy)
    {
        _legacy = legacy;
    }

    public Type TargetType => _legacy ? typeof(MineRock) : typeof(MineRock5);

    public bool Enabled => OttoLensPlugin.ShowMineables.Value;

    public LensReport? Read(Component target, GameObject hover)
        => _legacy ? ReadLegacy((MineRock)target, hover) : ReadRock5((MineRock5)target, hover);

    private LensReport? ReadRock5(MineRock5 rock, GameObject hover)
    {
        ZNetView view = rock.m_nview;
        List<MineRock5.HitArea> areas = rock.m_hitAreas;
        if (view == null || !view.IsValid() || areas == null || areas.Count == 0)
        {
            return null;
        }

        float areaMax = DestructibleReader.ScaledMax(rock.m_health);
        if (areaMax <= 0f)
        {
            return null;
        }

        Collider? hovered = hover.GetComponent<Collider>();
        float hoveredHealth = -1f;
        float total = 0f;
        int intact = 0;
        for (int i = 0; i < areas.Count; i++)
        {
            MineRock5.HitArea area = areas[i];
            float health = area.m_health;
            if (health > 0f)
            {
                intact++;
                total += health;
            }

            if (hovered != null && area.m_collider == hovered)
            {
                hoveredHealth = health;
            }
        }

        return Fill(LensFormat.Name(rock.m_name), hoveredHealth, total, intact, areas.Count, areaMax, rock.m_minToolTier);
    }

    private LensReport? ReadLegacy(MineRock rock, GameObject hover)
    {
        ZNetView view = rock.m_nview;
        Collider[] areas = rock.m_hitAreas;
        if (view == null || !view.IsValid() || areas == null || areas.Length == 0)
        {
            return null;
        }

        float areaMax = rock.GetHealth();
        if (areaMax <= 0f)
        {
            return null;
        }

        ZDO zdo = view.GetZDO();
        Collider? hovered = hover.GetComponent<Collider>();
        float hoveredHealth = -1f;
        float total = 0f;
        int intact = 0;
        for (int i = 0; i < areas.Length; i++)
        {
            float health = zdo.GetFloat(AreaKey(i), areaMax);
            if (health > 0f)
            {
                intact++;
                total += health;
            }

            if (hovered != null && areas[i] == hovered)
            {
                hoveredHealth = health;
            }
        }

        return Fill(LensFormat.Name(rock.m_name), hoveredHealth, total, intact, areas.Length, areaMax, rock.m_minToolTier);
    }

    private LensReport? Fill(string title, float hoveredHealth, float total, int intact, int count, float areaMax, int minToolTier)
    {
        if (intact == 0)
        {
            return null;
        }

        LensReport report = _report;
        report.Reset();
        report.Title = title;

        // Primary: the area under the crosshair when we found it, else the whole rock. The
        // label stays "Health" either way so a crosshair drift does not rebuild the panel.
        float fraction = hoveredHealth > 0f
            ? Mathf.Clamp01(hoveredHealth / areaMax)
            : Mathf.Clamp01(total / (areaMax * count));
        report.PrimaryMeter = LensReport.Meter("Health", fraction, LensFormat.Percent(fraction), LensFormat.Ramp(fraction));

        if (count > 1)
        {
            float areaFraction = (float)intact / count;
            report.Secondary1 = LensReport.Meter("Areas", areaFraction, LensFormat.Count(intact, count), LensColor.Dim);
        }

        report.Secondary2 = DestructibleReader.ToolTierMeter(minToolTier);
        return report;
    }

    private static int AreaKey(int index)
    {
        while (AreaKeys.Count <= index)
        {
            AreaKeys.Add(("Health" + AreaKeys.Count).GetStableHashCode());
        }

        return AreaKeys[index];
    }
}
