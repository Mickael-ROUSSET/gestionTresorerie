Option Strict On
Option Explicit On

Imports System
Imports System.Data

''' <summary>
''' Adaptateur de donnees pour une grille WinForms TEST.
''' Ne publie ni empreintes SHA-256 ni donnees comptables.
''' Aucune lecture ou ecriture externe.
''' </summary>
Public NotInheritable Class DocumentBatchTestReviewGrid
    Private Sub New()
    End Sub

    Public Shared Function CreateTable(snapshot As DocumentBatchTestReviewSnapshot) As DataTable
        If snapshot Is Nothing Then Throw New ArgumentNullException(NameOf(snapshot))

        Dim table As New DataTable("DocumentsTEST")
        table.Locale = Globalization.CultureInfo.InvariantCulture
        table.Columns.Add("Nom", GetType(String))
        table.Columns.Add("Motif", GetType(String))

        For Each item In snapshot.Pending
            table.Rows.Add(item.Name, item.Reason)
        Next

        table.AcceptChanges()
        Return table
    End Function
End Class
