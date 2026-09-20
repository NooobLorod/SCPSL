# SCPSL DMA

A high-performance DMA-based (Direct Memory Access) tool for **SCP: Secret Laboratory** built with .NET 10 and ImGui. Reads game memory through a PCIe DMA device (FPGA) to provide real-time ESP overlay and game information without injecting code or modules into the game process.

> 📋 **Changelogs**: See [CHANGELOG.md](CHANGELOG.md) for full version history and release notes.

---

## Features

### 🎯 Player ESP
- **Bounding Boxes** — 2D boxes around players, color-coded by team/role
- **17-Bone Skeleton** — Full 17-bone wireframe skeleton rendering including dedicated hand joints (supports human, SCP-106, and SCP-049-2 alternate bone hierarchies)
- **Instant Bone Acquisition** — Immediate skeletal mesh resolution upon player discovery with zero delay
- **Name Tags** — Player names with real-time distance display
- **Health Bars** — Vertical health bars with current and maximum HP
- **Distance Filter** — Configurable max render distance (10m–1000m)

### 📦 Ground Item ESP & Direct3D 11 Icon Pipeline
- **Mirror Network Pickup Scanning** — Zero-allocation stationary item caching and bulk memory reads of Mirror's spawned dictionary (`NetworkClient.spawned`)
- **Direct3D 11 GPU Icon Rendering** — High-resolution transparent PNG item icons rendered directly via Direct3D 11 and ImGui
- **3 Flexible Display Styles**:
  - **Icon + Text** — Floating icon stacked above centered category-colored name label
  - **Icon Only** — Icon with clean distance badge
  - **Text Only** — Lightweight category-colored label
- **Icon Size Slider** — Adjustable icon scaling (16px to 48px)
- **Custom Display Names**:
  - Keycards suffix-free: `Janitor`, `Scientist`, `Guard`, `MTF Operative`, `MTF Captain`, `Facility Manager`, `Chaos Insurgency`, `O5`, etc.
  - Custom Weapons & SCPs: `Micro`, `127`, `Particle Disruptor`, `Pills`, `SCP Pills`, `BouncyBall`, `Cola`, `Anti Scp Cola`, `Hat`, `Candy`, `Ghostlight`, `Vase`, `Performance Enhancer`, `Gramophone`, `Goggles`, `Machete`, `Grenade`, `Flashbang`, `SurfaceAccess`
- **7 Independent Category Filters** — Granular menu toggles for `Keycards`, `Weapons`, `Ammo`, `Armor`, `Medical`, `SCP Items`, and `Utility`

### 🔍 Smart Inventory-Aware Culling
- **Real-Time Inventory Polling** — Scans local player inventory (`UserInventory.Items` & `ReserveAmmo`) every ~250ms with zero heap allocations
- **Max Ammo Capacity Culling** — Automatically hides ground ammo when your reserve ammo for that caliber is full, scaled dynamically by equipped armor tier:
  - 9mm: 30 (None) / 65 (Light) / 85 (Combat) / 100 (Heavy)
  - 5.56: 40 (None) / 80 (Light) / 105 (Combat) / 120 (Heavy)
  - 7.62: 40 (None) / 80 (Light) / 105 (Combat) / 120 (Heavy)
  - 12ga: 14 (None) / 28 (Light) / 36 (Combat) / 44 (Heavy)
  - .44: 18 (None) / 36 (Light) / 48 (Combat) / 56 (Heavy)
- **Best-in-Slot Armor Culling** — Carrying or wearing Heavy Armor hides all ground armor; Combat Armor hides Light Armor
- **Keycard Permission Hierarchy Matching** — Evaluates door permission bitmasks to cull inferior keycards when holding superior cards (e.g. O5 hides all ground cards; Guard hides Janitor)
- **Caliber Compatibility Filter** — Culls ground ammo boxes that do not match any firearm currently held in your inventory
- **Duplicate Item Culling** — Automatically hides redundant keycards and duplicate utility items (Radios, Flashlights)
- **Minimum Tier Sliders** — Configurable thresholds for keycards (Janitor to O5) and armor (Light to Heavy)

### 🗺️ World ESP
- **Room ESP** — Displays facility room names with distance, filtered by your current zone (LCZ / HCZ / EZ / Surface)
- **Generator ESP** — Shows all 3 SCP-079 generators with live status (Locked → Unlocked → Activating → Engaged) and countdown timers

### 💣 HUD Panel
- **Live Player Counts** — Authoritative counts of Alive and Dead players
- **Alpha Warhead Tracker** — Real-time detonation countdown with client-side interpolation
- **Generator Progress** — Live counter showing engaged generators (X/3)
- **Round Timer & Phase** — Authoritative round duration and match phase
- **Granular HUD Toggles** — Toggle individual HUD elements with dynamic background card auto-resizing
- **Interactive Repositioning** — Press `F7` to unlock and drag the HUD anywhere on screen

### ⚙️ Memory Writes
- **World Camera FOV** — Custom player camera FOV (60°–120°) via native Unity 6 camera memory writes with zero-flicker redirection
- **Hold-to-Zoom** — Dedicated zoom FOV (15°–60°) while holding a configurable keybind (default `Mouse1` / Right Click) with an interactive keybind picker
- **Viewmodel FOV** — Custom viewmodel FOV (30°–140°) with dynamic attachment compensation and auto-restoration
- **No Weapon Sway & Bobbing** — Eliminates weapon lag, breathing, and walking bobbing on active firearm models
- **Weapon Recoil Control** — Scales or eliminates vertical kick, horizontal kick, and viewmodel sway (0%–100% slider)
- **Auto-Bunnyhop** — Frame-perfect jump timing when grounded while holding Space

> All writes target heap data only and are strictly guarded against `.text`, `.rdata`, and executable sections. See the 📄 **[Memory Writes Deep Dive](docs/MEMORY_WRITES.md)** for architecture, pointer chains, and protection details.

### 🔧 Diagnostics & Menu
- **Dual-Column Modern Menu** — Clean layout with Player/World ESP on the left and Item ESP / Filtering on the right (`F1` / `Insert`)
- **Streamer Mode** — Hides the overlay from OBS, Discord, and screen capture software (`SetWindowDisplayAffinity`)
- **Input Debugger** — Real-time DMA key-state reader and event visualizer
- **F6 Diagnostics HUD** — Live DMA read timings, FPS, read counts, player/pickup counts, and inventory slot status

---

## Keybinds

| Key | Action |
|-----|--------|
| `F1` / `Insert` | Toggle settings menu |
| `F6` | Toggle diagnostics panel |
| `F7` | Toggle HUD dragging |
| `End` | Exit application |

---

## Architecture

The tool runs **4 dedicated DMA threads** for maximum performance:

| Thread | Purpose | Rate |
|--------|---------|------|
| **FastLoop** | Positions, camera, bones, viewmodel FOV | As fast as possible (~2–5ms) |
| **HealthLoop** | Player health values, round transition detector | ~15ms interval |
| **RoleLoop** | Role identification, player names, inventory scan | ~50–250ms interval |
| **DiscoveryLoop** | Player list, ground pickups, rooms, generators, warhead | ~2000ms interval |

All threads publish to a **lock-free atomic snapshot** consumed by the ImGui overlay renderer.

---

## Requirements

- **DMA Hardware** — FPGA-based PCIe DMA device (Squirrel, CaptainDMA, etc.)
- **Windows** — x64 only
- **.NET 10** — `net10.0-windows`
- **Two PCs** — Target PC running SCP:SL, second PC running this tool

---

## Building

```bash
dotnet build SCPSLDMA.sln -c Release -p:Platform=x64
```

Single-file output is automatically packaged into the `Built/` folder on Release builds alongside the required native libraries and `Icons/` directory.

---

## Project Structure

```
├── SCPSLDMA.sln             # Solution file
├── src/                     # Unified SCPSLDMA application
│   ├── scpsl.cs             # All game logic, ESP, overlay, offsets
│   ├── SCPSLDMA.csproj      # Executable project
│   ├── DMA/                 # Core DMA memory access, scatter read API
│   ├── Unity/               # Unity IL2CPP structures, transforms, bones
│   │   └── Icons/           # 60+ item icons deployed to output
│   └── Misc/                # Logging, timers, config, native interop
└── *.dll                    # Required native DLLs (VMM, LeechCore, etc.)
```

---

## Dependencies

- [VmmSharpEx](https://www.nuget.org/packages/VmmSharpEx) — MemProcFS bindings for DMA access
- [LeechCore](https://github.com/ufrisk/LeechCore) — Physical memory acquisition and PCIe FPGA hardware abstraction library
- [MemProcFS](https://github.com/ufrisk/MemProcFS) — Memory Process File System (`vmm.dll`)
- [ClickableTransparentOverlay](https://www.nuget.org/packages/ClickableTransparentOverlay) — ImGui overlay framework
- Strictly required native DLLs: `vmm.dll`, `leechcore.dll`, `leechcore_driver.dll`, `FTD3XX.dll`, `dbghelp.dll`, `symsrv.dll`, `tinylz4.dll`
