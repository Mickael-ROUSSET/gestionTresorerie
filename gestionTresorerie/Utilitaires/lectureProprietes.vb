Public Class LectureProprietes
    Private Shared _env As String
    Private Shared _repInstallation As String

    Public Sub New()
        ' Lire la variable d'environnement 
        Call litEnv()
        Call litRepInstallation()
    End Sub
    Public Shared Sub litEnv()
        ' Lire la variable d'environnement 
        If _env = String.Empty Then
            _env = Environ("envAgumaaa")
            Logger.INFO("Env = " & _env)
        End If
    End Sub
    Public Sub litRepInstallation()
        ' Lire la variable d'environnement 
        If _repInstallation = String.Empty Then
            _repInstallation = If(_env = "Prod", My.Settings.repInstallationProd, My.Settings.repInstallationTest)
            Logger.INFO($"repInstallation = {_repInstallation}")
        End If
    End Sub

    Public ReadOnly Property repInstallation() As String
        Get
            Return _repInstallation
        End Get
    End Property
    Public Shared ReadOnly Property connexionString() As String
        Get
            If _env = "Prod" Then
                '"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=&quot;G:\Mon Drive\AGUMAAA\Documents\BacASable\bddAgumaaa.mdf&quot;;Integrated Security=True;Connect Timeout=30" /> 
                Return My.Settings.DataSource & "'" & My.Settings.AttachDbFilenameProd & "'" & My.Settings.ParamDb
            Else
                Return My.Settings.DataSource & My.Settings.AttachDbFilenameTest & My.Settings.ParamDb
            End If
        End Get
    End Property
    Public Shared Function GetVariable(nomVariable As String) As String
        ' Retourner la variable demandée 
        Return My.Settings.Item(nomVariable)
    End Function
    Public Shared Function GetCheminEtVariable(nomVariable As String) As String
        ' Retourner la variable demandée
        'N.B. : elle ne doit pas dépendre pas de l'environnement Prod / test 
        Return _repInstallation & My.Settings.Item(nomVariable)
    End Function
End Class