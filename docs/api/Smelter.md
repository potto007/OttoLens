# Smelter Type Analysis (Valheim 1.0.7)

## Type Declaration
`public class Smelter : MonoBehaviour, IHasHoverMenuExtended`

## Key Fields & Properties

### Core Configuration
- `public string m_name = "Smelter";` (line 17)
- `public ItemDrop m_fuelItem;` (line 45)
- `public int m_maxOre = 10;` (line 47)
- `public int m_maxFuel = 10;` (line 49)
- `public float m_secPerProduct = 10f;` (line 53)
- `public int m_fuelPerProduct = 4;` (line 51)
- `public List<ItemConversion> m_conversion = new List<ItemConversion>();` (line 65)

### Internal State
- `private ZNetView m_nview;` (line 73)

## Item Conversion
ItemConversion nested class (lines 10-15):
- `public ItemDrop m_from;`
- `public ItemDrop m_to;`

## Methods

### Fuel Management
- `private float GetFuel()` (lines 251-258): Returns fuel amount from ZDO
- `private void SetFuel(float fuel)` (lines 260-266): Sets fuel amount in ZDO

### Queue Operations
- `private int GetQueueSize()` (lines 268-275): Returns queued ore count via ZDOVars.s_queued
- `private string GetQueuedOre()` (lines 300-307): Returns first queued ore name from ZDOVars.s_item0
- `private void QueueOre(string name, bool cheated)` (lines 292-298): Adds ore to queue

### Baking/Processing
- `private float GetBakeTimer()` (lines 234-241): Returns bake progress from ZDOVars.s_bakeTimer
- `private void SetBakeTimer(float t)` (lines 243-249): Sets bake progress in ZDO

### Hover Text
- `private string OnHoverAddFuel()` (lines 621-625): Returns fuel status tooltip
- `private string OnHoverEmptyOre()` (lines 627-631): Returns processed items tooltip
- `private string OnHoverAddOre()` (lines 633-644): Returns ore queue status tooltip

### State Queries
- `public bool IsActive()` (lines 612-619): Checks if smelter is actively processing
- `private int GetProcessedQueueSize()` (lines 537-544): Returns count of finished items via ZDOVars.s_spawnAmount
- `public bool CanUseItems(Player player, Switch switchRef, bool sendErrorMessage = true)` (lines 664-711)
- `public bool TryGetItems(Player player, Switch switchRef, out List<string> items)` (lines 646-662)

## ZDO Keys Used
- `ZDOVars.s_fuel` - Current fuel amount
- `ZDOVars.s_bakeTimer` - Baking progress timer
- `ZDOVars.s_queued` - Count of queued ore items
- `ZDOVars.s_cheatedQueued` - Whether queued item was cheated
- `ZDOVars.s_item0` - First queued ore item name
- `ZDOVars.s_startTime` - Timestamp for delta time calculation
- `ZDOVars.s_accTime` - Accumulated processing time
- `ZDOVars.s_spawnOre` - Ore type for next spawn
- `ZDOVars.s_spawnAmount` - Stack size for spawning
- `ZDOVars.s_cheated` - Whether produced item was from cheated input
- `"item" + index` (dynamic string keys) - Queue array items (item0, item1, etc.)

## Missing Members
- `GetHoverText` - No method with this exact name (use OnHoverAddFuel/OnHoverAddOre/OnHoverEmptyOre instead)
- `m_outputMaxLevel` - Not present in decompiled code
