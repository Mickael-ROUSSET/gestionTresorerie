CREATE TABLE [dbo].[Tiers]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [nom] NVARCHAR(50) NULL, 
    [prenom] NVARCHAR(50) NULL, 
    [raisonSociale] NVARCHAR(50) NULL, 
    [categorieDefaut] NVARCHAR(50) NULL, 
    [sousCategorieDefaut] NVARCHAR(50) NULL, 
    [dateCreation] DATETIME NOT NULL, 
    [dateModification] DATETIME NOT NULL
)
