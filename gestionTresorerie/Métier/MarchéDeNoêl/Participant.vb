Imports System.Data.SqlClient

Public Class Participant

    Public Property IdTiers As Integer
    Public Property Nom As String
    Public Property Prenom As String
    Public Property RaisonSociale As String
    Public Property Activite As String
    Public Property Adresse As String
    Public Property CP As String
    Public Property Ville As String
    Public Property Grille As String
    Public Property Tables As String
    Public Property DateCourrier As Date
    Public Property Paiement As String
    Public Property Siret As String
    Public Property Tel As String
    Public Property Mail As String
    Public Property Type As String   ' Commerçant / particulier
    Public Property Sexe As String
    Public Property PieceIdentite As String
    Public Property DateNaissance As String
    Public Property LieuNaissance As String

    ' --- Constructeur ---
    Public Sub New(reader As SqlDataReader)

        Me.Nom = GetValue(Of String)(reader, "Nom")
        Me.Prenom = GetValue(Of String)(reader, "Prenom")
        Me.RaisonSociale = GetValue(Of String)(reader, "RaisonSociale")
        Me.Activite = GetValue(Of String)(reader, "Activite")
        Me.Adresse = GetValue(Of String)(reader, "Coordonnees")
        Me.CP = GetValue(Of String)(reader, "CP")
        Me.Ville = GetValue(Of String)(reader, "Ville")
        Me.Grille = GetValue(Of String)(reader, "Grille")
        Me.Tables = GetValue(Of String)(reader, "Tables")
        Me.DateCourrier = GetValue(Of Date)(reader, "DateCourrier")
        Me.Paiement = GetValue(Of String)(reader, "Paiement")
        Me.Siret = GetValue(Of String)(reader, "Siret")
        Me.Tel = GetValue(Of String)(reader, "Tel")
        Me.Mail = GetValue(Of String)(reader, "Mail")
        Me.Type = GetValue(Of String)(reader, "Type")
        Me.Sexe = GetValue(Of String)(reader, "Sexe")
        Me.PieceIdentite = GetValue(Of String)(reader, "PieceIdentite")
        Me.DateNaissance = GetValue(Of String)(reader, "DateNaissance")
        Me.LieuNaissance = GetValue(Of String)(reader, "LieuNaissance")

    End Sub
    Private Function GetValue(Of T)(reader As SqlDataReader, column As String) As T
        Dim value = reader(column)
        If IsDBNull(value) Then
            Return Nothing
        End If
        Return CType(value, T)
    End Function


End Class
