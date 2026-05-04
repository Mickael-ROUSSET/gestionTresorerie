Public Class RegleAffectationReleve
    Public Property Id As Integer
    Public Property MotCle As String
    Public Property IdTiers As Integer?
    Public Property IdCategorie As Integer?
    Public Property IdSousCategorie As Integer?
    Public Property Sens As Boolean?
    Public Property Priorite As Integer = 100
    Public Property Actif As Boolean = True
End Class