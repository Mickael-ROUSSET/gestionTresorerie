
Imports System.Text

Imports Newtonsoft.Json

''' <summary>
''' Compte-rendu global du batch
''' </summary>
Public Class BatchMailReport
    Public Property NomBatch As String
    Public Property Description As String
    Public Property DateDebut As DateTime
    Public Property DateFin As DateTime?
    Public Property NomRequete As String
    Public Property Parametres As Dictionary(Of String, Object)
    Public Property Total As Integer
    Public Property SuccessCount As Integer
    Public Property FailCount As Integer
    Public Property Results As New List(Of DestinataireResult)

    Public Function ToText() As String
        Dim sb As New StringBuilder()
        sb.AppendLine($"{NomBatch} - Début : {DateDebut:yyyy-MM-dd HH:mm:ss}")
        sb.AppendLine($"Description : {Description}")
        sb.AppendLine($"Requête : {NomRequete}")
        sb.AppendLine($"Paramètres : {If(Parametres IsNot Nothing, JsonConvert.SerializeObject(Parametres), "{}")}")
        sb.AppendLine($"Total destinataires : {Total}")
        sb.AppendLine($"Succès : {SuccessCount}, Échecs : {FailCount}")
        sb.AppendLine()
        sb.AppendLine("Détails :")
        For Each r In Results
            sb.AppendLine($"[{If(r.IsSuccess, "OK", "KO")}][{r.Time:yyyy-MM-dd HH:mm:ss}] {r.Adresse} - {r.Nom} {If(String.IsNullOrEmpty(r.ErrorMessage), "", " -> " & r.ErrorMessage)}")
        Next
        Return sb.ToString()
    End Function

    Public Function ToJson() As String
        Return JsonConvert.SerializeObject(Me, Formatting.Indented)
    End Function
End Class