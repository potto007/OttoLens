# ItemDrop Type Analysis - Valheim 1.0.7

**Source:** Decompiled from `/mnt/c/Program Files (x86)/Steam/steamapps/common/Valheim/valheim_Data/Managed/assembly_valheim.dll` using ilspycmd

## ItemData.SharedData Members

| Member | Signature | Line |
|--------|-----------|------|
| m_name | `public string m_name = "";` | 95 |
| m_icons | `public Sprite[] m_icons = Array.Empty<Sprite>();` | 103 |
| m_maxStackSize | `public int m_maxStackSize = 1;` | 112 |

## ItemData Members

| Member | Signature | Line |
|--------|-----------|------|
| m_stack | `public int m_stack = 1;` | 417 |
| m_durability | `public float m_durability = 100f;` | 419 |
| m_quality | `public int m_quality = 1;` | 421 |
| m_variant | `public int m_variant;` | 423 |
| m_gridPos | `public Vector2i m_gridPos = Vector2i.zero;` | 441 |

## ItemData Methods

| Method | Signature | Line |
|--------|-----------|------|
| GetMaxDurability() | `public float GetMaxDurability()` | 685 |
| GetMaxDurability(int) | `public float GetMaxDurability(int quality)` | 690 |
| GetIcon() | `public Sprite GetIcon()` | 798 |

## ItemDrop Members

| Member | Signature | Line |
|--------|-----------|------|
| m_itemData | `public ItemData m_itemData = new ItemData();` | 1210 |

## ItemDrop Methods

| Method | Signature | Line |
|--------|-----------|------|
| GetHoverText() | `public string GetHoverText()` | 1486 |

## ZDO Variables and Keys Accessed

### Static ZDOVars References
- **ZDOVars.s_spawnTime** - Used in GetLong/Set operations (lines 1288, 1290, 1338)
- **ZDOVars.s_piece** - Used in GetBool/Set operations (lines 1295, 1414)
- **ZDOVars.s_itemData** - Used in GetByteArray/Set operations (lines 1766, 1791, 1795)
- **ZDOVars.s_quality** - Used in GetInt/Set operations (lines 1780, 1784)
- **ZDOVars.s_variant** - Used in GetInt/Set operations (lines 1781, 1788)

### Dynamic Key Patterns
- `(index + "_itemData").GetStableHashCode()` - Used when index >= 0 in LoadFromZDO/SaveToZDO methods (lines 1766, 1791)

## Missing Members
None - all requested members exist in the type.

## Notes
- ItemData is a nested [Serializable] class within ItemDrop
- SharedData is a nested [Serializable] class within ItemData
- ItemData encapsulates item instance state (stack, quality, variant, durability, position)
- SharedData contains template/configuration data shared across all instances of an item type
- GetIcon() returns the sprite at index m_variant from m_icons array
- GetMaxDurability methods calculate durability based on SharedData template and quality level
