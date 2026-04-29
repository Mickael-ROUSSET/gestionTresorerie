Public Class RepositoryFactory

    Public Shared Function CreateExecutor(sBase As String) As ISqlExecutor
        Dim connectionString As String = ConnectionStringProvider.GetConnectionString(sBase)
        Dim factory As New AgumaaaConnectionFactory(connectionString)
        Dim provider As ISqlTextProvider = New LegacySqlTextProvider()
        Return New SqlExecutor(factory, provider)
    End Function

    Public Shared Function CreateConnectionFactory(sBase As String) As IConnectionFactory
        Return New AgumaaaConnectionFactory(ConnectionStringProvider.GetConnectionString(sBase))
    End Function

    Public Shared Function CreateSqlTextProvider() As ISqlTextProvider
        Return New LegacySqlTextProvider()
    End Function

End Class