
Public Class Seance
    Public Property IdSeance As Integer
    Public Property IdFilm As Integer
    Public Property DateHeureDebut As Date
    Public Property TarifBase As Decimal
    Public Property Langue As String
    Public Property Format As String
    ' --- Les nouvelles propriétés demandées ---
    Public Property NbEntreesAdultes As Integer
    Public Property NbEntreesEnfants As Integer
    Public Property NbEntreesGroupeEnfants As Integer
    Public Property CA_Adultes As Decimal
    Public Property CA_Enfants As Decimal
    Public Property CA_GroupeEnfants As Decimal
    Public Property CA_Total As Decimal

    ' --- Constructeur principal ---
    Public Sub New(idFilm As Integer,
                   dateHeureDebut As Date,
                   Optional tarifBase As Decimal = 0D,
                   Optional langue As String = Nothing,
                   Optional format As String = Nothing,
                   Optional nbAdultes As Integer = 0,
                   Optional nbEnfants As Integer = 0,
                   Optional nbGroupeEnfants As Integer = 0,
                   Optional tarifAdulte As Decimal = 0D,
                   Optional tarifEnfant As Decimal = 0D,
                   Optional tarifGroupeEnfant As Decimal = 0D
                   )

        ' ==== VALIDATIONS ====

        If idFilm <= 0 Then
            Throw New ArgumentException("IdFilm doit être un entier positif.", NameOf(idFilm))
        End If

        If dateHeureDebut.Date = Date.MinValue Then
            Throw New ArgumentException("La date de début est obligatoire et doit être valide.", NameOf(dateHeureDebut))
        End If

        If tarifBase < 0 Then
            Throw New ArgumentException("Le tarif de base ne peut pas être négatif.", NameOf(tarifBase))
        End If

        If nbAdultes < 0 Then
            Throw New ArgumentException("NbEntreesAdultes ne peut pas être négatif.", NameOf(nbAdultes))
        End If
        If nbEnfants < 0 Then
            Throw New ArgumentException("NbEntreesEnfants ne peut pas être négatif.", NameOf(nbEnfants))
        End If
        If nbGroupeEnfants < 0 Then
            Throw New ArgumentException("NbEntreesGroupeEnfants ne peut pas être négatif.", NameOf(nbGroupeEnfants))
        End If

        ' ==== AFFECTATIONS ====
        Me.IdFilm = idFilm
        Me.DateHeureDebut = dateHeureDebut
        Me.TarifBase = tarifBase
        Me.Langue = langue
        Me.Format = format

        Me.NbEntreesAdultes = nbAdultes
        Me.NbEntreesEnfants = nbEnfants
        Me.NbEntreesGroupeEnfants = nbGroupeEnfants

        ' Calcul du CA
        Me.CA_Adultes = nbAdultes * tarifAdulte
        Me.CA_Enfants = nbEnfants * tarifEnfant
        Me.CA_GroupeEnfants = nbGroupeEnfants * tarifGroupeEnfant
        Me.CA_Total = CA_Adultes + CA_Enfants + CA_GroupeEnfants
    End Sub


    ' --- Constructeur vide si nécessaire par EF, Sérialisation, etc. ---
    Public Sub New()
    End Sub
    Private Shared Function CreateRepository() As SeanceRepository
        Dim executor As ISqlExecutor =
    RepositoryFactory.CreateExecutor(Constantes.DataBases.Agumaaa)

        Return New SeanceRepository(executor)
    End Function
    Public Shared Function GetByFilm(idFilm As Integer) As List(Of Seance)
        Return CreateRepository().LireParFilm(idFilm)
    End Function

    Public Sub InsererSeance(seance As Seance)

        If seance.IdFilm <= 0 Then
            Throw New ArgumentException("L'identifiant du film (IdFilm) doit être valide.")
        End If

        If seance.DateHeureDebut = DateTime.MinValue Then
            Throw New ArgumentException("La date et l'heure de début sont obligatoires.")
        End If

        If seance.TarifBase < 0 Then
            Throw New ArgumentException("Le tarif de base doit être >= 0.")
        End If

        If seance.NbEntreesAdultes < 0 OrElse
       seance.NbEntreesEnfants < 0 OrElse
       seance.NbEntreesGroupeEnfants < 0 Then

            Throw New ArgumentException("Les nombres d’entrées ne peuvent pas être négatifs.")
        End If

        Dim lignes As Integer = CreateRepository().Inserer(seance)

        If lignes = 0 Then
            Throw New Exception("Aucune ligne insérée dans Seances.")
        End If

        Logger.INFO($"Séance insérée : film #{seance.IdFilm}, {seance.DateHeureDebut:dd/MM/yyyy HH:mm}")
    End Sub

    Public Sub Delete()
        If IdSeance = 0 Then Exit Sub

        Dim nb As Integer = CreateRepository().Supprimer(IdSeance)
        Logger.INFO($"Séance supprimée IdSeance={IdSeance}, lignes={nb}")
    End Sub
    ' Méthode Clone
    Public Function Clone() As Seance
        Return New Seance With {
            .IdSeance = Me.IdSeance,
            .IdFilm = Me.IdFilm,
            .DateHeureDebut = Me.DateHeureDebut,
            .NbEntreesAdultes = Me.NbEntreesAdultes,
            .NbEntreesEnfants = Me.NbEntreesEnfants,
            .NbEntreesGroupeEnfants = Me.NbEntreesGroupeEnfants,
            .CA_Adultes = Me.CA_Adultes,
            .CA_Enfants = Me.CA_Enfants,
            .CA_GroupeEnfants = Me.CA_GroupeEnfants,
            .CA_Total = Me.CA_Total
        }
    End Function
End Class
