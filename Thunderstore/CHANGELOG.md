# Changelog

All notable changes to OttoLens.

## Unreleased

- Windmills, kilns, spinning wheels and eitr refineries no longer call their
  input ore. The queue meter names the input (`Barley`, `Flax`), the empty
  headline follows it (`NO BARLEY`), and the running word is `MILLING` on a
  windmill and `WORKING` on the others. Smelters and blast furnaces are
  unchanged.

## v1.0.0

- First release, for Valheim 1.0.7 and BepInEx 5.4.2350.
- One ledger panel next to the vanilla hover text, with a title, one main meter,
  up to two smaller meters, and item rows with icon, name and count.
- Readers for chests, carts and ship holds; fires, torches, braziers and hearths;
  smelters, kilns and blast furnaces; cooking stations and ovens; fermenters,
  beehives and sap collectors; plants and pickables; build pieces; rocks, trees
  and logs; item stands and armor stands; tombstones; tamed creatures; wisp
  spawners; shield generators; feasts; ground items; and crafting stations.
- Read only. OttoLens never writes to the world and never changes burn rates,
  cook times or fuel limits.
- No patch on the hover text itself, so it runs alongside other hover mods.
- Client only. No ServerSync, nothing to install on a server.
- World-placed chests stay hidden until opened once. Off with
  `HideUnopenedWorldChests`.
- Build piece status shows with the hammer out by default. `BuildPieces` can be
  `Off`, `WithHammer` or `Always`.
- `H` toggles the panel in game. Change or disable it with `ToggleKey`.
- Lens dirt is off by default, because the Eye of Odin makes all things clear.
  `RemoveLensDirt` brings it back.
