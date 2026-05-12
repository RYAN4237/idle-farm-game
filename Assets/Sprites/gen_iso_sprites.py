"""
Generate isometric/oblique-perspective farm sprite sheets matching sample_UI.png style.
Tile size: 48x48px (slightly larger for more detail)
Style: 45-degree oblique top-down, pixel art, cozy farm aesthetic
"""
from PIL import Image, ImageDraw, ImageFilter
import math, random

T = 48  # tile size

def new_sheet(cols, rows):
    return Image.new("RGBA", (cols * T, rows * T), (0, 0, 0, 0))

def draw_tile(sheet, col, row, fn):
    img = Image.new("RGBA", (T, T), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    fn(img, d)
    sheet.paste(img, (col * T, row * T))

# ── Color palette (matched from sample_UI.png) ───────────────────────────
C = {
    # Grass
    "g_base":  ( 88, 148,  52, 255),
    "g_light": (108, 172,  64, 255),
    "g_dark":  ( 64, 108,  36, 255),
    "g_mid":   ( 76, 128,  44, 255),
    # Water
    "w_base":  ( 72, 136, 180, 255),
    "w_light": (100, 164, 208, 255),
    "w_dark":  ( 48,  96, 136, 255),
    "w_foam":  (200, 228, 244, 255),
    # Sand/bank
    "sand":    (196, 172, 112, 255),
    "sand_d":  (168, 144,  88, 255),
    "sand_l":  (220, 200, 144, 255),
    # Soil
    "soil":    (120,  80,  44, 255),
    "soil_d":  ( 92,  60,  32, 255),
    "soil_l":  (148, 104,  60, 255),
    # Wood/bridge
    "wood":    (148,  96,  48, 255),
    "wood_d":  (108,  68,  28, 255),
    "wood_l":  (180, 128,  72, 255),
    "wood_s":  ( 80,  52,  20, 255),  # shadow
    # Rock
    "rock":    (148, 144, 132, 255),
    "rock_d":  (108, 104,  92, 255),
    "rock_l":  (188, 184, 172, 255),
    "rock_s":  ( 80,  76,  68, 255),  # shadow
    # Tree
    "leaf_d":  ( 48, 112,  32, 255),
    "leaf_m":  ( 72, 148,  48, 255),
    "leaf_l":  ( 96, 180,  64, 255),
    "leaf_hl": (128, 208,  80, 255),
    "trunk_d": ( 80,  52,  24, 255),
    "trunk_m": (108,  72,  36, 255),
    "trunk_l": (136,  96,  52, 255),
    # Fruit/crops
    "orange":  (224, 128,  36, 255),
    "orange_l":(252, 164,  56, 255),
    "orange_d":(172,  88,  20, 255),
    "crop_g":  ( 80, 152,  44, 255),
    "pumpkin": (208, 104,  28, 255),
    # Sky
    "sky":     (136, 196, 220, 255),
    "cloud":   (248, 248, 248, 255),
    "cloud_s": (220, 224, 232, 255),
    # Fence
    "fence":   (164, 120,  64, 255),
    "fence_d": (120,  84,  40, 255),
}

# ════════════════════════════════════════════════════════════════════════════
# SHEET 1: Ground tiles  8 cols × 6 rows = 48 tiles
# Row 0: grass variants (4 plain + 4 with detail)
# Row 1: soil/dirt (4 plain + 4 row variants)
# Row 2: water tiles (8 variants)
# Row 3: sand bank (8 variants)
# Row 4: grass-water edges (8 directional transitions)
# Row 5: bridge planks (8 variants)
# ════════════════════════════════════════════════════════════════════════════
ground = new_sheet(8, 6)

# ── Row 0: Grass ─────────────────────────────────────────────────────────
def grass(img, d, seed=0, detail=False):
    rng = random.Random(seed)
    # Base fill
    d.rectangle([0,0,T-1,T-1], fill=C["g_base"])
    # Subtle diagonal shading for oblique perspective feel
    for y in range(0, T, 3):
        shade = C["g_dark"] if (y // 3) % 3 == 0 else C["g_base"]
        for x in range(T):
            if rng.random() < 0.15:
                d.point((x, y), fill=shade)
    # Texture variation
    for _ in range(20):
        x, y = rng.randint(0, T-1), rng.randint(0, T-1)
        c = rng.choice([C["g_light"], C["g_dark"], C["g_mid"]])
        d.point((x, y), fill=c)
    if detail:
        # Small grass tufts
        for _ in range(rng.randint(2, 4)):
            bx = rng.randint(4, T-5)
            by = rng.randint(T//2, T-6)
            col = rng.choice([C["g_light"], C["g_dark"]])
            d.line([(bx, by+4), (bx-2, by)], fill=col, width=1)
            d.line([(bx, by+4), (bx+2, by+1)], fill=col, width=1)

for i in range(4):
    draw_tile(ground, i,   0, lambda img,d,s=i:   grass(img,d,s,   False))
    draw_tile(ground, i+4, 0, lambda img,d,s=i+4: grass(img,d,s,   True))

# ── Row 1: Soil ──────────────────────────────────────────────────────────
def soil(img, d, seed=0):
    rng = random.Random(seed + 100)
    d.rectangle([0,0,T-1,T-1], fill=C["soil"])
    # Tilled rows - horizontal stripes for oblique angle
    for ry in range(0, T, 6):
        c = C["soil_d"] if (ry//6) % 2 == 0 else C["soil_l"]
        d.rectangle([0, ry, T-1, ry+4], fill=c)
        d.line([(0, ry+5), (T-1, ry+5)], fill=C["soil_d"], width=1)
    for _ in range(10):
        x, y = rng.randint(0, T-1), rng.randint(0, T-1)
        d.point((x, y), fill=C["soil_d"])

for i in range(8):
    draw_tile(ground, i, 1, lambda img,d,s=i: soil(img,d,s))

# ── Row 2: Water ─────────────────────────────────────────────────────────
def water(img, d, seed=0):
    rng = random.Random(seed + 200)
    d.rectangle([0,0,T-1,T-1], fill=C["w_base"])
    # Wave lines - slight diagonal for oblique feel
    for _ in range(3):
        wx = rng.randint(2, T-12)
        wy = rng.randint(4, T-8)
        wlen = rng.randint(8, 16)
        d.line([(wx, wy), (wx+wlen, wy-2)], fill=C["w_light"], width=1)
        d.line([(wx+2, wy+1), (wx+wlen-2, wy-1)], fill=C["w_foam"], width=1)
    # Ripple dots
    for _ in range(8):
        x, y = rng.randint(1, T-2), rng.randint(1, T-2)
        d.point((x, y), fill=rng.choice([C["w_light"], C["w_dark"]]))

for i in range(8):
    draw_tile(ground, i, 2, lambda img,d,s=i: water(img,d,s))

# ── Row 3: Sand bank ─────────────────────────────────────────────────────
def sand_tile(img, d, seed=0):
    rng = random.Random(seed + 300)
    d.rectangle([0,0,T-1,T-1], fill=C["sand"])
    for _ in range(16):
        x, y = rng.randint(0, T-1), rng.randint(0, T-1)
        d.point((x, y), fill=rng.choice([C["sand_d"], C["sand_l"]]))
    # Pebble dots
    for _ in range(4):
        px, py = rng.randint(2, T-3), rng.randint(2, T-3)
        d.ellipse([px-2, py-1, px+2, py+1], fill=C["sand_d"])

for i in range(8):
    draw_tile(ground, i, 3, lambda img,d,s=i: sand_tile(img,d,s))

# ── Row 4: Grass-water transitions ───────────────────────────────────────
def trans_grass_to_water_top(img, d):
    "Upper half = grass, lower half = water (upper bank)"
    d.rectangle([0, 0, T-1, T//2-2], fill=C["g_base"])
    d.rectangle([0, T//2+2, T-1, T-1], fill=C["w_base"])
    # Transition band - sand strip
    d.rectangle([0, T//2-2, T-1, T//2+2], fill=C["sand"])
    # Grass texture on top half
    for x in range(0, T, 2):
        if x % 6 < 3:
            d.point((x, T//2-4), fill=C["g_dark"])
    # Water shimmer on bottom
    d.line([(4, T//2+5), (T-8, T//2+4)], fill=C["w_light"], width=1)

def trans_grass_to_water_bot(img, d):
    "Lower half = grass, upper half = water (lower bank)"
    d.rectangle([0, 0, T-1, T//2-2], fill=C["w_base"])
    d.rectangle([0, T//2+2, T-1, T-1], fill=C["g_base"])
    d.rectangle([0, T//2-2, T-1, T//2+2], fill=C["sand"])
    d.line([(4, T//2-5), (T-8, T//2-4)], fill=C["w_light"], width=1)
    for x in range(0, T, 2):
        if x % 6 < 3:
            d.point((x, T//2+4), fill=C["g_dark"])

def trans_grass_only(img, d, seed=0):
    grass(img, d, seed)

draw_tile(ground, 0, 4, trans_grass_to_water_top)
draw_tile(ground, 1, 4, trans_grass_to_water_bot)
for i in range(2, 8):
    draw_tile(ground, i, 4, lambda img,d,s=i: trans_grass_only(img,d,s))

# ── Row 5: Bridge planks ──────────────────────────────────────────────────
def bridge_plank(img, d, seed=0, variant=0):
    rng = random.Random(seed + 400)
    d.rectangle([0,0,T-1,T-1], fill=C["wood"])
    # Horizontal plank lines
    for py in range(0, T, 8):
        c = C["wood_l"] if (py//8) % 2 == 0 else C["wood_d"]
        d.rectangle([0, py, T-1, py+6], fill=c)
        d.line([(0, py+7), (T-1, py+7)], fill=C["wood_d"], width=1)
    # Grain lines
    for _ in range(4):
        gx = rng.randint(2, T-2)
        d.line([(gx, 0), (gx, T-1)], fill=C["wood_d"], width=1)
    # Nail dots
    for ny in range(4, T, 8):
        d.ellipse([3, ny-1, 5, ny+1], fill=C["wood_s"])
        d.ellipse([T-5, ny-1, T-3, ny+1], fill=C["wood_s"])

for i in range(8):
    draw_tile(ground, i, 5, lambda img,d,s=i: bridge_plank(img,d,s))

ground.save("/Users/I755634/Repo/idle/Assets/Sprites/IsoFarm_Ground.png")
print(f"Ground sheet saved: {8*T}x{6*T}")

# ════════════════════════════════════════════════════════════════════════════
# SHEET 2: Objects/Decorations  8 cols × 12 rows = 96 tiles
# Row 0-1: Large oak tree (4-tile: TL,TR / BL,BR) — x2 variants cols 0-3 and 4-7
# Row 2-3: Fruit tree (4-tile) cols 0-3 + small tree cols 4-7
# Row 4:   Rocks (large L/R cols 0-1, medium cols 2-3, small cols 4-7)
# Row 5:   Crops (pumpkin/vegetable patches, 8 variants)
# Row 6:   Fence horizontal segments (4) + fence posts (4)
# Row 7:   Grass tufts/bushes (8)
# Row 8:   Bridge rails (L-post, mid, R-post) + shadow
# Row 9:   Cloud tiles (4 variants)
# Row 10:  Sky (4 variants with clouds)
# Row 11:  Extra decor (fallen log, flower patch, etc.)
# ════════════════════════════════════════════════════════════════════════════
deco = new_sheet(8, 12)

# ── Rows 0-1: Large Oak Tree (oblique perspective) ────────────────────────
# The tree is drawn as a 2x2 tile assembly (96x96 px total)
# Perspective: we see slightly from the side, so crown is round, trunk shows depth

def oak_canopy(img, d, qx, qy, seed=0, with_fruits=False):
    """
    Draw one quadrant of a large oak canopy.
    qx,qy = which quadrant (0=left, 1=right / 0=top, 1=bottom)
    with_fruits=False for plain oak, True for fruit tree.
    """
    rng = random.Random(seed)
    cx, cy = T, int(T * 0.55)
    ox = qx * T
    oy = qy * T

    full = Image.new("RGBA", (T*2, T*2), (0,0,0,0))
    fd = ImageDraw.Draw(full)

    # Shadow/dark base
    fd.ellipse([cx-34+4, cy-28+4, cx+34+4, cy+28+4], fill=C["leaf_d"])
    # Main body
    fd.ellipse([cx-34, cy-28, cx+34, cy+28], fill=C["leaf_m"])
    # Light area
    fd.ellipse([cx-26, cy-22, cx+8, cy+6], fill=C["leaf_l"])
    fd.ellipse([cx-18, cy-18, cx, cy-2], fill=C["leaf_hl"])
    # Detail bumps on silhouette
    for angle_deg, r in [(30,36),(70,34),(110,32),(150,36),(200,33),(260,35),(310,34)]:
        a = math.radians(angle_deg)
        bx = cx + int(r * math.cos(a))
        by = cy + int(r * 0.78 * math.sin(a))
        fd.ellipse([bx-6, by-5, bx+6, by+5], fill=C["leaf_d"])
        fd.ellipse([bx-4, by-4, bx+4, by+3], fill=C["leaf_m"])

    if with_fruits:
        for _ in range(6):
            fx = rng.randint(cx-28, cx+28)
            fy = rng.randint(cy-20, cy+20)
            fd.ellipse([fx-3, fy-3, fx+3, fy+3], fill=C["orange"])
            fd.ellipse([fx-1, fy-1, fx+1, fy+1], fill=C["orange_l"])

    crop = full.crop((ox, oy, ox+T, oy+T))
    img.paste(crop, (0, 0), crop)

def oak_trunk_base(img, d, side="L"):
    """Bottom of trunk visible in BL/BR tiles"""
    # Trunk rectangle - slightly tapered
    tx = T//2 - 5 if side == "L" else T//2 - 3
    d.rectangle([tx, 0, tx+10, T-1], fill=C["trunk_m"])
    d.rectangle([tx, 0, tx+3, T-1], fill=C["trunk_l"])   # light left
    d.rectangle([tx+7, 0, tx+10, T-1], fill=C["trunk_d"]) # dark right
    # Root spread at bottom
    d.ellipse([tx-8, T-12, tx+20, T+2], fill=C["trunk_d"])
    d.ellipse([tx-5, T-10, tx+18, T+2], fill=C["trunk_m"])
    # Ground shadow
    d.ellipse([tx-12, T-6, tx+28, T+6], fill=(30, 60, 20, 60))

# Oak variant 1 (cols 0-1, rows 0-1) — pure oak, no fruits
draw_tile(deco, 0, 0, lambda img,d: oak_canopy(img,d, 0,0, 1, False))
draw_tile(deco, 1, 0, lambda img,d: oak_canopy(img,d, 1,0, 1, False))
draw_tile(deco, 0, 1, lambda img,d: (oak_canopy(img,d, 0,1, 1, False), oak_trunk_base(img,d,"L")) and None or None)
draw_tile(deco, 1, 1, lambda img,d: (oak_canopy(img,d, 1,1, 1, False), oak_trunk_base(img,d,"R")) and None or None)

# Oak variant 2 slightly different (cols 2-3, rows 0-1) — pure oak, no fruits
draw_tile(deco, 2, 0, lambda img,d: oak_canopy(img,d, 0,0, 7, False))
draw_tile(deco, 3, 0, lambda img,d: oak_canopy(img,d, 1,0, 7, False))
draw_tile(deco, 2, 1, lambda img,d: (oak_canopy(img,d, 0,1, 7, False), oak_trunk_base(img,d,"L")) and None or None)
draw_tile(deco, 3, 1, lambda img,d: (oak_canopy(img,d, 1,1, 7, False), oak_trunk_base(img,d,"R")) and None or None)

# Smaller tree (cols 4-7, rows 0-1) - single tile, taller shape, no fruits
def small_tree(img, d, seed=0):
    rng = random.Random(seed)
    # Trunk
    d.rectangle([T//2-3, T//2, T//2+3, T-4], fill=C["trunk_m"])
    d.rectangle([T//2-3, T//2, T//2-1, T-4], fill=C["trunk_l"])
    d.rectangle([T//2+1, T//2, T//2+3, T-4], fill=C["trunk_d"])
    # Shadow
    d.ellipse([T//2-10, T-8, T//2+10, T+4], fill=(30,60,20,50))
    # Canopy (smaller)
    d.ellipse([T//2-14, 4, T//2+14, T//2+8], fill=C["leaf_d"])
    d.ellipse([T//2-12, 2, T//2+12, T//2+6], fill=C["leaf_m"])
    d.ellipse([T//2-8, 2, T//2+4, T//2], fill=C["leaf_l"])

for i in range(4):
    draw_tile(deco, 4+i, 0, lambda img,d,s=i: small_tree(img,d,s))
    draw_tile(deco, 4+i, 1, lambda img,d: None)  # empty bottom row for small trees

# ── Rows 2-3: Fruit tree (2x2) ───────────────────────────────────────────
def fruit_canopy(img, d, qx, qy):
    full = Image.new("RGBA", (T*2, T*2), (0,0,0,0))
    fd = ImageDraw.Draw(full)
    cx, cy = T, int(T*0.5)
    # Shadow
    fd.ellipse([cx-28+3, cy-22+3, cx+28+3, cy+22+3], fill=C["leaf_d"])
    fd.ellipse([cx-28, cy-22, cx+28, cy+22], fill=C["leaf_m"])
    fd.ellipse([cx-20, cy-16, cx+6, cy+4], fill=C["leaf_l"])
    # Orange fruits — kept strictly inside canopy ellipse
    rng = random.Random(42)
    for _ in range(12):
        fx = rng.randint(cx-16, cx+16)
        fy = rng.randint(cy-10, cy+12)  # shifted down slightly, away from top edge
        # Only draw if inside ellipse
        if ((fx-cx)/22)**2 + ((fy-cy)/16)**2 <= 1.0:
            fd.ellipse([fx-4, fy-4, fx+4, fy+4], fill=C["orange"])
            fd.ellipse([fx-2, fy-3, fx+2, fy+1], fill=C["orange_l"])
            fd.point((fx, fy-4), fill=(80, 40, 0, 255))
    crop = full.crop((qx*T, qy*T, qx*T+T, qy*T+T))
    img.paste(crop, (0,0), crop)

draw_tile(deco, 0, 2, lambda img,d: fruit_canopy(img,d, 0,0))
draw_tile(deco, 1, 2, lambda img,d: fruit_canopy(img,d, 1,0))
draw_tile(deco, 0, 3, lambda img,d: (fruit_canopy(img,d, 0,1), oak_trunk_base(img,d,"L")) and None or None)
draw_tile(deco, 1, 3, lambda img,d: (fruit_canopy(img,d, 1,1), oak_trunk_base(img,d,"R")) and None or None)

# Cols 2-5 rows 2-3: bush variants
def bush(img, d, seed=0):
    rng = random.Random(seed + 50)
    cx, cy = T//2, T//2 + 4
    # Shadow
    d.ellipse([cx-14, cy+4, cx+14, cy+12], fill=(30,60,20,60))
    # Bush body
    for _ in range(4):
        ox = rng.randint(-10, 10)
        oy = rng.randint(-8, 6)
        r  = rng.randint(8, 13)
        c  = rng.choice([C["leaf_d"], C["leaf_m"], C["leaf_l"]])
        d.ellipse([cx+ox-r, cy+oy-r, cx+ox+r, cy+oy+r], fill=c)
    d.ellipse([cx-6, cy-10, cx+6, cy+2], fill=C["leaf_l"])

for i in range(6):
    draw_tile(deco, 2+i, 2, lambda img,d,s=i: bush(img,d,s))
    draw_tile(deco, 2+i, 3, lambda img,d: None)

# ── Row 4: Rocks ──────────────────────────────────────────────────────────
def rock_large_L(img, d):
    d.rectangle([0,0,T-1,T-1], fill=(0,0,0,0))
    # Large rock left half
    d.ellipse([4, 8, T+8, T-4], fill=C["rock_d"])   # shadow
    d.ellipse([2, 6, T+6, T-6], fill=C["rock"])
    d.ellipse([4, 8, T, T-10], fill=C["rock_l"])     # highlight top
    d.arc([2, 6, T+6, T-6], 200, 340, fill=C["rock_d"], width=2)

def rock_large_R(img, d):
    d.rectangle([0,0,T-1,T-1], fill=(0,0,0,0))
    # Large rock right half
    d.ellipse([-8, 8, T-4, T-4], fill=C["rock_d"])
    d.ellipse([-6, 6, T-2, T-6], fill=C["rock"])
    d.ellipse([-4, 8, T-8, T-10], fill=C["rock_l"])
    d.arc([-6, 6, T-2, T-6], 200, 340, fill=C["rock_d"], width=2)

def rock_med(img, d, seed=0):
    rng = random.Random(seed + 400)
    d.rectangle([0,0,T-1,T-1], fill=(0,0,0,0))
    cx = T//2 + rng.randint(-4, 4)
    cy = T//2 + rng.randint(-2, 4)
    w, h = rng.randint(12, 18), rng.randint(9, 14)
    d.ellipse([cx-w+2, cy-h+3, cx+w+2, cy+h+3], fill=C["rock_d"])
    d.ellipse([cx-w, cy-h, cx+w, cy+h], fill=C["rock"])
    d.ellipse([cx-w+3, cy-h+2, cx, cy+2], fill=C["rock_l"])
    d.arc([cx-w, cy-h, cx+w, cy+h], 210, 330, fill=C["rock_d"], width=2)

def rock_small(img, d, seed=0):
    rng = random.Random(seed + 500)
    d.rectangle([0,0,T-1,T-1], fill=(0,0,0,0))
    cx = T//2 + rng.randint(-6, 6)
    cy = T//2 + rng.randint(-4, 6)
    w, h = rng.randint(6, 10), rng.randint(5, 8)
    d.ellipse([cx-w+1, cy-h+2, cx+w+1, cy+h+2], fill=C["rock_d"])
    d.ellipse([cx-w, cy-h, cx+w, cy+h], fill=C["rock"])
    d.ellipse([cx-w+2, cy-h+2, cx+1, cy+1], fill=C["rock_l"])

draw_tile(deco, 0, 4, rock_large_L)
draw_tile(deco, 1, 4, rock_large_R)
for i in range(2):
    draw_tile(deco, 2+i, 4, lambda img,d,s=i: rock_med(img,d,s))
for i in range(4):
    draw_tile(deco, 4+i, 4, lambda img,d,s=i: rock_small(img,d,s))

# ── Row 5: Crops (pumpkin patch) ──────────────────────────────────────────
def crop_pumpkin(img, d, seed=0):
    rng = random.Random(seed + 600)
    # Soil base
    d.rectangle([0,0,T-1,T-1], fill=C["soil"])
    for ry in range(0, T, 6):
        d.rectangle([0,ry,T-1,ry+4], fill=C["soil_d"] if (ry//6)%2==0 else C["soil_l"])
    # Leaves
    cx, cy = T//2, T//2
    d.ellipse([cx-10, cy-8, cx+10, cy+8], fill=C["crop_g"])
    d.ellipse([cx-7, cy-10, cx+7, cy-2], fill=C["leaf_m"])
    # Pumpkin body
    d.ellipse([cx-7, cy-5, cx+7, cy+7], fill=C["orange_d"])
    d.ellipse([cx-5, cy-3, cx+5, cy+5], fill=C["pumpkin"])
    d.ellipse([cx-2, cy-2, cx+2, cy+2], fill=C["orange_l"])
    # Ribs
    d.line([(cx, cy-5), (cx, cy+7)], fill=C["orange_d"], width=1)
    # Stem
    d.line([(cx, cy-5), (cx+2, cy-8)], fill=(80,40,0,255), width=1)

def crop_veggie(img, d, seed=0):
    rng = random.Random(seed + 700)
    d.rectangle([0,0,T-1,T-1], fill=C["soil"])
    for ry in range(0, T, 6):
        d.rectangle([0,ry,T-1,ry+4], fill=C["soil_d"] if (ry//6)%2==0 else C["soil_l"])
    # Multiple small plants
    for _ in range(rng.randint(2,4)):
        px = rng.randint(6, T-7)
        py = rng.randint(8, T-8)
        d.ellipse([px-5, py-4, px+5, py+4], fill=C["crop_g"])
        d.ellipse([px-3, py-6, px+3, py+1], fill=C["leaf_l"])
        d.ellipse([px-2, py-2, px+2, py+2], fill=C["orange"])

for i in range(4):
    draw_tile(deco, i,   5, lambda img,d,s=i:   crop_pumpkin(img,d,s))
    draw_tile(deco, i+4, 5, lambda img,d,s=i+4: crop_veggie(img,d,s))

# ── Row 6: Fence segments ─────────────────────────────────────────────────
def fence_horiz(img, d):
    d.rectangle([0,0,T-1,T-1], fill=(0,0,0,0))
    # Two horizontal rails
    mid = T//2
    d.rectangle([0, mid-6, T-1, mid-3], fill=C["fence"])
    d.rectangle([0, mid-6, T-1, mid-6], fill=C["fence_d"])
    d.rectangle([0, mid+2, T-1, mid+5], fill=C["fence"])
    # Post in middle
    d.rectangle([T//2-3, mid-10, T//2+3, mid+9], fill=C["fence"])
    d.rectangle([T//2-3, mid-10, T//2-1, mid+9], fill=C["wood_l"])

def fence_post(img, d):
    d.rectangle([0,0,T-1,T-1], fill=(0,0,0,0))
    mid = T//2
    d.rectangle([mid-4, 8, mid+4, T-4], fill=C["fence"])
    d.rectangle([mid-4, 8, mid-1, T-4], fill=C["wood_l"])
    d.rectangle([mid+2, 8, mid+4, T-4], fill=C["fence_d"])
    # Cap
    d.rectangle([mid-5, 6, mid+5, 10], fill=C["fence"])

for i in range(4):
    draw_tile(deco, i,   6, fence_horiz)
    draw_tile(deco, i+4, 6, fence_post)

# ── Row 7: Grass tufts / flower bushes ────────────────────────────────────
def grass_tuft(img, d, seed=0):
    rng = random.Random(seed + 800)
    d.rectangle([0,0,T-1,T-1], fill=(0,0,0,0))
    cx = T//2
    by = T - 8
    for _ in range(rng.randint(3, 6)):
        bx = rng.randint(cx-10, cx+10)
        h  = rng.randint(8, 16)
        c  = rng.choice([C["g_light"], C["g_dark"], C["leaf_m"]])
        d.polygon([(bx, by), (bx-3, by-h), (bx+3, by-h+2)], fill=c)
    if seed % 3 == 0:
        d.ellipse([cx-3, by-14, cx+3, by-8], fill=(220, 60, 60, 255))

for i in range(8):
    draw_tile(deco, i, 7, lambda img,d,s=i: grass_tuft(img,d,s))

# ── Row 8: Bridge rails & posts ───────────────────────────────────────────
def bridge_rail_post(img, d):
    d.rectangle([0,0,T-1,T-1], fill=(0,0,0,0))
    # Vertical post
    d.rectangle([T//2-3, 0, T//2+3, T-1], fill=C["wood"])
    d.rectangle([T//2-3, 0, T//2-1, T-1], fill=C["wood_l"])
    d.rectangle([T//2+1, 0, T//2+3, T-1], fill=C["wood_d"])
    # Horizontal rail
    d.rectangle([0, T//3, T-1, T//3+4], fill=C["wood"])
    d.rectangle([0, T//3, T-1, T//3+1], fill=C["wood_l"])
    d.rectangle([0, T*2//3, T-1, T*2//3+4], fill=C["wood"])

def bridge_rail_mid(img, d):
    d.rectangle([0,0,T-1,T-1], fill=(0,0,0,0))
    d.rectangle([0, T//3, T-1, T//3+4], fill=C["wood"])
    d.rectangle([0, T//3, T-1, T//3+1], fill=C["wood_l"])
    d.rectangle([0, T*2//3, T-1, T*2//3+4], fill=C["wood"])

draw_tile(deco, 0, 8, bridge_rail_post)
draw_tile(deco, 1, 8, bridge_rail_mid)
draw_tile(deco, 2, 8, bridge_rail_mid)
draw_tile(deco, 3, 8, bridge_rail_post)

# ── Row 9: Clouds ─────────────────────────────────────────────────────────
def cloud(img, d, seed=0, sky=True):
    rng = random.Random(seed + 900)
    if sky:
        d.rectangle([0,0,T-1,T-1], fill=C["sky"])
    cx = rng.randint(T//4, T*3//4)
    cy = rng.randint(T//4, T*2//3)
    # Cloud puffs
    for _ in range(rng.randint(3, 5)):
        ox = rng.randint(-12, 12)
        oy = rng.randint(-4, 4)
        r  = rng.randint(8, 14)
        d.ellipse([cx+ox-r, cy+oy-r, cx+ox+r, cy+oy+r], fill=C["cloud"])
    # Bottom shadow
    for _ in range(rng.randint(2, 4)):
        ox = rng.randint(-10, 10)
        r  = rng.randint(6, 11)
        d.ellipse([cx+ox-r, cy+4-r, cx+ox+r, cy+4+r], fill=C["cloud_s"])

for i in range(4):
    draw_tile(deco, i,   9, lambda img,d,s=i: cloud(img,d,s,   True))
    draw_tile(deco, i+4, 9, lambda img,d,s=i: cloud(img,d,s+4, False))

# ── Row 10: Sky background tiles ─────────────────────────────────────────
def sky_tile(img, d, seed=0):
    rng = random.Random(seed + 1000)
    # Gradient-like sky (lighter at top)
    for y in range(T):
        t = y / T
        r = int(136 + t * 20)
        g = int(196 + t * 20)
        b = int(220 + t * 16)
        d.line([(0, y), (T-1, y)], fill=(r, g, b, 255))
    # Occasional wisp
    if seed < 4:
        for _ in range(rng.randint(1, 3)):
            wx = rng.randint(2, T-8)
            wy = rng.randint(2, T//2)
            wl = rng.randint(6, 14)
            d.line([(wx, wy), (wx+wl, wy-1)], fill=(220,236,248,180), width=2)

for i in range(8):
    draw_tile(deco, i, 10, lambda img,d,s=i: sky_tile(img,d,s))

# ── Row 11: Extra decor ───────────────────────────────────────────────────
def flower_patch(img, d, seed=0):
    rng = random.Random(seed + 1100)
    d.rectangle([0,0,T-1,T-1], fill=(0,0,0,0))
    colors = [(220,60,60,255),(240,200,40,255),(200,80,220,255),(255,255,255,255)]
    for _ in range(rng.randint(3,5)):
        fx = rng.randint(4, T-5)
        fy = rng.randint(6, T-4)
        fc = rng.choice(colors)
        d.ellipse([fx-3, fy-3, fx+3, fy+3], fill=fc)
        d.ellipse([fx-1, fy-1, fx+1, fy+1], fill=(255,240,100,255))
        d.line([(fx, fy+3), (fx, fy+8)], fill=C["g_dark"], width=1)

def log_decor(img, d):
    d.rectangle([0,0,T-1,T-1], fill=(0,0,0,0))
    # Fallen log
    d.ellipse([4, T//2-6, T-4, T//2+6], fill=C["trunk_d"])
    d.ellipse([6, T//2-4, T-6, T//2+4], fill=C["trunk_m"])
    # End grain circle
    d.ellipse([T-10, T//2-5, T-2, T//2+5], fill=C["trunk_d"])
    d.ellipse([T-8, T//2-3, T-4, T//2+3], fill=C["trunk_l"])
    d.point((T-6, T//2), fill=C["trunk_m"])

for i in range(6):
    draw_tile(deco, i, 11, lambda img,d,s=i: flower_patch(img,d,s))
draw_tile(deco, 6, 11, log_decor)

deco.save("/Users/I755634/Repo/idle/Assets/Sprites/IsoFarm_Deco.png")
print(f"Deco sheet saved: {8*T}x{12*T}")
print("All done!")
