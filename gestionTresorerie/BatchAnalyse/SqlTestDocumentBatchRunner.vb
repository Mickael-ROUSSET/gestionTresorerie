Option Strict On

Imports System.Threading

' Adaptateur TEST explicite : la chaîne de lot reste indépendante de SQL.
' SaveVerified fournit l'identifiant réel et demande la revue des tiers si nécessaire.
Public NotInheritable Class SqlTestDocumentBatchRunner
    Public Shared Function Run(proposals As IEnumerable(Of DocumentArchiveProposal),
                               approve As Func(Of String, Boolean?),
                               Optional store As SqlDocumentTestStore = Nothing,
                               Optional cancellationToken As CancellationToken = Nothing) As TestDocumentBatchResult
        If proposals Is Nothing Then Throw New ArgumentNullException(NameOf(proposals))
        If approve Is Nothing Then Throw New ArgumentNullException(NameOf(approve))
        cancellationToken.ThrowIfCancellationRequested()
        If store Is Nothing Then store = New SqlDocumentTestStore()
        store.Initialize()
        Return TestDocumentBatchSaver.Run(proposals, Function(proposal) store.SaveVerified(proposal, approve), cancellationToken)
    End Function
End Class
