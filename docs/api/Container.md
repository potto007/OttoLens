# Valheim 1.0.7 Container Type - API Verification

**File:** `/mnt/c/Program Files (x86)/Steam/steamapps/common/Valheim/valheim_Data/Managed/assembly_valheim.dll`  
**Decompiled with:** ilspycmd  
**Line count:** 507

## Type Declaration
```csharp
public class Container : MonoBehaviour, Hoverable, Interactable
```

## Requested Members

### Methods

| Name | Signature | Line | Status |
|------|-----------|------|--------|
| GetInventory | `public Inventory GetInventory()` | 305-308 | ✓ exists |
| GetHoverText | `public string GetHoverText()` | 202-211 | ✓ exists |
| CheckAccess | `private bool CheckAccess(long playerID)` | 266-275 | ✓ exists (private) |
| IsOwner | `public bool IsOwner()` | 277-280 | ✓ exists |
| IsInUse | `public bool IsInUse()` | 282-285 | ✓ exists |

### Fields

| Name | Type | Signature | Line | Status |
|------|------|-----------|------|--------|
| m_nview | ZNetView | `private ZNetView m_nview;` | 54 | ✓ exists |
| m_name | string | `public string m_name = "Container";` | 18 | ✓ exists |
| m_width | int | `public int m_width = 3;` | 22 | ✓ exists |
| m_height | int | `public int m_height = 2;` | 24 | ✓ exists |
| m_privacy | PrivacySetting | `public PrivacySetting m_privacy = PrivacySetting.Public;` | 26 | ✓ exists |

## Missing Members
None - all requested members exist.

## ZDO Keys Accessed

The Container type reads and writes the following ZDOVars constants:

| ZDO Key | Operations | Lines |
|---------|-----------|-------|
| `ZDOVars.s_addedDefaultItems` | Read (GetBool), Write (Set) | 93, 96 |
| `ZDOVars.s_cheated` | Read (GetBool) | 106 |
| `ZDOVars.s_inUse` | Read (GetInt), Write (Set) | 172, 176 |
| `ZDOVars.s_items` | Read (GetByteArray), Write (Set) | 475, 490 |

### Dynamic Keys
- Line 238-241: Discovered stat tracking via `str.GetStableHashCode()` (Discovered_[PlayerName])
- Line 249: Generic hash key from RPC parameter (int keyHash)

## Nested Type
- `enum PrivacySetting` (lines 7-12): Private, Group, Public

## Method Signatures (Full Context)

```csharp
// Line 202-211
public string GetHoverText()

// Line 266-275
private bool CheckAccess(long playerID)

// Line 277-280
public bool IsOwner()

// Line 282-285
public bool IsInUse()

// Line 305-308
public Inventory GetInventory()
```
