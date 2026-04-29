
Public Class Film
    Public Property IdFilm As Integer
    Public Property Titre As String
    Public Property DureeMinutes As Integer
    Public Property Genre As String
    Public Property Realisateur As String
    Public Property DateSortie As Date?
    Public Property Synopsis As String
    Public Property AgeMinimum As Integer?
    Public Property AfficheUrl As String

    ' --- Constructeur principal ---
    Public Sub New(titre As String,
                   Optional dureeMinutes As Integer = 0,
                   Optional genre As String = Nothing,
                   Optional realisateur As String = Nothing,
                   Optional dateSortie As Date? = Nothing,
                   Optional synopsis As String = Nothing,
                   Optional ageMinimum As Integer? = Nothing,
                   Optional afficheUrl As String = Nothing)

        ' ===== VALIDATIONS =====

        If String.IsNullOrWhiteSpace(titre) Then
            Throw New ArgumentException("Le titre du film est obligatoire.", NameOf(titre))
        End If

        If dureeMinutes < 0 Then
            Throw New ArgumentException("La durée du film ne peut pas être négative.", NameOf(dureeMinutes))
        End If

        If ageMinimum.HasValue AndAlso ageMinimum.Value < 0 Then
            Throw New ArgumentException("L'âge minimum ne peut pas être négatif.", NameOf(ageMinimum))
        End If

        If dateSortie.HasValue AndAlso dateSortie.Value = Date.MinValue Then
            Throw New ArgumentException("La date de sortie fournie n'est pas valide.", NameOf(dateSortie))
        End If

        ' ===== AFFECTATIONS =====

        Me.Titre = titre
        Me.DureeMinutes = dureeMinutes
        Me.Genre = genre
        Me.Realisateur = realisateur
        Me.DateSortie = dateSortie
        Me.Synopsis = synopsis
        Me.AgeMinimum = ageMinimum
        Me.AfficheUrl = afficheUrl
    End Sub

    ' --- Constructeur vide (nécessaire pour EF / sérialisation) ---
    Public Sub New()
    End Sub
    Private Shared Function CreateRepository() As FilmRepository
        Dim executor As ISqlExecutor = RepositoryFactory.CreateExecutor(Constantes.DataBases.Cinema)

        Return New FilmRepository(executor)
    End Function
    ' ---------- CRUD ----------
    Public Shared Function GetAll() As List(Of Film)
        Return CreateRepository().LireTous()
    End Function
    Public Sub InsererFilm(film As Film)
        If film Is Nothing Then Throw New ArgumentNullException(NameOf(film))

        If String.IsNullOrWhiteSpace(film.Titre) Then
            Throw New ArgumentException("Le titre du film est obligatoire.")
        End If

        If film.DureeMinutes <= 0 Then
            Throw New ArgumentException("La durée du film doit être supérieure à 0.")
        End If

        If film.AgeMinimum.HasValue AndAlso film.AgeMinimum.Value < 0 Then
            Throw New ArgumentException("L'âge minimum doit être positif ou nul.")
        End If

        Dim lignesAffectees As Integer = CreateRepository().Inserer(film)

        If lignesAffectees = 0 Then
            Throw New Exception("Aucune ligne insérée : l’opération a échoué.")
        End If

        Logger.INFO($"TitreFilm inséré : {film.Titre} ({film.DureeMinutes} min)")
    End Sub

    Public Sub Delete()
        If IdFilm = 0 Then Exit Sub

        Dim nb As Integer = CreateRepository().Supprimer(IdFilm)
        Logger.INFO($"Film supprimé IdFilm={IdFilm}, lignes={nb}")
    End Sub
    ' Récupère le titre d’un film à partir de son IdFilm
    Public Function GetTitreById() As String
        Try
            Return CreateRepository().LireTitreParId(Me.IdFilm)
        Catch ex As Exception
            Logger.ERR($"Erreur GetTitreById : {ex.Message}")
            Return "Erreur"
        End Try
    End Function
End Class
