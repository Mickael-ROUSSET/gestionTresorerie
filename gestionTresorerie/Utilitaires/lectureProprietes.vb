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
        Logger.INFO("Env = " & _env)
    End Sub

    Public Shared Sub litEnv(readVariable As Func(Of String, EnvironmentVariableTarget, String))
        EnvironmentManager.LoadEnvironment(readVariable)
        Dim selected = EnvironmentManager.CurrentEnvironment.ToString()
        If Not String.Equals(_env, selected, StringComparison.Ordinal) Then
            _repInstallation = Nothing
        End If
        _env = selected
    End Sub
    Public Sub litRepInstallation()
        EnsureInstallationDirectory()
        Logger.INFO($"repInstallation = {_repInstallation}")
    End Sub

    Private Shared Sub EnsureInstallationDirectory()
        If String.IsNullOrEmpty(_env) Then litEnv()
        If String.Equals(_env, "TEST", StringComparison.Ordinal) Then
            _repInstallation = TestEnvironmentIsolation.GetInstallationDirectory(
                My.Settings.repInstallationProd, My.Settings.repInstallationTest)
        ElseIf String.IsNullOrEmpty(_repInstallation) Then
            _repInstallation = If(String.Equals(_env, "PROD", StringComparison.Ordinal),
                                 My.Settings.repInstallationProd, My.Settings.repInstallationTest)
        End If
    End Sub

    Public ReadOnly Property repInstallation() As String
        Get
            EnsureInstallationDirectory()
            Return _repInstallation
        End Get
    End Property
    Public Shared ReadOnly Property connexionString(sBase As String) As String
        Get
#If DEMO_BUILD Then
            Throw New InvalidOperationException("Configuration SQL interdite en DÉMONSTRATION.")
#Else
            Dim result = My.Settings.DataSoure &
                "Initial Catalog=" & GetDatabaseName() & ";" &
                My.Settings.IntegratedSecurity &
                My.Settings.MultipleActiveResultSets &
                My.Settings.ConnectTimeout
            If String.Equals(_env, "TEST", StringComparison.Ordinal) Then
                TestEnvironmentIsolation.ValidateConnectionString(result)
            End If
            Return result
#End If
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
        EnsureInstallationDirectory()
        If String.Equals(_env, "TEST", StringComparison.Ordinal) Then
            Return TestEnvironmentIsolation.GetFilePath(
                My.Settings.repInstallationProd, My.Settings.repInstallationTest,
                CStr(My.Settings.Item(nomVariable)))
        End If
        Return _repInstallation & My.Settings.Item(nomVariable)
    End Function
End Class