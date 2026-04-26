Imports System.Data.SqlClient

Public Class PresentationRepository

    Private ReadOnly _executor As ISqlExecutor

    Public Sub New(executor As ISqlExecutor)
        If executor Is Nothing Then Throw New ArgumentNullException(NameOf(executor))
        _executor = executor
    End Sub

    Public Function LireCategoriesMouvements() As List(Of Integer)
        Return _executor.ExecuteNamedReader(
            "reqCategoriesMouvements",
            Nothing,
            Function(reader As SqlDataReader)
                Return Convert.ToInt32(reader.GetValue(0))
            End Function)
    End Function

    Public Function LireSommesParSousCategorie(idCategorie As Integer) As List(Of (Legend As String, Value As Decimal))
        Return _executor.ExecuteNamedReader(
            "reqSommeCatMouvements",
            New List(Of SqlParameter) From {
                New SqlParameter("@categorie", idCategorie)
            },
            Function(reader As SqlDataReader)
                Return (reader.GetString(0), reader.GetDecimal(1))
            End Function)
    End Function

End Class