# gestionTresorerie — démonstration Windows (prototype)

**Statut : prototype non vérifié graphiquement.** N'utiliser **aucune donnée réelle**. L'archive n'est pas une version de production et ne doit pas être déployée.

## Prérequis

- Windows 10/11, système x64 recommandé.
- Microsoft .NET Desktop Runtime 8 (Windows Forms), si absent.
- Aucun compte Gmail, aucune base SQL, aucune clé API ni abonnement payant.

## Démarrage

1. Extraire entièrement l'archive ZIP dans un nouveau dossier local, hors des répertoires de comptabilité.
2. Exécuter `LANCER-DEMONSTRATION.cmd` (ou `gestionTresorerie.exe` : cette archive est construite avec `DEMO_BUILD` forcé).
3. Vérifier le titre **« DÉMONSTRATION HORS LIGNE »** et le bandeau de démonstration.
4. Dans **Analyse**, cliquer sur **Ouvrir une démonstration (documents fictifs, hors ligne)**.
5. Examiner la facture de 42,50 €, le chèque de 18,75 €, puis les rejets : doublon, type non reconnu et JSON invalide.
6. Tester la correction et la confirmation d'examen : **aucune opération n'est approuvée ni enregistrée**.
7. Fermer la revue, la rouvrir, puis utiliser **Effacer les propositions fictives en mémoire** pour recommencer.

## Isolation

L'interface ne charge pas les paramètres, les secrets, les messages Gmail ou la grille SQL au démarrage en mode démonstration. Les menus de production sont désactivés. Dans la démonstration simple, les documents et corrections restent en mémoire. L'entrée explicite Espace TEST local prépare des fichiers fictifs sur disque. Les confirmations restent en mémoire dans la démonstration simple. Dans l'espace TEST, le bouton Enregistrer la revue TEST conserve explicitement les corrections et examens. L'archive exclut les fichiers de configuration et les données réelles ; seuls les exécutables, DLL et manifestes .NET nécessaires sont copiés.

**Limite :** les contrôles automatisés ne remplacent pas un test graphique manuel sur un poste Windows. La version doit être considérée comme expérimentale tant que le lancement et le parcours utilisateur n'ont pas été vérifiés graphiquement et que l'isolation n'a pas fait l'objet d'une revue de sécurité indépendante.

## Traçabilité

Le fichier `COMMIT-DEMONSTRATION.txt` dans l'archive indique le commit exact. Les journaux de compilation et de tests sont disponibles dans l'exécution GitHub Actions ayant produit l'archive. Aucun rapprochement de branches, fusion ou déploiement n'est effectué automatiquement.

## Espace TEST local

Dans Analyse, ouvrir **Espace TEST local (fichiers fictifs)**. La fenêtre affiche TEST HORS LIGNE.

1. Cliquer sur Préparer / réinitialiser TEST. Une nouvelle session contenant cinq fichiers JSON est créée sous `%LOCALAPPDATA%\gestionTresorerie\TEST-hors-ligne`.
2. Cliquer sur Charger les documents TEST.
3. Ouvrir Analyse > Revue des documents préparés pour examiner les résultats.
4. Pour recommencer, cliquer sur Préparer / réinitialiser TEST puis charger la nouvelle session.

Chaque réinitialisation utilise un nouveau sous-dossier. Les anciennes sessions et les modifications de leurs fichiers sont conservées. Les confirmations de la nouvelle session sont remises à zéro. Aucun nettoyage automatique des anciens essais n'est effectué.

L'espace refuse les dossiers existants sans marqueur reconnu, les sessions incomplètes et les liens/jonctions observés. Une modification hostile concurrente du système de fichiers peut dépasser la portée de ces contrôles. Ce premier espace traite des JSON déjà extraits. La reconnaissance de scans/PDF, la base SQL TEST et les décisions comptables restent à développer.

## Enregistrer et reprendre les essais

Après une correction ou une confirmation, fermer la fenêtre de revue puis cliquer sur **Enregistrer la revue TEST** dans la fenêtre TEST. Le fichier `REVUE-TEST.json` conserve l'historique des corrections et les examens confirmés pour cette session.

Après fermeture de l'application, ouvrir Analyse > Espace TEST local puis cliquer sur **Reprendre la dernière session**. Les documents et la revue enregistrée sont chargés. Les examens sont restaurés comme simulations, avec leurs contrôles métier habituels.

La sauvegarde est liée au nom de session et à l'empreinte des cinq documents. Une modification des fichiers ou un état provenant d'une autre session est refusé. Une fenêtre dont la sauvegarde est devenue obsolète doit recharger la revue avant d'enregistrer. En cas de fichier de revue corrompu, la reprise est refusée et le diagnostic est affiché.

Les anciennes sessions dépourvues de pointeur sont recherchées parmi les sessions complètes, par date du marqueur de préparation. Elles démarrent sans correction ni examen s'il n'existe pas de sauvegarde. Les sessions réinitialisées utilisent de nouveaux fichiers et ne récupèrent pas les examens précédents.

## Espace TEST SQL documentaire

Le menu **Analyse → Espace TEST SQL** ouvre un espace séparé pour les documents. L’installateur prépare les deux catalogues sur `(LocalDB)\MSSQLLocalDB` pour le compte Windows courant. SQL Server Express LocalDB doit être installé au préalable. Il crée `bddAgumaaaTest` et sa table de documents si elles sont absentes. Il crée aussi `bddAgumaaa` si elle manque, comme catalogue PROD vide. Une base existante reste en place. Le bouton **Initialiser la base TEST** reste disponible dans l’application.

Ouvrez une facture fictive intégrée ou un PDF de test, cliquez sur **Analyser le document** et vérifiez les champs proposés. Après confirmation, la fiche est enregistrée en SQL et la copie est classée dans la session sous `%LOCALAPPDATA%\gestionTresorerie\TEST-SQL`. La liste est relue depuis SQL. La source reste à son emplacement et les fichiers de production et les services distants sont hors de ce parcours. L’espace TEST local hors ligne reste accessible séparément.

### Utiliser ses propres documents en TEST SQL

Après initialisation, choisissez **Ouvrir un document**, puis **Analyser / saisir le document**. Vous pouvez sélectionner vos propres PDF, JPEG, PNG ou TIFF. Vérifiez et complétez la fiche avant de confirmer son classement. La lecture de PDF textuels peut proposer des champs de facture. Pour les scans et les photos, Tesseract en français peut proposer des champs de facture si `AGUMAAA_TESSERACT_EXE` est configurée. La saisie reste disponible si l’extraction échoue. Pour les chèques, indiquez le sens, le numéro, le bénéficiaire, la date et le montant en euros. Les originaux sont conservés. Les fiches et copies peuvent contenir des données réelles et restent sur le poste, dans `bddAgumaaaTest` et dans les archives de la session TEST SQL.

La validation se fait dans TEST. L’installation ne transfère aucune donnée de TEST vers PROD. Le schéma métier et l’alimentation d’un catalogue PROD neuf, puis la synchronisation web, sont des étapes distinctes.
