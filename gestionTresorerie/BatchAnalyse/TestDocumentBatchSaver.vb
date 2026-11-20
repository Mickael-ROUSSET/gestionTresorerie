Option Strict On

Imports System.IO
Imports System.Data.Common
Imports System.Threading

' Orchestrateur commun : aucune dépendance Gmail, SQL ou interface.
' Le même parcours est utilisable avec un moteur simulé et des documents fictifs.
Public NotInheritable Class TestDocumentBatchSaver
    Public Shared Function Run(proposals As IEnumerable(Of DocumentArchiveProposal),
                               save As Func(Of DocumentArchiveProposal, Integer?),
                               Optional cancellationToken As CancellationToken = Nothing) As TestDocumentBatchResult
        If proposals Is Nothing Then Throw New ArgumentNullException(NameOf(proposals))
        If save Is Nothing Then Throw New ArgumentNullException(NameOf(save))
        Dim result As New TestDocumentBatchResult()
        Dim seen As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        For Each proposal In proposals
            cancellationToken.ThrowIfCancellationRequested()
            If proposal Is Nothing Then
                result.Rejected += 1
                Continue For
            End If
            ' Une destination absente ne doit ni interrompre le lot ni déclencher une écriture.
            If String.IsNullOrWhiteSpace(proposal.Destination) Then
                result.Rejected += 1
                Continue For
            End If
            ' Un doublon dans le lot ne doit pas déclencher deux écritures SQL.
            If seen.Contains(proposal.Destination) Then
                result.Duplicates += 1
                Continue For
            End If
            Try
                Dim id = save(proposal)
                If id.HasValue AndAlso id.Value > 0 Then
                    seen.Add(proposal.Destination)
                    result.Saved += 1
                    result.SavedDocumentIds.Add(id.Value)
                Else
                    ' Annulation de la revue d'un tiers : à reprendre ultérieurement.
                    result.Pending += 1
                End If
            Catch ex As Exception When TypeOf ex Is DbException OrElse
                                       TypeOf ex Is IOException OrElse
                                       TypeOf ex Is UnauthorizedAccessException OrElse
                                       TypeOf ex Is ArgumentException OrElse
                                       TypeOf ex Is InvalidOperationException
                ' Ne jamais journaliser ici des noms, chemins, montants ou identités.
                result.Rejected += 1
            End Try
        Next
        Return result
    End Function
End Class

Public NotInheritable Class TestDocumentBatchResult
    ' Identifiants SQL confirmés, dans l'ordre des enregistrements réussis.
    Public ReadOnly Property SavedDocumentIds As New List(Of Integer)()
    Public Property Saved As Integer
    Public Property Pending As Integer
    Public Property Duplicates As Integer
    Public Property Rejected As Integer
End Class
