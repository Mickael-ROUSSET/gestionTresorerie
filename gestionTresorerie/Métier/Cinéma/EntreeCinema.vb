Imports DocumentFormat.OpenXml.Spreadsheet
Imports ClosedXML.Excel
Imports System.IO
Imports OpenCvSharp.Aruco

Public Class EntreeCinema

    Public Property DateEntree As Date
    Public Property TitreFilm As String
    Public Property NbAdultes As Integer
    Public Property NbEnfants As Integer
    Public Property NbGroupeEnfants As Integer
    Public Property Payant As Boolean

    Private Shared Function CreateFilmRepository() As FilmRepository
        Dim connectionString As String =
        ConnexionDB.GetInstance(Constantes.DataBases.Cinema).
                    GetConnexion(Constantes.DataBases.Cinema).
                    ConnectionString

        Dim factory As New AgumaaaConnectionFactory(connectionString)
        Dim provider As ISqlTextProvider = New LegacySqlTextProvider()
        Dim executor As ISqlExecutor = New SqlExecutor(factory, provider)

        Return New FilmRepository(executor)
    End Function

    Private Shared Function CreateSeanceRepository() As SeanceRepository
        Dim connectionString As String =
        ConnexionDB.GetInstance(Constantes.DataBases.Cinema).
                    GetConnexion(Constantes.DataBases.Cinema).
                    ConnectionString

        Dim factory As New AgumaaaConnectionFactory(connectionString)
        Dim provider As ISqlTextProvider = New LegacySqlTextProvider()
        Dim executor As ISqlExecutor = New SqlExecutor(factory, provider)

        Return New SeanceRepository(executor)
    End Function

    ' --------------------------------------------------------------------
    ' IMPORT PRINCIPAL
    ' --------------------------------------------------------------------
    Public Shared Function ImporterEntreesDepuisExcel() As List(Of EntreeCinema)
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
                                    .Payant = Not row.Cell("J").IsEmpty() AndAlso row.Cell("J").GetValue(Of Boolean)()
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
    Private Shared Function SelectionnerFichierExcel() As String
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
    Private Shared Sub InsererFilmEtSeance(entree As EntreeCinema)
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
    Private Shared Function ObtenirIdFilm(titre As String) As Integer
        Try
            Dim id As Integer = CreateFilmRepository().LireIdParTitre(titre)

            If id <> -1 Then
                Logger.INFO($"Film trouvé : {titre} (Id={id})")
            End If

            Return id

        Catch ex As Exception
            Logger.ERR($"Erreur ObtenirIdFilm : {ex.Message}")
            Return -1
        End Try
    End Function

    Public Shared Function InsererFilm(film As Film) As Integer
        If film Is Nothing Then
            Throw New ArgumentNullException(NameOf(film), "L'objet film ne peut pas être null.")
        End If

        If String.IsNullOrWhiteSpace(film.Titre) Then
            Throw New ArgumentException("Le titre du film est obligatoire.", NameOf(film))
        End If

        Try
            Dim id As Integer = CreateFilmRepository().InsererEtRetournerId(film)
            Logger.INFO($"Film inséré : {film.Titre} (IdFilm = {id})")
            Return id

        Catch ex As Exception
            Logger.ERR($"Erreur InsererFilm : {ex.Message}")
            Return -1
        End Try
    End Function

    ' --------------------------------------------------------------------
    ' SEANCES
    ' --------------------------------------------------------------------
    Private Shared Function ObtenirIdSeance(idFilm As Integer, dateDebut As DateTime) As Integer
        Try
            Dim id As Integer =
            CreateSeanceRepository().LireIdParFilmEtDate(idFilm, dateDebut)

            If id <> -1 Then
                Logger.INFO($"Séance déjà existante pour film={idFilm} à {dateDebut}.")
            End If

            Return id

        Catch ex As Exception
            Logger.ERR($"Erreur ObtenirIdSeance : {ex.Message}")
            Return -1
        End Try
    End Function

    Private Shared Function InsererSeance(seance As Seance) As Integer
        Try
            Logger.INFO($"Insertion séance film={seance.IdFilm} - {seance.DateHeureDebut}")

            Dim id As Integer =
            CreateSeanceRepository().InsererEtRetournerId(seance)

            Return id

        Catch ex As Exception
            Logger.ERR($"Erreur InsererSeance : {ex.Message}")
            Return -1
        End Try
    End Function

End Class
