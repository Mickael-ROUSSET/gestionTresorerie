# Backlog gestionTresorerie

Vue de consultation générée automatiquement à partir du backlog local.
**Ne pas modifier ce fichier pour piloter l'orchestrateur.**

**Actualisation :** 2026-10-30 19:36 UTC
**Objectif :** Link read-only review to corrections and establish safe preapproval gates with offline regression tests; no automatic accounting writes

| Tâche | État | Validation | Commit |
|---|---|---|---|
| `treasury-review-correction-link-113` | Bloquée : failed-test | build OK, test échec | — |
| `treasury-prevalidation-rules-114` | Publiée | build OK, test OK | [`c26d1394664f`](https://github.com/Mickael-ROUSSET/gestionTresorerie/commit/c26d1394664f) |
| `treasury-review-session-115` | Publiée | build OK, test OK | [`e2fb1939726f`](https://github.com/Mickael-ROUSSET/gestionTresorerie/commit/e2fb1939726f) |
| `treasury-review-endtoend-tests-116` | Publiée | build OK, test OK | [`9a7ddbb2b7a8`](https://github.com/Mickael-ROUSSET/gestionTresorerie/commit/9a7ddbb2b7a8) |
| `treasury-ui-session-integration-118` | Publiée | build OK, test OK | [`c026623c6064`](https://github.com/Mickael-ROUSSET/gestionTresorerie/commit/c026623c6064) |
| `treasury-main-review-navigation-119` | À faire |  | — |
| `treasury-document-review-pipeline-120` | À faire |  | — |
| `treasury-readonly-acceptance-121` | À faire |  | — |
| `treasury-persistence-design-117` | Désactivée (sécurité) |  | — |

## Règles

- Les résultats sont issus des fichiers locaux `backlog.json` et `queue/*.result.json`.
- Un test réussi ne signifie pas qu'un commit a été publié.
- Aucune écriture comptable ou fusion vers `master` n'est autorisée par cette synchronisation.
- Le backlog exécutable reste sur le PC ; ceci est une copie de lecture.

[Voir la PR de développement #77](https://github.com/Mickael-ROUSSET/gestionTresorerie/pull/77)
