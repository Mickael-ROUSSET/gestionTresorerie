Option Strict On
Imports System.Data
Imports System.Data.SqlClient
Imports System.Globalization
Imports System.Text
Imports Newtonsoft.Json.Linq

Friend Class RevueTiers
    Friend Shared ReadOnly Champs As String() = {"nom","prenom","raisonSociale","nom_usage","genre","adresse","complement","code_postal","ville","pays","mail","telephone"}
    Friend Shared ReadOnly Libelles As String() = {"Nom","Prénom","Raison sociale","Nom d’usage","Genre","Rue","Complément","Code postal","Ville","Pays","Courriel","Téléphone"}
    Friend Property Avant As DataRow
    Friend Property Valeurs As New JObject()
    Friend Property Activite As String
    Friend Property Annee As Integer
    Friend Property RoleCours As String
    Friend Property Debut As Date?
    Friend Property Fin As Date?
    Friend Property CandidatsInitiaux As Integer()

    Friend Shared Function Normaliser(value As String) As String
        Return New String(System.Text.RegularExpressions.Regex.Replace(value.Trim(), "\s+", " ").Normalize(NormalizationForm.FormD).Where(Function(c) CharUnicodeInfo.GetUnicodeCategory(c) <> UnicodeCategory.NonSpacingMark).ToArray()).ToUpperInvariant()
    End Function

    Friend Shared Function Correspond(row As DataRow, lu As JObject) As Boolean
        Dim noms = {String.Join(" ", {CStr(lu("prenom")), CStr(lu("nom"))}).Trim(), CStr(lu("raisonSociale")), CStr(lu("nom")) & " " & CStr(lu("prenom"))}
        Dim enBase = {row("raisonSociale").ToString(), row("prenom").ToString() & " " & row("nom").ToString(), row("nom").ToString() & " " & row("prenom").ToString()}
        Return noms.Any(Function(n) Not String.IsNullOrWhiteSpace(n) AndAlso enBase.Any(Function(b) Normaliser(b)=Normaliser(n)))
    End Function

    Friend Shared Function Lire(cn As SqlConnection, Optional tx As SqlTransaction=Nothing) As DataTable
        Using cmd As New SqlCommand("SELECT * FROM dbo.VueTiersCoordonnees ORDER BY nom,prenom,raisonSociale,tiersId,coordId",cn,tx), reader=cmd.ExecuteReader()
            Dim table As New DataTable()
            table.Load(reader)
            Return table
        End Using
    End Function

    Friend Shared Function Extraire(doc As DocumentAgumaaa, analyse As JObject) As JObject
        Dim identite=TryCast(analyse?("donnees_extraites")?("identite"), JObject)
        If doc.CategorieDoc="FormulaireAdhesion" Then
            Dim membre=JObject.Parse(doc.metaDonnees)
            Dim organisme=identite?("raisonSociale")?.ToString()
            If Not String.IsNullOrWhiteSpace(organisme) Then
                membre("raisonSociale")=organisme
                If membre.Value(Of String)("nom")=organisme Then membre.Remove("nom")
            End If
            Return membre
        End If
        Dim facture=DirectCast(doc,Facture)
        Dim emise=Normaliser(If(facture.fournisseur_nom,"")).Contains("AGUMAAA")
        Dim res As New JObject()
        Dim prefix=If(emise,"destinataire_","fournisseur_")
        For Each champ In Champs
            Dim v=identite?(prefix & champ)?.ToString()
            If Not String.IsNullOrWhiteSpace(v) Then res(champ)=v
        Next
        If emise Then
            If res("nom") Is Nothing AndAlso res("raisonSociale") Is Nothing Then res("nom")=If(identite?("adherent_nom")?.ToString(),facture.adherent_nom)
        Else
            If res("prenom") Is Nothing Then
                res.Remove("nom")
                res("raisonSociale")=facture.fournisseur_nom
            End If
        End If
        Return res
    End Function

    Friend Function Enregistrer(cn As SqlConnection,tx As SqlTransaction, lu As JObject) As Integer
        Dim table=Lire(cn,tx)
        Dim id As Integer
        If Avant Is Nothing Then
            Dim ids=table.AsEnumerable().Where(Function(r) Correspond(r,lu)).Select(Function(r) CInt(r("tiersId"))).Distinct().OrderBy(Function(n) n)
            If Not ids.SequenceEqual(CandidatsInitiaux) Then Throw New InvalidOperationException("Les tiers correspondants ont changé. Reprenez la validation.")
        Else
            Dim actuel=table.AsEnumerable().FirstOrDefault(Function(r) Equals(r("tiersId"),Avant("tiersId")) AndAlso Equals(r("coordId"),Avant("coordId")))
            If actuel Is Nothing OrElse Not actuel.ItemArray.SequenceEqual(Avant.ItemArray) Then Throw New InvalidOperationException("Les coordonnées ont changé. Reprenez la validation.")
            id=CInt(Avant("tiersId"))
        End If
        Using cmd As New SqlCommand("",cn,tx)
            For Each champ In Champs
                Dim valeur=CStr(Valeurs(champ))
                cmd.Parameters.AddWithValue("@" & champ,If(String.IsNullOrWhiteSpace(valeur),CObj(DBNull.Value),valeur.Trim()))
            Next
            cmd.Parameters.AddWithValue("@id",id)
            If id=0 Then
                cmd.CommandText="INSERT dbo.Tiers(nom,prenom,raisonSociale,nomUsage,genre,dateCreation,dateModification) OUTPUT inserted.Id VALUES(@nom,@prenom,@raisonSociale,@nom_usage,@genre,GETDATE(),GETDATE())"
                id=CInt(cmd.ExecuteScalar())
                cmd.Parameters("@id").Value=id
            ElseIf Champs.Take(5).Any(Function(k) Avant(k).ToString()<>CStr(Valeurs(k))) Then
                cmd.CommandText="UPDATE dbo.Tiers SET nom=@nom,prenom=@prenom,raisonSociale=@raisonSociale,nomUsage=@nom_usage,genre=@genre,dateModification=GETDATE() WHERE Id=@id"
                cmd.ExecuteNonQuery()
            End If
            If Champs.Skip(5).Any(Function(k) If(Avant Is Nothing,"",Avant(k).ToString())<>CStr(Valeurs(k))) Then
                If Avant Is Nothing OrElse Avant.IsNull("coordId") Then
                    cmd.CommandText="INSERT dbo.Coordonnees(IdTiers,Rue1,Rue2,CodePostal,Ville,Pays,Email,Telephone) VALUES(@id,@adresse,@complement,@code_postal,@ville,@pays,@mail,@telephone)"
                Else
                    cmd.Parameters.AddWithValue("@coord",Avant("coordId"))
                    cmd.CommandText="UPDATE dbo.Coordonnees SET Rue1=@adresse,Rue2=@complement,CodePostal=@code_postal,Ville=@ville,Pays=@pays,Email=@mail,Telephone=@telephone WHERE Id=@coord AND IdTiers=@id"
                End If
                cmd.ExecuteNonQuery()
            End If
        End Using
        Return id
    End Function

    Friend Sub Rattacher(cn As SqlConnection,tx As SqlTransaction,docId As Integer,tiersId As Integer)
        Using cmd As New SqlCommand("IF EXISTS(SELECT 1 FROM dbo.DocumentTiers WHERE DocumentId=@doc AND TiersId<>@tiers) THROW 51200,N'Document déjà lié à un autre tiers.',1; IF NOT EXISTS(SELECT 1 FROM dbo.DocumentTiers WHERE DocumentId=@doc) INSERT dbo.DocumentTiers VALUES(@doc,@tiers);",cn,tx)
            cmd.Parameters.AddWithValue("@doc",docId)
            cmd.Parameters.AddWithValue("@tiers",tiersId)
            cmd.ExecuteNonQuery()
            If String.IsNullOrWhiteSpace(Activite) Then Return
            cmd.Parameters.AddWithValue("@activite",Activite)
            cmd.Parameters.AddWithValue("@annee",Annee)
            cmd.Parameters.AddWithValue("@role",RoleCours)
            cmd.Parameters.Add("@debut",SqlDbType.Date).Value=If(Debut.HasValue,CObj(Debut.GetValueOrDefault()),DBNull.Value)
            cmd.Parameters.Add("@fin",SqlDbType.Date).Value=If(Fin.HasValue,CObj(Fin.GetValueOrDefault()),DBNull.Value)
            cmd.CommandText="DECLARE @a int=(SELECT Id FROM dbo.ActiviteAssociation WHERE Libelle=@activite); " &
                "IF @a IS NULL BEGIN INSERT dbo.ActiviteAssociation VALUES(@activite); SET @a=SCOPE_IDENTITY(); END; " &
                "DECLARE @e int=(SELECT Id FROM dbo.EditionActivite WHERE ActiviteId=@a AND Annee=@annee); " &
                "IF @e IS NULL BEGIN INSERT dbo.EditionActivite VALUES(@a,@annee,@debut,@fin); SET @e=SCOPE_IDENTITY(); END ELSE BEGIN " &
                "IF EXISTS(SELECT 1 FROM dbo.EditionActivite WHERE Id=@e AND (Debut<>@debut OR Fin<>@fin)) THROW 51201,N'Dates incompatibles avec cette édition.',1; " &
                "UPDATE dbo.EditionActivite SET Debut=COALESCE(Debut,@debut),Fin=COALESCE(Fin,@fin) WHERE Id=@e; END; " &
                "DECLARE @p int=(SELECT Id FROM dbo.Participation WHERE TiersId=@tiers AND EditionId=@e AND RoleCours=@role); " &
                "IF @p IS NULL BEGIN INSERT dbo.Participation VALUES(@tiers,@e,@role); SET @p=SCOPE_IDENTITY(); END; " &
                "IF NOT EXISTS(SELECT 1 FROM dbo.JustificatifParticipation WHERE DocumentId=@doc AND ParticipationId=@p) INSERT dbo.JustificatifParticipation VALUES(@doc,@p,@tiers);"
            cmd.ExecuteNonQuery()
        End Using
    End Sub
End Class
