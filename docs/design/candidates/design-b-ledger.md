# OttoLens hover panel, design B: the vertical ledger

A narrow column to the right of the vanilla hover text. One entry per line.
Icon at the left, name in the middle, count right-aligned. The look comes from
alignment and typography, not from frames and boxes.

## 1. ASCII mockups

Chest with 11 item types, max rows 8.

```
                    +   <- crosshair, never covered
      Reinforced chest
      (24/28 slots)          |  REINFORCED CHEST
                             |  ------------------------
                             | [#] Wood                72
                             | [#] Fine wood           40
                             | [#] Iron                18
                             | [#] Iron nails         120
                             | [#] Deer hide           16
                             | [#] Carrot seeds         9
                             | [#] Thistle              5
                             | [#] Frostner             1
                             |  ------------------------
                             |  and 3 more
```

Smelter: fuel, ore queue, output ready. Status rows carry an inline meter.

```
                    +
      Smelter
      Fuel: 12/20                |  SMELTER
                                 |  ------------------------
                                 |  Coal          [####----]  12/20
                                 |  Smelt         [######--]  0:41
                                 |  ------------------------
                                 | [#] Copper ore           7
                                 | [#] Tin ore             3
                                 |  ------------------------
                                 | [#] Copper (ready)      2
```

The rule under the title, and the rules between the status block, the queue and
the output, are the only chrome. The three blocks read as one ledger because
every icon, name and count sits on the same three vertical guides.

## 2. uGUI hierarchy

All sizes are px at 1080p reference resolution. The panel parents to the Hud
canvas, which already carries a CanvasScaler. A config float `guiScale`
multiplies the root `localScale` for 1440p and above (default 1.0; 1.15 reads
well at 1440p).

```
OttoLensLedger                 RectTransform anchor (0.5,0.5) pivot (0,1)
                               anchoredPosition (150, -32)  size (268, auto)
  + CanvasGroup                (alpha fade, blocksRaycasts=false)
  + VerticalLayoutGroup        padding 10,10,8,8  spacing 2  childForceExpandW=true
  + ContentSizeFitter          vertical=PreferredSize
  + Image                      backdrop sprite, color Panel
  |
  +-- Title                    LayoutElement preferredHeight 22
  |     + TextMeshProUGUI      align Left, 17px, color TitleGold, caps via text
  |
  +-- TitleRule                LayoutElement preferredHeight 2
  |     + Image                sprite Rule, color RuleGold
  |
  +-- StatusBlock              VerticalLayoutGroup spacing 2 (hidden if empty)
  |     +-- StatusRow (pooled, N)   LayoutElement preferredHeight 20
  |           + HorizontalLayoutGroup spacing 6
  |           +-- Label        TMP 15px, color Body, LayoutElement minWidth 78
  |           +-- MeterFrame   LayoutElement preferredWidth 84, height 8
  |           |     + Image    color MeterTrack
  |           |     +-- Fill   RectTransform anchor (0,0)-(0,1) pivot (0,0.5)
  |           |           + Image  color MeterGood / MeterWarn / MeterBad
  |           +-- Value        TMP 15px, align Right, LayoutElement flexibleWidth 1
  |
  +-- BlockRule                Image, color RuleDim (one per block boundary)
  |
  +-- ItemBlock                VerticalLayoutGroup spacing 1
  |     +-- ItemRow (pooled, max 8 by config)   LayoutElement preferredHeight 26
  |           + HorizontalLayoutGroup spacing 8, childAlignment MiddleLeft
  |           +-- Icon         LayoutElement preferred 24x24
  |           |     + Image    ItemDrop.ItemData icon sprite, preserveAspect
  |           +-- Name         TMP 15px, align Left, overflow Ellipsis,
  |           |                LayoutElement flexibleWidth 1
  |           +-- Count        TMP 15px, align Right, LayoutElement minWidth 42
  |
  +-- OverflowRule             Image, color RuleDim
  +-- Overflow                 TMP 14px italic, color Dim, text "and N more"
```

Rows live in a pool. The panel builds rows once, then only sets `text`,
`sprite`, `color`, and `Fill.sizeDelta.x` when the hovered object changes or the
refresh tick fires. The refresh tick is a coroutine at 4 Hz, not `Update`.
`ContentSizeFitter` runs on the frame a rebuild happens, never per frame.

Placement clears the crosshair. The vanilla hover text sits centred at the
crosshair, so the column starts 150 px to its right and 32 px down. The panel
holds a fixed width of 268 px, so the left edge never travels back toward the
centre as rows get longer. Long names truncate with an ellipsis instead.

## 3. Colors, fonts, metrics

| Role | Hex | Alpha | Use |
| --- | --- | --- | --- |
| Panel | `#17110C` | 0.82 | backdrop behind the column |
| TitleGold | `#D9B45B` | 1.0 | title line |
| RuleGold | `#C9A227` | 0.85 | rule under the title |
| RuleDim | `#6B5A3E` | 0.55 | block separator rules |
| Body | `#E4D5B7` | 1.0 | item names, status labels |
| Count | `#F0E6CF` | 1.0 | right-aligned counts and values |
| Dim | `#9C8C6E` | 1.0 | overflow line, empty state |
| MeterTrack | `#2A211A` | 0.9 | meter background |
| MeterGood | `#6E8B3D` | 1.0 | muted green |
| MeterWarn | `#C9A227` | 1.0 | gold |
| MeterBad | `#A8443A` | 1.0 | muted red |

Font: the vanilla `Norse`/`AveriaSerifLibre` TMP asset taken from the hover text
label at runtime, so no font asset ships with the mod. Sizes: title 17, rows 15,
overflow 14. Icon 24x24. Row height 26, status row 20. Panel padding 10 left and
right, 8 top and bottom. Row spacing 1, status spacing 2. Meter 84x8 with a 1 px
inset fill.

## 4. Vanilla sprites and fallback

Found once at first panel build with
`Resources.FindObjectsOfTypeAll<Sprite>()`, cached in a static dictionary. Names
tried in order:

- Backdrop: `woodpanel_trophys`, then `woodpanel_settings`, then `darken_blob`.
  Applied as a sliced image with `color = Panel`.
- Rule: `panel_separator`, then `divider`.
- Meter track and fill: no sprite lookup. `Image.sprite = null` renders a plain
  quad, which tints cleanly and costs nothing.

Fallback for any miss: a plain `Image` with `sprite = null`, `color = Panel`,
`type = Simple`. The layout does not depend on any sprite, so a miss changes the
texture only, never the geometry. The rule falls back to a 2 px tinted quad,
which is what the sprite draws anyway.

If a single embedded PNG is ever added, spend it on the backdrop only: a 32x32
9-sliced parchment-edge tile. Nothing else in this design needs art.

## 5. What the meter encodes

One meter component, four bindings. Fill width is `clamp01(value) * 82`.

1. **Fuel.** Fill is `fuel / maxFuel`. Value text reads `12/20`. Color ramp on
   the same fraction: above 0.5 green, 0.15 to 0.5 gold, below 0.15 red. This
   covers smelters, kilns, fires and ovens.
2. **Time left.** Fill is `remaining / total`, so the bar drains to the left as
   the process finishes. Value text reads `m:ss`. Color is gold while the bar
   moves and flips to green at zero, where the value text becomes `Ready`. This
   covers smelt time, fermenters, beehives, cooking stations and plant growth.
3. **Health.** Fill is `health / maxHealth` for build pieces, trees, rocks and
   tamed pets. Value text reads a percent. The ramp is the fuel ramp, so a
   damaged wall and a low fire look alike on purpose.
4. **Support.** Fill is `support / maxSupport` for build pieces. The color ramp
   inverts: high support is green, low is red, and the value text carries the
   vanilla percent. A second status row named `Support` sits under `Health`, so
   both meters stack and compare at a glance.

Two rules keep the meters honest. A value with no maximum, such as a pickable
count or an unstacked item, renders no meter at all and right-aligns the value
into the same column. A value that is exactly full renders the bar full in
green, never empty.

## 6. Why this is not an icon row

A plain icon row under the hover text is a picture of a container. This is a
reading of one. The differences are structural.

- **The count is a number in a column, not a badge on a sprite.** Counts line
  up, so the player compares 120 against 9 by eye position, not by squinting at
  overlaid digits at 24 px.
- **Names are present.** An icon row makes the player recognise 11 sprites. The
  ledger lets them read `Iron nails` and stop.
- **Status and contents share one grammar.** Fuel, smelt time and the ore queue
  are all label plus value on the same guides, so a smelter and a chest are the
  same panel with different rows. An icon row has no place to put fuel.
- **Growth is vertical, away from the crosshair.** An icon row grows sideways
  and pushes toward or across the centre of the screen. The column grows down
  and to the right of a fixed left edge, so the aim point stays clear at any row
  count.
- **The chrome budget is near zero.** One backdrop, three rules, and text. It
  reads at 1440p because the type scale and the alignment carry it, not because
  a frame sprite got bigger.
