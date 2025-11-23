Imports System.Data.SqlClient

Module UtilitairesSql
    Public Function SafeGetInt(rdr As SqlDataReader, field As String) As Integer
        If rdr.IsDBNull(rdr.GetOrdinal(field)) Then Return 0
        Return Convert.ToInt32(rdr(field))
    End Function

    Public Function SafeGetDecimal(rdr As SqlDataReader, field As String) As Decimal
        If rdr.IsDBNull(rdr.GetOrdinal(field)) Then Return 0D
        Return Convert.ToDecimal(rdr(field))
    End Function

    Public Function SafeGetString(rdr As SqlDataReader, field As String) As String
        If rdr.IsDBNull(rdr.GetOrdinal(field)) Then Return String.Empty
        Return rdr(field).ToString()
    End Function

End Module
