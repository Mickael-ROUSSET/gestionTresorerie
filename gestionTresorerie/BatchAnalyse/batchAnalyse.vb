'Public Class cléApiMistral
'    ReadOnly _apiKey As String = "uswXFN4VPYfYmrTmNbLY16D3fxFunwjm"

Imports System.IO
Imports System.Reflection
Imports gestionTresorerie.Agumaaa
Imports Newtonsoft.Json
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
    ' <summary>
    ' Module d'exécution : Gère la boucle de traitement et les logs
    ' </summary> 
    Public Async Function DemarrerTraitement(repertoireSource As String) As Task
        Try
            Logger.INFO("Démarrage du lot d'analyse...")
            If Not Directory.Exists(repertoireSource) Then Exit Function

            ' 1. Récupération de tous les fichiers (Filtre multi-extensions)
            Dim extensions As String() = {"*.jpg", "*.jpeg", "*.pdf"}
            Dim tousLesFichiers = extensions.SelectMany(Function(ext) Directory.GetFiles(repertoireSource, ext, SearchOption.AllDirectories))

            Logger.INFO($"Nb fichiers à traiter : {tousLesFichiers.Count}")
            For Each fichier In tousLesFichiers
                Await TraiterFichierUnique(fichier).ConfigureAwait(False)
                Logger.INFO($"Traitement de : {fichier}")
            Next

            GenererCompteRendu()
            MsgBox("Terminé")
        Catch ex As Exception
            Logger.ERR($"Erreur fatale boucle principale : {ex.Message}")
        End Try
    End Function
    Private Async Function TraiterFichierUnique(cheminFichier As String) As Task
        Try
            ' 2. Appel Gemini (Identification + Extraction)
            Dim resultatJson As String = Await GeminiAnalyzer.AnalyserDocument(GlobalSettings.GeminiKey, cheminFichier).ConfigureAwait(False)

            If String.IsNullOrEmpty(resultatJson) Then
                Logger.WARN($"Aucune réponse pour {Path.GetFileName(cheminFichier)}")
                EnregistrerEchec(cheminFichier)
                Exit Function
            End If

            ' 3. Désérialisation pour connaître le type détecté par l'IA
            Dim extraction = JsonConvert.DeserializeObject(Of JObject)(resultatJson)
            Dim typeDetecte As String = extraction("type_document")?.ToString()

            ' 4. Instanciation dynamique de la classe correspondante
            Dim typeClasse As Type = Type.GetType("gestionTresorerie." & typeDetecte)
            If typeClasse IsNot Nothing Then
                Dim docMetier As DocumentAgumaaa = TryCast(Activator.CreateInstance(typeClasse), DocumentAgumaaa)

                ' Remplissage des données
                docMetier.metaDonnees = resultatJson
                docMetier.DateDoc = DateTime.Now
                docMetier.CheminDoc = docMetier.RenommerFichier(cheminFichier)

                ' Insertion SQL
                docMetier.InsererDocument(docMetier)

                nombreTraitementOK += 1
                Logger.INFO($"Succès : {typeDetecte} identifié pour {Path.GetFileName(cheminFichier)}")
            End If

        Catch ex As Exception
            EnregistrerEchec(cheminFichier, ex.Message)
        End Try
    End Function
    Private Sub EnregistrerEchec(fichier As String, Optional msg As String = "")
        nombreTraitementKO += 1
        Logger.ERR($"Echec traitement {fichier} : {msg}")
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
                Logger.ERR($"TypeMouvement non trouvé ou non dérivé de DocumentAgumaaa : {"gestionTresorerie." & _TypeDoc.ClasseTypeDoc}")
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
    Public Shared Async Function analyseDocument(doc As ITypeDoc) As Task(Of DocumentAgumaaa)
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
                    Logger.ERR($"TypeMouvement non trouvé : {doc.ClasseTypeDoc}")
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
            Dim resultatJson As String = Await GeminiAnalyzer.AnalyserDocument(GlobalSettings.GeminiKey, doc.ContenuBase64)
            If String.IsNullOrEmpty(resultatJson) Then
                Logger.ERR("AppelMistral.litImage a retourné des métadonnées vides ou null.")
                Return Nothing
            End If

            ' Initialiser les propriétés de DocumentAgumaaa 
            nouveauDoc.DateDoc = Date.Now
            nouveauDoc.CategorieDoc = doc.ClasseTypeDoc
            nouveauDoc.SousCategorieDoc = "" ' Peut être ajusté si nécessaire
            'Nettoie le json renvoyé par Mistral
            nouveauDoc.metaDonnees = resultatJson
            nouveauDoc.IdMvtDoc = 0 ' Valeur par défaut, à ajuster si nécessaire

            Logger.INFO($"Document {nouveauDoc.CategorieDoc} analysé avec succès. metaDonnees : {resultatJson}")
            Return nouveauDoc
        Catch ex As Exception
            Logger.ERR($"Erreur lors de l'analyse du document : {ex.Message}")
            Throw ' <-- On "re-propage" l'erreur pour que l'appelant sache que ça a échoué
        End Try
    End Function
    Private Sub GenererCompteRendu()
        ' Assurer que le rapport est démarré
        RapportTraitement.DemarrerRapport()

        ' Mettre à jour les compteurs globaux dans RapportTraitement
        For Each kvp As KeyValuePair(Of String, (Integer, Integer, Integer)) In compteursParRepertoire
            ' Mapper le répertoire à un TypeDocument (ajustez selon votre logique)
            Dim typeDoc As RapportTraitement.TypeDocument

            Select Case kvp.Key.ToLower()
                Case TypeDocument.Cheque
                    typeDoc = RapportTraitement.TypeDocument.Cheque
                Case TypeDocument.FormulaireAdhesion
                    typeDoc = RapportTraitement.TypeDocument.FormulaireAdhesion
                Case TypeDocument.QuestionnaireMedical
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
