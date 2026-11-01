#!/usr/bin/env python3
"""Zeichnet das Pathrova-Icon (eigene Vektorformen, 4x überabgetastet) und erzeugt alle App-, MSIX- und Store-Größen."""
from pathlib import Path
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parent.parent
S = 4096  # Arbeitsgröße

def rounded(d, box, r, fill):
    d.rounded_rectangle(box, radius=r, fill=fill)

def draw(small=False):
    """Eigenes Motiv: Ein Pfad aus Ordnerknoten stößt an eine Grenze (gelb); der letzte Knoten ragt darüber und ist rot.
    Die Plakette unten rechts zeigt: geprüft. small=True: 16–32 px nur Pfad + Grenze, ohne Plakette."""
    img = Image.new("RGBA", (S, S), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    m = S * 0.06
    rounded(d, (m, m, S - m, S - m), S * 0.21, (14, 111, 115, 255))
    white = (255, 255, 255, 255)
    soft = (178, 226, 223, 255)
    gold = (255, 196, 92, 255)
    red = (214, 69, 69, 255)
    dark = (8, 52, 54, 255)
    limit_x = S * (0.70 if small else 0.645)
    y = S * 0.5
    w = int(S * (0.10 if small else 0.075))
    r = S * (0.105 if small else 0.072)
    xs = [S * 0.22, S * 0.45] if small else [S * 0.19, S * 0.35, S * 0.51]
    over = S * (0.82 if small else 0.77)
    d.line([(xs[0], y), (over, y)], fill=white if small else soft, width=w)
    for x in xs:
        d.ellipse((x - r, y - r, x + r, y + r), fill=white)
    d.ellipse((over - r, y - r, over + r, y + r), fill=red)
    bar = S * (0.06 if small else 0.055)
    d.rounded_rectangle((limit_x - bar / 2, y - S * (0.3 if small else 0.24), limit_x + bar / 2, y + S * (0.3 if small else 0.24)), radius=bar / 2, fill=gold)
    return img
    cx, cy, R = S * 0.74, S * 0.76, S * 0.13
    ring = R * 0.14
    d.ellipse((cx - R - ring, cy - R - ring, cx + R + ring, cy + R + ring), fill=(14, 111, 115, 255))
    d.ellipse((cx - R, cy - R, cx + R, cy + R), fill=gold)
    t = int(R * 0.3)
    pts = [(cx - R * 0.46, cy + R * 0.02), (cx - R * 0.1, cy + R * 0.38), (cx + R * 0.48, cy - R * 0.34)]
    d.line(pts, fill=dark, width=t, joint="curve")
    for (x, yy) in (pts[0], pts[2]):
        d.ellipse((x - t / 2, yy - t / 2, x + t / 2, yy + t / 2), fill=dark)
    return img

def fit(src, w, h, scale=1.0, bg=(0, 0, 0, 0)):
    canvas = Image.new("RGBA", (w, h), bg)
    side = int(min(w, h) * scale)
    icon = src.resize((side, side), Image.LANCZOS)
    canvas.alpha_composite(icon, ((w - side) // 2, (h - side) // 2))
    return canvas

def main():
    icon = draw()
    out = ROOT / "packaging" / "Assets"; out.mkdir(parents=True, exist_ok=True)
    (ROOT / "assets").mkdir(exist_ok=True)
    icon.resize((1024, 1024), Image.LANCZOS).save(ROOT / "assets" / "pathrova-icon-1024.png")
    app = ROOT / "src" / "Pathrova.App" / "Assets"; app.mkdir(parents=True, exist_ok=True)
    icon.resize((256, 256), Image.LANCZOS).save(app / "logo.png")
    icon.resize((256, 256), Image.LANCZOS).save(app / "app.ico", sizes=[(48, 48), (64, 64), (128, 128), (256, 256)])
    # ICO mit eigenen kleinen Bildern: Pillow nimmt bei append_images die passende Größe.
    smalls = [draw(small=True).resize((n, n), Image.LANCZOS) for n in (16, 24, 32)]
    big = [icon.resize((n, n), Image.LANCZOS) for n in (48, 64, 128, 256)]
    big[-1].save(app / "app.ico", sizes=[(n, n) for n in (16, 24, 32, 48, 64, 128, 256)], append_images=smalls + big[:-1])
    sizes = {"StoreLogo": (50, 50, 1.0), "Square44x44Logo": (44, 44, 1.0), "Square150x150Logo": (150, 150, 0.66),
             "SmallTile": (71, 71, 0.8), "LargeTile": (310, 310, 0.6), "Wide310x150Logo": (310, 150, 0.8), "SplashScreen": (620, 300, 0.7)}
    for name, (w, h, sc) in sizes.items():
        for pct in (100, 125, 150, 200, 400):
            fit(icon, round(w * pct / 100), round(h * pct / 100), sc).save(out / f"{name}.scale-{pct}.png")
        fit(icon, w, h, sc).save(out / f"{name}.png")
    small = draw(small=True)
    for ts in (16, 24, 32, 48, 256):
        img = (small if ts <= 32 else icon).resize((ts, ts), Image.LANCZOS)
        img.save(out / f"Square44x44Logo.targetsize-{ts}.png")
        img.save(out / f"Square44x44Logo.targetsize-{ts}_altform-unplated.png")
        img.save(out / f"Square44x44Logo.targetsize-{ts}_altform-lightunplated.png")
    store = ROOT / "store" / "grafiken"; store.mkdir(parents=True, exist_ok=True)
    icon.resize((300, 300), Image.LANCZOS).save(store / "StoreLogo-300.png")
    fit(icon, 1080, 1080, 0.8, (246, 245, 242, 255)).save(store / "Poster-1080x1080.png")
    print("ok")

if __name__ == "__main__":
    main()
