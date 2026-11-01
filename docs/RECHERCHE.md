# Pathrova – Recherche (Stand 01.11.2026)

Arbeitsname **Pathrova**: Pfadprüfer vor dem Synchronisieren oder Migrieren nach OneDrive bzw. SharePoint. Name und Icon verwenden keine Microsoft-Marken. „OneDrive“ und „SharePoint“ stehen nur beschreibend im Text („für OneDrive und SharePoint“).

## Problem
Der OneDrive-Client meldet zu lange Pfade, ungültige Zeichen und gesperrte Namen erst während der Synchronisierung, und zwar einzeln. Bei großen Ablagen dauert es Tage, bis alle Probleme gefunden sind. Belege sind Microsoft-Q&A-Threads wie 5496924, 5656229, 5416905 und 4427394 auf learn.microsoft.com/answers, außerdem die IT-Anleitungen von Hochschulen (OSU empfiehlt vor Migrationen „Path Length Checker“). Laut Nordflux (08/2026) kommen SPMT-Fehler meist von zu langen Zielpfaden und ungültigen Ordnernamen.

## Konkurrenz
| Tool | Was es kann | Preis |
|---|---|---|
| Long File Path Checker (MS Store, 9PJTL35KLPLF) | nur Pfadlänge (Schwelle 240/260/400 oder eigene), CSV-Export | 0,99 €, ohne Bewertung |
| Path Length Checker (deadlydog, GitHub) | listet alle Pfade mit Länge, keine Regeln, kein Umbenennen | gratis, seit Jahren unverändert |
| TreeSize Free/Pro (JAM) | Speicheranalyse, Pro mit Suche nach langen Pfaden; keine OneDrive-Regeln, kein Umbenennen | Free gratis, Personal ca. 19,95 €, Pro ca. 46,95 € (zum Teil im Abo) |
| Microsoft SPMT | Migration mit Vorab-Scan („folder name is invalid“), braucht Tenant und Admin-Anmeldung | gratis |
| ShareGate Migrate | professionelle Migration inkl. Vorprüfung | ab 5.995 USD/Jahr |
| PowerShell-Skripte | zuverlässig, aber nur für Admins, ohne Vorschau oder Rückgängig | gratis |

**Lücke:** Es gibt kein Store-Werkzeug für Endkunden, das *alle* OneDrive-/SharePoint-Regeln in einem Durchgang prüft, sichere Umbenennungen mit Vorschau und Rückgängig anbietet und einen PDF-Bericht für Kunden erzeugt. Daran haben kleine Firmen und IT-Dienstleister Bedarf: Sie haben keinen ShareGate-Etat, wollen aber auch kein PowerShell.

## Regeln (Quelle: Microsoft Support „Restrictions and limitations in OneDrive and SharePoint“ und „What are the file path length limits?“, abgerufen am 01.11.2026)
- **Pfadlänge Cloud:** 400 Zeichen für den gesamten Pfad ab der Bibliothek, einschließlich Dateiname. Microsoft widerspricht sich selbst, ob die Zeichen dekodiert oder URL-kodiert gezählt werden (Leerzeichen → %20). Die App zeigt deshalb beide Werte und prüft standardmäßig streng mit dem kodierten Wert. Ein Präfix lässt sich einstellen (z. B. „Freigegebene Dokumente/Projekte“).
- **Lokaler Sync-Pfad:** höchstens 520 Zeichen (400 + OneDrive-Stammordner bis 120). **Office-Desktop/Explorer:** 260 bzw. 256 Zeichen (Warnung). Namen höchstens 255 Zeichen.
- **Ungültige Zeichen:** `" * : < > ? / \ |`, dazu Steuerzeichen. Warnungen gibt es für `#` und `%` (je nach Mandant), `;` (Office-Speichern) und `&` (Office 2010).
- **Reservierte Namen:** `.lock`, CON, PRN, AUX, NUL, COM0–COM9, LPT0–LPT9 (auch mit Endung), `desktop.ini`; Namen, die mit `~$` beginnen; `_vti_` irgendwo im Namen; `forms` auf der obersten Ebene einer Bibliothek; Ordner, die mit `゛` oder `ဧ` beginnen.
- **Leerzeichen** am Anfang und am Ende sind verboten; einen Punkt am Ende behandelt Windows ebenfalls als Problem (Warnung).
- **Dateitypen:** Es gibt keine allgemeine Sperrliste. `.tmp` und `.ds_store` werden nicht synchronisiert; Admins können Typen sperren (die App hat eine einstellbare Liste); `.pst` wird nur verzögert synchronisiert (Hinweis).
- **Dateigröße:** höchstens 250 GB je Datei. OneNote-Notizbücher bis 2 GB. Über 100 MB gibt es keine Vorschau (Hinweis).
- **Mengen:** Microsoft empfiehlt höchstens 300.000 Elemente je Sync (Hinweis bei Überschreitung); Freigaben bis 50.000 Unterelemente.
- **Sonstiges:** Symbolische Links und Junctions werden nicht unterstützt (Warnung).

Die App stellt diese Regeln als Hinweise dar. Sie gibt keine Garantie, dass eine Synchronisierung oder Migration gelingt. Mandanten-Einstellungen können abweichen.

## Funktionen 1.0
Scan in einem Durchgang (abbrechbar, auch Netzlaufwerke und UNC, 100.000+ Elemente), Probleme nach Art gruppiert, Filter und Suche; sichere Umbenennungsvorschläge (ersetzen, kürzen, trimmen) mit Vorschau und Konfliktprüfung, Ausführen nur nach Bestätigung, Rückgängig-Protokoll (JSON) und Rückgängig. Export als CSV und PDF. Deutsch und Englisch, offline, kein Konto, keine Telemetrie.

## Preis
Zielgruppe sind kleine Firmen und IT-Dienstleister, die das Tool pro Kundenprojekt einsetzen. Ein Freemium-Modell (Analyse gratis, Umbenennen und Export kostenpflichtig) bräuchte In-App-Käufe und Lizenzlogik; das kann eine spätere Version bringen. **Start: Kauf-App 14,99 € mit 7-Tage-Testversion über den Store** (der Store regelt Test und Kauf, im Code ist dafür nichts nötig). Kaufkraftstaffel wie bei Folvane, 14,99 € als Basis: Gruppe B 11,99 € (IT, ES, PT, GR, CZ, PL, HU), C 8,49 € (RO, BG, MX, BR, CL, ZA, MY, TH, CN, TR), D 399 ₹ (IN, ID, PH, VN, PK, BD). USA/UK/CH/Nordics 14,99 USD/GBP-Äquivalent. 24,99 € wäre bei null Bewertungen zu hoch; nach den ersten Bewertungen auf 19,99 € anheben.

## ASO
- **DE:** Pfadlänge prüfen, Pfad zu lang, OneDrive Synchronisierung Fehler, SharePoint Migration vorbereiten, ungültige Zeichen Dateiname, Dateinamen bereinigen, 400 Zeichen, Dateiserver Migration, Umbenennen
- **EN:** path too long, path length checker, OneDrive sync errors, SharePoint migration, invalid characters file name, rename files, file server migration, 400 character limit, Teams files

## Name
„Pathrova“ geprüft am 01.11.2026: Microsoft Store (Such-API) ohne Treffer, Apple App Store ohne Treffer, Steam ohne Treffer, Google Play ohne Treffer (Suche liefert nur fremde Apps), TMview „enthält pathrova“ ohne Treffer. Store-Titel: „Pathrova – Pfadprüfer für Cloud-Migration“ (EN: „Pathrova – Path Checker for Cloud Migration“).

## Urteil
**Lohnt sich, in maßvollem Umfang.** Der Schmerz ist belegt und wiederkehrend, im Store gibt es nur ein Minimal-Tool für 0,99 €, die Profi-Tools kosten Tausende. Realistisch sind wenige bis einige Dutzend Verkäufe im Monat (etwa 50–300 € netto); allein trägt die App das Ziel von 1.500 € nicht, als Baustein im Portfolio aber schon. Der Bauaufwand ist gering, weil Stack und Pipeline von Folvane übernommen werden. Risiko: Microsoft verbessert die eigene Vorprüfung im Sync-Client.
