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
