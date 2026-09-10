# Valheim 1.0.7 Fermenter Type Analysis

Decompiled from: `assembly_valheim.dll`

## Requested Members

### Fields

| Name | Signature |
|------|-----------|
| `m_conversion` | `public List<ItemConversion> m_conversion = new List<ItemConversion>();` |
| `m_fermentationDuration` | `public float m_fermentationDuration = 2400f;` |
| `m_nview` | `private ZNetView m_nview;` |

### Methods

| Name | Signature |
|------|-----------|
| `GetContent` | `private int GetContent()` |
| `GetStatus` | `private Status GetStatus()` |
| `GetFermentationTime` | `private double GetFermentationTime()` |
| `GetHoverText` | `public string GetHoverText()` |
| `GetItemConversion` | `private ItemConversion GetItemConversion(int nameHash)` |

## ZDO Keys (ZDOVars)

The Fermenter type reads and writes the following ZDO variables:

| Key | Operations | Description |
|-----|-----------|-------------|
| `ZDOVars.s_content` | Get (GetInt), Set | Stores the item hash being fermented |
| `ZDOVars.s_startTime` | Get (GetLong), Set | Stores fermentation start time (DateTime.Ticks) |
| `ZDOVars.s_cheatedQueued` | Get (GetBool), Set | Tracks if the item was obtained via cheats |
| `ZDOVars.s_cheated` | Get (GetBool) | Reads cheat status from container |

## Missing Members

None - all requested members exist in the type.

## Type Structure

- Base class: `MonoBehaviour`
- Interfaces: `Hoverable`, `Interactable`
- Inner class: `ItemConversion` (serializable)
- Inner enum: `Status` (Empty, Fermenting, Exposed, Ready)

