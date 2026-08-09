## 2026-07-22T11:07:02Z
You are teamwork_preview_worker for Milestone 4 Remediation in Hyper-Casual Runner Toolkit.
Your working directory is d:\Git\Hyper-Casual-Runner\.agents\worker_m4_2. Create this directory if it does not exist.

MANDATORY INTEGRITY WARNING:
DO NOT CHEAT. All implementations must be genuine. DO NOT hardcode test results, create dummy/facade implementations, or circumvent the intended task. A Forensic Auditor will independently verify your work. Integrity violations WILL be detected and your work WILL be rejected.

REMEDIATION TASKS:
Fix the following issues identified in `Assets/Scripts/UI/UIManagerSystem.cs` and test files:
1. **Double Gold Reward Bug**:
   - In `UIManagerSystem.cs`, remove `GameProgressData.SaveLevelCompletion()` from `Update()` when `CurrentState == GameState.Victory`. Gold must ONLY be awarded/saved when `ClaimMultipliedGold()` (or `NextLevelBtn`) is pressed by the player, avoiding duplicate gold persistence writes.
2. **UI Callback Leak**:
   - In `UIManagerSystem.cs`, ensure `InitializeUI()` and `BindCallbacks()` do not double-register event handlers on `Awake()` and `OnEnable()`.
   - Use named handler methods for event subscriptions (e.g. `_upgradeSwarmBtn.clicked += OnUpgradeSwarmClicked;`) and unsubscribe them (`-=`) in `OnDisable()` / `OnDestroy()` to prevent callback accumulation on re-enable.
3. **Dynamic Star Rating Calculation**:
   - Calculate stars dynamically (e.g. 1 star for victory, 2 stars for saving >50% coins/swarm, 3 stars for saving >80% coins/swarm) instead of hardcoding `3` stars.
4. **Verification**:
   - Run the Unity EditMode test suite in batchmode to verify 100% passing tests:
     `Start-Process -FilePath "C:\Program Files\Unity\Hub\Editor\6000.3.20f1\Editor\Unity.exe" -ArgumentList "-batchmode -nographics -silent-crashes -projectPath d:\Git\Hyper-Casual-Runner -runTests -testPlatform EditMode -testResults d:\Git\Hyper-Casual-Runner\.agents\worker_m4_2\test_results.xml -logFile d:\Git\Hyper-Casual-Runner\.agents\worker_m4_2\unity_test.log" -Wait -NoNewWindow`

Write a detailed handoff report in `d:\Git\Hyper-Casual-Runner\.agents\worker_m4_2\handoff.md` detailing all fixes made, compilation results, and test suite execution logs. Send a message to parent when complete.
