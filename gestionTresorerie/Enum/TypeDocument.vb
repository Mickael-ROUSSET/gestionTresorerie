Imports System.ComponentModel
Namespace Agumaaa
    Public Enum TypeDocument
        <Description("Aucun")>
        Aucun = 0
        <Description("Cheque")>
        Cheque = 1
        <Description("Cheque")>
        FormulaireAdhesion = 2
        <Description("QuestionnaireMedical")>
        QuestionnaireMedical = 3
        <Description("Facture")>
        Facture = 4
        <Description("Recu")>
        Recu = 5
    End Enum
    Public Module TypeDocumentLibelles
        Public ReadOnly Libelles As New Dictionary(Of TypeDocument, String) From {
            {TypeDocument.Aucun, "Aucun"},
            {TypeDocument.Cheque, "Cheque"},
            {TypeDocument.FormulaireAdhesion, "Cheque"},
            {TypeDocument.QuestionnaireMedical, "QuestionnaireMedical"},
            {TypeDocument.Facture, "Facture"},
            {TypeDocument.Recu, "Recu"}
        }
    End Module
End Namespace
