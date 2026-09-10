# Design A: compact rune-framed card

One card hangs below the vanilla hover text. It never overlaps the crosshair.
The card has a thin carved border, a title bar, an icon grid, and a status strip.

## 1. ASCII mockups

Chest, 11 item types, grid 8 x 2 (default caps), 1080p:

```
                        + <- crosshair (untouched)
                     Reinforced chest
                     Use  [E]                     <- vanilla hover text
    ,--<>--------------------------------------<>--,
    |  REINFORCED CHEST                    11 / 24 |  title 13px, slots gold
    |~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~|  rune divider, 2px
    | [wood][ston][coal][iron][flnt][leth][thst]   |  row 1, 30px icons
    |   64    41    88    12    23     7    150    |  badge, icon corner
    | [reso][gret][feth][amby]                     |  row 2
    |    3    19     6     2                       |
    |______________________________________________|
    |  Free 13                                     |  footer 11px, dim
    `----------------------------------------------'
```

Overflow example: with `maxColumns = 5`, `maxRows = 2`, the last tile becomes a
`+2` tile. The tile uses the badge color on the plain slot background.

Smelter, fuel bar plus ore queue plus output ready:

```
    ,--<>--------------------------------<>--,
    |  SMELTER                    SMELTING  |  state word, amber
    |~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~|
    |  Coal  ####################----  17/20|  fuel bar, amber
    |  Ore   ##########--------------   6/10|  queue bar, parchment
    |  Next  #####-------------------   0:22|  progress bar, gold
    |_______________________________________|
    | [copr]  ->  [copb]   READY x3         |  input icon, output icon
    |    6           3                      |
    `---------------------------------------'
```

## 2. uGUI hierarchy

All sizes are pixels at 1080p. The root scales for 1440p (see section 3).

```
OttoLensCard                     (child of Hud.m_crosshair parent canvas)
  RectTransform: anchor+pivot (0.5, 1) top-center of the hover text block,
                 anchoredPosition (0, -8), width 316, height auto
  ContentSizeFitter: vertical = PreferredSize
  VerticalLayoutGroup: padding 10/10/8/8, spacing 6, childForceExpandHeight off
  Image: frame sprite, type Sliced, color Frame Tint
  CanvasGroup: alpha driven on show and hide, blocksRaycasts off, interactable off
  |
  +- Title                       LayoutElement preferredHeight 18
  |    HorizontalLayoutGroup: spacing 6, childForceExpandWidth true
  |    +- Name    TextMeshProUGUI 13px, left, Parchment, uppercase, ellipsis
  |    +- Count   TextMeshProUGUI 13px, right, Gold, e.g. "11 / 24"
  |
  +- Divider                     LayoutElement preferredHeight 2
  |    Image: divider sprite, type Sliced, color Border
  |
  +- Grid                        LayoutElement preferredHeight = rows*34 + (rows-1)*3
  |    GridLayoutGroup: cellSize 34x34, spacing 3x3, constraint FixedColumnCount
  |    (column count = config maxColumns, default 8)
  |    +- Slot_0 .. Slot_N       Image: slot sprite, color Slot BG
  |         +- Icon              RectTransform stretch, offsets 2, Image sprite =
  |         |                    ItemDrop.ItemData.GetIcon(), preserveAspect true
  |         +- Badge            anchor+pivot (1,0), size 18x13, offset (-1, 1)
  |              Image: badge sprite, type Sliced, color Badge BG
  |              +- BadgeText   TMP 11px, center, Parchment, "64" or "+2"
  |
  +- StatusStrip                 VerticalLayoutGroup spacing 3 (hidden if no rows)
  |    +- StatRow_0 .. StatRow_2  LayoutElement preferredHeight 12
  |         HorizontalLayoutGroup: spacing 5
  |         +- Label   TMP 11px, width 42, Parchment Dim
  |         +- BarBG   Image slot sprite, color Bar BG, flexibleWidth 1, height 6
  |         |    +- BarFill  Image, type Filled, Horizontal, fillAmount 0..1
  |         +- Value   TMP 11px, width 46, right, Parchment
  |
  +- Footer                      LayoutElement preferredHeight 14, optional
       TextMeshProUGUI 11px, left, Parchment Dim
```

Cost control. The card builds its slot pool once (maxRows * maxColumns objects)
and reuses it. A refresh sets sprite, text, fillAmount, and `SetActive`. The
refresh runs on hover target change, and otherwise at most 4 times per second.
No `Update` work runs when the card is hidden.

## 3. Colors, sizes, spacing

| Role | Hex |
| --- | --- |
| Card fill | `#1B1512` at alpha 0.85 |
| Frame tint | `#6E5A3C` |
| Border and divider | `#8A7248` |
| Parchment text | `#E8DCC0` |
| Parchment dim | `#B8A882` |
| Gold accent | `#D9A742` |
| Amber fuel | `#C87A28` |
| Muted green (good, ready) | `#6E9B4C` |
| Muted red (low, damaged) | `#A83A2E` |
| Slot background | `#0F0C0A` at alpha 0.60 |
| Badge background | `#0D0B09` at alpha 0.85 |
| Bar background | `#0F0C0A` at alpha 0.75 |

Font: the same TMP asset as the vanilla hover text. Title 13px, badge 11px,
status and footer 11px. Icon cell 34px, icon 30px, grid gap 3px. Badge 18x13px
at the bottom right corner of the cell, inset 1px. Card padding 10px on the
sides, 8px top and bottom. Section spacing 6px.

Resolution. The card is a child of the vanilla HUD canvas, so the vanilla
CanvasScaler handles 1440p. A config value `uiScale` (default 1.0) multiplies
cell size, font sizes, and the card width for users who want a larger card.

## 4. Sprite names

Find sprites once at startup with `Resources.FindObjectsOfTypeAll<Sprite>()`.
Build a name to sprite map. Then take the first name that exists from each list.

1. Frame: `woodpanel_trophys`, `woodpanel_settings`, `woodpanel_password`,
   `panel_bkg_128`, `InventoryScreen_bkg`.
2. Divider: `panel_separator`, `hr`, `line`.
3. Slot and bar background: `item_background`, `inventory_slot`, `bkg`.
4. Badge: `item_background`, `bkg`, `darken_blob`.
5. Bar fill: `white`, `UISprite`, or `Sprite.Create` on a 4x4 white texture.

Fallback. If a list gives no hit, use a plain `Image` with no sprite, `type
Simple`, and the role color from section 3. The card then reads as a flat dark
panel with a 1px border child. The border child is a stretched `Image` with the
Border color and a 1px inset, so the carved look degrades and does not vanish.
No asset bundle is needed. No embedded PNG is needed.

## 5. Status strip encoding

Each status row is a label, a thin 6px bar, and a value. The bar carries the
shape. The value carries the exact number. Rows appear only when the target
supplies the data.

1. Fuel (fire, smelter, kiln, oven, sap collector). Fill is
   `fuel / maxFuel`. Color is Amber above 25 percent, Muted red at or below
   25 percent. Value is `17/20`.
2. Time left (fermenter, cooking station, oven, plant grow time, smelt step).
   Fill is `remaining / total` and it drains. Color is Gold. Value is `m:ss`.
   A finished item shows a full Muted green bar and the word `READY`.
3. Health (build pieces, trees, rocks, tamed pets). Fill is `hp / maxHp`.
   Color lerps Muted green to Muted red across the range. Value is a percent.
   Full health hides the row unless the config asks for it.
4. Support (build pieces). Fill is `support / maxSupport`. Color follows the
   vanilla support ramp: Muted green, Gold, Amber, Muted red. Value is a
   percent. The row sits directly under Health, so the two bars read as a pair.
5. Content level (chest fill, cart weight, ship cargo). Fill is
   `used / capacity`. Color is Parchment dim below 90 percent, Amber above.

Tamed pets add a one line footer for tame progress and star level. Tombstones
and item stands use the grid only, with no status strip.

## 6. Why this is not a plain icon row

A plain row under the hover text gives every target the same shape. The reader
must read numbers to learn the state. This card gives each target a silhouette.

1. The frame separates mod output from vanilla text. The player never has to
   work out where the vanilla line stops.
2. The bars are pre-attentive. A short amber stub means low fuel at a glance,
   with no reading. A row of numbers does not do that.
3. The grid plus corner badges hold more items in less width than icons with
   trailing counts on one line. Eleven types fit in 316px.
4. The title bar carries the summary (`11 / 24`, `SMELTING`). The player who
   only wants the headline reads one line and moves on.
5. One card shape serves all target types. Only the rows change, so the player
   learns the layout once.
