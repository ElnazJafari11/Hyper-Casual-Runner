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

        private static string IdleKey(int archetype, string field) => $"HCR_Idle_{(int)archetype}_{field}";

        public static void SaveIdleSlice(
            int archetype,
            double primaryCurrency,
            double prestigeCurrency,
            float globalMultiplier,
            int progressionLevel,
            double clickPower,
            double passiveRate,
            int ownedGenerators)
        {
            PlayerPrefs.SetString(IdleKey(archetype, "Primary"), primaryCurrency.ToString("R"));
            PlayerPrefs.SetString(IdleKey(archetype, "Prestige"), prestigeCurrency.ToString("R"));
            PlayerPrefs.SetFloat(IdleKey(archetype, "Mult"), globalMultiplier);
            PlayerPrefs.SetInt(IdleKey(archetype, "Level"), progressionLevel);
            PlayerPrefs.SetString(IdleKey(archetype, "Click"), clickPower.ToString("R"));
            PlayerPrefs.SetString(IdleKey(archetype, "Passive"), passiveRate.ToString("R"));
            PlayerPrefs.SetInt(IdleKey(archetype, "Gens"), ownedGenerators);
            LastIdleUpdateTime = System.DateTime.UtcNow.ToString("O");
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
            primaryCurrency = 0;
            prestigeCurrency = 0;
            globalMultiplier = 1f;
            progressionLevel = 0;
            clickPower = 1;
            passiveRate = 0;
            ownedGenerators = 0;

            string primaryKey = IdleKey(archetype, "Primary");
            if (!PlayerPrefs.HasKey(primaryKey)) return false;

            double.TryParse(PlayerPrefs.GetString(primaryKey, "0"), out primaryCurrency);
            double.TryParse(PlayerPrefs.GetString(IdleKey(archetype, "Prestige"), "0"), out prestigeCurrency);
            globalMultiplier = PlayerPrefs.GetFloat(IdleKey(archetype, "Mult"), 1f);
            progressionLevel = PlayerPrefs.GetInt(IdleKey(archetype, "Level"), 0);
            double.TryParse(PlayerPrefs.GetString(IdleKey(archetype, "Click"), "1"), out clickPower);
            double.TryParse(PlayerPrefs.GetString(IdleKey(archetype, "Passive"), "0"), out passiveRate);
            ownedGenerators = PlayerPrefs.GetInt(IdleKey(archetype, "Gens"), 0);
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
            Save();
        }
    }
}
