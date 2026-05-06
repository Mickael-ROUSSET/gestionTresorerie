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
    Private ReadOnly _errorProvider As New ErrorProvider()

    Private Sub FrmSaisie_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            InitialiserListeTiers()
            Dim indTiersDetecte As Integer = listeTiers.DetecteTiers(txtNote.Text)
            _errorProvider.ContainerControl = Me
            _errorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink
        Catch ex As Exception
            Logger.ERR($"Erreur lors du chargement du formulaire : {ex.Message}")
            ' Utilisation de Exit Sub au lieu de End pour un comportement plus propre dans WinForms
            MessageBox.Show($"Une erreur critique est survenue : {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Me.Close()
        End Try
    End Sub

    Private Function ValiderFormulaireAvecAffichage() As Boolean
        _errorProvider.Clear()

        Dim dto As SaisieMouvementDto = ConstruireDtoDepuisFormulaire()

        Dim result As SaisieValidationResult =
        SaisieValidationService.Valider(
            dto,
            _tiersSelectionne IsNot Nothing,
            _categorieSelectionne IsNot Nothing,
            _sousCategorieSelectionnee IsNot Nothing,
            _typeMvtSelectionne IsNot Nothing)

        If result.Erreurs.ContainsKey("Montant") Then
            _errorProvider.SetError(txtMontant, result.Erreurs("Montant"))
        End If

        If result.Erreurs.ContainsKey("Tiers") Then
            _errorProvider.SetError(txtTiers, result.Erreurs("Tiers"))
        End If

        If result.Erreurs.ContainsKey("Categorie") Then
            _errorProvider.SetError(txtCategorie, result.Erreurs("Categorie"))
        End If

        If result.Erreurs.ContainsKey("SousCategorie") Then
            _errorProvider.SetError(txtSousCategorie, result.Erreurs("SousCategorie"))
        End If

        If result.Erreurs.ContainsKey("TypeMvt") Then
            _errorProvider.SetError(txtTypeMvt, result.Erreurs("TypeMvt"))
        End If

        btnValider.Enabled = result.IsValid
        Return result.IsValid
    End Function
    Private Sub InitialiserListeTiers()
        If listeTiers Is Nothing Then
            listeTiers = New ListeTiers()
        End If
    End Sub
    Private Function ConstruireDtoDepuisFormulaire() As SaisieMouvementDto
        Return New SaisieMouvementDto With {
        .DateMvt = dateMvt.Value,
        .Note = txtNote.Text,
        .MontantTexte = txtMontant.Text,
        .EstDebit = rbDebit.Checked,
        .IdCategorie = If(_categorieSelectionne Is Nothing, 0, _categorieSelectionne.Id),
        .IdSousCategorie = If(_sousCategorieSelectionnee Is Nothing, 0, _sousCategorieSelectionnee.Id),
        .IdTiers = If(_tiersSelectionne Is Nothing, 0, _tiersSelectionne.Id),
        .IdDoc = _idDocSelectionne
    }
    End Function
    Private Sub BtnValider_Click(sender As Object, e As EventArgs) Handles btnValider.Click
        If Not ValiderFormulaireAvecAffichage() Then
            Logger.WARN("Validation du mouvement refusée : formulaire invalide.")
            Exit Sub
        End If

        Try
            InsereMouvement()
        Catch ex As Exception
            Logger.ERR($"Erreur lors de la validation du mouvement : {ex.Message}")
            MessageBox.Show("Erreur technique lors de l'enregistrement : " & ex.Message,
                        "Erreur",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub ChampValidation_Changed(sender As Object, e As EventArgs) _
    Handles txtMontant.TextChanged,
            txtTiers.TextChanged,
            txtCategorie.TextChanged,
            txtSousCategorie.TextChanged,
            txtTypeMvt.TextChanged

        ValiderFormulaireAvecAffichage()
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
        ValiderFormulaireAvecAffichage()
    End Sub
    Private Sub btnInsereTiers_Click(sender As Object, e As EventArgs) Handles btnInsereTiers.Click
        FrmNouveauTiers.Show()
    End Sub

    Private Sub InsereMouvement()
        Try
            _mvtEnCours = CreerMouvement()

            If _mvtEnCours Is Nothing Then
                Return
            End If

            _dtMvtsIdentiques = Mouvements.ChargerMouvementsSimilaires(_mvtEnCours)

            If _dtMvtsIdentiques IsNot Nothing AndAlso _dtMvtsIdentiques.Rows.Count > 0 Then
                Dim frmListe As New FrmListe(_dtMvtsIdentiques)
                AddHandler frmListe.objetSelectionneChanged, AddressOf mvtSelectionneChangedHandler
                frmListe.ShowDialog()
                Logger.INFO($"Le mouvement existe déjà : {_mvtEnCours.mvtValeursConcatenees}")
            Else
                Mouvements.InsererMouvementEnBase(_mvtEnCours)
                Logger.INFO($"Insertion du mouvement pour : {_mvtEnCours.mvtValeursConcatenees}")
            End If

        Catch ex As ArgumentException
            MessageBox.Show(ex.Message,
                        "Saisie invalide",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning)

        Catch ex As Exception
            Logger.ERR($"Erreur {ex.Message} lors de l'insertion des données {_mvtEnCours?.mvtValeursConcatenees}")
            MessageBox.Show($"Erreur lors de l'insertion du mouvement : {ex.Message}",
                        "Erreur",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error)
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
                Dim montantDecimal As Decimal = SaisieService.ParserMontant(txtMontant.Text)
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
    Private Function GetMontantValue(montantTexte As String, isCredit As Boolean) As Decimal
        Return SaisieService.ParserMontant(montantTexte)
    End Function

    Private Function CreerMouvement() As Mouvements
        Dim dto As SaisieMouvementDto = ConstruireDtoDepuisFormulaire()

        Dim typeDocLibelle As String =
        If(_typeDocSelectionne Is Nothing OrElse String.IsNullOrWhiteSpace(_typeDocSelectionne.Libellé),
           String.Empty,
           _typeDocSelectionne.Libellé.Trim())

        Dim reference As String =
        If(typeDocLibelle.Equals("Chèque", StringComparison.OrdinalIgnoreCase),
           Utilitaires.ExtraitNuméroChèque(txtNote.Text),
           String.Empty)

        Dim options As New MouvementBuildOptions With {
        .TiersSelectionne = _tiersSelectionne IsNot Nothing,
        .CategorieSelectionnee = _categorieSelectionne IsNot Nothing,
        .SousCategorieSelectionnee = _sousCategorieSelectionnee IsNot Nothing,
        .TypeMvtSelectionne = _typeMvtSelectionne IsNot Nothing,
        .EvenementLibelle = If(_evenementSelectionne Is Nothing, String.Empty, _evenementSelectionne.libelle),
        .TypeMvtLibelle = If(_typeMvtSelectionne Is Nothing, String.Empty, _typeMvtSelectionne.libelle),
        .Rapproche = rbRapproche.Checked,
        .Modifiable = False,
        .NumeroRemise = txtRemise.Text,
        .Reference = reference,
        .TypeReference = typeDocLibelle
    }

        Return MouvementBuildService.ConstruireDepuisSaisie(dto, options)
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
        Try
            Dim tiers As Tiers =
            AppelFrmSelectionUtils.OuvrirSelectionGenerique(Of Tiers)(
                nomRequete:="reqIdentiteCatTiers",
                titreFenetre:="Sélection du tiers",
                txtDestination:=txtTiers,
                champLibelle:="Nom"
            )

            If tiers Is Nothing Then
                Logger.INFO("Sélection du tiers annulée.")
                Exit Sub
            End If

            _tiersSelectionne = tiers

            Dim nomComplet As String =
            $"{If(_tiersSelectionne.Nom, String.Empty)} {If(_tiersSelectionne.Prenom, String.Empty)}".Trim()

            txtTiers.Text = nomComplet

        Catch ex As Exception
            Logger.ERR($"Erreur sélection tiers : {ex.Message}")
            MessageBox.Show("Erreur lors de la sélection du tiers : " & ex.Message,
                        "Erreur",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub txtTiers_TextChanged(sender As Object, e As EventArgs) Handles txtTiers.TextChanged
        Try
            If _tiersSelectionne Is Nothing Then
                Logger.INFO("Aucun tiers sélectionné, impossible de charger la catégorie.")
                txtCategorie.Clear()
                txtSousCategorie.Clear()
                Exit Sub
            End If

            Dim idCategorieDefaut As Integer = _tiersSelectionne.CategorieDefaut
            If idCategorieDefaut <= 0 Then
                Logger.INFO($"Le tiers '{_tiersSelectionne.Nom}' n’a pas de catégorie par défaut.")
                txtCategorie.Clear()
                txtSousCategorie.Clear()
                Exit Sub
            End If

            Dim parametres As New Dictionary(Of String, Object) From {
            {"@idCategorie", idCategorieDefaut}
        }

            Dim dt As DataTable =
            SqlCommandBuilder.ExecuteDataTable(
                Constantes.DataBases.Agumaaa,
                "selIdLibCat",
                parametres)

            If dt.Rows.Count = 0 Then
                Logger.WARN("Aucune catégorie trouvée.")
                Exit Sub
            End If

            For Each row As DataRow In dt.Rows
                _categorieSelectionne =
                New Categorie(Convert.ToInt32(row(0)), row(1).ToString())

                _sousCategorieSelectionnee =
                New SousCategorie(Convert.ToInt32(row(0)), row(1).ToString())

                txtCategorie.Text = $"{_categorieSelectionne.Id} - {_categorieSelectionne.Libelle}"
                txtSousCategorie.Text = $"{_sousCategorieSelectionnee.Id} - {_sousCategorieSelectionnee.Libelle}"
            Next

        Catch ex As Exception
            Logger.ERR($"Erreur lors du chargement de la catégorie du tiers '{txtTiers.Text}' : {ex.Message}")
        End Try
    End Sub

    Private Sub btnSelCat_Click(sender As Object, e As EventArgs) Handles btnSelCat.Click
        Try
            Dim categorie As Categorie =
            AppelFrmSelectionUtils.OuvrirSelectionGenerique(Of Categorie)(
                nomRequete:="selIdLibCat",
                titreFenetre:="Sélection de la catégorie",
                txtDestination:=txtCategorie
            )

            If categorie Is Nothing Then
                Logger.INFO("Sélection de la catégorie annulée.")
                Exit Sub
            End If

            _categorieSelectionne = categorie
            txtCategorie.Text = If(String.IsNullOrWhiteSpace(categorie.Libelle), String.Empty, categorie.Libelle.Trim())

        Catch ex As Exception
            Logger.ERR($"Erreur sélection catégorie : {ex.Message}")
            MessageBox.Show("Erreur lors de la sélection de la catégorie : " & ex.Message,
                        "Erreur",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub btnSelSousCategorie_Click(sender As Object, e As EventArgs) Handles btnSelSousCategorie.Click
        Try
            Dim sousCategorie As SousCategorie =
            AppelFrmSelectionUtils.OuvrirSelectionGenerique(Of SousCategorie)(
                nomRequete:="sqlSelSousCategoriesTout",
                titreFenetre:="Sélection de la sous-catégorie",
                txtDestination:=txtSousCategorie
            )

            If sousCategorie Is Nothing Then
                Logger.INFO("Sélection de la sous-catégorie annulée.")
                Exit Sub
            End If

            _sousCategorieSelectionnee = sousCategorie
            txtSousCategorie.Text = If(String.IsNullOrWhiteSpace(sousCategorie.Libelle), String.Empty, sousCategorie.Libelle.Trim())

        Catch ex As Exception
            Logger.ERR($"Erreur sélection sous-catégorie : {ex.Message}")
            MessageBox.Show("Erreur lors de la sélection de la sous-catégorie : " & ex.Message,
                        "Erreur",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub btnSelEvenement_Click(sender As Object, e As EventArgs) Handles btnSelEvenement.Click
        Try
            Dim evenement As Evenement =
            AppelFrmSelectionUtils.OuvrirSelectionGenerique(Of Evenement)(
                nomRequete:="reqEvenement",
                titreFenetre:="Sélection de l'événement",
                txtDestination:=txtEvenement,
                champLibelle:="Evénement"
            )

            If evenement Is Nothing Then
                Logger.INFO("Sélection de l'événement annulée.")
                Exit Sub
            End If

            _evenementSelectionne = evenement
            txtEvenement.Text = If(String.IsNullOrWhiteSpace(evenement.libelle), String.Empty, evenement.libelle.Trim())

        Catch ex As Exception
            Logger.ERR($"Erreur sélection événement : {ex.Message}")
            MessageBox.Show("Erreur lors de la sélection de l'événement : " & ex.Message,
                        "Erreur",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub btnSelTypeMvt_Click(sender As Object, e As EventArgs) Handles btnSelTypeMvt.Click
        Try
            Dim typeMvt As TypeMvt =
            AppelFrmSelectionUtils.OuvrirSelectionGenerique(Of TypeMvt)(
                nomRequete:="reqTypesMouvement",
                titreFenetre:="Sélection du type de mouvement",
                txtDestination:=txtTypeMvt,
                champLibelle:="TypeMouvement mouvement"
            )

            If typeMvt Is Nothing Then
                Logger.INFO("Sélection du type de mouvement annulée.")
                Exit Sub
            End If

            _typeMvtSelectionne = typeMvt
            txtTypeMvt.Text = If(String.IsNullOrWhiteSpace(typeMvt.libelle), String.Empty, typeMvt.libelle.Trim())

        Catch ex As Exception
            Logger.ERR($"Erreur sélection type mouvement : {ex.Message}")
            MessageBox.Show("Erreur lors de la sélection du type de mouvement : " & ex.Message,
                        "Erreur",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub btnSelTypeDoc_Click(sender As Object, e As EventArgs) Handles btnSelTypeDoc.Click
        Try
            Dim typeDoc As TypeDocImpl =
            AppelFrmSelectionUtils.OuvrirSelectionGenerique(Of TypeDocImpl)(
                nomRequete:="reqLibellesTypesDocuments",
                titreFenetre:="Sélection du type de document",
                txtDestination:=txtTypeDoc,
                champLibelle:="Libellé"
            )

            If typeDoc Is Nothing Then
                Logger.INFO("Sélection du type de document annulée.")
                Exit Sub
            End If

            _typeDocSelectionne = typeDoc
            txtTypeDoc.Text = If(String.IsNullOrWhiteSpace(typeDoc.Libellé), String.Empty, typeDoc.Libellé.Trim())

        Catch ex As Exception
            Logger.ERR($"Erreur sélection type document : {ex.Message}")
            MessageBox.Show("Erreur lors de la sélection du type de document : " & ex.Message,
                        "Erreur",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error)
        End Try
    End Sub
End Class