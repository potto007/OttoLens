# MineRock5 Type Analysis

**Status**: Type exists in vanilla Valheim 1.0.7 assembly_valheim.dll  
**Decompiled from**: `/mnt/c/Program Files (x86)/Steam/steamapps/common/Valheim/valheim_Data/Managed/assembly_valheim.dll`

## Key Fields

| Field | Type | Declaration |
|-------|------|-------------|
| m_health | float | `public float m_health = 2f;` (line 45) |
| m_minToolTier | int | `public int m_minToolTier;` (line 49) |
| m_hitAreas | List<HitArea> | `private List<HitArea> m_hitAreas;` (line 65) |
| m_nview | ZNetView | `private ZNetView m_nview;` (line 71) |

## HitArea Nested Class

Defined as a private class inside MineRock5 (lines 16-33).

### HitArea Fields

| Field | Type | Declaration |
|-------|------|-------------|
| m_collider | Collider | `public Collider m_collider;` (line 18) |
| m_meshRenderer | MeshRenderer | `public MeshRenderer m_meshRenderer;` (line 20) |
| m_meshFilter | MeshFilter | `public MeshFilter m_meshFilter;` (line 22) |
| m_physics | StaticPhysics | `public StaticPhysics m_physics;` (line 24) |
| m_health | float | `public float m_health;` (line 26) |
| m_bound | BoundData | `public BoundData m_bound;` (line 28) |
| m_supported | bool | `public bool m_supported;` (line 30) |
| m_baseScale | float | `public float m_baseScale;` (line 32) |

## Key Methods

| Method | Signature | Declaration |
|--------|-----------|-------------|
| GetHoverText | `public string GetHoverText()` | line 271-274 |
| GetHoverOffset | `public float GetHoverOffset()` | line 580-583 |
| Damage | `public void Damage(HitData hit)` | line 286-336 |
| DamageArea | `private bool DamageArea(int hitAreaIndex, HitData hit)` | line 359-449 |

## Health Storage Mechanism

Health per area is stored using a base64-encoded `ZPackage` in the ZDO:

1. **Serialization** (SaveHealth, lines 194-205):
   - Each HitArea's health value is written sequentially to a ZPackage as single-precision floats
   - The package is base64-encoded and stored in ZDOVars.s_health
   - Example: `m_nview.GetZDO().Set(ZDOVars.s_health, value)`

2. **Deserialization** (LoadHealth, lines 174-192):
   - Data is read from ZDOVars.s_health and base64-decoded
   - Read as a count of areas followed by individual float values
   - Example: `m_nview.GetZDO().GetString(ZDOVars.s_health)`

3. **Per-Area Health Calculation** (Awake, line 103):
   ```
   hitArea.m_health = m_health + (float)Game.m_worldLevel * m_health * Game.instance.m_worldLevelMineHPMultiplier;
   ```
   Each HitArea starts with a health value scaled by world level.

## ZDO Variable Keys

Only one ZDOVars key is used:

- **ZDOVars.s_health** (string key):
  - Line 176: `m_nview.GetZDO().GetString(ZDOVars.s_health)` - read operation
  - Line 203: `m_nview.GetZDO().Set(ZDOVars.s_health, value)` - write operation
  - Contains base64-encoded serialized health data for all HitAreas

## Members Not Found

None of the requested members were missing.
