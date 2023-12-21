# Community Fixes for KSP 2
This project aims to bring together community bug fixes for Kerbal Space Program 2 in one centralized place.

## Compatibility
- Tested with Kerbal Space Program 2 v0.2.0.0.30291
- Requires **[SpaceWarp 1.6.0+](https://github.com/SpaceWarpDev/SpaceWarp/releases/)**

## Implemented fixes
- **KSP 2 Save Fix** by [jayouimet](https://github.com/jayouimet) - Replaces the Control Owner Part to the first available Command module or to the Root part if not found when it is set to null.
- **Vessel Landed State Fix*** by [munix](https://github.com/jan-bures) - Checks if the vessel's state is Landed when not actually near the ground and resets it. Should fix [this persistent bug](https://forum.kerbalspaceprogram.com/topic/220260-incorrect-landed-state-causing-lack-of-trajectory-lines/).
- **Velocity Display Precision Fix** by [arthomnix](https://github.com/arthomnix) - Fixes the digit after the decimal point on the navball velocity display always being zero at velocities above 100m/s.
- **Suppress Transmissions Falsely Urgent Fix** by [schlosrat](https://github.com/schlosrat) - Suppresses unhelpful map view log messages.
- **VAB Redo Tooltip Fix** by [coldrifting](https://github.com/coldrifting) - Fixes the VAB Redo button tooltip not being at the same height as the button.
- **Revert After Recovery Fix** by [munix](https://github.com/jan-bures) - Fixes the Revert buttons being enabled after recovering a vessel.

## Planned fixes
To see what fixes are planned to be implemented, you can visit the [Issues page](https://github.com/Bit-Studios/CommunityFixes/issues) on the project's GitHub.

## Installation

### Recommended
1. Use [CKAN](https://github.com/KSP-CKAN/CKAN/releases/latest) to download and install Community Fixes.

### Manual
1. Download and extract [UITK for KSP 2](https://github.com/UitkForKsp2/UitkForKsp2/releases) into your game folder.
2. Download and extract [SpaceWarp](https://github.com/SpaceWarpDev/SpaceWarp/releases) into your game folder.
3. Download and extract this mod into the game folder. If done correctly, you should have the following folder structure: `<KSP Folder>/BepInEx/plugins/CommunityFixes`.

## Configuration
If you want to toggle any of the included fixes off, you can do so in game: `Main menu` -> `Settings` -> `Mods` -> `Community Fixes`. The changes will apply after restarting the game.

## Development wiki
If you'd like to contribute to this project, please take a look at [our wiki](https://github.com/Bit-Studios/CommunityFixes/wiki/Adding-your-fix).
