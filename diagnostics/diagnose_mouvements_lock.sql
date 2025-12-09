-- Script de diagnostic des verrous/transactions bloquantes sur la table dbo.Mouvements
-- Usage : exécuter dans la base concernée en tant que login avec droits VIEW SERVER STATE / sysadmin
-- Attention : les commandes KILL ou redémarrage doivent être faites avec précaution.

SET NOCOUNT ON;

PRINT '1) Sessions actives et requêtes en cours';
EXEC sp_who2;

PRINT '--- Détails des requêtes en cours (dm_exec_requests) ---';
SELECT r.session_id,
       s.login_name,
       s.host_name,
       s.program_name,
       r.status,
       r.command,
       r.wait_type,
       r.wait_time,
       r.blocking_session_id,
       r.percent_complete,
       r.estimated_completion_time,
       r.cpu_time,
       r.total_elapsed_time,
       r.start_time
FROM sys.dm_exec_requests r
JOIN sys.dm_exec_sessions s ON r.session_id = s.session_id
WHERE s.is_user_process = 1
ORDER BY r.start_time DESC;

PRINT '2) Transactions actives (DBCC OPENTRAN)';
DBCC OPENTRAN;

PRINT '--- Transactions actives (dm_tran_active_transactions) ---';
SELECT at.transaction_id,
       at.name,
       at.transaction_begin_time,
       dt.database_transaction_begin_time,
       dt.database_transaction_state,
       dt.database_transaction_status
FROM sys.dm_tran_active_transactions at
LEFT JOIN sys.dm_tran_database_transactions dt ON at.transaction_id = dt.transaction_id;

PRINT '3) Verrous sur l''objet dbo.Mouvements';
-- Remplacez le nom du schéma/objet si nécessaire
DECLARE @object_id INT = OBJECT_ID('dbo.Mouvements');

SELECT tl.resource_type,
       DB_NAME(tl.resource_database_id) AS database_name,
       tl.resource_associated_entity_id,
       tl.request_mode,
       tl.request_status,
       tl.request_session_id,
       OBJECT_NAME(p.object_id, tl.resource_database_id) AS object_name,
       p.index_id
FROM sys.dm_tran_locks tl
LEFT JOIN sys.partitions p ON p.hobt_id = tl.resource_associated_entity_id
WHERE tl.resource_type IN ('OBJECT','PAGE','KEY','HOBT')
  AND p.object_id = @object_id;

PRINT '4) Chaîne de blocage (blocking chain)';
;WITH blockers AS (
    SELECT r.session_id,
           r.blocking_session_id,
           s.login_name,
           s.host_name,
           r.command,
           r.wait_type,
           r.wait_time
    FROM sys.dm_exec_requests r
    JOIN sys.dm_exec_sessions s ON r.session_id = s.session_id
    WHERE r.blocking_session_id <> 0
)
SELECT * FROM blockers ORDER BY wait_time DESC;

PRINT '5) Surveillance d''un SPID (à remplacer par le SPID suspect)';
PRINT '-- Remplacez <spid> puis exécuter la section suivante si nécessaire';
-- SELECT * FROM sys.dm_exec_requests WHERE session_id = <spid>;
-- SELECT * FROM sys.dm_tran_session_transactions WHERE session_id = <spid>;
-- SELECT * FROM sys.dm_exec_sessions WHERE session_id = <spid>;

PRINT '6) Progression d''un rollback (si SPID en rollback)';
PRINT '-- Remplacez <spid> et exécutez :';
-- SELECT session_id, status, command, percent_complete, estimated_completion_time, total_elapsed_time FROM sys.dm_exec_requests WHERE session_id = <spid>;

PRINT '7) Commande KILL (exécuter manuellement avec prudence)';
PRINT '-- KILL <spid> -- (attention : rollback éventuel)';

PRINT '8) Recommandations';
PRINT '- Contacter l''utilisateur/processus responsable du SPID bloquant';
PRINT '- Si possible, essayer KILL <spid> ; surveiller percent_complete';
PRINT '- En dernier recours, planifier redémarrage contrôlé du service SQL Server';

SET NOCOUNT OFF;
