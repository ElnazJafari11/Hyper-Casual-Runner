# Project Context: Hyper-Casual Runner Toolkit

## Maturity Stage
Prototype / Toolkit Architecture phase. 
Building high-performance DOTS (Entities 1.0+) modular components, authoring scripts, systems, and UI Toolkit integration.

## Quality Bar
- Playable 60FPS DOTS runner mechanics across 21 core hyper-casual templates/slices.
- Clean separation: Authoring (MonoBehaviour) -> ECS Components (IComponentData) -> Systems (ISystem / SystemBase).
- UI Elements (UI Toolkit) for all game UI / Meta screens.
- Hybrid ECS for Audio and VFX presentation layer.

## Stub-Marker Convention
`// TODO: [STUB]` for non-critical temporary code or deferred items.

## Critical-Lane Domains
- DOTS ECS Systems (Simulation Group, SystemAPI Query, EntityCommandBuffer safety)
- Math Gate / Numerics logic
- Collision & Damage calculations
