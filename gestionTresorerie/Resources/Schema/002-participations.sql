IF COL_LENGTH('dbo.Tiers','nomUsage') IS NULL ALTER TABLE dbo.Tiers ADD nomUsage nvarchar(50) NULL;
IF COL_LENGTH('dbo.Tiers','genre') IS NULL ALTER TABLE dbo.Tiers ADD genre nvarchar(30) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.Tiers') AND name='UX_Tiers_Participations') CREATE UNIQUE INDEX UX_Tiers_Participations ON dbo.Tiers(Id);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.Documents') AND name='UX_Documents_Participations') CREATE UNIQUE INDEX UX_Documents_Participations ON dbo.Documents(idDoc);
IF OBJECT_ID('dbo.ActiviteAssociation') IS NULL CREATE TABLE dbo.ActiviteAssociation(
 Id int IDENTITY PRIMARY KEY, Libelle nvarchar(100) NOT NULL UNIQUE);
IF OBJECT_ID('dbo.EditionActivite') IS NULL CREATE TABLE dbo.EditionActivite(
 Id int IDENTITY PRIMARY KEY, ActiviteId int NOT NULL REFERENCES dbo.ActiviteAssociation(Id),
 Annee int NOT NULL, Debut date NULL, Fin date NULL, UNIQUE(ActiviteId,Annee),
 CHECK(Annee BETWEEN 1900 AND 9999 AND (Debut IS NULL OR YEAR(Debut)=Annee) AND (Fin IS NULL OR Fin>=Debut)));
IF OBJECT_ID('dbo.Participation') IS NULL CREATE TABLE dbo.Participation(
 Id int IDENTITY PRIMARY KEY, TiersId int NOT NULL REFERENCES dbo.Tiers(Id), EditionId int NOT NULL REFERENCES dbo.EditionActivite(Id),
 RoleCours nvarchar(100) NOT NULL, UNIQUE(TiersId,EditionId,RoleCours), UNIQUE(Id,TiersId));
IF OBJECT_ID('dbo.DocumentTiers') IS NULL CREATE TABLE dbo.DocumentTiers(
 DocumentId int PRIMARY KEY REFERENCES dbo.Documents(idDoc), TiersId int NOT NULL REFERENCES dbo.Tiers(Id), UNIQUE(DocumentId,TiersId));
IF OBJECT_ID('dbo.JustificatifParticipation') IS NULL CREATE TABLE dbo.JustificatifParticipation(
 DocumentId int NOT NULL, ParticipationId int NOT NULL, TiersId int NOT NULL,
 PRIMARY KEY(DocumentId,ParticipationId), FOREIGN KEY(DocumentId,TiersId) REFERENCES dbo.DocumentTiers(DocumentId,TiersId),
 FOREIGN KEY(ParticipationId,TiersId) REFERENCES dbo.Participation(Id,TiersId));
GO
CREATE OR ALTER VIEW dbo.VueTiersCoordonnees AS
SELECT t.Id AS tiersId,c.Id AS coordId,t.nom,t.prenom,t.raisonSociale,t.nomUsage AS nom_usage,t.genre,
 c.Rue1 AS adresse,c.Rue2 AS complement,c.CodePostal AS code_postal,c.Ville AS ville,c.Pays AS pays,c.Email AS mail,c.Telephone AS telephone
FROM dbo.Tiers t LEFT JOIN dbo.Coordonnees c ON c.IdTiers=t.Id;
GO
CREATE OR ALTER VIEW dbo.VueParticipantsActivite AS
SELECT p.TiersId,p.Id AS ParticipationId,a.Libelle AS Activite,e.Annee,e.Debut,e.Fin,p.RoleCours
FROM dbo.Participation p JOIN dbo.EditionActivite e ON e.Id=p.EditionId JOIN dbo.ActiviteAssociation a ON a.Id=e.ActiviteId;
GO
CREATE OR ALTER VIEW dbo.VueParticipantsAnnee AS
SELECT TiersId,Annee,STRING_AGG(CONVERT(nvarchar(max),Activite),N', ') WITHIN GROUP(ORDER BY Activite) AS Activites
FROM (SELECT DISTINCT TiersId,Annee,Activite FROM dbo.VueParticipantsActivite) a GROUP BY TiersId,Annee;
GO
-- Les bulletins historiques portent déjà l'identifiant validé du tiers.
-- Une date de signature seule ne prouve aucune saison.
INSERT dbo.DocumentTiers(DocumentId,TiersId)
SELECT d.idDoc,t.Id FROM dbo.Documents d
CROSS APPLY(SELECT CASE WHEN ISJSON(d.metaDonnees)=1 THEN d.metaDonnees ELSE N'{}' END AS meta) m
JOIN dbo.Tiers t ON t.Id=TRY_CONVERT(int,JSON_VALUE(m.meta,'$.idTiers'))
WHERE d.categorieDoc=N'FormulaireAdhesion' AND NOT EXISTS(SELECT 1 FROM dbo.DocumentTiers x WHERE x.DocumentId=d.idDoc);
GO
CREATE OR ALTER VIEW dbo.VueInscriptionsAVerifier AS
SELECT d.idDoc AS DocumentId,d.dateDoc,d.cheminDoc,dt.TiersId
FROM dbo.Documents d LEFT JOIN dbo.DocumentTiers dt ON dt.DocumentId=d.idDoc
WHERE d.categorieDoc=N'FormulaireAdhesion' AND NOT EXISTS(SELECT 1 FROM dbo.JustificatifParticipation j WHERE j.DocumentId=d.idDoc);
GO
-- Reprise uniquement des éditions et cours explicitement identifiés sur la pièce.
DECLARE @hist TABLE(DocumentId int PRIMARY KEY,TiersId int,Activite nvarchar(100),Annee int,RoleCours nvarchar(100));
INSERT @hist
SELECT dt.DocumentId,dt.TiersId,LTRIM(RTRIM(s.Activite)),s.Annee,LTRIM(RTRIM(s.RoleCours))
FROM dbo.DocumentTiers dt JOIN dbo.Documents d ON d.idDoc=dt.DocumentId
CROSS APPLY(SELECT CASE WHEN ISJSON(d.metaDonnees)=1 THEN d.metaDonnees ELSE N'{}' END AS meta) m
CROSS APPLY(SELECT JSON_VALUE(m.meta,'$.participation.activite') AS Activite,
 TRY_CONVERT(int,JSON_VALUE(m.meta,'$.participation.annee')) AS Annee,
 JSON_VALUE(m.meta,'$.participation.cours_role') AS RoleCours) s
WHERE d.categorieDoc=N'FormulaireAdhesion' AND s.Annee BETWEEN 1900 AND 9999
 AND LEN(TRIM(s.Activite)) BETWEEN 1 AND 100 AND LEN(TRIM(s.RoleCours)) BETWEEN 1 AND 100
 AND NOT EXISTS(SELECT 1 FROM dbo.JustificatifParticipation j WHERE j.DocumentId=dt.DocumentId);
INSERT dbo.ActiviteAssociation(Libelle)
SELECT DISTINCT h.Activite FROM @hist h WHERE NOT EXISTS(SELECT 1 FROM dbo.ActiviteAssociation a WHERE a.Libelle=h.Activite);
INSERT dbo.EditionActivite(ActiviteId,Annee)
SELECT DISTINCT a.Id,h.Annee FROM @hist h JOIN dbo.ActiviteAssociation a ON a.Libelle=h.Activite
WHERE NOT EXISTS(SELECT 1 FROM dbo.EditionActivite e WHERE e.ActiviteId=a.Id AND e.Annee=h.Annee);
INSERT dbo.Participation(TiersId,EditionId,RoleCours)
SELECT DISTINCT h.TiersId,e.Id,h.RoleCours FROM @hist h JOIN dbo.ActiviteAssociation a ON a.Libelle=h.Activite
JOIN dbo.EditionActivite e ON e.ActiviteId=a.Id AND e.Annee=h.Annee
WHERE NOT EXISTS(SELECT 1 FROM dbo.Participation p WHERE p.TiersId=h.TiersId AND p.EditionId=e.Id AND p.RoleCours=h.RoleCours);
INSERT dbo.JustificatifParticipation(DocumentId,ParticipationId,TiersId)
SELECT h.DocumentId,v.ParticipationId,h.TiersId FROM @hist h JOIN dbo.VueParticipantsActivite v
 ON v.TiersId=h.TiersId AND v.Activite=h.Activite AND v.Annee=h.Annee AND v.RoleCours=h.RoleCours;
