CREATE TABLE [dbo].[Exercices]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [dateDebut] DATE NOT NULL, 
    [dateFin] DATE NOT NULL, 
    [clos] BIT NOT NULL
)
