Imports System.Data.SqlClient

Public Class ListeTiers

    ReadOnly _listeTiers As New List(Of Tiers)
    Public Function getListeTiers(maConn As SqlConnection) As List(Of Tiers)
        Dim myCmdCategorie As SqlCommand
        Dim myReaderTiers As SqlDataReader

        myCmdCategorie = New SqlCommand("SELECT nom, prenom FROM Tiers;", maConn)
        myReaderTiers = myCmdCategorie.ExecuteReader()
        Do While myReaderTiers.Read()
            _listeTiers.Add(New Tiers(myReaderTiers.GetSqlString(0), myReaderTiers.GetSqlString(1)))
        Loop
        myReaderTiers.Close()
        myCmdCategorie = New SqlCommand("SELECT raisonSociale FROM Tiers;", maConn)
        myReaderTiers = myCmdCategorie.ExecuteReader()
        Do While myReaderTiers.Read()
            _listeTiers.Add(New Tiers(myReaderTiers.GetSqlString(0)))
        Loop
        myReaderTiers.Close()
        Return _listeTiers
    End Function
    Public Function getParId(id As Integer) As Tiers
        Return _listeTiers.Item(id)
    End Function
    Public Function count() As Integer
        Return _listeTiers.Count
    End Function
    Public Function getIdentiteParId(id As Integer) As String
        Dim sIdentite As String = ""

        With _listeTiers.Item(id)
            If id > 0 Then
                If .Nom > "" Then
                    sIdentite = .Nom
                ElseIf .RaisonSociale > "" Then
                    sIdentite = .RaisonSociale
                End If
            End If
        End With
        Return sIdentite
    End Function
    Public Function getIdParRaisonSociale(sIdentite As String) As Integer
        Dim indice As Integer = 0

        For Each tiers In _listeTiers
            If Trim(tiers.Nom) = sIdentite Then
                indice = tiers.id
                Exit For
            End If
        Next
        'Si on ne trouve pas sur le nom, on essaie sur la raison sociale
        For Each tiers In _listeTiers
            If Trim(Strings.UCase(tiers.RaisonSociale)) = sIdentite Then
                indice = tiers.id
                Exit For
            End If
        Next
        Return indice
    End Function
    'Public Function CatégorieParDéfaut(identité As String) As String
    '    Dim sCatégorie As String = ""
    '    For Each tiers As Tiers In listeTiers
    '        If tiers.RaisonSociale.Equals(identité) Or tiers.Nom.Equals(identité) Then
    '            sCatégorie = tiers.CategorieDefaut
    '            Exit For
    '        End If
    '    Next
    '    Return sCatégorie
    'End Function
    'Public Function SousCatégorieParDéfaut(identité As String) As String
    '    Dim sousCatégorie As String = ""
    '    For Each tiers As Tiers In listeTiers
    '        If tiers.RaisonSociale.Equals(identité) Or tiers.Nom.Equals(identité) Then
    '            sousCatégorie = tiers.SousCategorieDefaut
    '            Exit For
    '        End If
    '    Next
    '    Return sousCatégorie
    'End Function
    Public Sub Add(tiers As Tiers)
        _listeTiers.Add(tiers)
    End Sub
    Public Sub SupprimeTiers(tiers As Tiers)
        _listeTiers.Remove(tiers)
    End Sub
    Public Function CompteTiers() As Integer
        Return _listeTiers.Count
    End Function
End Class
