# Dynamic Terrain System

**A high-performance Unity terrain manipulation system using heightmap projection**

Created by [evesfect](https://github.com/evesfect)

---

## Overview

Dynamic Terrain System enables real-time terrain manipulation in Unity through mesh-based heightmap projection. Instead of using individual meshes for ground geometry (which severely impacts performance), this system projects mesh heights onto Unity's terrain heightmap, leveraging the engine's built-in LOD optimization and culling systems.

The entire terrain update process is handled through a single function call, making it ideal for both editor workflows and runtime applications.

---

## Goal

Building ground terrain in Unity traditionally involves either:
- **Individual meshes** → Poor performance due to excessive draw calls and lack of LOD optimization
- **Manual terrain sculpting** → Time-consuming and difficult to automate

This system solves both problems by:
- **Projecting mesh geometry to heightmap data** → Only terrain data is modified, no performance penalty
- **Leveraging Unity's terrain system** → Automatic LOD, occlusion culling, and optimized rendering
- **Single function API** → Can be called at any time, in editor or runtime
- **Supports runtime manipulation** → Ideal for procedural generation, building placement, and ground manipulation

### Use Cases
- **Procedural terrain generation** - Manipulate terrain from code at runtime
- **Building placement systems** - Automatically flatten or modify terrain when placing structures
- **Dynamic environment changes** - Modify terrain in response to gameplay events
- **Rapid level design** - Quickly block out terrain using simple meshes in the editor

---

## Features

- **Runtime and Editor Terrain Manipulation**

- **Automatic LOD's from native Unity Terrain**


- **Flexible Terrain Sizes** - Supports almost any terrain dimensions and height ranges (adjustable via settings)
- **Single Function Call** - `ApplyRenderTextureToTerrain()` handles the entire update process
- **GPU-Accelerated** - Uses GPU texture copy for efficient heightmap updates
- **Manual or Automatic** - Trigger updates manually / integrate into your own scripts
- **No Performance Impact** - Only modifies terrain data; rendering is handled by Unity's optimized terrain system

### Performance Notes
- For standard and large terrains, runtime performance is excellent due to Unity's terrain LOD system
- Extremely large maps (>4000x4000 resolution) may experience slower update times during heightmap application
- The terrain itself renders at native Unity terrain performance regardless of update frequency

---

## Technical Implementation

The system uses a three-stage pipeline to convert mesh geometry into terrain heightmap data:

### 1. Orthographic Camera Capture
An orthographic camera positioned above the terrain captures a top-down view:
- Positioned at terrain center, looking down along the Y-axis
- Orthographic size matches terrain dimensions for 1:1 mapping
- Culling mask set to only render the `TerrainMeshes` layer
- Outputs to a RenderTexture matching the terrain's heightmap resolution

### 2. Height Encoding via Shader
Meshes in the `TerrainMeshes` layer use a custom URP shader (`HeightShader.shader`) that:
- Reads the mesh vertex world-space Y position
- Normalizes height between configurable min/max height bounds
- Encodes normalized height as grayscale values (0.0 to 0.5 range)
- Outputs to the camera's RenderTexture

The shader formula: `normalizedHeight = saturate((worldY - minHeight) / (maxHeight - minHeight)) * 0.5`

### 3. GPU Texture-to-Heightmap Copy
The `TerrainHeightmapUpdater` component:
- Receives the RenderTexture containing encoded height data
- Uses Unity's `CopyActiveRenderTextureToHeightmap()` API for efficient GPU-to-terrain transfer
- Optionally syncs terrain LOD meshes via `SyncHeightmap()`
- Applies the heightmap in a single frame

---

## Requirements

- **Render Pipeline:** Universal Render Pipeline (URP)
- **Required Layer:** `TerrainMeshes` (Layer 10 by default)

---

## Setup

1. You can use the DynamicTerrain prefab for ease (foun in `Assets/DynamicTerrainSystem/Prefabs`)
2. Attach your terrain to the Dynamic Terrain Manager script (in the prefab)
3. You should call Update Terrain and Camera function (available in context menu)
4. Assign M_HeightShader material to your terrain meshes
5. Set your terrain meshes layer to 10:TerrainMeshes (just edit if necessary)
6. Ready to go, you can call `Apply Render Texture to Terrain` at editor/runtime
Note: I recommend disabling 10:TerrainMeshes layer from the main cameras culling mask, for convenience.

You can see the setup in the TestScene

## Usage

### Runtime/Code Integration

```csharp
// Get reference to the manager
DynamicTerrainManager manager = FindObjectOfType<DynamicTerrainManager>();

// Update terrain from current mesh positions
manager.ApplyRenderTextureToTerrain();

// Or update terrain configuration first, then apply
manager.UpdateTerrainAndCamera();
manager.ApplyRenderTextureToTerrain();
```
---

### Adjusting Terrain Settings

Modify the `TerrainSettings` in the `DynamicTerrainManager`:
- `terrainSize` - XZ dimensions of the terrain (Vector2)
- `minHeight` / `maxHeight` - Y-axis height range for encoding
- `heightmapResolution` - Resolution of the heightmap (must match RenderTexture resolution)

**Important:** Ensure the `_MinHeight` and `_MaxHeight` properties on the `M_HeightShader` material match your terrain settings.

---

## Credits

Created by **evesfect**

Built with Unity 6000.0.34f1 LTS and Universal Render Pipeline.
