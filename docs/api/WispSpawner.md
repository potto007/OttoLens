# WispSpawner - Vanilla Valheim 1.0.7 API

**Source**: `/mnt/c/Program Files (x86)/Steam/steamapps/common/Valheim/valheim_Data/Managed/assembly_valheim.dll`  
**Type**: `public class WispSpawner : MonoBehaviour, Hoverable`

## Requested Members

### m_maxSpawned
- **Declaration** (line 32): `public int m_maxSpawned = 3;`
- **Kind**: field

### m_spawnInterval
- **Declaration** (line 25): `public float m_spawnInterval = 5f;`
- **Kind**: field

### m_nview
- **Declaration** (line 55): `private ZNetView m_nview;`
- **Kind**: field

### GetHoverText
- **Declaration** (line 76): `public string GetHoverText()`
- **Kind**: method
- **Signature**: `public string GetHoverText()`

## ZDO Variables

The type reads and writes the following ZDO keys:

| Key | Usage | Line(s) |
|-----|-------|---------|
| `ZDOVars.s_lastSpawn` | GetLong (read) | 130 |
| `ZDOVars.s_lastSpawn` | Set (write) | 135 |

## Complete Member List

| Member | Kind | Declaration Line |
|--------|------|------------------|
| Status | enum | 7-13 |
| m_name | field | 15: `public string m_name = "$pieces_wisplure";` |
| m_noSpaceString | field | 17: `public string m_noSpaceString = "$piece_wisplure_nospace";` |
| m_fullString | field | 19: `public string m_fullString = "$piece_wisplure_full";` |
| m_tooMuchLightString | field | 21: `public string m_tooMuchLightString = "$piece_wisplure_light";` |
| m_wispsAreComingString | field | 23: `public string m_wispsAreComingString = "$piece_wisplure_ok";` |
| m_spawnInterval | field | 25: `public float m_spawnInterval = 5f;` |
| m_hoverOffset | field | 27: `public float m_hoverOffset;` |
| m_spawnChance | field | 30: `public float m_spawnChance = 0.5f;` |
| m_maxSpawned | field | 32: `public int m_maxSpawned = 3;` |
| m_onlySpawnAtNight | field | 34: `public bool m_onlySpawnAtNight = true;` |
| m_dontSpawnInCover | field | 36: `public bool m_dontSpawnInCover = true;` |
| m_maxCover | field | 39: `public float m_maxCover = 0.6f;` |
| m_wispPrefab | field | 41: `public GameObject m_wispPrefab;` |
| m_wispsNearbyObject | field | 43: `public GameObject m_wispsNearbyObject;` |
| m_nearbyTreshold | field | 45: `public float m_nearbyTreshold = 5f;` |
| m_spawnPoint | field | 47: `public Transform m_spawnPoint;` |
| m_coverPoint | field | 49: `public Transform m_coverPoint;` |
| m_spawnDistance | field | 51: `public float m_spawnDistance = 20f;` |
| m_maxSpawnedArea | field | 53: `public float m_maxSpawnedArea = 10f;` |
| m_nview | field | 55: `private ZNetView m_nview;` |
| m_status | field | 57: `private Status m_status = Status.Ok;` |
| m_lastStatusUpdate | field | 59: `private float m_lastStatusUpdate = -1000f;` |
| s_spawners | field | 61: `private static readonly List<WispSpawner> s_spawners = new List<WispSpawner>();` |
| Start | method | 63: `private void Start()` |
| OnDestroy | method | 71: `private void OnDestroy()` |
| GetHoverText | method | 76: `public string GetHoverText()` |
| GetHoverName | method | 88: `public string GetHoverName()` |
| UpdateDemister | method | 93: `private void UpdateDemister()` |
| GetStatus | method | 102: `private Status GetStatus()` |
| TrySpawn | method | 125: `private void TrySpawn()` |
| HaveFreeSpace | method | 140: `private bool HaveFreeSpace()` |
| OnDrawGizmos | method | 150: `private void OnDrawGizmos()` |
| GetBestSpawner | method | 154: `public static WispSpawner GetBestSpawner(Vector3 p, float maxRange)` |
| GetHoverOffset | method | 174: `public float GetHoverOffset()` |

## Missing Members

None. All requested members exist:
- m_maxSpawned: found
- m_spawnInterval: found
- m_nview: found
- GetHoverText: found
