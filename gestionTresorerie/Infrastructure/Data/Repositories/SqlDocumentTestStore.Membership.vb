Option Strict On
Imports System.Data
Imports System.Data.SqlClient
Imports Newtonsoft.Json.Linq

Partial Public NotInheritable Class SqlDocumentTestStore
    Private Shared ReadOnly IdentityColumns As String() = {"nom", "prenom", "nom_usage", "genre", "adresse", "code_postal", "ville", "mail", "telephone"}

    Private Shared Function ReadMember(connection As SqlConnection, tx As SqlTransaction, fields As JObject) As JObject
        Const sql As String = "SELECT t.Id AS tiersId,c.Id AS coordId,t.nom,t.prenom,t.nomUsage AS nom_usage,t.genre,c.Rue1 AS adresse," &
            "c.CodePostal AS code_postal,c.Ville AS ville,c.Email AS mail,c.Telephone AS telephone " &
            "FROM dbo.Tiers t LEFT JOIN dbo.Coordonnees c ON c.IdTiers=t.Id " &
            "WHERE LTRIM(RTRIM(t.nom)) COLLATE Latin1_General_100_CI_AI=@nom AND LTRIM(RTRIM(t.prenom)) COLLATE Latin1_General_100_CI_AI=@prenom;"
        Using cmd As New SqlCommand(sql, connection, tx)
            cmd.Parameters.AddWithValue("@nom", fields.Value(Of String)("nom"))
            cmd.Parameters.AddWithValue("@prenom", fields.Value(Of String)("prenom"))
            Using reader = cmd.ExecuteReader()
                Dim result As New JObject()
                If reader.Read() Then
                    For i = 0 To reader.FieldCount - 1
                        If Not reader.IsDBNull(i) Then result(reader.GetName(i)) = JToken.FromObject(reader.GetValue(i))
                    Next
                    If reader.Read() Then Throw New InvalidOperationException("Plusieurs tiers ou coordonnées correspondent à ce nom et prénom. Vérifiez les fiches avant de classer le bulletin.")
                End If
                Return result
            End Using
        End Using
    End Function

    Private Shared Function MemberChanges(current As JObject, fields As JObject) As String
        Dim lines As New List(Of String)
        For Each key In IdentityColumns
            Dim after = fields.Value(Of String)(key)
            Dim before = current.Value(Of String)(key)
            If Not String.IsNullOrWhiteSpace(after) AndAlso Not String.Equals(before, after, StringComparison.Ordinal) Then
                lines.Add(key & " : " & If(before, "(vide)") & " → " & after)
            End If
        Next
        Return String.Join(Environment.NewLine, lines)
    End Function

    Private Shared Function WriteMember(connection As SqlConnection, tx As SqlTransaction, current As JObject, fields As JObject, apply As Boolean) As Integer
        Dim id = current.Value(Of Integer?)("tiersId")
        If id.HasValue AndAlso Not apply Then Return id.Value
        Using cmd As New SqlCommand("", connection, tx)
            For Each key In IdentityColumns
                Dim value = fields.Value(Of String)(key)
                cmd.Parameters.AddWithValue("@" & key, If(String.IsNullOrWhiteSpace(value), CType(DBNull.Value, Object), value))
            Next
            If Not id.HasValue Then
                cmd.CommandText = "INSERT dbo.Tiers(nom,prenom,nomUsage,genre,dateCreation,dateModification) OUTPUT inserted.Id VALUES(@nom,@prenom,@nom_usage,@genre,GETDATE(),GETDATE());"
                id = CInt(cmd.ExecuteScalar())
            Else
                cmd.Parameters.AddWithValue("@id", id.Value)
                cmd.CommandText = "UPDATE dbo.Tiers SET nom=@nom,prenom=@prenom,nomUsage=COALESCE(@nom_usage,nomUsage),genre=COALESCE(@genre,genre),dateModification=GETDATE() WHERE Id=@id;"
                cmd.ExecuteNonQuery()
            End If
            If IdentityColumns.Skip(4).Any(Function(key) Not String.IsNullOrWhiteSpace(fields.Value(Of String)(key))) Then
                Dim coord = current.Value(Of Integer?)("coordId")
                If coord.HasValue Then
                    cmd.Parameters.AddWithValue("@coord", coord.Value)
                    cmd.CommandText = "UPDATE dbo.Coordonnees SET Rue1=COALESCE(@adresse,Rue1),CodePostal=COALESCE(@code_postal,CodePostal),Ville=COALESCE(@ville,Ville),Email=COALESCE(@mail,Email),Telephone=COALESCE(@telephone,Telephone) WHERE Id=@coord;"
                Else
                    cmd.Parameters.AddWithValue("@tiers", id.Value)
                    cmd.CommandText = "INSERT dbo.Coordonnees(IdTiers,TypeAdresse,Rue1,CodePostal,Ville,Email,Telephone) VALUES(@tiers,N'Domicile',@adresse,@code_postal,@ville,@mail,@telephone);"
                End If
                cmd.ExecuteNonQuery()
            End If
            Return id.Value
        End Using
    End Function

    Public Function SaveVerified(proposal As DocumentArchiveProposal, approve As Func(Of String, Boolean?)) As Integer?
        Return SaveReviewed(proposal, approve, Nothing, Nothing)
    End Function

    Friend Function SaveReviewed(proposal As DocumentArchiveProposal, approve As Func(Of String, Boolean?), revue As RevueTiers, lu As JObject, Optional production As Boolean=False) As Integer?
        Dim tiersValide As Integer?
        Dim member = proposal.Document.CategorieDoc = "FormulaireAdhesion"
        Dim fields As JObject = Nothing
        Dim snapshot As JObject = Nothing
        Dim apply As Boolean = False
        If production AndAlso (revue Is Nothing OrElse proposal.IdCategorie.HasValue OrElse proposal.MouvementChoisi.HasValue OrElse Not String.IsNullOrEmpty(proposal.EvenementComptable) OrElse Not String.IsNullOrEmpty(proposal.NatureComptable)) Then Throw New InvalidOperationException("Proposition de classement PROD incompatible.")
        Using connection = If(production,ParametresComptables.Ouvrir(False),OpenTest())
            If member AndAlso revue Is Nothing Then
                fields = JObject.Parse(proposal.Document.metaDonnees)
                If String.IsNullOrWhiteSpace(fields.Value(Of String)("nom")) OrElse String.IsNullOrWhiteSpace(fields.Value(Of String)("prenom")) Then Throw New ArgumentException("Nom et prénom requis.")
                snapshot = ReadMember(connection, Nothing, fields)
                Dim delta = MemberChanges(snapshot, fields)
                If snapshot.HasValues AndAlso delta.Length > 0 Then
                    Dim answer = approve(delta)
                    If Not answer.HasValue Then Return Nothing
                    apply = answer.Value
                End If
            End If
            Using tx = connection.BeginTransaction(IsolationLevel.Serializable)
                Using guard As New SqlCommand(If(production,"IF DB_NAME()<>N'bddAgumaaa' THROW 51202,N'Catalogue PROD inattendu.',1;",GuardSql & MigrationGuardSql), connection, tx)
                    guard.ExecuteNonQuery()
                End Using
                If member OrElse revue IsNot Nothing Then
                    Using triggers As New SqlCommand("IF EXISTS(SELECT 1 FROM sys.triggers WHERE is_disabled=0 AND parent_id IN (OBJECT_ID('dbo.Tiers'),OBJECT_ID('dbo.Coordonnees'))) THROW 51008,N'Déclencheur inattendu sur les fiches tiers.',1;", connection, tx)
                        triggers.ExecuteNonQuery()
                    End Using
                End If
                If revue IsNot Nothing Then
                    tiersValide = revue.Enregistrer(connection,tx,lu)
                    Dim meta = JObject.Parse(proposal.Document.metaDonnees)
                    meta("idTiers")=tiersValide.Value
                    proposal.Document.metaDonnees=meta.ToString(Newtonsoft.Json.Formatting.None)
                ElseIf member Then
                    Dim current = ReadMember(connection, tx, fields)
                    If Not JToken.DeepEquals(current, snapshot) Then Throw New InvalidOperationException("La fiche tiers a changé. Rouvrez le document pour vérifier les nouvelles valeurs.")
                    fields("idTiers") = WriteMember(connection, tx, current, fields, apply)
                    proposal.Document.metaDonnees = fields.ToString(Newtonsoft.Json.Formatting.None)
                End If
                Dim docId As Integer
                proposal.Confirm(Sub(doc)
                    Using cmd As New SqlCommand(DocumentRepository.ArchiveInsertSql &
                        If(member OrElse revue IsNot Nothing OrElse doc.CategorieDoc = "QuestionnaireMedical",
                           " UPDATE dbo.Documents SET dateModif=GETDATE(), metaDonnees=@metaDonnees WHERE cheminDoc=@cheminDoc AND (metaDonnees IS NULL OR metaDonnees COLLATE Latin1_General_100_BIN2 <> @metaDonnees);", "") &
                        " SELECT idDoc FROM dbo.Documents WHERE cheminDoc=@cheminDoc;", connection, tx)
                        cmd.Parameters.AddRange(DocumentRepository.InsertionParameters(doc).ToArray())
                        docId = CInt(cmd.ExecuteScalar())
                        If docId <= 0 Then Throw New InvalidOperationException("Écriture du document non confirmée.")
                    End Using
                End Sub)
                If revue IsNot Nothing Then revue.Rattacher(connection,tx,docId,tiersValide.Value)
                If Not proposal.MouvementChoisi.HasValue AndAlso proposal.IdCategorie.HasValue Then
                    EnregistrerMouvement(connection, tx, proposal, docId, tiersValide)
                End If
                If proposal.MouvementChoisi.HasValue Then
                    Using cmd As New SqlCommand("IF NOT EXISTS(SELECT 1 FROM dbo.Mouvements WHERE Id=@mvt) THROW 51030,N'Mouvement introuvable.',1; UPDATE dbo.Documents SET idMvtDoc=@mvt,dateModif=GETDATE() WHERE idDoc=@doc AND (idMvtDoc IS NULL OR idMvtDoc=0 OR idMvtDoc=@mvt); IF @@ROWCOUNT<>1 THROW 51031,N'Ce document est déjà rattaché à un autre mouvement.',1;", connection, tx)
                        cmd.Parameters.AddWithValue("@mvt", proposal.MouvementChoisi.Value)
                        cmd.Parameters.AddWithValue("@doc", docId)
                        cmd.ExecuteNonQuery()
                    End Using
                End If
                If Not String.IsNullOrWhiteSpace(proposal.NatureComptable) OrElse Not String.IsNullOrWhiteSpace(proposal.EvenementComptable) Then
                    Using cmd As New SqlCommand("UPDATE dbo.ClassementDocumentTest SET nature=@nature,evenement=@event,idCategorie=@cat,idSousCategorie=@sous WHERE idDoc=@doc; IF @@ROWCOUNT=0 INSERT dbo.ClassementDocumentTest(idDoc,nature,evenement,idCategorie,idSousCategorie) VALUES(@doc,@nature,@event,@cat,@sous);", connection, tx)
                        cmd.Parameters.AddWithValue("@doc", docId)
                        cmd.Parameters.Add("@cat", SqlDbType.Int).Value = If(proposal.IdCategorie.HasValue, CObj(proposal.IdCategorie.GetValueOrDefault()), DBNull.Value)
                        cmd.Parameters.Add("@sous", SqlDbType.Int).Value = If(proposal.IdSousCategorie.HasValue, CObj(proposal.IdSousCategorie.GetValueOrDefault()), DBNull.Value)
                        cmd.Parameters.AddWithValue("@nature", If(proposal.NatureComptable, ""))
                        cmd.Parameters.AddWithValue("@event", If(proposal.EvenementComptable, ""))
                        cmd.ExecuteNonQuery()
                    End Using
                End If
                tx.Commit()
                Return docId
            End Using
        End Using
    End Function
    Private Shared Sub EnregistrerMouvement(cn As SqlConnection, tx As SqlTransaction, p As DocumentArchiveProposal, idDoc As Integer, tiersValide As Integer?)
        Dim cheque = TryCast(p.Document, Cheque)
        Dim facture = TryCast(p.Document, Facture)
        If cheque Is Nothing AndAlso facture Is Nothing Then Return
        Dim credit As Boolean
        Dim montant As Decimal
        Dim reference, tiers As String
        If cheque IsNot Nothing Then
            credit = p.Document.SousCategorieDoc = "Reçus"
            montant = cheque.montant_numerique
            reference = DocumentArchiveProposal.NumeroCheque(cheque.numero_du_cheque)
            tiers = If(credit, cheque.emetteur_du_cheque, cheque.destinataire)
        Else
            credit = facture.fournisseur_nom.Contains("AGUMAAA", StringComparison.OrdinalIgnoreCase)
            montant = facture.montant_ttc
            reference = facture.numero_facture
            tiers = If(credit, facture.adherent_nom, facture.fournisseur_nom)
        End If
        Const sql = "DECLARE @m int=(SELECT NULLIF(idMvtDoc,0) FROM dbo.Documents WITH(UPDLOCK,HOLDLOCK) WHERE idDoc=@doc); " &
            "IF @m IS NOT NULL RETURN; " &
            "IF NOT EXISTS(SELECT 1 FROM dbo.Categorie WHERE Id=@cat AND debit=1-CONVERT(int,@credit)) OR (@sous IS NOT NULL AND NOT EXISTS(SELECT 1 FROM dbo.SousCategorie WHERE Id=@sous AND idCategorie=@cat)) THROW 51143,N'Classement incompatible avec le sens du mouvement.',1; " &
            "IF EXISTS(SELECT 1 FROM dbo.Mouvements WITH(UPDLOCK,HOLDLOCK) WHERE typeMouvement=@type AND reference=@ref AND montant=@montant AND sens=@credit AND dateMvt=@date AND note=@tiers) THROW 51144,N'Un mouvement correspondant existe déjà. Rattachez-le au document.',1; " &
            "DECLARE @t int=(SELECT CASE WHEN COUNT(*)=1 THEN MIN(Id) ELSE 0 END FROM dbo.Tiers WHERE NULLIF(@tiers,'') IN (raisonSociale,TRIM(CONCAT(prenom,' ',nom)),TRIM(CONCAT(nom,' ',prenom)))); " &
            "INSERT dbo.Mouvements(categorie,sousCategorie,montant,sens,tiers,note,dateMvt,dateCreation,etat,modifiable,evenement,typeMouvement,reference,typeReference,idDoc) VALUES(@cat,@sous,@montant,@credit,COALESCE(@tiersValide,@t),@tiers,@date,GETDATE(),0,1,@event,@type,@ref,@type,@doc); " &
            "UPDATE dbo.Documents SET idMvtDoc=CONVERT(int,SCOPE_IDENTITY()),dateModif=GETDATE() WHERE idDoc=@doc;"
        Using cmd As New SqlCommand(sql, cn, tx)
            cmd.Parameters.AddWithValue("@doc", idDoc)
            cmd.Parameters.AddWithValue("@cat", p.IdCategorie.Value)
            cmd.Parameters.Add("@sous", SqlDbType.Int).Value = If(p.IdSousCategorie.HasValue, CObj(p.IdSousCategorie.GetValueOrDefault()), DBNull.Value)
            cmd.Parameters.AddWithValue("@montant", Math.Abs(montant))
            cmd.Parameters.AddWithValue("@credit", credit)
            cmd.Parameters.AddWithValue("@tiers", If(tiers, "").Trim())
            cmd.Parameters.AddWithValue("@date", p.Document.DateDoc)
            cmd.Parameters.AddWithValue("@event", If(p.EvenementComptable, ""))
            cmd.Parameters.AddWithValue("@type", If(cheque Is Nothing, "Facture", "Chèque"))
            cmd.Parameters.AddWithValue("@ref", reference)
            cmd.ExecuteNonQuery()
        End Using
    End Sub
End Class
