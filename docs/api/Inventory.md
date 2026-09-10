# Vanilla Valheim 1.0.7 - Inventory Type

## Requested Members

### Constructors

| Member | Signature | Line |
|--------|-----------|------|
| Constructor 1 | `public Inventory(string name, Sprite bkg, int w, int h)` | 24 |
| Constructor 2 | `public Inventory(bool _)` | 33 |

### Fields

| Member | Signature | Line |
|--------|-----------|------|
| m_inventory | `private List<ItemDrop.ItemData> m_inventory = new List<ItemDrop.ItemData>()` | 14 |

### Methods

| Member | Signature | Line |
|--------|-----------|------|
| GetAllItems | `public List<ItemDrop.ItemData> GetAllItems()` | 686 |
| GetEmptySlots | `public int GetEmptySlots()` | 586 |
| GetWidth | `public int GetWidth()` | 773 |
| GetHeight | `public int GetHeight()` | 778 |
| NrOfItems | `public int NrOfItems()` | 557 |
| GetTotalWeight | `public float GetTotalWeight()` | 1141 |

## Missing Members

None - all requested members exist.

## ZDO Key Access

No ZDOVars or ZDO key access detected in this type's decompiled methods.

## Source

Decompiled from: `/mnt/c/Program Files (x86)/Steam/steamapps/common/Valheim/valheim_Data/Managed/assembly_valheim.dll`
Valheim version: 1.0.7
