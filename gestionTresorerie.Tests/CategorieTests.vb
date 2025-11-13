Imports Xunit

Public Class CategorieTests
    Inherits BaseDataRowTests(Of Categorie)

    <Fact>
    Public Sub Example_CustomTest()
        Dim c As New Categorie(1, "Achats", DateTime.Now.AddMonths(-1), DateTime.Now, True)
        Assert.Equal("Achats", c.Libelle)
    End Sub
End Class

