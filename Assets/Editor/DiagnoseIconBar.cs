using UnityEngine;
using UnityEditor;

public class DiagnoseIconBar
{
    public static void Execute()
    {
        var canvas = GameObject.Find("UICanvas");
        if (canvas == null) { Debug.LogError("UICanvas null"); return; }

        var bar = canvas.transform.Find("RightIconBar");
        if (bar == null) { Debug.LogError("RightIconBar not found!"); return; }

        var rt = bar.GetComponent<RectTransform>();
        var corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        // Convert to screen space
        Debug.Log($"IconBar SCREEN corners: BL={corners[0]:F0} TL={corners[1]:F0} TR={corners[2]:F0} BR={corners[3]:F0}");
        Debug.Log($"  rect={rt.rect} anchoredPos={rt.anchoredPosition} sizeDelta={rt.sizeDelta}");
        Debug.Log($"Screen: {Screen.width}x{Screen.height}, scaleFactor={canvas.GetComponent<Canvas>().scaleFactor}");

        for (int i = 0; i < bar.childCount; i++)
        {
            var ch = bar.GetChild(i);
            var crt = ch.GetComponent<RectTransform>();
            crt.GetWorldCorners(corners);
            Debug.Log($"  {ch.name}: screen BL={corners[0]:F0} TR={corners[2]:F0}");
        }
    }
}
