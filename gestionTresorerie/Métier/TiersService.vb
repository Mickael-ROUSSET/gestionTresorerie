'Public Class TiersService

'    Private ReadOnly _repo As TiersRepository

'    Public Sub New(conn As String)
'        _repo = New TiersRepository(conn)
'    End Sub

'    Public Function GetTiersComplet(id As Integer) As Tiers
'        Dim t = _repo.GetById(id)

'        ' Exemple de règle métier
'        If t IsNot Nothing AndAlso t.Coordonnees.Count = 0 Then
'            Logger.WARN($"Le tiers {t.Id} n'a pas de coordonnées.")
'        End If

'        Return t
'    End Function

'End Class
