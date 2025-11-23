Imports Microsoft.VisualStudio.TestPlatform.ObjectModel

Public Class CompteurTraitement
    Public Property NbParticipantsExtraits As Integer = 0
    Public Property NbParticipantsTraites As Integer = 0
    Public Property NbAttestationsGenerees As Integer = 0
    Public Property NbCommercants As Integer = 0
    Public Property NbMailsEnvoyes As Integer = 0

    Public Overrides Function ToString() As String
        Return "=== COMPTE-RENDU DE TRAITEMENT ===" & vbCrLf & vbCrLf &
               "Participants extraits :  " & NbParticipantsExtraits & vbCrLf &
               "Participants traités :   " & NbParticipantsTraites & vbCrLf &
               "Commerçants ignorés :    " & NbCommercants & vbCrLf &
               "Attestations générées :  " & NbAttestationsGenerees & vbCrLf &
               "Mails envoyés :          " & NbMailsEnvoyes
    End Function

End Class
