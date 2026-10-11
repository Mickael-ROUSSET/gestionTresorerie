Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System.Linq
Imports System.Data.SqlClient

<TestClass>
Public Class DocumentRepositoryTests

    <TestMethod>
    Public Sub MettreAJourChemin_UtiliseUpdCheminDoc()
        Dim fake As New FakeSqlExecutor()
        Dim repo As New DocumentRepository(fake, New UnusedConnectionFactory(), New UnusedSqlTextProvider())

        repo.MettreAJourChemin(5, "C:\test.pdf")

        Assert.AreEqual("updCheminDoc", fake.LastQueryName)

        Dim p = fake.LastParameters.ToList()
        Assert.HasCount(2, p)

        Assert.AreEqual("@idDoc", p(0).ParameterName)
        Assert.AreEqual(5, p(0).Value)

        Assert.AreEqual("@cheminDoc", p(1).ParameterName)
        Assert.AreEqual("C:\test.pdf", p(1).Value)
    End Sub

    <TestMethod>
    Public Sub MettreAJourMetaDonnees_UtiliseUpdateDocumentMetaDonnees()
        Dim fake As New FakeSqlExecutor()
        Dim repo As New DocumentRepository(fake, New UnusedConnectionFactory(), New UnusedSqlTextProvider())

        repo.MettreAJourMetaDonnees(5, "{""a"":1}")

        Assert.AreEqual("updateDocumentMetaDonnees", fake.LastQueryName)

        Dim p = fake.LastParameters.ToList()
        Assert.HasCount(2, p)

        Assert.AreEqual("@idDocument", p(0).ParameterName)
        Assert.AreEqual(5, p(0).Value)

        Assert.AreEqual("@metaDonnees", p(1).ParameterName)
        Assert.AreEqual("{""a"":1}", p(1).Value)
    End Sub

    <TestMethod>
    <DataRow("executor")>
    <DataRow("factory")>
    <DataRow("sqlTextProvider")>
    Public Sub Constructeur_DependanceNulle_EstRefusee(dependance As String)
        Dim executor As ISqlExecutor = New FakeSqlExecutor()
        Dim factory As IConnectionFactory = New UnusedConnectionFactory()
        Dim provider As ISqlTextProvider = New UnusedSqlTextProvider()
        If dependance = "executor" Then executor = Nothing
        If dependance = "factory" Then factory = Nothing
        If dependance = "sqlTextProvider" Then provider = Nothing

        Dim ex = Assert.ThrowsExactly(Of ArgumentNullException)(
            Sub()
                Dim repo As New DocumentRepository(executor, factory, provider)
            End Sub)
        Assert.AreEqual(dependance, ex.ParamName)
    End Sub

    Private Class UnusedConnectionFactory
        Implements IConnectionFactory

        Public Function CreateConnection() As SqlConnection Implements IConnectionFactory.CreateConnection
            Throw New InvalidOperationException("Ce test ne doit pas ouvrir de connexion SQL.")
        End Function
    End Class

    Private Class UnusedSqlTextProvider
        Implements ISqlTextProvider

        Public Function GetSql(queryName As String) As String Implements ISqlTextProvider.GetSql
            Throw New InvalidOperationException("Ce test doit passer par le faux exécuteur SQL.")
        End Function
    End Class
End Class
