using UnityEngine;
using UnityEditor;
using TMPro;

public class FixUIReferences
{
    public static void Execute()
    {
        var canvas = GameObject.Find("UICanvas");
        if (canvas == null) { Debug.LogError("UICanvas not found!"); return; }

        var uiManager = canvas.GetComponent<UIManager>();
        if (uiManager == null) { Debug.LogError("UIManager not found!"); return; }

        // progressRing managed by ProgressRingController, clear from UIManager
        uiManager.progressRing = null;

        // Fix serialized color values overriding code defaults
        uiManager.idleColor      = new Color(0.20f, 0.85f, 0.70f, 0.95f);
        uiManager.workingColor   = new Color(0.20f, 0.85f, 0.70f, 0.95f);
        uiManager.restColor      = new Color(0.22f, 0.54f, 0.87f, 1.00f);
        uiManager.completedColor = new Color(0.40f, 1.00f, 0.50f, 1.00f);

        // Set initial button text (English - LiberationSans doesn't support CJK)
        var btnTextGO = canvas.transform.Find("ButtonBar/StartPauseButton/Text");
        if (btnTextGO != null)
        {
            var tmp = btnTextGO.GetComponent<TextMeshProUGUI>();
            if (tmp != null) tmp.text = "Start";
        }

        // Set initial status text
        var statusGO = canvas.transform.Find("CenterContainer/StatusText");
        if (statusGO != null)
        {
            var tmp = statusGO.GetComponent<TextMeshProUGUI>();
            if (tmp != null) tmp.text = "Focus";
        }

        EditorUtility.SetDirty(uiManager);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("FixUIReferences complete!");
    }
}
