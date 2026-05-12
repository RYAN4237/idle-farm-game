using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.Linq;

public class DiagGrass
{
    public static void Execute()
    {
        var tmGo = GameObject.Find("Tilemap/Map_Ground");
        if (tmGo == null) { Debug.LogError("Map_Ground not found"); return; }
        var tm = tmGo.GetComponent<Tilemap>();

        // Sample a few positions
        int count = 0;
        foreach (var pos in tm.cellBounds.allPositionsWithin)
        {
            if (!tm.HasTile(pos)) continue;
            if (pos.y >= 0 && pos.y <= 3) continue;
            var tile = tm.GetTile(pos) as Tile;
            if (tile == null) { Debug.Log($"pos {pos}: tile is null or not Tile type"); }
            else Debug.Log($"pos {pos}: tile={tile.name} sprite={tile.sprite?.name ?? "NULL"} sprite.texture={tile.sprite?.texture?.name ?? "NULL"}");
            if (++count >= 5) break;
        }

        // Also check what sprites are loaded from Grass_AI
        var sprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Grass_AI.png")
            .OfType<Sprite>().ToArray();
        Debug.Log($"Grass_AI sprites: {sprites.Length}");
        foreach (var s in sprites)
            Debug.Log($"  sprite: {s.name} rect={s.rect} tex={s.texture.name} texSize={s.texture.width}x{s.texture.height}");

        // Check the tile assets
        var t0 = AssetDatabase.LoadAssetAtPath<Tile>("Assets/Tiles3/GrassAI_0.asset");
        var t1 = AssetDatabase.LoadAssetAtPath<Tile>("Assets/Tiles3/GrassAI_1.asset");
        Debug.Log($"Tile0: {t0?.name} sprite={t0?.sprite?.name}");
        Debug.Log($"Tile1: {t1?.name} sprite={t1?.sprite?.name}");
    }
}
