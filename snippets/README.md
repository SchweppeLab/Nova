# NovaSnippets

Every code example published on the Nova website is compiled here first. A
snippet that does not build does not go on the site.

This is not a test project. Snippets are written the way a Nova user writes code
in their own project, which is why Nova is restored from the release package
rather than from a project reference. If the release is awkward to consume, that
is worth knowing, and this is where it shows up.

This folder is excluded from the Jekyll build and publishes nothing.

## Restoring Nova

Nova is **not on nuget.org**. It ships as nupkg files inside the release zip, so
the packages must be fetched once before the project will build.

1. Download the nupkg zip for the current release from the
   [releases page](https://github.com/SchweppeLab/Nova/releases). For
   v1.0.0.18 that is `Nova-1_0_0_18-nupkg.zip`.
2. Extract it.
3. Copy into `packages/`:
   - `Nova.<version>.nupkg`
   - `Nova.IO.<version>.nupkg`
   - every `ThermoFisher.CommonCore.*.nupkg` from the `ThermoRawFileReader`
     folder

Do not copy older versions out of the zip. The v1.0.0.18 zip also contains
1.0.0.8 packages, and having both present invites the wrong one being resolved.

`packages/` is gitignored. The packages are release artifacts and do not belong
in this repository.

## Building

```shell
cd snippets
dotnet build
```

## Two traps worth knowing

**There is an unrelated package called `Nova` on nuget.org**, at versions 1.0.0
and 1.0.1, published by someone else. `nuget.config` clears inherited sources
and maps `Nova`, `Nova.IO` and `ThermoFisher.*` to the local folder so that
cannot be resolved by accident.

**`Nova.IO`'s nuspec declares a dependency on `Nova` version `1.0.0`**, not on
the matching build. NuGet resolves the lowest version satisfying a range, so
this project pins both packages to exact versions with bracket notation,
`[1.0.0.18]`. Without that pin the restore can quietly pick up a different
Nova than the one being documented.

## Updating for a new Nova release

1. Fetch and extract the new release zip, replacing the contents of `packages/`
2. Update both `Version` attributes in `NovaSnippets.csproj`
3. Build
4. Whatever fails to compile is an API change the website documentation has not
   caught up with yet
