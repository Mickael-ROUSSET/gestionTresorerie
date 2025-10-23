Imports System.Data.SqlClient

Public Class UtilitairesDgv
    Implements IDataService

    Public Function ExecuterRequete(query As String, Optional parameters As Dictionary(Of String, Object) = Nothing) As DataTable Implements IDataService.ExecuterRequete
        Dim dt As New DataTable
        Try
            Dim command As SqlCommand = SqlCommandBuilder.CreateSqlCommand(query, parameters)
            Using adpt As New SqlDataAdapter(command)
                Dim unused = adpt.Fill(dt)
            End Using

            Logger.INFO($"Requête exécutée avec succès : {query}. {dt.Rows.Count} lignes extraites")
        Catch ex As Exception
            Logger.ERR($"Erreur lors de l'exécution de la requête {query}. Message: {ex.Message}")
            Throw
        End Try
        Return dt
    End Function
    Public Shared Sub selectionneIndiceDvg(indiceCherche As Integer, dgv As DataGridView)
        Dim i As Integer, ligneCible As Integer = 0

        For i = 0 To dgv.RowCount
            If dgv.Rows(i).Cells(0).Value = indiceCherche Then
                ligneCible = i
                Exit For
            End If
        Next
        dgv.Rows(ligneCible).Selected = True
        dgv.FirstDisplayedScrollingRowIndex = ligneCible
    End Sub
    Public Shared Sub SelectRowInDataGridView(dgv As DataGridView, id As Object)
        For Each row As DataGridViewRow In dgv.Rows
            If row.Cells(0).Value IsNot Nothing AndAlso row.Cells(0).Value.Equals(id) Then
                row.Selected = True
                dgv.FirstDisplayedScrollingRowIndex = row.Index
                Exit For
            End If
        Next
    End Sub
    Public Shared Sub ChargeDgvGenerique(dgv As DataGridView, sRequete As String, Optional parameters As Dictionary(Of String, Object) = Nothing)
        Dim utilitairesDgv As New UtilitairesDgv

        Try
            dgv.DataSource = utilitairesDgv.ExecuterRequete(sRequete, parameters)
            Logger.INFO($"Chargement de {dgv.Name} avec la requête {sRequete} réussi. {dgv.Rows.Count} lignes chargées")
        Catch ex As SqlException
            ' On informe l'utilisateur qu'il y a eu un problème :
            Dim unused = MessageBox.Show($"Une erreur s'est produite lors du chargement des données dans {dgv.Name} : {ex}")
            Logger.ERR($"Une erreur s'est produite lors du chargement des données dans {dgv.Name} : {ex}")
        End Try
    End Sub
    Public Shared Sub SelectionnerLigneDgvType(dgv As DataGridView, criteres As String)
        Try
            ' Vérifier si le critère est vide ou null
            If String.IsNullOrEmpty(criteres) Then
                Logger.WARN($"Le critère de sélection '{criteres}' est vide ou null.")
                Return
            End If

            ' Parcourir chaque ligne du DataGridView
            For Each row As DataGridViewRow In dgv.Rows
                If Not row.IsNewRow AndAlso criteres.Equals(row.Cells(1).Value?.ToString(), StringComparison.OrdinalIgnoreCase) Then
                    row.Selected = True
                    Logger.DBG($"Ligne sélectionnée dans {dgv.Name} avec le critère : {criteres}")
                    Return
                Else
                    row.Selected = False
                End If
            Next
            ' Log si aucune ligne n'est sélectionnée
            Logger.WARN($"Aucune ligne sélectionnée dans {dgv.Name} avec le critère : {criteres}")
        Catch ex As Exception
            ' Log des exceptions
            Logger.ERR($"Erreur lors de la sélection dans {dgv.Name} de la ligne {criteres} : {ex.Message}")
        End Try
    End Sub
End Class