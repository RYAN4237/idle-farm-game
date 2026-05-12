"""
Generates Bridge_AI.png: 96x192px wooden bridge, top-down oblique view.
Horizontal planks, side railings with fence posts, cast shadow.
Transparent background, cozy warm palette, top-left light source.
"""
from PIL import Image, ImageDraw
import math, random

W, H = 96, 192

# Palette
TRANSPARENT   = (0, 0, 0, 0)
SHADOW        = (40, 25, 10, 90)

# Wood planks
P_LIGHT   = (210, 165, 105, 255)   # sunlit plank face
P_MID     = (185, 138,  82, 255)   # mid plank
P_DARK    = (150, 105,  55, 255)   # shadow-side plank
P_GRAIN   = (135,  92,  44, 255)   # grain line dark
P_GRAIN_L = (225, 182, 122, 255)   # grain highlight

# Rail / post
R_LIGHT  = (230, 185, 120, 255)
R_MID    = (195, 148,  80, 255)
R_DARK   = (140,  98,  48, 255)
R_SHADOW = (110,  72,  30, 255)

# Bridge geometry
BRIDGE_X1  = 18   # left rail inner edge
BRIDGE_X2  = 77   # right rail inner edge
BRIDGE_Y1  = 8    # top of bridge deck
BRIDGE_Y2  = 183  # bottom of bridge deck

RAIL_W     = 8    # width of each side railing
POST_H     = 14   # height of each fence post
POST_W     = 8
POST_EVERY = 24   # pixels between post centers

img = Image.new("RGBA", (W, H), TRANSPARENT)
draw = ImageDraw.Draw(img)

rng = random.Random(42)

# ── 1. Cast shadow (soft, below deck, slight offset right/down) ──────────────
for dy in range(6):
    alpha = int(90 * (1 - dy / 6))
    for x in range(BRIDGE_X1 + 2, BRIDGE_X2 + 2 + dy):
        for y in [BRIDGE_Y2 + dy]:
            if 0 <= x < W and 0 <= y < H:
                img.putpixel((x, y), SHADOW[:3] + (alpha,))

# ── 2. Deck planks ────────────────────────────────────────────────────────────
PLANK_H = 8   # height of each plank
num_planks = (BRIDGE_Y2 - BRIDGE_Y1) // PLANK_H

for i in range(num_planks):
    py1 = BRIDGE_Y1 + i * PLANK_H
    py2 = py1 + PLANK_H
    # alternate slight tone variation
    tone = rng.randint(-6, 6)

    for y in range(py1, py2):
        t = (y - py1) / PLANK_H  # 0=top of plank, 1=bottom
        # top edge lighter, bottom edge darker (depth illusion)
        if t < 0.15:
            base = P_LIGHT
        elif t > 0.82:
            base = P_DARK
        else:
            base = P_MID
        r = min(255, max(0, base[0] + tone))
        g = min(255, max(0, base[1] + tone))
        b = min(255, max(0, base[2] + tone))

        for x in range(BRIDGE_X1, BRIDGE_X2 + 1):
            # slight horizontal shading: left a touch lighter (top-left light)
            hx = (x - BRIDGE_X1) / (BRIDGE_X2 - BRIDGE_X1)
            hr = int(-8 * hx)
            img.putpixel((x, y), (
                min(255, max(0, r + hr)),
                min(255, max(0, g + hr)),
                min(255, max(0, b + hr)),
                255
            ))

    # wood grain lines (2-3 per plank)
    n_grains = rng.randint(2, 3)
    for _ in range(n_grains):
        gx = rng.randint(BRIDGE_X1 + 4, BRIDGE_X2 - 4)
        gy_start = py1 + rng.randint(1, 3)
        gy_end   = py2 - rng.randint(1, 3)
        # slight horizontal wobble
        for y in range(gy_start, gy_end):
            gx2 = gx + int(math.sin(y * 0.6) * 0.7)
            if BRIDGE_X1 <= gx2 <= BRIDGE_X2:
                img.putpixel((gx2, y), P_GRAIN)
            # highlight just to the left of grain
            if BRIDGE_X1 <= gx2 - 1 <= BRIDGE_X2:
                img.putpixel((gx2 - 1, y), P_GRAIN_L)

# ── 3. Side railings ─────────────────────────────────────────────────────────
def draw_rail(x1, x2, y1, y2):
    """Draw a railing strip with top-left lighting."""
    for y in range(y1, y2):
        for x in range(x1, x2):
            tx = (x - x1) / max(1, x2 - x1 - 1)
            # left edge bright, right edge dark
            if tx < 0.18:
                c = R_LIGHT
            elif tx > 0.75:
                c = R_DARK
            else:
                c = R_MID
            # add slight grain to rail too
            noise = rng.randint(-4, 4)
            img.putpixel((x, y), (
                min(255, max(0, c[0] + noise)),
                min(255, max(0, c[1] + noise)),
                min(255, max(0, c[2] + noise)),
                255
            ))

# Left railing
draw_rail(BRIDGE_X1 - RAIL_W, BRIDGE_X1, BRIDGE_Y1, BRIDGE_Y2)
# Right railing
draw_rail(BRIDGE_X2 + 1, BRIDGE_X2 + 1 + RAIL_W, BRIDGE_Y1, BRIDGE_Y2)

# Dark inner edge line on each rail (shadow between rail and deck)
for y in range(BRIDGE_Y1, BRIDGE_Y2):
    img.putpixel((BRIDGE_X1 - 1, y), R_SHADOW)  # left rail / deck junction
    img.putpixel((BRIDGE_X2 + 1, y), R_SHADOW)  # right rail / deck junction

# ── 4. Fence posts ────────────────────────────────────────────────────────────
def draw_post(cx, y_center, side):
    """Draw a fence post centered at cx, y_center. side='left'|'right'."""
    px1 = cx - POST_W // 2
    px2 = cx + POST_W // 2
    py1 = y_center - POST_H // 2
    py2 = y_center + POST_H // 2
    for y in range(py1, py2 + 1):
        for x in range(px1, px2 + 1):
            tx = (x - px1) / max(1, px2 - px1)
            # post cap (top) slightly lighter
            ty = (y - py1) / max(1, py2 - py1)
            if ty < 0.12:
                c = R_LIGHT
            elif tx < 0.2:
                c = R_LIGHT
            elif tx > 0.78:
                c = R_DARK
            else:
                c = R_MID
            img.putpixel((x, y), c)
    # dark outline
    for y in range(py1, py2 + 1):
        img.putpixel((px1,     y), R_SHADOW)
        img.putpixel((px2,     y), R_SHADOW)
    for x in range(px1, px2 + 1):
        img.putpixel((x, py1), R_SHADOW)
        img.putpixel((x, py2), R_SHADOW)

post_ys = list(range(BRIDGE_Y1 + POST_EVERY // 2, BRIDGE_Y2, POST_EVERY))

left_cx  = BRIDGE_X1 - RAIL_W // 2
right_cx = BRIDGE_X2 + 1 + RAIL_W // 2

for py in post_ys:
    draw_post(left_cx,  py, 'left')
    draw_post(right_cx, py, 'right')

# ── 5. Top/bottom deck end caps ───────────────────────────────────────────────
# Top cap (thick beam) — slightly darker to show thickness
for y in range(BRIDGE_Y1, BRIDGE_Y1 + 5):
    for x in range(BRIDGE_X1 - RAIL_W, BRIDGE_X2 + 1 + RAIL_W):
        if 0 <= x < W:
            img.putpixel((x, y), P_DARK)

# Bottom cap
for y in range(BRIDGE_Y2 - 4, BRIDGE_Y2):
    for x in range(BRIDGE_X1 - RAIL_W, BRIDGE_X2 + 1 + RAIL_W):
        if 0 <= x < W:
            img.putpixel((x, y), P_DARK)

# Dark outline around entire bridge
for y in range(BRIDGE_Y1, BRIDGE_Y2):
    lx = BRIDGE_X1 - RAIL_W
    rx = BRIDGE_X2 + RAIL_W
    if 0 <= lx < W: img.putpixel((lx, y), (R_SHADOW[0], R_SHADOW[1], R_SHADOW[2], 200))
    if 0 <= rx < W: img.putpixel((rx, y), (R_SHADOW[0], R_SHADOW[1], R_SHADOW[2], 200))
for x in range(BRIDGE_X1 - RAIL_W, BRIDGE_X2 + RAIL_W + 1):
    if 0 <= x < W:
        img.putpixel((x, BRIDGE_Y1), (R_SHADOW[0], R_SHADOW[1], R_SHADOW[2], 200))
        img.putpixel((x, BRIDGE_Y2 - 1), (R_SHADOW[0], R_SHADOW[1], R_SHADOW[2], 200))

out = "/Users/I755634/Repo/idle/Assets/Sprites/Bridge_AI.png"
img.save(out)
print(f"Saved {out}  ({W}x{H})")
