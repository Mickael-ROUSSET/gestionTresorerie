/*drop TABLE [dbo].[Mouvements];
CREATE TABLE [dbo].[Mouvements]
(
	[Id] INT IDENTITY(12485, 1) PRIMARY KEY NOT NULL, 
    [Sens] BIT  NULL, 
    [Montant] DECIMAL(5,2) NULL, 
    [Catégorie] NVARCHAR(50)  NULL, 
    [Sous-Catégorie] NVARCHAR(50) NULL, 
    [Type] NVARCHAR(50)  NULL, 
    [Tiers] NVARCHAR(50) NULL, 
    [Etat] CHAR(10)  NULL, 
    [NumChèque] BIGINT NULL, 
    [Note] NVARCHAR(MAX) NULL, 
    [Modifiable] BIT  NULL, 
    [DateModification] DATETIME2 NULL, 
    [DateCréation] DATETIME2 NULL , 
    [Evénement] NVARCHAR(50) NULL, 
    [Banque] NVARCHAR(50) NULL
)*/
--CREATE SEQUENCE Mouvements     START WITH 1  INCREMENT BY 1 ;
select * from Mouvements;