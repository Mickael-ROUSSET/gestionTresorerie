<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmPrincipale
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmPrincipale))
        btnSaisie = New Button()
        btnChargeRelevé = New Button()
        btnHistogramme = New Button()
        SuspendLayout()
        ' 
        ' btnSaisie
        ' 
        btnSaisie.Location = New Point(84, 66)
        btnSaisie.Name = "btnSaisie"
        btnSaisie.Size = New Size(104, 23)
        btnSaisie.TabIndex = 0
        btnSaisie.Text = "Saisie mouvement"
        btnSaisie.UseVisualStyleBackColor = True
        ' 
        ' btnChargeRelevé
        ' 
        btnChargeRelevé.AccessibleDescription = resources.GetString("btnChargeRelevé.AccessibleDescription")
        btnChargeRelevé.Location = New Point(84, 121)
        btnChargeRelevé.Name = "btnChargeRelevé"
        btnChargeRelevé.Size = New Size(104, 23)
        btnChargeRelevé.TabIndex = 1
        btnChargeRelevé.Text = "Charge relevé"
        btnChargeRelevé.UseVisualStyleBackColor = True
        ' 
        ' btnHistogramme
        ' 
        btnHistogramme.AccessibleDescription = resources.GetString("btnHistogramme.AccessibleDescription")
        btnHistogramme.Location = New Point(84, 178)
        btnHistogramme.Name = "btnHistogramme"
        btnHistogramme.Size = New Size(104, 23)
        btnHistogramme.TabIndex = 2
        btnHistogramme.Text = "Histogramme"
        btnHistogramme.UseVisualStyleBackColor = True
        ' 
        ' frmPrincipale
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(800, 450)
        Controls.Add(btnHistogramme)
        Controls.Add(btnChargeRelevé)
        Controls.Add(btnSaisie)
        Name = "frmPrincipale"
        Text = "Gestion des la  trésoreire de l'AGUMAAA"
        ResumeLayout(False)
    End Sub

    Friend WithEvents btnSaisie As Button
    Friend WithEvents btnChargeRelevé As Button
    Friend WithEvents btnHistogramme As Button
End Class
