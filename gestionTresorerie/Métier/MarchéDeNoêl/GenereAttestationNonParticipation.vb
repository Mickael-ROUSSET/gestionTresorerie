Imports System.Data.SqlClient
Imports System.IO
Imports System.Net
Imports System.Net.Mail
Imports ClosedXML.Excel
Imports DocumentFormat
Imports DocumentFormat.OpenXml.Packaging
Imports Microsoft.VisualBasic.Devices
Public Class GenereAttestationNonParticipation

    Public Shared Sub ImporterParticipantsDepuisExcel()
        ' --- Sélection du fichier Excel par utilisateur ---
        Dim dlg As New OpenFileDialog With {
        .Title = "Sélectionner le fichier des participants Excel",
        .Filter = "Fichiers Excel|*.xlsx;*.xls",
        .InitialDirectory = "G:\Mon Drive\AGUMAAA\Documents\Manifestations récurrentes\Marché de Noël\2025"
    }

        If dlg.ShowDialog() <> DialogResult.OK Then
            MessageBox.Show("Import annulé.")
        End If

        Dim fichierXlsx = dlg.FileName
        Logger.INFO($"Import à partir du fichier : {fichierXlsx}")
        Dim ligne As Integer = 2   ' On suppose ligne 1 = en-têtes
        Using wb As New XLWorkbook(fichierXlsx)
            Dim ws = wb.Worksheet(1)   ' 1er onglet

            While Not ws.Cell(ligne, 1).IsEmpty()

                Dim parametres As New Dictionary(Of String, Object) From {
                {"@Nom", ws.Cell(ligne, 1).GetString()},
                {"@Prenom", ws.Cell(ligne, 2).GetString()},
                {"@RaisonSociale", ws.Cell(ligne, 3).GetString()},
                {"@Activite", ws.Cell(ligne, 4).GetString()},
                {"@Coordonnees", ws.Cell(ligne, 5).GetString()},
                {"@CP", ws.Cell(ligne, 6).GetString()},
                {"@Ville", ws.Cell(ligne, 7).GetString()},
                {"@Grille", ws.Cell(ligne, 8).GetString()},
                {"@Tables", ws.Cell(ligne, 9).GetString()},
                {"@DateCourrier", If(ws.Cell(ligne, 10).DataType = XLDataType.DateTime,
                                      ws.Cell(ligne, 10).GetDateTime(),
                                      DBNull.Value)},
                {"@Paiement", ws.Cell(ligne, 11).GetString()},
                {"@Siret", ws.Cell(ligne, 16).GetString()},
                {"@Tel", ws.Cell(ligne, 17).GetString()},
                {"@Mail", ws.Cell(ligne, 18).GetString()},
                {"@TypeMouvement", ws.Cell(ligne, 19).GetString()},         ' Commerçant / particulier
                {"@Sexe", ws.Cell(ligne, 20).GetString()},
                {"@PieceIdentite", ws.Cell(ligne, 21).GetString()},
                {"@DateNaissance", ws.Cell(ligne, 22).GetString()},
                {"@LieuNaissance", ws.Cell(ligne, 23).GetString()}
            }

                ' ---- Insertion en base ----
                SqlCommandBuilder.CreateSqlCommand(Constantes.MarcheDeNoelDB, "insertParticipantMdN", parametres).ExecuteNonQuery()
                ligne += 1
            End While
        End Using
        Logger.INFO("Nombre de lignes insérées : " & (ligne - 1))
    End Sub

    Public Shared Sub GenererEtEnvoyerAttestations()
        'Générer les attestations de non participations à d'autres marchés de Noël pour les particuiliers non commerçants   
        Dim participants As List(Of Participant) = ChargerParticipantsDepuisSQL()
        Dim dossierSortie = LectureProprietes.GetVariable("repAttNonParticipation")
        Dim cheminModele = LectureProprietes.GetVariable("modeleAttestationNonParticipation")

        If Not Directory.Exists(dossierSortie) Then
            Directory.CreateDirectory(dossierSortie)
        End If

        Dim compteur As New CompteurTraitement With {
            .NbParticipantsExtraits = participants.Count
        }
        For Each p In participants

            ' --- On ignore les commerçants ---
            If p.Type IsNot Nothing AndAlso p.Type.Trim.Contains("COMMERÇANT", StringComparison.CurrentCultureIgnoreCase) Then
                compteur.NbCommercants += 1
                Continue For
            End If

            compteur.NbParticipantsTraites += 1
            ' --- Nom du fichier Word sortie ---
            Dim nomFichier = $"{p.Nom}_{p.Prenom}_Attestation.docx"
            Dim cheminSortie = Path.Combine(dossierSortie, nomFichier)

            ' Copier le modèle
            File.Copy(cheminModele, cheminSortie, True)

            ' Modifier le document
            Using doc As WordprocessingDocument = WordprocessingDocument.Open(cheminSortie, True)
                Dim body = doc.MainDocumentPart.Document.Body
                RemplaceTexte(body, "nomfamille", ValeurOuBlanc(p.Nom, 25))
                RemplaceTexte(body, "prénom", ValeurOuBlanc(p.Prenom, 25))
                RemplaceTexte(body, "adresse ", ValeurOuBlanc(p.Adresse, 30))
                RemplaceTexte(body, "cp", ValeurOuBlanc(p.CP, 10))
                RemplaceTexte(body, "ville", ValeurOuBlanc(p.Ville, 30))
                RemplaceTexte(body, "datedenaissance", ValeurOuBlanc(p.DateNaissance, 20))
                RemplaceTexte(body, "lieudenaissance", ValeurOuBlanc(p.LieuNaissance, 30))
                RemplaceTexte(body, "lieumarché", New String(" "c, 30))
                RemplaceTexte(body, "datemarché", New String(" "c, 20))
                RemplaceTexte(body, "lieusignature", New String(" "c, 30))
                RemplaceTexte(body, "datesignature", New String(" "c, 20))

                doc.MainDocumentPart.Document.Save()
            End Using

            compteur.NbAttestationsGenerees += 1
            ' --- Envoi du mail uniquement si un mail est renseigné ---
            If Not String.IsNullOrWhiteSpace(p.Mail) Then
                'Pour l'instant on envoie aussi une copie à Mickael pour vérification
                Dim fichiersAJoindre As New List(Of String) From {cheminSortie}
                EnvoyerMailAvecPJ_FromRessource(p.Mail, "Attestation de non-participation", p, fichiersAJoindre)
                compteur.NbMailsEnvoyes += 1
            End If
        Next
        Logger.INFO(compteur.ToString)
    End Sub
    Private Shared Function ValeurOuBlanc(obj As Object, Optional nbEspaces As Integer = 1) As String
        If obj Is Nothing OrElse String.IsNullOrWhiteSpace(obj.ToString()) Then
            Return New String(" "c, nbEspaces)
        Else
            Return obj.ToString()
        End If
    End Function
    Private Shared Sub RemplaceTexte(body As OpenXml.Wordprocessing.Body,
                            search As String,
                            replace As String)

        For Each paragraph In body.Descendants(Of OpenXml.Wordprocessing.Paragraph)()

            ' Reconstitue le texte complet du paragraphe
            Dim runs = paragraph.Descendants(Of OpenXml.Wordprocessing.Run)().ToList()
            Dim fullText As String = String.Concat(runs.Select(Function(r)
                                                                   Dim t = r.GetFirstChild(Of OpenXml.Wordprocessing.Text)()
                                                                   If t IsNot Nothing Then Return t.Text Else Return ""
                                                               End Function))

            ' Si aucun remplacement à faire, on passe au paragraphe suivant
            If Not fullText.Contains(search) Then Continue For

            ' On nettoie tous les runs existants
            For Each r In runs
                Dim t = r.GetFirstChild(Of OpenXml.Wordprocessing.Text)()
                If t IsNot Nothing Then t.Text = ""
            Next

            ' On fait le remplacement dans le texte complet
            Dim newText As String = fullText.Replace(search, replace)

            ' Réinjecte en utilisant le premier run pour conserver le style
            Dim firstRun = runs.First()
            Dim firstText = firstRun.GetFirstChild(Of OpenXml.Wordprocessing.Text)()

            If firstText Is Nothing Then
                firstText = New OpenXml.Wordprocessing.Text()
                firstRun.Append(firstText)
            End If

            firstText.Text = newText

        Next

    End Sub
    Public Shared Function ChargerParticipantsDepuisSQL() As List(Of Participant)
        Dim liste As New List(Of Participant)

        Using reader As SqlDataReader =
        SqlCommandBuilder.CreateSqlCommand(Constantes.MarcheDeNoelDB, "selParticipant").ExecuteReader()
            While reader.Read()
                liste.Add(New Participant(reader))
            End While
        End Using
        Return liste
    End Function

    Private Shared Sub EnvoyerMailAvecPJ_FromRessource(destinataire As String, sujet As String, p As Participant, piecesJointes As List(Of String))
        ' Lecture du HTML 
        Dim corpsHtml As String = LireModeleMailAttestation(LectureProprietes.GetVariable("repModeleBodyAttHtml"))

        Dim rplct As New Dictionary(Of String, String) From {
                                    {"«Nom»", p.Nom},
                                    {"«Prenom»", p.Prenom},
                                    {"«Coordonnees»", p.Adresse},
                                    {"«CP»", p.CP},
                                    {"«Ville»", p.Ville},
                                    {"«Date»", Date.Today.ToString("dd/MM/yyyy")}
                                    }
        ' Remplacements des champs
        corpsHtml = corpsHtml.ReplaceMany(rplct)

        ' Créer une instance du gestionnaire de mails
        Dim mailer As New MailManager()
        ' Liste des fichiers à joindre 
        If mailer.EnvoyerEmail(destinataire, sujet, corpsHtml, piecesJointes) Then
            Logger.INFO($"E-mail avec pièces jointes envoyé avec succès à {destinataire}")
        Else
            Logger.INFO($"Échec de l'envoi de l'e-mail à {destinataire}.")
        End If
    End Sub

    Public Shared Function LireModeleMailAttestation(cheminFichier As String) As String
        Dim contenuHtml As String

        Try
            ' 1. Vérifier le chemin d'accès au fichier 
            If String.IsNullOrEmpty(cheminFichier) Then
                ' Journaliser une erreur si le chemin n'est pas configuré
                Logger.ERR("Le chemin 'repModeleBodyAttHtml' n'est pas configuré dans les propriétés.")
                Return String.Empty
            End If

            ' 2. Vérifier l'existence du fichier
            If Not File.Exists(cheminFichier) Then
                ' Journaliser une erreur si le fichier n'existe pas
                Logger.ERR($"Le fichier modèle HTML est introuvable à l'emplacement : {cheminFichier}")
                Return String.Empty
            End If

            ' 3. Lire tout le contenu du fichier
            ' Encoding.UTF8 est recommandé pour le contenu HTML afin de supporter les caractères spéciaux
            contenuHtml = File.ReadAllText(cheminFichier, System.Text.Encoding.UTF8)

            Return contenuHtml

        Catch ex As Exception
            ' Gestion des erreurs d'accès au fichier (permissions, etc.)
            Logger.ERR($"Erreur lors de la lecture du fichier HTML {cheminFichier} : {ex.Message}")
            Return String.Empty
        End Try

    End Function
End Class
