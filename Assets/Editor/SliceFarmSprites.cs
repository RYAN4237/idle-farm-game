using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class SliceFarmSprites
{
    [MenuItem("Farm/Slice Farm Sprites")]
    public static void Execute()
    {
        // Trees: 2x2 grid, 1365x768 => 682x384 each
        // Inset 10px on all sides to remove white border Gemini added
        const int b = 10;
        SliceManual("Assets/Sprites/Farm_Trees.png", 1365, 768, new[]
        {
            ("tree_0", 0   + b, 384 + b, 682 - b*2, 384 - b*2),
            ("tree_1", 683 + b, 384 + b, 682 - b*2, 384 - b*2),
            ("tree_2", 0   + b, 0   + b, 682 - b*2, 384 - b*2),
            ("tree_3", 683 + b, 0   + b, 682 - b*2, 384 - b*2),
        });

        // Ground: horizontal strip 2928x352, 8 tiles => 366px each
        SliceManual("Assets/Sprites/Farm_Ground.png", 2928, 352, new[]
        {
            ("ground_grass",       0,    0, 366, 352),
            ("ground_grass_flower",366,  0, 366, 352),
            ("ground_tilled",      732,  0, 366, 352),
            ("ground_dry",         1098, 0, 366, 352),
            ("ground_water",       1464, 0, 366, 352),
            ("ground_water_edge",  1830, 0, 366, 352),
            ("ground_sand",        2196, 0, 366, 352),
            ("ground_stone",       2562, 0, 366, 352),
        });

        // Plants: 5 items horizontal, 1365x768 => 273px each
        SliceManual("Assets/Sprites/Farm_Plants.png", 1365, 768, new[]
        {
            ("plant_cattail",  0,   0, 273, 768),
            ("plant_lily",     273, 0, 273, 768),
            ("plant_bush",     546, 0, 273, 768),
            ("plant_grass",    819, 0, 273, 768),
            ("plant_mushroom", 1092,0, 273, 768),
        });

        // Deco: 3 rows, 1365x768 => row height 256
        SliceManual("Assets/Sprites/Farm_Deco.png", 1365, 768, new[]
        {
            ("deco_bridge",     0,           512, 1365, 256),
            ("deco_rock_small", 0,           256, 682,  256),
            ("deco_rock_large", 683,         256, 682,  256),
            ("deco_wheat_0",    0,           0,   455,  256),
            ("deco_wheat_1",    455,         0,   455,  256),
            ("deco_wheat_2",    910,         0,   455,  256),
        });

        AssetDatabase.Refresh();
        Debug.Log("All farm sprites sliced.");
    }

    static void SliceManual(string path, int texW, int texH,
        (string name, int x, int y, int w, int h)[] rects)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) { Debug.LogWarning("Not found: " + path); return; }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false;
        importer.maxTextureSize = 4096;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.spritePixelsPerUnit = 32;

        var metas = new List<SpriteMetaData>();
        foreach (var (name, x, y, w, h) in rects)
        {
            metas.Add(new SpriteMetaData
            {
                name      = name,
                rect      = new Rect(x, y, w, h),
                pivot     = new Vector2(0.5f, 0f),
                alignment = (int)SpriteAlignment.BottomCenter
            });
        }
        importer.spritesheet = metas.ToArray();
        EditorUtility.SetDirty(importer);
        importer.SaveAndReimport();
        Debug.Log($"Sliced {path}: {metas.Count} sprites");
    }
}
