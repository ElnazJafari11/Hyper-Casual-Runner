using System.Globalization;
using UnityEngine;

namespace HyperCasualRunner
{
    /// <summary>
    /// Cozy/assign extras (D18): workers, check-in cats, ADR wood/stoke.
    /// Pending/HasClaim/NarrStep/Explore/Soft live on SaveIdleSlice core params.
    /// </summary>
    public struct IdleSliceCozyPersist
    {
        public int AssignedWorkers;
        public int CheckInCats;
        public int NarrStokeCount;
        public double NarrWood;
    }

    /// <summary>
    /// Combat SoT (R4-F1): Zone/HP/DPS/Tap/GoldPerKill survive reload.
    /// Must not be rebuilt from Stage-inflated ProgressionLevel alone.
    /// </summary>
    public struct IdleCombatPersist
    {
        public int Zone;
        public float EnemyHp;
        public float EnemyMaxHp;
        public double HeroDps;
        public double TapDamage;
        public double GoldPerKill;
    }

    public static class GameProgressData
    {
        private const string GoldKey = "HCR_TotalGold";
        private const string SwarmLevelKey = "HCR_SwarmLevel";
        private const string IncomeLevelKey = "HCR_IncomeLevel";
        private const string LevelIndexKey = "HCR_CurrentLevelIndex";
        private const string UnlockedLevelKey = "HCR_UnlockedLevelIndex";

        public static int TotalGold
        {
            get => PlayerPrefs.GetInt(GoldKey, 0);
            set { PlayerPrefs.SetInt(GoldKey, value); Save(); }
        }

        public static int SwarmLevel
        {
            get => PlayerPrefs.GetInt(SwarmLevelKey, 1);
            set { PlayerPrefs.SetInt(SwarmLevelKey, value); Save(); }
        }

        public static int IncomeLevel
        {
            get => PlayerPrefs.GetInt(IncomeLevelKey, 1);
            set { PlayerPrefs.SetInt(IncomeLevelKey, value); Save(); }
        }

        public static int CurrentLevelIndex
        {
            get => PlayerPrefs.GetInt(LevelIndexKey, 0);
            set { PlayerPrefs.SetInt(LevelIndexKey, value); Save(); }
        }

        public static int UnlockedLevelIndex
        {
            get => PlayerPrefs.GetInt(UnlockedLevelKey, 0);
            set { PlayerPrefs.SetInt(UnlockedLevelKey, value); Save(); }
        }

        public static int GetLevelStars(int levelIndex)
        {
            return PlayerPrefs.GetInt($"HCR_LevelStars_{levelIndex}", 0);
        }

        public static void SetLevelStars(int levelIndex, int stars)
        {
            int clamped = Mathf.Clamp(stars, 0, 3);
            PlayerPrefs.SetInt($"HCR_LevelStars_{levelIndex}", clamped);
            Save();
        }

        public static void SaveLevelCompletion(int completedLevelIndex, int earnedCoins, int stars)
        {
            TotalGold += earnedCoins * IncomeLevel;
            int currentStars = GetLevelStars(completedLevelIndex);
            if (stars > currentStars)
            {
                SetLevelStars(completedLevelIndex, stars);
            }
            if (completedLevelIndex + 1 > UnlockedLevelIndex)
            {
                UnlockedLevelIndex = completedLevelIndex + 1;
            }
            Save();
        }

        public static void Save()
        {
            PlayerPrefs.Save();
        }

        public static int CurrentSkinIndex
        {
            get => PlayerPrefs.GetInt("HCR_SkinIndex", 0);
            set { PlayerPrefs.SetInt("HCR_SkinIndex", value); Save(); }
        }

        public static int UnlockedSkins
        {
            get => PlayerPrefs.GetInt("HCR_UnlockedSkins", 1); // 1 = bit 0 is set (default skin unlocked)
            set { PlayerPrefs.SetInt("HCR_UnlockedSkins", value); Save(); }
        }

        public static bool IsSkinUnlocked(int index)
        {
            return (UnlockedSkins & (1 << index)) != 0;
        }

        public static void UnlockSkin(int index)
        {
            UnlockedSkins |= (1 << index);
        }

        public static string LastLoginDate
        {
            get => PlayerPrefs.GetString("HCR_LastLoginDate", "");
            set { PlayerPrefs.SetString("HCR_LastLoginDate", value); Save(); }
        }

        public static string LastIdleUpdateTime
        {
            get => PlayerPrefs.GetString("HCR_LastIdleTime", "");
            set { PlayerPrefs.SetString("HCR_LastIdleTime", value); Save(); }
        }

        public static int LoginStreak
        {
            get => PlayerPrefs.GetInt("HCR_LoginStreak", 0);
            set { PlayerPrefs.SetInt("HCR_LoginStreak", value); Save(); }
        }

        public static int CheckDailyReward()
        {
            string lastLoginStr = LastLoginDate;
            if (string.IsNullOrEmpty(lastLoginStr)) return 100;

            if (System.DateTime.TryParse(lastLoginStr, out System.DateTime lastLogin))
            {
                if (System.DateTime.Now.Date > lastLogin.Date)
                {
                    if ((System.DateTime.Now.Date - lastLogin.Date).TotalDays > 1)
                    {
                        LoginStreak = 0; // Streak broken
                    }
                    return 100 + (LoginStreak * 50);
                }
            }
            return 0;
        }

        public static void ClaimDailyReward()
        {
            int reward = CheckDailyReward();
            if (reward > 0)
            {
                TotalGold += reward;
                LoginStreak++;
                LastLoginDate = System.DateTime.Now.ToString();
            }
        }

        public static int GetUpgradeCost(int currentLevel)
        {
            return currentLevel * 50; // simple linear scaling for MVP
        }

        // --- Idle toolkit slice persistence (lightweight PlayerPrefs) ---

        private static string IdleKey(int archetype, string field) => $"HCR_Idle_{archetype}_{field}";

        private static bool TryParseInvariantDouble(string raw, out double value) =>
            double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out value);

        public static void SaveIdleSlice(
            int archetype,
            double primaryCurrency,
            double prestigeCurrency,
            float globalMultiplier,
            int progressionLevel,
            double clickPower,
            double passiveRate,
            int ownedGenerators,
            int managersHired = 0,
            int phaseIndex = 0,
            int factionId = 0,
            float energyAllocated = 0f,
            bool managerIsHired = false,
            float energyPool = 50f,
            double pendingClaim = 0,
            bool hasOfflineClaim = false,
            int gachaStage = 0,
            int gachaPullCount = 0,
            int gachaBestRarity = 0,
            int narrRoomOrStep = 0,
            int narrExploreUnlocked = 0,
            double narrSoftCurrency = 0,
            float afkChestSeconds = 0f,
            int assignedWorkers = 0,
            int checkInCats = 0,
            int narrStokeCount = 0,
            double narrWood = 0)
        {
            PlayerPrefs.SetString(IdleKey(archetype, "Primary"), primaryCurrency.ToString("R", CultureInfo.InvariantCulture));
            PlayerPrefs.SetString(IdleKey(archetype, "Prestige"), prestigeCurrency.ToString("R", CultureInfo.InvariantCulture));
            PlayerPrefs.SetFloat(IdleKey(archetype, "Mult"), globalMultiplier);
            PlayerPrefs.SetInt(IdleKey(archetype, "Level"), progressionLevel);
            PlayerPrefs.SetString(IdleKey(archetype, "Click"), clickPower.ToString("R", CultureInfo.InvariantCulture));
            PlayerPrefs.SetString(IdleKey(archetype, "Passive"), passiveRate.ToString("R", CultureInfo.InvariantCulture));
            PlayerPrefs.SetInt(IdleKey(archetype, "Gens"), ownedGenerators);
            PlayerPrefs.SetInt(IdleKey(archetype, "Managers"), managersHired);
            PlayerPrefs.SetInt(IdleKey(archetype, "Phase"), phaseIndex);
            PlayerPrefs.SetInt(IdleKey(archetype, "Faction"), factionId);
            PlayerPrefs.SetString(IdleKey(archetype, "Energy"), energyAllocated.ToString("R", CultureInfo.InvariantCulture));
            PlayerPrefs.SetString(IdleKey(archetype, "EnergyPool"), energyPool.ToString("R", CultureInfo.InvariantCulture));
            PlayerPrefs.SetInt(IdleKey(archetype, "MgrHired"), managerIsHired ? 1 : 0);
            // Melvor (and Fallout) claim-bank: must survive PersistNow / quit after catch-up stamps time.
            PlayerPrefs.SetString(IdleKey(archetype, "Pending"), pendingClaim.ToString("R", CultureInfo.InvariantCulture));
            PlayerPrefs.SetInt(IdleKey(archetype, "HasClaim"), hasOfflineClaim ? 1 : 0);
            // Gacha / narrative / AFK extras (IH Stage+chest, LoM Stage, Capybara Explore).
            PlayerPrefs.SetInt(IdleKey(archetype, "GachaStage"), gachaStage);
            PlayerPrefs.SetInt(IdleKey(archetype, "GachaPulls"), gachaPullCount);
            PlayerPrefs.SetInt(IdleKey(archetype, "GachaRarity"), gachaBestRarity);
            PlayerPrefs.SetInt(IdleKey(archetype, "NarrStep"), narrRoomOrStep);
            PlayerPrefs.SetInt(IdleKey(archetype, "NarrExplore"), narrExploreUnlocked);
            PlayerPrefs.SetString(IdleKey(archetype, "NarrSoft"), narrSoftCurrency.ToString("R", CultureInfo.InvariantCulture));
            PlayerPrefs.SetFloat(IdleKey(archetype, "AfkChest"), afkChestSeconds);
            // Cozy assign / ADR wood-stoke / Neko cats (D18 session honesty).
            PlayerPrefs.SetInt(IdleKey(archetype, "Workers"), assignedWorkers);
            PlayerPrefs.SetInt(IdleKey(archetype, "Cats"), checkInCats);
            PlayerPrefs.SetInt(IdleKey(archetype, "NarrStoke"), narrStokeCount);
            PlayerPrefs.SetString(IdleKey(archetype, "NarrWood"), narrWood.ToString("R", CultureInfo.InvariantCulture));
            LastIdleUpdateTime = System.DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture);
            Save();
        }

        /// <summary>Load cozy-only extras. Missing keys → zeros (safe for pre-D18 saves).</summary>
        public static IdleSliceCozyPersist LoadIdleCozyPersist(int archetype)
        {
            var cozy = new IdleSliceCozyPersist
            {
                AssignedWorkers = PlayerPrefs.GetInt(IdleKey(archetype, "Workers"), 0),
                CheckInCats = PlayerPrefs.GetInt(IdleKey(archetype, "Cats"), 0),
                NarrStokeCount = PlayerPrefs.GetInt(IdleKey(archetype, "NarrStoke"), 0)
            };
            TryParseInvariantDouble(PlayerPrefs.GetString(IdleKey(archetype, "NarrWood"), "0"), out var wood);
            cozy.NarrWood = wood;
            return cozy;
        }

        /// <summary>Persist IdleCombatState SoT. Only call when the slice actually has combat.</summary>
        public static void SaveIdleCombatPersist(int archetype, in IdleCombatPersist combat)
        {
            PlayerPrefs.SetInt(IdleKey(archetype, "CombatZone"), combat.Zone);
            PlayerPrefs.SetFloat(IdleKey(archetype, "CombatEnemyHp"), combat.EnemyHp);
            PlayerPrefs.SetFloat(IdleKey(archetype, "CombatEnemyMaxHp"), combat.EnemyMaxHp);
            PlayerPrefs.SetString(IdleKey(archetype, "CombatHeroDps"),
                combat.HeroDps.ToString("R", CultureInfo.InvariantCulture));
            PlayerPrefs.SetString(IdleKey(archetype, "CombatTapDmg"),
                combat.TapDamage.ToString("R", CultureInfo.InvariantCulture));
            PlayerPrefs.SetString(IdleKey(archetype, "CombatGoldKill"),
                combat.GoldPerKill.ToString("R", CultureInfo.InvariantCulture));
            Save();
        }

        /// <summary>
        /// Load combat SoT. Missing CombatZone key → false (cold start / pre-R4 saves).
        /// Caller must not invent Zone from ProgressionLevel when this returns true.
        /// </summary>
        public static bool TryLoadIdleCombatPersist(int archetype, out IdleCombatPersist combat)
        {
            combat = default;
            string zoneKey = IdleKey(archetype, "CombatZone");
            if (!PlayerPrefs.HasKey(zoneKey)) return false;

            combat.Zone = PlayerPrefs.GetInt(zoneKey, 1);
            combat.EnemyHp = PlayerPrefs.GetFloat(IdleKey(archetype, "CombatEnemyHp"), 0f);
            combat.EnemyMaxHp = PlayerPrefs.GetFloat(IdleKey(archetype, "CombatEnemyMaxHp"), 0f);
            TryParseInvariantDouble(PlayerPrefs.GetString(IdleKey(archetype, "CombatHeroDps"), "0"), out combat.HeroDps);
            TryParseInvariantDouble(PlayerPrefs.GetString(IdleKey(archetype, "CombatTapDmg"), "0"), out combat.TapDamage);
            TryParseInvariantDouble(PlayerPrefs.GetString(IdleKey(archetype, "CombatGoldKill"), "0"), out combat.GoldPerKill);
            return true;
        }

        public static bool TryLoadIdleSlice(
            int archetype,
            out double primaryCurrency,
            out double prestigeCurrency,
            out float globalMultiplier,
            out int progressionLevel,
            out double clickPower,
            out double passiveRate,
            out int ownedGenerators)
        {
            return TryLoadIdleSlice(
                archetype,
                out primaryCurrency,
                out prestigeCurrency,
                out globalMultiplier,
                out progressionLevel,
                out clickPower,
                out passiveRate,
                out ownedGenerators,
                out _,
                out _,
                out _,
                out _,
                out _,
                out _,
                out _,
                out _);
        }

        public static bool TryLoadIdleSlice(
            int archetype,
            out double primaryCurrency,
            out double prestigeCurrency,
            out float globalMultiplier,
            out int progressionLevel,
            out double clickPower,
            out double passiveRate,
            out int ownedGenerators,
            out int managersHired,
            out int phaseIndex,
            out int factionId,
            out float energyAllocated,
            out bool managerIsHired)
        {
            return TryLoadIdleSlice(
                archetype,
                out primaryCurrency,
                out prestigeCurrency,
                out globalMultiplier,
                out progressionLevel,
                out clickPower,
                out passiveRate,
                out ownedGenerators,
                out managersHired,
                out phaseIndex,
                out factionId,
                out energyAllocated,
                out managerIsHired,
                out _,
                out _,
                out _);
        }

        public static bool TryLoadIdleSlice(
            int archetype,
            out double primaryCurrency,
            out double prestigeCurrency,
            out float globalMultiplier,
            out int progressionLevel,
            out double clickPower,
            out double passiveRate,
            out int ownedGenerators,
            out int managersHired,
            out int phaseIndex,
            out int factionId,
            out float energyAllocated,
            out bool managerIsHired,
            out float energyPool)
        {
            return TryLoadIdleSlice(
                archetype,
                out primaryCurrency,
                out prestigeCurrency,
                out globalMultiplier,
                out progressionLevel,
                out clickPower,
                out passiveRate,
                out ownedGenerators,
                out managersHired,
                out phaseIndex,
                out factionId,
                out energyAllocated,
                out managerIsHired,
                out energyPool,
                out _,
                out _);
        }

        public static bool TryLoadIdleSlice(
            int archetype,
            out double primaryCurrency,
            out double prestigeCurrency,
            out float globalMultiplier,
            out int progressionLevel,
            out double clickPower,
            out double passiveRate,
            out int ownedGenerators,
            out int managersHired,
            out int phaseIndex,
            out int factionId,
            out float energyAllocated,
            out bool managerIsHired,
            out float energyPool,
            out double pendingClaim,
            out bool hasOfflineClaim)
        {
            return TryLoadIdleSlice(
                archetype,
                out primaryCurrency,
                out prestigeCurrency,
                out globalMultiplier,
                out progressionLevel,
                out clickPower,
                out passiveRate,
                out ownedGenerators,
                out managersHired,
                out phaseIndex,
                out factionId,
                out energyAllocated,
                out managerIsHired,
                out energyPool,
                out pendingClaim,
                out hasOfflineClaim,
                out _,
                out _,
                out _,
                out _,
                out _,
                out _,
                out _);
        }

        public static bool TryLoadIdleSlice(
            int archetype,
            out double primaryCurrency,
            out double prestigeCurrency,
            out float globalMultiplier,
            out int progressionLevel,
            out double clickPower,
            out double passiveRate,
            out int ownedGenerators,
            out int managersHired,
            out int phaseIndex,
            out int factionId,
            out float energyAllocated,
            out bool managerIsHired,
            out float energyPool,
            out double pendingClaim,
            out bool hasOfflineClaim,
            out int gachaStage,
            out int gachaPullCount,
            out int gachaBestRarity,
            out int narrRoomOrStep,
            out int narrExploreUnlocked,
            out double narrSoftCurrency,
            out float afkChestSeconds)
        {
            primaryCurrency = 0;
            prestigeCurrency = 0;
            globalMultiplier = 1f;
            progressionLevel = 0;
            clickPower = 1;
            passiveRate = 0;
            ownedGenerators = 0;
            managersHired = 0;
            phaseIndex = 0;
            factionId = 0;
            energyAllocated = 0f;
            managerIsHired = false;
            energyPool = 50f;
            pendingClaim = 0;
            hasOfflineClaim = false;
            gachaStage = 0;
            gachaPullCount = 0;
            gachaBestRarity = 0;
            narrRoomOrStep = 0;
            narrExploreUnlocked = 0;
            narrSoftCurrency = 0;
            afkChestSeconds = 0f;

            string primaryKey = IdleKey(archetype, "Primary");
            if (!PlayerPrefs.HasKey(primaryKey)) return false;

            TryParseInvariantDouble(PlayerPrefs.GetString(primaryKey, "0"), out primaryCurrency);
            TryParseInvariantDouble(PlayerPrefs.GetString(IdleKey(archetype, "Prestige"), "0"), out prestigeCurrency);
            globalMultiplier = PlayerPrefs.GetFloat(IdleKey(archetype, "Mult"), 1f);
            progressionLevel = PlayerPrefs.GetInt(IdleKey(archetype, "Level"), 0);
            TryParseInvariantDouble(PlayerPrefs.GetString(IdleKey(archetype, "Click"), "1"), out clickPower);
            TryParseInvariantDouble(PlayerPrefs.GetString(IdleKey(archetype, "Passive"), "0"), out passiveRate);
            ownedGenerators = PlayerPrefs.GetInt(IdleKey(archetype, "Gens"), 0);
            managersHired = PlayerPrefs.GetInt(IdleKey(archetype, "Managers"), 0);
            phaseIndex = PlayerPrefs.GetInt(IdleKey(archetype, "Phase"), 0);
            factionId = PlayerPrefs.GetInt(IdleKey(archetype, "Faction"), 0);
            TryParseInvariantDouble(PlayerPrefs.GetString(IdleKey(archetype, "Energy"), "0"), out var energy);
            energyAllocated = (float)energy;
            string poolKey = IdleKey(archetype, "EnergyPool");
            if (PlayerPrefs.HasKey(poolKey) &&
                TryParseInvariantDouble(PlayerPrefs.GetString(poolKey, "50"), out var pool))
            {
                energyPool = (float)pool;
            }
            managerIsHired = PlayerPrefs.GetInt(IdleKey(archetype, "MgrHired"), 0) != 0;
            TryParseInvariantDouble(PlayerPrefs.GetString(IdleKey(archetype, "Pending"), "0"), out pendingClaim);
            hasOfflineClaim = PlayerPrefs.GetInt(IdleKey(archetype, "HasClaim"), 0) != 0
                              || pendingClaim > 0.5;
            gachaStage = PlayerPrefs.GetInt(IdleKey(archetype, "GachaStage"), 0);
            gachaPullCount = PlayerPrefs.GetInt(IdleKey(archetype, "GachaPulls"), 0);
            gachaBestRarity = PlayerPrefs.GetInt(IdleKey(archetype, "GachaRarity"), 0);
            narrRoomOrStep = PlayerPrefs.GetInt(IdleKey(archetype, "NarrStep"), 0);
            narrExploreUnlocked = PlayerPrefs.GetInt(IdleKey(archetype, "NarrExplore"), 0);
            TryParseInvariantDouble(PlayerPrefs.GetString(IdleKey(archetype, "NarrSoft"), "0"), out narrSoftCurrency);
            afkChestSeconds = PlayerPrefs.GetFloat(IdleKey(archetype, "AfkChest"), 0f);
            return true;
        }

        public static void ClearIdleSlice(int archetype)
        {
            PlayerPrefs.DeleteKey(IdleKey(archetype, "Primary"));
            PlayerPrefs.DeleteKey(IdleKey(archetype, "Prestige"));
            PlayerPrefs.DeleteKey(IdleKey(archetype, "Mult"));
            PlayerPrefs.DeleteKey(IdleKey(archetype, "Level"));
            PlayerPrefs.DeleteKey(IdleKey(archetype, "Click"));
            PlayerPrefs.DeleteKey(IdleKey(archetype, "Passive"));
            PlayerPrefs.DeleteKey(IdleKey(archetype, "Gens"));
            PlayerPrefs.DeleteKey(IdleKey(archetype, "Managers"));
            PlayerPrefs.DeleteKey(IdleKey(archetype, "Phase"));
            PlayerPrefs.DeleteKey(IdleKey(archetype, "Faction"));
            PlayerPrefs.DeleteKey(IdleKey(archetype, "Energy"));
            PlayerPrefs.DeleteKey(IdleKey(archetype, "EnergyPool"));
            PlayerPrefs.DeleteKey(IdleKey(archetype, "MgrHired"));
            PlayerPrefs.DeleteKey(IdleKey(archetype, "Pending"));
            PlayerPrefs.DeleteKey(IdleKey(archetype, "HasClaim"));
            PlayerPrefs.DeleteKey(IdleKey(archetype, "GachaStage"));
            PlayerPrefs.DeleteKey(IdleKey(archetype, "GachaPulls"));
            PlayerPrefs.DeleteKey(IdleKey(archetype, "GachaRarity"));
            PlayerPrefs.DeleteKey(IdleKey(archetype, "NarrStep"));
            PlayerPrefs.DeleteKey(IdleKey(archetype, "NarrExplore"));
            PlayerPrefs.DeleteKey(IdleKey(archetype, "NarrSoft"));
            PlayerPrefs.DeleteKey(IdleKey(archetype, "AfkChest"));
            PlayerPrefs.DeleteKey(IdleKey(archetype, "Workers"));
            PlayerPrefs.DeleteKey(IdleKey(archetype, "Cats"));
            PlayerPrefs.DeleteKey(IdleKey(archetype, "NarrStoke"));
            PlayerPrefs.DeleteKey(IdleKey(archetype, "NarrWood"));
            PlayerPrefs.DeleteKey(IdleKey(archetype, "CombatZone"));
            PlayerPrefs.DeleteKey(IdleKey(archetype, "CombatEnemyHp"));
            PlayerPrefs.DeleteKey(IdleKey(archetype, "CombatEnemyMaxHp"));
            PlayerPrefs.DeleteKey(IdleKey(archetype, "CombatHeroDps"));
            PlayerPrefs.DeleteKey(IdleKey(archetype, "CombatTapDmg"));
            PlayerPrefs.DeleteKey(IdleKey(archetype, "CombatGoldKill"));
            Save();
        }
    }
}
