Public Class ImportReleveResult

    Public Property LignesImport As List(Of LigneImportReleve)

    Public ReadOnly Property Lignes As List(Of LigneReleve)
        Get
            Return LignesImport.Select(Function(x) x.Ligne).ToList()
        End Get
    End Property

    Public ReadOnly Property Mouvements As List(Of Mouvements)
        Get
            Return LignesImport.Select(Function(x) x.Mouvement).ToList()
        End Get
    End Property

    Public ReadOnly Property NbLignes As Integer
        Get
            Return LignesImport.Count
        End Get
    End Property

    Public ReadOnly Property NbMouvements As Integer
        Get
            Return LignesImport.Where(Function(x) x.Mouvement IsNot Nothing).Count()
        End Get
    End Property

    Public ReadOnly Property NbDoublons As Integer
        Get
            Return LignesImport.Where(Function(x) x.ExisteDeja).Count()
        End Get
    End Property

    Public Sub New()
        LignesImport = New List(Of LigneImportReleve)()
    End Sub

End Class