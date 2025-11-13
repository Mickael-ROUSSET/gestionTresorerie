Imports System.Data.SqlClient

Public Class Tarif
    Public Property IdTarif As Integer
    Public Property NomTarif As String
    Public Property ReductionPourcent As Decimal
    Public Property Conditions As String

    Public Shared Function GetAll() As List(Of Tarif)
        Dim result As New List(Of Tarif)
        Using cn = GetConnection()
            cn.Open()
            Using cmd As New SqlCommand("SELECT * FROM Tarifs ORDER BY NomTarif", cn)
                Using rdr = cmd.ExecuteReader()
                    While rdr.Read()
                        result.Add(New Tarif With {
                            .IdTarif = rdr("IdTarif"),
                            .NomTarif = rdr("NomTarif").ToString(),
                            .ReductionPourcent = rdr("ReductionPourcent"),
                            .Conditions = rdr("Conditions").ToString()
                        })
                    End While
                End Using
            End Using
        End Using
        Return result
    End Function

    Public Sub Save()
        Using cn = GetConnection()
            cn.Open()
            Dim sql As String = If(IdTarif = 0,
                "INSERT INTO Tarifs (NomTarif, ReductionPourcent, Conditions)
                 VALUES (@Nom, @Reduc, @Cond); SELECT SCOPE_IDENTITY();",
                "UPDATE Tarifs SET NomTarif=@Nom, ReductionPourcent=@Reduc, Conditions=@Cond WHERE IdTarif=@Id")

            Using cmd As New SqlCommand(sql, cn)
                cmd.Parameters.AddWithValue("@Id", IdTarif)
                cmd.Parameters.AddWithValue("@Nom", NomTarif)
                cmd.Parameters.AddWithValue("@Reduc", ReductionPourcent)
                cmd.Parameters.AddWithValue("@Cond", Conditions)

                If IdTarif = 0 Then
                    IdTarif = Convert.ToInt32(cmd.ExecuteScalar())
                Else
                    cmd.ExecuteNonQuery()
                End If
            End Using
        End Using
    End Sub

    Public Sub Delete()
        If IdTarif = 0 Then Exit Sub
        Using cn = GetConnection()
            cn.Open()
            Using cmd As New SqlCommand("DELETE FROM Tarifs WHERE IdTarif=@Id", cn)
                cmd.Parameters.AddWithValue("@Id", IdTarif)
                cmd.ExecuteNonQuery()
            End Using
        End Using
    End Sub
End Class
