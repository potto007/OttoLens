# ZNet Type Specification (Valheim 1.0.7)

## Type
`ZNet : MonoBehaviour`

## Members

### Properties

| Name | Signature |
|------|-----------|
| instance | `public static ZNet instance => m_instance;` |

### Methods

| Name | Signature |
|------|-----------|
| IsServer | `public bool IsServer()` |
| IsDedicated | `public bool IsDedicated()` |
| GetTime | `public DateTime GetTime()` |
| GetTimeSeconds | `public double GetTimeSeconds()` |

## ZDO Key Access

ZNet does not directly read or write ZDO fields using ZDOVars.s_* keys or string-based ZDO field access. The class manages ZDOMan (the ZDO object manager) and stores ZDOID references but does not perform direct field access operations on ZDO objects.

## Implementation Notes

- `instance` is a static property providing singleton access to the ZNet instance
- `IsServer()` returns the value of `m_isServer` field
- `IsDedicated()` always returns `false` in this implementation
- `GetTime()` converts milliseconds of network time to a DateTime object
- `GetTimeSeconds()` returns the raw network time in seconds as a double
