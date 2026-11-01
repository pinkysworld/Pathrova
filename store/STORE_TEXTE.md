# Pathrova – Texte für den Store-Eintrag

Stand: 01.11.2026. Nichts eingereicht. Grenzen im Partner Center: Titel 256 Zeichen, Kurzbeschreibung 1.000, Beschreibung 10.000, Funktionen je 200 (höchstens 20), Suchbegriffe 7 × 30.

Paket: `artifacts/msix/Pathrova_1.0.0.0.msixbundle` (x64 + ARM64). Identity vorläufig `MinhStudios.Pathrova`, Publisher `CN=3B31B7FD-F878-49F0-9D9A-0F247C2D4671` („Minh Studios“, wie Folvane). Nach der Namensreservierung Identity aus dem Partner Center übernehmen und neu bauen.

**Markenhinweis:** Titel und Icon ohne „OneDrive“ und „SharePoint“. Im Text kommen die Begriffe nur beschreibend vor („für OneDrive und SharePoint“). Unten steht ein Hinweis, dass es sich um Marken von Microsoft handelt.

Kategorie: **Dienstprogramme & Tools** (Dateimanager). Altersfreigabe IARC: keine Inhalte. Datenschutz: `https://minh.systems/Pathrova/privacy.html`, Website `https://minh.systems/Pathrova/`, Support `https://minh.systems/impressum/`.
Systemanforderungen: Windows 10 1809 oder neuer, x64 oder ARM64.
Preis: 14,99 € mit 7-Tage-Testversion (Store) und Kaufkraftstaffel (siehe `docs/RECHERCHE.md`).

Hinweis an die Zertifizierung: „Pathrova ist eine Offline-App ohne Konto. Zum Test einen beliebigen Ordner wählen (z. B. Dokumente) und ‚Prüfen‘ klicken. Die App ändert nichts, solange der Nutzer Umbenennungen nicht in der Vorschau bestätigt; jede Umbenennung lässt sich rückgängig machen. runFullTrust wird für den Dateisystemzugriff einer Win32-(.NET-)Desktop-App benötigt.“

**Store-Bilder (hochladen, in dieser Reihenfolge):** `store/screenshots/{de,en}/marketing/1–5` (1920 × 1080 mit Überschrift: Überblick, Umbenennen mit Rückgängig, Nahaufnahme 461/400 bzw. 440/400, PDF-Bericht, Fehler/Warnungen dunkel). Erzeugt mit `tools/compose_store_screenshots.py` aus den Roh-Aufnahmen `store/screenshots/{de,en}/{light,dark}/` (1366 × 820, aus den UI-Tests).

---

## Deutsch (de-DE)

**Titel:** Pathrova – Pfadprüfer für Cloud-Migration

**Kurzbeschreibung:**
Findet vor der Migration jeden Pfad, den OneDrive und SharePoint ablehnen würden – in einem Durchgang. Danach sicher umbenennen mit Vorschau und Rückgängig, Bericht als CSV oder PDF. Offline, ohne Konto.

**Beschreibung:**
Der Sync-Client meldet Probleme erst während der Synchronisierung, und zwar eins nach dem anderen: „Pfad zu lang“, „ungültiger Name“, „Datei kann nicht hochgeladen werden“. Bei einem Dateiserver mit zehntausenden Ordnern kann das Tage kosten.

Pathrova prüft den kompletten Ordnerbaum vorher in einem Durchgang und zeigt alle Probleme auf einmal, nach Art gruppiert.

DAS PRÜFT PATHROVA
• Pfadlänge in der Cloud: 400 Zeichen inklusive Bibliothekspfad, wahlweise streng URL-kodiert gezählt (Leerzeichen = %20, Umlaute = %C3%A4)
• Lokaler Sync-Pfad: 520 Zeichen inklusive Sync-Ordner wie „C:\Users\Max\OneDrive - Firma“
• Office-Grenze: Word, Excel und PowerPoint öffnen keine Dateien mit mehr als 256 Zeichen im Pfad
• Namen über 255 Zeichen
• Ungültige Zeichen: " * : < > ? / \ | und Steuerzeichen
• Reservierte Namen: CON, PRN, AUX, NUL, COM0–9, LPT0–9, .lock, ~$…, _vti_, „forms“ auf oberster Ebene
• Leerzeichen am Anfang oder Ende, Punkt am Ende
• # und % (in manchen Organisationen gesperrt), ; und & (Office)
• Gesperrte Dateitypen nach deiner eigenen Liste, Dateien über 250 GB
• Symbolische Links und Junctions, Systemdateien, die nicht synchronisiert werden, Ordner ohne Zugriff
• Mehr als 300.000 Elemente je Synchronisierung

Liegen unter einem zu langen Ordner viele Dateien, zeigt Pathrova den Ordner einmal mit der Zahl der betroffenen Elemente. So bleibt die Liste übersichtlich.

SICHER UMBENENNEN
• Vorschläge für jedes behebbare Problem: Zeichen ersetzen, Leerzeichen und Punkte entfernen, reservierte Namen ergänzen, zu lange Dateinamen an Wortgrenzen kürzen (die Endung bleibt)
• Namenskonflikte im selben Ordner löst Pathrova automatisch, z. B. mit „Name (2).docx“
• Vorschau mit Auswahl. Ohne deine Bestätigung wird nichts geändert
• Jede Umbenennung landet in einem Protokoll und lässt sich mit einem Klick rückgängig machen

BERICHT FÜR KUNDEN UND KOLLEGEN
• CSV-Export (Excel-tauglich, mit Vorschlägen) und PDF-Bericht mit Zusammenfassung je Problemart

FÜR GROSSE ABLAGEN
• Ein Durchgang auch bei 100.000+ Dateien, jederzeit abbrechbar
• Lokale Laufwerke, USB-Platten und Netzwerkfreigaben (\\server\freigabe)
• Filter nach Fehlern, Warnungen und Problemart, dazu eine Suche

PRIVAT
Pathrova arbeitet vollständig auf deinem PC: kein Konto, keine Cloud-Verbindung, keine Telemetrie, keine Werbung. Die App liest nur den gewählten Ordner.

Hinweis: Pathrova prüft und zeigt an. Es garantiert nicht, dass eine Synchronisierung oder Migration gelingt; Einstellungen deiner Organisation können abweichen. Die Regeln entsprechen den veröffentlichten Angaben von Microsoft (Stand November 2026). Microsoft, OneDrive und SharePoint sind Marken der Microsoft-Unternehmensgruppe. Pathrova ist ein unabhängiges Produkt.

**Funktionen (je ≤ 200 Zeichen):**
1. Prüft ganze Ordnerbäume in einem Durchgang auf Probleme mit OneDrive und SharePoint
2. Pfadlänge 400 (Cloud), 520 (Sync) und 256 (Office), wahlweise URL-kodiert gezählt
3. Findet ungültige Zeichen, reservierte Namen, Leerzeichen am Anfang oder Ende und Punkte am Ende
4. Gruppiert nach Problemart mit Fehlern, Warnungen und Hinweisen
5. Sichere Umbenennungsvorschläge mit Vorschau, nichts ohne Bestätigung
6. Rückgängig-Protokoll für jede Umbenennung
7. CSV- und PDF-Bericht für Kunden und Dokumentation
8. Schnell bei 100.000+ Dateien, abbrechbar, auch für Netzlaufwerke
9. Eigene Liste gesperrter Dateitypen
10. Offline, kein Konto, keine Telemetrie; Deutsch und Englisch, hell und dunkel

**Suchbegriffe (7):** Pfad zu lang · Pfadlänge prüfen · OneDrive Sync Fehler · SharePoint Migration · ungültige Zeichen · Dateinamen bereinigen · Dateiserver Migration

**Neuerungen (1.0):** Erste Version.

---

## English (en-US)

**Title:** Pathrova – Path Checker for Cloud Migration

**Short description:**
Finds every path and name that OneDrive and SharePoint will reject – before you migrate, in one pass. Then rename safely with preview and undo, and export a CSV or PDF report. Offline, no account.

**Description:**
The sync client reports problems only while syncing, one at a time: “path too long”, “invalid name”, “can't upload”. On a file server with tens of thousands of folders that can take days.

Pathrova checks the whole folder tree up front in a single pass and shows every issue at once, grouped by type.

WHAT PATHROVA CHECKS
• Cloud path length: 400 characters including the library path, optionally counted strictly URL-encoded (space = %20)
• Local sync path: 520 characters including the sync folder, e.g. “C:\Users\Max\OneDrive - Contoso”
• Office limit: Word, Excel and PowerPoint can't open files whose path exceeds 256 characters
• Names longer than 255 characters
• Invalid characters: " * : < > ? / \ | and control characters
• Reserved names: CON, PRN, AUX, NUL, COM0–9, LPT0–9, .lock, ~$…, _vti_, “forms” at the top level
• Leading or trailing spaces, trailing dots
• # and % (blocked in some organizations), ; and & (Office)
• Blocked file types from your own list, files over 250 GB
• Symbolic links and junctions, system files that are not synced, folders without access
• More than 300,000 items per sync

When a folder path is too long, Pathrova lists the folder once with the number of affected items inside, so the list stays readable.

SAFE RENAMING
• Suggestions for every fixable issue: replace characters, trim spaces and dots, extend reserved names, shorten long file names at word boundaries (the extension is kept)
• Name clashes in the same folder are resolved automatically, e.g. “Name (2).docx”
• Preview with selection. Nothing changes until you confirm
• Every rename is logged and can be undone with one click

REPORTS FOR CLIENTS AND COLLEAGUES
• CSV export (Excel-ready, with suggestions) and a PDF report with a summary per issue type

BUILT FOR LARGE SHARES
• One pass even for 100,000+ files, cancel at any time
• Local drives, USB disks and network shares (\\server\share)
• Filter by errors, warnings and issue type, plus search

PRIVATE
Pathrova runs entirely on your PC: no account, no cloud connection, no telemetry, no ads. It only reads the folder you choose.

Note: Pathrova checks and reports. It does not guarantee that a sync or migration will succeed; your organization's settings may differ. Rules follow Microsoft's published guidance (as of November 2026). Microsoft, OneDrive and SharePoint are trademarks of the Microsoft group of companies. Pathrova is an independent product.

**Features:**
1. Checks entire folder trees in one pass for OneDrive and SharePoint issues
2. Path length 400 (cloud), 520 (sync) and 256 (Office), optionally URL-encoded
3. Finds invalid characters, reserved names, leading/trailing spaces and trailing dots
4. Grouped by issue type with errors, warnings and notes
5. Safe rename suggestions with preview, nothing changes without confirmation
6. Undo log for every rename
7. CSV and PDF reports for clients and documentation
8. Fast with 100,000+ files, cancellable, works on network shares
9. Your own list of blocked file types
10. Offline, no account, no telemetry; English and German, light and dark

**Search terms (7):** path too long · path length checker · OneDrive sync errors · SharePoint migration · invalid characters · rename files · file server migration

**What's new (1.0):** First release.
