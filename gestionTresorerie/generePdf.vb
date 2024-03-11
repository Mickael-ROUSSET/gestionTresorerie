'Imports System.Runtime.Serialization

'Module generePdf
'    Public Sub CreerEtat(ByVal unParametre As Integer)
'        ' Définir les variables Crystal Reports
'        Dim Etat As ReportDocument
'        Dim crExportOptions As ExportOptions
'        Dim crDiskFileDestinationOptions As DiskFileDestinationOptions
'        Dim Fichier As String

'        Dim tb As Table
'        Dim Para As New ParameterDiscreteValue
'        Dim paraCollection As New ParameterValues
'        Dim tliCurrent As CrystalDecisions.Shared.TableLogOnInfo 'objet pour connexion aux tables
'        Dim Req As String
'        Dim rsVa As JE
'        Dim valide As String

'        Para.Value = (unParametre)
'        paraCollection.Add(Para)
'        Etat = New ReportDocument
'        'chargement de l'état
'        Etat.Load(Application.StartupPath & "\Etat.rpt")
'        Etat.DataDefinition.ParameterFields("Champ_Para").ApplyCurrentValues(paraCollection)

'        Fichier = Application.StartupPath & unParametre & ".pdf"
'        crDiskFileDestinationOptions = New DiskFileDestinationOptions
'        crDiskFileDestinationOptions.DiskFileName = Fname
'        crExportOptions = Etat.ExportOptions
'        With crExportOptions
'            .DestinationOptions = crDiskFileDestinationOptions
'            .ExportDestinationType = ExportDestinationType.DiskFile
'            .ExportFormatType = ExportFormatType.PortableDocFormat
'        End With

'        For Each tb In Etat.Database.Tables
'            tliCurrent = tb.LogOnInfo 'creation de l'objet pour se connecter à la table
'            With tliCurrent.ConnectionInfo
'                .ServerName = source
'                .UserID = 'lutilisateur
'                .Password = le mot de passe
'                .DatabaseName = source
'            End With
'            tb.ApplyLogOnInfo(tliCurrent)
'        Next
'        Etat.Export()
'    End Sub
'End Module
