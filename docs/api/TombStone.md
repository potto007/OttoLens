# TombStone Type Analysis (Valheim 1.0.7)

## Type Found
- **Type**: `TombStone : MonoBehaviour, Hoverable, Interactable`
- **Location**: assembly_valheim.dll
- **Lines**: 267 (decompiled)

## Requested Members

### Fields
- **m_container** (line 23)
  - Signature: `private Container m_container;`

- **m_nview** (line 25)
  - Signature: `private ZNetView m_nview;`

### Methods
- **GetOwner()** (line 163)
  - Signature: `private long GetOwner()`
  - Returns: long - the owner's UID from ZDO

- **IsOwner()** (line 172)
  - Signature: `private bool IsOwner()`
  - Returns: bool - true if current player is the owner

- **GetHoverText()** (line 63)
  - Signature: `public string GetHoverText()`
  - Returns: string - the hover text for the tombstone

## ZDO Keys Read/Written

### ZDOVars.s_timeOfDeath
- Read (GetLong): line 43
- Written (Set): line 45

### ZDOVars.s_spawnPoint
- Written (Set): line 46
- Read (GetVec3): line 229

### ZDOVars.s_ownerName
- Read (GetString): lines 53, 60
- Written (Set): line 155

### ZDOVars.s_owner
- Written (Set): line 156
- Read (GetLong): line 167

### ZDOVars.s_inWater
- Written (Set): line 253
- Read (GetBool): line 258

## Missing Members
None - all requested members exist.

## Summary
TombStone is a container object that represents a grave/tomb stone in Valheim. It maintains owner information via ZDO variables, tracks spawn position and time of death, and manages interaction for opening/looting the tombstone. The type manages despawning after loot is taken and applies status effects to the owner.
