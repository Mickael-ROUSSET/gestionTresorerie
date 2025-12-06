Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema

<Table("AdherantsGym", Schema:="Gym")>
Public Class AdherantGym

    <Key>
    Public Property IdTiers As Integer

    <Required(ErrorMessage:="Le lieu de naissance est obligatoire")>
    <StringLength(100, ErrorMessage:="Le lieu de naissance ne peut pas dépasser 100 caractères")>
    Public Property LieuNaissance As String

    <Required(ErrorMessage:="L'adresse est obligatoire")>
    Public Property IdAdresse As Integer

    <Required(ErrorMessage:="Le cours est obligatoire")>
    <StringLength(50, ErrorMessage:="Le cours ne peut pas dépasser 50 caractères")>
    Public Property Cours As String

    <Required(ErrorMessage:="Le questionnaire est obligatoire")>
    Public Property Questionnaire As Boolean

    <Required(ErrorMessage:="Le paiement est obligatoire")>
    <Range(0, Double.MaxValue, ErrorMessage:="Le paiement doit être positif")>
    Public Property Paiement As Decimal

    <StringLength(20, ErrorMessage:="Le paiement en espèce ne peut pas dépasser 20 caractères")>
    Public Property Espece As String

    <StringLength(20, ErrorMessage:="Le numéro de chèque ne peut pas dépasser 20 caractères")>
    Public Property NumeroCheque As String

    <Required(ErrorMessage:="Le formulaire d'adhésion est obligatoire")>
    Public Property FormulaireAdhesion As Boolean

    Public Property NumLicence As Boolean

    ' --- Constructeur par défaut ---
    Public Sub New()
        LieuNaissance = String.Empty
        Cours = String.Empty
        Paiement = String.Empty
        NumeroCheque = String.Empty
        Questionnaire = False
        FormulaireAdhesion = False
        NumLicence = 0
    End Sub
End Class

