using System;
using System.IO;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace TowerCards.Items.Tests
{
    // Re-register callbacks after domain reload so lifecycle runs also export results.
    [InitializeOnLoad]
    public static class ItemSystemTestRunner
    {
        private const string ActiveKey = "TowerCards.ItemTests.ActiveRun";
        private const string ResultPath = "Library/ItemSystem-EditMode-results.xml";

        static ItemSystemTestRunner()
        {
            TestRunnerApi.RegisterTestCallback(new ResultsWriter());
        }

        [MenuItem("Tools/Tower Cards/Run Item System Tests")]
        public static void RunAll()
        {
            if (EditorApplication.isCompiling || EditorApplication.isPlaying)
                throw new InvalidOperationException("Run Item System tests from an idle editor in Edit Mode.");
            if (!string.IsNullOrEmpty(SessionState.GetString(ActiveKey, "")))
                throw new InvalidOperationException("An Item System test run is already active.");

            SessionState.SetString(ActiveKey, ResultPath);
            try
            {
                var api = ScriptableObject.CreateInstance<TestRunnerApi>();
                api.Execute(new ExecutionSettings(new Filter
                {
                    testMode = TestMode.EditMode,
                    groupNames = new[] { "^TowerCards.Items.Tests\\." }
                }));
                UnityEngine.Object.DestroyImmediate(api);
            }
            catch
            {
                SessionState.EraseString(ActiveKey);
                throw;
            }
        }

        private sealed class ResultsWriter : ICallbacks
        {
            public void RunStarted(ITestAdaptor test) { }
            public void TestStarted(ITestAdaptor test) { }
            public void TestFinished(ITestResultAdaptor test) { }

            public void RunFinished(ITestResultAdaptor result)
            {
                string path = SessionState.GetString(ActiveKey, "");
                if (string.IsNullOrEmpty(path))
                    return;
                try
                {
                    TestRunnerApi.SaveResultToFile(result, Path.GetFullPath(path));
                    Debug.Log($"Item System tests: {result.PassCount} passed, {result.FailCount} failed, " +
                        $"{result.SkipCount} skipped, {result.InconclusiveCount} inconclusive. Results: {path}");
                }
                finally
                {
                    SessionState.EraseString(ActiveKey);
                }
            }
        }
    }
}
