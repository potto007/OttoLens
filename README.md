![OttoLens - See with the Allfather's Eye](https://raw.githubusercontent.com/potto007/OttoLens/main/docs/images/ottolens-title.png)

# OttoLens

### For Valheim 1.0.7

Look at a chest and see what is in it. Look at a smelter and see how much ore is
queued, how much fuel is left, and how long until the next bar drops. Look at a
plant and see when it is ready. OttoLens puts a small ledger next to the vanilla
hover text with the numbers the game already has but does not show you.

**Maintainer:** Paul Otto

--------------------

### What it shows

Every target gets the same panel: a title, one main meter, up to two smaller
meters, and then item rows with the icon, the name and the count.

- **Chests, carts and ship holds.** Every item stack inside, sorted by count, with
  a slot meter and a `Free N` footer. Rows past the cap collapse to `and N more`.
- **Fires, torches, braziers and hearths.** Fuel left and how long it burns, in
  game days when it runs long.
- **Smelters, kilns and blast furnaces.** Fuel, the ore queue, and finished bars
  waiting to be picked up.
- **Cooking stations and ovens.** Each slot, what is on it, and the time to done
  or to burnt.
- **Fermenters, beehives and sap collectors.** Contents, time to ready, and the
  honey or sap level.
- **Plants and pickables.** Grow progress, and the item and yield on a live
  bush. Respawn timers on picked bushes are opt in (`ShowRespawn`).
- **Build pieces.** Health and support, with the hammer out by default so the
  panel stays out of the way during normal play.
- **Rocks, trees and logs.** Health as a fraction. Trees are off by default.
- **Item stands, armor stands, tombstones, tamed creatures, wisp spawners, shield
  generators, feasts, ground items and crafting stations.**

### What it does not do

OttoLens reads. It never writes to the world. It does not change burn rates,
cook times, fuel limits, or anything else about how the game plays. It does not
patch the hover text itself, so it sits alongside other hover mods instead of
fighting them. It is client only and needs nothing on the server.

A world-placed chest you have not opened yet shows its title and `Contents
unknown`. Open it once and the panel fills in. That is on by default because
the surprise is part of the game.

Lens dirt is off by default, because the Eye of Odin makes all things clear.
Bloom still glows. Only the smudges on the camera lens are gone. Set
`RemoveLensDirt` to `false` to bring them back.

--------------------

`A config file BepInEx/config/potto007.OttoLens.cfg is created after you run the game once with this mod.`

Change it in a text editor or in game with Configuration Manager. Layout knobs
apply on the next hover. `MaxRows` and `ShowNames` rebuild the panel at once.

| Section | Key | Default | What it does |
| --- | --- | --- | --- |
| OttoLens | `Enabled` | `true` | Master switch. Off hides the panel and skips every reader. |
| OttoLens | `ToggleKey` | `H` | Key that flips the master switch in game. `None` disables the key. |
| OttoLens | `OffsetX` | `150` | Left edge of the panel, pixels right of screen centre. 96 to 600. |
| OttoLens | `OffsetY` | `-32` | Top edge of the panel, pixels below screen centre. -400 to -16. |
| OttoLens | `PanelWidth` | `300` | Panel width in pixels. 240 to 420. |
| OttoLens | `GuiScale` | `1.0` | Scale of the whole panel. 0.75 to 1.6. Try 1.15 at 1440p. |
| OttoLens | `MaxRows` | `10` | Item rows per block before `and N more`. 1 to 16, and clamped to what fits on screen. |
| OttoLens | `SortRows` | `Count` | `Count` sorts by count then name. `Slot` keeps the container's own order. |
| OttoLens | `ShowNames` | `true` | Show the item name column. Off shows icon and count only. |
| OttoLens | `ShowFullHealth` | `false` | Show the Health row on a build piece at 100 percent. |
| OttoLens | `AvoidHoverText` | `true` | Push the panel right when the vanilla hover line would run under it. |
| OttoLens | `RefreshHz` | `4` | How often the values update while the panel is visible. 1 to 10. |
| OttoLens | `FadeSeconds` | `0.08` | Show and hide fade. 0 to 0.5. |
| OttoLens | `BackdropAlpha` | `0.88` | Opacity of the panel plate. 0.5 to 1.0. |
| OttoLens | `ShowDays` | `true` | Add a game day figure to fuel and grow times longer than half a day. |
| Camera | `RemoveLensDirt` | `true` | Remove the smudges that bloom draws over bright light. `false` brings them back. |
| Targets | `HideUnopenedWorldChests` | `true` | Hide the contents of world-placed chests until you have opened them once. |
| Targets | `Containers` | `true` | Chests, carts and ship holds. |
| Targets | `Fires` | `true` | Fireplaces, hearths, torches and braziers. |
| Targets | `Smelters` | `true` | Smelters, kilns, blast furnaces and windmill fed smelters. |
| Targets | `Cooking` | `true` | Cooking stations and ovens. |
| Targets | `Fermenting` | `true` | Fermenters, beehives and sap collectors. |
| Targets | `Plants` | `true` | Planted crops and saplings. |
| Targets | `Pickables` | `true` | Pickable plants and item piles. |
| Targets | `ShowRespawn` | `false` | Show the respawn countdown on an already picked pickable. Needs `Pickables` on. |
| Targets | `BuildPieces` | `WithHammer` | `Off`, `WithHammer` for place mode only, or `Always` for any hovered piece. |
| Targets | `Mineables` | `true` | Mineable rocks. |
| Targets | `TreesAndRocks` | `false` | Trees, stumps and logs. |
| Targets | `Stands` | `true` | Item stands and armor stands. |
| Targets | `Creatures` | `true` | Tamed creatures and pets. |
| Targets | `Misc` | `true` | Tombstones, wisp spawners, shield generators, feasts, ground items and crafting stations. |

___________________________
#### Installation (manual)

Extract the DLL from the zip into `<GameDirectory>\BepInEx\plugins`, then start the
game.
___________________________
#### Installation (automatic)

Use r2modman or the Thunderstore Mod Manager. Search for OttoLens and install.
___________________________

#### Servers

Client only. Nothing to install on the server, and players without it see the
game exactly as before.

### Version information

The release history lives in [CHANGELOG.md](Thunderstore/CHANGELOG.md), and it
renders on the Changelog tab of the Thunderstore package page.

## Credits

OttoLens is written and maintained by **Paul Otto**. It is a clean-room build.
I wrote every reader from the game's own behaviour in 1.0.7, and it depends on
nothing but BepInEx and Harmony. MIT licence.
