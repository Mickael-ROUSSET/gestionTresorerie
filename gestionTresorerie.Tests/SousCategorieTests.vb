Imports Xunit

Public Class SousCategorieTests
    Inherits BaseDataRowTests(Of SousCategorie)

    <Fact>
    Public Sub Example_CustomTest()
        Dim sc As New SousCategorie(1, "Logiciels", DateTime.Now.AddMonths(-2), DateTime.Now, False)
        Assert.Equal("Logiciels", sc.Libelle)
    End Sub
End Class


