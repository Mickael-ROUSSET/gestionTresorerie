Option Strict On
Option Explicit On

Public NotInheritable Class DemoDocumentSamples
    Private Sub New()
    End Sub

    Public Const Invoice As String =
        "{""type_document"":""Facture"",""donnees_extraites"":{" &
        """identite"":{""fournisseur_nom"":""Exemple fictif""}," &
        """transaction"":{""montant_ttc"":""42.50"",""devise"":""EUR""," &
        """date_facture"":""2026-04-01"",""numero_facture"":""DEMO-001""}}}"
    Public Const Cheque As String =
        "{""type_document"":""Cheque"",""donnees_extraites"":{" &
        """identite"":{""emetteur_numero_cheque"":""DEMO-002""}," &
        """transaction"":{""montant_numerique"":""18.75"",""devise"":""EUR""," &
        """date_emission"":""2026-04-02""}}}"
    Public Const Unknown As String =
        "{""type_document"":""Inconnu"",""donnees_extraites"":{""transaction"":{}}}"
    Public Const Invalid As String = "{invalide"
End Class
