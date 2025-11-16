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
    Public Shared Function GetTarifActif(nomTarif As String, dateRef As Date) As Tarif
        Dim param As New Dictionary(Of String, Object) From {
        {"@NomTarif", nomTarif},
        {"@DateCible", dateRef.Date}
    }

        Using cmd = SqlCommandBuilder.CreateSqlCommand(Constantes.cinemaDB, "selTarifActifAdate", param)
            Using rdr = cmd.ExecuteReader()
                If rdr.Read() Then
                    Return New Tarif With {
                    .IdTarif = rdr("IdTarif"),
                    .NomTarif = rdr("NomTarif"),
                    .ReductionPourcent = rdr("ReductionPourcent"),
                    .Montant = rdr("Montant"),
                    .Conditions = rdr("Conditions").ToString(),
                    .DateDebutValidite = rdr("DateDebutValidite"),
                    .DateFinValidite = If(IsDBNull(rdr("DateFinValidite")), Nothing, rdr("DateFinValidite"))
                }
                End If
            End Using
        End Using

        Return Nothing
    End Function
End Class
