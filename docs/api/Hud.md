# Valheim 1.0.7 Hud Type Analysis

**File**: assembly_valheim.dll (decompiled with ilspycmd)

## Overview
The `Hud` class is a `MonoBehaviour` that manages all HUD UI elements for Valheim gameplay, including health, stamina, crosshair, piece selection, and various status indicators.

## Requested Members

### Crosshair and Hover Name TMP Fields
- **m_crosshair** (line 240): `public Image m_crosshair;`
- **m_crosshairBow** (line 242): `public Image m_crosshairBow;`
- **m_hoverName** (line 244): `public TextMeshProUGUI m_hoverName;` - **TMP field for crosshair hover text**

### Root Object
- **m_rootObject** (line 28): `public GameObject m_rootObject;`

### Hover Name Field
- **m_hoverName** (line 244): `public TextMeshProUGUI m_hoverName;`

### Method That Updates Crosshair Hover Text
- **UpdateCrosshair** (line 818): `private void UpdateCrosshair(Player player, float bowDrawPercentage)`
  - Called each frame from `Update()` (line 537)
  - Updates `m_hoverName.text` at line 838 with hover object text
  - Updates `m_crosshair.color` based on hover state (lines 839-844)

### Instance Property
- **m_instance** (line 26): `private static Hud m_instance;`
- **instance** (line 368): `public static Hud instance => m_instance;`
  - Property to access the singleton instance

## Key Implementation Details

### UpdateCrosshair Method Behavior (lines 818-877)
1. Checks if player is attached to object (line 820)
2. Manages active state of crosshair image (lines 822-827)
3. Gets hover object and its Hoverable component (lines 828-829)
4. Updates hover text from Hoverable.GetHoverText() (line 832)
5. Sets crosshair color to yellow if hover text exists, white with 0.5 alpha if not (line 839)
6. Shows piece health bar if hovering over piece with WearNTear component (lines 846-865)
7. Handles bow crosshair display based on draw percentage (lines 866-876)

### Hover Text Update Location
Line 838: `m_hoverName.text = text;`

## ZDOVars Usage
**No ZDOVars references found** - The Hud class does not directly read or write to ZDO (Zonewide Data Objects). It is purely a UI/HUD display layer that responds to player and game state.

## All Found Members (Relevant Subset)

| Field/Property | Type | Declaration |
|---|---|---|
| m_rootObject | GameObject | `public GameObject m_rootObject;` |
| m_crosshair | Image | `public Image m_crosshair;` |
| m_crosshairBow | Image | `public Image m_crosshairBow;` |
| m_hoverName | TextMeshProUGUI | `public TextMeshProUGUI m_hoverName;` |
| m_instance | Hud | `private static Hud m_instance;` |
| instance | Hud | `public static Hud instance => m_instance;` |
| UpdateCrosshair | method | `private void UpdateCrosshair(Player player, float bowDrawPercentage)` |

## Missing Members
None - all requested members exist in the type.
