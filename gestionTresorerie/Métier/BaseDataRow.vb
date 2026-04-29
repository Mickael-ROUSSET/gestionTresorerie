Imports System.CodeDom.Compiler
Imports System.Data
Imports System.Data.SqlClient

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

    ' --- Chaque entité doit implémenter ceci ---
    Public MustOverride Sub LoadFromReader(reader As SqlDataReader)
    Public Shared Function Chargement(Of T As {BaseDataRow, New})(
            nomRequete As String,
            Optional params As Dictionary(Of String, Object) = Nothing
        ) As List(Of T)

        Dim executor As ISqlExecutor = RepositoryFactory.CreateExecutor(Constantes.DataBases.Agumaaa)

        Dim sqlParams As List(Of SqlParameter) = ConvertParameters(params)

        Return executor.ExecuteNamedReader(
        nomRequete,
        sqlParams,
        Function(reader As SqlDataReader)
            Dim obj As New T()
            obj.LoadFromReader(reader)
            Return obj
        End Function
    )
    End Function
    Private Shared Function ConvertParameters(params As Dictionary(Of String, Object)) As List(Of SqlParameter)
        Dim result As New List(Of SqlParameter)

        If params Is Nothing Then
            Return result
        End If

        For Each kvp In params
            result.Add(New SqlParameter(kvp.Key, If(kvp.Value, DBNull.Value)))
        Next

        Return result
    End Function

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

