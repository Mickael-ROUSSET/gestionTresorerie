Imports System.Data.SqlClient
Imports System.Runtime.CompilerServices

Public Module DbExtensions
    ' Récupère une valeur ou une valeur par défaut pour les colonnes Nullables
    <Runtime.CompilerServices.Extension()>
    Public Function GetValueOrDefault(Of T)(rdr As SqlDataReader, columnName As String, Optional defaultValue As T = Nothing) As T
        Dim ordinal = rdr.GetOrdinal(columnName)
        Return If(rdr.IsDBNull(ordinal), defaultValue, DirectCast(rdr.GetValue(ordinal), T))
    End Function

    ' Résout l'avertissement CA1854 pour les dictionnaires
    <Extension()>
    Public Function GetTarifOrDefault(tarifs As Dictionary(Of String, Decimal), nomTarif As String) As Decimal
        Dim montant As Decimal
        tarifs.TryGetValue(nomTarif, montant) ' montant reste à 0 si non trouvé
        Return montant
    End Function
    ''' <summary>
    ''' Récupère la valeur d'une colonne de manière sécurisée (Gestion NULL et existence).
    ''' </summary>
    <Extension()>
    Public Function GetSafe(Of T)(dr As DataRow, columnName As String, Optional defaultValue As T = Nothing) As T
        Try
            ' 1. Vérifie si la colonne existe dans le résultat
            If Not dr.Table.Columns.Contains(columnName) Then Return defaultValue

            ' 2. Vérifie si la valeur est NULL en base
            Dim value = dr(columnName)
            If value Is DBNull.Value OrElse value Is Nothing Then Return defaultValue

            ' 3. Conversion sécurisée
            Return DirectCast(Convert.ChangeType(value, GetType(T)), T)
        Catch
            Return defaultValue
        End Try
    End Function
End Module