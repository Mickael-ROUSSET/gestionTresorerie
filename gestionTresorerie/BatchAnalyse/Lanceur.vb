Imports System.IO

Public Class Lanceur
    'Private Shared iTypeDocument As String
    ' Propriétés pour stocker les états  
    Private Property RepertoireSource As String

    Private Shared Function CreateRepository() As BatchAnalyseRepository
        Dim connectionString As String =
        ConnexionDB.GetInstance(Constantes.DataBases.Agumaaa).
                    GetConnexion(Constantes.DataBases.Agumaaa).
                    ConnectionString

        Dim factory As New AgumaaaConnectionFactory(connectionString)
        Dim provider As ISqlTextProvider = New LegacySqlTextProvider()

        Return New BatchAnalyseRepository(factory, provider)
    End Function
    Public Async Function LanceTrt() As Task
        Try
            ' 1. Préparation (Données Tiers et Confirmation Utilisateur)
            VerifierOuGenererLstTiers()

            If Not DemanderConfirmationRepertoire() Then
                Logger.INFO("Analyse annulée par l'utilisateur.")
                Return ' Utiliser Return dans une Task au lieu de Exit Function
            End If

            ' 2. Lancement du traitement
            Dim repertoireSource = ConstruireCheminParDefaut()
            Dim batch = New BatchAnalyse()

            Logger.INFO($"Démarrage de l'analyse dans : {repertoireSource}")
            Await batch.DemarrerTraitement(repertoireSource)

        Catch ex As Exception
            Logger.ERR($"Erreur lors du traitement global : {ex.Message}")
        End Try
    End Function

    ''' <summary>
    ''' Module de configuration : Construit le chemin à partir des propriétés
    ''' </summary>
    Private Shared Function ConstruireCheminParDefaut() As String
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

    Private Shared Sub VerifierOuGenererLstTiers()
        Dim sFicLstTiers As String = LectureProprietes.GetVariable("ficLstTiers")

        If File.Exists(sFicLstTiers) Then
            Logger.INFO("Fichier lstTiers.csv trouvé, aucune régénération nécessaire.")
            Exit Sub
        End If

        Logger.WARN("Le fichier lstTiers.csv est introuvable. Génération en cours...")

        Try
            CreateRepository().ExporterCsvDepuisRequete("reqIdentiteTiers", sFicLstTiers)
            Logger.INFO("Fichier lstTiers.csv généré avec succès via la requête reqIdentiteTiers.")
        Catch ex As Exception
            Logger.ERR($"Erreur lors de la génération du fichier lstTiers.csv : {ex.Message}")
            Throw
        End Try
    End Sub

End Class