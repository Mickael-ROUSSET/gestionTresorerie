Imports System.Data.SqlClient

Public Class ListeTiers

    ReadOnly _listeTiers As New List(Of Tiers)
    Public Sub New()

        If _listeTiers.Count = 0 Then
            extraitListeTiers(ConnexionDB.GetInstance.getConnexion)
        End If
    End Sub
    Public Sub extraitListeTiers(maConn As SqlConnection)
        Try
            ' Ouvrir la connexion si elle n'est pas déjà ouverte
            If maConn.State <> ConnectionState.Open Then
                maConn.Open()
            End If

            ' Requête pour récupérer les tiers avec nom et prénom
            Dim lstTiersPhysique As New SqlCommand("SELECT id, nom, prenom FROM Tiers WHERE nom IS NOT NULL;", maConn)
            Using monReaderTiers As SqlDataReader = lstTiersPhysique.ExecuteReader()
                While monReaderTiers.Read()
                    _listeTiers.Add(New Tiers(monReaderTiers.GetInt32(0), monReaderTiers.GetString(1), monReaderTiers.GetString(2)))
                End While
            End Using

            ' Requête pour récupérer les tiers avec raison sociale
            Dim lstTiersMorale As New SqlCommand("SELECT id, raisonSociale FROM Tiers WHERE raisonSociale IS NOT NULL;", maConn)
            Using monReaderTiers As SqlDataReader = lstTiersMorale.ExecuteReader()
                While monReaderTiers.Read()
                    _listeTiers.Add(New Tiers(monReaderTiers.GetInt32(0), monReaderTiers.GetString(1)))
                End While
            End Using

            Logger.INFO("Extraction de la liste des tiers réussie.")
        Catch ex As Exception
            Logger.ERR($"Erreur lors de l'extraction de la liste des tiers : {ex.Message}")
            Throw
        End Try
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
        _listeTiers.Remove(tiers)
    End Sub
    Public Function CompteTiers() As Integer
        Return _listeTiers.Count
    End Function
End Class
