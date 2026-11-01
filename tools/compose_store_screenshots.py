#!/usr/bin/env python3
"""Store-Bilder 1920 × 1080: Überschrift + App-Aufnahme auf Markenfläche. Eingabe: store/screenshots/<lang>/<theme>/*.png (aus den UI-Tests)."""
import io
import fitz  # PyMuPDF: Bericht scharf rendern
from pathlib import Path
from PIL import Image, ImageDraw, ImageFilter, ImageFont

ROOT = Path(__file__).resolve().parent.parent
W, H = 1920, 1080
TEAL, TEAL_DARK, GOLD = (14, 111, 115), (8, 70, 73), (255, 196, 92)
EN_NUMBER = 440  # erste Zeile der englischen Demodaten (Aufnahme 1-overview)

SYS = Path("/System/Library/Fonts/Supplemental")
FONT_BOLD, FONT_REG = (SYS / "Arial Bold.ttf").read_bytes(), (SYS / "Arial.ttf").read_bytes()
def font(data, size): return ImageFont.truetype(io.BytesIO(data), size)

# (Quelle, Thema, Überschrift, Unterzeile, Art)  Art: None = ganzes Fenster, "zoom" = Nahaufnahme der ersten Zeile, "pdf" = Bericht
TEXT = {
    "de": [
        ("1-overview", "light", "Alle Pfadprobleme auf einen Blick", "Vor dem Sync nach OneDrive oder SharePoint – in einem Durchgang statt Fehler für Fehler.", None),
        ("4-rename-preview", "light", "Sicher umbenennen – mit Rückgängig", "Vorschau mit Auswahl. Ohne deine Bestätigung ändert sich nichts.", None),
        ("1-overview", "light", "461 statt 400 Zeichen? Sofort sichtbar.", "Streng gezählt wie in der Cloud – und gleich ein Vorschlag, der passt.", "zoom"),
        ("report", "light", "Bericht für Kunden als PDF und CSV", "Zusammenfassung je Problemart, jede Fundstelle mit Pfad. Offline, ohne Konto.", "pdf"),
        ("2-warnings", "dark", "Fehler, Warnungen, Hinweise – klar getrennt", "Hell oder dunkel. Filtern, suchen, im Explorer zeigen.", None),
    ],
    "en": [
        ("1-overview", "light", "Every path issue at a glance", "Before you sync to OneDrive or SharePoint – one pass instead of error by error.", None),
        ("4-rename-preview", "light", "Rename safely – with undo", "Preview with selection. Nothing changes until you confirm.", None),
        ("1-overview", "light", "{n} instead of 400 characters? Spotted.", "Counted strictly like the cloud – with a suggestion that fits.", "zoom"),
        ("report", "light", "Client-ready PDF and CSV reports", "Summary per issue type, every finding with its path. Offline, no account.", "pdf"),
        ("2-warnings", "dark", "Errors, warnings, notes – clearly apart", "Light or dark. Filter, search, reveal in Explorer.", None),
    ],
}
ZOOM = (365, 308, 1000, 474)   # erste zwei Zeilen der Liste in 1-overview (1366 × 820)
NUMBER = {"de": (556 - 365, 330 - 308, 606 - 365, 349 - 308), "en": (559 - 365, 330 - 308, 609 - 365, 349 - 308)}  # Zahl innerhalb von ZOOM

CARD = {
    "de": ("Grundriss Erdgeschoss – Revision 7 – freigegeben von Bauherr und Behörde.pdf", "→ Grundriss Erdgeschoss-Revision 7-freigegeben.pdf", "passt"),
    "en": ("Ground floor plan – revision 7 – approved by client and authority.pdf", "→ Ground floor plan-revision 7-approved by client.pdf", "fits"),
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

def compose(lang, item, number=None):
    name, theme, head, sub, kind = item
    head = head.replace("{n}", str(number or ""))
    c = background()
    d = ImageDraw.Draw(c)
    fh, fs = font(FONT_BOLD, 66), font(FONT_REG, 30)
    d.text((W // 2, 92), head, font=fh, fill="white", anchor="mm")
    d.text((W // 2, 162), sub, font=fs, fill=(205, 238, 236), anchor="mm")
    base = ROOT / "store" / "screenshots" / lang
    if kind == "pdf":
        doc = fitz.open(base / "report.pdf")
        pix = doc[0].get_pixmap(dpi=220, alpha=False)
        page = Image.open(io.BytesIO(pix.tobytes("png"))).convert("RGBA")
        page = page.crop((0, 0, page.width, int(page.width * 0.62)))  # obere Hälfte: Kopf + Zusammenfassung
        place(c, page, 1500, 820, 220)
    elif kind == "zoom":
        src = Image.open(base / theme / f"{name}@2x.png").convert("RGBA")
        part = src.crop(tuple(v * 2 for v in ZOOM))
        # rechten Rand weich ausblenden (abgeschnittener Text)
        bg = src.getpixel((ZOOM[2] * 2 - 4, ZOOM[1] * 2 + 4))
        fade = Image.new("RGBA", part.size, (0, 0, 0, 0)); fd = ImageDraw.Draw(fade)
        for i in range(180):
            fd.line([(part.width - 180 + i, 0), (part.width - 180 + i, part.height)], fill=bg[:3] + (int(255 * i / 179),))
        part.alpha_composite(fade)
        s = 1640 / (part.width / 2)
        x, shot = place(c, part, 1640, 600, 250)
        d = ImageDraw.Draw(c)
        nx0, ny0, nx1, ny1 = (int(v * s) for v in NUMBER[lang])
        d.rounded_rectangle((x + nx0 - 14, 250 + ny0 - 4, x + nx1 + 14, 250 + ny1 + 8), 16, outline=GOLD, width=6)
        # Erklärkarte darunter: vorher → nachher
        y = H - 80 - 230
        fb, fr, fc = font(FONT_BOLD, 40), font(FONT_REG, 34), font(FONT_BOLD, 30)
        before, after, ok = CARD[lang]
        d.rounded_rectangle((140, y, W - 140, y + 230), 22, fill=(255, 255, 255, 245))
        d.text((190, y + 62), before, font=fr, fill=(180, 35, 24), anchor="lm")
        d.text((190, y + 150), after, font=fb, fill=(14, 111, 115), anchor="lm")
        # grüner Chip mit gezeichnetem Haken
        tw = d.textlength(after, font=fb)
        cx0 = 190 + tw + 30; cw = d.textlength(ok, font=fc) + 90
        d.rounded_rectangle((cx0, y + 124, cx0 + cw, y + 176), 26, fill=(224, 241, 228))
        d.line([(cx0 + 24, y + 151), (cx0 + 36, y + 163), (cx0 + 56, y + 139)], fill=(46, 125, 70), width=7, joint="curve")
        d.text((cx0 + 70, y + 150), ok, font=fc, fill=(46, 125, 70), anchor="lm")
    else:
        hi = base / theme / f"{name}@2x.png"
        src = Image.open(hi if hi.exists() else base / theme / f"{name}.png").convert("RGBA")
        place(c, src, 1640, 820, 220)
    return c.convert("RGB")

def zoom_number(lang):
    """Liest die Länge der ersten Zeile aus der CSV-freien Quelle: hier fest aus den Demodaten (DE 461, EN siehe Aufnahme)."""
    return {"de": 461, "en": EN_NUMBER}[lang]

def main():
    for lang, items in TEXT.items():
        out = ROOT / "store" / "screenshots" / lang / "marketing"; out.mkdir(parents=True, exist_ok=True)
        for f in out.glob("*.png"): f.unlink()
        for n, item in enumerate(items, 1):
            compose(lang, item, zoom_number(lang)).save(out / f"{n}-{item[0]}.png", optimize=True)
    print("ok")

if __name__ == "__main__":
    main()
