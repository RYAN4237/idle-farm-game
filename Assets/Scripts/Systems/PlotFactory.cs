using UnityEngine;
using TMPro;

/// Creates a FarmPlot GameObject matching the Sprout Lands pixel art style.
public static class PlotFactory
{
    static Sprite _baseSprite;

    static Sprite GetSprite()
    {
        if (_baseSprite != null) return _baseSprite;
        var tex = new Texture2D(16, 16);
        tex.filterMode = FilterMode.Point;
        // Draw a simple pixel-art tilled dirt square
        for (int y = 0; y < 16; y++)
        for (int x = 0; x < 16; x++)
        {
            // Border pixels
            bool border = x == 0 || x == 15 || y == 0 || y == 15;
            // Inner cross marks (like tilled dirt)
            bool cross  = (x == 4 || x == 11) && (y > 3 && y < 12) ||
                          (y == 4 || y == 11) && (x > 3 && x < 12);
            Color c;
            if (border)      c = new Color(0.25f, 0.15f, 0.05f); // dark border
            else if (cross)  c = new Color(0.45f, 0.28f, 0.10f); // cross lines
            else             c = new Color(0.60f, 0.38f, 0.16f); // dirt fill
            tex.SetPixel(x, y, c);
        }
        tex.Apply();
        _baseSprite = Sprite.Create(tex, new Rect(0,0,16,16), new Vector2(0.5f,0.5f), 16f);
        return _baseSprite;
    }

    public static GameObject Create(Vector3 worldPos, float cellSize = 1f)
    {
        var go = new GameObject("FarmPlot");
        go.transform.position   = worldPos;
        go.transform.localScale = Vector3.one * cellSize * 0.96f; // slight inset

        // ── Shadow ────────────────────────────────────────────────────
        var shadow = new GameObject("Shadow");
        shadow.transform.SetParent(go.transform, false);
        shadow.transform.localPosition = new Vector3(0.03f, -0.03f, 0.01f);
        shadow.transform.localScale    = Vector3.one * 1.02f;
        var ssr = shadow.AddComponent<SpriteRenderer>();
        ssr.sprite       = GetSprite();
        ssr.color        = new Color(0.05f, 0.03f, 0f, 0.55f);
        ssr.sortingOrder = 0;

        // ── Main sprite ───────────────────────────────────────────────
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = GetSprite();
        sr.color        = new Color(0.60f, 0.38f, 0.16f); // tilled dirt colour
        sr.sortingOrder = 1;

        // ── Progress bar background ───────────────────────────────────
        var barBG = new GameObject("ProgressBarBG");
        barBG.transform.SetParent(go.transform, false);
        barBG.transform.localPosition = new Vector3(0f, -0.40f, -0.01f);
        barBG.transform.localScale    = new Vector3(0.85f, 0.09f, 1f);

        var barBGTex = new Texture2D(1,1);
        barBGTex.SetPixel(0,0, new Color(0.08f,0.05f,0.02f,0.9f));
        barBGTex.Apply();
        var barBGSpr = Sprite.Create(barBGTex, new Rect(0,0,1,1), new Vector2(0.5f,0.5f),1f);
        var bgsr = barBG.AddComponent<SpriteRenderer>();
        bgsr.sprite       = barBGSpr;
        bgsr.sortingOrder = 2;

        // ── Progress bar fill ─────────────────────────────────────────
        var barFill = new GameObject("ProgressBarFill");
        barFill.transform.SetParent(barBG.transform, false);
        barFill.transform.localPosition = new Vector3(-0.5f, 0f, -0.01f);
        barFill.transform.localScale    = new Vector3(0.001f, 1f, 1f);

        var fillTex = new Texture2D(1,1);
        fillTex.SetPixel(0,0, new Color(0.25f, 0.80f, 0.25f));
        fillTex.Apply();
        var fillSpr = Sprite.Create(fillTex, new Rect(0,0,1,1), new Vector2(0.5f,0.5f),1f);
        var fsr = barFill.AddComponent<SpriteRenderer>();
        fsr.sprite       = fillSpr;
        fsr.sortingOrder = 3;

        // ── Collider ──────────────────────────────────────────────────
        var col  = go.AddComponent<BoxCollider2D>();
        col.size = Vector2.one * 0.92f;

        // ── FarmPlot logic ────────────────────────────────────────────
        var plot              = go.AddComponent<FarmPlot>();
        plot.growthDuration   = 20f;
        plot.plantCost        = 10f;
        plot.harvestReward    = 30f;
        plot.emptyColor       = new Color(0.60f, 0.38f, 0.16f);
        plot.growingColor     = new Color(0.30f, 0.52f, 0.18f);
        plot.readyColor       = new Color(0.18f, 0.78f, 0.22f);

        Debug.Log($"[PlotFactory] Created FarmPlot at {worldPos}, size={cellSize}");
        return go;
    }
}
