-- Copie locale des données métier vers un TEST vierge. Lancer sur (LocalDB)\MSSQLLocalDB.
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_WARNINGS ON;

IF CONVERT(int, SERVERPROPERTY('IsLocalDB')) <> 1 OR DB_NAME() <> N'bddAgumaaaTest'
    THROW 51020, N'Connexion attendue : LocalDB, catalogue bddAgumaaaTest.', 1;
IF DB_ID(N'bddAgumaaa') IS NULL
    THROW 51021, N'La base source bddAgumaaa est absente de cette instance.', 1;

-- Films doit précéder Seances. Les comptes et le journal de migrations restent locaux à TEST.
DECLARE @tables table (position int IDENTITY PRIMARY KEY, name sysname NOT NULL);
INSERT @tables (name) VALUES
(N'Categorie'), (N'Communes'), (N'Tiers'), (N'Coordonnees'), (N'Evenements'), (N'Exercices'),
(N'Films'), (N'ParticipantMdN'), (N'ReglesAffectationReleve'), (N'Seances'), (N'SousCategorie'),
(N'Tarifs'), (N'Type'), (N'TypeDoc'), (N'TypeMouvement'), (N'Mouvements'), (N'Documents');

DECLARE @i int = 1, @name sysname, @target nvarchar(300), @source nvarchar(300),
        @targetId int, @sourceId int, @sql nvarchar(max), @cols nvarchar(max),
        @hasIdentity bit, @hasRows bit, @lock int;
BEGIN TRANSACTION;
BEGIN TRY
    EXEC @lock = sys.sp_getapplock @Resource=N'gestionTresorerie-Import-Test',
        @LockMode=N'Exclusive', @LockOwner=N'Transaction', @LockTimeout=10000;
    IF @lock < 0 THROW 51022, N'Un import TEST est déjà en cours.', 1;

    -- Verrouiller et contrôler toutes les destinations avant la première insertion.
    WHILE @i <= (SELECT COUNT(*) FROM @tables)
    BEGIN
        SELECT @name = name FROM @tables WHERE position = @i;
        SET @target = N'dbo.' + QUOTENAME(@name);
        SET @targetId = OBJECT_ID(@target, N'U');
        IF @targetId IS NULL OR OBJECT_ID(N'bddAgumaaa.' + @target, N'U') IS NULL
            THROW 51023, N'Une table métier est absente. Initialisez le schéma TEST et vérifiez PROD.', 1;
        IF EXISTS (SELECT 1 FROM sys.triggers WHERE parent_id = @targetId AND is_disabled = 0)
            THROW 51024, N'Un déclencheur actif existe dans TEST. Copie interrompue.', 1;
        SET @sql = N'SELECT @rows = CASE WHEN EXISTS (SELECT 1 FROM ' + @target +
                   N' WITH (TABLOCKX, HOLDLOCK)) THEN 1 ELSE 0 END;';
        EXEC sys.sp_executesql @sql, N'@rows bit OUTPUT', @rows = @hasRows OUTPUT;
        IF @hasRows = 1 THROW 51025, N'TEST contient déjà des données métier. Aucune donnée n’a été remplacée.', 1;
        SET @i += 1;
    END

    SET @i = 1;
    WHILE @i <= (SELECT COUNT(*) FROM @tables)
    BEGIN
        SELECT @name = name FROM @tables WHERE position = @i;
        SET @target = N'dbo.' + QUOTENAME(@name);
        SET @source = N'bddAgumaaa.' + @target;
        SET @targetId = OBJECT_ID(@target, N'U');
        SET @sourceId = OBJECT_ID(@source, N'U');

        -- Refuser un schéma divergent plutôt que de perdre silencieusement des colonnes.
        IF EXISTS (
            SELECT name COLLATE DATABASE_DEFAULT FROM bddAgumaaa.sys.columns WHERE object_id = @sourceId
            EXCEPT SELECT name FROM sys.columns WHERE object_id = @targetId)
            THROW 51026, N'Une colonne PROD n’existe pas dans TEST. Copie annulée.', 1;

        SELECT @cols = STRING_AGG(CONVERT(nvarchar(max), QUOTENAME(t.name)), N',')
                           WITHIN GROUP (ORDER BY t.column_id),
               @hasIdentity = CONVERT(bit, MAX(CONVERT(int, t.is_identity)))
        FROM sys.columns AS t
        JOIN bddAgumaaa.sys.columns AS s
          ON s.object_id = @sourceId AND s.name COLLATE DATABASE_DEFAULT = t.name
        WHERE t.object_id = @targetId AND t.is_computed = 0 AND t.system_type_id <> 189;

        IF @cols IS NULL THROW 51026, N'Aucune colonne compatible. Copie annulée.', 1;
        SET @sql = CASE WHEN @hasIdentity = 1 THEN N'SET IDENTITY_INSERT ' + @target + N' ON;' ELSE N'' END +
                   N'INSERT INTO ' + @target + N' (' + @cols + N') SELECT ' + @cols +
                   N' FROM ' + @source + N' WITH (HOLDLOCK);' +
                   CASE WHEN @hasIdentity = 1 THEN N'SET IDENTITY_INSERT ' + @target + N' OFF;' ELSE N'' END;
        EXEC sys.sp_executesql @sql;
        SET @i += 1;
    END
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
