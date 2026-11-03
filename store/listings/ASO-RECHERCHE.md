# ASO-Recherche Pathrova (Rohdaten, Stand 03.11.2026)

Methode: Store-Suche `https://apps.microsoft.com/api/products/search?query=…&mediaType=apps&gl=…&hl=…` (JSON, `productsList`, funktioniert) und Google Suggest (`client=firefox`). Gezeigt: Top 10 je Anfrage (Titel | Publisher | Kategorie).

## Auffälligkeiten
- Keine App im Store zielt auf „path too long“ / OneDrive-Pfadprüfung. Bei „path too long“ erscheinen ZIP/RAR-Entpacker, TreeSize Free, WinDirStat, Terminal/Notepad – die Store-Suche ist semantisch/unscharf, keine Treffer mit exakter Phrase.
- Nächste Wettbewerber nach Titelwörtern: Umbenenner („File Renamer“, „Advanced Renamer – Bulk Rename Files, Batch Rename“, „Smart File Renamer“, „Bulk File Rename“, „KuKnet FileRenamer“, „OneCommander“, „FileBot“), Speicher-Analysen (TreeSize Free, WinDirStat), Cloud-Clients (Dropbox, iCloud, Cloud Drive!, TeraBox, MobiDrive – Titel mit „Cloud Storage & Sync“). Titelwörter dieser Gruppe: rename, bulk, batch, file, folder, cloud, sync, ZIP/RAR.
- „sharepoint migration“ (US/DE): nur Teams, Power Apps, Dropbox, Dateiübertragungs-Apps – kein Migrations-Prüfer. Ranking praktisch frei.
- DE „pfad zu lang“, FR „chemin trop long“, JP „パス 長すぎる“: nur zufällige Treffer (Lebenslauf, Karten, Hilfs-Apps) – Wettbewerb praktisch null, der Titel mit exakter Phrase sollte genügen.
- Wettbewerber-Titel nennen selten die Fehlermeldung; sie nennen die Funktion (Rename, Cleaner, Sync).

## Store-Suche (Rohausgabe)
```
### [US/en-US] path too long
1. iCloud | Apple Inc. | ['Productivity']
2. ZIP RAR & 7Z | BreeZip | ['Productivity']
3. RAR Zip 7Z | Codaapp Studio | ['Utilities & tools']
4. TreeSize Free | JAM Software | ['Utilities & tools']
5. ZIP Unzip - RAR & 7Z File Compressor | Wuhan Net Power Technology Co., Ltd | ['Utilities & tools']
6. WinDirStat | WinDirStat Team | ['Utilities & tools']
7. CapCut | BYTEDANCE PTE. LTD | ['Photo & video']
8. Ubuntu | Canonical Group Limited | ['Developer tools']
9. Windows Notepad | Microsoft Corporation | ['Productivity']
10. Windows Terminal | Microsoft Corporation | ['Developer tools']
### [US/en-US] long path
1. HP | HP Inc. | ['Utilities & tools']
2. Telegram Desktop | Telegram Messenger LLP | ['Social']
3. Windows App | Microsoft Corp. | ['Productivity']
4. Ubuntu | Canonical Group Limited | ['Developer tools']
5. Bing Wallpaper | Microsoft Corporation | ['Photo & video']
6. Brave Browser | Brave | ['Productivity']
7. Plex for Windows | Plex | ['Entertainment']
8. LINE Desktop | LINE (LY Corporation) | ['Social']
9. WinDirStat | WinDirStat Team | ['Utilities & tools']
10. Pluto TV | Pluto Inc. | ['Entertainment']
### [US/en-US] onedrive
1. iCloud | Apple Inc. | ['Productivity']
2. Dropbox | Dropbox Inc. | ['Productivity']
3. Microsoft Teams | Microsoft Corporation | ['Productivity']
4. Microsoft Copilot | Microsoft Corporation | ['Productivity']
5. Adobe Creative Cloud | ADOBE INC. | ['Productivity']
6. Cloud Drive! : Cloud Storage with multi accounts | Define Studio | ['Productivity']
7. Samsung Cloud | Samsung Electronics Co. Ltd. | ['Productivity']
8. Dropbox Lite | Dropbox Inc. | ['Productivity']
9. TeraBox for PC | フレックステック株式会社 | ['Utilities & tools']
10. OneNote | Microsoft Corporation | ['Productivity']
### [US/en-US] sharepoint migration
1. Microsoft Teams | Microsoft Corporation | ['Productivity']
2. Notion | Notion Labs Inc. | ['Productivity']
3. Quick Share | Samsung Electronics Co, Ltd. | ['Utilities & tools']
4. TeamViewer Remote | TeamViewer | ['Productivity']
5. Power Apps | Microsoft Corp. | ['Business']
6. PowerShell | Microsoft Corporation | ['Developer tools']
7. Blip: Send Files in a Click | Blip Studio Inc. | ['Productivity']
8. Dropbox | Dropbox Inc. | ['Productivity']
9. Leap Share - Pc Phone File Transfer | Define Studio | ['Productivity']
10. TeraBox for PC | フレックステック株式会社 | ['Utilities & tools']
### [US/en-US] file name
1. File Renamer | Aftnet | ['Productivity']
2. Advanced Renamer - Bulk Rename Files, Batch Rename | Semivio Technologies | ['Utilities & tools']
3. Advanced Renamer | Hulubulu Software | ['Utilities & tools']
4. Files App | Files Community | ['Productivity']
5. Smart File Renamer · | Qiplex | ['Utilities & tools']
6. OneCommander | milosp | ['Utilities & tools']
7. FileBot | Point Planck Limited | ['Utilities & tools']
8. Bulk File Rename | Valley Software | ['Utilities & tools']
9. File Explorer Tool PRO | Art Group | ['Productivity']
10. 1Click Photo Viewer | dreamoji.com | ['Photo & video']
### [US/en-US] invalid characters
1. Microsoft PowerToys | Microsoft Corporation | ['Developer tools']
2. TypingMagic 12 | Cloud Kayak Labs, Inc | ['Productivity']
3. Voicemod | Voicemod | ['Entertainment']
4. Windows Notepad | Microsoft Corporation | ['Productivity']
5. Python 3.13 | Python Software Foundation | ['Developer tools']
6. Code::Blocks | Martin Halle | ['Developer tools']
7. Gears 5 x Luke Preece | Microsoft Corporation | ['Personalization']
8. Discord | Discord, Inc. | ['Social']
9. Duolingo - Language Lessons | Duolingo Inc. | ['Education']
10. Visual Studio Code - Insiders | Microsoft Corporation | ['Developer tools']
### [US/en-US] rename files
1. Smart File Renamer · | Qiplex | ['Utilities & tools']
2. Microsoft PowerToys | Microsoft Corporation | ['Developer tools']
3. Advanced Renamer | Hulubulu Software | ['Utilities & tools']
4. iCloud | Apple Inc. | ['Productivity']
5. Advanced Renamer - Bulk Rename Files, Batch Rename | Semivio Technologies | ['Utilities & tools']
6. OneCommander | milosp | ['Utilities & tools']
7. Adobe Acrobat Reader DC | ADOBE INC. | ['Productivity']
8. Xender - Share Music Transfer | Xender (HK) Limited | ['Utilities & tools']
9. HEIC Convert Pro - Image Converter | MeetmeXM Technology Co., Ltd | ['Utilities & tools']
10. ZIP RAR & 7Z | BreeZip | ['Productivity']
### [US/en-US] onedrive sync
1. MobiDrive - Cloud Storage & Sync | MobiSystems | ['Productivity']
2. iCloud | Apple Inc. | ['Productivity']
3. Dropbox | Dropbox Inc. | ['Productivity']
4. Dropbox Lite | Dropbox Inc. | ['Productivity']
5. Cloud Drive! : Cloud Storage with multi accounts | Define Studio | ['Productivity']
6. TeraBox for PC | フレックステック株式会社 | ['Utilities & tools']
7. Samsung Cloud | Samsung Electronics Co. Ltd. | ['Productivity']
8. SyncFolder | Jos Huybrighs | ['Utilities & tools']
9. OfficeSuite Office Pack & PDF Editor - MobiOffice | MobiSystems | ['Productivity']
10. Cloud Drive for Storage Service | Crity Inc | ['Productivity']
### [US/en-US] filename checker
1. Outlook for Windows | Microsoft Corporation | ['Productivity']
2. TeraCopy | Code Sector | ['Utilities & tools']
3. Grammarly: AI Writing and Grammar App | Grammarly | ['Productivity']
4. ZIP Unzip - RAR & 7Z File Compressor | Wuhan Net Power Technology Co., Ltd | ['Utilities & tools']
5. Xender - Share Music Transfer | Xender (HK) Limited | ['Utilities & tools']
6. Windows Notepad | Microsoft Corporation | ['Productivity']
7. ZIP RAR & 7Z | BreeZip | ['Productivity']
8. OneCommander | milosp | ['Utilities & tools']
9. RAR Zip 7Z | Codaapp Studio | ['Utilities & tools']
10. HEIC Convert Pro - Image Converter | MeetmeXM Technology Co., Ltd | ['Utilities & tools']
### [DE/de-DE] pfad zu lang
1. Lebenslauf Kostenlos | Meringtone Apps | ['Produktivität']
2. Freie Nachrichten | ESXsystems | ['Nachrichten & Wetter']
3. PasswortApp | Engelberth Developing | ['Produktivität']
4. LernSax | DigiOnline GmbH | ['Bildung']
5. ATEGRA Web-File-Handler für eGeKo | ATEGRA | ['Unternehmen']
6. WindowsUnited | cee | ['Nachrichten & Wetter']
7. dokublick | Buhl Data Service GmbH | ['Dienstprogramme & Tools']
8. Löffelhardt | Emil Löffelhardt GmbH & Co. KG | ['Shopping']
9. Backup Service Home | Alexander Seeliger | ['Dienstprogramme & Tools']
10. emis.dms | Karg EDV Dienstleistungen GmbH | ['Unternehmen']
### [DE/de-DE] dateinamen
1. Geldzähler | StengelL | ['Meine Finanzen']
2. ZUGFeRD-Konverter | Ossowski-Engineering | ['Unternehmen']
3. dm-Fotowelt | CEWE | ['Fotos & Videos']
4. TvProgramm10 | Thomas Gerber | ['Unterhaltung']
5. gaeb365.online | anouri GmbH | ['Unternehmen']
6. wingaebxml | wingaeb | ['Produktivität']
7. DBF View | App Model Soft | ['Dienstprogramme & Tools']
8. dokublick | Buhl Data Service GmbH | ['Dienstprogramme & Tools']
9. DiashowDL | Peter Sauer | ['Fotos & Videos']
10. emis.dms | Karg EDV Dienstleistungen GmbH | ['Unternehmen']
### [DE/de-DE] onedrive
1. iCloud | Apple Inc. | ['Produktivität']
2. Microsoft Teams | Microsoft Corporation | ['Produktivität']
3. Dropbox | Dropbox Inc. | ['Produktivität']
4. Samsung Cloud | Samsung Electronics Co. Ltd. | ['Produktivität']
5. Cloud Drive! : Cloud-Speicher mit Multi-Account-Unterstützung | Define Studio | ['Produktivität']
6. Cloud Drive for Storage Service | Crity Inc | ['Produktivität']
7. Coolmuster Backup Recovery for iCloud | SHENZHEN QIXINGSHI TECHNOLOGY CO.,LTD | ['Dienstprogramme & Tools']
8. HEIC Viewer for Windows | GameTools | ['Fotos & Videos']
9. GDrive Desktop | Axxvion S.R.L | ['Produktivität']
10. Microsoft Copilot | Microsoft Corporation | ['Produktivität']
### [DE/de-DE] sharepoint migration
1. OpenCloud | OpenCloud GmbH | ['Produktivität']
2. Easy Share - File Transfer | Jump Dong | ['Dienstprogramme & Tools']
3. PDF Converter. | LAT | ['Produktivität']
4. WebDAV Drive | IT Hit LTD. | ['Produktivität']
5. BackUp Maker - Automatische Datensicherung | ASCOMP Software GmbH | ['Dienstprogramme & Tools']
6. Fast Copy to USB - Ultra USB Copy | IO Lab | ['Dienstprogramme & Tools']
7. btDatabase Converter | hballin | ['Produktivität']
8. Intune Network Drive Mapping | Hauke Hasselberg | ['Dienstprogramme & Tools']
9. IfcToolbox | BIMMars | ['Produktivität']
10. Tacho File Uploader | Digitalflotte | ['Dienstprogramme & Tools']
### [DE/de-DE] dateipfad
1. Files App | Files Community | ['Produktivität']
2. MixPad Musikstudio-Software | NCH Software | ['Musik']
3. Ahnenblatt | Dirk Böttcher | ['Produktivität']
4. VideoPad Video-Editor | NCH Software | ['Fotos & Videos']
5. PasswortApp | Engelberth Developing | ['Produktivität']
6. VSB Haushaltsplaner | VerbraucherService Bayern im KDFB e.V. | ['Meine Finanzen']
7. TvProgramm10 | Thomas Gerber | ['Unterhaltung']
8. ATEGRA Web-File-Handler für eGeKo | ATEGRA | ['Unternehmen']
9. dokublick | Buhl Data Service GmbH | ['Dienstprogramme & Tools']
10. Express Zip ZIP-Programm | NCH Software | ['Dienstprogramme & Tools']
### [DE/de-DE] ordner umbenennen
1. Folder Icon Change | Niels Vermeiren | ['Entwicklungstools']
2. ZUGFeRD-Konverter | Ossowski-Engineering | ['Unternehmen']
3. Prism Video-Converter | NCH Software | ['Fotos & Videos']
4. KuKnet FileRenamer | KuKnet Software | ['Dienstprogramme & Tools']
5. Fold11 - Folder Icon Maker | Jang Oetama | ['Personalisierung']
6. Folder Icon Changer | MakeTone | ['Dienstprogramme & Tools']
7. Pixillion Bildkonverter | NCH Software | ['Fotos & Videos']
8. Digitaler Schulranzen | Paul Oberosler | ['Dienstprogramme & Tools']
9. Windows Folder Organizer | Microsoft Corporation | ['Dienstprogramme & Tools']
10. Express Zip ZIP-Programm | NCH Software | ['Dienstprogramme & Tools']
### [FR/fr-FR] chemin trop long
1. Écriture | Neomars | ['Productivité']
2. MALClient | Drutol | ['Divertissement']
3. Hub de commentaires | Microsoft Corporation | ['Utilitaires et outils']
4. Module d'expérience locale français (France) | Microsoft Corporation | ['Personnalisation']
5. My Map | Thomas Gössi | ['Navigation et cartes']
6. MaxiCompte | PersoApps Studio | ['Finances personnelles']
7. Cartes Pro | Nicollet Fabien | ['Navigation et cartes']
8. Horizon Link | Aragorn102 | ['Utilitaires et outils']
9. Litchi Hub Bridge | VC Technology | ['Productivité']
10. Kortext | Kortext | ['Livres et référence']
### [ES/es-ES] ruta demasiado larga
1. RAR Zip 7Z | Codaapp Studio | ['Utilities & tools']
2. RAR Opener | Tiny Opener | ['Utilities & tools']
3. RunCat 365 | Studio Kyome | ['Utilities & tools']
4. MadREAD sin límites | ODILO TID SL | ['Books & reference']
5. Control del Archivos | CodeBlocksPro | ['Utilities & tools']
6. Aprende Ortografia y Gramática | The City Of The Apps | ['Education']
7. RAR Opener & RAR to ZIP Converter | Cidade | ['Utilities & tools']
8. RandoCartes | ParkerzSoft | ['Navigation & maps']
9. Apagado automatico | ZurdoMLG | ['Utilities & tools']
10. ROG GameVisual | ASUSTeK COMPUTER INC. | ['Utilities & tools']
### [JP/ja-JP] パス 長すぎる
1. らくらく動画圧縮 | 野田工房 | ['マルチメディア デザイン']
2. JWビューワ | A.M.Soft | ['ユーティリティ & ツール']
3. 秀丸エディタ(ストアアプリ版) | SAITO-KIKAKU CORPORATON | ['仕事効率化']
4. 日本語 ローカル エクスペリエンス パック | Microsoft Corporation | ['個人用設定']
5. 速報ストリーム | 野田工房 | ['ニュース & 天気']
6. ホットキー履歴くん | 野田工房 | ['開発者ツール']
7. アプリ起動ガード | 野田工房 | ['開発者ツール']
8. 簡単！コピペ | Kenichi Ando | ['ユーティリティ & ツール']
9. URLアクセス制限ツール | 野田工房 | ['ユーティリティ & ツール']
10. パスワード総合管理(ストアアプリ版) | SAITO-KIKAKU CORPORATON | ['仕事効率化']
```

## Google Suggest (Rohausgabe)
Format: `## [Sprache] Eingabe` und darunter die Vorschläge. Leere Zeilen = keine Vorschläge (z. B. sv/it/pt „onedrive …“-Varianten).
```
## [de] pfad zu lang
pfad zu lang | pfad zu lang windows 11 | pfad zu lang windows | pfad zu lang beim extrahieren | pfad zu lang zip | pfad zu lang was tun | pfad zu lang lösung | pfad zu lang onedrive | pfad zu lang beim kopieren | pfad zu lang beim entpacken
## [de] dateipfad zu lang
dateipfad zu lang | dateipfad zu lang was tun | dateipfad zu lang umgehen | dateipfad zu lang zum kopieren | dateipfad zu lang windows | dateipfad zu lang zum öffnen | dateipfad zu lang kopieren nicht möglich | dateipfad zu lang onedrive | excel dateipfad zu lang | zip dateipfad zu lang
## [de] onedrive dateiname ungültig

## [de] sharepoint migration
sharepoint migration tool | sharepoint migration tool download | sharepoint migration manager | sharepoint migration tool (spmt) | sharepoint migration | sharepoint migration tool anleitung | sharepoint migration tool (spmt) download | sharepoint migration tool microsoft | sharepoint migration tool delta sync | sharepoint migration agent
## [en] path too long
path too long error while extracting | path too long error | path too long error unzip | path too long | path too long when extracting | path too long when extracting files | path too long windows | path too long error extracting zip file | path too long extracting zip | path too long error windows 11
## [en] onedrive file name
onedrive file name too long | onedrive file name length limit | onedrive file name too long fix | onedrive file name not allowed | onedrive file name character limit | onedrive file name limitations | onedrive file name restrictions | onedrive file name length | onedrive file name | onedrive file name characters not allowed
## [en] sharepoint migration
sharepoint migration tool | sharepoint migration manager | sharepoint migration | sharepoint migration tool (spmt) | sharepoint migration tool download | sharepoint migration agent | sharepoint migration assessment tool | sharepoint migration jobs | sharepoint migration agent cannot scan | sharepoint migration api
## [en] file path too long windows
file path too long windows 11 | file path too long windows | file path too long windows 11 fix | file path too long windows 10 | file names too long windows | file path to long windows | file path too long microsoft | file path name too long windows | file path too long error windows | windows file path too long fix
## [fr] chemin trop long
chemin trop long windows 11 | chemin trop long windows | chemin trop long | chemin trop long onedrive | chemin trop long pour copier | chemin trop long windows 10 | chemin destination trop long | chemin d accès trop long | chemin d accès trop long windows 11 | chemin d accès trop long zip
## [fr] onedrive nom de fichier
onedrive nom de fichier trop long
## [es] ruta demasiado larga
ruta demasiado larga windows 11 | ruta demasiado larga onedrive | ruta demasiado larga | ruta demasiado larga windows 10 | ruta demasiado larga zip | error ruta demasiado larga | windows ruta demasiado larga | solucionar ruta demasiado larga | ruta acceso demasiado larga | ruta de acceso demasiado larga windows 11
## [es] onedrive nombre de archivo
cambiar nombre de archivo en onedrive
## [it] percorso troppo lungo
percorso troppo lungo windows 11 | percorso troppo lungo | percorso troppo lungo zip | percorso troppo lungo onedrive | percorso troppo lungo windows 10 | percorso troppo lungo windows | errore percorso troppo lungo | errore percorso troppo lungo windows 11 | percorso file troppo lungo | percorso destinazione troppo lungo
## [it] onedrive nome file
onedrive nome file troppo lungo
## [nl] pad te lang
pad te lang oplossen | pad te lang | pad te lang onedrive | pad te lang zip | pad te lang windows | pad is te lang foutmelding | pad is te lang foutmelding windows 11 | sharepoint pad te lang | zip uitpakken pad te lang | fout pad is te lang
## [nl] onedrive bestandsnaam
onedrive bestandsnaam te lang
## [pt] caminho muito longo
caminho muito longo | caminho muito longo windows 11 | caminho muito longo windows | caminho muito longo zip | caminho muito longo onedrive | caminho muito longo erro | caminho muito longo windows 10 | caminho muito longo extrair | erro caminho muito longo windows | erro ao extrair caminho muito longo
## [pt] onedrive nome do arquivo

## [pl] ścieżka za długa
ścieżka długa luka | ścieżka długa luka opinie | ścieżka długa luka zdjęcia | ścieżka długa luka pogoda | ścieżka długa luka parking | ścieżka docelowa jest za długa | ścieżka docelowa jest za długa zip | ścieżka docelowa jest za długa zmień nazwę folderu skompresowanego | za długa ścieżka docelowa windows 11 | za długa ścieżka
## [pl] onedrive nazwa pliku
onedrive nazwa pliku lub folderu zawiera niedozwolone znaki
## [sv] sökvägen är för lång
sökvägen är för lång
## [sv] onedrive filnamn

## [ja] パスが長すぎます
パスが長すぎます | パスが長すぎます 解凍 | パスが長すぎます 対策 | パスが長すぎます コピーできない
## [ja] onedrive ファイル名
onedrive ファイル名またはフォルダー名に使用できない文字が含まれています | onedrive ファイル名変更 | onedrive ファイル名変更 遅い | onedrive ファイル名 長さ | onedrive ファイル名の左上 | onedrive ファイル名 左上 | onedrive ファイル名 一括変更 | onedrive ファイル名 pc名 | onedrive ファイル名 使用できない文字 | onedrive ファイル名変更 反映されない
## [de] dateiname ungültige zeichen
dateiname ungültige zeichen | dateiname enthält ungültige zeichen | windows dateiname ungültige zeichen
## [de] onedrive pfad zu lang
onedrive pfad zu lang | onedrive fehlermeldung pfad zu lang | onedrive mac pfad zu lang | onedrive synchronisierung pfad zu lang | onedrive synchronisiert nicht pfad zu lang
## [de] sharepoint dateiname zu lang
sharepoint dateiname zu lang
## [en] onedrive path too long
onedrive path too long | onedrive path too long fix | onedrive path too long error | onedrive path too long mac | onedrive path too long fix mac | onedrive path too long issue | onedrive path too long limit | onedrive path too long macos | onedrive path too long regedit | onedrive file name too long
## [en] sharepoint invalid characters
sharepoint invalid characters | sharepoint invalid characters in filename | sharepoint illegal characters | sharepoint illegal characters in path | sharepoint online invalid characters | sharepoint folder invalid characters | sharepoint migration invalid characters | sharepoint url invalid characters | sharepoint folder name invalid characters | sharepoint migration tool invalid characters
## [fr] onedrive chemin trop long
onedrive chemin trop long | onedrive chemin d accès trop long
## [es] onedrive ruta demasiado larga
onedrive ruta demasiado larga
## [nl] onedrive pad te lang
onedrive pad te lang
## [pt] onedrive caminho muito longo
onedrive caminho muito longo | onedrive erro caminho muito longo
## [sv] sökväg för lång
sökväg för lång | för lång sökväg windows 11 | för lång sökväg zip
## [sv] filnamn för långt
filnamn för långt
## [sv] onedrive sökväg
onedrive för lång sökväg
## [it] onedrive nome file non valido

## [pl] onedrive ścieżka za długa
onedrive za długa ścieżka
## [pl] ścieżka pliku za długa
za długa ścieżka pliku
## [ja] onedrive パス 長すぎる
onedrive パス 長すぎる
```

## Echte Formulierungen (Ableitung)
- DE: „pfad zu lang“, „dateipfad zu lang“, „pfad zu lang onedrive“, „onedrive synchronisierung pfad zu lang“, „dateiname enthält ungültige zeichen“, „sharepoint migration tool“.
- EN: „path too long“, „file path too long windows 11“, „onedrive path too long“, „onedrive file name too long / not allowed / characters not allowed“, „sharepoint invalid characters in filename“, „sharepoint migration tool“.
- FR: „chemin trop long“, „chemin d'accès trop long“, „chemin destination trop long“, „onedrive chemin trop long“.
- ES: „ruta demasiado larga“, „ruta de acceso demasiado larga“, „ruta demasiado larga onedrive“.
- IT: „percorso troppo lungo“, „percorso file troppo lungo“, „percorso destinazione troppo lungo“, „onedrive nome file troppo lungo“.
- NL: „pad te lang“, „pad is te lang foutmelding“, „sharepoint pad te lang“, „onedrive bestandsnaam te lang“.
- PT: „caminho muito longo“, „erro caminho muito longo“, „onedrive erro caminho muito longo“.
- PL: „ścieżka docelowa jest za długa“, „za długa ścieżka“, „onedrive nazwa pliku … zawiera niedozwolone znaki“.
- SV: „sökvägen är för lång“, „för lång sökväg windows 11“, „onedrive för lång sökväg“.
- JA: „パスが長すぎます“, „… 対策 / 解凍 / コピーできない“, „onedrive ファイル名 使えない文字“.
- Die meisten Phrasen kommen mit „windows 11“, „zip/extrahieren“ und „onedrive“; „Pfad zu lang + OneDrive“ ist in allen Sprachen stark.
