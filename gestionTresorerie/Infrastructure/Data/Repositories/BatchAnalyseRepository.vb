Imports System.Data.SqlClient
Imports System.IO
Imports System.Text

Public Class BatchAnalyseRepository

    Private ReadOnly _factory As IConnectionFactory
    Private ReadOnly _sqlTextProvider As ISqlTextProvider

    Public Sub New(factory As IConnectionFactory, sqlTextProvider As ISqlTextProvider)
        If factory Is Nothing Then Throw New ArgumentNullException(NameOf(factory))
        If sqlTextProvider Is Nothing Then Throw New ArgumentNullException(NameOf(sqlTextProvider))

        _factory = factory
        _sqlTextProvider = sqlTextProvider
    End Sub

    Public Function ExtraireDataTable(nomRequete As String,
                                      Optional params As Dictionary(Of String, Object) = Nothing) As DataTable
        Dim dt As New DataTable()
        Dim sql As String = _sqlTextProvider.GetSql(nomRequete)

        Using conn = _factory.CreateConnection()
            Using cmd As New SqlCommand(sql, conn)
                AjouterParametres(cmd, params)

                conn.Open()
                Using rdr As SqlDataReader = cmd.ExecuteReader()
                    dt.Load(rdr)
                End Using
            End Using
        End Using

        Return dt
    End Function

    Public Sub ExporterCsvDepuisRequete(nomRequete As String, cheminFichier As String)
        If String.IsNullOrWhiteSpace(cheminFichier) Then
            Throw New ArgumentException("Le chemin du fichier CSV est obligatoire.", NameOf(cheminFichier))
        End If

        Dim sql As String = _sqlTextProvider.GetSql(nomRequete)

        Using conn = _factory.CreateConnection()
            Using cmd As New SqlCommand(sql, conn)
                conn.Open()

                Using reader As SqlDataReader = cmd.ExecuteReader()
                    Using writer As New StreamWriter(cheminFichier, False, Encoding.UTF8)

                        For i As Integer = 0 To reader.FieldCount - 1
                            writer.Write(reader.GetName(i))
                            If i < reader.FieldCount - 1 Then writer.Write(";")
                        Next
                        writer.WriteLine()

                        While reader.Read()
                            For i As Integer = 0 To reader.FieldCount - 1
                                writer.Write(reader(i).ToString())
                                If i < reader.FieldCount - 1 Then writer.Write(";")
                            Next
                            writer.WriteLine()
                        End While

                    End Using
                End Using
            End Using
        End Using
    End Sub

    Private Shared Sub AjouterParametres(cmd As SqlCommand,
                                         params As Dictionary(Of String, Object))
        If params Is Nothing Then
            Exit Sub
        End If

        For Each kvp In params
            cmd.Parameters.Add(New SqlParameter(kvp.Key, If(kvp.Value, DBNull.Value)))
        Next
    End Sub

End Class