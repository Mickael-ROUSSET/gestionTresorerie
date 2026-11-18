# AGENTS.md — gestionTresorerie

## Mission
Faire évoluer l'application VB.NET Windows Forms vers une version fonctionnelle, testée, maintenable et documentée, en limitant les interventions de l'utilisateur.

## Référentiel
- Dépôt : Mickael-ROUSSET/gestionTresorerie
- Branche principale : master
- Une branche par changement ; jamais de développement directement sur master.
- GitHub issues et pull requests constituent le journal durable des travaux.

## Avant toute modification
1. Lire la solution, les projets, les tests, la documentation et les workflows existants.
2. Produire un inventaire des fonctionnalités et un backlog priorisé.
3. Repérer les dépendances Windows/COM, les références absolues, les secrets et les accès aux données.
4. Définir des critères d'acceptation vérifiables.
5. Ne jamais supposer qu'un test est réussi s'il n'a pas été exécuté.

## Cycle de travail
1. Choisir un ticket clairement délimité, prioritairement un blocage de la version utilisable.
2. Créer une branche dédiée et une PR décrivant le changement.
3. Implémenter le minimum nécessaire, ajouter ou adapter les tests.
4. Exécuter les contrôles possibles et documenter explicitement ceux qui sont indisponibles.
5. Traiter les commentaires de revue, relancer les contrôles et recommencer jusqu'à résolution.
6. Mettre à jour le ticket, les notes de version et la documentation pertinente.

## Politique de fusion
- L'utilisateur autorise la fusion automatique **après** tous les contrôles requis.
- Ne jamais contourner les protections de branche, forcer une fusion, ni assimiler un contrôle absent ou ignoré à un succès.
- Aucune fusion tant que les règles CI et protections de master ne sont pas établies et vérifiées.
- Les changements sensibles (données, migrations non réversibles, droits, paiements, secrets, production) requièrent une validation humaine explicite.

## Sécurité des données
- Ne pas modifier la production, exécuter de migration réelle ni déployer sans autorisation spécifique.
- Utiliser des données fictives ou anonymisées pour les tests.
- Ne pas exposer de secrets, identifiants, documents privés ou données bancaires dans les logs, commits et PR.
- Pour les montants, employer des types décimaux adaptés ; préserver les numéros de chèque sous forme textuelle (zéros initiaux).
- Valider les JSON de métadonnées et gérer les champs absents ou malformés.

## Particularités techniques
- Application VB.NET / .NET 8 / Windows Forms.
- Dépendances COM possibles : une compilation Windows peut être nécessaire.
- Ne pas remplacer une dépendance structurante sans justification ni tests de non-régression.
- Favoriser des modules testables indépendamment de l'interface et des ressources externes.

## Rapport de fin de tâche
Fournir : résumé, fichiers touchés, tests exécutés et résultats, contrôles non exécutés, risques, éventuelles migrations, lien de PR et prochaine tâche recommandée.

## En cas de blocage
Documenter le blocage dans la PR ou le ticket ; poursuivre une autre tâche indépendante si possible. Ne demander à l'utilisateur que les arbitrages fonctionnels ou autorisations réellement indispensables.
