# Loom Build Instructions

## Prerequisites

- .NET SDK from `global.json`
- Tool manifest in `dotnet-tools.json`

Install/restore tools:

```bash
dotnet tool restore
```

Required tools and dependent Loom modules:

| Tool Command | Tool Package | Dependent Module(s) |
| --- | --- | --- |
| `loom` | `loom.build` | CLI entry point used to run all targets/modules |
| `minver` | `minver-cli` | `MinVerModule` |
| `vpk` | `vpk` | `VelopackReleaseModule` |
| `reportgenerator` | `dotnet-reportgenerator-globaltool` | `ReportGeneratorModule` |

## Setup

Initialize Loom files:

```bash
dotnet loom init
```

Run tests:

```bash
dotnet loom test
```

Run release pipeline:

```bash
dotnet loom release
```

## Enable NuGet and GitHub Releases

To allow upload/publishing modules to run, enable the following flags in `.build/loom.json`:

```json
{
    "workspace": {
        "enableNugetUpload": true,
        "enableGithubRelease": true
    }
}
```

Also configure required GitHub secrets:

- `GITHUB_TOKEN` is the built-in GitHub Actions token (`secrets.GITHUB_TOKEN`).
- Create a repository secret named `NUGET_API_KEY`.

See release workflow setup in [.github/workflows/release.yml](../.github/workflows/release.yml).
