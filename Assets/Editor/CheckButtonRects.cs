using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class CheckButtonRects
{
    public static void Execute()
    {
        // Check StartPauseButton real pixel size at runtime
        var canvas = GameObject.Find("UICanvas");
        if (canvas == null) { Debug.LogError("UICanvas not found — is game playing?"); return; }

        // Find ButtonBar and check its actual rect
        var buttonBar = FindDeep(canvas.transform, "ButtonBar");
        if (buttonBar != null)
        {
            var rt = buttonBar.GetComponent<RectTransform>();
            Debug.Log($"ButtonBar rect: {rt.rect} | sizeDelta: {rt.sizeDelta} | anchorMin: {rt.anchorMin} | anchorMax: {rt.anchorMax}");
            Debug.Log($"ButtonBar world corners:");
            var corners = new Vector3[4];
            rt.GetWorldCorners(corners);
            for(int i=0;i<4;i++) Debug.Log($"  corner[{i}] = {corners[i]}");
        }

        var startBtn = FindDeep(canvas.transform, "StartPauseButton");
        if (startBtn != null)
        {
            var rt = startBtn.GetComponent<RectTransform>();
            Debug.Log($"StartBtn rect: {rt.rect}");
            var img = startBtn.GetComponent<Image>();
            Debug.Log($"StartBtn Image raycastTarget: {img?.raycastTarget}");
            Debug.Log($"StartBtn Button interactable: {startBtn.GetComponent<Button>()?.interactable}");
        }

        // Check Canvas scaler
        var scaler = canvas.GetComponent<CanvasScaler>();
        Debug.Log($"CanvasScaler mode: {scaler?.uiScaleMode} refRes: {scaler?.referenceResolution} match: {scaler?.matchWidthOrHeight}");

        var canvasRt = canvas.GetComponent<RectTransform>();
        Debug.Log($"Canvas rect: {canvasRt.rect}");
    }

    static Transform FindDeep(Transform root, string name)
    {
        if (root.name == name) return root;
        foreach (Transform ch in root)
        {
            var f = FindDeep(ch, name);
            if (f != null) return f;
        }
        return null;
    }
}
