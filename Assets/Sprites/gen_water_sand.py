"""
Generate WaterSand_AI.png: 192x96, 4 water tiles (row0) + 4 sand/dirt tiles (row1).
Each tile 48x48px, PPU=48. Cozy pixel art, top-down style matching Stardew Valley aesthetic.
"""
from PIL import Image, ImageDraw
import random, math

T = 48
COLS = 4
ROWS = 2
W, H = T * COLS, T * ROWS
out = Image.new("RGBA", (W, H), (0, 0, 0, 0))

# ── Palette (carefully chosen for cozy farm game) ─────────────────────────────
# Water — clear, slightly teal-blue
W_DEEP   = (52,  128, 186, 255)
W_MID    = (72,  152, 204, 255)
W_LIGHT  = (110, 182, 220, 255)
W_PALE   = (160, 210, 232, 255)
W_FOAM   = (210, 235, 245, 255)
W_SPEC   = (240, 250, 255, 255)
W_DARK   = (38,  100, 158, 255)
W_SHAD   = (42,  110, 170, 255)

# Sand — warm golden-beige
S_BASE   = (212, 188, 140, 255)
S_LIGHT  = (232, 210, 165, 255)
S_LIGHTER= (242, 224, 182, 255)
S_SHADOW = (185, 160, 115, 255)
S_DARK   = (158, 134,  92, 255)
S_DARKER = (138, 114,  76, 255)
# Pebbles
P_LIGHT  = (198, 178, 148, 255)
P_MED    = (168, 148, 112, 255)
P_DARK   = (142, 122,  88, 255)
P_DARK2  = (120, 100,  70, 255)
# Tiny plants / moss
MOSS     = (148, 168,  90, 255)
MOSS_D   = (120, 138,  68, 255)


def lerp_color(a, b, t):
    return tuple(int(a[i] + (b[i] - a[i]) * t) for i in range(4))


def water_tile(img, ox, oy, seed):
    rng = random.Random(seed * 31337)

    # Fill base gradient — deep at bottom-right, lighter at top-left
    for y in range(T):
        for x in range(T):
            f = (x + y) / (T * 2.0)  # 0=top-left, 1=bottom-right
            if f < 0.25:
                c = lerp_color(W_LIGHT, W_MID, f / 0.25)
            elif f < 0.6:
                c = lerp_color(W_MID, W_DEEP, (f - 0.25) / 0.35)
            else:
                c = lerp_color(W_DEEP, W_DARK, (f - 0.6) / 0.4)
            img.putpixel((ox + x, oy + y), c)

    # Subtle horizontal ripple bands — darker troughs, lighter crests
    y_shift = [0, 5, 10, 7][seed % 4]
    for band in range(6):
        base_y = 4 + band * 8 + y_shift
        for x in range(T):
            # Wavy crest line
            cy = base_y + int(1.5 * math.sin(x * 0.28 + seed * 0.9 + band * 0.5))
            cy = cy % T
            # Dark trough just below
            ty = (cy + 2) % T
            px_c = img.getpixel((ox + x, oy + cy))
            px_t = img.getpixel((ox + x, oy + ty))
            # Brighten crest
            img.putpixel((ox + x, oy + cy), lerp_color(px_c, W_PALE, 0.45))
            # Darken trough slightly
            img.putpixel((ox + x, oy + ty), lerp_color(px_t, W_SHAD, 0.3))

    # Foam/highlight flecks on crest of each wave
    for band in range(6):
        base_y = 4 + band * 8 + y_shift
        for x in range(0, T, 3):
            cy = base_y + int(1.5 * math.sin(x * 0.28 + seed * 0.9 + band * 0.5))
            cy = cy % T
            if rng.random() < 0.55:
                img.putpixel((ox + x, oy + cy), W_FOAM)
            if rng.random() < 0.18:
                img.putpixel((ox + x, oy + cy), W_SPEC)

    # Sparkle highlights (2×1 or 1×1 bright dots)
    n_sparks = [10, 8, 12, 9][seed % 4]
    for _ in range(n_sparks):
        sx = rng.randint(1, T - 3)
        sy = rng.randint(1, T - 3)
        img.putpixel((ox + sx,     oy + sy),     W_SPEC)
        img.putpixel((ox + sx + 1, oy + sy),     W_FOAM)
        img.putpixel((ox + sx,     oy + sy + 1), W_PALE)


def sand_tile(img, ox, oy, seed):
    rng = random.Random(seed * 99991 + 777)

    # Base fill with directional light (top-left bright)
    for y in range(T):
        for x in range(T):
            # Light factor: top-left = 1.0, bottom-right = 0.0
            lf = 1.0 - (x + y) / (T * 2.0)
            lf = max(0.0, min(1.0, lf))
            # 3-stop gradient
            if lf > 0.65:
                c = lerp_color(S_LIGHTER, S_LIGHT, (lf - 0.65) / 0.35)
            elif lf > 0.35:
                c = lerp_color(S_BASE, S_LIGHT, (lf - 0.35) / 0.3)
            else:
                c = lerp_color(S_SHADOW, S_BASE, lf / 0.35)
            img.putpixel((ox + x, oy + y), c)

    # Fine grain noise — occasional 1px dark/light specks
    for _ in range(120):
        gx = rng.randint(0, T - 1)
        gy = rng.randint(0, T - 1)
        choice = rng.randint(0, 2)
        if choice == 0:
            img.putpixel((ox + gx, oy + gy), S_DARK)
        elif choice == 1:
            img.putpixel((ox + gx, oy + gy), S_LIGHTER)

    # Pebbles — small 2-3px rounded rocks
    n_pebbles = [7, 9, 8, 10][seed % 4]
    prng = random.Random(seed + 5555)
    for _ in range(n_pebbles):
        px = prng.randint(4, T - 6)
        py = prng.randint(4, T - 6)
        rad = prng.choice([1, 1, 2, 2])
        base_col = prng.choice([P_MED, P_DARK, P_MED])

        # Draw ellipse body
        for dy in range(-rad, rad + 1):
            for dx in range(-rad - 1, rad + 2):
                dist = math.sqrt((dx / (rad + 0.5))**2 + (dy / rad)**2)
                if dist <= 1.0:
                    nx, ny = ox + px + dx, oy + py + dy
                    if 0 <= nx - ox < T and 0 <= ny - oy < T:
                        img.putpixel((nx, ny), base_col)

        # Highlight on top-left face
        for dy in range(-rad, 0):
            for dx in range(-rad - 1, 0):
                dist = math.sqrt((dx / (rad + 0.5))**2 + (dy / rad)**2)
                if dist <= 0.7:
                    nx, ny = ox + px + dx, oy + py + dy
                    if 0 <= nx - ox < T and 0 <= ny - oy < T:
                        img.putpixel((nx, ny), P_LIGHT)

        # Shadow on bottom-right
        for dy in range(0, rad + 1):
            for dx in range(0, rad + 2):
                dist = math.sqrt((dx / (rad + 0.5))**2 + (dy / rad)**2)
                if dist <= 0.7:
                    nx, ny = ox + px + dx, oy + py + dy
                    if 0 <= nx - ox < T and 0 <= ny - oy < T:
                        img.putpixel((nx, ny), P_DARK2)

    # Tiny moss patches (1-2px) in some variants
    n_moss = [0, 2, 1, 3][seed % 4]
    for _ in range(n_moss):
        mx = prng.randint(2, T - 3)
        my = prng.randint(2, T - 3)
        img.putpixel((ox + mx, oy + my), MOSS)
        if prng.random() < 0.5:
            img.putpixel((ox + mx + 1, oy + my), MOSS_D)

    # Thin darker streak lines (wind-blown sand texture)
    for i in range(3):
        lx = prng.randint(0, T - 12)
        ly = prng.randint(2, T - 3)
        length = prng.randint(4, 10)
        for k in range(length):
            nx = lx + k
            ny = ly + (1 if k > length // 2 else 0)
            if 0 <= nx < T and 0 <= ny < T:
                px = img.getpixel((ox + nx, oy + ny))
                img.putpixel((ox + nx, oy + ny), lerp_color(px, S_DARKER, 0.35))


# Generate
for col in range(COLS):
    water_tile(out, col * T, 0,     col)
for col in range(COLS):
    sand_tile(out,  col * T, T, col)

out_path = "/Users/I755634/Repo/idle/Assets/Sprites/WaterSand_AI.png"
out.save(out_path)
print(f"Saved {out_path}  ({W}x{H}px)")
