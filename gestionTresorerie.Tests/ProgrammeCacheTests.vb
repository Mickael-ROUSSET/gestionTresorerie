Imports System.IO
Imports DocumentFormat.OpenXml.Packaging
Imports DocumentFormat.OpenXml.Wordprocessing
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports Newtonsoft.Json.Linq
Imports gestionTresorerie

<TestClass>
Public Class ProgrammeCacheTests
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