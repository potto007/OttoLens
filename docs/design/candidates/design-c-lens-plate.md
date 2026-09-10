# OttoLens hover panel - Design C: "the lens"

One dark glass plate. One gold ring on the left edge of the title row. One primary
meter per target. Item icons in a tight grid with count badges.

## 1. ASCII mockups (approximate, 1 char = 8 px)

Chest with 11 item types. Grid is 8 columns by 2 rows.

```
                          + crosshair (panel sits below-right, never over it)

  o-------------------------------------------------------------o
  | (75%)  Reinforced Chest                                      |
  |  RING  Space 11 / 24                                         |
  |------------------------------------------------------------- |
  | [Wood ] [Ston ] [Coal ] [Iron ] [Flin ] [Rese ] [Feat ] [Amb ]|
  |   x342    x180    x97     x12     x64      x8      x23    x2  |
  | [Honey] [Cara ] [Surt ]                                       |
  |   x40     x11     x3                                          |
  o-------------------------------------------------------------o
     ^ 3 px gold bar on the left edge, full panel height
```

With 19 item types the last tile becomes the overflow tile:

```
  | [Wood ] [Ston ] [Coal ] [Iron ] [Flin ] [Rese ] [Feat ] [Amb ]|
  | [Honey] [Cara ] [Surt ] [Bark ] [Resi ] [Leat ] [This ] [ +5 ]|
```

Smelter. Fuel ring is the primary meter. Ore queue and output are secondary lines.

```
  o-----------------------------------------------o
  | (48%)  Smelter                                 |
  |  RING  Coal 9 / 20                             |
  |        Ore 4 / 10   ~2m 10s per bar            |
  |        Ready: 3 Iron                           |
  |----------------------------------------------- |
  | [Coal ] [ Ore ] [Iron ]                        |
  |   x9      x4      x3                           |
  o-----------------------------------------------o
```

## 2. uGUI hierarchy (px at 1080p)

All sizes are in the reference resolution 1920x1080. The panel is a child of the
Hud canvas, so the existing CanvasScaler scales it to 1440p without extra work.

```
OttoLensPanel                RectTransform anchor/pivot (0,1) top-left of anchor box
  width 480 (preferred, driven by ContentSizeFitter horizontal Unconstrained -> fixed 480)
  height auto
  components: Image (plate), VerticalLayoutGroup, ContentSizeFitter (vertical PreferredSize),
              CanvasGroup (alpha fade), OttoLensPanelController
  padding 12,12,10,10   spacing 6
  |
  +-- GoldEdge            anchorMin (0,0) anchorMax (0,1), sizeDelta (3, 0), pivot (0,0.5)
  |     offset x = -3 so the bar sits outside the plate padding
  |     components: Image (gold), LayoutElement ignoreLayout = true
  |
  +-- HeaderRow           height 44, HorizontalLayoutGroup, spacing 10, child align MiddleLeft
  |     |
  |     +-- RingSlot      LayoutElement preferred 40 x 40
  |     |     +-- RingTrack   stretch, Image, sprite = ring PNG, color = ring track
  |     |     +-- RingFill    stretch, Image, sprite = ring PNG,
  |     |     |                 type Filled, fillMethod Radial360, fillOrigin Top,
  |     |     |                 clockwise true, fillAmount 0..1
  |     |     +-- RingLabel   stretch, TextMeshProUGUI, center, size 13, "75%"
  |     |
  |     +-- TitleBlock   LayoutElement flexibleWidth 1, VerticalLayoutGroup, spacing 1
  |           +-- TitleText   TextMeshProUGUI, size 22, single line, overflow Ellipsis
  |           +-- PrimaryLine TextMeshProUGUI, size 16
  |
  +-- SecondaryLines      VerticalLayoutGroup, spacing 2, ContentSizeFitter vertical
  |     +-- Line1 .. LineN  TextMeshProUGUI, size 15  (pooled, max 4, SetActive toggled)
  |
  +-- Separator           LayoutElement preferred height 1, Image (separator color)
  |
  +-- IconGrid            GridLayoutGroup, cellSize 46x46, spacing 4x14,
  |                       constraint FixedColumnCount = maxColumns (default 8),
  |                       ContentSizeFitter vertical PreferredSize
  |     +-- Tile x (rows*cols)   pooled, never destroyed
  |           +-- Slot        stretch, Image, slot background sprite, color = slot
  |           +-- Icon        stretch with 3 px inset, Image, sprite = ItemData icon,
  |           |                 preserveAspect true, raycastTarget false
  |           +-- Badge       anchor bottom-right (1,0), pivot (1,0), size 30x16,
  |                 offset (2,-9) so it hangs under the tile in the 14 px row gap
  |                 +-- BadgeBg   stretch, Image, color = badge bg
  |                 +-- BadgeText stretch, TextMeshProUGUI, size 13, right align, "x342"
```

Layout notes.

1. Nothing rebuilds per frame. The controller rebuilds only when the hovered
   ZDO id changes, or when a tracked value crosses its display step.
2. Meters refresh on a 0.2 s timer and write `fillAmount` plus one string. No
   `LayoutRebuilder` call on that path.
3. Tiles are pooled at `maxRows * maxColumns` and toggled with `SetActive`.
4. The panel anchors to the hover text root, offset (0, -18) below it, and is
   pushed 24 px right of screen centre. The crosshair box stays clear.

## 3. Colors, sizes, spacing

| Role | Hex | Use |
| --- | --- | --- |
| plate | `#17120Ecc` | panel background, alpha 0.80 |
| plateEdge | `#0B0806aa` | 1 px inner outline via a second Image |
| gold | `#C9A54Aff` | left edge bar, ring fill for neutral meters |
| goldDim | `#6B5426ff` | ring track |
| parchment | `#E8DCBE` | title text |
| parchmentDim | `#B8AC90` | secondary lines, badge text |
| statusGood | `#7FA35A` | growth done, health full, output ready |
| statusWarn | `#D2A03C` | fuel low, time short, support marginal |
| statusBad | `#B8442E` | empty, dead plant, support critical |
| slot | `#2B211Acc` | icon tile background |
| badgeBg | `#100C08d9` | count badge plate |
| separator | `#5A4A32aa` | 1 px rule above the grid |

Sizes: title 22, primary line 16, secondary line 15, ring label 13, badge 13.
Icon tile 46x46, icon inset 3 px, badge 30x16 at the bottom-right corner.
Panel padding 12 left/right, 10 top/bottom. Row spacing 6. Grid gap 4 x 14.
Font: reuse the `TMP_FontAsset` from the vanilla hover text component. Do not
load a font. Copy `font`, `fontMaterial`, and enable outline width 0.12 on the
title only.

At 1440p the CanvasScaler does the work. Minimum legible check: badge text at
13 px scales to about 17 px at 1440p, which is above the vanilla item count size.

## 4. Sprites

Search once at startup with `Resources.FindObjectsOfTypeAll<Sprite>()` and cache
the first match by exact name. Candidate names, in priority order per slot:

1. Plate: `woodpanel_trophys`, then `panel_bkg`, then `darken_blob`. Use
   `Image.type = Sliced` when the sprite has a border, otherwise `Simple`.
2. Icon tile: `item_background`, then `inventory_slot`, then `button_fitted`.
3. Separator: `panel_separator`, then `line_bkg`.

Fallback for every slot: a plain `Image` with no sprite, `color` set to the role
color above. That is a flat semi-transparent dark rectangle and it still reads.
The panel must never throw when a name is missing.

Embedded PNG, one file only: `ottolens_ring.png`, 128x128, RGBA, premultiplied
off. It draws a single white anti-aliased annulus. Outer radius 62 px, inner
radius 48 px, so the stroke is 14 px, about 11 percent of the width. Fully
transparent elsewhere. White only, because the tint comes from `Image.color`.
Loaded once with `Texture2D.LoadImage`, then `Sprite.Create` with the full rect
and pivot (0.5, 0.5). Both `RingTrack` and `RingFill` use this one sprite.

## 5. What the ring encodes

The ring shows exactly one number per target. It is always a fraction from 0 to 1.
The ring label shows the percent. The primary line names the raw values.

| Target | Ring fraction | Ring color | Primary line |
| --- | --- | --- | --- |
| Fire, smelter, kiln | fuel / fuel max | good above 0.5, warn 0.15 to 0.5, bad below 0.15 | `Coal 9 / 20` |
| Fermenter, oven, cooking station | elapsed / total | gold while busy, good when done | `Ready in 3m 12s` |
| Plant, sap collector | growth progress | good, or bad when the plant is unhealthy | `Grown in 1m 40s` |
| Container, cart, ship cargo | used slots / total slots | good below 0.9, warn above 0.9 | `Space 11 / 24` |
| Build piece | health / max health | health color scale | `Health 87%` |
| Tree, rock, pickable | health / max health, or ready state | good when ready | `Ready` |
| Tamed pet | tameness, or hunger when tame | good, warn, bad | `Happy, 2 stars` |

Secondary lines, at most four, in this fixed order. Each line is one topic.

1. Rate or queue. Example: `Ore 4 / 10` or `~2m 10s per bar`.
2. Output. Example: `Ready: 3 Iron`. Colored `statusGood` when non zero.
3. Support, for build pieces only. A 10 cell bar drawn in text with the block
   characters, colored on the same good, warn, bad scale, plus the percent.
   Support gets its own line because it is a second independent meter and the
   design allows only one ring.
4. Ownership or state. Example: `Locked`, `Owner: Paul`, `No fuel`.

Time left is always absolute and short form: `2m 10s`, `45s`, `1h 04m`. Never a
bare percent for time, because a percent of an unknown total tells nothing.

## 6. Why this is not an icon row under the hover text

1. The eye lands on the ring first, not on the text. A plain row makes the
   player read numbers to learn the state. The ring answers "is it fine" before
   any reading starts.
2. The plate and the gold edge give the mod one silhouette. The player learns
   the shape once, and every target type then looks like the same instrument.
3. A row of icons under raw hover text inherits the hover text alignment and
   drifts as the name length changes. A fixed 480 px plate with a fixed grid
   does not move between targets, so the icons stay in known screen positions.
4. Counts sit in badges that hang into the row gap. That keeps the icon art
   unobstructed, which a count drawn over the icon corner does not.
5. One ring per target is a hard rule, not a preference. It forces a ranking of
   what matters per target type, so the panel never shows three competing meters.
