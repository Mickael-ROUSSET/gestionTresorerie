# Backlog gestionTresorerie

Vue de consultation générée automatiquement à partir du backlog local.
**Ne pas modifier ce fichier pour piloter l'orchestrateur.**

**Actualisation :** 2026-11-09 22:32 UTC
**Objectif :** Prepare safe first-use of the real Windows application: startup diagnostics, opt-in Gmail readiness, usable review workflow, offline acceptance tests and manual release gates. No live production deployment or accounting writes.

| Tâche | État | Validation | Commit |
|---|---|---|---|
| `treasury-review-correction-link-113` | Bloquée : failed-test | build OK, test échec | — |
| `treasury-prevalidation-rules-114` | Publiée | build OK, test OK | [`c26d1394664f`](https://github.com/Mickael-ROUSSET/gestionTresorerie/commit/c26d1394664f) |
| `treasury-review-session-115` | Publiée | build OK, test OK | [`e2fb1939726f`](https://github.com/Mickael-ROUSSET/gestionTresorerie/commit/e2fb1939726f) |
| `treasury-review-endtoend-tests-116` | Publiée | build OK, test OK | [`9a7ddbb2b7a8`](https://github.com/Mickael-ROUSSET/gestionTresorerie/commit/9a7ddbb2b7a8) |
| `treasury-ui-session-integration-118` | Publiée | build OK, test OK | [`c026623c6064`](https://github.com/Mickael-ROUSSET/gestionTresorerie/commit/c026623c6064) |
| `treasury-main-review-navigation-119` | Publiée | build OK, test OK | [`ab3c6d45867c`](https://github.com/Mickael-ROUSSET/gestionTresorerie/commit/ab3c6d45867c) |
| `treasury-document-review-pipeline-120` | Bloquée : failed-test | build OK, test échec | — |
| `treasury-readonly-acceptance-121` | Publiée | build OK, test OK | [`0ed00529a628`](https://github.com/Mickael-ROUSSET/gestionTresorerie/commit/0ed00529a628) |
| `treasury-gmail-import-boundary-122` | Publiée | build OK, test OK | [`52a5fcd7ad4a`](https://github.com/Mickael-ROUSSET/gestionTresorerie/commit/52a5fcd7ad4a) |
| `treasury-document-review-handoff-123` | Bloquée : failed-test | build OK, test échec | — |
| `treasury-main-ui-journey-124` | Publiée | build OK, test OK | [`12b2c90ac3bc`](https://github.com/Mickael-ROUSSET/gestionTresorerie/commit/12b2c90ac3bc) |
| `treasury-windows-release-readiness-125` | Publiée | build OK, test OK | [`5c6254709ea7`](https://github.com/Mickael-ROUSSET/gestionTresorerie/commit/5c6254709ea7) |
| `treasury-windows-startup-smoke-126` | Publiée | build OK, test OK | [`a6569b0fc86c`](https://github.com/Mickael-ROUSSET/gestionTresorerie/commit/a6569b0fc86c) |
| `treasury-gmail-connection-readiness-127` | Publiée | build OK, test OK | [`c4d1363f5442`](https://github.com/Mickael-ROUSSET/gestionTresorerie/commit/c4d1363f5442) |
| `treasury-first-use-review-128` | Publiée | build OK, test OK | [`994154df7787`](https://github.com/Mickael-ROUSSET/gestionTresorerie/commit/994154df7787) |
| `treasury-windows-acceptance-129` | Bloquée : blocked |  | — |
| `treasury-persistence-design-117` | Désactivée (sécurité) |  | — |

## Règles

- Les résultats sont issus des fichiers locaux `backlog.json` et `queue/*.result.json`.
- Un test réussi ne signifie pas qu'un commit a été publié.
- Aucune écriture comptable ou fusion vers `master` n'est autorisée par cette synchronisation.
- Le backlog exécutable reste sur le PC ; ceci est une copie de lecture.

[Voir la PR de développement #77](https://github.com/Mickael-ROUSSET/gestionTresorerie/pull/77)
