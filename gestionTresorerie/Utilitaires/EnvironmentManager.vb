Public Module EnvironmentManager

    Public Enum EnvironnementType
        DEV
        TEST
        PROD
    End Enum

    Public CurrentEnvironment As EnvironnementType = EnvironnementType.DEV

    ''' <summary>
    ''' Lit la variable système et met à jour l'environnement actif.
    ''' </summary>
    Public Sub LoadEnvironment()
        Dim env As String = Environment.GetEnvironmentVariable("envAgumaaa", EnvironmentVariableTarget.Machine)

        If String.IsNullOrWhiteSpace(env) Then
            env = Environment.GetEnvironmentVariable("envAgumaaa", EnvironmentVariableTarget.User)
        End If

        If String.IsNullOrWhiteSpace(env) Then
            env = "DEV" ' Par défaut
        End If

        Select Case env.ToUpper()
            Case "DEV"
                CurrentEnvironment = EnvironnementType.DEV
            Case "TEST"
                CurrentEnvironment = EnvironnementType.TEST
            Case "PROD"
                CurrentEnvironment = EnvironnementType.PROD
            Case Else
                CurrentEnvironment = EnvironnementType.DEV
        End Select
    End Sub

    ''' <summary>
    ''' Modification de la variable système.
    ''' </summary>
    Public Sub SetEnvironment(env As EnvironnementType)

        Environment.SetEnvironmentVariable("AGUMAAA_ENV", env.ToString(), EnvironmentVariableTarget.User)

        CurrentEnvironment = env
    End Sub

    ''' <summary>
    ''' Retourne une chaîne lisible.
    ''' </summary>
    Public Function GetEnvironmentLabel() As String
        Select Case CurrentEnvironment
            Case EnvironnementType.DEV : Return "Développement"
            Case EnvironnementType.TEST : Return "Test"
            Case EnvironnementType.PROD : Return "Production"
            Case Else : Return "Inconnu"
        End Select
    End Function
End Module
