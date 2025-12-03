# Unity Project Setup Guide

This guide will help you set up the Unity project for PixelGame from scratch.

## Prerequisites

1. **Unity Hub** - Download from [unity.com](https://unity.com/download)
2. **Unity Editor** - 2021.3 LTS or newer (recommended: 2022.3 LTS)
3. **Git** - Already initialized in this repository
4. **Code Editor** - Visual Studio, VS Code, or Rider

---

## Step 1: Install Unity

1. Open Unity Hub
2. Go to **Installs** tab
3. Click **Install Editor**
4. Select **2022.3 LTS** (recommended) or **2021.3 LTS**
5. Add modules:
   - **Development Tools**
   - **Visual Studio** (or your preferred IDE)
   - **Windows Build Support** (if on Windows)
   - **Mac Build Support** (if on Mac)
   - **Linux Build Support** (if on Linux)

---

## Step 2: Create Unity Project

1. Open Unity Hub
2. Click **Add** → **Add project from disk**
3. Navigate to this repository folder (`PixelGame`)
4. Select the folder
5. Unity Hub will detect it's a Git repository
6. Click on the project to open it in Unity

**OR** Create from scratch:

1. Click **New Project**
2. Select **2D Core** template
3. Name: `PixelGame`
4. Location: Choose this repository folder
5. Click **Create Project**

---

## Step 3: Configure Project Settings

### Player Settings
`Edit > Project Settings > Player`

- **Company Name**: Your name/studio
- **Product Name**: PixelGame
- **Default Icon**: (Add later)
- **Resolution and Presentation**:
  - Default Screen Width: 1920
  - Default Screen Height: 1080
  - Fullscreen Mode: Windowed
  - Run In Background: Enabled

### Quality Settings
`Edit > Project Settings > Quality`

- Set default quality level to "Medium" or "High"
- Disable shadows for 2D performance

### Physics 2D Settings
`Edit > Project Settings > Physics 2D`

- Gravity: (0, 0) - No gravity for top-down game
- **Layer Collision Matrix**:
  - Uncheck unnecessary collision pairs
  - Player should collide with: Enemy, Wall, Pickup
  - Enemy should collide with: Player, Wall
  - Projectile should collide with: Enemy (if player projectile) or Player (if enemy projectile), Wall

### Tags and Layers
`Edit > Project Settings > Tags and Layers`

**Tags:**
- Player
- Enemy
- Boss
- Projectile
- Pickup
- Wall
- Door

**Layers:**
- Default (0)
- UI (5) - Built-in
- Player (6)
- Enemy (7)
- PlayerProjectile (8)
- EnemyProjectile (9)
- Wall (10)
- Pickup (11)

### Input System
`Edit > Project Settings > Player > Other Settings`

- **Active Input Handling**: Both (old and new)
  - Or switch to "Input System Package (New)" if you want to use only the new system

---

## Step 4: Install Required Packages

`Window > Package Manager`

Install these packages:

1. **Input System**
   - Search: "Input System"
   - Click Install

2. **2D Animation**
   - Search: "2D Animation"
   - Click Install

3. **Cinemachine**
   - Search: "Cinemachine"
   - Click Install

4. **TextMesh Pro**
   - Should be already installed
   - If not, search and install

5. **2D Sprite**
   - Should be already installed

---

## Step 5: Create Folder Structure

In the **Project** window, create this folder structure:

```
Assets/
├── Scenes/
│   ├── MainMenu.unity
│   ├── GameScene.unity
│   └── TestScenes/
├── Scripts/
│   ├── Player/
│   ├── Enemies/
│   ├── Combat/
│   ├── Items/
│   ├── Dungeon/
│   ├── Progression/
│   ├── UI/
│   └── Managers/
├── Prefabs/
│   ├── Player/
│   ├── Enemies/
│   ├── Projectiles/
│   ├── Rooms/
│   ├── Items/
│   └── UI/
├── Art/
│   ├── Sprites/
│   │   ├── Characters/
│   │   ├── Enemies/
│   │   ├── Items/
│   │   ├── Tiles/
│   │   └── UI/
│   ├── Animations/
│   └── VFX/
├── Audio/
│   ├── Music/
│   └── SFX/
├── Data/
│   ├── Items/
│   ├── Weapons/
│   ├── Enemies/
│   ├── Classes/
│   └── Traits/
└── Materials/
```

**Quick way to create folders:**
1. Right-click in Project window
2. Create > Folder
3. Name it accordingly

---

## Step 6: Create Initial Scenes

### Main Menu Scene
1. `File > New Scene`
2. Select **2D**
3. `File > Save As`
4. Save to `Assets/Scenes/MainMenu.unity`

### Game Scene
1. `File > New Scene`
2. Select **2D**
3. Add a **Grid** GameObject
4. Add **Tilemap** child object
5. `File > Save As`
6. Save to `Assets/Scenes/GameScene.unity`

---

## Step 7: Configure Build Settings

`File > Build Settings`

1. Click **Add Open Scenes** to add current scene
2. Drag `MainMenu.unity` to top (first scene to load)
3. Add `GameScene.unity` below it
4. **Platform**: PC, Mac & Linux Standalone
5. **Target Platform**: Your OS

---

## Step 8: Set Up Version Control (Git)

Git is already initialized, but ensure Unity is properly configured:

### Unity Collaborate Settings
`Edit > Project Settings > Editor`

- **Version Control Mode**: Visible Meta Files
- **Asset Serialization Mode**: Force Text
- **Line Endings For New Scripts**: OS Native

This ensures:
- Meta files are tracked by Git
- Assets are stored as text (better for diffs)
- Proper line endings for your OS

### Verify .gitignore
The `.gitignore` file should already be set up correctly for Unity projects. It ignores:
- Library folder
- Temp folder
- Build folders
- User settings
- IDE files

---

## Step 9: Test the Setup

1. Create a test scene
2. Add a **Sprite** GameObject (right-click in Hierarchy > 2D Object > Sprite)
3. Add a simple C# script:

```csharp
using UnityEngine;

public class TestScript : MonoBehaviour
{
    void Start()
    {
        Debug.Log("Unity setup complete! Ready to develop PixelGame!");
    }
}
```

4. Attach the script to the sprite
5. Press **Play** button
6. Check **Console** for the log message

---

## Step 10: Initial Commit

After setup is complete:

1. Review changes in Git
2. Stage all Unity-generated files
3. Commit with message: "Unity project initialization"

```bash
git add .
git commit -m "Unity project initialization with folder structure"
git push
```

---

## Next Steps

1. Review `GAME_DESIGN.md` for game features
2. Read `IMPLEMENTATION.md` for technical details
3. Start with MVP features:
   - Basic player controller
   - Simple combat
   - One enemy type
   - Single room

---

## Troubleshooting

### Unity won't open the project
- Ensure correct Unity version (2021.3 LTS or newer)
- Delete `Library` and `Temp` folders, then reopen

### Scripts won't compile
- Check `Edit > Preferences > External Tools`
- Regenerate project files: `Assets > Open C# Project`

### Git issues
- Ensure `.gitignore` is properly configured
- Don't commit `Library`, `Temp`, or build folders

### Performance issues
- Close unnecessary Unity windows
- Disable **Auto-refresh** temporarily: `Edit > Preferences > Asset Pipeline`

---

## Recommended Unity Preferences

`Edit > Preferences`

**General:**
- Editor Theme: Personal preference (Dark or Light)
- Enable Editor Analytics: Optional

**External Tools:**
- External Script Editor: Visual Studio / VS Code / Rider
- Generate .csproj files for: Embedded packages, Local packages, Registry packages

**Asset Pipeline:**
- Auto Refresh: Enabled (disable if slow)
- Import Worker Count: 25% (adjust based on CPU)

**Colors:**
- Playmode tint: Enabled (helps identify when game is running)

---

## Useful Unity Shortcuts

- **F** - Frame selected object
- **Ctrl + D** - Duplicate
- **Ctrl + Shift + N** - New GameObject
- **Ctrl + S** - Save
- **Ctrl + P** - Play/Stop
- **Ctrl + Shift + B** - Build Settings
- **Ctrl + 0** - Scene view
- **Ctrl + 1** - Game view

---

**Setup Complete!** You're ready to start developing PixelGame! 🎮

Refer to `IMPLEMENTATION.md` for code examples and `GAME_DESIGN.md` for feature specifications.
