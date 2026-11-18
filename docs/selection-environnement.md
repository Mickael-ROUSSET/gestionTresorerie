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
