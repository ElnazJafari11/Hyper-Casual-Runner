using System.Globalization;
using UnityEngine;

namespace HyperCasualRunner
{
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
            float energyPool = 50f)
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
            LastIdleUpdateTime = System.DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture);
            Save();
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
            Save();
        }
    }
}
