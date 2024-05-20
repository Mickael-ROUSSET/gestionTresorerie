Imports System.Data.SqlClient
Imports System.Runtime.InteropServices.JavaScript.JSType

Public Class Tiers

    Dim _id As Integer
    Dim _raisonSociale As String
    Dim _nom As String
    Dim _prénom As String
    Dim _categorieDefaut As Integer
    Dim _sousCategorieDefaut As Integer
    Dim _dateCreation As Date
    Dim _dateModification As Date

    Public Sub New(id As Integer, sNom As String, sPrenom As String, Optional sCategorie As Integer = 0, Optional sSousCategorie As Integer = 0)
        If sNom IsNot Nothing Then
            _id = id
            _nom = sNom
            _prénom = sPrenom
            _categorieDefaut = sCategorie
            _sousCategorieDefaut = sSousCategorie
        End If
    End Sub
    Public Sub New(id As Integer, sRaisonSociale As String, Optional sCategorie As Integer = 0, Optional sSousCategorie As Integer = 0)
        If sRaisonSociale IsNot Nothing Then
            _id = id
            _raisonSociale = sRaisonSociale
            _categorieDefaut = sCategorie
            _sousCategorieDefaut = sSousCategorie
        End If
    End Sub
    Public Shared Function getCategorieTiers(idTiers As Double) As Integer
        ' Renvoie la catégorie et la sous catégorie d'un tiers 
        Dim maCmd As SqlCommand
        Dim monReader As SqlDataReader
        Dim iCategorie As Integer

        maCmd = New SqlCommand
        With maCmd
            'FrmPrincipale.maConn.Open()
            .Connection = FrmPrincipale.maConn
            .CommandText = "SELECT categorieDefaut FROM Tiers where id = '" & idTiers & "';"
        End With
        monReader = maCmd.ExecuteReader()
        Do While monReader.Read()
            Try
                iCategorie = monReader.GetInt32(0)
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try
        Loop
        'TODO : gérer les réponses multiples éventuelles
        monReader.Close()
        Return iCategorie
    End Function
    Public Shared Function getSousCategorieTiers(idTiers As Double) As Integer
        ' Renvoie la catégorie et la sous catégorie d'un tiers 
        Dim maCmd As SqlCommand
        Dim monReader As SqlDataReader
        Dim iSousCategorie As Integer

        maCmd = New SqlCommand
        With maCmd
            .Connection = FrmPrincipale.maConn
            .CommandText = "SELECT sousCategorieDefaut FROM Tiers where id = '" & idTiers & "';"
        End With
        monReader = maCmd.ExecuteReader()
        Do While monReader.Read()
            Try
                iSousCategorie = monReader.GetInt32(0)
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try
        Loop
        'TODO : gérer les réponses multiples éventuelles
        monReader.Close()
        Return iSousCategorie
    End Function
    Public ReadOnly Property id As Integer
        Get
            Return _id
        End Get
    End Property
    Public Property RaisonSociale As String
        Get
            Return _raisonSociale
        End Get
        Set(value As String)
            _raisonSociale = value
        End Set
    End Property

    Public Property Nom As String
        Get
            Return _nom
        End Get
        Set(value As String)
            _nom = value
        End Set
    End Property

    Public Property Prénom As String
        Get
            Return _prénom
        End Get
        Set(value As String)
            _prénom = value
        End Set
    End Property

    Public Property CategorieDefaut As Integer
        Get
            Return _categorieDefaut
        End Get
        Set(value As Integer)
            _categorieDefaut = value
        End Set
    End Property

    Public Property SousCategorieDefaut As Integer
        Get
            Return _sousCategorieDefaut
        End Get
        Set(value As Integer)
            _sousCategorieDefaut = value
        End Set
    End Property

    Public Property dateCreation As Date
        Get
            Return _dateCreation
        End Get
        Set()
            _dateCreation = Now
        End Set
    End Property
    Public Property dateModification As Date
        Get
            Return _dateModification
        End Get
        Set()
            _dateModification = Now
        End Set
    End Property

End Class
