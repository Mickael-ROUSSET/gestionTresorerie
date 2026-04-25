Imports System.Data.SqlClient

Public Class DocumentRepository

    Private ReadOnly _executor As ISqlExecutor
    Private ReadOnly _factory As IConnectionFactory
    Private ReadOnly _sqlTextProvider As ISqlTextProvider

    Public Sub New(executor As ISqlExecutor,
                   factory As IConnectionFactory,
                   sqlTextProvider As ISqlTextProvider)

        If executor Is Nothing Then Throw New ArgumentNullException(NameOf(executor))
        If factory Is Nothing Then Throw New ArgumentNullException(NameOf(factory))
        If sqlTextProvider Is Nothing Then Throw New ArgumentNullException(NameOf(sqlTextProvider))

        _executor = executor
        _factory = factory
        _sqlTextProvider = sqlTextProvider
    End Sub

    Public Sub Inserer(doc As DocumentAgumaaa)
        If doc Is Nothing Then Throw New ArgumentNullException(NameOf(doc))

        _executor.ExecuteNamedNonQuery(
            "insertDocAgumaaa",
            ToSqlParameters(New Dictionary(Of String, Object) From {
                {"@dateDoc", doc.DateDoc},
                {"@cheminDoc", doc.CheminDoc},
                {"@categorieDoc", If(String.IsNullOrEmpty(doc.CategorieDoc), DBNull.Value, doc.CategorieDoc)},
                {"@sousCategorieDoc", If(String.IsNullOrEmpty(doc.SousCategorieDoc), DBNull.Value, doc.SousCategorieDoc)},
                {"@idMvtDoc", If(doc.IdMvtDoc = 0, DBNull.Value, doc.IdMvtDoc)},
                {"@metaDonnees", If(doc.metaDonnees, DBNull.Value)}
            })
        )
    End Sub

    Public Function LireTous() As DataTable
        Return ExecuteNamedDataTable("reqDocs", Nothing)
    End Function

    Public Sub MettreAJour(idDoc As Integer,
                           dateDoc As Date,
                           contenuDoc As String,
                           cheminDoc As String,
                           categorieDoc As String,
                           sousCategorieDoc As String,
                           idMvtDoc As Integer,
                           Optional metaDonnees As String = Nothing)

        _executor.ExecuteNamedNonQuery(
            "updDocs",
            ToSqlParameters(New Dictionary(Of String, Object) From {
                {"@idDoc", idDoc},
                {"@dateDoc", dateDoc},
                {"@contenuDoc", If(contenuDoc, DBNull.Value)},
                {"@cheminDoc", If(cheminDoc, DBNull.Value)},
                {"@categorieDoc", If(categorieDoc, DBNull.Value)},
                {"@sousCategorieDoc", If(sousCategorieDoc, DBNull.Value)},
                {"@idMvtDoc", idMvtDoc},
                {"@metaDonnees", If(metaDonnees, DBNull.Value)}
            })
        )
    End Sub

    Public Sub Supprimer(idDoc As Integer)
        _executor.ExecuteNamedNonQuery(
            "delDocs",
            ToSqlParameters(New Dictionary(Of String, Object) From {
                {"@idDoc", idDoc}
            })
        )
    End Sub

    Public Sub MettreAJourChemin(idDoc As Integer, nouveauChemin As String)
        _executor.ExecuteNamedNonQuery(
            "updCheminDoc",
            ToSqlParameters(New Dictionary(Of String, Object) From {
                {"@idDoc", idDoc},
                {"@cheminDoc", nouveauChemin}
            })
        )
    End Sub

    Public Sub MettreAJourMetaDonnees(idDoc As Integer, metaDonnees As String)
        _executor.ExecuteNamedNonQuery(
            "updateDocumentMetaDonnees",
            ToSqlParameters(New Dictionary(Of String, Object) From {
                {"@idDocument", idDoc},
                {"@metaDonnees", metaDonnees}
            })
        )
    End Sub

    Public Function LireDocumentsPagines(offset As Integer, taillePage As Integer) As DataTable
        Return ExecuteNamedDataTable(
            "selDocPagination",
            ToSqlParameters(New Dictionary(Of String, Object) From {
                {"@Offset", offset},
                {"@TaillePage", taillePage}
            })
        )
    End Function

    Public Function CompterDocuments() As Integer
        Return _executor.ExecuteNamedScalar(Of Integer)("cptDocPagination", Nothing)
    End Function

    Public Function LireParMontant(montant As Decimal) As DataTable
        Return ExecuteNamedDataTable(
            "reqDocMontant",
            ToSqlParameters(New Dictionary(Of String, Object) From {
                {"@montant", montant}
            })
        )
    End Function

    Public Function LireCheque(numero As Decimal, montant As Decimal, emetteur As String) As DataTable
        Return ExecuteNamedDataTable(
            "reqDoc",
            ToSqlParameters(New Dictionary(Of String, Object) From {
                {"@numero", numero},
                {"@montant", montant},
                {"@emetteur", "%" & emetteur & "%"}
            })
        )
    End Function

    Private Function ExecuteNamedDataTable(queryName As String,
                                           parameters As IEnumerable(Of SqlParameter)) As DataTable
        Dim dt As New DataTable()
        Dim sql As String = _sqlTextProvider.GetSql(queryName)

        Using conn = _factory.CreateConnection()
            Using cmd As New SqlCommand(sql, conn)
                If parameters IsNot Nothing Then
                    For Each p As SqlParameter In parameters
                        cmd.Parameters.Add(p)
                    Next
                End If

                Using adapter As New SqlDataAdapter(cmd)
                    adapter.Fill(dt)
                End Using
            End Using
        End Using

        Return dt
    End Function

    Private Shared Function ToSqlParameters(values As Dictionary(Of String, Object)) As List(Of SqlParameter)
        Dim result As New List(Of SqlParameter)

        If values Is Nothing Then
            Return result
        End If

        For Each kvp In values
            result.Add(New SqlParameter(kvp.Key, If(kvp.Value, DBNull.Value)))
        Next

        Return result
    End Function

End Class