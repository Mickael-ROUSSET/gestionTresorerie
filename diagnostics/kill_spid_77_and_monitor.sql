-- Script pour tuer la session SPID 77 et surveiller le rollback
-- ATTENTION : exécuter uniquement si vous comprenez l'impact. KILL provoque un rollback qui peut durer longtemps.
-- Exécuter depuis la base concernée avec un compte sysadmin / suffisants droits.

SET NOCOUNT ON;

PRINT '=== INFORMATION: Vérifier d''abord la session 77 ===';
SELECT s.session_id, s.login_name, s.host_name, s.program_name, r.status, r.command, r.blocking_session_id, r.percent_complete, r.estimated_completion_time, r.start_time
FROM sys.dm_exec_sessions s
LEFT JOIN sys.dm_exec_requests r ON s.session_id = r.session_id
WHERE s.session_id = 77;

PRINT '=== SQL en cours pour SPID 77 ===';
SELECT st.text
FROM sys.dm_exec_requests r
CROSS APPLY sys.dm_exec_sql_text(r.sql_handle) st
WHERE r.session_id = 77;

PRINT '=== Verrous actuels sur dbo.Mouvements ===';
DECLARE @objid INT = OBJECT_ID('dbo.Mouvements');
SELECT tl.request_session_id, tl.resource_type, tl.request_mode, tl.request_status, p.index_id
FROM sys.dm_tran_locks tl
LEFT JOIN sys.partitions p ON p.hobt_id = tl.resource_associated_entity_id
WHERE p.object_id = @objid;

PRINT '=== Si vous décidez de tuer la session 77 : décommentez la ligne KILL ci?dessous et exécutez ===';
-- KILL 77;

PRINT '=== Surveillance après KILL (exécuter en boucle manuelle tant que rollback actif) ===';
PRINT 'Exécuter cette requête régulièrement pour surveiller percent_complete et estimated_completion_time :';

SELECT r.session_id, r.status, r.command, r.percent_complete, r.estimated_completion_time, r.total_elapsed_time, r.start_time,
       s.login_name, s.host_name, s.program_name
FROM sys.dm_exec_requests r
JOIN sys.dm_exec_sessions s ON r.session_id = s.session_id
WHERE r.session_id = 77;

PRINT '=== Voir les transactions actives pour la session 77 ===';
SELECT st.session_id, st.transaction_id, at.transaction_begin_time
FROM sys.dm_tran_session_transactions st
JOIN sys.dm_tran_active_transactions at ON st.transaction_id = at.transaction_id
WHERE st.session_id = 77;

PRINT '=== Vérifier la progression des verrous sur la table Mouvements ===';
SELECT tl.request_session_id, tl.resource_type, tl.request_mode, tl.request_status, p.index_id
FROM sys.dm_tran_locks tl
LEFT JOIN sys.partitions p ON p.hobt_id = tl.resource_associated_entity_id
WHERE p.object_id = @objid;

PRINT '=== Après rollback terminé : vérifier l''état général ===';
SELECT r.session_id, r.status, r.command, r.percent_complete, r.estimated_completion_time
FROM sys.dm_exec_requests r
WHERE r.session_id = 77;

PRINT '=== Recommandations post?action ===';
PRINT '- Vérifier l''intégrité des données si la session exécutait une grosse opération';
PRINT '- Vérifier que les jobs / procédures qui ont lancé la requête ont été corrigés (transactions courtes)';
PRINT '- Si le rollback ne progresse pas et que l''impact est critique, planifier redémarrage du service SQL Server.';

SET NOCOUNT OFF;
