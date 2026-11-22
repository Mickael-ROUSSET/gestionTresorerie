Option Strict On
Imports System.Data
Imports System.Data.SqlClient

Partial Public NotInheritable Class SqlDocumentTestStore
    Private Const DocumentsAvecMontant As String = "WITH docs AS (SELECT d.idDoc,d.dateDoc,d.categorieDoc,d.cheminDoc,d.sousCategorieDoc,CASE WHEN ISJSON(d.metaDonnees)=1 THEN d.metaDonnees ELSE N'{}' END AS j FROM dbo.Documents d), " &
            " candidats AS (SELECT d.*,COALESCE(JSON_VALUE(j,'$.numero_du_cheque'),JSON_VALUE(j,'$.numero_facture'),JSON_VALUE(j,'$.reference')) AS ref," &
            "TRY_CONVERT(decimal(19,2),REPLACE(COALESCE(JSON_VALUE(j,'$.montant_ttc'),JSON_VALUE(j,'$.montant_numerique')),',','.')) AS montant FROM docs d) "
    Private Const MontantCompatible As String = "ABS(d.montant)=ABS(@montant) AND (d.categorieDoc<>N'Cheque' OR d.sousCategorieDoc IS NULL OR (@credit=1 AND d.sousCategorieDoc<>N'Emis') OR (@credit=0 AND d.sousCategorieDoc<>N'Reçus'))"

    Public Shared Function JustificatifsReleve(hash As String, index As Integer, ligne As LigneReleve) As DataTable
        Dim m = ReleveToMouvementMapper.Mapper(ligne)
        Const sql As String = DocumentsAvecMontant &
            "SELECT d.idDoc,d.dateDoc,d.categorieDoc,d.cheminDoc,d.montant,CASE WHEN r.idDoc IS NULL THEN 0 ELSE 1 END AS lie, " &
            "CASE WHEN LEN(ref)>=3 AND CHARINDEX(ref,@libelle)>0 THEN 1 ELSE 0 END AS refOk FROM candidats d " &
            "LEFT JOIN dbo.ReleveJustificatifsTest r ON r.idDoc=d.idDoc AND r.empreinte=@hash AND r.operation=@ligne " &
            "WHERE " & MontantCompatible & " ORDER BY lie DESC,refOk DESC,ABS(DATEDIFF(day,dateDoc,@date)),d.idDoc;"
        Using cn = OpenTest(), cmd As New SqlCommand(sql,cn)
            cmd.Parameters.AddWithValue("@hash",hash)
            cmd.Parameters.AddWithValue("@ligne",index)
            cmd.Parameters.AddWithValue("@libelle",m.Note)
            cmd.Parameters.AddWithValue("@credit",m.Sens)
            cmd.Parameters.AddWithValue("@montant",Math.Abs(m.Montant))
            cmd.Parameters.AddWithValue("@date",m.DateMvt)
            Dim t As New DataTable()
            Using reader = cmd.ExecuteReader()
                t.Load(reader)
            End Using
            Return t
        End Using
    End Function

    Public Shared Sub RattacherJustificatifs(hash As String, index As Integer, ligne As LigneReleve, ids As IEnumerable(Of Integer), Optional mouvement As Integer? = Nothing)
        Dim m = ReleveToMouvementMapper.Mapper(ligne)
        Dim documents = ids.Distinct().ToArray()
        If documents.Length = 0 Then Throw New ArgumentException("Cochez au moins un justificatif.")
        Using cn = OpenTest(), tx = cn.BeginTransaction(IsolationLevel.Serializable)
            For Each id In documents
                Using cmd As New SqlCommand(DocumentsAvecMontant & "SELECT @compatible=COUNT(*) FROM candidats d WHERE d.idDoc=@id AND " & MontantCompatible & "; IF @compatible<>1 THROW 51100,N'Le montant ou le sens du justificatif ne correspond pas à l’opération.',1;" &
                    " IF NOT EXISTS(SELECT 1 FROM dbo.ReleveJustificatifsTest WITH(UPDLOCK,HOLDLOCK) WHERE empreinte=@hash AND operation=@ligne AND idDoc=@id) INSERT dbo.ReleveJustificatifsTest VALUES(@hash,@ligne,@id,@date,@signe,@libelle);" &
                    " IF @mvt IS NOT NULL BEGIN IF NOT EXISTS(SELECT 1 FROM dbo.Mouvements WHERE Id=@mvt AND sens=@sens AND ABS(montant)=ABS(@signe)) THROW 51101,N'Mouvement incompatible.',1;" &
                    " UPDATE dbo.Documents SET idMvtDoc=@mvt,dateModif=GETDATE() WHERE idDoc=@id AND (ISNULL(idMvtDoc,0)=0 OR idMvtDoc=@mvt); IF @@ROWCOUNT<>1 THROW 51102,N'Un justificatif est déjà lié à un autre mouvement.',1; END;",cn,tx)
                    cmd.Parameters.Add("@compatible",SqlDbType.Int).Value=0
                    cmd.Parameters.AddWithValue("@montant",Math.Abs(m.Montant))
                    cmd.Parameters.AddWithValue("@credit",m.Sens)
                    cmd.Parameters.AddWithValue("@id",id)
                    cmd.Parameters.AddWithValue("@hash",hash)
                    cmd.Parameters.AddWithValue("@ligne",index)
                    cmd.Parameters.AddWithValue("@date",m.DateMvt)
                    cmd.Parameters.AddWithValue("@signe",If(m.Sens,Math.Abs(m.Montant),-Math.Abs(m.Montant)))
                    cmd.Parameters.AddWithValue("@libelle",m.Note)
                    cmd.Parameters.AddWithValue("@sens",m.Sens)
                    cmd.Parameters.Add("@mvt",SqlDbType.Int).Value=If(mouvement.HasValue,CObj(mouvement.GetValueOrDefault()),DBNull.Value)
                    cmd.ExecuteNonQuery()
                End Using
            Next
            tx.Commit()
        End Using
    End Sub
End Class
