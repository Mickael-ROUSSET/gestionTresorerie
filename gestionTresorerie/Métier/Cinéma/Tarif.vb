Public Class Tarif
    Public Property IdTarif As Integer
    Public Property NomTarif As String
    Public Property ReductionPourcent As Decimal
    Public Property Montant As Decimal
    Public Property Conditions As String
    Public Property DateDebutValidite As Date
    Public Property DateFinValidite As Date?

    Public Sub New()
        ' Valeurs par défaut
        DateDebutValidite = New Date(1901, 1, 1)
    End Sub

    Public Sub New(nom As String,
                   montant As Decimal,
                   Optional reduction As Decimal = 0,
                   Optional conditions As String = Nothing,
                   Optional debut As Date = Nothing,
                   Optional fin As Date? = Nothing)

        If String.IsNullOrWhiteSpace(nom) Then
            Throw New ArgumentException("Le nom du tarif est obligatoire.", NameOf(nom))
        End If
        If montant < 0 Then
            Throw New ArgumentException("Le montant doit être positif.", NameOf(montant))
        End If
        If reduction < 0 OrElse reduction > 100 Then
            Throw New ArgumentException("La réduction doit être comprise entre 0 et 100.", NameOf(reduction))
        End If

        Me.NomTarif = nom
        Me.Montant = montant
        Me.ReductionPourcent = reduction
        Me.Conditions = conditions
        Me.DateDebutValidite = If(debut = Nothing, New Date(1901, 1, 1), debut)
        Me.DateFinValidite = fin
    End Sub
    Private Shared Function CreateRepository() As TarifRepository
        Dim connectionString As String =
        ConnexionDB.GetInstance(Constantes.DataBases.Cinema).
                    GetConnexion(Constantes.DataBases.Cinema).
                    ConnectionString

        Dim factory As New AgumaaaConnectionFactory(connectionString)
        Dim provider As ISqlTextProvider = New LegacySqlTextProvider()
        Dim executor As ISqlExecutor = New SqlExecutor(factory, provider)

        Return New TarifRepository(executor)
    End Function
    Public Shared Function GetTarifActifAdate(dateReference As Date) As Tarif
        Try
            Return CreateRepository().LireTarifActifAdate(dateReference)
        Catch ex As Exception
            Logger.ERR($"Erreur GetTarifActifAdate : {ex.Message}")
            Return Nothing
        End Try
    End Function
End Class
