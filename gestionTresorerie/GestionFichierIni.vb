Imports System.Runtime.InteropServices
Imports System.IO
Imports System.Text

Public Class GestionFichierIni

#Region "Natives Methods"

    Private Declare Unicode Function GetPrivateProfileString Lib "kernel32" Alias "GetPrivateProfileStringW" (lpApplicationName As String,
                                                                                                               lpKeyName As String, lpDefault As String,
                                                                                                               lpReturnedString As String, nSize As Integer,
                                                                                                               lpFileName As String) As Integer

    Private Declare Unicode Function WritePrivateProfileString Lib "kernel32" Alias "WritePrivateProfileStringW" (lpApplicationName As String,
                                                                                                               lpKeyName As String, lpString As String,
                                                                                                               lpFileName As String) As Integer

    <DllImport("kernel32")>
    Private Shared Function GetPrivateProfileString(Section As String, Key As Integer, Value As String, <MarshalAs(UnmanagedType.LPArray)> Result As Byte(), Size As Integer, FileName As String) As Integer
    End Function

    <DllImport("kernel32")>
    Private Shared Function GetPrivateProfileString(Section As Integer, Key As String, Value As String, <MarshalAs(UnmanagedType.LPArray)> Result As Byte(), Size As Integer, FileName As String) As Integer
    End Function

#End Region

#Region "Public Methods"

    'Suppression de section
    Public Shared Sub DeleteSection(INIPath As String, SectionName As String)
        Dim lpKeyName As String = Nothing
        Dim lpString As String = Nothing
        WritePrivateProfileString(SectionName, lpKeyName, lpString, INIPath)
    End Sub

    'Suppression de clé
    Public Shared Sub DeleteKey(INIPath As String, SectionName As String, KeyName As String)
        Dim lpString As String = Nothing
        WritePrivateProfileString(SectionName, KeyName, lpString, INIPath)
    End Sub

    'Lecture Valeur d'une section et clé spécifiée
    Public Shared Function ReadValue(INIPath As String, SectionName As String, KeyName As String) As String
        Return ReadValue(INIPath, SectionName, KeyName, "")
    End Function

    'Lecture Valeur d'une section et clé spécifiée (retourne valeur par défaut si elle n'existe pas)
    Public Shared Function ReadValue(INIPath As String, SectionName As String, KeyName As String, DefaultValue As String) As String
        Dim lpReturnedString As String = Strings.Space(2048)
        Dim length As Integer = GetPrivateProfileString(SectionName, KeyName, DefaultValue, lpReturnedString, lpReturnedString.Length, INIPath)
        If length > 0 Then
            Return lpReturnedString.Substring(0, length)
        End If
        Return ""
    End Function

    'Ecriture Clés/Valeurs dans une section spécifiée
    Public Shared Sub Write(INIPath As String, SectionName As String, KeyName As String, TheValue As String)
        WritePrivateProfileString(SectionName, KeyName, TheValue, INIPath)
    End Sub

    'Détecte si section spécifiée existe
    Public Shared Function SectionExists(INIPath As String, SectionName As String) As Boolean
        Return SectionNames(INIPath).Any(Function(s) String.Equals(s.ToLower, SectionName.ToLower))
    End Function

    'Retourne tous les noms des sections existantes dans le fichier de configuration
    Public Shared Function SectionNames(INIPath As String) As String()
        Dim maxsize As Integer = 500
        While True
            Dim bytes As Byte() = New Byte(maxsize - 1) {}
            Dim size As Integer = GetPrivateProfileString(0, "", "", bytes, maxsize, INIPath)
            If size < maxsize - 2 Then
                Dim Selected As String = Encoding.ASCII.GetString(bytes, 0, size - (If(size > 0, 1, 0)))
                Return Selected.Split(New Char() {ControlChars.NullChar})
            End If
            maxsize *= 2
        End While
        Return Nothing
    End Function

    'Retourne toutes les clés existantes d'une section spécifiée
    Public Shared Function SectionKeys(INIPath As String, sectionName As String) As String()
        Dim maxsize As Integer = 500
        While True
            Dim bytes As Byte() = New Byte(maxsize - 1) {}
            Dim size As Integer = GetPrivateProfileString(sectionName, 0, "", bytes, maxsize, INIPath)
            If size < maxsize - 2 Then
                Dim entries As String = Encoding.ASCII.GetString(bytes, 0, size - (If(size > 0, 1, 0)))
                Return entries.Split(New Char() {ControlChars.NullChar})
            End If
            maxsize *= 2
        End While
        Return Nothing
    End Function

#End Region

End Class