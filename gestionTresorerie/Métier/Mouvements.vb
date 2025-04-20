Imports System.Text.RegularExpressions
Imports System.Data.SqlClient
Imports System.IO
Public Class Mouvements
    Private _note As String
    Private _categorie As Integer
    Private _sousCategorie As Integer
    Private _tiers As Integer
    Private _dateCréation As Date
    Private _dateMvt As Date
    Private _montant As Decimal
    Private _sens As String
    Private _etat As String
    Private _événement As String
    Private _type As String
    Private _modifiable As Boolean
    Private _numeroRemise As String
    Private _idCheque As Integer
    Public Sub New(ByVal note As String, ByVal categorie As Integer, ByVal sousCategorie As Integer, ByVal tiers As Integer, ByVal dateMvt As Date, ByVal montant As String, ByVal sens As String, ByVal etat As String, ByVal événement As String, ByVal type As String, ByVal modifiable As Boolean, ByVal numeroRemise As String, ByVal idCheque As Integer)
        ' Set the property value.
        With Me
            If VerifParam(note, categorie, sousCategorie, tiers, dateMvt, montant, sens, etat, événement, type, modifiable, numeroRemise, idCheque) Then
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
            End If
        End With
    End Sub


    Public Shared Function Existe(mouvement As Mouvements) As Boolean
        ' Vérifie si le mouvement existe déjà
        'Dim query As String = "SELECT COUNT(*) FROM Mouvements WHERE dateMvt = @dateMvt AND montant = @montant AND sens = @sens;"
        Dim bExiste As Boolean = False

        Try
            Using cmd As New SqlCommand(lectureProprietes.GetVariable("reqNbMouvements"), connexionDB.GetInstance.getConnexion)
                cmd.Parameters.AddWithValue("@dateMvt", mouvement.DateMvt.ToString("yyyy-MM-dd"))
                cmd.Parameters.AddWithValue("@montant", mouvement.Montant)
                cmd.Parameters.AddWithValue("@sens", mouvement.Sens)

                Using reader As SqlDataReader = cmd.ExecuteReader()
                    If reader.Read() Then
                        bExiste = (reader.GetInt32(0) > 0)
                    End If
                End Using
            End Using

            ' Écrire un log d'information
            Logger.GetInstance.INFO($"Vérification de l'existence du mouvement réussie. Date: {mouvement.DateMvt}, Montant: {mouvement.Montant}, Sens:  {mouvement.Sens}")

        Catch ex As Exception
            ' Écrire un log d'erreur
            Logger.GetInstance.ERR($"Erreur lors de la vérification de l'existence du mouvement. Message: {ex.Message} " & mouvement.ObtenirValeursConcatenees)
            Throw ' Re-lancer l'exception après l'avoir loggée
        End Try

        Return bExiste
    End Function
    Public Shared Function ChargerMouvementsSimilaires(mouvement As Mouvements) As DataTable
        Dim dataTable As New DataTable()

        Try
            ' Définir la commande SQL pour appeler la procédure stockée
            Using cmd As New SqlCommand(lectureProprietes.GetVariable("procMvtsIdentiques", False), connexionDB.GetInstance.getConnexion)
                'cmd.CommandType = CommandType.StoredProcedure

                ' Ajouter les paramètres à la commande
                cmd.Parameters.AddWithValue("@dateMvt", mouvement.DateMvt)
                cmd.Parameters.AddWithValue("@montant", CDec(mouvement.Montant))
                cmd.Parameters.AddWithValue("@sens", mouvement.Sens)

                ' Créer un DataAdapter pour remplir le DataTable
                Using adapter As New SqlDataAdapter(cmd)
                    ' Remplir le DataTable avec les données de la base de données
                    adapter.Fill(dataTable)
                End Using
            End Using

            ' Écrire un log d'information
            Logger.GetInstance.INFO("Chargement des mouvements similaires réussi.")
        Catch ex As SqlException
            ' Écrire un log d'erreur en cas d'exception SQL
            Logger.GetInstance.ERR($"Erreur SQL lors du chargement des mouvements similaires : {ex.Message}")
        Catch ex As Exception
            ' Écrire un log d'erreur en cas d'exception générale
            Logger.GetInstance.ERR($"Erreur lors du chargement des mouvements similaires : {ex.Message}")
        End Try

        Return dataTable
    End Function


    Public Shared Sub MettreAJourMouvement(categorie As String, sousCategorie As String, montant As Decimal, sens As Boolean, tiers As String, note As String, dateMvt As Date, etat As Boolean, evenement As String, type As String, modifiable As Boolean, numeroRemise As Integer?, Optional idCheque As Integer? = Nothing)
        Dim sqlConnexion As SqlConnection = Nothing
        Dim rowsAffected As Integer = 0

        Try
            ' Obtenir la connexion SQL
            sqlConnexion = connexionDB.GetInstance.getConnexion

            ' Ouvrir la connexion
            If sqlConnexion.State <> ConnectionState.Open Then
                sqlConnexion.Open()
            End If

            ' Requête SQL pour mettre à jour la table Mouvements
            'Dim query As String = "UPDATE [dbo].[Mouvements] SET [catégorie] = @Categorie, [sousCatégorie] = @SousCategorie, [montant] = @Montant, [sens] = @Sens, [tiers] = @Tiers, [note] = @Note, [dateMvt] = @DateMvt, [dateModification] = GETDATE(), [etat] = @Etat, [événement] = @Evenement, [type] = @Type, [modifiable] = @Modifiable, [numeroRemise] = @NumeroRemise"
            Dim query As String = lectureProprietes.GetVariable("updMvt", False)

            ' Ajouter la partie conditionnelle pour idCheque
            'If idCheque.HasValue Then
            '    query &= ", [idCheque] = @IdCheque"
            'Else
            '    query &= ", [idCheque] = NULL"
            'End If

            Using command As New SqlCommand(query, sqlConnexion)
                ' Ajouter les paramètres à la requête
                command.Parameters.AddWithValue("@Categorie", categorie)
                command.Parameters.AddWithValue("@SousCategorie", If(sousCategorie Is Nothing, DBNull.Value, sousCategorie))
                command.Parameters.AddWithValue("@Montant", montant)
                command.Parameters.AddWithValue("@Sens", sens)
                command.Parameters.AddWithValue("@Tiers", If(tiers Is Nothing, DBNull.Value, tiers))
                command.Parameters.AddWithValue("@Note", If(note Is Nothing, DBNull.Value, note))
                command.Parameters.AddWithValue("@DateMvt", dateMvt)
                command.Parameters.AddWithValue("@Etat", etat)
                command.Parameters.AddWithValue("@Evenement", If(evenement Is Nothing, DBNull.Value, evenement))
                command.Parameters.AddWithValue("@Type", If(type Is Nothing, DBNull.Value, type))
                command.Parameters.AddWithValue("@Modifiable", modifiable)
                command.Parameters.AddWithValue("@NumeroRemise", If(numeroRemise.HasValue, numeroRemise.Value, DBNull.Value))

                ' Ajouter le paramètre idCheque s'il est fourni
                If idCheque.HasValue Then
                    command.Parameters.AddWithValue("@IdCheque", idCheque.Value)
                End If

                ' Exécuter la requête et obtenir le nombre de lignes affectées
                rowsAffected = command.ExecuteNonQuery()
            End Using

            ' Trace indiquant le nombre de lignes mises à jour
            Logger.GetInstance().INFO($"Nombre de lignes mises à jour : {rowsAffected}")

            ' Trace indiquant les valeurs mises à jour
            Logger.GetInstance().INFO($"Valeurs mises à jour - Catégorie: {categorie}, Sous-Catégorie: {sousCategorie}, Montant: {montant}, Sens: {sens}, Tiers: {tiers}, Note: {note}, DateMvt: {dateMvt}, Etat: {etat}, Evénement: {evenement}, Type: {type}, Modifiable: {modifiable}, Numéro Remise: {numeroRemise}, IdChèque: {idCheque}")

        Catch ex As Exception
            ' Trace en cas d'erreur
            Logger.GetInstance().ERR($"Erreur lors de la mise à jour du mouvement : {ex.Message}")
        Finally
            ' Fermer la connexion si elle est ouverte
            If sqlConnexion IsNot Nothing AndAlso sqlConnexion.State = ConnectionState.Open Then
                sqlConnexion.Close()
            End If
        End Try
    End Sub


    Public Shared Sub SupprimerMouvement(id As Integer)
            Dim sqlConnexion As SqlConnection = Nothing
            Dim rowsAffected As Integer = 0

            Try
                ' Obtenir la connexion SQL
                sqlConnexion = connexionDB.GetInstance.getConnexion

                ' Ouvrir la connexion
                If sqlConnexion.State <> ConnectionState.Open Then
                    sqlConnexion.Open()
                End If

                ' Requête SQL pour supprimer l'enregistrement
                Dim query As String = "DELETE FROM [dbo].[Mouvements] WHERE [Id] = @Id"

                Using command As New SqlCommand(query, sqlConnexion)
                    ' Ajouter le paramètre Id à la requête
                    command.Parameters.AddWithValue("@Id", id)

                    ' Exécuter la requête et obtenir le nombre de lignes affectées
                    rowsAffected = command.ExecuteNonQuery()
                End Using

                ' Trace indiquant le nombre de lignes supprimées
                Logger.GetInstance().INFO($"Nombre de lignes supprimées : {rowsAffected}")

                ' Trace indiquant l'Id supprimé
                Logger.GetInstance().INFO($"Enregistrement supprimé - Id: {id}")

            Catch ex As Exception
                ' Trace en cas d'erreur
                Logger.GetInstance().ERR($"Erreur lors de la suppression du mouvement : {ex.Message}")
            Finally
                ' Fermer la connexion si elle est ouverte
                If sqlConnexion IsNot Nothing AndAlso sqlConnexion.State = ConnectionState.Open Then
                    sqlConnexion.Close()
                End If
            End Try
        End Sub

    Public Shared Function VerifParam(note As String, categorie As String, sousCategorie As String, tiers As Integer, dateMvt As Date, montant As String, sens As String, etat As String, événement As String, type As String, modifiable As Boolean, numeroRemise As String, ByVal idCheque As Integer) As Boolean
        Dim bToutEstLa As Boolean = False

        'L'idCheque est facultatif
        If categorie <> "" AndAlso sousCategorie <> "" AndAlso tiers <> 0 AndAlso IsDate(dateMvt) AndAlso sens <> "" AndAlso etat <> "" AndAlso type <> "" Then
            bToutEstLa = True
        End If
        Return bToutEstLa
    End Function
    Public Property DateCréation() As Date
        'https://learn.microsoft.com/fr-fr/dotnet/standard/base-types/regular-expressions
        Get
            Return _dateCréation
        End Get
        Private Set(ByVal value As Date)
            _dateCréation = value
        End Set
    End Property
    Public Property Note() As String
        'https://learn.microsoft.com/fr-fr/dotnet/standard/base-types/regular-expressions
        Get
            Return IIf(_note > "", _note, "Null")
        End Get
        Set(ByVal value As String)
            '_note = Trim(value)
            'Dim pattern As String = "(Mr\\.? |Mrs\\.? |Miss |Ms\\.? )"
            'Dim names() As String = {"Mr. Henry Hunt", "Ms. Sara Samuels", "Abraham Adams", "Ms. Nicole Norris"}
            'Suppression des doubles quotes
            _note = Regex.Replace(value, "(.*)""(.*)", String.Empty)
        End Set
    End Property
    Public Property Categorie() As String
        Get
            Return _categorie
        End Get
        Set(ByVal value As String)
            _categorie = Trim(value)
        End Set
    End Property
    Public Property SousCategorie() As String
        Get
            Return _sousCategorie
        End Get
        Set(ByVal value As String)
            _sousCategorie = Trim(value)
        End Set
    End Property
    Public Property Tiers() As Integer
        Get
            Return Split(_tiers, vbTab)(0)
        End Get
        Set(ByVal value As Integer)
            _tiers = value
        End Set
    End Property
    Public Property DateMvt() As Date
        Get
            Return _dateMvt
        End Get
        Set(ByVal value As Date)
            _dateMvt = value
        End Set
    End Property
    Public Property Montant() As String
        Get
            Return CDec(_montant)
        End Get
        Set(ByVal value As String)
            If Decimal.TryParse(value, vbNull) Then
                _montant = value
            Else
                _montant = "0"
            End If
        End Set
    End Property
    Public Property Sens() As String
        Get
            Return _sens
        End Get
        Set(ByVal value As String)
            _sens = Trim(value)
        End Set
    End Property
    Public Property Etat() As String
        Get
            Return _etat
        End Get
        Set(ByVal value As String)
            _etat = value
        End Set
    End Property
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
        Get
            Return _modifiable
        End Get
        Set(ByVal value As Boolean)
            _modifiable = value
        End Set
    End Property
    Public Property idCheque() As Integer
        Get
            Return _idCheque
        End Get
        Set(ByVal value As Integer)
            _idCheque = value
        End Set
    End Property
    Public Property NumeroRemise() As String
        Get
            Return CInt(_numeroRemise)
        End Get
        Set(ByVal value As String)
            Dim s As String
            s = Trim(Strings.Replace(value, """", ""))
            If Integer.TryParse(Trim(Strings.Replace(value, """", "")), vbNull) Then
                _numeroRemise = Trim(Strings.Replace(value, """", ""))
            Else
                _numeroRemise = "0"
            End If
        End Set
    End Property
    Public Function ObtenirValeursConcatenees() As String
        ' Initialiser une chaîne vide pour stocker le résultat
        Dim resultat As String = ""

        ' Concaténer le nom de chaque variable avec sa valeur
        resultat &= "Note: " & _note & ", "
        resultat &= "Catégorie: " & _categorie & ", "
        resultat &= "Sous-Catégorie: " & _sousCategorie & ", "
        resultat &= "Tiers: " & _tiers.ToString() & ", "
        resultat &= "Date de Création: " & _dateCréation.ToString("yyyy-MM-dd") & ", "
        resultat &= "Date de Mouvement: " & _dateMvt.ToString("yyyy-MM-dd") & ", "
        resultat &= "Montant: " & _montant.ToString() & ", "
        resultat &= "Sens: " & _sens & ", "
        resultat &= "État: " & _etat & ", "
        resultat &= "Événement: " & _événement & ", "
        resultat &= "Type: " & _type & ", "
        resultat &= "Modifiable: " & _modifiable.ToString() & ", "
        resultat &= "Numéro de Remise: " & _numeroRemise & ", "
        resultat &= "ID Chèque: " & _idCheque.ToString() & vbCrLf

        ' Retourner la chaîne concaténée
        Return resultat
    End Function
End Class
