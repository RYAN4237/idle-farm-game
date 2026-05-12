using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.Linq;

/// Creates a persistent Grass0 tile asset and fills GrassLayer with it
public class FixGrassPersist
{
    [MenuItem("Tools/Fix Grass Persist")]
    public static void Execute()
    {
        string grassPath = "Assets/Sprout Lands - Sprites - Basic pack/Tilesets/Grass.png";
        string tileDir   = "Assets/Tiles";

        if (!AssetDatabase.IsValidFolder(tileDir))
            AssetDatabase.CreateFolder("Assets", "Tiles");

        var sprites = AssetDatabase.LoadAllAssetsAtPath(grassPath).OfType<Sprite>().ToArray();
        Debug.Log($"Grass sprites available: {sprites.Length}");

        // Find Grass_0 — confirmed rect (0, 96, 16, 16) from earlier check
        var grass0 = sprites.FirstOrDefault(s => s.name == "Grass_0");
        if (grass0 == null) { Debug.LogError("Grass_0 not found"); return; }

        // Save tile as persistent asset
        string tilePath = tileDir + "/Grass0Solid.asset";
        AssetDatabase.DeleteAsset(tilePath);
        var tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = grass0;
        tile.name   = "Grass0Solid";
        AssetDatabase.CreateAsset(tile, tilePath);
        AssetDatabase.SaveAssets();

        // Reload from disk to confirm persistence
        var savedTile = AssetDatabase.LoadAssetAtPath<Tile>(tilePath);
        if (savedTile == null || savedTile.sprite == null)
        {
            Debug.LogError($"Tile save failed or sprite null after save");
            return;
        }
        Debug.Log($"Saved tile: {tilePath}, sprite={savedTile.sprite.name}");

        // Fill GrassLayer
        var grassGO = GameObject.Find("Tilemap/GrassLayer") ?? GameObject.Find("GrassLayer");
        if (grassGO == null) { Debug.LogError("GrassLayer not found"); return; }
        var tm = grassGO.GetComponent<Tilemap>();
        tm.ClearAllTiles();

        for (int x = -3; x < 23; x++)
        for (int y = -3; y < 8; y++)
            tm.SetTile(new Vector3Int(x, y, 0), savedTile);

        // Verify one tile
        var check = tm.GetTile<Tile>(new Vector3Int(0, 0, 0));
        Debug.Log($"Verify (0,0): tile={check?.name}, sprite={check?.sprite?.name}");

        EditorUtility.SetDirty(grassGO);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[FixGrassPersist] Done");
    }
}
