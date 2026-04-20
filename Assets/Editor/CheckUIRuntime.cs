using UnityEngine;
using UnityEditor;

public class CheckUIRuntime
{
    public static void Execute()
    {
        var canvas = GameObject.Find("UICanvas");
        if (canvas == null) { Debug.LogError("UICanvas not found"); return; }

        var ui = canvas.GetComponent<UIManager>();
        if (ui == null) { Debug.LogError("UIManager not found on UICanvas"); return; }

        Debug.Log($"UIManager found. seedButton={ui.seedButton}, expandablePanel={ui.expandablePanel}");

        // Force toggle panel open
        ui.TogglePanel("SEEDS");
        Debug.Log("Toggled SEEDS panel");
    }
}
