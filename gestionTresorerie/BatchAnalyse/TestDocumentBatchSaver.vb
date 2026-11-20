Option Strict On

Imports System.IO

' Orchestrateur commun : aucune dépendance Gmail, SQL ou interface.
' Le même parcours est utilisable avec un moteur simulé et des documents fictifs.
Public NotInheritable Class TestDocumentBatchSaver
    Public Shared Function Run(proposals As IEnumerable(Of DocumentArchiveProposal),
                               save As Func(Of DocumentArchiveProposal, Integer?)) As TestDocumentBatchResult
        If proposals Is Nothing Then Throw New ArgumentNullException(NameOf(proposals))
        If save Is Nothing Then Throw New ArgumentNullException(NameOf(save))
        Dim result As New TestDocumentBatchResult()
        Dim seen As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        For Each proposal In proposals
            If proposal Is Nothing Then
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
                Else
                    ' Annulation de la revue d'un tiers : à reprendre ultérieurement.
                    result.Pending += 1
                End If
            Catch ex As Exception When TypeOf ex Is IOException OrElse
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
    Public Property Saved As Integer
    Public Property Pending As Integer
    Public Property Duplicates As Integer
    Public Property Rejected As Integer
End Class
