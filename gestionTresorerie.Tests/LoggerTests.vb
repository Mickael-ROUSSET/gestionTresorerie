Imports System.Diagnostics
Imports System.IO
Imports System.Reflection
Imports gestionTresorerie
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
<DoNotParallelize>
Public Class LoggerTests
    <TestMethod>
    <Timeout(10000)>
    Public Sub ErreurEcriture_JournaliseSansBloquer()
        ' Isoler le chemin sans charger la configuration d'installation réelle.
        Dim champ = GetType(LectureProprietes).GetField("_repInstallation", BindingFlags.NonPublic Or BindingFlags.Static)
        Dim ancienneValeur = champ.GetValue(Nothing)
        Dim repertoireAbsent = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()) & Path.DirectorySeparatorChar
        Using sortie As New StringWriter()
            Using listener As New TextWriterTraceListener(sortie)
                Trace.Listeners.Add(listener)
                Try
                    champ.SetValue(Nothing, repertoireAbsent)
                    Logger.ERR("Test de non-régression : chemin de log inexistant.")
                    listener.Flush()
                    StringAssert.Contains(sortie.ToString(), "Erreur lors de l'écriture dans le fichier de trace")
                    Assert.IsFalse(Directory.Exists(repertoireAbsent))
                Finally
                    champ.SetValue(Nothing, ancienneValeur)
                    Trace.Listeners.Remove(listener)
                End Try
            End Using
        End Using
    End Sub
End Class
