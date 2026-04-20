using UnityEngine;
using UnityEditor;
using TMPro;

/// Adds crop emoji/text icons on top of plots to show growth state
/// Uses TextMeshPro world-space text as "sprites" 
/// Empty=🟫 Growing=🌱 Ready=🌻 (rendered as Unicode in TMP)
public class AddCropIcons
{
    public static void Execute()
    {
        for (int i = 1; i <= 6; i++)
        {
            var go = GameObject.Find("FarmPlot_" + i);
            if (go == null) continue;

            // Remove old icon
            var old = go.transform.Find("CropIcon");
            if (old != null) Object.DestroyImmediate(old.gameObject);

            // Create big icon label above the plot text
            var iconGO = new GameObject("CropIcon");
            iconGO.transform.SetParent(go.transform, false);
            iconGO.transform.localPosition = new Vector3(0f, 0.025f, -0.15f);
            iconGO.transform.localScale    = new Vector3(0.040f, 0.040f, 1f);

            var tmp = iconGO.AddComponent<TextMeshPro>();
            tmp.text      = "🌱";   // default = empty
            tmp.fontSize  = 18f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color     = Color.white;
            tmp.enableWordWrapping  = false;
            tmp.textWrappingMode    = TMPro.TextWrappingModes.NoWrap;
            tmp.overflowMode        = TMPro.TextOverflowModes.Overflow;
            tmp.rectTransform.sizeDelta = new Vector2(3f, 3f);
            tmp.sortingOrder = 4;

            // Tag so FarmPlot can find and update it
            iconGO.name = "CropIcon";
            EditorUtility.SetDirty(go);
        }

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("AddCropIcons complete + saved!");
    }
}
