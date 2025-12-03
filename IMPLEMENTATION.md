# PixelGame - Technical Implementation Guide

This document provides technical implementation details for building the core systems of PixelGame.

## Table of Contents
1. [Project Setup](#project-setup)
2. [Core Architecture](#core-architecture)
3. [Player System](#player-system)
4. [Combat System](#combat-system)
5. [Enemy System](#enemy-system)
6. [Dungeon Generation](#dungeon-generation)
7. [Progression Systems](#progression-systems)
8. [Item & Equipment](#item--equipment)
9. [UI System](#ui-system)
10. [Data Management](#data-management)
11. [Performance Optimization](#performance-optimization)

---

## Project Setup

### Unity Configuration
```
Unity Version: 2021.3 LTS or newer
Scripting Runtime: .NET Standard 2.1
API Compatibility: .NET 4.x
Color Space: Linear (for better visuals)
```

### Required Packages
- **Input System** (new Unity Input System)
- **2D Animation** (sprite animation)
- **Cinemachine** (camera control)
- **TextMeshPro** (better text rendering)
- **2D Sprite** (sprite handling)

### Project Settings
- **Physics2D Settings**:
  - Disable "Queries Hit Triggers" for better control
  - Set up collision matrix for layers
- **Tags & Layers**:
  - Player
  - Enemy
  - Projectile
  - PlayerProjectile
  - EnemyProjectile
  - Wall
  - Pickup

---

## Core Architecture

### Game Manager Pattern
```csharp
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { MainMenu, Playing, Paused, GameOver, Victory }
    public GameState CurrentState { get; private set; }

    // References to managers
    public PlayerController Player { get; private set; }
    public FloorManager FloorManager { get; private set; }
    public UIManager UIManager { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }

    public void ChangeState(GameState newState) { /* ... */ }
}
```

### Event System
Use C# events for loose coupling:
```csharp
public static class GameEvents
{
    // Player events
    public static event Action<float> OnPlayerHealthChanged;
    public static event Action OnPlayerDeath;
    public static event Action<int> OnPlayerLevelUp;

    // Combat events
    public static event Action<Enemy> OnEnemyKilled;
    public static event Action<int> OnDamageDealt;

    // Room events
    public static event Action OnRoomCleared;
    public static event Action<Room> OnRoomEntered;

    // Methods to invoke events
    public static void PlayerHealthChanged(float newHealth)
        => OnPlayerHealthChanged?.Invoke(newHealth);
}
```

---

## Player System

### Player Controller
```csharp
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Combat")]
    [SerializeField] private Transform weaponPivot;
    private WeaponBase currentWeapon;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 aimDirection;

    private void Update()
    {
        HandleInput();
        HandleAiming();
        HandleAttack();
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        rb.velocity = moveInput.normalized * moveSpeed;
    }

    private void HandleAiming()
    {
        // Aim toward mouse or right stick
        Vector2 aimPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        aimDirection = (aimPos - (Vector2)transform.position).normalized;

        // Rotate weapon pivot
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        weaponPivot.rotation = Quaternion.Euler(0, 0, angle);
    }
}
```

### Player Stats System
```csharp
[System.Serializable]
public class PlayerStats
{
    // Base stats
    public float MaxHealth = 100f;
    public float CurrentHealth;
    public float MoveSpeed = 5f;

    // Combat stats
    public float Damage = 10f;
    public float AttackSpeed = 1f;
    public float CritChance = 0.05f;
    public float CritMultiplier = 2f;

    // Defense stats
    public float Armor = 0f;
    public float DodgeChance = 0f;

    // Resource stats
    public int CurrentXP = 0;
    public int XPToNextLevel = 100;
    public int CurrentLevel = 1;

    // Modifiers (from equipment/traits)
    public Dictionary<StatType, float> StatModifiers;

    public void ApplyModifier(StatType type, float value) { /* ... */ }
    public float GetModifiedStat(StatType type) { /* ... */ }
}

public enum StatType
{
    MaxHealth, Damage, AttackSpeed, MoveSpeed,
    CritChance, CritMultiplier, Armor, DodgeChance
}
```

---

## Combat System

### Weapon Base Class
```csharp
public abstract class WeaponBase : MonoBehaviour
{
    [Header("Weapon Stats")]
    public string weaponName;
    public WeaponType weaponType;
    public float baseDamage = 10f;
    public float attackSpeed = 1f;
    public float range = 5f;

    [Header("Projectile (if ranged)")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 10f;

    protected float lastAttackTime;
    protected PlayerStats playerStats;

    public abstract void Attack(Vector2 direction);

    protected bool CanAttack()
    {
        return Time.time >= lastAttackTime + (1f / attackSpeed);
    }

    protected float CalculateDamage()
    {
        float damage = baseDamage * playerStats.GetModifiedStat(StatType.Damage);

        // Critical hit calculation
        if (Random.value <= playerStats.CritChance)
        {
            damage *= playerStats.CritMultiplier;
        }

        return damage;
    }
}

public enum WeaponType { Melee, Ranged, Magic }
```

### Melee Weapon Example
```csharp
public class MeleeWeapon : WeaponBase
{
    [Header("Melee Specific")]
    public float swingArc = 90f;
    public LayerMask enemyLayer;

    public override void Attack(Vector2 direction)
    {
        if (!CanAttack()) return;

        lastAttackTime = Time.time;

        // Swing animation
        PlaySwingAnimation();

        // Hit detection
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position + (Vector3)direction * range,
            1f,
            enemyLayer
        );

        foreach (var hit in hits)
        {
            if (IsInSwingArc(hit.transform.position, direction))
            {
                hit.GetComponent<IDamageable>()?.TakeDamage(CalculateDamage());
            }
        }
    }

    private bool IsInSwingArc(Vector2 targetPos, Vector2 attackDir)
    {
        Vector2 toTarget = (targetPos - (Vector2)transform.position).normalized;
        float angle = Vector2.Angle(attackDir, toTarget);
        return angle <= swingArc / 2f;
    }
}
```

### Projectile System
```csharp
public class Projectile : MonoBehaviour
{
    public float damage;
    public float speed;
    public float lifetime = 5f;
    public bool isPiercing = false;
    public int maxPierceCount = 1;

    private Rigidbody2D rb;
    private int pierceCount = 0;
    private HashSet<Collider2D> hitEnemies = new HashSet<Collider2D>();

    public void Initialize(float dmg, Vector2 direction)
    {
        damage = dmg;
        rb.velocity = direction.normalized * speed;
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy") && !hitEnemies.Contains(collision))
        {
            collision.GetComponent<IDamageable>()?.TakeDamage(damage);
            hitEnemies.Add(collision);

            if (isPiercing && pierceCount < maxPierceCount)
            {
                pierceCount++;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        else if (collision.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}
```

### Damage Interface
```csharp
public interface IDamageable
{
    void TakeDamage(float damage);
    void Die();
}
```

---

## Enemy System

### Enemy Base Class
```csharp
public abstract class EnemyBase : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    public float maxHealth = 50f;
    public float currentHealth;
    public float moveSpeed = 3f;
    public float damage = 10f;
    public int xpValue = 10;

    [Header("Drops")]
    public LootTable lootTable;

    protected Transform player;
    protected Rigidbody2D rb;
    protected EnemyState currentState;

    protected enum EnemyState { Idle, Chasing, Attacking, Dead }

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        player = GameManager.Instance.Player.transform;
    }

    protected abstract void UpdateAI();

    public virtual void TakeDamage(float damage)
    {
        currentHealth -= damage;
        OnDamaged();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public virtual void Die()
    {
        // Drop loot
        lootTable?.RollLoot(transform.position);

        // Award XP
        GameManager.Instance.Player.GetComponent<PlayerStats>()
            .AddXP(xpValue);

        // Trigger events
        GameEvents.EnemyKilled(this);

        Destroy(gameObject);
    }

    protected virtual void OnDamaged()
    {
        // Visual feedback (flash, knockback)
    }
}
```

### Example Enemy Types
```csharp
// Melee Chaser
public class MeleeEnemy : EnemyBase
{
    public float attackRange = 1.5f;
    public float attackCooldown = 1f;
    private float lastAttackTime;

    protected override void UpdateAI()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            currentState = EnemyState.Attacking;
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                AttackPlayer();
                lastAttackTime = Time.time;
            }
        }
        else
        {
            currentState = EnemyState.Chasing;
            ChasePlayer();
        }
    }

    private void ChasePlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = direction * moveSpeed;
    }

    private void AttackPlayer()
    {
        player.GetComponent<IDamageable>()?.TakeDamage(damage);
    }
}

// Ranged Enemy
public class RangedEnemy : EnemyBase
{
    public GameObject projectilePrefab;
    public float attackRange = 8f;
    public float shootCooldown = 2f;
    public float keepDistance = 5f;
    private float lastShootTime;

    protected override void UpdateAI()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            // Shoot
            if (Time.time >= lastShootTime + shootCooldown)
            {
                ShootAtPlayer();
                lastShootTime = Time.time;
            }

            // Keep distance
            if (distanceToPlayer < keepDistance)
            {
                MoveAwayFromPlayer();
            }
        }
        else
        {
            ChasePlayer();
        }
    }

    private void ShootAtPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        proj.GetComponent<Projectile>().Initialize(damage, direction);
    }
}
```

### Bullet-Hell Pattern System
```csharp
public class BulletPattern : MonoBehaviour
{
    public enum PatternType { Circle, Spiral, Burst, Wave }

    public PatternType patternType;
    public GameObject projectilePrefab;
    public int projectileCount = 8;
    public float projectileSpeed = 5f;
    public float damage = 10f;

    public void ExecutePattern()
    {
        switch (patternType)
        {
            case PatternType.Circle:
                SpawnCirclePattern();
                break;
            case PatternType.Spiral:
                StartCoroutine(SpawnSpiralPattern());
                break;
            // ... other patterns
        }
    }

    private void SpawnCirclePattern()
    {
        float angleStep = 360f / projectileCount;

        for (int i = 0; i < projectileCount; i++)
        {
            float angle = i * angleStep;
            Vector2 direction = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            );

            GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            proj.GetComponent<Projectile>().Initialize(damage, direction);
        }
    }

    private IEnumerator SpawnSpiralPattern()
    {
        float currentAngle = 0f;

        for (int i = 0; i < projectileCount; i++)
        {
            Vector2 direction = new Vector2(
                Mathf.Cos(currentAngle * Mathf.Deg2Rad),
                Mathf.Sin(currentAngle * Mathf.Deg2Rad)
            );

            GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            proj.GetComponent<Projectile>().Initialize(damage, direction);

            currentAngle += 360f / projectileCount;
            yield return new WaitForSeconds(0.1f);
        }
    }
}
```

---

## Dungeon Generation

### Room System
```csharp
public class Room : MonoBehaviour
{
    public enum RoomType { Combat, Boss, Shop, Treasure, Event, Blacksmith }

    public RoomType roomType;
    public List<Door> doors;
    public List<Transform> enemySpawnPoints;
    public List<EnemyWave> waves;

    private int currentWave = 0;
    private int enemiesRemaining = 0;
    private bool isCleared = false;

    public void OnPlayerEnter()
    {
        CloseDoors();

        if (roomType == RoomType.Combat && !isCleared)
        {
            SpawnWave(currentWave);
        }

        GameEvents.RoomEntered(this);
    }

    private void SpawnWave(int waveIndex)
    {
        EnemyWave wave = waves[waveIndex];

        foreach (var enemyType in wave.enemies)
        {
            Transform spawnPoint = enemySpawnPoints[Random.Range(0, enemySpawnPoints.Count)];
            GameObject enemy = Instantiate(enemyType, spawnPoint.position, Quaternion.identity);
            enemiesRemaining++;
        }
    }

    public void OnEnemyKilled()
    {
        enemiesRemaining--;

        if (enemiesRemaining <= 0)
        {
            currentWave++;

            if (currentWave < waves.Count)
            {
                SpawnWave(currentWave);
            }
            else
            {
                RoomCleared();
            }
        }
    }

    private void RoomCleared()
    {
        isCleared = true;
        OpenDoors();
        SpawnRewards();
        GameEvents.RoomCleared();
    }

    private void CloseDoors()
    {
        foreach (var door in doors) door.Close();
    }

    private void OpenDoors()
    {
        foreach (var door in doors) door.Open();
    }
}

[System.Serializable]
public class EnemyWave
{
    public List<GameObject> enemies;
}
```

### Floor Manager
```csharp
public class FloorManager : MonoBehaviour
{
    [Header("Floor Settings")]
    public int currentFloor = 1;
    public int roomsPerFloor = 10;

    [Header("Room Prefabs")]
    public List<GameObject> combatRoomPrefabs;
    public List<GameObject> bossRoomPrefabs;
    public GameObject shopRoomPrefab;
    public GameObject treasureRoomPrefab;

    private List<Room> generatedRooms = new List<Room>();
    private int currentRoomIndex = 0;

    public void GenerateFloor()
    {
        // Simple linear progression for MVP
        // Later: implement procedural layout

        for (int i = 0; i < roomsPerFloor - 1; i++)
        {
            SpawnRoom(GetRandomCombatRoom(), i);
        }

        // Boss room at the end
        SpawnRoom(GetRandomBossRoom(), roomsPerFloor - 1);
    }

    private void SpawnRoom(GameObject roomPrefab, int index)
    {
        Vector3 position = new Vector3(index * 30f, 0, 0); // Simple linear layout
        GameObject roomObj = Instantiate(roomPrefab, position, Quaternion.identity);
        generatedRooms.Add(roomObj.GetComponent<Room>());
    }

    public void MoveToNextRoom()
    {
        currentRoomIndex++;

        if (currentRoomIndex >= roomsPerFloor)
        {
            // Floor complete
            currentFloor++;
            GenerateFloor();
        }
    }
}
```

---

## Progression Systems

### Leveling System
```csharp
public class LevelingSystem : MonoBehaviour
{
    private PlayerStats stats;

    public void AddXP(int amount)
    {
        stats.CurrentXP += amount;

        while (stats.CurrentXP >= stats.XPToNextLevel)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        stats.CurrentLevel++;
        stats.CurrentXP -= stats.XPToNextLevel;
        stats.XPToNextLevel = CalculateXPForNextLevel(stats.CurrentLevel);

        // Trigger trait selection
        GameEvents.PlayerLevelUp(stats.CurrentLevel);
        ShowTraitSelection();
    }

    private int CalculateXPForNextLevel(int level)
    {
        // Exponential scaling
        return Mathf.RoundToInt(100 * Mathf.Pow(1.15f, level - 1));
    }

    private void ShowTraitSelection()
    {
        TraitManager.Instance.ShowTraitSelection(3); // Show 3 random traits
    }
}
```

### Trait System
```csharp
[CreateAssetMenu(fileName = "New Trait", menuName = "Game/Trait")]
public class Trait : ScriptableObject
{
    public string traitName;
    public string description;
    public Sprite icon;
    public TraitRarity rarity;

    public List<StatModifier> statModifiers;
    public List<SpecialEffect> specialEffects;

    public void Apply(PlayerStats stats)
    {
        foreach (var modifier in statModifiers)
        {
            stats.ApplyModifier(modifier.statType, modifier.value);
        }

        foreach (var effect in specialEffects)
        {
            effect.Activate();
        }
    }
}

public enum TraitRarity { Common, Uncommon, Rare, Epic, Legendary }

[System.Serializable]
public class StatModifier
{
    public StatType statType;
    public float value;
    public bool isMultiplicative; // true = %, false = flat
}
```

---

## Item & Equipment

### Item Base
```csharp
[CreateAssetMenu(fileName = "New Item", menuName = "Game/Item")]
public class Item : ScriptableObject
{
    public string itemName;
    public string description;
    public Sprite icon;
    public ItemType itemType;
    public ItemRarity rarity;

    public List<StatModifier> statModifiers;
}

public enum ItemType { Weapon, Armor, Helmet, Boots, Ring, Amulet, Consumable }
public enum ItemRarity { Common, Uncommon, Rare, Epic, Legendary }
```

### Inventory System
```csharp
public class InventorySystem : MonoBehaviour
{
    public Dictionary<ItemType, Item> equippedItems = new Dictionary<ItemType, Item>();
    public List<Item> consumables = new List<Item>();

    private PlayerStats stats;

    public void EquipItem(Item item)
    {
        // Unequip current item in slot
        if (equippedItems.ContainsKey(item.itemType))
        {
            UnequipItem(item.itemType);
        }

        // Equip new item
        equippedItems[item.itemType] = item;
        ApplyItemStats(item);

        GameEvents.ItemEquipped(item);
    }

    public void UnequipItem(ItemType slot)
    {
        if (equippedItems.TryGetValue(slot, out Item item))
        {
            RemoveItemStats(item);
            equippedItems.Remove(slot);
        }
    }

    private void ApplyItemStats(Item item)
    {
        foreach (var modifier in item.statModifiers)
        {
            stats.ApplyModifier(modifier.statType, modifier.value);
        }
    }

    private void RemoveItemStats(Item item)
    {
        foreach (var modifier in item.statModifiers)
        {
            stats.ApplyModifier(modifier.statType, -modifier.value);
        }
    }
}
```

---

## UI System

### HUD Manager
```csharp
public class HUDManager : MonoBehaviour
{
    [Header("Health")]
    public Slider healthBar;
    public TextMeshProUGUI healthText;

    [Header("XP")]
    public Slider xpBar;
    public TextMeshProUGUI levelText;

    [Header("Stats")]
    public TextMeshProUGUI damageText;
    public TextMeshProUGUI speedText;

    private void OnEnable()
    {
        GameEvents.OnPlayerHealthChanged += UpdateHealth;
        GameEvents.OnPlayerLevelUp += UpdateLevel;
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerHealthChanged -= UpdateHealth;
        GameEvents.OnPlayerLevelUp -= UpdateLevel;
    }

    private void UpdateHealth(float currentHealth, float maxHealth)
    {
        healthBar.value = currentHealth / maxHealth;
        healthText.text = $"{currentHealth:F0} / {maxHealth:F0}";
    }

    private void UpdateLevel(int level)
    {
        levelText.text = $"Level {level}";
    }
}
```

---

## Data Management

### ScriptableObject Architecture
```csharp
// Store all game data as ScriptableObjects
[CreateAssetMenu(fileName = "Game Database", menuName = "Game/Database")]
public class GameDatabase : ScriptableObject
{
    public List<Trait> allTraits;
    public List<Item> allItems;
    public List<WeaponData> allWeapons;
    public List<CharacterClass> allClasses;

    public Trait GetRandomTrait(TraitRarity rarity)
    {
        var traitsOfRarity = allTraits.Where(t => t.rarity == rarity).ToList();
        return traitsOfRarity[Random.Range(0, traitsOfRarity.Count)];
    }
}
```

### Save System
```csharp
public class SaveManager : MonoBehaviour
{
    private string saveFilePath;

    [System.Serializable]
    public class SaveData
    {
        public int masteryLevel;
        public List<string> unlockedClasses;
        public List<string> unlockedItems;
        public Dictionary<string, int> statistics;
    }

    public void SaveGame(SaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(saveFilePath, json);
    }

    public SaveData LoadGame()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            return JsonUtility.FromJson<SaveData>(json);
        }
        return new SaveData();
    }
}
```

---

## Performance Optimization

### Object Pooling
```csharp
public class ObjectPool : MonoBehaviour
{
    public GameObject prefab;
    public int poolSize = 50;

    private Queue<GameObject> pool = new Queue<GameObject>();

    private void Start()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(prefab);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public GameObject GetObject()
    {
        if (pool.Count > 0)
        {
            GameObject obj = pool.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        else
        {
            return Instantiate(prefab);
        }
    }

    public void ReturnObject(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}
```

### Best Practices
1. **Object Pooling**: Pool projectiles, VFX, enemies
2. **Culling**: Disable off-screen enemies
3. **Batching**: Use sprite atlases
4. **Avoid Frequent Instantiation**: Pre-spawn what you can
5. **Use Object References**: Cache GetComponent calls
6. **Optimize Physics**: Use appropriate collision detection modes

---

## Next Steps

1. Set up Unity project with folder structure
2. Implement core GameManager and event system
3. Create player controller with movement
4. Build basic weapon system
5. Implement enemy AI
6. Create simple room with combat
7. Add leveling and traits
8. Iterate and expand

## Testing Checklist

- [ ] Player moves smoothly in all directions
- [ ] Weapons attack and deal damage correctly
- [ ] Enemies spawn and pursue player
- [ ] Projectiles hit and deal damage
- [ ] Room clears when all enemies defeated
- [ ] XP awards and leveling works
- [ ] Traits apply effects correctly
- [ ] Equipment changes stats properly
- [ ] Game saves and loads correctly
- [ ] 60 FPS maintained with 50+ projectiles on screen

---

**This guide will be updated as development progresses.**
