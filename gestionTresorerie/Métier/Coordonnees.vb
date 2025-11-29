Imports System.Data.SqlClient
Imports System.Text.RegularExpressions

Public Class Coordonnees
    Inherits BaseDataRow

    ' --- Propriétés ---
    Public Property Id As Integer
    Public Property IdTiers As Integer
    Public Property Rue1 As String
    Public Property Rue2 As String
    Public Property CodePostal As String
    Public Property Ville As String
    Public Property Pays As String
    Public Property Email As String
    Public Property Telephone As String

    ' --- Constructeur principal ---
    Public Sub New()
    End Sub

    Public Sub New(
        idTiers As Integer,
        Optional rue1 As String = Nothing,
        Optional rue2 As String = Nothing,
        Optional cp As String = Nothing,
        Optional nomCommune As String = Nothing,
        Optional pays As String = Nothing,
        Optional email As String = Nothing,
        Optional telephone As String = Nothing)

        Me.IdTiers = idTiers
        Me.Rue1 = rue1?.Trim()
        Me.Rue2 = rue2?.Trim()
        Me.CodePostal = cp?.Trim()
        Me.Ville = nomCommune?.Trim()
        Me.Pays = pays?.Trim()
        Me.Email = email?.Trim()
        Me.Telephone = telephone?.Trim()

        Validate()
    End Sub

    ''' <summary>
    ''' Valide l'email et le téléphone si fournis
    ''' </summary>
    '''     
    Public Overrides Sub LoadFromReader(reader As SqlDataReader)
        Id = CInt(reader("Id"))
        IdTiers = CInt(reader("IdTiers"))
        Rue1 = reader("Rue1").ToString()
        Rue2 = reader("Rue2").ToString()
        CodePostal = reader("CodePostal").ToString()
        Ville = reader("Ville").ToString()
        Pays = reader("Pays").ToString()
        Email = reader("Email").ToString()
        Telephone = reader("Telephone").ToString()
    End Sub
    Public Sub Validate()
        If Not ValiderEmail(Email) Then
            Throw New ArgumentException($"Email invalide : {Email}")
        End If

        If Not ValiderTelephone(Telephone) Then
            Throw New ArgumentException($"Téléphone invalide : {Telephone}")
        End If

        If Not ValiderCP(CodePostal) Then
            Throw New ArgumentException($"Code postal invalide : {CodePostal}")
        End If
    End Sub
    Public Shared Function ValiderCP(cp As String) As Boolean
        If String.IsNullOrWhiteSpace(cp) Then Return True   ' champ non obligatoire

        Dim regex As New Regex("^\d{5}$")   ' Format : 5 chiffres
        Return regex.IsMatch(cp)
    End Function

    Public Shared Function ValiderEmail(email As String) As Boolean
        If String.IsNullOrWhiteSpace(email) Then Return True
        Dim regex As New Regex("^[\w\.-]+@[\w\.-]+\.\w+$")
        Return regex.IsMatch(email)
    End Function

    Public Shared Function ValiderTelephone(telephone As String) As Boolean
        If String.IsNullOrWhiteSpace(telephone) Then Return True
        Dim regex As New Regex("^\+?[0-9\s\-]{4,20}$")
        Return regex.IsMatch(telephone)
    End Function

    ''' <summary>
    ''' Insère ou met à jour la coordonnée en base
    ''' </summary>
    Public Sub InsererOuMettreAJour()
        If IdTiers = 0 Then Throw New InvalidOperationException("IdTiers doit être renseigné.")
        Validate()

        ' Vérifie si la coordonnée existe
        Dim existe As Boolean
        Using cmdCheck As SqlCommand = SqlCommandBuilder.CreateSqlCommand(
            Constantes.bddAgumaaa, "existeCoordonnee", New Dictionary(Of String, Object) From {
            {"@IdTiers", Me.IdTiers}
            })
            Using rdr = cmdCheck.ExecuteReader()
                existe = rdr.HasRows
            End Using
        End Using

        Dim param As New Dictionary(Of String, Object) From {
            {"@IdTiers", Me.IdTiers},
            {"@Rue1", If(String.IsNullOrWhiteSpace(Me.Rue1), DBNull.Value, Me.Rue1)},
            {"@Rue2", If(String.IsNullOrWhiteSpace(Me.Rue2), DBNull.Value, Me.Rue2)},
            {"@CodePostal", If(String.IsNullOrWhiteSpace(Me.CodePostal), DBNull.Value, Me.CodePostal)},
            {"@Ville", If(String.IsNullOrWhiteSpace(Me.Ville), DBNull.Value, Me.Ville)},
            {"@Pays", If(String.IsNullOrWhiteSpace(Me.Pays), DBNull.Value, Me.Pays)},
            {"@Email", If(String.IsNullOrWhiteSpace(Me.Email), DBNull.Value, Me.Email)},
            {"@Telephone", If(String.IsNullOrWhiteSpace(Me.Telephone), DBNull.Value, Me.Telephone)}
        }

        If existe Then
            ' Mise à jour
            Using cmdUpdate = SqlCommandBuilder.CreateSqlCommand(Constantes.bddAgumaaa, "updateCoordonnees", param)
                Dim nb = cmdUpdate.ExecuteNonQuery()
                Logger.INFO($"Coordonnée mise à jour pour IdTiers={IdTiers}, lignes affectées={nb}")
            End Using
        Else
            ' Insertion
            Using cmdInsert = SqlCommandBuilder.CreateSqlCommand(Constantes.bddAgumaaa, "insertCoordonnees", param)
                Dim nb = cmdInsert.ExecuteNonQuery()
                Logger.INFO($"Coordonnée insérée pour IdTiers={IdTiers}, lignes affectées={nb}")
            End Using
        End If
    End Sub

    ' --- Retourne une chaîne descriptive ---
    Public Overrides Function ToString() As String
        Return $"[{Rue1} {Rue2}, {CodePostal} {Ville}, {Pays} | Email: {Email}, Tel: {Telephone}"
    End Function

End Class
