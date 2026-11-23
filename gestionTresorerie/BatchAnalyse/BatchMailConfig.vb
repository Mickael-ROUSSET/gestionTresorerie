Imports Newtonsoft.Json

Public Class BatchMailConfig
    Public Property NomBatch As String
    Public Property Description As String
    Public Property NomRequeteSQL As String
    Public Property ParametresRequeteSQL As New Dictionary(Of String, Object)
    Public Property ColonneEmail As String
    Public Property SujetMail As String
    Public Property RplctSujetMail As New Dictionary(Of String, String)
    Public Property RplctCorpsMail As New Dictionary(Of String, String)
    Public Property RplctPJ As New Dictionary(Of String, String)
    Public Property CheminTemplateCorpsEmail As String
    Public Property CheminsPjRessource As New List(Of String)
    Public Property CheminRapportTrt As String

    Public Shared Function Load(path As String) As BatchMailConfig
        Dim json = IO.File.ReadAllText(path)
        Return JsonConvert.DeserializeObject(Of BatchMailConfig)(json, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
    End Function
End Class