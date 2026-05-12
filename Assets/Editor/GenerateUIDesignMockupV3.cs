using UnityEngine;
using UnityEditor;
using System.IO;

/// UI design mockup using actual Kenney UI sprites composited onto a farm scene background.
public class GenerateUIDesignMockupV3
{
    const string KENNEY = "Assets/KenneyUI/";

    [MenuItem("Tools/Generate UI Design Mockup V3 (Kenney)")]
    public static void Execute()
    {
        int W = 1280, HH = 720;
        var canvas = new Texture2D(W, HH, TextureFormat.RGBA32, false);

        // ── Load Kenney sprites ────────────────────────────────────────────────
        var panelBrown    = LoadTex(KENNEY + "panel_brown.png");
        var panelBeige    = LoadTex(KENNEY + "panel_beige.png");
        var slotBrown     = LoadTex(KENNEY + "panelInset_brown.png");
        var slotBeige     = LoadTex(KENNEY + "panelInset_beige.png");
        var btnBrown      = LoadTex(KENNEY + "buttonSquare_brown.png");
        var btnBrownSel   = LoadTex(KENNEY + "buttonSquare_beige.png");
        var barBackL      = LoadTex(KENNEY + "barBack_horizontalLeft.png");
        var barBackM      = LoadTex(KENNEY + "barBack_horizontalMid.png");
        var barBackR      = LoadTex(KENNEY + "barBack_horizontalRight.png");
        var barRedL       = LoadTex(KENNEY + "barRed_horizontalLeft.png");
        var barRedM       = LoadTex(KENNEY + "barRed_horizontalMid.png");
        var barGrnL       = LoadTex(KENNEY + "barGreen_horizontalLeft.png");
        var barGrnM       = LoadTex(KENNEY + "barGreen_horizontalMid.png");
        var barYelL       = LoadTex(KENNEY + "barYellow_horizontalLeft.png");
        var barYelM       = LoadTex(KENNEY + "barYellow_horizontalMid.png");
        var checkBronze   = LoadTex(KENNEY + "iconCheck_bronze.png");

        // ── Color palette ──────────────────────────────────────────────────────
        var SKY_TOP   = C("#87CEEB"); var SKY_BOT   = C("#B8E4F8");
        var CLOUD_W   = C("#FEFCFC");
        var GRASS_LT  = C("#7DB83A"); var GRASS_DK  = C("#5A9020");
        var WATER_LT  = C("#48A8D8"); var WATER_DK  = C("#2E80B0");
        var WATER_FOAM= C("#9CCCE8");
        var DIRT      = C("#B89050"); var DIRT_DK   = C("#8A6830");
        var STONE_LT  = C("#A8A098"); var STONE_DK  = C("#787068");
        var TREE_DK   = C("#225010"); var TREE_LT   = C("#3A7818");
        var TREE_MID  = C("#2E6414"); var TRUNK     = C("#5A3010");
        var BUSH      = C("#2A7010"); var CROP_Y    = C("#C8A820");
        var CROP_G    = C("#508828");
        var TXT_DRK   = C("#3A2010"); var TXT_MID   = C("#7A5030");
        var TXT_LT    = C("#F0E0C0"); var TXT_GRN   = C("#406020");

        // ── SCENE BACKGROUND ──────────────────────────────────────────────────
        // Sky
        for (int y = 340; y < HH; y++)
        {
            var col = Color.Lerp(SKY_TOP, SKY_BOT, (float)(y - 340) / (HH - 340));
            for (int x = 0; x < W; x++) canvas.SetPixel(x, y, col);
        }
        // Clouds
        PaintCloud(canvas, 70,  615, 100, 35, CLOUD_W);
        PaintCloud(canvas, 250, 645, 75,  28, CLOUD_W);
        PaintCloud(canvas, 500, 635, 115, 36, CLOUD_W);
        PaintCloud(canvas, 790, 622, 85,  30, CLOUD_W);
        PaintCloud(canvas, 990, 648, 105, 32, CLOUD_W);
        PaintCloud(canvas, 1175,632, 80,  28, CLOUD_W);
        // Ground
        FillR(canvas, 0, 0, W, 340, GRASS_LT);
        // Grass detail patches
        for (int i = 0; i < 45; i++)
        {
            int gx = (i * 139 + 28) % (W - 80);
            int gy = (i * 83  + 15) % 260;
            FillR(canvas, gx, gy, 16 + (i%5)*8, 6, GRASS_DK);
        }
        // River
        FillR(canvas, 0, 188, W, 84, WATER_LT);
        FillR(canvas, 0, 215, W, 34, WATER_DK);
        for (int x = 0; x < W; x += 20)
        {
            int wv = (int)(Mathf.Sin(x * 0.08f) * 6);
            FillR(canvas, x,    265 + wv, 12, 3, WATER_FOAM);
            FillR(canvas, x+10, 195 + wv, 9,  2, WATER_FOAM);
        }
        FillR(canvas, 0, 182, W, 8, GRASS_DK);
        FillR(canvas, 0, 272, W, 8, GRASS_DK);

        // ── SCENE ELEMENTS ────────────────────────────────────────────────────
        // Bridge (center crossing river)
        PaintBridge(canvas, W/2-38, 182, 76, 92, C("#9A6828"), C("#5A3810"));
        // Trees left cluster
        PaintTree(canvas, 50,  340, 64, TREE_LT, TREE_DK, TRUNK);
        PaintTree(canvas, 122, 358, 54, TREE_MID, TREE_DK, TRUNK);
        PaintTree(canvas, 35,  302, 46, TREE_DK, C("#182E08"), TRUNK);
        PaintTree(canvas, 175, 375, 40, TREE_MID, TREE_DK, TRUNK);
        // Trees right cluster
        PaintTree(canvas, 950, 348, 66, TREE_LT, TREE_DK, TRUNK);
        PaintTree(canvas, 1032,330, 56, TREE_MID, TREE_DK, TRUNK);
        PaintTree(canvas, 1108,358, 50, TREE_DK, C("#182E08"), TRUNK);
        PaintTree(canvas, 860, 382, 42, TREE_LT,  TREE_DK, TRUNK);
        // Bushes
        PaintBush(canvas, 252, 296, 30, BUSH);
        PaintBush(canvas, 315, 288, 24, TREE_LT);
        PaintBush(canvas, 882, 300, 27, BUSH);
        PaintBush(canvas, 720, 290, 22, TREE_MID);
        // Crops (center-right of bridge)
        PaintCropPlot(canvas, 490, 308, 84, 64, DIRT, DIRT_DK, CROP_Y, CROP_G);
        // Stones near water
        PaintStone(canvas, 740, 184, 24, 15, STONE_LT, STONE_DK);
        PaintStone(canvas, 774, 278, 19, 13, STONE_LT, STONE_DK);
        PaintStone(canvas, 412, 182, 17, 11, C("#989090"), STONE_DK);
        PaintStone(canvas, 844, 183, 26, 15, STONE_LT, STONE_DK);
        PaintStone(canvas, 380, 275, 20, 12, STONE_LT, STONE_DK);

        // ── UI PANELS (using real Kenney sprites) ─────────────────────────────

        // 1. TOP BAR — full-width, 52px tall
        int tbH = 52, tbY = HH - tbH;
        Blit9Slice(canvas, panelBrown, 0, tbY, W, tbH, 20);

        // Season badge
        FillR(canvas, 8, tbY+6, 78, 38, C("#3A68C8"));
        FillR(canvas, 7, tbY+5, 80, 40, C("#2A4898")); // border
        FillR(canvas, 8, tbY+6, 78, 38, C("#3A68C8"));
        DrawLabel(canvas, 15, tbY+18, "SPRING", TXT_LT, 2);

        // Day + time boxes
        BlitScaled(canvas, btnBrown, 94, tbY+6, 68, 38);
        DrawLabel(canvas, 101, tbY+18, "Day  7", TXT_DRK, 1);
        BlitScaled(canvas, btnBrown, 170, tbY+6, 68, 38);
        DrawLabel(canvas, 177, tbY+18, "06:30", TXT_DRK, 1);

        // Resources (4 badges)
        int rx = W/2 - 210;
        PaintResBadge(canvas, btnBrown, rx,       tbY+6, C("#E8B820"), "G 1,240", TXT_DRK);
        PaintResBadge(canvas, btnBrown, rx+132,   tbY+6, C("#9A6020"), "W   480", TXT_DRK);
        PaintResBadge(canvas, btnBrown, rx+264,   tbY+6, C("#909090"), "S   320", TXT_DRK);
        PaintResBadge(canvas, btnBrown, rx+396,   tbY+6, C("#60A820"), "F    16", TXT_DRK);

        // HP + Energy bars (right side)
        int barsX = W - 230;
        DrawLabel(canvas, barsX, tbY+9,  "HP", TXT_DRK, 1);
        PaintHBar(canvas, barBackL, barBackM, barBackR,
                          barRedL,  barRedM,  null,
                          barsX+24, tbY+9,  150, 16, 0.72f);
        DrawLabel(canvas, barsX, tbY+30, "EP", C("#C89000"), 1);
        PaintHBar(canvas, barBackL, barBackM, barBackR,
                          barYelL,  barYelM,  null,
                          barsX+24, tbY+30, 150, 16, 0.48f);

        // 2. HOTBAR — 9 slots × 44px
        int hbSlot=44, hbCount=9;
        int hbW = hbCount*hbSlot + (hbCount-1)*2 + 18;
        int hbX = W/2 - hbW/2, hbY = 8;
        Blit9Slice(canvas, panelBrown, hbX-8, hbY, hbW+16, hbSlot+18, 18);
        for (int i = 0; i < hbCount; i++)
        {
            int sx = hbX + i*(hbSlot+2);
            bool sel = i == 2;
            BlitScaled(canvas, sel ? btnBrownSel : btnBrown, sx, hbY+8, hbSlot, hbSlot);
            DrawLabel(canvas, sx+2, hbY+9, (i+1).ToString(), sel ? TXT_DRK : TXT_MID, 1);
            // Item icons: simple colored squares with shading
            if (i==0) PaintItemDot(canvas, sx+12, hbY+18, 20, C("#9A5820")); // wood
            if (i==1) PaintItemDot(canvas, sx+12, hbY+18, 20, C("#808888")); // stone
            if (i==2) PaintItemDot(canvas, sx+12, hbY+18, 20, C("#58A020")); // seed
        }

        // 3. TASK LIST panel (left)
        int lpW=175, lpH=210, lpX=8, lpY=100;
        Blit9Slice(canvas, panelBrown, lpX, lpY, lpW, lpH, 18);
        // Header strip (darker brown)
        FillR(canvas, lpX+4, lpY+lpH-32, lpW-8, 28, C("#7A4820"));
        DrawLabel(canvas, lpX+12, lpY+lpH-22, "TASKS", TXT_LT, 2);
        // Divider
        FillR(canvas, lpX+4, lpY+lpH-34, lpW-8, 2, C("#5A3010"));
        // Items
        var tasks = new[]{("v","Water crops",true),("v","Harvest wheat",true),
                          (">","Build fence",false),(">","Catch fish",false),(">","Sleep",false)};
        for (int i = 0; i < tasks.Length; i++)
        {
            var (b,lbl,done) = tasks[i];
            int ty2 = lpY + lpH - 58 - i*24;
            DrawLabel(canvas, lpX+8,  ty2, b,   done ? TXT_GRN : C("#C05818"), 1);
            DrawLabel(canvas, lpX+22, ty2, lbl, done ? TXT_MID : TXT_DRK, 1);
        }

        // 4. INVENTORY panel (right) — 5×4 grid
        int rpSlot=38, rpCols=5, rpRows=4;
        int rpW = rpCols*rpSlot+(rpCols-1)*3+20;
        int rpH = rpRows*rpSlot+(rpRows-1)*3+50;
        int rpX = W-rpW-8, rpY = 60;
        Blit9Slice(canvas, panelBrown, rpX, rpY, rpW, rpH, 18);
        FillR(canvas, rpX+4, rpY+rpH-32, rpW-8, 28, C("#7A4820"));
        DrawLabel(canvas, rpX+10, rpY+rpH-22, "INVENTORY", TXT_LT, 2);
        FillR(canvas, rpX+4, rpY+rpH-34, rpW-8, 2, C("#5A3010"));

        Color?[,] itemColors = {
            { C("#58A020"), C("#9A5820"), C("#808888"), C("#E8B820"), C("#D03030") },
            { C("#E0A020"), C("#C8A820"), C("#48A8D8"), null, null },
            { null,null,null,null,null },{ null,null,null,null,null }
        };
        for (int row = 0; row < rpRows; row++)
        for (int col = 0; col < rpCols; col++)
        {
            int sx = rpX+10+col*(rpSlot+3);
            int sy = rpY+8+row*(rpSlot+3);
            BlitScaled(canvas, slotBrown, sx, sy, rpSlot, rpSlot);
            if (itemColors[row,col].HasValue)
                PaintItemDot(canvas, sx+7, sy+7, rpSlot-14, itemColors[row,col].Value);
        }

        // 5. NOTIFICATION toast (top center)
        int nfW=228, nfH=50;
        int nfX=W/2-nfW/2, nfY=tbY-nfH-10;
        Blit9Slice(canvas, panelBeige, nfX, nfY, nfW, nfH, 16);
        FillR(canvas, nfX+10, nfY+19, 14, 14, C("#50A020"));
        DrawLabel(canvas, nfX+30, nfY+12, "Wheat is ready!", TXT_DRK, 2);
        DrawLabel(canvas, nfX+30, nfY+30, "Tap to harvest",  TXT_MID, 1);

        // 6. DIALOGUE BOX (above hotbar)
        int dlgW=510, dlgH=96;
        int dlgX=W/2-dlgW/2, dlgY=hbY+hbSlot+22;
        Blit9Slice(canvas, panelBrown, dlgX, dlgY, dlgW, dlgH, 18);
        // Portrait slot
        BlitScaled(canvas, slotBrown, dlgX+10, dlgY+10, 72, 72);
        // Simple NPC silhouette
        FillR(canvas, dlgX+26, dlgY+38, 20, 24, C("#C89060")); // body
        FillR(canvas, dlgX+30, dlgY+60, 14, 14, C("#E8B880")); // head
        FillR(canvas, dlgX+34, dlgY+72,  6, 6,  C("#C07840")); // hat brim hint
        // Text
        DrawLabel(canvas, dlgX+92, dlgY+18, "\"Good morning! The crops are", TXT_DRK, 1);
        DrawLabel(canvas, dlgX+92, dlgY+34,  " looking great today...\"",     TXT_DRK, 1);
        DrawLabel(canvas, dlgX+92, dlgY+54, "NPC: Old Farmer Joe", TXT_MID, 1);
        // Arrow
        DrawLabel(canvas, dlgX+dlgW-24, dlgY+dlgH-20, "v", TXT_MID, 2);

        // EXP bar
        int exW=220, exY=dlgY+dlgH+4, exX=W/2-exW/2;
        DrawLabel(canvas, exX, exY+3, "Lv.5", TXT_DRK, 1);
        PaintHBar(canvas, barBackL, barBackM, barBackR,
                          barGrnL, barGrnM, null,
                          exX+30, exY, exW-30, 14, 0.62f);

        // ── SPEC FOOTER ────────────────────────────────────────────────────────
        DrawLabel(canvas, 10, 4,
            "Grid:16px  Scale:x2  Font:m5x7  Sprites:KenneyUI(CC0)  Pipeline:URP2D  1280x720",
            TXT_MID, 1);

        // ── CALLOUT BORDERS ────────────────────────────────────────────────────
        Callout(canvas, 0,      tbY,  W,      tbH,         C("#FF4040"), "1 TOP BAR");
        Callout(canvas, hbX-8,  hbY,  hbW+16, hbSlot+18,  C("#FF9020"), "2 HOTBAR");
        Callout(canvas, lpX,    lpY,  lpW,    lpH,         C("#30CC30"), "3 TASKS");
        Callout(canvas, rpX,    rpY,  rpW,    rpH,         C("#3090FF"), "4 INVENTORY");
        Callout(canvas, nfX,    nfY,  nfW,    nfH,         C("#FF30FF"), "5 NOTIFICATION");
        Callout(canvas, dlgX,   dlgY, dlgW,   dlgH,        C("#F8D800"), "6 DIALOGUE");

        // ── SAVE ───────────────────────────────────────────────────────────────
        canvas.Apply();
        byte[] png = canvas.EncodeToPNG();
        Object.DestroyImmediate(canvas);
        string path = Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop),
            "FocusFarm_UI_Design_v3.png");
        File.WriteAllBytes(path, png);
        Debug.Log("[UIDesignMockupV3] Saved -> " + path);
        EditorUtility.RevealInFinder(path);
    }

    // ── Sprite compositing ─────────────────────────────────────────────────────

    /// Load a texture from disk (not through AssetDatabase — bypasses import pipeline)
    static Texture2D LoadTex(string assetPath)
    {
        string full = Path.Combine(Application.dataPath.Replace("Assets",""), assetPath);
        if (!File.Exists(full)) { Debug.LogWarning("Missing: " + assetPath); return null; }
        var t = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        t.LoadImage(File.ReadAllBytes(full));
        return t;
    }

    /// Blit src scaled to fill dst rect (bilinear)
    static void BlitScaled(Texture2D dst, Texture2D src, int dx, int dy, int dw, int dh)
    {
        if (src == null) return;
        for (int y = 0; y < dh; y++)
        for (int x = 0; x < dw; x++)
        {
            float u = (float)x / (dw - 1);
            float v = (float)y / (dh - 1);
            var col = src.GetPixelBilinear(u, v);
            if (col.a > 0.05f) AlphaBlit(dst, dx+x, dy+y, col);
        }
    }

    /// 9-slice blit: corners are fixed size, middle is stretched
    static void Blit9Slice(Texture2D dst, Texture2D src, int dx, int dy, int dw, int dh, int border)
    {
        if (src == null) return;
        int sw = src.width, sh = src.height;
        int b = border;
        // We split into 9 regions and blit each
        // corners
        BlitRegionScaled(dst, src, dx,       dy,       b,       b,       0,    0,    b,   b);
        BlitRegionScaled(dst, src, dx+dw-b,  dy,       b,       b,       sw-b, 0,    b,   b);
        BlitRegionScaled(dst, src, dx,       dy+dh-b,  b,       b,       0,    sh-b, b,   b);
        BlitRegionScaled(dst, src, dx+dw-b,  dy+dh-b,  b,       b,       sw-b, sh-b, b,   b);
        // edges
        BlitRegionScaled(dst, src, dx+b,     dy,       dw-b*2,  b,       b,    0,    sw-b*2, b);
        BlitRegionScaled(dst, src, dx+b,     dy+dh-b,  dw-b*2,  b,       b,    sh-b, sw-b*2, b);
        BlitRegionScaled(dst, src, dx,       dy+b,     b,       dh-b*2,  0,    b,    b,      sh-b*2);
        BlitRegionScaled(dst, src, dx+dw-b,  dy+b,     b,       dh-b*2,  sw-b, b,    b,      sh-b*2);
        // center
        BlitRegionScaled(dst, src, dx+b,     dy+b,     dw-b*2,  dh-b*2,  b,    b,    sw-b*2, sh-b*2);
    }

    /// Blit a sub-region of src (sx,sy,sw,sh) scaled to dst rect (dx,dy,dw,dh)
    static void BlitRegionScaled(Texture2D dst, Texture2D src,
                                  int dx, int dy, int dw, int dh,
                                  int sx, int sy, int sw, int sh)
    {
        if (dw <= 0 || dh <= 0 || sw <= 0 || sh <= 0) return;
        for (int y = 0; y < dh; y++)
        for (int x = 0; x < dw; x++)
        {
            float u = sw <= 1 ? 0 : (float)x / (dw - 1) * (sw - 1);
            float v = sh <= 1 ? 0 : (float)y / (dh - 1) * (sh - 1);
            int px = Mathf.Clamp(sx + Mathf.RoundToInt(u), 0, src.width  - 1);
            int py = Mathf.Clamp(sy + Mathf.RoundToInt(v), 0, src.height - 1);
            var col = src.GetPixel(px, py);
            if (col.a > 0.05f) AlphaBlit(dst, dx+x, dy+y, col);
        }
    }

    static void AlphaBlit(Texture2D dst, int x, int y, Color src)
    {
        if (x < 0 || x >= dst.width || y < 0 || y >= dst.height) return;
        var bg = dst.GetPixel(x, y);
        float a = src.a;
        dst.SetPixel(x, y, new Color(
            bg.r*(1-a)+src.r*a,
            bg.g*(1-a)+src.g*a,
            bg.b*(1-a)+src.b*a, 1));
    }

    static void PaintHBar(Texture2D dst,
                           Texture2D backL, Texture2D backM, Texture2D backR,
                           Texture2D fillL, Texture2D fillM, Texture2D fillR,
                           int dx, int dy, int dw, int dh, float fill)
    {
        int capW = dh; // square end caps
        // Background
        BlitScaled(dst, backL, dx,          dy, capW, dh);
        BlitScaled(dst, backM, dx+capW,     dy, dw-capW*2, dh);
        BlitScaled(dst, backR, dx+dw-capW,  dy, capW, dh);
        // Fill
        int fw = Mathf.RoundToInt((dw - capW*2) * Mathf.Clamp01(fill));
        if (fw > 0)
        {
            BlitScaled(dst, fillL, dx+capW, dy, Mathf.Min(capW, fw), dh);
            if (fw > capW)
                BlitScaled(dst, fillM, dx+capW*2, dy, fw-capW, dh);
        }
    }

    // ── Scene painters ─────────────────────────────────────────────────────────

    static void PaintCloud(Texture2D tex, int x, int y, int w, int h, Color c)
    {
        FillR(tex, x+h/3, y, w-h*2/3, h, c);
        int[] bx = { w/6, w/2, w*5/6 };
        int[] br = { h/2+5, h/2+7, h/2+3 };
        for (int i=0;i<3;i++) FillOval(tex,x+bx[i],y,br[i]*2,br[i]*2,c);
    }

    static void FillOval(Texture2D tex, int cx, int cy, int rw, int rh, Color c)
    {
        int hrw=rw/2,hrh=rh/2;
        for (int dy=-hrh;dy<=hrh;dy++)
        for (int dx=-hrw;dx<=hrw;dx++)
        {
            float nx=(float)dx/hrw,ny=(float)dy/hrh;
            if (nx*nx+ny*ny<=1f) SetPx(tex,cx+dx,cy+dy,c);
        }
    }

    static void PaintTree(Texture2D tex, int cx, int baseY, int size, Color lt, Color dk, Color trunk)
    {
        int tw=size/4+2;
        FillR(tex,cx-tw/2,baseY,tw,size/3,trunk);
        int[] ts={size,size*3/4,size/2};
        for(int t=0;t<3;t++){
            int fw=ts[t],fy=baseY+size/3+t*(size/4);
            FillOval(tex,cx-3,fy+4,fw-4,(fw-4)*3/4,dk);
            FillOval(tex,cx,fy,fw,fw*3/4,t%2==0?lt:dk);
        }
    }

    static void PaintBush(Texture2D tex, int cx, int y, int size, Color c)
    {
        FillOval(tex,cx,y,size,size*2/3,c);
        FillOval(tex,cx-size/3,y,size*2/3,size/2,C("#1E5010"));
        FillOval(tex,cx+size/3,y,size*2/3,size/2,C("#1E5010"));
    }

    static void PaintCropPlot(Texture2D tex,int x,int y,int w,int h,Color dirt,Color dk,Color cy2,Color cg)
    {
        FillR(tex,x,y,w,h,dirt);
        for(int i=0;i<w;i+=4){FillR(tex,x+i,y,2,2,dk);FillR(tex,x+i,y+h-2,2,2,dk);}
        int cols=4,rows=3;
        for(int row=0;row<rows;row++) for(int col=0;col<cols;col++){
            int bx=x+8+col*(w-16)/cols,by=y+6+row*(h-12)/rows;
            FillR(tex,bx+2,by,3,10,cg); FillR(tex,bx,by+8,7,8,cy2);
        }
    }

    static void PaintBridge(Texture2D tex,int x,int y,int w,int h,Color wood,Color dark)
    {
        for(int row=0;row<5;row++){int py=y+row*(h/5);FillR(tex,x,py,w,h/5-2,wood);FillR(tex,x,py,w,2,dark);}
        FillR(tex,x,y,6,h,dark);FillR(tex,x+w-6,y,6,h,dark);
        for(int i=0;i<3;i++){int px=x+i*(w/2);FillR(tex,px,y+h,6,22,dark);FillR(tex,px,y-22,6,22,dark);}
    }

    static void PaintStone(Texture2D tex,int x,int y,int w,int h,Color lt,Color dk)
    {
        FillOval(tex,x+w/2,y+h/2,w,h,dk);
        FillOval(tex,x+w/2-2,y+h/2+2,w-4,h-4,lt);
    }

    static void PaintItemDot(Texture2D tex,int x,int y,int size,Color c)
    {
        FillR(tex,x,y,size,size,c);
        FillR(tex,x+size-size/3,y,size/3,size/3,new Color(c.r*.65f,c.g*.65f,c.b*.65f,1));
        FillR(tex,x,y+size-size/3,size/3,size/3,new Color(Mathf.Min(c.r*1.35f,1),Mathf.Min(c.g*1.35f,1),Mathf.Min(c.b*1.35f,1),1));
    }

    static void PaintResBadge(Texture2D tex,Texture2D btn,int x,int y,Color ic,string lbl,Color tc)
    {
        BlitScaled(tex,btn,x,y,120,38);
        FillR(tex,x+7,y+10,16,16,ic);
        DrawLabel(tex,x+30,y+12,lbl,tc,1);
    }

    static void Callout(Texture2D tex,int x,int y,int w,int h,Color c,string lbl)
    {
        var bc=new Color(c.r,c.g,c.b,.9f);
        FillR(tex,x,y,w,2,bc);FillR(tex,x,y+h-2,w,2,bc);
        FillR(tex,x,y,2,h,bc);FillR(tex,x+w-2,y,2,h,bc);
        int lw=lbl.Length*6+8;
        FillR(tex,x+4,y+h-13,lw,13,bc);
        DrawLabel(tex,x+6,y+h-12,lbl,Color.black,1);
    }

    // ── Primitives ─────────────────────────────────────────────────────────────

    static void FillR(Texture2D tex,int x,int y,int w,int h,Color c)
    {
        x=Mathf.Clamp(x,0,tex.width-1);y=Mathf.Clamp(y,0,tex.height-1);
        w=Mathf.Clamp(w,0,tex.width-x);h=Mathf.Clamp(h,0,tex.height-y);
        for(int dy=0;dy<h;dy++) for(int dx=0;dx<w;dx++) tex.SetPixel(x+dx,y+dy,c);
    }

    static void SetPx(Texture2D tex,int x,int y,Color c)
    { if(x>=0&&x<tex.width&&y>=0&&y<tex.height) tex.SetPixel(x,y,c); }

    // ── Pixel font ─────────────────────────────────────────────────────────────
    static void DrawLabel(Texture2D tex,int x,int y,string text,Color c,int scale)
    { int cx=x; foreach(char ch in text){DrawChar5x7(tex,cx,y,ch,c,scale);cx+=(5+1)*scale;} }

    static readonly string[] FONT5x7 = {
        "00000:00000:00000:00000:00000:00000:00000","00100:00100:00100:00100:00000:00100:00000",
        "01010:01010:00000:00000:00000:00000:00000","01010:11111:01010:11111:01010:00000:00000",
        "01110:10100:01110:00101:11110:00100:00000","11000:11001:00010:00100:01001:00011:00000",
        "01100:10010:01100:10011:10010:01101:00000","00100:00100:00000:00000:00000:00000:00000",
        "00110:01100:01000:01000:01000:01100:00110","11000:00110:00010:00010:00010:00110:11000",
        "00000:00100:10101:01110:10101:00100:00000","00000:00100:00100:11111:00100:00100:00000",
        "00000:00000:00000:00000:00110:00100:01000","00000:00000:00000:11111:00000:00000:00000",
        "00000:00000:00000:00000:00000:01100:01100","00001:00010:00010:00100:01000:01000:10000",
        "01110:10001:10011:10101:11001:10001:01110","00100:01100:00100:00100:00100:00100:01110",
        "01110:10001:00001:00110:01000:10000:11111","11110:00001:00001:01110:00001:00001:11110",
        "00010:00110:01010:10010:11111:00010:00010","11111:10000:11110:00001:00001:10001:01110",
        "00110:01000:10000:11110:10001:10001:01110","11111:00001:00010:00100:01000:01000:01000",
        "01110:10001:10001:01110:10001:10001:01110","01110:10001:10001:01111:00001:00010:01100",
        "00000:01100:01100:00000:01100:01100:00000","00000:01100:01100:00000:01100:00100:01000",
        "00010:00100:01000:10000:01000:00100:00010","00000:00000:11111:00000:11111:00000:00000",
        "10000:01000:00100:00010:00100:01000:10000","01110:10001:00001:00110:00100:00000:00100",
        "01110:10001:10111:10101:10110:10000:01111","01110:10001:10001:11111:10001:10001:10001",
        "11110:10001:10001:11110:10001:10001:11110","01110:10001:10000:10000:10000:10001:01110",
        "11100:10010:10001:10001:10001:10010:11100","11111:10000:10000:11110:10000:10000:11111",
        "11111:10000:10000:11110:10000:10000:10000","01110:10001:10000:10111:10001:10001:01111",
        "10001:10001:10001:11111:10001:10001:10001","01110:00100:00100:00100:00100:00100:01110",
        "00111:00010:00010:00010:00010:10010:01100","10001:10010:10100:11000:10100:10010:10001",
        "10000:10000:10000:10000:10000:10000:11111","10001:11011:10101:10101:10001:10001:10001",
        "10001:10001:11001:10101:10011:10001:10001","01110:10001:10001:10001:10001:10001:01110",
        "11110:10001:10001:11110:10000:10000:10000","01110:10001:10001:10001:10101:10010:01101",
        "11110:10001:10001:11110:10100:10010:10001","01111:10000:10000:01110:00001:00001:11110",
        "11111:00100:00100:00100:00100:00100:00100","10001:10001:10001:10001:10001:10001:01110",
        "10001:10001:10001:10001:01010:01010:00100","10001:10001:10101:10101:10101:11011:10001",
        "10001:10001:01010:00100:01010:10001:10001","10001:10001:01010:00100:00100:00100:00100",
        "11111:00001:00010:00100:01000:10000:11111","01110:01000:01000:01000:01000:01000:01110",
        "10000:01000:01000:00100:00010:00010:00001","01110:00010:00010:00010:00010:00010:01110",
        "00100:01010:10001:00000:00000:00000:00000","00000:00000:00000:00000:00000:00000:11111",
        "01000:00100:00000:00000:00000:00000:00000","00000:00000:01110:00001:01111:10001:01111",
        "10000:10000:11110:10001:10001:10001:11110","00000:00000:01110:10000:10000:10001:01110",
        "00001:00001:01111:10001:10001:10001:01111","00000:00000:01110:10001:11111:10000:01110",
        "00110:01001:01000:11110:01000:01000:01000","00000:00000:01111:10001:01111:00001:11110",
        "10000:10000:11110:10001:10001:10001:10001","00100:00000:01100:00100:00100:00100:01110",
        "00010:00000:00110:00010:00010:10010:01100","10000:10000:10010:10100:11000:10100:10010",
        "01100:00100:00100:00100:00100:00100:01110","00000:00000:11010:10101:10101:10001:10001",
        "00000:00000:11110:10001:10001:10001:10001","00000:00000:01110:10001:10001:10001:01110",
        "00000:00000:11110:10001:11110:10000:10000","00000:00000:01111:10001:01111:00001:00001",
        "00000:00000:10110:11001:10000:10000:10000","00000:00000:01110:10000:01110:00001:11110",
        "01000:01000:11110:01000:01000:01001:00110","00000:00000:10001:10001:10001:10011:01101",
        "00000:00000:10001:10001:10001:01010:00100","00000:00000:10001:10101:10101:10101:01010",
        "00000:00000:10001:01010:00100:01010:10001","00000:00000:10001:10001:01111:00001:11110",
        "00000:00000:11111:00010:00100:01000:11111",
    };

    static void DrawChar5x7(Texture2D tex,int x,int y,char ch,Color c,int scale)
    {
        int idx=(int)ch-32;
        if(idx<0||idx>=FONT5x7.Length) return;
        var rows=FONT5x7[idx].Split(':');
        for(int row=0;row<7&&row<rows.Length;row++){
            var r=rows[row];int py=y+(6-row)*scale;
            for(int col=0;col<5&&col<r.Length;col++)
                if(r[col]=='1') FillR(tex,x+col*scale,py,scale,scale,c);
        }
    }

    static Color C(string hex)
    {
        hex=hex.TrimStart('#');
        return new Color(
            System.Convert.ToInt32(hex.Substring(0,2),16)/255f,
            System.Convert.ToInt32(hex.Substring(2,2),16)/255f,
            System.Convert.ToInt32(hex.Substring(4,2),16)/255f,1f);
    }
}
