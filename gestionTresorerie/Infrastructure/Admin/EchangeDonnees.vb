Option Strict On
Imports System.Data.SqlClient
Imports System.IO
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Friend Module EchangeDonnees
    Private Function Colonnes(cn As SqlConnection, tx As SqlTransaction, table As String) As DataTable
        Using cmd As New SqlCommand("SELECT TOP(0) * FROM dbo.[" & table & "]",cn,tx), r = cmd.ExecuteReader()
            Dim modele As New DataTable(table)
            modele.Load(r)
            Return modele
        End Using
    End Function

    Friend Sub Exporter(cs As String, fichier As String)
        Dim tables = AdministrationBase.TablesMetier(cs)
        Dim contenu As New JObject()
        Using cn As New SqlConnection(cs)
            cn.Open()
            Using tx = cn.BeginTransaction(IsolationLevel.Serializable)
                For Each table In tables
                    Using cmd As New SqlCommand("SELECT * FROM dbo.[" & table & "]",cn,tx), r = cmd.ExecuteReader()
                        Dim donnees As New DataTable()
                        donnees.Load(r)
                        contenu(table) = JArray.FromObject(donnees)
                    End Using
                Next
                tx.Commit()
            End Using
            Dim doc As New JObject From {{"format","AGUMAAA-1"},{"base",cn.Database},{"tables",contenu}}
            Dim tmp = fichier & "." & Guid.NewGuid().ToString("N") & ".tmp"
            Try
                File.WriteAllText(tmp,doc.ToString(Formatting.Indented))
                File.Move(tmp,fichier,True)
            Finally
                File.Delete(tmp)
            End Try
        End Using
    End Sub

    Friend Sub Importer(cs As String, fichier As String)
        Dim doc As JObject
        Using flux = File.OpenText(fichier), lecteur As New JsonTextReader(flux) With {.DateParseHandling=DateParseHandling.None,.FloatParseHandling=FloatParseHandling.Decimal}
            doc = JObject.Load(lecteur, New JsonLoadSettings With {.DuplicatePropertyNameHandling=DuplicatePropertyNameHandling.Error})
            If lecteur.Read() Then Throw New InvalidDataException("Contenu après la fin du fichier.")
        End Using
        Dim tables = AdministrationBase.TablesMetier(cs)
        Dim contenu = TryCast(doc("tables"),JObject)
        If CStr(doc("format")) <> "AGUMAAA-1" OrElse contenu Is Nothing OrElse Not New HashSet(Of String)(tables).SetEquals(contenu.Properties().Select(Function(p) p.Name)) Then Throw New InvalidDataException("Format ou liste des tables incompatible avec la base cible.")
        Using cn As New SqlConnection(cs)
            cn.Open()
            Using tx = cn.BeginTransaction(IsolationLevel.Serializable)
                For Each table In tables
                    Using cmd As New SqlCommand("DELETE FROM dbo.[" & table & "]",cn,tx)
                        cmd.ExecuteNonQuery()
                    End Using
                Next
                For Each table In Enumerable.Reverse(tables)
                    Dim rows = TryCast(contenu(table),JArray)
                    If rows Is Nothing Then Throw New InvalidDataException("Table invalide : " & table)
                    Using donnees = Colonnes(cn,tx,table)
                        Dim noms As New HashSet(Of String)(donnees.Columns.Cast(Of DataColumn)().Select(Function(c) c.ColumnName))
                        For Each token In rows
                            Dim obj = TryCast(token,JObject)
                            If obj Is Nothing OrElse Not noms.SetEquals(obj.Properties().Select(Function(p) p.Name)) Then Throw New InvalidDataException("Colonnes incompatibles : " & table)
                            Dim row = donnees.NewRow()
                            For Each col As DataColumn In donnees.Columns
                                Dim v = obj(col.ColumnName)
                                row(col) = If(v.Type=JTokenType.Null, CObj(DBNull.Value), v.ToObject(col.DataType))
                            Next
                            donnees.Rows.Add(row)
                        Next
                        Using copie As New SqlBulkCopy(cn, SqlBulkCopyOptions.KeepIdentity Or SqlBulkCopyOptions.KeepNulls Or SqlBulkCopyOptions.CheckConstraints,tx)
                            copie.DestinationTableName = "dbo.[" & table & "]"
                            For Each col As DataColumn In donnees.Columns
                                copie.ColumnMappings.Add(col.ColumnName,col.ColumnName)
                            Next
                            copie.WriteToServer(donnees)
                        End Using
                    End Using
                Next
                tx.Commit()
            End Using
        End Using
    End Sub
End Module
