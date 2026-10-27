# Letter Controller

> [!NOTE]
> **AI-assisted development notice:** This mod was created with the assistance of AI (Google Gemini). The code and documentation were reviewed by the author, but may still contain mistakes. Please report any issues you find.

Take full control over RimWorld's letter notifications, **per incident**.

Instead of controlling generic letter categories (`LetterDef`), Letter Controller lets you decide how the letter produced by each individual incident (`IncidentDef`) is handled.

## Features

- Settings screen under **Options > Mod Options**, available from both the main menu and during gameplay.
- Real-time keyword search by incident label, `defName`, or mod name.
- Filter incidents by DLC or source mod.
- Complete list of incidents that send letters: vanilla, DLC, and modded.
- Sort by title, mod, or configured behavior.
- Per-incident behavior:
  | Behavior | Description |
  | --- | --- |
  | **Default** | Vanilla behavior; respects the "Pause on Letter" game option. |
  | **Ignore** | The letter is not shown and its arrival sound does not play. |
  | **Pause** | The game pauses when the letter arrives, even if the letter type is not covered by "Pause on Letter". |
- Reset button to restore all incidents to Default (with a confirmation dialog).
- Changes take effect immediately, with no game restart or reload.

## Requirements

- RimWorld 1.7
- [Harmony](https://steamcommunity.com/sharedfiles/filedetails/?id=2009463077) (`brrainz.harmony`)

## Installation

1. Install Harmony and enable it.
2. Download or clone this repository into your RimWorld `Mods` folder, so the folder is `RimWorld/Mods/LetterController`.
3. Enable **Letter Controller** in the mod list, below Harmony.
4. Restart the game if prompted.

It can be added to or removed from an existing save at any time. Behaviors you changed are stored in the mod settings, not in the save.

## Usage

1. Open **Options > Mod Options > Letter Controller**.
2. Search, filter, or sort to find the incident you want.
3. Choose **Default**, **Ignore**, or **Pause** from the dropdown.

## How It Works

RimWorld letters do not record which incident created them. The mod works as follows:

1. It tracks the currently executing incident by patching `IncidentWorker.TryExecute` with Harmony, including nested incidents.
2. It patches `LetterStack.ReceiveLetter` and looks up the configured behavior for that incident.
   - **Ignore** skips the original method, so no letter or sound is produced.
   - **Pause** pauses the game after the letter is received.
   - **Default** leaves vanilla handling untouched.
3. Letters sent outside an incident (player actions, quests, and so on) are not affected.

Settings are kept in a dictionary keyed by `defName` (case-insensitive), so lookups are O(1). Incidents set to Default are not saved.

## Compatibility

- Works with modded incidents, including ones that send letters from their own worker classes.
- Only letters sent while an incident is executing can be controlled.
- Safe to add or remove mid-save.

## Credits

- Author: Hiroaki Yamamoto
- Developed with AI assistance (Google Gemini)
- Built on [Harmony](https://github.com/pardeike/Harmony) by Andreas Pardeike

## License

Released under the [MIT License](LICENSE.md).
