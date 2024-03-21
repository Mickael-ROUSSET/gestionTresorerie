Imports System.Xml
Imports Windows.Win32.System
Public Class GereXml
    'Private Sub creeXml()
    '    'création d'une nouvelle instance du membre xmldocument
    '    Dim XmlDoc As XmlDocument = New XmlDocument()
    '    Dim raisonSociale As XmlElement 'raisonSociale pour le nœud [raisonSociale][/raisonSociale]
    '    Dim prénom As XmlElement 'ElemSite pour le nœud [URL][/URL]
    '    Dim nom As XmlElement 'ElemSite pour le nœud [NOM][/NOM]
    '    Dim categorieDefaut As XmlElement 'ElemSite pour le nœud [NOM][/NOM]
    '    Dim sousCategorieDefaut As XmlElement 'ElemSite pour le nœud [NOM][/NOM]

    '    'creation de la balise [raisonSociale][/raisonSociale]
    '    raisonSociale = XmlDoc.CreateElement("raisonSociale")
    '    prénom = XmlDoc.CreateElement("prénom")
    '    nom = XmlDoc.CreateElement("nom")

    '    raisonSociale.InnerText = " http://www.peuw.net/index.xml "
    '    prénom.InnerText = " peuw.net "
    'End Sub
    Public Shared Function LitXml(sFichier As String) As ListeTiers
        'creation d'une nouvelle instance du membre xmldocument
        Dim XmlDoc As New XmlDocument()
        Dim listetiers As XmlNodeList
        Dim noeudEnf As XmlNode

        Dim unTiers As New ClsTiers
        Dim desTiers As New ListeTiers

        'XmlDoc.Load(Application.StartupPath & "tiers.XML")
        XmlDoc.Load(sFichier)
        listetiers = XmlDoc.DocumentElement.GetElementsByTagName("listetiers")
        For Each tiers In listetiers
            For Each noeudEnf In tiers.ChildNodes
                If noeudEnf.LocalName = "raisonSociale" Then
                    unTiers.RaisonSociale = noeudEnf.InnerText
                Else
                    If (noeudEnf.LocalName = "nom") Then
                        unTiers.Nom = noeudEnf.InnerText
                    End If
                    If (noeudEnf.LocalName = "prénom") Then
                        unTiers.Prénom = noeudEnf.InnerText
                    End If
                    If (noeudEnf.LocalName = "catégorieDéfaut") Then
                        unTiers.CategorieDefaut = noeudEnf.InnerText
                    End If
                    If (noeudEnf.LocalName = "sousCatégorieDéfaut") Then
                        unTiers.SousCategorieDefaut = noeudEnf.InnerText
                    End If
                End If
                desTiers.Add(unTiers)
            Next
        Next
        Return desTiers
    End Function
End Class
