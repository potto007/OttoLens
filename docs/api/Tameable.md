# Tameable Type - Valheim 1.0.7 Decompilation

## Type Information

- **Namespace**: Global (public class)
- **Base**: `MonoBehaviour, Interactable, TextReceiver`
- **File**: assembly_valheim.dll
- **Exists**: Yes

## Requested Members

### Methods

| Name | Signature | Line |
|------|-----------|------|
| GetRemainingTime | `private float GetRemainingTime()` | 602-609 |
| GetTameness | `private int GetTameness()` | 563-567 |
| IsHungry | `public bool IsHungry()` | 530-547 |
| GetHoverText | `public string GetHoverText()` | 121-147 |
| GetStatusString | `public string GetStatusString()` | 149-164 |

### Fields

| Name | Signature | Line |
|------|-----------|------|
| m_tamingTime | `public float m_tamingTime = 1800f;` | 18 |
| m_nview | `private ZNetView m_nview;` | 70 |
| m_character | `private Character m_character;` | 64 |
| m_monsterAI | `private MonsterAI m_monsterAI;` | 66 |

## Missing Members

- **GetLevel**: Not found in type. Closest members: `GetRiderSkill()` (line 620)

## ZDO Keys Used

The following ZDOVars hashes/string keys are read or written by Tameable:

- `ZDOVars.s_tamedName` - Tamed creature custom name
- `ZDOVars.s_tamedNameAuthor` - Author of the tamed creature's name
- `ZDOVars.s_haveSaddleHash` - Boolean flag for saddle presence
- `ZDOVars.s_follow` - Name of player being followed
- `ZDOVars.s_maxInstances` - Max concurrent instances of same creature
- `ZDOVars.s_tameLastFeeding` - Timestamp of last feeding
- `ZDOVars.s_tameTimeLeft` - Remaining taming time in seconds

## Key Behaviors

- **Taming**: Creature progresses toward tamed state over `m_tamingTime` (1800s = 30 min default)
- **Feeding**: Resets hunger timer via `s_tameLastFeeding`
- **Following**: Players command tamed creatures via `s_follow` ZDO key
- **Saddles**: Track saddle state with `s_haveSaddleHash`
- **Naming**: Store custom pet names in `s_tamedName` with author in `s_tamedNameAuthor`
