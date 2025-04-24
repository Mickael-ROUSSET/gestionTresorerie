Imports System.Data.SqlClient
Imports System.Diagnostics

Public Class SqlCommandBuilder
    ' Constructeur privé pour empêcher l'instanciation directe
    Private Sub New()
    End Sub

    ' Méthode pour créer et renvoyer un SqlCommand
    Public Shared Function CreateSqlCommand(query As String, Optional parameters As Dictionary(Of String, Object) = Nothing) As SqlCommand
        Try
            Dim command As New SqlCommand(LectureProprietes.GetVariable(query))

            ' Associer la connexion à la commande
            command.Connection = ConnexionDB.GetInstance.getConnexion
            ' Ajouter les paramètres à la commande si fournis
            If parameters IsNot Nothing Then
                For Each param In parameters
                    command.Parameters.AddWithValue(param.Key, param.Value)
                Next
            End If

            Return command
        Catch ex As SqlException
            LogError("Erreur SQL lors de la création de la commande", ex)
            Throw
        Catch ex As Exception
            LogError("Erreur inattendue lors de la création de la commande", ex)
            Throw
        End Try
    End Function

    ' Méthode pour logger les erreurs avec le nom de la méthode appelante
    Private Shared Sub LogError(message As String, ex As Exception)
        Dim stackTrace As New StackTrace()
        Dim callingMethod As String = stackTrace.GetFrame(1).GetMethod().Name
        Logger.ERR($"{message} dans {callingMethod} : {ex.Message}")
    End Sub

End Class