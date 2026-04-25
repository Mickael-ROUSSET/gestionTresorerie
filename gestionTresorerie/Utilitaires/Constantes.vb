<DebuggerDisplay("{GetDebuggerDisplay(),nq}")>
Public NotInheritable Class Constantes

    ''' <summary>
    ''' Formats et Expressions régulières pour la validation des données.
    ''' </summary>
    Public NotInheritable Class Formats
        Public Const RegExMontant As String = "^[0-9]+(,[0-9]{0,2})*$"
        Public Const RegDateReleve As String = "^[0-3][0-9]/[0-9]{2}/[0-9]{4}"
        Public Const TimestampTrace As String = "yyyy-MM-dd HH:mm:ss"
    End Class

    ''' <summary>
    ''' Caractères et symboles typographiques utilisés dans l'application.
    ''' </summary>
    Public NotInheritable Class Symboles
        Public Const PointVirgule As String = ";"
        Public Const Virgule As String = ","
        Public Const Espace As String = " "
        Public Const Point As String = "."
        Public Const Tiret As String = "-"
        Public Const Euro As String = " €"
        Public Const Plus As String = "+"
    End Class

    ''' <summary>
    ''' Identifiants des Bases de Données.
    ''' </summary>
    Public NotInheritable Class DataBases
        Public Const Agumaaa As String = "bddAgumaaa"
        Public Const Cinema As String = "cinemaDB"
        Public Const MarcheDeNoel As String = "MarcheDeNoelDB"
    End Class

    ''' <summary>
    ''' Noms des procédures ou requêtes SQL classées par action.
    ''' </summary>
    Public NotInheritable Class Sql
        Public NotInheritable Class Deletion
            Public Const CptTiers As String = "cptTiers"
            Public Const Compte As String = "delCompte"
            Public Const Docs As String = "delDocs"
            Public Const Mvt As String = "delMvt"
        End Class

        Public NotInheritable Class Insertion
            Public Const Compte As String = "insertCompte"
            Public Const DocAgumaaa As String = "insertDocAgumaaa"
            Public Const Mvts As String = "insertMvts"
            Public Const Tiers As String = "insertTiers"
        End Class

        Public NotInheritable Class Selection
            Public Const MvtsIdentiques As String = "procMvtsIdentiques"
            Public Const CategoriesDefautMouvements As String = "reqCategoriesDefautMouvements"
            Public Const TiersPhysique As String = "selTiersPhysique"
            Public Const TiersMorale As String = "selTiersMorale"
            ' ... Ajoute les autres ici de la même manière ...
            Public Const TypesDocuments As String = "reqLibellesTypesDocuments"
            Public Const SeancesAvecFilm As String = "SeancesAvecFilm"
        End Class

        Public NotInheritable Class Update
            Public Const Compate As String = "updCompte"
            Public Const Docs As String = "updDocs"
            Public Const Mvt As String = "updMvt"
            Public Const MvtIdDoc As String = "updMvtIdDoc"
            'Mises à jour
            Public Const sqlUpdCompte As String = "updCompte"
            Public Const sqlUpdDocs As String = "updDocs"
            Public Const sqlUpdMvt As String = "updMvt"
            Public Const sqlUpdMvtIdDoc As String = "updMvtIdDoc"
        End Class
    End Class

    ''' <summary>
    ''' Paramètres liés à l'environnement de l'application.
    ''' </summary>
    Public NotInheritable Class Environnement
        Public Const NiveauLog As String = "niveauLog"
        Public Const DicoTypeMvt As String = "dicoTypeMvt"
    End Class

    Private Shared Function GetDebuggerDisplay() As String
        Return "Conteneur de Constantes Globales"
    End Function
    Public Class Dossiers
        Public Const Sauvegardes As String = "D:\Sauvegardes"
    End Class
End Class