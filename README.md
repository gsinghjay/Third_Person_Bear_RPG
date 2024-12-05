# Bear Hunter RPG - Character & Terrain

## Project Overview
This is a Third Person RPG where players hunt different types of bears across various forest regions. The game features a central village hub with paths leading to three distinct combat arenas and a victory tower.

## Current Implementation

## Camera System

### Virtual Camera Setup
- **Normal Camera (Default View)**
  - FOV: 60°
  - Follow Distance: 5 units
  - Follow Height: 1.5 units
  - Damping: 0 for WebGL optimization
  - Priority: 10 (default)

- **Combat Camera**
  - FOV: 55°
  - Follow Distance: 4 units
  - Follow Height: 2 units
  - Damping: 0 for WebGL optimization
  - Priority: 20 (when active)

- **Sprint Camera**
  - FOV: 70°
  - Follow Distance: 6 units
  - Follow Height: 1.7 units
  - Damping: 0 for WebGL optimization
  - Priority: 30 (when active)

### Camera Controller Settings
```json
{
    "normalFOV": 60,
    "sprintFOV": 70,
    "combatFOV": 55,
    "followDistance": 5,
    "followHeight": 1.5,
    "mouseSensitivity": {
    "horizontal": 175,
    "vertical": 1.5
    }
}
```

### WebGL Optimizations
- Zero acceleration/deceleration times for responsive control
- Direct input axis mapping through Cinemachine
- Optimized axis speeds (X: 175, Y: 1.5)
- Simplified camera state transitions
- Efficient priority-based camera switching

### Input System Updates
- Mouse X/Y axis directly mapped to Cinemachine
- Removed manual delta calculations
- Optimized for WebGL performance
- Maintained Y-axis inversion option
- Smooth camera transitions between states

### Required Components
- Main Camera with CameraController script
- Three Cinemachine Virtual Cameras
- Cinemachine Brain on Main Camera

### Player Character System
- Using MaleCharacterPBR from RPG Tiny Hero Duo asset
- Character Controller with terrain-aware positioning
- Input system using interface segregation (IPlayerInput)
- Animation states for movement and combat
- WebGL-optimized character materials and shaders

### Required Packages
```json
{
    "com.unity.cinemachine": "2.10.3",
    "com.unity.render-pipelines.universal": "14.0.11",
    "com.unity.terrain-tools": "5.0.5"
}
```

### Layer Setup
```
Layer 6: Ground
Layer 7: Terrain
Layer 8: Environment
Layer 9: Player
Layer 10: Enemy
Layer 11: Interactable
```

## Character Settings Guide

### Player Controller Settings
```
Move Speed: 5
Rotation Speed: 10
Character Controller:
- Height: 1.5
- Radius: 0.4
- Slope Limit: 45
- Step Offset: 0.3
```

### Animation States
- Idle_Battle_SwordAndShield
- MoveFWD_Battle_InPlace_SwordAndShield
- Attack01_SwordAndShield
- Victory_Battle_SwordAndShield

## Terrain Generation System
We've implemented a custom terrain generator that creates:
- Central village hub (flat area)
- Three combat arenas for bear encounters
- Elevated victory tower area
- Interconnecting paths between all locations

### Required Packages
```json
{
    "com.unity.cinemachine": "2.10.3",
    "com.unity.render-pipelines.universal": "14.0.11",
    "com.unity.terrain-tools": "5.0.5"
}
```
## Scene Hierarchy Guide

### Main Camera Components
- **Main Camera**
  - Primary camera with Cinemachine Brain component
  - Handles camera blending and transitions
  - Update Method: FixedUpdate for consistent frame timing

### Virtual Cameras
- **CM vcam Normal** (Priority: 10)
  - Default third-person camera view
  - Used for general exploration and movement
  - FOV: 60°, Follow Distance: 5 units
  - Optimized damping for smooth movement

- **CM vcam Combat** (Priority: 20)
  - Activated during combat encounters
  - Closer follow distance for better action visibility
  - FOV: 55°, Follow Distance: 4 units
  - Increased damping for stable combat view

- **CM vcam Sprint** (Priority: 30)
  - Activates during player sprinting
  - Wider FOV and increased follow distance
  - FOV: 70°, Follow Distance: 6 units
  - Optimized for fast movement

### Player Setup
- **Player**
  - Root object with CharacterController component
  - Contains PlayerInput and PlayerController scripts
  - **MaleCharacterPBR**
    - Character model and animations
  - **CameraTarget**
    - Empty transform for camera follow/look target
    - Offset slightly above player model
    - Used by all virtual cameras

### Required Components
- Each virtual camera requires:
  - CinemachineFreeLook component
  - Properly configured Input settings
  - Assigned Follow/Look target
  - WebGL-optimized damping values

### Camera Priority System
- Higher priority cameras override lower ones
- Normal Camera: 10 (default)
- Combat Camera: 20 (when in combat)
- Sprint Camera: 30 (when sprinting)
- Smooth blending between states

## Inspector Settings Guide

### Terrain Object Settings
```
Terrain Width: 1000
Terrain Length: 1000
Terrain Height: 200
Heightmap Resolution: 513
Detail Resolution: 1024
Control Texture Resolution: 1024
Base Texture Resolution: 1024
```

### Terrain Generator Window (Tools > Terrain Generator)
#### General Settings
- **Base Height**: 20 (Base terrain elevation)
- **Mountain Height**: 100 (Maximum mountain peaks)
- **Noise Scale**: 50 (Controls terrain roughness)

#### Village Settings
- **Village Radius**: 100 (Size of central flat area)
- **Transition Zone**: 50 (Smooth blend to mountains)
- **Village Height**: 30 (Elevation of village area)

#### Path Settings
- **Path Width**: 15 (Width of connecting paths)
- **Path Smoothness**: 20 (Blend between path and terrain)

#### Victory Tower Settings
- **Include Tower Path**: Toggle for testing
- **Tower Position**: (0.5, 0.1) Vector2 field
- **Tower Area Radius**: 50
- **Tower Height**: 80
- **Tower Path Width**: 20

## Combat Areas
### Northwest Arena - Fire Bears
- Radius: 35 units
- Population: 2 normal bears, 2 fire bears
- Flatness: 0.85 (slight terrain variation)
- Position: (0.25, 0.25)

### Northeast Arena - Ice Bears
- Radius: 40 units
- Population: 1 normal bear, 3 ice bears
- Flatness: 0.9 (more even terrain)
- Position: (0.75, 0.25)

### South Arena - Boss Area
- Radius: 50 units (largest arena)
- Population: 2 normal bears, 2 fire bears, 2 ice bears
- Flatness: 0.95 (very flat for boss fights)
- Position: (0.5, 0.75)

## Bear Types
- **Normal Bears**: Standard enemy type
- **Fire Bears**: Special enemy with fire-based attacks
- **Ice Bears**: Special enemy with ice-based attacks

## Spawn System
- Each arena has predetermined spawn points
- Spawn points are generated in a circular pattern
- Points are placed at 60% of arena radius
- Equal spacing between spawn points

### Important Notes
- All positions are normalized (0-1 range)
- Heights are in world units
- Radius values are in terrain units
- Changes take effect upon clicking "Generate Terrain"
- Settings can be adjusted in real-time
- Generator preserves terrain data resolution
- Character position starts at terrain center
- Animation controllers are WebGL-optimized
- PBR materials used for best visual quality
- Input system is abstracted for future platform support

## Usage Instructions
1. Create a new terrain in Unity (3D Object > Terrain)
2. Open the Terrain Generator (Tools > Terrain Generator)
3. Assign your terrain to the generator
4. Configure settings in each section:
   - Adjust general terrain parameters
   - Set village area size and height
   - Position forest areas
   - Configure victory tower location
5. Click "Generate Terrain" to create the landscape
6. Fine-tune settings as needed
7. Save terrain changes in Unity

## Next Steps
1. Implement combat system
2. Add bear AI and behaviors
3. Set up quest/mission system
4. Add inventory management
5. Implement save/load system

## Troubleshooting
- If character movement feels sluggish: Adjust moveSpeed in PlayerController
- If rotations are too fast/slow: Modify rotationSpeed
- If character clips through terrain: Check CharacterController settings
- If animations are choppy: Verify WebGL optimization settings
- If terrain appears too steep: Reduce mountain height or increase noise scale
- If paths are too narrow: Increase path width values
- If transitions are harsh: Increase transition zone or path smoothness
- If forest areas overlap: Adjust positions in normalized space
```


