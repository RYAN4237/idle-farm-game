using UnityEngine;
using UnityEditor;
using System.IO;

/// Generates a UI design mockup PNG saved to the Desktop.
/// Shows the complete HUD layout for the farm idle game.
public class GenerateUIDesignMockup
{
    [MenuItem("Tools/Generate UI Design Mockup")]
    public static void Execute()
    {
        // Canvas: 1920x1080 mockup
        int W = 1920, H = 1080;
        var canvas = new Texture2D(W, H, TextureFormat.RGBA32, false);

        // ── Color palette (from Sprout Lands UI Pack) ──────────────────────────
        var COL_SKY        = Hex("#87CEEB"); // sky blue bg
        var COL_GRASS      = Hex("#C0CF6E"); // grass green
        var COL_WATER      = Hex("#59AEDF"); // water blue

        var COL_PANEL_BG   = Hex("#E8D5A3"); // warm parchment (main panel bg)
        var COL_PANEL_DARK = Hex("#C4A06A"); // darker wood border
        var COL_PANEL_DEEP = Hex("#8B6340"); // deep wood (frame outer)
        var COL_SLOT_BG    = Hex("#D4B483"); // slot background
        var COL_SLOT_DARK  = Hex("#A07840"); // slot border
        var COL_SLOT_SEL   = Hex("#F0C060"); // selected slot highlight

        var COL_TEXT_PRI   = Hex("#4A3020"); // dark brown text
        var COL_TEXT_SEC   = Hex("#8B6340"); // medium brown
        var COL_WHITE      = Hex("#FEFEFE");
        var COL_BLACK      = new Color(0,0,0,0.6f);

        var COL_HP_RED     = Hex("#E05050"); // health red
        var COL_ENERGY_YEL = Hex("#F0C030"); // energy yellow
        var COL_EXP_GRN    = Hex("#70C040"); // exp green
        var COL_SEASON_BLU = Hex("#6090E0"); // season blue

        var COL_COIN       = Hex("#F0C030"); // gold coin
        var COL_SEED       = Hex("#80C050"); // seed green
        var COL_WOOD       = Hex("#C07840"); // wood brown
        var COL_STONE      = Hex("#A0A0A0"); // stone grey

        // ── Fill background (game world mockup) ────────────────────────────────
        FillRect(canvas, 0, 0, W, H, COL_SKY);

        // Sky gradient (top 320px)
        for (int y = H - 320; y < H; y++)
        {
            float t = (float)(y - (H - 320)) / 320f;
            var skyCol = Color.Lerp(Hex("#60B8E8"), Hex("#A8E0F8"), t);
            for (int x = 0; x < W; x++) canvas.SetPixel(x, y, skyCol);
        }

        // Ground (bottom 760px)
        FillRect(canvas, 0, 0, W, 760, COL_GRASS);

        // River stripe across middle
        FillRect(canvas, 0, 380, W, 80, COL_WATER);
        // River wavy edges (simple scallop)
        for (int x = 0; x < W; x++)
        {
            int wave = (int)(Mathf.Sin(x * 0.08f) * 6);
            FillRect(canvas, x, 450 + wave, 1, 12, COL_GRASS);
            FillRect(canvas, x, 380 + wave, 1, 10, COL_GRASS);
        }

        // ── SECTION LABELS ──────────────────────────────────────────────────────
        // We draw everything as colored rectangles with labels in a design-doc style

        // ── 1. TOP BAR (HUD) — 48px tall ──────────────────────────────────────
        int topBarH = 56;
        int topBarY = H - topBarH;

        // Panel background
        DrawPanel(canvas, 0, topBarY, W, topBarH, COL_PANEL_BG, COL_PANEL_DEEP, 3);

        // LEFT: Season + Day + Time
        // Season badge (60x40, rounded)
        DrawPanel(canvas, 12, topBarY + 8, 120, 40, COL_SEASON_BLU, COL_PANEL_DEEP, 2);
        DrawLabel(canvas, 18, topBarY + 16, "SPRING", COL_WHITE, 2);

        // Day counter
        DrawPanel(canvas, 140, topBarY + 8, 80, 40, COL_PANEL_DARK, COL_PANEL_DEEP, 2);
        DrawLabel(canvas, 148, topBarY + 16, "Day 7", COL_TEXT_PRI, 2);

        // Clock
        DrawPanel(canvas, 228, topBarY + 8, 80, 40, COL_PANEL_DARK, COL_PANEL_DEEP, 2);
        DrawLabel(canvas, 235, topBarY + 16, "06:30", COL_TEXT_PRI, 2);

        // CENTER: Resources row — Coin | Wood | Stone | Seeds
        int resX = W/2 - 280;
        DrawResourceBadge(canvas, resX,       topBarY + 8, COL_COIN,   "G  1,240", COL_TEXT_PRI);
        DrawResourceBadge(canvas, resX + 160, topBarY + 8, COL_WOOD,   "W    480", COL_TEXT_PRI);
        DrawResourceBadge(canvas, resX + 320, topBarY + 8, COL_STONE,  "S    320", COL_TEXT_PRI);
        DrawResourceBadge(canvas, resX + 480, topBarY + 8, COL_SEED,   "✿    16", COL_TEXT_PRI);

        // RIGHT: HP + Energy bars
        int barX = W - 420;
        DrawLabel(canvas, barX, topBarY + 10, "HP", COL_TEXT_PRI, 2);
        DrawBar(canvas, barX + 30, topBarY + 12, 160, 14, 0.75f, COL_HP_RED, COL_SLOT_BG, COL_PANEL_DEEP);
        DrawLabel(canvas, barX, topBarY + 32, "EP", COL_ENERGY_YEL, 2);
        DrawBar(canvas, barX + 30, topBarY + 34, 160, 14, 0.55f, COL_ENERGY_YEL, COL_SLOT_BG, COL_PANEL_DEEP);

        // Settings button (top right)
        DrawPanel(canvas, W - 56, topBarY + 8, 40, 40, COL_PANEL_DARK, COL_PANEL_DEEP, 2);
        DrawLabel(canvas, W - 48, topBarY + 16, "⚙", COL_TEXT_PRI, 2);

        // ── 2. BOTTOM TOOLBAR — Hotbar 10 slots ───────────────────────────────
        int hotbarY = 8;
        int slotSize = 48;
        int hotbarW = slotSize * 10 + 24;
        int hotbarX = W/2 - hotbarW/2;

        DrawPanel(canvas, hotbarX - 8, hotbarY, hotbarW + 16, slotSize + 20, COL_PANEL_BG, COL_PANEL_DEEP, 3);

        for (int i = 0; i < 10; i++)
        {
            int sx = hotbarX + i * slotSize + i * 2;
            bool selected = (i == 2);
            var slotBg = selected ? COL_SLOT_SEL : COL_SLOT_BG;
            var slotBorder = selected ? Hex("#D09020") : COL_SLOT_DARK;
            DrawPanel(canvas, sx, hotbarY + 8, slotSize, slotSize, slotBg, slotBorder, 2);
            // Slot number
            DrawLabel(canvas, sx + 2, hotbarY + 9, (i+1).ToString(), COL_TEXT_SEC, 1);
        }
        // Label
        DrawLabel(canvas, hotbarX + hotbarW/2 - 28, hotbarY - 2, "HOTBAR", COL_TEXT_SEC, 1);

        // ── 3. LEFT PANEL — Tasks / Quests (collapsed tab) ────────────────────
        int lpW = 200, lpH = 320;
        int lpX = 12, lpY = 120;
        DrawPanel(canvas, lpX, lpY, lpW, lpH, COL_PANEL_BG, COL_PANEL_DEEP, 3);
        DrawLabel(canvas, lpX + 8, lpY + lpH - 24, "TASKS", COL_TEXT_PRI, 2);
        // Divider
        FillRect(canvas, lpX + 4, lpY + lpH - 32, lpW - 8, 2, COL_PANEL_DARK);
        // Task items
        string[] tasks = { "✓ Water crops", "✓ Harvest wheat", "→ Build fence", "→ Catch fish", "→ Sleep" };
        for (int i = 0; i < tasks.Length; i++)
        {
            bool done = tasks[i].StartsWith("✓");
            var col = done ? COL_TEXT_SEC : COL_TEXT_PRI;
            DrawLabel(canvas, lpX + 10, lpY + lpH - 56 - i * 22, tasks[i], col, 1);
        }

        // ── 4. RIGHT PANEL — Inventory (3x5 grid) ─────────────────────────────
        int rpSlot = 44;
        int rpCols = 5, rpRows = 4;
        int rpW = rpCols * rpSlot + (rpCols - 1) * 4 + 24;
        int rpH = rpRows * rpSlot + (rpRows - 1) * 4 + 60;
        int rpX = W - rpW - 12, rpY = 80;
        DrawPanel(canvas, rpX, rpY, rpW, rpH, COL_PANEL_BG, COL_PANEL_DEEP, 3);
        DrawLabel(canvas, rpX + 8, rpY + rpH - 24, "INVENTORY", COL_TEXT_PRI, 2);
        FillRect(canvas, rpX + 4, rpY + rpH - 32, rpW - 8, 2, COL_PANEL_DARK);

        // Draw inventory slots
        Color[] itemColors = { COL_SEED, COL_WOOD, COL_STONE, COL_COIN, COL_HP_RED,
                                COL_ENERGY_YEL, COL_WATER, COL_SEED, new Color(0,0,0,0), new Color(0,0,0,0),
                                new Color(0,0,0,0), new Color(0,0,0,0), new Color(0,0,0,0), new Color(0,0,0,0), new Color(0,0,0,0),
                                new Color(0,0,0,0), new Color(0,0,0,0), new Color(0,0,0,0), new Color(0,0,0,0), new Color(0,0,0,0) };
        for (int row = 0; row < rpRows; row++)
        for (int col = 0; col < rpCols; col++)
        {
            int idx = row * rpCols + col;
            int sx = rpX + 12 + col * (rpSlot + 4);
            int sy = rpY + 12 + row * (rpSlot + 4);
            DrawPanel(canvas, sx, sy, rpSlot, rpSlot, COL_SLOT_BG, COL_SLOT_DARK, 2);
            if (idx < itemColors.Length && itemColors[idx].a > 0)
                FillRect(canvas, sx + 8, sy + 8, rpSlot - 16, rpSlot - 16, itemColors[idx]);
        }

        // ── 5. CENTER: EXP bar above hotbar ───────────────────────────────────
        int expY = hotbarY + slotSize + 28;
        int expW = 300;
        int expX = W/2 - expW/2;
        DrawLabel(canvas, expX, expY + 2, "Lv.5", COL_TEXT_PRI, 1);
        DrawBar(canvas, expX + 30, expY, expW - 30, 12, 0.62f, COL_EXP_GRN, COL_SLOT_BG, COL_PANEL_DEEP);
        DrawLabel(canvas, expX + expW - 8, expY + 2, "6", COL_TEXT_SEC, 1);

        // ── 6. POPUP: Crop ready notification (top-center) ────────────────────
        int notifW = 280, notifH = 56;
        int notifX = W/2 - notifW/2, notifY = H - topBarH - notifH - 16;
        DrawPanel(canvas, notifX, notifY, notifW, notifH, COL_PANEL_BG, COL_PANEL_DEEP, 3);
        // Green dot indicator
        FillRect(canvas, notifX + 12, notifY + 20, 14, 14, COL_EXP_GRN);
        DrawLabel(canvas, notifX + 32, notifY + 16, "Wheat is ready!", COL_TEXT_PRI, 2);
        DrawLabel(canvas, notifX + 32, notifY + 34, "Tap to harvest", COL_TEXT_SEC, 1);

        // ── 7. DIALOGUE BOX (bottom, when NPC talks) ──────────────────────────
        int dlgW = 640, dlgH = 100;
        int dlgX = W/2 - dlgW/2, dlgY = hotbarY + slotSize + 50;
        DrawPanel(canvas, dlgX, dlgY, dlgW, dlgH, COL_PANEL_BG, COL_PANEL_DEEP, 4);
        // Portrait slot
        DrawPanel(canvas, dlgX + 12, dlgY + 12, 76, 76, COL_SLOT_BG, COL_SLOT_DARK, 3);
        DrawLabel(canvas, dlgX + 24, dlgY + 44, "NPC", COL_TEXT_SEC, 2);
        // Text area
        DrawLabel(canvas, dlgX + 100, dlgY + 20, "\"Good morning! The crops are", COL_TEXT_PRI, 2);
        DrawLabel(canvas, dlgX + 100, dlgY + 38, " looking great today...\"", COL_TEXT_PRI, 2);
        // Click indicator
        DrawLabel(canvas, dlgX + dlgW - 48, dlgY + dlgH - 20, "▼", COL_TEXT_SEC, 2);

        // ── 8. ANNOTATION OVERLAYS (design doc callouts) ──────────────────────
        // Draw red callout boxes around each UI zone with labels
        DrawCallout(canvas, 0,          topBarY,     W,    topBarH, Hex("#FF4444"), "① TOP BAR  —  Season / Day / Resources / Stats  (56px)");
        DrawCallout(canvas, hotbarX-8,  hotbarY,     hotbarW+16, slotSize+20, Hex("#FF8800"), "② HOTBAR  —  10 slots × 48px, 9-slice panel");
        DrawCallout(canvas, lpX,        lpY,         lpW,  lpH,     Hex("#44AA44"), "③ TASK LIST  —  200×320px, collapsible");
        DrawCallout(canvas, rpX,        rpY,         rpW,  rpH,     Hex("#4488FF"), "④ INVENTORY  —  5×4 grid, 44px slots");
        DrawCallout(canvas, notifX,     notifY,      notifW, notifH, Hex("#FF44FF"), "⑤ NOTIFICATION  —  slides in from top");
        DrawCallout(canvas, dlgX,       dlgY,        dlgW, dlgH,    Hex("#FFFF44"), "⑥ DIALOGUE BOX  —  640×100px, portrait + text");

        // ── 9. LEGEND (bottom-right) ───────────────────────────────────────────
        int legX = W - 380, legY = 460;
        DrawPanel(canvas, legX, legY, 360, 300, COL_PANEL_BG, COL_PANEL_DEEP, 3);
        DrawLabel(canvas, legX + 10, legY + 280, "COLOR PALETTE", COL_TEXT_PRI, 2);
        FillRect(canvas, legX + 4, legY + 272, 352, 2, COL_PANEL_DARK);

        var palette = new (Color c, string name)[] {
            (COL_PANEL_BG,   "#E8D5A3  Panel Parchment"),
            (COL_PANEL_DARK, "#C4A06A  Panel Border"),
            (COL_PANEL_DEEP, "#8B6340  Deep Wood"),
            (COL_SLOT_BG,    "#D4B483  Slot Background"),
            (COL_SLOT_SEL,   "#F0C060  Selected Slot"),
            (COL_TEXT_PRI,   "#4A3020  Text Primary"),
            (COL_HP_RED,     "#E05050  HP Bar"),
            (COL_ENERGY_YEL, "#F0C030  Energy Bar"),
            (COL_EXP_GRN,    "#70C040  EXP Bar"),
            (COL_SEASON_BLU, "#6090E0  Season Badge"),
            (COL_COIN,       "#F0C030  Gold / Coin"),
        };
        for (int i = 0; i < palette.Length; i++)
        {
            int py2 = legY + 248 - i * 22;
            FillRect(canvas, legX + 10, py2, 18, 14, palette[i].c);
            FillRect(canvas, legX + 10, py2, 18, 1, COL_PANEL_DEEP);
            FillRect(canvas, legX + 10, py2+13, 18, 1, COL_PANEL_DEEP);
            DrawLabel(canvas, legX + 34, py2 + 1, palette[i].name, COL_TEXT_PRI, 1);
        }

        // ── 10. TITLE ─────────────────────────────────────────────────────────
        DrawPanel(canvas, W/2 - 280, H/2 + 80, 560, 60, COL_PANEL_BG, COL_PANEL_DEEP, 4);
        DrawLabel(canvas, W/2 - 200, H/2 + 100, "FOCUS FARM  —  UI DESIGN SPEC  v1.0", COL_TEXT_PRI, 3);

        // Specs note
        DrawLabel(canvas, W/2 - 260, H/2 + 68, "Grid: 16px  |  Scale: ×2 (32px base)  |  Font: pixel-letters-7  |  Pipeline: URP 2D  |  Resolution: 1920×1080", COL_TEXT_SEC, 1);

        // ── Save ───────────────────────────────────────────────────────────────
        canvas.Apply();
        byte[] png = canvas.EncodeToPNG();
        Object.DestroyImmediate(canvas);

        string desktopPath = System.IO.Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop),
            "FocusFarm_UI_Design_Spec.png");
        File.WriteAllBytes(desktopPath, png);

        Debug.Log($"[UIDesignMockup] Saved to: {desktopPath}");
        EditorUtility.RevealInFinder(desktopPath);
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    static void FillRect(Texture2D tex, int x, int y, int w, int h, Color c)
    {
        x = Mathf.Clamp(x, 0, tex.width);
        y = Mathf.Clamp(y, 0, tex.height);
        w = Mathf.Clamp(w, 0, tex.width - x);
        h = Mathf.Clamp(h, 0, tex.height - y);
        for (int dy = 0; dy < h; dy++)
        for (int dx = 0; dx < w; dx++)
            tex.SetPixel(x + dx, y + dy, c);
    }

    static void DrawPanel(Texture2D tex, int x, int y, int w, int h, Color bg, Color border, int b)
    {
        FillRect(tex, x, y, w, h, border);
        FillRect(tex, x+b, y+b, w-b*2, h-b*2, bg);
    }

    static void DrawBar(Texture2D tex, int x, int y, int w, int h, float fill, Color filled, Color empty, Color border)
    {
        FillRect(tex, x, y, w, h, border);
        FillRect(tex, x+2, y+2, w-4, h-4, empty);
        int fw = (int)((w-4) * fill);
        if (fw > 0) FillRect(tex, x+2, y+2, fw, h-4, filled);
    }

    static void DrawResourceBadge(Texture2D tex, int x, int y, Color iconColor, string label, Color textColor)
    {
        var bg   = Hex("#D4B483");
        var brd  = Hex("#8B6340");
        DrawPanel(tex, x, y, 140, 40, bg, brd, 2);
        FillRect(tex, x+8, y+10, 18, 18, iconColor);
        DrawLabel(tex, x+32, y+12, label, textColor, 2);
    }

    static void DrawCallout(Texture2D tex, int x, int y, int w, int h, Color c, string label)
    {
        c.a = 0.85f;
        // Draw border only (4px)
        FillRect(tex, x,   y,   w, 3, c);
        FillRect(tex, x,   y+h-3, w, 3, c);
        FillRect(tex, x,   y,   3, h, c);
        FillRect(tex, x+w-3, y, 3, h, c);
        // Label background
        DrawLabel(tex, x+8, y+4, label, c, 1);
    }

    // Minimal bitmap-style label (3×5 font scaled by scale)
    static void DrawLabel(Texture2D tex, int x, int y, string text, Color c, int scale)
    {
        int cx = x;
        foreach (char ch in text)
        {
            DrawChar(tex, cx, y, ch, c, scale);
            cx += (5 + 1) * scale;
        }
    }

    // 5×7 bitmap font (printable ASCII subset)
    static readonly string[] FONT5x7 = {
        // space(32) ! " # $ % & ' ( ) * + , - . /
        "00000:00000:00000:00000:00000:00000:00000", // 32 space
        "00100:00100:00100:00100:00000:00100:00000", // 33 !
        "01010:01010:00000:00000:00000:00000:00000", // 34 "
        "01010:11111:01010:11111:01010:00000:00000", // 35 #
        "01110:10100:01110:00101:11110:00100:00000", // 36 $
        "11000:11001:00010:00100:01001:00011:00000", // 37 %
        "01100:10010:01100:10011:10010:01101:00000", // 38 &
        "00100:00100:00000:00000:00000:00000:00000", // 39 '
        "00110:01100:01000:01000:01000:01100:00110", // 40 (
        "11000:00110:00010:00010:00010:00110:11000", // 41 )
        "00000:00100:10101:01110:10101:00100:00000", // 42 *
        "00000:00100:00100:11111:00100:00100:00000", // 43 +
        "00000:00000:00000:00000:00110:00100:01000", // 44 ,
        "00000:00000:00000:11111:00000:00000:00000", // 45 -
        "00000:00000:00000:00000:00000:01100:01100", // 46 .
        "00001:00010:00010:00100:01000:01000:10000", // 47 /
        // 0-9
        "01110:10001:10011:10101:11001:10001:01110", // 48 0
        "00100:01100:00100:00100:00100:00100:01110", // 49 1
        "01110:10001:00001:00110:01000:10000:11111", // 50 2
        "11110:00001:00001:01110:00001:00001:11110", // 51 3
        "00010:00110:01010:10010:11111:00010:00010", // 52 4
        "11111:10000:11110:00001:00001:10001:01110", // 53 5
        "00110:01000:10000:11110:10001:10001:01110", // 54 6
        "11111:00001:00010:00100:01000:01000:01000", // 55 7
        "01110:10001:10001:01110:10001:10001:01110", // 56 8
        "01110:10001:10001:01111:00001:00010:01100", // 57 9
        // : ; < = > ? @
        "00000:01100:01100:00000:01100:01100:00000", // 58 :
        "00000:01100:01100:00000:01100:00100:01000", // 59 ;
        "00010:00100:01000:10000:01000:00100:00010", // 60 <
        "00000:00000:11111:00000:11111:00000:00000", // 61 =
        "10000:01000:00100:00010:00100:01000:10000", // 62 >
        "01110:10001:00001:00110:00100:00000:00100", // 63 ?
        "01110:10001:10111:10101:10110:10000:01111", // 64 @
        // A-Z
        "01110:10001:10001:11111:10001:10001:10001", // 65 A
        "11110:10001:10001:11110:10001:10001:11110", // 66 B
        "01110:10001:10000:10000:10000:10001:01110", // 67 C
        "11100:10010:10001:10001:10001:10010:11100", // 68 D
        "11111:10000:10000:11110:10000:10000:11111", // 69 E
        "11111:10000:10000:11110:10000:10000:10000", // 70 F
        "01110:10001:10000:10111:10001:10001:01111", // 71 G
        "10001:10001:10001:11111:10001:10001:10001", // 72 H
        "01110:00100:00100:00100:00100:00100:01110", // 73 I
        "00111:00010:00010:00010:00010:10010:01100", // 74 J
        "10001:10010:10100:11000:10100:10010:10001", // 75 K
        "10000:10000:10000:10000:10000:10000:11111", // 76 L
        "10001:11011:10101:10101:10001:10001:10001", // 77 M
        "10001:10001:11001:10101:10011:10001:10001", // 78 N
        "01110:10001:10001:10001:10001:10001:01110", // 79 O
        "11110:10001:10001:11110:10000:10000:10000", // 80 P
        "01110:10001:10001:10001:10101:10010:01101", // 81 Q
        "11110:10001:10001:11110:10100:10010:10001", // 82 R
        "01111:10000:10000:01110:00001:00001:11110", // 83 S
        "11111:00100:00100:00100:00100:00100:00100", // 84 T
        "10001:10001:10001:10001:10001:10001:01110", // 85 U
        "10001:10001:10001:10001:01010:01010:00100", // 86 V
        "10001:10001:10101:10101:10101:11011:10001", // 87 W
        "10001:10001:01010:00100:01010:10001:10001", // 88 X
        "10001:10001:01010:00100:00100:00100:00100", // 89 Y
        "11111:00001:00010:00100:01000:10000:11111", // 90 Z
        // [ \ ] ^ _ ` a-z
        "01110:01000:01000:01000:01000:01000:01110", // 91 [
        "10000:01000:01000:00100:00010:00010:00001", // 92 \
        "01110:00010:00010:00010:00010:00010:01110", // 93 ]
        "00100:01010:10001:00000:00000:00000:00000", // 94 ^
        "00000:00000:00000:00000:00000:00000:11111", // 95 _
        "01000:00100:00000:00000:00000:00000:00000", // 96 `
        "00000:00000:01110:00001:01111:10001:01111", // 97 a
        "10000:10000:11110:10001:10001:10001:11110", // 98 b
        "00000:00000:01110:10000:10000:10001:01110", // 99 c
        "00001:00001:01111:10001:10001:10001:01111", // 100 d
        "00000:00000:01110:10001:11111:10000:01110", // 101 e
        "00110:01001:01000:11110:01000:01000:01000", // 102 f
        "00000:00000:01111:10001:01111:00001:11110", // 103 g
        "10000:10000:11110:10001:10001:10001:10001", // 104 h
        "00100:00000:01100:00100:00100:00100:01110", // 105 i
        "00010:00000:00110:00010:00010:10010:01100", // 106 j
        "10000:10000:10010:10100:11000:10100:10010", // 107 k
        "01100:00100:00100:00100:00100:00100:01110", // 108 l
        "00000:00000:11010:10101:10101:10001:10001", // 109 m
        "00000:00000:11110:10001:10001:10001:10001", // 110 n
        "00000:00000:01110:10001:10001:10001:01110", // 111 o
        "00000:00000:11110:10001:11110:10000:10000", // 112 p
        "00000:00000:01111:10001:01111:00001:00001", // 113 q
        "00000:00000:10110:11001:10000:10000:10000", // 114 r
        "00000:00000:01110:10000:01110:00001:11110", // 115 s
        "01000:01000:11110:01000:01000:01001:00110", // 116 t
        "00000:00000:10001:10001:10001:10011:01101", // 117 u
        "00000:00000:10001:10001:10001:01010:00100", // 118 v
        "00000:00000:10001:10101:10101:10101:01010", // 119 w
        "00000:00000:10001:01010:00100:01010:10001", // 120 x
        "00000:00000:10001:10001:01111:00001:11110", // 121 y
        "00000:00000:11111:00010:00100:01000:11111", // 122 z
    };

    static void DrawChar(Texture2D tex, int x, int y, char ch, Color c, int scale)
    {
        int idx = (int)ch - 32;
        if (idx < 0 || idx >= FONT5x7.Length) return;
        var rows = FONT5x7[idx].Split(':');
        for (int row = 0; row < rows.Length && row < 7; row++)
        {
            string rowStr = rows[row];
            int ty = y + (6 - row) * scale; // bottom-up
            for (int col = 0; col < rowStr.Length && col < 5; col++)
            {
                if (rowStr[col] == '1')
                {
                    int tx = x + col * scale;
                    FillRect(tex, tx, ty, scale, scale, c);
                }
            }
        }
    }

    static Color Hex(string hex)
    {
        hex = hex.TrimStart('#');
        float r = System.Convert.ToInt32(hex.Substring(0,2), 16) / 255f;
        float g = System.Convert.ToInt32(hex.Substring(2,2), 16) / 255f;
        float b = System.Convert.ToInt32(hex.Substring(4,2), 16) / 255f;
        return new Color(r, g, b, 1f);
    }
}
