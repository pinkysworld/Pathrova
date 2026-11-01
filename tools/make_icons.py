#!/usr/bin/env python3
"""Zeichnet das Pathrova-Icon (eigene Vektorformen, 4x überabgetastet) und erzeugt alle App-, MSIX- und Store-Größen."""
from pathlib import Path
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parent.parent
S = 4096  # Arbeitsgröße

def rounded(d, box, r, fill):
    d.rounded_rectangle(box, radius=r, fill=fill)

def draw(plate=True):
    """Eigenes Motiv: Pfad aus Knoten (Ordnerbaum), der in ein Prüfhäkchen mündet."""
    img = Image.new("RGBA", (S, S), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    m = S * 0.06
    rounded(d, (m, m, S - m, S - m), S * 0.2, (14, 111, 115, 255))
    w = int(S * 0.055)
    light = (205, 238, 236, 255)
    white = (255, 255, 255, 255)
    # Baum: senkrechte Linie mit drei Abzweigen
    x0 = S * 0.27
    nodes = [(x0, S * 0.26), (x0, S * 0.46), (x0, S * 0.66)]
    d.line([(x0, S * 0.26), (x0, S * 0.66)], fill=light, width=w)
    for (x, y) in nodes[1:]:
        d.line([(x, y), (S * 0.47, y)], fill=light, width=w)
    for (x, y) in nodes:
        r = S * 0.058
        d.ellipse((x - r, y - r, x + r, y + r), fill=white)
    for y in (S * 0.46, S * 0.66):
        r = S * 0.045
        rounded(d, (S * 0.47, y - r, S * 0.62, y + r), r * 0.6, light)
    # Prüfplakette unten rechts
    cx, cy, R = S * 0.66, S * 0.66, S * 0.17
    d.ellipse((cx - R, cy - R, cx + R, cy + R), fill=(255, 196, 92, 255))
    t = int(S * 0.05)
    d.line([(cx - R * 0.5, cy + R * 0.02), (cx - R * 0.12, cy + R * 0.4), (cx + R * 0.52, cy - R * 0.38)], fill=(10, 60, 62, 255), width=t, joint="curve")
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
    icon.resize((256, 256), Image.LANCZOS).save(app / "app.ico", sizes=[(16, 16), (24, 24), (32, 32), (48, 48), (64, 64), (128, 128), (256, 256)])
    sizes = {"StoreLogo": (50, 50, 1.0), "Square44x44Logo": (44, 44, 1.0), "Square150x150Logo": (150, 150, 0.66),
             "SmallTile": (71, 71, 0.8), "LargeTile": (310, 310, 0.6), "Wide310x150Logo": (310, 150, 0.8), "SplashScreen": (620, 300, 0.7)}
    for name, (w, h, sc) in sizes.items():
        for pct in (100, 125, 150, 200, 400):
            fit(icon, round(w * pct / 100), round(h * pct / 100), sc).save(out / f"{name}.scale-{pct}.png")
        fit(icon, w, h, sc).save(out / f"{name}.png")
    for ts in (16, 24, 32, 48, 256):
        img = icon.resize((ts, ts), Image.LANCZOS)
        img.save(out / f"Square44x44Logo.targetsize-{ts}.png")
        img.save(out / f"Square44x44Logo.targetsize-{ts}_altform-unplated.png")
        img.save(out / f"Square44x44Logo.targetsize-{ts}_altform-lightunplated.png")
    store = ROOT / "store" / "grafiken"; store.mkdir(parents=True, exist_ok=True)
    icon.resize((300, 300), Image.LANCZOS).save(store / "StoreLogo-300.png")
    fit(icon, 1080, 1080, 0.8, (246, 245, 242, 255)).save(store / "Poster-1080x1080.png")
    print("ok")

if __name__ == "__main__":
    main()
