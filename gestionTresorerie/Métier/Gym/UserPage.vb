Imports Newtonsoft.Json

Public Class UserPage
    <JsonProperty("items")>
    Public Property Items As List(Of UserContext)

    <JsonProperty("page")>
    Public Property Page As Integer

    <JsonProperty("lastPage")>
    Public Property LastPage As Integer

    <JsonProperty("itemsPerPage")>
    Public Property ItemsPerPage As Integer

    <JsonProperty("count")>
    Public Property Count As Integer

    <JsonProperty("total")>
    Public Property Total As Integer
End Class


