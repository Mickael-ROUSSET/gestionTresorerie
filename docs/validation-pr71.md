# Validation Windows — corrections de la PR #71

## Périmètre et inventaire
L'application couvre les mouvements de trésorerie, les relevés bancaires, les documents et doublons d'images, les tiers/adresses, les catégories et les utilisateurs. Les tests ciblés vérifient les paramètres SQL sans connexion réelle, le hash d'images et le déplacement des doublons. Windows Forms, OpenCvSharp et les références COM imposent une validation Windows avec MSBuild Visual Studio. Le projet contient aussi une référence absolue WindowsBase.resources ; les avertissements COM et NuGet préexistants ne sont pas traités ici.

## Priorités et critères d'acceptation
1. Corriger les dépendances factices et l'index de paramètre utilisateur : tous les tests de repositories doivent passer sans accès SQL.
2. Générer des images PNG fictives et isoler/nettoyer les répertoires temporaires : hash non vide, un doublon déplacé, deuxième passage sans déplacement, fichier non image ignoré.
3. Remplacer le dialogue du logger par Trace.TraceError : une erreur de fichier log doit retourner normalement et produire un diagnostic.
4. Valider compilation et suite complète sous Windows ; arrêter un test bloqué après 60 secondes et conserver le diagnostic blame/TRX.

## Notes de correction
Les validations de dépendances de DocumentRepository sont conservées et couvertes par trois cas de non-régression. Les tests n'utilisent ni images externes ni données de production. Les erreurs du logger restent diagnostiquées mais n'ouvrent plus de boîte modale. Aucun changement de schéma, migration, déploiement ou fusion.

## Première validation des corrections
Le run Windows 38135161123 compile et termine normalement : 34 tests réussis, 9 échecs, aucun ignoré. Ces échecs révèlent des fixtures supplémentaires obsolètes : dépendances nulles de MouvementRepository, paramètres SQL de mouvements/utilisateurs/tiers, appel incorrect du mapper générique par réflexion, attentes de dates de suivi de Tiers et montants sans centimes dans la fixture de relevé. Les corrections restent limitées aux tests ; le mapper métier spécifique de Tiers reste distinct du mapper générique testé.
