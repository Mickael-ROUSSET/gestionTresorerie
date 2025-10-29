Imports System.IO
Imports System.Windows.Forms
Imports PdfiumViewer

Public Class DocumentViewerManager
    Implements IDisposable

    ' ----- EVENEMENTS -----
    Public Event DocumentLoaded()
    Public Event DocumentFailed(ex As Exception)

    ' Contrôles gérés
    Private ReadOnly _parent As Control
    Private _pdfViewer As PdfViewer
    Private _webBrowser As WebBrowser
    Private _pictureBox As PictureBox

    ' Gestion des fichiers temporaires PDF
    Private _tempPdfFiles As New List(Of String)
    Private _disposed As Boolean = False

    Public Sub New(parent As Control, Optional dock As DockStyle = DockStyle.Fill)
        _parent = parent
        CreateControls(dock)
        AddHandler _parent.HandleDestroyed, AddressOf Parent_HandleDestroyed
    End Sub

    ' =============================
    ' Création / réutilisation des contrôles
    ' =============================
    Private Sub CreateControls(dock As DockStyle)
        _pictureBox = _parent.Controls.OfType(Of PictureBox).FirstOrDefault()
        If _pictureBox Is Nothing Then
            _pictureBox = New PictureBox() With {
                .Dock = dock,
                .Visible = False,
                .SizeMode = PictureBoxSizeMode.Zoom
            }
            _parent.Controls.Add(_pictureBox)
        Else
            _pictureBox.Visible = False
            _pictureBox.SizeMode = PictureBoxSizeMode.Zoom
        End If

        _webBrowser = _parent.Controls.OfType(Of WebBrowser).FirstOrDefault()
        If _webBrowser Is Nothing Then
            _webBrowser = New WebBrowser() With {
                .Dock = dock,
                .Visible = False,
                .AllowWebBrowserDrop = False
            }
            _parent.Controls.Add(_webBrowser)
        Else
            _webBrowser.Visible = False
        End If

        RemoveHandler _webBrowser.DocumentCompleted, AddressOf WebBrowser_DocumentCompleted
        AddHandler _webBrowser.DocumentCompleted, AddressOf WebBrowser_DocumentCompleted

        _pdfViewer = _parent.Controls.OfType(Of PdfiumViewer.PdfViewer).FirstOrDefault()
        If _pdfViewer Is Nothing Then
            _pdfViewer = New PdfiumViewer.PdfViewer() With {
                .Dock = dock,
                .Visible = False
            }
            _parent.Controls.Add(_pdfViewer)
        Else
            _pdfViewer.Visible = False
        End If
    End Sub

    ' =============================
    ' Fermeture et nettoyage
    ' =============================
    Public Sub FermerDocument()
        Try
            If _pdfViewer IsNot Nothing AndAlso _pdfViewer.Document IsNot Nothing Then
                _pdfViewer.Document.Dispose()
                _pdfViewer.Document = Nothing
            End If
            If _pdfViewer IsNot Nothing Then _pdfViewer.Visible = False

            If _webBrowser IsNot Nothing Then
                Try
                    _webBrowser.Stop()
                    _webBrowser.Navigate("about:blank")
                Catch
                End Try
                _webBrowser.Visible = False
            End If

            If _pictureBox IsNot Nothing AndAlso _pictureBox.Image IsNot Nothing Then
                _pictureBox.Image.Dispose()
                _pictureBox.Image = Nothing
            End If
            If _pictureBox IsNot Nothing Then _pictureBox.Visible = False

            DeleteAllTempFiles()

            Dim form = _parent.FindForm()
            If form IsNot Nothing Then form.ActiveControl = Nothing
        Catch ex As Exception
            ' En cas d'erreur dans la fermeture, signale mais ne throw pas
            RaiseEvent DocumentFailed(ex)
        End Try
    End Sub

    Private Sub DeleteAllTempFiles()
        For Each fichier In _tempPdfFiles
            Try
                If File.Exists(fichier) Then File.Delete(fichier)
            Catch
            End Try
        Next
        _tempPdfFiles.Clear()
    End Sub

    Private Sub Parent_HandleDestroyed(sender As Object, e As EventArgs)
        DeleteAllTempFiles()
    End Sub

    ' =============================
    ' Affichage d’un document Base64
    ' =============================
    Public Sub AfficherDocumentBase64(base64Data As String)
        If String.IsNullOrWhiteSpace(base64Data) Then
            RaiseEvent DocumentFailed(New Exception("Aucun document à afficher."))
            Return
        End If

        If _parent.InvokeRequired Then
            _parent.Invoke(New Action(Of String)(AddressOf AfficherDocumentBase64), base64Data)
            Return
        End If

        FermerDocument()

        Try
            Dim bytes() As Byte = Convert.FromBase64String(base64Data)

            Dim isPdf As Boolean = bytes.Length > 4 AndAlso bytes(0) = &H25 AndAlso bytes(1) = &H50 AndAlso bytes(2) = &H44 AndAlso bytes(3) = &H46

            If isPdf Then
                Try
                    Using ms As New MemoryStream(bytes)
                        Dim doc = PdfDocument.Load(ms)
                        _pdfViewer.Document = doc
                        _pdfViewer.Visible = True
                        _pdfViewer.BringToFront()
                        RaiseEvent DocumentLoaded()
                        Return
                    End Using
                Catch exPdf As Exception
                    ' fallback WebBrowser
                    Dim tempPath As String = Path.Combine(Path.GetTempPath(), "document_" & Guid.NewGuid().ToString() & ".pdf")
                    File.WriteAllBytes(tempPath, bytes)
                    _webBrowser.Navigate(tempPath)
                    _webBrowser.Visible = True
                    _webBrowser.BringToFront()
                    _tempPdfFiles.Add(tempPath)
                    ' DocumentLoaded sera déclenché dans WebBrowser_DocumentCompleted
                    Return
                End Try
            Else
                Using ms As New MemoryStream(bytes)
                    Dim img = Image.FromStream(ms)
                    _pictureBox.Image = New Bitmap(img)
                    _pictureBox.Visible = True
                    _pictureBox.BringToFront()
                    RaiseEvent DocumentLoaded()
                End Using
            End If

            Dim form = _parent.FindForm()
            If form IsNot Nothing Then form.ActiveControl = Nothing

        Catch ex As Exception
            RaiseEvent DocumentFailed(ex)
        End Try
    End Sub

    Private Sub WebBrowser_DocumentCompleted(sender As Object, e As WebBrowserDocumentCompletedEventArgs)
        ' Dès que la navigation a fini, considérer le document comme chargé
        Try
            ' On peut supprimer les fichiers temporaires s'ils sont libérables
            For i = _tempPdfFiles.Count - 1 To 0 Step -1
                Dim fichier = _tempPdfFiles(i)
                Try
                    If File.Exists(fichier) Then
                        File.Delete(fichier)
                        _tempPdfFiles.RemoveAt(i)
                    End If
                Catch
                    ' ignore, on réessaiera plus tard
                End Try
            Next
        Catch
        End Try

        ' Relâcher le focus (évite blocage)
        Dim form = _parent.FindForm()
        If form IsNot Nothing Then form.ActiveControl = Nothing

        ' Signaler chargement réussi
        RaiseEvent DocumentLoaded()
    End Sub

#Region "IDisposable Support"
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not _disposed Then
            If disposing Then
                FermerDocument()
                If _pdfViewer IsNot Nothing Then
                    _parent.Controls.Remove(_pdfViewer)
                    _pdfViewer.Dispose()
                    _pdfViewer = Nothing
                End If
                If _webBrowser IsNot Nothing Then
                    _parent.Controls.Remove(_webBrowser)
                    _webBrowser.Dispose()
                    _webBrowser = Nothing
                End If
                If _pictureBox IsNot Nothing Then
                    _parent.Controls.Remove(_pictureBox)
                    _pictureBox.Dispose()
                    _pictureBox = Nothing
                End If
            End If
            _disposed = True
        End If
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region

End Class
