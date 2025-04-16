Imports System.Data.SqlClient

Public Class frmNouveauTiers

    Private Sub btnCreerTiers_Click(sender As Object, e As EventArgs) Handles btnCreerTiers.Click
        Dim listeTiers As ListeTiers

        If ListeTiers Is Nothing Then
            'listeTiers = New ListeTiers(FrmPrincipale.maConnexionDB.getConnexion)
            listeTiers = New ListeTiers(connexionDB.GetInstance.getConnexion)
        End If
        If rbPersonneMorale.Checked = True Then
            insereNouveauTiers(listeTiers, txtRaisonSociale.Text, CInt(txtCategorie.Text), CInt(txtSousCategorie.Text))
        Else
            insereNouveauTiers(listeTiers, txtNom.Text, txtPrenom.Text, CInt(txtCategorie.Text), CInt(txtSousCategorie.Text))
        End If
    End Sub
    Sub insereNouveauTiers(listeTiers As ListeTiers, sPrenom As String, sNom As String, Optional iCategorie As Integer = 0, Optional iSousCategorie As Integer = 0)
        Dim sRequete As String
        Dim neoTiers As Tiers

        neoTiers = listeTiers.Add(sPrenom, sNom, iCategorie, iSousCategorie)
        sRequete = "Insert into Tiers (prenom, nom, dateCreation, dateModification, categorieDefaut, sousCategorieDefaut) 
               values (@prenom, @nom, @dateCreation, @dateModification, @categorieDefaut, @sousCategorieDefaut)"
        Call creeNouveauTiers(sRequete, neoTiers)
    End Sub
    Sub insereNouveauTiers(listeTiers As ListeTiers, sRaisonSociale As String, Optional iCategorie As Integer = 0, Optional iSousCategorie As Integer = 0)
        Dim sRequete As String
        Dim neoTiers As Tiers

        neoTiers = listeTiers.Add(sRaisonSociale, iCategorie, iSousCategorie)
        sRequete = "Insert into Tiers (raisonSociale, dateCreation, dateModification, categorieDefaut, sousCategorieDefaut) 
               values (@raisonSociale, @dateCreation, @dateModification, @categorieDefaut, @sousCategorieDefaut)"
        Call creeNouveauTiers(sRequete, neoTiers)
    End Sub
    'Sub insereNouveauTiers(sPrenom As String, sNom As String, Optional iCategorie As Integer = 0, Optional iSousCategorie As Integer = 0)
    '    Dim maCmd As SqlCommand
    '    Dim iNbLignes As Integer
    '    Dim neoTiers As Tiers
    '    Dim listeTiers As ListeTiers

    '    If listeTiers Is Nothing Then
    '        listeTiers = New ListeTiers(FrmPrincipale.maConnexionDB.getConnexion)
    '    End If
    '    neoTiers = listeTiers.Add(sPrenom, sNom, iCategorie, iSousCategorie)
    '    maCmd = New SqlCommand
    '    With maCmd
    '        .Connection = FrmPrincipale.maConn
    '        .CommandText = "Insert into Tiers (prenom, nom, dateCreation, dateModification, categorieDefaut, sousCategorieDefaut) 
    '           values (@prenom, @nom, @dateCreation, @dateModification, @categorieDefaut, @sousCategorieDefaut)"
    '    End With
    '    Try
    '        maCmd = AjouteParam(maCmd, neoTiers)
    '        iNbLignes = maCmd.ExecuteNonQuery
    '    Catch ex As Exception
    '        msgbox(ex.Message)
    '    End Try

    '    MsgBox(iNbLignes & ", ligne insérée : " & neoTiers.Prénom & ", " & neoTiers.Nom)
    'End Sub
    'Sub insereNouveauTiers(sRaisonSociale As String, Optional iCategorie As Integer = 0, Optional iSousCategorie As Integer = 0)
    '    Dim maCmd As SqlCommand
    '    Dim iNbLignes As Integer
    '    Dim neoTiers As Tiers
    '    Dim listeTiers As ListeTiers

    '    If listeTiers Is Nothing Then
    '        listeTiers = New ListeTiers(FrmPrincipale.maConnexionDB.getConnexion)
    '    End If
    '    neoTiers = listeTiers.Add(sRaisonSociale, iCategorie, iSousCategorie)
    '    maCmd = New SqlCommand
    '    With maCmd
    '        .Connection = FrmPrincipale.maConn
    '        .CommandText = "Insert into Tiers (raisonSociale, dateCreation, dateModification, categorieDefaut, sousCategorieDefaut) 
    '           values (@raisonSociale, @dateCreation, @dateModification, @categorieDefaut, @sousCategorieDefaut)"
    '    End With
    '    Try
    '        maCmd = AjouteParam(maCmd, neoTiers)
    '        iNbLignes = maCmd.ExecuteNonQuery
    '    Catch ex As Exception
    '        msgbox(ex.Message)
    '    End Try

    '    MsgBox(iNbLignes & ", ligne insérée : " & neoTiers.RaisonSociale)
    'End Sub
    Sub creeNouveauTiers(sRequete As String, tiers As Tiers)
        Dim maCmd As SqlCommand
        Dim iNbLignes As Integer

        maCmd = New SqlCommand
        With maCmd
            '.Connection = FrmPrincipale.maConn
            .Connection = connexionDB.GetInstance.getConnexion
            .CommandText = sRequete
        End With
        Try
            maCmd = AjouteParam(maCmd, tiers)
            iNbLignes = maCmd.ExecuteNonQuery
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try

        MsgBox(iNbLignes & ", ligne insérée : " & tiers.RaisonSociale)
    End Sub
    Private Function AjouteParam(myCmd As SqlCommand, neoTiers As Tiers) As SqlCommand
        With myCmd
            .Parameters.Clear()
            .Parameters.Add("@raisonSociale", SqlDbType.NVarChar)
            .Parameters(0).Value = varAbsente(neoTiers.RaisonSociale)
            .Parameters.Add("@nom", SqlDbType.NVarChar)
            .Parameters(1).Value = varAbsente(neoTiers.Nom)
            .Parameters.Add("@prenom", SqlDbType.NVarChar)
            .Parameters(2).Value = varAbsente(neoTiers.Prénom)
            .Parameters.Add("@dateCreation", SqlDbType.Date)
            .Parameters(3).Value = Now.Date
            .Parameters.Add("@dateModification", SqlDbType.Date)
            .Parameters(4).Value = Now.Date
            .Parameters.Add("@categorieDefaut", SqlDbType.Int)
            .Parameters(5).Value = neoTiers.CategorieDefaut
            .Parameters.Add("@sousCategorieDefaut", SqlDbType.Int)
            .Parameters(6).Value = neoTiers.SousCategorieDefaut
        End With
        Return myCmd
    End Function
    Private Function varAbsente(variable As String) As String
        'Renvoie une espace si la variable n'est pas fournie
        If variable Is Nothing Then
            variable = ""
        End If
        Return variable
    End Function
    Private Sub rbPersonneMorale_CheckedChanged(sender As Object, e As EventArgs) Handles rbPersonneMorale.CheckedChanged
        txtRaisonSociale.Visible = rbPersonneMorale.Checked
        txtNom.Visible = Not (rbPersonneMorale.Checked)
        txtPrenom.Visible = Not (rbPersonneMorale.Checked)
        lblRaisonSociale.Visible = rbPersonneMorale.Checked
        lblNom.Visible = Not (rbPersonneMorale.Checked)
        lblPrenom.Visible = Not (rbPersonneMorale.Checked)
    End Sub

End Class