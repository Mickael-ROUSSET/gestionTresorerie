Public Class Evenement
    Inherits BaseDataRow

    Public Property libelle As String
    Public Property DateDebut As Date
    Public Property DateFin As Date

    Public Sub New()
        MyBase.New()
    End Sub
End Class
