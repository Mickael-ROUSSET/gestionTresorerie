Imports System.Windows.Forms

Public Module AppelFrmSelectionUtils

    ''' <summary>
    ''' Ouvre une fenêtre de sélection générique et retourne l’entité choisie.
    ''' </summary>
    ''' <typeparam name="T">Type de l’entité (Categorie, SousCategorie, TypeDocImpl...)</typeparam>
    ''' <param name="nomRequete">Nom de la requête SQL à exécuter</param>
    ''' <param name="titreFenetre">Titre affiché dans la fenêtre</param>
    ''' <param name="txtDestination">TextBox à alimenter avec le libellé de l’entité sélectionnée</param>
    ''' <param name="champLibelle">Nom de la propriété à afficher dans la TextBox (par défaut "Libelle")</param>
    ''' <param name="parametres">Paramètres SQL éventuels</param>
    ''' <returns>L’objet sélectionné, ou Nothing si aucun choix</returns>
    Public Function OuvrirSelectionGenerique(Of T As {BaseDataRow, New})(
    nomRequete As String,
    titreFenetre As String,
    txtDestination As TextBox,
    Optional champLibelle As String = "Libelle",
    Optional parametres As Dictionary(Of String, Object) = Nothing
) As T

        Try
            Dim frm As New FrmSelectionGenerique(
            GetType(T),
            nomRequete,
            parametres,
            multiSelect:=False,
            lectureSeule:=True
        )

            frm.Text = titreFenetre

            If frm.ShowDialog() = DialogResult.OK AndAlso
           frm.ResultatsSelectionnes IsNot Nothing AndAlso
           frm.ResultatsSelectionnes.Count > 0 Then

                'Dim dr As DataRow = frm.ResultatsSelectionnes(0)
                'Dim entity As T = DataRowUtils.FromDataRowGeneric(Of T)(dr)
                Dim entity As T = CType(frm.ResultatsSelectionnes(0), T)


                If entity IsNot Nothing Then
                    ' Met à jour la TextBox si précisée
                    If txtDestination IsNot Nothing Then
                        Dim prop = GetType(T).GetProperty(champLibelle)
                        If prop IsNot Nothing Then
                            txtDestination.Text = prop.GetValue(entity)?.ToString()
                        End If
                    End If
                    Return entity
                End If
            End If

            Logger.INFO($"Aucun(e) {GetType(T).Name} sélectionné(e).")
            Return Nothing

        Catch ex As Exception
            Logger.ERR($"Erreur dans OuvrirSelectionGenerique({GetType(T).Name}) : {ex.Message}")
            Return Nothing
        End Try

    End Function

End Module

