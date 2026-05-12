using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

/// Best-quality UI mockup — top-left origin (y=0 top, y=720 bottom)
public class GenerateUIDesignMockupV4
{
    const int W = 1280, HH = 720;
    static string ProjectRoot => Application.dataPath.Replace("/Assets", "");

    [MenuItem("Tools/Generate UI Mockup V4 (Best)")]
    public static void Execute()
    {
        var canvas = NewTex(W, HH);

        DrawSky(canvas);
        DrawClouds(canvas);
        DrawGround(canvas);
        DrawRiver(canvas);
        DrawTrees(canvas);
        DrawCrops(canvas);
        DrawTopHUD(canvas);
        DrawQuestPanel(canvas);
        DrawHotbar(canvas);
        DrawInventory(canvas);
        DrawDialogue(canvas);
        DrawResourceBadges(canvas);
        DrawMinimap(canvas);
        DrawLegend(canvas);

        canvas.Apply();
        byte[] png = canvas.EncodeToPNG();
        string outPath = Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop),
            "FocusFarm_UI_Design_v4.png");
        File.WriteAllBytes(outPath, png);
        Debug.Log($"[MockupV4] Saved → {outPath}");
        EditorUtility.RevealInFinder(outPath);
    }

    // ── Scene background ────────────────────────────────────────────────────
    // Coordinate system: y=0 top, y=HH bottom (top-left origin)
    // Screen zones: sky y=0..420, ground y=380..560, underground y=560..720

    static void DrawSky(Texture2D c)
    {
        // Sky gradient top=deep blue, bottom=light cyan
        for (int y = 0; y < 460; y++)
        {
            float t = y / 460f;
            Color col = Color.Lerp(H("#5BA8D4"), H("#C8E8F0"), t);
            for (int x = 0; x < W; x++) P(c, x, y, col);
        }
        // Underground / dark soil at bottom
        for (int y = 560; y < HH; y++)
        {
            float t = (y - 560f) / (HH - 560f);
            Color col = Color.Lerp(H("#3a1a05"), H("#1a0a00"), t);
            for (int x = 0; x < W; x++) P(c, x, y, col);
        }
    }

    static void DrawClouds(Texture2D c)
    {
        DrawCloud(c, 120, 80, 140, 38);
        DrawCloud(c, 400, 55, 110, 28);
        DrawCloud(c, 700, 70, 160, 42);
        DrawCloud(c, 1050, 65, 120, 32);
    }

    static void DrawCloud(Texture2D c, int cx, int cy, int w, int h)
    {
        for (int dx = -w/2; dx < w/2; dx++)
        for (int dy = -h/2; dy < h/2; dy++)
        {
            float ex = dx/(w/2f), ey = dy/(h/2f);
            if (ex*ex + ey*ey <= 1f) AlphaBlit(c, cx+dx, cy+dy, new Color(1f,1f,1f,0.88f));
        }
        // Puffy top bump
        for (int dx = -w/3; dx < w/3; dx++)
        for (int dy = -h*2/3; dy < 0; dy++)
        {
            float ex = dx/(w/3f), ey = dy/(h/2f);
            if (ex*ex + ey*ey <= 1f) AlphaBlit(c, cx+dx, cy+dy, new Color(1f,1f,1f,0.88f));
        }
    }

    static void DrawGround(Texture2D c)
    {
        // Grass band y=400..560
        for (int y = 400; y < 560; y++)
        {
            float t = (y-400f)/160f;
            Color col = Color.Lerp(H("#5a9a30"), H("#3d7020"), t);
            for (int x = 0; x < W; x++) P(c, x, y, col);
        }
        // Dirt path center
        for (int y = 480; y < 560; y++)
        for (int x = 480; x < 650; x++)
        {
            Color col = ((x+y)%8<4) ? H("#9a6a38") : H("#a87040");
            P(c, x, y, col);
        }
    }

    static void DrawRiver(Texture2D c)
    {
        for (int x = 0; x < W; x++)
        {
            float t = x / (float)W;
            int ry = 440 + (int)(Mathf.Sin(t * 7f) * 12f);
            for (int dy = -16; dy < 16; dy++)
            {
                float alpha = 1f - Mathf.Abs(dy)/16f;
                AlphaBlit(c, x, ry+dy, new Color(0.25f, 0.58f, 0.85f, alpha*0.9f));
            }
            if (x % 22 < 5)
                for (int dy = -8; dy < 8; dy++)
                    AlphaBlit(c, x, ry+dy, new Color(1f,1f,1f,0.1f));
        }
    }

    static void DrawTrees(Texture2D c)
    {
        int[] xs = { 55, 165, 285, 830, 970, 1110, 1210 };
        foreach (int tx in xs) DrawTree(c, tx, 395);
    }

    static void DrawTree(Texture2D c, int tx, int ty)
    {
        FillRect(c, tx-5, ty+10, 11, 38, H("#7a4a20")); // trunk downward
        Ellipse(c, tx,    ty,    34, 26, H("#2d6e1a"));
        Ellipse(c, tx-17, ty+10, 26, 20, H("#3a8a25"));
        Ellipse(c, tx+17, ty+10, 26, 20, H("#3a8a25"));
    }

    static void DrawCrops(Texture2D c)
    {
        Color[] cc = { H("#88cc44"), H("#55aa22"), H("#aadd55"), H("#66bb33") };
        for (int row = 0; row < 3; row++)
        for (int col = 0; col < 6; col++)
        {
            int cx = 340 + col*22, cy = 510 + row*18;
            FillRect(c, cx-3, cy+2, 6, 10, H("#5a3010"));
            Ellipse(c, cx, cy, 7, 5, cc[(row+col)%4]);
        }
    }

    // ── HUD elements (top-left origin coords) ───────────────────────────────

    static void DrawTopHUD(Texture2D c)
    {
        // HUD bar: y=0..64
        var panel = LoadTex("Assets/KenneyUI/panel_brown.png");
        if (panel != null) Blit9Slice(c, panel, 0, 0, W, 64, 10);
        else FillRect(c, 0, 0, W, 64, new Color(0.28f,0.16f,0.06f,0.97f));

        // Player avatar
        FillCircle(c, 196, 32, 24, H("#5a3010"));
        FillCircle(c, 196, 32, 22, H("#ffd8b0"));
        FillRect(c, 190, 26, 4, 4, H("#222222")); // left eye
        FillRect(c, 199, 26, 4, 4, H("#222222")); // right eye
        FillRect(c, 188, 16, 22, 4, H("#7a4a20")); // hair
        FillRect(c, 191, 38, 10, 2, H("#c07058")); // mouth

        // HP/EN/XP bars  x=20, y=10,28,46
        DrawBarGroup(c, 20, 10, 160, 12, 0.72f, H("#e84040"), "HP");
        DrawBarGroup(c, 20, 26, 160, 12, 0.55f, H("#f0c020"), "EN");
        DrawBarGroup(c, 20, 42, 160, 12, 0.33f, H("#44cc88"), "XP");

        // Day/time center
        DrawLabel(c, W/2-75, 14, "Day 12  *  Spring", H("#F5C542"), 2);
        DrawLabel(c, W/2-36, 38, "08:45 AM", H("#F5EDDC"), 1);

        // Settings gear top-right
        var gear = LoadTex("Assets/KenneyIcons/gear.png");
        if (gear != null) BlitTinted(c, gear, W-52, 16, 32, 32, H("#F5C542"));
        else FillRect(c, W-52, 16, 32, 32, H("#F5C542"));
    }

    static void DrawBarGroup(Texture2D c, int x, int y, int w, int h, float fill, Color fillCol, string label)
    {
        var bL = LoadTex("Assets/KenneyUI/barBack_horizontalLeft.png");
        var bM = LoadTex("Assets/KenneyUI/barBack_horizontalMid.png");
        var bR = LoadTex("Assets/KenneyUI/barBack_horizontalRight.png");
        int bx = x+24;
        if (bL != null) { PaintHBar(c, bL, bM, bR, null, null, null, bx, y, w-24, h, 0f); }
        else FillRect(c, bx, y, w-24, h, H("#333333"));
        int fw = (int)((w-32)*fill);
        if (fw > 0) FillRect(c, bx+4, y+2, fw, h-4, fillCol);
        DrawLabel(c, x, y+2, label, H("#F5EDDC"), 1);
    }

    static void DrawQuestPanel(Texture2D c)
    {
        // Left panel y=72..380, x=12..232
        int px=12, py=72, pw=220, ph=300;
        var panel = LoadTex("Assets/KenneyFantasyUI/Panel/panel-000.png");
        if (panel != null) Blit9Slice(c, panel, px, py, pw, ph, 16);
        else { FillRect(c,px,py,pw,ph,new Color(0.22f,0.12f,0.04f,0.95f)); DrawBorder(c,px,py,pw,ph,H("#8B5E3C"),2); }

        DrawLabel(c, px+14, py+14, "Daily Quests", H("#F5C542"), 2);

        // Divider
        var div = LoadTex("Assets/KenneyFantasyUI/Divider/divider-000.png");
        if (div != null) BlitScaled(c, div, px+10, py+36, pw-20, 8);
        else FillRect(c, px+10, py+36, pw-20, 2, H("#8B5E3C"));

        string[] quests = { "Water 5 crops","Harvest tomatoes","Talk to the Elder","Sell 10 items","Find lost sheep" };
        bool[]   done   = { true, true, false, false, false };
        var chk  = LoadTex("Assets/KenneyIcons/checkmark.png");
        var crs  = LoadTex("Assets/KenneyIcons/cross.png");
        var slot = LoadTex("Assets/KenneyUI/panelInset_beige.png");

        for (int i = 0; i < quests.Length; i++)
        {
            int qy = py + 50 + i*42;
            if (slot != null) Blit9Slice(c, slot, px+10, qy, pw-20, 32, 8);
            else FillRect(c, px+10, qy, pw-20, 32, new Color(0.15f,0.08f,0.02f,0.7f));

            if (done[i]) { if (chk!=null) BlitTinted(c,chk,px+16,qy+7,18,18,H("#44cc88")); else FillRect(c,px+16,qy+7,18,18,H("#44cc88")); }
            else         { if (crs!=null) BlitTinted(c,crs,px+16,qy+7,18,18,H("#888888")); else FillRect(c,px+16,qy+7,18,18,H("#888888")); }

            DrawLabel(c, px+42, qy+10, quests[i], done[i]?H("#88cc88"):H("#F5EDDC"), 1);
        }

        DrawLabel(c, px+14, py+ph-22, "Progress: 2/5 complete", H("#A89070"), 1);
    }

    static void DrawHotbar(Texture2D c)
    {
        int slots=8, ss=56, gap=4;
        int tw = slots*ss + (slots-1)*gap;
        int hx = (W-tw)/2, hy = HH-ss-20; // bottom of screen

        var panel = LoadTex("Assets/KenneyUI/panel_brown.png");
        if (panel!=null) Blit9Slice(c, panel, hx-12, hy-8, tw+24, ss+20, 10);

        Color[] itemCols = { H("#e84040"),H("#e89020"),H("#20a040"),H("#4080e8"),H("#e040e0"),H("#e8c020"),H("#60c0e8"),H("#c0c0c0") };
        bool[] has = { true,true,true,true,false,true,false,false };

        for (int i=0; i<slots; i++)
        {
            int sx = hx + i*(ss+gap);
            var slotTex = i==2
                ? LoadTex("Assets/KenneyUI/buttonSquare_beige.png")
                : LoadTex("Assets/KenneyUI/buttonSquare_brown.png");
            if (slotTex!=null) Blit9Slice(c, slotTex, sx, hy, ss, ss, 8);
            else FillRect(c, sx, hy, ss, ss, H(i==2?"#d4b88a":"#6e3e1a"));
            if (i==2) DrawBorder(c, sx, hy, ss, ss, H("#F5C542"), 2);
            if (has[i]) ItemIcon(c, sx+ss/2, hy+ss/2, itemCols[i]);
            DrawLabel(c, sx+3, hy+3, (i+1).ToString(), H("#A89070"), 1);
        }
    }

    static void DrawInventory(Texture2D c)
    {
        int cols=4, rows=4, ss=44, gap=4;
        int iw=cols*ss+(cols-1)*gap, ih=rows*ss+(rows-1)*gap;
        int ix=12, iy=384; // left side, below quest panel

        var panel = LoadTex("Assets/KenneyUI/panel_brown.png");
        if (panel!=null) Blit9Slice(c, panel, ix-16, iy-8, iw+32, ih+44, 12);
        else { FillRect(c,ix-16,iy-8,iw+32,ih+44,new Color(0.22f,0.12f,0.04f,0.95f)); DrawBorder(c,ix-16,iy-8,iw+32,ih+44,H("#8B5E3C"),2); }

        DrawLabel(c, ix+4, iy-4, "Inventory", H("#F5C542"), 2);
        int labelH = 18;

        Color[] itemCols = {
            H("#e84040"),H("#44aa22"),H("#e09020"),H("#4488dd"),
            H("#ee5580"),H("#22aacc"),Color.clear,H("#cccc22"),
            Color.clear,H("#ff8830"),H("#8855ee"),Color.clear,
            H("#55dd88"),Color.clear,Color.clear,H("#ee3333"),
        };

        var slotTex = LoadTex("Assets/KenneyUI/panelInset_brown.png");
        for (int row=0; row<rows; row++)
        for (int col=0; col<cols; col++)
        {
            int idx = row*cols+col;
            int sx=ix+col*(ss+gap), sy=iy+labelH+row*(ss+gap);
            if (slotTex!=null) Blit9Slice(c, slotTex, sx, sy, ss, ss, 8);
            else FillRect(c, sx, sy, ss, ss, new Color(0.12f,0.06f,0.01f,0.9f));
            if (itemCols[idx].a > 0f)
            {
                ItemIcon(c, sx+ss/2, sy+ss/2, itemCols[idx]);
                if (idx%3==0) DrawLabel(c, sx+ss-14, sy+ss-12, "x3", H("#F5EDDC"), 1);
            }
        }
    }

    static void DrawDialogue(Texture2D c)
    {
        int dw=560, dh=120, dx=(W-dw)/2-60, dy=HH-dh-100;

        var dlg = LoadTex("Assets/Sprout Lands - UI Pack - Basic pack/Sprite sheets/Dialouge UI/Premade dialog box medium.png");
        if (dlg!=null) BlitScaled(c, dlg, dx, dy, dw, dh);
        else
        {
            var panel = LoadTex("Assets/KenneyUI/panel_beige.png");
            if (panel!=null) Blit9Slice(c, panel, dx, dy, dw, dh, 12);
            else FillRect(c, dx, dy, dw, dh, new Color(0.9f,0.85f,0.72f,0.97f));
            DrawBorder(c, dx, dy, dw, dh, H("#8B5E3C"), 2);
        }

        // Portrait
        FillRect(c, dx+12, dy+12, 88, 96, H("#2C1810"));
        DrawBorder(c, dx+12, dy+12, 88, 96, H("#8B5E3C"), 2);
        DrawFarmerFace(c, dx+56, dy+48);
        DrawLabel(c, dx+20, dy+14, "Elder", H("#F5C542"), 1);

        // Text
        DrawLabel(c, dx+112, dy+30, "Welcome back, farmer! The fields", H("#2C1810"), 1);
        DrawLabel(c, dx+112, dy+44, "need your attention today.", H("#2C1810"), 1);
        DrawLabel(c, dx+112, dy+62, "Spring is short!", H("#2C1810"), 1);
        DrawLabel(c, dx+112, dy+82, "\"Elder Morrow\"", H("#8B5E3C"), 1);
        DrawLabel(c, dx+dw-90, dy+dh-18, "v Continue", H("#8B5E3C"), 1);
    }

    static void DrawFarmerFace(Texture2D c, int cx, int cy)
    {
        FillCircle(c, cx, cy, 22, H("#f0c898"));
        FillRect(c, cx-20, cy-22, 40, 10, H("#6a3010")); // hair top
        FillRect(c, cx-22, cy-18, 6, 20, H("#6a3010")); // hair left
        FillRect(c, cx+16, cy-18, 6, 20, H("#6a3010")); // hair right
        FillRect(c, cx-8, cy-6, 5, 4, H("#332211")); // eye L
        FillRect(c, cx+3,  cy-6, 5, 4, H("#332211")); // eye R
        FillRect(c, cx-1, cy+2, 3, 4, H("#d8a070")); // nose
        FillRect(c, cx-6, cy+10, 12, 2, H("#c07058")); // mouth
        FillRect(c, cx-10, cy+14, 20, 6, H("#c0c0b0")); // beard
    }

    static void DrawResourceBadges(Texture2D c)
    {
        // Top-right area under HUD: y=72, x=1040
        int bx=1038, by=72;
        string[] labels = { "Coins","Wood","Stone","Seeds" };
        string[] values = { "1,248","342","89","56" };
        string[] icons  = {
            "Assets/KenneyIcons/star.png",
            "Assets/KenneyIcons/minus.png",
            "Assets/KenneyIcons/information.png",
            "Assets/KenneyIcons/plus.png"
        };
        Color[] cols = { H("#F5C542"),H("#c89040"),H("#a0a0a0"),H("#88dd44") };
        var badge = LoadTex("Assets/KenneyUI/buttonLong_brown.png");

        for (int i=0; i<4; i++)
        {
            int bbx=bx+(i%2)*118, bby=by+(i/2)*36;
            if (badge!=null) Blit9Slice(c, badge, bbx, bby, 110, 28, 8);
            else { FillRect(c,bbx,bby,110,28,new Color(0.22f,0.12f,0.04f,0.92f)); DrawBorder(c,bbx,bby,110,28,H("#8B5E3C"),1); }
            var ico = LoadTex(icons[i]);
            if (ico!=null) BlitTinted(c, ico, bbx+4, bby+4, 20, 20, cols[i]);
            else FillRect(c, bbx+4, bby+6, 16, 16, cols[i]);
            DrawLabel(c, bbx+28, bby+9, values[i], H("#F5EDDC"), 1);
        }
        DrawLabel(c, bx+2, by-14, "Resources", H("#F5C542"), 1);
    }

    static void DrawMinimap(Texture2D c)
    {
        // Below resource badges: y=148, x=1038
        int mx=1038, my=148, mw=190, mh=130;

        var panel = LoadTex("Assets/KenneyFantasyUI/Panel/panel-000.png");
        if (panel!=null) Blit9Slice(c, panel, mx-6, my-6, mw+12, mh+12, 12);
        else { FillRect(c,mx-6,my-6,mw+12,mh+12,new Color(0.18f,0.10f,0.03f,0.95f)); DrawBorder(c,mx-6,my-6,mw+12,mh+12,H("#8B5E3C"),2); }

        FillRect(c, mx, my, mw, mh, H("#4a7c2f"));
        // River line
        FillRect(c, mx, my+mh/2-4, mw, 10, H("#3a90cc"));
        // Pond
        FillRect(c, mx+18, my, 14, mh/3, H("#3a90cc"));
        // Structures
        FillRect(c, mx+10, my+10, 14, 12, H("#cc6622"));
        FillRect(c, mx+38, my+12, 10, 10, H("#cc8822"));
        FillRect(c, mx+98, my+10, 12, 10, H("#cc8822"));
        // Player dot
        FillCircle(c, mx+mw/2, my+mh/2, 4, H("#F5C542"));
        DrawLabel(c, mx+6, my+mh-16, "Map", H("#F5C542"), 1);
    }

    static void DrawLegend(Texture2D c)
    {
        // Very top strip (above HUD) — spec label
        for (int y=0; y<14; y++)
        for (int x=0; x<W; x++)
            AlphaBlit(c, x, y, new Color(0f,0f,0f,0.6f));
        DrawLabel(c, 8, 3, "FocusFarm UI Design  |  Kenney UI + Fantasy Panels + Sprout Lands  |  Font: m5x7  |  Scale: 2x (32px grid)  |  URP 2D", H("#A89070"), 1);
        DrawLabel(c, W-110, 3, "v4.0  |  1280x720", H("#6a5040"), 1);
    }

    // ── Item icon helper ─────────────────────────────────────────────────────
    static void ItemIcon(Texture2D c, int cx, int cy, Color col)
    {
        int r=14;
        for (int dx=-r; dx<=r; dx++)
        for (int dy=-r; dy<=r; dy++)
        {
            float d=Mathf.Sqrt(dx*dx+dy*dy);
            if (d<=r)
            {
                float b=1f-d/r*0.45f;
                AlphaBlit(c, cx+dx, cy+dy, new Color(col.r*b, col.g*b, col.b*b, 1f));
            }
        }
        // Highlight
        for (int dx=-4; dx<=-2; dx++)
        for (int dy=-5; dy<=-2; dy++)
            AlphaBlit(c, cx+dx, cy+dy, new Color(1f,1f,1f,0.45f));
    }

    // ── Compositing ──────────────────────────────────────────────────────────

    static Texture2D LoadTex(string assetPath)
    {
        try
        {
            string full = Path.Combine(ProjectRoot, assetPath);
            if (!File.Exists(full)) return null;
            var t = new Texture2D(2,2,TextureFormat.RGBA32,false);
            t.LoadImage(File.ReadAllBytes(full));
            return t;
        }
        catch { return null; }
    }

    // Sprites use bottom-left origin; we need to flip them when reading pixels
    static Color SampleTex(Texture2D src, int sx, int sy)
    {
        // src uses bottom-left origin → flip y when sampling
        int fy = src.height - 1 - sy;
        sx = Mathf.Clamp(sx, 0, src.width-1);
        fy = Mathf.Clamp(fy, 0, src.height-1);
        return src.GetPixel(sx, fy);
    }

    static void Blit9Slice(Texture2D dst, Texture2D src, int dx, int dy, int dw, int dh, int border)
    {
        int sw=src.width, sh=src.height;
        int b = Mathf.Min(border, Mathf.Min(sw/3, sh/3));
        int db = Mathf.Min(b, Mathf.Min(dw/3, dh/3));
        // 9 regions (dst coords in top-left space, src coords in bottom-left space)
        BlitReg(dst, src, dx,         dy,          db,       db,       0,    sh-b, b,    b);    // TL
        BlitReg(dst, src, dx+db,      dy,          dw-2*db,  db,       b,    sh-b, sw-2*b, b);  // TM
        BlitReg(dst, src, dx+dw-db,   dy,          db,       db,       sw-b, sh-b, b,    b);    // TR
        BlitReg(dst, src, dx,         dy+db,       db,       dh-2*db,  0,    b,    b,    sh-2*b); // ML
        BlitReg(dst, src, dx+db,      dy+db,       dw-2*db,  dh-2*db,  b,    b,    sw-2*b, sh-2*b); // MM
        BlitReg(dst, src, dx+dw-db,   dy+db,       db,       dh-2*db,  sw-b, b,    b,    sh-2*b); // MR
        BlitReg(dst, src, dx,         dy+dh-db,    db,       db,       0,    0,    b,    b);    // BL
        BlitReg(dst, src, dx+db,      dy+dh-db,    dw-2*db,  db,       b,    0,    sw-2*b, b); // BM
        BlitReg(dst, src, dx+dw-db,   dy+dh-db,    db,       db,       sw-b, 0,    b,    b);   // BR
    }

    // dst coords: top-left space. src coords: bottom-left space (raw sprite coords).
    static void BlitReg(Texture2D dst, Texture2D src,
                        int dx, int dy, int dw, int dh,
                        int sx, int sy, int sw2, int sh2)
    {
        if (dw<=0||dh<=0||sw2<=0||sh2<=0) return;
        for (int iy=0; iy<dh; iy++)
        for (int ix=0; ix<dw; ix++)
        {
            int srcX = sx + (int)(ix/(float)dw * sw2);
            int srcY = sy + (int)(iy/(float)dh * sh2);
            Color col = SampleTex(src, srcX, srcY);
            AlphaBlit(dst, dx+ix, dy+iy, col);
        }
    }

    static void BlitScaled(Texture2D dst, Texture2D src, int dx, int dy, int dw, int dh)
    {
        for (int iy=0; iy<dh; iy++)
        for (int ix=0; ix<dw; ix++)
        {
            int srcX = (int)(ix/(float)dw * src.width);
            int srcY = (int)(iy/(float)dh * src.height);
            AlphaBlit(dst, dx+ix, dy+iy, SampleTex(src, srcX, srcY));
        }
    }

    static void BlitTinted(Texture2D dst, Texture2D src, int dx, int dy, int dw, int dh, Color tint)
    {
        for (int iy=0; iy<dh; iy++)
        for (int ix=0; ix<dw; ix++)
        {
            int srcX = (int)(ix/(float)dw * src.width);
            int srcY = (int)(iy/(float)dh * src.height);
            Color sc = SampleTex(src, srcX, srcY);
            AlphaBlit(dst, dx+ix, dy+iy, new Color(sc.r*tint.r, sc.g*tint.g, sc.b*tint.b, sc.a*tint.a));
        }
    }

    static void PaintHBar(Texture2D dst,
                          Texture2D bL, Texture2D bM, Texture2D bR,
                          Texture2D fL, Texture2D fM, Texture2D fR,
                          int dx, int dy, int dw, int dh, float fill)
    {
        int cap = dh, mid = dw-2*cap;
        if (mid<0){cap=dw/2;mid=0;}
        if (bL!=null) BlitScaled(dst,bL,dx,dy,cap,dh);
        if (bM!=null&&mid>0) BlitScaled(dst,bM,dx+cap,dy,mid,dh);
        if (bR!=null) BlitScaled(dst,bR,dx+cap+mid,dy,cap,dh);
        if (fill>0f&&fL!=null)
        {
            int fp=(int)(dw*fill), fm=fp-2*cap;
            if (fm<0){fm=0;fp=Mathf.Min(fp,2*cap);}
            if (fp>=cap)
            {
                BlitScaled(dst,fL,dx,dy,cap,dh);
                if (fM!=null&&fm>0) BlitScaled(dst,fM,dx+cap,dy,fm,dh);
            }
        }
    }

    // AlphaBlit: callers use top-left coords; store in texture bottom-left coords
    static void AlphaBlit(Texture2D c, int x, int y, Color src)
    {
        if (src.a <= 0f) return;
        int ty = c.height - 1 - y;
        if (x<0||x>=c.width||ty<0||ty>=c.height) return;
        Color dst = c.GetPixel(x, ty);
        float a = src.a;
        c.SetPixel(x, ty, new Color(
            dst.r*(1-a)+src.r*a,
            dst.g*(1-a)+src.g*a,
            dst.b*(1-a)+src.b*a,
            1f));
    }

    // Direct pixel write (bottom-left coords) — used only by NewTex init
    static void P(Texture2D c, int x, int y, Color col)
    {
        // top-left y → store via AlphaBlit
        AlphaBlit(c, x, y, col);
    }

    static void FillRect(Texture2D c, int x, int y, int w, int h, Color col)
    {
        for (int iy=y; iy<y+h; iy++)
        for (int ix=x; ix<x+w; ix++)
            AlphaBlit(c, ix, iy, col);
    }

    static void DrawBorder(Texture2D c, int x, int y, int w, int h, Color col, int t)
    {
        FillRect(c, x,     y,     w, t, col);
        FillRect(c, x,     y+h-t, w, t, col);
        FillRect(c, x,     y,     t, h, col);
        FillRect(c, x+w-t, y,     t, h, col);
    }

    static void FillCircle(Texture2D c, int cx, int cy, int r, Color col)
    {
        for (int dx=-r; dx<=r; dx++)
        for (int dy=-r; dy<=r; dy++)
            if (dx*dx+dy*dy<=r*r) AlphaBlit(c, cx+dx, cy+dy, col);
    }

    static void Ellipse(Texture2D c, int cx, int cy, int rw, int rh, Color col)
    {
        for (int dx=-rw; dx<=rw; dx++)
        for (int dy=-rh; dy<=rh; dy++)
        {
            float ex=dx/(float)rw, ey=dy/(float)rh;
            if (ex*ex+ey*ey<=1f) c.SetPixel(cx+dx, c.height-1-(cy+dy), col); // direct, fully opaque
        }
    }

    // ── Pixel font (5×7, top-left origin for glyphs) ────────────────────────
    static readonly Dictionary<char,byte[]> FONT = BuildFont();

    static Dictionary<char,byte[]> BuildFont()
    {
        var f = new Dictionary<char,byte[]>();
        f[' ']=new byte[]{0,0,0,0,0};
        f['A']=new byte[]{0x7C,0x12,0x11,0x12,0x7C};
        f['B']=new byte[]{0x7F,0x49,0x49,0x49,0x36};
        f['C']=new byte[]{0x3E,0x41,0x41,0x41,0x22};
        f['D']=new byte[]{0x7F,0x41,0x41,0x22,0x1C};
        f['E']=new byte[]{0x7F,0x49,0x49,0x49,0x41};
        f['F']=new byte[]{0x7F,0x09,0x09,0x09,0x01};
        f['G']=new byte[]{0x3E,0x41,0x49,0x49,0x7A};
        f['H']=new byte[]{0x7F,0x08,0x08,0x08,0x7F};
        f['I']=new byte[]{0x00,0x41,0x7F,0x41,0x00};
        f['J']=new byte[]{0x20,0x40,0x41,0x3F,0x01};
        f['K']=new byte[]{0x7F,0x08,0x14,0x22,0x41};
        f['L']=new byte[]{0x7F,0x40,0x40,0x40,0x40};
        f['M']=new byte[]{0x7F,0x02,0x0C,0x02,0x7F};
        f['N']=new byte[]{0x7F,0x04,0x08,0x10,0x7F};
        f['O']=new byte[]{0x3E,0x41,0x41,0x41,0x3E};
        f['P']=new byte[]{0x7F,0x09,0x09,0x09,0x06};
        f['Q']=new byte[]{0x3E,0x41,0x51,0x21,0x5E};
        f['R']=new byte[]{0x7F,0x09,0x19,0x29,0x46};
        f['S']=new byte[]{0x46,0x49,0x49,0x49,0x31};
        f['T']=new byte[]{0x01,0x01,0x7F,0x01,0x01};
        f['U']=new byte[]{0x3F,0x40,0x40,0x40,0x3F};
        f['V']=new byte[]{0x1F,0x20,0x40,0x20,0x1F};
        f['W']=new byte[]{0x3F,0x40,0x38,0x40,0x3F};
        f['X']=new byte[]{0x63,0x14,0x08,0x14,0x63};
        f['Y']=new byte[]{0x07,0x08,0x70,0x08,0x07};
        f['Z']=new byte[]{0x61,0x51,0x49,0x45,0x43};
        f['a']=new byte[]{0x20,0x54,0x54,0x54,0x78};
        f['b']=new byte[]{0x7F,0x48,0x44,0x44,0x38};
        f['c']=new byte[]{0x38,0x44,0x44,0x44,0x20};
        f['d']=new byte[]{0x38,0x44,0x44,0x48,0x7F};
        f['e']=new byte[]{0x38,0x54,0x54,0x54,0x18};
        f['f']=new byte[]{0x08,0x7E,0x09,0x01,0x02};
        f['g']=new byte[]{0x0C,0x52,0x52,0x52,0x3E};
        f['h']=new byte[]{0x7F,0x08,0x04,0x04,0x78};
        f['i']=new byte[]{0x00,0x44,0x7D,0x40,0x00};
        f['j']=new byte[]{0x20,0x40,0x44,0x3D,0x00};
        f['k']=new byte[]{0x7F,0x10,0x28,0x44,0x00};
        f['l']=new byte[]{0x00,0x41,0x7F,0x40,0x00};
        f['m']=new byte[]{0x7C,0x04,0x18,0x04,0x78};
        f['n']=new byte[]{0x7C,0x08,0x04,0x04,0x78};
        f['o']=new byte[]{0x38,0x44,0x44,0x44,0x38};
        f['p']=new byte[]{0x7C,0x14,0x14,0x14,0x08};
        f['q']=new byte[]{0x08,0x14,0x14,0x18,0x7C};
        f['r']=new byte[]{0x7C,0x08,0x04,0x04,0x08};
        f['s']=new byte[]{0x48,0x54,0x54,0x54,0x20};
        f['t']=new byte[]{0x04,0x3F,0x44,0x40,0x20};
        f['u']=new byte[]{0x3C,0x40,0x40,0x20,0x7C};
        f['v']=new byte[]{0x1C,0x20,0x40,0x20,0x1C};
        f['w']=new byte[]{0x3C,0x40,0x30,0x40,0x3C};
        f['x']=new byte[]{0x44,0x28,0x10,0x28,0x44};
        f['y']=new byte[]{0x0C,0x50,0x50,0x50,0x3C};
        f['z']=new byte[]{0x44,0x64,0x54,0x4C,0x44};
        f['0']=new byte[]{0x3E,0x51,0x49,0x45,0x3E};
        f['1']=new byte[]{0x00,0x42,0x7F,0x40,0x00};
        f['2']=new byte[]{0x42,0x61,0x51,0x49,0x46};
        f['3']=new byte[]{0x21,0x41,0x45,0x4B,0x31};
        f['4']=new byte[]{0x18,0x14,0x12,0x7F,0x10};
        f['5']=new byte[]{0x27,0x45,0x45,0x45,0x39};
        f['6']=new byte[]{0x3C,0x4A,0x49,0x49,0x30};
        f['7']=new byte[]{0x01,0x71,0x09,0x05,0x03};
        f['8']=new byte[]{0x36,0x49,0x49,0x49,0x36};
        f['9']=new byte[]{0x06,0x49,0x49,0x29,0x1E};
        f[':']=new byte[]{0x00,0x36,0x36,0x00,0x00};
        f['/']=new byte[]{0x20,0x10,0x08,0x04,0x02};
        f['.']=new byte[]{0x00,0x60,0x60,0x00,0x00};
        f[',']=new byte[]{0x00,0x50,0x30,0x00,0x00};
        f['!']=new byte[]{0x00,0x00,0x5F,0x00,0x00};
        f['|']=new byte[]{0x00,0x00,0x7F,0x00,0x00};
        f['-']=new byte[]{0x08,0x08,0x08,0x08,0x08};
        f['_']=new byte[]{0x40,0x40,0x40,0x40,0x40};
        f['(']=new byte[]{0x00,0x1C,0x22,0x41,0x00};
        f[')']=new byte[]{0x00,0x41,0x22,0x1C,0x00};
        f['"']=new byte[]{0x03,0x00,0x03,0x00,0x00};
        f['\'']=new byte[]{0x00,0x03,0x00,0x00,0x00};
        f['?']=new byte[]{0x02,0x01,0x51,0x09,0x06};
        f['%']=new byte[]{0x23,0x13,0x08,0x64,0x62};
        f['<']=new byte[]{0x08,0x14,0x22,0x41,0x00};
        f['>']=new byte[]{0x00,0x41,0x22,0x14,0x08};
        f['*']=new byte[]{0x14,0x08,0x3E,0x08,0x14};
        f['v']=new byte[]{0x1C,0x20,0x40,0x20,0x1C};
        f['+']=new byte[]{0x08,0x08,0x3E,0x08,0x08};
        f['#']=new byte[]{0x14,0x7F,0x14,0x7F,0x14};
        f['@']=new byte[]{0x3E,0x41,0x5D,0x55,0x1E};
        f['[']=new byte[]{0x00,0x7F,0x41,0x41,0x00};
        f[']']=new byte[]{0x00,0x41,0x41,0x7F,0x00};
        f['x']=new byte[]{0x44,0x28,0x10,0x28,0x44};
        return f;
    }

    // DrawLabel: x,y in top-left coords. Glyph bit0=bottom row of glyph (displayed at y+6).
    static void DrawLabel(Texture2D c, int x, int y, string text, Color col, int scale)
    {
        int cx = x;
        foreach (char ch in text)
        {
            byte[] glyph;
            if (!FONT.TryGetValue(ch, out glyph) && !FONT.TryGetValue(char.ToUpper(ch), out glyph))
            { cx += (5+1)*scale; continue; }
            for (int g=0; g<5; g++)
            {
                byte colData = glyph[g];
                for (int row=0; row<7; row++)
                {
                    if ((colData & (1<<row)) != 0)
                    {
                        // bit0 = bottom row; col0 = rightmost column in this font format
                        int px2 = cx + g*scale;
                        for (int sy=0; sy<scale; sy++)
                        for (int sx=0; sx<scale; sx++)
                            AlphaBlit(c, px2+sx, y+row*scale+sy, col);
                    }
                }
            }
            cx += (5+1)*scale;
        }
    }

    static Color H(string hex)
    {
        hex = hex.TrimStart('#');
        float r=System.Convert.ToInt32(hex.Substring(0,2),16)/255f;
        float g=System.Convert.ToInt32(hex.Substring(2,2),16)/255f;
        float b=System.Convert.ToInt32(hex.Substring(4,2),16)/255f;
        return new Color(r,g,b,1f);
    }

    static Texture2D NewTex(int w, int h)
    {
        var t = new Texture2D(w, h, TextureFormat.RGBA32, false);
        Color[] px = new Color[w*h];
        for (int i=0; i<px.Length; i++) px[i]=Color.black;
        t.SetPixels(px);
        return t;
    }
}
