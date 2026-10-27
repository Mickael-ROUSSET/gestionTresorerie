Imports System.IO

Public Class Lanceur
    'Private Shared iTypeDocument As String
    ' Propriétés pour stocker les états  
    Private Property RepertoireSource As String

    Private Shared Function CreateRepository() As BatchAnalyseRepository
        Dim factory As IConnectionFactory = RepositoryFactory.CreateConnectionFactory(Constantes.DataBases.Agumaaa)

        Dim provider As ISqlTextProvider = RepositoryFactory.CreateSqlTextProvider()

        Return New BatchAnalyseRepository(factory, provider)
    End Function
    Public Async Function LanceTrt() As Task
        Try
            ' La proposition documentaire ne requiert aucune connexion SQL.
            ' La consultation des tiers sera reservee a la validation humaine.

            If Not DemanderConfirmationRepertoire() Then
                Logger.INFO("Analyse annulée par l'utilisateur.")
                Return ' Utiliser Return dans une Task au lieu de Exit Function
            End If

            ' 2. Lancement du traitement
            Dim repertoireSource = ValiderRepertoireSelectionne(RepertoireSource)
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
    ''' Utiliser exclusivement le dossier confirme par l'utilisateur.
    ''' Ne jamais revenir silencieusement au dossier par defaut.
    ''' </summary>
    Public Shared Function ValiderRepertoireSelectionne(repertoire As String) As String
        If String.IsNullOrWhiteSpace(repertoire) Then
            Throw New ArgumentException("Aucun repertoire selectionne.", NameOf(repertoire))
        End If
        Dim cheminComplet = Path.GetFullPath(repertoire)
        If Not Directory.Exists(cheminComplet) Then
            Throw New DirectoryNotFoundException("Repertoire selectionne introuvable : " & cheminComplet)
        End If
        Return cheminComplet
    End Function

    ''' <summary>
    ''' Module d'interface : Gère le dialogue de sélection
    ''' </summary>
    Private Function DemanderConfirmationRepertoire() As Boolean
        Using fbd As New FolderBrowserDialog()
            fbd.Description = "Sélectionnez le répertoire à analyser"
            fbd.ShowNewFolderButton = False

            ' Positionne l'explorateur sur le chemin par défaut s'il existe
            Dim repertoireInitial = If(String.IsNullOrWhiteSpace(Me.RepertoireSource),
                                       ConstruireCheminParDefaut(), Me.RepertoireSource)
            If Directory.Exists(repertoireInitial) Then
                fbd.SelectedPath = repertoireInitial
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