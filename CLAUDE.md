# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run

```bash
# Build the project
dotnet build

# Run the project
dotnet run

# Build for release
dotnet build -c Release
```

The project targets .NET 6.0 with unsafe blocks enabled for OpenGL interop.

## Project Overview

SharpCraft is a voxel-based Minecraft clone written from scratch in C# and OpenGL (via OpenTK). No game engines are used - all rendering, world generation, and game logic are custom implementations.

## Architecture

### Entry Point & Main Loop
- **Program.cs** - Creates the window and starts the Application
- **Application.cs** - Inherits from OpenTK's `GameWindow`; manages the main game loop, input, and rendering

### World System
The world is divided into chunks for efficient rendering and generation:

- **World.cs** - Manages the chunk system using `ConcurrentDictionary<Vector2, Chunk>`. Handles:
  - Chunk generation in a spiral pattern around the player
  - Chunk loading/unloading based on render distance
  - Mesh buffer updates (multi-threaded: background generation, main thread GPU upload)
  - World seed and random number generation

- **Chunk.cs** - Represents a 16x256x16 voxel region. Key lifecycle:
  1. `GenerateLandScape()` - Base terrain generation via noise
  2. `GeneratePostScape()` - Biomes, trees, surface materials
  3. `GenerateMesh()` - Builds geometry with greedy meshing (only visible faces)
  4. `MeshUpload()` - Uploads mesh data to GPU

### Rendering Pipeline
- **GlShader.cs** - Wrapper for OpenGL shader programs (vertex/fragment shaders)
- **MeshRenderer.cs** - Handles VAO/VBO management and draw calls
- **ChunkMesh.cs** - Builds triangle geometry for chunk faces with UVs and lighting
- **VoxelData.cs** - Static data for cube vertices, triangles, face normals, and lighting values

### World Generation
- **Generation.cs** - Orchestrates terrain generation phases
- **LandScape.cs** - Perlin/Simplex noise for terrain heightmaps
- **BiomeScape.cs** - Surface materials and structures based on biome

### Block System
- **Blocks.cs** - Loads block definitions from `res/blocks.yaml` (YAML)
- **Block.cs** - Represents a block type with ID, name, and texture mappings
- **Atlas.cs** - Generates a texture atlas from individual block textures; calculates UVs

### Player & Camera
- **Camera.cs** - FPS-style camera with pitch/yaw control; produces view/projection matrices
- **PlayerController.cs** - Handles WASD movement, mouse look, and flight controls

## Resource Structure

```
res/
├── Shaders/           # GLSL shader files
│   ├── chunkVert.glsl
│   ├── chunkFrag.glsl
│   └── shader.vert/frag
├── textures/blocks/   # Individual 16x16 PNG textures
└── blocks.yaml        # Block definitions (ID, name, textures per face)
```

## World Constants

- `World.CHUNK_SIZE = 16` - Horizontal chunk dimensions
- `World.CHUNK_HEIGHT = 256` - Vertical chunk dimension
- Chunks are indexed by `Vector2(chunkX, chunkZ)`
- World coordinates: `xWorld = chunkX * 16 + localX`

## Coordinate Systems

- **Voxel coordinates**: Local 0-15 (X/Z), 0-255 (Y) within a chunk
- **Chunk coordinates**: Integer grid of chunk positions
- **World coordinates**: Absolute position in the world

The voxel map is stored as a 1D array with index calculation:
```csharp
index = (z * CHUNK_SIZE * CHUNK_HEIGHT) + (y * CHUNK_SIZE) + x
```

## Visible Face Culling

When generating chunk meshes, faces are only added if the adjacent block is air (ID=0) or outside the chunk boundary. Chunk neighbors are linked to enable cross-chunk face culling.

## Threading Model

- Main thread: Rendering, input handling, GPU mesh uploads
- Background thread (optional via `StartUpdateThread()`): Chunk generation, mesh building
- Thread-safe collections (`ConcurrentDictionary`, `ConcurrentBag`, `ConcurrentQueue`) are used for chunk management
