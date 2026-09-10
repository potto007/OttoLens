# Piece Type - Valheim 1.0.7 (assembly_valheim.dll)

## Members

### m_name
**Declaration:** `public string m_name = "";`
**Line:** 121
**Type:** Field (string)

### m_category
**Declaration:** `public PieceCategory m_category;`
**Line:** 127
**Type:** Field (PieceCategory enum)

### m_nview
**Declaration:** `private ZNetView m_nview;`
**Line:** 231
**Type:** Field (ZNetView)

### m_creator
**Declaration:** `private long m_creator;`
**Line:** 233
**Type:** Field (long)

### m_description
**Declaration:** `public string m_description = "";`
**Line:** 123
**Type:** Field (string)

### m_icon
**Declaration:** `public Sprite m_icon;`
**Line:** 119
**Type:** Field (Sprite)

### GetCreator
**Declaration:** `public long GetCreator()`
**Lines:** 440-443
**Type:** Method
**Signature:** `public long GetCreator()`
**Returns:** `long`

### IsCreator
**Declaration:** `public bool IsCreator()`
**Lines:** 450-455
**Type:** Method
**Signature:** `public bool IsCreator()`
**Returns:** `bool`

## ZDO Variable Access

The following ZDOVars keys are read/written by the Piece type:

- `ZDOVars.s_creator` - Read in Awake() (line 262), written in SetCreator() (line 434)
- `ZDOVars.s_creatorIndex` - Read in Awake() (line 263), written in SetCreator() (line 436)
- `ZDOVars.s_cheated` - Read in DropResources() (lines 381, 394)

## Notes

All requested members exist in the decompiled type. No missing members reported.
