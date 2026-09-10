# Beehive Type Analysis (Valheim 1.0.7)

## Basic Info
- **Type**: Beehive (public class, extends MonoBehaviour, implements Hoverable and Interactable)
- **Assembly**: assembly_valheim.dll
- **Decompilation successful**: Yes (232 lines)

## Requested Members

### Field: m_maxHoney
```csharp
public int m_maxHoney = 4;
```
- **Kind**: field
- **Type**: int
- **Default**: 4

### Field: m_secPerUnit
```csharp
public float m_secPerUnit = 10f;
```
- **Kind**: field
- **Type**: float
- **Default**: 10f

### Method: GetHoneyLevel
```csharp
private int GetHoneyLevel()
```
- **Kind**: method
- **Signature**: `private int GetHoneyLevel()`
- **Return type**: int
- **Parameters**: none
- **Lines**: 184-191

### Method: CheckBiome
```csharp
private bool CheckBiome()
```
- **Kind**: method
- **Signature**: `private bool CheckBiome()`
- **Return type**: bool
- **Parameters**: none
- **Lines**: 223-226

### Method: HaveFreeSpace
```csharp
private bool HaveFreeSpace()
```
- **Kind**: method
- **Signature**: `private bool HaveFreeSpace()`
- **Return type**: bool
- **Parameters**: none
- **Lines**: 213-221

### Method: GetHoverText
```csharp
public string GetHoverText()
```
- **Kind**: method
- **Signature**: `public string GetHoverText()`
- **Return type**: string
- **Parameters**: none
- **Lines**: 74-86

### Field: m_nview
```csharp
private ZNetView m_nview;
```
- **Kind**: field
- **Type**: ZNetView
- **Access**: private

### Field: m_biome
```csharp
[BitMask(typeof(Heightmap.Biome))]
public Heightmap.Biome m_biome;
```
- **Kind**: field
- **Type**: Heightmap.Biome
- **Attribute**: BitMask

## ZDO Keys Used

The Beehive class reads and writes the following ZDOVars keys:

| Key | Usage | Type | Operations |
|-----|-------|------|------------|
| `ZDOVars.s_lastTime` | Timestamp of last update | long | GetLong (lines 65, 159), Set (lines 67, 162) |
| `ZDOVars.s_level` | Current honey level (0 to m_maxHoney) | int | GetInt (line 190), Set (lines 173, 181) |
| `ZDOVars.s_product` | Accumulated production time | float | GetFloat (line 201), Set (line 209) |

## Missing Members
None of the requested members are missing. All requested fields and methods exist in the decompiled type.
