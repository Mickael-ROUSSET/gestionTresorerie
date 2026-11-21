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

    Public Sub Ajouter(cn As SqlConnection, type As Rubrique, nom As String, debut As Date?, fin As Date?, clos As Boolean)
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
                cmd.CommandText = "IF EXISTS(SELECT 1 FROM dbo.Exercices WITH(TABLOCKX,HOLDLOCK) WHERE dateDebut<=@fin AND dateFin>=@debut) THROW 51210,N'Cette période recouvre un exercice existant.',1; INSERT dbo.Exercices(dateDebut,dateFin,clos) VALUES(@debut,@fin,@clos);"
                cmd.Parameters.Add("@clos", SqlDbType.Bit).Value = clos
            Else
                cmd.CommandText = "IF EXISTS(SELECT 1 FROM " & table & " WITH(TABLOCKX,HOLDLOCK) WHERE libelle=@nom) THROW 51211,N'Ce libellé existe déjà.',1; INSERT " & table & "(libelle,dateDebut,dateFin) VALUES(@nom,@debut,@fin);"
                cmd.Parameters.Add("@nom", SqlDbType.NVarChar, 50).Value = nom
            End If
            cmd.Parameters.Add("@debut", SqlDbType.Date).Value = If(debut.HasValue, CObj(debut.GetValueOrDefault().Date), DBNull.Value)
            cmd.Parameters.Add("@fin", SqlDbType.Date).Value = If(fin.HasValue, CObj(fin.GetValueOrDefault().Date), DBNull.Value)
            cmd.ExecuteNonQuery()
            tx.Commit()
        End Using
    End Sub
End Module
