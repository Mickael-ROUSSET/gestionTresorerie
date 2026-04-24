Public Interface IAdresseRepository
    Function CreerAdresse(adresse As Coordonnees) As Integer
    Function LireAdressesParTiers(idTiers As Integer) As List(Of Coordonnees)
    Function MettreAJourAdresse(adresse As Coordonnees) As Boolean
    Function SupprimerAdresse(idAdresse As Integer) As Boolean
End Interface