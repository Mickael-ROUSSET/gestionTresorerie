'Public Class cléApiMistral
'    ReadOnly _apiKey As String = "uswXFN4VPYfYmrTmNbLY16D3fxFunwjm"

Imports System.IO
Imports System.Reflection
Imports Newtonsoft.Json.Linq

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
        'Dim nouveauDoc As New DocumentAgumaaa
        Try

            Dim compteur As (fichiersTraites As Integer, traitementOK As Integer, traitementKO As Integer) = Nothing            ' Initialiser les compteurs pour le répertoire courant
            If Not compteursParRepertoire.TryGetValue(repertoire, compteur) Then
                compteur = (0, 0, 0)
                compteursParRepertoire(repertoire) = compteur
            End If

            ' Obtenir tous les fichiers jpg dans le répertoire courant
            Dim extensions As String() = {"*.jpg", "*.jpeg", "*.png", "*.bmp", "*.gif"}
            Dim fichiers As New List(Of String)

            For Each extension In extensions
                fichiers.AddRange(Directory.GetFiles(repertoire, extension, SearchOption.AllDirectories))
            Next


            ' Parcourir chaque fichier et appeler analyseChq
            For Each sNomFichier As String In fichiers
                'sNomFichier contient le nom du fichier et le chemin
                Try
                    'Il faut une instance de classe du bon type
                    _TypeDoc.ContenuBase64 = TypeDocImpl.EncodeImageToBase64(sNomFichier)
                    '-----------------------------------
                    ' Appel Mistral dans analyseDocument
                    '-----------------------------------
                    Dim nouveauDoc As DocumentAgumaaa = analyseDocument(_TypeDoc)
                    ProcessDocument(nouveauDoc, sNomFichier)
                    Logger.INFO("Insertion en base du document " & nouveauDoc.ToString)
                    nombreFichiersTraites += 1
                    nombreTraitementOK += 1
                    compteursParRepertoire(repertoire) = (compteur.fichiersTraites + 1, compteur.traitementOK + 1, compteur.traitementKO)
                Catch ex As Exception
                    Logger.ERR($"Erreur lors de l'analyse du fichier : {sNomFichier} - {ex.Message}")
                    nombreTraitementKO += 1
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
    Public Sub ProcessDocument(nouveauDoc As DocumentAgumaaa, sNomFichier As String)
        'sNomFichier contient le chemin
        Try
            ' Analyser le document pour obtenir une instance de DocumentAgumaaa 
            If nouveauDoc Is Nothing Then
                Logger.ERR("analyseDocument a retourné un document null.")
                Return
            End If

            ' Vérifier si ClasseTypeDoc est valide
            If String.IsNullOrEmpty(_TypeDoc.ClasseTypeDoc) Then
                Logger.ERR($"ClasseTypeDoc est vide ou null : {_TypeDoc.ClasseTypeDoc}")
                Return
            End If

            ' Obtenir le type de la classe dérivée
            Dim typeClasse As Type = Type.GetType("gestionTresorerie." & _TypeDoc.ClasseTypeDoc)
            If typeClasse Is Nothing OrElse Not GetType(DocumentAgumaaa).IsAssignableFrom(typeClasse) Then
                Logger.ERR($"Type non trouvé ou non dérivé de DocumentAgumaaa : {"gestionTresorerie." & _TypeDoc.ClasseTypeDoc}")
                Return
            End If

            ' Instancier la classe dérivée
            Dim derivedDoc As DocumentAgumaaa = TryCast(Activator.CreateInstance(typeClasse), DocumentAgumaaa)
            If derivedDoc Is Nothing Then
                Logger.ERR($"Impossible d'instancier la classe {"gestionTresorerie." & _TypeDoc.ClasseTypeDoc}")
                Return
            End If

            ' Copier les propriétés de nouveauDoc vers derivedDoc
            derivedDoc.DateDoc = nouveauDoc.DateDoc
            'derivedDoc.ContenuDoc = nouveauDoc.ContenuDoc
            derivedDoc.CategorieDoc = nouveauDoc.CategorieDoc
            derivedDoc.SousCategorieDoc = nouveauDoc.SousCategorieDoc
            derivedDoc.IdMvtDoc = nouveauDoc.IdMvtDoc
            derivedDoc.metaDonnees = nouveauDoc.metaDonnees

            ' Appeler renommerFichier et récupérer le nouveau chemin après renommage 
            derivedDoc.CheminDoc = derivedDoc.RenommerFichier(sNomFichier)

            ' Insérer le document (version non statique)
            derivedDoc.InsererDocument(derivedDoc)

        Catch ex As Exception
            Logger.ERR($"Erreur lors du traitement du document {sNomFichier} : {ex.Message}")
        End Try
    End Sub

    Public Shared Function analyseDocument(doc As ITypeDoc) As DocumentAgumaaa
        Try
            ' Vérifier si l'objet doc est null
            If doc Is Nothing Then
                Logger.ERR("L'objet ITypeDoc passé à analyseDocument est null.")
                Return Nothing
            End If

            Logger.INFO("-------------------" & vbCrLf & "Analyse du document " & doc.ToString)

            ' Vérifier si ClasseTypeDoc est valide
            If String.IsNullOrEmpty(doc.ClasseTypeDoc) Then
                Logger.ERR("ClasseTypeDoc est vide ou null.")
                Return Nothing
            End If

            ' Obtenir le type de la classe dérivée
            Dim typeClasse As Type = Type.GetType("gestionTresorerie." & doc.ClasseTypeDoc, False, True)
            If typeClasse Is Nothing Then
                ' Tentative alternative : chercher dans l'assembly courant
                typeClasse = Assembly.GetExecutingAssembly().GetType(doc.ClasseTypeDoc, False, True)
                If typeClasse Is Nothing Then
                    Logger.ERR($"Type non trouvé : {doc.ClasseTypeDoc}")
                    Return Nothing
                End If
            End If

            ' Vérifier si le type dérive de DocumentAgumaaa
            If Not GetType(DocumentAgumaaa).IsAssignableFrom(typeClasse) Then
                Logger.ERR($"Le type {doc.ClasseTypeDoc} ne dérive pas de DocumentAgumaaa.")
                Return Nothing
            End If

            ' Instancier la classe dérivée
            Dim nouveauDoc As DocumentAgumaaa = TryCast(Activator.CreateInstance(typeClasse), DocumentAgumaaa)
            If nouveauDoc Is Nothing Then
                Logger.ERR($"Impossible d'instancier la classe {doc.ClasseTypeDoc}")
                Return Nothing
            End If

            ' Vérifier si ContenuBase64 est présent
            If String.IsNullOrEmpty(doc.ContenuBase64) Then
                Logger.ERR("ContenuBase64 est vide ou null pour le document.")
                Return Nothing
            End If

            ' Appeler litImage pour analyser l'image et obtenir les métadonnées
            Dim sMetaDonnees As String = AppelMistral.litImage(doc)
            If String.IsNullOrEmpty(sMetaDonnees) Then
                Logger.ERR("AppelMistral.litImage a retourné des métadonnées vides ou null.")
                Return Nothing
            End If

            ' Valider que sMetaDonnees est un JSON valide
            Try
                JObject.Parse(sMetaDonnees)
            Catch ex As Exception
                Logger.ERR($"Les métadonnées retournées par AppelMistral.litImage ne sont pas un JSON valide : {ex.Message}")
                Return Nothing
            End Try

            ' Initialiser les propriétés de DocumentAgumaaa
            'nouveauDoc.ContenuDoc = doc.ContenuBase64
            nouveauDoc.DateDoc = Date.Now
            nouveauDoc.CategorieDoc = doc.ClasseTypeDoc
            nouveauDoc.SousCategorieDoc = "" ' Peut être ajusté si nécessaire
            'Nettoie le json renvoyé par Mistral
            nouveauDoc.metaDonnees = Utilitaires.ExtraireJsonValide(sMetaDonnees)
            nouveauDoc.IdMvtDoc = 0 ' Valeur par défaut, à ajuster si nécessaire

            Logger.INFO($"Document {nouveauDoc.CategorieDoc} analysé avec succès. metaDonnees : {sMetaDonnees}")
            Return nouveauDoc
        Catch ex As Exception
            Logger.ERR($"Erreur lors de l'analyse du document : {ex.Message}")
            Return Nothing
        End Try
    End Function

    ' Méthode dédiée pour générer le compte rendu de traitement
    'Private Sub GenererCompteRendu()
    '    ' Log des informations générales
    '    Logger.INFO($"Compte rendu de traitement :")
    '    Logger.INFO($"Nombre de fichiers traités : {nombreFichiersTraites}")
    '    Logger.INFO($"Nombre de traitements OK : {nombreTraitementOK}")
    '    Logger.INFO($"Nombre de traitements KO : {nombreTraitementKO}")
    '    Logger.INFO($"Date/Heure de début : {dateHeureDebut}")
    '    Logger.INFO($"Date/Heure de fin : {dateHeureFin}")

    '    ' Log des compteurs par répertoire
    '    Logger.INFO($"Compte rendu par répertoire :")
    '    For Each kvp As KeyValuePair(Of String, (Integer, Integer, Integer)) In compteursParRepertoire
    '        Logger.INFO($"Répertoire : {kvp.Key}")
    '        Logger.INFO($"  Fichiers traités : {kvp.Value.Item1}")
    '        Logger.INFO($"  Traitements OK : {kvp.Value.Item2}")
    '        Logger.INFO($"  Traitements KO : {kvp.Value.Item3}")
    '    Next
    'End Sub
    Private Sub GenererCompteRendu()
        ' Assurer que le rapport est démarré
        RapportTraitement.DemarrerRapport()

        ' Mettre à jour les compteurs globaux dans RapportTraitement
        For Each kvp As KeyValuePair(Of String, (Integer, Integer, Integer)) In compteursParRepertoire
            ' Mapper le répertoire à un TypeDocument (ajustez selon votre logique)
            Dim typeDoc As RapportTraitement.TypeDocument
            Select Case kvp.Key.ToLower()
                Case "cheques"
                    typeDoc = RapportTraitement.TypeDocument.Cheque
                Case "formulairesadhesion"
                    typeDoc = RapportTraitement.TypeDocument.FormulaireAdhesion
                Case "questionnairesmedicaux"
                    typeDoc = RapportTraitement.TypeDocument.QuestionnaireMedical
                Case Else
                    RapportTraitement.WriteToLog($"Répertoire inconnu : {kvp.Key}", "WARN")
                    Continue For
            End Select

            ' Mettre à jour les compteurs pour ce type de document
            For i As Integer = 1 To kvp.Value.Item2 ' Traitements OK
                RapportTraitement.MettreAJour(typeDoc, True, $"Traitement OK dans {kvp.Key}")
            Next
            For i As Integer = 1 To kvp.Value.Item3 ' Traitements KO
                RapportTraitement.MettreAJour(typeDoc, False, $"Traitement KO dans {kvp.Key}")
            Next
            ' Les avertissements ne sont pas dans le tuple, mais peuvent être ajoutés si nécessaire
        Next

        ' Écrire les informations générales dans le log
        RapportTraitement.WriteToLog("Compte rendu de traitement :", "INFO")
        RapportTraitement.WriteToLog($"Nombre de fichiers traités : {nombreFichiersTraites}", "INFO")
        RapportTraitement.WriteToLog($"Nombre de traitements OK : {nombreTraitementOK}", "INFO")
        RapportTraitement.WriteToLog($"Nombre de traitements KO : {nombreTraitementKO}", "INFO")
        RapportTraitement.WriteToLog($"Date/Heure de début : {dateHeureDebut}", "INFO")
        RapportTraitement.WriteToLog($"Date/Heure de fin : {dateHeureFin}", "INFO")

        ' Générer et écrire les rapports (texte et JSON)
        Dim rapportTexte As String = RapportTraitement.GenererRapport()
        RapportTraitement.WriteToLog("Rapport texte généré :", "INFO")
        RapportTraitement.WriteToLog(rapportTexte, "INFO")

        Dim rapportJson As String = RapportTraitement.GenererRapportJson()
        RapportTraitement.WriteToLog("Rapport JSON généré :", "INFO")
        RapportTraitement.WriteToLog(rapportJson, "INFO")
    End Sub
End Class
