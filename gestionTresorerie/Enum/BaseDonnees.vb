Imports System.ComponentModel

Public Enum BaseDonnees
    <Description("Prod")>
    bddAgumaaa = 1

    <Description("Test")>
    bddAgumaaaTest = 2
End Enum
Public Module BaseDonneesLibelles
    Public ReadOnly Libelles As New Dictionary(Of BaseDonnees, String) From {
        {BaseDonnees.bddAgumaaa, "bddAgumaaa"},
        {BaseDonnees.bddAgumaaaTest, "bddAgumaaaTest"}
    }
End Module
