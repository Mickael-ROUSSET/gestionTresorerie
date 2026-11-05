Option Strict On
Option Explicit On

Imports System
Imports System.Configuration
Imports System.Reflection
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass, DoNotParallelize>
Public Class TestEnvironmentIsolationTests
    <TestMethod>
    <DataRow("C:\GT-PROD", "C:\GT-TEST")>
    <DataRow("C:\GT-PROD\", "C:\GT-PROD-autre\")>
    Public Sub DossiersDistinctsAcceptes(production As String, testing As String)
        Assert.IsTrue(TestEnvironmentIsolation.GetInstallationDirectory(
            production, testing).EndsWith("\", StringComparison.Ordinal))
    End Sub

    <TestMethod>
    <DataRow("C:\GT-PROD", "c:\gt-prod\")>
    <DataRow("C:\GT-PROD", "C:\GT-PROD\Test")>
    <DataRow("C:\GT-PROD", "C:\")>
    <DataRow("C:\", "C:\GT-TEST")>
    <DataRow("C:\GT-PROD", "C:\autre\..\GT-PROD")>
    <DataRow("C:\GT-PROD", "\\serveur\partage\Test")>
    <DataRow("C:\GT-PROD", "Test")>
    <DataRow("C:\GT-PROD", "C:\GT-PROD. ")>
    Public Sub DossiersPartagesOuAmbigusRefuses(production As String, testing As String)
        Assert.ThrowsExactly(Of InvalidOperationException)(
            Sub()
                Dim unused = TestEnvironmentIsolation.GetInstallationDirectory(production, testing)
            End Sub)
    End Sub

    <TestMethod>
    Public Sub FichierRelatifResteDansTest()
        Assert.AreEqual("C:\GT-TEST\ressources\essai.log",
                        TestEnvironmentIsolation.GetFilePath(
                            "C:\GT-PROD", "C:\GT-TEST", "ressources\essai.log"))
    End Sub

    <TestMethod>
    <DataRow("..\GT-PROD\essai.json")>
    <DataRow("C:\GT-PROD\essai.json")>
    <DataRow("\essai.json")>
    <DataRow("essai.json:flux")>
    <DataRow(".. \GT-PROD\essai.json")>
    <DataRow("")>
    Public Sub SortiesDuDossierTestRefusees(relativePath As String)
        Assert.ThrowsExactly(Of InvalidOperationException)(
            Sub()
                Dim unused = TestEnvironmentIsolation.GetFilePath(
                    "C:\GT-PROD", "C:\GT-TEST", relativePath)
            End Sub)
    End Sub

    Private Const LocalTest As String =
        "Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=bddAgumaaaTest;Integrated Security=True"

    <TestMethod>
    Public Sub ConfigurationLocaleTestAccepteeSansOuverture()
        TestEnvironmentIsolation.ValidateConnectionString(LocalTest)
    End Sub

    <TestMethod>
    <DataRow("Data Source=serveur;Initial Catalog=bddAgumaaaTest;Integrated Security=True")>
    <DataRow("Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=bddAgumaaa;Integrated Security=True")>
    <DataRow("Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=bddAgumaaaTest;Integrated Security=False;User ID=fictif;Password=fictif")>
    <DataRow("Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=bddAgumaaaTest;Integrated Security=True;AttachDbFilename=C:\fictif.mdf")>
    <DataRow("configuration invalide")>
    Public Sub DestinationsSqlIncompatiblesRefusees(configuration As String)
        Assert.ThrowsExactly(Of InvalidOperationException)(
            Sub() TestEnvironmentIsolation.ValidateConnectionString(configuration))
    End Sub

    <TestMethod>
    Public Sub LecteurProprietesAppliqueLesControlesAuxVraisPointsDEntree()
        Dim flags = BindingFlags.NonPublic Or BindingFlags.Static
        Dim envField = GetType(LectureProprietes).GetField("_env", flags)
        Dim pathField = GetType(LectureProprietes).GetField("_repInstallation", flags)
        Dim settingsType = GetType(LectureProprietes).Assembly.GetType(
            "gestionTresorerie.My.MySettings", True)
        Dim settings = DirectCast(settingsType.GetProperty("Default").GetValue(Nothing),
                                  ApplicationSettingsBase)
        Dim names = New String() {"repInstallationProd", "repInstallationTest",
                                   "fichierLog", "DataSoure", "IntegratedSecurity"}
        Dim previousValues = Array.ConvertAll(names, Function(name) settings(name))
        Dim oldEnv = envField.GetValue(Nothing)
        Dim oldPath = pathField.GetValue(Nothing)
        Try
            envField.SetValue(Nothing, "TEST")
            pathField.SetValue(Nothing, Nothing)
            settings("repInstallationProd") = "C:\GT-PROD\"
            settings("repInstallationTest") = "C:\GT-PROD\"
            settings("fichierLog") = "essai.log"
            Assert.ThrowsExactly(Of InvalidOperationException)(
                Sub()
                    Dim unused = LectureProprietes.GetCheminEtVariable("fichierLog")
                End Sub)

            settings("repInstallationTest") = "C:\GT-TEST\"
            Assert.AreEqual("C:\GT-TEST\essai.log",
                            LectureProprietes.GetCheminEtVariable("fichierLog"))
            settings("fichierLog") = "..\GT-PROD\essai.log"
            Assert.ThrowsExactly(Of InvalidOperationException)(
                Sub()
                    Dim unused = LectureProprietes.GetCheminEtVariable("fichierLog")
                End Sub)

            settings("IntegratedSecurity") = "Integrated Security=True;"
            settings("DataSoure") = "Data Source=serveur-fictif;"
            Assert.ThrowsExactly(Of InvalidOperationException)(
                Sub()
                    Dim unused = LectureProprietes.connexionString("Agumaaa")
                End Sub)
            settings("DataSoure") = "Data Source=(LocalDB)\MSSQLLocalDB;"
#If DEMO_BUILD Then
            Assert.ThrowsExactly(Of InvalidOperationException)(
                Sub()
                    Dim unused = LectureProprietes.connexionString("Agumaaa")
                End Sub)
#Else
            TestEnvironmentIsolation.ValidateConnectionString(
                LectureProprietes.connexionString("Agumaaa"))
#End If
        Finally
            For index As Integer = 0 To names.Length - 1
                settings(names(index)) = previousValues(index)
            Next
            envField.SetValue(Nothing, oldEnv)
            pathField.SetValue(Nothing, oldPath)
        End Try
    End Sub
End Class
