"""
Regenerate only the grass row (row0) of FarmBG_Ground.png with clean tiles (no sparkle dots).
"""
from PIL import Image, ImageDraw

T = 32
C_g2 = (68, 118, 40, 255)   # base green
C_g1 = (82, 140, 50, 255)   # lighter green
C_g3 = (55, 98, 30, 255)    # darker green

ground = Image.open("/Users/I755634/Repo/idle/Assets/Sprites/FarmBG_Ground.png").convert("RGBA")

def clean_grass(variant):
    img = Image.new("RGBA", (T, T), C_g2)
    d = ImageDraw.Draw(img)
    # Subtle shade pattern only — no bright dots
    import random
    rng = random.Random(variant * 31 + 7)
    # Just a few slightly lighter/darker green pixels, no white/yellow/purple
    for _ in range(12):
        x, y = rng.randint(0, T-1), rng.randint(0, T-1)
        c = rng.choice([C_g1, C_g3])
        d.point((x, y), fill=c)
    return img

# Replace row 0, all 8 cols with clean grass
for col in range(8):
    tile = clean_grass(col)
    ground.paste(tile, (col * T, 0))

ground.save("/Users/I755634/Repo/idle/Assets/Sprites/FarmBG_Ground.png")
print("Grass row cleaned - no sparkles")
