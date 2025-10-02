Imports System.Data.SqlClient
Imports System.IO
Imports System.Runtime.InteropServices

Public Class batchAnalyse
    Private nombreFichiersTraites As Integer = 0
    Private nombreTraitementOK As Integer = 0
    Private nombreTraitementKO As Integer = 0
    Private dateHeureDebut As DateTime
    Private dateHeureFin As DateTime
    Private _TypeDoc As ITypeDoc

    Private compteursParRepertoire As New Dictionary(Of String, (fichiersTraites As Integer, traitementOK As Integer, traitementKO As Integer))

    Public Sub New(TypeDoc As ITypeDoc)
        _TypeDoc = TypeDoc
    End Sub
    Public Sub ParcourirRepertoireEtAnalyser()
        Dim sRepDoc As String = LectureProprietes.GetVariable("repFichiersDocuments")

        Try
            ' Enregistrer la date et l'heure de début
            dateHeureDebut = DateTime.Now

            ' Vérifier si le répertoire existe
            If Directory.Exists(sRepDoc) Then
                ' Appeler la méthode récursive pour parcourir le répertoire et ses sous-dossiers
                ParcourirEtAnalyserRecursif(sRepDoc)
            Else
                Logger.INFO($"Le répertoire spécifié : '{sRepDoc}' n'existe pas. Penser à se connecter au google drive AGUMAAA")
            End If

            ' Enregistrer la date et l'heure de fin
            dateHeureFin = DateTime.Now

            ' Appeler la méthode pour générer le compte rendu de traitement
            GenererCompteRendu()

            ' Message final
            Logger.INFO($"Analyse terminée pour tous les fichiers du répertoire '{sRepDoc}'.")

        Catch ex As Exception
            Logger.ERR($"Erreur lors du parcours du répertoire : '{sRepDoc}' " & ex.Message)
        End Try
    End Sub
    Private Sub ParcourirEtAnalyserRecursif(repertoire As String)
        Try

            Dim compteur As (fichiersTraites As Integer, traitementOK As Integer, traitementKO As Integer) = Nothing            ' Initialiser les compteurs pour le répertoire courant
            If Not compteursParRepertoire.TryGetValue(repertoire, compteur) Then
                compteur = (0, 0, 0)
                compteursParRepertoire(repertoire) = compteur
            End If

            ' Obtenir tous les fichiers dans le répertoire courant
            Dim fichiers As String() = Directory.GetFiles(repertoire)

            ' Parcourir chaque fichier et appeler analyseChq
            For Each cheminFichier As String In fichiers
                Try
                    analyseDocument(cheminFichier, _TypeDoc.Prompt)
                    nombreFichiersTraites += 1
                    nombreTraitementOK += 1
                    compteursParRepertoire(repertoire) = (compteur.fichiersTraites + 1, compteur.traitementOK + 1, compteur.traitementKO)
                Catch ex As Exception
                    Logger.ERR($"Erreur lors de l'analyse du fichier : {cheminFichier} - {ex.Message}")
                    nombreTraitementKO += 1
                    'Dim compteur = compteursParRepertoire(repertoire)
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
            Logger.ERR($"Erreur lors du parcours du répertoire : {repertoire} " & ex.Message)
        End Try
    End Sub
    Public Sub analyseDocument(cheminChq As String, sPrompt As String)
        'Dim extraction As New AppelMistral()

        Logger.INFO("-------------------" & vbCrLf & "Analyse du chèque " & cheminChq)
        Dim chqJson = AppelMistral.litImage(cheminChq, sPrompt)
        cheminChq = RenommerFichier(cheminChq, chqJson.numero_du_cheque)
        chqJson.InsereEnBase(cheminChq)
        Logger.INFO("Insertion en base du chèque " & cheminChq)
    End Sub

    ' Méthode pour renommer un fichier et renvoyer le nouveau nom
    Public Function RenommerFichier(cheminFichier As String, numChq As String) As String
        Dim sRepSortie As String = LectureProprietes.GetVariable("repFichiersDocuments") & "\" & LectureProprietes.GetVariable("repFichiersDocumentsATrier")
        Try
            ' Vérifier si le fichier existe
            If Not File.Exists(cheminFichier) Then
                Throw New FileNotFoundException("Le fichier spécifié n'existe pas.", cheminFichier)
            End If

            ' Vérifier si le répertoire de sortie existe, sinon le créer
            If Not Directory.Exists(sRepSortie) Then
                Directory.CreateDirectory(sRepSortie)
            End If

            ' Obtenir l'extension du fichier
            Dim extension As String = Path.GetExtension(cheminFichier)

            ' Construire le nouveau chemin complet du fichier dans le répertoire de sortie
            Dim nouveauNomFichier As String = Path.Combine(sRepSortie, $"CHQ_{numChq}{extension}")

            ' Vérifier si un fichier avec le même nom existe déjà dans le répertoire de sortie
            If File.Exists(nouveauNomFichier) Then
                ' Écrire un log de niveau INFO
                Logger.INFO($"Un fichier avec le nom '{nouveauNomFichier}' existe déjà dans le répertoire de sortie. Renommage et déplacement annulés.")
                Return cheminFichier ' Retourne le nom d'origine si le renommage/déplacement est annulé
            End If

            ' Renommer et déplacer le fichier
            File.Move(cheminFichier, nouveauNomFichier)
            Logger.INFO($"Fichier {cheminFichier} renommé et déplacé avec succès vers '{nouveauNomFichier}'.")

            ' Retourner le nouveau chemin complet du fichier
            Return nouveauNomFichier

        Catch ex As Exception
            ' Écrire un log d'erreur en cas d'exception
            Logger.ERR($"Erreur lors du renommage et déplacement du fichier : {ex.Message}")
            Return cheminFichier ' Retourne le nom d'origine en cas d'erreur
        End Try
    End Function
    Private Function construitRepSortie(sTypeDoc) As String
        Dim sRepSortie As String = LectureProprietes.GetVariable("repFichiersDocuments") & "\" & LectureProprietes.GetVariable("repFichiersDocumentsATrier")

        'Todo : à écrire
    End Function
    Public Shared Function RemplacerAnnees(gabarit As String) As String
        ' Obtenir l'année en cours
        Dim anneeEnCours As Integer = DateTime.Now.Year
        Dim anneeSuivante As Integer = anneeEnCours + 1

        ' Vérifier quel motif est présent et effectuer le remplacement
        If gabarit.Contains("{SSAA}") Then
            Return gabarit.Replace("{SSAA}", anneeEnCours.ToString())
        ElseIf gabarit.Contains("{SCO_SSAA}") Then
            Return gabarit.Replace("{SCO_SSAA}", $"{anneeEnCours}-{anneeSuivante}")
        End If

        ' Retourner la chaîne inchangée si aucun motif n'est trouvé
        Return gabarit
    End Function
    Private Shared Function GetRepSortie(sTypeDoc As Integer) As String
        Dim sRepSortie As String

        Using reader As SqlDataReader =
            SqlCommandBuilder.
            CreateSqlCommand("reqRepSortie",
                             New Dictionary(Of String, Object) From {{"@typeDoc", sTypeDoc}}
                             ).
            ExecuteReader()

            ' Vérifier si le reader contient des lignes
            If reader.HasRows Then
                While reader.Read()
                    sRepSortie = reader.GetString(0)
                End While
                Logger.INFO($"Sous-répertoire trouvé pour le type de document '{sTypeDoc}'.")
            Else
                ' Gérer le cas où le reader est vide
                Logger.WARN($"Aucun sous-répertoire trouvé pour le type de document '{sTypeDoc}'.")
            End If
        End Using

        Return sRepSortie
    End Function
    ' Méthode dédiée pour générer le compte rendu de traitement
    Private Sub GenererCompteRendu()
        ' Log des informations générales
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
