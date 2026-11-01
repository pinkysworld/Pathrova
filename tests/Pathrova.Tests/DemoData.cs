namespace Pathrova.Tests;

/// <summary>Realistische Firmenablage mit typischen Problemen (auf macOS/Linux anlegbar, unter Windows teils nicht).</summary>
public static class DemoData
{
    public static string Create(string root, bool english)
    {
        var top = Directory.CreateDirectory(Path.Combine(root, english ? "Company share" : "Firmenablage")).FullName;
        void F(string rel, string content = "x")
        {
            var p = Path.Combine(top, rel);
            Directory.CreateDirectory(Path.GetDirectoryName(p)!);
            File.WriteAllText(p, content);
        }
        string projects = english ? "Projects/2024/Customer Müller & Sons – Office building renovation/Planning documents and approvals from the building authority"
                                  : "Projekte/2024/Kunde Müller & Söhne – Sanierung Bürogebäude Hauptstraße/Planungsunterlagen und Genehmigungen vom Bauamt";
        string deep = projects + (english ? "/Revised drawings after the second on-site inspection appointment with the structural engineer/Final versions approved"
                                          : "/Überarbeitete Zeichnungen nach zweitem Ortstermin mit Statiker und Brandschutz/Endgültige freigegebene Fassungen");
        F(deep + (english ? "/Ground floor plan – revision 7 – approved by client and authority.pdf" : "/Grundriss Erdgeschoss – Revision 7 – freigegeben von Bauherr und Behörde.pdf"));
        F(deep + (english ? "/Sections A-A and B-B – revision 4.pdf" : "/Schnitte A-A und B-B – Revision 4.pdf"));
        F(projects + (english ? "/Quote: final?.docx" : "/Angebot: final?.docx"));
        F(english ? "Accounting/Invoices 2024/Invoice #1043.pdf" : "Buchhaltung/Rechnungen 2024/Rechnung #1043.pdf");
        F(english ? "Accounting/Invoices 2024/Invoice 1044 .pdf" : "Buchhaltung/Rechnungen 2024/Rechnung 1044 .pdf");
        F(english ? "Accounting/ Tax advisor/Annual statement 2023.xlsx" : "Buchhaltung/ Steuerberater/Jahresabschluss 2023.xlsx");
        F(english ? "Staff/CON.txt" : "Personal/CON.txt");
        F(english ? "Staff/Holiday plan <draft>.xlsx" : "Personal/Urlaubsplan <Entwurf>.xlsx");
        F(english ? "Staff/~$Holiday plan.xlsx" : "Personal/~$Urlaubsplan.xlsx");
        F(english ? "Marketing/Logo|old.png" : "Marketing/Logo|alt.png");
        F(english ? "Marketing/Drafts./Flyer spring.pdf" : "Marketing/Entwürfe./Flyer Frühjahr.pdf");
        F(english ? "Marketing/desktop.ini" : "Marketing/desktop.ini");
        F(english ? "Marketing/Campaign 50% off.pptx" : "Marketing/Aktion 50% Rabatt.pptx");
        F(english ? "IT/_vti_cnf/settings.xml" : "IT/_vti_cnf/einstellungen.xml");
        F(english ? "IT/Setup tools/install.exe" : "IT/Setup-Tools/install.exe");
        F("forms/" + (english ? "Contact form.docx" : "Kontaktformular.docx"));
        F(english ? "Archive/Scan_2019_03.tmp" : "Archiv/Scan_2019_03.tmp");
        for (int i = 1; i <= 24; i++) F((english ? "Archive/Letters/Letter " : "Archiv/Briefe/Brief ") + i.ToString("00") + ".pdf");
        return top;
    }
}
