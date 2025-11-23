Public Interface IDataService
    Function ExecuterRequete(query As String, Optional parameters As Dictionary(Of String, Object) = Nothing) As DataTable

End Interface
