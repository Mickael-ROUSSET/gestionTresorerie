Option Strict On
Imports System.Data
Imports System.Data.SqlClient

Partial Public NotInheritable Class SqlDocumentTestStore
    Public Shared Function AnnualTestSummary(firstDay As Date, lastDay As Date) As DataTable
        If firstDay.Date > lastDay.Date Then Throw New ArgumentException("La fin de l’exercice précède le début.")
        Const sql As String =
            "WITH lignes AS (SELECT m.Id, m.sens, ABS(CONVERT(decimal(19,2),m.montant)) montant, " &
            "COALESCE(NULLIF(k.nature,''),NULLIF(sc.libelle,''),NULLIF(c.libelle,''),N'À classer') nature, " &
            "COALESCE(NULLIF(k.evenement,''),NULLIF(m.evenement,''),N'Sans événement') evenement " &
            "FROM dbo.Mouvements m LEFT JOIN dbo.Categorie c ON c.Id=m.categorie LEFT JOIN dbo.SousCategorie sc ON sc.Id=m.sousCategorie AND sc.idCategorie=m.categorie " &
            "OUTER APPLY (SELECT CASE WHEN COUNT(*)=1 THEN MIN(d.idDoc) END AS idDoc FROM dbo.Documents d WHERE d.idMvtDoc=m.Id) links " &
            "LEFT JOIN dbo.ClassementDocumentTest k ON k.idDoc=COALESCE(NULLIF(m.idDoc,0),links.idDoc) WHERE m.dateMvt BETWEEN @debut AND @fin) " &
            "SELECT evenement,nature,COUNT(*) AS nombre,SUM(CASE WHEN sens=1 THEN montant ELSE 0 END) AS recettes,SUM(CASE WHEN sens=0 THEN montant ELSE 0 END) AS depenses FROM lignes GROUP BY evenement,nature ORDER BY evenement,nature;"
        Using connection = OpenTest(), cmd As New SqlCommand(sql, connection)
            cmd.Parameters.Add("@debut", SqlDbType.Date).Value = firstDay.Date
            cmd.Parameters.Add("@fin", SqlDbType.Date).Value = lastDay.Date
            Using reader = cmd.ExecuteReader()
                Dim result As New DataTable()
                result.Load(reader)
                Return result
            End Using
        End Using
    End Function

    Friend Shared Function ClassificationChoices() As DataTable
        Using connection = OpenTest(), cmd As New SqlCommand("SELECT 'nature' AS famille,libelle FROM dbo.Categorie UNION SELECT 'nature',libelle FROM dbo.SousCategorie UNION SELECT 'evenement',libelle FROM dbo.Evenements ORDER BY libelle;", connection)
            Using reader = cmd.ExecuteReader()
                Dim result As New DataTable()
                result.Load(reader)
                Return result
            End Using
        End Using
    End Function
End Class
