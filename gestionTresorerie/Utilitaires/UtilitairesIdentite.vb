Imports System.Globalization
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions

Public Class UtilitairesIdentite
    ''' <summary>
    ''' Nettoie une chaîne pour qu'elle puisse servir de nom de dossier ou de fichier.
    ''' Supprime les caractères interdits, les accents et les espaces superflus.
    ''' </summary>
    Public Shared Function NettoyerNomFichier(nomSource As String) As String
        If String.IsNullOrWhiteSpace(nomSource) Then Return "INCONNU"

        ' 1. Conversion en Majuscules pour l'uniformité
        Dim resultat As String = nomSource.ToUpper()

        ' 2. Remplacer les caractères accentués (É -> E, È -> E, etc.)
        resultat = RemplacerAccents(resultat)

        ' 3. Remplacer les caractères interdits Windows par un underscore (_)
        ' Caractères : \ / : * ? " < > |
        Dim caracteresInterdits As String = New String(Path.GetInvalidFileNameChars()) & New String(Path.GetInvalidPathChars())
        For Each c As Char In caracteresInterdits
            resultat = resultat.Replace(c, "_"c)
        Next

        ' 4. Remplacer les espaces et caractères spéciaux restants par un underscore
        ' Garder uniquement les lettres, chiffres et underscore
        resultat = Regex.Replace(resultat, "[^A-Z0-9]", "_")

        ' 5. Nettoyage final : éviter les doubles "__" et enlever l'éventuel "_" à la fin
        resultat = Regex.Replace(resultat, "_+", "_").Trim("_"c)

        Return resultat
    End Function

    Private Shared Function RemplacerAccents(texte As String) As String
        Dim stFormD As String = texte.Normalize(NormalizationForm.FormD)
        Dim sb As New StringBuilder()

        For Each c As Char In stFormD
            Dim uc As UnicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c)
            If uc <> UnicodeCategory.NonSpacingMark Then
                sb.Append(c)
            End If
        Next

        Return sb.ToString().Normalize(NormalizationForm.FormC)
    End Function
End Class