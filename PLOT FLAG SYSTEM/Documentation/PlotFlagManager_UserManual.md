# Plot Flag Manager - User Manual

## Overview

The **Plot Flag Manager** is a powerful scripting component for Endstar that allows creators to track game progress through named flags. Flags are boolean (true/false) values that can be set, checked, and reset during gameplay. This system is ideal for:

- Tracking collectibles (treasures, coins, keys)
- Managing puzzle progress
- Controlling quest completion states
- Unlocking doors, rewards, or new areas based on player actions
- Creating multi-step objectives

The Plot Flag Manager maintains an internal state for each flag you define and provides events that fire when flags change, allowing you to wire up complex game logic without writing additional code.

---

## Inspector Fields

Configure these fields in the Inspector panel when the Plot Flag Manager script is selected:

### FlagNames (string[])

A list of unique string names for each flag you want to track.

| Property | Type | Description |
|----------|------|-------------|
| FlagNames | string[] | Array of flag name strings |

**Guidelines:**
- Each flag name must be unique
- Use descriptive names (e.g., "Treasure1", "LeverA", "KeyCollected")
- Avoid spaces and special characters
- Names are case-sensitive ("Treasure1" and "treasure1" are different flags)

**Example:**
```
FlagNames[0]: "Treasure1"
FlagNames[1]: "Treasure2"
FlagNames[2]: "Treasure3"
```

---

### DebugPrefix (string)

A prefix string added to all console debug messages output by this component.

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| DebugPrefix | string | "" | Text prepended to debug output |

**Example:**
```
DebugPrefix: "[TreasureQuest]"
```

Console output will appear as: `[TreasureQuest] Treasure1`

---

### ProgressMessage (string)

A customizable message template that displays current progress. Use special placeholders for dynamic values:

| Placeholder | Replaced With |
|-------------|---------------|
| "C" | Number of completed (true) flags |
| "A" | Total number of flags (all) |

| Property | Type | Description |
|----------|------|-------------|
| ProgressMessage | string | Template string with "C" and "A" placeholders |

**Example:**
```
ProgressMessage: "Progress: "C" / "A" collected"
```

When 2 of 3 flags are set, output becomes: `Progress: 2 / 3 collected`

---

## Receivers (Inputs)

Receivers are functions that can be triggered by wiring events from other components to the Plot Flag Manager.

### SetFlagByName

Sets a specified flag to `true`. If the flag is already true, no action is taken (prevents duplicate triggers).

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| flagName | string | Yes | The name of the flag to set |

**Behavior:**
- If `flagName` is empty or null: fires `OnError` event
- If `flagName` is not in FlagNames list: fires `OnInvalidFlag` event
- If flag is already true: does nothing (silent)
- If flag is set successfully: fires `OnFlagTriggered` event and outputs the flag name with progress message

**Wire To:** Any event that should mark progress (e.g., OnInteract, OnTriggerEnter, OnCollect)

---

### CheckFlagByName

Checks the current state of a flag and fires the appropriate event based on its value.

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| flagName | string | Yes | The name of the flag to check |

**Behavior:**
- If `flagName` is empty or null: fires `OnError` event
- If `flagName` is not in FlagNames list: fires `OnInvalidFlag` event
- If flag is `true`: fires `OnFlagTrue` event
- If flag is `false`: fires `OnFlagFalse` event

**Wire To:** Use when you need to branch logic based on flag state

---

### OutputDebugState

Outputs the current state of all flags to the console and fires `OnDebugOutput`.

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| (none) | - | - | No parameters required |

**Output Format:**
```
Flag1=1, Flag2=0, Flag3=1
Progress: 2 / 3 completed
```

Where `1` = true (completed) and `0` = false (not completed)

**Wire To:** Debug buttons, admin commands, or testing triggers

---

### ResetAllFlags

Resets all flags back to `false` and fires `OnFlagsReset`.

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| (none) | - | - | No parameters required |

**Behavior:**
- All flags in FlagNames are set to `false`
- Outputs "RESET" with progress message
- Fires `OnFlagsReset` event

**Wire To:** Level restart triggers, "New Game" buttons, checkpoint resets

---

## Events (Outputs)

Events are signals fired by the Plot Flag Manager that can be wired to trigger actions on other components.

### OnFlagTriggered

Fires when a flag is successfully set to `true` for the first time.

| Event | Parameters | When It Fires |
|-------|------------|---------------|
| OnFlagTriggered | context | After SetFlagByName successfully sets a new flag |

**Common Uses:**
- Play a sound effect
- Show a particle effect
- Update UI elements
- Check if all flags are complete

---

### OnFlagTrue

Fires when CheckFlagByName finds that the specified flag is `true`.

| Event | Parameters | When It Fires |
|-------|------------|---------------|
| OnFlagTrue | context | When checked flag state is true |

**Common Uses:**
- Allow passage through a door
- Show "already collected" message
- Enable a feature

---

### OnFlagFalse

Fires when CheckFlagByName finds that the specified flag is `false`.

| Event | Parameters | When It Fires |
|-------|------------|---------------|
| OnFlagFalse | context | When checked flag state is false |

**Common Uses:**
- Show "not yet collected" message
- Keep a door locked
- Disable a feature

---

### OnDebugOutput

Fires whenever text is output, passing the message string.

| Event | Parameters | When It Fires |
|-------|------------|---------------|
| OnDebugOutput | context, message (string) | When any text output occurs |

**Common Uses:**
- Display messages on a Billboard or TextMesh
- Log to custom UI
- Send to analytics

---

### OnInvalidFlag

Fires when an invalid flag name is provided to SetFlagByName or CheckFlagByName.

| Event | Parameters | When It Fires |
|-------|------------|---------------|
| OnInvalidFlag | context | When flagName is not in FlagNames list |

**Common Uses:**
- Debug logging
- Error handling
- Show warning message

---

### OnError

Fires when an error occurs (e.g., empty flag name provided).

| Event | Parameters | When It Fires |
|-------|------------|---------------|
| OnError | context | When flagName is empty or null |

**Common Uses:**
- Debug logging
- Error handling

---

### OnFlagsReset

Fires when all flags have been reset to false via ResetAllFlags.

| Event | Parameters | When It Fires |
|-------|------------|---------------|
| OnFlagsReset | context | After ResetAllFlags completes |

**Common Uses:**
- Reset related game objects
- Close previously opened doors
- Respawn collectibles
- Reset UI displays

---

## Example 1: Treasure Collection Quest

### Scenario

Create a quest where the player must collect 3 treasures scattered around the level. A billboard displays current progress, and when all 3 treasures are collected, a door opens to reveal a bonus area.

### Props Needed

| Prop | Quantity | Purpose |
|------|----------|---------|
| Plot Flag Manager | 1 | Tracks treasure collection |
| Treasure (collectible prop) | 3 | Items player collects |
| Billboard (or TextMesh) | 1 | Displays progress |
| Door (animated or movable) | 1 | Opens when quest complete |
| Trigger Zone | 1 | Detects when all treasures collected |

### Step-by-Step Setup

#### Step 1: Configure the Plot Flag Manager

Add a **Plot Flag Manager** script to an empty GameObject in your scene.

**Inspector Settings:**

| Field | Value |
|-------|-------|
| FlagNames[0] | "Treasure1" |
| FlagNames[1] | "Treasure2" |
| FlagNames[2] | "Treasure3" |
| DebugPrefix | "[Treasures]" |
| ProgressMessage | "Treasures: "C" / "A"" |

#### Step 2: Set Up the Treasures

Place 3 treasure props in your level. Each treasure needs to send its flag name to the Plot Flag Manager when collected.

**Treasure 1 Configuration:**
- Add an **Interactable** or **Collectible** component
- In the interaction event, send the flag name "Treasure1"

**Treasure 2 Configuration:**
- Add an **Interactable** or **Collectible** component
- In the interaction event, send the flag name "Treasure2"

**Treasure 3 Configuration:**
- Add an **Interactable** or **Collectible** component
- In the interaction event, send the flag name "Treasure3"

#### Step 3: Set Up the Billboard

Place a **Billboard** prop where players can see their progress.

#### Step 4: Set Up the Door

Place a **Door** prop that will open when all treasures are collected. Ensure it has an animation or movement script with a "Open" receiver.

#### Step 5: Create the Counter Check

Add a **Counter** or use script logic to check when completed count equals total count.

### Wiring Connections

#### Treasure Collection Wiring

| Source | Event | Target | Receiver | Parameter |
|--------|-------|--------|----------|-----------|
| Treasure1 (Collectible) | OnCollect | Plot Flag Manager | SetFlagByName | "Treasure1" |
| Treasure2 (Collectible) | OnCollect | Plot Flag Manager | SetFlagByName | "Treasure2" |
| Treasure3 (Collectible) | OnCollect | Plot Flag Manager | SetFlagByName | "Treasure3" |

#### Progress Display Wiring

| Source | Event | Target | Receiver | Parameter |
|--------|-------|--------|----------|-----------|
| Plot Flag Manager | OnDebugOutput | Billboard | SetText | (message) |

#### Door Opening Wiring (Using Counter Method)

| Source | Event | Target | Receiver | Parameter |
|--------|-------|--------|----------|-----------|
| Plot Flag Manager | OnFlagTriggered | Counter | Increment | - |
| Counter | OnTargetReached (target=3) | Door | Open | - |

### Alternative: Door Opening Without Counter

If you want to check completion without a counter, you can wire the `OnFlagTriggered` event to check all flags:

| Source | Event | Target | Receiver | Parameter |
|--------|-------|--------|----------|-----------|
| Plot Flag Manager | OnFlagTriggered | Plot Flag Manager | CheckFlagByName | "Treasure1" |
| Plot Flag Manager | OnFlagTrue | (Chain to check Treasure2, then Treasure3) | ... | ... |

### Visual Diagram

```
[Treasure1] ──OnCollect──> [Plot Flag Manager] ──OnDebugOutput──> [Billboard]
                               │                                   (shows progress)
[Treasure2] ──OnCollect──────>│
                               │
[Treasure3] ──OnCollect──────>│
                               │
                               └──OnFlagTriggered──> [Counter] ──OnTargetReached──> [Door]
                                                     (target: 3)                    (Opens)
```

### Expected Behavior

1. Player approaches Treasure1 and collects it
   - Flag "Treasure1" is set to true
   - Billboard shows: "Treasures: 1 / 3"
   - Console shows: `[Treasures] Treasure1`
   - Counter increments to 1

2. Player collects Treasure2
   - Flag "Treasure2" is set to true
   - Billboard shows: "Treasures: 2 / 3"
   - Counter increments to 2

3. Player collects Treasure3
   - Flag "Treasure3" is set to true
   - Billboard shows: "Treasures: 3 / 3"
   - Counter reaches target (3)
   - Door opens!

4. If player tries to collect same treasure again
   - Nothing happens (flag already true)
   - No duplicate counting

---

## Example 2: Puzzle Sequence (Two Levers)

### Scenario

Create a puzzle room where the player must activate 2 levers in any order. When both levers are activated, a reward spawns in the center of the room.

### Props Needed

| Prop | Quantity | Purpose |
|------|----------|---------|
| Plot Flag Manager | 1 | Tracks lever states |
| Lever (interactable) | 2 | Player activates these |
| Reward Prop (e.g., Chest) | 1 | Spawns when puzzle solved |
| Spawn Point | 1 | Location where reward appears |
| Counter | 1 | Counts activated levers |
| Billboard (optional) | 1 | Shows puzzle progress |

### Step-by-Step Setup

#### Step 1: Configure the Plot Flag Manager

Add a **Plot Flag Manager** script to an empty GameObject.

**Inspector Settings:**

| Field | Value |
|-------|-------|
| FlagNames[0] | "LeverA" |
| FlagNames[1] | "LeverB" |
| DebugPrefix | "[Puzzle]" |
| ProgressMessage | "Levers: "C" / "A" activated" |

#### Step 2: Set Up the Levers

Place 2 lever props on opposite sides of the room.

**Lever A Configuration:**
- Add an **Interactable** component
- Set interaction type to "Toggle" or "Activate"

**Lever B Configuration:**
- Add an **Interactable** component
- Set interaction type to "Toggle" or "Activate"

#### Step 3: Set Up the Reward

Place your reward prop (chest, item, etc.) and set it to **inactive/hidden** by default. It will be activated when the puzzle is solved.

#### Step 4: Set Up the Counter

Add a **Counter** component to track completions.

**Counter Settings:**

| Field | Value |
|-------|-------|
| Target | 2 |
| StartValue | 0 |

### Wiring Connections

#### Lever to Flag Manager Wiring

| Source | Event | Target | Receiver | Parameter |
|--------|-------|--------|----------|-----------|
| Lever A | OnInteract | Plot Flag Manager | SetFlagByName | "LeverA" |
| Lever B | OnInteract | Plot Flag Manager | SetFlagByName | "LeverB" |

#### Flag Manager to Counter Wiring

| Source | Event | Target | Receiver | Parameter |
|--------|-------|--------|----------|-----------|
| Plot Flag Manager | OnFlagTriggered | Counter | Increment | - |

#### Counter to Reward Wiring

| Source | Event | Target | Receiver | Parameter |
|--------|-------|--------|----------|-----------|
| Counter | OnTargetReached | Reward Prop | Activate | - |
| Counter | OnTargetReached | Reward Prop | PlaySpawnEffect | - |

#### Optional: Progress Display Wiring

| Source | Event | Target | Receiver | Parameter |
|--------|-------|--------|----------|-----------|
| Plot Flag Manager | OnDebugOutput | Billboard | SetText | (message) |

### Visual Diagram

```
[Lever A] ──OnInteract──> [Plot Flag Manager] ──OnFlagTriggered──> [Counter]
                               │                                    (target: 2)
[Lever B] ──OnInteract──────>│                                        │
                               │                                        │
                               └──OnDebugOutput──> [Billboard]          │
                                                   (optional)           │
                                                                        v
                                                              [Reward Prop]
                                                              (Activates & Spawns)
```

### Expected Behavior

**Scenario A: Player activates Lever A first, then Lever B**

1. Player interacts with Lever A
   - Flag "LeverA" is set to true
   - Console shows: `[Puzzle] LeverA`
   - Billboard shows: "Levers: 1 / 2 activated"
   - Counter increments to 1

2. Player interacts with Lever B
   - Flag "LeverB" is set to true
   - Console shows: `[Puzzle] LeverB`
   - Billboard shows: "Levers: 2 / 2 activated"
   - Counter reaches target (2)
   - Reward spawns!

**Scenario B: Player activates Lever B first, then Lever A**

1. Player interacts with Lever B
   - Flag "LeverB" is set to true
   - Billboard shows: "Levers: 1 / 2 activated"
   - Counter increments to 1

2. Player interacts with Lever A
   - Flag "LeverA" is set to true
   - Billboard shows: "Levers: 2 / 2 activated"
   - Counter reaches target (2)
   - Reward spawns!

**Scenario C: Player tries to activate same lever twice**

1. Player interacts with Lever A
   - Flag "LeverA" is set to true
   - Counter increments to 1

2. Player interacts with Lever A again
   - Flag already true, nothing happens
   - Counter stays at 1
   - No duplicate counting!

### Adding a Reset Feature (Optional)

If you want players to be able to reset the puzzle:

| Source | Event | Target | Receiver | Parameter |
|--------|-------|--------|----------|-----------|
| Reset Button | OnInteract | Plot Flag Manager | ResetAllFlags | - |
| Plot Flag Manager | OnFlagsReset | Counter | Reset | - |
| Plot Flag Manager | OnFlagsReset | Reward Prop | Deactivate | - |
| Plot Flag Manager | OnFlagsReset | Lever A | ResetState | - |
| Plot Flag Manager | OnFlagsReset | Lever B | ResetState | - |

---

## Tips and Best Practices

### Naming Conventions

- Use clear, descriptive flag names: "RedKey", "BossDefeated", "TutorialComplete"
- Use consistent prefixes for related flags: "Coin1", "Coin2", "Coin3"
- Avoid spaces and special characters in flag names

### Debugging

- Use `OutputDebugState` receiver to check all flag states during testing
- Set a descriptive `DebugPrefix` to identify which Plot Flag Manager is outputting
- Wire `OnDebugOutput` to a Billboard during development to see real-time status

### Performance

- One Plot Flag Manager can handle many flags efficiently
- Consider using separate Plot Flag Managers for unrelated systems (e.g., one for collectibles, one for story progress)

### Common Patterns

**Completion Gate:**
Use a Counter with target equal to total flags, increment on `OnFlagTriggered`

**Conditional Check:**
Wire `OnFlagTrue` and `OnFlagFalse` to different actions for branching logic

**Progress Persistence:**
Connect to a save system by wiring flag states to save data on `OnFlagTriggered`

### Troubleshooting

| Problem | Solution |
|---------|----------|
| Flag not setting | Check that flag name matches exactly (case-sensitive) |
| OnFlagTriggered not firing | Flag may already be true; it only fires on first set |
| Invalid flag errors | Verify flag name is in FlagNames array |
| Progress not updating | Ensure OnDebugOutput is wired to Billboard's SetText |
| Counter counting twice | Check for duplicate wiring or multiple triggers |

---

## Quick Reference Card

### Inspector Fields
```
FlagNames[]     - List of flag names to track
DebugPrefix     - Prefix for console output
ProgressMessage - Template with "C" (completed) and "A" (all)
```

### Receivers (Inputs)
```
SetFlagByName(flagName)   - Set flag to true
CheckFlagByName(flagName) - Check flag state
OutputDebugState          - Output all flag states
ResetAllFlags             - Reset all flags to false
```

### Events (Outputs)
```
OnFlagTriggered    - Flag was set (first time only)
OnFlagTrue         - Checked flag is true
OnFlagFalse        - Checked flag is false
OnDebugOutput      - Text was output (includes message)
OnInvalidFlag      - Invalid flag name used
OnError            - Error occurred
OnFlagsReset       - All flags were reset
```

---

*This manual covers the Plot Flag Manager system for Endstar. For additional support, consult the Endstar creator documentation or community resources.*
