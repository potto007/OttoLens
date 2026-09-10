# Pickable Type - Vanilla Valheim 1.0.7

## Source
Decompiled from: `assembly_valheim.dll`

## Type Information
- **Type**: `public class Pickable : MonoBehaviour, Hoverable, Interactable`
- **File**: `/mnt/c/Program Files (x86)/Steam/steamapps/common/Valheim/valheim_Data/Managed/assembly_valheim.dll`

## Requested Members

### Fields
- **m_respawnTimeMinutes**: `public float m_respawnTimeMinutes;` (line 24)
- **m_respawnTimeInitMax**: `public float m_respawnTimeInitMax;` (line 28)
- **m_amount**: `public int m_amount = 1;` (line 14)
- **m_itemPrefab**: `public GameObject m_itemPrefab;` (line 10)
- **m_nview**: `private ZNetView m_nview;` (line 64)

### Methods
- **GetHoverText**: `public string GetHoverText()` (line 114)
  - Returns empty string if `m_picked || m_enabled == 0`
  - Otherwise returns localized string with hover name and interaction hint

### ZDO Keys (Picked and Picked Time)
- **s_picked**: Accessed via `ZDOVars.s_picked` 
  - Written at line 282 in `SetPicked(bool picked)`
  - Written at line 105, 282 (Get operations)
  - Read at lines 86, 105
  
- **s_pickedTime**: Accessed via `ZDOVars.s_pickedTime`
  - Written at line 145 in `UpdateRespawn()`
  - Written at line 286 in `SetPicked(bool picked)` 
  - Read at lines 87, 166 in `ShouldRespawn()`

## All ZDO Keys Used by Pickable

The following `ZDOVars` hashes/keys are read or written by this type's methods:

1. **ZDOVars.s_picked** - Boolean flag indicating if item has been picked
2. **ZDOVars.s_pickedTime** - Long timestamp of when item was picked
3. **ZDOVars.s_enabled** - Boolean flag for enabled state

## Additional Fields and Methods of Interest

### Private Fields
- `m_pickedLocal`: `private bool m_pickedLocal;` (line 70)
- `m_pickedTime`: `private long m_pickedTime;` (line 74)

### Public Properties
- `GetEnabled`: `public int GetEnabled => m_enabled;` (line 76)

### Other Public Methods
- `GetPicked()`: `public bool GetPicked()` (line 295)
- `SetPicked(bool picked)`: `public void SetPicked(bool picked)` (line 269)
- `SetEnabled(bool value)`: `public void SetEnabled(bool value)` (line 300)
- `SetEnabled(int value)`: `public void SetEnabled(int value)` (line 305)
- `Interact(Humanoid character, bool repeat, bool alt)`: `public bool Interact(Humanoid character, bool repeat, bool alt)` (line 180)

## Missing Members
None - all requested members exist in the type.
