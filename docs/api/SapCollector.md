# SapCollector Type Analysis

Vanilla Valheim 1.0.7, decompiled from `assembly_valheim.dll`.

## Fields

| Member | Kind | Signature |
|--------|------|-----------|
| m_secPerUnit | field | public float m_secPerUnit = 10f; |
| m_maxLevel | field | public int m_maxLevel = 4; |
| m_nview | field | private ZNetView m_nview; |

## Methods

| Member | Kind | Signature |
|--------|------|-----------|
| GetLevel | method | private int GetLevel() |
| GetHoverText | method | public string GetHoverText() |

## Notable Observations

- **GetLevel()** reads `ZDOVars.s_level` as an int via `m_nview.GetZDO().GetInt(ZDOVars.s_level)` (line 179).
- **GetHoverText()** constructs hover display text including level display and status.
- **GetStatusText()** (closest to requested "GetStatus") returns localized status strings based on level, connection, and resource drain state (lines 102-117).

## ZDO Keys

The following ZDO keys are accessed by SapCollector methods:

| ZDO Key | Type | Operations |
|---------|------|-----------|
| ZDOVars.s_lastTime | long | GetLong (lines 56, 148), Set (lines 58, 151) |
| ZDOVars.s_level | int | GetInt (line 179), Set (lines 162, 170) |
| ZDOVars.s_product | float | GetFloat (line 202), Set (line 221) |

## Missing Members

- **GetStatus**: No method with this exact name exists. Closest alternative: `GetStatusText()` (private, returns string) at lines 102-117.
