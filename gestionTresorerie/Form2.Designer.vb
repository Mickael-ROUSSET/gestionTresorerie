<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmGraphiques
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
        CheckBox1 = New CheckBox()
        picGraph1 = New PictureBox()
        CType(picGraph1, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' CheckBox1
        ' 
        CheckBox1.AutoSize = True
        CheckBox1.Location = New Point(656, 284)
        CheckBox1.Name = "CheckBox1"
        CheckBox1.Size = New Size(85, 19)
        CheckBox1.TabIndex = 0
        CheckBox1.Text = "CheckBox1"
        CheckBox1.UseVisualStyleBackColor = True
        ' 
        ' picGraph1
        ' 
        picGraph1.Location = New Point(282, 219)
        picGraph1.Name = "picGraph1"
        picGraph1.Size = New Size(100, 50)
        picGraph1.TabIndex = 1
        picGraph1.TabStop = False
        ' 
        ' frmGraphiques
        ' 
        AutoScaleDimensions = New SizeF(7.0F, 15.0F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(800, 450)
        Controls.Add(picGraph1)
        Controls.Add(CheckBox1)
        Name = "frmGraphiques"
        Text = "Graphiques"
        CType(picGraph1, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents CheckBox1 As CheckBox

    Private Sub frmGraphiques_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim tabX As Integer(), tabY As Integer()
        Dim newBitmap As Bitmap = New Bitmap(150, 150)
        Dim g As Graphics = Graphics.FromImage(newBitmap)
        Dim PF(499) As PointF
        Dim i As Integer

        For i = 1 To 100
            tabX(i) = i
            tabY(i) = 100 - i
        Next i
        ' je remplis mon tableau de pointF avec les valeurs de mes tableaux
        For i = 0 To 499
            Dim point As New PointF(CSng(tabX(i)), CSng(tabY(i)))
            PF(i) = point
        Next
        g.DrawLines(Pens.Blue, PF)
        picGraph1.Image = newBitmap
    End Sub

    Friend WithEvents picGraph1 As PictureBox
End Class
