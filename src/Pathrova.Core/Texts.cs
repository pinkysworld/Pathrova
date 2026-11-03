using System.Globalization;

namespace Pathrova.Core;

public static class Format
{
    public static string Size(long b)
    {
        string[] u = ["B", "KB", "MB", "GB", "TB"];
        double v = b; int i = 0;
        while (v >= 1024 && i < u.Length - 1) { v /= 1024; i++; }
        return i == 0 ? $"{b} B" : $"{v:0.#} {u[i]}";
    }

    public static string Number(long n, bool english) => n.ToString("N0", english ? CultureInfo.GetCultureInfo("en-US") : CultureInfo.GetCultureInfo("de-DE"));
}

/// <summary>Alle Oberflächen- und Berichtstexte, Deutsch und Englisch.</summary>
public sealed class Texts
{
    public bool English { get; }
    private Texts(bool en) => English = en;
    private static readonly Texts De = new(false), En = new(true);
    public static Texts For(bool english) => english ? En : De;
    private string T(string de, string en) => English ? en : de;

    public string AppTitle => T("Pathrova – Pfadprüfer für Cloud-Migration", "Pathrova – Path Checker for Cloud Migration");
    public string Tagline => T("Findet zu lange Pfade, ungültige Zeichen und gesperrte Namen vor dem Synchronisieren nach OneDrive oder SharePoint – alle auf einmal.",
                               "Finds paths that are too long, invalid characters and blocked names before you sync to OneDrive or SharePoint – all at once.");
    public string Folder => T("Ordner", "Folder");
    public string FolderHint => T("Ordner, Laufwerk oder Netzwerkfreigabe (\\\\server\\freigabe) wählen oder hierher ziehen", "Choose or drop a folder, drive or network share (\\\\server\\share)");
    public string Browse => T("Durchsuchen …", "Browse …");
    public string Scan => T("Prüfen", "Check");
    public string Cancel => T("Abbrechen", "Cancel");
    public string Options => T("Ziel und Regeln", "Target and rules");
    public string TargetPrefix => T("Pfad in der Zielbibliothek", "Path in target library");
    public string TargetPrefixHint => T("z. B. Freigegebene Dokumente/Projekte – zählt zur 400-Zeichen-Grenze", "e.g. Shared Documents/Projects – counts towards the 400-character limit");
    public string LocalRoot => T("Lokaler Sync-Ordner", "Local sync folder");
    public string LocalRootHint => T("z. B. C:\\Users\\Max\\OneDrive - Firma – zählt zu 520 bzw. 256 Zeichen", "e.g. C:\\Users\\Max\\OneDrive - Company – counts towards 520 and 256 characters");
    public string CountEncoded => T("Streng zählen: Sonderzeichen und Leerzeichen URL-kodiert (%20)", "Strict: count special characters and spaces URL-encoded (%20)");
    public string WarnHash => T("# und % melden (in manchen Organisationen gesperrt)", "Report # and % (blocked in some organizations)");
    public string WarnOffice => T("; und & melden (Probleme beim Speichern aus Office)", "Report ; and & (issues saving from Office)");
    public string Blocked => T("Gesperrte Dateitypen", "Blocked file types");
    public string BlockedHint => T("von deiner IT vorgegeben, z. B. exe; bat; mdb", "set by your IT, e.g. exe; bat; mdb");
    public string Scanning(long files, long folders) => T($"Prüfe … {Format.Number(files, false)} Dateien, {Format.Number(folders, false)} Ordner", $"Checking … {Format.Number(files, true)} files, {Format.Number(folders, true)} folders");
    public string Done(ScanResult r) => T(
        $"{Format.Number(r.Files, false)} Dateien und {Format.Number(r.Folders, false)} Ordner in {r.Duration.TotalSeconds.ToString("0.0", CultureInfo.GetCultureInfo("de-DE"))} s geprüft{(r.Cancelled ? " (abgebrochen, unvollständig)" : "")}.",
        $"Checked {Format.Number(r.Files, true)} files and {Format.Number(r.Folders, true)} folders in {r.Duration.TotalSeconds.ToString("0.0", CultureInfo.InvariantCulture)} s{(r.Cancelled ? " (cancelled, incomplete)" : "")}.");
    public string Empty => T("Ordner, Laufwerk oder Netzwerkfreigabe oben wählen oder hierher ziehen und „Prüfen“ klicken. Beim Prüfen wird nichts verändert.", "Choose or drop a folder, drive or network share above and click “Check”. Checking changes nothing.");
    public string NoIssues => T("Keine Probleme gefunden.", "No issues found.");
    public string About => T("Über Pathrova", "About Pathrova");
    public string AboutPrivacy => T("Offline, kein Konto, keine Telemetrie. © 2026 Minh Studios.", "Offline, no account, no telemetry. © 2026 Minh Studios.");
    public string Fixable => T("automatisch behebbar", "fixable automatically");
    public string All => T("Alle", "All");
    public string Errors => T("Fehler", "Errors");
    public string Warnings => T("Warnungen", "Warnings");
    public string Infos => T("Hinweise", "Notes");
    public string Search => T("Suchen …", "Search …");
    public string Rename => T("Umbenennen …", "Rename …");
    public string Undo => T("Letzte Umbenennung rückgängig", "Undo last rename");
    public string ExportCsv => T("CSV", "CSV");
    public string ExportPdf => T("PDF-Bericht", "PDF report");
    public string Settings => T("Einstellungen", "Settings");
    public string Theme => T("Darstellung", "Theme");
    public string Language => T("Sprache", "Language");
    public string System => T("System", "System");
    public string Light => T("Hell", "Light");
    public string Dark => T("Dunkel", "Dark");
    public string KindCol => T("Problem", "Issue");
    public string PathCol => T("Pfad", "Path");
    public string DetailCol => T("Details", "Details");
    public string SeverityCol => T("Stufe", "Level");
    public string ProposalCol => T("Vorschlag", "Suggestion");
    public string LengthCol => T("Länge", "Length");
    public string AndInside(int n) => T($"+ {Format.Number(n, false)} Elemente darin", $"+ {Format.Number(n, true)} items inside");
    public string Disclaimer => T("Pathrova prüft und zeigt an. Es garantiert nicht, dass eine Synchronisierung oder Migration gelingt; Einstellungen deiner Organisation können abweichen.",
                                  "Pathrova checks and reports. It does not guarantee that a sync or migration will succeed; your organization's settings may differ.");
    public string Severity(Severity s) => s switch { Core.Severity.Error => T("Fehler", "Error"), Core.Severity.Warning => T("Warnung", "Warning"), _ => T("Hinweis", "Note") };

    public string Kind(IssueKind k) => k switch
    {
        IssueKind.PathTooLong => T("Pfad zu lang (Cloud, 400)", "Path too long (cloud, 400)"),
        IssueKind.LocalPathTooLong => T("Pfad zu lang (Sync, 520)", "Path too long (sync, 520)"),
        IssueKind.NameTooLong => T("Name zu lang (255)", "Name too long (255)"),
        IssueKind.InvalidCharacter => T("Ungültige Zeichen", "Invalid characters"),
        IssueKind.ReservedName => T("Reservierter Name", "Reserved name"),
        IssueKind.LeadingTrailingSpace => T("Leerzeichen am Anfang oder Ende", "Leading or trailing space"),
        IssueKind.BlockedType => T("Gesperrter Dateityp", "Blocked file type"),
        IssueKind.FileTooLarge => T("Datei größer als 250 GB", "File larger than 250 GB"),
        IssueKind.OfficePathLong => T("Pfad zu lang für Office (256)", "Path too long for Office (256)"),
        IssueKind.TrailingDot => T("Punkt am Ende", "Trailing dot"),
        IssueKind.RestrictedCharacter => T("# oder % im Namen", "# or % in name"),
        IssueKind.OfficeCharacter => T("; oder & im Namen", "; or & in name"),
        IssueKind.LinkNotSupported => T("Verknüpfung (Symlink/Junction)", "Link (symlink/junction)"),
        IssueKind.NotSynced => T("Wird nicht synchronisiert", "Not synced"),
        IssueKind.AccessDenied => T("Kein Zugriff", "Access denied"),
        IssueKind.TooManyItems => T("Mehr als 300.000 Elemente", "More than 300,000 items"),
        _ => k.ToString(),
    };

    public string Explain(IssueKind k) => k switch
    {
        IssueKind.PathTooLong => T("Der Pfad in der Bibliothek ist länger als 400 Zeichen. Ordner kürzen oder Ebenen zusammenlegen.", "The path in the library exceeds 400 characters. Shorten folders or flatten levels."),
        IssueKind.LocalPathTooLong => T("Mit dem lokalen Sync-Ordner länger als 520 Zeichen – die Synchronisierung bricht ab.", "Including the local sync folder longer than 520 characters – sync fails."),
        IssueKind.NameTooLong => T("Datei- und Ordnernamen dürfen höchstens 255 Zeichen haben.", "File and folder names may have at most 255 characters."),
        IssueKind.InvalidCharacter => T("Nicht erlaubt: \" * : < > ? / \\ | und Steuerzeichen.", "Not allowed: \" * : < > ? / \\ | and control characters."),
        IssueKind.ReservedName => T("Gesperrt: CON, PRN, AUX, NUL, COM0–9, LPT0–9, .lock, ~$…, _vti_, „forms“ auf oberster Ebene.", "Blocked: CON, PRN, AUX, NUL, COM0–9, LPT0–9, .lock, ~$…, _vti_, “forms” at the top level."),
        IssueKind.LeadingTrailingSpace => T("Namen dürfen nicht mit einem Leerzeichen beginnen oder enden.", "Names must not start or end with a space."),
        IssueKind.BlockedType => T("Dieser Dateityp steht auf deiner Sperrliste.", "This file type is on your block list."),
        IssueKind.FileTooLarge => T("Einzelne Dateien dürfen höchstens 250 GB groß sein.", "Single files may be at most 250 GB."),
        IssueKind.OfficePathLong => T("Word, Excel und PowerPoint öffnen Dateien mit mehr als 256 Zeichen im lokalen Pfad nicht.", "Word, Excel and PowerPoint cannot open files whose local path exceeds 256 characters."),
        IssueKind.TrailingDot => T("Windows entfernt Punkte am Namensende; das führt zu Konflikten.", "Windows strips trailing dots from names, which causes conflicts."),
        IssueKind.RestrictedCharacter => T("Manche Organisationen lassen # und % nicht zu.", "Some organizations do not allow # and %."),
        IssueKind.OfficeCharacter => T("; und & stören das Speichern aus älteren Office-Versionen.", "; and & interfere with saving from older Office versions."),
        IssueKind.LinkNotSupported => T("Symbolische Links und Junctions werden nicht synchronisiert; der Inhalt wurde nicht geprüft.", "Symbolic links and junctions are not synced; their content was not checked."),
        IssueKind.NotSynced => T("Systemdateien wie desktop.ini, .DS_Store und .tmp werden ausgelassen.", "System files such as desktop.ini, .DS_Store and .tmp are skipped."),
        IssueKind.AccessDenied => T("Dieser Ordner konnte nicht gelesen werden und wurde nicht geprüft.", "This folder could not be read and was not checked."),
        IssueKind.TooManyItems => T("Microsoft empfiehlt höchstens 300.000 Elemente je Synchronisierung.", "Microsoft recommends at most 300,000 items per sync."),
        _ => "",
    };

    // Umbenennen
    public string RenameTitle => T("Umbenennen – Vorschau", "Rename – preview");
    public string RenameIntro(int n) => T($"{n} Vorschläge. Nichts wird geändert, bevor du bestätigst. Jede Änderung kommt ins Rückgängig-Protokoll.",
                                          $"{n} suggestions. Nothing is changed until you confirm. Every change is written to the undo log.");
    public string OldName => T("Bisher", "Current");
    public string NewName => T("Neu", "New");
    public string RenameNow(int n) => T($"{n} umbenennen", $"Rename {n}");
    public string SelectAll => T("Alle", "All");
    public string SelectNone => T("Keine", "None");
    public string Close => T("Schließen", "Close");
    public string ConfirmRename(int n) => T($"{n} Elemente jetzt umbenennen? Programme und Verknüpfungen, die auf die alten Namen zeigen, finden sie danach nicht mehr.",
                                            $"Rename {n} items now? Programs and shortcuts pointing to the old names will no longer find them.");
    public string Yes => T("Umbenennen", "Rename");
    public string No => T("Abbrechen", "Cancel");
    public string Renamed(int ok, int failed) => T($"{ok} umbenannt{(failed > 0 ? $", {failed} fehlgeschlagen" : "")}. Neu prüfen …", $"{ok} renamed{(failed > 0 ? $", {failed} failed" : "")}. Checking again …");
    public string ConfirmUndo(int n) => T($"{n} Umbenennungen rückgängig machen?", $"Undo {n} renames?");
    public string UndoDo => T("Rückgängig machen", "Undo");
    public string Undone(int ok, int failed) => T($"{ok} zurückbenannt{(failed > 0 ? $", {failed} fehlgeschlagen" : "")}.", $"{ok} restored{(failed > 0 ? $", {failed} failed" : "")}.");
    public string NothingToRename => T("Für die gefundenen Probleme gibt es keine sicheren automatischen Vorschläge. Zu lange Ordnerpfade bitte von Hand kürzen.",
                                       "There are no safe automatic suggestions for these issues. Please shorten long folder paths by hand.");
    public string Saved(string path) => T($"Gespeichert: {path}", $"Saved: {path}");
    public string NotFound => T("Ordner nicht gefunden.", "Folder not found.");

    // Bericht
    public string ReportTitle => T("Pfadprüfung vor der Cloud-Migration", "Path check before cloud migration");
    public string Created => T("Erstellt", "Created");
    public string Target => T("Ziel", "Target");
    public string Summary => T("Zusammenfassung", "Summary");
    public string Counting(bool encoded) => encoded ? T("URL-kodiert gezählt", "counted URL-encoded") : T("als Zeichen gezählt", "counted as characters");
    public string Truncated(int shown, int total) => T($"Gezeigt: {shown} von {total} Einträgen. Die vollständige Liste steht im CSV-Export.", $"Showing {shown} of {total} entries. The full list is in the CSV export.");
    public string Page(int n, int of) => T($"Seite {n} von {of}", $"Page {n} of {of}");
}
