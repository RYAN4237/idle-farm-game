using UnityEngine;
using UnityEditor;

public class TestSettingsMenu
{
    public static void Execute()
    {
        // Toggle the settings popup
        var ctrl = Object.FindFirstObjectByType<SettingsMenuController>();
        if (ctrl == null) { Debug.Log("SettingsMenuController not found"); return; }
        ctrl.TogglePopup();
        Debug.Log($"SettingsPopup active: {ctrl.popup?.activeSelf}");
    }
}
