# Fireplace Type Analysis

## Requested Members Found

### Fields

| Member | Signature | Line |
|--------|-----------|------|
| m_nview | private ZNetView m_nview; | 17 |
| m_fuelItem | public ItemDrop m_fuelItem; | 66 |
| m_maxFuel | public float m_maxFuel = 10f; | 26 |
| m_secPerFuel | public float m_secPerFuel = 3f; | 28 |
| m_infiniteFuel | public bool m_infiniteFuel; | 30 |
| m_startFuel | public float m_startFuel = 3f; | 24 |

### Methods

| Member | Signature | Lines |
|--------|-----------|-------|
| GetHoverText | public string GetHoverText() | 336-353 |
| IsBurning | public bool IsBurning() | 537-556 |

### ZDO Fuel Key

The code uses **`ZDOVars.s_fuel`** as the persistent fuel storage key. This is accessed via `GetFloat()` and `Set()` calls on the ZDO object.

## Missing Members

- **m_fuelPerSecond**: No such member exists. Closest real members are `m_secPerFuel` (seconds per fuel unit, inverse of m_fuelPerSecond) and the calculated consumption rate in `UpdateFireplace()` method (line 190).

## ZDO Keys Read/Written

The Fireplace class interacts with the following ZDOVars keys:

1. **ZDOVars.s_fuel** - Primary fuel amount storage (float)
   - Read in: Awake (line 127), UpdateFireplace (line 185), GetHoverText (line 342), AddFuel (line 364), Interact (line 389), UseItem (line 431), RPC methods (lines 473, 501, 513, 551)
   - Written in: Awake (line 129), UpdateFireplace (line 196), RPC_AddFuel (line 479), RPC_AddFuelAmount (line 503), RPC_SetFuelAmount (line 526)

2. **ZDOVars.s_lastTime** - Last update timestamp for fuel consumption calculations (long)
   - Read in: GetTimeSinceLastUpdate (line 166)
   - Written in: GetTimeSinceLastUpdate (line 168)

3. **ZDOVars.s_state** - Fireplace state (on/off) stored as int (1=on, 2=off)
   - Read in: UpdateFireplace (line 187), UpdateState (line 279), RPC_ToggleOn (line 490), IsBurning (line 543)
   - Written in: RPC_ToggleOn (line 491)

## Summary

The Fireplace type uses three core ZDO keys for persistent state management:
- `s_fuel`: Current fuel amount
- `s_lastTime`: Timestamp for fuel consumption calculations
- `s_state`: On/off toggle state
