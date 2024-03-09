Imports System.Security

Imports System.ComponentModel
Imports System.Diagnostics
Imports System.IO
Imports System.Windows.Forms
Imports System.Text.RegularExpressions
Module LitExcel

    Dim WithEvents selectButton As Button
    Dim openFileDialog1 As OpenFileDialog

    'Public Sub Main()
    '    Application.SetCompatibleTextRenderingDefault(False)
    '    Application.EnableVisualStyles()
    '    Dim frm As New OpenFileDialogForm()
    '    Application.Run(frm)
    'End Sub

    Sub New()
        openFileDialog1 = New OpenFileDialog() With
        {
           .FileName = "Sélectionner un fichier csv",
           .Filter = "Text files (*.csv)|*.csv",
           .Title = "Open csv file"
        }

        selectButton = New Button() With {.Text = "Select file"}
    End Sub

    Public Sub ouvreFichier()
        If openFileDialog1.ShowDialog() = DialogResult.OK Then
            Try
                Dim filePath = openFileDialog1.FileName
                'Using str = openFileDialog1.OpenFile()
                '    Process.Start("notepad.exe", filePath)
                'End Using
                'Using str = openFileDialog1.OpenFile()
                '    Process.Start("notepad.exe", filePath)
                Call traiteFichier(openFileDialog1.FileName)
                'End Using
            Catch SecEx As SecurityException
                MessageBox.Show($"Security error:{vbCrLf}{vbCrLf}{SecEx.Message}{vbCrLf}{vbCrLf}" &
                $"Details:{vbCrLf}{vbCrLf}{SecEx.StackTrace}")
            End Try
        End If
    End Sub
    Private Sub traiteFichier(sFichier As String)
        Dim sLigneEntiere As String = ""
        Dim bLigne1 As Boolean = True
        Try
            Dim monStreamReader As New StreamReader(sFichier) 'Stream pour la lecture
            Dim ligne As String ' Variable contenant le texte de la ligne
            Dim file As System.IO.StreamWriter
            file = My.Computer.FileSystem.OpenTextFileWriter("C:\Users\User\Downloads\test.txt", True)

            ligne = monStreamReader.ReadLine
            While Not ligne Is Nothing
                ' Match ignoring case of letters.
                Dim match As Match = Regex.Match(ligne, "^[0-3][0-9]/[0-9]{2}/[0-9]{4};", RegexOptions.IgnoreCase)
                If match.Success Then
                    'On est au début d'une ligne => on écrit le ligne en cours et on en commence une autre
                    'Mais pas sur la 1ère qui contient des entêtes
                    If bLigne1 = False Then
                        file.WriteLine(sLigneEntiere)
                    Else
                        bLigne1 = False
                    End If
                    sLigneEntiere = ligne
                Else
                    sLigneEntiere = sLigneEntiere & ligne
                End If
                ligne = monStreamReader.ReadLine
            End While
            monStreamReader.Close()
            file.Close()
        Catch ex As Exception
            MsgBox("Une erreur est survenue sur a lecture du relevé : " & sFichier, MsgBoxStyle.Critical)
        End Try
    End Sub
End Module
