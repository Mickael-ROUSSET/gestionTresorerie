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
        btnAfficheDoc = New Button()
        CType(pbDocument, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' btnSelDoc
        ' 
        btnSelDoc.Location = New Point(61, 145)
        btnSelDoc.Name = "btnSelDoc"
        btnSelDoc.Size = New Size(84, 23)
        btnSelDoc.TabIndex = 2
        btnSelDoc.Text = "&Sélectionne"
        btnSelDoc.UseVisualStyleBackColor = True
        ' 
        ' pbDocument
        ' 
        pbDocument.Location = New Point(61, 197)
        pbDocument.Name = "pbDocument"
        pbDocument.Size = New Size(549, 233)
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
        ' btnAfficheDoc
        ' 
        btnAfficheDoc.Location = New Point(58, 441)
        btnAfficheDoc.Name = "btnAfficheDoc"
        btnAfficheDoc.Size = New Size(100, 23)
        btnAfficheDoc.TabIndex = 5
        btnAfficheDoc.Text = "Affiche doc"
        btnAfficheDoc.UseVisualStyleBackColor = True
        ' 
        ' FrmSelectionneDocument
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(800, 483)
        Controls.Add(btnAfficheDoc)
        Controls.Add(lstDocuments)
        Controls.Add(pbDocument)
        Controls.Add(btnSelDoc)
        Name = "FrmSelectionneDocument"
        Text = "Sélectionne un document"
        CType(pbDocument, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub
    Friend WithEvents btnSelDoc As Button
    Friend WithEvents pbDocument As PictureBox
    Friend WithEvents lstDocuments As ListView
    Friend WithEvents btnAfficheDoc As Button
End Class
