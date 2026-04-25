Imports System.Data.SqlClient

Public Class MouvementRepository

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

    Public Function Inserer(mouvement As Mouvements) As Integer
        If mouvement Is Nothing Then Throw New ArgumentNullException(NameOf(mouvement))

        Return _executor.ExecuteNamedNonQuery(
            "insertMvts",
            ToSqlParameters(New Dictionary(Of String, Object) From {
                {"@note", If(String.IsNullOrWhiteSpace(mouvement.Note), DBNull.Value, mouvement.Note.Trim())},
                {"@categorie", mouvement.Categorie},
                {"@sousCategorie", If(mouvement.SousCategorie = 0, DBNull.Value, mouvement.SousCategorie)},
                {"@tiers", Convert.ToInt32(mouvement.Tiers)},
                {"@dateCreation", DateTime.Now.Date},
                {"@dateMvt", If(mouvement.DateMvt = Date.MinValue, DBNull.Value, mouvement.DateMvt.Date)},
                {"@montant", Convert.ToDecimal(mouvement.Montant)},
                {"@sens", If(mouvement.Sens, 1, 0)},
                {"@etat", If(mouvement.Etat, 1, 0)},
                {"@evenement", If(String.IsNullOrWhiteSpace(mouvement.Evenement), DBNull.Value, mouvement.Evenement)},
                {"@typeMouvement", If(String.IsNullOrWhiteSpace(mouvement.TypeMouvement), DBNull.Value, mouvement.TypeMouvement)},
                {"@modifiable", If(mouvement.Modifiable, 1, 0)},
                {"@numeroRemise", If(String.IsNullOrWhiteSpace(mouvement.NumeroRemise) OrElse Not IsNumeric(mouvement.NumeroRemise), DBNull.Value, Convert.ToInt32(mouvement.NumeroRemise))},
                {"@reference", If(String.IsNullOrWhiteSpace(mouvement.reference), DBNull.Value, mouvement.reference.Trim())},
                {"@typeReference", If(String.IsNullOrWhiteSpace(mouvement.typeReference), DBNull.Value, mouvement.typeReference.Trim())},
                {"@idDoc", If(mouvement.idDoc = 0, DBNull.Value, mouvement.idDoc)}
            })
        )
    End Function

    Public Function MettreAJourIdDoc(idMouvement As Integer, nouvelIdDoc As Integer) As Integer
        Return _executor.ExecuteNamedNonQuery(
            "updMvtIdDoc",
            ToSqlParameters(New Dictionary(Of String, Object) From {
                {"@nouvelIdDoc", nouvelIdDoc},
                {"@idMouvement", idMouvement}
            })
        )
    End Function

    Public Function Existe(mouvement As Mouvements) As Boolean
        If mouvement Is Nothing Then Throw New ArgumentNullException(NameOf(mouvement))

        Dim nb As Integer = _executor.ExecuteNamedScalar(Of Integer)(
            "reqNbMouvements",
            ToSqlParameters(New Dictionary(Of String, Object) From {
                {"@dateMvt", mouvement.DateMvt.ToString("yyyy-MM-dd")},
                {"@montant", CDec(mouvement.Montant)},
                {"@sens", mouvement.Sens}
            })
        )

        Return nb > 0
    End Function

    Public Function ChargerMouvementsSimilaires(mouvement As Mouvements) As DataTable
        If mouvement Is Nothing Then Throw New ArgumentNullException(NameOf(mouvement))

        Return ExecuteNamedDataTable(
            "reqMouvementsSimilaires",
            ToSqlParameters(New Dictionary(Of String, Object) From {
                {"@dateMvt", mouvement.DateMvt},
                {"@montant", CDec(mouvement.Montant)},
                {"@sens", mouvement.Sens}
            })
        )
    End Function

    Public Function MettreAJour(id As Integer,
                                categorie As Integer,
                                sousCategorie As Integer,
                                montant As Decimal,
                                sens As Boolean,
                                tiers As Integer,
                                note As String,
                                dateMvt As Date,
                                etat As Boolean,
                                evenement As String,
                                type As String,
                                modifiable As Boolean,
                                numeroRemise As Integer?,
                                reference As String,
                                typeReference As String,
                                idDoc As Integer) As Integer

        Return _executor.ExecuteNamedNonQuery(
            "updMvt",
            ToSqlParameters(New Dictionary(Of String, Object) From {
                {"@Id", id},
                {"@Categorie", categorie},
                {"@SousCategorie", sousCategorie},
                {"@Montant", montant},
                {"@Sens", sens},
                {"@Tiers", tiers},
                {"@Note", If(note, DBNull.Value)},
                {"@DateMvt", dateMvt},
                {"@Etat", etat},
                {"@Evenement", If(evenement, DBNull.Value)},
                {"@TypeMouvement", If(type, DBNull.Value)},
                {"@Modifiable", modifiable},
                {"@NumeroRemise", If(numeroRemise, DBNull.Value)},
                {"@reference", If(reference, DBNull.Value)},
                {"@typeReference", If(typeReference, DBNull.Value)},
                {"@idDoc", idDoc}
            })
        )
    End Function

    Public Function Supprimer(id As Integer) As Integer
        Return _executor.ExecuteNamedNonQuery(
            "delMvt",
            ToSqlParameters(New Dictionary(Of String, Object) From {
                {"@Id", id}
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

        For Each kvp In values
            result.Add(New SqlParameter(kvp.Key, If(kvp.Value, DBNull.Value)))
        Next

        Return result
    End Function

End Class