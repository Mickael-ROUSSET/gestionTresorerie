select Id 
, (select libelle from Categorie where id = catégorie) as Catégorie
, (select libelle from sousCategorie where id = sousCatégorie) as sousCatégorie
, montant 
, CASE sens
	WHEN 1 THEN 'Crédit'
	ELSE 'Débit'
END as 'Sens'
, (select coalesce(nom + ' ' + prenom, raisonSociale ) from Tiers where id = tiers) as Tiers
, note 
, dateMvt 
, dateCréation 
, dateModification 
, CASE etat
	WHEN 1 THEN 'Rapproché'
	ELSE 'A valider'
END as 'Etat' 
, événement 
, type 
,  CASE modifiable
	WHEN 1 THEN 'modifiable'
	ELSE 'Non modifiable'
END  as 'Modifiable'
, numeroRemise , idCheque from Mouvements;