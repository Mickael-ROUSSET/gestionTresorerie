Imports Newtonsoft.Json

Public Class BatchMailConfig
    Public Property NomRequeteSQL As String
    Public Property ParametresRequeteSQL As Dictionary(Of String, Object)
    Public Property ColonneEmail As String
    Public Property SujetMail As String
    Public Property RplctSujetMail As Dictionary(Of String, String)
    Public Property RplctCorpsMail As Dictionary(Of String, String)
    Public Property RplctPJ As Dictionary(Of String, String)
    Public Property CheminTemplateCorpsEmail As String
    Public Property CheminsPjRessource As List(Of String)
    Public Property CheminRapportTrt As String

    Public Shared Function Load(path As String) As BatchMailConfig
        Dim json = IO.File.ReadAllText(path)
        Return JsonConvert.DeserializeObject(Of BatchMailConfig)(json)
    End Function
End Class

