Imports System.Data.SqlClient

Public Class TarifRepository

    Private ReadOnly _executor As ISqlExecutor

    Public Sub New(executor As ISqlExecutor)
        If executor Is Nothing Then Throw New ArgumentNullException(NameOf(executor))
        _executor = executor
    End Sub

    Public Function LireTarifActifAdate(dateReference As Date) As Tarif
        Dim lignes = _executor.ExecuteNamedReader(
        "selTarifActifAdate",
        New List(Of SqlParameter) From {
            New SqlParameter("@dateReference", dateReference)
        },
        Function(r As SqlDataReader)
            Return New Tarif With {
                .IdTarif = Convert.ToInt32(r("IdTarif")),
                .NomTarif = r("NomTarif").ToString(),
                .Montant = Convert.ToDecimal(r("Montant"))
            }
        End Function)

        If lignes.Count = 0 Then Return Nothing
        Return lignes(0)
    End Function

End Class