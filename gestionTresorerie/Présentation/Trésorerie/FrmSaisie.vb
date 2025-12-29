Imports System.Data.SqlClient
Imports System.Globalization
Imports System.Text.RegularExpressions
Imports gestionTresorerie.Agumaaa

Public Class FrmSaisie
    Inherits System.Windows.Forms.Form

    Private listeTiers As ListeTiers
    Private _mvtEnCours As Mouvements ' Renommé pour plus de clarté
    Private _dtMvtsIdentiques As DataTable = Nothing
    Private _tiersSelectionne As Tiers
    Private _categorieSelectionne As Categorie
    Private _sousCategorieSelectionnee As SousCategorie
    Private _typeDocSelectionne As TypeDocImpl
    Private _evenementSelectionne As Evenement ' Renommé pour la cohérence
    Private _typeMvtSelectionne As TypeMvt ' Renommé pour la cohérence

    Public Property Properties As Object
    Private isExpanded As Boolean = True ' Non utilisé dans les méthodes fournies, mais conservé
    Private _idDocSelectionne As Integer = 0

    Private Sub FrmSaisie_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            InitialiserListeTiers()
            Dim indTiersDetecte As Integer = listeTiers.DetecteTiers(txtNote.Text)
        Catch ex As Exception
            Logger.ERR($"Erreur lors du chargement du formulaire : {ex.Message}")
            ' Utilisation de Exit Sub au lieu de End pour un comportement plus propre dans WinForms
            MessageBox.Show($"Une erreur critique est survenue : {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Me.Close()
        End Try
    End Sub
    Private Sub InitialiserListeTiers()
        If listeTiers Is Nothing Then
            listeTiers = New ListeTiers()
        End If
    End Sub
    Private Sub BtnValider_Click(sender As Object, e As EventArgs) Handles btnValider.Click
        If VerifierValiditeMouvement() Then
            InsereMouvement()
        Else
            ' La méthode VerifierValiditeMouvement gère déjà l'affichage de l'erreur
            ' et le focus, donc ce MessageBox est redondant sauf pour une erreur non gérée
            ' Je le laisse pour la rétrocompatibilité, mais l'erreur spécifique est affichée par la fonction
            MessageBox.Show("Le mouvement est invalide. Veuillez vérifier les informations saisies.", "Erreur de validation", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End If
    End Sub

    ' --- Logique de Validation et de Gestion des Erreurs ---

    Private Function VerifierValiditeMouvement() As Boolean
        Dim champsManquants As New List(Of String)
        Dim objetsManquants As New List(Of String)
        Dim premierControleNonValide As Control = Nothing

        ' 1. Vérification des champs de texte obligatoires
        AjouterSiVide(txtMontant, "Montant", champsManquants, premierControleNonValide)
        AjouterSiVide(txtTiers, "Tiers", champsManquants, premierControleNonValide)
        AjouterSiVide(txtCategorie, "Catégorie", champsManquants, premierControleNonValide)
        AjouterSiVide(txtSousCategorie, "Sous-catégorie", champsManquants, premierControleNonValide)
        'AjouterSiVide(txtTypeDoc, "Type de document", champsManquants, premierControleNonValide)
        'AjouterSiVide(txtDocument, "Document", champsManquants, premierControleNonValide)
        AjouterSiVide(txtTypeMvt, "Type de mouvement", champsManquants, premierControleNonValide)

        ' 2. Vérification de la sélection des objets métiers (plus fiable que le texte du contrôle)
        If _tiersSelectionne Is Nothing Then objetsManquants.Add("Tiers (non sélectionné)")
        If _categorieSelectionne Is Nothing Then objetsManquants.Add("Catégorie (non sélectionnée)")
        If _sousCategorieSelectionnee Is Nothing Then objetsManquants.Add("Sous-catégorie (non sélectionnée)")
        'If _typeDocSelectionne Is Nothing Then objetsManquants.Add("Type de document (non sélectionné)")
        If _typeMvtSelectionne Is Nothing Then objetsManquants.Add("Type de mouvement (non sélectionné)")

        ' 3. Affichage des erreurs et gestion du focus
        If champsManquants.Count > 0 OrElse objetsManquants.Count > 0 Then
            Dim msg As String = "Champs obligatoires manquants : " & String.Join(", ", champsManquants)
            If objetsManquants.Count > 0 Then
                msg &= vbCrLf & "Objets métiers manquants : " & String.Join(", ", objetsManquants)
            End If

            MessageBox.Show(msg, "Champs obligatoires", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Logger.WARN($"Mouvement invalide — champs manquants : {String.Join(", ", champsManquants)} | Objets manquants : {String.Join(", ", objetsManquants)}")

            ' Donne le focus au premier contrôle dont le texte est vide
            If Not premierControleNonValide Is Nothing Then
                premierControleNonValide.Focus()
            End If

            Return False ' mouvement invalide
        End If

        Return True ' tout renseigné
    End Function

    ''' <summary>
    ''' Fonction utilitaire pour vérifier si un contrôle est vide et l'ajouter à la liste des manquants.
    ''' Gère également la définition du premier contrôle non valide pour le focus.
    ''' </summary>
    Private Sub AjouterSiVide(ByVal ctrl As Control, ByVal nomChamp As String, ByVal listeManquants As List(Of String), ByRef premierControleNonValide As Control)
        If String.IsNullOrWhiteSpace(ctrl.Text) Then
            listeManquants.Add(nomChamp)
            If premierControleNonValide Is Nothing Then
                premierControleNonValide = ctrl
            End If
        End If
    End Sub

    ' --- Gestion des événements de contrôles ---

    Private Sub TxtMontant_Leave(sender As Object, e As EventArgs) Handles txtMontant.Leave
        ' Utilisation de Handles TxtMontant.Leave plutôt que TextChanged pour la validation
        ' après que l'utilisateur ait quitté le champ.
        If Not Regex.Match(txtMontant.Text, Constantes.regExMontant, RegexOptions.IgnoreCase).Success Then
            MessageBox.Show($"Le montant '{txtMontant.Text}' doit être numérique ou respecter le format attendu!")
            ' Remet le focus sur la zone de saisie du montant
            txtMontant.Focus()
        End If
    End Sub
    Private Sub btnInsereTiers_Click(sender As Object, e As EventArgs) Handles btnInsereTiers.Click
        FrmNouveauTiers.Show()
    End Sub

    Private Sub InsereMouvement()
        Try
            ' Les infos de création du mouvement sont récupérées sur la fenêtre de saisie
            _mvtEnCours = CreerMouvement()

            If _mvtEnCours Is Nothing Then
                ' Une erreur est survenue dans CreerMouvement (déjà loggée)
                Return
            End If

            _dtMvtsIdentiques = Mouvements.ChargerMouvementsSimilaires(_mvtEnCours)

            If _dtMvtsIdentiques IsNot Nothing AndAlso _dtMvtsIdentiques.Rows.Count > 0 Then
                ' Un mouvement identique existe déjà
                Dim frmListe As New FrmListe(_dtMvtsIdentiques)
                AddHandler frmListe.objetSelectionneChanged, AddressOf mvtSelectionneChangedHandler
                frmListe.ShowDialog()
                Logger.INFO($"Le mouvement existe déjà : {_mvtEnCours.mvtValeursConcatenees}")
            Else
                Mouvements.InsererMouvementEnBase(_mvtEnCours)
                Logger.INFO($"Insertion du mouvement pour : {_mvtEnCours.mvtValeursConcatenees}")
                ' TODO : Ajouter une réinitialisation du formulaire après insertion réussie
            End If
        Catch ex As Exception
            MsgBox($"Erreur {ex.Message} lors de l'insertion des données {_mvtEnCours?.mvtValeursConcatenees}")
            Logger.ERR($"Erreur {ex.Message} lors de l'insertion des données {_mvtEnCours?.mvtValeursConcatenees}")
        End Try
    End Sub
    Private Sub mvtSelectionneChangedHandler(sender As Object, index As Integer)
        Try
            ' Vérifier si l'objet peut être converti en Mouvements
            If index = -1 Then
                Logger.INFO("L'objet sélectionné est nul (annulation ou non-sélection) => mouvement à insérer.")
                ' Insertion du nouveau mouvement si l'utilisateur annule le choix dans la liste des similaires
                Mouvements.InsererMouvementEnBase(_mvtEnCours)
            Else
                ' Récupération de l'ID du mouvement existant à mettre à jour
                Dim idMouvementExistant As Integer = Convert.ToInt32(_dtMvtsIdentiques.Rows(index).ItemArray(0))

                ' Utilisation des objets sélectionnés (plus fiable)
                Dim idCategorie As Integer = If(_categorieSelectionne Is Nothing, 0, _categorieSelectionne.Id)
                Dim idSousCategorie As Integer = If(_sousCategorieSelectionnee Is Nothing, 0, _sousCategorieSelectionnee.Id)
                Dim montantDecimal As Decimal = GetMontantValue(txtMontant.Text, rbCredit.Checked) ' Utilisation de la nouvelle fonction
                Dim idTiers As Integer = If(_tiersSelectionne Is Nothing, 0, _tiersSelectionne.Id)

                ' Autres valeurs
                Dim note As String = txtNote.Text
                Dim dateMouvement As Date = dateMvt.Value
                Dim rapproche As Boolean = rbRapproche.Checked
                Dim evenementLibelle As String = If(_evenementSelectionne Is Nothing, String.Empty, _evenementSelectionne.libelle)
                Dim typeDocLibelle As String = If(_typeDocSelectionne Is Nothing, String.Empty, _typeDocSelectionne.Libellé)
                ' TypeMvt n'est pas utilisé dans MettreAJourMouvement, mais conservé pour l'exemple
                Dim modifiable As Boolean = True ' À revoir selon la logique métier
                Dim remise As Integer = GetRemiseValue(txtRemise.Text)
                Dim reference As String = Utilitaires.ExtraitNuméroChèque(txtNote.Text) ' Calcul de la référence
                Dim typeReference As String = If(_typeDocSelectionne Is Nothing, String.Empty, _typeDocSelectionne.Libellé)

                ' Mettre à jour le mouvement existant
                Dim rowsAffected As Integer = Mouvements.MettreAJourMouvement(
                    idMouvementExistant, idCategorie, idSousCategorie, montantDecimal.ToString(), rbCredit.Checked, idTiers, note, dateMouvement, rapproche, evenementLibelle, typeDocLibelle, modifiable, remise, reference, typeReference, _idDocSelectionne
                )

                Logger.INFO($"Mouvement existant ID {idMouvementExistant} mis à jour : {rowsAffected} ligne(s) affectée(s).")
            End If
        Catch ex As Exception
            ' Log des exceptions
            Logger.ERR($"Erreur lors de la mise à jour du mouvement : {ex.Message}")
            MessageBox.Show($"Une erreur est survenue lors de la mise à jour du mouvement : {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Function GetRemiseValue(texteRemise As String) As Integer
        If Integer.TryParse(texteRemise, GetRemiseValue) Then
            Return GetRemiseValue ' Retourne la valeur convertie en entier (via TryParse)
        Else
            Return 0 ' Retourne 0 si la conversion échoue
        End If
    End Function

    ''' <summary>
    ''' Calcule la valeur décimale du montant en tenant compte du sens (Crédit/Débit).
    ''' </summary>
    Private Function GetMontantValue(montantTexte As String, isCredit As Boolean) As Decimal
        Dim montantDecimal As Decimal
        montantTexte = montantTexte.Trim().Replace(Constantes.espace, String.Empty)

        If Not Decimal.TryParse(montantTexte, NumberStyles.Any, CultureInfo.CurrentCulture, montantDecimal) Then
            Throw New FormatException($"Montant invalide : « {montantTexte} »")
        End If

        ' Ajustement du sens : négatif si Débit (et si le montant est positif)
        If Not isCredit AndAlso montantDecimal > 0 Then
            montantDecimal *= -1D
        End If

        Return montantDecimal
    End Function

    Private Function CreerMouvement() As Mouvements
        Try
            ' 🔹 Conversion sécurisée du montant (utilise la nouvelle fonction)
            Dim montantDecimal As Decimal = GetMontantValue(txtMontant.Text, rbCredit.Checked)

            ' 🔹 Récupération des ID et Libellés (plus sûr avec les objets)
            Dim idCategorie As Integer = If(_categorieSelectionne Is Nothing, 0, _categorieSelectionne.Id)
            Dim idSousCategorie As Integer = If(_sousCategorieSelectionnee Is Nothing, 0, _sousCategorieSelectionnee.Id)
            Dim idTiers As Integer = If(_tiersSelectionne Is Nothing, 0, _tiersSelectionne.Id)
            Dim evenementLibelle As String = If(_evenementSelectionne Is Nothing, String.Empty, _evenementSelectionne.libelle).Trim()
            Dim typeMvtLibelle As String = If(_typeMvtSelectionne Is Nothing, String.Empty, _typeMvtSelectionne.libelle).Trim()
            Dim typeDocLibelle As String = If(_typeDocSelectionne Is Nothing, String.Empty, _typeDocSelectionne.Libellé)

            ' 🔹 Extraction de la référence (numéro de chèque si TypeDoc est "Chèque")
            Dim sNumCheque As String = If(typeDocLibelle.Equals("Chèque", StringComparison.OrdinalIgnoreCase),
                                          Utilitaires.ExtraitNuméroChèque(txtNote.Text),
                                          String.Empty)

            ' 🔹 Création de l'objet Mouvements
            Dim mouvement As New Mouvements(
                note:=txtNote.Text.Trim(),
                categorie:=idCategorie,
                sousCategorie:=idSousCategorie,
                tiers:=idTiers,
                dateMvt:=dateMvt.Value,
                montant:=montantDecimal,
                sens:=rbCredit.Checked,
                etat:=rbRapproche.Checked,
                événement:=evenementLibelle,
                type:=typeMvtLibelle,
                modifiable:=False,
                numeroRemise:=txtRemise.Text.Trim(),
                reference:=sNumCheque,
                typeReference:=typeDocLibelle,
                idDoc:=_idDocSelectionne
            )

            Logger.INFO($"Mouvement créé : {mouvement.Note} | Montant {montantDecimal} | TypeDoc={typeDocLibelle} | idDoc={_idDocSelectionne}")
            Return mouvement

        Catch ex As Exception
            Dim unused = MessageBox.Show($"Erreur lors de la création du mouvement : {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Logger.ERR("Erreur CreerMouvement : " & ex.ToString())
            Return Nothing
        End Try
    End Function

    ' --- Gestion des Documents Associés ---

    Private Sub btnSelDoc_Click(sender As Object, e As EventArgs) Handles btnSelDoc.Click
        Try

            ' Récupère et valide le montant
            Dim montant As Decimal
            ' Utilisation d'une fonction pour la conversion du montant
            Try
                montant = GetMontantValue(txtMontant.Text, rbCredit.Checked)
            Catch ex As FormatException
                MessageBox.Show(ex.Message, "Erreur de format", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return
            End Try

            Dim selectionneDocument As New FrmSelectionneDocument()

            AddHandler selectionneDocument.IdDocSelectionneChanged, AddressOf IdDocSelectionneChangedHandler

            ' Récupération du type de document sélectionné
            Dim typeDocLibelle As String = If(_typeDocSelectionne Is Nothing, String.Empty, _typeDocSelectionne.Libellé)

            If typeDocLibelle.Equals(TypeDocumentLibelles.Libelles(TypeDocument.Cheque), StringComparison.OrdinalIgnoreCase) Then
                Dim numeroCheque As String = Utilitaires.ExtraitNuméroChèque(txtNote.Text)
                Dim nomTiers As String = txtTiers.Text ' Utilisation du texte du Tiers pour la recherche

                ' Convertir le numéro de chèque en Decimal si nécessaire pour chargeListeDoc
                Dim numChequeDecimal As Decimal
                If Decimal.TryParse(numeroCheque, numChequeDecimal) Then
                    selectionneDocument.chargeListeDoc(numChequeDecimal, montant, nomTiers)
                Else
                    selectionneDocument.chargeListeDoc(montant) ' Fallback si le numéro de chèque n'est pas numérique
                End If

                ' 🔹 Cas 2 : autres types → 1 seul argument
            Else
                selectionneDocument.chargeListeDoc(montant)
            End If

            ' Affiche la fenêtre modale
            If selectionneDocument.ShowDialog() = DialogResult.OK Then
                _idDocSelectionne = selectionneDocument.IdDocSelectionne
                If _idDocSelectionne = 0 Then
                    Logger.WARN($"Aucun document associé à ce mouvement pour le montant : {montant}")
                    txtDocument.Clear()
                Else
                    txtDocument.Text = _idDocSelectionne.ToString()
                    Logger.INFO($"Document sélectionné : ID {_idDocSelectionne}")
                End If
            End If

        Catch ex As Exception
            Logger.ERR($"Erreur lors de la sélection du document : {ex.Message}")
            MessageBox.Show($"Erreur lors de la sélection du document : {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub IdDocSelectionneChangedHandler(ByVal idDoc As Integer)
        'Mettre à jour le Mouvement 
        Mouvements.MettreAJourIdDoc(_mvtEnCours.Id, idDoc)
    End Sub
    Private Sub btnSelTiers_Click(sender As Object, e As EventArgs) Handles btnSelTiers.Click
        _tiersSelectionne = AppelFrmSelectionUtils.OuvrirSelectionGenerique(Of gestionTresorerie.Tiers)(
        nomRequete:="reqIdentiteCatTiers",
        titreFenetre:="Sélection du tiers",
        txtDestination:=txtTiers,
        champLibelle:="Nom"
    )
        'Je force l'affichage du nom et du prénom car l'appel ci-dessus ne ramène que la propriété "Nom"
        txtTiers.Text = _tiersSelectionne.Nom & " " & _tiersSelectionne.Prenom
    End Sub
    Private Sub txtTiers_TextChanged(sender As Object, e As EventArgs) Handles txtTiers.TextChanged
        Try
            ' On suppose que tu as stocké le Tiers sélectionné dans une variable ou propriété
            If _tiersSelectionne Is Nothing Then
                Logger.INFO("Aucun typeDoc sélectionné, impossible de charger la catégorie.")
                txtCategorie.Clear()
                Exit Sub
            End If

            ' Récupération de la catégorie par défaut du typeDoc
            Dim idCategorieDefaut As Integer = _tiersSelectionne.CategorieDefaut
            If idCategorieDefaut <= 0 Then
                Logger.INFO($"Le typeDoc '{_tiersSelectionne.Nom}' n’a pas de catégorie par défaut.")
                txtCategorie.Clear()
                Exit Sub
            End If

            ' --- Préparer le paramètre pour la requête ---
            Dim parametres As New Dictionary(Of String, Object) From {
                {"@idCategorie", idCategorieDefaut}
            }

            ' --- Exécuter la requête selIdLibCat --- 
            Using Reader As SqlDataReader = SqlCommandBuilder.CreateSqlCommand(Constantes.bddAgumaaa, "selIdLibCat", parametres).ExecuteReader
                If Reader.HasRows Then
                    While Reader.Read()
                        ' Créer une instance concrète implémentant ITypeDoc
                        _categorieSelectionne = New Categorie(Reader.GetInt32(0), Reader.GetString(1))
                        _sousCategorieSelectionnee = New SousCategorie(Reader.GetInt32(0), Reader.GetString(1))

                        txtCategorie.Text = $"{_categorieSelectionne.Id} - {_categorieSelectionne.Libelle}"
                        txtSousCategorie.Text = $"{_sousCategorieSelectionnee.Id} - {_sousCategorieSelectionnee.Libelle}"
                    End While
                Else
                    ' Gérer le cas où le reader est vide
                    Logger.WARN("Aucune catégorie trouvée.")
                End If
            End Using
        Catch ex As Exception
            Logger.ERR($"Erreur lors du chargement de la catégorie du typeDoc '{txtTiers.Text}' : {ex.Message}")
        End Try
    End Sub

    Private Sub btnSelCat_Click(sender As Object, e As EventArgs) Handles btnSelCat.Click
        _categorieSelectionne = AppelFrmSelectionUtils.OuvrirSelectionGenerique(Of Categorie)(
        nomRequete:="selIdLibCat",
        titreFenetre:="Sélection de la catégorie",
        txtDestination:=txtCategorie
    )
        txtCategorie.Text = _categorieSelectionne.Libelle
    End Sub
    Private Sub btnSelSousCategorie_Click(sender As Object, e As EventArgs) Handles btnSelSousCategorie.Click
        _sousCategorieSelectionnee = AppelFrmSelectionUtils.OuvrirSelectionGenerique(Of SousCategorie)(
        nomRequete:="sqlSelSousCategoriesTout",
        titreFenetre:="Sélection de la sous-catégorie",
        txtDestination:=txtSousCategorie
    )
        txtSousCategorie.Text = _sousCategorieSelectionnee.Libelle
    End Sub
    Private Sub btnSelEvenement_Click(sender As Object, e As EventArgs) Handles btnSelEvenement.Click
        _evenementSelectionne = AppelFrmSelectionUtils.OuvrirSelectionGenerique(Of Evenement)(
            nomRequete:="reqEvenement",
            titreFenetre:="Sélection de l'événement",
            txtDestination:=txtEvenement,
            champLibelle:="Evénement"  ' ou autre propriété si besoin
        )
        txtEvenement.Text = _evenementSelectionne.libelle
    End Sub
    Private Sub btnSelTypeMvt_Click(sender As Object, e As EventArgs) Handles btnSelTypeMvt.Click
        _typeMvtSelectionne = AppelFrmSelectionUtils.OuvrirSelectionGenerique(Of TypeMvt)(
            nomRequete:="reqTypesMouvement",
            titreFenetre:="Sélection du typeDoc de mouvement",
            txtDestination:=txtTypeMvt,
            champLibelle:="TypeMouvement mouvement"  ' ou autre propriété si besoin
        )
        txtTypeMvt.Text = _typeMvtSelectionne.libelle
    End Sub
    Private Sub btnSelTypeDoc_Click(sender As Object, e As EventArgs) Handles btnSelTypeDoc.Click
        _typeDocSelectionne = AppelFrmSelectionUtils.OuvrirSelectionGenerique(Of TypeDocImpl)(
            nomRequete:="reqLibellesTypesDocuments",
            titreFenetre:="Sélection du typeDoc de document",
            txtDestination:=txtTypeDoc,
            champLibelle:="Libellé"  ' ou autre propriété si besoin
        )
        txtTypeDoc.Text = _typeDocSelectionne.Libellé
    End Sub
End Class