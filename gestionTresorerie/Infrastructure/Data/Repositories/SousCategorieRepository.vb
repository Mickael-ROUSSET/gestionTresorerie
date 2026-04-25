Imports System.Data.SqlClient

Public Class SousCategorieRepository

    Private ReadOnly _executor As ISqlExecutor

    Public Sub New(executor As ISqlExecutor)
        If executor Is Nothing Then Throw New ArgumentNullException(NameOf(executor))
        _executor = executor
    End Sub

    Public Function LireToutes() As List(Of SousCategorie)
        Return _executor.ExecuteNamedReader(
            "sqlSelSousCategoriesTout",
            Nothing,
            Function(reader As SqlDataReader)
                Dim sc As New SousCategorie()
                sc.LoadFromReader(reader)
                Return sc
            End Function
        )
    End Function

    Public Function LireParCategorie(idCategorie As Integer) As List(Of SousCategorie)
        Return _executor.ExecuteNamedReader(
            "reqSousCategorie",
            New List(Of SqlParameter) From {
                New SqlParameter("@idCategorie", idCategorie)
            },
            Function(reader As SqlDataReader)
                Dim sc As New SousCategorie()
                sc.LoadFromReader(reader)
                Return sc
            End Function
        )
    End Function

End Class