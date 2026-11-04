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
2. Inventorier les projets de tests, les scripts CI Windows, les jeux de données fictifs et leurs mécanismes de réinitialisation sur la branche cible (non encore audités dans ce document).
3. Définir une configuration TEST **explicitement isolée et refusant par défaut** toute connexion SQL/Gmail réelle, sans dépendre d'une variable utilisateur ambiguë.
4. Ajouter des tests hors ligne prouvant que les chemins TEST ne peuvent viser les paramètres PROD, puis seulement des tests Windows du parcours complet.
5. Pour l'installateur #8, ne conditionner que la variante DEMO_BUILD et prévoir installation/désinstallation en environnement Windows jetable.

## Contrôle de ce lot

Contrôle effectué : lecture directe des six fichiers cités sur GitHub et rapprochement manuel des noms de variables et chemins. **Aucune compilation ni aucun test Windows exécuté pour cette modification documentaire.** Aucun code applicatif modifié, aucune donnée réelle consultée, aucune fusion ni déploiement.
