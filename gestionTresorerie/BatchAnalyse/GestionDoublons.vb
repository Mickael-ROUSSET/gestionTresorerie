Imports System.IO
Imports System.Linq

Public Class GestionDoublons

    Public Shared Function DeplacerDoublons(repertoire As String) As List(Of String)
        Dim fichiersDeplaces As New List(Of String)

        Try
            ' --- Vérifie si le répertoire existe ---
            If Not Directory.Exists(repertoire) Then
                Logger.INFO($"Le répertoire n'existe pas : {repertoire}")
                Return fichiersDeplaces
            End If

            Logger.INFO($"🔍 Début de l'analyse des fichiers dans '{repertoire}'...")

            ' --- Crée le dossier Doublons ---
            Dim dossierDoublons = Path.Combine(repertoire, "Doublons")
            If Not Directory.Exists(dossierDoublons) Then Directory.CreateDirectory(dossierDoublons)

            ' --- Dictionnaire : hash → liste de fichiers ---
            Dim fichiersParHash As New Dictionary(Of String, List(Of String))(StringComparer.OrdinalIgnoreCase)

            ' --- Parcours récursif des fichiers ---
            For Each fichier In Directory.EnumerateFiles(repertoire, "*.*", SearchOption.AllDirectories)
                Try
                    ' Ignore le sous-dossier Doublons
                    If Path.GetDirectoryName(fichier).StartsWith(dossierDoublons, StringComparison.OrdinalIgnoreCase) Then
                        Continue For
                    End If

                    Dim extension = Path.GetExtension(fichier).ToLowerInvariant()
                    Dim hash As String = ""

                    ' --- Hash perceptuel pour les images ---
                    If {".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tif", ".tiff", ".webp"}.Contains(extension) Then
                        hash = UtilitairesHash.CalculerHashPerceptuel(fichier)
                    Else
                        ' --- Hash binaire ou intelligent pour les autres fichiers ---
                        hash = UtilitairesHash.CalculerHashIntelligent(fichier)
                    End If

                    If String.IsNullOrEmpty(hash) Then Continue For

                    If Not fichiersParHash.ContainsKey(hash) Then
                        fichiersParHash(hash) = New List(Of String)
                    End If
                    fichiersParHash(hash).Add(fichier)

                    Logger.DBG($"📄 {Path.GetFileName(fichier)} → Hash : {hash}")

                Catch ex As Exception
                    Logger.WARN($"⚠️ Erreur sur {fichier} : {ex.Message}")
                End Try
            Next

            ' --- Déplacement des doublons ---
            For Each kvp In fichiersParHash
                Dim fichiers = kvp.Value
                If fichiers.Count > 1 Then
                    Logger.INFO($"🟡 Doublons détectés ({fichiers.Count}) : {String.Join(", ", fichiers.Select(Function(f) Path.GetFileName(f)))}")

                    ' Trie les fichiers du plus ancien au plus récent
                    Dim fichiersTries = fichiers.OrderBy(Function(f) File.GetCreationTime(f)).ToList()

                    ' Garde le plus ancien, déplace les autres
                    For i = 1 To fichiersTries.Count - 1
                        Dim source = fichiersTries(i)
                        Dim destination = Path.Combine(dossierDoublons, Path.GetFileName(source))

                        ' Gère les conflits de nom
                        Dim compteur As Integer = 1
                        While File.Exists(destination)
                            Dim nomSansExt = Path.GetFileNameWithoutExtension(source)
                            Dim ext = Path.GetExtension(source)
                            destination = Path.Combine(dossierDoublons, $"{nomSansExt}_{compteur}{ext}")
                            compteur += 1
                        End While

                        File.Move(source, destination)
                        fichiersDeplaces.Add(destination)
                        Logger.INFO($"➡️ Déplacé : {source} → {destination}")
                    Next
                End If
            Next

            Logger.INFO("✅ Traitement des doublons terminé.")
            Return fichiersDeplaces

        Catch ex As Exception
            Logger.ERR($"❌ Erreur lors du traitement des doublons : {ex.Message}")
            Return fichiersDeplaces
        End Try
    End Function

End Class
