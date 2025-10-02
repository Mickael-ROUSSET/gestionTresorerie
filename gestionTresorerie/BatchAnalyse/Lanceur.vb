

Imports System.Data.SqlClient
Imports System.Reflection

Public Class Lanceur
    Private Shared iTypeDocument As String

    Public Shared Sub LanceTrt()
        ' Sélectionner les types de document (classes) avec sqlTypesDocuments
        For Each typeDoc In GetTypesDocument()
            Try
                ' Créer une instance de la classe spécifiée par sClasse via réflexion
                'Dim typeClasse As Type = Type.GetType("gestionTresorerie." & typeDoc.classe)
                ' Si Type.GetType échoue, essayer de chercher dans l'assembly courant
                'If typeClasse Is Nothing Then
                '    typeClasse = Assembly.GetExecutingAssembly().GetType("gestionTresorerie." & typeDoc.classe)
                '    If typeClasse Is Nothing Then
                '        Logger.WARN("Classe " & "gestionTresorerie." & typeDoc.classe & " non trouvée dans l'assembly courant.")
                '        Continue For
                '    End If
                'End If


                'Dim instanceClasse As Object = Activator.CreateInstance(typeDoc)
                Dim batchAnalyse = New batchAnalyse(typeDoc)

                ' Appel de l'analyse des fichiers avec le type d'analyse défini dans le constructeur
                batchAnalyse.ParcourirRepertoireEtAnalyser()
            Catch ex As Exception
                Logger.ERR("Erreur lors de l'instanciation ou de l'analyse pour la classe " & "gestionTresorerie." & typeDoc.classe & " : " & ex.Message)
            End Try
        Next typeDoc
    End Sub
    Private Shared Function GetTypesDocument() As List(Of ITypeDoc)
        Dim listTypeDoc As New List(Of ITypeDoc)

        Using reader As SqlDataReader = SqlCommandBuilder.CreateSqlCommand("sqlTypesDocuments").ExecuteReader()
            ' Vérifier si le reader contient des lignes
            If reader.HasRows Then
                While reader.Read()
                    ' Créer une instance concrète implémentant ITypeDoc
                    Dim typeDoc As New TypeDocImpl(reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetString(3))
                    listTypeDoc.Add(typeDoc)
                End While
                Logger.INFO("Types de documents récupérés avec succès.")
            Else
                ' Gérer le cas où le reader est vide
                Logger.WARN("Aucun type de document trouvé.")
            End If
        End Using

        Return listTypeDoc
    End Function
End Class