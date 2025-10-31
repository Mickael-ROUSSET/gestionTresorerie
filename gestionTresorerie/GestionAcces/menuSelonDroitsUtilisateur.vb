Module menuSelonDroitsUtilisateur
    Private Sub ConfigurerMenusSelonRole(menu As MenuStrip)
        ' Tout masquer par défaut
        menu.Items("mnuDocuments").Visible = False
        menu.Items("mnuAdmin").Visible = False
        menu.Items("mnuFichier").Visible = True
        menu.Items("mnuAide").Visible = True

        Dim sousMenu As ToolStripMenuItem = DirectCast(menu.Items("mnuDocuments"), ToolStripMenuItem)
        ' 🔹 Lecteur : lecture seule
        If UtilisateurActif.EstLecteur() Then
            menu.Items("mnuDocuments").Visible = True
            sousMenu.DropDownItems("ConsulterToolStripMenuItem").Visible = True
            sousMenu.DropDownItems("AjouterToolStripMenuItem").Visible = False
            sousMenu.DropDownItems("SupprimerToolStripMenuItem").Visible = False
        End If

        ' 🔹 Écrivain : lecture + ajout/modif
        If UtilisateurActif.EstEcrivain() Then
            menu.Items("mnuDocuments").Visible = True
            sousMenu.DropDownItems("ConsulterToolStripMenuItem").Visible = True
            sousMenu.DropDownItems("AjouterToolStripMenuItem").Visible = True
            sousMenu.DropDownItems("SupprimerToolStripMenuItem").Visible = False
        End If

        ' 🔹 Admin : tout visible
        If UtilisateurActif.EstAdmin() Then
            menu.Items("mnuDocuments").Visible = True
            menu.Items("mnuAdmin").Visible = True
            For Each item As ToolStripItem In sousMenu.DropDownItems
                item.Visible = True
            Next
        End If
    End Sub

End Module
