Option Strict On
Imports System.Data
Imports System.Data.SqlClient

Public Module ParametresComptables
    Public Enum Rubrique
        Evenements
        Exercices
        TypesMouvement
    End Enum

    Friend Function Ouvrir(test As Boolean) As SqlConnection
        If test Then
            Dim store As New SqlDocumentTestStore()
            store.Initialize()
            Return SqlDocumentTestStore.OpenTest()
        End If
#If DEMO_BUILD Then
        Throw New InvalidOperationException("Cette édition utilise exclusivement bddAgumaaaTest.")
#Else
        If EnvironmentManager.CurrentEnvironment <> EnvironmentManager.EnvironnementType.PROD Then
            Throw New InvalidOperationException("Sélectionnez TEST ou PROD puis redémarrez l’application.")
        End If
        Dim cn = RepositoryFactory.CreateConnectionFactory(Constantes.DataBases.Agumaaa).CreateConnection()
        Try
            cn.Open()
            If Not String.Equals(cn.Database, "bddAgumaaa", StringComparison.OrdinalIgnoreCase) Then
                Throw New InvalidOperationException("Le catalogue configuré pour PROD doit être bddAgumaaa.")
            End If
            Return cn
        Catch
            cn.Dispose()
            Throw
        End Try
#End If
    End Function

    Private Function TableSql(type As Rubrique) As String
        Select Case type
            Case Rubrique.Evenements : Return "dbo.Evenements"
            Case Rubrique.Exercices : Return "dbo.Exercices"
            Case Rubrique.TypesMouvement : Return "dbo.TypeMouvement"
            Case Else : Throw New ArgumentOutOfRangeException(NameOf(type))
        End Select
    End Function

    Friend Sub SupprimerNature(cn As SqlConnection, sous As Boolean, id As Integer)
        Dim colonne = If(sous, "sousCategorie", "categorie")
        Dim table = If(sous, "SousCategorie", "Categorie")
        Using tx = cn.BeginTransaction(IsolationLevel.Serializable)
            Dim references = "EXISTS(SELECT 1 FROM dbo.Mouvements WHERE " & colonne & "=@id) OR EXISTS(SELECT 1 FROM dbo.Tiers WHERE " & colonne & "Defaut=@id) OR EXISTS(SELECT 1 FROM dbo.ReglesAffectationReleve WHERE id" & colonne & "=@id)"
            If Not sous Then references &= " OR EXISTS(SELECT 1 FROM dbo.SousCategorie WHERE idCategorie=@id)"
            If cn.Database.Equals("bddAgumaaaTest", StringComparison.OrdinalIgnoreCase) Then references &= " OR EXISTS(SELECT 1 FROM dbo.ClassementDocumentTest WHERE id" & colonne & "=@id)"
            Using cmd As New SqlCommand("IF " & references & " THROW 51141,N'Suppression impossible : cette rubrique est utilisée ou contient des sous-catégories.',1; DELETE FROM dbo." & table & " WHERE Id=@id; IF @@ROWCOUNT<>1 THROW 51142,N'Rubrique introuvable.',1;", cn, tx)
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = id
                cmd.ExecuteNonQuery()
            End Using
            tx.Commit()
        End Using
    End Sub

    Friend Function LireNatures(cn As SqlConnection) As DataTable
        Const sql = "SELECT c.Id AS cat,c.libelle AS categorie,c.debit,s.Id AS sousCat,s.libelle AS sousCategorie FROM dbo.Categorie c LEFT JOIN dbo.SousCategorie s ON s.idCategorie=c.Id ORDER BY c.libelle,c.Id,s.libelle,s.Id"
        Using cmd As New SqlCommand(sql, cn), reader = cmd.ExecuteReader()
            Dim result As New DataTable()
            result.Load(reader)
            Return result
        End Using
    End Function

    Public Function Lire(cn As SqlConnection, type As Rubrique) As DataTable
        Dim champs = If(type = Rubrique.Exercices, "dateDebut AS Début,dateFin AS Fin,clos AS Clos",
                        "libelle AS Libellé,dateDebut AS Début,dateFin AS Fin")
        Using cmd As New SqlCommand("SELECT Id," & champs & " FROM " & TableSql(type) &
                                    If(type = Rubrique.Exercices, " ORDER BY dateDebut DESC", " ORDER BY libelle"), cn)
            Using reader = cmd.ExecuteReader()
                Dim rows As New DataTable()
                rows.Load(reader)
                Return rows
            End Using
        End Using
    End Function

    Public Sub Ajouter(cn As SqlConnection, type As Rubrique, nom As String, debut As Date?, fin As Date?, clos As Boolean, Optional id As Integer? = Nothing)
        Dim table = TableSql(type)
        nom = If(nom, "").Trim()
        If type = Rubrique.Exercices Then
            If Not debut.HasValue OrElse Not fin.HasValue Then Throw New ArgumentException("Renseignez les deux dates de l’exercice.")
        ElseIf nom.Length = 0 OrElse nom.Length > 50 Then
            Throw New ArgumentException("Le libellé doit comporter entre 1 et 50 caractères.")
        End If
        If debut.HasValue AndAlso fin.HasValue AndAlso fin.Value.Date < debut.Value.Date Then
            Throw New ArgumentException("La fin doit être postérieure ou égale au début.")
        End If
        Using tx = cn.BeginTransaction(), cmd = cn.CreateCommand()
            cmd.Transaction = tx
            If type = Rubrique.Exercices Then
                cmd.CommandText = "IF EXISTS(SELECT 1 FROM dbo.Exercices WITH(TABLOCKX,HOLDLOCK) WHERE (@id IS NULL OR Id<>@id) AND dateDebut<=@fin AND dateFin>=@debut) THROW 51210,N'Cette période recouvre un exercice existant.',1; IF @id IS NULL INSERT dbo.Exercices(dateDebut,dateFin,clos) VALUES(@debut,@fin,@clos); ELSE UPDATE dbo.Exercices SET dateDebut=@debut,dateFin=@fin,clos=@clos WHERE Id=@id;"
                cmd.Parameters.Add("@clos", SqlDbType.Bit).Value = clos
            Else
                cmd.CommandText = "IF EXISTS(SELECT 1 FROM " & table & " WITH(TABLOCKX,HOLDLOCK) WHERE (@id IS NULL OR Id<>@id) AND libelle=@nom) THROW 51211,N'Ce libellé existe déjà.',1; IF @id IS NULL INSERT " & table & "(libelle,dateDebut,dateFin) VALUES(@nom,@debut,@fin); ELSE UPDATE " & table & " SET libelle=@nom,dateDebut=@debut,dateFin=@fin WHERE Id=@id;"
                cmd.Parameters.Add("@nom", SqlDbType.NVarChar, 50).Value = nom
            End If
            cmd.CommandText &= " IF @@ROWCOUNT<>1 THROW 51212,N'La ligne a disparu. Actualisez la liste.',1;"
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = If(id.HasValue, CObj(id.GetValueOrDefault()), DBNull.Value)
            cmd.Parameters.Add("@debut", SqlDbType.Date).Value = If(debut.HasValue, CObj(debut.GetValueOrDefault().Date), DBNull.Value)
            cmd.Parameters.Add("@fin", SqlDbType.Date).Value = If(fin.HasValue, CObj(fin.GetValueOrDefault().Date), DBNull.Value)
            cmd.ExecuteNonQuery()
            tx.Commit()
        End Using
    End Sub
End Module
