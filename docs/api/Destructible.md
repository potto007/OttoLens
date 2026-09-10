# Destructible Type Analysis (Valheim 1.0.7)

## Type Declaration
- **Type**: `public class Destructible : MonoBehaviour, IDestructible`
- **Namespace**: (default/global)
- **Assembly**: assembly_valheim.dll

## Requested Members

### Existing Members

#### m_health
- **Signature**: `public float m_health = 1f;`
- **Kind**: field
- **Line**: 14

#### m_minToolTier
- **Signature**: `public int m_minToolTier;`
- **Kind**: field
- **Line**: 20

#### m_nview
- **Signature**: `private ZNetView m_nview;`
- **Kind**: field
- **Line**: 49

#### GetHoverText
- **Status**: NOT FOUND
- **Closest member**: No method with similar name exists

## ZDO Key Operations

The type accesses the following ZDOVars keys:

| ZDO Key | Operation | Line(s) | Context |
|---------|-----------|---------|---------|
| `ZDOVars.s_health` | Read (GetFloat) | 106 | RPC_Damage: reads current health with fallback to `m_health + Game.m_worldLevel * m_health * Game.instance.m_worldLevelMineHPMultiplier` |
| `ZDOVars.s_health` | Write (Set) | 128 | RPC_Damage: stores reduced health after applying damage |
| `ZDOVars.s_cheated` | Write (Set) | 201, 211 | Destroy: flags objects as cheated if destroyed by player with cheated item |

## Summary

The Destructible type manages destructible objects in Valheim. It stores health in the ZDO under `ZDOVars.s_health`, respects minimum tool tier requirements via `m_minToolTier`, and tracks damage through the networked `m_nview` (ZNetView) component. Damage is applied via RPC, health is persisted to ZDO, and destruction can be flagged as cheated via `ZDOVars.s_cheated`. No GetHoverText method is present in this type.
