Option Strict On
Option Explicit On

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

    ''' <summary>Construit un instantane de presentation sans conserver les documents sources.</summary>
    Public Function PreparerRevue(entrees As IEnumerable(Of PropositionDocumentService.Entree)) As PropositionRevue
        Return New PropositionRevue(Proposer(entrees))
    End Function
End Class
