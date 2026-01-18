================================================================================
PLOT FLAG SYSTEM FOR ENDSTAR
================================================================================

This system provides a centralized way to manage plot flags, quest stages, and
game state in Endstar. It uses the built-in Context persistence which
automatically saves data between levels within a session.

================================================================================
FILES INCLUDED
================================================================================

1. PlotFlagManager.lua   - Main centralized flag management
2. PlotFlagChecker.lua   - Utility for checking flags on any prop
3. QuestStageChecker.lua - Specialized quest stage management

================================================================================
QUICK START
================================================================================

1. Import the scripts into your Endstar project via Prop Tool > Lua Script IDE

2. For the PlotFlagManager:
   - Create a Rule Block prop and add the PlotFlagManager script
   - Configure the FlagName in the Inspector
   - Wire triggers TO the receivers (SetFlagTrue, IncrementFlag, etc.)
   - Wire FROM the events to your game logic

3. For checking flags elsewhere:
   - Add PlotFlagChecker to props that need to check flags
   - Configure the same FlagName and FlagType
   - Wire your trigger TO CheckFlag
   - Wire FROM OnPassed/OnFailed to your logic

================================================================================
PLOTFLAGMANAGER.LUA - REFERENCE
================================================================================

INSPECTOR PROPERTIES:
---------------------
- FlagName (string)        : Name of the flag to operate on
- UseGlobalScope (bool)    : true=global, false=per-player
- DefaultIntValue (int)    : Default for unset int flags
- DefaultBoolValue (bool)  : Default for unset bool flags
- DefaultStringValue (str) : Default for unset string flags
- DefaultFloatValue (float): Default for unset float flags

RECEIVERS (wire TO these):
--------------------------
- SetFlagTrue       : Sets FlagName bool to true
- SetFlagFalse      : Sets FlagName bool to false
- ToggleFlag        : Toggles FlagName bool
- IncrementFlag     : Adds 1 to FlagName int
- DecrementFlag     : Subtracts 1 from FlagName int
- CheckBoolFlag     : Fires OnConditionTrue/False based on FlagName
- CheckIntFlagEquals: Compares FlagName int to DefaultIntValue
- CheckIntFlagAtLeast: Checks if FlagName int >= DefaultIntValue
- ResetAllFlags     : Clears the local cache

EVENTS (wire FROM these):
-------------------------
- OnBoolFlagChanged   : Any bool flag changed
- OnIntFlagChanged    : Any int flag changed
- OnFloatFlagChanged  : Any float flag changed
- OnStringFlagChanged : Any string flag changed
- OnAnyFlagChanged    : Any flag of any type changed
- OnQuestStageChanged : A quest stage changed
- OnConditionTrue     : Check receiver passed
- OnConditionFalse    : Check receiver failed
- OnFlagsReset        : ResetAllFlags was called

LUA FUNCTIONS (for advanced scripting):
---------------------------------------
SetBool(context, flagName, value)
GetBool(context, flagName) -> bool
ToggleBool(context, flagName)

SetInt(context, flagName, value)
GetInt(context, flagName) -> int
IncrementInt(context, flagName, amount)
DecrementInt(context, flagName, amount)

SetFloat(context, flagName, value)
GetFloat(context, flagName) -> float

SetString(context, flagName, value)
GetString(context, flagName) -> string

SetQuestStage(context, questName, stage)
GetQuestStage(context, questName) -> int
AdvanceQuest(context, questName)
IsQuestAtLeast(context, questName, minStage) -> bool
IsQuestCompleted(context, questName) -> bool
IsQuestStarted(context, questName) -> bool
IsQuestFailed(context, questName) -> bool

================================================================================
QUESTSTAGECHECKER.LUA - REFERENCE
================================================================================

QUEST STAGES:
-------------
-1 = FAILED
 0 = NOT_STARTED
 1 = STARTED
 2 = IN_PROGRESS
 3 = OBJECTIVE_COMPLETE
 4 = READY_TO_TURN_IN
 5 = COMPLETED

INSPECTOR PROPERTIES:
---------------------
- QuestName (string)      : Name of the quest
- UseGlobalScope (bool)   : true=global, false=per-player
- RequiredStage (int)     : Stage to compare against
- SetToStage (int)        : Stage to set with SetStage receiver
- CheckOnStart (bool)     : Auto-check when level loads

RECEIVERS:
----------
- CheckStageEquals     : Is stage exactly RequiredStage?
- CheckStageAtLeast    : Is stage >= RequiredStage?
- CheckQuestStarted    : Is quest started (stage > 0)?
- CheckQuestCompleted  : Is quest at COMPLETED stage?
- CheckQuestFailed     : Is quest at FAILED stage?
- CheckQuestInProgress : Is quest started but not done?
- SetStage             : Set to SetToStage value
- StartQuest           : Set to STARTED (stage 1)
- AdvanceQuest         : Increment stage by 1
- CompleteQuest        : Set to COMPLETED (stage 5)
- FailQuest            : Set to FAILED (stage -1)
- ResetQuest           : Set to NOT_STARTED (stage 0)

EVENTS:
-------
- OnQuestStageChanged  : Stage changed
- OnQuestStarted       : Quest just started
- OnQuestCompleted     : Quest just completed
- OnQuestFailed        : Quest just failed
- OnQuestReset         : Quest was reset
- OnCheckPassed        : Check condition met
- OnCheckFailed        : Check condition not met

================================================================================
EXAMPLE SCENARIOS
================================================================================

EXAMPLE 1: Door that unlocks after talking to NPC
-------------------------------------------------
Setup:
1. NPC has PlotFlagManager with FlagName="TalkedToGuard"
2. When dialogue ends, wire to SetFlagTrue
3. Door has PlotFlagChecker with FlagName="TalkedToGuard"
4. Door's interact wires to CheckFlag
5. OnPassed wires to Door's Open receiver
6. OnFailed wires to show "Talk to the guard first" message


EXAMPLE 2: Quest with multiple stages
-------------------------------------
Setup:
1. Create QuestStageChecker with QuestName="FindTheArtifact"

Quest flow:
- Quest giver NPC: Wire dialogue end to StartQuest
- Finding artifact: Wire pickup to AdvanceQuest
- Returning to NPC:
  - CheckStageAtLeast (RequiredStage=2) to verify player has artifact
  - OnPassed wires to CompleteQuest and give reward


EXAMPLE 3: Counter for collectibles
-----------------------------------
Setup:
1. PlotFlagManager with FlagName="CoinsCollected", UseGlobalScope=false
2. Each coin pickup wires to IncrementFlag
3. Door with PlotFlagChecker:
   - FlagName="CoinsCollected"
   - FlagType="Int"
   - CompareMode="AtLeast"
   - CompareIntValue=10
4. Door interact wires to CheckFlag
5. OnPassed opens door, OnFailed shows "Need 10 coins"


EXAMPLE 4: Branching dialogue based on choices
----------------------------------------------
Setup:
1. PlotFlagManager with FlagName="PlayerChoice", UseGlobalScope=false
2. Dialogue choice A: Set DefaultIntValue=1, wire to SetStage (use SetInt)
3. Dialogue choice B: Set DefaultIntValue=2, wire to SetStage
4. Later NPC uses PlotFlagChecker:
   - FlagType="Int"
   - CompareMode="Equals"
   - CompareIntValue=1 for choice A response
   - CompareIntValue=2 for choice B response

================================================================================
PERSISTENCE NOTES
================================================================================

WITHIN SESSION:
- Flags automatically persist across level transitions
- Uses Endstar's built-in Context:SetBool/SetInt/SetFloat/SetString
- Data is stored on the Context (player or prop)

CROSS-SESSION:
- Currently, data does NOT persist after the game session ends
- Endstar may have server-side storage for published games
- For cross-session persistence, you would need to:
  1. Check if Endstar has a SaveData API (not found in current docs)
  2. Potentially use player inventory items as "flag tokens"
  3. Wait for Endstar to add persistent storage features

================================================================================
TIPS
================================================================================

1. Use consistent naming: "Quest_MainStory", "Flag_DoorUnlocked", etc.

2. For per-player flags, ensure you pass player context through wiring

3. Test with multiple players if using per-player flags

4. Use global scope for world-state flags (boss defeated, bridge built)

5. Use per-player scope for individual progress (quests, dialogue)

6. The Log() calls help debug - check console output during testing
