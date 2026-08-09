# Round 01 Implement 08 — UI Toolkit (Idle)

**Agent:** implement 8/10  
**Source review:** `review_08_ui.md`  
**Architecture:** UI Toolkit only  
**Commit:** `b784bbf` local only (no push)

---

## Decision (UI-01)

**Option B:** Keep `IdleSliceUIController` as the sole live idle-slice HUD.  
Fold cosmetics (Actions / Cosmetics tabs + prestige-priced skins 0–3) into that controller.  
Mark `IdleGameHUD.uxml` + `IdleUIManagerSystem` as **deferred sandbox** path, gated by explicit `IdleGameHudBinder`.

---

## ASSUMPTIONS

1. Idle prefabs already ship `UIDocument` + `IdleSliceUIController` with empty VisualTree — confidence: **high** — verified by review inventory / prefab grep.
2. `CosmeticsShopSystem` consumes `CosmeticPurchaseEventComponent` and updates `GameProgressData.CurrentSkinIndex` — confidence: **high** — read system + tests.
3. Hyper-Casual-Runner Unity editor not connected to MCP this session — confidence: **high** — doctor pinned `thepcgtoolkit` only.

---

## Changes

| File | Change |
| --- | --- |
| `Assets/Scripts/UI/IdleSliceUIController.cs` | Actions/Cosmetics tabs; skin buy/equip → `CosmeticPurchaseEventComponent`; stub markers on inline styles |
| `Assets/Scripts/UI/IdleGameHudBinder.cs` | **New** — explicit `UIDocument` authoring; `Active` static registry |
| `Assets/Scripts/UI/IdleUIManagerSystem.cs` | Remove `FindObjectOfType<UIDocument>`; bind via `IdleGameHudBinder.Active`; cache Gold/Prestige labels; `PresentationSystemGroup`; restore locked-skin button color |
| `Assets/UI/IdleGameHUD.uxml` | Deferred banner comment; link `IdleGameHUD.uss`; class-based layout |
| `Assets/UI/IdleGameHUD.uss` | **New** — extracted styles |
| `Assets/Scripts/UI/LevelSelectScreenController.cs` | Explicit runner-only policy + `// TODO: [STUB]` idle browser |

---

## Acceptance criteria

| ID | Criterion | Result | Evidence |
| --- | --- | --- | --- |
| UI-01 | Single idle HUD authority | **PASS** | Live = `IdleSliceUIController`; `IdleGameHUD` marked deferred + binder-gated |
| UI-02 | Cosmetics reachable in play | **PASS (static)** | Cosmetics tab + skin buttons in `IdleSliceUIController`; fires `CosmeticPurchaseEventComponent` |
| UI-03 | No FindObjectOfType UI bind | **PASS** | Grep `FindObjectOfType*<UIDocument>` in `Assets/Scripts` → no matches; binder uses `IdleGameHudBinder.Active` |
| UI-04 | Toolkit-only idle path | **PASS** | No `UnityEngine.UI` in idle UI scripts |
| UI-05 | Styles extracted or stubbed | **PASS** | `IdleGameHUD.uss` linked; `IdleSliceUIController` has 5× `// TODO: [STUB]` inline style markers |
| UI-06 | LevelSelect idle policy explicit | **PASS** | Class summary + `_totalLevels` comment + STUB for idle browser |
| UI-07 | Label queries cached | **PASS** | Gold/Prestige `Q` only in `BindOnce`; slice stats use cached `_stats` |

---

## Verification

- Static greps for UI-03/04/05/07: **PASS** (see above).
- Unity compile / Play Mode cosmetics smoke: **UNVERIFIED** — no Hyper-Casual-Runner editor registered with MCP (`doctor` → `thepcgtoolkit` only).
- Risks: Play Mode needed to confirm Cosmetics tab visibility and `CurrentSkinIndex` after buy on an idle slice prefab.

**To verify in editor:**
1. Open any `Assets/ToolkitExamples/Idle/*_Slice.prefab`, Enter Play.
2. Confirm Actions + Cosmetics tabs; open Cosmetics; buy/equip skin → `GameProgressData.CurrentSkinIndex` updates.
3. Confirm console clean (no UIDocument bind errors).

---

## STATUS

**UNVERIFIED** (static criteria met; Play Mode not run).  
P0 cohesion gaps from review addressed in code.
