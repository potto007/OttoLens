# Vagon Type Analysis (Valheim 1.0.7)

## Requested Members

### m_container
- **Declaration**: `public Container m_container;` (line 25)
- **Type**: Container
- **Access**: Public field

### m_nview
- **Declaration**: `private ZNetView m_nview;` (line 86)
- **Type**: ZNetView
- **Access**: Private field

### GetHoverText
- **Signature**: `public string GetHoverText()` (line 140)
- **Return Type**: string
- **Parameters**: None
- **Implementation**: Returns localized hover text with interaction prompt

## ZDO Keys Read/Written

The type reads and writes the following ZDO key:

- **ZDOVars.s_attachJointHash** 
  - Read at line 280 in `IsAttached()` method: `m_nview.GetZDO().GetBool(ZDOVars.s_attachJointHash)`
  - Written at line 328 in `AttachTo()` method: `m_nview.GetZDO().Set(ZDOVars.s_attachJointHash, value: true)`
  - Written at line 353 in `Detach()` method: `m_nview.GetZDO().Set(ZDOVars.s_attachJointHash, value: false)`

## Source

Decompiled from: `/mnt/c/Program Files (x86)/Steam/steamapps/common/Valheim/valheim_Data/Managed/assembly_valheim.dll`
