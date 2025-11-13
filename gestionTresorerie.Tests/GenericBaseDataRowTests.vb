Imports Xunit
Imports System.Reflection
Imports System.Data

Public MustInherit Class BaseDataRowTests(Of T As {BaseDataRow, New})

    ' --- Génération automatique de valeurs réalistes ---
    Protected Function GenerateRealisticValue(prop As PropertyInfo) As Object
        Dim t As Type = If(Nullable.GetUnderlyingType(prop.PropertyType), prop.PropertyType)

        If T Is GetType(String) Then
            Return $"Test_{Guid.NewGuid().ToString("N")}"
        ElseIf T Is GetType(Integer) Then
            Return New Random().Next(1, 1000)
        ElseIf T Is GetType(Boolean) Then
            Return CBool(New Random().Next(0, 2))
        ElseIf T Is GetType(DateTime) Then
            Return DateTime.Now.AddDays(New Random().Next(-1000, 1000))
        ElseIf T Is GetType(Decimal) Then
            Return Convert.ToDecimal(New Random().NextDouble() * 1000)
        Else
            Return Nothing
        End If
    End Function

    ' --- Teste que les propriétés peuvent être lues et écrites ---
    <Fact>
    Public Sub Properties_CanBeSetAndGet()
        Dim entity As New T()
        For Each prop In GetType(T).GetProperties(BindingFlags.Public Or BindingFlags.Instance)
            If Not prop.CanWrite OrElse Not prop.CanRead Then Continue For
            Dim value = GenerateRealisticValue(prop)
            If value IsNot Nothing Then
                prop.SetValue(entity, value)
                Dim actual = prop.GetValue(entity)
                Assert.Equal(value, actual)
            End If
        Next
    End Sub

    ' --- Teste la conversion DataRow → entité ---
    <Fact>
    Public Sub FromDataRow_ShouldPopulateEntity()
        Dim dt As New DataTable()

        ' Crée les colonnes correspondant aux propriétés
        For Each prop In GetType(T).GetProperties(BindingFlags.Public Or BindingFlags.Instance)
            dt.Columns.Add(prop.Name, If(Nullable.GetUnderlyingType(prop.PropertyType), prop.PropertyType))
        Next

        ' Crée une ligne avec des valeurs réalistes
        Dim dr = dt.NewRow()
        For Each col As DataColumn In dt.Columns
            dr(col.ColumnName) = GenerateRealisticValue(GetType(T).GetProperty(col.ColumnName))
        Next
        dt.Rows.Add(dr)

        ' Appelle FromDataRow
        Dim entityFromRow = GetType(T).GetMethod("FromDataRow", BindingFlags.Public Or BindingFlags.Static).Invoke(Nothing, New Object() {dr})

        ' Vérifie que toutes les propriétés sont bien remplies
        For Each prop In GetType(T).GetProperties(BindingFlags.Public Or BindingFlags.Instance)
            Dim expected = dr(prop.Name)
            Dim actual = prop.GetValue(entityFromRow)
            Assert.Equal(expected, actual)
        Next
    End Sub

End Class
