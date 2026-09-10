# CookingStation Type Decompilation (Valheim 1.0.7)

**File**: `/mnt/c/Program Files (x86)/Steam/steamapps/common/Valheim/valheim_Data/Managed/assembly_valheim.dll`  
**Type**: CookingStation  
**Status**: Found and decompiled successfully (1007 lines)

## Members

### Fields

| Name | Signature | Line |
|------|-----------|------|
| m_slots | public Transform[] m_slots; | 43 |
| m_conversion | public List<ItemConversion> m_conversion = new List<ItemConversion>(); | 100 |
| m_requireFire | public bool m_requireFire = true; | 105 |
| m_useFuel | public bool m_useFuel; | 112 |
| m_maxFuel | public int m_maxFuel = 10; | 118 |
| m_secPerFuel | public int m_secPerFuel = 5000; | 120 |
| m_overCookedItem | public ItemDrop m_overCookedItem; | 40 |

### Nested Type: ItemConversion

| Field | Signature | Line |
|-------|-----------|------|
| m_from | public ItemDrop m_from; | 13 |
| m_to | public ItemDrop m_to; | 15 |
| m_cookTime | public float m_cookTime = 10f; | 17 |

### Methods

| Name | Signature | Line |
|------|-----------|------|
| GetSlot | private void GetSlot(int slot, out string itemName, out float cookedTime, out Status status, out bool cheated) | 825-841 |
| IsFireLit | private bool IsFireLit() | 719-745 |
| GetFuel | private float GetFuel() | 901-904 |
| GetHoverText | public string GetHoverText() | 543-550 |
| IsItemAllowed (overload 1) | private bool IsItemAllowed(ItemDrop.ItemData item) | 867-870 |
| IsItemAllowed (overload 2) | private bool IsItemAllowed(string itemName) | 872-882 |

## ZDO Keys and Variables

### ZDOVars Hashes

- `ZDOVars.s_cheated` - Read/Write at lines 182, 225
- `ZDOVars.s_startTime` - Read/Write at lines 324, 326
- `ZDOVars.s_cheatedQueued` - Read/Write at lines 821, 839 (used with slot concatenation)
- `ZDOVars.s_fuel` - Read/Write at lines 898, 903

### String Keys

- `"slot" + slot_number` - Stores item name and cooked time (lines 818-819, 836-837, 847, 859)
- `"slotstatus" + slot_number` - Stores cooking status (lines 820, 838)

## Notes

- All requested members exist in the type
- GetSlot signature confirms it uses `out` parameters for all return values
- ItemConversion is a nested serializable class with m_from, m_to, and m_cookTime
- IsItemAllowed has two overloads (ItemDrop.ItemData and string variants)
- Fire lit status is checked via EffectArea burning checks
- Fuel is stored as a float in ZDO under s_fuel key
