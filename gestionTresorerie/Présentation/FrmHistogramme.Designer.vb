<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FrmHistogramme
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
        components = New ComponentModel.Container()
        Dim ChartArea1 As System.Windows.Forms.DataVisualization.Charting.ChartArea = New DataVisualization.Charting.ChartArea()
        Dim Legend1 As System.Windows.Forms.DataVisualization.Charting.Legend = New DataVisualization.Charting.Legend()
        Dim Series1 As System.Windows.Forms.DataVisualization.Charting.Series = New DataVisualization.Charting.Series()
        Dim Title1 As System.Windows.Forms.DataVisualization.Charting.Title = New DataVisualization.Charting.Title()
        ToolTipHisto = New ToolTip(components)
        ChartBilan = New DataVisualization.Charting.Chart()
        CType(ChartBilan, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' ChartBilan
        ' 
        ChartArea1.Name = "ChartArea1"
        ChartBilan.ChartAreas.Add(ChartArea1)
        Legend1.Name = "Legend1"
        ChartBilan.Legends.Add(Legend1)
        ChartBilan.Location = New Point(3, 51)
        ChartBilan.Name = "ChartBilan"
        Series1.ChartArea = "ChartArea1"
        Series1.Legend = "Legend1"
        Series1.Name = "Series1"
        ChartBilan.Series.Add(Series1)
        ChartBilan.Size = New Size(689, 465)
        ChartBilan.TabIndex = 0
        ChartBilan.Text = "Chart1"
        Title1.Name = "Title1"
        ChartBilan.Titles.Add(Title1)
        ' 
        ' frmHistogramme
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(747, 561)
        Controls.Add(ChartBilan)
        Name = "frmHistogramme"
        Text = "Histogramme"
        CType(ChartBilan, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents ToolTipHisto As ToolTip
    Friend WithEvents ChartBilan As DataVisualization.Charting.Chart
End Class
