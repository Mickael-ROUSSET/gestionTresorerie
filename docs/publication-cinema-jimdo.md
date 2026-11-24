# Publication du programme cinéma sur le site de l’AGUMAAA

Étude du 24 novembre 2026. La page Cinéma conserve les tarifs, les liens Cinévasion et les informations permanentes. Le bloc des séances change à chaque programme.

## Intégration proposée

1. Produire un programme web à partir de la même liste de séances enrichies que le document Word. Inclure les titres, horaires, affiches et notices disponibles. Signaler les films introuvables et les homonymes avant publication.
2. Héberger cette page à une adresse HTTPS stable, par exemple sur GitHub Pages dans un dépôt public réservé au cinéma. Y transférer uniquement le contenu destiné aux visiteurs. Garder les clés TMDb et les identifiants de publication sur le poste.
3. Lors de la mise en service, placer un élément Widget/HTML dans la page Cinéma depuis l’éditeur Jimdo Creator. Y intégrer le programme externe avec une iframe titrée et un lien direct vers le programme. Conserver les informations permanentes autour du widget.
4. Lors des mises à jour suivantes, une commande dans l’application pourra publier la version approuvée à cette même adresse. Vérifier la version publique sur ordinateur et mobile, conserver la précédente en cas d’échec et permettre un retour à cette version.

Cette option évite de connecter un robot à l’éditeur Jimdo à chaque programme. La documentation Creator consultée décrit le widget et le code HTML personnalisé. Aucune API officielle de modification de cette page existante n’a été identifiée. Le connecteur Jimdo disponible propose la création d’un site. La connexion automatisée à l’éditeur a déjà rencontré FriendlyCaptcha.

## Vérifications avant mise en service

L’édition du site AGUMAAA doit permettre l’ajout du widget. Vérifier sur une page d’essai que l’hébergement autorise l’affichage dans une iframe et que la page reste lisible sur téléphone, notamment avec plusieurs séances. Vérifier les conditions d’usage et d’attribution des affiches et notices avant publication publique. Si l’intégration est refusée, un lien permanent vers le programme hébergé reste possible.

Cette version de l’application génère le fichier Word. L’export web, le déploiement, la pose du widget et la vérification de bout en bout constituent la prochaine étape.

[Page Cinéma AGUMAAA](https://agumaaa.jimdofree.com/cin%C3%A9ma/) · [Widgets Jimdo Creator](https://help.jimdo.com/hc/fr/articles/115005947286-Comment-ajouter-des-widgets) · [GitHub Pages](https://docs.github.com/fr/pages/getting-started-with-github-pages/creating-a-github-pages-site)
