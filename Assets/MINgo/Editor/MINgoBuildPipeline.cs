using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace MINgo.EditorTools
{
    public static class MINgoBuildPipeline
    {
        private const string ScenePath = "Assets/Scenes/FreeFlightSandbox.unity";
        private const string BuildDirectory = "Builds/macOS";
        private const string BuildPath = BuildDirectory + "/MINgo.app";

        [MenuItem("MINgo/Build macOS Player")]
        public static void BuildMacOSPlayer()
        {
            Directory.CreateDirectory(BuildDirectory);

            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = BuildPath,
                target = BuildTarget.StandaloneOSX,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new BuildFailedException($"MINgo macOS build failed: {report.summary.result}");
            }
        }
    }
}
