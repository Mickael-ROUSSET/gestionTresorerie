Imports System.Data.SqlClient

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
    ' ---------- CRUD ----------
    Public Shared Function GetAll() As List(Of Film)
        Dim result As New List(Of Film)
        Using cmd = SqlCommandBuilder.CreateSqlCommand(Constantes.cinemaDB, "selTousFilms")
            Using rdr = cmd.ExecuteReader()
                While rdr.Read()
                    result.Add(New Film With {
                            .IdFilm = rdr("IdFilm"),
                            .Titre = rdr("Titre").ToString(),
                            .DureeMinutes = rdr("DureeMinutes"),
                            .Genre = rdr("Genre").ToString(),
                            .Realisateur = rdr("Realisateur").ToString(),
                            .DateSortie = If(IsDBNull(rdr("DateSortie")), Nothing, rdr("DateSortie")),
                            .Synopsis = rdr("Synopsis").ToString(),
                            .AgeMinimum = If(IsDBNull(rdr("AgeMinimum")), Nothing, rdr("AgeMinimum")),
                            .AfficheUrl = rdr("AfficheUrl").ToString()
                        })
                End While
            End Using
        End Using
        Return result
    End Function

    'Public Sub Save()
    '    Using cn = GetConnection()
    '        cn.Open()
    '        Dim sql As String

    '        If IdFilm = 0 Then
    '            sql = "INSERT INTO Films (Titre, DureeMinutes, Genre, Realisateur, DateSortie, Synopsis, AgeMinimum, AfficheUrl)
    '                   VALUES (@Titre, @Duree, @Genre, @Realisateur, @DateSortie, @Synopsis, @AgeMin, @Affiche);
    '                   SELECT SCOPE_IDENTITY();"
    '        Else
    '            sql = "UPDATE Films SET Titre=@Titre, DureeMinutes=@Duree, Genre=@Genre, Realisateur=@Realisateur, 
    '                   DateSortie=@DateSortie, Synopsis=@Synopsis, AgeMinimum=@AgeMin, AfficheUrl=@Affiche WHERE IdFilm=@IdFilm"
    '        End If

    '        Using cmd As New SqlCommand(sql, cn)
    '            cmd.Parameters.AddWithValue("@IdFilm", IdFilm)
    '            cmd.Parameters.AddWithValue("@Titre", Titre)
    '            cmd.Parameters.AddWithValue("@Duree", DureeMinutes)
    '            cmd.Parameters.AddWithValue("@Genre", Genre)
    '            cmd.Parameters.AddWithValue("@Realisateur", Realisateur)
    '            cmd.Parameters.AddWithValue("@DateSortie", If(DateSortie, DBNull.Value))
    '            cmd.Parameters.AddWithValue("@Synopsis", Synopsis)
    '            cmd.Parameters.AddWithValue("@AgeMin", If(AgeMinimum, DBNull.Value))
    '            cmd.Parameters.AddWithValue("@Affiche", AfficheUrl)

    '            If IdFilm = 0 Then
    '                IdFilm = Convert.ToInt32(cmd.ExecuteScalar())
    '            Else
    '                cmd.ExecuteNonQuery()
    '            End If
    '        End Using
    '    End Using
    'End Sub
    Public Sub InsererFilm(film As Film)
        ' ✅ 1. Validation des données
        If String.IsNullOrWhiteSpace(film.Titre) Then
            Throw New ArgumentException("Le titre du film est obligatoire.")
        End If

        If film.DureeMinutes <= 0 Then
            Throw New ArgumentException("La durée du film doit être supérieure à 0.")
        End If

        If film.AgeMinimum < 0 Then
            Throw New ArgumentException("L'âge minimum doit être positif ou nul.")
        End If

        ' ✅ 2. Préparation des paramètres SQL
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

        ' ✅ 3. Exécution de la requête via SqlCommandBuilder
        Using cmd = SqlCommandBuilder.CreateSqlCommand(Constantes.cinemaDB, "insertFilm", parametres)
            Dim lignesAffectees = cmd.ExecuteNonQuery()

            If lignesAffectees = 0 Then
                Throw New Exception("Aucune ligne insérée : l’opération a échoué.")
            End If

            Logger.INFO($"TitreFilm inséré : {film.Titre} ({film.DureeMinutes} min)")
        End Using
    End Sub

    Public Sub Delete()
        If IdFilm = 0 Then Exit Sub
        ' ✅ 1. Préparation des paramètres SQL
        Dim parametres As New Dictionary(Of String, Object) From {
                        {"@Id", IdFilm}
                    }
        Using cmd = SqlCommandBuilder.CreateSqlCommand(Constantes.cinemaDB, "delFilm", parametres)
            cmd.Parameters.AddWithValue("@Id", IdFilm)
            cmd.ExecuteNonQuery()
        End Using
    End Sub
End Class
