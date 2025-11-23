Imports System.Runtime.CompilerServices

Public Module StringExtensions

    ''' <summary>
    ''' Remplace plusieurs valeurs dans une chaîne en un seul appel.
    ''' </summary>
    <Extension()>
    Public Function ReplaceMany(
        source As String,
        replacements As Dictionary(Of String, String)
    ) As String

        If String.IsNullOrEmpty(source) OrElse replacements Is Nothing Then
            Return source
        End If

        Dim output = source

        For Each kvp In replacements
            output = output.Replace(kvp.Key, kvp.Value)
        Next

        Return output
    End Function

End Module
