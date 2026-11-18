# Contrôles de configuration TEST

Le lecteur de propriétés vérifie les paramètres avant de retourner les chemins de fichiers et la chaîne SQL en mode TEST.

- Les dossiers PROD et TEST doivent être absolus, distincts et sans imbrication. Le dossier TEST doit être local.
- Les chemins fournis à GetCheminEtVariable doivent être relatifs et rester dans le dossier TEST après normalisation. Les segments avec points ou espaces finaux sont refusés.
- SQL TEST accepte uniquement l'instance (LocalDB)\MSSQLLocalDB et le catalogue bddAgumaaaTest, avec authentification Windows. Les attachements MDF et les identifiants SQL sont refusés.
- ConnexionDB vérifie aussi la compatibilité d'une connexion déjà en cache et d'une demande de création avant leur utilisation en TEST.
- La propriété de chaîne SQL refuse tout appel dans une compilation DEMO_BUILD.

## Configuration existante

Les valeurs actuelles repInstallationProd et repInstallationTest désignent le même dossier. Cette configuration est maintenant refusée en TEST. Les fichiers et paramètres utilisateur existants sont conservés.

## Étapes restantes

Préparer un dossier local TEST distinct avec ses ressources et un jeu de données fictives reproductible. Vérifier la provenance du catalogue local avant toute utilisation. Recenser les autres accès directs SQL, fichiers et Gmail, et auditer les liens symboliques, jonctions, alias de volumes et changements de fichiers concurrents. Les contrôles de chemins de ce lot sont lexicaux et ne prouvent pas la séparation physique du stockage.

Les tests utilisent uniquement des chemins et chaînes fictifs. Les paramètres changés en mémoire pour les tests d'intégration sont restaurés dans un bloc Finally. Ils ne créent pas de dossier et n'ouvrent aucune connexion SQL.

La recette graphique complète, le provisionnement et la remise à zéro restent à développer et vérifier avant de déclarer un environnement TEST utilisable.
