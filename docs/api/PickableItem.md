# PickableItem Type Analysis (Valheim 1.0.7)

## File
`/tmp/claude-1000/-home-potto-src-valheim-mods-ZenMods-Valheim/c12f2d67-1094-4240-96ec-88fcc2d10bca/scratchpad/api/PickableItem.cs`

## Type Declaration
```
public class PickableItem : MonoBehaviour, Hoverable, Interactable
```

## Requested Members

### m_itemPrefab
- **Line 16**
- **Signature:** `public ItemDrop m_itemPrefab;`
- **Kind:** field
- **Exists:** Yes

### m_stack
- **Line 18**
- **Signature:** `public int m_stack;`
- **Kind:** field
- **Exists:** Yes

### GetHoverText()
- **Line 75**
- **Signature:** `public string GetHoverText()`
- **Kind:** method
- **Exists:** Yes
- **Returns:** string
- **Parameters:** (none)

### GetStackSize()
- **Line 137**
- **Signature:** `private int GetStackSize()`
- **Kind:** method
- **Exists:** Yes
- **Returns:** int
- **Parameters:** (none)
- **Note:** This is a private method, not public

## ZDO Variables Used

The following `ZDOVars` keys are read or written by this type's methods:

- **ZDOVars.s_itemPrefab**
  - Read at line 49 (GetInt call in SetupRandomPrefab)
  - Written at line 58 (Set call in SetupRandomPrefab)

- **ZDOVars.s_itemStack**
  - Written at line 59 (Set call in SetupRandomPrefab)
  - Read at line 71 (GetInt call in SetupRandomPrefab)

## Summary

All four requested members exist in the PickableItem class. The type uses a ZNetView component to persist item prefab hash and stack size to the ZDO (Zeppelin Data Object). The GetStackSize() method is private, not public, and performs clamping based on the item's max stack size and the current game resource rate multiplier.
