Option Strict On
Option Explicit On

Public Module EnvironmentManager
    Public Const EnvironmentVariableName As String = "AGUMAAA_ENV"
    Private Const LegacyVariableName As String = "envAgumaaa"

    Public Enum EnvironnementType
        DEV
        TEST
        PROD
    End Enum

    Public CurrentEnvironment As EnvironnementType = EnvironnementType.DEV

    ' Le choix utilisateur explicite prime sur la configuration machine.
    ' Sans variable canonique, conserver la priorité historique Machine puis User.
    Public Function ResolveEnvironment(userValue As String, machineValue As String,
                                       legacyMachineValue As String,
                                       legacyUserValue As String) As EnvironnementType
        For Each value In New String() {userValue, machineValue, legacyMachineValue, legacyUserValue}
            If String.IsNullOrWhiteSpace(value) Then Continue For
            Select Case value.Trim().ToUpperInvariant()
                Case "DEV" : Return EnvironnementType.DEV
                Case "TEST" : Return EnvironnementType.TEST
                Case "PROD" : Return EnvironnementType.PROD
                Case Else
                    Throw New InvalidOperationException(
                        "Environnement invalide. Utilisez DEV, TEST ou PROD.")
            End Select
        Next
        Return EnvironnementType.DEV
    End Function

    Public Sub LoadEnvironment()
        LoadEnvironment(AddressOf Environment.GetEnvironmentVariable)
    End Sub

    Public Sub LoadEnvironment(readVariable As Func(Of String, EnvironmentVariableTarget, String))
#If DEMO_BUILD Then
        ' La variante distribuée reste indépendante des paramètres externes.
        CurrentEnvironment = EnvironnementType.DEV
#Else
        If readVariable Is Nothing Then Throw New ArgumentNullException(NameOf(readVariable))
        CurrentEnvironment = ResolveEnvironment(
            readVariable(EnvironmentVariableName, EnvironmentVariableTarget.User),
            readVariable(EnvironmentVariableName, EnvironmentVariableTarget.Machine),
            readVariable(LegacyVariableName, EnvironmentVariableTarget.Machine),
            readVariable(LegacyVariableName, EnvironmentVariableTarget.User))
#End If
    End Sub

    Public Sub SetEnvironment(env As EnvironnementType)
        SetEnvironment(env, AddressOf Environment.SetEnvironmentVariable)
    End Sub

    Public Sub SetEnvironment(env As EnvironnementType,
                              writeVariable As Action(Of String, String, EnvironmentVariableTarget))
#If DEMO_BUILD Then
        Throw New InvalidOperationException(
            "Le changement d'environnement est interdit en DÉMONSTRATION.")
#Else
        If writeVariable Is Nothing Then Throw New ArgumentNullException(NameOf(writeVariable))
        If Not [Enum].IsDefined(GetType(EnvironnementType), env) Then
            Throw New ArgumentOutOfRangeException(NameOf(env))
        End If
        ' Mettre à jour l'état uniquement après une persistance réussie.
        writeVariable(EnvironmentVariableName, env.ToString(), EnvironmentVariableTarget.User)
        CurrentEnvironment = env
#End If
    End Sub

    Public Function GetEnvironmentLabel() As String
        Select Case CurrentEnvironment
            Case EnvironnementType.DEV : Return "Développement"
            Case EnvironnementType.TEST : Return "Test"
            Case EnvironnementType.PROD : Return "Production"
            Case Else : Return "Inconnu"
        End Select
    End Function
End Module
