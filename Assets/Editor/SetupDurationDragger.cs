using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class SetupDurationDragger
{
    public static void Execute()
    {
        var label = GameObject.Find("UICanvas/BackgroundPanel/MainContainer/DurationRow/DurationLabel");
        if (label == null)
        {
            Debug.LogError("[DurationDragger] DurationLabel not found!");
            return;
        }
        Debug.Log("[DurationDragger] Found label");

        var rect = label.GetComponent<RectTransform>();
        if (rect != null) rect.sizeDelta = new Vector2(200, 50);

        // Image component for raycast hit area
        var img = label.GetComponent<Image>();
        if (img == null)
        {
            img = label.AddComponent<Image>();
            Debug.Log("[DurationDragger] Added Image component");
        }
        if (img != null)
        {
            img.color = new Color(1f, 1f, 1f, 0.08f);
            img.raycastTarget = true;
        }

        var tmp = label.GetComponent<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.raycastTarget = false;
            tmp.text = "\u2190 25 min \u2192";
            tmp.fontSize = 24;
        }

        var dragger = label.GetComponent<DurationDragger>();
        if (dragger == null)
        {
            dragger = label.AddComponent<DurationDragger>();
            Debug.Log("[DurationDragger] Added DurationDragger component");
        }
        if (dragger != null && tmp != null)
        {
            dragger.label = tmp;
        }

        // Hide +5/-5 buttons
        var decBtn = GameObject.Find("UICanvas/BackgroundPanel/MainContainer/DurationRow/DecBtn");
        var incBtn = GameObject.Find("UICanvas/BackgroundPanel/MainContainer/DurationRow/IncBtn");
        if (decBtn != null) decBtn.SetActive(false);
        if (incBtn != null) incBtn.SetActive(false);

        EditorUtility.SetDirty(label);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        Debug.Log("[DurationDragger] Setup complete! Drag the label left/right to adjust duration.");
    }
}
