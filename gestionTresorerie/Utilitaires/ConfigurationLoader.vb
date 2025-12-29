Public Class ConfigurationLoader
    ' Constantes pour éviter les fautes de frappe dans les noms de variables
    Private Const ENV_GEMINI_KEY As String = "AGUMAAA_GEMINI_API_KEY"
    Private Const ENV_GMAIL_PASS As String = "SmtpPassword"
    Private Const ENV_CLE_TMDB As String = "cleApiTmdb"
    Private Const ENV_CLE_OPENAI As String = "OPENAI_API_KEY"
    Private Const ENV_CLE_MISTRAL As String = "cleApiMistralcleApiMistral"
    Private Const ENV_CLE_ALEATOIRE As String = "selAleatoire"

    ''' <summary>
    ''' Charge tous les secrets nécessaires au fonctionnement d'Agumaaa
    ''' </summary>
    Public Shared Function LoadSecrets() As (GeminiKey As String, GmailPass As String, GCleTmdb As String)
        Dim gKey = Environment.GetEnvironmentVariable(ENV_GEMINI_KEY, EnvironmentVariableTarget.User)
        'Dim sConn = Environment.GetEnvironmentVariable(ENV_SQL_CONN, EnvironmentVariableTarget.User)
        Dim gPass = Environment.GetEnvironmentVariable(ENV_GMAIL_PASS, EnvironmentVariableTarget.User)
        Dim gCleTmdb = Environment.GetEnvironmentVariable(ENV_CLE_TMDB, EnvironmentVariableTarget.User)

        ' Vérification de présence
        If String.IsNullOrEmpty(gKey) Or String.IsNullOrEmpty(gPass) Or String.IsNullOrEmpty(gCleTmdb) Then
            Throw New Exception("Certains secrets sont manquants dans les variables d'environnement (AGUMAAA_...).")
            Logger.ERR($"gKey : {gKey}, gPass : {gPass }, gCleTmdb : {gCleTmdb }")
        End If

        Return (gKey, gPass, gCleTmdb)
    End Function
End Class