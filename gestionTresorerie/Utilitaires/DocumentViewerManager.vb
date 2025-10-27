Imports System.IO
Imports System.Windows.Forms
Imports PdfiumViewer
Imports SixLabors.ImageSharp.Metadata.Profiles

Public Class DocumentViewerManager
    Implements IDisposable

    ' ----- EVENEMENTS -----
    Public Event DocumentLoaded()
    Public Event DocumentFailed(ex As Exception)

    Private ReadOnly _parent As Control
    Private _pdfViewer As PdfViewer
    Private _webBrowser As WebBrowser
    Private _pictureBox As PictureBox
    Private _tempPdfFiles As New List(Of String)
    Private _disposed As Boolean = False

    Public Sub New(parent As Control, Optional dock As DockStyle = DockStyle.Fill)
        _parent = parent
        CreateControls(dock)
        AddHandler _parent.HandleDestroyed, AddressOf Parent_HandleDestroyed
    End Sub

    Private Sub CreateControls(dock As DockStyle)
        _pictureBox = New PictureBox() With {
            .Dock = dock,
            .Visible = False,
            .SizeMode = PictureBoxSizeMode.Zoom
        }
        _parent.Controls.Add(_pictureBox)
        _pictureBox.BringToFront()

        _webBrowser = New WebBrowser() With {
            .Dock = dock,
            .Visible = False,
            .AllowWebBrowserDrop = False
        }
        _parent.Controls.Add(_webBrowser)
        _webBrowser.BringToFront()
        AddHandler _webBrowser.DocumentCompleted, AddressOf WebBrowser_DocumentCompleted

        _pdfViewer = New PdfViewer() With {
            .Dock = dock,
            .Visible = False
        }
        _parent.Controls.Add(_pdfViewer)
        _pdfViewer.BringToFront()
    End Sub

    Private Sub WebBrowser_DocumentCompleted(sender As Object, e As WebBrowserDocumentCompletedEventArgs)
        ' Supprime les fichiers temporaires qui ne sont plus utilisés
        For i = _tempPdfFiles.Count - 1 To 0 Step -1
            Dim fichier = _tempPdfFiles(i)
            Try
                If File.Exists(fichier) Then
                    File.Delete(fichier)
                    _tempPdfFiles.RemoveAt(i)
                End If
            Catch
            End Try
        Next
        ' Déclenche l'événement DocumentLoaded pour PDF fallback WebBrowser
        RaiseEvent DocumentLoaded()
    End Sub

    Private Sub Parent_HandleDestroyed(sender As Object, e As EventArgs)
        DeleteAllTempFiles()
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

    Public Sub FermerDocument()
        If _pdfViewer IsNot Nothing AndAlso _pdfViewer.Document IsNot Nothing Then
            _pdfViewer.Document.Dispose()
            _pdfViewer.Document = Nothing
            _pdfViewer.Visible = False
        End If

        If _webBrowser IsNot Nothing Then
            Try
                _webBrowser.Stop()
                _webBrowser.Navigate("about:blank")
            Catch
            End Try
            _webBrowser.Visible = False
        End If

        DeleteAllTempFiles()

        If _pictureBox IsNot Nothing AndAlso _pictureBox.Image IsNot Nothing Then
            _pictureBox.Image.Dispose()
            _pictureBox.Image = Nothing
            _pictureBox.Visible = False
        End If
    End Sub

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
                ' PDF via PdfViewer
                Try
                    Using ms As New MemoryStream(bytes)
                        Dim doc = PdfDocument.Load(ms)
                        _pdfViewer.Document = doc
                        _pdfViewer.Visible = True
                        RaiseEvent DocumentLoaded()
                        Return
                    End Using
                Catch exPdf As Exception
                    ' fallback WebBrowser
                    Dim tempPath As String = Path.Combine(Path.GetTempPath(), "document_" & Guid.NewGuid().ToString() & ".pdf")
                    File.WriteAllBytes(tempPath, bytes)
                    _webBrowser.Navigate(tempPath)
                    _webBrowser.Visible = True
                    _tempPdfFiles.Add(tempPath)
                    ' DocumentLoaded sera déclenché par DocumentCompleted
                    Return
                End Try
            Else
                ' IMAGE
                Using ms As New MemoryStream(bytes)
                    Dim img = Image.FromStream(ms)
                    _pictureBox.Image = New Bitmap(img)
                    _pictureBox.Visible = True
                    RaiseEvent DocumentLoaded()
                End Using
            End If

        Catch ex As Exception
            RaiseEvent DocumentFailed(ex)
        End Try
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

