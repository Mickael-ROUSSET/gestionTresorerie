# Extraction documentaire et validation humaine (travail en cours)

## Parcours propose
1. Les pieces jointes Gmail sont copiees localement par `EmailProcessor` (journal UID + SHA-256).
2. `BatchAnalyse.DemarrerTraitement` lit les documents avec un `IDocumentExtractionEngine` interchangeable.
3. `DocumentProposalStore` ecrit un JSON par empreinte SHA-256 dans `_PropositionsAValider`, avec le statut `EnAttenteValidation`.
4. **Aucune insertion SQL ni aucun renommage du document source n'est effectue dans ce parcours.**
5. Une interface de relecture et d'approbation explicite reste **a developper** avant toute ecriture comptable. Ne pas assimiler un JSON de proposition a une operation validee.

## Moteurs et arbitrages
| Moteur | Precision / limites | Cout | Maintenance | Donnees |
| --- | --- | --- | --- | --- |
| Texte PDF local (par defaut, PdfPig) | Fiable pour recuperer le texte de PDF natifs; ne comprend pas le document et ne lit pas les scans image | Pas de cout par page | Faible | Aucune transmission externe |
| OCR local (a evaluer : Tesseract, IronOCR deja reference) | Dependant de la qualite du scan, langue et mise en page; erreurs possibles sur montants et dates | Tesseract libre, IronOCR selon licence | Modeles de langues et dependances a maintenir | Traitement local |
| Regles de lecture | Previsible pour formats stables, fragile aux changements de modele | Tres faible | Regles a maintenir par emetteur | Local |
| IA distante (Gemini ou autre, facultative) | Plus souple pour les mises en page, mais erreurs/hallucinations possibles | Tarif et quotas variables | API, modele, prompts et supervision | Envoi du document au prestataire : consentement et regles de confidentialite necessaires |

Le moteur local est le seul utilise par defaut dans ce lot. Une image JPEG ou un PDF sans texte donne un statut `OCR_ou_saisie_manuelle_requis`; **aucune analyse IA externe n'est declenchee**. L'interface permet d'injecter ulterieurement un autre moteur sans changer le stockage des propositions.

## Garanties et limites
- Le fichier source est conserve intact.
- Un contenu identique (SHA-256) ne cree pas de seconde proposition. Une proposition existante n'est jamais ecrasee.
- Les propositions sont des fichiers JSON locaux potentiellement sensibles : appliquer les memes droits d'acces et la meme politique de sauvegarde que les documents sources.
- Le contenu extrait n'est pas encore normalise en categorie, tiers, date, montant ou sens debit/credit. La validation humaine, l'interface de correction, la liaison a un mouvement et les tests de bout en bout restent a faire.
- Les tests de ce lot utilisent des fichiers factices et un moteur simule, sans connexion Gmail ou SQL.
