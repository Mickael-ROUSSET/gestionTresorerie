
-- Rattachement du fichier .mdf
/*CREATE DATABASE [MarcheDeNoelDB]
    ON (FILENAME = 'C:\Users\User\OneDrive\Documents\CinemaDB.mdf')
    FOR ATTACH;
GO*/
USE [master];
GO

-- 1. On force la fermeture de toutes les connexions actives
ALTER DATABASE [MarcheDeNoelDB] 
SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
GO

-- 2. On détache la base
EXEC sp_detach_db 'MarcheDeNoelDB';
GO