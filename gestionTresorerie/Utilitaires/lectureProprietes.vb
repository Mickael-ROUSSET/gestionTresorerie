Imports System.Configuration

Public Class lectureProprietes
    Private Shared _env As String
    Private Shared _repInstallation As String

    Public Sub New()
        ' Lire la variable d'environnement 
        _env = Environ("envAgumaaa")
        If _env = "Prod" Then
            _repInstallation = My.Settings.repInstallationProd
        Else
            _repInstallation = My.Settings.repInstallationTest
        End If
        Logger.GetInstance().INFO("Env = " & _env)
        Logger.GetInstance().INFO("repInstallation = " & _repInstallation)
    End Sub

    Public ReadOnly Property repInstallation() As String
        Get
            If _env = "Prod" Then
                Return My.Settings.repInstallationProd
            Else
                Return My.Settings.repInstallationTest
            End If
        End Get
    End Property
    Public ReadOnly Property repBdd() As String
        Get
            If _env = "Prod" Then
                Return My.Settings.ficBddDonneesProd
            Else
                Return My.Settings.ficBddDonneesTest
            End If
        End Get
    End Property
    Public ReadOnly Property repChq() As String
        Get
            If _env = "Prod" Then
                Return My.Settings.repChqProd
            Else
                Return My.Settings.repChqTest
            End If
        End Get
    End Property
    Public ReadOnly Property connexionString() As String
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
        'N.B. : elle ne doit pas dépendre pas de l'environnement Prod / test
        Return _repInstallation & My.Settings.Item(nomVariable)
    End Function
End Class