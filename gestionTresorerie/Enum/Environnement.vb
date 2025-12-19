Imports System.ComponentModel

Public Enum Environnement
    <Description("Prod")>
    Prod = 1

    <Description("Test")>
    Test = 2
End Enum
Public Module EnvironnementLibelles
    Public ReadOnly Libelles As New Dictionary(Of Environnement, String) From {
        {Environnement.Prod, "Prod"},
        {Environnement.Test, "Test"}
    }
End Module
