Imports Newtonsoft.Json

Public NotInheritable Class RapportFormatter
    Public Enum TypeDoc
        Cheque
        Facture
    End Enum

    ' Structure simplifiée : Succès / Total
    Public Class Compteur
        Public Property Succes As Integer = 0
        Public Property Total As Integer = 0
        Public Property Alertes As New List(Of String)
    End Class

    Private Shared ReadOnly _stats As New Dictionary(Of TypeDoc, Compteur)
    Private Shared _debut As DateTime

    Public Shared Sub Demarrer()
        _stats.Clear()
        For Each t In [Enum].GetValues(GetType(TypeDoc))
            _stats(t) = New Compteur()
        Next
        _debut = DateTime.Now
    End Sub

    Public Shared Sub Ajouter(type As TypeDoc, succes As Boolean, Optional msg As String = "")
        Dim c = _stats(type)
        c.Total += 1
        If succes Then c.Succes += 1
        If Not String.IsNullOrEmpty(msg) Then c.Alertes.Add(msg)
    End Sub

    ' Exportation JSON en 1 seule ligne !
    Public Shared Function ToJson() As String
        Dim rapport = New With {
            .Periode = $"Du {_debut} au {DateTime.Now}",
            .Resultats = _stats
        }
        Return JsonConvert.SerializeObject(rapport, Formatting.Indented)
    End Function

    ' Exportation Texte simple
    Public Shared Function ToTexte() As String
        Dim sb As New System.Text.StringBuilder()
        sb.AppendLine($"RAPPORT DU {DateTime.Now}")
        For Each kvp In _stats
            sb.AppendLine($"- {kvp.Key}: {kvp.Value.Succes}/{kvp.Value.Total} réussi(s)")
        Next
        Return sb.ToString()
    End Function
End Class