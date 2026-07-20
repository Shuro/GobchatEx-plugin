# GobchatEx Plugin

Dalamud plugin for FFXIV, built with Dalamud.NET.Sdk.

## API Ground Truth — check `.references/` before the web

The `.references/` folder holds local clones of the actual source of the APIs
this plugin builds against. When verifying a Dalamud or FFXIVClientStructs API
(signatures, service members, enums, struct layouts), grep these first — they
beat both web search and the curated `dalamud-*` skills, which may drift from
the real API.

| What | Where |
|---|---|
| Dalamud service interfaces (`IChatGui`, `IClientState`, …) | `.references/Dalamud/Dalamud/Plugin/Services/I*.cs` |
| Plugin lifecycle, `IDalamudPluginInterface`, IPC | `.references/Dalamud/Dalamud/Plugin/` |
| Game structs, agents, UI modules | `.references/FFXIVClientStructs/FFXIVClientStructs/FFXIV/` |

Use the web only for things not in the source: changelogs, submission policy
(DalamudPluginsD17), community discussion.

Note: `.references/` is local-only and git-ignored (not committed, not a
submodule) so Plogon doesn't fetch it when building for DalamudPluginsD17. If an
answer from `.references/` conflicts with the SDK/API level the plugin targets,
its clones may be stale — refresh by pulling in each clone (`git -C
.references/Dalamud pull` / `git -C .references/FFXIVClientStructs pull`), or
re-clone from `goatcorp/Dalamud` and `aers/FFXIVClientStructs`.
