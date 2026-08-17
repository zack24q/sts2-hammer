# HammerMod automated tests

Run the fast rule and card-contract suite from the repository root:

```bash
dotnet test tests/HammerMod.Tests/HammerMod.Tests.csproj \
  -p:CopyModOnBuild=false \
  -p:RunPckExport=false \
  -p:RitsuLibAutoCopy=false
```

Run the test project directly. It is intentionally kept out of `HammerMod.sln` because
building both projects as solution roots stalls the shared Release build process in the
current Godot/Publicizer toolchain.

The current suite covers deterministic combat formulas (including counter scaling),
charge-release replay snapshots, construction, upgrade, registration, and localization
contracts for every Hammer card, plus a compatibility guard for the locked game build's
combat test-support API.

Do not initialize `CombatManager` from this plain .NET test process. The game build exposes
`TestMode`, mock encounters, and mock monsters, but a fixture spike that called
`CombatManager.SetUpCombat` outside Godot terminated the test host with exit code 139.
Full card-resolution tests therefore need a separate harness hosted by the Godot/game
process. Tests that use global game state must remain non-parallel.
