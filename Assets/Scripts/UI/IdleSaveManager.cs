using UnityEngine;
using System;
using HyperCasualRunner.ECS.Authoring;

namespace HyperCasualRunner
{
    public class IdleSaveManager : MonoBehaviour
    {
        private IdleSliceBootstrap _bootstrap;

        private void Awake()
        {
            _bootstrap = GetComponent<IdleSliceBootstrap>();
        }

        private void SaveIdleTime()
        {
            GameProgressData.LastIdleUpdateTime = DateTime.UtcNow.ToString("O");
            if (_bootstrap != null) _bootstrap.PersistNow();
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
