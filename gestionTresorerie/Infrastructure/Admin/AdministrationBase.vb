Option Strict On
Imports System.Data.SqlClient

Friend Module AdministrationBase
    Private ReadOnly ordre As String() = {"ReleveJustificatifsTest", "ClassementDocumentTest", "Documents", "Mouvements", "ParticipantMdN", "ReglesAffectationReleve", "Seances", "Coordonnees", "Tiers", "SousCategorie", "Categorie", "Evenements", "Exercices", "Tarifs", "Films", "Comptes", "Communes", "TypeDoc", "TypeMouvement", "Type"}

    Private Function VerifierCatalogue(cs As String) As SqlConnectionStringBuilder
        Dim cible As New SqlConnectionStringBuilder(cs)
        If cible.InitialCatalog <> "bddAgumaaaTest" AndAlso cible.InitialCatalog <> "bddAgumaaa" Then Throw New ArgumentException("Catalogue AGUMAAA attendu.")
        Return cible
    End Function

    Friend Function TablesMetier(cs As String) As List(Of String)
        Dim tables As New List(Of String)
        Using cn As New SqlConnection(VerifierCatalogue(cs).ConnectionString), cmd As New SqlCommand("SELECT name FROM sys.tables WHERE schema_id=SCHEMA_ID('dbo')", cn)
            cn.Open()
            Using lecteur = cmd.ExecuteReader()
                While lecteur.Read()
                    tables.Add(lecteur.GetString(0))
                End While
            End Using
        End Using
        Return ordre.Intersect(tables, StringComparer.OrdinalIgnoreCase).ToList()
    End Function

    Friend Sub Vider(cs As String, tables As List(Of String))
        Using cn As New SqlConnection(VerifierCatalogue(cs).ConnectionString)
            cn.Open()
            Using tx = cn.BeginTransaction()
                For Each table In tables
                    If Not ordre.Contains(table) Then Throw New ArgumentException("Table hors périmètre.")
                    Using cmd As New SqlCommand("DELETE FROM dbo.[" & table & "]", cn, tx)
                        cmd.CommandTimeout = 600
                        cmd.ExecuteNonQuery()
                    End Using
                Next
                tx.Commit()
            End Using
        End Using
    End Sub

    Friend Sub Restaurer(cs As String, chemin As String)
        Dim cible = AdministrationBase.VerifierCatalogue(cs)
        Dim nom = cible.InitialCatalog
        cible.InitialCatalog = "master"
        Using pool As New SqlConnection(cs)
            SqlConnection.ClearPool(pool)
        End Using
        Using cn As New SqlConnection(cible.ConnectionString), cmd = cn.CreateCommand()
            cn.Open()
            cmd.CommandTimeout = 600
            cmd.Parameters.AddWithValue("@f", chemin)
            cmd.CommandText = "RESTORE HEADERONLY FROM DISK=@f"
            Using reader = cmd.ExecuteReader()
                If Not reader.Read() OrElse CStr(reader("DatabaseName")) <> nom OrElse CInt(reader("BackupType")) <> 1 OrElse reader.Read() Then Throw New ArgumentException("Choisissez une sauvegarde complète unique de " & nom & ".")
            End Using
            Dim actuels As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
            cmd.CommandText = "SELECT name, physical_name FROM sys.master_files WHERE database_id=DB_ID(@base)"
            cmd.Parameters.AddWithValue("@base", nom)
            Using r = cmd.ExecuteReader()
                While r.Read()
                    actuels.Add(r.GetString(0), r.GetString(1))
                End While
            End Using
            Dim deplacements As New List(Of String)
            cmd.CommandText = "RESTORE FILELISTONLY FROM DISK=@f"
            Using r = cmd.ExecuteReader()
                While r.Read()
                    Dim logique = CStr(r("LogicalName"))
                    If Not actuels.ContainsKey(logique) Then Throw New ArgumentException("Fichiers de sauvegarde incompatibles avec la base cible.")
                    deplacements.Add("MOVE N'" & logique.Replace("'", "''") & "' TO N'" & actuels(logique).Replace("'", "''") & "'")
                    actuels.Remove(logique)
                End While
            End Using
            If actuels.Count <> 0 OrElse deplacements.Count = 0 Then Throw New ArgumentException("Jeu de fichiers incomplet.")
            Dim relogement = String.Join(", ", deplacements)
            cmd.CommandText = "RESTORE VERIFYONLY FROM DISK=@f WITH " & relogement
            cmd.ExecuteNonQuery()
            cmd.CommandText = "ALTER DATABASE [" & nom & "] SET SINGLE_USER WITH ROLLBACK IMMEDIATE"
            cmd.ExecuteNonQuery()
            Try
                cmd.CommandText = "RESTORE DATABASE [" & nom & "] FROM DISK=@f WITH REPLACE, RECOVERY, " & relogement
                cmd.ExecuteNonQuery()
            Finally
                cmd.CommandText = "ALTER DATABASE [" & nom & "] SET MULTI_USER"
                cmd.ExecuteNonQuery()
            End Try
        End Using
    End Sub
End Module
