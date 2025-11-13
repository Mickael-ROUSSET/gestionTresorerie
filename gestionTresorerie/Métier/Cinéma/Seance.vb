Imports System.Data.SqlClient

Public Class Seance
    Public Property IdSeance As Integer
    Public Property IdFilm As Integer
    Public Property DateHeureDebut As Date
    Public Property TarifBase As Decimal
    Public Property Langue As String
    Public Property Format As String

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
        ' ✅ 1. Validation des données
        If seance.IdFilm <= 0 Then
            Throw New ArgumentException("L'identifiant du film (IdFilm) doit être valide.")
        End If

        If seance.DateHeureDebut = DateTime.MinValue Then
            Throw New ArgumentException("La date et l'heure de début sont obligatoires.")
        End If

        If seance.TarifBase < 0 Then
            Throw New ArgumentException("Le tarif de base doit être supérieur ou égal à 0.")
        End If

        ' (Optionnel) Si tu veux interdire les séances dans le passé :
        If seance.DateHeureDebut < DateTime.Now Then
            Logger.WARN($"Séance insérée dans le passé : {seance.DateHeureDebut}.")
        End If

        ' ✅ 2. Préparation des paramètres SQL
        Dim parametres As New Dictionary(Of String, Object) From {
        {"@IdFilm", seance.IdFilm},
        {"@DateHeureDebut", seance.DateHeureDebut},
        {"@TarifBase", seance.TarifBase},
        {"@Langue", If(seance.Langue, DBNull.Value)},
        {"@Format", If(seance.Format, DBNull.Value)}
    }

        ' ✅ 3. Exécution avec SqlCommandBuilder
        Using cmd = SqlCommandBuilder.CreateSqlCommand("insertSeance", parametres)
            Dim lignesAffectees = cmd.ExecuteNonQuery()

            If lignesAffectees = 0 Then
                Throw New Exception("Aucune ligne insérée dans Seances : l’opération a échoué.")
            End If

            Logger.INFO($"Séance insérée pour le film #{seance.IdFilm} - {seance.DateHeureDebut:dd/MM/yyyy HH:mm}")
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
