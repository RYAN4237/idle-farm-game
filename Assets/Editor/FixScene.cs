using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class FixScene
{
    public static void Execute()
    {
        // Hide objects created by SetupSproutLandsResources that cover the BG
        foreach (var name in new[] { "FarmBackground", "SproutLandsTilemap" })
        {
            var go = GameObject.Find(name);
            if (go != null)
            {
                go.SetActive(false);
                EditorUtility.SetDirty(go);
                Debug.Log($"[FixScene] Hid {name}");
            }
        }

        // Duplicate FarmPlot to make 6 total
        var container = GameObject.Find("FarmPlots");
        var existing  = new List<Transform>();
        foreach (Transform t in container.transform)
            existing.Add(t);

        int need = 6 - existing.Count;
        for (int i = 0; i < need; i++)
        {
            var clone = Object.Instantiate(existing[0].gameObject, container.transform);
            clone.name = "FarmPlot";
            existing.Add(clone.transform);
        }

        // Layout 6 plots in 3x2 grid on crop field
        // BG visible area: x 1.61~19.39, y -2.3~7.7
        // Crop area: roughly x 8.8~11.8, y 3.0~4.0
        int cols     = 3;
        float cell   = 0.75f;
        float startX = 9.0f;
        float startY = 3.75f;

        for (int i = 0; i < existing.Count; i++)
        {
            int col = i % cols;
            int row = i / cols;
            existing[i].position   = new Vector3(startX + col * cell, startY - row * cell, -0.1f);
            existing[i].localScale = Vector3.one * 0.65f;
            EditorUtility.SetDirty(existing[i].gameObject);
        }

        // Move Farmer to crop area center
        var farmer = GameObject.Find("Farmer");
        if (farmer != null)
        {
            farmer.transform.position   = new Vector3(10.5f, 3.38f, -0.2f);
            farmer.transform.localScale = Vector3.one * 0.42f;
            EditorUtility.SetDirty(farmer);
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log($"[FixScene] Done — {existing.Count} FarmPlots placed on crop field.");
    }
}
