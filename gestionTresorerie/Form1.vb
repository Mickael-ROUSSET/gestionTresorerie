Imports System.Collections.Specialized.BitVector32
Imports System.Text.RegularExpressions
Imports Windows.Win32.System
Imports System.Data.SqlClient
Imports System.Data.Common
Imports System.Configuration
Imports System.IO


Public Class FrmSaisie

    Inherits System.Windows.Forms.Form
    'Create ADO.NET objects.
    Private myConn As SqlConnection
    Private myCmd As SqlCommand
    Private myReader As SqlDataReader
    Private results As String

    Public Property Properties As Object

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim sFicParam As String

        'Create a Connection object.
        'myConn = New SqlConnection("Initial Catalog=Northwind;" & "Data Source=localhost;Integrated Security=SSPI;")
        'myConn = New SqlConnection("Data Source=G:\Mon Drive\AGUMAAA\Documents\BacASable\bddAgumaaa.mdf;Integrated Security=True;Connect Timeout=30")
        'chConnection = "Data Source = (LocalDB) \ MSSQLLocalDB;AttachDbFilename=" 
        myConn = New SqlConnection(GetConnectionString())
        'Create a Command object.
        'myCmd = myConn.CreateCommand
        'myCmd.CommandText = "SELECT FirstName, LastName FROM Employees"

        ''Open the connection.
        'myConn.Open()
        'myReader = myCmd.ExecuteReader() 
        ''Close the reader and the database connection.
        'myReader.Close()
        'myConn.Close()

        sFicParam = My.Settings.ficRessources
        For Each Section In GestionFichierIni.SectionNames(sFicParam)
            Me.lstCategorie.Items.Add(Section)
        Next
        'Chargement du fichier contenant la liste des tiers
        Call chargeFichierTexte(Me.lstTiers, My.Settings.ficTiers)
        'Chargement du fichier contenant la liste des événements
        Call chargeFichierTexte(Me.lstEvénement, My.Settings.ficEvénement)
    End Sub

    Private Sub chargeFichierTexte(lstBox As ListBox, fichierTexte As String)
        'Throw New NotImplementedException()

        Try
            Dim monStreamReader As New StreamReader(fichierTexte) 'Stream pour la lecture
            Dim ligne As String ' Variable contenant le texte de la ligne
            Do
                ligne = monStreamReader.ReadLine
                lstBox.Items.Add(ligne)
                'MsgBox(ligne)
            Loop Until ligne Is Nothing
            monStreamReader.Close()
        Catch ex As Exception
            MsgBox("Une erreur est survenue au cours de l'accès en lecture du fichier de configuration du logiciel." & vbCrLf & vbCrLf & "Veuillez vérifier l'emplacement : " & fichierTexte, MsgBoxStyle.Critical, "Erreur lors e l'ouverture du fichier conf...")
        End Try
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
        myReader.Close()
        myConn.Close()

    End Sub
    Private Sub LstCategorie_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lstCategorie.SelectedIndexChanged
        Me.lstSousCategorie.Items.Clear()
        ChargeListBox(Me.lstSousCategorie, Me.lstCategorie.SelectedItem)
    End Sub
    Private Sub ChargeListBox(listBox As ListBox, categorie As String)
        Dim sFicParam As String

        sFicParam = My.Settings.ficRessources
        For Each valeur In GestionFichierIni.SectionKeys(sFicParam, categorie)
            listBox.Items.Add(GestionFichierIni.ReadValue(sFicParam, Me.lstCategorie.SelectedItem, valeur))
        Next
    End Sub

    Private Sub BtnValider_Click(sender As Object, e As EventArgs) Handles btnValider.Click
        'Enregistre les informations sur le mouvement saisies
        'myCmd.CommandText = "insert into Mouvements (categorie) values "
        Try
            'Dim myCmd As New SqlCommand
            myCmd = New SqlCommand
            Call CreeConnexion()
            myCmd.Connection = myConn
            'Call LectureBase()
            'myCmd.CommandText = "INSERT INTO [dbo].[Mouvements] ([categorie],[montant]) values (@Catégorie, @montant)"
            'myCmd.CommandText = "INSERT INTO [dbo].[Mouvements] ([Id],[categorie]) values (@Id,@Catégorie)"
            'myCmd.CommandText = "INSERT INTO [dbo].[Mouvements] (note, catégorie) VALUES (@note, @categorie);"
            'myCmd.CommandText = "INSERT INTO [dbo].[Mouvements] (note, catégorie, sousCatégorie) VALUES (@note, @categorie, @sousCategorie);"
            'myCmd.CommandText = "INSERT INTO [dbo].[Mouvements] (note, catégorie, sousCatégorie, tiers) VALUES (@note, @categorie, @sousCategorie, @tiers);"
            'myCmd.CommandText = "INSERT INTO [dbo].[Mouvements] (note, catégorie, sousCatégorie, tiers,dateCréation) VALUES (@note, @categorie, @sousCategorie, @tiers, @dateCréation);"
            'myCmd.CommandText = "INSERT INTO [dbo].[Mouvements] (note, catégorie, sousCatégorie, tiers,dateCréation,dateMvt) VALUES (@note, @categorie, @sousCategorie, @tiers, @dateCréation, @dateMvt);"
            'myCmd.CommandText = "INSERT INTO [dbo].[Mouvements] (note, catégorie, sousCatégorie, tiers,dateCréation,dateMvt,montant) VALUES (@note, @categorie, @sousCategorie, @tiers, @dateCréation, @dateMvt, @montant);"
            'myCmd.CommandText = "INSERT INTO [dbo].[Mouvements] (note, catégorie, sousCatégorie, tiers,dateCréation,dateMvt,montant,sens) VALUES (@note, @categorie, @sousCategorie, @tiers, @dateCréation, @dateMvt, @montant, @sens);"
            'myCmd.CommandText = "INSERT INTO [dbo].[Mouvements] (note, catégorie, sousCatégorie, tiers,dateCréation,dateMvt,montant,sens,etat) VALUES (@note, @categorie, @sousCategorie, @tiers, @dateCréation, @dateMvt, @montant, @senst, @etat);"
            myCmd.CommandText = "INSERT INTO [dbo].[Mouvements] (note, catégorie, sousCatégorie, tiers,dateCréation,dateMvt,montant,sens,etat,événement) VALUES (@note, @categorie, @sousCategorie, @tiers, @dateCréation, @dateMvt, @montant, @sens, @etat, @événement);"
            'myCmd.CommandText = "INSERT INTO [dbo].[Mouvements] (Id) VALUES ('h');"
            myCmd.Parameters.Clear()
            myCmd.Parameters.Add("@note", SqlDbType.NVarChar)
            myCmd.Parameters(0).Value = "Note de test"
            myCmd.Parameters.Add("@categorie", SqlDbType.VarChar)
            myCmd.Parameters(1).Value = lstCategorie.SelectedItem
            myCmd.Parameters.Add("@sousCategorie", SqlDbType.VarChar)
            myCmd.Parameters(2).Value = lstSousCategorie.SelectedItem
            myCmd.Parameters.Add("@tiers", SqlDbType.VarChar)
            'myCmd.Parameters(3).Value = lstTiers.SelectedItem
            myCmd.Parameters(3).Value = "Tiers de test"
            myCmd.Parameters.Add("@dateCréation", SqlDbType.Date)
            myCmd.Parameters(4).Value = Now.Date
            myCmd.Parameters.Add("@dateMvt", SqlDbType.Date)
            myCmd.Parameters(5).Value = dateMvt.Value
            myCmd.Parameters.Add("@montant", SqlDbType.Decimal)
            myCmd.Parameters(6).Value = txtMontant.Text
            myCmd.Parameters.Add("@sens", SqlDbType.Bit)
            myCmd.Parameters(7).Value = IIf(rbCredit.Checked, 1, 0)
            myCmd.Parameters.Add("@etat", SqlDbType.Bit)
            myCmd.Parameters(8).Value = IIf(rbRapproche.Checked, 1, 0)
            myCmd.Parameters.Add("@événement", SqlDbType.VarChar)
            myCmd.Parameters(9).Value = lstEvénement.SelectedItem

            'myCmd.Parameters.Add("@Catégorie", SqlDbType.NVarChar)
            'myCmd.Parameters(1).Value = lstCategorie.SelectedItem
            'myCmd.Parameters.Add("@Montant", SqlDbType.Decimal)
            'myCmd.Parameters(1).Value = txtMontant.Text
            'For i As Integer = 0 To dgvElementSalaire.Rows.Count - 1
            '    cmd.Parameters(0).Value = dgvElementSalaire.Rows(i).Cells(5).Value
            '    cmd.ExecuteNonQuery()
            'Next
            myCmd.ExecuteNonQuery()
            MsgBox("Ajout effectué avec succès")
        Catch ex As Exception
            Console.WriteLine(ex.Message)
            End
        End Try

    End Sub
    Private Sub LectureBase()
        Dim myReader As SqlDataReader
        'Create a Command object.  
        'Dim results As String

        'myCmd.ExecuteNonQuery()
        myCmd.CommandText = "SELECT Id FROM Mouvements;"

        myReader = myCmd.ExecuteReader()
        'Concatenate the query result into a string.
        Do While myReader.Read()
            'results = results & myReader.GetString(0) & vbTab & myReader.GetString(1) & vbLf
            'results = results & myReader.GetString(0)
            MsgBox(myReader.GetString(0))
        Loop
        myReader.Close()
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

    Private Sub TxtMontant_TextChanged(sender As Object, e As EventArgs) Handles txtMontant.Leave

        'If Not Regex.Match(txtMontant.Text, "^[0-9]*$", RegexOptions.IgnoreCase).Success Then
        If Not Regex.Match(txtMontant.Text, "^[0-9]+(,[0-9]{0,2})*$", RegexOptions.IgnoreCase).Success Then
            MessageBox.Show("Le montant doit être numérique!")
            'Remet le focus su rla zone de saisie du montant
            txtMontant.Focus()
        End If
    End Sub

End Class
