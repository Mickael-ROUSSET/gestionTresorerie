<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmChargeRelevé
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
        Dim fldBrowserDialog As FolderBrowserDialog
        lstMouvements = New ListView()
        btnOuvreFichier = New Button()
        fldBrowserDialog = New FolderBrowserDialog()
        SuspendLayout()
        ' 
        ' lstMouvements
        ' 
        lstMouvements.Location = New Point(100, 140)
        lstMouvements.Name = "lstMouvements"
        lstMouvements.Size = New Size(641, 275)
        lstMouvements.TabIndex = 1
        lstMouvements.UseCompatibleStateImageBehavior = False
        ' 
        ' btnOuvreFichier
        ' 
        btnOuvreFichier.Location = New Point(175, 63)
        btnOuvreFichier.Name = "btnOuvreFichier"
        btnOuvreFichier.Size = New Size(75, 23)
        btnOuvreFichier.TabIndex = 2
        btnOuvreFichier.Text = "Ouvre relevé"
        btnOuvreFichier.UseVisualStyleBackColor = True
        ' 
        ' frmChargeRelevé
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(800, 450)
        Controls.Add(btnOuvreFichier)
        Controls.Add(lstMouvements)
        Name = "frmChargeRelevé"
        Text = "Charge Relevé"
        ResumeLayout(False)
    End Sub

    Friend WithEvents lstMouvements As ListView
    Friend WithEvents btnOuvreFichier As Button
End Class
