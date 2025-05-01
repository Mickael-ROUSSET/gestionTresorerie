Imports System.IO

Public Class batchAnalyseChq
    Private nombreFichiersTraites As Integer = 0
    Private nombreTraitementOK As Integer = 0
    Private nombreTraitementKO As Integer = 0
    Private dateHeureDebut As DateTime
    Private dateHeureFin As DateTime
    Private compteursParRepertoire As New Dictionary(Of String, (fichiersTraites As Integer, traitementOK As Integer, traitementKO As Integer))

    Public Sub ParcourirRepertoireEtAnalyser()
        Dim lectureProprietes As New LectureProprietes()
        Dim sRepChq As String = lectureProprietes.repChq & DateTime.Now.Year

        Try
            ' Enregistrer la date et l'heure de début
            dateHeureDebut = DateTime.Now

            ' Vérifier si le répertoire existe
            If Directory.Exists(sRepChq) Then
                ' Appeler la méthode récursive pour parcourir le répertoire et ses sous-dossiers
                ParcourirEtAnalyserRecursif(sRepChq)
            Else
                Logger.INFO("Le répertoire spécifié : " & sRepChq & " n'existe pas.")
            End If

            ' Enregistrer la date et l'heure de fin
            dateHeureFin = DateTime.Now

            ' Appeler la méthode pour générer le compte rendu de traitement
            GenererCompteRendu()

            ' Message final
            Logger.INFO("Analyse terminée pour tous les fichiers.")

        Catch ex As Exception
            Logger.ERR("Erreur lors du parcours du répertoire : " & sRepChq & " " & ex.Message)
        End Try
    End Sub
    Private Sub ParcourirEtAnalyserRecursif(repertoire As String)
        Try
            ' Initialiser les compteurs pour le répertoire courant
            If Not compteursParRepertoire.ContainsKey(repertoire) Then
                compteursParRepertoire(repertoire) = (0, 0, 0)
            End If

            ' Obtenir tous les fichiers dans le répertoire courant
            Dim fichiers As String() = Directory.GetFiles(repertoire)

            ' Parcourir chaque fichier et appeler analyseChq
            For Each cheminFichier As String In fichiers
                Try
                    analyseChq(cheminFichier)
                    nombreFichiersTraites += 1
                    nombreTraitementOK += 1
                    Dim compteur = compteursParRepertoire(repertoire)
                    compteursParRepertoire(repertoire) = (compteur.fichiersTraites + 1, compteur.traitementOK + 1, compteur.traitementKO)
                Catch ex As Exception
                    Logger.ERR($"Erreur lors de l'analyse du fichier : {cheminFichier} - {ex.Message}")
                    nombreTraitementKO += 1
                    Dim compteur = compteursParRepertoire(repertoire)
                    compteursParRepertoire(repertoire) = (compteur.fichiersTraites + 1, compteur.traitementOK, compteur.traitementKO + 1)
                End Try
            Next

            ' Obtenir tous les sous-dossiers dans le répertoire courant
            Dim sousDossiers As String() = Directory.GetDirectories(repertoire)

            ' Parcourir chaque sous-dossier et appeler récursivement la méthode
            For Each sousDossier As String In sousDossiers
                ParcourirEtAnalyserRecursif(sousDossier)
            Next
        Catch ex As Exception
            Logger.ERR("Erreur lors du parcours du répertoire : " & repertoire & " " & ex.Message)
        End Try
    End Sub
    Public Shared Sub analyseChq(cheminChq As String)
        'Dim extraction As New AppelMistral()

        Logger.INFO("-------------------" & vbCrLf & "Analyse du chèque " & cheminChq)
        Dim chqJson = AppelMistral.litImage(cheminChq)
        cheminChq = RenommerFichier(cheminChq, chqJson.numero_du_cheque)
        chqJson.InsereEnBase(cheminChq)
        Logger.INFO("Insertion en base du chèque " & cheminChq)
    End Sub

    ' Méthode pour renommer un fichier et renvoyer le nouveau nom
    Public Shared Function RenommerFichier(cheminFichier As String, numChq As String) As String
        Try
            ' Vérifier si le fichier existe
            If Not File.Exists(cheminFichier) Then
                Throw New FileNotFoundException("Le fichier spécifié n'existe pas.", cheminFichier)
            End If

            ' Obtenir le répertoire et l'extension du fichier
            Dim repertoire As String = Path.GetDirectoryName(cheminFichier)
            Dim extension As String = Path.GetExtension(cheminFichier)

            ' Construire le nouveau nom de fichier
            Dim nouveauNomFichier As String = Path.Combine(repertoire, $"CHQ_{numChq}{extension}")

            ' Vérifier si un fichier avec le même nom existe déjà
            If File.Exists(nouveauNomFichier) Then
                ' Écrire un log de niveau INFO
                Logger.INFO($"Un fichier avec le nom '{nouveauNomFichier}' existe déjà. Renommage annulé.")
                Return cheminFichier ' Retourne le nom d'origine si le renommage est annulé
            End If

            ' Renommer le fichier
            File.Move(cheminFichier, nouveauNomFichier)
            Logger.INFO($"Fichier {cheminFichier} renommé avec succès en '{nouveauNomFichier}'.")

            ' Retourner le nouveau nom du fichier
            Return nouveauNomFichier

        Catch ex As Exception
            ' Écrire un log d'erreur en cas d'exception
            Logger.ERR($"Erreur lors du renommage du fichier : {ex.Message}")
            Return cheminFichier ' Retourne le nom d'origine en cas d'erreur
        End Try
    End Function

    ' Méthode dédiée pour générer le compte rendu de traitement
    Private Sub GenererCompteRendu()
        Logger.INFO($"Compte rendu de traitement :")
        Logger.INFO($"Nombre de fichiers traités : {nombreFichiersTraites}")
        Logger.INFO($"Nombre de traitements OK : {nombreTraitementOK}")
        Logger.INFO($"Nombre de traitements KO : {nombreTraitementKO}")
        Logger.INFO($"Date/Heure de début : {dateHeureDebut}")
        Logger.INFO($"Date/Heure de fin : {dateHeureFin}")

        ' Log des compteurs par répertoire
        Logger.INFO($"Compte rendu par répertoire :")
        For Each kvp As KeyValuePair(Of String, (Integer, Integer, Integer)) In compteursParRepertoire
            Logger.INFO($"Répertoire : {kvp.Key}")
            Logger.INFO($"  Fichiers traités : {kvp.Value.Item1}")
            Logger.INFO($"  Traitements OK : {kvp.Value.Item2}")
            Logger.INFO($"  Traitements KO : {kvp.Value.Item3}")
        Next
    End Sub
End Class
