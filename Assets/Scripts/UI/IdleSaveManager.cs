using UnityEngine;
using System;

namespace HyperCasualRunner
{
    public class IdleSaveManager : MonoBehaviour
    {
        private void SaveIdleTime()
        {
            GameProgressData.LastIdleUpdateTime = DateTime.UtcNow.ToString("O");
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus) SaveIdleTime();
        }

        private void OnApplicationQuit()
        {
            SaveIdleTime();
        }
    }
}
