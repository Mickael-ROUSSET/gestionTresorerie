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
End Class
