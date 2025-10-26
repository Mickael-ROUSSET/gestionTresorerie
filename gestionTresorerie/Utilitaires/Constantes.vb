<DebuggerDisplay("{GetDebuggerDisplay(),nq}")>
Public Class Constantes
    Public Const regExMontant As String = "^[0-9]+(,[0-9]{0,2})*$"
    Public Const regDateReleve As String = "^[0-3][0-9]/[0-9]{2}/[0-9]{4}"
    Public Const formatTimestampTrace As String = "yyyy-MM-dd HH:mm:ss"
    Public Const pointVirgule As String = ";"
    Public Const virgule As String = ","
    Public Const espace As String = " "
    Public Const point As String = "."
    Public Const tiret As String = "-"
    Public Const euro As String = " €"
    Public Const plus As String = "+"

    'Requêtes SQL
    '------------
    'Suppressions
    Public Const sqlCptTiers As String = "cptTiers"
    Public Const sqlDelCompte As String = "delCompte"
    Public Const sqlDelDocs As String = "delDocs"
    Public Const sqlDelMvt As String = "delMvt"
    'Insertions 
    Public Const sqlInsertCompte As String = "insertCompte"
    Public Const sqlInsertDocAgumaaa As String = "insertDocAgumaaa"
    Public Const sqlInsertMvts As String = "insertMvts"
    Public Const sqlinsertTiers As String = "insertTiers"
    'Sélections
    Public Const sqlProcMvtsIdentiques As String = "procMvtsIdentiques"
    Public Const sqlSelCategoriesDefautMouvements As String = "reqCategoriesDefautMouvements"
    Public Const sqlSelCategoriesMouvements As String = "reqCategoriesMouvements"
    Public Const sqlSelSousCategories As String = "reqSousCategorie"
    Public Const sqlSelSousCategoriesDefautMouvements As String = "reqSousCategoriesDefautMouvements"
    Public Const sqlSelSousCategoriesTout As String = "sqlSelSousCategoriesTout"
    Public Const sqlSelCategoriesTout As String = "reqCategorieTout"
    Public Const sqlSelChq As String = "reqChq"
    Public Const sqlSelDocs As String = "reqDocs"
    Public Const sqlSelIdentiteCatTiers As String = "reqIdentiteCatTiers"
    Public Const sqlSelIdentiteTiers As String = "reqIdentiteTiers"
    Public Const sqlSelImagesChq As String = "reqImagesChq"
    Public Const sqlSelNbMouvements As String = "reqNbMouvements"
    Public Const sqlSelSommeCatMouvements As String = "reqSommeCatMouvements"
    Public Const sqlSelTiersMorale As String = "selTiersMorale"
    Public Const sqlSelTiersPhysique As String = "selTiersPhysique"
    Public Const sqlSelSelectMouvementsLibelles As String = "sqlSelectMouvementsLibelles"
    Public Const sqlSelLibCat As String = "reqLibCat"
    Public Const sqlSelTypes As String = "reqTypes"
    Public Const sqlSelEvenement As String = "reqEvenement"
    Public Const sqlSelTypesDocuments As String = "reqLibellesTypesDocuments"
    'Mises à jour
    Public Const sqlUpdCompte As String = "updCompte"
    Public Const sqlUpdDocs As String = "updDocs"
    Public Const sqlUpdMvt As String = "updMvt"
    Public Const sqlUpdMvtIdDoc As String = "updMvtIdDoc"

    'Environnement
    Public Const paramNiveauLog As String = "niveauLog"
    Public Const dicoTypeMvt As String = "dicoTypeMvt"

    Private Function GetDebuggerDisplay() As String
        Return ToString()
    End Function
End Class