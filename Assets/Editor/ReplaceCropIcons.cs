using UnityEngine;
using UnityEditor;
using TMPro;

/// Replaces emoji CropIcon with colored SpriteRenderer indicators
/// Empty = nothing visible
/// Growing = small yellow-green diamond
/// Ready = bright star-like glow dot
public class ReplaceCropIcons
{
    public static void Execute()
    {
        var uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        for (int i = 1; i <= 6; i++)
        {
            var go = GameObject.Find("FarmPlot_" + i);
            if (go == null) continue;

            // Remove old TMP CropIcon
            var oldIcon = go.transform.Find("CropIcon");
            if (oldIcon != null) Object.DestroyImmediate(oldIcon.gameObject);

            // ── Growth indicator: small circle, starts hidden ──
            var indicatorGO = new GameObject("CropIcon");
            indicatorGO.transform.SetParent(go.transform, false);
            indicatorGO.transform.localPosition = new Vector3(0f, 0.035f, -0.15f);
            indicatorGO.transform.localScale    = new Vector3(0.25f, 0.25f, 1f);

            var sr = indicatorGO.AddComponent<SpriteRenderer>();
            sr.sprite       = uiSprite;
            sr.drawMode     = SpriteDrawMode.Simple;
            sr.color        = new Color(0.9f, 0.85f, 0.1f, 0f); // hidden by default
            sr.sortingOrder = 4;

            EditorUtility.SetDirty(go);
        }

        // Update label positions to make room for indicator
        for (int i = 1; i <= 6; i++)
        {
            var go = GameObject.Find("FarmPlot_" + i);
            if (go == null) continue;
            var label = go.transform.Find("Label");
            if (label != null)
            {
                label.localPosition = new Vector3(0f, -0.01f, -0.1f);
                var tmp = label.GetComponent<TextMeshPro>();
                if (tmp != null) tmp.fontSize = 9f;
                EditorUtility.SetDirty(label.gameObject);
            }
        }

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("ReplaceCropIcons complete + saved!");
    }
}
