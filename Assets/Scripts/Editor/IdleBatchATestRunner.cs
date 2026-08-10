using System;
using System.IO;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace HyperCasualRunner.Editor
{
    /// <summary>
    /// Batchmode-safe EditMode runner for idle matrix smoke fixtures.
    /// Usage (no -quit): Unity -batchmode -nographics -projectPath ... -executeMethod HyperCasualRunner.Editor.IdleBatchATestRunner.RunAndExit
    /// Also: RunBatchBCAndExit, RunAllIdleSmokeAndExit
    /// </summary>
    public static class IdleBatchATestRunner
    {
        public static void RunAndExit() => RunFixturesAndExit(
            "Logs/IdleBatchA-Summary.txt",
            "Logs/IdleBatchA-TestResults.xml",
            "HyperCasualRunner.Tests.IdleBatchASmokeTests");

        public static void RunBatchBCAndExit() => RunFixturesAndExit(
            "Logs/IdleBatchBC-Summary.txt",
            "Logs/IdleBatchBC-TestResults.xml",
            "HyperCasualRunner.Tests.IdleBatchBCSmokeTests");

        public static void RunAllIdleSmokeAndExit() => RunFixturesAndExit(
            "Logs/IdleAllSmoke-Summary.txt",
            "Logs/IdleAllSmoke-TestResults.xml",
            "HyperCasualRunner.Tests.IdleBatchASmokeTests",
            "HyperCasualRunner.Tests.IdleBatchBCSmokeTests",
            "HyperCasualRunner.Tests.IdleKernelCorrectnessTests",
            "HyperCasualRunner.Tests.CosmeticsShopTests");

        private static void RunFixturesAndExit(string summaryPath, string resultsPath, params string[] fixtures)
        {
            Directory.CreateDirectory("Logs");
            var api = ScriptableObject.CreateInstance<TestRunnerApi>();
            var filter = new Filter
            {
                testMode = TestMode.EditMode,
                testNames = fixtures
            };

            var callbacks = new Callbacks(summaryPath, resultsPath);
            api.RegisterCallbacks(callbacks);
            Debug.Log("[IdleToolkit] Starting EditMode fixtures: " + string.Join(", ", fixtures));
            api.Execute(new ExecutionSettings(filter));
        }

        private sealed class Callbacks : ICallbacks
        {
            private readonly string _summaryPath;
            private readonly string _resultsPath;

            public Callbacks(string summaryPath, string resultsPath)
            {
                _summaryPath = summaryPath;
                _resultsPath = resultsPath;
            }

            public void RunStarted(ITestAdaptor testsToRun) { }

            public void RunFinished(ITestResultAdaptor result)
            {
                string summary =
                    $"result={result.ResultState} pass={result.PassCount} fail={result.FailCount} " +
                    $"skip={result.SkipCount} inconclusive={result.InconclusiveCount} duration={result.Duration}\n";
                // Write only the requested artifact paths — do not overwrite Batch A when running BC/All.
                File.WriteAllText(_summaryPath, summary);
                Debug.Log("[IdleToolkit] " + summary.Trim() + " → " + _summaryPath);

                try
                {
                    string xml =
                        "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                        $"<test-run total=\"{result.PassCount + result.FailCount + result.SkipCount}\" " +
                        $"passed=\"{result.PassCount}\" failed=\"{result.FailCount}\" " +
                        $"result=\"{result.ResultState}\" />\n";
                    File.WriteAllText(_resultsPath, xml);
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
                if (result.ResultState == "Passed")
                {
                    Debug.Log($"[IdleToolkit] {result.Name} => Passed");
                    return;
                }

                string msg = result.Message ?? "";
                string stack = result.StackTrace ?? "";
                string output = result.Output ?? "";
                Debug.LogWarning(
                    $"[IdleToolkit] FAIL {result.Name} => {result.ResultState}\n" +
                    $"MESSAGE: {msg}\nOUTPUT: {output}\nSTACK: {stack}");
            }
        }
    }
}
