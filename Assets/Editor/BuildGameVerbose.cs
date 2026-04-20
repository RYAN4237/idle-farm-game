using UnityEngine;
using UnityEditor;
using System.IO;

public class BuildGameVerbose
{
    public static void Execute()
    {
        string buildPath = "Builds/FocusFarm/FocusFarm.exe";
        Directory.CreateDirectory(Path.GetDirectoryName(buildPath));

        var opts = new BuildPlayerOptions
        {
            scenes           = new[] { "Assets/DesktopIdleGame.unity" },
            locationPathName = buildPath,
            target           = BuildTarget.StandaloneWindows64,
            options          = BuildOptions.None,
        };

        var report = BuildPipeline.BuildPlayer(opts);
        Debug.Log($"[Build] {report.summary.result} — {report.summary.totalErrors} errors, {report.summary.totalWarnings} warnings");

        // Print each step with errors
        foreach (var step in report.steps)
        {
            if (step.messages.Length > 0)
            {
                foreach (var msg in step.messages)
                {
                    if (msg.type == LogType.Error || msg.type == LogType.Warning)
                        Debug.Log($"  [{msg.type}] {step.name}: {msg.content}");
                }
            }
        }
    }
}
