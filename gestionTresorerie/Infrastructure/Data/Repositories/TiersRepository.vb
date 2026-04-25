Imports System.Data.SqlClient

Public Class TiersRepository

    Private ReadOnly _executor As ISqlExecutor

    Public Sub New(executor As ISqlExecutor)
        If executor Is Nothing Then Throw New ArgumentNullException(NameOf(executor))
        _executor = executor
    End Sub

    Public Function GetCategorieDefaut(idTiers As Integer) As Integer
        Return _executor.ExecuteNamedScalar(Of Integer)(
            "reqCategoriesDefautMouvements",
            New List(Of SqlParameter) From {
                New SqlParameter("@Id", idTiers)
            })
    End Function

    Public Function GetSousCategorieDefaut(idTiers As Integer) As Integer
        Return _executor.ExecuteNamedScalar(Of Integer)(
            "reqSousCategoriesDefautMouvements",
            New List(Of SqlParameter) From {
                New SqlParameter("@Id", idTiers)
            })
    End Function

    Public Function GetIdTiersByUser(nom As String,
                                     prenom As String,
                                     Optional dateNaissance As Date? = Nothing) As Integer
        Dim id As Integer =
            _executor.ExecuteNamedScalar(Of Integer)(
                "getIdTiersByNomPrenomDate",
                New List(Of SqlParameter) From {
                    New SqlParameter("@nom", If(String.IsNullOrWhiteSpace(nom), DBNull.Value, nom.Trim())),
                    New SqlParameter("@prenom", If(String.IsNullOrWhiteSpace(prenom), DBNull.Value, prenom.Trim())),
                    New SqlParameter("@dateNaissance", If(dateNaissance.HasValue, CType(dateNaissance.Value.Date, Object), DBNull.Value))
                })

        Return id
    End Function

    Public Function TiersExiste(nom As String, prenom As String, raisonSociale As String) As Boolean
        Dim count As Integer =
            _executor.ExecuteNamedScalar(Of Integer)(
                "cptTiers",
                New List(Of SqlParameter) From {
                    New SqlParameter("@nom", If(String.IsNullOrWhiteSpace(nom), DBNull.Value, nom.Trim())),
                    New SqlParameter("@prenom", If(String.IsNullOrWhiteSpace(prenom), DBNull.Value, prenom.Trim())),
                    New SqlParameter("@raisonSociale", If(String.IsNullOrWhiteSpace(raisonSociale), DBNull.Value, raisonSociale.Trim()))
                })

        Return count > 0
    End Function

    Public Function Inserer(sRaisonSociale As String,
                            sPrenom As String,
                            sNom As String,
                            iCategorie As Integer?,
                            iSousCategorie As Integer?) As Integer

        Dim result As Object =
            _executor.ExecuteNamedScalar(Of Object)(
                "insertTiers",
                New List(Of SqlParameter) From {
                    New SqlParameter("@nom", If(String.IsNullOrWhiteSpace(sNom), DBNull.Value, sNom.Trim())),
                    New SqlParameter("@prenom", If(String.IsNullOrWhiteSpace(sPrenom), DBNull.Value, sPrenom.Trim())),
                    New SqlParameter("@raisonSociale", If(String.IsNullOrWhiteSpace(sRaisonSociale), DBNull.Value, sRaisonSociale.Trim())),
                    New SqlParameter("@categorieDefaut", If(iCategorie.HasValue, CType(iCategorie.Value, Object), DBNull.Value)),
                    New SqlParameter("@sousCategorieDefaut", If(iSousCategorie.HasValue, CType(iSousCategorie.Value, Object), DBNull.Value)),
                    New SqlParameter("@dateCreation", DateTime.Now),
                    New SqlParameter("@dateModification", DateTime.Now)
                })

        If result Is Nothing OrElse result Is DBNull.Value Then
            Return 0
        End If

        Dim newId As Integer
        If Integer.TryParse(result.ToString(), newId) Then
            Return newId
        End If

        Return 0
    End Function

    Public Function LireTiersPhysiques() As List(Of Tiers)
        Return _executor.ExecuteNamedReader(
            "selTiersPhysique",
            Nothing,
            Function(reader As SqlDataReader)
                Return New Tiers(reader.GetInt32(0),
                                 reader.GetString(1),
                                 reader.GetString(2))
            End Function)
    End Function

    Public Function LireTiersMoraux() As List(Of Tiers)
        Return _executor.ExecuteNamedReader(
            "selTiersMorale",
            Nothing,
            Function(reader As SqlDataReader)
                Return New Tiers(reader.GetInt32(0),
                                 reader.GetString(1))
            End Function)
    End Function

End Class