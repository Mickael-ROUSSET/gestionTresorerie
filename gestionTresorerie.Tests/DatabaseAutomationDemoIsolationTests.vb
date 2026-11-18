Option Strict On
Option Explicit On

Imports System
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class DatabaseAutomationDemoIsolationTests
#If DEMO_BUILD Then
    <TestMethod>
    Public Sub DemoBuild_RefuseSauvegardeRestaurationEtAttachementSQL()
        ' Les trois appels doivent échouer avant toute boîte de dialogue,
        ' lecture de fichiers ou tentative de connexion à une base réelle.
        Dim sauvegarde = Assert.ThrowsExactly(Of InvalidOperationException)(
            Sub() DatabaseAutomation.ExportDatabase())
        StringAssert.Contains(sauvegarde.Message, "démonstration")

        Dim restauration = Assert.ThrowsExactly(Of InvalidOperationException)(
            Sub() DatabaseAutomation.ImportDatabase())
        StringAssert.Contains(restauration.Message, "démonstration")

        Dim attachement = Assert.ThrowsExactly(Of InvalidOperationException)(
            Sub()
                Dim inutilise = DatabaseAutomation.AttachDatabaseAuto(
                    "Z:\\chemin-fictif\\inexistant.mdf",
                    "Z:\\chemin-fictif\\inexistant.ldf")
            End Sub)
        StringAssert.Contains(attachement.Message, "démonstration")
    End Sub
#End If
End Class
