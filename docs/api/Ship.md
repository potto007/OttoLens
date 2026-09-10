# Vanilla Valheim 1.0.7 Ship Type API

**File:** `assembly_valheim.dll`  
**Type:** `Ship : MonoBehaviour, IMonoUpdater`  
**Total lines decompiled:** 886

## Requested Members

### Found Members

| Member | Signature |
|--------|-----------|
| m_nview | `private ZNetView m_nview;` |
| m_waterImpactEffect | `public EffectList m_waterImpactEffect = new EffectList();` |
| m_changeSailPosEffect | `public EffectList m_changeSailPosEffect = new EffectList();` |
| m_ashdamageEffects | `public GameObject m_ashdamageEffects;` |

### Members Not Found

| Requested | Note |
|-----------|------|
| m_shipEffects | Does not exist; closest: `m_waterImpactEffect`, `m_changeSailPosEffect` |
| GetHoverText | Method does not exist in decompiled output |
| cargo container | No cargo field or property found; no Container child access pattern evident |

**Note on cargo:** The Ship class does not expose cargo storage or container access in its public API. Cargo is likely managed through a separate component (e.g., via `GetComponent<>()` pattern).

## ZDO Keys Used

The Ship class reads/writes the following ZDO variables:

| Key | Access Pattern | Context |
|-----|-----------------|---------|
| `ZDOVars.s_forward` | `GetInt()`, `Set()` | Ship speed state (Speed enum) |
| `ZDOVars.s_rudder` | `GetFloat()`, `Set()` | Rudder/steering value |

**Usage:** Lines 582-589 in `UpdateControlls()` method; synchronizes speed and rudder state between owner and non-owner clients.

## Architecture Notes

- Ship data is synchronized via ZNetView with owner authority
- Speed and rudder state are network-synchronized state variables
- Water impact and sail position changes trigger EffectList.Create() for visual/audio feedback
- Ashlands damage is tracked locally but responds to region state (WorldGenerator.GetAshlandsOceanGradient)
