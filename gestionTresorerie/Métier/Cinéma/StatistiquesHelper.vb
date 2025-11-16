'Imports System.Data.SqlClient
'Imports System.Formats.Tar

'Public Class StatFilm
'    Public Property IdFilm As Integer
'    Public Property Titre As String
'    Public Property NbSeances As Integer
'    Public Property TotalAdultes As Integer
'    Public Property TotalEnfants As Integer
'    Public Property TotalGroupeEnfants As Integer
'    Public Property CA_Adultes As Decimal
'    Public Property CA_Enfants As Decimal
'    Public Property CA_GroupeEnfants As Decimal
'    Public Property CA_Total As Decimal
'End Class

'Public Class StatistiquesHelper
'    Public Shared Function LireVue(Of T)(nomVue As String,
'                                         creator As Func(Of SqlDataReader, T)) As List(Of T)

'        Dim liste As New List(Of T)

'        Using cmd = SqlCommandBuilder.CreateSqlCommand(Constantes.cinemaDB, $"SELECT * FROM {nomVue}")
'            Using rdr = cmd.ExecuteReader()
'                While rdr.Read()
'                    liste.Add(creator(rdr))
'                End While
'            End Using
'        End Using

'        Return liste
'    End Function

'    Public Shared Function GetStatsParFilm() As List(Of StatFilm)
'        Dim result As New List(Of StatFilm)

'        Try
'            Using cmd = SqlCommandBuilder.CreateSqlCommand(Constantes.cinemaDB, "selStatsParFilm")
'                Using rdr = cmd.ExecuteReader()

'                    While rdr.Read()
'                        Dim stat As New StatFilm With {
'                        .IdFilm = UtilitairesSql.SafeGetInt(rdr, "IdFilm"),
'                        .Titre = UtilitairesSql.SafeGetString(rdr, "Titre"),
'                        .NbSeances = UtilitairesSql.SafeGetInt(rdr, "NbSeances"),
'                        .TotalAdultes = UtilitairesSql.SafeGetInt(rdr, "TotalAdultes"),
'                        .TotalEnfants = UtilitairesSql.SafeGetInt(rdr, "TotalEnfants"),
'                        .TotalGroupeEnfants = UtilitairesSql.SafeGetInt(rdr, "TotalGroupeEnfants"),
'                        .CA_Adultes = UtilitairesSql.SafeGetDecimal(rdr, "CA_Adultes"),
'                        .CA_Enfants = UtilitairesSql.SafeGetDecimal(rdr, "CA_Enfants"),
'                        .CA_GroupeEnfants = UtilitairesSql.SafeGetDecimal(rdr, "CA_GroupeEnfants"),
'                        .CA_Total = UtilitairesSql.SafeGetDecimal(rdr, "CA_Total")
'                    }
'                        result.Add(stat)
'                    End While
'                End Using
'            End Using

'        Catch ex As Exception
'            ' 🔥 Journalisation possible :
'            Logger.ERR("Erreur dans GetStatsParFilm : " & ex.ToString())

'            ' 👉 Au choix :
'            ' Throw
'            ' ou retourner une liste vide :
'            Return New List(Of StatFilm)
'        End Try

'        Return result
'    End Function

'    Public Shared Function GetStatsParFilm(idFilm As Integer,
'                                       dateDebut As Date,
'                                       dateFin As Date) As Dictionary(Of String, Decimal)

'        Dim stats As New Dictionary(Of String, Decimal) From {
'            {"MontantAdultes", 0D},
'            {"MontantEnfants", 0D},
'            {"MontantGroupeEnfants", 0D},
'            {"Total", 0D}
'        }

'        Dim param As New Dictionary(Of String, Object) From {
'            {"@IdFilm", idFilm},
'            {"@DateDebut", dateDebut},
'            {"@DateFin", dateFin}
'        }

'        Using cmd = SqlCommandBuilder.CreateSqlCommand(Constantes.cinemaDB, "selSeancesParFilmPeriode", param)
'            Using rdr = cmd.ExecuteReader()
'                While rdr.Read()

'                    Dim dateSeance = CType(rdr("DateHeureDebut"), Date).Date

'                    Dim nbA = rdr("NbEntreesAdultes")
'                    Dim nbE = rdr("NbEntreesEnfants")
'                    Dim nbG = rdr("NbEntreesGroupeEnfants")

'                    Dim tarifA = Tarif.GetTarifActif("Adulte", dateSeance)?.Montant
'                    Dim tarifE = Tarif.GetTarifActif("Enfant", dateSeance)?.Montant
'                    Dim tarifG = Tarif.GetTarifActif("GroupeEnfants", dateSeance)?.Montant

'                    Dim montantA = nbA * If(tarifA, 0)
'                    Dim montantE = nbE * If(tarifE, 0)
'                    Dim montantG = nbG * If(tarifG, 0)

'                    stats("MontantAdultes") += montantA
'                    stats("MontantEnfants") += montantE
'                    stats("MontantGroupeEnfants") += montantG
'                End While
'            End Using
'        End Using

'        stats("Total") = stats("MontantAdultes") +
'                         stats("MontantEnfants") +
'                         stats("MontantGroupeEnfants")

'        Return stats
'    End Function
'End Class
