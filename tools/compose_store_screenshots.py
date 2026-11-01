#!/usr/bin/env python3
"""Store-Bilder 1920 × 1080: Überschrift + App-Aufnahme auf Markenfläche. Eingabe: store/screenshots/<lang>/<theme>/*.png (aus den UI-Tests)."""
import io, zipfile
from pathlib import Path
from PIL import Image, ImageDraw, ImageFilter, ImageFont

ROOT = Path(__file__).resolve().parent.parent
W, H = 1920, 1080
TEAL, TEAL_DARK, GOLD = (14, 111, 115), (8, 70, 73), (255, 196, 92)

SYS = Path("/System/Library/Fonts/Supplemental")
FONT_BOLD, FONT_REG = (SYS / "Arial Bold.ttf").read_bytes(), (SYS / "Arial.ttf").read_bytes()
def font(data, size): return ImageFont.truetype(io.BytesIO(data), size)

TEXT = {
    "de": [
        ("1-overview", "light", "Alle Pfadprobleme auf einen Blick", "Vor dem Sync nach OneDrive oder SharePoint – in einem Durchgang statt Fehler für Fehler.", None),
        ("4-rename-preview", "light", "Sicher umbenennen – mit Rückgängig", "Vorschau mit Auswahl. Ohne deine Bestätigung ändert sich nichts.", None),
        ("1-overview", "light", "461 statt 400 Zeichen? Sofort sichtbar.", "Streng gezählt wie in der Cloud – mit Zielbibliothek und URL-Kodierung.", (8, 166, 1000, 640)),
        ("3-rules", "light", "Regeln passend zu deinem Ziel", "Bibliothekspfad, Sync-Ordner, gesperrte Dateitypen, # und %.", None),
        ("2-errors", "dark", "Bericht für Kunden als CSV und PDF", "Für IT-Dienstleister und kleine Firmen. Offline, ohne Konto.", None),
    ],
    "en": [
        ("1-overview", "light", "Every path issue at a glance", "Before you sync to OneDrive or SharePoint – one pass instead of error by error.", None),
        ("4-rename-preview", "light", "Rename safely – with undo", "Preview with selection. Nothing changes until you confirm.", None),
        ("1-overview", "light", "461 instead of 400 characters? Spotted.", "Counted strictly like the cloud – with target library and URL encoding.", (8, 166, 1000, 640)),
        ("3-rules", "light", "Rules that match your target", "Library path, sync folder, blocked file types, # and %.", None),
        ("2-errors", "dark", "Client-ready CSV and PDF reports", "For IT service providers and small businesses. Offline, no account.", None),
    ],
}

def background():
    bg = Image.new("RGB", (W, H), TEAL)
    d = ImageDraw.Draw(bg)
    for y in range(H):  # ruhiger Verlauf nach unten
        t = y / H
        d.line([(0, y), (W, y)], fill=tuple(int(TEAL[i] * (1 - t) + TEAL_DARK[i] * t) for i in range(3)))
    return bg.convert("RGBA")

def rounded(img, r):
    mask = Image.new("L", img.size, 0)
    ImageDraw.Draw(mask).rounded_rectangle((0, 0, img.width - 1, img.height - 1), r, fill=255)
    out = img.convert("RGBA"); out.putalpha(mask)
    return out

def place(canvas, shot, box_w, box_h, top):
    s = min(box_w / shot.width, box_h / shot.height)
    shot = shot.resize((int(shot.width * s), int(shot.height * s)), Image.LANCZOS)
    shot = rounded(shot, 14)
    x = (W - shot.width) // 2
    shadow = Image.new("RGBA", (shot.width + 80, shot.height + 80), (0, 0, 0, 0))
    ImageDraw.Draw(shadow).rounded_rectangle((40, 50, shot.width + 40, shot.height + 50), 18, fill=(0, 20, 22, 120))
    canvas.alpha_composite(shadow.filter(ImageFilter.GaussianBlur(22)), (x - 40, top - 40))
    canvas.alpha_composite(shot, (x, top))
    return x, shot

def compose(lang, item):
    name, theme, head, sub, crop = item
    src = Image.open(ROOT / "store" / "screenshots" / lang / theme / f"{name}.png").convert("RGBA")
    c = background()
    d = ImageDraw.Draw(c)
    fh, fs = font(FONT_BOLD, 66), font(FONT_REG, 30)
    d.text((W // 2, 92), head, font=fh, fill="white", anchor="mm")
    d.text((W // 2, 162), sub, font=fs, fill=(205, 238, 236), anchor="mm")
    if crop:
        part = src.crop(crop)
        x, shot = place(c, part, 1640, 820, 230)
        # Zahl markieren
        d = ImageDraw.Draw(c)
        d.rounded_rectangle((x - 6, 224, x + shot.width + 6, 236 + shot.height), 18, outline=GOLD, width=5)
    else:
        place(c, src, 1640, 850, 220)
    return c.convert("RGB")

def main():
    for lang, items in TEXT.items():
        out = ROOT / "store" / "screenshots" / lang / "marketing"; out.mkdir(parents=True, exist_ok=True)
        for f in out.glob("*.png"): f.unlink()
        for n, item in enumerate(items, 1):
            compose(lang, item).save(out / f"{n}-{item[0]}.png", optimize=True)
    print("ok")

if __name__ == "__main__":
    main()
