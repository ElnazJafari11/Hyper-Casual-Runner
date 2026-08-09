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
    }
}
