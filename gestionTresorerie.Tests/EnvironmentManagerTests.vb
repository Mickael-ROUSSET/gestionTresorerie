Option Strict On
Option Explicit On

Imports System
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass, DoNotParallelize>
Public Class EnvironmentManagerTests
    <DataTestMethod>
    <DataRow("TEST", "PROD", "PROD", "PROD", EnvironnementType.TEST)>
    <DataRow("", "TEST", "PROD", "PROD", EnvironnementType.TEST)>
    <DataRow("", "", "TEST", "PROD", EnvironnementType.TEST)>
    <DataRow("", "", "", "TEST", EnvironnementType.TEST)>
    <DataRow(Nothing, Nothing, Nothing, Nothing, EnvironnementType.DEV)>
    <DataRow("  test  ", "", "", "", EnvironnementType.TEST)>
    <DataRow("DEV", "PROD", "", "", EnvironnementType.DEV)>
    <DataRow("PROD", "", "", "", EnvironnementType.PROD)>
    Public Sub ResolutionRespectePrioritesEtCompatibilite(
        userValue As String, machineValue As String,
        legacyMachineValue As String, legacyUserValue As String,
        expected As EnvironnementType)
        Assert.AreEqual(expected, ResolveEnvironment(
            userValue, machineValue, legacyMachineValue, legacyUserValue))
    End Sub

    <TestMethod>
    Public Sub ValeurInconnueEmpecheLeRepliVersProduction()
        Assert.ThrowsExactly(Of InvalidOperationException)(
            Sub()
                Dim unused = ResolveEnvironment("INCONNU", "PROD", "PROD", "PROD")
            End Sub)
        Assert.ThrowsExactly(Of InvalidOperationException)(
            Sub()
                Dim unused = ResolveEnvironment("", "", "INCONNU", "PROD")
            End Sub)
    End Sub

#If DEMO_BUILD Then
    <TestMethod>
    Public Sub DemoBuild_IgnoreConfigurationExterneEtInterditPersistance()
        Dim previous = CurrentEnvironment
        Try
            LoadEnvironment(
                Function(name, target)
                    Assert.Fail("La démonstration ne doit pas lire la configuration externe.")
                    Return "PROD"
                End Function)
            Assert.AreEqual(EnvironnementType.DEV, CurrentEnvironment)
            Assert.ThrowsExactly(Of InvalidOperationException)(
                Sub() SetEnvironment(
                    EnvironnementType.PROD,
                    Sub(name, value, target)
                        Assert.Fail("La démonstration ne doit pas écrire la configuration.")
                    End Sub))
            Assert.AreEqual(EnvironnementType.DEV, CurrentEnvironment)
        Finally
            CurrentEnvironment = previous
        End Try
    End Sub
#Else
    <TestMethod>
    Public Sub SelectionPuisRechargementConserveTestMalgreAncienneProductionMachine()
        Dim previous = CurrentEnvironment
        Dim persisted As String = Nothing
        Try
            SetEnvironment(
                EnvironnementType.TEST,
                Sub(name, value, target)
                    Assert.AreEqual("AGUMAAA_ENV", name)
                    Assert.AreEqual(EnvironmentVariableTarget.User, target)
                    persisted = value
                End Sub)
            Assert.AreEqual("TEST", persisted)
            CurrentEnvironment = EnvironnementType.DEV
            LoadEnvironment(
                Function(name, target)
                    If name = "AGUMAAA_ENV" AndAlso target = EnvironmentVariableTarget.User Then
                        Return persisted
                    End If
                    Return "PROD"
                End Function)
            Assert.AreEqual(EnvironnementType.TEST, CurrentEnvironment)
            Assert.AreEqual("Test", GetEnvironmentLabel())
        Finally
            CurrentEnvironment = previous
        End Try
    End Sub

    <TestMethod>
    Public Sub EchecPersistanceConserveEnvironnementActif()
        Dim previous = CurrentEnvironment
        Try
            CurrentEnvironment = EnvironnementType.TEST
            Assert.ThrowsExactly(Of UnauthorizedAccessException)(
                Sub() SetEnvironment(
                    EnvironnementType.PROD,
                    Sub(name, value, target)
                        Throw New UnauthorizedAccessException()
                    End Sub))
            Assert.AreEqual(EnvironnementType.TEST, CurrentEnvironment)
        Finally
            CurrentEnvironment = previous
        End Try
    End Sub

    <TestMethod>
    Public Sub EnumerationInvalideEstRefuseeAvantEcriture()
        Assert.ThrowsExactly(Of ArgumentOutOfRangeException)(
            Sub() SetEnvironment(
                CType(99, EnvironnementType),
                Sub(name, value, target)
                    Assert.Fail("Aucune écriture attendue pour une valeur invalide.")
                End Sub))
    End Sub
#End If
    Private NotInheritable Class PrincipaleSelectionFictive
        Inherits FrmPrincipale

        Public Property NombreDemandes As Integer

        Protected Overrides Function DemanderEnvironnement() As String
            NombreDemandes += 1
            Return "" ' Annulation sans persistance ni ouverture de dialogue.
        End Function
    End Class

    <TestMethod>
    Public Sub MenuPrincipalDeclencheLaSelectionSaufEnDemoBuild()
        Dim failure As Exception = Nothing
        Dim worker As New System.Threading.Thread(
            Sub()
                Try
                    ' Construction seule, sans chargement des secrets ni de la grille SQL.
                    Using form As New PrincipaleSelectionFictive()
                        Dim menu = DirectCast(form.MainMenuStrip.Items.Find(
                            "EnvironnementToolStripMenuItem", True)(0),
                            System.Windows.Forms.ToolStripMenuItem)
                        menu.PerformClick()
#If DEMO_BUILD Then
                        Assert.AreEqual(0, form.NombreDemandes)
#Else
                        Assert.AreEqual(1, form.NombreDemandes)
#End If
                        Assert.IsFalse(form.IsHandleCreated)
                    End Using
                Catch ex As Exception
                    failure = ex
                End Try
            End Sub)
        worker.SetApartmentState(System.Threading.ApartmentState.STA)
        worker.Start()
        Assert.IsTrue(worker.Join(TimeSpan.FromSeconds(10)), "Le menu ne doit pas bloquer.")
        If failure IsNot Nothing Then
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(failure).Throw()
        End If
    End Sub
End Class
