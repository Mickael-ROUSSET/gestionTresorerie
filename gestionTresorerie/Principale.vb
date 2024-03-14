Imports System.Data.Common
Imports System.Data.SqlClient
Imports System.IO
Imports System.Windows.Forms.VisualStyles.VisualStyleElement

Public Class frmPrincipale

    Inherits System.Windows.Forms.Form
    'Create ADO.NET objects.
    Public myConn As SqlConnection
    Private myCmd As SqlCommand
    Private myReader As SqlDataReader
    Private results As String

    Public Property Properties As Object

    Private Sub btnHistogramme_Click(sender As Object, e As EventArgs) Handles btnHistogramme.Click
        Call LectureBase()
    End Sub
    Private Sub frmPrincipale_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        myConn = New SqlConnection(GetConnectionString())
        Call CreeConnexion()
    End Sub
    Private Function GetConnectionString() As String
        ' To avoid storing the connection string in your code,
        ' you can retrieve it from a configuration file.
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
        Dim myReader As SqlDataReader
        Dim tabLegendes() As String = {}
        Dim tabValeurs() As Decimal = {}
        Dim i As Integer = 0
        'Create a Command object.  
        'Dim results As String

        'myCmd.ExecuteNonQuery()
        'myCmd.CommandText = "SELECT id,montant FROM Mouvements;"
        myCmd = New SqlCommand("SELECT id,montant FROM Mouvements;", myConn)

        myReader = myCmd.ExecuteReader()
        'Concatenate the query result into a string.
        Do While myReader.Read()
            Try
                ReDim Preserve tabLegendes(UBound(tabLegendes) + 1)
                tabLegendes(i) = CStr(myReader.GetInt32(0))
                ReDim Preserve tabValeurs(UBound(tabValeurs) + 1)
                tabValeurs(i) = myReader.GetDecimal(1)
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try
            i += 1
        Loop
        Call frmHistogramme.Histogramme("Montant par id", tabValeurs, tabLegendes, 20, 40, 300, 30, 10)
        frmHistogramme.Show()
        myReader.Close()
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
        'myReader.Close()
        myConn.Close()
    End Sub
    Private Sub btnSaisie_Click(sender As Object, e As EventArgs) Handles btnSaisie.Click
        FrmSaisie.Show()
    End Sub

    Private Sub btnChargeRelevé_Click(sender As Object, e As EventArgs) Handles btnChargeRelevé.Click
        frmChargeRelevé.Show()
    End Sub

    Private Sub BindingSource1_CurrentChanged(sender As Object, e As EventArgs) Handles BindingSource1.CurrentChanged

    End Sub

    Private Sub btnConsultation_Click(sender As Object, e As EventArgs) Handles btnConsultation.Click
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
End Class