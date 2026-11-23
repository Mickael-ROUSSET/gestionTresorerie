Option Strict On
Imports System.Data
Imports System.Globalization
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports NPOI.SS.UserModel
Imports NPOI.SS.Util

Friend Module SuggestionsClassement
    Private Const Gym As String = "G:\Mon Drive\AGUMAAA\Documents\Manifestations récurrentes\Activités\Gym"

    Friend Function EstCotisation(montant As Decimal, jour As Date, Optional dossierGym As String = Gym) As Boolean
        Dim an = If(jour.Month >= 9, jour.Year, jour.Year - 1)
        If montant <= 0 Then Return False
        If an = 2026 AndAlso montant = 155D Then Return True
        Dim saison = an.ToString() & "-" & (an + 1).ToString()
        For Each extension In {".xls", ".xlsx"}
            Dim fichier = Path.Combine(dossierGym, saison, "InscriptionGym" & saison & extension)
            If Not File.Exists(fichier) Then Continue For
            Try
                Using flux = New FileStream(fichier, FileMode.Open, FileAccess.Read, FileShare.ReadWrite Or FileShare.Delete),
                      livre = WorkbookFactory.Create(flux)
                    For Each cle In {"COTISATION_GYM_DOUCE", "COTISATION_GYM_TONIQUE"}
                        Dim plage = livre.GetName(cle)
                        If plage Is Nothing Then Continue For
                        Dim adresse As New CellReference(plage.RefersToFormula)
                        Dim cellule = livre.GetSheet(adresse.SheetName)?.GetRow(adresse.Row)?.GetCell(adresse.Col)
                        If cellule IsNot Nothing AndAlso (cellule.CellType = CellType.Numeric OrElse
                            (cellule.CellType = CellType.Formula AndAlso cellule.CachedFormulaResultType = CellType.Numeric)) AndAlso
                            CDec(cellule.NumericCellValue) = montant Then Return True
                    Next
                End Using
            Catch ex As Exception
                ' Classeur indisponible : laisser la proposition textuelle accessible.
            End Try
        Next
        Return False
    End Function

    Private Function CleLibelle(valeur As String) As String
        Dim texte = If(valeur, "").Normalize(NormalizationForm.FormD).ToLowerInvariant()
        Return Regex.Replace(texte, "[^\p{L}\p{N}]", "")
    End Function

    Friend Function PaiementCinevasion(catalogue As DataTable, beneficiaire As String, emis As Boolean) As DataRow
        If Not emis OrElse CleLibelle(beneficiaire) <> "cinevasion" Then Return Nothing
        Dim lignes = catalogue.AsEnumerable().Where(Function(r) CInt(r("debit")) = 1 AndAlso
            CleLibelle(Convert.ToString(r("categorie"))) = "cotisationsetlicenceversees" AndAlso
            Not r.IsNull("sousCat") AndAlso CleLibelle(Convert.ToString(r("sousCategorie"))) = "cinevasionpaiementmensuel").Take(2).ToArray()
        Return If(lignes.Length = 1, lignes(0), Nothing)
    End Function

    Private Function Mots(libelle As String) As String()
        Dim simple = Regex.Replace(If(libelle, "").Normalize(NormalizationForm.FormD), "\p{M}", "").ToLowerInvariant()
        simple = Regex.Replace(simple, "\bgym(?:nastique)?\b", "gym")
        Return Regex.Matches(simple, "[a-z0-9]+").Cast(Of Match)().Select(Function(m) m.Value).
            Where(Function(m) m.Length >= 3 AndAlso Not {"des","les","pour","aux","revenus","depenses","activites"}.Contains(m)).Distinct().ToArray()
    End Function

    Friend Function SousCategorieProbable(lignes As IEnumerable(Of DataRow), indices As String) As DataRow
        Dim motsCles = Mots(indices)
        Dim scores = lignes.Where(Function(r) Not r.IsNull("sousCat")).Select(Function(r) New With {
            .Ligne = r, .Score = Mots(CStr(r("sousCategorie"))).Intersect(motsCles).Count()}).
            Where(Function(x) x.Score > 0).OrderByDescending(Function(x) x.Score).Take(2).ToArray()
        If scores.Length = 0 OrElse (scores.Length = 2 AndAlso scores(0).Score = scores(1).Score) Then Return Nothing
        Return scores(0).Ligne
    End Function
End Module
