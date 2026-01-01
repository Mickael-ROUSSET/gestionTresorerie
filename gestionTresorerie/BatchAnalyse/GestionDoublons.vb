Imports System.IO
Imports System.Linq
Imports System.Threading

Public Class GestionDoublons
    Public Shared Function DeplacerDoublonsAvecProgress(
    repertoire As String,
    Optional progress As IProgress(Of Integer) = Nothing,
    Optional statusProgress As IProgress(Of String) = Nothing,
    Optional token As CancellationToken = Nothing
) As List(Of String)

        Dim fichiersDeplaces As New List(Of String)

        Try
            ' Vérifie si le répertoire existe
            If Not Directory.Exists(repertoire) Then
                Logger.INFO($"Le répertoire n'existe pas : {repertoire}")
                Return fichiersDeplaces
            End If

            Logger.INFO($"🔍 Début de l'analyse des fichiers dans '{repertoire}'...")

            ' --- Crée le dossier Doublons ---
            Dim dossierDoublons = Path.Combine(repertoire, "Doublons")
            If Not Directory.Exists(dossierDoublons) Then Directory.CreateDirectory(dossierDoublons)

            ' --- Récupère tous les fichiers en parcourant aussi les sous-répertoires ---
            Dim fichiers = Directory.EnumerateFiles(repertoire, "*.*", SearchOption.AllDirectories).
            Where(Function(f) Not Path.GetDirectoryName(f).Equals(dossierDoublons, StringComparison.OrdinalIgnoreCase)).
            ToList()

            Dim total As Integer = fichiers.Count
            Dim compteur As Integer = 0

            ' Communique la taille max à la barre de progression
            progress?.Report(0)
            statusProgress?.Report($"Analyse de {total} fichier(s)...")

            ' --- Dictionnaire de hash → liste de fichiers ---
            Dim fichiersParHash As New Dictionary(Of String, List(Of String))(StringComparer.OrdinalIgnoreCase)

            ' --- Parcours des fichiers ---
            For Each fichier In fichiers
                If token.IsCancellationRequested Then
                    Logger.INFO("⛔ Annulation demandée.")
                    Exit For
                End If

                compteur += 1
                statusProgress?.Report($"Analyse du fichier {compteur}/{total} : {Path.GetFileName(fichier)}")
                Dim hash As String = String.Empty
                Try
                    Dim recadree = RecadrerImageUtile(fichier)
                    If recadree IsNot Nothing Then
                        hash = UtilitairesHash.CalculerHashPerceptuel(recadree)
                    Else
                        hash = UtilitairesHash.CalculerHashPerceptuel(fichier)
                    End If
                    If String.IsNullOrEmpty(hash) Then Continue For

                    Dim value As List(Of String) = Nothing
                    If Not fichiersParHash.TryGetValue(hash, value) Then
                        value = New List(Of String)
                        fichiersParHash(hash) = value
                    End If

                    value.Add(fichier)
                    Logger.DBG($"hash {hash} pour {fichier}")

                Catch ex As Exception
                    Logger.INFO($"⚠️ Erreur sur {fichier} : {ex.Message}")
                End Try

                ' Mise à jour visuelle
                progress?.Report(CInt((compteur / total) * 100))
            Next

            ' --- Étape 2 : Déplacement des doublons ---
            statusProgress?.Report("Déplacement des doublons détectés...")

            For Each kvp In fichiersParHash
                If token.IsCancellationRequested Then Exit For

                Dim liste = kvp.Value
                If liste.Count > 1 Then
                    Logger.INFO($"Doublons détectés : {String.Join(", ", liste)}")

                    ' Trie par date de création (le plus ancien en dernier)
                    Dim fichiersTries = liste.OrderBy(Function(f) File.GetCreationTime(f)).ToList()

                    ' On garde le premier (le plus ancien) et déplace les autres
                    For i = 1 To fichiersTries.Count - 1
                        Dim source = fichiersTries(i)
                        Dim destination = Path.Combine(dossierDoublons, Path.GetFileName(source))

                        ' Gère les conflits de nom
                        Dim cpt = 1
                        While File.Exists(destination)
                            Dim nomSansExt = Path.GetFileNameWithoutExtension(source)
                            Dim ext = Path.GetExtension(source)
                            destination = Path.Combine(dossierDoublons, $"{nomSansExt}_{cpt}{ext}")
                            cpt += 1
                        End While

                        File.Move(source, destination)
                        fichiersDeplaces.Add(destination)
                        Logger.INFO($"📦 Déplacé : {source} → {destination}")
                    Next
                End If
            Next

            statusProgress?.Report($"✅ Traitement terminé : {fichiersDeplaces.Count} doublon(s) déplacé(s).")
            Logger.INFO("✅ Traitement des doublons terminé.")
            Return fichiersDeplaces

        Catch ex As Exception
            Logger.ERR($"Erreur lors du traitement des doublons : {ex.Message}")
            Return fichiersDeplaces
        End Try

    End Function

End Class
