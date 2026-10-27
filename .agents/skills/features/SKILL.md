---
name: features
description: |
  Use this skill file when building this mod to clearly outline its intended
  purpose and the features to be implemented.
---

# The goal

This mod aims to control RimWorld's letter system.

# Features

This mod consists of the following features:

- Mod settings screen

- Implementation to control letters based on configured settings


# Mod UI Design

The UI design of this mod must respect RimWorld's native design.


# Settings Screen

This mod shall be implemented so that the settings screen is accessible via
Options > Mod Options, both from the main menu and during gameplay.

## Requirements for the Settings Screen

- Keyword search to find specific event letters

- Filtering functionality to filter by DLC or mod

- A list of **all** letters.

  - The list must support sorting by criteria such as title and mod.
  - As implied above, the letter list must include letters added by mods in
    addition to vanilla and DLC letters.
- The user must be able to select the following behaviors for individual
  letters in the list via a dropdown menu or similar UI element:

  - Ignore: The target letter will not be displayed as an in-game letter,
    and no letter arrival sound will play.
  - Default: Respects the behavior of the "Pause on Letter" game option.
  - Pause: Pauses the game even if the target letter is not a letter type
    covered by the "Pause on Letter" option.

## Settings Application Trigger

When settings are modified, the changes must take effect immediately without
requiring the game to be reloaded.


# Implementation

Implement the letter filtering and control features according to the settings
screen. When implementing, strictly adhere to the General Skill Rules.
