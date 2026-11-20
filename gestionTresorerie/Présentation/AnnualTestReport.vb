Option Strict On
Imports System.Data
Imports System.Globalization
Imports System.IO
Imports System.Net
Imports System.Text
Imports System.Windows.Forms

Friend Module AnnualTestReport
    Public Sub Export(owner As IWin32Window)
        Using dates As New Form With {.Text = "Exercice du compte rendu financier TEST", .StartPosition = FormStartPosition.CenterParent, .AutoSize = True, .AutoSizeMode = AutoSizeMode.GrowAndShrink},
              panel As New FlowLayoutPanel With {.Dock = DockStyle.Fill, .AutoSize = True, .Padding = New Padding(12)},
              debut As New DateTimePicker With {.Format = DateTimePickerFormat.Short, .Width = 110, .Value = New Date(Today.Year, 1, 1)},
              fin As New DateTimePicker With {.Format = DateTimePickerFormat.Short, .Width = 110, .Value = New Date(Today.Year, 12, 31)},
              generate As New Button With {.Text = "Générer…", .DialogResult = DialogResult.OK}
            panel.Controls.AddRange(New Control() {New Label With {.Text = "Du", .AutoSize = True}, debut, New Label With {.Text = "au", .AutoSize = True}, fin, generate})
            dates.Controls.Add(panel)
            If dates.ShowDialog(owner) <> DialogResult.OK Then Return
            Try
                Dim store As New SqlDocumentTestStore()
                store.Initialize()
                Using rows = SqlDocumentTestStore.AnnualTestSummary(debut.Value, fin.Value),
                    file As New SaveFileDialog With {.Filter = "Compte rendu HTML (*.html)|*.html", .FileName = $"CR-financier-TEST-{fin.Value:yyyy}.html"}
                    If file.ShowDialog(owner) <> DialogResult.OK Then Return
                    IO.File.WriteAllText(file.FileName, Render(rows, debut.Value, fin.Value), Encoding.UTF8)
                    MessageBox.Show(owner, "Compte rendu enregistré dans " & file.FileName)
                End Using
            Catch ex As Exception
                MessageBox.Show(owner, ex.Message, "Génération du compte rendu impossible")
            End Try
        End Using
    End Sub

    Private Function Render(rows As DataTable, debut As Date, fin As Date) As String
        Dim fr = CultureInfo.GetCultureInfo("fr-FR")
        Dim html As New StringBuilder("<!doctype html><html lang='fr'><meta charset='utf-8'><title>Compte rendu financier AGUMAAA</title><style>body{font:16px sans-serif;max-width:960px;margin:3em auto}table{border-collapse:collapse;width:100%}td,th{padding:.5em;border-bottom:1px solid #aaa;text-align:left}td:nth-child(n+3){text-align:right}</style><h1>AGUMAAA · Compte rendu financier TEST</h1>")
        html.Append($"<p>Du {debut:dd/MM/yyyy} au {fin:dd/MM/yyyy} · Mouvements enregistrés dans bddAgumaaaTest.</p>")
        Dim receipts, expenses As Decimal
        html.Append("<table><thead><tr><th>Événement ou activité</th><th>Nature</th><th>Mouvements</th><th>Identifiants SQL TEST</th><th>Recettes (€)</th><th>Dépenses (€)</th></tr></thead><tbody>")
        For Each row As DataRow In rows.Rows
            Dim credit = CDec(row("recettes")), debit = CDec(row("depenses"))
            receipts += credit
            expenses += debit
            html.Append("<tr><td>" & WebUtility.HtmlEncode(CStr(row("evenement"))) & "</td><td>" & WebUtility.HtmlEncode(CStr(row("nature"))) & "</td><td>" & CInt(row("nombre")).ToString() & "</td><td>" & WebUtility.HtmlEncode(CStr(row("idsMouvements"))) & "</td><td>" & credit.ToString("N2", fr) & "</td><td>" & debit.ToString("N2", fr) & "</td></tr>")
        Next
        html.Append("</tbody></table>")
        If rows.Rows.Count = 0 Then html.Append("<p>Aucun mouvement enregistré pour cette période.</p>")
        html.Append("<p>Total recettes : " & receipts.ToString("C", fr) & " · Total dépenses : " & expenses.ToString("C", fr) & " · Solde de la période : " & (receipts-expenses).ToString("C", fr) & "</p><p>Les justificatifs sans mouvement rattaché sont exclus des totaux.</p></html>")
        Return html.ToString()
    End Function
End Module
