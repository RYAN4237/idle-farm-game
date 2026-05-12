using UnityEngine;
using UnityEditor;

public class TestOpenSeedsPanel
{
    public static void Execute()
    {
        var ui = Object.FindFirstObjectByType<UIManager>();
        if (ui == null) { Debug.Log("UIManager not found"); return; }
        var pos = ui.expandablePanel.anchoredPosition;
        ui.expandablePanel.anchoredPosition = new Vector2(0f, pos.y);
        var t = ui.GetType();
        t.GetField("_open", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(ui, true);
        t.GetField("_tab",  System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(ui, "SEEDS");
        var tabs = Object.FindFirstObjectByType<TabMenuController>();
        if (tabs != null) tabs.SwitchTab(TabMenuController.Tab.Seeds);
        Debug.Log("Opened SEEDS panel");
    }
}
