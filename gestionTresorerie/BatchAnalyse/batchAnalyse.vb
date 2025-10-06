'Public Class cléApiMistral
'    ReadOnly _apiKey As String = "uswXFN4VPYfYmrTmNbLY16D3fxFunwjm"

Imports System.IO

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
        Dim sRepDoc As String = LectureProprietes.GetVariable("repRacineAgumaaa") &
            LectureProprietes.GetVariable("repRacineDocuments") &
            LectureProprietes.GetVariable("repFichiersDocumentsATrier")

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
        Dim nouveauDoc As New DocumentAgumaaa
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
                    'Il faut une instance de classe du bon type
                    _TypeDoc.ContenuBase64 = TypeDocImpl.EncodeImageToBase64(cheminFichier)
                    nouveauDoc = analyseDocument(_TypeDoc)
                    '_TypeDoc.renommerFichier(cheminFichier)
                    'nouveauDoc.InsererDocument(nouveauDoc)
                    ProcessDocument(_TypeDoc, cheminFichier)
                    Logger.INFO("Insertion en base du document " & nouveauDoc.ToString)
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
    Public Sub ProcessDocument(_TypeDoc As ITypeDoc, cheminFichier As String)
        Try
            ' Analyser le document pour obtenir une instance de DocumentAgumaaa
            Dim nouveauDoc As DocumentAgumaaa = analyseDocument(_TypeDoc)
            If nouveauDoc Is Nothing Then
                Logger.ERR("analyseDocument a retourné un document null.")
                Return
            End If

            ' Vérifier si ClasseTypeDoc est valide
            If String.IsNullOrEmpty(_TypeDoc.ClasseTypeDoc) Then
                Logger.ERR("ClasseTypeDoc est vide ou null.")
                Return
            End If

            ' Obtenir le type de la classe dérivée
            Dim typeClasse As Type = Type.GetType(_TypeDoc.ClasseTypeDoc)
            If typeClasse Is Nothing OrElse Not GetType(DocumentAgumaaa).IsAssignableFrom(typeClasse) Then
                Logger.ERR($"Type non trouvé ou non dérivé de DocumentAgumaaa : {_TypeDoc.ClasseTypeDoc}")
                Return
            End If

            ' Instancier la classe dérivée
            Dim derivedDoc As DocumentAgumaaa = TryCast(Activator.CreateInstance(typeClasse), DocumentAgumaaa)
            If derivedDoc Is Nothing Then
                Logger.ERR($"Impossible d'instancier la classe {_TypeDoc.ClasseTypeDoc}")
                Return
            End If

            ' Copier les propriétés de nouveauDoc vers derivedDoc
            derivedDoc.DateDoc = nouveauDoc.DateDoc
            derivedDoc.ContenuDoc = nouveauDoc.ContenuDoc
            derivedDoc.CheminDoc = nouveauDoc.CheminDoc
            derivedDoc.CategorieDoc = nouveauDoc.CategorieDoc
            derivedDoc.SousCategorieDoc = nouveauDoc.SousCategorieDoc
            derivedDoc.IdMvtDoc = nouveauDoc.IdMvtDoc
            derivedDoc.metaDonnees = nouveauDoc.metaDonnees

            ' Appeler renommerFichier sur _TypeDoc
            derivedDoc.RenommerFichier(cheminFichier)

            ' Insérer le document (version non statique)
            derivedDoc.InsererDocument(derivedDoc)

        Catch ex As Exception
            Logger.ERR($"Erreur lors du traitement du document : {ex.Message}")
        End Try
    End Sub
    Public Function analyseDocument(doc As ITypeDoc) As DocumentAgumaaa
        Dim nouveauDoc As New DocumentAgumaaa

        Logger.INFO("-------------------" & vbCrLf & "Analyse du document " & doc.ToString)
        Dim sMetaDonnees = AppelMistral.litImage(doc)
        nouveauDoc.ContenuDoc = doc.ContenuBase64
        nouveauDoc.DateDoc = Date.Now
        nouveauDoc.CategorieDoc = doc.ClasseTypeDoc
        nouveauDoc.SousCategorieDoc = ""
        nouveauDoc.metaDonnees = sMetaDonnees
        Return nouveauDoc
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
