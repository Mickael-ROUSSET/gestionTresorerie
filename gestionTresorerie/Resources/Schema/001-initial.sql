-- Version 001 du schéma métier de bddAgumaaa.
-- Tables, index et contraintes de l’export SSMS du 08/11/2026.
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

IF OBJECT_ID(N'dbo.Categorie', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Categorie](
    	[Id] [int] IDENTITY(1,1) NOT NULL,
    	[libelle] [varchar](50) NOT NULL,
    	[dateDebut] [date] NOT NULL,
    	[dateFin] [date] NULL,
    	[debit] [int] NOT NULL
    ) ON [PRIMARY];
END;

IF OBJECT_ID(N'dbo.Communes', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Communes](
    	[Id] [int] IDENTITY(1,1) NOT NULL,
    	[CodeINSEE] [varchar](10) NOT NULL,
    	[NomCommune] [varchar](150) NOT NULL,
    	[CodePostal] [varchar](10) NOT NULL,
    	[LibelleAcheminement] [varchar](150) NULL
    ) ON [PRIMARY];
END;

IF OBJECT_ID(N'dbo.Comptes', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Comptes](
    	[Id] [int] IDENTITY(1,1) NOT NULL,
    	[login] [nvarchar](50) NOT NULL,
    	[motDePasse] [nvarchar](50) NOT NULL,
    	[typeAcces] [nchar](10) NOT NULL
    ) ON [PRIMARY];
END;

IF OBJECT_ID(N'dbo.Coordonnees', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Coordonnees](
    	[Id] [int] IDENTITY(1,1) NOT NULL,
    	[IdTiers] [int] NOT NULL,
    	[TypeAdresse] [nvarchar](50) NULL,
    	[Rue1] [nvarchar](100) NULL,
    	[Rue2] [nvarchar](100) NULL,
    	[CodePostal] [nvarchar](10) NULL,
    	[Ville] [nvarchar](50) NULL,
    	[Pays] [nvarchar](50) NULL,
    	[Email] [nvarchar](150) NULL,
    	[Telephone] [nvarchar](30) NULL
    ) ON [PRIMARY];
END;

IF OBJECT_ID(N'dbo.Documents', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Documents](
    	[idDoc] [int] IDENTITY(1,1) NOT NULL,
    	[dateDoc] [datetime] NOT NULL,
    	[contenuDoc] [nvarchar](max) NULL,
    	[cheminDoc] [nvarchar](max) NOT NULL,
    	[categorieDoc] [nvarchar](50) NULL,
    	[sousCategorieDoc] [nvarchar](50) NULL,
    	[idMvtDoc] [int] NULL,
    	[metaDonnees] [nvarchar](max) NULL,
    	[dateModif] [datetime] NULL
    ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];
END;

IF OBJECT_ID(N'dbo.Evenements', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Evenements](
    	[Id] [int] IDENTITY(1,1) NOT NULL,
    	[libelle] [nvarchar](max) NULL,
    	[dateDebut] [date] NULL,
    	[dateFin] [date] NULL
    ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];
END;

IF OBJECT_ID(N'dbo.Exercices', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Exercices](
    	[Id] [int] IDENTITY(1,1) NOT NULL,
    	[dateDebut] [date] NOT NULL,
    	[dateFin] [date] NOT NULL,
    	[clos] [bit] NOT NULL
    ) ON [PRIMARY];
END;

IF OBJECT_ID(N'dbo.Films', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Films](
    	[IdFilm] [int] IDENTITY(1,1) NOT NULL,
    	[Titre] [nvarchar](200) NOT NULL,
    	[DureeMinutes] [int] NULL,
    	[Genre] [nvarchar](100) NULL,
    	[Realisateur] [nvarchar](150) NULL,
    	[DateSortie] [date] NULL,
    	[Synopsis] [nvarchar](max) NULL,
    	[AgeMinimum] [int] NULL,
    	[AfficheUrl] [nvarchar](300) NULL,
    PRIMARY KEY CLUSTERED
    (
    	[IdFilm] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
    ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];
    ALTER TABLE [dbo].[Films]  WITH CHECK ADD CHECK  (([AgeMinimum]>=(0)));
    ALTER TABLE [dbo].[Films]  WITH CHECK ADD CHECK  (([DureeMinutes]>=(0)));
END;

IF OBJECT_ID(N'dbo.Mouvements', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Mouvements](
    	[Id] [int] IDENTITY(1,1) NOT NULL,
    	[categorie] [int] NOT NULL,
    	[sousCategorie] [int] NULL,
    	[montant] [decimal](7, 2) NOT NULL,
    	[sens] [bit] NOT NULL,
    	[tiers] [int] NOT NULL,
    	[note] [nvarchar](500) NULL,
    	[dateMvt] [date] NOT NULL,
    	[dateCreation] [date] NOT NULL,
    	[dateModification] [date] NULL,
    	[etat] [bit] NOT NULL,
    	[evenement] [nvarchar](50) NULL,
    	[typeMouvement] [nvarchar](50) NULL,
    	[modifiable] [bit] NOT NULL,
    	[numeroRemise] [int] NULL,
    	[idCheque] [int] NULL,
    	[reference] [nvarchar](50) NULL,
    	[typeReference] [nvarchar](50) NULL,
    	[idDoc] [int] NULL
    ) ON [PRIMARY];
END;

IF OBJECT_ID(N'dbo.ParticipantMdN', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ParticipantMdN](
    	[IdParticipant] [int] IDENTITY(1,1) NOT NULL,
    	[IdTiers] [int] NOT NULL,
    	[Nom] [varchar](100) NOT NULL,
    	[Prenom] [varchar](100) NOT NULL,
    	[RaisonSociale] [varchar](200) NULL,
    	[Activite] [varchar](200) NULL,
    	[Adresse] [varchar](300) NULL,
    	[CP] [varchar](10) NULL,
    	[Ville] [varchar](150) NULL,
    	[Grille] [varchar](50) NULL,
    	[Tables] [varchar](50) NULL,
    	[DateCourrier] [date] NULL,
    	[Paiement] [varchar](100) NULL,
    	[Siret] [varchar](20) NULL,
    	[Tel] [varchar](50) NULL,
    	[Mail] [varchar](200) NULL,
    	[Type] [varchar](50) NULL,
    	[Sexe] [varchar](20) NULL,
    	[PieceIdentite] [varchar](200) NULL,
    	[DateNaissance] [date] NULL,
    	[LieuNaissance] [varchar](200) NULL,
    PRIMARY KEY CLUSTERED
    (
    	[IdParticipant] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
    ) ON [PRIMARY];
    ALTER TABLE [dbo].[ParticipantMdN] ADD  DEFAULT ((0)) FOR [IdTiers];
END;

IF OBJECT_ID(N'dbo.ReglesAffectationReleve', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ReglesAffectationReleve](
    	[Id] [int] IDENTITY(1,1) NOT NULL,
    	[MotCle] [nvarchar](200) NOT NULL,
    	[IdTiers] [int] NULL,
    	[IdCategorie] [int] NULL,
    	[IdSousCategorie] [int] NULL,
    	[Sens] [bit] NULL,
    	[Priorite] [int] NOT NULL,
    	[Actif] [bit] NOT NULL,
    PRIMARY KEY CLUSTERED
    (
    	[Id] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
    ) ON [PRIMARY];
    ALTER TABLE [dbo].[ReglesAffectationReleve] ADD  DEFAULT ((100)) FOR [Priorite];
    ALTER TABLE [dbo].[ReglesAffectationReleve] ADD  DEFAULT ((1)) FOR [Actif];
END;

IF OBJECT_ID(N'dbo.Seances', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Seances](
    	[IdSeance] [int] IDENTITY(1,1) NOT NULL,
    	[IdFilm] [int] NOT NULL,
    	[DateHeureDebut] [datetime] NOT NULL,
    	[TarifBase] [decimal](6, 2) NOT NULL,
    	[Langue] [nvarchar](50) NULL,
    	[Format] [nvarchar](50) NULL,
    	[NbEntreesAdultes] [int] NOT NULL,
    	[NbEntreesEnfants] [int] NOT NULL,
    	[NbEntreesGroupeEnfants] [int] NOT NULL,
    PRIMARY KEY CLUSTERED
    (
    	[IdSeance] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
    ) ON [PRIMARY];
    ALTER TABLE [dbo].[Seances] ADD  CONSTRAINT [DF_Seances_NbEntreesAdultes]  DEFAULT ((0)) FOR [NbEntreesAdultes];
    ALTER TABLE [dbo].[Seances] ADD  CONSTRAINT [DF_Seances_NbEntreesEnfants]  DEFAULT ((0)) FOR [NbEntreesEnfants];
    ALTER TABLE [dbo].[Seances] ADD  CONSTRAINT [DF_Seances_NbEntreesGroupeEnfants]  DEFAULT ((0)) FOR [NbEntreesGroupeEnfants];
    ALTER TABLE [dbo].[Seances]  WITH CHECK ADD  CONSTRAINT [FK_Seances_Films] FOREIGN KEY([IdFilm])
    REFERENCES [dbo].[Films] ([IdFilm])
    ON DELETE CASCADE;
    ALTER TABLE [dbo].[Seances] CHECK CONSTRAINT [FK_Seances_Films];
    ALTER TABLE [dbo].[Seances]  WITH CHECK ADD CHECK  (([NbEntreesAdultes]>=(0)));
    ALTER TABLE [dbo].[Seances]  WITH CHECK ADD CHECK  (([NbEntreesEnfants]>=(0)));
    ALTER TABLE [dbo].[Seances]  WITH CHECK ADD CHECK  (([NbEntreesGroupeEnfants]>=(0)));
    ALTER TABLE [dbo].[Seances]  WITH CHECK ADD CHECK  (([TarifBase]>=(0)));
END;

IF OBJECT_ID(N'dbo.SousCategorie', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SousCategorie](
    	[Id] [int] IDENTITY(1,1) NOT NULL,
    	[libelle] [varchar](50) NOT NULL,
    	[dateDebut] [date] NOT NULL,
    	[dateFin] [date] NULL,
    	[idCategorie] [int] NOT NULL
    ) ON [PRIMARY];
END;

IF OBJECT_ID(N'dbo.Tarifs', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Tarifs](
    	[IdTarif] [int] IDENTITY(1,1) NOT NULL,
    	[NomTarif] [nvarchar](100) NOT NULL,
    	[ReductionPourcent] [decimal](5, 2) NOT NULL,
    	[Montant] [decimal](5, 2) NOT NULL,
    	[Conditions] [nvarchar](200) NULL,
    	[DateDebutValidite] [date] NOT NULL,
    	[DateFinValidite] [date] NULL,
    PRIMARY KEY CLUSTERED
    (
    	[IdTarif] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
    ) ON [PRIMARY];
    ALTER TABLE [dbo].[Tarifs] ADD  DEFAULT ((0)) FOR [ReductionPourcent];
    ALTER TABLE [dbo].[Tarifs] ADD  CONSTRAINT [DF_Tarifs_DateDebutValidite]  DEFAULT ('19010101') FOR [DateDebutValidite];
    ALTER TABLE [dbo].[Tarifs]  WITH CHECK ADD CHECK  (([DateFinValidite] IS NULL OR [DateFinValidite]>=[DateDebutValidite]));
    ALTER TABLE [dbo].[Tarifs]  WITH CHECK ADD CHECK  (([Montant]>=(0) AND [Montant]<=(100)));
    ALTER TABLE [dbo].[Tarifs]  WITH CHECK ADD CHECK  (([ReductionPourcent]>=(0) AND [ReductionPourcent]<=(100)));
END;

IF OBJECT_ID(N'dbo.Tiers', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Tiers](
    	[Id] [int] IDENTITY(1,1) NOT NULL,
    	[nom] [nvarchar](50) NULL,
    	[prenom] [nvarchar](50) NULL,
    	[raisonSociale] [nvarchar](50) NULL,
    	[categorieDefaut] [int] NULL,
    	[sousCategorieDefaut] [int] NULL,
    	[dateCreation] [datetime] NOT NULL,
    	[dateModification] [datetime] NOT NULL,
    	[dateNaissance] [date] NULL,
    	[lieuNaissance] [nvarchar](100) NULL
    ) ON [PRIMARY];
END;

IF OBJECT_ID(N'dbo.Type', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Type](
    	[Id] [int] IDENTITY(1,1) NOT NULL,
    	[libelle] [nvarchar](max) NULL,
    	[dateDebut] [date] NULL,
    	[dateFin] [date] NULL
    ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];
END;

IF OBJECT_ID(N'dbo.TypeDoc', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[TypeDoc](
    	[Id] [int] IDENTITY(1,1) NOT NULL,
    	[prompt] [nvarchar](max) NULL,
    	[libellé] [nvarchar](50) NOT NULL,
    	[gabaritRepertoire] [nvarchar](max) NULL,
    	[gabaritNomFichier] [nvarchar](max) NULL,
    	[classe] [nvarchar](50) NULL
    ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];
END;

IF OBJECT_ID(N'dbo.TypeMouvement', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[TypeMouvement](
    	[Id] [int] IDENTITY(1,1) NOT NULL,
    	[libelle] [nvarchar](max) NULL,
    	[dateDebut] [date] NULL,
    	[dateFin] [date] NULL
    ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];
END;

-- Les versions antérieures de l’installateur créaient Documents sans dateModif.
IF COL_LENGTH(N'dbo.Documents', N'dateModif') IS NULL
    ALTER TABLE dbo.Documents ADD dateModif datetime NULL;

