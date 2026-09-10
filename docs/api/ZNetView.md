# ZNetView API Analysis - Valheim 1.0.7

## Type Information
- **Type**: `ZNetView`
- **Base Class**: `MonoBehaviour`, `IReferenceHolder`
- **File**: `assembly_valheim.dll` (decompiled)
- **Line Count**: 371 lines

## Requested Members

### Member: GetZDO
- **Status**: ✓ FOUND
- **Signature**: `public ZDO GetZDO()`
- **Location**: Lines 253-256
- **Return Type**: `ZDO`
- **Body**: Returns `m_zdo`

### Member: IsValid
- **Status**: ✓ FOUND
- **Signature**: `public bool IsValid()`
- **Location**: Lines 258-265
- **Return Type**: `bool`
- **Body**: Returns `m_zdo != null && m_zdo.IsValid()`, or `false` if `m_zdo` is `null`

### Member: IsOwner
- **Status**: ✓ FOUND
- **Signature**: `public bool IsOwner()`
- **Location**: Lines 227-234
- **Return Type**: `bool`
- **Body**: Returns `false` if not `IsValid()`, otherwise `m_zdo.IsOwner()`

### Member: ClaimOwnership
- **Status**: ✓ FOUND
- **Signature**: `public void ClaimOwnership()`
- **Location**: Lines 245-251
- **Return Type**: `void`
- **Body**: If not `IsOwner()`, calls `m_zdo.SetOwner(ZDOMan.GetSessionID())`

### Member: HasOwner
- **Status**: ✓ FOUND
- **Signature**: `public bool HasOwner()`
- **Location**: Lines 236-243
- **Return Type**: `bool`
- **Body**: Returns `false` if not `IsValid()`, otherwise `m_zdo.HasOwner()`

## ZDO Key Access Summary

### ZDOVars Hash Keys
- **`ZDOVars.s_scaleHash`**: Read at line 69 (via `GetVec3`), Written at line 128 (via `Set`)
- **`ZDOVars.s_scaleScalarHash`**: Read at line 76 (via `GetFloat`)

### String Keys
- **`"HasFields"`**: Read at line 148 (via `GetBool`)
- **`"HasFields" + <component_name>`**: Read at line 156 (via `GetBool`)
- **Dynamic field names**: Read via `GetInt`, `GetFloat`, `GetBool`, `GetVec3`, `GetString` at lines 169-199 (component field serialization)

## Complete Member Summary

| Member Name | Kind | Signature |
|---|---|---|
| GetZDO | method | `public ZDO GetZDO()` |
| IsValid | method | `public bool IsValid()` |
| IsOwner | method | `public bool IsOwner()` |
| ClaimOwnership | method | `public void ClaimOwnership()` |
| HasOwner | method | `public bool HasOwner()` |

## Notes
- All five requested members are present in the type
- The type uses `ZDO` (Zoned Distributed Object) for networked state
- Ownership is managed via session ID (ZDOMan.GetSessionID())
- Scale synchronization uses both vector form (`s_scaleHash`) and scalar form (`s_scaleScalarHash`)
