Imports System.Data.SqlClient
Imports System.IO

Public Class Lanceur
    Private Shared iTypeDocument As String
    ' Propriétés pour stocker les états  
    Private Property RepertoireSource As String

    Public Async Function LanceTrt() As Task
        ' Vérifie si le fichier lstTiers.csv existe, sinon le génère.
        VerifierOuGenererLstTiers()


        ' Demander confirmation/choix à l'utilisateur
        If Not DemanderConfirmationRepertoire() Then
            Logger.INFO("Analyse annulée par l'utilisateur.")
            Exit Function
        End If

        ' Sélectionner les types de document (classes) avec GetTypesDocument
        For Each typeDoc In GetTypesDocument()
            Try
                Dim batchAnalyse = New batchAnalyse(typeDoc)

                ' Appel de l'analyse des fichiers avec le type d'analyse défini dans le constructeur
                Await ParcourirRepertoireEtAnalyser(batchAnalyse)
            Catch ex As Exception
                Logger.ERR("Erreur lors de l'instanciation ou de l'analyse pour la classe " & "gestionTresorerie." & typeDoc.ClasseTypeDoc & " : " & ex.Message)
            End Try
        Next typeDoc
    End Function

    ' <summary>
    ' Point d'entrée principal : Orchestre le processus
    ' </summary>
    Public Async Function ParcourirRepertoireEtAnalyser(batchAnalyse As batchAnalyse) As Task
        ' 1. Préparer le chemin
        Me.RepertoireSource = ConstruireCheminParDefaut()

        ' 2. Lancer le traitement
        Await batchAnalyse.DemarrerTraitement(Me.RepertoireSource)
    End Function

    ''' <summary>
    ''' Module de configuration : Construit le chemin à partir des propriétés
    ''' </summary>
    Private Function ConstruireCheminParDefaut() As String
        Return Path.Combine(
            LectureProprietes.GetVariable("repRacineAgumaaa"),
            LectureProprietes.GetVariable("repRacineDocuments"),
            LectureProprietes.GetVariable("repFichiersDocumentsATrier")
        )
    End Function

    ''' <summary>
    ''' Module d'interface : Gère le dialogue de sélection
    ''' </summary>
    Private Function DemanderConfirmationRepertoire() As Boolean
        Using fbd As New FolderBrowserDialog()
            fbd.Description = "Sélectionnez le répertoire à analyser"
            fbd.ShowNewFolderButton = False

            ' Positionne l'explorateur sur le chemin par défaut s'il existe
            If Directory.Exists(Me.RepertoireSource) Then
                fbd.SelectedPath = Me.RepertoireSource
            End If

            If fbd.ShowDialog() = DialogResult.OK Then
                Me.RepertoireSource = fbd.SelectedPath
                Return True
            End If
        End Using
        Return False
    End Function
    Private Shared Function GetTypesDocument() As List(Of ITypeDoc)
        Dim listTypeDoc As New List(Of ITypeDoc)

        Using reader As SqlDataReader = SqlCommandBuilder.CreateSqlCommand(Constantes.bddAgumaaa, "sqlTypesDocuments").ExecuteReader()
            ' Vérifier si le reader contient des lignes
            If reader.HasRows Then
                While reader.Read()
                    ' Créer une instance concrète implémentant ITypeDoc avec seulement la classe
                    Dim typeDoc As New TypeDocImpl(reader.GetString(3))
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