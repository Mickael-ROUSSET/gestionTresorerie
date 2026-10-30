Option Strict On
Option Explicit On

Imports System
Imports System.Globalization

''' <summary>
''' Identite stable d'une piece jointe d'un message IMAP, sans acces reseau.
''' L'index est la position (base zero) dans la liste d'origine des pieces jointes,
''' y compris celles qui ne produisent pas de proposition. Il doit rester identique
''' lors d'une reprise pour ne pas creer une nouvelle identite.
''' </summary>
Public NotInheritable Class GmailDocumentSourceKey
    Private Sub New()
    End Sub

    Public Shared Function MakeKey(account As String, uidValidity As UInteger, uid As UInteger,
                                   attachmentIndex As Integer) As String
        If attachmentIndex < 0 Then Throw New ArgumentOutOfRangeException(NameOf(attachmentIndex))
        Return GmailImportJournal.MakeKey(account, uidValidity, uid) &
               ":attachment:" & attachmentIndex.ToString(CultureInfo.InvariantCulture)
    End Function
End Class
