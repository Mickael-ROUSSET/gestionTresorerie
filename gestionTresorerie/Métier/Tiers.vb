Imports System.Data.SqlClient
Imports System.Runtime.InteropServices.JavaScript.JSType

Public Class Tiers

    Dim _id As Integer
    Dim _raisonSociale As String
    Dim _nom As String
    Dim _prénom As String
    Dim _categorieDefaut As Integer
    Dim _sousCategorieDefaut As Integer


    Public Sub New(sNom As String, sPrenom As String)
        If Not sNom Is Nothing Then
            _nom = sNom
            _prénom = sPrenom
        End If
    End Sub
    Public Sub New(sRaisonSociale As String)
        If Not sRaisonSociale Is Nothing Then
            _raisonSociale = sRaisonSociale
        End If
    End Sub
    Public Shared Function categorieTiers(idTiers As Double) As Integer
        ' Renvoie la catégorie et la sous catégorie d'un tiers 
        Dim maCmd As SqlCommand
        Dim monReader As SqlDataReader
        Dim iCategorie As Integer

        maCmd = New SqlCommand
        With maCmd
            FrmPrincipale.maConn.Open()
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
    Public Shared Function sousCategorieTiers(idTiers As Double) As Integer
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
End Class
