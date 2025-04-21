Public Class GereXml
    'Public Shared Function LitXml(sFichier As String) As ListeTiers
    '    'creation d'une nouvelle instance du membre xmldocument
    '    Dim XmlDoc As New XmlDocument()
    '    Dim listetiers As XmlNodeList
    '    Dim noeudEnf As XmlNode
    '    Dim unTiers As Tiers

    '    Dim desTiers As New ListeTiers
    '    'TODO : à définir correctement
    '    Const CATEGORIE_DEFAUT As String = "CATEGORIE_DEFAUT"
    '    Const SOUS_CATEGORIE_DEFAUT As String = "SOUS_CATEGORIE_DEFAUT"

    '    XmlDoc.Load(sFichier)
    '    listetiers = XmlDoc.DocumentElement.GetElementsByTagName("tiers")
    '    For Each tiers In listetiers
    '        unTiers = New Tiers
    '        For Each noeudEnf In tiers.ChildNodes
    '            If noeudEnf.LocalName = "raisonSociale" Then
    '                unTiers.RaisonSociale = noeudEnf.InnerText
    '            Else
    '                If (noeudEnf.LocalName = "nom") Then
    '                    unTiers.Nom = noeudEnf.InnerText
    '                End If
    '                If (noeudEnf.LocalName = "prenom") Then
    '                    unTiers.Prénom = noeudEnf.InnerText
    '                End If
    '                If (noeudEnf.LocalName = "categorieDefaut") Then
    '                    If noeudEnf.InnerText <> "" Then
    '                        unTiers.CategorieDefaut = noeudEnf.InnerText
    '                    Else
    '                        unTiers.CategorieDefaut = CATEGORIE_DEFAUT
    '                    End If
    '                End If
    '                If (noeudEnf.LocalName = "sousCategorieDefaut") Then
    '                    If noeudEnf.InnerText <> "" Then
    '                        unTiers.SousCategorieDefaut = noeudEnf.InnerText
    '                    Else
    '                        unTiers.SousCategorieDefaut = SOUS_CATEGORIE_DEFAUT
    '                    End If
    '                End If
    '            End If
    '        Next
    '        desTiers.Add(unTiers)
    '    Next
    '    Return desTiers
    'End Function
End Class
