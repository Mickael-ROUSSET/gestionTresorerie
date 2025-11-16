Imports DocumentFormat.OpenXml.Spreadsheet
Imports ClosedXML.Excel
Imports System.IO
Imports System.Data.SqlClient
Imports OpenCvSharp.Aruco

Public Class EntreeCinema

    Public Property DateEntree As Date
    Public Property TitreFilm As String
    Public Property NbAdultes As Integer
    Public Property NbEnfants As Integer
    Public Property NbGroupeEnfants As Integer
    Public Property Payant As Boolean

    ' --------------------------------------------------------------------
    ' IMPORT PRINCIPAL
    ' --------------------------------------------------------------------
    Public Function ImporterEntreesDepuisExcel() As List(Of EntreeCinema)
        Dim entrees As New List(Of EntreeCinema)

        Try
            Dim cheminFichier As String = SelectionnerFichierExcel()
            If String.IsNullOrEmpty(cheminFichier) OrElse Not File.Exists(cheminFichier) Then
                Logger.ERR($"❌ Fichier Excel introuvable ou non sélectionné : {cheminFichier}")
                Return entrees
            End If

            If Not File.Exists(cheminFichier) Then
                Logger.ERR($"❌ Fichier Excel introuvable : {cheminFichier}")
                Return entrees
            End If

            Logger.INFO($"📘 Début import des entrées cinéma depuis : {cheminFichier}")

            Using wb As New XLWorkbook(cheminFichier)
                Dim ws = wb.Worksheets.First()
                Dim premiereLigne As Boolean = True

                For Each row In ws.RowsUsed()

                    ' ➤ Ignore l’en-tête
                    If premiereLigne Then
                        premiereLigne = False
                        Continue For
                    End If

                    Try
                        Dim entree As New EntreeCinema With {
                                    .DateEntree = row.Cell("A").GetDateTime(),
                                    .TitreFilm = row.Cell("C").GetString(),
                                    .NbAdultes = If(row.Cell("D").IsEmpty(), 0, row.Cell("D").GetValue(Of Integer)()),
                                    .NbEnfants = If(row.Cell("E").IsEmpty(), 0, row.Cell("E").GetValue(Of Integer)()),
                                    .NbGroupeEnfants = If(row.Cell("F").IsEmpty(), 0, row.Cell("F").GetValue(Of Integer)()),
                                    .Payant = If(row.Cell("J").IsEmpty(), False, row.Cell("J").GetValue(Of Boolean)())
                                }


                        ' Log de la ligne lue
                        Logger.INFO($"Ligne importée : {entree.DateEntree:dd/MM/yyyy} - {entree.TitreFilm} - Adultes:{entree.NbAdultes}, Enfants:{entree.NbEnfants}, Groupe:{entree.NbGroupeEnfants}, Payant:{entree.Payant}")

                        entrees.Add(entree)

                        ' -----------------------------------------------------------------
                        ' INSERTION SQL AUTOMATIQUE
                        ' -----------------------------------------------------------------
                        InsererFilmEtSeance(entree)

                    Catch ex As Exception
                        Logger.ERR($"⚠️ Erreur sur la ligne {row.RowNumber()} : {ex.Message}")
                    End Try
                Next
            End Using

            Logger.INFO($"✅ Import terminé ({entrees.Count} lignes importées).")

        Catch ex As Exception
            Logger.ERR($"❌ Erreur dans ImporterEntreesDepuisExcel : {ex.Message}")
        End Try

        Return entrees
    End Function

    ''' <summary>
    ''' Ouvre une fenêtre pour sélectionner un fichier Excel et retourne le chemin.
    ''' </summary>
    ''' <returns>Chemin complet du fichier Excel choisi, ou Nothing si annulation</returns>
    Private Function SelectionnerFichierExcel() As String
        Using ofd As New OpenFileDialog()
            ofd.Filter = "Fichiers Excel|*.xlsx;*.xls"
            ofd.Title = "Sélectionner le fichier des entrées cinéma"
            ofd.InitialDirectory = LectureProprietes.GetVariable("repRacineAgumaaa")

            If ofd.ShowDialog() = DialogResult.OK Then
                Return ofd.FileName
            Else
                Logger.INFO("📌 Import annulé par l'utilisateur.")
                Return Nothing
            End If
        End Using
    End Function

    ' --------------------------------------------------------------------
    ' INSERTION AUTOMATIQUE DES FILMS + SEANCES
    ' --------------------------------------------------------------------
    Private Sub InsererFilmEtSeance(entree As EntreeCinema)
        Try
            Dim idFilm = ObtenirIdFilm(entree.TitreFilm)

            If idFilm = -1 Then
                idFilm = InsererFilm(New Film(entree.TitreFilm))
            End If

            Dim idSeance = ObtenirIdSeance(idFilm, entree.DateEntree)

            If idSeance = -1 Then
                idSeance = InsererSeance(New Seance(idFilm,
                                                    entree.DateEntree,
                                                    0D,
                                                    Nothing,
                                                    Nothing,
                                                    entree.NbAdultes,
                                                    entree.NbEnfants,
                                                    entree.NbGroupeEnfants
                                                    )
                                         )
            End If
        Catch ex As Exception
            Logger.ERR($"❌ Erreur lors de l’insertion TitreFilm/Séance : {ex.Message}")
        End Try
    End Sub


    ' --------------------------------------------------------------------
    ' FILMS
    ' --------------------------------------------------------------------
    Private Function ObtenirIdFilm(titre As String) As Integer
        Try
            Dim parametres As New Dictionary(Of String, Object) From {
                        {"@titre", titre}
                    }
            Dim cmd = SqlCommandBuilder.CreateSqlCommand(Constantes.cinemaDB, "selFilmParTitre", parametres)
            Dim result = cmd.ExecuteScalar()

            If result IsNot Nothing AndAlso Not DBNull.Value.Equals(result) Then
                Logger.INFO($"TitreFilm trouvé : {titre} (Id={result})")
                Return Convert.ToInt32(result)
            End If

        Catch ex As Exception
            Logger.ERR($"Erreur ObtenirIdFilm : {ex.Message}")
        End Try

        Return -1
    End Function

    Public Function InsererFilm(film As Film) As Integer
        ' ✅ Validation
        If String.IsNullOrWhiteSpace(film.Titre) Then
            Throw New ArgumentException("Le titre du film est obligatoire.", NameOf(film.Titre))
        End If

        If film.DureeMinutes < 0 Then
            Throw New ArgumentException("La durée du film doit être positive.", NameOf(film.DureeMinutes))
        End If

        ' ✅ Préparation des paramètres SQL
        Dim parametres As New Dictionary(Of String, Object) From {
        {"@Titre", film.Titre},
        {"@DureeMinutes", film.DureeMinutes},
        {"@Genre", If(film.Genre, DBNull.Value)},
        {"@Realisateur", If(film.Realisateur, DBNull.Value)},
        {"@DateSortie", If(film.DateSortie, DBNull.Value)},
        {"@Synopsis", If(film.Synopsis, DBNull.Value)},
        {"@AgeMinimum", If(film.AgeMinimum, DBNull.Value)},
        {"@AfficheUrl", If(film.AfficheUrl, DBNull.Value)}
    }

        ' ✅ Exécution de la commande
        Using cmd = SqlCommandBuilder.CreateSqlCommand(Constantes.cinemaDB, "insertFilm", parametres)
            ' ExecuteScalar retourne le SCOPE_IDENTITY()
            Dim idObj = cmd.ExecuteScalar()
            Dim id As Integer = Convert.ToInt32(idObj)
            Logger.INFO($"Film inséré : {film.Titre} (IdFilm = {id})")
            Return id
        End Using
    End Function



    ' --------------------------------------------------------------------
    ' SEANCES
    ' --------------------------------------------------------------------
    Private Function ObtenirIdSeance(idFilm As Integer, dateDebut As DateTime) As Integer
        Try
            Dim parametres As New Dictionary(Of String, Object) From {
                        {"@idFilm", idFilm},
                        {"@dateDebut", dateDebut}
                    }
            Dim cmd = SqlCommandBuilder.CreateSqlCommand(Constantes.cinemaDB, "selSeanceIdFilm", parametres)
            Dim result = cmd.ExecuteScalar()

            If result IsNot Nothing AndAlso Not DBNull.Value.Equals(result) Then
                Logger.INFO($"Séance déjà existante pour film={idFilm} à {dateDebut}.")
                Return Convert.ToInt32(result)
            End If

        Catch ex As Exception
            Logger.ERR($"Erreur ObtenirIdSeance : {ex.Message}")
        End Try

        Return -1
    End Function

    Private Function InsererSeance(seance As Seance) As Integer
        Try
            Logger.INFO($"📌 Insertion séance film={seance.IdFilm} - {seance.DateHeureDebut}")

            Dim parametres As New Dictionary(Of String, Object) From {
                {"@idFilm", seance.IdFilm},
                {"@DateHeureDebut", seance.DateHeureDebut},
                {"@TarifBase", 0D},
                {"@langue", DBNull.Value},
                {"@format", DBNull.Value},
                {"@nbEntreesAdultes", seance.NbEntreesAdultes},
                {"@nbEntreesEnfants", seance.NbEntreesEnfants},
                {"@nbEntreesGroupeEnfants", seance.NbEntreesGroupeEnfants}
            }

            Dim id = SqlCommandBuilder.CreateSqlCommand(Constantes.cinemaDB, "insertSeance", parametres).ExecuteScalar()

            Return Convert.ToInt32(id)

        Catch ex As Exception
            Logger.ERR($"Erreur InsererSeance : {ex.Message}")
            Return -1
        End Try
    End Function

End Class
