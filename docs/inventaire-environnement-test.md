# Inventaire initial pour l'environnement de test (#139)

État : **inventaire statique partiel**, établi le 4 novembre 2026 sur la branche de démonstration `feature/windows-demo-isolee-20261102`. Ce document ne constitue ni un audit exhaustif ni une validation Windows. Aucun environnement TEST réel ne doit être démarré sur la seule base de cet inventaire.

## Éléments existants vérifiés dans le code

| Élément | Emplacement | Constat | Réutilisation / précaution |
| --- | --- | --- | --- |
| Sélection DEV / TEST / PROD | `gestionTresorerie/Utilitaires/EnvironmentManager.vb` | `LoadEnvironment` lit `envAgumaaa` au niveau Machine puis User, défaut DEV ; `SetEnvironment` écrit **`AGUMAAA_ENV`** au niveau User | **Incohérence de noms** : une sélection par `SetEnvironment` ne sera pas relue par `LoadEnvironment`. Ne pas utiliser comme garde de sécurité sans correction et tests. |
| Libellés d'environnement | même module | `GetEnvironmentLabel` retourne Développement, Test ou Production | Réutilisable pour un affichage, mais ne prouve aucune isolation. |
| Configuration SQL | `Infrastructure/Data/Configuration/ConnectionStringProvider.vb`, `RepositoryFactory.vb` | La chaîne vient de `LectureProprietes.connexionString(sBase)` ; `RepositoryFactory` peut créer un exécuteur SQL | Une sélection TEST ne suffit pas à garantir que la chaîne vise une base fictive. Bloquer les connexions tant que la destination n'est pas vérifiée. |
| Paramètres historiques | `gestionTresorerie/App.config` | `repInstallationProd` et `repInstallationTest` désignent le même chemin local dans la version examinée ; `bddAgumaaaTest` désigne un fichier MDF sur un lecteur Drive ; `ParamDb` est présent | **Ne pas** utiliser ce MDF ni les chemins locaux pour des tests automatisés : provenance et contenu non établis. |
| Chargement de secrets | `Utilitaires/ConfigurationLoader.vb` | `LoadSecrets` lit plusieurs variables utilisateur, puis échoue si certaines manquent | À neutraliser dans un mode TEST hors ligne ; ne jamais journaliser de secrets. |
| Démonstration hors ligne | `docs/demo-windows.md` et PR #138 | Démarrage `DEMO_BUILD`, exemples fictifs en mémoire, menus de production neutralisés, effacement en mémoire | Point de départ sûr pour un premier environnement TEST sans SQL ni Gmail. L'isolation complète reste à auditer. |

## Lacunes à examiner avant implémentation

1. Recenser les autres appels à `LoadEnvironment`, `SetEnvironment`, `LectureProprietes.connexionString`, `LoadSecrets`, les accès fichiers et tous les chemins de création SQL.
2. Compléter l'inventaire des projets de tests, des scripts CI Windows, des jeux de données fictifs et de leur réinitialisation ; premiers tests identifiés ci-dessous, sans audit exhaustif.
3. Définir une configuration TEST **explicitement isolée et refusant par défaut** toute connexion SQL/Gmail réelle, sans dépendre d'une variable utilisateur ambiguë.
4. Ajouter des tests hors ligne prouvant que les chemins TEST ne peuvent viser les paramètres PROD, puis seulement des tests Windows du parcours complet.
5. Pour l'installateur #8, ne conditionner que la variante DEMO_BUILD et prévoir installation/désinstallation en environnement Windows jetable.

## Contrôle de ce lot

Contrôle effectué : lecture directe des six fichiers cités sur GitHub et rapprochement manuel des noms de variables et chemins. **Aucune compilation ni aucun test Windows exécuté pour cette modification documentaire.** Aucun code applicatif modifié, aucune donnée réelle consultée, aucune fusion ni déploiement.

## Complément d'inventaire vérifié le 4 novembre 2026 (23 h, heure de Paris)

Lecture sur la branche de la PR #140 : `gestionTresorerie.Tests/DemonstrationWindowsTests.vb` et `gestionTresorerie/Utilitaires/EnvironmentManager.vb`.

| Contrôle existant dans les tests | Portée réellement couverte | Limite pour #139 |
| --- | --- | --- |
| `DemoBuild_InterditLesTroisPointsDEntreeSQL` | En compilation `DEMO_BUILD`, attend des exceptions pour `ConnexionDB.GetInstance`, `GetConnexion` et `CreeConnexion` | Ne prouve pas que le mode TEST ordinaire refuse tous les accès SQL |
| `DemoBuild_ForceLeModeFictif_MemeSansOptionDemo` | Vérifie le bandeau, la grille neutralisée et la sauvegarde des paramètres désactivée | Test STA sans affichage effectif ; ne constitue pas une recette graphique |
| `Demo_ChargeFactureChequeEtTroisRejets_PuisReinitialiseSansFichier` | Vérifie deux propositions, trois rejets et la réinitialisation de la session | N'exerce ni une base TEST persistante ni des documents importés depuis un dossier |
| `Demo_ExamenFictifNeValideRien_EtReinitialisationEffaceLaConfirmation` | Vérifie une confirmation d'examen sans approbation et sa remise à zéro | Ne couvre pas une validation métier simulée persistante |
| `Demo_PreparationInvalideRetireLaRevuePrecedente_EtPermetDeRecommencer` et `Demo_IdentiteImapInvalideEffaceLeLotPrecedent` | Évitent de conserver une revue obsolète après une entrée invalide | Ne couvrent pas l'ensemble des erreurs de fichiers ni les connexions externes |

**Conséquence technique :** conserver `DEMO_BUILD` comme frontière de sécurité pour les premiers essais, plutôt que de basculer directement vers `EnvironnementType.TEST`. Avant tout changement de variable d'environnement, spécifier la priorité Machine/User, le comportement en cas de valeur inconnue et l'absence d'accès réel par défaut. Un simple renommage `AGUMAAA_ENV`/`envAgumaaa` ne suffit pas à garantir l'isolation.

**Contrôle de ce complément :** lecture statique des fichiers indiqués ; noms et assertions vérifiés dans le code. Aucun test ni build nouveau exécuté. La vérification du workflow CI Windows et l'inventaire exhaustif des scripts restent à faire. Aucun fichier de données TEST réel ouvert.

## Inventaire CI et conditionnement (5 novembre 2026)

Fichiers relus sur cette branche : `.github/workflows/validation.yml`, `.github/scripts/package-demo.ps1`, `docs/demo-windows.md`. Constatations **statiques**, sans exécution du workflow.

| Mécanisme | Existant vérifié | Limite / réutilisation pour #8 et #139 |
| --- | --- | --- |
| CI Windows | `windows-latest`, .NET 8, MSBuild Visual Studio, `dotnet test` Release, artefact TRX | Depuis le commit `0c8da9f2`, la branche `docs/inventaire-environnement-test-20261104` figure parmi les branches `push` autorisées. La cible DEMO n'est toujours pas dans `pull_request.branches` : un événement PR ne déclenche donc pas ce workflow par cette seule configuration. Aucun run associé au commit `0c8da9f2` n'a été trouvé via la recherche des exécutions liées au commit ; cette recherche ne couvre que les runs déclenchés par PR et ne permet pas de conclure à l'absence de run `push`. |
| Étapes DEMO_BUILD | Conditions explicites limitées au push/PR dont la **branche source** est `feature/windows-demo-isolee-20261102` | Depuis `0c8da9f2`, les trois étapes compilation Demo, compilation des tests Demo et tests Demo autorisent aussi un événement `push` sur la branche documentaire. Les captures, le conditionnement et les smoke tests restent limités à la branche DEMO initiale. **Aucun résultat d'exécution n'est établi pour ce nouveau déclencheur.** |
| Archive candidate | `package-demo.ps1` sélectionne le binaire `Demo/net8.0-windows`, copie exe/DLL/manifestes .NET et trois fichiers de support, contrôle une liste blanche dans le dossier et le ZIP | Base réutilisable pour un futur installateur DEMO seulement ; aucune logique d'installation, de raccourci ni de désinstallation n'y figure. |
| Traçabilité | `COMMIT-DEMONSTRATION.txt` contient le SHA, `CONFIGURATION=Demo`, `CONSTANTE=DEMO_BUILD`, et `CONTROLE_GRAPHIQUE=NON_EFFECTUE` | Conserver ces preuves, ajouter la traçabilité de l'installateur et son empreinte SHA-256. Ne pas substituer les captures à une recette interactive. |
| Smoke tests | Démarrage du binaire extrait, sondage TCP pendant environ trois secondes ; second lancement sans `--demo` et contrôle du titre de fenêtre | Contrôles utiles mais incomplets : ne prouvent ni absence de tout trafic réseau ni parcours graphique complet. |
| Pré-requis | `docs/demo-windows.md` : Windows 10/11, .NET Desktop Runtime 8, aucun Gmail/SQL/secret | Installer uniquement cette variante isolée ; tester installation, raccourci, parcours et désinstallation dans un Windows jetable. |

**Évolution publiée au commit `0c8da9f2` :** le workflow autorise désormais les tests DEMO_BUILD sur le `push` de cette branche documentaire, sans publication d'archive et sans élargissement des permissions. **À vérifier :** l'apparition effective d'un run `push`, ses journaux et les résultats Release/Demo ; ne pas annoncer une validation Windows sans ces preuves. Si aucun run n'apparaît, examiner les règles GitHub Actions du dépôt et le comportement des workflows définis uniquement sur une branche non par défaut, sans modifier `master`.
