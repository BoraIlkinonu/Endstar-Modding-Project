# Zayed Character Mod

## YOUR JOB

Research and understand Endstar's internals by:
1. Analyzing DLLs at `C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed\`
2. Using PowerShell reflection to inspect types, methods, fields
3. Collecting runtime data from Unity logs at `%USERPROFILE%\AppData\LocalLow\Endless Studios\Endstar\Player.log`

Do real research. Get actual data. No guessing.

## RULES

- You CANNOT launch Endstar.exe or Endless Launcher
- You CAN use any PowerShell, cmd, or bash commands
- You CAN read/write/edit any files
- Figure out how to research on your own

## KEY DLLs

- `Assembly-CSharp.dll` - Main game logic
- `Assembly-CSharp-firstpass.dll` - Early game code
- `UnityEngine.*.dll` - Unity engine modules

## GOAL

Understand how character systems work so you can create mods for Zayed character.
