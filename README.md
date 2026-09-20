# SCPSL DMA

Latest additons - added no fog for distance + the 2 scp items, No flash (for flashbangs), fullbright

---

## Features

### 🎯 Player ESP
- **Bounding Boxes**
- **Skeletons**
- **Instant Bone Acquisition** (should work most the time, some people have no bones + scps dont)
- **Name Tags** player names
- **Health Bars** (semi broken)
- **Distance Filter** — Configurable max render distance (10m–1000m)

### 📦 Ground Item ESP & Direct3D 11 Icon Pipeline
- **Mirror Network Pickup Scanning** — Zero-allocation stationary item caching and bulk memory reads of Mirror's spawned dictionary (`NetworkClient.spawned`)
- **Direct3D 11 GPU Icon Rendering** — High-resolution transparent PNG item icons rendered directly via Direct3D 11 and ImGui
- **3 Flexible Display Styles**:
  - **Icon + Text** — Floating icon stacked above centered category-colored name label
  - **Icon Only** — Icon with clean distance badge
  - **Text Only** — Lightweight category-colored label
- **Icon Size Slider** — Adjustable icon scaling (16px to 48px)
- **Custom Display Names**: for items

- **7 Independent Category Filters** — Granular menu toggles for `Keycards`, `Weapons`, `Ammo`, `Armor`, `Medical`, `SCP Items`, and `Utility`

### 🔍 Smart Inventory-Aware Culling
- **Real-Time Inventory Polling** — Scans local player inventory (`UserInventory.Items` & `ReserveAmmo`) every ~250ms with zero heap allocations
- **Max Ammo Capacity Culling** — Automatically hides ground ammo when your reserve ammo for that caliber is full, scaled dynamically by equipped armor tier
- **Best-in-Slot Armor Culling** — Carrying or wearing Heavy Armor hides all ground armor; Combat Armor hides Light Armor
- **Keycard Permission Hierarchy Matching** — Evaluates door permission bitmasks to cull inferior keycards when holding superior cards (e.g. O5 hides all ground cards; Guard hides Janitor)
- **Caliber Compatibility Filter** — Culls ground ammo boxes that do not match any firearm currently held in your inventory
- **Duplicate Item Culling** — Automatically hides redundant keycards and duplicate utility items (Radios, Flashlights)
- **Minimum Tier Sliders** — Configurable thresholds for keycards (Janitor to O5) and armor (Light to Heavy)

### 🗺️ World ESP
- **Room ESP** — Displays facility room names with distance, filtered by your current zone (LCZ / HCZ / EZ / Surface)
- **Generator ESP** — Shows all 3 SCP-079 generators with live status (Locked → Unlocked → Activating → Engaged) and countdown timers

### 💣 HUD Panel
- **Live Player Counts** 
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
- **no fog** for scp items + distance fog
- **no flash from flashbangs**
- **fullbright** (pretty much just brightness options in write form)

**FLASHLIGHT MODS** 
- Beam angle - how wide flashlight is
- brightness mult - self explanatory
- throw distance - how far the light goes


### 🔧 Diagnostics & Menu
- **Dual-Column Modern Menu** — Clean layout with Player/World ESP on the left and Item ESP / Filtering on the right (`F1` / `Insert`)
- **Streamer Mode** — Hides the overlay from OBS, Discord, and screen capture software (`SetWindowDisplayAffinity`)
- **Input Debugger** — Real-time DMA key-state reader and event visualizer
- **F6 Diagnostics HUD**

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


## Building

```bash
dotnet build SCPSLDMA.sln -c Release -p:Platform=x64
```
