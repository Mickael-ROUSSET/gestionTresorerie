

Imports System.Data.SqlClient
Imports System.IO

Public Class Lanceur
    Private Shared iTypeDocument As String

    Public Shared Sub LanceTrt()
        ' Vérifie si le fichier lstTiers.csv existe, sinon le génère.
        VerifierOuGenererLstTiers()
        ' Sélectionner les types de document (classes) avec GetTypesDocument
        For Each typeDoc In GetTypesDocument()
            Try
                Dim batchAnalyse = New batchAnalyse(typeDoc)

                ' Appel de l'analyse des fichiers avec le type d'analyse défini dans le constructeur
                batchAnalyse.ParcourirRepertoireEtAnalyser()
            Catch ex As Exception
                Logger.ERR("Erreur lors de l'instanciation ou de l'analyse pour la classe " & "gestionTresorerie." & typeDoc.ClasseTypeDoc & " : " & ex.Message)
            End Try
        Next typeDoc
    End Sub
    Private Shared Function GetTypesDocument() As List(Of ITypeDoc)
        Dim listTypeDoc As New List(Of ITypeDoc)

        Using reader As SqlDataReader = SqlCommandBuilder.CreateSqlCommand(Constantes.bddAgumaaa, "sqlTypesDocuments").ExecuteReader()
            ' Vérifier si le reader contient des lignes
            If reader.HasRows Then
                While reader.Read()
                    ' Créer une instance concrète implémentant ITypeDoc
                    Dim typeDoc As New TypeDocImpl(reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetString(3))
                    listTypeDoc.Add(typeDoc)
                End While
                Logger.INFO($"{reader.RecordsAffected} types de documents récupérés avec succès.")
            Else
                ' Gérer le cas où le reader est vide
                Logger.WARN("Aucun type de document trouvé.")
            End If
        End Using

        Return listTypeDoc
    End Function

    Private Shared Sub VerifierOuGenererLstTiers()
        ' Vérifie si le fichier lstTiers.csv existe, sinon exécute la requête reqIdentiteTiers pour le générer.
        Dim sFicLstTiers As String = LectureProprietes.GetVariable("ficLstTiers")
        If Not File.Exists(sFicLstTiers) Then
            Logger.WARN("Le fichier lstTiers.csv est introuvable. Génération en cours...")

            Try
                Using reader As SqlDataReader = SqlCommandBuilder.CreateSqlCommand(Constantes.bddAgumaaa, "reqIdentiteTiers").ExecuteReader()
                    Using writer As New StreamWriter(sFicLstTiers, False, System.Text.Encoding.UTF8)
                        ' Écrire les en-têtes
                        For i As Integer = 0 To reader.FieldCount - 1
                            writer.Write(reader.GetName(i))
                            If i < reader.FieldCount - 1 Then writer.Write(";")
                        Next
                        writer.WriteLine()

                        ' Écrire les lignes
                        While reader.Read()
                            For i As Integer = 0 To reader.FieldCount - 1
                                writer.Write(reader(i).ToString())
                                If i < reader.FieldCount - 1 Then writer.Write(";")
                            Next
                            writer.WriteLine()
                        End While
                    End Using
                End Using

                Logger.INFO("Fichier lstTiers.csv généré avec succès via la requête reqIdentiteTiers.")
            Catch ex As Exception
                Logger.ERR($"Erreur lors de la génération du fichier lstTiers.csv : {ex.Message}")
                Throw
            End Try
        Else
            Logger.INFO("Fichier lstTiers.csv trouvé, aucune régénération nécessaire.")
        End If
    End Sub

End Class