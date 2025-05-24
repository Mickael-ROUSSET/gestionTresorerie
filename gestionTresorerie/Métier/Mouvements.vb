Imports System.Data.SqlClient
Imports System.Text.RegularExpressions
Public Class Mouvements
    Private _note As String
    Private _événement As String
    Private _type As String
    Private _numeroRemise As String

    Public Shared Sub InsereMouvement(mouvement)
        Try
            'Les infos de création du mouvement sont récupérées sur la fenêtre de saisie 
            Dim dtMvtsIdentiques As DataTable = Mouvements.ChargerMouvementsSimilaires(mouvement)
            If dtMvtsIdentiques.Rows.Count > 0 Then
                'Un mouvement identique existe déjà
                Logger.WARN($"Le mouvement existe déjà : {mouvement.ObtenirValeursConcatenees}")
            Else
                Mouvements.InsererMouvementEnBase(mouvement)
                Logger.INFO($"Insertion du mouvement pour : {mouvement.ObtenirValeursConcatenees}")
            End If
        Catch ex As Exception
            MsgBox($"Erreur {ex.Message} lors de l'insertion des données {mouvement.ObtenirValeursConcatenees}")
            Logger.ERR($"Erreur {ex.Message} lors de l'insertion des données {mouvement.ObtenirValeursConcatenees}")
        End Try
    End Sub
    Public Sub New(ByVal note As String, ByVal categorie As Integer, ByVal sousCategorie As Integer, ByVal tiers As Integer, ByVal dateMvt As Date, ByVal montant As String, ByVal sens As String, ByVal etat As String, ByVal événement As String, ByVal type As String, ByVal modifiable As Boolean, ByVal numeroRemise As String, ByVal idCheque As Integer)
        ' Set the property value.
        With Me
            If VerifParam(categorie, sousCategorie, tiers, dateMvt, montant, sens, etat, type) Then
                .Note = note
                .Categorie = categorie
                .SousCategorie = sousCategorie
                .Tiers = tiers
                .DateMvt = dateMvt
                .DateCréation = Date.Now
                .Montant = montant
                .Sens = sens
                .Etat = etat
                .Événement = événement
                .Type = type
                .Modifiable = modifiable
                .NumeroRemise = numeroRemise
                .idCheque = idCheque
            Else
                Logger.WARN($"Infos manquantes pour la création du mouvement : {note} {categorie} {sousCategorie} {tiers} {dateMvt} {montant} {sens} {etat} {événement} {type} {modifiable} {numeroRemise} {idCheque}")
            End If
        End With
    End Sub
    Public Sub New()
        If Not IsDate(DateMvt) AndAlso Sens = String.Empty AndAlso Montant = String.Empty Then
            Err.Raise("Erreur dans la création du mouvement : " & ObtenirValeursConcatenees())
        End If
    End Sub
    Public Shared Sub InsererMouvementEnBase(mouvement As Mouvements)
        Try
            SqlCommandBuilder.
                CreateSqlCommand("insertMvts",
                                 New Dictionary(Of String, Object) From
                                                         {{"@note", mouvement.Note},
                                                         {"@categorie", mouvement.Categorie},
                                                         {"@sousCategorie", mouvement.SousCategorie},
                                                         {"@tiers", mouvement.Tiers},
                                                         {"@dateCréation", DateTime.Now},
                                                         {"@dateMvt", mouvement.DateMvt},
                                                         {"@montant", Utilitaires.ConvertToDecimal(mouvement.Montant)},
                                                         {"@sens", mouvement.Sens},
                                                         {"@etat", mouvement.Etat},
                                                         {"@événement", mouvement.Événement},
                                                         {"@type", mouvement.Type},
                                                         {"@modifiable", mouvement.Modifiable},
                                                         {"@numeroRemise", mouvement.NumeroRemise},
                                                         {"@idCheque", mouvement.idCheque}}
                             ).ExecuteNonQuery()
            Logger.INFO($"Insertion du mouvement réussie : {mouvement.ObtenirValeursConcatenees}")
        Catch ex As Exception
            Logger.ERR($"Erreur générale lors de l'insertion du mouvement : {ex.Message}, Mouvement : {mouvement.ObtenirValeursConcatenees}")
            Throw ' Re-lancer l'exception après l'avoir loggée
        End Try
    End Sub
    Public Shared Sub MettreAJourIdCheque(idMouvement As Integer, nouvelIdCheque As Integer)
        Try
            Dim rowsAffected As Integer = SqlCommandBuilder.CreateSqlCommand("updMvtIdChq",
                                      New Dictionary(Of String, Object) From {
                                      {"@nouvelIdCheque", nouvelIdCheque},
                                      {"@idMouvement", idMouvement}}).ExecuteNonQuery
            If rowsAffected > 0 Then
                Logger.INFO($"Mise à jour réussie de idCheque pour le mouvement avec Id = {idMouvement}")
            Else
                Logger.WARN($"Aucune ligne n'a été mise à jour pour le mouvement avec Id = {idMouvement}")
            End If
        Catch ex As Exception
            Logger.ERR($"Erreur inattendue lors de la mise à jour de idCheque. Message: {ex.Message}")
            Throw
        End Try
    End Sub
    Public Shared Function Existe(mouvement As Mouvements) As Boolean
        ' Vérifie si le mouvement existe déjà
        Dim bExiste As Boolean = False

        Try
            Using reader As SqlDataReader =
                SqlCommandBuilder.CreateSqlCommand("reqNbMouvements",
                                           New Dictionary(Of String, Object) From {{"@dateMvt", mouvement.DateMvt.ToString("yyyy-MM-dd")},
                                           {"@montant", CDec(mouvement.Montant)},
                                           {"@sens", mouvement.Sens}
                                            }).ExecuteReader()
                If reader.Read() Then
                    bExiste = (reader.GetInt32(0) > 0)
                End If
            End Using
            ' Écrire un log d'information
            Logger.DBG($"Vérification de l'existence du mouvement réussie. Date: {mouvement.DateMvt}, Montant: {mouvement.Montant}, Sens:  {mouvement.Sens}")

        Catch ex As Exception
            ' Écrire un log d'erreur
            Logger.ERR($"Erreur lors de la vérification de l'existence du mouvement. Message: {ex.Message} " & mouvement.ObtenirValeursConcatenees)
            Throw ' Re-lancer l'exception après l'avoir loggée
        End Try

        Return bExiste
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
        Dim dataTable As New DataTable()

        Try
            ' Définir la commande SQL pour appeler la procédure stockée 
            Dim cmd As SqlCommand = SqlCommandBuilder.CreateSqlCommand("procMvtsIdentiques",
                                           New Dictionary(Of String, Object) From {{"@dateMvt", mouvement.DateMvt},
                                           {"@montant", CDec(mouvement.Montant)},
                                           {"@sens", mouvement.Sens}})
            ' Créer un DataAdapter pour remplir le DataTable
            Using adapter As New SqlDataAdapter(cmd)
                ' Remplir le DataTable avec les données de la base de données
                adapter.Fill(dataTable)
            End Using

            ' Écrire un log d'information
            Logger.INFO($"Chargement des mouvements similaires réussi : {mouvement.ObtenirValeursConcatenees}")
        Catch ex As Exception
            ' Écrire un log d'erreur en cas d'exception générale
            Logger.ERR($"Erreur lors du chargement des mouvements similaires : {ex.Message}")
        End Try

        Return dataTable
    End Function
    Public Shared Function MettreAJourMouvement(Id As Integer, categorie As Integer, sousCategorie As Integer, montant As Decimal, sens As Boolean, tiers As Integer, note As String, dateMvt As Date, etat As Boolean, evenement As String, type As String, modifiable As Boolean, numeroRemise As Integer?, Optional idCheque As Integer? = Nothing) As Integer
        Try
            Return SqlCommandBuilder.
            CreateSqlCommand("updMvt",
                             New Dictionary(Of String, Object) From {{"@Id", Id},
                                                                     {"@Categorie", categorie},
                                                                     {"@SousCategorie", sousCategorie},
                                                                     {"@Montant", montant},
                                                                     {"@Sens", sens},
                                                                     {"@Tiers", tiers},
                                                                     {"@Note", If(note, DBNull.Value)},
                                                                     {"@DateMvt", dateMvt},
                                                                     {"@Etat", etat},
                                                                     {"@Evenement", If(evenement, DBNull.Value)},
                                                                     {"@Type", If(type, DBNull.Value)},
                                                                     {"@Modifiable", modifiable},
                                                                     {"@NumeroRemise", If(numeroRemise, DBNull.Value)},
                                                                     {"@IdCheque", If(idCheque, DBNull.Value)}}
                             ).
                             ExecuteNonQuery()
            ' Trace indiquant les valeurs mises à jour
            Logger.INFO($"Valeurs mises à jour - Catégorie: {categorie}, Sous-Catégorie: {sousCategorie}, Montant: {montant}, Sens: {sens}, Tiers: {tiers}, Note: {note}, DateMvt: {dateMvt}, Etat: {etat}, Evénement: {evenement}, Type: {type}, Modifiable: {modifiable}, Numéro Remise: {numeroRemise}, IdChèque: {idCheque}")
        Catch ex As Exception
            ' Trace en cas d'erreur
            Logger.ERR($"Erreur lors de la mise à jour du mouvement : {ex.Message}")
            ' Retourner -1 en cas d'erreur
            Return -1
        End Try
    End Function
    Public Shared Sub SupprimerMouvement(id As Integer)
        Dim sqlConnexion As SqlConnection = Nothing

        Try
            Dim command = SqlCommandBuilder.CreateSqlCommand("delMvt")
            ' Ajouter le paramètre Id à la requête
            command.Parameters.AddWithValue("@Id", id)

            ' Exécuter la requête et obtenir le nombre de lignes affectées
            Dim rowsAffected As Integer = command.ExecuteNonQuery()

            ' Trace indiquant le nombre de lignes supprimées
            Logger.INFO($"Nombre de lignes supprimées : {rowsAffected}")

            ' Trace indiquant l'Id supprimé
            Logger.INFO($"Enregistrement supprimé - Id: {id}")

        Catch ex As Exception
            ' Trace en cas d'erreur
            Logger.ERR($"Erreur lors de la suppression du mouvement : {ex.Message}")
        Finally
            ' Fermer la connexion si elle est ouverte
            If sqlConnexion IsNot Nothing AndAlso sqlConnexion.State = ConnectionState.Open Then
                sqlConnexion.Close()
            End If
        End Try
    End Sub
    Public Shared Function VerifParam(categorie As String, sousCategorie As String, tiers As Integer, dateMvt As Date, montant As String, sens As String, etat As String, type As String) As Boolean
        Dim bToutEstLa As Boolean = False

        'L'idCheque est facultatif
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
    Public ReadOnly Property Id() As Integer
    Public Property Categorie() As Integer
    Public Property SousCategorie() As Integer
    Public Property Tiers() As Integer
    Public Property DateMvt() As Date
    Public Property Montant() As Decimal
    Public Property Sens() As Boolean
    Public Property Etat() As String
    Public Property Événement() As String
        Get
            Return _événement
        End Get
        Set(ByVal value As String)
            _événement = Trim(value)
        End Set
    End Property
    Public Property Type() As String
        Get
            Return _type
        End Get
        Set(ByVal value As String)
            _type = Trim(value)
        End Set
    End Property
    Public Property Modifiable() As Boolean
    Public Property idCheque() As Integer
    Public Property NumeroRemise() As String
        Get
            Return CInt(_numeroRemise)
        End Get
        Set(ByVal value As String)
            Dim s As String
            s = Trim(Strings.Replace(value, """", String.Empty))
            If Integer.TryParse(Trim(Strings.Replace(value, """", String.Empty)), vbNull) Then
                _numeroRemise = Trim(Strings.Replace(value, """", String.Empty))
            Else
                _numeroRemise = "0"
            End If
        End Set
    End Property
    Public Function ObtenirValeursConcatenees() As String
        ' Initialiser une chaîne vide pour stocker le résultat
        Dim resultat As String = String.Empty

        ' Concaténer le nom de chaque variable avec sa valeur
        resultat &= "Note: " & _note & ", "
        resultat &= "Catégorie: " & _categorie & ", "
        resultat &= "Sous-Catégorie: " & _sousCategorie & ", "
        resultat &= "Tiers: " & _tiers.ToString() & ", "
        resultat &= "Date de Création: " & _dateCréation.ToString("yyyy-MM-dd") & ", "
        resultat &= "Date de Mouvement: " & DateMvt.ToString("yyyy-MM-dd") & ", "
        resultat &= "Montant: " & _montant.ToString() & ", "
        resultat &= "Sens: " & _sens & ", "
        resultat &= "État: " & Etat & ", "
        resultat &= "Événement: " & _événement & ", "
        resultat &= "Type: " & _type & ", "
        resultat &= "Modifiable: " & Modifiable.ToString() & ", "
        resultat &= "Numéro de Remise: " & _numeroRemise & ", "
        resultat &= "ID Chèque: " & idCheque.ToString() & vbCrLf

        ' Retourner la chaîne concaténée
        Return resultat
    End Function
End Class