Option Strict On
Imports System.Data
Imports System.Data.SqlClient

Partial Public NotInheritable Class SqlDocumentTestStore
    Public Shared Function JustificatifsReleve(hash As String, index As Integer, ligne As LigneReleve) As DataTable
        Dim m = ReleveToMouvementMapper.Mapper(ligne)
        Const sql As String = "WITH docs AS (SELECT d.idDoc,d.dateDoc,d.categorieDoc,d.cheminDoc,d.sousCategorieDoc,CASE WHEN ISJSON(d.metaDonnees)=1 THEN d.metaDonnees ELSE N'{}' END AS j FROM dbo.Documents d), " &
            " candidats AS (SELECT d.*,COALESCE(JSON_VALUE(j,'$.numero_du_cheque'),JSON_VALUE(j,'$.numero_facture'),JSON_VALUE(j,'$.reference')) AS ref," &
            "TRY_CONVERT(decimal(19,2),REPLACE(COALESCE(JSON_VALUE(j,'$.montant_ttc'),JSON_VALUE(j,'$.montant_numerique')),',','.')) AS montant FROM docs d), " &
            " scores AS (SELECT d.*,CASE WHEN r.idDoc IS NULL THEN 0 ELSE 1 END AS lie," &
            " CASE WHEN LEN(ref)>=3 AND CHARINDEX(ref,@libelle)>0 THEN 1 ELSE 0 END AS refOk," &
            " CASE WHEN ABS(montant)=@montant AND (categorieDoc<>N'Cheque' OR sousCategorieDoc IS NULL OR (@credit=1 AND sousCategorieDoc<>N'Emis') OR (@credit=0 AND sousCategorieDoc<>N'Reçus')) THEN 1 ELSE 0 END AS montantOk FROM candidats d LEFT JOIN dbo.ReleveJustificatifsTest r ON r.idDoc=d.idDoc AND r.empreinte=@hash AND r.operation=@ligne) " &
            "SELECT idDoc,dateDoc,categorieDoc,cheminDoc,montant,lie,refOk,montantOk,sousCategorieDoc FROM scores WHERE lie=1 OR refOk=1 OR ABS(DATEDIFF(day,dateDoc,@date))<=90 ORDER BY lie DESC,refOk DESC,montantOk DESC,ABS(DATEDIFF(day,dateDoc,@date)),idDoc;"
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
            ' Pour un règlement groupé, repérer les paires de factures ou les paires de chèques.
            ' Ne jamais sommer une facture avec le chèque susceptible de la régler.
            t.Columns.Add("partenaire",GetType(Integer))
            Dim eligible=t.AsEnumerable().Where(Function(row)
                If row.IsNull("montant") Then Return False
                Dim kind=Convert.ToString(row("categorieDoc"))
                If kind <> "Facture" AndAlso kind <> "Cheque" Then Return False
                If kind="Cheque" AndAlso ((m.Sens AndAlso Convert.ToString(row("sousCategorieDoc"))="Emis") OrElse
                    (Not m.Sens AndAlso Convert.ToString(row("sousCategorieDoc"))="Reçus")) Then Return False
                Return CDec(row("montant"))>0D AndAlso CDec(row("montant"))<Math.Abs(m.Montant)
            End Function).ToLookup(Function(row) (CStr(row("categorieDoc")),CDec(row("montant"))))
            For Each g In eligible
                Dim reste=Math.Abs(m.Montant)-g.Key.Item2
                If eligible.Contains((g.Key.Item1,reste)) AndAlso (reste<>g.Key.Item2 OrElse g.Count()>1) Then
                    For Each row In g
                        Dim partenaire=eligible((g.Key.Item1,reste)).First(Function(autre) CInt(autre("idDoc"))<>CInt(row("idDoc")))
                        row("partenaire")=partenaire("idDoc")
                    Next
                End If
            Next
            Return t
        End Using
    End Function

    Public Shared Sub RattacherJustificatifs(hash As String, index As Integer, ligne As LigneReleve, ids As IEnumerable(Of Integer), Optional mouvement As Integer? = Nothing)
        Dim m = ReleveToMouvementMapper.Mapper(ligne)
        Dim documents = ids.Distinct().ToArray()
        If documents.Length = 0 Then Throw New ArgumentException("Cochez au moins un justificatif.")
        Using cn = OpenTest(), tx = cn.BeginTransaction(IsolationLevel.Serializable)
            For Each id In documents
                Using cmd As New SqlCommand("IF NOT EXISTS(SELECT 1 FROM dbo.Documents WHERE idDoc=@id) THROW 51100,N'Document devenu indisponible.',1;" &
                    " IF NOT EXISTS(SELECT 1 FROM dbo.ReleveJustificatifsTest WITH(UPDLOCK,HOLDLOCK) WHERE empreinte=@hash AND operation=@ligne AND idDoc=@id) INSERT dbo.ReleveJustificatifsTest VALUES(@hash,@ligne,@id,@date,@signe,@libelle);" &
                    " IF @mvt IS NOT NULL BEGIN IF NOT EXISTS(SELECT 1 FROM dbo.Mouvements WHERE Id=@mvt AND sens=@sens AND ABS(montant)=ABS(@signe)) THROW 51101,N'Mouvement incompatible.',1;" &
                    " UPDATE dbo.Documents SET idMvtDoc=@mvt,dateModif=GETDATE() WHERE idDoc=@id AND (ISNULL(idMvtDoc,0)=0 OR idMvtDoc=@mvt); IF @@ROWCOUNT<>1 THROW 51102,N'Un justificatif est déjà lié à un autre mouvement.',1; END;",cn,tx)
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
