Imports System.Data.SqlClient

Public Class UtilisateurRepository

    Private ReadOnly _executor As ISqlExecutor

    Public Sub New(executor As ISqlExecutor)
        If executor Is Nothing Then Throw New ArgumentNullException(NameOf(executor))
        _executor = executor
    End Sub

    Public Function ModifierMotDePasse(idUtilisateur As Integer, nouveauMotDePasse As String) As Integer
        Return _executor.ExecuteNamedNonQuery(
            "updateMotDePasse",
            New List(Of SqlParameter) From {
                New SqlParameter("@idUtilisateur", idUtilisateur),
                New SqlParameter("@motDePasse", nouveauMotDePasse)
            })
    End Function

    Public Function InsererUtilisateur(login As String,
                                       motDePasse As String,
                                       typeAcces As String,
                                       actif As Boolean) As Integer
        Return _executor.ExecuteNamedNonQuery(
            "insertUtilisateur",
            New List(Of SqlParameter) From {
                New SqlParameter("@login", If(login, DBNull.Value)),
                New SqlParameter("@motDePasse", If(motDePasse, DBNull.Value)),
                New SqlParameter("@typeAcces", If(typeAcces, DBNull.Value)),
                New SqlParameter("@actif", actif)
            })
    End Function

    Public Function MettreAJourUtilisateur(idUtilisateur As Integer,
                                           login As String,
                                           typeAcces As String,
                                           actif As Boolean) As Integer
        Return _executor.ExecuteNamedNonQuery(
            "updateUtilisateur",
            New List(Of SqlParameter) From {
                New SqlParameter("@idUtilisateur", idUtilisateur),
                New SqlParameter("@login", If(login, DBNull.Value)),
                New SqlParameter("@typeAcces", If(typeAcces, DBNull.Value)),
                New SqlParameter("@actif", actif)
            })
    End Function

    Public Function MettreAJourActif(idUtilisateur As Integer, actif As Boolean) As Integer
        Return _executor.ExecuteNamedNonQuery(
            "updateUtilisateurActif",
            New List(Of SqlParameter) From {
                New SqlParameter("@idUtilisateur", idUtilisateur),
                New SqlParameter("@actif", actif)
            })
    End Function

End Class