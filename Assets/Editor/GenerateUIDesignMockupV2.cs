using UnityEngine;
using UnityEditor;
using System.IO;

/// Generates a UI design mockup matching the Gemini reference image style.
/// Cozy farm scene: wood-panel UI, inventory, task list, dialogue, top HUD.
public class GenerateUIDesignMockupV2
{
    [MenuItem("Tools/Generate UI Design Mockup V2")]
    public static void Execute()
    {
        int W = 1280, HH = 720;
        var tex = new Texture2D(W, HH, TextureFormat.RGBA32, false);

        // Palette
        var GRASS_LT   = H("#8DC644");
        var GRASS_DK   = H("#6B9E2A");
        var WATER_LT   = H("#5BB8E8");
        var WATER_DK   = H("#3A90C8");
        var WATER_FOAM = H("#A8D8F0");
        var CLOUD_W    = H("#FEFEFE");
        var DIRT_BG    = H("#C8A464");
        var DIRT_DK    = H("#A07840");
        var STONE_LT   = H("#B0A898");
        var STONE_DK   = H("#888078");

        var PAN_BG    = H("#EDD9A3");
        var PAN_MID   = H("#C8A464");
        var PAN_DRK   = H("#8B6340");
        var SLOT_BG   = H("#D4B870");
        var SLOT_BRD  = H("#9A7040");
        var SLOT_SEL  = H("#F0C040");
        var SEL_BRD   = H("#C89020");

        var TXT_DRK   = H("#3A2010");
        var TXT_MID   = H("#7A5030");
        var TXT_LT    = H("#F5E8C0");

        var HP_COL    = H("#E04040");
        var EN_COL    = H("#F0C030");
        var EXP_COL   = H("#58C038");
        var SEA_COL   = H("#5080D8");
        var COIN_COL  = H("#F0C030");
        var SEED_COL  = H("#70C040");
        var WOOD_COL  = H("#B07838");
        var STONE_COL = H("#909898");

        var TREE_DK   = H("#2A6018");
        var TREE_LT   = H("#4A8828");
        var TREE_MID  = H("#3A7020");
        var TREE_TRUNK= H("#6A4020");
        var BUSH_GRN  = H("#3A8020");
        var CROP_YEL  = H("#D8B830");
        var CROP_GRN  = H("#60A030");

        // === BACKGROUND ===
        // Sky gradient
        for (int y = 360; y < HH; y++)
        {
            float t = (float)(y - 360) / (HH - 360);
            var col = Color.Lerp(H("#87CEEB"), H("#B8E4F8"), t);
            for (int x = 0; x < W; x++) tex.SetPixel(x, y, col);
        }
        // Clouds
        DrawCloud(tex, 80,  620, 90,  32, CLOUD_W);
        DrawCloud(tex, 250, 650, 70,  26, CLOUD_W);
        DrawCloud(tex, 490, 640, 110, 34, CLOUD_W);
        DrawCloud(tex, 780, 630, 80,  28, CLOUD_W);
        DrawCloud(tex, 980, 655, 100, 30, CLOUD_W);
        DrawCloud(tex, 1170,640, 75,  26, CLOUD_W);
        // Ground
        FillR(tex, 0, 0, W, 360, GRASS_LT); // ground
        // Grass patches
        for (int i = 0; i < 40; i++)
        {
            int gx = (i * 137 + 30) % (W - 60);
            int gy = (i * 79  + 20) % 280;
            FillR(tex, gx, gy, 20 + (i%4)*10, 8, GRASS_DK);
        }
        // River
        FillR(tex, 0, 200, W, 80, WATER_LT);
        FillR(tex, 0, 225, W, 32, WATER_DK);
        for (int x = 0; x < W; x += 18)
        {
            int wv = (int)(Mathf.Sin(x * 0.09f) * 5);
            FillR(tex, x,   268 + wv, 10, 3, WATER_FOAM);
            FillR(tex, x+9, 205 + wv, 8,  2, WATER_FOAM);
        }
        FillR(tex, 0, 194, W, 8, GRASS_DK);
        FillR(tex, 0, 280, W, 8, GRASS_DK);

        // === SCENE ELEMENTS ===
        // Bridge
        DrawBridge(tex, W/2 - 36, 194, 72, 88, WOOD_COL, TXT_DRK);
        // Trees left
        DrawTree(tex, 55,  345, 60, TREE_LT, TREE_DK, TREE_TRUNK);
        DrawTree(tex, 125, 365, 52, TREE_MID, TREE_DK, TREE_TRUNK);
        DrawTree(tex, 38,  305, 44, TREE_DK, H("#1E4A10"), TREE_TRUNK);
        // Trees right
        DrawTree(tex, 955, 355, 64, TREE_LT, TREE_DK, TREE_TRUNK);
        DrawTree(tex, 1035,338, 55, TREE_MID, TREE_DK, TREE_TRUNK);
        DrawTree(tex, 1108,365, 48, TREE_DK, H("#1E4A10"), TREE_TRUNK);
        // Background trees
        DrawTree(tex, 195, 385, 40, TREE_MID, TREE_DK, TREE_TRUNK);
        DrawTree(tex, 848, 390, 42, TREE_LT,  TREE_DK, TREE_TRUNK);
        // Bushes
        DrawBush(tex, 255, 300, 28, BUSH_GRN);
        DrawBush(tex, 318, 292, 22, TREE_LT);
        DrawBush(tex, 888, 302, 25, BUSH_GRN);
        // Crops center
        DrawCropPlot(tex, 495, 315, 80, 60, DIRT_BG, DIRT_DK, CROP_YEL, CROP_GRN);
        // Stones near water
        DrawStone(tex, 748, 192, 22, 14, STONE_LT, STONE_DK);
        DrawStone(tex, 778, 286, 18, 12, STONE_LT, STONE_DK);
        DrawStone(tex, 418, 190, 16, 10, STONE_COL, STONE_DK);
        DrawStone(tex, 848, 190, 24, 14, STONE_LT,  STONE_DK);

        // === UI PANELS ===

        // 1. TOP BAR (56px)
        int tbH = 50;
        int tbY = HH - tbH;
        DrawWoodPanel(tex, 0, tbY, W, tbH, PAN_BG, PAN_MID, PAN_DRK, 3);
        // Season badge
        FillR(tex, 6,  tbY+4,  76, 34, PAN_DRK);
        FillR(tex, 8,  tbY+6,  72, 30, SEA_COL);
        DrawLabel(tex, 14, tbY+16, "SPRING", TXT_LT, 1);
        // Day + time
        DrawSBox(tex, 90, tbY+6, 64, 30, PAN_MID, PAN_DRK);
        DrawLabel(tex, 96, tbY+16, "Day  7", TXT_DRK, 1);
        DrawSBox(tex, 162, tbY+6, 64, 30, PAN_MID, PAN_DRK);
        DrawLabel(tex, 168, tbY+16, "06:30", TXT_DRK, 1);
        // Resources
        int rx = W/2 - 200;
        DrawResBadge(tex, rx,       tbY+6, PAN_BG, PAN_DRK, COIN_COL, "G 1,240", TXT_DRK);
        DrawResBadge(tex, rx + 126, tbY+6, PAN_BG, PAN_DRK, WOOD_COL, "W   480", TXT_DRK);
        DrawResBadge(tex, rx + 252, tbY+6, PAN_BG, PAN_DRK, STONE_COL,"S   320", TXT_DRK);
        DrawResBadge(tex, rx + 378, tbY+6, PAN_BG, PAN_DRK, SEED_COL, "F    16", TXT_DRK);
        // HP+EN bars
        int barsX = W - 210;
        DrawLabel(tex, barsX,    tbY+8,  "HP", TXT_DRK, 1);
        DrawHBar(tex, barsX+22,  tbY+8,  130, 13, 0.72f, HP_COL, H("#C0C0C0"), PAN_DRK);
        DrawLabel(tex, barsX,    tbY+27, "EP", EN_COL,  1);
        DrawHBar(tex, barsX+22,  tbY+27, 130, 13, 0.50f, EN_COL, H("#C0C0C0"), PAN_DRK);

        // 2. HOTBAR (bottom center, 9 slots × 40px)
        int hbSlot = 40, hbCount = 9;
        int hbW = hbCount * hbSlot + (hbCount-1)*3 + 20;
        int hbX = W/2 - hbW/2, hbY = 8;
        DrawWoodPanel(tex, hbX-8, hbY, hbW+16, hbSlot+18, PAN_BG, PAN_MID, PAN_DRK, 3);
        for (int i = 0; i < hbCount; i++)
        {
            int sx = hbX + i*(hbSlot+3);
            bool sel = i == 2;
            DrawSlot(tex, sx, hbY+8, hbSlot, hbSlot, sel ? SLOT_SEL : SLOT_BG, sel ? SEL_BRD : SLOT_BRD, 2);
            DrawLabel(tex, sx+2, hbY+9, (i+1).ToString(), TXT_MID, 1);
            if (i == 0) FillR(tex, sx+10, hbY+16, 20, 20, WOOD_COL);
            if (i == 1) FillR(tex, sx+10, hbY+16, 20, 20, STONE_COL);
            if (i == 2) FillR(tex, sx+10, hbY+16, 20, 20, SEED_COL);
        }

        // 3. TASK LIST panel (left)
        int lpX = 8, lpY = 110, lpW = 170, lpH = 200;
        DrawWoodPanel(tex, lpX, lpY, lpW, lpH, PAN_BG, PAN_MID, PAN_DRK, 3);
        FillR(tex, lpX+3, lpY+lpH-28, lpW-6, 25, PAN_MID);
        DrawLabel(tex, lpX+10, lpY+lpH-20, "TASKS", TXT_DRK, 2);
        FillR(tex, lpX+4, lpY+lpH-30, lpW-8, 2, PAN_DRK);
        var tasks = new[] { ("v","Water crops",true),("v","Harvest wheat",true),
                             (">","Build fence",false),(">","Catch fish",false),(">","Sleep",false) };
        for (int i = 0; i < tasks.Length; i++)
        {
            var (bullet,label,done) = tasks[i];
            int ty2 = lpY + lpH - 54 - i*22;
            DrawLabel(tex, lpX+8,  ty2, bullet, done ? EXP_COL : H("#D06020"), 1);
            DrawLabel(tex, lpX+22, ty2, label,  done ? TXT_MID : TXT_DRK, 1);
        }

        // 4. INVENTORY panel (right)
        int rpSlot = 36, rpCols = 5, rpRows = 4;
        int rpW = rpCols*rpSlot + (rpCols-1)*3 + 20;
        int rpH = rpRows*rpSlot + (rpRows-1)*3 + 46;
        int rpX = W - rpW - 8, rpY = 65;
        DrawWoodPanel(tex, rpX, rpY, rpW, rpH, PAN_BG, PAN_MID, PAN_DRK, 3);
        FillR(tex, rpX+3, rpY+rpH-28, rpW-6, 25, PAN_MID);
        DrawLabel(tex, rpX+8, rpY+rpH-20, "INVENTORY", TXT_DRK, 2);
        FillR(tex, rpX+4, rpY+rpH-30, rpW-8, 2, PAN_DRK);
        Color?[,] icons = {
            { SEED_COL, WOOD_COL, STONE_COL, COIN_COL, HP_COL },
            { EN_COL, CROP_YEL, WATER_LT, null, null },
            { null,null,null,null,null }, { null,null,null,null,null }
        };
        for (int row = 0; row < rpRows; row++)
        for (int col = 0; col < rpCols; col++)
        {
            int sx = rpX + 10 + col*(rpSlot+3);
            int sy = rpY + 8  + row*(rpSlot+3);
            DrawSlot(tex, sx, sy, rpSlot, rpSlot, SLOT_BG, SLOT_BRD, 2);
            if (icons[row,col].HasValue)
                DrawItemIcon(tex, sx+6, sy+6, rpSlot-12, icons[row,col].Value);
        }

        // 5. NOTIFICATION (top center)
        int nfW = 220, nfH = 48;
        int nfX = W/2 - nfW/2, nfY2 = tbY - nfH - 10;
        DrawWoodPanel(tex, nfX, nfY2, nfW, nfH, PAN_BG, PAN_MID, PAN_DRK, 3);
        FillR(tex, nfX+10, nfY2+18, 12, 12, EXP_COL);
        DrawLabel(tex, nfX+28, nfY2+12, "Wheat is ready!", TXT_DRK, 2);
        DrawLabel(tex, nfX+28, nfY2+29, "Tap to harvest",  TXT_MID, 1);

        // 6. DIALOGUE BOX (above hotbar)
        int dlgW = 500, dlgH = 90;
        int dlgX = W/2 - dlgW/2, dlgY = hbY + hbSlot + 24;
        DrawWoodPanel(tex, dlgX, dlgY, dlgW, dlgH, PAN_BG, PAN_MID, PAN_DRK, 3);
        DrawWoodPanel(tex, dlgX+8, dlgY+8, 68, 68, SLOT_BG, SLOT_BRD, PAN_DRK, 3);
        // Character silhouette
        FillR(tex, dlgX+24, dlgY+36, 18, 22, H("#D4A874"));
        FillR(tex, dlgX+28, dlgY+56, 12, 12, H("#F0C890"));
        DrawLabel(tex, dlgX+86, dlgY+18, "\"Good morning! The crops are",   TXT_DRK, 1);
        DrawLabel(tex, dlgX+86, dlgY+33, " looking great today...\"",        TXT_DRK, 1);
        DrawLabel(tex, dlgX+86, dlgY+52, "NPC: Old Farmer Joe",              TXT_MID, 1);
        DrawLabel(tex, dlgX+dlgW-20, dlgY+dlgH-18, "v", TXT_MID, 2);

        // EXP bar above dialogue
        int exW = 220, exY2 = dlgY + dlgH + 4;
        DrawLabel(tex, W/2 - exW/2, exY2+2, "Lv.5", TXT_DRK, 1);
        DrawHBar(tex, W/2 - exW/2 + 28, exY2, exW-28, 10, 0.62f, EXP_COL, H("#A0A0A0"), PAN_DRK);

        // Callout borders
        DrawCallout(tex, 0,      tbY,  W,    tbH,   H("#FF4444"), "1 TOP BAR");
        DrawCallout(tex, hbX-8,  hbY,  hbW+16, hbSlot+18, H("#FF8800"), "2 HOTBAR");
        DrawCallout(tex, lpX,    lpY,  lpW,  lpH,   H("#44CC44"), "3 TASKS");
        DrawCallout(tex, rpX,    rpY,  rpW,  rpH,   H("#4499FF"), "4 INVENTORY");
        DrawCallout(tex, nfX,    nfY2, nfW,  nfH,   H("#FF44FF"), "5 NOTIFICATION");
        DrawCallout(tex, dlgX,   dlgY, dlgW, dlgH,  H("#FFDD00"), "6 DIALOGUE");

        // Spec footer
        DrawLabel(tex, 10, 4,
            "Grid:16px  Scale:x2  Font:pixelFont-7-8x14  Pipeline:URP2D  1280x720",
            TXT_MID, 1);

        // Save
        tex.Apply();
        byte[] png = tex.EncodeToPNG();
        Object.DestroyImmediate(tex);
        string path = Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop),
            "FocusFarm_UI_Design_v2.png");
        File.WriteAllBytes(path, png);
        Debug.Log("[UIDesignMockupV2] Saved -> " + path);
        EditorUtility.RevealInFinder(path);
    }

    static void DrawCloud(Texture2D tex, int x, int y, int w, int h, Color c)
    {
        FillR(tex, x+h/3, y, w-h*2/3, h, c);
        int[] bumps = { w/6, w/2, w*5/6 };
        int[] sizes = { h/2+4, h/2+6, h/2+2 };
        for (int i = 0; i < 3; i++)
            FillOval(tex, x+bumps[i], y, sizes[i]*2, sizes[i]*2, c);
    }

    static void FillOval(Texture2D tex, int cx, int cy, int rw, int rh, Color c)
    {
        int hrw = rw/2, hrh = rh/2;
        for (int dy = -hrh; dy <= hrh; dy++)
        for (int dx = -hrw; dx <= hrw; dx++)
        {
            float nx = (float)dx/hrw, ny = (float)dy/hrh;
            if (nx*nx + ny*ny <= 1f)
                SetPx(tex, cx+dx, cy+dy, c);
        }
    }

    static void DrawTree(Texture2D tex, int cx, int baseY, int size, Color lt, Color dk, Color trunk)
    {
        int tw = size/4+2;
        FillR(tex, cx-tw/2, baseY, tw, size/3, trunk);
        int[] ts = { size, size*3/4, size/2 };
        for (int t = 0; t < 3; t++)
        {
            int fw = ts[t], fy = baseY + size/3 + t*(size/4);
            FillOval(tex, cx-4, fy+4, fw-4, fw*3/4-4, dk);
            FillOval(tex, cx,   fy,   fw,   fw*3/4,   t%2==0?lt:dk);
        }
    }

    static void DrawBush(Texture2D tex, int cx, int y, int size, Color c)
    {
        FillOval(tex, cx, y, size, size*2/3, c);
        FillOval(tex, cx-size/3, y, size*2/3, size/2, H("#2A6018"));
        FillOval(tex, cx+size/3, y, size*2/3, size/2, H("#2A6018"));
    }

    static void DrawCropPlot(Texture2D tex, int x, int y, int w, int h,
                              Color dirt, Color dk, Color cy2, Color cg)
    {
        FillR(tex, x, y, w, h, dirt);
        for (int i = 0; i < w; i+=4) FillR(tex, x+i, y,   2, 2, dk);
        for (int i = 0; i < w; i+=4) FillR(tex, x+i, y+h-2, 2, 2, dk);
        int cols=4, rows=3;
        for (int row=0; row<rows; row++) for (int col=0; col<cols; col++)
        {
            int bx=x+8+col*(w-16)/cols, by=y+6+row*(h-12)/rows;
            FillR(tex, bx+2, by,   3, 10, cg);
            FillR(tex, bx,   by+8, 7, 8,  cy2);
        }
    }

    static void DrawBridge(Texture2D tex, int x, int y, int w, int h, Color wood, Color dark)
    {
        for (int row=0; row<5; row++) { int py=y+row*(h/5); FillR(tex,x,py,w,h/5-2,wood); FillR(tex,x,py,w,2,dark); }
        FillR(tex, x,     y, 6, h, dark); FillR(tex, x+w-6, y, 6, h, dark);
        for (int i=0; i<3; i++) { int px=x+i*(w/2); FillR(tex,px,y+h,6,20,dark); FillR(tex,px,y-20,6,20,dark); }
    }

    static void DrawStone(Texture2D tex, int x, int y, int w, int h, Color lt, Color dk)
    {
        FillOval(tex, x+w/2, y+h/2, w, h, dk);
        FillOval(tex, x+w/2-2, y+h/2+2, w-4, h-4, lt);
    }

    static void DrawWoodPanel(Texture2D tex, int x, int y, int w, int h,
                               Color bg, Color mid, Color drk, int b)
    {
        FillR(tex, x, y, w, h, drk);
        FillR(tex, x+1, y+1, w-2, h-2, mid);
        FillR(tex, x+b, y+b, w-b*2, h-b*2, bg);
    }

    static void DrawSBox(Texture2D tex, int x, int y, int w, int h, Color bg, Color brd)
    { FillR(tex,x,y,w,h,brd); FillR(tex,x+2,y+2,w-4,h-4,bg); }

    static void DrawSlot(Texture2D tex, int x, int y, int w, int h, Color bg, Color brd, int b)
    {
        FillR(tex, x, y, w, h, brd);
        FillR(tex, x+b, y+b, w-b*2, h-b*2, bg);
        FillR(tex, x+b, y+h-b-2, w-b*2, 2, new Color(bg.r+0.15f,bg.g+0.15f,bg.b+0.15f,1));
    }

    static void DrawItemIcon(Texture2D tex, int x, int y, int size, Color c)
    {
        FillR(tex, x, y, size, size, c);
        var sh = new Color(c.r*0.7f,c.g*0.7f,c.b*0.7f,1);
        FillR(tex, x+size-size/3, y, size/3, size/3, sh);
        FillR(tex, x, y, size/3, size/3, new Color(Mathf.Min(c.r*1.3f,1),Mathf.Min(c.g*1.3f,1),Mathf.Min(c.b*1.3f,1),1));
    }

    static void DrawResBadge(Texture2D tex, int x, int y, Color bg, Color brd,
                              Color ic, string lbl, Color tc)
    { DrawSBox(tex,x,y,112,32,bg,brd); FillR(tex,x+6,y+7,16,16,ic); DrawLabel(tex,x+28,y+11,lbl,tc,1); }

    static void DrawHBar(Texture2D tex, int x, int y, int w, int h,
                          float fill, Color filled, Color empty, Color brd)
    {
        FillR(tex,x,y,w,h,brd);
        FillR(tex,x+2,y+2,w-4,h-4,empty);
        int fw=(int)((w-4)*Mathf.Clamp01(fill));
        if(fw>0) FillR(tex,x+2,y+2,fw,h-4,filled);
    }

    static void DrawCallout(Texture2D tex, int x, int y, int w, int h, Color c, string lbl)
    {
        var bc = new Color(c.r,c.g,c.b,0.9f);
        FillR(tex,x,  y,  w,2,bc); FillR(tex,x,y+h-2,w,2,bc);
        FillR(tex,x,  y,  2,h,bc); FillR(tex,x+w-2,y,2,h,bc);
        int lw=lbl.Length*6+8;
        FillR(tex,x+4,y+h-12,lw,12,bc);
        DrawLabel(tex,x+6,y+h-11,lbl,Color.black,1);
    }

    static void DrawLabel(Texture2D tex, int x, int y, string text, Color c, int scale)
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

    static void DrawChar5x7(Texture2D tex, int x, int y, char ch, Color c, int scale)
    {
        int idx=(int)ch-32;
        if(idx<0||idx>=FONT5x7.Length) return;
        var rows=FONT5x7[idx].Split(':');
        for(int row=0;row<7&&row<rows.Length;row++)
        {
            var r=rows[row]; int py=y+(6-row)*scale;
            for(int col=0;col<5&&col<r.Length;col++)
                if(r[col]=='1') FillR(tex,x+col*scale,py,scale,scale,c);
        }
    }

    static void FillR(Texture2D tex, int x, int y, int w, int h, Color c)
    {
        x=Mathf.Clamp(x,0,tex.width-1); y=Mathf.Clamp(y,0,tex.height-1);
        w=Mathf.Clamp(w,0,tex.width-x); h=Mathf.Clamp(h,0,tex.height-y);
        for(int dy=0;dy<h;dy++) for(int dx=0;dx<w;dx++) tex.SetPixel(x+dx,y+dy,c);
    }

    static void SetPx(Texture2D tex, int x, int y, Color c)
    { if(x>=0&&x<tex.width&&y>=0&&y<tex.height) tex.SetPixel(x,y,c); }

    static Color H(string hex)
    {
        hex=hex.TrimStart('#');
        return new Color(
            System.Convert.ToInt32(hex.Substring(0,2),16)/255f,
            System.Convert.ToInt32(hex.Substring(2,2),16)/255f,
            System.Convert.ToInt32(hex.Substring(4,2),16)/255f,1f);
    }
}
