# ArmorStand Type Analysis (Vanilla Valheim 1.0.7)

## Type
`ArmorStand : MonoBehaviour, IHasHoverMenuExtended`

## Key Members

### Fields
- `private ZNetView m_nview` (line 40)
- `public List<ArmorStandSlot> m_slots = new List<ArmorStandSlot>()` (line 42)

### Nested Class: ArmorStandSlot
- `public Switch m_switch` (line 11)
- `public VisSlot m_slot` (line 13)
- `public List<ItemDrop.ItemData.ItemType> m_supportedTypes = new List<ItemDrop.ItemData.ItemType>()` (line 15)
- `public ItemDrop.ItemData m_item` (line 18)
- `public int m_visualHash` (line 21)
- `public int m_visualVariant` (line 24)
- `public string m_currentItemName = ""` (line 27)

### Methods
- `private void SetPose(int index, bool effect = true)` (line 175)
- `public void RPC_SetPose(long sender, int index)` (line 189)
- `private bool UseItem(Switch caller, Humanoid user, ItemDrop.ItemData item)` (line 194)
- `public void DestroyAttachment(int index)` (line 262)
- `public void RPC_DestroyAttachment(long sender, int index)` (line 267)
- `private void RPC_DropItemByName(long sender, string name)` (line 277)
- `private void RPC_DropItem(long sender, int index)` (line 292)
- `private void DropItem(int index)` (line 300)
- `private void UpdateAttach()` (line 320)
- `private void RPC_RequestOwn(long sender)` (line 342)
- `private void InitKeys()` (line 350)
- `private void SetKeyHashes(ref int[] hashArray, string name)` (line 356)
- `private void UpdateVisual()` (line 368)
- `private void RPC_SetVisualItem(long sender, int index, int itemHash, int variant)` (line 381)
- `private void SetVisualItem(int index, int itemHash, int variant)` (line 386)
- `private void UpdateSupports()` (line 422)
- `private GameObject GetAttachPrefab(GameObject item)` (line 454)
- `private bool CanAttach(ArmorStandSlot slot, ItemDrop.ItemData item)` (line 469)
- `public bool HaveAttachment(int index)` (line 478)
- `public int GetAttachedItem(int index)` (line 487)
- `public int GetNrOfAttachedItems()` (line 496)
- `public bool TryGetItems(Player player, Switch switchRef, out List<string> items)` (line 509)
- `public bool CanUseItems(Player player, Switch switchRef, bool sendErrorMessage = true)` (line 522)

## ZDO Keys Used
- `ZDOVars.s_pose` - stores current pose index
- Dynamic keys: `{index}_item` - stores item prefab hash (generated via `(i + "_item").GetStableHashCode()`)
- Dynamic keys: `{index}_variant` - stores item variant (generated via `(i + "_variant").GetStableHashCode()`)

## Notes
- Item data is saved/loaded via `ItemDrop.SaveToZDO()` and `ItemDrop.LoadFromZDO()` (lines 309, 331)
- No GetHoverText() method exists; hover text is set via Switch callbacks
