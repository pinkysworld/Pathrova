# Stand Pathrova 1.0.0

Stand: 01.11.2026. Nichts reserviert, eingereicht oder veröffentlicht.

## Fertig
- **Recherche:** `docs/RECHERCHE.md` (Konkurrenz, Regeln von OneDrive/SharePoint, Preis, ASO, Name, Urteil: lohnt sich in maßvollem Umfang).
- **Name „Pathrova“** frei geprüft: MS Store, Google Play, App Store, Steam, TMview.
- **App** (Avalonia 12 / .NET 10, gleicher Stack wie Folvane, eigener Code):
  - Scan in einem Durchgang mit Stapel statt Rekursion, abbrechbar, auch UNC
  - 16 Problemarten, nach Art und Stufe gruppiert, mit Suche
  - Zu lange Ordnerpfade erscheinen einmal mit „+N Elemente darin“
  - Umbenennen mit Vorschau, Auswahl, Bestätigung, Konfliktlösung und Rückgängig-Protokoll (JSON im App-Datenordner)
  - Export als CSV und PDF
  - Deutsch und Englisch, hell und dunkel, offline, `--selftest`
- **Tests:** `~/.dotnet/dotnet test Pathrova.slnx` → 47/47 grün (Regeln, Zählung URL-kodiert, Scan mit 20.000 Dateien, Symlinks, Abbruch, Umbenennen + Rückgängig, Konflikte, Kürzen, CSV/PDF, UI headless, Screenshots).
- **Icon:** eigenes Motiv (Pfadbaum mit Prüfplakette), `tools/make_icons.py`, alle MSIX-/Store-Größen.
- **Screenshots:** `store/screenshots/{de,en}/{light,dark}/1–4` (1366 × 820).
- **Store-Texte:** `store/STORE_TEXTE.md` (DE + EN, ohne Microsoft-Marken im Titel, mit Markenhinweis).
- **Webseite:** `website/` (Landingpage + Datenschutz, DE/EN, lokal geprüft auf Handybreite). Nicht veröffentlicht.
- **Windows-Builds:** `artifacts/publish/win-x64` und `win-arm64` (self-contained, Single-File).

## Windows (01.11.2026, nach Art Review Runde 5)
- `tools/windows_check.sh`: Selbsttest x64 PASS (echte Pfade > 260 Zeichen, Umbenennen, Rückgängig, PDF), Fenster startet, MSIX-Bundle gebaut, **WACK x64 PASS**.
- Bundle: `artifacts/msix/Pathrova_1.0.0.0.msixbundle` (x64 + ARM64, Identity vorläufig `MinhStudios.Pathrova`).

## Chief Art Review (unabhängiger Prüfer, 5 Runden)
5 → 7 → 7,5 → 8 → **8,5/10** (Icon 9, UI 8,5, Store-Bilder 8,5, Webseite 8,5, Kurzbeschreibung 9). Laut Prüfer hängt die 10 an Dingen nach dem Start (echtes Store-Badge, Bewertungen).
Umgesetzt: neues Icon „Pfad trifft Grenze“ mit eigener 16–32-px-Fassung, KPI-Leiste, Leerzustand, Stufensymbole, zeichengenauer Diff in der Vorschau, behutsames Kürzen (Versionsnummern bleiben, keine Füllwörter am Ende), PDF mit Kopfband und gleicher Zählweise wie die App, 5 Marketing-Bilder je Sprache (`store/screenshots/{de,en}/marketing/`, `tools/compose_store_screenshots.py`), Webseite mit Nutzenpunkten, Chips und Store-Platzhalter (Handy 375 px geprüft).
Offen aus dem Review (klein): Diff-Chip „·–·“ bei 13 px schwer lesbar; keine Hover-/Fokuszustände in den Bildern.

## Offen
- Nach OK: Namen im Partner Center reservieren, Identity übernehmen, neu bauen, Webseite als `pinkysworld/Pathrova` veröffentlichen, einreichen (gestaffelt, nicht vor Mitte November).
- Nicht geprüft: Bedienung am Windows-Bildschirm, ARM64 auf echter Hardware, echte Windows-Pfade > 260 Zeichen (deckt der Selbsttest ab, sobald er auf Windows läuft).

## Partner Center (01.11.2026)
Store-ID 9NJN21Z01WPM, Identity `MinhStudios.Pathrova` (passt zum Bundle). Entwurf Submission 1 vollständig bis auf Paket: 12 Titel reserviert, 12 Einträge (store/listings), Preise B 11,99 € / C 8,48 € / D 394 ₹, IARC, Zertifizierungshinweis. Offen: Bundle hochladen und einreichen – bewusst gestaffelt, wartet auf OK des Nutzers.
