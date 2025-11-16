Imports System.Data.SqlClient
Imports ADODB

Public Class SqlCommandBuilder
    ' Constructeur privé pour empêcher l'instanciation directe
    Private Sub New()
    End Sub

    ' Méthode pour créer et renvoyer un SqlCommand
    Public Shared Function CreateSqlCommand(sBase As String, query As String, Optional parameters As Dictionary(Of String, Object) = Nothing) As SqlCommand
        Try
            ' Associer la connexion à la commande
            Dim command As New SqlCommand(LectureProprietes.GetVariable(query)) With {
                .Connection = ConnexionDB.GetInstance(sBase).getConnexion(sBase)
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
    End Function    ''' <summary>
    ''' Exécute une requête SQL paramétrée et retourne les résultats sous forme d'une liste d'objets typés.
    ''' </summary>
    ''' <typeparam name="T">Type métier cible (ex : Tiers, Categorie...)</typeparam>
    ''' <param name="nomRequete">Nom ou texte de la requête SQL</param>
    ''' <param name="parametres">Dictionnaire de paramètres nommés</param>
    ''' <returns>Liste d'objets de type T</returns>
    ' 🔹 Exécute une requête et renvoie un DataTable
    Public Shared Function ExecuteDataTable(sBase As String, nomRequete As String, Optional params As Dictionary(Of String, Object) = Nothing) As DataTable
        Dim dt As New DataTable
        Dim conn As SqlConnection = ConnexionDB.GetInstance(sBase).getConnexion(sBase)
        Using cmd = CreateSqlCommand(sBase, nomRequete, params)
            cmd.Connection = conn
            Using da As New SqlDataAdapter(cmd)
                da.Fill(dt)
            End Using
        End Using
        Return dt
    End Function

    ' 🔹 Exécute une requête et renvoie une liste d'entités typées
    Public Shared Function GetEntities(Of T As {BaseDataRow, New})(sBase As String, nomRequete As String, Optional params As Dictionary(Of String, Object) = Nothing) As List(Of T)
        Try
            Dim dt = ExecuteDataTable(sBase, nomRequete, params)
            Return DataRowUtils.FromDataTableGeneric(Of T)(dt)
        Catch ex As Exception
            Logger.ERR($"Erreur lors du chargement des entités {GetType(T).Name} depuis {nomRequete} : {ex.Message}")
            Return New List(Of T)
        End Try
    End Function
End Class