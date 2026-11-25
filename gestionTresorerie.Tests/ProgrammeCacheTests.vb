Imports System.IO
Imports DocumentFormat.OpenXml.Packaging
Imports DocumentFormat.OpenXml.Wordprocessing
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports Newtonsoft.Json.Linq
Imports gestionTresorerie

<TestClass>
Public Class ProgrammeCacheTests
    <TestMethod>
    Public Sub LitLaCleTmdbSystemeMemeSiLeProcessusAUneAncienneValeur()
        Dim source As String=Nothing
        Dim cle = AccesTmdb.LireEnvironnement(
            Function(nom, cible)
                If nom="TMDB_READ_ACCESS_TOKEN" Then Return "autre-jeton-fictif"
                If nom <> "cleApiTmdb" Then Return Nothing
                Select Case cible
                    Case EnvironmentVariableTarget.Machine
                        Return " cle-fictive-systeme "
                    Case EnvironmentVariableTarget.Process
                        Return "ancienne-cle-fictive"
                    Case Else
                        Return Nothing
                End Select
            End Function,source)
        Assert.AreEqual("cleApiTmdb (Système Windows)",source)
        Assert.AreEqual("cle-fictive-systeme", cle)
    End Sub

    Private Class ReponseTmdb
        Inherits System.Net.Http.HttpMessageHandler
        Friend Refus As Boolean
        Protected Overrides Function SendAsync(req As System.Net.Http.HttpRequestMessage,annulation As System.Threading.CancellationToken) As Task(Of System.Net.Http.HttpResponseMessage)
            Assert.AreEqual("Bearer",req.Headers.Authorization.Scheme)
            Assert.IsFalse(req.RequestUri.Query.Contains("api_key"))
            Dim recherche=req.RequestUri.AbsolutePath.EndsWith("/search/movie")
            Dim json=If(recherche,"{'results':[{'id':1,'title':'Autre film'},{'id':8290,'title':'La Grande Vadrouille','overview':'Synopsis français de test'}]}","{'title':'La Grande Vadrouille','overview':''}")
            If Not recherche Then Assert.IsTrue(req.RequestUri.AbsolutePath.EndsWith("/8290"))
            Return Task.FromResult(New System.Net.Http.HttpResponseMessage(If(Refus,System.Net.HttpStatusCode.Unauthorized,System.Net.HttpStatusCode.OK)) With {.Content=New System.Net.Http.StringContent(If(Refus,"{'status_code':7}",json))})
        End Function
    End Class

    <TestMethod>
    Public Async Function RetrouveLaGrandeVadrouilleEtDistingueUnRefusTmdb() As Task
        Dim serveur As New ReponseTmdb()
        Using http As New System.Net.Http.HttpClient(serveur)
            Dim service As New TMDbService("jeton-fictif",http)
            Dim fiche=Await service.FicheAsync("la grande vadrouille")
            Assert.AreEqual("Synopsis français de test",fiche.Synopsis)
            serveur.Refus=True
            Dim refuse As Boolean
            Try
                Await service.FicheAsync("la grande vadrouille")
            Catch ex As InvalidOperationException
                refuse=ex.Message.Contains("HTTP 401") AndAlso ex.Message.Contains("TMDb 7") AndAlso Not ex.Message.Contains("jeton-fictif")
            End Try
            Assert.IsTrue(refuse)
        End Using
    End Function

    <TestMethod>
    Public Sub ProgrammePublicNeDivulguePasDeCheminLocal()
        Dim film As New FilmSeance With {.Titre="Film fictif",.DateDiffusion=New Date(2026,12,11),.HeureDiffusion=New TimeSpan(20,30,0),.UrlAffiche="C:\prive\affiche.jpg"}
        Dim obj=JObject.Parse(PublicationCinema.Programme({film}))
        Dim seance=obj("seances")(0)
        Assert.AreEqual("20:30",seance("heure").ToString())
        Assert.AreEqual("2026-12-11",seance("date").ToString())
        Assert.IsNull(seance("affiche"))
    End Sub

    <STATestMethod>
    Public Sub ProgrammeDemandeTroisColonnes()
        Using f As New FrmProgrammeCinema()
            Dim grille=f.Controls.OfType(Of System.Windows.Forms.DataGridView)().Single()
            CollectionAssert.AreEqual({"Film","Date (jj/mm/aaaa)","Heure (20h30)"},grille.Columns.Cast(Of System.Windows.Forms.DataGridViewColumn)().Select(Function(c) c.HeaderText).ToArray())
        End Using
    End Sub

    <TestMethod>
    Public Sub InventaireConserveLesFichiersDistinctsAvantAnalyse()
        Dim dossier=Directory.CreateTempSubdirectory().FullName
        Try
            Dim a=Path.Combine(dossier,"a.pdf"), copie=Path.Combine(dossier,"copie.pdf")
            Dim autre=Path.Combine(Directory.CreateDirectory(Path.Combine(dossier,"sous")).FullName,"a.pdf")
            File.WriteAllText(a,"même contenu")
            File.Copy(a,copie)
            File.WriteAllText(autre,"autre contenu")
            Dim lot=GestionDoublons.Inventorier({copie,autre,a,Path.Combine(dossier,"absent.pdf")})
            CollectionAssert.AreEqual({a,autre},lot.Fichiers.ToArray())
            Assert.AreEqual(1,lot.Doublons)
            Assert.AreEqual(1,lot.Erreurs.Count)
            Assert.IsTrue(File.Exists(copie))
        Finally
            Directory.Delete(dossier,True)
        End Try
    End Sub

    <TestMethod>
    Public Async Function ReprendLeResultatEtRepareUneEntreeIllisible() As Task
        Dim dossier=Directory.CreateTempSubdirectory().FullName
        Try
            Dim appels As Integer
            Dim lire As Func(Of Task(Of JObject))=Function()
                appels+=1
                Return Task.FromResult(JObject.Parse("{'type_document':'Cheque','donnees_extraites':{'identite':{},'transaction':{}}}"))
            End Function
            Dim cle=New String("A"c,64)
            For i=1 To 2
                Dim resultat=Await CacheAnalyse.Recuperer(dossier,cle,lire,AddressOf ChatGptDocumentReader.ParseResult)
                Assert.AreEqual("Cheque",CStr(resultat("type_document")))
            Next
            Assert.AreEqual(1,appels)
            File.WriteAllText(Directory.GetFiles(dossier).Single(),"invalide")
            Await CacheAnalyse.Recuperer(dossier,cle,lire,AddressOf ChatGptDocumentReader.ParseResult)
            Await CacheAnalyse.Recuperer(dossier,New String("B"c,64),lire,AddressOf ChatGptDocumentReader.ParseResult)
            Assert.AreEqual(3,appels)
        Finally
            Directory.Delete(dossier,True)
        End Try
    End Function

    <TestMethod>
    Public Async Function ProgrammeWordContientSeanceEtNotice() As Task
        Dim dossier=Directory.CreateTempSubdirectory().FullName
        Try
            Dim sortie=Path.Combine(dossier,"programme.docx")
            Dim films As New List(Of FilmSeance) From {New FilmSeance With {.Titre="Film fictif",.DateDiffusion=New Date(2026,12,18),.HeureDiffusion=New TimeSpan(20,30,0),.Synopsis="Synopsis fictif",.Genre="Drame",.Pays="France",.DureeMinutes=100,.Realisateur="Réalisatrice fictive",.Casting=New List(Of String) From {"Acteur fictif"}}}
            Assert.IsTrue(Await New ProgrammeCinemaCreator().GenererDocxAsync(films,sortie,"Adulte 7 €"))
            Using doc=WordprocessingDocument.Open(sortie,False)
                Dim corps=doc.MainDocumentPart.Document.Body
                For Each valeur In {"vendredi 18 décembre 20h30","Film fictif","Synopsis fictif","Adulte 7 €","décembre 2026"}
                    StringAssert.Contains(corps.InnerText,valeur)
                Next
                Assert.IsInstanceOfType(Of SectionProperties)(corps.LastChild)
                Dim section = DirectCast(corps.LastChild, SectionProperties)
                Assert.IsNotNull(section.GetFirstChild(Of TitlePage)())
                Dim reference = section.Elements(Of FooterReference)().Single()
                Assert.AreEqual(HeaderFooterValues.First, reference.Type.Value)
                Dim pied = DirectCast(doc.MainDocumentPart.GetPartById(reference.Id.Value), FooterPart)
                StringAssert.Contains(pied.Footer.InnerText, "ne pas jeter sur la voie publique")
                Assert.IsFalse(corps.InnerText.Contains("ne pas jeter sur la voie publique"))
                For Each valeur In {"Genre : Drame", "Réalisation : Réalisatrice fictive", "Avec : Acteur fictif", "Pays : France", "Durée : 100 min"}
                    Dim ligne = corps.Descendants(Of Run)().Single(Function(r) r.InnerText = valeur)
                    Assert.AreEqual("953A21", ligne.RunProperties.Color.Val.Value)
                Next
                Assert.AreEqual(3,doc.MainDocumentPart.ImageParts.Count())
            End Using
        Finally
            Directory.Delete(dossier,True)
        End Try
    End Function
End Class