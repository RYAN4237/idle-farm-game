using UnityEngine;
using UnityEditor;
using System.IO;

public class BuildGame
{
    public static void Execute()
    {
        string buildPath = "Builds/FocusFarm/FocusFarm.exe";

        // Make sure build directory exists
        Directory.CreateDirectory(Path.GetDirectoryName(buildPath));

        var opts = new BuildPlayerOptions
        {
            scenes        = new[] { "Assets/DesktopIdleGame.unity" },
            locationPathName = buildPath,
            target        = BuildTarget.StandaloneWindows64,
            options       = BuildOptions.None,
        };

        Debug.Log($"[Build] Building to: {buildPath}");
        var report = BuildPipeline.BuildPlayer(opts);
        Debug.Log($"[Build] Result: {report.summary.result} — {report.summary.totalErrors} errors, {report.summary.totalWarnings} warnings");
        Debug.Log($"[Build] Output size: {report.summary.totalSize / 1024 / 1024:F1} MB");
    }
}
