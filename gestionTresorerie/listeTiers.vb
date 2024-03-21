Imports DocumentFormat.OpenXml.Drawing

Public Class ListeTiers

    ReadOnly listeTiers As New List(Of ClsTiers)
    Public Function RenvoieListe() As List(Of ClsTiers)
        Return listeTiers
    End Function
    Public Function CatégorieParDéfaut(identité As String) As String
        Dim sCatégorie As String = ""
        For Each tiers As ClsTiers In listeTiers
            If tiers.RaisonSociale.Equals(identité) Or tiers.Nom.Equals(identité) Then
                sCatégorie = tiers.CategorieDefaut
                Exit For
            End If
        Next
        Return sCatégorie
    End Function
    Public Function SousCatégorieParDéfaut(identité As String) As String
        Dim sousCatégorie As String = ""
        For Each tiers As ClsTiers In listeTiers
            If tiers.RaisonSociale.Equals(identité) Or tiers.Nom.Equals(identité) Then
                sousCatégorie = tiers.SousCategorieDefaut
                Exit For
            End If
        Next
        Return sousCatégorie
    End Function
    Public Sub Add(tiers As ClsTiers)
        listeTiers.Add(tiers)
    End Sub
    Public Sub Supprime(tiers As ClsTiers)
        listeTiers.Remove(tiers)
    End Sub
    Public Function Compte() As Integer
        Return listeTiers.Count
    End Function
End Class
