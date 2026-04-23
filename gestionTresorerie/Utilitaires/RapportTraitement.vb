Imports System.IO
Imports Newtonsoft.Json

Public NotInheritable Class RapportTraitement

    Public Enum TypeDoc
        Cheque
        Facture
    End Enum

    ' Structure pour stocker les résultats
    Public Class Compteur
        Public Property Succes As Integer = 0
        Public Property Total As Integer = 0
        Public Property Alertes As New List(Of String)
    End Class

    Private Shared ReadOnly _stats As New Dictionary(Of TypeDoc, Compteur)
    Private Shared _debut As DateTime

    ' --- MÉTHODES DE COLLECTE ---

    Public Shared Sub Demarrer()
        _stats.Clear()
        For Each t In [Enum].GetValues(GetType(TypeDoc))
            _stats(CType(t, TypeDoc)) = New Compteur()
        Next
        _debut = DateTime.Now
    End Sub

    Public Shared Sub Ajouter(type As TypeDoc, succes As Boolean, Optional msg As String = "")
        If Not _stats.ContainsKey(type) Then Return

        Dim c = _stats(type)
        c.Total += 1
        If succes Then c.Succes += 1
        If Not String.IsNullOrEmpty(msg) Then c.Alertes.Add(msg)
    End Sub

    ' --- MÉTHODES DE GÉNÉRATION (Remplacent RapportFormatter) ---

    Public Shared Function GenererRapportTexte() As String
        Dim sb As New System.Text.StringBuilder()
        sb.AppendLine($"=== RAPPORT DE TRAITEMENT DU {DateTime.Now:dd/MM/yyyy HH:mm} ===")
        sb.AppendLine($"Début : {_debut:HH:mm:ss}")
        sb.AppendLine($"Durée : {DateTime.Now - _debut:hh\:mm\:ss}")
        sb.AppendLine(New String("-"c, 40))

        For Each kvp In _stats
            sb.AppendLine($"DOCUMENT : {kvp.Key}")
            sb.AppendLine($"  - Réussis : {kvp.Value.Succes}/{kvp.Value.Total}")
            sb.AppendLine($"  - Alertes : {kvp.Value.Alertes.Count}")
        Next

        Return sb.ToString()
    End Function

    Public Shared Function GenererRapportJson() As String
        ' Un objet anonyme pour une structure JSON propre et simple
        Dim rapport = New With {
            .Infos = New With {
                .Debut = _debut,
                .Fin = DateTime.Now,
                .Duree = (DateTime.Now - _debut).ToString("hh\:mm\:ss")
            },
            .Resultats = _stats
        }
        Return JsonConvert.SerializeObject(rapport, Formatting.Indented)
    End Function

End Class