Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System.IO
Imports System.Windows.Forms
Imports gestionTresorerie ' Assurez-vous d'importer l'espace de noms correct
Imports Microsoft.VisualStudio.TestTools.UnitTesting.PrivateObject

<TestClass>
Public Class FrmChargeRelevéTests

    Private _frmChargeRelevé As FrmChargeRelevé
    Private _dgvRelevé As DataGridView
    Private _privateObject As PrivateObject

    <TestInitialize>
    Public Sub Setup()
        ' Initialiser les composants nécessaires
        _frmChargeRelevé = New FrmChargeRelevé()
        _dgvRelevé = New DataGridView()

        ' Associer le DataGridView au formulaire
        _frmChargeRelevé.dgvRelevé = _dgvRelevé

        ' Initialiser PrivateObject pour accéder aux méthodes privées
        _privateObject = New PrivateObject(_frmChargeRelevé)
    End Sub

    <TestMethod>
    Public Sub TestAlimenteLstMvtCA_FichierValide()
        ' Arrange
        Dim fichierTest As String = Path.Combine(Path.GetTempPath(), "test.csv")
        File.WriteAllText(fichierTest, "2023-01-01;Note1;100;;" & vbCrLf & "2023-01-02;Note2;;200;")

        ' Act
        _frmChargeRelevé.AlimenteLstMvtCA(fichierTest)

        ' Assert
        Dim dgv As DataGridView = _frmChargeRelevé.dgvRelevé()
        Assert.AreEqual(2, dgv.Rows.Count)
        Assert.AreEqual("2023-01-01", dgv.Rows(0).Cells(0).Value)
        Assert.AreEqual("Note1", dgv.Rows(0).Cells(1).Value)
        Assert.AreEqual("100", dgv.Rows(0).Cells(2).Value)
    End Sub

    <TestMethod>
    Public Sub TestAlimenteLstMvtCA_FichierInvalide()
        ' Arrange
        Dim fichierTest As String = Path.Combine(Path.GetTempPath(), "test_invalid.csv")
        File.WriteAllText(fichierTest, "Invalid content")

        ' Act & Assert
        Assert.ThrowsException(Of Exception)(Sub() _frmChargeRelevé.AlimenteLstMvtCA(fichierTest))
    End Sub

    <TestMethod>
    Public Sub TestAjouterColonneTraite()
        ' Arrange
        ' Ajouter 4 colonnes au DataGridView
        _dgvRelevé.Columns.Add("Date", "Date")
        _dgvRelevé.Columns.Add("Note", "Note")
        _dgvRelevé.Columns.Add("MontantDebit", "MontantDebit")
        _dgvRelevé.Columns.Add("MontantCredit", "MontantCredit")

        ' Ajouter des lignes au DataGridView
        _dgvRelevé.Rows.Add("2023-01-01", "Note1", "100", "")
        _dgvRelevé.Rows.Add("2023-01-02", "Note2", "", "200")

        ' Act
        _privateObject.Invoke("AjouterColonneTraite")

        ' Assert
        Assert.IsTrue(_dgvRelevé.Columns.Contains("traiteImage"))
        Assert.IsNotNull(_dgvRelevé.Rows(0).Cells("traiteImage").Value)
        Assert.IsNotNull(_dgvRelevé.Rows(1).Cells("traiteImage").Value)
    End Sub

End Class
