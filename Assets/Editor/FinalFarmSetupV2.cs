using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class FinalFarmSetupV2
{
    [MenuItem("Tools/Final Farm Setup V2")]
    public static void Execute()
    {
        var container = GameObject.Find("FarmPlots");
        if (container == null) { Debug.LogError("FarmPlots not found"); return; }
        var plots = new System.Collections.Generic.List<Transform>();
        foreach (Transform t in container.transform) plots.Add(t);

        string dirtPath = "Assets/Resources/Tilled_Dirt.png";
        var allAssets = AssetDatabase.LoadAllAssetsAtPath(dirtPath);

        // Find the solid center-fill tilled dirt tile
        // Tilled_Dirt_4 = rect x:64, y:96 (row 0, col 4) = solid dark dirt
        Sprite bestSprite = null;
        foreach (var a in allAssets)
        {
            if (a is Sprite s)
            {
                if (s.name == "Tilled_Dirt_4") { bestSprite = s; break; }
            }
        }
        if (bestSprite == null)
            foreach (var a in allAssets)
                if (a is Sprite s && s.name == "Tilled_Dirt_0") { bestSprite = s; break; }

        Debug.Log($"Using sprite: {bestSprite?.name} rect={bestSprite?.rect}");

        // Camera: x=1.61~19.39, y=-2.3~7.7 (17.78u wide, 10u tall)
        // BG wheat field is at ~55-65% x = ~11~13u, ~45-58% y = ~2.2~3.5u 
        // But BG y coords: camera y=-2.3~7.7, so 45% up from bottom = -2.3 + 10*0.45 = 2.2
        // 58% = -2.3 + 10*0.58 = 3.5. Wheat field center ~x=11, y=3.0
        
        // Actually from BG capture: wheat is at the center of the image (not top)
        // Camera center at y=2.7. Wheat field is slightly above center.
        // Let me use y=4.0~5.5 range (safe area of grass above river)
        // And x=9.5~12.5 (center)

        // Scale: 16px/100PPU = 0.16u. Scale=10 → 1.6u per tile. Cell=1.7u
        float scale  = 10.0f;
        float cellW  = 1.7f;
        float cellH  = 1.7f;
        float startX = 9.8f;
        float startY = 5.2f;
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
            farmer.transform.position = new Vector3(startX - 1.3f, startY - cellH * 0.5f, -0.3f);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        int rows = Mathf.CeilToInt((float)plots.Count/cols);
        Debug.Log($"Grid x:{startX:F1}~{startX+(cols-1)*cellW:F1}, y:{startY:F1}~{startY-(rows-1)*cellH:F1}, tile={0.16f*scale:F2}u");
    }
}
