Imports System.Security

Imports System.ComponentModel
Imports System.Diagnostics
Imports System.IO
Imports System.Windows.Forms
Imports System.Text.RegularExpressions
Module LitRelevé

    Dim WithEvents SelectButton As Button
    ReadOnly openFileDialog1 As OpenFileDialog

    Sub New()
        openFileDialog1 = New OpenFileDialog() With
        {
           .FileName = "Sélectionner un fichier csv",
           .Filter = "Text files (*.csv)|*.csv",
           .Title = "Open csv file"
        }

        SelectButton = New Button() With {.Text = "Select file"}
    End Sub

    Public Function OuvreFichier() As String
        Dim sFicTemp As String = ""
        'Un seul fichier peut être sélectionné
        openFileDialog1.Multiselect = False
        If openFileDialog1.ShowDialog() = DialogResult.OK Then
            Try
                Dim filePath = openFileDialog1.FileName
                'Using str = openFileDialog1.OpenFile()
                '    Process.Start("notepad.exe", filePath)
                'End Using 
                sFicTemp = TraiteFichier(openFileDialog1.FileName)
            Catch SecEx As SecurityException
                MessageBox.Show($"Security error:{vbCrLf}{vbCrLf}{SecEx.Message}{vbCrLf}{vbCrLf}" &
                $"Details:{vbCrLf}{vbCrLf}{SecEx.StackTrace}")
            End Try
        End If
        Return sFicTemp
    End Function
    Private Function TraiteFichier(sFichier As String) As String
        Dim sLigneEntiere As String = ""
        Dim bLigne1 As Boolean = True
        Dim sFicTemp As String = Path.GetTempPath() & "test.txt"
        Try
            Dim monStreamReader As New StreamReader(sFichier) 'Stream pour la lecture
            Dim ligne As String ' Variable contenant le texte de la ligne
            Dim file As System.IO.StreamWriter
            file = My.Computer.FileSystem.OpenTextFileWriter(sFicTemp, True)

            ligne = monStreamReader.ReadLine
            While ligne IsNot Nothing
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
                    sLigneEntiere &= ligne
                End If
                ligne = monStreamReader.ReadLine
            End While
            monStreamReader.Close()
            file.Close()
        Catch ex As Exception
            MsgBox("Une erreur est survenue sur a lecture du relevé : " & sFichier, MsgBoxStyle.Critical)
        End Try
        Return sFicTemp
    End Function

End Module
