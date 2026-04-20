using UnityEngine;
using UnityEditor;

public class LaunchBuild
{
    public static void Execute()
    {
        string exePath = System.IO.Path.GetFullPath("Builds/FocusFarm/FocusFarm.exe");
        if (!System.IO.File.Exists(exePath))
        {
            Debug.LogError($"EXE not found: {exePath}");
            return;
        }
        System.Diagnostics.Process.Start(exePath);
        Debug.Log($"[Launch] Started: {exePath}");
    }
}
