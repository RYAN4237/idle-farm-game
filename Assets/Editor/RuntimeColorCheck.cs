using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class RuntimeColorCheck
{
    public static void Execute()
    {
        var canvas = GameObject.Find("UICanvas");
        var bar = canvas?.transform.Find("RightIconBar");
        if (bar == null) { Debug.LogError("bar null"); return; }

        // Force button colors to vivid values at runtime
        var btnColors = new[] { "#FF4400", "#FF8800", "#0088FF" };
        int i = 0;
        foreach (Transform child in bar)
        {
            var img = child.GetComponent<Image>();
            if (img != null)
            {
                ColorUtility.TryParseHtmlString(btnColors[i % 3], out Color c);
                img.color = c;
                // Also disable Button transition temporarily
                var btn = child.GetComponent<Button>();
                if (btn != null)
                {
                    btn.transition = Selectable.Transition.None;
                }
                Debug.Log($"{child.name}: forced color {btnColors[i%3]}, active={child.gameObject.activeSelf}");
            }
            i++;
        }
    }
}
