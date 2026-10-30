# API13 TC relog world compatibility

This candidate is based on `2.5.3.6` / `810b6135e96adfe5991d7770962d9f47463b1586` and keeps all seven submodule pins. The dedicated API13 PR base branch points to that exact release; the default `main` is already net10, and `tw13` has different dependency pins. No existing branch is reset or merged.

Candidate **2.5.3.601** deliberately differs from upstream **2.5.3.7**, which uses API14/net10. Build the actual project with API13 references and the existing net9 target. Do not substitute a DLL from a newer release.

The policy adapts Taiwan production-world IDs and aliases from `ffxiv-tc-port/Lifestream` `f4deed52ca1b4b176af98236f21ac0b80a09aae8`, `Lifestream/PublicWorlds.cs`. This is a narrower relog-only adaptation: require client language 7, an existing row 4028–4035, DC151 and region8 for non-public Taiwan worlds. `泰坦` and `Titan` resolve to 4035; missing rows, malformed DC metadata, development worlds and unknown inputs fail closed. Global public-world exact spelling is retained, and JP Titan must not be selected as a fallback for a missing TC row.

World identity is resolved consistently through the request, connection setup, native character/world selection and character-select IPC dispatch. Service-account keys are read using TC world aliases without rewriting either plugin's configuration; exact local keys keep priority and conflicting alias preferences fail before logout. The character name is never translated or normalized. Request queueing and IPC Success do not prove login completion.

Several existing methods are split into partial files solely so the offline harness compiles their actual production bodies. Native UI callbacks, logout/login confirmation, sheets and the queue clock are synthetic boundaries. No new native offsets, login strategy, cross-DC travel policy, world-menu filters, IPC signature, configuration schema or serialized keys are introduced.

Offline checks, before building Release:

```sh
dotnet run --project tests/RelogWorlds -c Release
```

The harness covers TC/global name collisions, both sheet spellings, missing/wrong rows, aliases, account preference preservation, invalid-world rejection before side effects and the managed logout → connect → lobby world selection → character dispatch → selection → confirmation chain. It does not initialize a game client or validate native callbacks/real authentication.

Cloud Release build:

```sh
dotnet build Lifestream/Lifestream.csproj -c Release \
  -p:EnableWindowsTargeting=true -p:DalamudLibPath=/absolute/API13/reference/directory/ \
  -p:RunAnalyzers=false
```

Initialize only the pinned managed dependency submodules needed by the project; CustomCS remains false. Restore can reconcile the wrapper's old project-reference lock metadata from ECommons 3.0.1.22 to the already pinned source's 3.0.1.26; no package or gitlink is upgraded. That generated lock rewrite is not committed.

Mac acceptance remains separate: first verify a manual **alt → main** switch with the candidate's loaded SHA, then automatic rotation. Preserve Lifestream and AutoRetainer settings independently. Do not interpret offline success as a live switch or assume history fields identify the current character. Stop automation and unload the plugin before replacing/rolling back its complete portable payload. Baseline is the original 2.5.3.6 package; no deployment is performed by this PR.
