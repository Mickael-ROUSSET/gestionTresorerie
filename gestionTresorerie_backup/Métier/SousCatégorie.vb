Public Class SousCategorie
    Inherits BaseDataRow

    Public Property Id As Int32
    Public Property Libelle As String
    Public Property DateDebut As Date?
    Public Property DateFin As Date?
    Public Property idCategorie As Integer?

    Public Sub New()
        MyBase.New()
    End Sub
    ' --- Constructeur principal ---
    Public Sub New(id As Integer, libelle As String, Optional dateDebut As Date? = Nothing,
                   Optional dateFin As Date? = Nothing, Optional idCategorie As Integer? = 0)
        Me.Id = id
        Me.Libelle = libelle
        Me.DateDebut = dateDebut
        Me.DateFin = dateFin
        Me.idCategorie = idCategorie
    End Sub
End Class
