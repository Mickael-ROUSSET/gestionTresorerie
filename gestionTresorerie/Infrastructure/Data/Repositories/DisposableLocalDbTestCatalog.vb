Option Strict On

Imports System.Data
Imports System.Data.SqlClient

''' <summary>
''' Catalogue LocalDB éphémère réservé aux essais fictifs.
''' Ne touche jamais aux catalogues bddAgumaaa et bddAgumaaaTest.
''' </summary>
Friend NotInheritable Class DisposableLocalDbTestCatalog
    Implements IDisposable

    Private Const Prefix As String = "gtDemoTest_"
    Private ReadOnly _name As String
    Private _created As Boolean

    Private Sub New(name As String)
        _name = name
    End Sub

    Friend ReadOnly Property Name As String
        Get
            Return _name
        End Get
    End Property

    Friend ReadOnly Property ConnectionString As String
        Get
            If Not _created Then Throw New InvalidOperationException("Le catalogue éphémère n’est pas actif.")
            Return ConnectionStringForName(_name)
        End Get
    End Property

    Friend Shared Function ConnectionStringForName(name As String) As String
        If Not IsDisposableName(name) Then Throw New ArgumentException("Nom de catalogue éphémère invalide.", NameOf(name))
        Return LocalDbCatalogs.ConnectionString(name)
    End Function

    Friend Shared Function NewName() As String
        Return Prefix & Guid.NewGuid().ToString("N")
    End Function

    Friend Shared Function IsDisposableName(name As String) As Boolean
        If String.IsNullOrEmpty(name) OrElse Not name.StartsWith(Prefix, StringComparison.Ordinal) Then Return False
        Dim suffix = name.Substring(Prefix.Length)
        Dim parsed As Guid
        Return suffix.Length = 32 AndAlso Guid.TryParseExact(suffix, "N", parsed) AndAlso parsed <> Guid.Empty
    End Function

    Friend Shared Function Create() As DisposableLocalDbTestCatalog
        Dim catalog As New DisposableLocalDbTestCatalog(NewName())
        Using connection As New SqlConnection(LocalDbCatalogs.ConnectionString("master"))
            connection.Open()
            Using command = connection.CreateCommand()
                command.CommandText = "SELECT DB_ID(@name);"
                command.Parameters.Add("@name", SqlDbType.NVarChar, 128).Value = catalog.Name
                If command.ExecuteScalar() IsNot DBNull.Value Then
                    Throw New InvalidOperationException("Le catalogue éphémère existe déjà.")
                End If
                command.Parameters.Clear()
                command.CommandText = "CREATE DATABASE [" & catalog.Name & "];"
                command.ExecuteNonQuery()
                catalog._created = True
            End Using
        End Using
        Return catalog
    End Function

    Public Sub Dispose() Implements IDisposable.Dispose
        If Not _created OrElse Not IsDisposableName(_name) Then Return
        Using connection As New SqlConnection(LocalDbCatalogs.ConnectionString("master"))
            connection.Open()
            Using command = connection.CreateCommand()
                command.CommandText = "IF DB_ID(@name) IS NOT NULL BEGIN " &
                    "ALTER DATABASE [" & _name & "] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; " &
                    "DROP DATABASE [" & _name & "]; END"
                command.Parameters.Add("@name", SqlDbType.NVarChar, 128).Value = _name
                command.ExecuteNonQuery()
            End Using
        End Using
        _created = False
    End Sub
End Class
