<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmSaisie
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        lblType = New Label()
        dateMvt = New DateTimePicker()
        lstType = New ListBox()
        lstTiers = New ListBox()
        lblTiers = New Label()
        lblCategorie = New Label()
        lstCategorie = New ListBox()
        lstSousCategorie = New ListBox()
        lblSousCategorie = New Label()
        SuspendLayout()
        ' 
        ' lblType
        ' 
        lblType.AutoSize = True
        lblType.Location = New Point(20, 103)
        lblType.Name = "lblType"
        lblType.Size = New Size(31, 15)
        lblType.TabIndex = 0
        lblType.Text = "Type"
        ' 
        ' dateMvt
        ' 
        dateMvt.AllowDrop = True
        dateMvt.Location = New Point(17, 33)
        dateMvt.Name = "dateMvt"
        dateMvt.Size = New Size(194, 23)
        dateMvt.TabIndex = 1
        ' 
        ' lstType
        ' 
        lstType.FormattingEnabled = True
        lstType.ItemHeight = 15
        lstType.Location = New Point(94, 98)
        lstType.Name = "lstType"
        lstType.Size = New Size(241, 34)
        lstType.TabIndex = 2
        ' 
        ' lstTiers
        ' 
        lstTiers.FormattingEnabled = True
        lstTiers.ItemHeight = 15
        lstTiers.Location = New Point(97, 141)
        lstTiers.Name = "lstTiers"
        lstTiers.Size = New Size(265, 34)
        lstTiers.TabIndex = 3
        ' 
        ' lblTiers
        ' 
        lblTiers.AutoSize = True
        lblTiers.Location = New Point(12, 133)
        lblTiers.Name = "lblTiers"
        lblTiers.Size = New Size(31, 15)
        lblTiers.TabIndex = 4
        lblTiers.Text = "Tiers"
        ' 
        ' lblCategorie
        ' 
        lblCategorie.AutoSize = True
        lblCategorie.Location = New Point(20, 186)
        lblCategorie.Name = "lblCategorie"
        lblCategorie.Size = New Size(58, 15)
        lblCategorie.TabIndex = 5
        lblCategorie.Text = "Catégorie"
        ' 
        ' lstCategorie
        ' 
        lstCategorie.AllowDrop = True
        lstCategorie.FormattingEnabled = True
        lstCategorie.ItemHeight = 15
        lstCategorie.Location = New Point(119, 182)
        lstCategorie.Name = "lstCategorie"
        lstCategorie.Size = New Size(336, 34)
        lstCategorie.TabIndex = 6
        ' 
        ' lstSousCategorie
        ' 
        lstSousCategorie.AllowDrop = True
        lstSousCategorie.FormattingEnabled = True
        lstSousCategorie.ItemHeight = 15
        lstSousCategorie.Location = New Point(123, 235)
        lstSousCategorie.Name = "lstSousCategorie"
        lstSousCategorie.Size = New Size(328, 34)
        lstSousCategorie.TabIndex = 7
        ' 
        ' lblSousCategorie
        ' 
        lblSousCategorie.AutoSize = True
        lblSousCategorie.Location = New Point(12, 234)
        lblSousCategorie.Name = "lblSousCategorie"
        lblSousCategorie.Size = New Size(86, 15)
        lblSousCategorie.TabIndex = 8
        lblSousCategorie.Text = "Sous-categorie"
        ' 
        ' FrmSaisie
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(800, 450)
        Controls.Add(lblSousCategorie)
        Controls.Add(lstSousCategorie)
        Controls.Add(lstCategorie)
        Controls.Add(lblCategorie)
        Controls.Add(lblTiers)
        Controls.Add(lstTiers)
        Controls.Add(lstType)
        Controls.Add(dateMvt)
        Controls.Add(lblType)
        Name = "FrmSaisie"
        Text = "Saisie d'un mouvement"
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents lblType As Label
    Friend WithEvents dateMvt As DateTimePicker
    Friend WithEvents lstType As ListBox
    Friend WithEvents lstTiers As ListBox
    Friend WithEvents lblTiers As Label
    Friend WithEvents lblCategorie As Label
    Friend WithEvents lstCategorie As ListBox
    Friend WithEvents lstSousCategorie As ListBox
    Friend WithEvents lblSousCategorie As Label

End Class
