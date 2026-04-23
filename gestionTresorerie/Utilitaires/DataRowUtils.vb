Public Module DataRowUtils

    Public Function FromDataRowGeneric(Of T As {BaseDataRow, New})(dr As DataRow) As T
        Return DataRowMapper.FromDataRowGeneric(Of T)(dr)
    End Function

    Public Function FromDataTableGeneric(Of T As {BaseDataRow, New})(dt As DataTable) As List(Of T)
        Return DataRowMapper.FromDataTableGeneric(Of T)(dt)
    End Function

    Public Function ToDataTableGeneric(Of T As BaseDataRow)(list As IEnumerable(Of T)) As DataTable
        Return DataRowMapper.ToDataTableGeneric(Of T)(list)
    End Function

End Module