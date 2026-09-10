# ItemStand Type Decompilation

Vanilla Valheim 1.0.7 - assembly_valheim.dll

## Type
Public class `ItemStand : MonoBehaviour, Interactable, Hoverable`

## Requested Members

### m_visualHash (Field)
```csharp
private int m_visualHash;
```
Line 65

### m_nview (Field)
```csharp
private ZNetView m_nview;
```
Line 76

### GetAttachedItem (Method)
```csharp
public int GetAttachedItem()
```
Lines 551-558

### HaveAttachment (Method)
```csharp
public bool HaveAttachment()
```
Lines 542-549

### GetHoverText (Method)
```csharp
public string GetHoverText()
```
Lines 115-157

## ZDO Keys Referenced

The ItemStand type reads and writes the following ZDOVars keys:

- `ZDOVars.s_item` - Read/Write: stores item hash (GetInt/Set)
- `ZDOVars.s_type` - Read/Write: stores orientation type (GetInt/Set)
- `ZDOVars.s_variant` - Read: stores item variant (GetInt)
- `ZDOVars.s_quality` - Read: stores item quality (GetInt, default 1)

### Key Usage Map
- s_item: lines 337, 354, 375, 403, 548, 557 (RPC_DestroyAttachment, DropItem, UpdateAttach, UpdateVisual, HaveAttachment, GetAttachedItem)
- s_type: lines 166, 201, 236, 355 (GetOrientation, Interact, UpdateOrientation, DropItem)
- s_variant: line 404 (UpdateVisual)
- s_quality: line 405 (UpdateVisual)

## Nested Types

- Orientation enum (lines 8-14) - flags for Vertical, Horizontal, All
- OrientationSettings class (lines 17-26) - stores position, rotation, scale, and orientation constraints for item display
