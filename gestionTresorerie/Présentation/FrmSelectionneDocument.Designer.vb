<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FrmSelectionneDocument
    Inherits System.Windows.Forms.Form

    'Form remplace la méthode Dispose pour nettoyer la liste des composants.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Requise par le Concepteur Windows Form
    Private components As System.ComponentModel.IContainer

    'REMARQUE : la procédure suivante est requise par le Concepteur Windows Form
    'Elle peut être modifiée à l'aide du Concepteur Windows Form.  
    'Ne la modifiez pas à l'aide de l'éditeur de code.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        btnSelDoc = New Button()
        pbDocument = New PictureBox()
        lstDocuments = New ListView()
        btnValider = New Button()
        flpMetaDonnees = New FlowLayoutPanel()
        Panel1 = New Panel()
        lblRecherche = New Label()
        txtRecherche = New TextBox()
        btnRechercher = New Button()
        btnEffacerFiltre = New Button()
        lblPage = New Label()
        btnPrecedent = New Button()
        btnSuivant = New Button()
        CType(pbDocument, ComponentModel.ISupportInitialize).BeginInit()
        Panel1.SuspendLayout()
        SuspendLayout()
        ' 
        ' btnSelDoc
        ' 
        btnSelDoc.Location = New Point(61, 206)
        btnSelDoc.Name = "btnSelDoc"
        btnSelDoc.Size = New Size(84, 23)
        btnSelDoc.TabIndex = 2
        btnSelDoc.Text = "&Sélectionne"
        btnSelDoc.UseVisualStyleBackColor = True
        ' 
        ' pbDocument
        ' 
        pbDocument.Location = New Point(61, 250)
        pbDocument.Name = "pbDocument"
        pbDocument.Size = New Size(653, 307)
        pbDocument.TabIndex = 3
        pbDocument.TabStop = False
        ' 
        ' lstDocuments
        ' 
        lstDocuments.Location = New Point(61, 30)
        lstDocuments.Name = "lstDocuments"
        lstDocuments.Size = New Size(541, 109)
        lstDocuments.TabIndex = 4
        lstDocuments.UseCompatibleStateImageBehavior = False
        ' 
        ' btnValider
        ' 
        btnValider.Location = New Point(281, 206)
        btnValider.Name = "btnValider"
        btnValider.Size = New Size(100, 23)
        btnValider.TabIndex = 5
        btnValider.Text = "&Valider"
        btnValider.UseVisualStyleBackColor = True
        ' 
        ' flpMetaDonnees
        ' 
        flpMetaDonnees.AutoScroll = True
        flpMetaDonnees.FlowDirection = FlowDirection.TopDown
        flpMetaDonnees.Location = New Point(740, 250)
        flpMetaDonnees.Name = "flpMetaDonnees"
        flpMetaDonnees.Size = New Size(467, 226)
        flpMetaDonnees.TabIndex = 6
        flpMetaDonnees.WrapContents = False
        ' 
        ' Panel1
        ' 
        Panel1.Controls.Add(btnSuivant)
        Panel1.Controls.Add(btnPrecedent)
        Panel1.Controls.Add(lblPage)
        Panel1.Controls.Add(btnEffacerFiltre)
        Panel1.Controls.Add(btnRechercher)
        Panel1.Controls.Add(txtRecherche)
        Panel1.Controls.Add(lblRecherche)
        Panel1.Location = New Point(66, 147)
        Panel1.Name = "Panel1"
        Panel1.Size = New Size(536, 53)
        Panel1.TabIndex = 7
        ' 
        ' lblRecherche
        ' 
        lblRecherche.AutoSize = True
        lblRecherche.Location = New Point(9, 8)
        lblRecherche.Name = "lblRecherche"
        lblRecherche.Size = New Size(68, 15)
        lblRecherche.TabIndex = 0
        lblRecherche.Text = "Recherche :"
        ' 
        ' txtRecherche
        ' 
        txtRecherche.Location = New Point(83, 3)
        txtRecherche.Name = "txtRecherche"
        txtRecherche.Size = New Size(200, 23)
        txtRecherche.TabIndex = 1
        ' 
        ' btnRechercher
        ' 
        btnRechercher.Location = New Point(329, 5)
        btnRechercher.Name = "btnRechercher"
        btnRechercher.Size = New Size(95, 23)
        btnRechercher.TabIndex = 2
        btnRechercher.Text = "🔍 Rechercher"
        btnRechercher.UseVisualStyleBackColor = True
        ' 
        ' btnEffacerFiltre
        ' 
        btnEffacerFiltre.Location = New Point(430, 5)
        btnEffacerFiltre.Name = "btnEffacerFiltre"
        btnEffacerFiltre.Size = New Size(75, 23)
        btnEffacerFiltre.TabIndex = 3
        btnEffacerFiltre.Text = "❌ Effacer"
        btnEffacerFiltre.UseVisualStyleBackColor = True
        ' 
        ' lblPage
        ' 
        lblPage.AutoSize = True
        lblPage.Location = New Point(436, 30)
        lblPage.Name = "lblPage"
        lblPage.Size = New Size(55, 15)
        lblPage.TabIndex = 4
        lblPage.Text = "Page X/Y"
        ' 
        ' btnPrecedent
        ' 
        btnPrecedent.Location = New Point(55, 33)
        btnPrecedent.Name = "btnPrecedent"
        btnPrecedent.Size = New Size(75, 23)
        btnPrecedent.TabIndex = 5
        btnPrecedent.Text = "◀ Précédent"
        btnPrecedent.UseVisualStyleBackColor = True
        ' 
        ' btnSuivant
        ' 
        btnSuivant.Location = New Point(169, 32)
        btnSuivant.Name = "btnSuivant"
        btnSuivant.Size = New Size(75, 23)
        btnSuivant.TabIndex = 6
        btnSuivant.Text = "Suivant ▶"
        btnSuivant.UseVisualStyleBackColor = True
        ' 
        ' FrmSelectionneDocument
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(1257, 569)
        Controls.Add(Panel1)
        Controls.Add(flpMetaDonnees)
        Controls.Add(btnValider)
        Controls.Add(lstDocuments)
        Controls.Add(pbDocument)
        Controls.Add(btnSelDoc)
        Name = "FrmSelectionneDocument"
        Text = "Sélectionne un document"
        CType(pbDocument, ComponentModel.ISupportInitialize).EndInit()
        Panel1.ResumeLayout(False)
        Panel1.PerformLayout()
        ResumeLayout(False)
    End Sub
    Friend WithEvents btnSelDoc As Button
    Friend WithEvents pbDocument As PictureBox
    Friend WithEvents lstDocuments As ListView
    Friend WithEvents btnValider As Button
    Friend WithEvents flpMetaDonnees As FlowLayoutPanel
    Friend WithEvents Panel1 As Panel
    Friend WithEvents btnEffacerFiltre As Button
    Friend WithEvents btnRechercher As Button
    Friend WithEvents txtRecherche As TextBox
    Friend WithEvents lblRecherche As Label
    Friend WithEvents btnSuivant As Button
    Friend WithEvents btnPrecedent As Button
    Friend WithEvents lblPage As Label
End Class
