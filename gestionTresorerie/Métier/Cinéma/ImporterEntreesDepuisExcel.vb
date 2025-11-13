Imports DocumentFormat.OpenXml.Spreadsheet
Imports ClosedXML.Excel
Imports System.IO

Public Class EntreeCinema
    Public Property DateEntree As Date
    Public Property Film As String
    Public Property NbAdultes As Integer
    Public Property NbEnfants As Integer
    Public Property NbGroupeEnfants As Integer
    Public Property Payant As Boolean

    Public Function ImporterEntreesDepuisExcel() As List(Of EntreeCinema)
        Dim entrees As New List(Of EntreeCinema)
        Try
            Dim cheminFichier As String = Path.Combine(
                                                LectureProprietes.GetVariable("repRacineAgumaaa"),
                                                LectureProprietes.GetVariable("fichierEntreesCinemaExcel")
                                            )

            If Not File.Exists(cheminFichier) Then
                Logger.ERR($"❌ Fichier Excel introuvable : {cheminFichier}")
                Return entrees
            End If

            Logger.INFO($"📘 Début import des entrées cinéma depuis : {cheminFichier}")

            Using wb As New XLWorkbook(cheminFichier)
                Dim ws = wb.Worksheets.First()
                Dim premiereLigne = True

                For Each row In ws.RowsUsed()
                    If premiereLigne Then
                        premiereLigne = False
                        Continue For ' ignore l'en-tête
                    End If

                    Try
                        Dim entree As New EntreeCinema With {
                        .DateEntree = row.Cell("A").GetDateTime(),
                        .Film = row.Cell("C").GetString(),
                        .NbAdultes = row.Cell("D").GetValue(Of Integer)(),
                        .NbEnfants = row.Cell("E").GetValue(Of Integer)(),
                        .NbGroupeEnfants = row.Cell("F").GetValue(Of Integer)(),
                        .Payant = row.Cell("J").GetValue(Of Boolean)()
                    }

                        ' Log de la ligne lue
                        Logger.INFO($"Ligne importée : {entree.DateEntree:dd/MM/yyyy} - {entree.Film} - Adultes:{entree.NbAdultes}, Enfants:{entree.NbEnfants}, Groupe:{entree.NbGroupeEnfants}, Payant:{entree.Payant}")

                        entrees.Add(entree)

                    Catch ex As Exception
                        Logger.ERR($"⚠️ Erreur sur la ligne {row.RowNumber()} : {ex.Message}")
                    End Try
                Next
            End Using

            Logger.INFO($"✅ Import terminé ({entrees.Count} lignes importées).")

        Catch ex As Exception
            Logger.ERR($"Erreur dans ImporterEntreesDepuisExcel : {ex.Message}")
        End Try

        Return entrees
    End Function

End Class

