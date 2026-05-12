using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.Linq;

/// Find which Grass sprite is actually a solid fill tile (interior, no transparency)
public class FindSolidGrass
{
    [MenuItem("Tools/Find Solid Grass Tile")]
    public static void Execute()
    {
        string grassPath = "Assets/Sprout Lands - Sprites - Basic pack/Tilesets/Grass.png";
        var sprites = AssetDatabase.LoadAllAssetsAtPath(grassPath).OfType<Sprite>().OrderBy(s => {
            // Parse number from name
            var n = s.name.Replace("Grass_", "");
            return int.TryParse(n, out int v) ? v : 999;
        }).ToArray();

        // Log all sprite positions to find the interior fill tile
        var sb = new System.Text.StringBuilder();
        foreach (var s in sprites)
            sb.AppendLine($"{s.name}: rect=({s.rect.x},{s.rect.y},{s.rect.width}x{s.rect.height})");
        Debug.Log(sb.ToString());

        // In Sprout Lands blob autotile:
        // Interior fill tiles are at the bottom rows (low y values in Unity = bottom of texture)
        // The VERY bottom row (y=0) contains isolated/single tiles
        // Row y=16 contains peninsula tiles
        // Row y=32 contains inner corners
        // ...
        // Row y=96 contains TOP ROW = isolated/single shape tiles
        //
        // For a pure solid fill, we want the tile from the CENTER of the sheet
        // Looking at the layout: y=0 row (bottom) = full interior solid tiles
        // Let's try Grass_66 (row 0, first col) which should be y=0
        var solidCandidate = sprites.FirstOrDefault(s => s.rect.y == 0 && s.rect.x == 0);
        Debug.Log($"Bottom-left tile (y=0,x=0): {solidCandidate?.name}");

        // Also try the middle of the sheet
        var midRow = sprites.FirstOrDefault(s => s.rect.y == 48 && s.rect.x == 80);
        Debug.Log($"Mid tile (y=48,x=80): {midRow?.name}");
    }
}
