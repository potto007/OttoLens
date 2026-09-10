# Valheim 1.0.7 MonsterAI Type Analysis

## File
`/mnt/c/Program Files (x86)/Steam/steamapps/common/Valheim/valheim_Data/Managed/assembly_valheim.dll`

## Type Found
**Yes** - MonsterAI (954 lines decompiled)

## Requested Members

### Found Members
1. **m_consumeItems** (line 96)
   - Declaration: `public List<ItemDrop> m_consumeItems;`
   - Kind: field

2. **m_consumeSearchRange** (line 100)
   - Declaration: `public float m_consumeSearchRange = 5f;`
   - Kind: field

### Missing Members (Not Found in MonsterAI)

1. **IsAlerted**
   - Status: NOT IN MonsterAI (inherited from BaseAI)
   - Called throughout code but not defined here
   - Only override present: `protected override void SetAlerted(bool alert)` at line 919

2. **IsHungry** 
   - Status: NOT IN MonsterAI
   - Closest reference: `m_tamable.IsHungry()` at line 650
   - Hunger state managed through the m_tamable component (Tamable type)

## ZDO Keys Read/Written

### ZDOVars accessed by MonsterAI methods:
- `ZDOVars.s_despawnInDay` (lines 152, 760, 768)
- `ZDOVars.s_eventCreature` (lines 153, 776, 784)
- `ZDOVars.s_sleeping` (lines 154, 884, 907)

### ZDO Operations:
- **GetBool** calls: lines 152, 153, 154, 768, 784
- **Set** calls (writes): lines 760, 776, 884, 907

## Additional Methods of Interest

- `public void MakeTame()` (line 225) - Sets tamed state and clears targets
- `public void SetDespawnInDay(bool despawn)` (line 757) - Sets and syncs despawn flag
- `public bool DespawnInDay()` (line 763) - Gets cached despawn state
- `public void SetEventCreature(bool despawn)` (line 773) - Sets event creature flag
- `public bool IsEventCreature()` (line 779) - Gets cached event creature state
- `public override bool IsSleeping()` (line 914) - Returns m_sleeping state
- `protected override void SetAlerted(bool alert)` (line 919) - Overrides base, resets m_timeSinceSensedTargetCreature on alert

## Summary

MonsterAI manages three primary ZDO state flags (despawnInDay, eventCreature, sleeping) and has two key item consumption fields. Hunger checking is delegated to the m_tamable component. IsAlerted functionality is inherited from BaseAI base class.
