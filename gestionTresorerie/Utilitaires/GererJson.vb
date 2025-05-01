Imports Newtonsoft.Json
Imports System.Collections.Generic
Imports System.IO
'Imports Newtonsoft.Json

Public Class GererJson
    Private Sub New()
        ' Constructeur privé pour empêcher l'instanciation directe
    End Sub
    ' Déclaration d'une variable globale
    Public Shared dicoClesValeurs As Dictionary(Of String, String)

    'Public Shared Sub KeyValuePairsExample() ' Exemple de dictionnaire de paramètres
    '    Dim parameters As New Dictionary(Of String, String) From {
    '        {"DatabaseConnection", "Server=myServer;Database=myDb;User Id=myUser;Password=myPassword;"},
    '        {"ApiUrl", "https://api.example.com"},
    '        {"Timeout", "30"}
    '    }

    '    ' Sauvegarder les paramètres dans un fichier JSON
    '    'Call SaveParametersToFile(parameters)

    '    ' Lire les paramètres depuis le fichier JSON
    '    Dim loadedParameters As Dictionary(Of String, String) = LoadParametersFromFile()

    '    ' Afficher les paramètres chargés
    '    For Each kvp As KeyValuePair(Of String, String) In loadedParameters
    '        MsgBox($"{kvp.Key}: {kvp.Value}")
    '    Next
    'End Sub

    ' Méthode pour sauvegarder les paramètres dans un fichier JSON
    Private Shared Sub SaveParametersToFile(parameters As Dictionary(Of String, String))
        Dim json As String = JsonConvert.SerializeObject(parameters, Formatting.Indented)
        File.WriteAllText(LectureProprietes.GetCheminEtVariable("parametresJson"), json)
    End Sub

    ' Méthode pour lire les paramètres depuis un fichier JSON
    Public Shared Sub LoadParametersFromFile()
        Dim sFicParam As String = LectureProprietes.GetCheminEtVariable("parametresJson")
        If Not File.Exists(sFicParam) Then
            Logger.ERR($"Le fichier {sFicParam} n'existe pas.")
        End If

        Dim json As String = File.ReadAllText(sFicParam)
        dicoClesValeurs = JsonConvert.DeserializeObject(Of Dictionary(Of String, String))(json)
    End Sub

End Class
