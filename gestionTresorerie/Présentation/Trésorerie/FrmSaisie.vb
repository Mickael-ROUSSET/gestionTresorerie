Imports System.Data.SqlClient
Imports System.Globalization
Imports System.Text.RegularExpressions

Public Class FrmSaisie
    Inherits System.Windows.Forms.Form

    Private listeTiers As ListeTiers
    Private _Mvt As Mouvements
    Private _dtMvtsIdentiques As DataTable = Nothing
    Private _tiersSelectionne As Tiers
    Private _categorieSelectionne As Categorie
    Private _sousCategorieSelectionnee As SousCategorie
    Private _typeDocSelectionne As TypeDocImpl
    Private _typeEvenement As Evenement
    Private _typeMvt As TypeMvt

    Public Property Properties As Object
    Private isExpanded As Boolean = True
    Private _idDocSelectionne As Integer = 0

    Private Sub FrmSaisie_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            InitialiserListeTiers()
            Dim indTiersDetecte As Integer = listeTiers.DetecteTiers(txtNote.Text)
        Catch ex As Exception
            Logger.ERR($"Erreur lors du chargement du formulaire : {ex.Message}")
            End
        End Try
    End Sub
    Private Sub InitialiserListeTiers()
        If listeTiers Is Nothing Then
            listeTiers = New ListeTiers()
        End If
    End Sub
    Private Sub BtnValider_Click(sender As Object, e As EventArgs) Handles btnValider.Click
        If verifMouvement() Then
            Call InsereMouvement()
        Else
            MessageBox.Show("Le mouvement est invalide. Veuillez vérifier les informations saisies.", "Erreur de validation", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End If
    End Sub
    Private Function verifMouvement() As Boolean
        Dim missing As New List(Of String)

        If String.IsNullOrWhiteSpace(txtMontant.Text) Then missing.Add("Montant")
        If String.IsNullOrWhiteSpace(txtTiers.Text) Then missing.Add("Tiers")
        If String.IsNullOrWhiteSpace(txtCategorie.Text) Then missing.Add("Catégorie")
        If String.IsNullOrWhiteSpace(txtSousCategorie.Text) Then missing.Add("Sous-catégorie")
        If String.IsNullOrWhiteSpace(txtTypeDoc.Text) Then missing.Add("TypeMouvement de document")
        If String.IsNullOrWhiteSpace(txtDocument.Text) Then missing.Add("Document")
        If String.IsNullOrWhiteSpace(txtTypeMvt.Text) Then missing.Add("TypeMouvement de mouvement")

        If missing.Count > 0 Then
            Dim msg As String = "Champs obligatoires manquants : " & String.Join(", ", missing)
            MessageBox.Show(msg, "Champs obligatoires", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Logger.WARN($"Mouvement invalide — champs manquants : {String.Join(", ", missing)}")

            ' Donner le focus au premier champ manquant dans l'ordre défini
            If missing.Contains("Montant") Then
                txtMontant.Focus()
            ElseIf missing.Contains("Tiers") Then
                txtTiers.Focus()
            ElseIf missing.Contains("Catégorie") Then
                txtCategorie.Focus()
            ElseIf missing.Contains("Sous-catégorie") Then
                txtSousCategorie.Focus()
            ElseIf missing.Contains("TypeMouvement de document") Then
                txtTypeDoc.Focus()
            ElseIf missing.Contains("Document") Then
                txtDocument.Focus()
            ElseIf missing.Contains("TypeMouvement de mouvement") Then
                txtTypeMvt.Focus()
            End If

            Return False ' mouvement invalide
        End If

        Return True ' tout renseigné
    End Function
    Private Sub TxtMontant_TextChanged(sender As Object, e As EventArgs) Handles txtMontant.Leave

        If Not Regex.Match(txtMontant.Text, Constantes.regExMontant, RegexOptions.IgnoreCase).Success Then
            Dim unused1 = MessageBox.Show($"Le montant {txtMontant.Text} doit être numérique!")
            'Remet le focus sur la zone de saisie du montant
            Dim unused = txtMontant.Focus()
        End If
    End Sub
    Private Sub btnInsereTiers_Click(sender As Object, e As EventArgs) Handles btnInsereTiers.Click
        FrmNouveauTiers.Show()
    End Sub
    Private Sub InsereMouvement()
        'TODO à mettre dans la classe Mouvements
        Dim mouvement As Mouvements
        Try
            'Les infos de création du mouvement sont récupérées sur la fenêtre de saisie
            mouvement = CreerMouvement()
            _Mvt = mouvement
            _dtMvtsIdentiques = Mouvements.ChargerMouvementsSimilaires(mouvement)
            If _dtMvtsIdentiques.Rows.Count > 0 Then
                'Un mouvement identique existe déjà
                Dim frmListe As New FrmListe(_dtMvtsIdentiques)
                AddHandler frmListe.objetSelectionneChanged, AddressOf mvtSelectionneChangedHandler
                Dim unused1 = frmListe.ShowDialog()
                Logger.INFO($"Le mouvement existe déjà : {mouvement.ObtenirValeursConcatenees}")
            Else
                Mouvements.InsererMouvementEnBase(mouvement)
                Logger.INFO($"Insertion du mouvement pour : {mouvement.ObtenirValeursConcatenees}")
            End If
        Catch ex As Exception
            MsgBox($"Erreur {ex.Message} lors de l'insertion des données {mouvement.ObtenirValeursConcatenees}")
            Logger.ERR($"Erreur {ex.Message} lors de l'insertion des données {mouvement.ObtenirValeursConcatenees}")
        End Try
    End Sub
    Private Sub mvtSelectionneChangedHandler(sender As Object, index As Integer)
        Try
            ' Vérifier si l'objet peut être converti en Mouvements
            If index = -1 Then
                Logger.INFO("L'objet sélectionné est nul => mouvement à insérer")
                Mouvements.InsererMouvementEnBase(_Mvt)
            Else
                ' Utiliser des variables intermédiaires pour rendre le code plus lisible
                Dim id As Integer = _dtMvtsIdentiques.Rows(index).ItemArray(0)
                Dim categorie As String = txtCategorie.Text.Split("-"c)(0).Trim()
                Dim sousCategorie As String = txtSousCategorie.Text.Split("-"c)(0).Trim()
                Dim montant As String = txtMontant.Text.Trim().Replace(Constantes.espace, String.Empty)
                Dim credit As Boolean = rbCredit.Checked
                Dim tiers As Integer = Convert.ToInt32(txtTiers.Text.Split("-"c)(0).Trim())
                Dim note As String = txtNote.Text
                Dim dateMouvement As Date = dateMvt.Value
                Dim rapproche As Boolean = rbRapproche.Checked
                Dim evenement As String = txtEvenement.Text
                Dim typeDoc As String = txtTypeDoc.Text
                Dim typeMvt = txtTypeMvt.Text
                Dim modifiable As Boolean = True
                Dim remise As Integer = GetRemiseValue(txtRemise.Text)
                Dim reference As String = ""
                Dim typeReference As String = ""
                Dim idDoc As Integer = 0
                ' Mettre à jour le mouvement
                Dim rowsAffected As Integer = Mouvements.MettreAJourMouvement(
                id, categorie, sousCategorie, montant, credit, tiers, note, dateMouvement, rapproche, evenement, typeDoc, modifiable, remise, reference, typeReference, idDoc
            )
                ' Trace indiquant le nombre de lignes mises à jour
                Logger.INFO($"Nombre de mouvements mis à jour : {rowsAffected}")
            End If
        Catch ex As Exception
            ' Log des exceptions
            Logger.ERR($"Erreur lors de la mise à jour du mouvement : {ex.Message}")
        End Try
    End Sub
    Private Function GetRemiseValue(texteRemise As String) As Integer

        If String.IsNullOrEmpty(texteRemise) Then
            Return 0 ' Retourne 0 si le texte est vide
        End If

        Dim remiseValue As Integer
        If Integer.TryParse(texteRemise, remiseValue) Then
            Return remiseValue ' Retourne la valeur convertie en entier
        Else
            Return 0 ' Retourne 0 si la conversion échoue
        End If
    End Function
    Private Function CreerMouvement() As Mouvements
        Try

            ' 🔹 Récupération du typeDoc de document
            Dim sTypeDoc As String = ""
            If txtTypeDoc.Text <> String.Empty Then
                sTypeDoc = txtTypeDoc.Text
            End If

            ' 🔹 Extraction du numéro de chèque uniquement si le typeDoc est "Chèque"
            Dim sNumCheque As String = ""
            If sTypeDoc.Equals("Chèque", StringComparison.OrdinalIgnoreCase) Then
                sNumCheque = Utilitaires.ExtraitNuméroChèque(txtNote.Text)
            End If

            ' 🔹 Conversion sécurisée du montant
            Dim montantDecimal As Decimal
            Dim montantTexte As String = txtMontant.Text.Trim().Replace(Constantes.espace, String.Empty)

            If Not Decimal.TryParse(montantTexte, NumberStyles.Any, CultureInfo.CurrentCulture, montantDecimal) Then
                Throw New FormatException($"Montant invalide : « {txtMontant.Text} »")
            End If

            ' 🔹 Ajustement du sens : négatif si débit sélectionné
            If rbDebit.Checked Then
                montantDecimal *= -1D
            End If

            ' 🔹 Création de l'objet Mouvements 
            Dim evenementSafe As String = If(_typeEvenement, String.Empty).Trim()
            Dim mouvement As New Mouvements(
                            note:=txtNote.Text.Trim(),
                            categorie:=_categorieSelectionne.Id,
                            sousCategorie:=_sousCategorieSelectionnee.Id,
                            tiers:=_tiersSelectionne.Id,
                            dateMvt:=dateMvt.Value,
                            montant:=montantDecimal,
                            sens:=rbCredit.Checked,
                            etat:=rbRapproche.Checked,
                            événement:=evenementSafe,
                            type:=_typeMvt.libelle,
                            modifiable:=False,
                            numeroRemise:=txtRemise.Text.Trim(),
                            reference:=sNumCheque,
                            typeReference:=_typeDocSelectionne.Libellé,
                            idDoc:=_idDocSelectionne
                        )

            ' 🔹 Log de création
            Logger.INFO($"Mouvement créé : {mouvement.Note} | Montant {montantDecimal} | TypeDoc={sTypeDoc} | idDoc={_idDocSelectionne}")

            Return mouvement

        Catch ex As Exception
            Dim unused = MessageBox.Show($"Erreur lors de la création du mouvement : {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Logger.ERR("Erreur CreerMouvement : " & ex.ToString())
            Return Nothing
        End Try
    End Function
    Private Sub btnSelDoc_Click(sender As Object, e As EventArgs) Handles btnSelDoc.Click
        Try

            ' Récupère et valide le montant
            Dim montant As Decimal
            If Not Decimal.TryParse(txtMontant.Text, montant) Then
                Dim unused1 = MessageBox.Show("Montant invalide.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If

            ' ✅ Si rbDebit est sélectionné → montant négatif
            montant = If(rbDebit.Checked, -Math.Abs(montant), Math.Abs(montant))

            ' Instancie la fenêtre de sélection
            Dim selectionneDocument As New FrmSelectionneDocument()

            AddHandler selectionneDocument.IdDocSelectionneChanged, AddressOf IdDocSelectionneChangedHandler

            ' 🔹 Cas 1 : typeDoc "cheque" → 3 arguments
            'TODO : supprimer la valeur en dur "Cheque" et utiliser une constante ou une énumération
            If txtTypeDoc.Text = "Chèque" Then
                Dim numeroCheque As Decimal = CDec(Utilitaires.ExtraitNuméroChèque(txtNote.Text))
                Dim nomTiers As String = txtTiers.Text

                selectionneDocument.chargeListeDoc(numeroCheque, montant, nomTiers)

                ' 🔹 Cas 2 : autres types → 1 seul argument
            Else
                selectionneDocument.chargeListeDoc(montant)
            End If

            ' Affiche la fenêtre modale
            If selectionneDocument.ShowDialog() = DialogResult.OK Then
                _idDocSelectionne = selectionneDocument.IdDocSelectionne
                If _idDocSelectionne = 0 Then
                    Logger.WARN($"Aucun document associé à ce mouvement pour le montant : {montant}")
                Else
                    txtDocument.Text = _idDocSelectionne
                    Logger.INFO($"Document sélectionné : ID {_idDocSelectionne}")
                End If

                ' Toutes les grilles ont une sélection → procéder
                Try
                Catch ex As Exception
                    MessageBox.Show("Erreur lors de la validation : " & ex.Message,
                        "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Try
                ' 💡 Ici tu peux lancer ton traitement :
                ' Charger les métadonnées, afficher le contenu, lier à un mouvement, etc.
            End If

        Catch ex As Exception
            Dim unused = MessageBox.Show($"Erreur lors de la sélection du document : {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub IdDocSelectionneChangedHandler(ByVal idDoc As Integer)
        'Mettre à jour le Mouvement 
        Mouvements.MettreAJourIdDoc(_Mvt.Id, idDoc)
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
        _typeEvenement = AppelFrmSelectionUtils.OuvrirSelectionGenerique(Of Evenement)(
            nomRequete:="reqEvenement",
            titreFenetre:="Sélection de l'événement",
            txtDestination:=txtEvenement,
            champLibelle:="Evénement"  ' ou autre propriété si besoin
        )
        txtEvenement.Text = _typeEvenement.libelle
    End Sub
    Private Sub btnSelTypeMvt_Click(sender As Object, e As EventArgs) Handles btnSelTypeMvt.Click
        _typeMvt = AppelFrmSelectionUtils.OuvrirSelectionGenerique(Of TypeMvt)(
            nomRequete:="reqTypesMouvement",
            titreFenetre:="Sélection du typeDoc de mouvement",
            txtDestination:=txtTypeMvt,
            champLibelle:="TypeMouvement mouvement"  ' ou autre propriété si besoin
        )
        txtTypeMvt.Text = _typeMvt.libelle
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