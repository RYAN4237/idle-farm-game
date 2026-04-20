using UnityEngine;
using UnityEditor;

public class LayoutFix2
{
    public static void Execute()
    {
        // ── 1. Camera: match background to dark theme ──
        var cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.09f, 0.11f, 0.13f, 1f);
            EditorUtility.SetDirty(cam.gameObject);
            Debug.Log("Camera bg set to dark theme color.");
        }

        // ── 2. FarmPlot positions ──
        // Camera orthographic size = 5 → world height -5 to +5
        // Canvas TopBackground covers top 65% of screen → world y ≈ -0.5 to +5
        // Farm zone: world y = -5 to -0.5 → center at y = -2.75
        // Place plots at y = -2.5, comfortably in the farm band
        var plotPositions = new Vector3[]
        {
            new Vector3(-2.4f, -2.8f, 0f),
            new Vector3( 0.0f, -2.8f, 0f),
            new Vector3( 2.4f, -2.8f, 0f),
        };

        for (int i = 0; i < 3; i++)
        {
            var go = GameObject.Find("FarmPlot_" + (i + 1));
            if (go == null) continue;
            go.transform.position = plotPositions[i];

            // Ensure scale is correct: 10 → 1.6 world units
            go.transform.localScale = new Vector3(10f, 10f, 1f);

            EditorUtility.SetDirty(go);
        }

        // ── 3. TopBackground: covers y=0.35 to 1.0 of screen ──
        var canvas = GameObject.Find("UICanvas");
        var topBG  = canvas?.transform.Find("TopBackground");
        if (topBG != null)
        {
            var r = topBG.GetComponent<RectTransform>();
            // Cover top 62% of screen, leaving bottom 38% for Farm
            r.anchorMin = new Vector2(0f, 0.38f);
            r.anchorMax = new Vector2(1f, 1.0f);
            r.offsetMin = Vector2.zero;
            r.offsetMax = Vector2.zero;
            EditorUtility.SetDirty(topBG.gameObject);
        }

        // ── 4. Add a subtle Farm label at the bottom ──
        if (canvas != null)
        {
            var existingLabel = canvas.transform.Find("FarmLabel");
            if (existingLabel == null)
            {
                var farmLabelGO = new GameObject("FarmLabel");
                farmLabelGO.transform.SetParent(canvas.transform, false);

                var rect = farmLabelGO.AddComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0f);
                rect.anchorMax = new Vector2(0.5f, 0f);
                rect.anchoredPosition = new Vector2(0f, 18f);
                rect.sizeDelta = new Vector2(300f, 30f);

                var tmp = farmLabelGO.AddComponent<TMPro.TextMeshProUGUI>();
                tmp.text = "— Farm —";
                tmp.fontSize = 14f;
                tmp.color = new Color(0.5f, 0.5f, 0.5f, 0.6f);
                tmp.alignment = TMPro.TextAlignmentOptions.Center;
                tmp.raycastTarget = false;

                EditorUtility.SetDirty(farmLabelGO);
                Debug.Log("FarmLabel added.");
            }
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("LayoutFix2 complete!");
    }
}
