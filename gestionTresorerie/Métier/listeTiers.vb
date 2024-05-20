Imports System.Data.SqlClient

Public Class ListeTiers

    ReadOnly _listeTiers As New List(Of Tiers)
    Public Sub New(maConn As SqlConnection)

        If _listeTiers.Count = 0 Then
            extraitListeTiers(maConn)
        End If
    End Sub
    Public Sub extraitListeTiers(maConn As SqlConnection)
        Dim maCmdCategorie As SqlCommand
        Dim monReaderTiers As SqlDataReader

        maCmdCategorie = New SqlCommand("SELECT id,nom, prenom FROM Tiers where nom is not null;", maConn)
        monReaderTiers = maCmdCategorie.ExecuteReader()

        Do While monReaderTiers.Read()
            _listeTiers.Add(New Tiers(monReaderTiers.GetInt32(0), monReaderTiers.GetString(1), monReaderTiers.GetString(2)))
        Loop
        monReaderTiers.Close()
        maCmdCategorie = New SqlCommand("SELECT id,raisonSociale FROM Tiers where raisonSociale is not null;", maConn)
        monReaderTiers = maCmdCategorie.ExecuteReader()
        Do While monReaderTiers.Read()
            _listeTiers.Add(New Tiers(monReaderTiers.GetInt32(0), monReaderTiers.GetSqlString(1)))
        Loop
        monReaderTiers.Close()
    End Sub
    Public Function getListeTiers() As List(Of Tiers)
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
        Dim indice As Integer = -1

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
    Public Function Add(sNom As String, sPrenom As String, Optional iCategorie As Integer = 0, Optional iSousCategorie As Integer = 0) As Tiers
        Dim monTiers As Tiers
        'Renvoie l'id du Tiers créé 
        monTiers = New Tiers(CompteTiers() + 1, sNom, sPrenom, iCategorie, iSousCategorie)
        _listeTiers.Add(monTiers)
        Return monTiers
    End Function
    Public Function Add(sRaisonSociale As String, Optional iCategorie As Integer = 0, Optional iSousCategorie As Integer = 0) As Tiers
        Dim monTiers As Tiers
        'Renvoie l'id du Tiers créé 
        monTiers = New Tiers(CompteTiers() + 1, sRaisonSociale, iCategorie, iSousCategorie)
        _listeTiers.Add(monTiers)
        Return monTiers
    End Function
    Public Sub SupprimeTiers(tiers As Tiers)
        'Todo : voir si je crée des trous de séquence
        _listeTiers.Remove(tiers)
    End Sub
    Public Function CompteTiers() As Integer
        Return _listeTiers.Count
    End Function
End Class
