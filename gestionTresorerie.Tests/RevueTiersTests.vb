Option Strict On
Imports System.Data
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports Newtonsoft.Json.Linq

<TestClass>
Public Class RevueTiersTests
    <TestMethod>
    Public Sub RapprocheNomAccentsEtDeuxAdressesDuMemeTiers()
        Dim table As New DataTable()
        For Each champ In {"nom","prenom","raisonSociale","adresse"}
            table.Columns.Add(champ)
        Next
        table.Rows.Add("Noël","Élise",Nothing,"Rue A")
        table.Rows.Add("Noël","Élise",Nothing,"Rue B")
        table.Rows.Add("Noël","Jean",Nothing,"Rue C")
        Dim lu=JObject.Parse("{'nom':' NOEL ','prenom':'Elise'}")
        Assert.AreEqual(2,table.AsEnumerable().Count(Function(r) RevueTiers.Correspond(r,lu)))
    End Sub

    <TestMethod>
    Public Sub FactureEmiseProposeLesCoordonneesDuDestinataire()
        Dim facture As New Facture With {.fournisseur_nom="AGUMAAA"}
        Dim resultat=RevueTiers.Extraire(facture,JObject.Parse("{'donnees_extraites':{'identite':{'destinataire_nom':'Martin','destinataire_mail':'client@example.invalid','fournisseur_mail':'asso@example.invalid'}}}"))
        Assert.AreEqual("Martin",resultat.Value(Of String)("nom"))
        Assert.AreEqual("client@example.invalid",resultat.Value(Of String)("mail"))
    End Sub
    <TestMethod, TestCategory("LocalDbIntegration")>
    Public Sub DeuxCoursEtDeuxJustificatifsComptentUneSeulePersonneParAnnee()
        If Environment.GetEnvironmentVariable("AGUMAAA_RUN_LOCALDB_INTEGRATION")<>"1" Then Assert.Inconclusive()
        Dim store As New SqlDocumentTestStore()
        store.Initialize()
        Using cn=SqlDocumentTestStore.OpenTest(), tx=cn.BeginTransaction(IsolationLevel.Serializable)
            Dim lu=JObject.Parse("{'nom':'Participant fictif'}")
            lu("nom")=Guid.NewGuid().ToString("N")
            Dim revue As New RevueTiers With {.Valeurs=lu,.CandidatsInitiaux=Array.Empty(Of Integer)(),.Activite="Gym fictive",.Annee=2026,.RoleCours="Douce"}
            Dim tiers=revue.Enregistrer(cn,tx,lu)
            Using cmd As New System.Data.SqlClient.SqlCommand("INSERT dbo.Documents(dateDoc,cheminDoc) OUTPUT inserted.idDoc VALUES(GETDATE(),N'fictif')",cn,tx)
                Dim doc1=CInt(cmd.ExecuteScalar())
                Dim doc2=CInt(cmd.ExecuteScalar())
                revue.Rattacher(cn,tx,doc1,tiers)
                revue.Rattacher(cn,tx,doc1,tiers)
                revue.Rattacher(cn,tx,doc2,tiers)
                revue.RoleCours="Tonique"
                revue.Rattacher(cn,tx,0,tiers)
                cmd.Parameters.AddWithValue("@t",tiers)
                For Each attendu In New Dictionary(Of String,Integer) From {{"VueParticipantsActivite",2},{"VueParticipantsAnnee",1},{"JustificatifParticipation",2}}
                    cmd.CommandText="SELECT COUNT(*) FROM dbo." & attendu.Key & " WHERE TiersId=@t"
                    Assert.AreEqual(attendu.Value,CInt(cmd.ExecuteScalar()),attendu.Key)
                Next
            End Using
            tx.Rollback()
        End Using
    End Sub
End Class
