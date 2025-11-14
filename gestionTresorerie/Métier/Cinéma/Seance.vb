Imports System.Data.SqlClient

Public Class Seance
    Public Property IdSeance As Integer
    Public Property IdFilm As Integer
    Public Property DateHeureDebut As Date
    Public Property TarifBase As Decimal
    Public Property Langue As String
    Public Property Format As String
    ' --- Les nouvelles propriétés demandées ---
    Public Property NbEntreesAdultes As Integer
    Public Property NbEntreesEnfants As Integer
    Public Property NbEntreesGroupeEnfants As Integer

    ' --- Constructeur principal ---
    Public Sub New(idFilm As Integer,
                   dateHeureDebut As Date,
                   Optional tarifBase As Decimal = 0D,
                   Optional langue As String = Nothing,
                   Optional format As String = Nothing,
                   Optional nbAdultes As Integer = 0,
                   Optional nbEnfants As Integer = 0,
                   Optional nbGroupeEnfants As Integer = 0)

        ' ==== VALIDATIONS ====

        If idFilm <= 0 Then
            Throw New ArgumentException("IdFilm doit être un entier positif.", NameOf(idFilm))
        End If

        If dateHeureDebut.Date = Date.MinValue Then
            Throw New ArgumentException("La date de début est obligatoire et doit être valide.", NameOf(dateHeureDebut))
        End If

        If tarifBase < 0 Then
            Throw New ArgumentException("Le tarif de base ne peut pas être négatif.", NameOf(tarifBase))
        End If

        If nbAdultes < 0 Then
            Throw New ArgumentException("NbEntreesAdultes ne peut pas être négatif.", NameOf(nbAdultes))
        End If
        If nbEnfants < 0 Then
            Throw New ArgumentException("NbEntreesEnfants ne peut pas être négatif.", NameOf(nbEnfants))
        End If
        If nbGroupeEnfants < 0 Then
            Throw New ArgumentException("NbEntreesGroupeEnfants ne peut pas être négatif.", NameOf(nbGroupeEnfants))
        End If

        ' ==== AFFECTATIONS ====
        Me.IdFilm = idFilm
        Me.DateHeureDebut = dateHeureDebut
        Me.TarifBase = tarifBase
        Me.Langue = langue
        Me.Format = format

        Me.NbEntreesAdultes = nbAdultes
        Me.NbEntreesEnfants = nbEnfants
        Me.NbEntreesGroupeEnfants = nbGroupeEnfants
    End Sub


    ' --- Constructeur vide si nécessaire par EF, Sérialisation, etc. ---
    Public Sub New()
    End Sub
    Public Shared Function GetByFilm(idFilm As Integer) As List(Of Seance)
        Dim result As New List(Of Seance)
        ' ✅ 2. Préparation des paramètres SQL
        Dim parametres As New Dictionary(Of String, Object) From {
        {"@IdFilm", idFilm}
    }
        Using cmd = SqlCommandBuilder.CreateSqlCommand("selSeanceIdFilm", parametres)
            cmd.Parameters.AddWithValue("@IdFilm", idFilm)
            Using rdr = cmd.ExecuteReader()
                While rdr.Read()
                    result.Add(New Seance With {
                            .IdSeance = rdr("IdSeance"),
                            .IdFilm = rdr("IdFilm"),
                            .DateHeureDebut = rdr("DateHeureDebut"),
                            .TarifBase = rdr("TarifBase"),
                            .Langue = rdr("Langue").ToString(),
                            .Format = rdr("Format").ToString()
                        })
                End While
            End Using
        End Using
        Return result
    End Function

    'Public Sub Save()
    '    Using cn = GetConnection()
    '        cn.Open()
    '        Dim sql As String = If(IdSeance = 0,
    '            "INSERT INTO Seances (IdFilm, DateHeureDebut, TarifBase, Langue, Format)
    '             VALUES (@IdFilm, @DateHeure, @Tarif, @Langue, @Format);
    '             SELECT SCOPE_IDENTITY();",
    '            "UPDATE Seances SET IdFilm=@IdFilm, DateHeureDebut=@DateHeure, TarifBase=@Tarif, Langue=@Langue, Format=@Format WHERE IdSeance=@IdSeance")

    '        Using cmd As New SqlCommand(sql, cn)
    '            cmd.Parameters.AddWithValue("@IdSeance", IdSeance)
    '            cmd.Parameters.AddWithValue("@IdFilm", IdFilm)
    '            cmd.Parameters.AddWithValue("@DateHeure", DateHeureDebut)
    '            cmd.Parameters.AddWithValue("@Tarif", TarifBase)
    '            cmd.Parameters.AddWithValue("@Langue", Langue)
    '            cmd.Parameters.AddWithValue("@Format", Format)

    '            If IdSeance = 0 Then
    '                IdSeance = Convert.ToInt32(cmd.ExecuteScalar())
    '            Else
    '                cmd.ExecuteNonQuery()
    '            End If
    '        End Using
    '    End Using
    'End Sub
    Public Sub InsererSeance(seance As Seance)

        ' ==== VALIDATIONS ====

        If seance.IdFilm <= 0 Then
            Throw New ArgumentException("L'identifiant du film (IdFilm) doit être valide.")
        End If

        If seance.DateHeureDebut = DateTime.MinValue Then
            Throw New ArgumentException("La date et l'heure de début sont obligatoires.")
        End If

        If seance.TarifBase < 0 Then
            Throw New ArgumentException("Le tarif de base doit être >= 0.")
        End If

        If seance.NbEntreesAdultes < 0 OrElse
           seance.NbEntreesEnfants < 0 OrElse
           seance.NbEntreesGroupeEnfants < 0 Then

            Throw New ArgumentException("Les nombres d’entrées ne peuvent pas être négatifs.")
        End If

        ' ==== PRÉPARATION PARAMÈTRES SQL ====

        Dim parametres As New Dictionary(Of String, Object) From {
            {"@IdFilm", seance.IdFilm},
            {"@DateHeureDebut", seance.DateHeureDebut},
            {"@TarifBase", seance.TarifBase},
            {"@Langue", If(seance.Langue, DBNull.Value)},
            {"@Format", If(seance.Format, DBNull.Value)},
            {"@NbEntreesAdultes", seance.NbEntreesAdultes},
            {"@NbEntreesEnfants", seance.NbEntreesEnfants},
            {"@NbEntreesGroupeEnfants", seance.NbEntreesGroupeEnfants}
        }

        ' ==== EXÉCUTION ====

        Using cmd = SqlCommandBuilder.CreateSqlCommand("insertSeance", parametres)
            Dim lignes = cmd.ExecuteNonQuery()

            If lignes = 0 Then
                Throw New Exception("Aucune ligne insérée dans Seances.")
            End If

            Logger.INFO($"Séance insérée : film #{seance.IdFilm}, {seance.DateHeureDebut:dd/MM/yyyy HH:mm}")
        End Using

    End Sub

    Public Sub Delete()
        If IdSeance = 0 Then Exit Sub
        Dim parametres As New Dictionary(Of String, Object) From {
                        {"@Id", IdSeance}
                    }
        Using cmd = SqlCommandBuilder.CreateSqlCommand("delSeance", parametres)
            cmd.Parameters.AddWithValue("@Id", IdSeance)
            cmd.ExecuteNonQuery()
        End Using
    End Sub
End Class
