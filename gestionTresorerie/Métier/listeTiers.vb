Imports System.Data.SqlClient

Public Class ListeTiers

    ReadOnly _listeTiers As New List(Of Tiers)
    Public Sub New()
        If _listeTiers.Count = 0 Then
            extraitListeTiers()
        End If
    End Sub
    Public Sub extraitListeTiers()
        Try
            ' Requête pour récupérer les tiers avec nom et prénom
            Dim lstTiersPhysique = SqlCommandBuilder.CreateSqlCommand(Constantes.sqlSelTiersPhysique)
            Using monReaderTiers As SqlDataReader = lstTiersPhysique.ExecuteReader()
                While monReaderTiers.Read()
                    _listeTiers.Add(New Tiers(monReaderTiers.GetInt32(0), monReaderTiers.GetString(1), monReaderTiers.GetString(2)))
                End While
            End Using

            ' Requête pour récupérer les tiers avec raison sociale
            Dim lstTiersMorale = SqlCommandBuilder.CreateSqlCommand(Constantes.sqlSelTiersMorale)
            Using monReaderTiers As SqlDataReader = lstTiersMorale.ExecuteReader()
                While monReaderTiers.Read()
                    _listeTiers.Add(New Tiers(monReaderTiers.GetInt32(0), monReaderTiers.GetString(1)))
                End While
            End Using

            Logger.INFO($"Extraction de {_listeTiers.Count} tiers réussie")
        Catch ex As Exception
            Logger.ERR($"Erreur lors de l'extraction de la liste des tiers : {ex.Message}")
            Throw
        End Try
    End Sub
    Public Function DetecteTiers(sTexte As String) As Integer
        ' Essaie de déterminer le tiers en fonction du contenu de la note
        Dim sMots() As String = sTexte.Split(New Char() {" "c}, StringSplitOptions.RemoveEmptyEntries)
        Dim i As Integer = -1

        For Each sMot As String In sMots
            If Not String.IsNullOrWhiteSpace(sMot) Then
                i = getIdParRaisonSociale(sMot.ToUpper(Globalization.CultureInfo.CurrentCulture))
                If i > -1 Then
                    Exit For
                End If
            End If
        Next

        Return i
    End Function
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
        Dim sIdentite As String = String.Empty

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
        ' Rechercher d'abord par nom
        Dim tiers As Tiers = _listeTiers.FirstOrDefault(Function(t) Trim(t.Nom) = sIdentite)
        If tiers IsNot Nothing Then
            Return tiers.id
        End If

        ' Si non trouvé par nom, rechercher par raison sociale
        tiers = _listeTiers.FirstOrDefault(Function(t) Trim(Strings.UCase(t.RaisonSociale)) = sIdentite)
        If tiers IsNot Nothing Then
            Return tiers.id
        End If

        ' Retourner -1 si non trouvé
        Return -1
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
