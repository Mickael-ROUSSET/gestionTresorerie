<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class selectionneCheque
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
        btnSelCheque = New Button()
        pbCheque = New PictureBox()
        lstCheques = New ListView()
        CType(pbCheque, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' btnSelCheque
        ' 
        btnSelCheque.Location = New Point(52, 448)
        btnSelCheque.Name = "btnSelCheque"
        btnSelCheque.Size = New Size(84, 23)
        btnSelCheque.TabIndex = 2
        btnSelCheque.Text = "&Sélectionne"
        btnSelCheque.UseVisualStyleBackColor = True
        ' 
        ' pbCheque
        ' 
        pbCheque.Location = New Point(61, 197)
        pbCheque.Name = "pbCheque"
        pbCheque.Size = New Size(549, 233)
        pbCheque.TabIndex = 3
        pbCheque.TabStop = False
        ' 
        ' lstCheques
        ' 
        lstCheques.Location = New Point(69, 67)
        lstCheques.Name = "lstCheques"
        lstCheques.Size = New Size(541, 109)
        lstCheques.TabIndex = 4
        lstCheques.UseCompatibleStateImageBehavior = False
        ' 
        ' selectionneCheque
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(800, 483)
        Controls.Add(lstCheques)
        Controls.Add(pbCheque)
        Controls.Add(btnSelCheque)
        Name = "selectionneCheque"
        Text = "Sélectionne un chèque"
        CType(pbCheque, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub
    Friend WithEvents btnSelCheque As Button
    Friend WithEvents pbCheque As PictureBox
    Friend WithEvents lstCheques As ListView
End Class
