# Suppression de l'intégration Mistral

Décision : abandonner Mistral dans gestionTresorerie. Une éventuelle intégration OpenAI fera l'objet d'une PR distincte.

## Périmètre identifié
- `gestionTresorerie/Agent IA/MistralApi.vb` et `TestMistral.vb` : client HTTP et état de l'agent.
- `gestionTresorerie/Présentation/FrmPrincipale.vb` : appel à `TestMistral.CreeAgent`.
- `gestionTresorerie/BatchAnalyse/batchAnalyse.vb` : référence à `cléApiMistral`.
- `gestionTresorerie/Properties/cléApiMistral.vb` : ancienne valeur de clé dans des commentaires.
- Copies dans `gestionTresorerie_backup/` : vérifier les usages et supprimer les éléments Mistral obsolètes.
- Rechercher aussi les références de projet, interfaces, fichiers d'état `mistral_state.json`, paramètres et tests.

## Critères d'acceptation
1. Aucun appel réseau à Mistral, aucune référence au client Mistral ni à une clé Mistral dans le code actif.
2. Aucune valeur de clé, même commentée, dans les fichiers de la branche.
3. Les parcours d'analyse de documents ne doivent pas échouer silencieusement : retirer/désactiver clairement les commandes dépendantes de Mistral en attendant leur remplacement.
4. Build et tests Windows réussis via GitHub Actions.
5. Aucun changement de schéma de base, suppression de données métier, déploiement ou modification directe de master.
6. Audit des fichiers sensibles et des secrets dans l'historique : ne pas réécrire l'historique dans cette PR.

## Suite
Concevoir une abstraction de fournisseur IA avant une intégration OpenAI. Une souscription ChatGPT ne fournit pas automatiquement une clé API OpenAI ; prévoir configuration et gestion sécurisée des secrets.
