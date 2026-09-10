# Valheim 1.0.7 Player Type Analysis

Source: `/mnt/c/Program Files (x86)/Steam/steamapps/common/Valheim/valheim_Data/Managed/assembly_valheim.dll`
Decompiled using ilspycmd

## Requested Members

### Existing Members

| Member | Signature | Line |
|--------|-----------|------|
| m_localPlayer | `public static Player m_localPlayer = null;` | 158 |
| m_hovering | `private GameObject m_hovering;` | 416 |
| m_hoveringCreature | `private Character m_hoveringCreature;` | 418 |
| m_placementGhost | `private GameObject m_placementGhost;` | 378 |
| GetHoverObject | `public override GameObject GetHoverObject()` | 4406 |
| FindHoverObject | `private void FindHoverObject(out GameObject hover, out Character hoverCreature)` | 4270 |
| IsPieceAvailable | `public bool IsPieceAvailable(Piece piece)` | 1646 |
| InPlaceMode | `public override bool InPlaceMode()` | 3685 |

### Missing Members

| Requested | Closest Real Member | Notes |
|-----------|-------------------|-------|
| GetRightItem | N/A | Not defined in Player class; inherited from Humanoid base class |

## ZDOVars Keys Used

The Player type reads and writes the following ZDO variables via `ZDOVars.s_*` hashes:

- `s_dodgeinv` - dodge invincibility state
- `s_playerID` - player ID
- `s_playerName` - player name
- `s_stamina` - stamina value
- `s_wakeup` - wakeup state
- `s_weaponLoaded` - weapon loaded state
- `s_baseValue` - base value (unknown context)
- `s_adrenaline` - adrenaline value
- `s_eitr` - eitr (magic resource) value
- `s_cheated` - cheated flag
- `s_dead` - death state
- `s_crowned` - crowned mode
- `s_debugFly` - debug fly mode
- `s_pvp` - PvP mode flag
- `s_emoteID` - emote ID counter
- `s_emote` - emote name
- `s_emoteOneshot` - emote one-shot flag
- `s_inBed` - in bed state
- `s_stealth` - stealth value

Total ZDO keys: 19
