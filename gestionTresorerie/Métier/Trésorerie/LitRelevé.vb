Imports System.Globalization
Imports System.IO
Imports System.Security
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Windows.Forms

Public Class LigneReleve
    Public Property DateOperation As DateTime
    Public Property Libelle As String
    Public Property MontantCredit As Decimal?
    Public Property MontantDebit As Decimal?
End Class

Public Module LitRelevé

    Private Const FILTRE_CSV As String = "Fichiers CSV (*.csv)|*.csv"
    Private ReadOnly CultureFr As CultureInfo = CultureInfo.GetCultureInfo("fr-FR")
    Private ReadOnly RegexMontant As New Regex("([+-]?\s*\d{1,3}(?:[\s\u00A0]?\d{3})*,\d{2})\s*€?", RegexOptions.Compiled)

    Public Function OuvreFichier() As String
        Dim fichierTraite As String = LectureProprietes.GetCheminEtVariable("ficRelevéTraité")

        Using dialogue As New OpenFileDialog With {
            .FileName = "Sélectionner un fichier csv",
            .Filter = FILTRE_CSV,
            .Title = "Ouvrir un relevé bancaire",
            .Multiselect = False
        }
            If dialogue.ShowDialog() <> DialogResult.OK Then
                Return fichierTraite
            End If

            Try
                NormaliserFichierReleve(dialogue.FileName, fichierTraite)
                Return fichierTraite

            Catch ex As SecurityException
                Logger.ERR($"Erreur de sécurité lors de l'ouverture du relevé : {ex.Message}")
                Throw

            Catch ex As Exception
                Logger.ERR($"Erreur lors du traitement du relevé '{dialogue.FileName}' : {ex.Message}")
                Throw
            End Try
        End Using
    End Function

    Public Function OuvreEtParseFichier() As List(Of LigneReleve)
        Using dialogue As New OpenFileDialog With {
            .FileName = "Sélectionner un fichier csv",
            .Filter = FILTRE_CSV,
            .Title = "Ouvrir un relevé bancaire",
            .Multiselect = False
        }
            If dialogue.ShowDialog() <> DialogResult.OK Then
                Return New List(Of LigneReleve)
            End If

            Return ParserFichier(dialogue.FileName)
        End Using
    End Function

    Public Function ParserFichier(fichierSource As String) As List(Of LigneReleve)

        If String.IsNullOrWhiteSpace(fichierSource) Then
            Throw New ArgumentException("Le fichier source est obligatoire.", NameOf(fichierSource))
        End If

        If Not File.Exists(fichierSource) Then
            Throw New FileNotFoundException("Le fichier source du relevé est introuvable.", fichierSource)
        End If

        Dim lignesList As List(Of String) = LireFichierAvecEncodageAuto(fichierSource)

        Return ParserLignes(lignesList)

    End Function
    Private Function EstFormatCsvStructure(lignes As IEnumerable(Of String)) As Boolean
        Return lignes.Any(Function(l)
                              Return l IsNot Nothing AndAlso
                                 l.Trim().StartsWith("Date;Libellé;Débit euros;Crédit euros",
                                                     StringComparison.OrdinalIgnoreCase)
                          End Function)
    End Function
    Private Function ParserCsvStructure(lignes As IEnumerable(Of String)) As List(Of LigneReleve)
        Dim resultat As New List(Of LigneReleve)
        Dim commencer As Boolean = False

        Dim enregistrements As List(Of String) = RegrouperEnregistrementsCsv(lignes)

        For Each ligneBrute As String In enregistrements
            Dim ligne As String = If(ligneBrute, String.Empty).Trim()

            If String.IsNullOrWhiteSpace(ligne) Then Continue For

            If ligne.StartsWith("Date;Libellé;Débit euros;Crédit euros", StringComparison.OrdinalIgnoreCase) Then
                commencer = True
                Continue For
            End If

            If Not commencer Then Continue For
            If Not EstDebutMouvement(ligne) Then Continue For

            Dim colonnes As List(Of String) = SplitCsvPointVirgule(ligne)

            If colonnes.Count < 4 Then Continue For

            Dim item As New LigneReleve With {
            .DateOperation = DateTime.ParseExact(colonnes(0).Trim(), "dd/MM/yyyy", CultureFr),
            .Libelle = NettoyerLibelle(colonnes(1))
        }

            If Not String.IsNullOrWhiteSpace(colonnes(2)) Then
                item.MontantDebit = ExtraireMontant(colonnes(2))
            End If

            If Not String.IsNullOrWhiteSpace(colonnes(3)) Then
                item.MontantCredit = ExtraireMontant(colonnes(3))
            End If

            If item.MontantDebit.HasValue OrElse item.MontantCredit.HasValue Then
                resultat.Add(item)
            End If
        Next

        Return resultat
    End Function
    Private Function SplitCsvPointVirgule(ligne As String) As List(Of String)
        Dim resultat As New List(Of String)
        Dim courant As New StringBuilder()
        Dim dansGuillemets As Boolean = False

        For i As Integer = 0 To ligne.Length - 1
            Dim c As Char = ligne(i)

            If c = """"c Then
                dansGuillemets = Not dansGuillemets
            ElseIf c = ";"c AndAlso Not dansGuillemets Then
                resultat.Add(courant.ToString().Trim())
                courant.Clear()
            Else
                courant.Append(c)
            End If
        Next

        resultat.Add(courant.ToString().Trim())

        Return resultat
    End Function

    Public Function ParserLignes(lignesSource As IEnumerable(Of String)) As List(Of LigneReleve)
        If lignesSource Is Nothing Then
            Throw New ArgumentNullException(NameOf(lignesSource))
        End If

        Dim lignesReleve As New List(Of LigneReleve)
        Dim ligneCourante As LigneReleve = Nothing
        Dim buffer As New StringBuilder()
        Dim libelleCourant As New StringBuilder()

        Dim lignesList As List(Of String) = lignesSource.ToList()

        If EstFormatCsvStructure(lignesList) Then
            Return ParserCsvStructure(lignesList)
        End If
        For Each ligneBrute As String In lignesSource
            Dim ligne As String = If(ligneBrute, String.Empty).Trim()

            If String.IsNullOrWhiteSpace(ligne) Then
                Continue For
            End If

            If EstDebutMouvement(ligne) Then
                AjouterLigneSiValide(lignesReleve, ligneCourante, libelleCourant)

                ligneCourante = New LigneReleve With {
                    .DateOperation = ExtraireDate(ligne),
                    .Libelle = ExtraireLibelleSurLigneDate(ligne)
                }

                libelleCourant.Clear()

                If Not String.IsNullOrWhiteSpace(ligneCourante.Libelle) Then
                    libelleCourant.Append(ligneCourante.Libelle)
                    libelleCourant.Append(" ")
                End If

            ElseIf ligneCourante IsNot Nothing Then
                Dim montant As Decimal
                Dim isCredit As Boolean

                If ExtraireMontantEtSens(ligne, montant, isCredit) Then

                    If isCredit Then
                        ligneCourante.MontantCredit = montant
                    Else
                        ligneCourante.MontantDebit = montant
                    End If
                Else
                    libelleCourant.Append(ligne).Append(" ")
                End If
            End If
        Next

        AjouterLigneSiValide(lignesReleve, ligneCourante, libelleCourant)

        Return lignesReleve
    End Function

    Public Sub NormaliserFichierReleve(fichierSource As String, fichierDestination As String)
        If String.IsNullOrWhiteSpace(fichierDestination) Then
            Throw New ArgumentException("Le fichier destination est obligatoire.", NameOf(fichierDestination))
        End If

        Dim lignes As List(Of LigneReleve) = ParserFichier(fichierSource)

        Dim dossierDestination As String = Path.GetDirectoryName(fichierDestination)
        If Not String.IsNullOrWhiteSpace(dossierDestination) AndAlso Not Directory.Exists(dossierDestination) Then
            Directory.CreateDirectory(dossierDestination)
        End If

        Dim lignesCsv As IEnumerable(Of String) = lignes.Select(Function(l) ConvertirLigneReleveEnCsv(l))
        File.WriteAllLines(fichierDestination, lignesCsv, Encoding.UTF8)

        Logger.INFO($"Relevé normalisé : source='{fichierSource}', destination='{fichierDestination}', lignes={lignes.Count}")
    End Sub

    Public Function ConvertirLigneReleveEnCsv(ligne As LigneReleve) As String
        If ligne Is Nothing Then
            Throw New ArgumentNullException(NameOf(ligne))
        End If

        Dim debit As String = If(ligne.MontantDebit.HasValue, ligne.MontantDebit.Value.ToString("0.00", CultureFr), String.Empty)
        Dim credit As String = If(ligne.MontantCredit.HasValue, ligne.MontantCredit.Value.ToString("0.00", CultureFr), String.Empty)

        Return String.Join(";", New String() {
            ligne.DateOperation.ToString("dd/MM/yyyy"),
            NettoyerChampCsv(ligne.Libelle),
            debit,
            credit
        })
    End Function

    Private Sub AjouterLigneSiValide(resultat As List(Of LigneReleve),
                                     ligneCourante As LigneReleve,
                                     libelleCourant As StringBuilder)
        If ligneCourante Is Nothing Then
            Exit Sub
        End If

        ligneCourante.Libelle = NettoyerLibelle(libelleCourant.ToString())

        If Not ligneCourante.MontantCredit.HasValue AndAlso Not ligneCourante.MontantDebit.HasValue Then
            Logger.WARN($"Ligne relevé ignorée sans montant : {ligneCourante.DateOperation:dd/MM/yyyy} {ligneCourante.Libelle}")
            Exit Sub
        End If

        If ligneCourante.MontantCredit.HasValue AndAlso ligneCourante.MontantDebit.HasValue Then
            Throw New FormatException($"Ligne relevé incohérente : débit et crédit présents pour {ligneCourante.DateOperation:dd/MM/yyyy} {ligneCourante.Libelle}")
        End If

        resultat.Add(ligneCourante)
    End Sub

    Private Function EstDebutMouvement(ligne As String) As Boolean
        Return Regex.IsMatch(ligne, Constantes.Formats.RegDateReleve, RegexOptions.IgnoreCase)
    End Function

    Private Function ExtraireMontantEtSens(ligne As String,
                                       ByRef montant As Decimal,
                                       ByRef isCredit As Boolean) As Boolean

        Dim match = RegexMontant.Match(ligne)

        If Not match.Success Then
            Return False
        End If

        Dim valeur = match.Groups(1).Value

        isCredit = valeur.Trim().StartsWith("+")
        Dim isDebit = valeur.Trim().StartsWith("-")

        Dim nettoye =
        valeur.Replace("+", "").
               Replace("-", "").
               Replace("€", "").
               Replace(ChrW(160), " ").
               Trim()

        montant = Decimal.Parse(nettoye, Globalization.CultureInfo.GetCultureInfo("fr-FR"))

        If isDebit Then
            isCredit = False
        End If

        Return True

    End Function

    Private Function ExtraireDate(ligne As String) As DateTime
        Dim match As Match = Regex.Match(ligne, Constantes.Formats.RegDateReleve, RegexOptions.IgnoreCase)

        If Not match.Success Then
            Throw New FormatException($"Date invalide dans la ligne : {ligne}")
        End If

        Return DateTime.ParseExact(match.Value, "dd/MM/yyyy", CultureFr)
    End Function

    Private Function ExtraireLibelleSurLigneDate(ligne As String) As String
        Dim match As Match = Regex.Match(ligne, Constantes.Formats.RegDateReleve, RegexOptions.IgnoreCase)

        If Not match.Success Then
            Return String.Empty
        End If

        Return ligne.Substring(match.Index + match.Length).Trim(" "c, ";"c, ControlChars.Tab)
    End Function

    Private Function ExtraireMontant(ligne As String) As Decimal
        Dim valeur As String =
        ligne.Replace(Constantes.Symboles.Euro, String.Empty).
              Replace(Constantes.Symboles.Plus, String.Empty).
              Replace(Constantes.Symboles.Tiret, String.Empty).
              Replace(ChrW(160), String.Empty).
              Replace(" ", String.Empty).
              Trim()

        If String.IsNullOrWhiteSpace(valeur) Then
            Throw New FormatException($"Montant vide dans la ligne : {ligne}")
        End If

        Return Decimal.Parse(valeur, NumberStyles.Number, CultureFr)
    End Function

    Private Function NettoyerLibelle(libelle As String) As String
        If libelle Is Nothing Then
            Return String.Empty
        End If

        Return Regex.Replace(libelle, "\s+", " ").Trim()
    End Function

    Private Function NettoyerChampCsv(value As String) As String
        If value Is Nothing Then
            Return String.Empty
        End If

        Return value.Replace(";", " ").Trim()
    End Function

    'Fonctions utilitaires
    Private Function RegrouperEnregistrementsCsv(lignes As IEnumerable(Of String)) As List(Of String)
        Dim result As New List(Of String)
        Dim buffer As New StringBuilder()
        Dim dansGuillemets As Boolean = False

        For Each ligneBrute As String In lignes
            Dim ligne As String = If(ligneBrute, String.Empty)

            If buffer.Length > 0 Then
                buffer.Append(Environment.NewLine)
            End If

            buffer.Append(ligne)

            For Each c As Char In ligne
                If c = """"c Then
                    dansGuillemets = Not dansGuillemets
                End If
            Next

            If Not dansGuillemets Then
                result.Add(buffer.ToString())
                buffer.Clear()
            End If
        Next

        If buffer.Length > 0 Then
            result.Add(buffer.ToString())
        End If

        Return result
    End Function
    Private Function LireFichierAvecEncodageAuto(fichier As String) As List(Of String)

        Dim encodages = {
            Encoding.UTF8,
            Encoding.GetEncoding(1252) ' ANSI
        }

        For Each enc In encodages
            Try
                Dim lignes = File.ReadAllLines(fichier, enc)

                ' Test simple : si on voit des caractères bizarres, on continue
                If lignes.Any(Function(l) l.Contains("�")) Then
                    Continue For
                End If

                Return lignes.ToList()

            Catch
                ' ignore
            End Try
        Next

        ' fallback
        Return File.ReadAllLines(fichier).ToList()

    End Function
End Module