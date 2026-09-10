# Plant Type - Valheim 1.0.7

Decompiled from `assembly_valheim.dll` (vanilla).

## Members

### Fields

| Member | Signature | Line |
|--------|-----------|------|
| m_name | `public string m_name = "Plant";` | 22 |
| m_growTime | `public float m_growTime = 10f;` | 24 |
| m_growTimeMax | `public float m_growTimeMax = 2000f;` | 26 |
| m_minScale | `public float m_minScale = 1f;` | 30 |
| m_maxScale | `public float m_maxScale = 1f;` | 32 |
| m_growRadius | `public float m_growRadius = 1f;` | 34 |
| m_needCultivatedGround | `public bool m_needCultivatedGround;` | 38 |
| m_nview | `private ZNetView m_nview;` | 70 |

### Enums

| Member | Signature | Lines |
|--------|-----------|-------|
| Status | `public enum Status { Healthy, NoSun, NoSpace, WrongBiome, NotCultivated, NoAttachPiece, TooHot, TooCold }` | 6-16 |

### Methods

| Member | Signature | Line |
|--------|-----------|------|
| GetGrowTime | `private float GetGrowTime()` | 172 |
| TimeSincePlanted | `private double TimeSincePlanted()` | 138 |
| GetStatus | `public Status GetStatus()` | 418 |
| GetHoverText | `public string GetHoverText()` | 117 |

## ZDO Keys

The Plant type reads/writes the following ZDO variables:

- `ZDOVars.s_seed` - used in GetInt (line 98) and Set (line 102)
- `ZDOVars.s_plantTime` - used in GetLong (lines 104, 140) and Set (line 106)

## Notes

- All requested members exist in this type
- The `Status` enum has 8 values: Healthy, NoSun, NoSpace, WrongBiome, NotCultivated, NoAttachPiece, TooHot, TooCold
- Plant uses network synchronization via `ZNetView` (m_nview)
- Plant tracks planting time via `ZDOVars.s_plantTime` for growth calculation
