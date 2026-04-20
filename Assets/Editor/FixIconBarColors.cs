using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class FixIconBarColors
{
    public static void Execute()
    {
        var canvas = GameObject.Find("UICanvas");
        var bar = canvas?.transform.Find("RightIconBar");
        if (bar == null) { Debug.LogError("RightIconBar not found"); return; }

        // Fix bar background
        var barImg = bar.GetComponent<Image>();
        if (barImg != null)
        {
            ColorUtility.TryParseHtmlString("#2a1a06", out Color c);
            barImg.color = c;
            EditorUtility.SetDirty(bar.gameObject);
        }

        // Fix each button
        var btnColors = new string[] { "#1e6008", "#6a3a08", "#0a2858" };
        for (int i = 0; i < bar.childCount && i < btnColors.Length; i++)
        {
            var btn = bar.GetChild(i).gameObject;
            var img = btn.GetComponent<Image>();
            if (img != null)
            {
                ColorUtility.TryParseHtmlString(btnColors[i], out Color c);
                img.color = c;
                EditorUtility.SetDirty(btn);
                Debug.Log($"Set {btn.name} color to {btnColors[i]}");
            }
        }

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("FixIconBarColors done!");
    }
}
