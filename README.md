# Hybrid Cymatic Sim

Minimal Unity 2022.3 LTS cymatics sandbox with XR Interaction Toolkit + OpenXR support.

## Overview
- 2D wave solver driving a mesh tray
- Live parameter control via world‑space UI sliders
- Optional continuous excitation (frequency/amplitude)
- PNG and STL export shortcuts

## Setup
1) Open the project in Unity 2022.3 LTS.
2) Install packages:
   - XR Interaction Toolkit
   - OpenXR
3) Open the scene:
   - `Assets/Scenes/CymaticsSandbox.unity`
4) Press Play.

## Controls
- Space: impulse excite
- Sliders: depth, viscosity, damping, wave speed, light params, excite freq/amp
- P: export PNG to `Application.persistentDataPath`
- L: export STL to `Application.persistentDataPath`

## Notes
- If UI clicks don’t register, assign the Canvas Event Camera to the XR camera.
- `Assets/` + `Packages/` + `ProjectSettings/` are the intended commit set.
