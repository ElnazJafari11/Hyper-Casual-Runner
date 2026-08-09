# Capability Map: Hyper-Casual Runner Toolkit

> Hypothesis map. Update from `docs/traces/` evidence at milestones or when
> Pro High issues fix lists on two consecutive tasks.

## Routing tiers

| Tier | Domains | Notes |
|------|---------|-------|
| **Pro High-dominant** | Planning, architecture, deep analysis, hard debugging, plan/receipt evaluation, criticism | Critical-lane tasks always run full Plan Contract |
| **Parity (Flash High solo)** | Routine implementation, refactors, test writing, tooling scripts | Misrouting fuse: if a task touches critical-lane, convert mid-flight |
| **Mechanical (Flash Base)** | Formatting, renames, boilerplate, log parsing, search/scrape, doc mirroring, test-data generation | Flash High reviews every diff before it lands |

## Critical-lane domains (seeded)

- DOTS ECS Systems (Simulation Group, SystemAPI Query, EntityCommandBuffer safety)
- Math Gate / Numerics logic
- Collision & Damage calculations

## Evidence log

| Date | Signal | Map change |
|------|--------|------------|
| 2026-08-09 | Session init — map created from `docs/project-context.md` | Initial seed |
