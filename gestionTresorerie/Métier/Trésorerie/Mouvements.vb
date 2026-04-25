Imports System.Data.SqlClient
Imports System.Text.RegularExpressions
Imports System.Globalization
Public Class Mouvements
    Private _note As String
    Private _evenement As String
    Private _typeMouvement As String
    Private _numeroRemise As String
    Public ReadOnly Property Id() As Integer
    Public Property Categorie() As Integer
    Public Property SousCategorie() As Integer
    Public Property Tiers() As Integer
    Public Property DateMvt() As Date
    Public Property Montant() As Decimal
    Public Property Sens() As Boolean
    Public Property Etat() As Boolean
    Public Property reference() As String
    Public Property typeReference() As String
    Public Property idDoc() As Integer
    Public Property Evenement() As String
        Get
            Return _evenement
        End Get
        Set(ByVal value As String)
            _evenement = If(value, String.Empty).Trim()
        End Set
    End Property
    Public Property TypeMouvement() As String
        Get
            Return _typeMouvement
        End Get
        Set(ByVal value As String)
            _typeMouvement = If(value, String.Empty).Trim()
        End Set
    End Property
    Public Property Modifiable() As Boolean
    Public Property NumeroRemise() As String
        Get
            Return _numeroRemise
        End Get
        Set(ByVal value As String)
            Dim s As String = Trim(Strings.Replace(If(value, String.Empty), """", String.Empty))
            Dim remiseInt As Integer
            If Integer.TryParse(s, remiseInt) Then
                _numeroRemise = s
            Else
                _numeroRemise = "0"
            End If
        End Set
    End Property
    Private Shared Function CreateRepository() As MouvementRepository
        Dim connectionString As String =
        ConnexionDB.GetInstance(Constantes.DataBases.Agumaaa).
                    GetConnexion(Constantes.DataBases.Agumaaa).
                    ConnectionString

        Dim factory As New AgumaaaConnectionFactory(connectionString)
        Dim provider As ISqlTextProvider = New LegacySqlTextProvider()
        Dim executor As ISqlExecutor = New SqlExecutor(factory, provider)

        Return New MouvementRepository(executor, factory, provider)
    End Function

    Public Shared Sub InsererMouvementEnBase(mouvement As Mouvements)
        ArgumentNullException.ThrowIfNull(mouvement)

        Try
            Dim lignes As Integer = CreateRepository().Inserer(mouvement)

            If lignes = 1 Then
                Logger.INFO("Mouvement inséré")
            Else
                Logger.WARN($"Insertion anormale : {lignes} ligne(s) affectée(s).")
            End If

        Catch sqlEx As SqlException
            Logger.ERR($"SQL Error {sqlEx.Number}: {sqlEx.Message}" & vbCrLf & mouvement.mvtValeursConcatenees())
            Throw

        Catch ex As Exception
            Logger.ERR($"Erreur insertion : {ex.Message}" & vbCrLf & mouvement.mvtValeursConcatenees())
            Throw
        End Try
    End Sub

    ' Constructeur rendu tolérant aux valeurs nulles et conversions robustes
    Public Sub New(ByVal note As String, ByVal categorie As Integer, ByVal sousCategorie As Integer, ByVal tiers As Integer, ByVal dateMvt As Date, ByVal montant As String, ByVal sens As String, ByVal etat As String, ByVal événement As String, ByVal type As String, ByVal modifiable As Boolean, ByVal numeroRemise As String, ByVal reference As String, ByVal typeReference As String, ByVal idDoc As Integer)
        ' Préparer des valeurs sûres et convertir silencieusement les types
        Dim noteSafe As String = If(note, String.Empty).Trim()
        Dim categorieSafe As Integer = categorie
        Dim sousCategorieSafe As Integer = sousCategorie
        Dim tiersSafe As Integer = tiers
        Dim dateMvtSafe As Date = If(dateMvt = Nothing, Date.MinValue, dateMvt)

        Dim montantSafe As Decimal = 0D
        Dim montantStr As String = Convert.ToString(montant)
        If Not Decimal.TryParse(montantStr, NumberStyles.Any, CultureInfo.CurrentCulture, montantSafe) Then
            ' Si impossible à parser, conserver 0
            montantSafe = 0D
        End If

        Dim sensBool As Boolean = False
        Dim sensStr As String = Convert.ToString(sens)
        If Not Boolean.TryParse(sensStr, sensBool) Then
            ' essayer conversion numérique (0/1)
            If Integer.TryParse(sensStr, Nothing) Then
                sensBool = CInt(sensStr) <> 0
            Else
                sensBool = False
            End If
        End If

        Dim etatSafe As String = If(etat, String.Empty).Trim()
        Dim evenementSafe As String = If(événement, String.Empty).Trim()
        Dim typeSafe As String = If(type, String.Empty).Trim()
        Dim numeroRemiseSafe As String = If(numeroRemise, String.Empty).Trim()
        Dim referenceSafe As String = If(reference, String.Empty).Trim()
        Dim typeReferenceSafe As String = If(typeReference, String.Empty).Trim()
        Dim idDocSafe As Integer = idDoc

        ' Vérifier la présence minimale des paramètres en appelant VerifParam avec des chaînes sûres
        If VerifParam(categorieSafe.ToString(), sousCategorieSafe.ToString(), tiersSafe, dateMvtSafe, montantSafe.ToString(CultureInfo.CurrentCulture), sensBool.ToString(), etatSafe, typeSafe) Then
            With Me
                .Note = noteSafe
                .Categorie = categorieSafe
                .SousCategorie = sousCategorieSafe
                .Tiers = tiersSafe
                .DateMvt = dateMvtSafe
                .DateCréation = Date.Now
                .Montant = montantSafe
                .Sens = sensBool
                .Etat = etatSafe
                .Evenement = evenementSafe
                .TypeMouvement = typeSafe
                .Modifiable = modifiable
                .NumeroRemise = numeroRemiseSafe
                .reference = referenceSafe
                .typeReference = typeReferenceSafe
                .idDoc = idDocSafe
            End With
        Else
            Logger.WARN($"Infos manquantes pour la création du mouvement : {noteSafe} {categorieSafe} {sousCategorieSafe} {tiersSafe} {dateMvtSafe} {montantSafe} {sensBool} {etatSafe} {evenementSafe} {typeSafe} {modifiable} {numeroRemiseSafe} {referenceSafe} {typeReferenceSafe} {idDocSafe}")
        End If
    End Sub

    Public Sub New()
        If Not IsDate(DateMvt) AndAlso Sens = String.Empty AndAlso Montant = String.Empty Then
            Err.Raise("Erreur dans la création du mouvement : " & mvtValeursConcatenees())
        End If
    End Sub
    Public Shared Sub MettreAJourIdDoc(idMouvement As Integer, nouvelIdDoc As Integer)
        Try
            Dim rowsAffected As Integer = CreateRepository().MettreAJourIdDoc(idMouvement, nouvelIdDoc)

            If rowsAffected > 0 Then
                Logger.INFO($"Mise à jour réussie de idDoc pour le mouvement avec Id = {idMouvement}")
            Else
                Logger.WARN($"Aucune ligne n'a été mise à jour pour le mouvement avec Id = {idMouvement}")
            End If

        Catch ex As Exception
            Logger.ERR($"Erreur inattendue lors de la mise à jour de idDoc. Message: {ex.Message}")
            Throw
        End Try
    End Sub
    Public Shared Function Existe(mouvement As Mouvements) As Boolean
        If mouvement Is Nothing Then Throw New ArgumentNullException(NameOf(mouvement))

        Try
            Dim bExiste As Boolean = CreateRepository().Existe(mouvement)

            Logger.DBG($"Vérification existence OK - Date: {mouvement.DateMvt}, Montant: {mouvement.Montant}, Sens: {mouvement.Sens}")

            Return bExiste

        Catch ex As Exception
            Logger.ERR($"Erreur lors de la vérification d'existence : {ex.Message} {mouvement.mvtValeursConcatenees()}")
            Throw
        End Try
    End Function
    ' Surcharge de la méthode Existe pour vérifier l'existence d'un mouvement avec les paramètres date, montant et sens 
    Public Shared Function Existe(dateMvt As Date, montant As Decimal, sens As Boolean) As Boolean
        ' Créer un objet Mouvements temporaire
        Dim mouvement As New Mouvements With {
            .DateMvt = dateMvt,
            .Montant = montant,
            .Sens = sens
        }

        ' Appeler la méthode originale Existe avec l'objet Mouvements temporaire
        Return Existe(mouvement)
    End Function
    Public Shared Function ChargerMouvementsSimilaires(mouvement As Mouvements) As DataTable
        If mouvement Is Nothing Then
            Throw New ArgumentNullException(NameOf(mouvement))
        End If

        Try
            Dim dataTable As DataTable =
            CreateRepository().ChargerMouvementsSimilaires(mouvement)

            Logger.INFO($"Chargement des mouvements similaires réussi : {mouvement.mvtValeursConcatenees}")

            Return dataTable

        Catch ex As Exception
            Logger.ERR($"Erreur lors du chargement des mouvements similaires : {ex.Message}")
            Return New DataTable()
        End Try
    End Function
    Public Shared Function MettreAJourMouvement(Id As Integer,
                                            categorie As Integer,
                                            sousCategorie As Integer,
                                            montant As Decimal,
                                            sens As Boolean,
                                            tiers As Integer,
                                            note As String,
                                            dateMvt As Date,
                                            etat As Boolean,
                                            evenement As String,
                                            type As String,
                                            modifiable As Boolean,
                                            numeroRemise As Integer?,
                                            reference As String,
                                            typeReference As String,
                                            idDoc As Integer) As Integer
        Try
            Dim rowsAffected As Integer =
            CreateRepository().MettreAJour(Id,
                                           categorie,
                                           sousCategorie,
                                           montant,
                                           sens,
                                           tiers,
                                           note,
                                           dateMvt,
                                           etat,
                                           evenement,
                                           type,
                                           modifiable,
                                           numeroRemise,
                                           reference,
                                           typeReference,
                                           idDoc)

            Logger.INFO($"Valeurs mises à jour - Catégorie: {categorie}, Sous-Catégorie: {sousCategorie}, Montant: {montant}, Sens: {sens}, Tiers: {tiers}, Note: {note}, DateMvt: {dateMvt}, Etat: {etat}, Evénement: {evenement}, TypeMouvement: {type}, Modifiable: {modifiable}, Numéro Remise: {numeroRemise}, reference: {reference}, typeReference: {typeReference}, idDoc: {idDoc}")

            Return rowsAffected

        Catch ex As Exception
            Logger.ERR($"Erreur lors de la mise à jour du mouvement : {ex.Message}")
            Return -1
        End Try
    End Function
    Public Shared Sub SupprimerMouvement(id As Integer)
        Try
            Dim rowsAffected As Integer =
            CreateRepository().Supprimer(id)

            If rowsAffected > 0 Then
                Logger.INFO($"Mouvement supprimé - Id: {id}")
            Else
                Logger.WARN($"Aucun mouvement supprimé pour Id: {id}")
            End If

        Catch ex As Exception
            Logger.ERR($"Erreur lors de la suppression du mouvement {id} : {ex.Message}")
            Throw
        End Try
    End Sub
    Public Shared Function VerifParam(categorie As String, sousCategorie As String, tiers As Integer, dateMvt As Date, montant As String, sens As String, etat As String, type As String) As Boolean
        Dim bToutEstLa As Boolean = False

        'La référence (dont idCheque) est facultatif
        If categorie <> String.Empty AndAlso
            sousCategorie <> String.Empty AndAlso
            tiers <> 0 AndAlso
            IsDate(dateMvt) AndAlso
            sens <> String.Empty AndAlso
            etat <> String.Empty AndAlso
            type <> String.Empty AndAlso
            montant <> String.Empty Then
            bToutEstLa = True
        End If
        Return bToutEstLa
    End Function
    Public Property DateCréation() As Date
    Public Property Note() As String
        'https://learn.microsoft.com/fr-fr/dotnet/standard/base-types/regular-expressions
        Get
            Return IIf(_note > String.Empty, _note, "Null")
        End Get
        Set(ByVal value As String)
            'Suppression des doubles quotes
            _note = Regex.Replace(value, "(.*)""(.*)", String.Empty)
        End Set
    End Property
    ''' <summary>
    ''' Retourne une chaîne formatée avec toutes les valeurs du mouvement.
    ''' </summary>
    Public Function mvtValeursConcatenees() As String
        Dim sb As New Text.StringBuilder()

        ' === Ajout des champs ===
        AjouterChamp(sb, "Note", _note)
        AjouterChamp(sb, "Catégorie", _Categorie)
        AjouterChamp(sb, "Sous-Catégorie", _SousCategorie)
        AjouterChamp(sb, "Tiers", _Tiers)
        AjouterChamp(sb, "Date de Création", _DateCréation, "yyyy-MM-dd")
        AjouterChamp(sb, "Date de Mouvement", DateMvt, "yyyy-MM-dd")
        AjouterChamp(sb, "Montant", _Montant, "N2")
        AjouterChamp(sb, "Sens", _Sens)
        AjouterChamp(sb, "État", Etat)
        AjouterChamp(sb, "Evenement", _evenement)
        AjouterChamp(sb, "TypeMouvement", _typeMouvement)
        AjouterChamp(sb, "Modifiable", Modifiable)
        AjouterChamp(sb, "Numéro de Remise", _numeroRemise)
        AjouterChamp(sb, "Référence", reference)
        AjouterChamp(sb, "TypeMouvement de référence", typeReference)
        AjouterChamp(sb, "idDoc", idDoc)

        ' Nettoyage final
        Dim resultat = sb.ToString()
        If resultat.EndsWith(", ") Then resultat = resultat.Substring(0, resultat.Length - 2)
        If resultat.EndsWith(","c) Then resultat = resultat.TrimEnd(","c)

        Return resultat.Trim()
    End Function

    ''' <summary>
    ''' Ajoute un champ au StringBuilder avec formatage et gestion des valeurs nulles.
    ''' </summary>
    Private Shared Sub AjouterChamp(sb As Text.StringBuilder, nom As String, valeur As Object, Optional format As String = Nothing)
        Dim valeurStr As String

        If valeur Is Nothing OrElse IsDBNull(valeur) Then
            valeurStr = "(vide)"
        ElseIf TypeOf valeur Is Date AndAlso DirectCast(valeur, Date) = Date.MinValue Then
            valeurStr = "(vide)"
        ElseIf format IsNot Nothing AndAlso TypeOf valeur Is IFormattable Then
            valeurStr = DirectCast(valeur, IFormattable).ToString(format, Nothing)
        Else
            valeurStr = valeur.ToString()
        End If

        sb.Append($"{nom}: {valeurStr}, ")
    End Sub
End Class