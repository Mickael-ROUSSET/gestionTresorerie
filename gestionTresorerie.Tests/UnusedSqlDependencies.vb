Imports System.Data.SqlClient

Friend Class UnusedConnectionFactory
        Implements IConnectionFactory

        Public Function CreateConnection() As SqlConnection Implements IConnectionFactory.CreateConnection
            Throw New InvalidOperationException("Ce test ne doit pas ouvrir de connexion SQL.")
        End Function
    End Class

Friend Class UnusedSqlTextProvider
        Implements ISqlTextProvider

        Public Function GetSql(queryName As String) As String Implements ISqlTextProvider.GetSql
            Throw New InvalidOperationException("Ce test doit passer par le faux exécuteur SQL.")
        End Function
    End Class
