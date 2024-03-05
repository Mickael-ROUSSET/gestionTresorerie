CREATE TABLE [dbo].[Mouvements]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [Sens] BIT NOT NULL, 
    [Montant] DECIMAL NOT NULL, 
    [Catégorie] NVARCHAR(50) NOT NULL, 
    [Sous-Catégorie] NVARCHAR(50) NULL, 
    [Type] NVARCHAR(50) NOT NULL, 
    [Tiers] NVARCHAR(50) NULL, 
    [Etat] CHAR(10) NOT NULL, 
    [NumChèque] BIGINT NULL, 
    [Note] NVARCHAR(MAX) NULL, 
    [Modifiable] BIT NOT NULL, 
    [DateModification] DATETIME2 NULL, 
    [DateCréation] DATETIME2 NOT NULL, 
    [Evénement] NVARCHAR(50) NOT NULL, 
    [Banque] NVARCHAR(50) NULL
)
