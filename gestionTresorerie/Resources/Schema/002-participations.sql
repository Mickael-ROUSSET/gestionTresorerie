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
