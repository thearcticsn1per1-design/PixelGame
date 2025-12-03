# PixelGame - Rogue-lite Dungeon Crawler

A challenging fantasy rogue-lite bullet-hell dungeon crawler inspired by Tiny Rogues, featuring RPG elements, procedural dungeon generation, and deep meta-progression systems.

## 🎮 Game Overview

PixelGame is a top-down action dungeon crawler where players:
- Choose from multiple character classes with unique abilities
- Battle through procedurally generated rooms filled with enemies
- Dodge bullet-hell style attack patterns
- Collect powerful weapons and equipment
- Level up and select traits to build unique character builds
- Defeat challenging bosses at the end of each floor
- Progress through meta-progression systems for long-term advancement

## ✨ Key Features

### Combat & Gameplay
- **3 Combat Archetypes**: Melee, Ranged, and Magic builds
- **Bullet-Hell Mechanics**: Dodge dense projectile patterns
- **Room-by-Room Progression**: Clear rooms, choose rewards, advance

### Character Progression
- **30+ Character Classes** with unique passive abilities
- **100+ Traits** to customize your build each run
- **400+ Weapons** across all combat types
- **500+ Equipment Items** for endless build variety

### Dungeon Structure
- **20+ Unique Floors** with themed enemies
- **40+ Boss Encounters** with unique mechanics
- **Multiple Room Types**: Combat, Shop, Treasure, Events, Blacksmith

### Meta-Progression
- **Mastery System**: Permanent progression across runs
- **Unlockable Content**: New classes, items, and floors
- **Persistent Perks**: Permanent upgrades
- **World Objectives**: Quest-based unlocks

## 🛠️ Technology Stack

- **Engine**: Unity 2021+ (recommended)
- **Language**: C#
- **Art Style**: Pixel art (16x16 or 32x32)
- **Version Control**: Git

## 📁 Project Structure

```
PixelGame/
├── Assets/               # Unity assets (created after initialization)
│   ├── Scripts/         # C# game scripts
│   ├── Prefabs/         # Game object prefabs
│   ├── Scenes/          # Unity scenes
│   ├── Art/             # Sprites, animations, VFX
│   ├── Audio/           # Music and sound effects
│   └── Data/            # ScriptableObjects for game data
├── .gitignore           # Unity-specific gitignore
├── README.md            # This file
├── GAME_DESIGN.md       # Detailed design document
└── IMPLEMENTATION.md    # Technical implementation guide
```

## 🚀 Getting Started

### Prerequisites
- Unity Hub installed
- Unity 2021.3 LTS or newer
- Git for version control
- Basic C# knowledge

### Initial Setup
1. Clone this repository
2. Open Unity Hub
3. Click "Add" and select this project folder
4. Let Unity create the project files
5. Open the project in Unity

### First Steps
1. Review `GAME_DESIGN.md` for game concept and features
2. Check `IMPLEMENTATION.md` for technical guidelines (to be created)
3. Start with the MVP (Minimum Viable Product) features
4. Follow the development phases outlined in the design doc

## 🎯 Development Roadmap

### Current Phase: Foundation
- [ ] Unity project initialization
- [ ] Core folder structure setup
- [ ] Basic player controller
- [ ] Simple combat prototype

### Upcoming Phases
1. **Combat & Enemies** - Weapon types, enemy AI, bullet patterns
2. **Dungeon Generation** - Room generation, floor progression
3. **Progression Systems** - Leveling, traits, equipment
4. **Content Creation** - Classes, items, weapons, traits
5. **Boss Battles** - Unique boss encounters
6. **Meta-Progression** - Mastery, unlocks, persistent perks
7. **Polish & UI** - Complete UI, audio, VFX
8. **Balancing & Testing** - Difficulty tuning, optimization

## 🎨 Art Style Guidelines

- **Resolution**: 16x16 or 32x32 pixels per sprite
- **Palette**: Fantasy dark dungeon aesthetic
- **Animation**: Smooth 8-12 frame animations
- **VFX**: Impactful combat effects, readable projectiles

## 🎵 Audio Guidelines

- **Music**: Atmospheric dungeon themes, intense boss music
- **SFX**: Clear, impactful combat sounds
- **Mixing**: Balanced levels, important sounds prioritized

## 📝 Contributing

This is a personal/learning project. Development approach:
1. Follow the design document
2. Keep code modular and documented
3. Test frequently during development
4. Iterate based on playtesting

## 📊 Performance Goals

- **Target**: 60 FPS on mid-range hardware
- **Optimization**: Object pooling for projectiles/enemies
- **Memory**: Efficient asset loading/unloading

## 🎓 Learning Resources

- [Unity Documentation](https://docs.unity3d.com/)
- [Roguelike Development Tips](https://www.reddit.com/r/roguelikedev/)
- [Game Programming Patterns](https://gameprogrammingpatterns.com/)
- [Tiny Rogues on Steam](https://store.steampowered.com/app/2088570/Tiny_Rogues/) (inspiration)

## 📄 License

This is a personal project. All rights reserved.

## 🤝 Credits

- Inspired by **Tiny Rogues** by RubyDev
- Developed as a learning project

---

**Current Status**: 🟡 Initial Setup Phase
**Last Updated**: December 2025
