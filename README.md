# Feed the Realm — World Editor

Unity-based front-end client for designing, editing, and uploading game worlds to the Feed the Realm platform.

Built with **Unity 6000.3.10f1**.

To visit the main game repository [[click here]](https://github.com/feedTheRealm-org/game/).

## How to build

Open the project in Unity (version `6000.3.10f1`) and build the client executable from `File > Build Profiles`.

## How to run

After building, run the generated executable from the `Builds/` folder.

## CI/CD

| Workflow | Description |
|---|---|
| `precommit-check.yaml` | Linting and formatting checks on pull requests |
| `git-leaks.yaml` | Scans for accidentally committed secrets |

## Makefile Commands

Utility commands for cleaning local editor data.

```bash
make clean                          # No-op by default (all flags are false)
make clean CLEAN_ASSETS=true        # Remove downloaded assets
make clean CLEAN_WORLDS=true        # Remove local world data
make clean CLEAN_SESSION=true       # Remove local session data
make clean CLEAN_ALL=true           # Remove all of the above
make clean CLEAN_ALL=true WINDOWS=true  # Same, using Windows-compatible paths
```

## Structure

```
.
├── Assets/
│   ├── 1_FeedTheRealm/       # Project source (scenes, scripts, prefabs)
│   ├── Cartoon_Texture_Pack/ # Asset pack
│   ├── HeroEditor4D/         # Asset pack
│   ├── Plugins/              # Third-party plugins
│   ├── Settings/             # Unity project settings assets
│   └── TextMesh Pro/         # Asset pack
├── Builds/                   # Compiled editor executables
├── Packages/                 # Unity package manifest
├── ProjectSettings/          # Unity project settings
├── Scripts/                  # Shell scripts (clean_assets.sh, clean_worlds.sh, clean_session.sh)
└── .github/workflows/        # CI/CD pipeline definitions
```

## Asset Packs

❗ **Please keep this list up to date!**

| Pack | Link |
|---|---|
| Cartoon Texture Pack | [Cartoon Texture HD - Megapack](https://assetstore.unity.com/packages/2d/textures-materials/cartoon-texture-hd-megapack-40870) |
| HeroEditor4D | [Character Editor 4D \[Megapack\]](https://assetstore.unity.com/packages/2d/characters/character-editor-4d-megapack-147364) |
| TextMesh Pro | [Documentación oficial Unity](https://docs.unity3d.com/Manual/com.unity.textmeshpro.html) |

## External Dependencies

❗ **Please keep this list up to date!**

| Package | Link |
|---|---|
| Feed the Realm — Shared Package | [shared-unity-package](https://github.com/feedTheRealm-org/shared-unity-package/) |
| VContainer | [vcontainer.hadashikick.jp](https://vcontainer.hadashikick.jp/) |
| UniTask | [Cysharp/UniTask](https://github.com/Cysharp/UniTask) |
| Simple File Browser | [yasirkula/UnitySimpleFileBrowser](https://github.com/yasirkula/UnitySimpleFileBrowser) |
