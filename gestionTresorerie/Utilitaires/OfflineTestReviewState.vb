Option Strict On
Option Explicit On

Imports System
Imports System.Collections.Generic
Imports Newtonsoft.Json

' Format propre aux essais hors ligne. Les corrections sont rejouées par le domaine.
Friend NotInheritable Class OfflineTestReviewState
    Public Class WarningConfirmation
        Public Property Field As PropositionCorrection.Champ
        Public Property Warning As String
    End Class

    Public Class Correction
        Public Property Key As String
        Public Property ProposedDate As DateTime
        Public Property Amount As Decimal
        Public Property Direction As PropositionMouvement.SensMouvement
        Public Property Label As String
        Public Property Reference As String
        Public Property Confirmations As List(Of WarningConfirmation)
    End Class

    Public Class Snapshot
        Public Property Version As Integer
        Public Property SessionId As String
        Public Property SourceHash As String
        Public Property Corrections As List(Of Correction)
        Public Property ExaminedKeys As List(Of String)
    End Class

    Public Shared Function Capture(workflow As GmailDocumentReviewWorkflow,
                                   sessionId As String, sourceHash As String) As String
        Dim snapshot As New Snapshot With {
            .Version = 1, .SessionId = sessionId, .SourceHash = sourceHash,
            .Corrections = New List(Of Correction)(),
            .ExaminedKeys = New List(Of String)()}
        For Each audit In workflow.SessionRevue.Audits
            Dim item As New Correction With {
                .Key = audit.Proposition.CleSource,
                .ProposedDate = audit.Proposition.DateProposee,
                .Amount = audit.Proposition.Montant,
                .Direction = audit.Proposition.Sens,
                .Label = audit.Proposition.Libelle,
                .Reference = audit.Proposition.Reference,
                .Confirmations = New List(Of WarningConfirmation)()}
            For Each confirmation In audit.Confirmations
                item.Confirmations.Add(New WarningConfirmation With {
                    .Field = confirmation.Champ, .Warning = confirmation.Avertissement})
            Next
            snapshot.Corrections.Add(item)
        Next
        For Each item In workflow.SessionRevue.Propositions
            If workflow.SessionRevue.EstExamenConfirme(item.Key) Then
                snapshot.ExaminedKeys.Add(item.Key)
            End If
        Next
        If snapshot.Corrections.Count > 1000 Then
            Throw New InvalidOperationException("Trop de corrections pour une sauvegarde TEST.")
        End If
        Return JsonConvert.SerializeObject(snapshot, Formatting.Indented)
    End Function

    Public Shared Sub Restore(workflow As GmailDocumentReviewWorkflow, sessionId As String,
                              sourceHash As String, json As String)
        Dim snapshot = JsonConvert.DeserializeObject(Of Snapshot)(
            json, New JsonSerializerSettings With {.TypeNameHandling = TypeNameHandling.None})
        If snapshot Is Nothing OrElse snapshot.Version <> 1 OrElse
           Not String.Equals(snapshot.SessionId, sessionId, StringComparison.Ordinal) OrElse
           Not String.Equals(snapshot.SourceHash, sourceHash, StringComparison.Ordinal) OrElse
           snapshot.Corrections Is Nothing OrElse snapshot.ExaminedKeys Is Nothing OrElse
           snapshot.Corrections.Count > 1000 OrElse
           snapshot.ExaminedKeys.Count > workflow.SessionRevue.Propositions.Count Then
            Throw New InvalidOperationException("La sauvegarde TEST ne correspond pas aux documents de cette session.")
        End If
        For Each item In snapshot.Corrections
            If item Is Nothing OrElse item.Key Is Nothing OrElse
               Not workflow.SessionRevue.Propositions.ContainsKey(item.Key) OrElse
               item.Confirmations Is Nothing Then
                Throw New InvalidOperationException("Correction TEST incompatible.")
            End If
            Dim confirmations As New List(Of PropositionCorrection.ConfirmationAvertissement)()
            For Each confirmation In item.Confirmations
                If confirmation Is Nothing Then Throw New InvalidOperationException("Confirmation TEST absente.")
                confirmations.Add(New PropositionCorrection.ConfirmationAvertissement(
                    confirmation.Field, confirmation.Warning))
            Next
            Dim current = workflow.SessionRevue.Propositions(item.Key)
            Dim correction = PropositionCorrection.Corriger(
                current, item.ProposedDate, item.Amount, item.Direction,
                item.Label, item.Reference, confirmations)
            workflow.SessionRevue.AppliquerCorrection(item.Key, correction)
        Next
        Dim keys As New HashSet(Of String)(StringComparer.Ordinal)
        For Each key In snapshot.ExaminedKeys
            If key Is Nothing OrElse Not keys.Add(key) OrElse
               Not workflow.SessionRevue.Propositions.ContainsKey(key) Then
                Throw New InvalidOperationException("Examen TEST incompatible.")
            End If
            workflow.SessionRevue.ConfirmerExamen(key, workflow.SessionRevue.Propositions(key))
        Next
    End Sub
End Class
