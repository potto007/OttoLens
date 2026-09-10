# TreeLog Type Analysis

**Source:** Valheim 1.0.7 - assembly_valheim.dll  
**Decompilation Tool:** ilspycmd

## Type Declaration
```
public class TreeLog : MonoBehaviour, IDestructible, IProjectile
```

## Requested Members

### m_health
- **Signature:** `public float m_health = 60f;`
- **Line:** 7
- **Kind:** field
- **Status:** EXISTS

### m_minToolTier
- **Signature:** `public int m_minToolTier;`
- **Line:** 11
- **Kind:** field
- **Status:** EXISTS

### m_nview
- **Signature:** `private ZNetView m_nview;`
- **Line:** 46
- **Kind:** field
- **Status:** EXISTS

### ZDO Health Key
- **Key Name:** `ZDOVars.s_health`
- **Read Locations:** Lines 66, 160
- **Write Locations:** Lines 69, 187
- **Type Operations:** GetFloat, Set

## All Members

### Fields (Public)
- `public float m_health = 60f;` (line 7)
- `public HitData.DamageModifiers m_damages;` (line 9)
- `public int m_minToolTier;` (line 11)
- `public EffectList m_destroyedEffect = new EffectList();` (line 13)
- `public EffectList m_hitEffect = new EffectList();` (line 15)
- `public DropTable m_dropWhenDestroyed = new DropTable();` (line 17)
- `public GameObject m_subLogPrefab;` (line 19)
- `public Transform[] m_subLogPoints = Array.Empty<Transform>();` (line 21)
- `public bool m_useSubLogPointRotation;` (line 23)
- `public float m_spawnDistance = 2f;` (line 25)
- `public float m_hitNoise = 100f;` (line 27)
- `public GameObject m_snowHit;` (line 30)
- `public float m_snowHitDistance = 1f;` (line 32)
- `public List<GameObject> m_snowHitPositions;` (line 34)
- `public float m_snowMaxWeightMultiplier = 10f;` (line 36)
- `public float m_minSnow = 0.1f;` (line 38)
- `public float m_snowStartDot = 0.25f;` (line 40)
- `public float m_snowHeightBias = 0.3f;` (line 42)

### Fields (Private)
- `private Rigidbody m_body;` (line 44)
- `private ZNetView m_nview;` (line 46)
- `private bool m_firstFrame = true;` (line 48)
- `private Heightmap.Biome m_biome;` (line 50)
- `private Vector3 m_lastSnowPos;` (line 52)
- `private float m_baseWeight;` (line 54)
- `private float m_baseDrag;` (line 56)

### Methods
- `private void Awake()` (line 58)
- `private void UpdateSnow()` (line 92)
- `private void EnableDamage()` (line 136)
- `public DestructibleType GetDestructibleType()` (line 141)
- `public void Damage(HitData hit)` (line 146)
- `private void RPC_Damage(long sender, HitData hit)` (line 154)
- `public void Setup(Character owner, Vector3 velocity, float hitNoise, HitData hitData, ItemDrop.ItemData item, ItemDrop.ItemData ammo)` (line 218)
- `public string GetTooltipString(int itemQuality)` (line 226)
- `private void Destroy(HitData hitData, bool cheatedTool)` (line 231)

## ZDO Keys Used

| Key | Reads | Writes |
|-----|-------|--------|
| `ZDOVars.s_health` | Lines 66, 160 | Lines 69, 187 |

## Missing Members
None - all requested members exist.
