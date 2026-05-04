Imports System.Data.SqlClient

Public Class RegleAffectationReleveRepository

    Private ReadOnly _executor As ISqlExecutor

    Public Sub New(executor As ISqlExecutor)
        If executor Is Nothing Then Throw New ArgumentNullException(NameOf(executor))
        _executor = executor
    End Sub

    Public Function LireToutes() As List(Of RegleAffectationReleve)
        Return _executor.ExecuteNamedReader(
            "selReglesAffectationReleve",
            Nothing,
            Function(r As SqlDataReader)
                Return New RegleAffectationReleve With {
                    .Id = Convert.ToInt32(r("Id")),
                    .MotCle = r("MotCle").ToString(),
                    .IdTiers = If(IsDBNull(r("IdTiers")), CType(Nothing, Integer?), Convert.ToInt32(r("IdTiers"))),
                    .IdCategorie = If(IsDBNull(r("IdCategorie")), CType(Nothing, Integer?), Convert.ToInt32(r("IdCategorie"))),
                    .IdSousCategorie = If(IsDBNull(r("IdSousCategorie")), CType(Nothing, Integer?), Convert.ToInt32(r("IdSousCategorie"))),
                    .Sens = If(IsDBNull(r("Sens")), CType(Nothing, Boolean?), Convert.ToBoolean(r("Sens"))),
                    .Priorite = Convert.ToInt32(r("Priorite")),
                    .Actif = Convert.ToBoolean(r("Actif"))
                }
            End Function)
    End Function

    Public Function Inserer(regle As RegleAffectationReleve) As Integer
        Return _executor.ExecuteNamedNonQuery("insertRegleAffectationReleve", Params(regle, False))
    End Function

    Public Function MettreAJour(regle As RegleAffectationReleve) As Integer
        Return _executor.ExecuteNamedNonQuery("updateRegleAffectationReleve", Params(regle, True))
    End Function

    Public Function Supprimer(id As Integer) As Integer
        Return _executor.ExecuteNamedNonQuery(
            "deleteRegleAffectationReleve",
            New List(Of SqlParameter) From {
                New SqlParameter("@Id", id)
            })
    End Function

    Private Shared Function Params(regle As RegleAffectationReleve, includeId As Boolean) As List(Of SqlParameter)
        Dim p As New List(Of SqlParameter)

        If includeId Then p.Add(New SqlParameter("@Id", regle.Id))

        p.Add(New SqlParameter("@MotCle", regle.MotCle))
        p.Add(New SqlParameter("@IdTiers", If(regle.IdTiers.HasValue, CType(regle.IdTiers.Value, Object), DBNull.Value)))
        p.Add(New SqlParameter("@IdCategorie", If(regle.IdCategorie.HasValue, CType(regle.IdCategorie.Value, Object), DBNull.Value)))
        p.Add(New SqlParameter("@IdSousCategorie", If(regle.IdSousCategorie.HasValue, CType(regle.IdSousCategorie.Value, Object), DBNull.Value)))
        p.Add(New SqlParameter("@Sens", If(regle.Sens.HasValue, CType(regle.Sens.Value, Object), DBNull.Value)))
        p.Add(New SqlParameter("@Priorite", regle.Priorite))
        p.Add(New SqlParameter("@Actif", regle.Actif))

        Return p
    End Function

End Class