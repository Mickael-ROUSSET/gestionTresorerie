Imports System.Data.SqlClient

Public Interface IConnectionFactory
    Function CreateConnection() As SqlConnection
End Interface