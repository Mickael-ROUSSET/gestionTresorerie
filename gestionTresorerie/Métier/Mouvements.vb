Imports System.Data.SqlClient
Imports System.Text.RegularExpressions
Imports Windows.Win32.System
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
                'Logger.INFO($"Insertion du mouvement pour : {mouvement.ObtenirValeursConcatenees}")
                Logger.INFO($"Insertion du mouvement pour  ")
            End If
        Catch ex As Exception
            Dim unused = MsgBox($"Erreur {ex.Message} lors de l'insertion des données {mouvement.ObtenirValeursConcatenees}")
            Logger.ERR($"Erreur {ex.Message} lors de l'insertion des données {mouvement.ObtenirValeursConcatenees}")
        End Try
    End Sub
    Public Sub New(ByVal note As String, ByVal categorie As Integer, ByVal sousCategorie As Integer, ByVal tiers As Integer, ByVal dateMvt As Date, ByVal montant As String, ByVal sens As String, ByVal etat As String, ByVal événement As String, ByVal type As String, ByVal modifiable As Boolean, ByVal numeroRemise As String, ByVal reference As String, ByVal typeReference As String, ByVal idDoc As Integer)
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
                .reference = reference
                .typeReference = typeReference
                .idDoc = idDoc
            Else
                Logger.WARN($"Infos manquantes pour la création du mouvement : {note} {categorie} {sousCategorie} {tiers} {dateMvt} {montant} {sens} {etat} {événement} {type} {modifiable} {numeroRemise} {reference} {typeReference} {idDoc}")
            End If
        End With
    End Sub
    Public Sub New()
        If Not IsDate(DateMvt) AndAlso Sens = String.Empty AndAlso Montant = String.Empty Then
            Err.Raise("Erreur dans la création du mouvement : " & ObtenirValeursConcatenees())
        End If
    End Sub
    'Public Shared Sub InsererMouvementEnBase(mouvement As Mouvements)
    '    Try
    '        Dim unused = SqlCommandBuilder.
    '            CreateSqlCommand("insertMvts",
    '                             New Dictionary(Of String, Object) From
    '                                                     {{"@note", mouvement.Note},
    '                                                     {"@categorie", mouvement.Categorie},
    '                                                     {"@sousCategorie", mouvement.SousCategorie},
    '                                                     {"@tiers", mouvement.Tiers},
    '                                                     {"@dateCréation", DateTime.Now},
    '                                                     {"@dateMvt", mouvement.DateMvt},
    '                                                     {"@montant", Utilitaires.ConvertToDecimal(mouvement.Montant)},
    '                                                     {"@sens", mouvement.Sens},
    '                                                     {"@etat", mouvement.Etat},
    '                                                     {"@événement", mouvement.Événement},
    '                                                     {"@type", mouvement.Type},
    '                                                     {"@modifiable", mouvement.Modifiable},
    '                                                     {"@numeroRemise", mouvement.NumeroRemise},
    '                                                     {"@reference", mouvement.reference},
    '                                                     {"@typeReference", mouvement.typeReference},
    '                                                     {"@idDoc", mouvement.idDoc}}
    '                         ).ExecuteNonQuery()
    '        Logger.INFO($"Insertion du mouvement réussie : {mouvement.ObtenirValeursConcatenees}")
    '    Catch ex As Exception
    '        Logger.ERR($"Erreur générale lors de l'insertion du mouvement : {ex.Message}, Mouvement : {mouvement.ObtenirValeursConcatenees}")
    '        Throw ' Re-lancer l'exception après l'avoir loggée
    '    End Try
    'End Sub
    Public Shared Sub InsererMouvementEnBase(mouvement As Mouvements)
        If mouvement Is Nothing Then Throw New ArgumentNullException(NameOf(mouvement))

        Try
            Dim paramètres As New Dictionary(Of String, Object) From {
            {"@note", If(String.IsNullOrWhiteSpace(mouvement.Note), DBNull.Value, mouvement.Note.Trim())},
            {"@categorie", If(String.IsNullOrWhiteSpace(mouvement.Categorie), DBNull.Value, mouvement.Categorie)},
            {"@sousCategorie", If(String.IsNullOrWhiteSpace(mouvement.SousCategorie), DBNull.Value, mouvement.SousCategorie)},
            {"@tiers", If(String.IsNullOrWhiteSpace(mouvement.Tiers), DBNull.Value, mouvement.Tiers)},
            {"@dateCréation", DateTime.Now},
            {"@dateMvt", If(mouvement.DateMvt = Date.MinValue, DBNull.Value, mouvement.DateMvt)},
            {"@montant", mouvement.Montant},
            {"@sens", If(String.IsNullOrWhiteSpace(mouvement.Sens), DBNull.Value, mouvement.Sens)},
            {"@etat", If(String.IsNullOrWhiteSpace(mouvement.Etat), DBNull.Value, mouvement.Etat)},
            {"@événement", If(String.IsNullOrWhiteSpace(mouvement.Événement), DBNull.Value, mouvement.Événement)},
            {"@type", If(String.IsNullOrWhiteSpace(mouvement.Type), DBNull.Value, mouvement.Type)},
            {"@modifiable", mouvement.Modifiable},
            {"@numeroRemise", If(String.IsNullOrWhiteSpace(mouvement.NumeroRemise), DBNull.Value, mouvement.NumeroRemise.Trim())},
            {"@reference", If(String.IsNullOrWhiteSpace(mouvement.reference), DBNull.Value, mouvement.reference.Trim())},
            {"@typeReference", If(String.IsNullOrWhiteSpace(mouvement.typeReference), DBNull.Value, mouvement.typeReference.Trim())},
            {"@idDoc", If(mouvement.idDoc = 0, DBNull.Value, mouvement.idDoc)}
        }
            Utilitaires.LogCommand(SqlCommandBuilder.CreateSqlCommand("insertMvts", paramètres))
            Dim lignes = SqlCommandBuilder.CreateSqlCommand("insertMvts", paramètres).ExecuteNonQuery()

            If lignes = 1 Then
                'Logger.INFO($"Mouvement inséré : {mouvement.ObtenirValeursConcatenees()}")
                Logger.INFO($"Mouvement inséré  ")
            Else
                'Logger.WARN($"Insertion anormale : {lignes} ligne(s) affectée(s). {mouvement.ObtenirValeursConcatenees()}")
                Logger.WARN($"Insertion anormale : {lignes} ligne(s) affectée(s). ")
            End If

        Catch sqlEx As SqlException
            Logger.ERR($"SQL Error {sqlEx.Number}: {sqlEx.Message}" & vbCrLf & mouvement.ObtenirValeursConcatenees())
            Throw
        Catch ex As Exception
            Logger.ERR($"Erreur insertion : {ex.Message}" & vbCrLf & mouvement.ObtenirValeursConcatenees())
            Throw
        End Try
    End Sub
    Public Shared Sub MettreAJourIdDoc(idMouvement As Integer, nouvelIdDoc As Integer)
        Try
            Dim rowsAffected As Integer = SqlCommandBuilder.CreateSqlCommand("updMvtIdDoc",
                                      New Dictionary(Of String, Object) From {
                                      {"@nouvelIdDoc", nouvelIdDoc},
                                      {"@idMouvement", idMouvement}}).ExecuteNonQuery
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
                    bExiste = reader.GetInt32(0) > 0
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
                Dim unused = adapter.Fill(dataTable)
            End Using

            ' Écrire un log d'information
            Logger.INFO($"Chargement des mouvements similaires réussi : {mouvement.ObtenirValeursConcatenees}")
        Catch ex As Exception
            ' Écrire un log d'erreur en cas d'exception générale
            Logger.ERR($"Erreur lors du chargement des mouvements similaires : {ex.Message}")
        End Try

        Return dataTable
    End Function
    Public Shared Function MettreAJourMouvement(Id As Integer, categorie As Integer, sousCategorie As Integer, montant As Decimal, sens As Boolean, tiers As Integer, note As String, dateMvt As Date, etat As Boolean, evenement As String, type As String, modifiable As Boolean, numeroRemise As Integer?, reference As String, typeReference As String, idDoc As Integer) As Integer
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
                                                                     {"@reference", If(reference, DBNull.Value)},
                                                                     {"@typeReference", If(typeReference, DBNull.Value)},
                                                                     {"@idDoc", idDoc}}
                             ).
                             ExecuteNonQuery()
            ' Trace indiquant les valeurs mises à jour
            Logger.INFO($"Valeurs mises à jour - Catégorie: {categorie}, Sous-Catégorie: {sousCategorie}, Montant: {montant}, Sens: {sens}, Tiers: {tiers}, Note: {note}, DateMvt: {dateMvt}, Etat: {etat}, Evénement: {evenement}, Type: {type}, Modifiable: {modifiable}, Numéro Remise: {numeroRemise}, reference: {reference}, typeReference: {typeReference}, idDoc: {idDoc}")
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
            Dim unused = command.Parameters.AddWithValue("@Id", id)

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
    Public ReadOnly Property Id() As Integer
    Public Property Categorie() As Integer
    Public Property SousCategorie() As Integer
    Public Property Tiers() As Integer
    Public Property DateMvt() As Date
    Public Property Montant() As Decimal
    Public Property Sens() As Boolean
    Public Property Etat() As String
    Public Property reference() As String
    Public Property typeReference() As String
    Public Property idDoc() As Integer
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
    Public Property NumeroRemise() As String
        Get
            Return CInt(_numeroRemise)
        End Get
        Set(ByVal value As String)
            Dim s As String
            s = Trim(Strings.Replace(value, """", String.Empty))
            _numeroRemise = If(Integer.TryParse(Trim(Strings.Replace(value, """", String.Empty)), vbNull),
                Trim(Strings.Replace(value, """", String.Empty)),
                "0")
        End Set
    End Property
    ''' <summary>
    ''' Retourne une chaîne formatée avec toutes les valeurs du mouvement.
    ''' </summary>
    Public Function ObtenirValeursConcatenees() As String
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
        AjouterChamp(sb, "Événement", _événement)
        AjouterChamp(sb, "Type", _type)
        AjouterChamp(sb, "Modifiable", Modifiable)
        AjouterChamp(sb, "Numéro de Remise", _numeroRemise)
        sb.Append(vbCrLf)
        AjouterChamp(sb, "Référence", reference)
        AjouterChamp(sb, "Type de référence", typeReference)
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
    Private Sub AjouterChamp(sb As Text.StringBuilder, nom As String, valeur As Object, Optional format As String = Nothing)
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