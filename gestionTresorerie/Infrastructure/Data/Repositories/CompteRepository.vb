Imports System.Data.SqlClient

Public Class CompteRepository

    Private ReadOnly _executor As ISqlExecutor

    Public Sub New(executor As ISqlExecutor)
        If executor Is Nothing Then Throw New ArgumentNullException(NameOf(executor))
        _executor = executor
    End Sub

    Public Function Inserer(compte As Compte) As Integer
        Return _executor.ExecuteNamedNonQuery(
            Constantes.Sql.Insertion.Compte,
            ToParams(compte, includeId:=False))
    End Function

    Public Function MettreAJour(compte As Compte) As Integer
        Return _executor.ExecuteNamedNonQuery(
            Constantes.Sql.Update.sqlUpdCompte,
            ToParams(compte, includeId:=True))
    End Function

    Public Function Supprimer(id As Integer) As Integer
        Return _executor.ExecuteNamedNonQuery(
            Constantes.Sql.Deletion.Compte,
            New List(Of SqlParameter) From {
                New SqlParameter("@Id", id)
            })
    End Function

    Private Shared Function ToParams(compte As Compte, includeId As Boolean) As List(Of SqlParameter)
        Dim p As New List(Of SqlParameter)

        If includeId Then
            p.Add(New SqlParameter("@Id", compte.Id))
        End If

        p.Add(New SqlParameter("@Libelle", If(compte.Libelle, DBNull.Value)))
        p.Add(New SqlParameter("@Iban", If(compte.Iban, DBNull.Value)))
        p.Add(New SqlParameter("@Bic", If(compte.Bic, DBNull.Value)))
        p.Add(New SqlParameter("@Actif", compte.Actif))

        Return p
    End Function

End Class