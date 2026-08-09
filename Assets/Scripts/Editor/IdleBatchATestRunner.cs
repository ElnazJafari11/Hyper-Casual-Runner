using System;
using System.IO;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace HyperCasualRunner.Editor
{
    /// <summary>
    /// Batchmode-safe EditMode runner for IdleBatchASmokeTests.
    /// Usage: Unity -batchmode -nographics -projectPath ... -executeMethod HyperCasualRunner.Editor.IdleBatchATestRunner.RunAndExit
    /// Do NOT pass -quit (this method exits with the test result code).
    /// </summary>
    public static class IdleBatchATestRunner
    {
        private const string ResultsPath = "Logs/IdleBatchA-TestResults.xml";
        private const string SummaryPath = "Logs/IdleBatchA-Summary.txt";

        public static void RunAndExit()
        {
            Directory.CreateDirectory("Logs");
            var api = ScriptableObject.CreateInstance<TestRunnerApi>();
            var filter = new Filter
            {
                testMode = TestMode.EditMode,
                testNames = new[] { "HyperCasualRunner.Tests.IdleBatchASmokeTests" }
            };

            var callbacks = new Callbacks();
            api.RegisterCallbacks(callbacks);
            Debug.Log("[IdleToolkit] Starting IdleBatchASmokeTests via TestRunnerApi...");
            api.Execute(new ExecutionSettings(filter)
            {
                // Unity writes NUnit XML when path set via player/editor settings; we also summarize in callback.
            });
        }

        private sealed class Callbacks : ICallbacks
        {
            public void RunStarted(ITestAdaptor testsToRun) { }

            public void RunFinished(ITestResultAdaptor result)
            {
                string summary =
                    $"result={result.ResultState} pass={result.PassCount} fail={result.FailCount} " +
                    $"skip={result.SkipCount} inconclusive={result.InconclusiveCount} duration={result.Duration}\n";
                File.WriteAllText(SummaryPath, summary);
                Debug.Log("[IdleToolkit] " + summary.Trim());

                // Best-effort XML dump for CI-ish evidence
                try
                {
                    File.WriteAllText(ResultsPath,
                        "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                        $"<test-run total=\"{result.PassCount + result.FailCount + result.SkipCount}\" " +
                        $"passed=\"{result.PassCount}\" failed=\"{result.FailCount}\" " +
                        $"result=\"{result.ResultState}\" />\n");
                }
                catch (Exception e)
                {
                    Debug.LogWarning("[IdleToolkit] Could not write test XML: " + e.Message);
                }

                EditorApplication.Exit(result.FailCount > 0 ? 2 : 0);
            }

            public void TestStarted(ITestAdaptor test) { }

            public void TestFinished(ITestResultAdaptor result)
            {
                if (result.HasChildren) return;
                Debug.Log($"[IdleToolkit] {result.Name} => {result.ResultState}");
            }
        }
    }
}
