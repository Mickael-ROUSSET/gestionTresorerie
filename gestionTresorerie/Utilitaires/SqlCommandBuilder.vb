Imports System.Data.SqlClient

Public Class SqlCommandBuilder
    ' Constructeur privé pour empêcher l'instanciation directe
    Private Sub New()
    End Sub

    ' Méthode pour créer et renvoyer un SqlCommand
    Public Shared Function CreateSqlCommand(query As String, Optional parameters As Dictionary(Of String, Object) = Nothing) As SqlCommand
        Try
            ' Associer la connexion à la commande
            Dim command As New SqlCommand(LectureProprietes.GetVariable(query)) With {
                .Connection = ConnexionDB.GetInstance.getConnexion
            }
            ' Ajouter les paramètres à la commande si fournis
            If parameters IsNot Nothing Then
                For Each param In parameters
                    Dim unused = command.Parameters.AddWithValue(param.Key, param.Value)
                Next
            End If
            'Logger.INFO($"Création de la commande : {command.CommandText }")

            Return command
        Catch ex As SqlException
            Logger.ERR($"Erreur SQL lors de la création de la commande : {ex.Message}")
            Throw
        Catch ex As Exception
            Logger.ERR($"Erreur inattendue lors de la création de la commande : {ex.Message}")
            Throw
        End Try
    End Function
End Class