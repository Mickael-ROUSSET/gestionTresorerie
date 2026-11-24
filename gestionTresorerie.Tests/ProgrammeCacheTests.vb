Imports System.IO
Imports DocumentFormat.OpenXml.Packaging
Imports DocumentFormat.OpenXml.Wordprocessing
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports Newtonsoft.Json.Linq
Imports gestionTresorerie

<TestClass>
Public Class ProgrammeCacheTests
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
            Dim films As New List(Of FilmSeance) From {New FilmSeance With {.Titre="Film fictif",.DateDiffusion=New Date(2026,12,18),.HeureDiffusion=New TimeSpan(20,30,0),.Synopsis="Synopsis fictif"}}
            Assert.IsTrue(Await New ProgrammeCinemaCreator().GenererDocxAsync(films,sortie,"Adulte 7 €"))
            Using doc=WordprocessingDocument.Open(sortie,False)
                Dim corps=doc.MainDocumentPart.Document.Body
                For Each valeur In {"vendredi 18 décembre 20h30","Film fictif","Synopsis fictif","Adulte 7 €","décembre 2026"}
                    StringAssert.Contains(corps.InnerText,valeur)
                Next
                Assert.IsInstanceOfType(Of SectionProperties)(corps.LastChild)
                Assert.AreEqual(3,doc.MainDocumentPart.ImageParts.Count())
            End Using
        Finally
            Directory.Delete(dossier,True)
        End Try
    End Function
End Class