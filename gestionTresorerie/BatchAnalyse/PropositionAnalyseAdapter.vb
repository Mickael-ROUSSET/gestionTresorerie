Option Strict On
Option Explicit On

Imports System
Imports System.Collections.Generic

''' <summary>Adapte uniquement des documents deja analyses en memoire, sans effet externe.</summary>
Public NotInheritable Class PropositionAnalyseAdapter
    ''' <summary>
    ''' Chaque entree exige une cle source stable et unique et un sens explicite de l'appelant.
    ''' Aucune valeur n'est deduite du chemin, de l'identite ou du fournisseur.
    ''' Le service conserve tous les rejets et rejette les cles dupliquees (comparaison Ordinal).
    ''' Le resultat peut etre transmis directement a FrmPrincipale.OuvrirRevuePropositions.
    ''' L'enumerable fourni doit lui aussi etre en memoire et sans effets externes.
    ''' </summary>
    Public Function Proposer(entrees As IEnumerable(Of PropositionDocumentService.Entree)) As PropositionDocumentService.Resultat
        Return New PropositionDocumentService().Proposer(entrees)
    End Function

    ''' <summary>Raccorde des objets deja analyses par reference exacte, jamais par position, chemin ou numero.
    ''' Les deux collections doivent etre en memoire ; chaque document exige une seule correspondance.
    ''' Les correspondances etrangeres au lot sont rejetees, sans produire de proposition.
    ''' </summary>
    Public Function ProposerDocuments(documents As IEnumerable(Of DocumentAgumaaa),
                                      correspondances As IEnumerable(Of PropositionDocumentService.Entree)) As PropositionDocumentService.Resultat
        If documents Is Nothing Then Throw New ArgumentNullException(NameOf(documents))
        If correspondances Is Nothing Then Throw New ArgumentNullException(NameOf(correspondances))
        Dim lot As New List(Of DocumentAgumaaa)(documents)
        Dim mappings As New List(Of PropositionDocumentService.Entree)(correspondances)
        Dim entrees As New List(Of PropositionDocumentService.Entree)()
        For Each document In lot
            Dim candidats As New List(Of PropositionDocumentService.Entree)()
            For Each mapping In mappings
                If mapping IsNot Nothing AndAlso document IsNot Nothing AndAlso
                   Object.ReferenceEquals(mapping.Document, document) Then candidats.Add(mapping)
            Next
            If candidats.Count = 1 Then
                entrees.Add(candidats(0))
            Else
                entrees.Add(New PropositionDocumentService.Entree(document, Nothing, PropositionMouvement.SensMouvement.Debit) With {
                    .ErreurCorrespondance = If(candidats.Count = 0, "Correspondance source absente.", "Correspondance source ambigue.")})
            End If
        Next
        For Each mapping In mappings
            If mapping Is Nothing OrElse Not lot.Exists(Function(document) document IsNot Nothing AndAlso
                                                        Object.ReferenceEquals(document, mapping.Document)) Then
                entrees.Add(New PropositionDocumentService.Entree(Nothing, If(mapping Is Nothing, Nothing, mapping.CleSource),
                    PropositionMouvement.SensMouvement.Debit) With {.ErreurCorrespondance = "Correspondance sans document du lot."})
            End If
        Next
        Return Proposer(entrees)
    End Function

    ''' <summary>Construit un instantane de presentation sans conserver les documents sources.</summary>
    Public Function PreparerRevue(entrees As IEnumerable(Of PropositionDocumentService.Entree)) As PropositionRevue
        Return New PropositionRevue(Proposer(entrees))
    End Function
End Class
