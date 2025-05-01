Imports Newtonsoft.Json
Imports System.Collections.Generic
Imports System.IO
'Imports Newtonsoft.Json

Public Class GererJson

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
    Private Shared Function LoadParametersFromFile() As Dictionary(Of String, String)
        Dim sFicParam As String = LectureProprietes.GetCheminEtVariable("parametresJson")
        If Not File.Exists(sFicParam) Then
            Return New Dictionary(Of String, String)
        End If

        Dim json As String = File.ReadAllText(sFicParam)
        Return JsonConvert.DeserializeObject(Of Dictionary(Of String, String))(json)
    End Function

End Class
