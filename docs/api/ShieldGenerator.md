# ShieldGenerator - Valheim 1.0.7 API

## Type Summary
**Namespace:** Global  
**Base Class:** MonoBehaviour, implements Hoverable, Interactable, IHasHoverMenu

## Requested Members

### Fields
- **m_fuelItems** (line 30): `public List<ItemDrop> m_fuelItems`
- **m_maxFuel** (line 32): `public int m_maxFuel`
- **m_nview** (line 83): `private ZNetView m_nview`

### Methods
- **GetFuel** (lines 267-274): `private float GetFuel()`
- **GetHoverText** (lines 168-183): `public string GetHoverText()`

## ZDO Keys

The ShieldGenerator type accesses the following ZDOVars:

- `ZDOVars.s_startTime` - Long value tracking when shield attack started (lines 220, 246, 426, 429)
- `ZDOVars.s_fuel` - Float value tracking current fuel level (lines 273, 285)

## Key Methods Using ZDO

- **GetFuel()** - Returns current fuel from ZDO as float with default value
- **RPC_SetFuel()** - Sets fuel value in ZDO (line 285)
- **GetAttackCharge()** - Reads s_startTime from ZDO to calculate charge progress (line 246)
- **RPC_Attack()** - Resets s_startTime and fuel on attack (lines 220, 219)
- **UpdateShield()** - Reads fuel and sets s_startTime when shield is fully charged (line 426, 429)
