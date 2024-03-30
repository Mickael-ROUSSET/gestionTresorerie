Imports System.Data.Common
Imports System.Data.SqlClient
Imports System.IO
Imports System.Windows.Forms.VisualStyles.VisualStyleElement

Public Class FrmPrincipale

    Inherits System.Windows.Forms.Form
    'Create ADO.NET objects.
    Public myConn As SqlConnection
    Private myCmd As SqlCommand
    Private myReader As SqlDataReader
    Private results As String

    Public Property Properties As Object

    Private Sub BtnHistogramme_Click(sender As Object, e As EventArgs) Handles btnHistogramme.Click
        Call LectureBase()
    End Sub
    Private Sub FrmPrincipale_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        myConn = New SqlConnection(GetConnectionString())
        Call CreeConnexion()
    End Sub
    Private Function GetConnectionString() As String
        ' To avoid storing the connection string in your code, you can retrieve it from a configuration file.
        'Return "Data Source=MSSQL1;Initial Catalog=AdventureWorks;" & "Integrated Security=true;"
        'Return "Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename="G:\Mon Drive\AGUMAAA\Documents\BacASable\bddAgumaaa.mdf";Integrated Security=True;Connect Timeout=30"
        Dim builder As New System.Data.SqlClient.SqlConnectionStringBuilder
        builder("Data Source") = "(LocalDB)\MSSQLLocalDB"
        builder("AttachDbFilename") = My.Settings.ficBddDonnees
        builder("Integrated Security") = True
        builder("Connect Timeout") = 30
        Return builder.ConnectionString
    End Function
    Private Sub LectureBase()
        Dim myReaderCategorie As SqlDataReader
        Dim myReaderSousCategorie As SqlDataReader
        Dim tabLegendes() As String = Array.Empty(Of String)()
        Dim tabCatégorie() As String = Array.Empty(Of String)()
        Dim tabValeurs() As Decimal = Array.Empty(Of Decimal)()
        Dim myCmdCategorie As SqlCommand
        Dim myCmdSousCategorie As SqlCommand
        Dim i As Integer, iNbCat As Integer
        Dim sNomImage As String

        myCmdCategorie = New SqlCommand("SELECT distinct catégorie FROM Mouvements ;", myConn)
        myReaderCategorie = myCmdCategorie.ExecuteReader()
        Do While myReaderCategorie.Read()
            ReDim Preserve tabCatégorie(UBound(tabCatégorie) + 1)
            tabCatégorie(i) = myReaderCategorie.GetSqlString(0)
            i += 1
        Loop
        myReaderCategorie.Close()

        For iNbCat = 0 To UBound(tabCatégorie)
            ReDim tabLegendes(0)
            ReDim tabValeurs(0)
            myCmdSousCategorie = New SqlCommand("SELECT sousCatégorie, sum(montant) FROM Mouvements where catégorie = '" & tabCatégorie(iNbCat) & "' group by sousCatégorie;", myConn)
            myReaderSousCategorie = myCmdSousCategorie.ExecuteReader()
            Do While myReaderSousCategorie.Read()
                i = 0
                Try
                    ReDim Preserve tabLegendes(UBound(tabLegendes) + 1)
                    tabLegendes(i) = CStr(myReaderSousCategorie.GetSqlString(0))
                    ReDim Preserve tabValeurs(UBound(tabValeurs) + 1)
                    tabValeurs(i) = myReaderSousCategorie.GetDecimal(1)
                Catch ex As Exception
                    Console.WriteLine(ex.Message)
                End Try
                i += 1
            Loop
            myReaderSousCategorie.Close()
            'Supprime tous les controls de la fenêtre (=> les images précédentes)
            Call SupprimeControlesfenetre(frmHistogramme)
            Call frmHistogramme.Histogramme("Montants par sous-catégorie : " & tabCatégorie(iNbCat), tabValeurs, tabLegendes, 20, 40, 500, 30, 10)
            'frmHistogramme.ShotActiveWin.Save("C:\Users\User\source\repos\gestionTresorerie\gestionTresorerie\image" & "Montants par sous-catégorie" & ".bmp")
            'Call frmHistogramme.testHisto()
            frmHistogramme.Show()
            sNomImage = "C:\Users\User\Downloads\frmHistogramme" & iNbCat & ".png"
            Using bmp = New Bitmap(frmHistogramme.Width, frmHistogramme.Height)
                frmHistogramme.DrawToBitmap(bmp, New Rectangle(0, 0, bmp.Width, bmp.Height))
                Try
                    Kill(sNomImage)
                Catch
                    'Ne fait rien : c'est que le fichier à supprimer ne devait pas exister
                End Try
                bmp.Save(sNomImage)
            End Using
        Next iNbCat

        myReaderCategorie.Close()
    End Sub
    Private Sub SupprimeControlesfenetre(frm As Form)
        Dim i As Integer, nbControls As Integer

        nbControls = frm.Controls.Count
        For i = 0 To nbControls - 1
            frm.Controls.Remove(frm.Controls.Item(0))
            'Debug.Print("Name : " & frm.Controls.Item(0).Name)
        Next i
    End Sub
    Private Sub FrmMain_Closing(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles MyBase.Closing
        Try
            Call SuprimeConnexion()
        Catch
        End Try
    End Sub
    Private Sub CreeConnexion()
        'Open the connection.
        myConn.Open()
    End Sub
    Private Sub SuprimeConnexion()
        'Close the reader and the database connection. 
        myConn.Close()
    End Sub
    Private Sub BtnSaisie_Click(sender As Object, e As EventArgs) Handles btnSaisie.Click
        FrmSaisie.Show()
    End Sub

    Private Sub BtnChargeRelevé_Click(sender As Object, e As EventArgs) Handles btnChargeRelevé.Click
        FrmChargeRelevé.Show()
    End Sub

    Private Sub BindingSource1_CurrentChanged(sender As Object, e As EventArgs) Handles BindingSource1.CurrentChanged

    End Sub

    Private Sub BtnConsultation_Click(sender As Object, e As EventArgs) Handles btnConsultation.Click
        ' Evite de définir la chaine de connexion à chaque endroit où tu l'utilises : si tu dois la changer,
        ' ça fait autant d'endroits à modifier, et ça force à recompiler. Il vaut mieux la définir dans les
        ' paramètres de l'application, comme ça si tu dois la changer tu n'auras qu'un seul endroit à modifier.

        'TODO : bonne idée
        'Dim connectString As String = My.Settings.ChaineDeConnection
        'Dim connection As New System.Data.SqlClient.SqlConnection(connectString)

        ' Essaie de taper une apostrophe (') dans TextBox1, et observe le résultat ;)
        ' Ensuite, va faire un tour ici pour apprendre à régler le problème :
        ' http://johannblais.developpez.com/tutoriel/dotnet/bonnes-pratiques-acces-donnees/#LIV
        Dim command As New System.Data.SqlClient.SqlCommand("SELECT * FROM Mouvements", myConn)

        Dim dt As New DataTable
        Dim adpt As New Data.SqlClient.SqlDataAdapter(command)

        Try
            ' Place la connection dans le bloc try : c'est typiquement le genre d'instruction qui peut lever une exception. 
            adpt.Fill(dt)
            DataGridView1.DataSource = dt
        Catch ex As SqlException
            ' On informe l'utilisateur qu'il y a eu un problème :
            MessageBox.Show("Une erreur s'est produite lors du chargement des données !" & vbCrLf & ex.ToString())
        Finally
            ' Le code du bloc Finally est toujours exécuté, même en cas d'erreur dans le Try
            ' On y place donc la fermeture de la connection :
            'myConn.Close()
        End Try
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles btnCreeBilans.Click
        'Call CreeBilans()
        'Call genereBilans.AjouteImage()
        Call genereBilans.GenereBilanStructure()
    End Sub
End Class