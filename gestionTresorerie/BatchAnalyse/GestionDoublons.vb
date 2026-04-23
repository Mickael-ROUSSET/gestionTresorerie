Imports System.IO
Imports System.Linq
Imports System.Threading

''' <summary>
''' Classe responsable de l'identification et du déplacement des fichiers en double.
''' </summary>
Public NotInheritable Class GestionDoublons

    ' --- Constantes de configuration ---
    Private Const NomDossierDoublons As String = "Doublons"

    Public Shared Async Function ExecuterTraitementAsync(
        repertoire As String,
        Optional progress As IProgress(Of Integer) = Nothing,
        Optional status As IProgress(Of String) = Nothing,
        Optional token As CancellationToken = Nothing) As Task(Of List(Of String))

        ' Note : On utilise Task.Run pour ne pas bloquer l'UI pendant le scan
        Return Await Task.Run(Function() DeplacerDoublons(repertoire, progress, status, token))
    End Function

    Private Shared Function DeplacerDoublons(
        repertoire As String,
        progress As IProgress(Of Integer),
        status As IProgress(Of String),
        token As CancellationToken) As List(Of String)

        Dim fichiersDeplaces As New List(Of String)

        Try
            ' 1. Initialisation
            If Not Directory.Exists(repertoire) Then Return fichiersDeplaces
            Dim dossierCible = InitialiserDossierDoublons(repertoire)
            Dim fichiers = RecupererListeFichiers(repertoire, dossierCible)

            ' 2. Analyse (Hashage)
            Dim fichiersParHash = AnalyserFichiers(fichiers, progress, status, token)

            ' 3. Traitement (Déplacement)
            status?.Report("Déplacement des doublons détectés...")
            fichiersDeplaces = TraiterDoublons(fichiersParHash, dossierCible, token)

            status?.Report($"✅ Terminé : {fichiersDeplaces.Count} doublon(s) déplacé(s).")
            Return fichiersDeplaces

        Catch ex As Exception
            Logger.ERR($"Erreur globale : {ex.Message}")
            Return fichiersDeplaces
        End Try
    End Function

    ' --- Sous-fonctions spécialisées ---

    Private Shared Function InitialiserDossierDoublons(repertoire As String) As String
        Dim chemin = Path.Combine(repertoire, NomDossierDoublons)
        If Not Directory.Exists(chemin) Then Directory.CreateDirectory(chemin)
        Return chemin
    End Function

    Private Shared Function RecupererListeFichiers(repertoire As String, dossierExclure As String) As List(Of String)
        Return Directory.EnumerateFiles(repertoire, "*.*", SearchOption.AllDirectories).
               Where(Function(f) Not Path.GetDirectoryName(f).Equals(dossierExclure, StringComparison.OrdinalIgnoreCase)).
               ToList()
    End Function

    Private Shared Function AnalyserFichiers(fichiers As List(Of String), p As IProgress(Of Integer), s As IProgress(Of String), t As CancellationToken) As Dictionary(Of String, List(Of String))
        Dim map As New Dictionary(Of String, List(Of String))(StringComparer.OrdinalIgnoreCase)
        Dim total = fichiers.Count

        For i = 0 To total - 1
            If t.IsCancellationRequested Then
                Logger.INFO("Annulation demandée par l'utilisateur.")
                Exit For ' On sort proprement de la boucle For
            End If

            Dim f = fichiers(i)
            s?.Report($"Analyse {i + 1}/{total} : {Path.GetFileName(f)}")

            Dim h = ObtenirHashFichier(f)
            If Not String.IsNullOrEmpty(h) Then
                If Not map.ContainsKey(h) Then map(h) = New List(Of String)
                map(h).Add(f)
            End If

            p?.Report(CInt(((i + 1) / total) * 100))
        Next
        Return map
    End Function

    Private Shared Function ObtenirHashFichier(chemin As String) As String
        Try
            ' On tente le recadrage, sinon hash standard
            Dim recadree = RecadrerImageUtile(chemin)
            Return If(recadree IsNot Nothing,
                      UtilitairesHash.CalculerHashPerceptuel(recadree),
                      UtilitairesHash.CalculerHashPerceptuel(chemin))
        Catch
            Return String.Empty
        End Try
    End Function

    Private Shared Function TraiterDoublons(map As Dictionary(Of String, List(Of String)), dossier As String, t As CancellationToken) As List(Of String)
        Dim deplaces As New List(Of String)
        For Each kvp In map.Where(Function(x) x.Value.Count > 1)
            If t.IsCancellationRequested Then Exit For

            ' Garder le plus ancien, déplacer les autres
            Dim tries = kvp.Value.OrderBy(Function(f) File.GetCreationTime(f)).ToList()
            For i = 1 To tries.Count - 1
                Dim dest = GenererNomUnique(tries(i), dossier)
                File.Move(tries(i), dest)
                deplaces.Add(dest)
            Next
        Next
        Return deplaces
    End Function

    Private Shared Function GenererNomUnique(source As String, dossier As String) As String
        Dim nom = Path.GetFileNameWithoutExtension(source)
        Dim ext = Path.GetExtension(source)
        Dim dest = Path.Combine(dossier, nom & ext)
        Dim cpt = 1
        While File.Exists(dest)
            dest = Path.Combine(dossier, $"{nom}_{cpt}{ext}")
            cpt += 1
        End While
        Return dest
    End Function
End Class