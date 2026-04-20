using UnityEngine;
using UnityEditor;
using TMPro;

/// Adds a dark border "shadow" child sprite to each plot
/// giving the appearance of a raised dirt tile like Rusty's Retirement
public class AddPlotBorders
{
    public static void Execute()
    {
        var uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        for (int i = 1; i <= 6; i++)
        {
            var go = GameObject.Find("FarmPlot_" + i);
            if (go == null) continue;

            // Remove old shadow if exists
            var oldShadow = go.transform.Find("Shadow");
            if (oldShadow != null) Object.DestroyImmediate(oldShadow.gameObject);

            // ── Shadow/border layer (slightly larger, darker, behind main sprite) ──
            var shadowGO = new GameObject("Shadow");
            shadowGO.transform.SetParent(go.transform, false);
            shadowGO.transform.localPosition = new Vector3(0.005f, -0.008f, 0.05f); // slightly offset
            shadowGO.transform.localScale    = new Vector3(1.08f, 1.08f, 1f);        // slightly bigger

            var shadowSR = shadowGO.AddComponent<SpriteRenderer>();
            shadowSR.sprite       = uiSprite;
            shadowSR.drawMode     = SpriteDrawMode.Simple;
            shadowSR.color        = new Color(0.18f, 0.12f, 0.06f, 0.7f); // dark brown shadow
            shadowSR.sortingOrder = -1; // behind main sprite

            // ── Make main sprite SortingOrder = 0 ──
            var mainSR = go.GetComponent<SpriteRenderer>();
            if (mainSR != null) mainSR.sortingOrder = 0;

            // ── Label SortingOrder stays at 3 ──

            EditorUtility.SetDirty(go);
        }

        // ── Update FarmPlot colors: richer dirt brown ──
        for (int i = 1; i <= 6; i++)
        {
            var go   = GameObject.Find("FarmPlot_" + i);
            var plot = go?.GetComponent<FarmPlot>();
            if (plot == null) continue;

            plot.emptyColor   = new Color(0.45f, 0.32f, 0.16f, 1f);  // rich dirt
            plot.growingColor = new Color(0.20f, 0.58f, 0.20f, 1f);  // lush green
            plot.readyColor   = new Color(0.30f, 1.00f, 0.30f, 1f);  // bright harvest

            var sr = go.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = plot.emptyColor;

            EditorUtility.SetDirty(go);
        }

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("AddPlotBorders complete + saved!");
    }
}
