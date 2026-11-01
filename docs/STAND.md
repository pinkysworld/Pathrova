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
- **Tests:** `~/.dotnet/dotnet test Pathrova.slnx` → 45/45 grün (Regeln, Zählung URL-kodiert, Scan mit 20.000 Dateien, Symlinks, Abbruch, Umbenennen + Rückgängig, Konflikte, Kürzen, CSV/PDF, UI headless, Screenshots).
- **Icon:** eigenes Motiv (Pfadbaum mit Prüfplakette), `tools/make_icons.py`, alle MSIX-/Store-Größen.
- **Screenshots:** `store/screenshots/{de,en}/{light,dark}/1–4` (1366 × 820).
- **Store-Texte:** `store/STORE_TEXTE.md` (DE + EN, ohne Microsoft-Marken im Titel, mit Markenhinweis).
- **Webseite:** `website/` (Landingpage + Datenschutz, DE/EN, lokal geprüft auf Handybreite). Nicht veröffentlicht.
- **Windows-Builds:** `artifacts/publish/win-x64` und `win-arm64` (self-contained, Single-File).

## Offen
- **MSIX bauen + WACK:** Der Windows-PC (192.168.0.182) war am 01.11. nicht erreichbar (Host down). Sobald er läuft: `tools/windows_check.sh` (Selbsttest, Fensterstart, MSIX-Bundle, WACK x64).
- Nach OK: Namen im Partner Center reservieren, Identity übernehmen, neu bauen, Webseite als `pinkysworld/Pathrova` veröffentlichen, einreichen (gestaffelt, nicht vor Mitte November).
- Nicht geprüft: Bedienung am Windows-Bildschirm, ARM64 auf echter Hardware, echte Windows-Pfade > 260 Zeichen (deckt der Selbsttest ab, sobald er auf Windows läuft).
