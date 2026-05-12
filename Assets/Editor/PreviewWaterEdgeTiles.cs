using UnityEngine;
using UnityEditor;
using System.Linq;

public class PreviewWaterEdgeTiles
{
    [MenuItem("Tools/Preview Water Edge Tiles")]
    public static void Execute()
    {
        string[] names = { "wc","wt","wb","wl","wr","wtl","wtr","wbl","wbr","witl","witr","wibl","wibr" };
        var sb = new System.Text.StringBuilder("Water edge tile sprites:\n");
        foreach (var name in names)
        {
            string path = $"Assets/Tiles/WaterEdgeSprites/{name}.png";
            var spr = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex == null) { sb.AppendLine($"  {name}: MISSING"); continue; }

            // Sample corners and center to understand what's in each tile
            var ti2 = AssetImporter.GetAtPath(path) as TextureImporter;
            bool wasR = ti2.isReadable;
            if (!wasR) { ti2.isReadable = true; ti2.SaveAndReimport(); tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path); }

            var c  = tex.GetPixel(8, 8);
            var tl = tex.GetPixel(0, 15);
            var tr = tex.GetPixel(15, 15);
            var bl = tex.GetPixel(0, 0);
            var br = tex.GetPixel(15, 0);

            string classify(Color col) => col.a < 0.1f ? "TRANSP" :
                col.b > 0.5f && col.b > col.r * 1.2f ? "WATER" :
                col.g > 0.4f && col.g > col.r * 1.1f ? "GRASS" :
                col.r > 0.45f && col.g > 0.3f && col.b < 0.4f ? "SAND" : "OTHER";

            sb.AppendLine($"  {name,-6}: center={classify(c)} tl={classify(tl)} tr={classify(tr)} bl={classify(bl)} br={classify(br)}  alpha_min={Mathf.Min(c.a,tl.a,tr.a,bl.a,br.a):F2}");

            if (!wasR) { ti2.isReadable = false; ti2.SaveAndReimport(); }
        }
        Debug.Log(sb.ToString());
    }
}
