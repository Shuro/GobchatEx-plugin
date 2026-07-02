# GobchatEx Plugin

Dalamud plugin for FFXIV, built with Dalamud.NET.Sdk.

## API Ground Truth — check `.references/` before the web

The `.references/` submodules contain the actual source of the APIs this plugin
builds against. When verifying a Dalamud or FFXIVClientStructs API (signatures,
service members, enums, struct layouts), grep these first — they beat both web
search and the curated `dalamud-*` skills, which may drift from the real API.

| What | Where |
|---|---|
| Dalamud service interfaces (`IChatGui`, `IClientState`, …) | `.references/Dalamud/Dalamud/Plugin/Services/I*.cs` |
| Plugin lifecycle, `IDalamudPluginInterface`, IPC | `.references/Dalamud/Dalamud/Plugin/` |
| Game structs, agents, UI modules | `.references/FFXIVClientStructs/FFXIVClientStructs/FFXIV/` |

Use the web only for things not in the source: changelogs, submission policy
(DalamudPluginsD17), community discussion.

Note: both submodules are shallow and pinned to a commit. If an answer from
`.references/` conflicts with the SDK/API level the plugin targets, check the
pin first; refresh with `git submodule update --remote --depth 1`.
