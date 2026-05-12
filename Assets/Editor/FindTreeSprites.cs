using UnityEngine;
using UnityEditor;
using System.Linq;

public class FindTreeSprites
{
    const string SHEET = "Assets/Farm Sprite.png";

    [MenuItem("Tools/Find Tree Sprites")]
    public static void Execute()
    {
        var ti = AssetImporter.GetAtPath(SHEET) as TextureImporter;
        bool wasReadable = false;
        if (ti != null) { wasReadable = ti.isReadable; if (!wasReadable) { ti.isReadable = true; ti.SaveAndReimport(); } }

        var sprites = AssetDatabase.LoadAllAssetsAtPath(SHEET).OfType<Sprite>().ToArray();
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(SHEET);
        if (tex == null) { Debug.LogError("Texture not found"); return; }

        Debug.Log($"=== SPRITE COLORS ({sprites.Length}) ===");
        foreach (var sp in sprites)
        {
            var parts = sp.name.Split('_');
            if (!int.TryParse(parts[parts.Length - 1], out int idx)) continue;
            var rect = sp.rect;
            float cx = rect.x + rect.width / 2f;
            float cy = rect.y + rect.height / 2f;
            float r=0,g=0,b=0,a=0; int n=0;
            for (int dy=-2;dy<=2;dy++) for (int dx=-2;dx<=2;dx++) {
                var c=tex.GetPixel((int)cx+dx,(int)cy+dy); r+=c.r; g+=c.g; b+=c.b; a+=c.a; n++;
            }
            r/=n; g/=n; b/=n; a/=n;
            if (a < 0.15f) continue;
            int row=idx/16, col=idx%16;
            string dom;
            if (b>r+0.05f && b>g+0.05f) dom="BLUE";
            else if (r>0.45f && r>g*1.2f && r>b*1.8f) dom="RED";
            else if (r>0.35f && g>0.2f && b<0.15f && r>b*2f) dom="BROWN";
            else if (r>0.6f && g>0.4f && b<0.25f) dom="ORANGE/YEL";
            else if (r>0.7f && g>0.7f && b>0.7f) dom="WHITE";
            else if (g>r*1.05f && g>b*1.2f) dom="GREEN";
            else dom=$"MIX r{r:F2}g{g:F2}b{b:F2}";
            if (dom=="GREEN" && row>2) continue; // skip most greens
            Debug.Log($"idx={idx:3} r{row}c{col:2} R={r:F2} G={g:F2} B={b:F2} A={a:F2} [{dom}]");
        }
        Debug.Log("=== END ===");
        if (ti != null && !wasReadable) { ti.isReadable = false; ti.SaveAndReimport(); }
    }
}
