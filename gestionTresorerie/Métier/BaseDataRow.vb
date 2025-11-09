Imports System.Data
Imports DocumentFormat.OpenXml.Office2010.Excel

Public MustInherit Class BaseDataRow
    ' 🔹 Clé primaire commune à tous les objets métiers
    'Public Property Id As Integer
    ' 🔹 Dates génériques de suivi (optionnelles)
    'Public Property DateCreation As Date?
    'Public Property DateModification As Date?
    ''' <summary>
    ''' Initialise une instance depuis un DataRow.
    ''' </summary>
    Public Sub New()
        ' Constructeur vide requis pour la contrainte {New}
    End Sub
    Public Shared Function FromDataRow(Of T As {BaseDataRow, New})(dr As DataRow) As T
        Return DataRowUtils.FromDataRowGeneric(Of T)(dr)
    End Function

    ''' <summary>
    ''' Initialise cette instance depuis un DataRow (version non partagée).
    ''' </summary>
    'Public Overridable Sub LoadFromDataRow(dr As DataRow)
    '    Dim entity = DataRowUtils.FromDataRowGeneric(Of BaseDataRow)(dr)
    '    ' Copie des propriétés dans l'instance courante
    '    For Each prop In entity.GetType().GetProperties()
    '        Dim currentProp = Me.GetType().GetProperty(prop.Name)
    '        If currentProp IsNot Nothing AndAlso currentProp.CanWrite Then
    '            currentProp.SetValue(Me, prop.GetValue(entity))
    '        End If
    '    Next
    'End Sub
    'Public Shared Function FromDataTable(Of T As {BaseDataRow, New})(dt As DataTable) As List(Of T)
    '    Dim result As New List(Of T)
    '    For Each dr As DataRow In dt.Rows
    '        result.Add(DataRowUtils.FromDataRowGeneric(Of T)(dr))
    '    Next
    '    Return result
    'End Function 

    ''' <summary>
    ''' Fabrique une instance d’une classe dérivée à partir d’une DataRow
    ''' </summary>
    Public Shared Function Create(Of T As {BaseDataRow, New})(dr As DataRow) As T
        Return DataRowUtils.FromDataRowGeneric(Of T)(dr)
    End Function


    ' 🔹 Optionnel : méthode utilitaire pour journalisation ou debug
    Public Overridable Function ResumeTexte() As String
        'Return $"{Me.GetType().Name} #{Id}"
        Return $"{Me.GetType().Name}"
    End Function
End Class

