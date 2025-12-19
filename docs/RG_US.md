# Règles de Gestion (RG) détaillées par User Story — gestionTresorerie

Ce document liste les Règles de Gestion (RG) nécessaires pour valider chaque User Story (US) priorisée. Chaque RG précise : préconditions, validations, post‑conditions, erreurs gérées, et exemples d’acceptation.

---

## US1 — Saisie et enregistrement d’un mouvement
Objectif : Permettre à un utilisateur de saisir un mouvement complet et de l’enregistrer en base sans générer d’exception.

### RG-1.1 — Champs obligatoires
- Préconditions : formulaire ouvert.
- Liste obligatoire : `Montant`, `Tiers`, `Catégorie`, `Sous-catégorie`, `Type de document`, `Document` (id), `Type de mouvement`.
- Règle : la validation (`verifMouvement`) bloque l’enregistrement si un champ est manquant.
- Message utilisateur : "Champs obligatoires manquants : <liste>".
- Focus : mettre le focus sur le premier champ manquant suivant l’ordre métier.
- Tests d’acceptation :
  - Cas valide : tous les champs remplis → passe la validation.
  - Cas invalide : un champ vide → message et focus sur le champ.

### RG-1.2 — Format et conversion du montant
- Préconditions : `txtMontant` non vide.
- Règle : `txtMontant` doit correspondre à `Constantes.regExMontant` et être convertible en Decimal via `Decimal.TryParse`.
- Comportement : si non match regex ou échec de parse → message d’erreur informatif et blocage.
- Particularité : si `rbDebit.Checked` alors le montant final stocké = -|montant|.
- Tests d’acceptation :
  - "1 234,56" → convertible selon culture → accepté.
  - "abc" → rejeté.

### RG-1.3 — Construction robuste du modèle métier
- Règle : la transformation UI → `Mouvements` doit normaliser les valeurs :
  - Utiliser `String.Trim()`, valeurs par défaut pour `Nothing` (ex. événement vide), et conversions sûres (montant/sens).
  - Ne pas accéder à des propriétés d’objets potentiellement `Nothing` (ex. `_typeEvenement`) sans garde.
- Post-condition : l’objet `Mouvements` retourne des valeurs valides ou une erreur contrôlée.
- Tests d’acceptation :
  - `_typeEvenement` non sélectionné → `Événement` enregistré comme chaîne vide, pas d’exception.

### RG-1.4 — Insertion en base et idempotence
- Règle : avant insertion appeler `ChargerMouvementsSimilaires` pour éviter doublons.
- Comportement :
  - Si doublon trouvé → affichage de la liste (`FrmListe`) et point d’entrée pour l’utilisateur.
  - Si aucun doublon → appeler `InsererMouvementEnBase`.
- Logging : consigner INFO/WARN selon résultat.
- Tests d’acceptation :
  - Doublon détecté → fenêtre de doublons affichée.
  - Aucun doublon → exécution de la procédure `insertMvts` et log INFO.

---

## US2 — Détection de mouvements similaires (doublons)
Objectif : Détecter les mouvements similaires avant insertion et prévenir l’utilisateur.

### RG-2.1 — Critères de similarité
- Critères utilisés (procédure `procMvtsIdentiques`) : minimum `dateMvt`, `montant`, `sens`.
- Règle : recherche exact-match sur ces critères. Conversion et normalisation avant la requête (montant en Decimal, date au format DB).
- Tests :
  - Même date, même montant, même sens → considéré similaire.
  - Différence mineure de centimes selon arrondi → dépend de la procédure ; documenter comportement.

### RG-2.2 — UX après détection
- Si _>=1_ résultat :
  - Ouvrir `FrmListe` avec les résultats.
  - Ajouter handler `objetSelectionneChanged` pour laisser l’utilisateur choisir action (choisir existant / insérer malgré tout).
- Si sélection annulée ou confirmée comme insertion :
  - Si insertion demandée alors appeler `InsererMouvementEnBase`.
- Logging : INFO sur décision utilisateur.

---

## US3 — Blocage de la validation si champs obligatoires absents
Objectif : Interdire l’enregistrement tant que les champs requis ne sont pas complets.

### RG-3.1 — Liste de champs et libellés
- Champs à vérifier (conforme verifMouvement) :
  - `Montant`, `Tiers`, `Catégorie`, `Sous-catégorie`, `Type de document` (libellé), `Document` (id), `Type de mouvement` (libellé).
- Règle : si l’un est manquant, arrêter l’opération avant toute transformation métier.
- Message structuré : énumération des champs manquants.

### RG-3.2 — Focus et guidance
- Règle : focus sur champ prioritaire, et affichage d’un message pour guider l’utilisateur.
- Test d’acceptation :
  - Champ `Type de mouvement` vide → message et focus sur `txtTypeMvt`.

---

## US4 — Tolérance aux éléments optionnels non sélectionnés (ex. Événement)
Objectif : Empêcher que l’absence de sélection d’éléments optionnels provoque des NullReferenceException.

### RG-4.1 — Définition des champs optionnels
- Champs optionnels identifiés : `Événement`, `reference` (numéro de chèque), `typeReference`, `idDoc` (peut être 0), `numeroRemise`.
- Règle : si champ optionnel non renseigné, utiliser valeur par défaut explicite (ex. chaîne vide, 0, DBNull pour DB).

### RG-4.2 — Sécurité côté modèle
- Règle : le constructeur et les setters de `Mouvements` doivent accepter `Nothing` / valeurs invalides et les normaliser.
- Comportement : pas d’accès direct à membres d’objets possiblement `Nothing` (ex. `_typeEvenement.libelle`) — utiliser `If(_typeEvenement IsNot Nothing, _typeEvenement.libelle, txtEvenement.Text.Trim())` côté UI ou laisser le constructeur normaliser.
- Tests :
  - Aucun événement sélectionné → création objet sans exception ; DB reçoit NULL si champ vide.

---

## Règles transverses & comportements d’erreur

### RG-T.1 — Gestion des exceptions
- Les blocs `Try/Catch` doivent différencier validation (front) et erreurs système (DB).
- Erreurs de validation → message utilisateur (non-logged stacktrace).
- Erreurs système (SqlException, null inattendu) → logger ERR avec détails et remontée contrôlée si nécessaire.

### RG-T.2 — Logging
- Niveau INFO : création, insertion, sélection document, détection doublon.
- Niveau WARN : champs manquants, insertion anormale (rows <> 1), aucune catégorie trouvée.
- Niveau ERR : exceptions non prévues.

### RG-T.3 — Conventions de données
- Dates : stocker Date.MinValue → traité comme NULL en base.
- Montant : Decimal, utiliser culture courante pour parsing.
- IDs : 0 signifie non renseigné → traiter comme NULL au moment de l’insertion DB.

---

## Cas d’exemple (scénarios d’acceptation)

1. Scénario nominal :
   - Saisie complète avec `Type de mouvement` et `Événement` sélectionnés → `verifMouvement()` OK → pas de doublon → insertion OK → log INFO.

2. Scénario événement absent :
   - `txtEvenement` vide, `_typeEvenement` non sélectionné → création `Mouvements` s’exécute, `Événement` = `""`, insertion OK (champ DB NULL).

3. Scénario doublon :
   - Mêmes date/montant/sens → `FrmListe` affiché, utilisateur annule → insertion non effectuée ; log WARN/INFO selon action.

4. Scénario montant invalide :
   - `txtMontant = "abc"` → message d’erreur → focus sur montant → pas d’appel à `CreerMouvement()`.

---

## Critères d’acceptation (synthèse)
- Aucune NullReferenceException durant la création d’un `Mouvements` depuis l’UI.
- Validation frontale empêche toute tentative d’insertion avec champs obligatoires vides.
- Détection de doublons fonctionnelle et UX pour choix utilisateur.
- Insertions en base traitent correctement valeurs vides (NULL) pour champs optionnels.

---

## Annexes courtes
- Procédures DB impliquées : `insertMvts`, `procMvtsIdentiques`, `updMvt`, `updMvtIdDoc`.
- Utilitaires mentionnés : `Utilitaires.ExtraitNuméroChèque`, `AppelFrmSelectionUtils.OuvrirSelectionGenerique(Of T)`.
- Recommandation d’amélioration : décaler la logique de construction du `Mouvements` dans une usine/service pour séparer UI et métier.
