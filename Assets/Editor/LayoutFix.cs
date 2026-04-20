using UnityEngine;
using UnityEditor;

/// Repositions UI and FarmPlots so they don't overlap.
/// Layout (1920x1080 reference):
///   Top half    → Timer UI (CenterContainer stays centred, slightly up)
///   Bottom band → Buttons + FarmPlots visible
public class LayoutFix
{
    public static void Execute()
    {
        var canvas = GameObject.Find("UICanvas");
        if (canvas == null) { Debug.LogError("UICanvas not found"); return; }

        // ── 1. BackgroundPanel covers full screen again (FarmPlots are World Space,
        //        they render behind the Canvas; we'll make the panel semi-transparent
        //        at the bottom so the Farm shows through) ──
        var bg = canvas.transform.Find("BackgroundPanel");
        if (bg != null)
        {
            var bgRect = bg.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            // Make top 60% opaque dark, bottom 40% transparent so Farm shows
            // Simplest: just lower the alpha so Farm squares bleed through
            var img = bg.GetComponent<UnityEngine.UI.Image>();
            if (img != null)
                img.color = new Color(0.09f, 0.11f, 0.13f, 0.0f); // fully transparent
        }

        // ── 2. Add a top panel for opaque dark background behind Timer ──
        var topBG = canvas.transform.Find("TopBackground");
        if (topBG == null)
        {
            var topBGgo = new GameObject("TopBackground");
            topBGgo.transform.SetParent(canvas.transform, false);
            topBGgo.transform.SetSiblingIndex(0); // behind everything

            var topRect = topBGgo.AddComponent<RectTransform>();
            topRect.anchorMin = new Vector2(0, 0.35f);
            topRect.anchorMax = Vector2.one;
            topRect.offsetMin = Vector2.zero;
            topRect.offsetMax = Vector2.zero;

            var topImg = topBGgo.AddComponent<UnityEngine.UI.Image>();
            topImg.color = new Color(0.09f, 0.11f, 0.13f, 1f);
            topImg.raycastTarget = false;
            Debug.Log("TopBackground created.");
        }

        // ── 3. Move CenterContainer up slightly ──
        var center = canvas.transform.Find("CenterContainer");
        if (center != null)
        {
            var r = center.GetComponent<RectTransform>();
            r.anchoredPosition = new Vector2(0, 80f); // shift up 80px
            EditorUtility.SetDirty(center.gameObject);
        }

        // ── 4. ButtonBar: keep at bottom of top panel ──
        var buttonBar = canvas.transform.Find("ButtonBar");
        if (buttonBar != null)
        {
            var r = buttonBar.GetComponent<RectTransform>();
            r.anchorMin = new Vector2(0.5f, 0.35f);
            r.anchorMax = new Vector2(0.5f, 0.35f);
            r.anchoredPosition = new Vector2(0, 35f);
            EditorUtility.SetDirty(buttonBar.gameObject);
        }

        // ── 5. CycleDots: just above ButtonBar ──
        var cycleDots = canvas.transform.Find("CycleDots");
        if (cycleDots != null)
        {
            var r = cycleDots.GetComponent<RectTransform>();
            r.anchorMin = new Vector2(0.5f, 0.35f);
            r.anchorMax = new Vector2(0.5f, 0.35f);
            r.anchoredPosition = new Vector2(0f, 80f);
            EditorUtility.SetDirty(cycleDots.gameObject);
        }

        // ── 6. Move FarmPlots up so they're clearly in the bottom 35% ──
        // Camera ortho size=5, world height ±5. Bottom 35% of 1080 screen
        // maps to world y ≈ -5 to -1.75.
        // Place plots at y=-2.5, nicely centred in the farm band.
        Vector3[] positions = new Vector3[]
        {
            new Vector3(-2.2f, -2.6f, 0f),
            new Vector3( 0.0f, -2.6f, 0f),
            new Vector3( 2.2f, -2.6f, 0f),
        };

        for (int i = 0; i < 3; i++)
        {
            var go = GameObject.Find("FarmPlot_" + (i + 1));
            if (go != null)
            {
                go.transform.position = positions[i];
                EditorUtility.SetDirty(go);
            }
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("LayoutFix complete! Farm plots visible in bottom 35%.");
    }
}
