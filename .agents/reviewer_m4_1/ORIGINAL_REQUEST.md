## 2026-07-22T08:02:59Z
You are teamwork_preview_reviewer for Milestone 4: Multi-Level Progression Loader & UI Integration in Hyper-Casual Runner Toolkit.
Your working directory is d:\Git\Hyper-Casual-Runner\.agents\reviewer_m4_1. Create this directory if it does not exist.

TASK SCOPE:
Perform an architecture, code quality, and compliance review of Milestone 4.
Inspect created and modified files:
- `Assets/Scripts/ECS/Components/LevelProgressionComponents.cs`
- `Assets/Scripts/ECS/Authoring/LevelSequenceAuthoring.cs`
- `Assets/Scripts/ECS/Systems/LevelProgressionSystem.cs`
- `Assets/Scripts/ECS/Systems/MetaProgressionSaveSystem.cs`
- `Assets/Scripts/GameProgressData.cs`
- `Assets/Scripts/UI/UIManagerSystem.cs`
- `Assets/UI/` UXML and USS files (`GameHUD`, `LevelSelect`, `LevelCardItem`, `VictoryScreen`, `DefeatScreen`)

Verify:
1. Pure DOTS level sequence system and prefab buffer capacity (21 prefabs).
2. Clean slice entity teardown (`SliceEntityTag` & `LinkedEntityGroup` destruction).
3. 100% UI Toolkit compliance (zero legacy uGUI / Canvas references).
4. Persistence integrity via `GameProgressData.cs`.
5. Run the Unity EditMode test suite to confirm 16/16 tests pass:
   Command: `Start-Process -FilePath "C:\Program Files\Unity\Hub\Editor\6000.3.20f1\Editor\Unity.exe" -ArgumentList "-batchmode -nographics -silent-crashes -projectPath d:\Git\Hyper-Casual-Runner -runTests -testPlatform EditMode -testResults d:\Git\Hyper-Casual-Runner\.agents\reviewer_m4_1\test_results.xml -logFile d:\Git\Hyper-Casual-Runner\.agents\reviewer_m4_1\unity_test.log" -Wait -NoNewWindow`

Write a detailed handoff report in `d:\Git\Hyper-Casual-Runner\.agents\reviewer_m4_1\handoff.md` with your verdict (APPROVED or REJECTED) and evidence chain. Send a message to parent when complete.
