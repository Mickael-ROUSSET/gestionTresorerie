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

    Public Shared Sub InsereMouvement(mouvement)
        Try
            'Les infos de création du mouvement sont récupérées sur la fenêtre de saisie 
            Dim dtMvtsIdentiques As DataTable = Mouvements.ChargerMouvementsSimilaires(mouvement)
            If dtMvtsIdentiques.Rows.Count > 0 Then
                'Un mouvement identique existe déjà
                Logger.WARN($"Le mouvement existe déjà : {mouvement.ObtenirValeursConcatenees}")
            Else
                Mouvements.InsererMouvementEnBase(mouvement)
                'Logger.INFO($"Insertion du mouvement pour : {mouvement.mvtValeursConcatenees}")
                Logger.INFO($"Insertion du mouvement pour  ")
            End If
        Catch ex As Exception
            Dim unused = MsgBox($"Erreur {ex.Message} lors de l'insertion des données {mouvement.ObtenirValeursConcatenees}")
            Logger.ERR($"Erreur {ex.Message} lors de l'insertion des données {mouvement.ObtenirValeursConcatenees}")
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
    Public Shared Sub InsererMouvementEnBase(mouvement As Mouvements)
        Try
            Dim db = ConnexionDB.GetInstance(Constantes.bddAgumaaa)
            Using conn As SqlConnection = db.GetConnexion(Constantes.bddAgumaaa)

                Dim sql As String = "INSERT INTO [Mouvements] (categorie, montant, sens, tiers, dateMvt, dateCreation, note) " &
                                 "VALUES (@cat, @mont, @sens, @tiers, @dMvt, @dCrea, @note)"

                Using cmd As New SqlCommand(sql, conn)
                    ' Typage explicite pour garantir le plan d'exécution
                    cmd.Parameters.Add("@cat", SqlDbType.Int).Value = 20
                    cmd.Parameters.Add("@mont", SqlDbType.Decimal).Value = -263.22
                    cmd.Parameters.Add("@sens", SqlDbType.Bit).Value = 0
                    cmd.Parameters.Add("@tiers", SqlDbType.Int).Value = 1
                    cmd.Parameters.Add("@dMvt", SqlDbType.Date).Value = New DateTime(2025, 2, 13)
                    cmd.Parameters.Add("@dCrea", SqlDbType.Date).Value = DateTime.Now
                    cmd.Parameters.Add("@note", SqlDbType.NVarChar, 500).Value = "TEST RESET INSTANCE"

                    cmd.ExecuteNonQuery()
                    MessageBox.Show("L'insertion a fonctionné ! Le moteur est réparé.", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information)
                End Using
            End Using
        Catch ex As Exception
            Logger.ERR("Erreur après reset : " & ex.Message)
            MessageBox.Show("Détails de l'erreur : " & ex.ToString())
        End Try
    End Sub
    '    Public Shared Sub InsererMouvementEnBase(mouvement As Mouvements)
    '        If mouvement Is Nothing Then Throw New ArgumentNullException(NameOf(mouvement))

    '        Try
    '            Dim paramètres As New Dictionary(Of String, Object) From {
    '    {"@note", If(String.IsNullOrWhiteSpace(mouvement.Note), DBNull.Value, mouvement.Note.Trim())},
    '    {"@categorie", mouvement.Categorie}, ' C'est un INT NOT NULL, pas besoin de IF si > 0
    '    {"@sousCategorie", If(mouvement.SousCategorie = 0, DBNull.Value, mouvement.SousCategorie)},
    '    {"@tiers", Convert.ToInt32(mouvement.Tiers)},
    '    {"@dateCreation", DateTime.Now.Date}, ' On force le type DATE (sans l'heure)
    '    {"@dateMvt", If(mouvement.DateMvt = Date.MinValue, DBNull.Value, mouvement.DateMvt.Date)},
    '    {"@montant", Convert.ToDecimal(mouvement.Montant)}, ' Conversion explicite en Decimal
    '    {"@sens", If(mouvement.Sens, 1, 0)}, ' BIT attend 0 ou 1
    '    {"@etat", If(mouvement.Etat, 1, 0)},
    '    {"@evenement", If(String.IsNullOrWhiteSpace(mouvement.Evenement), DBNull.Value, mouvement.Evenement)},
    '    {"@typeMouvement", If(String.IsNullOrWhiteSpace(mouvement.TypeMouvement), DBNull.Value, mouvement.TypeMouvement)},
    '    {"@modifiable", If(mouvement.Modifiable, 1, 0)},
    '    {"@numeroRemise", If(String.IsNullOrWhiteSpace(mouvement.NumeroRemise) OrElse Not IsNumeric(mouvement.NumeroRemise),
    '                        DBNull.Value, Convert.ToInt32(mouvement.NumeroRemise))},
    '    {"@reference", If(String.IsNullOrWhiteSpace(mouvement.reference), DBNull.Value, mouvement.reference.Trim())},
    '    {"@typeReference", If(String.IsNullOrWhiteSpace(mouvement.typeReference), DBNull.Value, mouvement.typeReference.Trim())},
    '    {"@idDoc", If(mouvement.idDoc = 0, DBNull.Value, mouvement.idDoc)}
    '}
    '            Dim cmd As SqlCommand = SqlCommandBuilder.CreateSqlCommand(Constantes.bddAgumaaa, "insertMvts", paramètres)
    '            Utilitaires.LogCommand(cmd)
    '            Logger.WARN($"cmd.Connection.State.ToString(1) : {cmd.Connection.State.ToString()}")

    '            'Dim lignes = cmd.ExecuteNonQuery()
    '            ' TEST DE DÉPANNAGE DIRECT
    '            'Dim sqlTest As String = "INSERT INTO [Mouvements] (categorie, montant, sens, dateMvt, dateCreation, tiers) " &
    '            '             "VALUES (20, -263.22, 0, '2025-02-13', '2025-12-18', 1)"

    '            'Dim lignes As Integer
    '            'Using testCmd As New SqlCommand(sqlTest, cmd.Connection)
    '            '    lignes = testCmd.ExecuteNonQuery()
    '            'End Using
    '            ' Test de survie du moteur SQL
    '            Dim sqlSimple = "INSERT INTO Mouvements (categorie, montant, sens, tiers, dateMvt, dateCreation) " &
    '                "VALUES (@cat, @montant, @sens, @tiers, @dmvt, @dcrea)"
    '            Dim lignes As Integer
    '            Using cmdTest As New SqlCommand(sqlSimple, cmd.Connection)
    '                cmdTest.Parameters.Add("@cat", SqlDbType.Int).Value = 20
    '                cmdTest.Parameters.Add("@montant", SqlDbType.Decimal).Value = -263.22
    '                cmdTest.Parameters.Add("@sens", SqlDbType.Bit).Value = 0
    '                cmdTest.Parameters.Add("@tiers", SqlDbType.Int).Value = 1
    '                cmdTest.Parameters.Add("@dmvt", SqlDbType.Date).Value = DateTime.Now
    '                cmdTest.Parameters.Add("@dcrea", SqlDbType.Date).Value = DateTime.Now

    '                lignes = cmdTest.ExecuteNonQuery()
    '            End Using
    '            Logger.WARN($"cmd.Connection.State.ToString(2) : {cmd.Connection.State.ToString()}")

    '            If lignes = 1 Then
    '                Logger.INFO($"Mouvement inséré")
    '            Else
    '                Logger.WARN($"Insertion anormale : {lignes} ligne(s) affectée(s). ")
    '            End If

    '        Catch sqlEx As SqlException
    '            Logger.ERR($"SQL Error {sqlEx.Number}: {sqlEx.Message}" & vbCrLf & mouvement.mvtValeursConcatenees())
    '            Throw
    '        Catch ex As Exception
    '            Logger.ERR($"Erreur insertion : {ex.Message}" & vbCrLf & mouvement.mvtValeursConcatenees())
    '            Throw
    '        End Try
    '    End Sub
    Public Shared Sub MettreAJourIdDoc(idMouvement As Integer, nouvelIdDoc As Integer)
        Try
            Dim rowsAffected As Integer = SqlCommandBuilder.CreateSqlCommand(Constantes.bddAgumaaa, "updMvtIdDoc",
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
                SqlCommandBuilder.CreateSqlCommand(Constantes.bddAgumaaa, "reqNbMouvements",
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
            Logger.ERR($"Erreur lors de la vérification de l'existence du mouvement. Message: {ex.Message} " & mouvement.mvtValeursConcatenees)
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
            ' Définir la commande SQL pour appeler la requête
            Dim cmd As SqlCommand = SqlCommandBuilder.CreateSqlCommand(Constantes.bddAgumaaa, "reqMouvementsSimilaires",
                                           New Dictionary(Of String, Object) From {{"@dateMvt", mouvement.DateMvt},
                                           {"@montant", CDec(mouvement.Montant)},
                                           {"@sens", mouvement.Sens}})
            ' Créer un DataAdapter pour remplir le DataTable
            Using adapter As New SqlDataAdapter(cmd)
                ' Remplir le DataTable avec les données de la base de données
                adapter.Fill(dataTable)
            End Using

            ' Écrire un log d'information
            Logger.INFO($"Chargement des mouvements similaires réussi : {mouvement.mvtValeursConcatenees}")
        Catch ex As Exception
            ' Écrire un log d'erreur en cas d'exception générale
            Logger.ERR($"Erreur lors du chargement des mouvements similaires : {ex.Message}")
        End Try

        Return dataTable
    End Function
    Public Shared Function MettreAJourMouvement(Id As Integer, categorie As Integer, sousCategorie As Integer, montant As Decimal, sens As Boolean, tiers As Integer, note As String, dateMvt As Date, etat As Boolean, evenement As String, type As String, modifiable As Boolean, numeroRemise As Integer?, reference As String, typeReference As String, idDoc As Integer) As Integer
        Try
            Return SqlCommandBuilder.
            CreateSqlCommand(Constantes.bddAgumaaa, "updMvt",
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
                                                                     {"@TypeMouvement", If(type, DBNull.Value)},
                                                                     {"@Modifiable", modifiable},
                                                                     {"@NumeroRemise", If(numeroRemise, DBNull.Value)},
                                                                     {"@reference", If(reference, DBNull.Value)},
                                                                     {"@typeReference", If(typeReference, DBNull.Value)},
                                                                     {"@idDoc", idDoc}}
                             ).
                             ExecuteNonQuery()
            ' Trace indiquant les valeurs mises à jour
            Logger.INFO($"Valeurs mises à jour - Catégorie: {categorie}, Sous-Catégorie: {sousCategorie}, Montant: {montant}, Sens: {sens}, Tiers: {tiers}, Note: {note}, DateMvt: {dateMvt}, Etat: {etat}, Evénement: {evenement}, TypeMouvement: {type}, Modifiable: {modifiable}, Numéro Remise: {numeroRemise}, reference: {reference}, typeReference: {typeReference}, idDoc: {idDoc}")
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
            Dim command = SqlCommandBuilder.CreateSqlCommand(Constantes.bddAgumaaa, "delMvt")
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