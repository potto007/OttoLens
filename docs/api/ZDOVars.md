# Vanilla Valheim 1.0.7 ZDOVars Type Analysis

## Type Information
- **Assembly**: assembly_valheim.dll
- **Namespace**: (default/root)
- **Type**: ZDOVars (static class)
- **Status**: FOUND

## Static Hash Fields (Requested Keywords)

### Fields Containing "fuel"
- Line 109: `public static readonly int s_fuel = "fuel".GetStableHashCode();`

### Fields Containing "health"
- Line 119: `public static readonly int s_health = "health".GetStableHashCode();`

### Fields Containing "support"
- Line 357: `public static readonly int s_support = "support".GetStableHashCode();`

### Fields Containing "level"
- Line 173: `public static readonly int s_level = "level".GetStableHashCode();`

### Fields Containing "picked"
- Line 9: `public static readonly int s_pickedTime = "picked_time".GetStableHashCode();`
- Line 209: `public static readonly int s_picked = "picked".GetStableHashCode();`

### Fields Containing "item"
- Line 141: `public static readonly int s_item = "item".GetStableHashCode();`
- Line 143: `public static readonly int s_item0 = "item0".GetStableHashCode();`
- Line 145: `public static readonly int s_itemData = "itemData".GetStableHashCode();`
- Line 147: `public static readonly int s_itemPrefab = "itemPrefab".GetStableHashCode();`
- Line 149: `public static readonly int s_itemStack = "itemStack".GetStableHashCode();`
- Line 151: `public static readonly int s_items = "items".GetStableHashCode();`

### Fields Containing "queue"
- Line 231: `public static readonly int s_queued = "queued".GetStableHashCode();`

### Fields Containing "bake"
- Line 53: `public static readonly int s_bakeTimer = "bakeTimer".GetStableHashCode();`

### Fields Containing "content"
- Line 67: `public static readonly int s_content = "Content".GetStableHashCode();`

### Fields Containing "StartTime"
- Line 17: `public static readonly int s_startTime = "StartTime".GetStableHashCode();`

### Fields Containing "plantTime"
- Line 11: `public static readonly int s_plantTime = "plantTime".GetStableHashCode();`

### Fields Containing "product"
- Line 225: `public static readonly int s_product = "product".GetStableHashCode();`

### Fields Containing "ore"
- Line 281: `public static readonly int s_spawnOre = "SpawnOre".GetStableHashCode();`

### Fields Containing "done"
- Line 87: `public static readonly int s_done = "done".GetStableHashCode();`

### Fields Containing "ammo"
- Line 39: `public static readonly int s_ammo = "ammo".GetStableHashCode();`
- Line 41: `public static readonly int s_ammoType = "ammoType".GetStableHashCode();`

### Fields Containing "TCData"
- Line 29: `public static readonly int s_TCData = "TCData".GetStableHashCode();`

### Fields Containing "tamed"
- Line 297: `public static readonly int s_tamed = "tamed".GetStableHashCode();`

### Fields Containing "tameTimeLeft"
- Line 21: `public static readonly int s_tameTimeLeft = "TameTimeLeft".GetStableHashCode();`

### Fields Containing "TameLastFeeding"
- Line 19: `public static readonly int s_tameLastFeeding = "TameLastFeeding".GetStableHashCode();`

### Fields Containing "hunger"
- **NONE FOUND**

## Missing Members
- `hunger` - closest real members: `saturation` (line 255)

## Other Notable Static Members
- Line 365-369: `public static readonly List<int> s_sessionHashes` - collection of session-related hash values
- Lines 413-423: `KeyValuePair<int, int>` fields for composite ZDOID keys:
  - `s_zdoidUser`
  - `s_zdoidRodOwner`
  - `s_sessionCatchID`
  - `s_toRemoveTarget`
  - `s_toRemoveSpawnID`
  - `s_toRemoveParentID`

## ZDO Key Strings (All s_* Hashes)

All keys referenced through GetStableHashCode() calls (extracted from field initialization):

- "alive_time", "lastTime", "picked_time", "plantTime", "pregnant", "spawntime", "StartTime", "TameLastFeeding", "TameTimeLeft", "timeOfDeath", "lastWorldTime", "0_item", "TCData", "accTime", "addedDefaultItems", "adrenaline", "aggravated", "ammo", "ammoType", "attachJoint", "Attackers", "author", "authorPlatformDisplayName", "Bait", "bakeTimer", "baseValue", "BeardItem", "bosscount", "cheated", "cheatedQueued", "ChestItem", "Content", "creator", "creatorIndex", "creatorName", "crowned", "data", "dead", "DebugFly", "DespawnInDay", "dodgeinv", "done", "drops", "eitr", "emote", "emoteID", "emote_oneshot", "enabled", "escape", "EventCreature", "follow", "forward", "fuel", "GrowStart", "HairColor", "HairItem", "HaveSaddle", "health", "HelmetItem", "HitDir", "HitPoint", "hooked", "Hue", "huntplayer", "inBed", "inWater", "InitVel", "IsBlocking", "item", "item0", "itemData", "itemPrefab", "itemStack", "items", "junkData", "lastAttack", "LastSpawn", "LeftBackItem", "LeftBackItemVariant", "LeftBackItemQuality", "LeftItem", "LeftItemVariant", "LeftItemQuality", "LegItem", "level", "LiquidData", "location", "Locked", "lovePoints", "max_health", "MaxInstances", "ModelIndex", "Modifiers", "offset", "owner", "ownerName", "ownerZDOUserId", "ownerZDOId", "patrol", "patrolPoint", "permitted", "pickable", "picked", "piece", "played", "playerID", "playerName", "plays", "pose", "preSnow", "product", "pvp", "quality", "queued", "RandomSkillFactor", "relPos", "relRot", "RightBackItem", "RightBackItemQuality", "RightItem", "RightItemQuality", "rodOwner", "roomData", "rooms", "rudder", "Saturation", "scale", "scaleScalar", "seAttrib", "seed", "ShoulderItem", "ShoulderItemVariant", "ShoulderItemQuality", "ShownAlertMessage", "SkinColor", "sleeping", "snow", "SpawnAmount", "SpawnOre", "spread", "stamina", "state", "Stealth", "switch", "tag", "tagauthor", "tamed", "TamedName", "TamedNameAuthor", "targets", "text", "triggered", "TrinketItem", "type", "user", "UtilityItem", "Value", "variant", "wakeup", "WeaponLoaded", "visual", "overrideHoverName", "noHair", "noBeard", "count", "terrainModifierTimeCreated", "alert", "animation_speed", "body_avel", "body_vel", "BodyVelocity", "haveTarget", "InUse", "landed", "LookTarget", "noise", "support", "tiltrot", "vel", "velRel", "autoDespawn", "catchID", "cut_time", "generated", "inBase", "LookDir", "patrolSpawnPoint", "RideSpeed", "targetHear", "targetSee", "durability", "stack", "worldLevel", "crafterID", "crafterName", "dataCount", "pickedUp", "spawnpoint", "SpawnPoint", "SpawnTime", "spawn_time", "user_u", "user_i", "RodOwner_u", "RodOwner_i", "CatchID_u", "CatchID_i", "target_u", "target_i", "spawn_id_u", "spawn_id_i", "parent_id_u", "parent_id_i"

## Notes

- ZDOVars is a static utility class containing only readonly fields and hash constants
- No methods are defined; the class serves as a centralized hash registry for ZDO data keys
- All hashes are computed via GetStableHashCode() on string literals (ZDO keys)
- Composite ZDOID fields use KeyValuePair<int, int> to store user/id hash pairs
- The s_sessionHashes list groups transient session-related keys for bulk operations
