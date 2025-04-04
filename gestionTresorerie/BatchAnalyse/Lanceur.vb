Imports System.Data.SqlClient

Public Class Lanceur
    Public Shared maConn As SqlConnection
    Public Sub lanceTrt()
        Dim batchAnalyse As New batchAnalyseChq

        ' Initialisation des objets et établissement de la connexion
        Dim lectureProprietes As New lectureProprietes()

        batchAnalyse.ParcourirRepertoireEtAnalyser()
    End Sub
End Class
