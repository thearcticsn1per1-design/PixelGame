# PixelGame - Rogue-lite Dungeon Crawler Design Document

## Game Concept
A challenging fantasy rogue-lite bullet-hell dungeon crawler inspired by Tiny Rogues, featuring RPG elements, procedural generation, and meta-progression.

## Core Gameplay Loop
1. Select character class
2. Enter procedurally generated dungeon floor
3. Clear room of enemies
4. Choose reward/upgrade
5. Proceed to next room
6. Defeat floor boss
7. Progress to next floor or die and restart with meta-progression

## Key Features

### 1. Combat System
- **Player Combat**: Top-down action combat with dodge mechanics
- **Attack Types**:
  - Melee: Close-range physical attacks
  - Ranged: Projectile-based attacks
  - Magic: Spell casting with various effects
- **Bullet-Hell Elements**: Dense enemy projectile patterns requiring precision dodging

### 2. Dungeon Structure
- **Floors**: 10-20+ unique themed floors per run
- **Rooms**: Various room types
  - Combat rooms (standard enemy encounters)
  - Boss rooms (end of each floor)
  - Shop rooms (buy items/upgrades)
  - Treasure rooms (free items)
  - Event rooms (random encounters/choices)
  - Blacksmith rooms (upgrade weapons)

### 3. Character Classes
- **Goal**: 30+ unique character classes
- **Each Class Has**:
  - Unique passive abilities
  - Starting weapon
  - Starting stats
  - Special mechanics

### 4. Progression Systems

#### Within-Run Progression
- **Level Up System**: Gain XP from defeating enemies
- **Trait Selection**: Choose from 3-4 traits when leveling up
  - 100+ unique traits total
  - Traits modify stats, abilities, or gameplay mechanics
- **Equipment**: Find and equip items during run
  - 400+ weapons
  - 500+ equipment items (armor, accessories, consumables)

#### Meta-Progression
- **Mastery Level**: Persistent XP system across all runs
- **Unlocks**: New classes, items, floors unlock over time
- **Perks**: Permanent upgrades purchased with meta-currency
- **World Objectives**: Quest system to unlock new content

### 5. Enemy System
- **Unique Enemy Sets**: Each floor has themed enemies
- **AI Behaviors**:
  - Melee chasers
  - Ranged attackers
  - Bullet-hell pattern shooters
  - Special ability enemies
- **Boss Encounters**: 40+ unique boss fights
  - Multi-phase fights
  - Unique attack patterns
  - Special mechanics per boss

### 6. Weapon System
- **Weapon Categories**:
  - Swords, axes, spears (melee)
  - Bows, guns, crossbows (ranged)
  - Wands, staves, tomes (magic)
- **Weapon Stats**:
  - Damage
  - Attack speed
  - Range
  - Special effects
- **Rarity Tiers**: Common, Uncommon, Rare, Epic, Legendary

### 7. Equipment System
- **Equipment Slots**:
  - Weapon (primary)
  - Armor (chest)
  - Helmet
  - Boots
  - Rings (2 slots)
  - Amulet
  - Consumables (multiple slots)
- **Equipment Effects**: Stat bonuses, special abilities, synergies

## Technical Architecture

### Project Structure
```
Assets/
├── Scripts/
│   ├── Player/
│   │   ├── PlayerController.cs
│   │   ├── PlayerStats.cs
│   │   ├── PlayerCombat.cs
│   │   └── PlayerInventory.cs
│   ├── Enemies/
│   │   ├── EnemyBase.cs
│   │   ├── EnemyAI.cs
│   │   └── BossBase.cs
│   ├── Combat/
│   │   ├── WeaponBase.cs
│   │   ├── ProjectileBase.cs
│   │   └── DamageSystem.cs
│   ├── Items/
│   │   ├── ItemBase.cs
│   │   ├── EquipmentItem.cs
│   │   └── Consumable.cs
│   ├── Dungeon/
│   │   ├── RoomGenerator.cs
│   │   ├── FloorManager.cs
│   │   └── DoorController.cs
│   ├── Progression/
│   │   ├── LevelingSystem.cs
│   │   ├── TraitManager.cs
│   │   └── MetaProgressionManager.cs
│   ├── UI/
│   │   ├── HUDManager.cs
│   │   ├── MenuManager.cs
│   │   └── InventoryUI.cs
│   └── Managers/
│       ├── GameManager.cs
│       ├── SaveManager.cs
│       └── AudioManager.cs
├── Prefabs/
│   ├── Player/
│   ├── Enemies/
│   ├── Rooms/
│   ├── Items/
│   └── UI/
├── Scenes/
│   ├── MainMenu.unity
│   ├── GameScene.unity
│   └── TestScenes/
├── Art/
│   ├── Sprites/
│   ├── Animations/
│   └── VFX/
├── Audio/
│   ├── Music/
│   └── SFX/
└── Data/
    ├── Items/
    ├── Enemies/
    ├── Classes/
    └── Traits/
```

### Core Systems

#### 1. Game Manager
- Singleton pattern
- Manages game state (menu, playing, paused, game over)
- Handles scene transitions
- Coordinates between systems

#### 2. Room/Floor Generation
- Procedural generation algorithm
- Room templates with spawn points
- Door connections between rooms
- Floor themes and difficulty scaling

#### 3. Combat System
- Damage calculation
- Hit detection (collision-based)
- Projectile pooling for performance
- Status effects system

#### 4. Save System
- JSON-based save files
- Separate files for:
  - Meta-progression (unlocks, mastery)
  - Settings (audio, controls)
  - Statistics (runs completed, enemies killed)

## Development Phases

### Phase 1: Core Foundation (Weeks 1-3)
- [ ] Unity project setup
- [ ] Basic player movement and controls
- [ ] Simple combat (one weapon type)
- [ ] Basic enemy AI
- [ ] Single room prototype

### Phase 2: Combat & Enemies (Weeks 4-6)
- [ ] Multiple weapon types
- [ ] Advanced enemy behaviors
- [ ] Bullet-hell patterns
- [ ] Damage system refinement
- [ ] Visual feedback (hit effects, screen shake)

### Phase 3: Dungeon Generation (Weeks 7-9)
- [ ] Room generation system
- [ ] Multiple room types
- [ ] Floor progression
- [ ] Door mechanics
- [ ] Room transitions

### Phase 4: Progression Systems (Weeks 10-12)
- [ ] Level-up system
- [ ] Trait selection
- [ ] Equipment system
- [ ] Inventory management
- [ ] Character stats

### Phase 5: Content Creation (Weeks 13-16)
- [ ] Create 10+ character classes
- [ ] Design 50+ traits
- [ ] Create 100+ weapons
- [ ] Create 100+ equipment items
- [ ] Design 5+ floor themes

### Phase 6: Boss Battles (Weeks 17-19)
- [ ] Boss framework
- [ ] 10+ unique boss fights
- [ ] Boss phases
- [ ] Special mechanics
- [ ] Boss rewards

### Phase 7: Meta-Progression (Weeks 20-22)
- [ ] Mastery system
- [ ] Unlock system
- [ ] Persistent perks
- [ ] World objectives
- [ ] Achievement system

### Phase 8: Polish & UI (Weeks 23-25)
- [ ] Complete UI overhaul
- [ ] Menu systems
- [ ] Audio implementation
- [ ] Visual effects
- [ ] Animations

### Phase 9: Balancing & Testing (Weeks 26-30)
- [ ] Difficulty balancing
- [ ] Item/trait balancing
- [ ] Bug fixing
- [ ] Playtesting
- [ ] Performance optimization

## Art Style
- **Pixel Art**: 16x16 or 32x32 sprite resolution
- **Color Palette**: Fantasy theme with dark dungeon aesthetics
- **Animation**: Smooth character and enemy animations
- **VFX**: Impactful combat effects, projectiles, explosions

## Audio Design
- **Music**: Atmospheric dungeon ambience, intense boss themes
- **SFX**:
  - Combat (hits, projectiles, abilities)
  - UI (menu navigation, item pickup)
  - Ambient (dungeon sounds, doors)

## Performance Targets
- **60 FPS** on mid-range hardware
- **Object Pooling**: For projectiles, enemies, VFX
- **Optimization**: Efficient room loading/unloading

## Minimum Viable Product (MVP)
To get a playable prototype:
1. Player movement + basic attack
2. 1-2 enemy types
3. Single room combat
4. Simple level-up system (3-5 traits)
5. 5-10 weapons
6. 3 character classes
7. Basic UI (health, level, inventory)
8. 1 boss fight

## Future Expansion Ideas
- Multiplayer co-op
- Daily/weekly challenges
- Leaderboards
- Modding support
- Additional game modes (endless mode, challenge runs)
- More floors, bosses, items, and classes

## Success Metrics
- **Replayability**: Each run feels unique
- **Difficulty Balance**: Challenging but fair
- **Content Variety**: Hundreds of items/traits/builds
- **Meta-Progression**: Meaningful long-term goals
- **Performance**: Smooth 60 FPS even with many projectiles

---

## Next Steps
1. Set up Unity project with proper folder structure
2. Create core game manager and player controller
3. Implement basic combat prototype
4. Build room generation system
5. Iterate and expand based on playtesting
