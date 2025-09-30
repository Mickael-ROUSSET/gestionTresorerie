

Imports System.Data.SqlClient

Public Class Lanceur
    Private Shared iTypeDocument As String

    Public Shared Sub LanceTrt()
        ' Sélectionner les types de document (classes) avec sqlTypesDocuments
        For Each sClasse In GetTypesDocument()
            Try
                ' Créer une instance de la classe spécifiée par sClasse via réflexion
                Dim typeClasse As Type = Type.GetType(sClasse)
                If typeClasse Is Nothing Then
                    Logger.WARN($"Classe {sClasse} non trouvée.")
                    Continue For
                End If

                Dim instanceClasse As Object = Activator.CreateInstance(typeClasse)
                Dim batchAnalyse = New batchAnalyse(instanceClasse)

                ' Appel de l'analyse des fichiers avec le type d'analyse défini dans le constructeur
                batchAnalyse.ParcourirRepertoireEtAnalyser()
            Catch ex As Exception
                Logger.ERR($"Erreur lors de l'instanciation ou de l'analyse pour la classe {sClasse} : {ex.Message}")
            End Try
        Next sClasse
    End Sub
    Private Shared Function GetTypesDocument() As List(Of String)
        Dim listId As New List(Of String)

        Using reader As SqlDataReader = SqlCommandBuilder.CreateSqlCommand("sqlTypesDocuments").ExecuteReader()
            ' Vérifier si le reader contient des lignes
            If reader.HasRows Then
                While reader.Read()
                    listId.Add(reader.GetString(0)) 
                End While
                Logger.INFO("Types de documents récupérés avec succès.")
            Else
                ' Gérer le cas où le reader est vide
                Logger.WARN("Aucun type de document trouvé.")
            End If
        End Using

        Return listId
    End Function
End Class