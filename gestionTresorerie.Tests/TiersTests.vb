Imports Xunit

Public Class TiersTests
    Inherits BaseDataRowTests(Of Tiers)

    <Fact>
    Public Sub Example_CustomTest()
        Dim t As New Tiers(1, "Dupont", "Jean")
        Assert.Equal("Dupont", t.Nom)
        Assert.Equal("Jean", t.Prenom)
    End Sub
End Class


