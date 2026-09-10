# Valheim 1.0.7 WearNTear Type Reference

Decompiled from: `/mnt/c/Program Files (x86)/Steam/steamapps/common/Valheim/valheim_Data/Managed/assembly_valheim.dll`

## Members Found

### Fields

| Name | Signature | Line |
|------|-----------|------|
| `m_health` | `public float m_health = 100f;` | 108 |
| `m_supports` | `public bool m_supports = true;` | 79 |
| `m_materialType` | `public MaterialType m_materialType;` | 77 |
| `m_nview` | `private ZNetView m_nview;` | 169 |

### Methods

| Name | Signature | Line |
|------|-----------|------|
| `GetHealthPercentage` | `public float GetHealthPercentage()` | 1162 |
| `GetSupport` | `private float GetSupport()` | 411 |
| `GetMaxSupport` | `private float GetMaxSupport()` | 1347 |
| `GetMinSupport` | `private float GetMinSupport()` | 1353 |
| `IsWet` | `public bool IsWet()` | 799 |

## Missing Members

The following requested members were not found:
- `GetRemainingHealth` - No method with this name exists

Closest alternatives:
- `GetHealthPercentage()` returns the current health as a percentage (0-1)
- `ApplyDamage()` method at line 1248 handles damage application and health reduction

## ZDO Keys Used

The following ZDOVars keys are read or written by WearNTear methods:

| ZDO Key | Usage | Lines |
|---------|-------|-------|
| `ZDOVars.s_health` | Get/Set current health value | 289, 293, 389, 406, 1195, 1211, 1250, 1256, 1295 |
| `ZDOVars.s_support` | Get/Set current support value | 425, 536, 915, 957, 987, 1296 |
| `ZDOVars.s_snow` | Get/Set snow buildup value | 330, 607, 694, 766, 768 |
| `ZDOVars.s_preSnow` | Get/Set pre-snow flag | 329, 601 |

## MaterialType Enum

```csharp
public enum MaterialType
{
    Wood,
    Stone,
    Iron,
    HardWood,
    Marble,
    Ashstone,
    Ancient,
    Ice,
    Timberwood
}
```

## Key Methods Analyzed

- **GetHealthPercentage()**: Returns health percentage (0-1 clamped) from ZDO or 1.0 if invalid
- **GetSupport()**: Returns current support value, reads from ZDO key `s_support` if owner, otherwise from `m_support` private field
- **GetMaxSupport()**: Calls `GetMaterialProperties()` to retrieve max support based on `m_materialType`
- **GetMinSupport()**: Calls `GetMaterialProperties()` to retrieve min support based on `m_materialType`
- **IsWet()**: Returns true if raining without roof (`m_rainWet`) or underwater
