Public Class LectureProprietes
    Private Shared _env As String
    Private Shared _repInstallation As String

    Public Sub New()
        ' Lire la variable d'environnement 
        Call litEnv()
        Call litRepInstallation()
    End Sub
    Public Shared Sub litEnv()
        litEnv(AddressOf Environment.GetEnvironmentVariable)
    End Sub

    Public Shared Sub litEnv(readVariable As Func(Of String, EnvironmentVariableTarget, String))
        EnvironmentManager.LoadEnvironment(readVariable)
        Dim selected = EnvironmentManager.CurrentEnvironment.ToString()
        If Not String.Equals(_env, selected, StringComparison.Ordinal) Then
            _repInstallation = Nothing
        End If
        _env = selected
        Logger.INFO("Env = " & _env)
    End Sub
    Public Sub litRepInstallation()
        ' Lire la variable d'environnement 
        If _repInstallation = String.Empty Then
            _repInstallation = If(String.Equals(_env, "PROD", StringComparison.OrdinalIgnoreCase), My.Settings.repInstallationProd, My.Settings.repInstallationTest)
            Logger.INFO($"repInstallation = {_repInstallation}")
        End If
    End Sub

    Public ReadOnly Property repInstallation() As String
        Get
            Return _repInstallation
        End Get
    End Property
    Public Shared ReadOnly Property connexionString(sBase As String) As String
        Get
            Return My.Settings.DataSoure &
                "Initial Catalog=" & GetDatabaseName() & ";" &
                My.Settings.IntegratedSecurity &
                My.Settings.MultipleActiveResultSets &
                My.Settings.ConnectTimeout
        End Get
    End Property
    Private Shared Function GetDatabaseName() As String
        If String.IsNullOrEmpty(_env) Then litEnv()
        Select Case _env.ToUpperInvariant()
            Case EnvironnementLibelles.Libelles(Environnement.Prod).ToUpperInvariant()
                'Prod
                Return BaseDonneesLibelles.Libelles(BaseDonnees.bddAgumaaa)
            Case Else
                'Test
                Return BaseDonneesLibelles.Libelles(BaseDonnees.bddAgumaaaTest)
        End Select
    End Function
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