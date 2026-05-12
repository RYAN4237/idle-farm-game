using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// Fills GrassLayer under water areas so no black holes appear
public class FillGrassUnderWater
{
    [MenuItem("Tools/Fill Grass Under Water")]
    public static void Execute()
    {
        var grassGO = GameObject.Find("Tilemap/GrassLayer") ?? GameObject.Find("GrassLayer");
        if (grassGO == null) { Debug.LogError("GrassLayer not found"); return; }

        var tm = grassGO.GetComponent<Tilemap>();

        // Load a plain grass tile for the water-area fills
        var allGrass = AssetDatabase.LoadAllAssetsAtPath(
            "Assets/Sprout Lands - Sprites - Basic pack/Tilesets/Grass.png")
            .OfType<Sprite>().ToArray();

        Tile MakeGrass(string name)
        {
            var spr = allGrass.FirstOrDefault(s => s.name == name) ?? allGrass.FirstOrDefault();
            if (spr == null) return null;
            var t = ScriptableObject.CreateInstance<Tile>();
            t.sprite = spr;
            return t;
        }

        // Fill all water positions with grass underneath (unconditionally — cleared cells
        // are stored as empty, not null, so null-check would miss them)
        var grass0 = MakeGrass("Grass_0");

        // River y=2
        for (int x = -3; x < 23; x++)
            tm.SetTile(new Vector3Int(x, 2, 0), grass0);

        // Pond
        int[,] pond = {
            {1,-2},{2,-2},{3,-2},{4,-2},
            {1,-1},{2,-1},{3,-1},{4,-1},{5,-1},
            {2, 0},{3, 0},{4, 0}
        };
        for (int i = 0; i < pond.GetLength(0); i++)
            tm.SetTile(new Vector3Int(pond[i,0], pond[i,1], 0), grass0);

        // Upper pool
        int[,] pool = {
            {16,5},{17,5},{18,5},
            {17,6},{18,6},{19,6}
        };
        for (int i = 0; i < pool.GetLength(0); i++)
            tm.SetTile(new Vector3Int(pool[i,0], pool[i,1], 0), grass0);

        EditorUtility.SetDirty(grassGO);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("[FillGrassUnderWater] Done — grass filled under all water areas");
    }
}
