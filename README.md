# Community Fixes for KSP 2
This project aims to bring together community bug fixes for Kerbal Space Program 2 in one centralized place.

## Compatibility
- Tested with Kerbal Space Program 2 v0.1.0.0.20892
- Requires **[SpaceWarp 0.4+](https://github.com/SpaceWarpDev/SpaceWarp/releases/)**

## Implemented fixes
- **Separation CommNet Fix** by [munix](https://github.com/jan-bures) - Fixes CommNet disconnecting after separating two controllable vessels.
- **Sticky Orbit Markers** by [munix](https://github.com/jan-bures) - Makes Ap/Pe and other orbit markers stay pinned when plotting a maneuver.
- **KSP 2 Save Fix** by [jayouimet](https://github.com/jayouimet) - Replaces the Control Owner Part to the first available Command module or to the Root part if not found when it is set to null.
- **Vessel Landed State Fix*** by [munix](https://github.com/jan-bures) - Checks if the vessel's state is Landed when not actually near the ground and resets it.

_*This fix is experimental and untested for now - if you are experiencing the bug with its fix enabled, please create an issue [here](https://github.com/Bit-Studios/CommunityFixes/issues)._

## Planned fixes
To see what fixes are planned to be implemented, you can visit the [Issues page](https://github.com/Bit-Studios/CommunityFixes/issues) on the project's GitHub.

## Installation
1. Download and extract [SpaceWarp 0.4+](https://github.com/SpaceWarpDev/SpaceWarp/releases) into your game folder - if you don't already have BepInEx installed, use the version `spacewarp-<version>+bepinex.zip`.
2. Download and extract this mod into the game folder. If done correctly, you should have the following folder structure: `<KSP Folder>/BepInEx/plugins/community_fixes`.

## Configuration
If you want to toggle any of the included fixes off, you can do so in game: `Main menu` -> `Mods` -> `Open Configuration Manager` -> `Community Fixes`. The changes will apply after restarting the game.

## Development wiki
If you'd like to contribute to this project, please take a look at [our wiki](https://github.com/Bit-Studios/CommunityFixes/wiki/Adding-your-fix).
