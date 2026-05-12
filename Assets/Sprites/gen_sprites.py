"""
Generate farm background sprite sheets (32x32 tiles, transparent PNG).
Matches sample_UI.png color palette for isometric/top-down cozy farm style.
"""
from PIL import Image, ImageDraw
import math, random

T = 32  # tile size

def new_sheet(cols, rows):
    return Image.new("RGBA", (cols * T, rows * T), (0, 0, 0, 0))

def draw_tile(sheet, col, row, fn):
    img = Image.new("RGBA", (T, T), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    fn(img, d)
    sheet.paste(img, (col * T, row * T))

# ── colors from sample_UI.png ──────────────────────────────────────────────
C = {
    "sky":      (135, 194, 210, 255),
    "grass1":   ( 82, 140,  50, 255),  # bright upper grass
    "grass2":   ( 68, 118,  40, 255),  # mid grass
    "grass3":   ( 55,  98,  30, 255),  # dark grass shadow
    "grass4":   ( 96, 156,  58, 255),  # highlight grass
    "soil":     (120,  85,  48, 255),  # tilled soil
    "soil2":    (100,  70,  38, 255),  # soil dark
    "soil3":    (145, 105,  60, 255),  # soil light
    "water1":   ( 60, 120, 165, 255),  # river main
    "water2":   ( 80, 145, 190, 255),  # water highlight
    "water3":   ( 45,  95, 135, 255),  # water shadow
    "sand":     (195, 168, 110, 255),  # riverbank sand
    "sand2":    (215, 192, 135, 255),
    "wood":     (140,  90,  45, 255),  # bridge plank
    "wood2":    (110,  70,  30, 255),  # wood dark
    "wood3":    (165, 115,  60, 255),  # wood light
    "rock":     (140, 135, 125, 255),  # stone
    "rock2":    (110, 105,  95, 255),
    "rock3":    (165, 162, 152, 255),
    "leaf1":    ( 40, 110,  30, 255),  # oak dark leaf
    "leaf2":    ( 55, 140,  40, 255),  # oak mid leaf
    "leaf3":    ( 75, 165,  55, 255),  # oak highlight
    "trunk":    ( 95,  60,  25, 255),
    "trunk2":   ( 70,  44,  15, 255),
    "orange":   (220, 130,  40, 255),  # fruit/crop
    "orange2":  (245, 155,  55, 255),
    "crop_g":   ( 80, 155,  45, 255),  # crop leaves
    "flower_y": (230, 200,  50, 255),
    "flower_w": (235, 230, 210, 255),
    "flower_p": (180, 120, 200, 255),
}

# ════════════════════════════════════════════════════════════
# SHEET 1: Ground tiles (8 cols × 4 rows = grass + soil + water + transitions)
# ════════════════════════════════════════════════════════════
ground = new_sheet(8, 6)

def grass_base(img, d, variant=0):
    """Grass tile - plain and variations"""
    g1, g2, g3, g4 = C["grass1"], C["grass2"], C["grass3"], C["grass4"]
    # fill base
    d.rectangle([0, 0, T-1, T-1], fill=g2)
    # random texture dots
    rng = random.Random(variant * 17 + 3)
    for _ in range(18):
        x, y = rng.randint(0, T-1), rng.randint(0, T-1)
        c = rng.choice([g1, g3, g4])
        d.point((x, y), fill=c)
    for _ in range(6):
        x, y = rng.randint(1, T-2), rng.randint(1, T-2)
        d.rectangle([x, y, x+1, y+1], fill=rng.choice([g1, g4]))

def grass_flower(img, d, variant=0):
    grass_base(img, d, variant + 100)
    rng = random.Random(variant * 31 + 7)
    for _ in range(rng.randint(2, 4)):
        x, y = rng.randint(3, T-4), rng.randint(3, T-4)
        fc = rng.choice([C["flower_y"], C["flower_w"], C["flower_p"]])
        d.ellipse([x-1, y-1, x+1, y+1], fill=fc)

# Row 0: plain grass variants
for i in range(4):
    draw_tile(ground, i, 0, lambda img, d, v=i: grass_base(img, d, v))
for i in range(4):
    draw_tile(ground, i+4, 0, lambda img, d, v=i: grass_flower(img, d, v))

# Row 1: soil/tilled dirt
def soil_tile(img, d, variant=0):
    d.rectangle([0, 0, T-1, T-1], fill=C["soil"])
    rng = random.Random(variant * 23 + 5)
    for row_y in range(0, T, 4):
        shade = rng.choice([C["soil2"], C["soil3"], C["soil"]])
        d.rectangle([0, row_y, T-1, row_y+2], fill=shade)
    for _ in range(8):
        x, y = rng.randint(0, T-1), rng.randint(0, T-1)
        d.point((x, y), fill=C["soil2"])

for i in range(8):
    draw_tile(ground, i, 1, lambda img, d, v=i: soil_tile(img, d, v))

# Row 2: water tiles
def water_tile(img, d, variant=0):
    d.rectangle([0, 0, T-1, T-1], fill=C["water1"])
    rng = random.Random(variant * 19 + 2)
    # wave highlights
    for _ in range(4):
        wx = rng.randint(2, T-6)
        wy = rng.randint(2, T-4)
        ww = rng.randint(4, 10)
        d.line([(wx, wy), (wx+ww, wy)], fill=C["water2"], width=1)
        d.line([(wx+1, wy+1), (wx+ww-1, wy+1)], fill=C["water3"], width=1)
    for _ in range(10):
        x, y = rng.randint(0, T-1), rng.randint(0, T-1)
        d.point((x, y), fill=rng.choice([C["water2"], C["water3"]]))

for i in range(8):
    draw_tile(ground, i, 2, lambda img, d, v=i: water_tile(img, d, v))

# Row 3: water-to-grass edges (top edge, bottom edge, left, right)
def water_edge_top(img, d):
    "Grass on bottom, water on top"
    d.rectangle([0, 0, T-1, T//2-1], fill=C["water1"])
    d.rectangle([0, T//2, T-1, T-1], fill=C["grass2"])
    # transition row
    for x in range(T):
        shade = C["grass3"] if x % 3 == 0 else C["grass2"]
        d.point((x, T//2), fill=shade)

def water_edge_bot(img, d):
    "Water on bottom, grass on top"
    d.rectangle([0, 0, T-1, T//2-1], fill=C["grass2"])
    d.rectangle([0, T//2, T-1, T-1], fill=C["water1"])
    for x in range(T):
        d.point((x, T//2-1), fill=C["grass3"])

def sand_bank(img, d, variant=0):
    d.rectangle([0, 0, T-1, T-1], fill=C["sand"])
    rng = random.Random(variant * 13 + 1)
    for _ in range(12):
        x, y = rng.randint(0, T-1), rng.randint(0, T-1)
        d.point((x, y), fill=rng.choice([C["sand2"], C["sand"]]))

draw_tile(ground, 0, 3, water_edge_top)
draw_tile(ground, 1, 3, water_edge_bot)
for i in range(6):
    draw_tile(ground, i+2, 3, lambda img, d, v=i: sand_bank(img, d, v))

# Row 4: grass-to-soil path transitions
def grass_soil_edge(img, d, side="top"):
    half = T // 2
    if side == "top":
        d.rectangle([0, 0, T-1, half], fill=C["soil"])
        d.rectangle([0, half+1, T-1, T-1], fill=C["grass2"])
        for x in range(T):
            d.point((x, half), fill=C["grass3"])
    elif side == "bot":
        d.rectangle([0, 0, T-1, half], fill=C["grass2"])
        d.rectangle([0, half+1, T-1, T-1], fill=C["soil"])
        for x in range(T):
            d.point((x, half), fill=C["soil2"])

draw_tile(ground, 0, 4, lambda img, d: grass_soil_edge(img, d, "top"))
draw_tile(ground, 1, 4, lambda img, d: grass_soil_edge(img, d, "bot"))

# Row 5: bridge planks
def bridge_plank(img, d, variant=0):
    d.rectangle([0, 0, T-1, T-1], fill=C["wood"])
    for gy in range(0, T, 6):
        c = C["wood3"] if (gy // 6) % 2 == 0 else C["wood2"]
        d.rectangle([0, gy, T-1, gy+5], fill=c)
    # plank lines
    for gy in range(0, T, 6):
        d.line([(0, gy), (T-1, gy)], fill=C["wood2"], width=1)
    # side grain
    rng = random.Random(variant * 7 + 4)
    for _ in range(6):
        gx = rng.randint(0, T-1)
        d.line([(gx, 0), (gx, T-1)], fill=C["wood2"], width=1)

for i in range(8):
    draw_tile(ground, i, 5, lambda img, d, v=i: bridge_plank(img, d, v))

ground.save("/Users/I755634/Repo/idle/Assets/Sprites/FarmBG_Ground.png")
print("Ground sheet saved: 256x192")

# ════════════════════════════════════════════════════════════
# SHEET 2: Decorations (trees, rocks, crops) — 8 cols × 8 rows
# ════════════════════════════════════════════════════════════
deco = new_sheet(8, 10)

# ── Large Oak Tree (4 tiles: TL, TR, BL, BR) ──────────────────────────────
# Each tile = 32x32, assembled 2×2 = 64×64 tree
def oak_TL(img, d):
    "Top-Left canopy"
    d.rectangle([0, 0, T-1, T-1], fill=(0,0,0,0))
    # curved canopy edge - top-left quadrant
    d.ellipse([0, 0, T*2-1, T*2-1], fill=C["leaf2"])  # oversized, only TL shows
    # highlights
    for (x,y) in [(5,5),(8,3),(3,8),(10,6)]:
        d.ellipse([x-2,y-2,x+3,y+3], fill=C["leaf3"])
    # shadows at bottom-right
    for (x,y) in [(18,20),(22,18),(26,24)]:
        d.ellipse([x-2,y-2,x+3,y+3], fill=C["leaf1"])

def oak_TR(img, d):
    "Top-Right canopy"
    d.rectangle([0, 0, T-1, T-1], fill=(0,0,0,0))
    d.ellipse([-T, 0, T-1, T*2-1], fill=C["leaf2"])
    for (x,y) in [(20,4),(25,7),(28,3),(15,6)]:
        d.ellipse([x-2,y-2,x+3,y+3], fill=C["leaf3"])
    for (x,y) in [(8,18),(5,22),(10,26)]:
        d.ellipse([x-2,y-2,x+3,y+3], fill=C["leaf1"])

def oak_BL(img, d):
    "Bottom-Left canopy + trunk left"
    d.rectangle([0, 0, T-1, T-1], fill=(0,0,0,0))
    d.ellipse([0, -T, T*2-1, T-1], fill=C["leaf2"])
    for (x,y) in [(4,20),(6,25),(3,28)]:
        d.ellipse([x-2,y-2,x+3,y+3], fill=C["leaf3"])
    # trunk
    d.rectangle([12, 22, 18, T-1], fill=C["trunk"])
    d.rectangle([12, 22, 14, T-1], fill=C["trunk2"])
    d.rectangle([17, 22, 18, T-1], fill=C["wood3"])

def oak_BR(img, d):
    "Bottom-Right canopy + trunk right"
    d.rectangle([0, 0, T-1, T-1], fill=(0,0,0,0))
    d.ellipse([-T, -T, T-1, T-1], fill=C["leaf2"])
    for (x,y) in [(22,20),(26,25),(28,28)]:
        d.ellipse([x-2,y-2,x+3,y+3], fill=C["leaf1"])
    # trunk
    d.rectangle([12, 22, 18, T-1], fill=C["trunk"])
    d.rectangle([15, 22, 18, T-1], fill=C["trunk2"])

draw_tile(deco, 0, 0, oak_TL)
draw_tile(deco, 1, 0, oak_TR)
draw_tile(deco, 0, 1, oak_BL)
draw_tile(deco, 1, 1, oak_BR)

# ── Small/Fruit Tree (2×2) ─────────────────────────────────────────────────
def fruit_TL(img, d):
    d.rectangle([0,0,T-1,T-1], fill=(0,0,0,0))
    # orange-green canopy
    d.ellipse([2, 2, T*2-4, T*2-4], fill=C["leaf2"])
    # orange fruit dots
    for (x,y) in [(6,6),(10,4),(4,10),(14,8)]:
        d.ellipse([x-2,y-2,x+2,y+2], fill=C["orange"])

def fruit_TR(img, d):
    d.rectangle([0,0,T-1,T-1], fill=(0,0,0,0))
    d.ellipse([-T+4, 2, T-2, T*2-4], fill=C["leaf2"])
    for (x,y) in [(18,5),(24,8),(28,5),(20,10)]:
        d.ellipse([x-2,y-2,x+2,y+2], fill=C["orange2"])

def fruit_BL(img, d):
    d.rectangle([0,0,T-1,T-1], fill=(0,0,0,0))
    d.ellipse([2, -T+4, T*2-4, T-2], fill=C["leaf2"])
    for (x,y) in [(4,20),(8,26),(5,28)]:
        d.ellipse([x-2,y-2,x+2,y+2], fill=C["orange"])
    d.rectangle([13, 18, 18, T-1], fill=C["trunk"])

def fruit_BR(img, d):
    d.rectangle([0,0,T-1,T-1], fill=(0,0,0,0))
    d.ellipse([-T+4, -T+4, T-2, T-2], fill=C["leaf2"])
    for (x,y) in [(22,20),(26,24)]:
        d.ellipse([x-2,y-2,x+2,y+2], fill=C["orange2"])
    d.rectangle([13, 18, 18, T-1], fill=C["trunk"])

draw_tile(deco, 2, 0, fruit_TL)
draw_tile(deco, 3, 0, fruit_TR)
draw_tile(deco, 2, 1, fruit_BL)
draw_tile(deco, 3, 1, fruit_BR)

# ── Bush (1 tile) ──────────────────────────────────────────────────────────
def bush(img, d, variant=0):
    d.rectangle([0,0,T-1,T-1], fill=(0,0,0,0))
    rng = random.Random(variant * 11 + 3)
    cx, cy = T//2, T//2 + 4
    for _ in range(5):
        ox = rng.randint(-8, 8)
        oy = rng.randint(-6, 6)
        r  = rng.randint(5, 9)
        c  = rng.choice([C["leaf1"], C["leaf2"], C["leaf3"]])
        d.ellipse([cx+ox-r, cy+oy-r, cx+ox+r, cy+oy+r], fill=c)

for i in range(4):
    draw_tile(deco, 4+i, 0, lambda img, d, v=i: bush(img, d, v))

# ── Rock (1 tile, 4 variants) ──────────────────────────────────────────────
def rock(img, d, variant=0):
    d.rectangle([0,0,T-1,T-1], fill=(0,0,0,0))
    rng = random.Random(variant * 29 + 6)
    cx = rng.randint(10, T-10)
    cy = rng.randint(10, T-10)
    w  = rng.randint(10, 16)
    h  = rng.randint(8, 13)
    d.ellipse([cx-w, cy-h, cx+w, cy+h], fill=C["rock"])
    d.ellipse([cx-w+2, cy-h+1, cx+w-3, cy+h-3], fill=C["rock3"])
    d.arc([cx-w, cy-h, cx+w, cy+h], 200, 320, fill=C["rock2"], width=2)

for i in range(4):
    draw_tile(deco, i, 2, lambda img, d, v=i: rock(img, d, v))

# ── Rock cluster (2×1) ────────────────────────────────────────────────────
def rock_cluster_L(img, d):
    d.rectangle([0,0,T-1,T-1], fill=(0,0,0,0))
    d.ellipse([2, 8, 22, 24], fill=C["rock"])
    d.ellipse([4, 9, 20, 22], fill=C["rock3"])
    d.ellipse([8, 14, 20, 26], fill=C["rock2"])
    d.ellipse([10, 4, 28, 18], fill=C["rock"])
    d.ellipse([12, 5, 26, 16], fill=C["rock3"])

def rock_cluster_R(img, d):
    d.rectangle([0,0,T-1,T-1], fill=(0,0,0,0))
    d.ellipse([4, 10, 24, 26], fill=C["rock2"])
    d.ellipse([6, 11, 22, 24], fill=C["rock3"])
    d.ellipse([0, 4, 18, 20], fill=C["rock"])
    d.ellipse([2, 5, 16, 18], fill=C["rock3"])

draw_tile(deco, 4, 2, rock_cluster_L)
draw_tile(deco, 5, 2, rock_cluster_R)

# ── Crop tile (pumpkin/vegetable patch) ───────────────────────────────────
def crop_tile(img, d, variant=0):
    "Tilled soil with orange crop plant"
    d.rectangle([0,0,T-1,T-1], fill=C["soil"])
    for row_y in range(0, T, 4):
        d.rectangle([0,row_y, T-1, row_y+2], fill=C["soil2"] if row_y%8==0 else C["soil3"])
    rng = random.Random(variant * 17 + 2)
    cx = T//2 + rng.randint(-4, 4)
    cy = T//2 + rng.randint(-3, 3)
    # leaves
    d.ellipse([cx-7, cy-5, cx+7, cy+5], fill=C["crop_g"])
    d.ellipse([cx-5, cy-7, cx+5, cy+3], fill=C["leaf2"])
    # fruit
    d.ellipse([cx-4, cy-3, cx+4, cy+4], fill=C["orange"])
    d.ellipse([cx-2, cy-2, cx+2, cy+1], fill=C["orange2"])

for i in range(8):
    draw_tile(deco, i, 3, lambda img, d, v=i: crop_tile(img, d, v))

# ── Tall grass / flower tuft ───────────────────────────────────────────────
def grass_tuft(img, d, variant=0):
    d.rectangle([0,0,T-1,T-1], fill=(0,0,0,0))
    rng = random.Random(variant * 13)
    for _ in range(5):
        bx = rng.randint(4, T-4)
        d.polygon([(bx, T-2), (bx-3, T-10), (bx+3, T-10)], fill=C["leaf2"])
        d.polygon([(bx, T-2), (bx-2, T-8), (bx+2, T-8)],   fill=C["leaf3"])
    if variant % 2 == 0:
        d.ellipse([T//2-3, 4, T//2+3, 10], fill=C["flower_y"])

for i in range(8):
    draw_tile(deco, i, 4, lambda img, d, v=i: grass_tuft(img, d, v))

# ── Bridge rails (top/bottom edges) ───────────────────────────────────────
def bridge_rail(img, d):
    d.rectangle([0,0,T-1,T-1], fill=(0,0,0,0))
    # horizontal rail
    d.rectangle([0, T//2-3, T-1, T//2+2], fill=C["wood"])
    d.rectangle([0, T//2-3, T-1, T//2-2], fill=C["wood3"])
    d.rectangle([0, T//2+2, T-1, T//2+2], fill=C["wood2"])
    # vertical posts every 8px
    for px in range(4, T, 8):
        d.rectangle([px-1, 0, px+1, T-1], fill=C["wood"])
        d.rectangle([px-1, 0, px,   T-1], fill=C["wood3"])

draw_tile(deco, 0, 5, bridge_rail)
draw_tile(deco, 1, 5, bridge_rail)

# ── Tree shadow (ground shadow below tree) ────────────────────────────────
def tree_shadow(img, d):
    shadow = Image.new("RGBA", (T, T), (0,0,0,0))
    sd = ImageDraw.Draw(shadow)
    sd.ellipse([2, T//2, T-2, T-2], fill=(30, 60, 20, 80))
    img.paste(shadow, (0,0), shadow)

draw_tile(deco, 2, 5, tree_shadow)

deco.save("/Users/I755634/Repo/idle/Assets/Sprites/FarmBG_Deco.png")
print("Deco sheet saved: 256x320")

print("All done.")
