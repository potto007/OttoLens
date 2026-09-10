# TreeBase Type - Valheim 1.0.7

## Type Declaration
```
public class TreeBase : MonoBehaviour, IDestructible
```

## Requested Members

### m_health
**Line 9**: `public float m_health = 1f;`

### m_minToolTier
**Line 13**: `public int m_minToolTier;`

### m_nview
**Line 7**: `private ZNetView m_nview;`

### m_logPrefab
**Line 25**: `public GameObject m_logPrefab;`

### GetHoverText
**Missing** - This method does not exist in TreeBase.
Closest real method: `public DestructibleType GetDestructibleType()` (line 57)

## ZDO Keys Used

The TreeBase type reads and writes the following ZDO variables:

- **ZDOVars.s_health** - Used for storing tree health value
  - Read operations (GetFloat): Lines 51, 107
  - Write operations (Set): Line 128

## Member Signatures

All members from the requested list that exist:

- `private ZNetView m_nview;` (line 7)
- `public float m_health = 1f;` (line 9)
- `public int m_minToolTier;` (line 13)
- `public GameObject m_logPrefab;` (line 25)

## Additional Context

- The health value is read with a fallback calculation in Awake() that includes world level multiplier
- Health is decremented on damage and checked against zero for destruction
- The m_logPrefab is instantiated in the SpawnLog() method when the tree is destroyed
