Imports System.Data.SqlClient

Public Class Lanceur
    Public Shared Sub lanceTrt()
        Dim batchAnalyse As New batchAnalyseChq

        ' Initialisation des objets et établissement de la connexion 
        batchAnalyse.ParcourirRepertoireEtAnalyser()
    End Sub
End Class
