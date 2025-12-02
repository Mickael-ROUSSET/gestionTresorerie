Imports System.IO
Imports Newtonsoft.Json
'Imports Newtonsoft.Json

Public Class GererJson
    Private Sub New()
        ' Constructeur privé pour empêcher l'instanciation directe
    End Sub
    ' Déclaration d'une variable globale
    Private Shared dicoClesValeurs As Dictionary(Of String, String)

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
