Namespace My
    ' The following events are available for MyApplication:
    ' Startup: Raised when the application starts, before the startup form is created.
    ' Shutdown: Raised after all application forms are closed.  This event is not raised if the application terminates abnormally.
    ' UnhandledException: Raised if the application encounters an unhandled exception.
    ' StartupNextInstance: Raised when launching a single-instance application and the application is already active. 
    ' NetworkAvailabilityChanged: Raised when the network connection is connected or disconnected.

    ' **NEW** ApplyApplicationDefaults: Raised when the application queries default values to be set for the application.

    ' Example:
    ' Private Sub MyApplication_ApplyApplicationDefaults(sender As Object, e As ApplyApplicationDefaultsEventArgs) Handles Me.ApplyApplicationDefaults
    '
    '   ' Setting the application-wide default Font:
    '   e.Font = New Font(FontFamily.GenericSansSerif, 12, FontStyle.Regular)
    '
    '   ' Setting the HighDpiMode for the Application:
    '   e.HighDpiMode = HighDpiMode.PerMonitorV2
    '
    '   ' If a splash dialog is used, this sets the minimum display time:
    '   e.MinimumSplashScreenDisplayTime = 4000
    ' End Sub

    Partial Friend Class MyApplication
        Private Sub ProvisionLocalDb(sender As Object, e As Microsoft.VisualBasic.ApplicationServices.StartupEventArgs) Handles Me.Startup
            If e.CommandLine.Count <> 1 OrElse
               Not String.Equals(e.CommandLine(0), "--provision-localdb", StringComparison.OrdinalIgnoreCase) Then Return

            e.Cancel = True
            SaveMySettingsOnExit = False
            Try
                Dim documents As New SqlDocumentTestStore()
                documents.Initialize()
            Catch
                System.Environment.ExitCode = 10
                Return
            End Try

            Try
                LocalDbCatalogs.EnsureProductionCatalog()
                System.Environment.ExitCode = 0
            Catch
                System.Environment.ExitCode = 11
            End Try
        End Sub

    End Class
End Namespace
