# Valheim Character Type API (v1.0.7)

**Assembly:** assembly_valheim.dll  
**Decompiled with:** ilspycmd

## Overview
Character is the base class for all character entities in Valheim, implementing IDestructible, Hoverable, IWaterInteractable, and IMonoUpdater interfaces.

## Members

### Fields

#### m_name
```csharp
public string m_name = "";
```
- Line: 79
- Type: String field
- Default: empty string
- Description: Character name, serialized to UI

### Methods

#### GetHealth()
```csharp
public float GetHealth()
```
- Line: 2992
- Return type: float
- Parameters: none
- Description: Returns current health from ZDO or max health if not found
- ZDO Key: `ZDOVars.s_health`

#### GetMaxHealth()
```csharp
public float GetMaxHealth()
```
- Line: 3059
- Return type: float
- Parameters: none
- Description: Returns max health from ZDO or default m_health if not found
- ZDO Key: `ZDOVars.s_maxHealth`

#### GetLevel()
```csharp
public int GetLevel()
```
- Line: 789
- Return type: int
- Parameters: none
- Description: Returns current character level (minimum 1)

#### IsTamed()
```csharp
public bool IsTamed()
```
- Line: 4335
- Return type: bool
- Parameters: none
- Description: Returns whether the character is tamed (checks m_tamed field with time parameter)
- ZDO Key: `ZDOVars.s_tamed`

#### GetHoverName()
```csharp
public virtual string GetHoverName()
```
- Line: 3857
- Return type: string
- Parameters: none
- Modifiers: virtual
- Description: Returns display name for hover text, checks for Tameable component first
- ZDO Key: `ZDOVars.s_overrideHoverName`

#### GetHoverText()
```csharp
public virtual string GetHoverText()
```
- Line: 3847
- Return type: string
- Parameters: none
- Modifiers: virtual
- Description: Returns hover text display, checks for Tameable component first

## ZDO Variables Read/Written

The Character type accesses the following ZDOVars keys:

- `ZDOVars.s_health` - Current health value
- `ZDOVars.s_maxHealth` - Maximum health value
- `ZDOVars.s_level` - Character level
- `ZDOVars.s_tamed` - Tamed status flag
- `ZDOVars.s_overrideHoverName` - Override for hover display name
- `ZDOVars.s_attackers` - List of attackers
- `ZDOVars.s_bodyVelocity` - Rigidbody velocity
- `ZDOVars.s_bossCount` - Boss entity count
- `ZDOVars.s_cheated` - Cheat flag
- `ZDOVars.s_modifiers` - Damage modifiers
- `ZDOVars.s_noise` - Noise level for AI detection
- `ZDOVars.s_randomSkillFactor` - Skill variation factor
- `ZDOVars.s_tiltrot` - Tilt rotation state

## Notes

- All requested members exist in the vanilla Character type
- All ZDO access uses ZDOVars.s_* hashes (no literal string keys found)
- Character is the base class for both players and creatures
- The type uses lazy initialization for health values (defaults to max if not set in ZDO)
