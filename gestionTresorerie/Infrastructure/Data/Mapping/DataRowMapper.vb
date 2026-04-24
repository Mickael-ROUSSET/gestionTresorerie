Imports System.Reflection

Public Module DataRowMapper

    Public Function FromDataRowGeneric(Of T As {BaseDataRow, New})(dr As DataRow) As T
        If dr Is Nothing Then
            Return Nothing
        End If

        Dim obj As New T()
        Dim tType As Type = GetType(T)

        For Each prop As PropertyInfo In tType.GetProperties(BindingFlags.Public Or BindingFlags.Instance)
            If dr.Table.Columns.Contains(prop.Name) AndAlso dr(prop.Name) IsNot DBNull.Value Then
                Try
                    Dim targetType As Type = If(Nullable.GetUnderlyingType(prop.PropertyType), prop.PropertyType)
                    Dim targetValue = Convert.ChangeType(dr(prop.Name), targetType)
                    prop.SetValue(obj, targetValue)
                Catch ex As Exception
                    Logger.WARN($"⚠️ Impossible d’affecter la colonne {prop.Name} à la propriété {prop.PropertyType.Name} : {ex.Message}")
                End Try
            End If
        Next

        Return obj
    End Function

    Public Function FromDataTableGeneric(Of T As {BaseDataRow, New})(dt As DataTable) As List(Of T)
        Dim result As New List(Of T)

        If dt Is Nothing OrElse dt.Rows.Count = 0 Then
            Return result
        End If

        For Each dr As DataRow In dt.Rows
            Dim item = FromDataRowGeneric(Of T)(dr)
            If item IsNot Nothing Then
                result.Add(item)
            End If
        Next

        Return result
    End Function

    Public Function ToDataTableGeneric(Of T As BaseDataRow)(list As IEnumerable(Of T)) As DataTable
        Dim dt As New DataTable(GetType(T).Name)

        If list Is Nothing Then
            Return dt
        End If

        Dim props As PropertyInfo() = GetType(T).GetProperties(BindingFlags.Public Or BindingFlags.Instance)

        For Each prop As PropertyInfo In props
            Dim colType As Type = If(Nullable.GetUnderlyingType(prop.PropertyType), prop.PropertyType)
            dt.Columns.Add(prop.Name, colType)
        Next

        For Each item As T In list
            Dim row = dt.NewRow()

            For Each prop As PropertyInfo In props
                Dim value = prop.GetValue(item, Nothing)
                row(prop.Name) = If(value, DBNull.Value)
            Next

            dt.Rows.Add(row)
        Next

        Return dt
    End Function

End Module