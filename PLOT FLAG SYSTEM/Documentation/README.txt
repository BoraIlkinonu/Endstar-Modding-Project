===============================================
       ENDSTAR MOD INSTALLER
===============================================

This package allows you to install custom character mods for Endstar.

QUICK START
-----------
1. Install Python 3.x if you don't have it (https://python.org/downloads)
2. Double-click: EndstarModInstaller.py
3. Select option [1] to install mods
4. Launch game with LaunchModdedEndstar.bat

IMPORTANT NOTES
---------------
- Launch game directly with Endstar.exe, NOT via Endless Launcher!
- Endless Launcher will restore original files and remove your mods
- Custom characters work in gameplay but show as silhouette in Party screen

FILES INCLUDED
--------------
- EndstarModInstaller.py  : Full automated installer with menu
- EndstarModPatcher.py    : Command-line patcher
- InstallMods.bat         : Quick install script
- UninstallMods.bat       : Restore original files
- LaunchModdedEndstar.bat : Launch modded game

MODS FOLDER
-----------
Place character mods in the 'mods' subfolder. Each mod should have:
  mods/
    YourMod/
      bundle.bundle   (the Unity asset bundle)
      manifest.json   (mod metadata)

UNINSTALL
---------
Run UninstallMods.bat or select option [2] in the installer to restore
all original game files.

TROUBLESHOOTING
---------------
- "Python not found": Install Python 3 from python.org
- "Game not found": Run installer and enter correct game path
- Mods not loading: Make sure you're NOT using Endless Launcher

KNOWN ISSUES
------------
- Party/lobby screen shows character as silhouette (gameplay works fine)
- Only one character replacement at a time (replaces Felix)

FOR MOD CREATORS
----------------
See CUSTOM_CHARACTER_INJECTION_DOCUMENTATION.md for complete workflow.

Created: January 2026
