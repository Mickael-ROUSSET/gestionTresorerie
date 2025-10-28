Public Class DocumentAgumaaaImpl
    Inherits DocumentAgumaaa

    ' Constructeur qui appelle celui de la classe de base
    Public Sub New(idMvtDoc As Integer,
                   dateDoc As Date,
                   contenuDoc As String,
                   cheminDoc As String,
                   categorieDoc As String,
                   sousCategorieDoc As String,
                   numCompte As Integer,
                   metaDonnees As String,
                   dateModif As Date)
        MyBase.New(idMvtDoc, dateDoc, contenuDoc, cheminDoc,
                   categorieDoc, sousCategorieDoc, numCompte,
                   metaDonnees, dateModif)
    End Sub

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub

    Public Overrides Function Equals(obj As Object) As Boolean
        Return MyBase.Equals(obj)
    End Function

    Public Overrides Function GetHashCode() As Integer
        Return MyBase.GetHashCode()
    End Function

    Public Overrides Function ToString() As String
        Return MyBase.ToString()
    End Function

    Public Overrides Function RenommerFichier(sNomFichier As String, Optional sNouveauNom As String = "") As String
        Throw New NotImplementedException()
    End Function
End Class

