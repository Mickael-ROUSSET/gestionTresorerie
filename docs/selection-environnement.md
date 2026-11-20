# Sélection de l'environnement

Le menu Paramètres > Environnement permet de choisir DEV, TEST ou PROD. La sélection est enregistrée dans la variable utilisateur `AGUMAAA_ENV`, puis relue au démarrage suivant. Redémarrer l'application après un changement.

Ordre de priorité au chargement

1. `AGUMAAA_ENV` utilisateur.
2. `AGUMAAA_ENV` machine.
3. Ancienne variable `envAgumaaa` machine.
4. Ancienne variable `envAgumaaa` utilisateur.
5. DEV si toutes les valeurs sont absentes ou vides.

Les espaces périphériques et la casse sont normalisés. Une valeur renseignée inconnue provoque une erreur d'initialisation au lieu d'utiliser une configuration de priorité inférieure. Les anciennes variables sont conservées pour compatibilité. Le choix explicite de l'utilisateur prime désormais sur les paramètres machine historiques.

La variante `DEMO_BUILD` ignore ces variables et refuse leur modification. Son démarrage hors ligne reste forcé.

## Portée du lot #139

Cette correction fiabilise la sélection. La séparation des chemins, la validation des destinations SQL et l'isolation complète d'un environnement TEST restent à réaliser avant son utilisation avec des données persistantes. La sélection TEST seule ne constitue pas une preuve d'isolation.

Les tests de résolution et de persistance utilisent des valeurs simulées. Le test de raccordement clique le menu du formulaire construit sans lancer son chargement, puis annule la demande. Il ne remplace pas une recette interactive complète.

## Depuis la distribution TEST ou la version complète

L’installateur gestionTresorerie-demo-setup installe une version compilée en DEMO_BUILD. Cette édition permet de tester le classement et la création d’événements dans bddAgumaaaTest. L’accès à la production nécessite la version complète (configuration Release), lancée sans --demo. Depuis son espace TEST, ouvrir Analyse > Changer d’environnement au prochain démarrage…, saisir PROD et redémarrer. Vérifier l’environnement dans la barre d’état avant la saisie. Le changement ne copie aucune donnée de TEST vers PROD.

En saisie de mouvement, le bouton Créer… à côté du sélecteur d’événement enregistre le libellé et le sélectionne dans la fiche en cours. Dans la fiche de classement SQL TEST, utiliser Créer un événement… à côté d’Événement ou activité. L’événement est enregistré dès la confirmation de son libellé. Le mouvement ou le document conserve sa propre étape de validation.

Pour ajouter un événement TEST sans ouvrir de pièce, choisir Analyse > Espace TEST SQL > Créer un événement….
