# Feast Type (Valheim 1.0.7)

## Type Location
- **File**: assembly_valheim.dll
- **Status**: FOUND
- **Class**: `Feast : MonoBehaviour, Hoverable, Interactable`

## Key Fields

### Remaining Eats Tracking
- **Field**: `int GetStack()` (method at line 113-120)
  - Returns remaining stacks from ZDO, default to `m_eatStacks`
  - ZDO Key: `ZDOVars.s_value` (int)

### Stack Visual State
- **Field**: `float GetStackPercentige()` (method at line 122-125)
  - Returns `(float)Mathf.Max(GetStack(), 0) / (float)m_eatStacks`
  - Used to determine which visual parts are active

### HoverText Method
- **Method**: `public string GetHoverText()` (line 132-144)
  - Checks if stack > 0; returns empty string if empty
  - Checks distance; returns "$piece_toofar" if too far
  - Returns formatted string: `GetHoverName() + "\n[<color=yellow><b>$KEY_Use</b></color>] $item_eat ( {stack}/{m_eatStacks} )"`
  - Signature: `public string GetHoverText()`

## Important Fields for Configuration
- `public int m_eatStacks = 5` (line 19) - max food items in feast
- `public float m_useDistance = 2f` (line 21) - interaction range
- `public ItemDrop m_foodItem` (line 23) - food item reference
- `public List<FeastLevel> m_feastParts` (line 25) - visual state objects
- `public EffectList m_eatEffect` (line 27) - eat effect
- `public float m_hoverOffset` (line 29) - hover text offset

## ZDO Variables Used
- **`ZDOVars.s_value`**: Stores remaining stack count
  - Written in `RPC_TryEat()` (lines 82, 86)
  - Read in `GetStack()` (line 117)

## Related Nested Class
- **`FeastLevel`** (lines 8-17): Holds visual state thresholds
  - `GameObject m_onAboveEquals`
  - `GameObject m_onBelow`
  - `float m_threshold`
  - `float m_thresholdBelowMax`
