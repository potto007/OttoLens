# OttoLens specification

Target game: Valheim 1.0.7. Loader: BepInEx 5. Mod is client only.

OttoLens shows useful status in the hover text of a world object. It also shows
container contents as item icons in a grid next to the hover text.

## 0. Ground rules for the implementer

1. Every vanilla type and member named in this document is marked VERIFIED. It
   was read out of the shipped game assembly with a decompiler in the same
   session that produced this specification. The assembly is
   `assembly_valheim.dll`, except where the table says `assembly_guiutils.dll`
   or `assembly_utils.dll`.
2. Many members that OttoLens must read are private. Reference the three game
   assemblies through the BepInEx assembly publicizer, as the sibling mod
   OttoAura does. Its project file shows the pattern:
   `/home/potto/src/valheim/mods/OttoAura/OttoAura.csproj`.
3. Do not copy code from any other mod. Write every reader from the behavior
   described here.
4. Read `/home/potto/src/valheim/mods/OttoAura/WardAura.cs` for the house style
   of a Harmony postfix on a `GetHoverText` method. That file is ours and is
   MIT-0. You may copy from it.
5. Never write to a ZDO. OttoLens is a read-only lens. Several vanilla helper
   methods look like getters but write to the ZDO as a side effect. Section 6
   names them.
6. Do not change game behavior. Do not adjust burn rates, cook times, or fuel
   limits. OttoLens reports; it does not tune.
7. Do not use a translation key that vanilla does not ship. A missing key
   renders as the raw `$key` string on screen. Section 2 states the rule.

## 1. Hover pipeline

### 1.1 How vanilla produces hover text

The hover text comes from the `Hoverable` interface. VERIFIED members of
`Hoverable`:

| Member | Signature |
| --- | --- |
| `Hoverable.GetHoverText` | `string GetHoverText()` |
| `Hoverable.GetHoverName` | `string GetHoverName()` |
| `Hoverable.GetHoverOffset` | `float GetHoverOffset()` |

`GetHoverOffset` returns `float` in 1.0.7. Any component that OttoLens adds and
that implements `Hoverable` must implement all three members. Return `0f` from
`GetHoverOffset` to keep the vanilla hover distance.

### 1.2 How the player picks a hover target

VERIFIED members of `Player`:

| Member | Signature | Note |
| --- | --- | --- |
| `Player.m_localPlayer` | `static Player m_localPlayer` | public static field |
| `Player.GetHoverObject` | `override GameObject GetHoverObject()` | returns the private field `m_hovering` |
| `Player.GetHoverCreature` | `Character GetHoverCreature()` | returns the private field `m_hoveringCreature` |
| `Player.GetHoveringPiece` | `Piece GetHoveringPiece()` | returns null unless the player is in place mode |
| `Player.m_hovering` | `private GameObject m_hovering` | prefer the public getter |
| `Player.m_hoveringPiece` | `private Piece m_hoveringPiece` | prefer the public getter |
| `Player.m_maxInteractDistance` | `public float m_maxInteractDistance` | default 5 |
| `Player.m_maxPlaceDistance` | `public float m_maxPlaceDistance` | default 5 |

Two separate raycasts run each frame inside the local player update.

1. The interact raycast fills `m_hovering` and `m_hoveringCreature`. It runs
   only when the player is not in place mode, is not dead, and controls no
   doodad. In place mode both fields are set to null.
2. The build raycast fills `m_hoveringPiece`. It runs only in place mode.

The consequence matters for the whole design. **When the hammer is out, there is
no hover object and `GetHoverText` is never called.** Build piece status
therefore cannot be appended to hover text. It needs its own path. Section 3.14
describes that path.

`Player.GetHoverObject` returns the collider game object, or the attached
rigidbody game object when the collider itself carries no `Hoverable`. The
`Hoverable` component may sit on a parent. Always resolve it with
`GetComponentInParent<Hoverable>()`, the way the heads up display does.

### 1.3 What the heads up display does each frame

VERIFIED members of `Hud`:

| Member | Signature | Note |
| --- | --- | --- |
| `Hud.instance` | `static Hud instance { get; }` | null on a dedicated server |
| `Hud.UpdateCrosshair` | `private void UpdateCrosshair(Player player, float bowDrawPercentage)` | called once per frame from the update method |
| `Hud.m_hoverName` | `public TextMeshProUGUI m_hoverName` | holds the whole hover text, not only the name |
| `Hud.m_crosshair` | `public Image m_crosshair` | tinted yellow when the hover text is not empty |
| `Hud.m_crosshairBow` | `public Image m_crosshairBow` | |
| `Hud.m_pieceHealthRoot` | `public RectTransform m_pieceHealthRoot` | shown in place mode only |
| `Hud.m_pieceHealthBar` | `public GuiBar m_pieceHealthBar` | fed with the piece health fraction |
| `Hud.m_rootObject` | `public GameObject m_rootObject` | moved off screen to hide the display |
| `Hud.IsVisible` | `public bool IsVisible()` | false while the display is parked off screen |
| `Hud.InRadial` | `public static bool InRadial()` | |

Each frame the crosshair update does the following, in this order.

1. It reads the hover object from the player and resolves a `Hoverable` on it or
   on a parent.
2. If a `Hoverable` was found, and the text viewer is not visible, it calls
   `GetHoverText()` once and assigns the result to `m_hoverName.text`.
3. If gamepad input is active, it rewrites the key hint markup inside the string
   before assigning it.
4. If no `Hoverable` was found, it assigns the empty string to
   `m_hoverName.text` and resets the crosshair color.
5. In place mode it shows the piece health bar from
   `WearNTear.GetHealthPercentage()` for the piece under the build raycast.

`TextViewer.instance.IsVisible()` is VERIFIED as `public bool IsVisible()` on
`TextViewer`, and `TextViewer.instance` is VERIFIED as
`static TextViewer instance { get; }`.

### 1.4 Where OttoLens hooks

Use two kinds of hook. Do not use more.

**Hook A, per target type.** A Harmony postfix on the `GetHoverText` method of
the vanilla component. Append lines to `__result`. This is the primary
mechanism. It runs only when that object is really the hover target, so it costs
nothing on other frames. It is also the mechanism other hover mods use, so keep
each postfix short and never replace `__result` wholesale. Section 3 lists the
exact method per target type.

**Hook B, one frame hook.** A Harmony postfix on
`Hud.UpdateCrosshair(Player, float)`. This hook is the only place that knows
whether a hover exists at all this frame. It owns three jobs.

1. Drive the icon grid. Show it when the frame set icons. Hide it when the frame
   set none. Section 2.5 gives the exact rule.
2. Render build piece status, because in place mode hook A never fires. Read
   `player.GetHoveringPiece()`.
3. Render tree and mineable status when the target carries no `Hoverable`.
   Section 3.12 explains that case.

Order matters. The crosshair update assigns `m_hoverName.text` before the
postfix runs, so the postfix may read and extend that text.

Do not patch `Hud.Update`. It does much more than the crosshair and other mods
patch it heavily.

### 1.5 Knowing the current hover object inside a postfix

Inside a hook A postfix, `__instance` is the target. That is enough. Do not read
the player hover field there.

Inside the hook B postfix, use `player.GetHoverObject()` for the interact hover
and `player.GetHoveringPiece()` for the build hover. Both may be null in the
same frame.

## 2. Icon grid attachment

### 2.1 What to parent it to

Parent one container game object to `Hud.instance.m_hoverName.transform.parent`.
That is the transform that holds the hover text, so the grid moves and hides
with the crosshair group. Resolve the parent at runtime from the field. Do not
hard code a path string. If the field's parent is null, fall back to
`Hud.instance.m_crosshair.transform.parent`, and if that is also null, log a
warning once and disable the grid for the session.

Create the container once, when the world loads and `Hud.instance` is not null.
Destroy it when the world unloads. Section 5 gives the lifecycle.

### 2.2 Position

The grid sits below and to one side of the crosshair, so that it never covers
the crosshair or the first line of hover text.

- Use a `GridLayoutGroup` on the container.
- Set the cell size from the icon size setting. A square cell of 42 to 48
  reference pixels reads well at the default display scale.
- Set `startCorner` to the lower left and `startAxis` to horizontal. Rows then
  fill from the bottom upward, so the crosshair area stays clear as the item
  count grows.
- Set `constraint` to a fixed column count. The column count is the icons per
  row setting.
- Use the layout group padding to push the whole block clear of the crosshair.
  A left pad of about 76 reference pixels and a bottom pad of about 46 reference
  pixels puts the block under the hover text on the right of the crosshair.
- Set the container local scale to about 0.8 so the icons read as status, not as
  an inventory window.
- Set the container local position to zero and let the parent's anchoring do the
  work.

### 2.3 What an element is made of

Build the element yourself. Do not clone the vanilla slot prefab.

The vanilla slot prefab is reachable as
`InventoryGui.instance.m_playerGrid.m_elementPrefab`, VERIFIED as
`public GameObject m_elementPrefab` on `InventoryGrid`, with
`InventoryGui.m_playerGrid` VERIFIED as `public InventoryGrid m_playerGrid` and
`InventoryGui.instance` VERIFIED as `static InventoryGui instance { get; }`.
That prefab carries a button, drag handlers, tooltip, durability bar, a food
icon, and a touch raycast padding component that declares a required `Image`.
Stripping it is fragile. Unity refuses to remove a component that another
component still requires, and a normal destroy call only takes effect at the end
of the frame, so the strip order has to be exact. Building the element by hand
avoids the whole problem.

An OttoLens element is three objects.

1. Root. A `RectTransform` plus an `Image` used as a dark translucent
   background. Black at about 30 percent alpha reads well over any terrain.
2. Icon child. An `Image` stretched to the root with a small inset. This holds
   the item sprite. Disable the image when the sprite is null.
3. Count child. A `TMP_Text` anchored to the lower right of the root. This holds
   the count badge. Disable the text when the count string is empty.

Reference `TMPro` for `TMP_Text`. The heads up display already uses
`TextMeshProUGUI`, so the font asset is loadable from
`Hud.instance.m_hoverName.font`.

For the count badge, prefer plain digits. A badge like `12` or `12/20` needs no
translation.

### 2.4 Where an icon sprite comes from

The single verified path to an item icon is through the item data.

| Member | Signature |
| --- | --- |
| `ItemDrop.ItemData.GetIcon` | `public Sprite GetIcon()` |
| `ItemDrop.ItemData.m_shared.m_icons` | `public Sprite[] m_icons` |
| `ItemDrop.ItemData.m_variant` | `public int m_variant` |
| `ItemDrop.m_itemData` | `public ItemDrop.ItemData m_itemData` |

`GetIcon()` returns `m_shared.m_icons[m_variant]`. There is no `GetIcon` method
on the `ItemDrop` component itself in 1.0.7. Go through `m_itemData`.

Three cases arise.

1. The reader already holds an `ItemDrop.ItemData`. Call `GetIcon()` on it. This
   covers every container item and every ground item drop.
2. The reader holds an `ItemDrop` component, for example a fuel item or a honey
   item on a piece. Use `component.m_itemData.GetIcon()`.
3. The reader holds only a prefab name or a stable hash. Resolve it through the
   object database, then take the item data.

VERIFIED members of `ObjectDB`:

| Member | Signature |
| --- | --- |
| `ObjectDB.instance` | `static ObjectDB instance { get; }` |
| `ObjectDB.GetItemPrefab` | `public GameObject GetItemPrefab(string name)` |
| `ObjectDB.GetItemPrefab` | `public GameObject GetItemPrefab(int hash)` |
| `ObjectDB.TryGetItemPrefab` | `public bool TryGetItemPrefab(string name, out GameObject prefab)` |
| `ObjectDB.TryGetItemPrefab` | `public bool TryGetItemPrefab(int hash, out GameObject prefab)` |

Use the `TryGetItemPrefab` overloads. A lookup can fail for a modded or removed
item, and the plain getter logs an error each frame when it fails.

Cache the resolution. A dictionary from prefab name or hash to `Sprite`, filled
on first use and cleared when the world unloads, removes the per frame lookup
cost. Section 6 covers the performance rules.

For a variant sprite, index `m_shared.m_icons` with the stored variant value
instead of calling `GetIcon()`. Guard the index against the array length.

One non-item sprite is useful. `Minimap.GetSprite(Minimap.PinType type)` is
VERIFIED as `private Sprite GetSprite(Minimap.PinType type)`, with
`Minimap.instance` VERIFIED as `static Minimap instance { get; }` and
`Minimap.PinType` VERIFIED as a public nested enum. The publicizer makes the
method reachable. Use it only for the tombstone marker.

### 2.5 When to build, refresh, hide, and destroy

Build once per world session, in the world start step. Create the container and
create every element up to the maximum icon count. Start every element hidden.

Refresh inside the hook A postfixes. A reader that wants icons calls the grid
with an ordered list of pairs of sprite and count label. The grid writes the
first N pairs into its elements, shows those elements, and marks itself dirty.

Hide in the hook B postfix, at the end of the frame's work. If no reader fed the
grid this frame, hide every element. Implement this as a dirty flag, not as a
clear before every read, because several postfixes may feed the grid in one
frame and the clear must not erase an earlier one.

The exact rule that works:

1. A reader that supplies icons sets the dirty flag.
2. The frame hook, after the readers, checks the flag. If the flag is clear,
   every element is already hidden and nothing happens. If the flag is set, the
   hook leaves the icons visible for this frame and clears the flag so the next
   frame hides them unless a reader feeds the grid again.

Also hide the whole container when `Hud.instance.IsVisible()` is false, when
`Player.m_localPlayer` is null, and when the inventory screen is open.
`InventoryGui.IsVisible()` is VERIFIED as `public static bool IsVisible()`.

Destroy on world unload. Destroy the child elements first, then the container.
Unity destroys a subtree when the parent goes, but destroying the parent first
leaves the child references dangling for the rest of the frame, and a reader
that fires in that same frame then writes into a destroyed object. Walk the
element list, destroy each element, clear the list, then destroy the container.

### 2.6 Row limit lesson

Keep exactly one number for the row limit and one for the icons per row. Derive
every other bound from those two.

An earlier implementation of this feature had four hard coded limits: one in the
grid builder, one in the icon writer, one in the configuration range, and one in
the container reader. The four numbers disagreed, so a second row of icons was
built but never filled, and the second row silently never appeared. Define the
maximum icon count as the icons per row multiplied by the row limit, and use
that one expression everywhere, including the configuration range.

### 2.7 Translation lesson

A missing translation key renders on screen as the literal `$key` text. That
mistake is visible to the user and looks like a crash.

Rules:

1. Use plain English for any OttoLens string. Do not invent a `$key`.
2. Call `Localization.instance.Localize(string)` only on a key that vanilla
   ships, and only when the reader appends that key to hover text. Vanilla keys
   already inside `__result` are already localized; do not localize twice.
3. Numbers, slashes, and percent signs need no translation.

VERIFIED, in `assembly_guiutils.dll`:

| Member | Signature |
| --- | --- |
| `Localization.instance` | `static Localization instance { get; }` |
| `Localization.Localize` | `public string Localize(string text)` |
| `Localization.Localize` | `public string Localize(string text, params string[] words)` |

## 3. Per-target readers

### 3.0 Shared reading rules

Almost every piece stores its live state in its ZDO, not in a field. A client
that is not the owner of the object reads the same ZDO, so remote pieces read
correctly, with one caveat: the values only advance when the owner ticks them.
A smelter in an unloaded zone shows the state as of the last owner tick. Say
"as of last update" nowhere in the display; just accept the staleness.

VERIFIED accessors:

| Member | Signature |
| --- | --- |
| `ZNetView.IsValid` | `public bool IsValid()` |
| `ZNetView.GetZDO` | `public ZDO GetZDO()` |
| `ZNetView.IsOwner` | `public bool IsOwner()` |
| `ZNetView.HasOwner` | `public bool HasOwner()` |
| `ZDO.GetFloat` | `public float GetFloat(int hash, float defaultValue = 0f)` |
| `ZDO.GetInt` | `public int GetInt(int hash, int defaultValue = 0)` |
| `ZDO.GetLong` | `public long GetLong(int hash, long defaultValue = 0L)` |
| `ZDO.GetString` | `public string GetString(int hash, string defaultValue = "")` |
| `ZDO.GetBool` | `public bool GetBool(int hash, bool defaultValue = false)` |
| `ZDO.GetByteArray` | `public byte[] GetByteArray(int hash, byte[] defaultValue = null)` |
| `ZDO.DataRevision` | `public uint DataRevision { get; set; }` |

Every getter also has a `string name` overload. Prefer the `int hash` overload
with a key from `ZDOVars`, and cache any key you build yourself from a name and
an index.

VERIFIED `ZDOVars` keys used below. Each is
`public static readonly int`, computed from the quoted string.

| Field | Underlying key |
| --- | --- |
| `ZDOVars.s_fuel` | `"fuel"` |
| `ZDOVars.s_lastTime` | `"lastTime"` |
| `ZDOVars.s_startTime` | `"StartTime"` |
| `ZDOVars.s_accTime` | `"accTime"` |
| `ZDOVars.s_bakeTimer` | `"bakeTimer"` |
| `ZDOVars.s_queued` | `"queued"` |
| `ZDOVars.s_item0` | `"item0"` |
| `ZDOVars.s_item` | `"item"` |
| `ZDOVars.s_spawnOre` | `"SpawnOre"` |
| `ZDOVars.s_spawnAmount` | `"SpawnAmount"` |
| `ZDOVars.s_level` | `"level"` |
| `ZDOVars.s_product` | `"product"` |
| `ZDOVars.s_content` | `"Content"` |
| `ZDOVars.s_health` | `"health"` |
| `ZDOVars.s_support` | `"support"` |
| `ZDOVars.s_picked` | `"picked"` |
| `ZDOVars.s_pickedTime` | `"picked_time"` |
| `ZDOVars.s_plantTime` | `"plantTime"` |
| `ZDOVars.s_seed` | `"seed"` |
| `ZDOVars.s_variant` | `"variant"` |
| `ZDOVars.s_quality` | `"quality"` |
| `ZDOVars.s_items` | `"items"` |
| `ZDOVars.s_inUse` | `"InUse"` |
| `ZDOVars.s_cheated` | `"cheated"` |
| `ZDOVars.s_cheatedQueued` | `"cheatedQueued"` |
| `ZDOVars.s_tameTimeLeft` | `"TameTimeLeft"` |
| `ZDOVars.s_tameLastFeeding` | `"TameLastFeeding"` |
| `ZDOVars.s_tamedName` | `"TamedName"` |
| `ZDOVars.s_state` | `"state"` |

Game clock. `ZNet.instance` is VERIFIED as `static ZNet instance { get; }` and
`ZNet.GetTime` is VERIFIED as `public DateTime GetTime()`. Several pieces store
a `DateTime` tick count in a ZDO long, and the elapsed seconds are the
difference from `ZNet.instance.GetTime()`.

Day length. `EnvMan.instance` is VERIFIED as `static EnvMan instance { get; }`.
`EnvMan.m_dayLengthSec` is VERIFIED as `public long m_dayLengthSec` with default
1200. It is a `long` in 1.0.7, so cast before dividing.
`EnvMan.IsDaylight` is VERIFIED as `public static bool IsDaylight()`.

Time formatting. Write one helper that turns seconds into a short human string.
Use `1h 05m`, `12m 30s`, and `45s` shapes. Add an optional day figure computed
as seconds divided by `EnvMan.instance.m_dayLengthSec`. Days are the only unit
the game itself gives the player a feel for, so a fireplace reading in days is
more useful than one in hours.

Access checks. `PrivateArea.CheckAccess` is VERIFIED as
`public static bool CheckAccess(Vector3 point, float radius = 0f, bool flash = true, bool wardCheck = false)`.
Call it with `flash: false` from a hover reader. A ward flash every frame is a
visual bug. Several vanilla `GetHoverText` methods already return a no access
string. When they do, append nothing and feed no icons.

### 3.1 Container: chest, cart, ship hold

Component: `Container`, VERIFIED as
`public class Container : MonoBehaviour, Hoverable, Interactable`.
Hook: postfix `Container.GetHoverText()`.

| Member | Signature |
| --- | --- |
| `Container.GetHoverText` | `public string GetHoverText()` |
| `Container.GetInventory` | `public Inventory GetInventory()` |
| `Container.m_name` | `public string m_name` |
| `Container.m_wagon` | `public Vagon m_wagon` |
| `Container.m_checkGuardStone` | `public bool m_checkGuardStone` |
| `Container.m_privacy` | `public Container.PrivacySetting m_privacy` |
| `Container.m_piece` | `private Piece m_piece` |
| `Container.m_nview` | `private ZNetView m_nview` |
| `Container.m_inUse` | `private bool m_inUse` |
| `Container.IsInUse` | `public bool IsInUse()` |
| `Container.CheckAccess` | `private bool CheckAccess(long playerID)` |
| `Container.PrivacySetting` | public nested enum with `Public`, `Private`, `Group` |

Inventory reading, all VERIFIED on `Inventory`:

| Member | Signature |
| --- | --- |
| `Inventory.GetAllItems` | `public List<ItemDrop.ItemData> GetAllItems()` |
| `Inventory.NrOfItems` | `public int NrOfItems()` |
| `Inventory.HaveEmptySlot` | `public bool HaveEmptySlot()` |
| `Inventory.GetEmptySlots` | `public int GetEmptySlots()` |
| `Inventory.GetWidth` | `public int GetWidth()` |
| `Inventory.GetHeight` | `public int GetHeight()` |
| `Inventory.SlotsUsedPercentage` | `public float SlotsUsedPercentage()` |

Item fields, all VERIFIED on `ItemDrop.ItemData` and its nested `SharedData`:

| Member | Signature |
| --- | --- |
| `m_stack` | `public int m_stack` |
| `m_quality` | `public int m_quality` |
| `m_durability` | `public float m_durability` |
| `m_variant` | `public int m_variant` |
| `m_gridPos` | `public Vector2i m_gridPos` |
| `GetMaxDurability` | `public float GetMaxDurability()` |
| `m_shared.m_name` | `public string m_name` |
| `m_shared.m_maxStackSize` | `public int m_maxStackSize` |
| `m_shared.m_description` | `public string m_description` |
| `m_shared.m_icons` | `public Sprite[] m_icons` |

What to show.

1. Group the items by shared name. Sum the stacks per group. Sort by summed
   count, descending, then by name, so the display is stable frame to frame.
2. Feed the grid one icon per group, in that order, up to the icon limit. The
   count label is the summed count. If every group holds exactly one item, leave
   every label empty; the icons alone are then clearer.
3. Append one summary line. With one group, show that group's item name. With
   several groups, show the group count. When the icon limit hid some groups,
   show how many groups are hidden instead.
4. When the inventory is full, meaning no empty slot and every stack at its
   maximum, add a full marker to the summary line.
5. When the inventory is empty, append an empty marker and feed no icons.

Edge cases.

- **No access.** When `m_checkGuardStone` is set and
  `PrivateArea.CheckAccess(transform.position, 0f, flash: false)` is false,
  vanilla already returns a no access string. Append nothing.
- **Private chest.** The private setting denies access to everyone but the
  creator. `Container.CheckAccess(long playerID)` gives the answer. The local
  player id comes from the player. When access is denied, append nothing and
  feed no icons. Do not reveal a locked chest's contents.
- **Contents not loaded.** The inventory is deserialized from the ZDO byte array
  under `ZDOVars.s_items` by a repeating one second check, not every frame. A
  chest that just came into view can read as empty for up to one second. Accept
  that. Do not force a load.
- **In use.** While the container is open the vanilla reload is suppressed. The
  inventory object is still live and correct for the local player, so nothing
  special is needed.
- **Cart and ship.** A cart container has `m_wagon` set. A ship hold container
  is found by walking up to a `Ship` component with
  `GetComponentInParent<Ship>()`. `Ship` is VERIFIED as
  `public class Ship : MonoBehaviour, IMonoUpdater`. Both are ordinary
  containers for reading purposes. The only reason to detect them is to skip the
  ward check, because a cart or a ship is not inside its own ward.
- **Not player built.** A container with no `Piece`, or whose piece reports
  `Piece.IsPlacedByPlayer()` false, is a world chest. Showing its contents
  removes the reason to open it. Offer this as a setting. When the setting hides
  them, feed one question mark icon and append a short "contents unknown" line
  in plain English. Remember which world chests the player has opened, so that a
  chest already seen keeps showing its contents. `InventoryGui.Show` is the
  place to record that. Clear the record when the world unloads.

`Piece.IsPlacedByPlayer` is VERIFIED as `public bool IsPlacedByPlayer()`.

Cache. `Container.m_nview.GetZDO().DataRevision` changes when the contents
change. Cache the grouped list and the icon list per container, keyed on that
revision value. Rebuild only when the revision moves. This is the single most
important performance rule in the mod, because a container hover holds still for
seconds at a time.

### 3.2 Fireplace, hearth, torch, standing brazier

Component: `Fireplace`, VERIFIED as
`public class Fireplace : MonoBehaviour, Hoverable, Interactable, IHasHoverMenu`.
Hook: postfix `Fireplace.GetHoverText()`.

| Member | Signature |
| --- | --- |
| `Fireplace.GetHoverText` | `public string GetHoverText()` |
| `Fireplace.m_name` | `public string m_name` |
| `Fireplace.m_maxFuel` | `public float m_maxFuel` |
| `Fireplace.m_secPerFuel` | `public float m_secPerFuel` |
| `Fireplace.m_infiniteFuel` | `public bool m_infiniteFuel` |
| `Fireplace.m_canRefill` | `public bool m_canRefill` |
| `Fireplace.m_canTurnOff` | `public bool m_canTurnOff` |
| `Fireplace.m_fuelItem` | `public ItemDrop m_fuelItem` |
| `Fireplace.IsBurning` | `public bool IsBurning()` |
| `Fireplace.m_nview` | `private ZNetView m_nview` |

Values.

- Fuel units remaining: the ZDO float under `ZDOVars.s_fuel`.
- Seconds remaining: fuel units multiplied by `m_secPerFuel`.
- Days remaining: seconds remaining divided by
  `(float)EnvMan.instance.m_dayLengthSec`.
- Capacity display: the ceiling of fuel units over `m_maxFuel`.

Lines to show.

1. Capacity, as current over maximum, when the icon is off. When the icon is on,
   the count belongs on the icon badge instead, so leave it out of the text.
2. Time remaining, in the day form when the day setting is on, otherwise in the
   short time form.
3. Feed one icon: the fuel item icon, with the capacity as its badge.

Edge cases.

- `m_infiniteFuel` is true for some pieces. Vanilla returns an empty hover text
  for those. Append nothing.
- When the network view is not valid, vanilla returns an empty string. Append
  nothing.
- A wet or blocked fire reports not burning while still holding fuel.
  `IsBurning()` covers wet, blocked, submerged, and switched off. When the fire
  is not burning, show the fuel figure without a time remaining, because the
  fuel is not draining.
- The state key `ZDOVars.s_state` holds 1 for on. A fire that can be turned off
  and is off holds a different value. Fuel does not drain in that state.

Do not change `m_secPerFuel`. An earlier implementation rewrote the burn rate so
that the day figure came out round. That changed the game.

### 3.3 Smelter, kiln, blast furnace, windmill fed smelter

Component: `Smelter`, VERIFIED as
`public class Smelter : MonoBehaviour, IHasHoverMenuExtended`. **The smelter is
not `Hoverable`.** Its hover text comes from the `Switch` components on the
piece. `Switch` is VERIFIED as
`public class Switch : MonoBehaviour, Interactable, Hoverable` with
`public Switch.TooltipCallback m_onHover` and
`public string GetHoverText()`, which invokes the callback.

The smelter assigns three private tooltip methods to those switches. Patch the
tooltip methods directly. They are VERIFIED:

| Member | Signature | Switch |
| --- | --- | --- |
| `Smelter.OnHoverAddOre` | `private string OnHoverAddOre()` | `m_addOreSwitch` |
| `Smelter.OnHoverAddFuel` | `private string OnHoverAddFuel()` | `m_addWoodSwitch` |
| `Smelter.OnHoverEmptyOre` | `private string OnHoverEmptyOre()` | `m_emptyOreSwitch` |

Postfix each one and extend `__result`. This is better than patching
`Switch.GetHoverText`, because it identifies the smelter and the switch in one
step, with no parent walk and no delegate comparison.

Other VERIFIED `Smelter` members:

| Member | Signature |
| --- | --- |
| `m_name` | `public string m_name` |
| `m_maxOre` | `public int m_maxOre` |
| `m_maxFuel` | `public int m_maxFuel` |
| `m_fuelPerProduct` | `public int m_fuelPerProduct` |
| `m_secPerProduct` | `public float m_secPerProduct` |
| `m_requiresRoof` | `public bool m_requiresRoof` |
| `m_haveRoof` | `private bool m_haveRoof` |
| `m_fuelItem` | `public ItemDrop m_fuelItem` |
| `m_conversion` | `public List<Smelter.ItemConversion> m_conversion` |
| `m_windmill` | `public Windmill m_windmill` |
| `m_nview` | `private ZNetView m_nview` |
| `IsActive` | `public bool IsActive()` |
| `GetFuel` | `private float GetFuel()` |
| `GetQueueSize` | `private int GetQueueSize()` |
| `GetQueuedOre` | `private string GetQueuedOre()` |
| `GetProcessedQueueSize` | `private int GetProcessedQueueSize()` |
| `GetBakeTimer` | `private float GetBakeTimer()` |
| `GetItemConversion` | `private Smelter.ItemConversion GetItemConversion(string itemName)` |
| `Smelter.ItemConversion.m_from` | `public ItemDrop m_from` |
| `Smelter.ItemConversion.m_to` | `public ItemDrop m_to` |

`Windmill.GetPowerOutput` is VERIFIED as `public float GetPowerOutput()`.

State layout in the ZDO.

- Fuel units: float under `ZDOVars.s_fuel`.
- Ore queue length: int under `ZDOVars.s_queued`.
- Ore queue entries: strings under the keys `item0`, `item1`, and so on up to
  the queue length minus one. Each is a prefab name.
  `ZDOVars.s_item0` is the hash of `item0`. Build the rest by name, and cache
  the built hashes in an array indexed by position.
- Bake progress in seconds toward the current product: float under
  `ZDOVars.s_bakeTimer`.
- Unclaimed accumulated real time: float under `ZDOVars.s_accTime`.
- Finished output waiting to be taken: name string under `ZDOVars.s_spawnOre`
  and count int under `ZDOVars.s_spawnAmount`.

Formulas.

- Seconds to the next product: `m_secPerProduct` minus the bake timer, divided
  by the windmill power output when a windmill is attached and its output is
  above zero.
- Seconds to finish the whole queue: the queue length multiplied by
  `m_secPerProduct`, minus the bake timer, with the same windmill divisor.
- Seconds of fuel left: fuel units multiplied by
  `m_secPerProduct / m_fuelPerProduct`. Guard against a zero
  `m_fuelPerProduct`.
- Effective seconds left: the smaller of the queue time and the fuel time. That
  is the honest "when does it stop" figure.

Lines to show, on the add ore tooltip.

1. Queue as current over `m_maxOre`.
2. One line per distinct ore in the queue, with its count, in queue order.
3. Time to the next product and time to finish the queue.
4. A no roof warning when `m_requiresRoof` is set and `m_haveRoof` is false.
   Vanilla already blinks this warning into the string on alternate frames.
   Blinking text reads as a glitch inside a longer status block, so remove the
   blinking copy and add a steady one.
5. Feed one icon per distinct ore, with its count as the badge.

Lines to show, on the add fuel tooltip.

1. Fuel as the ceiling of fuel units over `m_maxFuel`.
2. Seconds of fuel left, in the short time form.
3. Feed one icon: the fuel item, with the capacity as the badge.

Lines to show, on the empty output tooltip.

1. The output count from `ZDOVars.s_spawnAmount`.
2. Feed one icon: the output item of the conversion that matches the queued ore,
   with the output count as the badge. Resolve the conversion with
   `GetItemConversion` on the queued ore name. The conversion can be null, for
   example after a mod removed a recipe. Guard it.

Edge cases.

- A queue entry can be an empty string when the queue length and the stored
  entries disagree. Skip an empty entry. Do not treat it as an item.
- `m_maxFuel` can be zero for a smelter that needs no fuel. Then skip every fuel
  figure and treat the fuel time as unlimited.
- `m_maxOre` can be zero for a piece that needs no ore.
- A smelter with blocked smoke reports not active while still holding fuel and
  ore. `IsActive()` covers fuel, ore, roof, and smoke. When it is false, show
  the stock figures but no countdown.

### 3.4 Cooking station and oven

Component: `CookingStation`, VERIFIED as
`public class CookingStation : MonoBehaviour, Interactable, Hoverable, IHasHoverMenuExtended`.

Two hook points, because a station either is the hoverable itself or delegates
to switches.

1. `CookingStation.GetHoverText()`, VERIFIED as `public string GetHoverText()`.
   It returns the empty string when `m_addFoodSwitch` is set, which is the oven
   case. Postfix it for the plain cooking station.
2. For the oven, postfix the two private tooltip methods. `OnHoverFuelSwitch` is
   VERIFIED as `private string OnHoverFuelSwitch()`. The food switch has no
   tooltip method; the station instead writes the food hover string into
   `m_addFoodSwitch.m_hoverText` on every cooking update. To extend the oven
   food hover, postfix `Switch.GetHoverText()` and check whether the switch is
   the station's `m_addFoodSwitch`. Keep that postfix cheap: return at once
   unless the switch matches.

| Member | Signature |
| --- | --- |
| `m_name` | `public string m_name` |
| `m_slots` | `public Transform[] m_slots` |
| `m_conversion` | `public List<CookingStation.ItemConversion> m_conversion` |
| `m_canOvercookItems` | `public bool m_canOvercookItems` |
| `m_overCookedItem` | `public ItemDrop m_overCookedItem` |
| `m_useFuel` | `public bool m_useFuel` |
| `m_maxFuel` | `public int m_maxFuel` |
| `m_secPerFuel` | `public int m_secPerFuel` |
| `m_fuelItem` | `public ItemDrop m_fuelItem` |
| `m_requireFire` | `public bool m_requireFire` |
| `m_addFoodSwitch` | `public Switch m_addFoodSwitch` |
| `m_addFuelSwitch` | `public Switch m_addFuelSwitch` |
| `m_nview` | `private ZNetView m_nview` |
| `GetSlot` | `private void GetSlot(int slot, out string itemName, out float cookedTime, out CookingStation.Status status, out bool cheated)` |
| `GetFuel` | `private float GetFuel()` |
| `IsFireLit` | `private bool IsFireLit()` |
| `GetItemConversion` | `private CookingStation.ItemConversion GetItemConversion(string itemName)` |
| `IsEverythingCooked` | `private bool IsEverythingCooked()` |
| `CookingStation.Status` | private nested enum with `NotDone`, `Done`, `Burnt` |
| `CookingStation.ItemConversion.m_from` | `public ItemDrop m_from` |
| `CookingStation.ItemConversion.m_to` | `public ItemDrop m_to` |
| `CookingStation.ItemConversion.m_cookTime` | `public float m_cookTime` |

`GetSlot` returns `void` and takes four out parameters in 1.0.7. The fourth,
`cheated`, is new. Any older call shape with three out parameters does not
compile.

Slot state layout in the ZDO. Per slot index the keys are `slot` plus the index
for the item name as a string, the same `slot` plus index key for the cooked
seconds as a float, and `slotstatus` plus the index for the status as an int.
The name and the float share one key name. That is legal, because the ZDO keeps
a separate map per value type, and it is a trap for anyone who assumes one key
holds one value. Read the slot through `GetSlot` rather than by hand.

Values per slot.

- Empty when the item name is the empty string.
- Done when the status is `Done`. Burnt when the status is `Burnt`.
- Seconds to done: the conversion cook time minus the cooked seconds.
- Seconds to burnt: twice the conversion cook time minus the cooked seconds,
  when `m_canOvercookItems` is set.
- Progress fraction: the cooked seconds over the conversion cook time, clamped.

Lines to show.

1. One line per occupied slot: the item name, then either the time to done, or
   a ready marker, or a burnt marker.
2. The soonest burn time across the done slots, as a warning, because that is
   the number the player actually needs.
3. Fuel as the ceiling of fuel units over `m_maxFuel` and the fuel seconds,
   which are fuel units multiplied by `m_secPerFuel`, when `m_useFuel` is set.
4. A no fire marker when `m_requireFire` is set and `IsFireLit()` is false.
5. Feed one icon per occupied slot, in slot order, with no badge.

Edge cases.

- The conversion lookup matches either the input or the output prefab name, so
  it also resolves a slot that already holds a cooked item. It still returns
  null for the burnt item, because the burnt item is not part of any conversion.
  Guard for null and show only the burnt marker for that slot.
- The cooked seconds do not advance unless the fire is lit or the station holds
  fuel, and only the owner advances them.
- `m_secPerFuel` is an `int` on this component, with a value in the thousands,
  and it is a `float` on the fireplace. Do not share one fuel helper across the
  two without a cast.

### 3.5 Fermenter

Component: `Fermenter`, VERIFIED as
`public class Fermenter : MonoBehaviour, Hoverable, Interactable`.
Hook: postfix `Fermenter.GetHoverText()`, VERIFIED as
`public string GetHoverText()`.

| Member | Signature |
| --- | --- |
| `m_name` | `public string m_name` |
| `m_fermentationDuration` | `public float m_fermentationDuration` |
| `m_conversion` | `public List<Fermenter.ItemConversion> m_conversion` |
| `m_exposed` | `private bool m_exposed` |
| `m_hasRoof` | `private bool m_hasRoof` |
| `m_nview` | `private ZNetView m_nview` |
| `GetStatus` | `private Fermenter.Status GetStatus()` |
| `GetContent` | `private int GetContent()` |
| `GetContentName` | `private string GetContentName()` |
| `GetFermentationTime` | `private double GetFermentationTime()` |
| `GetItemConversion` | `private Fermenter.ItemConversion GetItemConversion(int nameHash)` |
| `Fermenter.Status` | private nested enum with `Empty`, `Fermenting`, `Exposed`, `Ready` |
| `Fermenter.ItemConversion.m_from` | `public ItemDrop m_from` |
| `Fermenter.ItemConversion.m_to` | `public ItemDrop m_to` |
| `Fermenter.ItemConversion.m_producedItems` | `public int m_producedItems` |

Content is a stable hash in 1.0.7, not a name. `GetContent` returns the int
under `ZDOVars.s_content`. `GetItemConversion` takes that int and compares it
against the stable hash code of each conversion input prefab name. Zero means
empty.

Values.

- Elapsed seconds: `GetFermentationTime()`. It returns minus one when no start
  time is stored, which means nothing has been added.
- Seconds remaining: `m_fermentationDuration` minus the elapsed seconds.
- Progress fraction: elapsed over duration, clamped.

Lines to show.

1. The content name. `GetContentName()` returns the literal string `Invalid`
   when the stored hash matches no conversion. Treat `Invalid` and the empty
   string as "no readable content" and show nothing for the content.
2. Seconds remaining while fermenting. A ready marker when ready.
3. An exposed warning when `m_exposed` is set, and a needs roof warning when
   `m_hasRoof` is false. Vanilla already puts one of these in the string; do not
   duplicate it.
4. Feed one icon: the conversion input item, with the produced item count as its
   badge. The conversion can be null. Guard it. Vanilla itself null checks the
   conversion here, and any reader that skips the check throws on a barrel that
   holds a removed recipe.

### 3.6 Beehive

Component: `Beehive`, VERIFIED as
`public class Beehive : MonoBehaviour, Hoverable, Interactable`.
Hook: postfix `Beehive.GetHoverText()`, VERIFIED as
`public string GetHoverText()`.

| Member | Signature |
| --- | --- |
| `m_name` | `public string m_name` |
| `m_maxHoney` | `public int m_maxHoney` |
| `m_secPerUnit` | `public float m_secPerUnit` |
| `m_honeyItem` | `public ItemDrop m_honeyItem` |
| `m_maxCover` | `public float m_maxCover` |
| `m_effectOnlyInDaylight` | `public bool m_effectOnlyInDaylight` |
| `m_biome` | `public Heightmap.Biome m_biome` |
| `m_areaText` | `public string m_areaText` |
| `m_freespaceText` | `public string m_freespaceText` |
| `m_sleepText` | `public string m_sleepText` |
| `m_happyText` | `public string m_happyText` |
| `m_nview` | `private ZNetView m_nview` |
| `GetHoneyLevel` | `private int GetHoneyLevel()` |
| `HaveFreeSpace` | `private bool HaveFreeSpace()` |
| `CheckBiome` | `private bool CheckBiome()` |

Values.

- Honey count: the int under `ZDOVars.s_level`, which is what `GetHoneyLevel()`
  returns.
- Progress toward the next honey, in seconds: the float under
  `ZDOVars.s_product`.
- Seconds to the next honey: `m_secPerUnit` minus that progress value.

Lines to show.

1. Honey as current over `m_maxHoney`.
2. Seconds to the next honey, when the hive is below its maximum and the hive is
   working.
3. The vanilla status message for the hive: blocked biome, no free space,
   asleep, or happy. The four message fields hold the vanilla keys. Localize the
   one that applies. Choose between them the same way vanilla does: biome first,
   then free space, then daylight when `m_effectOnlyInDaylight` is set,
   otherwise happy.
4. Feed icons. Below six honey, feed one honey icon per honey unit with no
   badge, so the count reads at a glance. At six or above, feed one honey icon
   with the count over the maximum as its badge.

Edge case. The progress value only advances while the owner ticks the hive and
the biome and free space checks pass. A covered hive shows a frozen countdown.
That is correct; the hive really is stalled. Show the status line so the reason
is visible.

Never call the vanilla time since last update helper. It writes the current time
back into the ZDO.

### 3.7 Sap collector

Component: `SapCollector`, VERIFIED with `public string GetHoverText()`.
Hook: postfix that method.

| Member | Signature |
| --- | --- |
| `m_name` | `public string m_name` |
| `m_maxLevel` | `public int m_maxLevel` |
| `m_secPerUnit` | `public float m_secPerUnit` |
| `m_spawnItem` | `public ItemDrop m_spawnItem` |
| `m_extractText` | `public string m_extractText` |
| `m_fullText` | `public string m_fullText` |
| `m_drainingText` | `public string m_drainingText` |
| `m_drainingSlowText` | `public string m_drainingSlowText` |
| `m_notConnectedText` | `public string m_notConnectedText` |
| `m_nview` | `private ZNetView m_nview` |
| `m_root` | `private ResourceRoot m_root` |
| `GetLevel` | `private int GetLevel()` |
| `GetStatusText` | `private string GetStatusText()` |

`ResourceRoot.GetLevel` is VERIFIED as `public float GetLevel()`,
`ResourceRoot.IsLevelLow` as `public bool IsLevelLow()`, and
`ResourceRoot.CanDrain` as `public bool CanDrain(float amount)`.

Values and lines mirror the beehive. Level is the int under `ZDOVars.s_level`.
Progress is the float under `ZDOVars.s_product`. Seconds to the next unit is
`m_secPerUnit` minus the progress. The vanilla hover already shows the level and
a status word, so OttoLens adds the countdown and the icons, and it adds a meter
when the meter setting is on. Feed the same icon pattern as the beehive, using
`m_spawnItem`.

Edge case. Sap only flows while the collector is attached to a tree root that
can still drain. When `m_root` is null the vanilla status word already says not
connected. Show no countdown in that case.

### 3.8 Plant and crop

Component: `Plant`, VERIFIED as `public class Plant : SlowUpdate, Hoverable`.
Hook: postfix `Plant.GetHoverText()`, VERIFIED as
`public string GetHoverText()`.

| Member | Signature |
| --- | --- |
| `m_name` | `public string m_name` |
| `m_growTime` | `public float m_growTime` |
| `m_growTimeMax` | `public float m_growTimeMax` |
| `m_grownPrefabs` | `public GameObject[] m_grownPrefabs` |
| `m_biome` | `public Heightmap.Biome m_biome` |
| `GetStatus` | `public Plant.Status GetStatus()` |
| `Plant.Status` | public nested enum with `Healthy`, `NoSun`, `NoSpace`, `WrongBiome`, `NotCultivated`, `NoAttachPiece`, `TooHot`, `TooCold` |
| `m_status` | `private Plant.Status m_status` |
| `m_seed` | `private int m_seed` |
| `m_nview` | `private ZNetView m_nview` |
| `TimeSincePlanted` | `private double TimeSincePlanted()` |
| `GetGrowTime` | `private float GetGrowTime()` |

Values.

- Elapsed seconds: the difference between the current game time and the tick
  count stored under `ZDOVars.s_plantTime`. `TimeSincePlanted()` does the same
  and does not write to the ZDO, so calling it is safe.
- Grow time in seconds: `GetGrowTime()`. It is a linear interpolation from
  `m_growTime` to `m_growTimeMax`, using a random value seeded from the plant's
  own seed, so it is stable for a given plant. The seed is the int under
  `ZDOVars.s_seed`. Prefer calling `GetGrowTime()` over reproducing it. If you
  must reproduce it, save the global random state, seed it, take one value,
  and restore the state, exactly as vanilla does. Failing to restore the state
  perturbs every other random draw in the game that frame.
- Seconds remaining: grow time minus elapsed seconds.
- Progress fraction: elapsed over grow time, clamped.

Lines to show.

1. The status word. `Healthy` means growing. `NoSpace` is the crowded case that
   players care about most, because a crowded crop never finishes.
   `NoAttachPiece` applies to a vine and reads better as "no wall".
2. Progress as a percent, and seconds remaining, but only when the status is
   `Healthy`. An unhealthy plant does not progress, so a countdown there is a
   lie.
3. Feed no icons for a plant. The grown prefab icon is not an item icon and the
   grown prefab array can be empty.

Colour the status word by meaning: healthy in the normal information colour,
too hot in a warm colour, too cold in a cool colour, everything else in a
warning colour. Colour with inline markup in the string. The hover text field is
a rich text component.

Vanilla only recomputes the status roughly every ten seconds in a slow update.
A plant that just gained a neighbour keeps the old status for a few seconds.
Do not recompute it yourself.

### 3.9 Pickable and pickable item

Components: `Pickable` and `PickableItem`, both VERIFIED as
`Hoverable, Interactable`. Hook: postfix `GetHoverText()` on each, VERIFIED as
`public string GetHoverText()`.

`Pickable` VERIFIED members:

| Member | Signature |
| --- | --- |
| `m_itemPrefab` | `public GameObject m_itemPrefab` |
| `m_amount` | `public int m_amount` |
| `m_overrideName` | `public string m_overrideName` |
| `m_respawnTimeMinutes` | `public float m_respawnTimeMinutes` |
| `m_respawnTimeInitMin` | `public float m_respawnTimeInitMin` |
| `m_respawnTimeInitMax` | `public float m_respawnTimeInitMax` |
| `m_harvestable` | `public bool m_harvestable` |
| `GetPicked` | `public bool GetPicked()` |
| `CanBePicked` | `public bool CanBePicked()` |
| `GetEnabled` | `public int GetEnabled { get; }` |
| `m_picked` | `private bool m_picked` |
| `m_nview` | `private ZNetView m_nview` |

`PickableItem` VERIFIED members: `public ItemDrop m_itemPrefab`,
`public int m_stack`, `public PickableItem.RandomItem[] m_randomItemPrefabs`,
`private bool m_picked`, and the nested struct `PickableItem.RandomItem` with
`public ItemDrop m_itemPrefab`, `public int m_stackMin`, `public int m_stackMax`.

Values.

- Picked state: the bool under `ZDOVars.s_picked`.
- Picked time: the tick count under `ZDOVars.s_pickedTime`. A value of zero
  means the timer has not started. A value of one is a sentinel the game writes
  when the computed initial time would have been negative.
- Minutes since picked: the difference between the current game time and the
  picked time, in minutes.
- Minutes to respawn: `m_respawnTimeMinutes` minus the minutes since picked.

Lines to show.

1. When not picked, append the item name and the yield count, and feed one icon:
   the item icon, with the yield count as the badge when it is above one.
2. When picked and `m_respawnTimeMinutes` is above zero, append the time to
   respawn. A picked bush usually returns an empty hover text from vanilla, so
   an appended line has nothing to attach to. Handle this in one of two ways.
   Either accept that picked pickables show nothing, which is the simple and
   safe choice, or render the respawn line from the frame hook when the hover
   object carries a picked pickable. Recommend the simple choice for the first
   release and gate the respawn line behind a setting.

Edge cases.

- A pickable with `m_respawnTimeMinutes` at zero never respawns. Show no timer.
- `GetEnabled` returning zero means the pickable is switched off by the game,
  for example a seasonal spawn. Show nothing.
- `PickableItem` picks its concrete item at spawn from the random list. Read
  `m_itemPrefab` after that, never the random list.

### 3.10 Build piece status when a hammer is not out

`WearNTear` is on almost every build piece. When the piece also carries a
`Hoverable`, for example a chest or a fireplace, the hover text exists and
OttoLens can append the piece condition to it. That is the "always" mode.

VERIFIED `WearNTear` members:

| Member | Signature |
| --- | --- |
| `m_health` | `public float m_health` |
| `m_materialType` | `public WearNTear.MaterialType m_materialType` |
| `m_minToolTier` | `public int m_minToolTier` |
| `m_supports` | `public bool m_supports` |
| `m_noSupportWear` | `public bool m_noSupportWear` |
| `m_noRoofWear` | `public bool m_noRoofWear` |
| `m_burnable` | `public bool m_burnable` |
| `GetHealthPercentage` | `public float GetHealthPercentage()` |
| `Repair` | `public bool Repair()` |
| `m_healthPercentage` | `private float m_healthPercentage` |
| `m_haveRoof` | `private bool m_haveRoof` |
| `m_support` | `private float m_support` |
| `GetSupport` | `private float GetSupport()` |
| `GetMaxSupport` | `private float GetMaxSupport()` |
| `GetMinSupport` | `private float GetMinSupport()` |
| `GetSupportColorValue` | `private float GetSupportColorValue()` |
| `GetMaterialProperties` | `private void GetMaterialProperties(out float maxSupport, out float minSupport, out float horizontalLoss, out float verticalLoss)` |
| `HaveSupport` | `private bool HaveSupport()` |
| `WearNTear.MaterialType` | public nested enum, values include `Wood`, `HardWood`, `Stone`, `Iron`, `Marble`, `Ashstone`, `Ancient`, `Ice`, `Timberwood` |

Values.

- Health fraction: `GetHealthPercentage()`. It is a clamped fraction, not a
  percent, despite the name. Multiply by one hundred for display.
- Maximum health: `m_health`. It already includes the world level multiplier,
  which the component applies once at wake time from
  `Game.instance.m_worldLevelPieceHPMultiplier` and `Game.m_worldLevel`.
- Current health: the health fraction multiplied by `m_health`.
- Support: `GetSupport()`. It returns the local value when this client owns the
  piece, the float under `ZDOVars.s_support` when another client owns it, and
  the material maximum when the piece has no owner or no valid view.
- Support maximum and minimum: `GetMaxSupport()` and `GetMinSupport()`, both
  derived from the material type.
- Integrity fraction for display: support over the support maximum, clamped.
- Wear state, matching the vanilla visual switch: new above 0.75 health
  fraction, worn above 0.25, broken at or below 0.25.

`Game.instance` is VERIFIED as `static Game instance { get; private set; }`,
`Game.m_worldLevel` as `public static int m_worldLevel`,
`Game.instance.m_worldLevelPieceHPMultiplier` as
`public float m_worldLevelPieceHPMultiplier`, and
`Game.instance.m_worldLevelMineHPMultiplier` as
`public float m_worldLevelMineHPMultiplier`.

Lines to show: current over maximum health with the percent, the integrity
percent, and the wear word. Colour each by threshold, with configurable low,
middle, and high colours.

Caveat on the health fraction. The component recomputes it at wake time and on
the health changed broadcast, not continuously. It is the same value the vanilla
piece health bar uses, so it is as fresh as vanilla is.

### 3.11 Build piece status while the hammer is out

In place mode there is no hover object and no hover text, so hook A cannot fire.
Use the frame hook.

1. Read `player.GetHoveringPiece()`. Null means no piece under the build
   raycast.
2. Get the `WearNTear` on that piece. Null means an indestructible piece. Show
   nothing.
3. Compute the same values as section 3.10.
4. Render them into two text objects that OttoLens owns, one for health and one
   for integrity, each with its own configurable pixel offset from the crosshair
   and its own font size. Parent them beside the icon grid container, so one
   lifecycle covers all of it.
5. Optionally show a health bar. Vanilla already shows one at
   `Hud.m_pieceHealthRoot`, driven from the same fraction, so a second bar is
   redundant. Offer a setting that hides the vanilla bar instead of adding
   another.
6. Clear both text objects on any frame where the hovering piece is null.

`Piece` VERIFIED members that are useful here: `public string m_name`,
`public Piece.Requirement[] m_resources`, `public bool IsPlacedByPlayer()`,
`public long GetCreator()`, and the nested
`Piece.Requirement` with `public ItemDrop m_resItem`, `public int m_amount`, and
`public int GetAmount(int qualityLevel)`.

### 3.12 Tree, stump, log, and mineable rock

This group needs care, because most of it is not hoverable.

| Component | Hoverable | Signature evidence |
| --- | --- | --- |
| `MineRock` | yes | `public class MineRock : MonoBehaviour, IDestructible, Hoverable` |
| `MineRock5` | yes | `public class MineRock5 : MonoBehaviour, IDestructible, Hoverable` |
| `TreeBase` | no | `public class TreeBase : MonoBehaviour, IDestructible` |
| `TreeLog` | no | `public class TreeLog : MonoBehaviour, IDestructible, IProjectile` |
| `Destructible` | no | `public class Destructible : MonoBehaviour, IDestructible` |

For the two rock types, hook A works. `MineRock.GetHoverText()` and
`MineRock5.GetHoverText()` are both VERIFIED as `public string GetHoverText()`
and both return only the piece name, so there is plenty of room to append.

`MineRock` VERIFIED members: `public string m_name`, `public float m_health`,
`public int m_minToolTier`, `public float GetHealth()`,
`private ZNetView m_nview`. `GetHealth()` returns the world level scaled maximum
health, computed from `m_health`, `Game.m_worldLevel`, and
`Game.instance.m_worldLevelMineHPMultiplier`. This type keeps no per area health
in the ZDO, so show the maximum health and the tool tier only.

`MineRock5` VERIFIED members: `public string m_name`,
`public float m_health`, `public int m_minToolTier`,
`public bool m_supportCheck`, `private List<MineRock5.HitArea> m_hitAreas`,
`private ZNetView m_nview`, `private void LoadHealth()`,
`private MineRock5.HitArea GetHitArea(int index)`,
`private int GetAreaIndex(Collider area)`, `private bool AllDestroyed()`. The
nested `MineRock5.HitArea` is a private class with `public float m_health`,
`public Collider m_collider`, and `public bool m_supported`. Per area health is
persisted as a base64 string under `ZDOVars.s_health`, packed as a count
followed by one float per area, and it is loaded into the hit area list whenever
the data revision changes. Read the loaded list, not the packed string.

What to show for a five area rock: the number of intact areas over the total,
the summed remaining health over the summed maximum, and the tool tier. Showing
the health of the one area under the crosshair would be better, but the hover
text hook does not receive the collider that was hit, so it is out of reach
without a second raycast. Do not add that raycast for this feature.

For the tree group there is no hover text at all, because the components are not
hoverable. Two options exist.

**Option one, recommended.** Extend the frame hook. After the vanilla crosshair
update, when the hover object is null, cast one ray from the camera along its
forward vector with the same interact layer mask and distance the player uses,
and look for a `TreeBase`, `TreeLog`, or `Destructible` on the hit. Write the
status into `Hud.instance.m_hoverName.text`. Cost is one raycast per frame, and
only when nothing else is hovered. Gate it behind the tree setting, default off,
so a player who does not want it pays nothing.

**Option two.** Attach a small OttoLens component that implements `Hoverable` to
the tree prefabs at scene setup, the way a mod adds hover text to a resource
pile. This is cheaper per frame, but it changes vanilla behavior: the crosshair
turns yellow on a tree, which reads as "this is interactable". If you take this
option, keep the setting default off and document the crosshair change.

Values for the tree group.

- Maximum health: `m_health` on the component, scaled by
  `Game.m_worldLevel` multiplied by `m_health` multiplied by
  `Game.instance.m_worldLevelMineHPMultiplier`, added to `m_health`. That is the
  same expression vanilla uses as the ZDO default.
- Current health: the float under `ZDOVars.s_health`, defaulting to the scaled
  maximum.
- Tool tier: `m_minToolTier`. Show it as the minimum tool tier needed, and mark
  it clearly when the player's equipped tool tier is below it, because that is
  the case where nothing happens when the player swings.

VERIFIED fields: `TreeBase.m_health` is `public float m_health`,
`TreeBase.m_minToolTier` is `public int m_minToolTier`,
`TreeLog.m_health` is `public float m_health`,
`TreeLog.m_minToolTier` is `public int m_minToolTier`,
`Destructible.m_health` is `public float m_health`,
`Destructible.m_minToolTier` is `public int m_minToolTier`, and
`Destructible.m_destructibleType` is
`public DestructibleType m_destructibleType`.

### 3.13 Item stand

Component: `ItemStand`, VERIFIED as
`public class ItemStand : MonoBehaviour, Interactable, Hoverable`.
Hook: postfix `ItemStand.GetHoverText()`, VERIFIED as
`public string GetHoverText()`.

| Member | Signature |
| --- | --- |
| `m_name` | `public string m_name` |
| `m_currentItemName` | `public string m_currentItemName` |
| `m_canBeRemoved` | `public bool m_canBeRemoved` |
| `m_guardianPower` | `public StatusEffect m_guardianPower` |
| `m_supportedItems` | `public List<ItemDrop> m_supportedItems` |
| `HaveAttachment` | `public bool HaveAttachment()` |
| `GetAttachedItem` | `public int GetAttachedItem()` |
| `m_visualHash` | `private int m_visualHash` |
| `m_visualVariant` | `private int m_visualVariant` |
| `m_nview` | `private ZNetView m_nview` |

In 1.0.7 the attached item is a stable hash, not a name. The field is
`m_visualHash`, and there is no `m_visualName`. There is also no `IsBossStone`
method and no `CanBeRemoved` method on this component in 1.0.7. Use the
`m_canBeRemoved` field and the guardian power field to tell a decorative stand
from a boss offering stone: a stand with `m_canBeRemoved` false and a non null
`m_guardianPower` is a boss stone.

Prefer `GetAttachedItem()` over `m_visualHash`. It reads the authoritative int
under `ZDOVars.s_item`, so it is correct even before the local visual has
updated. Resolve the hash to a prefab through
`ObjectDB.TryGetItemPrefab(int, out GameObject)`, then take the item data.
Variant and quality come from the ints under `ZDOVars.s_variant` and
`ZDOVars.s_quality`.

Lines to show: the attached item name, and the item description when the
description setting is on. Feed one icon: the attached item, indexed by its
variant. Show nothing for a boss stone; vanilla already shows the power text
there and appending to it is noise.

### 3.14 Armor stand

Component: `ArmorStand`. It has **no** `GetHoverText` method and does not
implement `Hoverable`. Each slot has its own `Switch`, and the pose control has
another. Hook: postfix `Switch.GetHoverText()` and resolve an `ArmorStand` with
`GetComponentInParent<ArmorStand>()`. Return at once when there is none.

| Member | Signature |
| --- | --- |
| `m_name` | `public string m_name` |
| `m_slots` | `public List<ArmorStand.ArmorStandSlot> m_slots` |
| `m_changePoseSwitch` | `public Switch m_changePoseSwitch` |
| `m_visEquipment` | `public VisEquipment m_visEquipment` |
| `HaveAttachment` | `public bool HaveAttachment(int index)` |
| `GetAttachedItem` | `public int GetAttachedItem(int index)` |
| `GetNrOfAttachedItems` | `public int GetNrOfAttachedItems()` |
| `m_nview` | `private ZNetView m_nview` |
| `ArmorStand.ArmorStandSlot.m_switch` | `public Switch m_switch` |
| `ArmorStand.ArmorStandSlot.m_visualHash` | `public int m_visualHash` |
| `ArmorStand.ArmorStandSlot.m_visualVariant` | `public int m_visualVariant` |
| `ArmorStand.ArmorStandSlot.m_currentItemName` | `public string m_currentItemName` |
| `ArmorStand.ArmorStandSlot.m_item` | `public ItemDrop.ItemData m_item` |
| `ArmorStand.ArmorStandSlot.m_supportedTypes` | `public List<ItemDrop.ItemData.ItemType> m_supportedTypes` |

Read each slot with `GetAttachedItem(index)`, which returns the int stored under
a per slot key built from the index and the suffix `_item`. A slot with zero is
empty. The per slot stable hash replaced the older per slot name string; a
reader that looks for the string finds nothing and reports every slot as empty.

Lines to show: one line per occupied slot, giving the item name from
`m_currentItemName` on the slot, localized. Feed one icon per occupied slot.
Sort the icons so that weapons come first, then shields, then helmets, then body
armor, then utility items. Rank by
`ItemDrop.ItemData.m_shared.m_itemType`, VERIFIED as
`public ItemDrop.ItemData.ItemType m_itemType`, whose enum includes
`OneHandedWeapon`, `TwoHandedWeapon`, `TwoHandedWeaponLeft`, `Bow`, `Tool`,
`Attach_Atgeir`, `Shield`, `Helmet`, `Chest`, `Legs`, `Shoulder`, and `Utility`.

Both the icons and the text lines are worth separate settings. The text list is
useful; the icon row on an armor stand is largely decorative.

### 3.15 Tombstone

Component: `TombStone`, VERIFIED as
`public class TombStone : MonoBehaviour, Hoverable, Interactable`.
Hook: postfix `TombStone.GetHoverText()`, VERIFIED as
`public string GetHoverText()`.

| Member | Signature |
| --- | --- |
| `m_text` | `public string m_text` |
| `GetOwnerName` | `public string GetOwnerName()` |
| `m_container` | `private Container m_container` |
| `m_nview` | `private ZNetView m_nview` |
| `IsOwner` | `private bool IsOwner()` |

The vanilla hover already gives the owner name. It returns the empty string when
the grave is empty. Append the item count from
`m_container.GetInventory().NrOfItems()`, and feed one icon: the death map
marker from the minimap sprite lookup. Showing the grave's contents as icons is
possible through the container, but a grave usually holds a full inventory, so
the icon row overflows and tells the player nothing. Show the count instead.

### 3.16 Tamed creature and pet

Component: `Tameable`, VERIFIED as
`public class Tameable : MonoBehaviour, Interactable, TextReceiver`. It has a
`GetHoverText` method but does not implement `Hoverable`; the character or pet
component calls it. Hook: postfix `Tameable.GetHoverText()`, VERIFIED as
`public string GetHoverText()`.

| Member | Signature |
| --- | --- |
| `m_fedDuration` | `public float m_fedDuration` |
| `m_tamingTime` | `public float m_tamingTime` |
| `m_startsTamed` | `public bool m_startsTamed` |
| `m_commandable` | `public bool m_commandable` |
| `m_tamingBoostMultiplier` | `public float m_tamingBoostMultiplier` |
| `m_tamingSpeedMultiplierRange` | `public float m_tamingSpeedMultiplierRange` |
| `m_saddleItem` | `public ItemDrop m_saddleItem` |
| `IsTamed` | `public bool IsTamed()` |
| `IsHungry` | `public bool IsHungry()` |
| `GetStatusString` | `public string GetStatusString()` |
| `GetName` | `public string GetName()` |
| `HaveRider` | `public bool HaveRider()` |
| `GetRemainingTime` | `private float GetRemainingTime()` |
| `GetTameness` | `private int GetTameness()` |
| `m_nview` | `private ZNetView m_nview` |
| `m_character` | `private Character m_character` |
| `m_monsterAI` | `private MonsterAI m_monsterAI` |

Values.

- Taming seconds left: `GetRemainingTime()`, which is the float under
  `ZDOVars.s_tameTimeLeft`, defaulting to `m_tamingTime`.
- Taming percent: `GetTameness()`, which is one minus the clamped ratio of
  remaining time to `m_tamingTime`, times one hundred.
- Hunger: `IsHungry()`. It compares the current game time against the tick count
  under `ZDOVars.s_tameLastFeeding` and the `m_fedDuration` window.
- Seconds until hungry: `m_fedDuration` minus the seconds since the last
  feeding.

Lines to show. The vanilla hover already shows the tameness percent and a status
word. Add the seconds until hungry for a tamed creature, and the taming seconds
left for a creature still being tamed. Feed no icons here, except for the pet
case below.

Important. Taming progress only ticks while the creature is not alerted, not
hungry, and owned by a client that is running its update. A frightened creature
shows a frozen countdown, and the vanilla status word explains why.

For the pet piece, component `Pet`, VERIFIED as
`public class Pet : MonoBehaviour, Hoverable, Interactable, IRemoved, IPlaced`,
hook `Pet.GetHoverText()`, VERIFIED as `public string GetHoverText()`. Its
VERIFIED members include `public ItemDrop m_FeedItem`,
`private ItemStand m_itemStand`, `private Tameable m_tameable`, and
`private ZNetView m_nview`. The pet forwards to the tameable text and adds its
attached item, so the tameable postfix already covers the status. Add only the
icon: the item on the pet's own item stand, resolved as in section 3.13.

### 3.17 Wisp spawner

Component: `WispSpawner`, VERIFIED as
`public class WispSpawner : MonoBehaviour, Hoverable`.
Hook: postfix `WispSpawner.GetHoverText()`, VERIFIED as
`public string GetHoverText()`.

VERIFIED members: `public string m_name`, `public string m_noSpaceString`,
`public string m_fullString`, `public string m_tooMuchLightString`,
`public string m_wispsAreComingString`, `public int m_maxSpawned`,
`public float m_spawnInterval`, `public float m_spawnChance`,
`public bool m_onlySpawnAtNight`, `public float m_maxCover`,
`private WispSpawner.Status GetStatus()`, and the private nested enum
`WispSpawner.Status`.

The vanilla hover already names the status inside parentheses on one line. The
only useful change is formatting: put the status on its own line so a long
status string does not run past the crosshair. Feed no icons.

### 3.18 Shield generator

Component: `ShieldGenerator`. Its hover comes from a `Switch`, the same as the
smelter. Hook: postfix `ShieldGenerator.OnHoverAddFuel()`, VERIFIED as
`private string OnHoverAddFuel()`.

| Member | Signature |
| --- | --- |
| `m_name` | `public string m_name` |
| `m_maxFuel` | `public int m_maxFuel` |
| `m_defaultFuel` | `public int m_defaultFuel` |
| `m_fuelItems` | `public List<ItemDrop> m_fuelItems` |
| `m_fuelPerDamage` | `public float m_fuelPerDamage` |
| `m_minShieldRadius` | `public float m_minShieldRadius` |
| `m_maxShieldRadius` | `public float m_maxShieldRadius` |
| `GetFuelRatio` | `public float GetFuelRatio()` |
| `GetFuel` | `private float GetFuel()` |
| `m_nview` | `private ZNetView m_nview` |

Fuel is the float under `ZDOVars.s_fuel`, defaulting to `m_defaultFuel`. The
vanilla hover already shows the ceiling of fuel over the maximum. Add a text
meter drawn from filled and empty block characters, sized from the ratio of fuel
to `m_maxFuel`, when the meter setting is on. Feed one icon: the first entry of
`m_fuelItems`, with the capacity as the badge.

### 3.19 Feast

Component: `Feast`, VERIFIED as
`public class Feast : MonoBehaviour, Hoverable, Interactable`.
Hook: postfix `Feast.GetHoverText()`, VERIFIED as
`public string GetHoverText()`.

VERIFIED members: `public int m_eatStacks`, `public float m_useDistance`,
`public ItemDrop m_foodItem`, `public int GetStack()`,
`public float GetStackPercentige()`, `private bool InUseDistance(Humanoid human)`,
`private ZNetView m_nview`.

The vanilla hover shows the servings left inside parentheses, and it shows a
"too far" line instead when the player is out of range. Move the servings figure
onto an icon badge and feed one icon from `m_foodItem`. Append nothing when the
hover text is the "too far" string. Detect that case by checking
`InUseDistance(Player.m_localPlayer)` rather than by matching the string.

### 3.20 Item drop on the ground

Component: `ItemDrop`, hook: postfix `ItemDrop.GetHoverText()`, VERIFIED as
`public string GetHoverText()`. `ItemDrop.GetHoverName()` is VERIFIED as
`public string GetHoverName()` and `ItemDrop.GetHoverOffset()` as
`public float GetHoverOffset()`.

The vanilla hover appends the stack count to the name as an `x` followed by the
number. Move that count onto the icon badge and remove the trailing count from
the text, so the name reads cleanly. Feed one icon from `m_itemData`. Append the
item description when the description setting is on.

This reader fires very often, because item drops litter a base. Keep it to two
string operations and one icon.

### 3.21 Crafting station

Component: `CraftingStation`, hook: postfix `CraftingStation.GetHoverText()`,
VERIFIED as `public string GetHoverText()`. VERIFIED members:
`public string m_name`, `public float m_useDistance`,
`public bool InUseDistance(Humanoid human)`, `private ZNetView m_nview`.

Some stations carry a `Container` as well. When they do, and the player is in
use distance, the container reader has the more useful information, so feed the
container icons from here too. Do not replace the station's hover text with the
container's. Append the container summary instead.

## 4. Configuration surface

### 4.1 What the earlier mod exposed

Grouped by behavior, not by name.

- **Global.** A master switch for the hover icons. A switch for showing static
  resource piles and stacks as icons. A hold key that reveals the total coin
  value of the valuables in a container, with an option to always show it.
- **Fuel group.** Three separate flag sets, each selecting which fuel bearing
  piece types are affected: which show an icon, which show a capacity as current
  over maximum, and which show a text meter. The piece types were fireplaces,
  smelter fuel, and smelter ore. A switch for showing fireplace fuel as game
  days. A switch that rewrote the fireplace burn rate so that the day figure
  came out round. A switch for listing the smelter ore queue.
- **Containers.** A maximum icon count with a range bound. A switch for the
  summary text. A switch for keeping or removing the vanilla hold to stack hint.
  A switch that hides the contents of containers the player did not build until
  the player opens them once.
- **Armor stand.** Separate switches for the icons and for the text list.
- **Items.** A switch for appending the item description on ground items and on
  stands.
- **Shield generator.** A switch for the text meter.
- **Plants.** A switch for reformatting the crop hover text.

A second configuration shape, grouped the same way, adds these ideas.

- A keyboard shortcut that switches every visual off and on. Default `H`.
- Per target type switches for build pieces, fireplaces, cooking stations, mine
  rocks, trees, pickables, plants, sap collectors, beehives, and fermenters,
  with trees defaulting to off and the rest defaulting to on.
- For build pieces: separate switches for the health bar, the health text, and
  the integrity text; switches for using custom colours for health and for
  integrity; six colour values, being low, middle, and high for each of health
  and integrity; format strings for the health and integrity text with
  placeholders for current, maximum, and percent; a font size for each; and a
  pixel offset position for each.
- For chest contents: a master switch, a maximum entry count where minus one
  means all, a sort field, a sort direction, a per entry format string, an
  overflow marker string, and a capacity format string showing used slots over
  total slots.
- A note that the mod is client only and that a file watcher reloads the
  configuration when the file changes on disk.

### 4.2 Recommended set for OttoLens

Keep it compact. Eighteen settings, in five sections. Client only. No server
sync, because nothing here affects fairness: every value shown is already
readable by opening the piece.

**General.**

1. `Master` toggle. Off disables every OttoLens visual and every patch body.
   Default on.
2. `ToggleKey`. A key that flips the master toggle at runtime. Default `H`.
   Honour `KeyCode.None` as "no key".
3. `ShowTimes` toggle. Show the time remaining figures. Default on.
4. `ShowDays` toggle. Express long fuel and grow times in game days as well as
   in clock time. Default on.
5. `ShowMeters` toggle. Draw text meters for fuel, ore, sap, honey, and shield
   power. Default off.

**Icons.**

6. `ShowIcons` toggle. Default on.
7. `IconsPerRow`. Integer, range 1 to 12, default 6.
8. `IconRows`. Integer, range 1 to 3, default 2. The maximum icon count is this
   value multiplied by `IconsPerRow`, and that product is the only bound used
   anywhere.
9. `IconSize`. Float, range 24 to 64 reference pixels, default 42.

**Containers.**

10. `Containers` toggle. Default on.
11. `ContainerSummary` toggle. Show the group count and the full marker. Default
    on.
12. `HideUnopenedWorldChests` toggle. Default on.

**Targets.**

13. `Pieces`. A three state setting: `Off`, `WithHammer`, `Always`. Default
    `WithHammer`.
14. `Processors` toggle. Fireplaces, smelters, cooking stations, fermenters,
    beehives, sap collectors, and shield generators. Default on.
15. `Nature` toggle. Plants and pickables. Default on.
16. `Mineables` toggle. Rocks. Default on.
17. `Trees` toggle. Trees, stumps, and logs. Default off, because it costs a
    raycast or a prefab change. Document that.
18. `Decor` toggle. Armor stands, item stands, tombstones, tamed creatures, wisp
    spawners, feasts, and ground item drops. Default on.

Two colour values are enough: one warning colour and one information colour.
Six configurable piece colours is more surface than the feature earns. If the
build piece colours prove popular, add them later behind the `Pieces` section.

Every toggle must short circuit its reader on the first line. A disabled feature
should cost one boolean read per hover, not a formatted string that is thrown
away.

## 5. Plugin lifecycle

### 5.1 Awake

1. Bind every configuration entry. Bind them all in `Awake`, before patching, so
   that no patch body can read an unbound entry.
2. Create one Harmony instance keyed on the plugin identifier. Patch all
   annotated classes with one call.
3. Subscribe to the setting changed events for the icon geometry settings, so
   that a change rebuilds the grid rather than leaving a stale layout.
4. Do not touch `Hud.instance`, `Player.m_localPlayer`, `ObjectDB.instance`, or
   `InventoryGui.instance` in `Awake`. All four are null at that point.

### 5.2 World start

The grid needs a heads up display, so build it when one exists. Two workable
triggers.

- A postfix on `Hud.Awake`, VERIFIED as `private void Awake()` on `Hud`, which
  sets the static instance as its first statement.
- A lazy build inside the frame hook: if the container is null and
  `Hud.instance` is not null, build it now.

Prefer the lazy build. It has no ordering dependency on other mods that also
patch the display's wake method, and the frame hook already runs at the right
moment.

At build time: create the container, create the elements, create the two build
piece text objects, and clear the icon and sprite caches.

### 5.3 Scene change and logout

A logout destroys the heads up display object and every world object. Anything
OttoLens cached becomes a stale reference, and a stale `Sprite` or `Transform`
reference from the previous session is the classic source of a null reference
spam on the next login.

Clean up when the display goes away. The reliable trigger is a postfix on
`Hud.OnDestroy`, VERIFIED as `private void OnDestroy()` on `Hud`, which clears
the static instance.

On that trigger:

1. Destroy the element objects, one at a time, then clear the element list.
2. Destroy the two build piece text objects.
3. Destroy the container object last.
4. Null every cached `Transform`, `Image`, `TMP_Text`, and `GridLayoutGroup`
   reference.
5. Clear the sprite cache, the item group cache keyed on container data
   revision, and any set of revealed world chests.
6. Clear the cached per index ZDO key arrays only if they hold anything other
   than ints. Int key hashes are session independent and may stay.

Every reader must also tolerate a null container. Guard the grid entry point
with a single null check and return.

### 5.4 Harmony patch inventory

One patch class per target type, each holding one or two postfixes. The full
list, all postfixes unless marked:

1. `Hud.UpdateCrosshair(Player, float)`, the frame hook.
2. `Hud.OnDestroy()`, the cleanup hook.
3. `Container.GetHoverText()`.
4. `InventoryGui.Show(Container, int)`, to record an opened world chest. The
   signature is VERIFIED as
   `public void Show(Container container, int activeGroup = 1)`. The container
   argument is null when the player opens only their own inventory, so guard it.
5. `Fireplace.GetHoverText()`.
6. `Smelter.OnHoverAddOre()`, `Smelter.OnHoverAddFuel()`,
   `Smelter.OnHoverEmptyOre()`.
7. `CookingStation.GetHoverText()`, `CookingStation.OnHoverFuelSwitch()`.
8. `Switch.GetHoverText()`, one shared postfix that dispatches to the armor
   stand reader and the oven food reader. Keep it to one component lookup and an
   early return.
9. `Fermenter.GetHoverText()`.
10. `Beehive.GetHoverText()`.
11. `SapCollector.GetHoverText()`.
12. `Plant.GetHoverText()`.
13. `Pickable.GetHoverText()`, `PickableItem.GetHoverText()`.
14. `MineRock.GetHoverText()`, `MineRock5.GetHoverText()`.
15. `ItemStand.GetHoverText()`.
16. `TombStone.GetHoverText()`.
17. `Tameable.GetHoverText()`, `Pet.GetHoverText()`.
18. `WispSpawner.GetHoverText()`.
19. `ShieldGenerator.OnHoverAddFuel()`.
20. `Feast.GetHoverText()`.
21. `ItemDrop.GetHoverText()`.
22. `CraftingStation.GetHoverText()`.

Use no prefixes and no transpilers. A prefix that suppresses the original method
replaces the vanilla string, which breaks every other hover mod and every
translation. An earlier implementation used suppressing prefixes on the
fireplace, the fermenter, the sap collector, and plants, and rebuilt the whole
string by hand. That is why it had to re-add key hints, ward messages, and roof
warnings, and why a vanilla wording change silently dropped lines. Append only.

## 6. Risks and gotchas

### 6.1 Null and empty cases

1. **Null hover target.** Both hover fields are null on many frames: in place
   mode, while dead, while driving a boat or cart, and whenever nothing is in
   range. The frame hook must handle all null in its first lines.
2. **Empty hover text.** Several vanilla methods return the empty string:
   fireplace with infinite fuel, fireplace with an invalid view, cooking station
   with a food switch, picked pickable, empty tombstone, item stand for a
   guardian power that is mid activation, and feast with no servings. A postfix
   that appends to an empty string produces a status block with a leading blank
   line and no name. Always return early when `__result` is null or empty,
   unless the reader deliberately owns that case.
3. **No access.** A ward denied hover returns a short no access string. Append
   nothing and feed no icons. Never call the access check with the flash flag
   set from a hover reader.
4. **Invalid network view.** Every reader that touches a ZDO must first check
   `IsValid()` on the view and check the ZDO for null. A view is invalid during
   placement preview and for one frame after spawn.
5. **Placement ghost.** The piece the player is about to place is a live object
   with components and no ZDO. Guard with `Player.IsPlacementGhost`, VERIFIED as
   `public static bool IsPlacementGhost(GameObject obj)`. That is the guard
   vanilla itself uses.
6. **Dedicated server.** `Hud.instance` is null on a dedicated server and there
   is no local player. Every hook must survive that. Since this is a client only
   mod, the cheapest guard is a single early return when `Hud.instance` is null,
   and a plugin level flag set from `ZNet.instance.IsDedicated()`, VERIFIED as
   `public bool IsDedicated()`.

### 6.2 Methods that look like getters and are not

Do not call these from a hover reader. Each writes to a ZDO or mutates state.

1. `Fireplace.GetTimeSinceLastUpdate`, `Beehive.GetTimeSinceLastUpdate`, and
   `SapCollector.GetTimeSinceLastUpdate`. Each stores the current time back into
   the ZDO under `ZDOVars.s_lastTime`. Calling one from a hover reader corrupts
   the piece's own accounting and makes fuel or honey advance wrongly.
2. `Smelter.GetDeltaTime` and `CookingStation.GetDeltaTime`. Each stores the
   current time back under `ZDOVars.s_startTime`.
3. `Fireplace.UpdateState`, `Smelter.UpdateState`, `CookingStation.UpdateVisual`,
   and `SapCollector.UpdateEffects`. These drive visuals and effects.
4. `Container.StackAll`, `Container.TakeAll`, and every method that begins with
   `RPC_`.
5. `CraftingStation.GetLevel()`, `CraftingStation.GetExtentionCount(true)`, and
   `CraftingStation.GetStationBuildRange`. All three reach
   `CraftingStation.GetExtensions`, which every two seconds clears and rebuilds
   `m_attachedExtensions`, resets the station's own `m_updateExtensionTimer`,
   recomputes `m_buildRange`, and writes the radius of the live effect area
   collider and of the area marker circle. Vanilla never runs this from a hover.
   Pass `checkExtensions: false`, which only reads `m_attachedExtensions.Count`.

Read the ZDO values directly instead. The read only getters are safe:
`Smelter.GetFuel`, `Smelter.GetQueueSize`, `Smelter.GetQueuedOre`,
`Smelter.GetProcessedQueueSize`, `Smelter.GetBakeTimer`,
`CookingStation.GetSlot`, `CookingStation.GetFuel`, `Fermenter.GetStatus`,
`Fermenter.GetContent`, `Fermenter.GetFermentationTime`,
`Beehive.GetHoneyLevel`, `SapCollector.GetLevel`, `Plant.TimeSincePlanted`,
`Plant.GetGrowTime`, `WearNTear.GetHealthPercentage`, `WearNTear.GetSupport`,
`Tameable.GetRemainingTime`, and `Tameable.IsHungry`.

### 6.3 Patch conflicts with other hover mods

The hover surface is crowded. These are the methods that other hover mods
commonly patch, and where a conflict shows up.

1. `Hud.UpdateCrosshair(Player, float)`. Patched by nearly every hover mod. A
   mod that prefixes it and suppresses the original leaves OttoLens with no
   hover text at all. Nothing can be done about that from our side except to
   fail quietly.
2. `Container.GetHoverText()`. Patched by every chest contents mod. Two mods
   that both append produce a doubled item list. There is no protocol for
   detecting that. Keep our append recognizable and document the overlap.
3. `Switch.GetHoverText()`. Patched by smelter, oven, and armor stand mods. This
   is the highest risk method in the list, because a single postfix runs for
   every switch in the game. Ours must return in a few instructions when the
   switch is not one of ours.
4. `Fireplace.GetHoverText()`, `Smelter.OnHoverAddOre()`, and
   `Plant.GetHoverText()`. Patched by fuel and farming mods.
5. `ItemDrop.GetHoverText()`. Patched by item mods and by mods that show item
   quality.
6. `Player.UpdateHover` and `Player.FindHoverObject`. Patched by reach and
   interaction mods. A longer reach mod changes which object is hovered, which
   is fine for us.

Rules that reduce the damage.

- Append only. Never assign `__result` outright, except in the two documented
  cases where a vanilla figure moves onto an icon badge, and even there edit the
  existing string rather than rebuild it.
- Use a Harmony priority only where order really matters, and prefer a low
  priority so other mods' edits land first and ours reads the final text.
- Never patch a vanilla method for two different features in the same class.
  One patch, one job.
- Do not delete substrings by regular expression. An earlier implementation
  stripped parenthesized groups from smelter and shield generator strings with a
  pattern, which also ate any text another mod had put in parentheses. If a
  vanilla figure must be replaced, find it by computing the exact expected
  substring and replacing that one string.

### 6.4 Performance

The hover readers run once per frame while the player looks at something. That
is a hot path.

1. **Cache the container item list.** Key it on
   `ZDO.DataRevision` from the container's network view. Rebuild only when the
   revision changes. Without this, a player looking at a full chest re-groups
   and re-sorts a list of thirty items sixty times a second.
2. **Cache icon sprites.** Key the cache on the prefab name or the stable hash.
   The object database lookup and the component walk are far more expensive than
   the assignment.
3. **Cache built ZDO key hashes.** The smelter ore queue, the cooking station
   slots, and the armor stand slots all use keys built from a name and an index.
   Compute the string hash once per index and hold it in an array. A stable hash
   over a freshly concatenated string, sixty times a second, per slot, allocates
   for no reason.
4. **Do not rebuild the grid.** Elements are created once. A refresh assigns a
   sprite, a label, and an active flag. Never instantiate or destroy an element
   during a refresh.
5. **Reuse one string builder** per reader, cleared at entry. Do not build
   status text by repeated concatenation in a loop.
6. **Return before formatting.** Check the feature toggle, the null guards, and
   the empty text guard before touching a string builder or a dictionary.
7. **One raycast, at most.** The only extra raycast in the design is the
   optional tree lookup in section 3.12, and it runs only when nothing else is
   hovered and only when the tree setting is on.
8. **No allocation in the frame hook** on a frame where nothing is hovered. That
   is the common case, so it must cost a few comparisons.

### 6.5 Version drift notes for 1.0.7

These five changes broke the earlier implementation. Any reader written from
older documentation will hit them.

1. `Hoverable.GetHoverOffset()` returns `float`. A component that implements
   `Hoverable` must implement it.
2. The inventory grid element type is `InventoryElement`, and its grid position
   is the property `Position`, not a field named for an element.
3. `ZInput.pointerPosition` is VERIFIED as
   `public static Vector3 pointerPosition`. The older mouse position member is
   gone.
4. `ItemStand` stores its attached item as `m_visualHash`, an int, and the older
   name field does not exist. The armor stand slot type does the same. The armor
   stand also stores per slot item hashes, so any reader that looks for a per
   slot string finds nothing.
5. `CookingStation.GetSlot` takes a fourth out parameter, `out bool cheated`,
   and returns `void`.

One more, found during this verification and not in the earlier list:
`EnvMan.m_dayLengthSec` is a `long`. Cast it before using it as a divisor in a
float expression.
