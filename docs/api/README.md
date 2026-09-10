# Valheim API Digest

## Summary

### Types Not Found

None - all 34 queried types resolved in the decompiled assembly.

### Commonly Assumed Members That Are Missing

| Type | Assumed / Old Name | Closest Real Name / Note |
|------|--------------------|--------------------------|
| Player | GetRightItem | No such method exists |
| Fireplace | m_fuelPerSecond | `m_secPerFuel` - seconds per fuel unit; invert for burn rate |
| Smelter | GetHoverText | Not present; compose text from queue/fuel getters manually |
| Smelter | m_outputMaxLevel | No such field |
| WearNTear | GetRemainingHealth | `GetHealthPercentage()` - returns 0-1 clamped float |
| TreeBase | GetHoverText | Not present |
| Destructible | GetHoverText | Not present |
| SapCollector | GetStatus | Not present |
| ArmorStand | GetHoverText | Not present |
| Tameable | GetLevel | `GetRiderSkill` is the nearest method; no GetLevel on Tameable |
| MonsterAI | IsAlerted | Inherited from BaseAI; not declared on MonsterAI itself |
| MonsterAI | IsHungry | Use `m_tamable.IsHungry()` instead (line 650 of MonsterAI) |
| Ship | m_shipEffects | Not present |
| Ship | GetHoverText | Not present |
| Ship | cargo container accessor | Not present |
| ZDOVars | s_hunger | No such hash constant; use ZDO key string directly |

### ZDOVars Hashes by Purpose

**Fuel / Energy**
- `ZDOVars.s_fuel` = `"fuel".GetStableHashCode()`

**Health / Structural Integrity**
- `ZDOVars.s_health` = `"health".GetStableHashCode()`
- `ZDOVars.s_support` = `"support".GetStableHashCode()`

**Level / Progress**
- `ZDOVars.s_level` = `"level".GetStableHashCode()`

**Picked / Enabled State**
- `ZDOVars.s_picked` = `"picked".GetStableHashCode()`
- `ZDOVars.s_pickedTime` = `"picked_time".GetStableHashCode()`

**Item Slots**
- `ZDOVars.s_item` = `"item".GetStableHashCode()`
- `ZDOVars.s_item0` = `"item0".GetStableHashCode()`
- `ZDOVars.s_itemData` = `"itemData".GetStableHashCode()`
- `ZDOVars.s_itemPrefab` = `"itemPrefab".GetStableHashCode()`
- `ZDOVars.s_itemStack` = `"itemStack".GetStableHashCode()`
- `ZDOVars.s_items` = `"items".GetStableHashCode()`

**Timers / Timestamps**
- `ZDOVars.s_startTime` = `"StartTime".GetStableHashCode()`
- `ZDOVars.s_plantTime` = `"plantTime".GetStableHashCode()`
- `ZDOVars.s_bakeTimer` = `"bakeTimer".GetStableHashCode()`

**Processing / Queue**
- `ZDOVars.s_queued` = `"queued".GetStableHashCode()`
- `ZDOVars.s_content` = `"Content".GetStableHashCode()`
- `ZDOVars.s_product` = `"product".GetStableHashCode()`
- `ZDOVars.s_spawnOre` = `"SpawnOre".GetStableHashCode()`
- `ZDOVars.s_done` = `"done".GetStableHashCode()`

**Taming**
- `ZDOVars.s_tamed` = `"tamed".GetStableHashCode()`
- `ZDOVars.s_tameTimeLeft` = `"TameTimeLeft".GetStableHashCode()`
- `ZDOVars.s_tameLastFeeding` = `"TameLastFeeding".GetStableHashCode()`

**Ammo / Combat**
- `ZDOVars.s_ammo` = `"ammo".GetStableHashCode()`
- `ZDOVars.s_ammoType` = `"ammoType".GetStableHashCode()`
- `ZDOVars.s_TCData` = `"TCData".GetStableHashCode()`

---

## Hud

| Name | Kind | Signature | Note |
|------|------|-----------|------|
| m_rootObject | field | `public GameObject m_rootObject;` | |
| m_crosshair | field | `public Image m_crosshair;` | |
| m_crosshairBow | field | `public Image m_crosshairBow;` | |
| m_hoverName | field | `public TextMeshProUGUI m_hoverName;` | |
| m_instance | field | `private static Hud m_instance;` | |
| instance | property | `public static Hud instance => m_instance;` | |
| UpdateCrosshair | method | `private void UpdateCrosshair(Player player, float bowDrawPercentage)` | |

**Missing:** none

**ZDO Keys:** none

---

## Player

| Name | Kind | Signature | Note |
|------|------|-----------|------|
| m_localPlayer | field | `public static Player m_localPlayer = null;` | |
| m_hovering | field | `private GameObject m_hovering;` | |
| m_hoveringCreature | field | `private Character m_hoveringCreature;` | |
| m_placementGhost | field | `private GameObject m_placementGhost;` | |
| GetHoverObject | method | `public override GameObject GetHoverObject()` | |
| FindHoverObject | method | `private void FindHoverObject(out GameObject hover, out Character hoverCreature)` | |
| IsPieceAvailable | method | `public bool IsPieceAvailable(Piece piece)` | |
| InPlaceMode | method | `public override bool InPlaceMode()` | |

**Missing:**
- `GetRightItem` - no such method exists

**ZDO Keys:** s_dodgeinv, s_playerID, s_playerName, s_stamina, s_wakeup, s_weaponLoaded, s_baseValue, s_adrenaline, s_eitr, s_cheated, s_dead, s_crowned, s_debugFly, s_pvp, s_emoteID, s_emote, s_emoteOneshot, s_inBed, s_stealth

---

## Hoverable

| Name | Kind | Signature | Note |
|------|------|-----------|------|
| GetHoverText | method | `string GetHoverText()` | |
| GetHoverName | method | `string GetHoverName()` | |
| GetHoverOffset | method | `float GetHoverOffset()` | |

**Missing:** none

**ZDO Keys:** none

---

## Localization

| Name | Kind | Signature | Note |
|------|------|-----------|------|
| instance | property | `public static Localization instance { get; }` | |
| textStrings | field | `private Dictionary<Text, string> textStrings` | |
| textMeshStrings | field | `private Dictionary<TMP_Text, string> textMeshStrings` | |
| m_stringBuilder | field | `private readonly StringBuilder m_stringBuilder` | |
| m_instance | field | `private static Localization m_instance` | |
| m_localizationSettings | field | `private static LocalizationSettings m_localizationSettings` | |
| OnLanguageChange | field | `public static Action OnLanguageChange` | |
| m_endChars | field | `private char[] m_endChars` | |
| m_translations | field | `private Dictionary<string, string> m_translations` | |
| m_languages | field | `private List<string> m_languages` | |
| m_cache | field | `private readonly LRUCache<string> m_cache` | |
| LocalizationConstants | nested | `internal static class LocalizationConstants` | |
| Initialize | method | `private static void Initialize()` | |
| Localization | method | `private Localization()` | |
| SetStartupLanguage | method | `private void SetStartupLanguage()` | |
| SetLanguageFromLocale | method | `private void SetLanguageFromLocale()` | |
| SetLanguage | method | `public void SetLanguage(string language)` | |
| GetSelectedLanguage | method | `public string GetSelectedLanguage()` | |
| GetNextLanguage | method | `public string GetNextLanguage(string lang)` | |
| GetPrevLanguage | method | `public string GetPrevLanguage(string lang)` | |
| Localize (Transform) | method | `public void Localize(Transform root)` | |
| RemoveTextFromCache | method | `public void RemoveTextFromCache(Text text)` | |
| RemoveTextFromCache | method | `public void RemoveTextFromCache(TMP_Text text)` | |
| ReLocalizeVisible | method | `public void ReLocalizeVisible(Transform root)` | |
| ReLocalizeAll | method | `public void ReLocalizeAll(Transform root)` | |
| Localize (string+params) | method | `public string Localize(string text, params string[] words)` | |
| InsertWords | method | `private string InsertWords(string text, string[] words)` | |
| Localize (string) | method | `public string Localize(string text)` | |
| FindNextWord | method | `private bool FindNextWord(string text, int startIndex, out string word, out int wordStart, out int wordEnd)` | |
| Translate | method | `private string Translate(string word)` | |
| GetBoundKeyString | method | `public string GetBoundKeyString(string bindingName, bool emptyStringOnMissing = false)` | |
| AddWord | method | `private void AddWord(string key, string text)` | |
| Clear | method | `private void Clear()` | |
| StripCitations | method | `private string StripCitations(string s)` | |
| SetupLanguage | method | `public bool SetupLanguage(string language)` | |
| LoadCSV | method | `public bool LoadCSV(TextAsset file, string language)` | |
| DoQuoteLineSplit | method | `private List<List<string>> DoQuoteLineSplit(StringReader reader)` | |
| TranslateSingleId | method | `public string TranslateSingleId(string locaId, string language)` | |
| GetLanguages | method | `public List<string> GetLanguages()` | |
| LoadLanguages | method | `private List<string> LoadLanguages()` | |
| IsConsolePlatform | method | `private static bool IsConsolePlatform()` | |
| IsLanguageSupported | method | `private static bool IsLanguageSupported(string language)` | |
| IsLanguageProfessionallyTranslated | method | `public static bool IsLanguageProfessionallyTranslated(string language)` | |

**Missing:** none

**ZDO Keys:** none

---

## ZNetView

| Name | Kind | Signature | Note |
|------|------|-----------|------|
| GetZDO | method | `public ZDO GetZDO()` | Lines 253-256; returns m_zdo |
| IsValid | method | `public bool IsValid()` | Lines 258-265; returns (m_zdo != null && m_zdo.IsValid()) |
| IsOwner | method | `public bool IsOwner()` | Lines 227-234; returns false if !IsValid(), else m_zdo.IsOwner() |
| ClaimOwnership | method | `public void ClaimOwnership()` | Lines 245-251; calls m_zdo.SetOwner(ZDOMan.GetSessionID()) if !IsOwner() |
| HasOwner | method | `public bool HasOwner()` | Lines 236-243; returns false if !IsValid(), else m_zdo.HasOwner() |

**Missing:** none

**ZDO Keys:** ZDOVars.s_scaleHash, ZDOVars.s_scaleScalarHash, HasFields

---

## ZDO

| Name | Kind | Signature | Note |
|------|------|-----------|------|
| IsValid | method | `public bool IsValid()` | Line 263 |
| IsOwner | method | `public bool IsOwner()` | Line 1363 |
| GetFloat (string) | method | `public float GetFloat(string name, float defaultValue = 0f)` | String name overload, line 598 |
| GetFloat (int) | method | `public float GetFloat(int hash, float defaultValue = 0f)` | Int hash overload, line 603 |
| GetFloat (string, out) | method | `public bool GetFloat(string name, out float value)` | Out parameter variant (string), line 608 |
| GetFloat (int, out) | method | `public bool GetFloat(int hash, out float value)` | Out parameter variant (int), line 613 |
| GetInt (string) | method | `public int GetInt(string name, int defaultValue = 0)` | String name overload, line 658 |
| GetInt (int) | method | `public int GetInt(int hash, int defaultValue = 0)` | Int hash overload, line 663 |
| GetInt (string, out) | method | `public bool GetInt(string name, out int value)` | Out parameter variant (string), line 668 |
| GetInt (int, out) | method | `public bool GetInt(int hash, out int value)` | Out parameter variant (int), line 673 |
| GetBool (string) | method | `public bool GetBool(string name, bool defaultValue = false)` | String name overload, line 678 |
| GetBool (int) | method | `public bool GetBool(int hash, bool defaultValue = false)` | Int hash overload, line 683 |
| GetBool (string, out) | method | `public bool GetBool(string name, out bool value)` | Out parameter variant (string), line 688 |
| GetBool (int, out) | method | `public bool GetBool(int hash, out bool value)` | Out parameter variant (int), line 693 |
| GetLong (string) | method | `public long GetLong(string name, long defaultValue = 0L)` | String name overload, line 698 |
| GetLong (int) | method | `public long GetLong(int hash, long defaultValue = 0L)` | Int hash overload, line 703 |
| GetString (string) | method | `public string GetString(string name, string defaultValue = "")` | String name overload, line 708 |
| GetString (int) | method | `public string GetString(int hash, string defaultValue = "")` | Int hash overload, line 713 |
| GetString (string, out) | method | `public bool GetString(string name, out string value)` | Out parameter variant (string), line 718 |
| GetString (int, out) | method | `public bool GetString(int hash, out string value)` | Out parameter variant (int), line 723 |

**Missing:** none

**ZDO Keys:** ZDOVars.s_SpawnTime__DontUse, ZDOVars.s_spawn_time__DontUse, ZDOVars.s_spawnTime, ZDOVars.s_SpawnPoint__DontUse, ZDOVars.s_spawnPoint

---

## ZNet

| Name | Kind | Signature | Note |
|------|------|-----------|------|
| instance | property | `public static ZNet instance => m_instance;` | |
| IsServer | method | `public bool IsServer()` | |
| IsDedicated | method | `public bool IsDedicated()` | |
| GetTime | method | `public DateTime GetTime()` | |
| GetTimeSeconds | method | `public double GetTimeSeconds()` | |

**Missing:** none

**ZDO Keys:** none

---

## ZDOVars

| Name | Kind | Signature | Note |
|------|------|-----------|------|
| s_fuel | field | `public static readonly int s_fuel = "fuel".GetStableHashCode();` | |
| s_health | field | `public static readonly int s_health = "health".GetStableHashCode();` | |
| s_support | field | `public static readonly int s_support = "support".GetStableHashCode();` | |
| s_level | field | `public static readonly int s_level = "level".GetStableHashCode();` | |
| s_pickedTime | field | `public static readonly int s_pickedTime = "picked_time".GetStableHashCode();` | |
| s_picked | field | `public static readonly int s_picked = "picked".GetStableHashCode();` | |
| s_item | field | `public static readonly int s_item = "item".GetStableHashCode();` | |
| s_item0 | field | `public static readonly int s_item0 = "item0".GetStableHashCode();` | |
| s_itemData | field | `public static readonly int s_itemData = "itemData".GetStableHashCode();` | |
| s_itemPrefab | field | `public static readonly int s_itemPrefab = "itemPrefab".GetStableHashCode();` | |
| s_itemStack | field | `public static readonly int s_itemStack = "itemStack".GetStableHashCode();` | |
| s_items | field | `public static readonly int s_items = "items".GetStableHashCode();` | |
| s_queued | field | `public static readonly int s_queued = "queued".GetStableHashCode();` | |
| s_bakeTimer | field | `public static readonly int s_bakeTimer = "bakeTimer".GetStableHashCode();` | |
| s_content | field | `public static readonly int s_content = "Content".GetStableHashCode();` | |
| s_startTime | field | `public static readonly int s_startTime = "StartTime".GetStableHashCode();` | |
| s_plantTime | field | `public static readonly int s_plantTime = "plantTime".GetStableHashCode();` | |
| s_product | field | `public static readonly int s_product = "product".GetStableHashCode();` | |
| s_spawnOre | field | `public static readonly int s_spawnOre = "SpawnOre".GetStableHashCode();` | |
| s_done | field | `public static readonly int s_done = "done".GetStableHashCode();` | |
| s_ammo | field | `public static readonly int s_ammo = "ammo".GetStableHashCode();` | |
| s_ammoType | field | `public static readonly int s_ammoType = "ammoType".GetStableHashCode();` | |
| s_TCData | field | `public static readonly int s_TCData = "TCData".GetStableHashCode();` | |
| s_tamed | field | `public static readonly int s_tamed = "tamed".GetStableHashCode();` | |
| s_tameTimeLeft | field | `public static readonly int s_tameTimeLeft = "TameTimeLeft".GetStableHashCode();` | |
| s_tameLastFeeding | field | `public static readonly int s_tameLastFeeding = "TameLastFeeding".GetStableHashCode();` | |

**Missing:**
- `hunger` - no s_hunger constant; use raw ZDO key string if needed

**Full raw ZDO key list (from ZDOVars source scan):** alive_time, lastTime, picked_time, plantTime, pregnant, spawntime, StartTime, TameLastFeeding, TameTimeLeft, timeOfDeath, lastWorldTime, 0_item, TCData, accTime, addedDefaultItems, adrenaline, aggravated, ammo, ammoType, attachJoint, Attackers, author, authorPlatformDisplayName, Bait, bakeTimer, baseValue, BeardItem, bosscount, cheated, cheatedQueued, ChestItem, Content, creator, creatorIndex, creatorName, crowned, data, dead, DebugFly, DespawnInDay, dodgeinv, done, drops, eitr, emote, emoteID, emote_oneshot, enabled, escape, EventCreature, follow, forward, fuel, GrowStart, HairColor, HairItem, HaveSaddle, health, HelmetItem, HitDir, HitPoint, hooked, Hue, huntplayer, inBed, inWater, InitVel, IsBlocking, item, item0, itemData, itemPrefab, itemStack, items, junkData, lastAttack, LastSpawn, LeftBackItem, LeftBackItemVariant, LeftBackItemQuality, LeftItem, LeftItemVariant, LeftItemQuality, LegItem, level, LiquidData, location, Locked, lovePoints, max_health, MaxInstances, ModelIndex, Modifiers, offset, owner, ownerName, ownerZDOUserId, ownerZDOId, patrol, patrolPoint, permitted, pickable, picked, piece, played, playerID, playerName, plays, pose, preSnow, product, pvp, quality, queued, RandomSkillFactor, relPos, relRot, RightBackItem, RightBackItemQuality, RightItem, RightItemQuality, rodOwner, roomData, rooms, rudder, Saturation, scale, scaleScalar, seAttrib, seed, ShoulderItem, ShoulderItemVariant, ShoulderItemQuality, ShownAlertMessage, SkinColor, sleeping, snow, SpawnAmount, SpawnOre, spread, stamina, state, Stealth, switch, tag, tagauthor, tamed, TamedName, TamedNameAuthor, targets, text, triggered, TrinketItem, type, user, UtilityItem, Value, variant, wakeup, WeaponLoaded, visual, overrideHoverName, noHair, noBeard, count, terrainModifierTimeCreated, alert, animation_speed, body_avel, body_vel, BodyVelocity, haveTarget, InUse, landed, LookTarget, noise, support, tiltrot, vel, velRel, autoDespawn, catchID, cut_time, generated, inBase, LookDir, patrolSpawnPoint, RideSpeed, targetHear, targetSee, durability, stack, worldLevel, crafterID, crafterName, dataCount, pickedUp, spawnpoint, SpawnPoint, SpawnTime, spawn_time, user_u, user_i, RodOwner_u, RodOwner_i, CatchID_u, CatchID_i, target_u, target_i, spawn_id_u, spawn_id_i, parent_id_u, parent_id_i

---

## Container

| Name | Kind | Signature | Note |
|------|------|-----------|------|
| m_nview | field | `private ZNetView m_nview;` | |
| m_name | field | `public string m_name = "Container";` | |
| m_width | field | `public int m_width = 3;` | |
| m_height | field | `public int m_height = 2;` | |
| m_privacy | field | `public PrivacySetting m_privacy = PrivacySetting.Public;` | |
| GetInventory | method | `public Inventory GetInventory()` | |
| GetHoverText | method | `public string GetHoverText()` | |
| CheckAccess | method | `private bool CheckAccess(long playerID)` | |
| IsOwner | method | `public bool IsOwner()` | |
| IsInUse | method | `public bool IsInUse()` | |

**Missing:** none

**ZDO Keys:** ZDOVars.s_addedDefaultItems, ZDOVars.s_cheated, ZDOVars.s_inUse, ZDOVars.s_items

---

## Inventory

| Name | Kind | Signature | Note |
|------|------|-----------|------|
| Inventory (main ctor) | method | `public Inventory(string name, Sprite bkg, int w, int h)` | |
| Inventory (bool ctor) | method | `public Inventory(bool _)` | |
| m_inventory | field | `private List<ItemDrop.ItemData> m_inventory = new List<ItemDrop.ItemData>()` | |
| GetAllItems | method | `public List<ItemDrop.ItemData> GetAllItems()` | |
| GetEmptySlots | method | `public int GetEmptySlots()` | |
| GetWidth | method | `public int GetWidth()` | |
| GetHeight | method | `public int GetHeight()` | |
| NrOfItems | method | `public int NrOfItems()` | |
| GetTotalWeight | method | `public float GetTotalWeight()` | |

**Missing:** none

**ZDO Keys:** none

---

## ItemDrop

| Name | Kind | Signature | Note |
|------|------|-----------|------|
| m_itemData | field | `public ItemData m_itemData = new ItemData();` | |
| GetHoverText | method | `public string GetHoverText()` | |
| ItemData.m_stack | field | `public int m_stack = 1;` | |
| ItemData.m_durability | field | `public float m_durability = 100f;` | |
| ItemData.m_quality | field | `public int m_quality = 1;` | |
| ItemData.m_variant | field | `public int m_variant;` | |
| ItemData.m_gridPos | field | `public Vector2i m_gridPos = Vector2i.zero;` | |
| ItemData.GetMaxDurability() | method | `public float GetMaxDurability()` | |
| ItemData.GetMaxDurability(int) | method | `public float GetMaxDurability(int quality)` | |
| ItemData.GetIcon | method | `public Sprite GetIcon()` | |
| ItemData.SharedData.m_name | field | `public string m_name = "";` | |
| ItemData.SharedData.m_icons | field | `public Sprite[] m_icons = Array.Empty<Sprite>();` | |
| ItemData.SharedData.m_maxStackSize | field | `public int m_maxStackSize = 1;` | |

**Missing:** none

**ZDO Keys:** ZDOVars.s_spawnTime, ZDOVars.s_piece, ZDOVars.s_itemData, ZDOVars.s_quality, ZDOVars.s_variant

---

## Fireplace

| Name | Kind | Signature | Note |
|------|------|-----------|------|
| m_nview | field | `private ZNetView m_nview;` | |
| m_fuelItem | field | `public ItemDrop m_fuelItem;` | |
| m_maxFuel | field | `public float m_maxFuel = 10f;` | |
| m_secPerFuel | field | `public float m_secPerFuel = 3f;` | Seconds per fuel unit; use inverse for burn rate |
| m_infiniteFuel | field | `public bool m_infiniteFuel;` | |
| m_startFuel | field | `public float m_startFuel = 3f;` | |
| GetHoverText | method | `public string GetHoverText()` | |
| IsBurning | method | `public bool IsBurning()` | |

**Missing:**
- `m_fuelPerSecond` - does not exist; closest is `m_secPerFuel` (seconds per fuel unit, inverse calculation)

**ZDO Keys:** ZDOVars.s_fuel, ZDOVars.s_lastTime, ZDOVars.s_state

---

## Smelter

| Name | Kind | Signature | Note |
|------|------|-----------|------|
| m_name | field | `public string m_name = "Smelter";` | |
| m_fuelItem | field | `public ItemDrop m_fuelItem;` | |
| m_maxOre | field | `public int m_maxOre = 10;` | |
| m_maxFuel | field | `public int m_maxFuel = 10;` | |
| m_secPerProduct | field | `public float m_secPerProduct = 10f;` | |
| m_fuelPerProduct | field | `public int m_fuelPerProduct = 4;` | |
| m_conversion | field | `public List<ItemConversion> m_conversion = new List<ItemConversion>();` | |
| m_nview | field | `private ZNetView m_nview;` | |
| GetFuel | method | `private float GetFuel()` | |
| SetFuel | method | `private void SetFuel(float fuel)` | |
| GetQueueSize | method | `private int GetQueueSize()` | |
| GetQueuedOre | method | `private string GetQueuedOre()` | |
| GetBakeTimer | method | `private float GetBakeTimer()` | |
| SetBakeTimer | method | `private void SetBakeTimer(float t)` | |
| OnHoverAddFuel | method | `private string OnHoverAddFuel()` | |
| OnHoverEmptyOre | method | `private string OnHoverEmptyOre()` | |
| OnHoverAddOre | method | `private string OnHoverAddOre()` | |
| GetProcessedQueueSize | method | `private int GetProcessedQueueSize()` | |
| IsActive | method | `public bool IsActive()` | |
| CanUseItems | method | `public bool CanUseItems(Player player, Switch switchRef, bool sendErrorMessage = true)` | |
| TryGetItems | method | `public bool TryGetItems(Player player, Switch switchRef, out List<string> items)` | |
| ItemConversion | nested | `public class ItemConversion { public ItemDrop m_from; public ItemDrop m_to; }` | |

**Missing:**
- `GetHoverText` - not present; compose from OnHoverAddFuel / OnHoverAddOre / OnHoverEmptyOre
- `m_outputMaxLevel` - no such field

**ZDO Keys:** s_fuel, s_bakeTimer, s_queued, s_cheatedQueued, s_item0, s_startTime, s_accTime, s_spawnOre, s_spawnAmount, s_cheated

---

## Fermenter

| Name | Kind | Signature | Note |
|------|------|-----------|------|
| m_conversion | field | `public List<ItemConversion> m_conversion = new List<ItemConversion>();` | |
| m_fermentationDuration | field | `public float m_fermentationDuration = 2400f;` | |
| m_nview | field | `private ZNetView m_nview;` | |
| GetContent | method | `private int GetContent()` | |
| GetStatus | method | `private Status GetStatus()` | |
| GetFermentationTime | method | `private double GetFermentationTime()` | |
| GetHoverText | method | `public string GetHoverText()` | |
| GetItemConversion | method | `private ItemConversion GetItemConversion(int nameHash)` | |

**Missing:** none

**ZDO Keys:** ZDOVars.s_content, ZDOVars.s_startTime, ZDOVars.s_cheatedQueued, ZDOVars.s_cheated

---

## Beehive

| Name | Kind | Signature | Note |
|------|------|-----------|------|
| m_maxHoney | field | `public int m_maxHoney = 4;` | |
| m_secPerUnit | field | `public float m_secPerUnit = 10f;` | |
| m_nview | field | `private ZNetView m_nview;` | |
| m_biome | field | `public Heightmap.Biome m_biome;` | |
| GetHoneyLevel | method | `private int GetHoneyLevel()` | |
| CheckBiome | method | `private bool CheckBiome()` | |
| HaveFreeSpace | method | `private bool HaveFreeSpace()` | |
| GetHoverText | method | `public string GetHoverText()` | |

**Missing:** none

**ZDO Keys:** ZDOVars.s_lastTime, ZDOVars.s_level, ZDOVars.s_product

---

## CookingStation

| Name | Kind | Signature | Note |
|------|------|-----------|------|
| m_slots | field | `public Transform[] m_slots;` | |
| m_conversion | field | `public List<ItemConversion> m_conversion = new List<ItemConversion>();` | |
| m_requireFire | field | `public bool m_requireFire = true;` | |
| m_useFuel | field | `public bool m_useFuel;` | |
| m_maxFuel | field | `public int m_maxFuel = 10;` | |
| m_secPerFuel | field | `public int m_secPerFuel = 5000;` | |
| m_overCookedItem | field | `public ItemDrop m_overCookedItem;` | |
| ItemConversion | nested | `[Serializable] public class ItemConversion { public ItemDrop m_from; public ItemDrop m_to; public float m_cookTime = 10f; }` | |
| GetSlot | method | `private void GetSlot(int slot, out string itemName, out float cookedTime, out Status status, out bool cheated)` | |
| IsFireLit | method | `private bool IsFireLit()` | |
| GetFuel | method | `private float GetFuel()` | |
| GetHoverText | method | `public string GetHoverText()` | |
| IsItemAllowed | method | `private bool IsItemAllowed(ItemDrop.ItemData item); private bool IsItemAllowed(string itemName)` | |

**Missing:** none

**ZDO Keys:** ZDOVars.s_cheated, ZDOVars.s_startTime, ZDOVars.s_cheatedQueued, ZDOVars.s_fuel, slot, slotstatus

---

## Plant

| Name | Kind | Signature | Note |
|------|------|-----------|------|
| m_growTime | field | `public float m_growTime = 10f;` | |
| m_growTimeMax | field | `public float m_growTimeMax = 2000f;` | |
| m_name | field | `public string m_name = "Plant";` | |
| m_minScale | field | `public float m_minScale = 1f;` | |
| m_maxScale | field | `public float m_maxScale = 1f;` | |
| m_growRadius | field | `public float m_growRadius = 1f;` | |
| m_needCultivatedGround | field | `public bool m_needCultivatedGround;` | |
| m_nview | field | `private ZNetView m_nview;` | |
| Status | enum | `public enum Status { Healthy, NoSun, NoSpace, WrongBiome, NotCultivated, NoAttachPiece, TooHot, TooCold }` | |
| GetGrowTime | method | `private float GetGrowTime()` | |
| TimeSincePlanted | method | `private double TimeSincePlanted()` | |
| GetStatus | method | `public Status GetStatus()` | |
| GetHoverText | method | `public string GetHoverText()` | |

**Missing:** none

**ZDO Keys:** ZDOVars.s_seed, ZDOVars.s_plantTime

---

## Pickable

| Name | Kind | Signature | Note |
|------|------|-----------|------|
| m_respawnTimeMinutes | field | `public float m_respawnTimeMinutes;` | Respawn time in minutes for the pickable item |
| m_respawnTimeInitMax | field | `public float m_respawnTimeInitMax;` | Maximum initial respawn time in minutes |
| m_respawnTimeInitMin | field | `public float m_respawnTimeInitMin;` | Minimum initial respawn time in minutes (related field) |
| m_amount | field | `public int m_amount = 1;` | Number of items to spawn when picked |
| m_itemPrefab | field | `public GameObject m_itemPrefab;` | GameObject prefab for the item to drop |
| m_nview | field | `private ZNetView m_nview;` | Network view for synchronization |
| m_pickedTime | field | `private long m_pickedTime;` | Timestamp when item was picked (in ticks) |
| GetHoverText | method | `public string GetHoverText()` | Returns hover text; empty if picked or disabled |
| GetPicked | method | `public bool GetPicked()` | Returns whether item has been picked |
| SetPicked | method | `public void SetPicked(bool picked)` | Sets picked state and synchronizes via ZDO |

**Missing:** none

**ZDO Keys:** ZDOVars.s_picked, ZDOVars.s_pickedTime, ZDOVars.s_enabled

---

## PickableItem

| Name | Kind | Signature | Note |
|------|------|-----------|------|
| m_itemPrefab | field | `public ItemDrop m_itemPrefab;` | Line 16 |
| m_stack | field | `public int m_stack;` | Line 18 |
| GetHoverText | method | `public string GetHoverText()` | Line 75; returns string, no parameters |
| GetStackSize | method | `private int GetStackSize()` | Line 137; clamps based on m_itemPrefab max stack size and Game.m_resourceRate |

**Missing:** none

**ZDO Keys:** ZDOVars.s_itemPrefab, ZDOVars.s_itemStack

---

## TreeBase

| Name | Kind | Signature | Note |
|------|------|-----------|------|
| m_nview | field | `private ZNetView m_nview;` | |
| m_health | field | `public float m_health = 1f;` | |
| m_minToolTier | field | `public int m_minToolTier;` | |
| m_logPrefab | field | `public GameObject m_logPrefab;` | |
| GetDestructibleType | method | `public DestructibleType GetDestructibleType()` | |
| Damage | method | `public void Damage(HitData hit)` | |

**Missing:**
- `GetHoverText` - not present on TreeBase

**ZDO Keys:** ZDOVars.s_health

---

## TreeLog

| Name | Kind | Signature | Note |
|------|------|-----------|------|
| m_health | field | `public float m_health = 60f;` | |
| m_damages | field | `public HitData.DamageModifiers m_damages;` | |
| m_minToolTier | field | `public int m_minToolTier;` | |
| m_destroyedEffect | field | `public EffectList m_destroyedEffect = new EffectList();` | |
| m_hitEffect | field | `public EffectList m_hitEffect = new EffectList();` | |
| m_dropWhenDestroyed | field | `public DropTable m_dropWhenDestroyed = new DropTable();` | |
| m_subLogPrefab | field | `public GameObject m_subLogPrefab;` | |
| m_subLogPoints | field | `public Transform[] m_subLogPoints = Array.Empty<Transform>();` | |
| m_useSubLogPointRotation | field | `public bool m_useSubLogPointRotation;` | |
| m_spawnDistance | field | `public float m_spawnDistance = 2f;` | |
| m_hitNoise | field | `public float m_hitNoise = 100f;` | |
| m_snowHit | field | `public GameObject m_snowHit;` | |
| m_snowHitDistance | field | `public float m_snowHitDistance = 1f;` | |
| m_snowHitPositions | field | `public List<GameObject> m_snowHitPositions;` | |
| m_snowMaxWeightMultiplier | field | `public float m_snowMaxWeightMultiplier = 10f;` | |
| m_minSnow | field | `public float m_minSnow = 0.1f;` | |
| m_snowStartDot | field | `public float m_snowStartDot = 0.25f;` | |
| m_snowHeightBias | field | `public float m_snowHeightBias = 0.3f;` | |
| m_body | field | `private Rigidbody m_body;` | |
| m_nview | field | `private ZNetView m_nview;` | |
| m_firstFrame | field | `private bool m_firstFrame = true;` | |
| m_biome | field | `private Heightmap.Biome m_biome;` | |
| m_lastSnowPos | field | `private Vector3 m_lastSnowPos;` | |
| m_baseWeight | field | `private float m_baseWeight;` | |
| m_baseDrag | field | `private float m_baseDrag;` | |
| Awake | method | `private void Awake()` | |
| UpdateSnow | method | `private void UpdateSnow()` | |
| EnableDamage | method | `private void EnableDamage()` | |
| GetDestructibleType | method | `public DestructibleType GetDestructibleType()` | |
| Damage | method | `public void Damage(HitData hit)` | |
| RPC_Damage | method | `private void RPC_Damage(long sender, HitData hit)` | |
| Setup | method | `public void Setup(Character owner, Vector3 velocity, float hitNoise, HitData hitData, ItemDrop.ItemData item, ItemDrop.ItemData ammo)` | |
| GetTooltipString | method | `public string GetTooltipString(int itemQuality)` | |
| Destroy | method | `private void Destroy(HitData hitData, bool cheatedTool)` | |

**Missing:** none

**ZDO Keys:** ZDOVars.s_health

---

## MineRock5

| Name | Kind | Signature | Note |
|------|------|-----------|------|
| m_health | field | `public float m_health = 2f;` | Base health per HitArea; scaled by world level on init (line 103) |
| m_minToolTier | field | `public int m_minToolTier;` | |
| m_hitAreas | field | `private List<HitArea> m_hitAreas;` | |
| m_nview | field | `private ZNetView m_nview;` | |
| HitArea | nested | `private class HitArea` | Per-area data |
| HitArea.m_collider | field | `public Collider m_collider;` | |
| HitArea.m_meshRenderer | field | `public MeshRenderer m_meshRenderer;` | |
| HitArea.m_meshFilter | field | `public MeshFilter m_meshFilter;` | |
| HitArea.m_physics | field | `public StaticPhysics m_physics;` | |
| HitArea.m_health | field | `public float m_health;` | Per-area health; serialized via base64 ZPackage in ZDOVars.s_health |
| HitArea.m_bound | field | `public BoundData m_bound;` | |
| HitArea.m_supported | field | `public bool m_supported;` | |
| HitArea.m_baseScale | field | `public float m_baseScale;` | |
| GetHoverText | method | `public string GetHoverText()` | |
| Damage | method | `public void Damage(HitData hit)` | |
| DamageArea | method | `private bool DamageArea(int hitAreaIndex, HitData hit)` | |
| GetHoverOffset | method | `public float GetHoverOffset()` | |
| LoadHealth | method | `private void LoadHealth()` | Reads base64-encoded health data from ZDOVars.s_health |
| SaveHealth | method | `private void SaveHealth()` | Serializes per-area health values to base64-encoded ZPackage |

**Missing:** none

**ZDO Keys:** ZDOVars.s_health

---

## Destructible

| Name | Kind | Signature | Note |
|------|------|-----------|------|
| m_health | field | `public float m_health = 1f;` | Default health; actual stored in ZDO under ZDOVars.s_health |
| m_minToolTier | field | `public int m_minToolTier;` | Minimum tool tier required to damage |
| m_nview | field | `private ZNetView m_nview;` | Network view for synchronizing damage across clients |
| RPC_Damage | method | `private void RPC_Damage(long sender, HitData hit)` | Reads ZDOVars.s_health via GetFloat, writes via Set |
| Destroy | method | `public void Destroy(HitData hit = null)` | Sets ZDOVars.s_cheated flag if destroyed by cheated item |

**Missing:**
- `GetHoverText` - not present on Destructible

**ZDO Keys:** ZDOVars.s_health, ZDOVars.s_cheated

---

## SapCollector

| Name | Kind | Signature | Note |
|------|------|-----------|------|
| m_secPerUnit | field | `public float m_secPerUnit = 10f;` | Seconds per unit of sap collection |
| m_maxLevel | field | `public int m_maxLevel = 4;` | Maximum level capacity |
| m_nview | field | `private ZNetView m_nview;` | Network view for syncing state |
| GetLevel | method | `private int GetLevel()` | Returns current level from ZDO |
| GetHoverText | method | `public string GetHoverText()` | Returns hover tooltip with level and status |

**Missing:**
- `GetStatus` - not present

**ZDO Keys:** ZDOVars.s_lastTime, ZDOVars.s_level, ZDOVars.s_product

---

## WearNTear

| Name | Kind | Signature | Note |
|------|------|-----------|------|
| m_health | field | `public float m_health = 100f;` | |
| m_supports | field | `public bool m_supports = true;` | |
| m_materialType | field | `public MaterialType m_materialType;` | |
| m_nview | field | `private ZNetView m_nview;` | |
| GetHealthPercentage | method | `public float GetHealthPercentage()` | |
| GetSupport | method | `private float GetSupport()` | |
| GetMaxSupport | method | `private float GetMaxSupport()` | |
| GetMinSupport | method | `private float GetMinSupport()` | |
| IsWet | method | `public bool IsWet()` | |

**Missing:**
- `GetRemainingHealth` - no such method; use `GetHealthPercentage()` which returns 0-1 clamped (or 1.0 if invalid)

**ZDO Keys:** ZDOVars.s_health, ZDOVars.s_support, ZDOVars.s_snow, ZDOVars.s_preSnow

---

## Piece

| Name | Kind | Signature | Note |
|------|------|-----------|------|
| m_name | field | `public string m_name = "";` | Line 121 |
| m_description | field | `public string m_description = "";` | Line 123 |
| m_icon | field | `public Sprite m_icon;` | Line 119 |
| m_category | field | `public PieceCategory m_category;` | Line 127 |
| m_nview | field | `private ZNetView m_nview;` | Line 231 |
| m_creator | field | `private long m_creator;` | Line 233 |
| GetCreator | method | `public long GetCreator()` | Lines 440-443 |
| IsCreator | method | `public bool IsCreator()` | Lines 450-455 |

**Missing:** none

**ZDO Keys:** ZDOVars.s_creator, ZDOVars.s_creatorIndex, ZDOVars.s_cheated

---

## ItemStand

| Name | Kind | Signature | Note |
|------|------|-----------|------|
| m_visualHash | field | `private int m_visualHash;` | |
| m_nview | field | `private ZNetView m_nview;` | |
| GetAttachedItem | method | `public int GetAttachedItem()` | |
| HaveAttachment | method | `public bool HaveAttachment()` | |
| GetHoverText | method | `public string GetHoverText()` | |

**Missing:** none

**ZDO Keys:** s_item, s_type, s_variant, s_quality

---

## ArmorStand

| Name | Kind | Signature | Note |
|------|------|-----------|------|
| m_slots | field | `public List<ArmorStandSlot> m_slots = new List<ArmorStandSlot>();` | |
| m_nview | field | `private ZNetView m_nview;` | |
| ArmorStandSlot | nested | `public class ArmorStandSlot` | |
| ArmorStandSlot.m_switch | field | `public Switch m_switch;` | |
| ArmorStandSlot.m_slot | field | `public VisSlot m_slot;` | |
| ArmorStandSlot.m_supportedTypes | field | `public List<ItemDrop.ItemData.ItemType> m_supportedTypes = new List<ItemDrop.ItemData.ItemType>();` | |
| ArmorStandSlot.m_item | field | `public ItemDrop.ItemData m_item;` | |
| ArmorStandSlot.m_visualHash | field | `public int m_visualHash;` | |
| ArmorStandSlot.m_visualVariant | field | `public int m_visualVariant;` | |
| ArmorStandSlot.m_currentItemName | field | `public string m_currentItemName = "";` | |
| SetPose | method | `private void SetPose(int index, bool effect = true)` | |
| RPC_SetPose | method | `public void RPC_SetPose(long sender, int index)` | |
| UseItem | method | `private bool UseItem(Switch caller, Humanoid user, ItemDrop.ItemData item)` | |
| DestroyAttachment | method | `public void DestroyAttachment(int index)` | |
| RPC_DestroyAttachment | method | `public void RPC_DestroyAttachment(long sender, int index)` | |
| HaveAttachment | method | `public bool HaveAttachment(int index)` | |
| GetAttachedItem | method | `public int GetAttachedItem(int index)` | |
| GetNrOfAttachedItems | method | `public int GetNrOfAttachedItems()` | |
| TryGetItems | method | `public bool TryGetItems(Player player, Switch switchRef, out List<string> items)` | |
| CanUseItems | method | `public bool CanUseItems(Player player, Switch switchRef, bool sendErrorMessage = true)` | |

**Missing:**
- `GetHoverText` - not present on ArmorStand

**ZDO Keys:** ZDOVars.s_pose, {index}_item, {index}_variant

---

## TombStone

| Name | Kind | Signature | Note |
|------|------|-----------|------|
| m_container | field | `private Container m_container;` | |
| m_nview | field | `private ZNetView m_nview;` | |
| GetOwner | method | `private long GetOwner()` | |
| IsOwner | method | `private bool IsOwner()` | |
| GetHoverText | method | `public string GetHoverText()` | |

**Missing:** none

**ZDO Keys:** ZDOVars.s_timeOfDeath, ZDOVars.s_spawnPoint, ZDOVars.s_ownerName, ZDOVars.s_owner, ZDOVars.s_inWater

---

## Tameable

| Name | Kind | Signature | Note |
|------|------|-----------|------|
| m_tamingTime | field | `public float m_tamingTime = 1800f;` | |
| m_nview | field | `private ZNetView m_nview;` | |
| m_character | field | `private Character m_character;` | |
| m_monsterAI | field | `private MonsterAI m_monsterAI;` | |
| GetRemainingTime | method | `private float GetRemainingTime()` | |
| GetTameness | method | `private int GetTameness()` | |
| IsHungry | method | `public bool IsHungry()` | |
| GetHoverText | method | `public string GetHoverText()` | |
| GetStatusString | method | `public string GetStatusString()` | |

**Missing:**
- `GetLevel` - no such method; closest is `GetRiderSkill`

**ZDO Keys:** ZDOVars.s_tamedName, ZDOVars.s_tamedNameAuthor, ZDOVars.s_haveSaddleHash, ZDOVars.s_follow, ZDOVars.s_maxInstances, ZDOVars.s_tameLastFeeding, ZDOVars.s_tameTimeLeft

---

## MonsterAI

| Name | Kind | Signature | Note |
|------|------|-----------|------|
| m_consumeItems | field | `public List<ItemDrop> m_consumeItems;` | List of items this AI will consume (hunger satisfaction) |
| m_consumeSearchRange | field | `public float m_consumeSearchRange = 5f;` | Search radius for consumable items |
| SetAlerted | method | `protected override void SetAlerted(bool alert)` | |
| IsSleeping | method | `public override bool IsSleeping()` | |
| MakeTame | method | `public void MakeTame()` | |
| DespawnInDay | method | `public bool DespawnInDay()` | |
| SetDespawnInDay | method | `public void SetDespawnInDay(bool despawn)` | |
| IsEventCreature | method | `public bool IsEventCreature()` | |
| SetEventCreature | method | `public void SetEventCreature(bool despawn)` | |
| HuntPlayer | method | `public override bool HuntPlayer()` | |

**Missing:**
- `IsAlerted` - inherited from BaseAI; not declared on MonsterAI
- `IsHungry` - not on MonsterAI; use `m_tamable.IsHungry()` instead (line 650)

**ZDO Keys:** ZDOVars.s_despawnInDay, ZDOVars.s_eventCreature, ZDOVars.s_sleeping

---

## Character

| Name | Kind | Signature | Note |
|------|------|-----------|------|
| m_name | field | `public string m_name = "";` | Line 79 |
| GetHealth | method | `public float GetHealth()` | Returns current health from ZDO, line 2992 |
| GetMaxHealth | method | `public float GetMaxHealth()` | Returns max health from ZDO, line 3059 |
| GetLevel | method | `public int GetLevel()` | Returns character level, line 789 |
| IsTamed | method | `public bool IsTamed()` | Returns tamed status, line 4335 |
| GetHoverName | method | `public virtual string GetHoverName()` | Returns hover display name, line 3857 |
| GetHoverText | method | `public virtual string GetHoverText()` | Returns hover text, line 3847 |

**Missing:** none

**ZDO Keys:** ZDOVars.s_health, ZDOVars.s_maxHealth, ZDOVars.s_level, ZDOVars.s_tamed, ZDOVars.s_overrideHoverName, ZDOVars.s_attackers, ZDOVars.s_bodyVelocity, ZDOVars.s_bossCount, ZDOVars.s_cheated, ZDOVars.s_modifiers, ZDOVars.s_noise, ZDOVars.s_randomSkillFactor, ZDOVars.s_tiltrot

---

## Ship

| Name | Kind | Signature | Note |
|------|------|-----------|------|
| m_nview | field | `private ZNetView m_nview;` | |
| m_waterImpactEffect | field | `public EffectList m_waterImpactEffect = new EffectList();` | |
| m_changeSailPosEffect | field | `public EffectList m_changeSailPosEffect = new EffectList();` | |
| m_ashdamageEffects | field | `public GameObject m_ashdamageEffects;` | |
| UpdateControlls | method | `private void UpdateControlls(float dt)` | |

**Missing:**
- `m_shipEffects` - not present
- `GetHoverText` - not present
- cargo container accessor - not present

**ZDO Keys:** ZDOVars.s_forward, ZDOVars.s_rudder

---

## Vagon

| Name | Kind | Signature | Note |
|------|------|-----------|------|
| m_container | field | `public Container m_container;` | |
| m_nview | field | `private ZNetView m_nview;` | |
| GetHoverText | method | `public string GetHoverText()` | |

**Missing:** none

**ZDO Keys:** ZDOVars.s_attachJointHash

---

## WispSpawner

| Name | Kind | Signature | Note |
|------|------|-----------|------|
| m_maxSpawned | field | `public int m_maxSpawned = 3;` | |
| m_spawnInterval | field | `public float m_spawnInterval = 5f;` | |
| m_nview | field | `private ZNetView m_nview;` | |
| m_name | field | `public string m_name = "$pieces_wisplure";` | |
| m_noSpaceString | field | `public string m_noSpaceString = "$piece_wisplure_nospace";` | |
| m_fullString | field | `public string m_fullString = "$piece_wisplure_full";` | |
| m_tooMuchLightString | field | `public string m_tooMuchLightString = "$piece_wisplure_light";` | |
| m_wispsAreComingString | field | `public string m_wispsAreComingString = "$piece_wisplure_ok";` | |
| m_hoverOffset | field | `public float m_hoverOffset;` | |
| m_spawnChance | field | `public float m_spawnChance = 0.5f;` | |
| m_onlySpawnAtNight | field | `public bool m_onlySpawnAtNight = true;` | |
| m_dontSpawnInCover | field | `public bool m_dontSpawnInCover = true;` | |
| m_maxCover | field | `public float m_maxCover = 0.6f;` | |
| m_wispPrefab | field | `public GameObject m_wispPrefab;` | |
| m_wispsNearbyObject | field | `public GameObject m_wispsNearbyObject;` | |
| m_nearbyTreshold | field | `public float m_nearbyTreshold = 5f;` | |
| m_spawnPoint | field | `public Transform m_spawnPoint;` | |
| m_coverPoint | field | `public Transform m_coverPoint;` | |
| m_spawnDistance | field | `public float m_spawnDistance = 20f;` | |
| m_maxSpawnedArea | field | `public float m_maxSpawnedArea = 10f;` | |
| m_status | field | `private Status m_status = Status.Ok;` | |
| m_lastStatusUpdate | field | `private float m_lastStatusUpdate = -1000f;` | |
| s_spawners | field | `private static readonly List<WispSpawner> s_spawners = new List<WispSpawner>();` | |
| Status | enum | `public enum Status { NoSpace, TooBright, Full, Ok }` | |
| Start | method | `private void Start()` | |
| OnDestroy | method | `private void OnDestroy()` | |
| GetHoverText | method | `public string GetHoverText()` | |
| GetHoverName | method | `public string GetHoverName()` | |
| UpdateDemister | method | `private void UpdateDemister()` | |
| GetStatus | method | `private Status GetStatus()` | |
| TrySpawn | method | `private void TrySpawn()` | |
| HaveFreeSpace | method | `private bool HaveFreeSpace()` | |
| OnDrawGizmos | method | `private void OnDrawGizmos()` | |
| GetBestSpawner | method | `public static WispSpawner GetBestSpawner(Vector3 p, float maxRange)` | |
| GetHoverOffset | method | `public float GetHoverOffset()` | |

**Missing:** none

**ZDO Keys:** ZDOVars.s_lastSpawn

---

## ShieldGenerator

| Name | Kind | Signature | Note |
|------|------|-----------|------|
| m_fuelItems | field | `public List<ItemDrop> m_fuelItems = new List<ItemDrop>();` | Fuel items that can be added to the shield |
| m_maxFuel | field | `public int m_maxFuel = 10;` | Maximum fuel capacity |
| m_nview | field | `private ZNetView m_nview;` | Network view for syncing shield state |
| GetFuel | method | `private float GetFuel()` | Returns current fuel level from ZDO |
| GetHoverText | method | `public string GetHoverText()` | Returns hover text for the shield generator UI |

**Missing:** none

**ZDO Keys:** ZDOVars.s_startTime, ZDOVars.s_fuel

---

## Feast

| Name | Kind | Signature | Note |
|------|------|-----------|------|
| m_eatStacks | field | `public int m_eatStacks = 5;` | Maximum number of food stacks (default 5) |
| m_useDistance | field | `public float m_useDistance = 2f;` | Distance required to interact |
| m_foodItem | field | `public ItemDrop m_foodItem;` | Reference to the food item |
| m_feastParts | field | `public List<FeastLevel> m_feastParts = new List<FeastLevel>();` | Visual state parts for different stack levels |
| m_eatEffect | field | `public EffectList m_eatEffect = new EffectList();` | Effect played when food is eaten |
| m_hoverOffset | field | `public float m_hoverOffset;` | Offset for hover text display |
| FeastLevel | nested | `public class FeastLevel` | Nested serializable class for visual state thresholds |
| GetStack | method | `public int GetStack()` | Returns remaining food stacks from ZDO (ZDOVars.s_value) |
| GetStackPercentige | method | `public float GetStackPercentige()` | Returns percentage of remaining stacks |
| GetHoverText | method | `public string GetHoverText()` | Hover text with stack count; checks distance and availability |
| GetHoverName | method | `public string GetHoverName()` | Returns the food item name for display |
| UpdateVisual | method | `public void UpdateVisual()` | Updates visual state based on stack percentage |
| RPC_TryEat | method | `private void RPC_TryEat(long sender)` | Decrements stack and triggers eat effect; sets ZDOVars.s_value |
| RPC_OnEat | method | `public void RPC_OnEat(long sender)` | Called when food is eaten; plays eat effect |
| RPC_EatConfirmation | method | `private void RPC_EatConfirmation(long sender)` | Confirms eat on client and applies food benefits |
| Interact | method | `public bool Interact(Humanoid human, bool hold, bool alt)` | Interactable interface implementation for eating |
| UseItem | method | `public bool UseItem(Humanoid user, ItemDrop.ItemData item)` | Interactable interface; returns false |
| GetHoverOffset | method | `public float GetHoverOffset()` | Returns hover text offset |

**Missing:** none

**ZDO Keys:** s_value
