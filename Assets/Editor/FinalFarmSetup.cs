using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class FinalFarmSetup
{
    [MenuItem("Tools/Final Farm Setup")]
    public static void Execute()
    {
        var container = GameObject.Find("FarmPlots");
        if (container == null) { Debug.LogError("FarmPlots not found"); return; }

        var plots = new System.Collections.Generic.List<Transform>();
        foreach (Transform t in container.transform) plots.Add(t);

        string dirtPath = "Assets/Resources/Tilled_Dirt.png";
        var allAssets = AssetDatabase.LoadAllAssetsAtPath(dirtPath);

        // Log first 15 sprite names to find a dark soil one
        int logged = 0;
        foreach (var a in allAssets)
        {
            if (a is Sprite s && logged < 15)
            {
                Debug.Log($"Sprite[{logged}] {s.name} rect={s.rect}");
                logged++;
            }
        }

        // Tilled_Dirt from Sprout Lands: the darker tilled soil tiles
        // are typically in rows 1+ of the sheet. Index 0 = top-left tile.
        // Try index 12 or 13 for the center-filled dark dirt tile
        Sprite bestSprite = null;
        foreach (var a in allAssets)
        {
            if (a is Sprite s && s.name == "Tilled_Dirt_12") { bestSprite = s; break; }
        }
        if (bestSprite == null)
            foreach (var a in allAssets)
                if (a is Sprite s && s.name == "Tilled_Dirt_0") { bestSprite = s; break; }

        // Scale: 16px / 100PPU = 0.16u per tile. Scale=6 → 0.96u per tile (~1 unit)
        // Use scale=8 → 1.28u per tile, cell=1.4u
        float scale  = 8.0f;
        float cellW  = 1.4f;
        float cellH  = 1.4f;
        float startX = 9.5f;
        float startY = 5.5f;
        int   cols   = 3;

        for (int i = 0; i < plots.Count; i++)
        {
            int col = i % cols;
            int row = i / cols;
            plots[i].localPosition = new Vector3(startX + col * cellW, startY - row * cellH, -0.2f);
            plots[i].localScale    = Vector3.one * scale;

            var sr = plots[i].GetComponent<SpriteRenderer>();
            if (sr != null && bestSprite != null)
            {
                var so = new SerializedObject(sr);
                so.FindProperty("m_Sprite").objectReferenceValue = bestSprite;
                so.ApplyModifiedProperties();
                sr.color = Color.white;
                sr.sortingOrder = 5;
                EditorUtility.SetDirty(sr);
            }
        }

        var farmer = GameObject.Find("Farmer");
        if (farmer != null)
            farmer.transform.position = new Vector3(startX - 1.2f, startY - cellH * 0.5f, -0.3f);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log($"Done. Scale={scale}, tile size={0.16f*scale:F2}u, sprite={bestSprite?.name}");
    }
}
