# Valheim 1.0.7 ZDO Type Decompilation

**Source:** assembly_valheim.dll via ilspycmd  
**Status:** Type found and decompiled successfully

## Type Definition

```
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class ZDO : IEquatable<ZDO>
```

## Requested Members

### GetFloat Overloads

| Signature | Line |
|-----------|------|
| `public float GetFloat(string name, float defaultValue = 0f)` | 598 |
| `public float GetFloat(int hash, float defaultValue = 0f)` | 603 |
| `public bool GetFloat(string name, out float value)` | 608 |
| `public bool GetFloat(int hash, out float value)` | 613 |

### GetInt Overloads

| Signature | Line |
|-----------|------|
| `public int GetInt(string name, int defaultValue = 0)` | 658 |
| `public int GetInt(int hash, int defaultValue = 0)` | 663 |
| `public bool GetInt(string name, out int value)` | 668 |
| `public bool GetInt(int hash, out int value)` | 673 |

### GetLong Overloads

| Signature | Line |
|-----------|------|
| `public long GetLong(string name, long defaultValue = 0L)` | 698 |
| `public long GetLong(int hash, long defaultValue = 0L)` | 703 |

### GetString Overloads

| Signature | Line |
|-----------|------|
| `public string GetString(string name, string defaultValue = "")` | 708 |
| `public string GetString(int hash, string defaultValue = "")` | 713 |
| `public bool GetString(string name, out string value)` | 718 |
| `public bool GetString(int hash, out string value)` | 723 |

### GetBool Overloads

| Signature | Line |
|-----------|------|
| `public bool GetBool(string name, bool defaultValue = false)` | 678 |
| `public bool GetBool(int hash, bool defaultValue = false)` | 683 |
| `public bool GetBool(string name, out bool value)` | 688 |
| `public bool GetBool(int hash, out bool value)` | 693 |

### IsOwner

| Signature | Line |
|-----------|------|
| `public bool IsOwner()` | 1363 |

### IsValid

| Signature | Line |
|-----------|------|
| `public bool IsValid()` | 263 |

## ZDOVars Keys Referenced

The following ZDOVars static hash/string fields are read or written by ZDO methods:

| Key | Context | Line |
|-----|---------|------|
| `ZDOVars.s_SpawnTime__DontUse` | Legacy spawn time (obsolete) | 1133 |
| `ZDOVars.s_spawn_time__DontUse` | Legacy spawn time (obsolete) | 1133 |
| `ZDOVars.s_spawnTime` | Current spawn time field | 1135 |
| `ZDOVars.s_SpawnPoint__DontUse` | Legacy spawn point (obsolete) | 1143 |
| `ZDOVars.s_spawnPoint` | Current spawn point field | 1145 |

All referenced in `StripConvert` methods for backward-compatible data migration during load.

## Key Observations

- No GetLong `out` parameter overload exists (unlike GetFloat, GetInt, GetString, GetBool)
- Getters delegate to `ZDOExtraData` static methods
- Setters call `ZDOExtraData.Set()` and trigger `IncreaseDataRevision()`
- ZDOVars references appear only in legacy format conversion (`LoadOldFormat`, `StripConvert`)
- All Get methods support both string name and int hash variants
