
-- Rattachement du fichier .mdf
CREATE DATABASE [bddAgumaaa]
    ON (FILENAME = 'C:\Users\User\OneDrive\Documents\CinemaDB.mdf')
    FOR ATTACH;
GO
USE [master];
GO

-- 1. On force la fermeture de toutes les connexions actives
ALTER DATABASE [bddAgumaaa] 
SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
GO

-- 2. On détache la base
EXEC sp_detach_db 'bddAgumaaa';
GO