using UnityEngine;
using UnityEditor;

public class TestPanelOpen
{
    public static void Execute()
    {
        var canvas = GameObject.Find("UICanvas");
        var uiMgr  = canvas?.GetComponent<UIManager>();
        if (uiMgr == null) { Debug.LogError("UIManager not found"); return; }
        uiMgr.TogglePanel("SEEDS");
        Debug.Log("Panel toggled: SEEDS");
    }
}
