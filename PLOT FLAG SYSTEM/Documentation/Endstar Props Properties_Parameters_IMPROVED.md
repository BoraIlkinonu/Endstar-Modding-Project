# Endstar Props & Rule Blocks - Complete Guide

> **For Students Ages 11-18** | KF Winter Camp Game Development

---

## PART 1: QUICK START CONCEPTS

### What's the Difference Between Props and Rule Blocks?

| Props | Rule Blocks |
|-------|-------------|
| **Visible** objects in your game | **Invisible** logic controllers |
| Players can see and interact with them | Work behind the scenes |
| Examples: Doors, Keys, Treasure, NPCs | Examples: Timer, Counter, Trigger Volume |
| Have physical presence | Define behaviors and rules |

**Think of it this way:** Props are the *actors* in your game. Rule Blocks are the *directors* telling them what to do.

---

### Understanding Wiring: Sender & Receiver

Wiring connects props and rule blocks so they can communicate and trigger actions.

**Key Concept:**
- **Sender** = "When THIS happens..." (the trigger)
- **Receiver** = "...do THAT" (the action)

**Example:** When player picks up a Key (Sender: OnPickup) → Open the Door (Receiver: Open)

#### How to Wire (Step by Step)
1. Press **Key 5** on keyboard to see wiring options
2. **Click first** on the prop/rule block you want as **Sender** (appears on LEFT)
3. **Click second** on the prop/rule block you want as **Receiver** (appears on RIGHT)
4. Click and drag from Sender's connector point to Receiver's connector point
   - OR single-click Sender connector, then single-click Receiver connector
5. A **colored wire** appears connecting them
6. Choose options in the **pop-up menu** that appears
7. **Click "Confirm"** to finalize (IMPORTANT - don't skip this!)

#### Deleting a Wire
- Click on the wire/line between two connected items
- Click **"Delete"** button in the pop-up

#### Understanding "On..." Names
When you see wiring options starting with **"On..."** (like "OnPickup", "OnTimerFinished"):
- "On" simply means **"When"**
- "OnPickup" = "When picked up"
- "OnTimerFinished" = "When timer finishes"

---

## PART 2: PROPS & RULE BLOCKS BY CATEGORY

### Category Overview

| Category | What It's For | Difficulty |
|----------|---------------|------------|
| 🎒 **Collectibles & Items** | Things players pick up | ⭐ Beginner |
| 🚪 **Doors & Barriers** | Controlling access and paths | ⭐ Beginner |
| 🔘 **Triggers & Switches** | Player-activated controls | ⭐ Beginner |
| 🧠 **Logic & Flow Control** | Game rules and conditions | ⭐⭐ Intermediate |
| ⏱️ **Timers & Counters** | Tracking time and counts | ⭐⭐ Intermediate |
| 👾 **NPCs & Combat** | Enemies, allies, fighting | ⭐⭐ Intermediate |
| 🗣️ **NPC Behavior Nodes** | Controlling NPC actions | ⭐⭐⭐ Advanced |
| 🌍 **Environment & Effects** | World settings, visuals | ⭐⭐ Intermediate |
| 🚀 **Movement & Teleportation** | Moving players around | ⭐⭐ Intermediate |
| 📺 **UI & Feedback** | Showing info to players | ⭐⭐ Intermediate |
| 🎮 **Level & Spawn Management** | Game flow and respawning | ⭐⭐ Intermediate |

---

## PART 3: DETAILED REFERENCE BY CATEGORY

---

## 🎒 COLLECTIBLES & ITEMS

Items players can pick up, collect, or use.

---

### Treasure
**Type:** Prop | **Difficulty:** ⭐ Beginner

**What it does:** A collectible item players can pick up. Great for coins, gems, or any reward.

**When to use it:**
- Creating collectible coins or gems
- Reward items scattered in levels
- Objectives like "collect all treasures"

**Inspector Properties:**
| Property | Description |
|----------|-------------|
| Name | Unique identifier for this treasure |

**Wiring Options:**
| Sender | What it means |
|--------|---------------|
| OnPickup | Triggers when player collects this treasure |

**Works well with:**
- **Counter Rule** - Track how many treasures collected
- **Door - Mechanical** - Open door after collecting
- **Anachronist Billboard** - Display collection count
- **Do Once Rule** - Ensure pickup only triggers once

**Example Recipe:** See "Collect X Items to Open Door" in Recipe Patterns

---

### Key
**Type:** Prop | **Difficulty:** ⭐ Beginner

**What it does:** A special collectible that can unlock locked doors.

**When to use it:**
- Classic key-and-lock puzzles
- Gating progress until player explores
- Multi-key door puzzles

**Inspector Properties:**
| Property | Description |
|----------|-------------|
| Name | Unique identifier for this key |

**Wiring Options:**
| Sender | What it means |
|--------|---------------|
| OnPickup | Triggers when player picks up the key |

**Works well with:**
- **Door - Locked** - Assign this key to unlock specific door
- **Counter Rule** - Track multiple keys collected
- **Anachronist Billboard** - Show "Key collected!" message

**Example Recipe:** See "Key and Locked Door" in Recipe Patterns

---

### Resource
**Type:** Prop | **Difficulty:** ⭐ Beginner

**What it does:** A collectible resource item with quantity.

**When to use it:**
- Currency (coins, gold)
- Crafting materials
- Score pickups

**Inspector Properties:**
| Property | Description | Values |
|----------|-------------|--------|
| Name | Unique identifier | Text |
| Quantity | How many resources this gives | 1-10 |

**Wiring Options:**
| Sender | What it means |
|--------|---------------|
| OnCollected | Triggers when player collects this resource |

**Works well with:**
- **Item Quantity Checker** - Check if player has enough
- **Counter Rule** - Track total collected
- **Anachronist Billboard** - Display resource count

---

### Instant Health Pickup
**Type:** Prop | **Difficulty:** ⭐ Beginner

**What it does:** Immediately restores player health when collected.

**When to use it:**
- Health restoration in combat areas
- Rewards after defeating enemies
- Hidden health bonuses

**Inspector Properties:**
| Property | Description |
|----------|-------------|
| Name | Unique identifier |

**Wiring Options:**
| Sender | What it means |
|--------|---------------|
| OnCollected | Triggers when player picks up the health |

**Works well with:**
- **Dynamic Item Spawner** - Drop health when enemy dies
- **Timer Rule** - Respawn health pickup after delay

---

### Healing Injector
**Type:** Prop | **Difficulty:** ⭐ Beginner

**What it does:** A stackable healing item players carry in inventory.

**When to use it:**
- Healing items player saves for later
- Limited healing resources (survival games)
- Inventory-based health system

**Inspector Properties:**
| Property | Description | Values |
|----------|-------------|--------|
| Name | Unique identifier | Text |
| Starting Stack Count | How many uses | 1-10 |

**Wiring Options:**
| Sender | What it means |
|--------|---------------|
| OnPickup | Triggers when player picks up the injector |

---

### Healing Fountain
**Type:** Prop | **Difficulty:** ⭐ Beginner

**What it does:** A stationary healing source that activates periodically.

**When to use it:**
- Safe zones where players can heal
- Checkpoints with healing
- Strategic healing locations in combat areas

**Inspector Properties:**
| Property | Description | Values |
|----------|-------------|--------|
| Name | Unique identifier | Text |
| Initial Interval | Seconds before first activation | Seconds |
| Interval Scalar | Seconds between each heal after first | Seconds |
| Healing Amount | Health restored per activation | Number |

**Note:** This prop has no wiring options - it works automatically based on its settings.

---

### Thrown Bomb
**Type:** Prop | **Difficulty:** ⭐⭐ Intermediate

**What it does:** Explosive weapon players can pick up and throw.

**When to use it:**
- Combat weapon pickup
- Puzzle element (destroy walls)
- Limited-use powerful attack

**Inspector Properties:**
| Property | Description | Values |
|----------|-------------|--------|
| Name | Unique identifier | Text |
| Starting Stack Count | How many bombs | 1-10 |

**Wiring Options:**
| Sender | What it means |
|--------|---------------|
| OnPickup | Triggers when player picks up bombs |

---

### Weapons (One Handed Sword, Two Handed Sword, 1H Range, 2H Ranged)
**Type:** Prop | **Difficulty:** ⭐ Beginner

**What they do:** Weapon pickups players can equip and use.

| Weapon | Description |
|--------|-------------|
| One Handed Sword | Fast melee weapon, can use shield |
| Two Handed Sword Pickup | Powerful but slower melee |
| 1H Range | Single-handed ranged weapon |
| 2H Ranged | Two-handed ranged weapon |

**Inspector Properties:**
| Property | Description |
|----------|-------------|
| Name | Unique identifier |

**Wiring Options:**
| Sender | What it means |
|--------|---------------|
| OnPickup | Triggers when player picks up weapon |

**Works well with:**
- **Dynamic Item Spawner** - Drop weapons from enemies
- **Trigger Volume** - Entering area grants weapon

---

### Dash Pack
**Type:** Prop | **Difficulty:** ⭐ Beginner

**What it does:** Gives player a dash/dodge ability.

**When to use it:**
- Movement upgrade pickup
- Combat mobility enhancement
- Exploration reward

**Inspector Properties:**
| Property | Description |
|----------|-------------|
| Name | Unique identifier |

**Wiring Options:**
| Sender | What it means |
|--------|---------------|
| OnPickup | Triggers when player picks up dash pack |

---

### Jetpack
**Type:** Prop | **Difficulty:** ⭐ Beginner

**What it does:** Gives player flight/hover ability.

**When to use it:**
- Vertical exploration
- Movement upgrade
- Accessing high areas

**Inspector Properties:**
| Property | Description |
|----------|-------------|
| Name | Unique identifier |

**Wiring Options:**
| Sender | What it means |
|--------|---------------|
| OnPickup | Triggers when player picks up jetpack |

---

## 🚪 DOORS & BARRIERS

Control access to areas and create obstacles.

---

### Door - Locked
**Type:** Prop | **Difficulty:** ⭐ Beginner

**What it does:** A door that requires a specific key to open.

**When to use it:**
- Classic key-and-lock puzzles
- Gating important areas
- Exploration rewards

**Inspector Properties:**
| Property | Description | Options |
|----------|-------------|---------|
| Name | Unique identifier | Text |
| Key Reference | Which key unlocks this door | Select a Key prop |
| Locked | Start locked or unlocked | Yes/No |
| NPC Door Interaction | Can NPCs use this door? | Not Openable, Openable, Open and Close Behind |

**Wiring Options:**
| Sender | What it means |
|--------|---------------|
| OnInteractFailed | Player tried to open without key |
| OnDoorOpened | Door finished opening |
| OnDoorClosed | Door finished closing |

| Receiver | What it does | Options |
|----------|--------------|---------|
| Open | Opens the door | Forward Direction |
| Close | Closes the door | - |
| ToggleOpen | Switches open/closed state | Forward Direction |
| Unlock | Unlocks without opening | - |

**Works well with:**
- **Key** - Assign key to unlock
- **Lever** - Alternative unlock mechanism
- **Pressure Plate** - Step-on unlock
- **Counter Rule** - Unlock after multiple conditions met

**Video Tutorial:** See "Bridges" video for gate mechanics: https://youtu.be/buj58tI54xI

---

### Door - Locked (Small)
**Type:** Prop | **Difficulty:** ⭐ Beginner

**What it does:** Same as Door - Locked but smaller size. Perfect for interior doors.

*Properties and wiring identical to Door - Locked*

---

### Door - Mechanical
**Type:** Prop | **Difficulty:** ⭐ Beginner

**What it does:** A door controlled purely by wiring (no key required).

**When to use it:**
- Puzzle-triggered doors
- Doors opened by switches/levers
- Automatic doors

**Inspector Properties:**
| Property | Description | Options |
|----------|-------------|---------|
| Name | Unique identifier | Text |
| NPC Door Interaction | Can NPCs use this door? | Not Openable, Openable, Open and Close Behind |

**Wiring Options:**
| Sender | What it means |
|--------|---------------|
| OnDoorOpened | Door finished opening |
| OnDoorClosed | Door finished closing |

| Receiver | What it does | Options |
|----------|--------------|---------|
| Open | Opens the door | Forward Direction |
| Close | Closes the door | - |
| ToggleOpen | Switches open/closed state | Forward Direction |

**Works well with:**
- **Lever** - Pull lever to open
- **Pressure Plate** - Step to open
- **Counter Rule** - Open after X items collected
- **Timer Rule** - Auto-close after delay
- **Trigger Volume** - Open when player approaches

**Example Recipe:** See "Collect X to Open Door" in Recipe Patterns

---

### Basic Level Gate
**Type:** Prop | **Difficulty:** ⭐⭐ Intermediate

**What it does:** A gate that transitions players to another level when entered.

**When to use it:**
- End of level exits
- Doorways between areas
- Level selection hubs

**Inspector Properties:**
| Property | Description |
|----------|-------------|
| Name | Unique identifier |
| Level | Which level to go to |
| Target Spawn Points | Where players appear in new level |

**Wiring Options:**
| Sender | What it means |
|--------|---------------|
| OnLevelEntered | Player entered the gate |
| OnTransitionStarted | Level change began |

| Receiver | What it does |
|----------|--------------|
| SetText | Change displayed text |
| SetRawText | Set text from another source |

---

## 🔘 TRIGGERS & SWITCHES

Player-activated controls and sensors.

---

### Lever
**Type:** Prop | **Difficulty:** ⭐ Beginner

**What it does:** A switch players can flip between two positions.

**When to use it:**
- Activating mechanisms
- Opening doors
- Turning systems on/off
- Puzzle elements

**Inspector Properties:**
| Property | Description |
|----------|-------------|
| Name | Unique identifier |
| Start in Position 2 | Should lever start flipped? (No action on initial flip) |

**Wiring Options:**
| Sender | What it means |
|--------|---------------|
| OnFlipped | Triggers every time lever is flipped |
| OnFlippedPosition1 | Triggers when flipped to position 1 |
| OnFlippedPosition2 | Triggers when flipped to position 2 |

| Receiver | What it does |
|----------|--------------|
| FlipLever | Flip the lever (from another trigger) |
| SetToPosition1 | Force lever to position 1 |
| SetToPosition2 | Force lever to position 2 |

**Works well with:**
- **Door - Mechanical** - Flip lever to open door
- **Timer Rule** - Start/stop timers
- **NPC Spawner** - Trigger enemy waves
- **Sensor Spike Trap** - Activate/deactivate traps
- **Health Modifier** - Create deadly trap levers

**Video Tutorial:**
- Countdown Timer & Timer Rule (lever controls): https://youtu.be/1QuOaU2-Qyk
- Pull Lever to Kill Character: https://youtu.be/4QoLIxX50XQ

---

### Pressure Plate
**Type:** Prop | **Difficulty:** ⭐ Beginner

**What it does:** A floor switch activated by standing on it.

**When to use it:**
- Weight-triggered puzzles
- Automatic doors
- Trap triggers
- "Stand here" mechanics

**Inspector Properties:**
| Property | Description |
|----------|-------------|
| Name | Unique identifier |

**Wiring Options:**
| Sender | What it means |
|--------|---------------|
| OnPressed | Player stepped on plate |
| OnReleased | Player stepped off plate |

**Works well with:**
- **Door - Mechanical** - Step on to open
- **Sensor Spike Trap** - Step on to trigger trap
- **Bool Gate Rule** - Set true when pressed
- **Counter Rule** - Track how many plates pressed

**Tip:** Use OnPressed for "step on to activate" and OnReleased for "hold down to keep active"

---

### Trigger Volume
**Type:** Rule Block | **Difficulty:** ⭐⭐ Intermediate

**What it does:** An invisible zone that detects when players enter or exit.

**When to use it:**
- Area-based triggers (enter room → something happens)
- Trap zones
- Checkpoint detection
- Cutscene triggers
- Proximity-based events

**Inspector Properties:**
| Property | Description |
|----------|-------------|
| Name | Unique identifier |
| Forward | Zone size forward (units) |
| Backward | Zone size backward (units) |
| Left | Zone size left (units) |
| Right | Zone size right (units) |
| Up | Zone size up (units) |
| Down | Zone size down (units) |
| Offset (X,Y,Z) | Move the zone from the rule block position |
| Only Once | Trigger only first time? |

**Wiring Options:**
| Sender | What it means |
|--------|---------------|
| OnTriggered | Player entered the volume |
| OnExit | Player left the volume |

**Works well with:**
- **Dialogue Node** - Start conversation when player approaches NPC
- **NPC Spawner** - Spawn enemies when player enters area
- **Timer Rule** - Start countdown when entering zone
- **Health Modifier** - Damage zone
- **Door - Mechanical** - Auto-open doors
- **Ambient Settings** - Change atmosphere when entering area

**Video Tutorials:**
- Rising Ocean (trigger volume example): https://youtu.be/fsNwqFtzqsE
- Item Quantity Checker (shop scenario): https://youtu.be/Yi29pfpJg_4
- Pull Lever to Kill NPC: https://youtu.be/4QoLIxX50XQ

---

### Interactable Volume
**Type:** Rule Block | **Difficulty:** ⭐⭐ Intermediate

**What it does:** An invisible zone that shows an interaction prompt and triggers when player interacts.

**When to use it:**
- "Press E to interact" areas
- Custom interaction points
- Object examination
- Dialogue triggers

**Inspector Properties:**
| Property | Description |
|----------|-------------|
| Name | Unique identifier |
| Forward, Backward, Left, Right, Up, Down | Zone dimensions |
| Offset (X,Y,Z) | Position offset |
| Anchor Position | Where to show the interaction prompt |
| Only Once | Allow interaction only once? |

**Wiring Options:**
| Sender | What it means |
|--------|---------------|
| OnInteracted | Player pressed interact in the volume |

**Works well with:**
- **Dialogue Node** - Trigger conversations
- **Door - Mechanical** - Custom door interactions
- **Item Spawner** - Give item on interaction

---

## 🧠 LOGIC & FLOW CONTROL

Control game flow, conditions, and complex behaviors.

---

### Bool Gate Rule
**Type:** Rule Block | **Difficulty:** ⭐⭐ Intermediate

**What it does:** A true/false switch that can trigger different actions based on its state.

**When to use it:**
- On/off state tracking
- Conditional logic (if this, then that)
- Toggle systems
- Requirement checking

**Inspector Properties:**
| Property | Description | Options |
|----------|-------------|---------|
| Name | Unique identifier | Text |
| Initial State | Starting state | Active (true) / Inactive (false) |

**Wiring Options:**
| Sender | What it means |
|--------|---------------|
| OnTrue | Triggers when gate evaluates to true |
| OnFalse | Triggers when gate evaluates to false |

| Receiver | What it does |
|----------|--------------|
| SetGateState | Set to true or false (Completed) |
| EvaluateGate | Check current state and trigger OnTrue/OnFalse |
| FlipGateState | Toggle between true and false |

**Works well with:**
- **Lever** - Set state when flipped
- **Counter Rule** - Set true when count reached
- **Door - Mechanical** - Open when true
- **Multiple Bool Gates** - Complex logic chains

**Example Use:** "Player has key" boolean - set true OnPickup, check before allowing door open

---

### Do Once Rule
**Type:** Rule Block | **Difficulty:** ⭐ Beginner

**What it does:** Ensures something only happens one time, then blocks further triggers.

**When to use it:**
- One-time events (cutscenes, tutorials)
- Preventing exploit/spam
- First-time pickups
- Single-use triggers

**Inspector Properties:**
| Property | Description |
|----------|-------------|
| Name | Unique identifier |

**Wiring Options:**
| Sender | What it means |
|--------|---------------|
| OnTriggered | Fires the FIRST time trigger received |

| Receiver | What it does |
|----------|--------------|
| Trigger | Attempt to trigger (only works once) |
| Reset | Allow the Do Once to trigger again |

**Works well with:**
- **Trigger Volume** - One-time area events
- **Dialogue Node** - One-time conversations
- **Key** - Ensure key only triggers once

**Video Tutorial:** Item Quantity Checker (Do Once example): https://youtu.be/Yi29pfpJg_4

---

### Relay Rule
**Type:** Rule Block | **Difficulty:** ⭐⭐ Intermediate

**What it does:** Receives an event and passes it on to multiple receivers. Like a signal splitter.

**When to use it:**
- One trigger → multiple actions
- Organizing complex wiring
- Signal distribution

**Inspector Properties:**
| Property | Description |
|----------|-------------|
| Name | Unique identifier |

**Wiring Options:**
| Sender | What it means |
|--------|---------------|
| OnEventReceived | Triggers when relay receives an event |

| Receiver | What it does |
|----------|--------------|
| ExecuteEvent | Send event to this relay |

**Works well with:**
- Any prop/rule block that needs to trigger multiple things
- Cleaning up messy wiring

**Example:** Lever flip → Relay → (Door opens AND lights turn on AND sound plays)

---

### Sequence Rule
**Type:** Rule Block | **Difficulty:** ⭐⭐⭐ Advanced

**What it does:** Fires events in order, one after another. Each trigger advances to next step.

**When to use it:**
- Multi-step puzzles
- Sequential events
- Wave-based spawning
- Combo systems

**Inspector Properties:**
| Property | Description | Values |
|----------|-------------|--------|
| Name | Unique identifier | Text |
| Max Sequence | How many steps (1-10) | 1-10 |
| Loop | Restart after reaching end? | Yes/No |

**Wiring Options:**
| Sender | What it means |
|--------|---------------|
| OnResult1 | First event in sequence |
| OnResult2 | Second event |
| ... | ... |
| OnResult10 | Tenth event |

| Receiver | What it does |
|----------|--------------|
| FireNextEvent | Advance to next step and fire it |

**Works well with:**
- **Timer Rule** - Timed sequences
- **NPC Spawner** - Spawn different enemies each wave
- **Door - Mechanical** - Open doors in sequence

**Example:** FireNextEvent → OnResult1 (spawn easy enemy) → FireNextEvent → OnResult2 (spawn medium enemy) → ...

---

### Randomizer Rule
**Type:** Rule Block | **Difficulty:** ⭐⭐ Intermediate

**What it does:** When triggered, randomly fires ONE of up to 10 possible events.

**When to use it:**
- Random rewards
- Unpredictable enemy spawns
- Loot variety
- Random events

**Inspector Properties:**
| Property | Description | Values |
|----------|-------------|--------|
| Name | Unique identifier | Text |
| Max Result | How many possible outcomes (1-10) | 1-10 |

**Wiring Options:**
| Sender | What it means |
|--------|---------------|
| OnResult1 | Random outcome 1 |
| OnResult2 | Random outcome 2 |
| ... | ... |
| OnResult10 | Random outcome 10 |

| Receiver | What it does |
|----------|--------------|
| FireRandomEvent | Trigger a random result |

**Works well with:**
- **Item Spawner** - Random loot drops
- **NPC Spawner** - Random enemy types
- **Dynamic Item Spawner** - Random death drops

---

### Level Start Rule
**Type:** Rule Block | **Difficulty:** ⭐ Beginner

**What it does:** Triggers automatically when the level/game starts.

**When to use it:**
- Initialize game state
- Start background music
- Spawn initial enemies
- Show intro dialogue
- Set initial conditions

**Inspector Properties:**
| Property | Description |
|----------|-------------|
| Name | Unique identifier |
| Only Fire On First Load | Only trigger first time level loads (not on respawn) |

**Wiring Options:**
| Sender | What it means |
|--------|---------------|
| OnLevelStart | Level has started |

**Works well with:**
- **Timer Rule** - Start timers when level begins
- **NPC Spawner** - Spawn enemies at start
- **Countdown Timer** - Begin countdown immediately
- **Dialogue Node** - Opening conversation

---

## ⏱️ TIMERS & COUNTERS

Track time and count events.

---

### Timer Rule
**Type:** Rule Block | **Difficulty:** ⭐⭐ Intermediate

**What it does:** Waits for a specified duration, then triggers an event. Can loop.

**When to use it:**
- Delayed events
- Repeating spawns
- Timed puzzles
- Cooldowns
- Wave intervals

**Inspector Properties:**
| Property | Description | Values |
|----------|-------------|--------|
| Name | Unique identifier | Text |
| Duration | How long to wait (seconds) | Seconds |
| Loop | Restart timer after finishing? | Yes/No |
| Start Immediately | Begin timing when level starts? | Yes/No |

**Wiring Options:**
| Sender | What it means |
|--------|---------------|
| OnTimerFinished | Timer completed its duration |

| Receiver | What it does |
|----------|--------------|
| StartTimer | Begin the countdown |
| StopTimer | Pause/stop the timer |

**Works well with:**
- **NPC Spawner** - Spawn enemies every X seconds
- **Counter Rule** - Track number of timer cycles
- **Door - Mechanical** - Auto-close after delay
- **Lever** - Start/stop timer

**Video Tutorial:** Countdown Timer & Timer Rule: https://youtu.be/1QuOaU2-Qyk

**Example Recipes:**
- See "Wave-Based Enemy Spawner" in Recipe Patterns
- See "Timed Challenge" in Recipe Patterns

---

### Countdown Timer
**Type:** Rule Block | **Difficulty:** ⭐⭐ Intermediate

**What it does:** A visible countdown that can display remaining time.

**When to use it:**
- Race against time challenges
- Bomb defusal timers
- Visible countdowns for players
- Time limits

**Inspector Properties:**
| Property | Description | Values |
|----------|-------------|--------|
| Name | Unique identifier | Text |
| Time Step | Countdown interval (seconds) | Seconds |
| Start Immediately | Begin when level starts? | Yes/No |
| Countdown Value | Starting number | Seconds |

**Wiring Options:**
| Sender | What it means |
|--------|---------------|
| OutputAsText | Sends countdown number as text data |

| Receiver | What it does |
|----------|--------------|
| StartTimer | Begin countdown |
| ResetTimer | Reset to starting value |
| PauseTimer | Pause countdown |

**Works well with:**
- **Anachronist Billboard** - Display the countdown (wire OutputAsText → SetRawText)
- **Level Transition Rule** - End level when time runs out
- **Lever** - Start/pause timer

**Video Tutorials:**
- Countdown Timer & Timer Rule: https://youtu.be/1QuOaU2-Qyk
- Rising Ocean (countdown example): https://youtu.be/fsNwqFtzqsE
- Teleport Point (countdown display): https://youtu.be/cUBOVIw2__o

---

### Counter Rule
**Type:** Rule Block | **Difficulty:** ⭐⭐ Intermediate

**What it does:** Counts events and triggers when a goal number is reached.

**When to use it:**
- "Collect 5 coins to open door"
- "Kill 10 enemies to win"
- "Press 3 buttons in any order"
- Tracking player progress

**Inspector Properties:**
| Property | Description | Values |
|----------|-------------|--------|
| Name | Unique identifier | Text |
| Goal Number | Target count to reach | Whole number |
| Loop and Repeat | Reset to 0 after reaching goal? | Yes/No |

**Wiring Options:**
| Sender | What it means |
|--------|---------------|
| OnCountReached | Counter reached the goal number |

| Receiver | What it does |
|----------|--------------|
| ModifyCount | Add to counter (usually +1) |
| SetGoal | Change the goal number |
| ResetAndClearCounter | Reset count to 0 |

**Works well with:**
- **Treasure/Key/Resource** - OnPickup → ModifyCount
- **Medium NPC** - OnDefeated → ModifyCount
- **Door - Mechanical** - OnCountReached → Open
- **Anachronist Billboard** - Display current count

**Video Tutorial:** Counter Rule: https://youtu.be/Ey2FRDx7eNc

**Example Recipe:** See "Collect X Items to Open Door" in Recipe Patterns

---

## 👾 NPCs & COMBAT

Enemies, allies, and combat systems.

---

### Medium NPC
**Type:** Prop | **Difficulty:** ⭐⭐ Intermediate

**What it does:** A pre-placed NPC character (enemy, ally, or neutral).

**When to use it:**
- Pre-positioned enemies
- Quest NPCs
- Allies/companions
- Shop keepers

**Inspector Properties:**
| Property | Description | Options |
|----------|-------------|---------|
| Name | Unique identifier | Text |
| NPC Name | Displayed name in game | Text |
| Character Visuals | Appearance | Character options |
| NPC Class | Combat type | Blank (no weapon), Grunt (sword), Rifleman (ranged), Zombie |
| Starting Health | HP amount | 1-20 |
| Team | Ally or enemy? | Player's team or others |
| Group | Group for shared behavior | Group name |
| Combat | Aggression | Passive, Defensive, Aggressive |
| Damage | Take damage? | Take Damage, Ignore Damage |
| Physics | Obey physics? | Take Physics, Ignore Physics |
| Idle | Default behavior | Stand, Idle, Wander, Rove |
| NPC Movement Mode | Speed | Walk, Run |
| Pathfinding Range | How far NPC can navigate | Global, Area, Island |

**Wiring Options:**
| Sender | What it means |
|--------|---------------|
| OnEntityHealthZeroed | This NPC's health reached 0 |
| OnTargetHealthZeroed | NPC's target's health reached 0 |
| OnDefeated | This NPC was killed |

| Receiver | What it does |
|----------|--------------|
| AttemptSwitchTarget | Change NPC's target |
| SetDisplayName | Change displayed name |
| SetLocalizedText | Set translated text |
| SetText | Set text |

**Works well with:**
- **Counter Rule** - Track NPC kills
- **Dynamic Item Spawner** - Drop items on death
- **Dialogue Node** - Conversations
- **NPC behavior nodes** - Complex AI

**Video Tutorials:**
- Dynamic Item Spawner: https://youtu.be/60M_UAvr0dI
- Health UI for NPCs: https://youtu.be/wxPeRg1u6gU
- Item Quantity Checker (NPC shop): https://youtu.be/Yi29pfpJg_4

---

### NPC Spawner
**Type:** Rule Block | **Difficulty:** ⭐⭐ Intermediate

**What it does:** Spawns an NPC when triggered (not pre-placed).

**When to use it:**
- Wave-based enemy spawning
- Triggered enemy encounters
- Dynamic ally spawning
- Boss fights

**Inspector Properties:**
| Property | Description | Options |
|----------|-------------|---------|
| Name | Unique identifier | Text |
| Spawn on Start | Spawn when level begins? | Yes/No |
| Position | Where to spawn | Position selector |
| Visuals | NPC appearance | Character options |
| Class | Combat type | Blank, Grunt, Rifleman, Zombie |
| Health | HP amount | 1-100 |
| NPC Team | Ally/enemy | Team selection |
| Group | Behavior group | Group name |
| Range | Navigation range | Global, Area, Island |
| Combat | Aggression | Passive, Defensive, Aggressive |
| Damage | Take damage? | Take Damage, Ignore Damage |
| Physics | Obey physics? | Take Physics, Ignore Physics |
| Idle | Default behavior | Stand, Idle, Wander, Rove |
| NPC Movement Mode | Speed | Walk, Run |

**Wiring Options:**
| Sender | What it means |
|--------|---------------|
| OnNpcSpawned | NPC was successfully spawned |

| Receiver | What it does |
|----------|--------------|
| TrySpawnNpc | Attempt to spawn the NPC |

**Works well with:**
- **Timer Rule** - Spawn enemies every X seconds
- **Trigger Volume** - Spawn when player enters area
- **Counter Rule** - Track spawned/killed NPCs
- **Lever** - Player-controlled spawning

**Video Tutorial:** Countdown Timer & Timer Rule (spawning): https://youtu.be/1QuOaU2-Qyk

---

### Random Multiple NPC Spawner
**Type:** Rule Block | **Difficulty:** ⭐⭐⭐ Advanced

**What it does:** Spawns random NPCs from a pool at defined locations.

**When to use it:**
- Varied enemy encounters
- Surprise enemy spawns
- Emergent gameplay
- Arena modes

**Video Tutorials:**
- Random Multiple NPC Spawner: https://youtu.be/CCKxMd_Q8kU
- NPC Death Toll: https://youtu.be/2D8L8fw35Z0
- Health UI for NPCs: https://youtu.be/wxPeRg1u6gU

---

### Sensor Spike Trap
**Type:** Prop | **Difficulty:** ⭐⭐ Intermediate

**What it does:** A trap that damages entities when triggered by proximity.

**When to use it:**
- Dungeon traps
- Hazard areas
- Defensive mechanisms
- Puzzle obstacles

**Inspector Properties:**
| Property | Description |
|----------|-------------|
| Name | Unique identifier |
| Start Active | Begin active when level starts? |

**Wiring Options:**
| Sender | What it means |
|--------|---------------|
| OnEntityHit | Something was damaged by trap |
| OnTriggered | Trap was activated |

| Receiver | What it does |
|----------|--------------|
| Activate | Turn trap on |
| Deactivate | Turn trap off |
| ToggleActive | Switch on/off state |

**Works well with:**
- **Lever** - Player control of traps
- **Pressure Plate** - Step-triggered activation
- **Timer Rule** - Timed activation cycles

---

### Mechanical Spike Trap
**Type:** Prop | **Difficulty:** ⭐⭐ Intermediate

**What it does:** A manually triggered spike trap (doesn't auto-sense).

**When to use it:**
- Script-controlled traps
- Puzzle traps
- Timed hazards

**Inspector Properties:**
| Property | Description |
|----------|-------------|
| Name | Unique identifier |

**Wiring Options:**
| Sender | What it means |
|--------|---------------|
| OnEntityHit | Something was damaged |
| OnTriggered | Trap was triggered |

| Receiver | What it does |
|----------|--------------|
| Trigger | Activate the trap |

---

### Sensor Arrow Trap
**Type:** Prop | **Difficulty:** ⭐⭐ Intermediate

**What it does:** Fires arrows when it detects entities.

**When to use it:**
- Ranged trap hazards
- Dungeon corridors
- Defensive systems

**Inspector Properties:**
| Property | Description | Values |
|----------|-------------|--------|
| Name | Unique identifier | Text |
| Set Active | Start active? | Yes/No |
| Fire Delay | Cooldown between shots | 0.25-5 seconds |

**Wiring Options:**
| Sender | What it means |
|--------|---------------|
| OnFired | Arrow was shot |
| OnFiringStarted | Trap began firing |
| OnFiringStopped | Trap stopped firing |

| Receiver | What it does |
|----------|--------------|
| Activate | Turn on |
| Deactivate | Turn off |
| ToggleActive | Switch state |

---

### Sentry Turret
**Type:** Prop | **Difficulty:** ⭐⭐ Intermediate

**What it does:** An automated turret that shoots enemies.

**When to use it:**
- Defensive positions
- Allied fire support
- Enemy hazards

**Inspector Properties:**
| Property | Description |
|----------|-------------|
| Name | Unique identifier |
| Team | Whose side is it on? |

**Note:** Sentry Turret has no wiring options - it operates automatically based on team settings.

---

### Damager Block
**Type:** Rule Block | **Difficulty:** ⭐⭐ Intermediate

**What it does:** Deals damage to a specified target when triggered.

**When to use it:**
- Custom damage traps
- Scripted damage events
- Invisible hazards

**Inspector Properties:**
| Property | Description |
|----------|-------------|
| Name | Unique identifier |
| Target Object | What to damage |

**Wiring Options:**
| Receiver | What it does |
|----------|--------------|
| DamageTarget | Deal damage to target |

**Works well with:**
- **Trigger Volume** - Damage when entering area
- **Timer Rule** - Periodic damage
- **Lever** - Controlled damage

---

### Health Modifier
**Type:** Rule Block | **Difficulty:** ⭐⭐ Intermediate

**What it does:** Modifies health of connected character (damage or heal).

**When to use it:**
- Damage zones
- Healing areas
- Custom death mechanics
- Fake respawn systems

**Video Tutorial:** Teleporter & Health Utilities & Health Modifier: https://youtu.be/CFBrBYaVHrs

---

### Health Utilities
**Type:** Rule Block | **Difficulty:** ⭐⭐ Intermediate

**What it does:** Restores health to connected character.

**When to use it:**
- Healing zones
- Checkpoint healing
- Triggered healing

**Video Tutorial:** Teleporter & Health Utilities & Health Modifier: https://youtu.be/CFBrBYaVHrs

---

### Dynamic Item Spawner
**Type:** Rule Block | **Difficulty:** ⭐⭐ Intermediate

**What it does:** Spawns items at a location when triggered (like enemy death drops).

**When to use it:**
- Loot drops from enemies
- Reward spawning
- Key drops from bosses

**Video Tutorial:** Dynamic Item Spawner: https://youtu.be/60M_UAvr0dI

**Works well with:**
- **Medium NPC** - OnDefeated → spawn item
- **Randomizer Rule** - Random loot type

---

### Item Spawner
**Type:** Rule Block | **Difficulty:** ⭐⭐ Intermediate

**What it does:** Spawns a specific item when triggered.

**When to use it:**
- Shop item giving
- Reward distribution
- Puzzle rewards

**Video Tutorial:** Item Quantity Checker & Item Spawner: https://youtu.be/Yi29pfpJg_4

---

### Item Quantity Checker
**Type:** Rule Block | **Difficulty:** ⭐⭐⭐ Advanced

**What it does:** Checks if player has enough of a specific item.

**When to use it:**
- Shop requirements
- Crafting checks
- Gate conditions

**Video Tutorial:** Item Quantity Checker & Item Spawner: https://youtu.be/Yi29pfpJg_4

---

### NPC Death Toll
**Type:** Rule Block | **Difficulty:** ⭐⭐⭐ Advanced

**What it does:** Tracks total NPCs, deaths, and survivors automatically.

**Video Tutorial:** NPC Death Toll: https://youtu.be/2D8L8fw35Z0

---

### Health UI for NPCs
**Type:** Rule Block | **Difficulty:** ⭐⭐ Intermediate

**What it does:** Shows health bars above NPCs when they take damage.

**Video Tutorial:** Health UI for NPCs: https://youtu.be/wxPeRg1u6gU

---

## 🗣️ NPC BEHAVIOR NODES

Advanced NPC control (requires NPC Reference).

---

### Dialogue Node
**Type:** Rule Block (NPC-specific) | **Difficulty:** ⭐⭐ Intermediate

**What it does:** Creates conversations with NPCs.

**When to use it:**
- NPC dialogues
- Quest giving
- Story elements
- Shop interactions

**Inspector Properties:**
| Property | Description |
|----------|-------------|
| Name | Unique identifier |
| On Start | Active when level starts? |
| NPC Reference | Which NPC talks |
| Text | Dialogue text |
| Only Once | One-time conversation? |
| NPC Behavior Overrides | Change NPC behavior during dialogue |

**Wiring Options:**
| Sender | What it means |
|--------|---------------|
| OnFinishedTalking | Dialogue completed |
| OnInteractionCompleted | Player finished interaction |
| OnInteractionCanceled | Player left early |
| OnInteractionFinished | Any interaction end |

| Receiver | What it does |
|----------|--------------|
| GiveInstruction | Start this dialogue |
| RescindInstruction | Cancel this instruction |

**Video Tutorial:** Dialogue Node: https://youtu.be/0vbVkD5qLms

---

### Move To Position Node
**Type:** Rule Block (NPC-specific) | **Difficulty:** ⭐⭐⭐ Advanced

**What it does:** Commands NPC to move to a specific location.

**Inspector Properties:**
| Property | Description |
|----------|-------------|
| Name | Unique identifier |
| NPC Reference | Which NPC to command |
| Position | Destination |
| Destination Tolerance | How close is "arrived" |
| NPC Behavior Overrides | Behavior during movement |

**Wiring Options:**
| Sender | What it means |
|--------|---------------|
| OnCommandSucceeded | NPC reached destination |
| OnCommandFailed | NPC couldn't reach |
| OnCommandFinished | Movement ended (either way) |

| Receiver | What it does |
|----------|--------------|
| GiveInstruction | Start movement |
| RescindInstruction | Cancel movement |

---

### Idle Node / Stand Node / Wander Node / Rove Node
**Type:** Rule Block (NPC-specific) | **Difficulty:** ⭐⭐⭐ Advanced

**What they do:** Set NPC idle behaviors.

| Node | Behavior |
|------|----------|
| Idle Node | NPC does idle animations in place |
| Stand Node | NPC stands still |
| Wander Node | NPC wanders within max distance |
| Rove Node | NPC roams around |

All have similar properties:
- NPC Reference
- NPC Behavior Overrides
- (Wander/Rove have max distance settings)

**Wiring:** All receive GiveInstruction and RescindInstruction

---

## 🌍 ENVIRONMENT & EFFECTS

World settings, visual effects, and environmental props.

---

### Ambient Settings
**Type:** Rule Block | **Difficulty:** ⭐⭐ Intermediate

**What it does:** Controls skybox and atmosphere settings.

**When to use it:**
- Day/night transitions
- Weather changes
- Mood shifts

**Inspector Properties:**
| Property | Description |
|----------|-------------|
| Name | Unique identifier |
| Is Default | Is this the starting setting? |
| Current Skybox | Which skybox to use |

**Wiring Options:**
| Receiver | What it does |
|----------|--------------|
| ChangeSkybox | Switch to this ambient setting |

**Works well with:**
- **Trigger Volume** - Change atmosphere when entering area
- **Lever** - Player-controlled atmosphere
- **Timer Rule** - Timed atmosphere changes

---

### Filters
**Type:** Rule Block | **Difficulty:** ⭐⭐ Intermediate

**What it does:** Applies visual filters/effects to the screen.

**When to use it:**
- Damage effects (red flash)
- Underwater effects
- Dream sequences
- Status effects

**Inspector Properties:**
| Property | Description |
|----------|-------------|
| Name | Unique identifier |
| Visible in Creator | Show in editor? |
| Transition Time | Fade duration (seconds) |

**Wiring Options:**
| Receiver | What it does |
|----------|--------------|
| SetFilterType | Change to a filter |
| SetTransitionTime | Change fade speed |
| SetFilterTypeForPlayer | Apply filter to player only |

---

### Ocean Plane
**Type:** Rule Block | **Difficulty:** ⭐⭐⭐ Advanced

**What it does:** A water plane that can rise, fall, and detect levels.

**When to use it:**
- Rising water puzzles
- Flooding mechanics
- Water level changes

**Inspector Properties:**
| Property | Description | Values |
|----------|-------------|--------|
| Name | Unique identifier | Text |
| Plane Type | Empty or Ocean | Empty, Ocean |
| Move Speed | How fast it moves | 0.1-10 |

**Wiring Options:**
| Sender | What it means |
|--------|---------------|
| OnWaterLevelReached | Water reached set level |

| Receiver | What it does |
|----------|--------------|
| ModifyHeight | Change height by amount |
| SetHeight | Set specific height |
| ModifyMoveSpeed | Change speed |
| SetMoveSpeed | Set specific speed |

**Video Tutorial:** Rising Ocean: https://youtu.be/fsNwqFtzqsE

---

### Torch on the Wall
**Type:** Prop | **Difficulty:** ⭐ Beginner

**What it does:** A light-emitting wall torch.

**Video Tutorial:** Torch on the Wall: https://youtu.be/01n5brVV2Xw

---

### Glass / Stained Glass
**Type:** Prop | **Difficulty:** ⭐ Beginner

**What they do:** Transparent/decorative glass props.

| Prop | Description |
|------|-------------|
| Glass | Transparent cube |
| Glass 1X1, 1X2, 2X1, 2X2 | Stained glass in sizes |

**Video Tutorials:**
- Glass Prop: https://youtu.be/qjW9nXoQgwo
- Stained Glass: https://youtu.be/n7TBpTJcrg8

---

### Bridge / Opening Bridge / Gate
**Type:** Prop | **Difficulty:** ⭐⭐ Intermediate

**What they do:** Bridge and gate structures.

| Prop | Description |
|------|-------------|
| Bridge | Static stone bridge |
| Opening Bridge | Tower Bridge-style opening |
| Gate | Medieval lifting gate |

**Video Tutorial:** Bridges: https://youtu.be/buj58tI54xI

---

### Physics Cube
**Type:** Prop | **Difficulty:** ⭐ Beginner

**What it does:** A cube affected by physics (can be pushed, falls).

**When to use it:**
- Physics puzzles
- Pushable objects
- Weight mechanics

**Video Tutorial:** Rising Ocean (physics cube example): https://youtu.be/fsNwqFtzqsE

---

## 🚀 MOVEMENT & TELEPORTATION

Moving players and NPCs around.

---

### Portal
**Type:** Prop | **Difficulty:** ⭐⭐ Intermediate

**What it does:** Teleports players/NPCs to destination points within same level.

**When to use it:**
- Fast travel points
- Shortcuts
- Puzzle teleportation

**Inspector Properties:**
| Property | Description |
|----------|-------------|
| Name | Unique identifier |
| Destination Points | Where to teleport to (one or more) |
| Destination Offset | Randomization around destination |
| Cooldown | Seconds before can use again |

**Wiring Options:**
| Receiver | What it does |
|----------|--------------|
| AttemptSwitchTarget | Change destination |

**Video Tutorial:** Portal: https://youtu.be/QwbOPz89h6g

---

### Teleporter
**Type:** Rule Block | **Difficulty:** ⭐⭐ Intermediate

**What it does:** Rule block that teleports a target to a destination.

**When to use it:**
- Scripted teleportation
- Triggered warps
- Trap teleporters

**Inspector Properties:**
| Property | Description |
|----------|-------------|
| Name | Unique identifier |
| Teleport Target | Who to teleport |
| Destination | Where to send them |
| Teleport Effect | Visual style (Materialize or Instant) |

**Wiring Options:**
| Sender | What it means |
|--------|---------------|
| OnTeleportSuccess | Teleport worked |
| OnTeleportFailure | Teleport failed |

| Receiver | What it does |
|----------|--------------|
| TriggerTeleport | Execute teleport |

**Video Tutorial:** Teleporter & Health Utilities: https://youtu.be/CFBrBYaVHrs

---

### Teleport Point
**Type:** Prop | **Difficulty:** ⭐⭐ Intermediate

**What it does:** A teleporter with visual countdown display and range detection.

**When to use it:**
- Star Trek-style beam pads
- Visible teleport areas
- Range-based teleportation

**Video Tutorial:** Teleport Point: https://youtu.be/cUBOVIw2__o

---

### Anachronist Bounce Pad
**Type:** Prop | **Difficulty:** ⭐⭐ Intermediate

**What it does:** Launches players/entities into the air.

**When to use it:**
- Platforming mechanics
- Reaching high areas
- Fun movement

**Inspector Properties:**
| Property | Description | Values |
|----------|-------------|--------|
| Name | Unique identifier | Text |
| Start Active | Active when level starts? | Yes/No |
| Starting Bounce Height | Launch power | 1-35 |

**Wiring Options:**
| Sender | What it means |
|--------|---------------|
| OnEntityBounced | Something bounced |

| Receiver | What it does |
|----------|--------------|
| Activate | Turn on |
| Deactivate | Turn off |
| ToggleActive | Switch state |
| SetBounceHeight | Set exact height |
| ModifyBounceHeight | Change height by amount |

**Video Tutorial:** Rising Ocean (bounce pad example): https://youtu.be/fsNwqFtzqsE

---

### Level Transition Rule
**Type:** Rule Block | **Difficulty:** ⭐⭐ Intermediate

**What it does:** Changes to a different level when triggered.

**When to use it:**
- Win conditions
- Fail conditions
- Level progression

**Inspector Properties:**
| Property | Description |
|----------|-------------|
| Name | Unique identifier |
| Level | Destination level |
| Target Spawn Points | Where to spawn in new level |

**Wiring Options:**
| Receiver | What it does |
|----------|--------------|
| ChangeLevel | Execute level transition |

**Works well with:**
- **Counter Rule** - Win after collecting all items
- **Countdown Timer** - Fail when time runs out
- **Trigger Volume** - Enter area to win

---

## 📺 UI & FEEDBACK

Display information to players.

---

### Anachronist Billboard
**Type:** Prop | **Difficulty:** ⭐⭐ Intermediate

**What it does:** A display board that shows text to players.

**When to use it:**
- Score displays
- Countdown displays
- Instructions
- Messages

**Inspector Properties:**
| Property | Description |
|----------|-------------|
| Name | Unique identifier |
| Text | Initial displayed text |

**Wiring Options:**
| Receiver | What it does |
|----------|--------------|
| SetText | Display specific text |
| SetRawText | Display data from sender (auto or manual override) |

**Works well with:**
- **Countdown Timer** - OutputAsText → SetRawText shows countdown
- **Counter Rule** - Display count
- **Timer Rule** - Display remaining time

**Video Tutorials:**
- NPC Death Toll (billboard display): https://youtu.be/2D8L8fw35Z0
- Banner race (billboard): https://youtu.be/Wo-IXmCvQzg
- Countdown Timer: https://youtu.be/1QuOaU2-Qyk

---

### Anachronist Floor Sign
**Type:** Prop | **Difficulty:** ⭐⭐ Intermediate

**What it does:** A sign players can read with sequential text pages.

**When to use it:**
- Tutorial signs
- Lore/story
- Instructions
- Multi-page info

**Inspector Properties:**
| Property | Description |
|----------|-------------|
| Name | Unique identifier |
| Text | Text pages (shown sequentially) |
| Sign Name | Displayed name of sign |

**Wiring Options:**
| Sender | What it means |
|--------|---------------|
| OnFinishedReading | Player read all pages |

| Receiver | What it does |
|----------|--------------|
| Generated | - |
| SetDisplayName | Change sign name |
| SetLocalizedText | Set translated text |
| SetText | Change text content |

---

### Banner
**Type:** Prop | **Difficulty:** ⭐⭐ Intermediate

**What it does:** A flag/banner that detects when players reach it (includes teleport).

**When to use it:**
- Race finish lines
- Capture points
- Goal markers

**Video Tutorial:** Banner - First to Flag Wins: https://youtu.be/Wo-IXmCvQzg

---

## 🎮 LEVEL & SPAWN MANAGEMENT

Control game flow and player spawning.

---

### Basic Spawn Point
**Type:** Prop | **Difficulty:** ⭐ Beginner

**What it does:** Where players spawn/respawn in the level.

**When to use it:**
- Initial player spawn
- Respawn points
- Checkpoint spawns

**Inspector Properties:**
| Property | Description | Values |
|----------|-------------|--------|
| Name | Unique identifier | Text |
| Starting Health | Player health on spawn | 1-20 |
| Inventory Size | Inventory slots | 1-10 |

**Wiring Options:**
| Sender | What it means |
|--------|---------------|
| OnPlayerSpawned | Player spawned here |
| OnLevelEntered | Level loaded with this spawn |

---

### Checkpoint System
**Type:** Rule Block | **Difficulty:** ⭐⭐ Intermediate

**What it does:** Automatically respawns player at closest spawn point when they die.

**Video Tutorial:** Checkpoint System: https://youtu.be/ob46I22Qjs8

---

---

## PART 4: RECIPE PATTERNS

Common game mechanics with step-by-step instructions.

---

### Recipe: Key and Locked Door
**Difficulty:** ⭐ Beginner
**Props needed:** Key, Door - Locked

**Goal:** Player must find key to open door.

**Steps:**
1. Place a **Door - Locked** where you want the barrier
2. Place a **Key** somewhere in the level
3. Select the Door and in Inspector, set **Key Reference** to your Key
4. Done! Door automatically unlocks when player has the key

**No wiring required** - the key reference handles it automatically.

---

### Recipe: Collect X Items to Open Door
**Difficulty:** ⭐⭐ Intermediate
**Props needed:** Treasure (x3 or more), Counter Rule, Door - Mechanical

**Goal:** Door opens after collecting 3 treasures.

**Steps:**
1. Place 3 (or more) **Treasure** props in your level
2. Place a **Counter Rule** - set Goal Number = 3
3. Place a **Door - Mechanical** where you want the barrier
4. Wire each Treasure:
   - Select Treasure first (sender)
   - Select Counter Rule second (receiver)
   - Connect **OnPickup** → **ModifyCount**
   - Confirm
5. Wire Counter to Door:
   - Select Counter Rule first (sender)
   - Select Door second (receiver)
   - Connect **OnCountReached** → **Open**
   - Confirm

**Result:** Door opens when 3rd treasure is collected!

**Video Reference:** Counter Rule: https://youtu.be/Ey2FRDx7eNc

---

### Recipe: Lever-Controlled Door
**Difficulty:** ⭐ Beginner
**Props needed:** Lever, Door - Mechanical

**Goal:** Flip lever to open/close door.

**Steps:**
1. Place a **Lever** near the door location
2. Place a **Door - Mechanical**
3. Wire:
   - Select Lever first (sender)
   - Select Door second (receiver)
   - Connect **OnFlipped** → **ToggleOpen**
   - Confirm

**Result:** Each lever flip toggles the door!

---

### Recipe: Pressure Plate Door (Hold to Keep Open)
**Difficulty:** ⭐ Beginner
**Props needed:** Pressure Plate, Door - Mechanical

**Goal:** Door opens when standing on plate, closes when stepping off.

**Steps:**
1. Place **Pressure Plate** where players will step
2. Place **Door - Mechanical**
3. Wire for opening:
   - Pressure Plate → Door
   - **OnPressed** → **Open**
4. Wire for closing:
   - Pressure Plate → Door
   - **OnReleased** → **Close**

---

### Recipe: Wave-Based Enemy Spawner
**Difficulty:** ⭐⭐ Intermediate
**Props needed:** Timer Rule, NPC Spawner (multiple)

**Goal:** Enemies spawn every 10 seconds.

**Steps:**
1. Place **Timer Rule** - set Duration = 10, Loop = Yes, Start Immediately = Yes
2. Place multiple **NPC Spawner** rule blocks with different positions
3. Wire Timer to each spawner:
   - Timer Rule → NPC Spawner
   - **OnTimerFinished** → **TrySpawnNpc**
4. Repeat for each spawner

**Result:** Every 10 seconds, NPCs spawn at all connected spawners!

**Enhancement:** Use **Sequence Rule** instead to spawn different enemy types each wave.

---

### Recipe: Kill All Enemies to Proceed
**Difficulty:** ⭐⭐ Intermediate
**Props needed:** Medium NPC (multiple) OR NPC Spawner, Counter Rule, Door - Mechanical

**Goal:** Door opens after killing all enemies.

**Steps:**
1. Place your **Medium NPCs** (e.g., 5 enemies)
2. Place **Counter Rule** - set Goal Number = 5 (number of enemies)
3. Place **Door - Mechanical**
4. Wire each NPC to counter:
   - Medium NPC → Counter Rule
   - **OnDefeated** → **ModifyCount**
5. Wire counter to door:
   - Counter Rule → Door
   - **OnCountReached** → **Open**

---

### Recipe: Timed Challenge with Countdown Display
**Difficulty:** ⭐⭐ Intermediate
**Props needed:** Countdown Timer, Anachronist Billboard, Level Transition Rule (x2)

**Goal:** Player has 60 seconds to reach goal or fail.

**Steps:**
1. Place **Countdown Timer** - set Countdown Value = 60, Start Immediately = Yes
2. Place **Anachronist Billboard** to display time
3. Wire countdown to billboard:
   - Countdown Timer → Billboard
   - **OutputAsText** → **SetRawText**
4. Place **Level Transition Rule** for failure (links to fail/retry level)
5. Wire (you'll need additional logic for this - see "Bool Gate" for conditions)

**For win condition:** Use **Trigger Volume** at goal location → Level Transition Rule

---

### Recipe: NPC Shop System
**Difficulty:** ⭐⭐⭐ Advanced
**Props needed:** Medium NPC, Dialogue Node, Item Quantity Checker, Item Spawner, Do Once Rule, Trigger Volume

**Goal:** Player talks to NPC, if they have enough coins, they get an item.

**Video Tutorial:** Item Quantity Checker & Item Spawner: https://youtu.be/Yi29pfpJg_4

**Steps:**
1. Place **Medium NPC** (the shopkeeper)
2. Place **Trigger Volume** around NPC
3. Place **Dialogue Node** - set NPC Reference, Text = "Want to buy a sword? (2 coins)"
4. Place **Item Quantity Checker** - set required item and quantity
5. Place **Item Spawner** - set item to give
6. Place **Do Once Rule** (prevents buying multiple times per interaction)
7. Wire:
   - Trigger Volume → Dialogue Node (OnTriggered → GiveInstruction)
   - Dialogue Node → Item Quantity Checker (OnInteractionCompleted → check)
   - Item Quantity Checker (if has enough) → Item Spawner
   - Use Do Once to prevent spam

---

### Recipe: Fake Respawn (Teleport + Heal Instead of Death)
**Difficulty:** ⭐⭐⭐ Advanced
**Props needed:** Health Modifier, Health Utilities, Teleporter, Trigger Volume

**Goal:** When player would die, teleport them and heal instead.

**Video Tutorial:** Teleporter & Health Utilities & Health Modifier: https://youtu.be/CFBrBYaVHrs

---

### Recipe: Rising Water Escape
**Difficulty:** ⭐⭐⭐ Advanced
**Props needed:** Ocean Plane, Timer Rule, Countdown Timer, Anachronist Billboard, Physics Cube, Lever, Door

**Goal:** Water rises over time. Player must solve puzzle before drowning.

**Video Tutorial:** Rising Ocean: https://youtu.be/fsNwqFtzqsE

---

### Recipe: Random Enemy Types
**Difficulty:** ⭐⭐⭐ Advanced
**Props needed:** Randomizer Rule, NPC Spawner (multiple with different enemy types)

**Goal:** Random enemy spawns from pool of possibilities.

**Steps:**
1. Place **Randomizer Rule** - set Max Result = 3 (for 3 enemy types)
2. Place 3 **NPC Spawner** rule blocks with different enemy types
3. Wire randomizer to spawners:
   - Randomizer → Spawner 1: **OnResult1** → **TrySpawnNpc**
   - Randomizer → Spawner 2: **OnResult2** → **TrySpawnNpc**
   - Randomizer → Spawner 3: **OnResult3** → **TrySpawnNpc**
4. Wire trigger to randomizer:
   - (Timer Rule or Trigger Volume) → Randomizer: **FireRandomEvent**

**Video Tutorial:** Random Multiple NPC Spawner: https://youtu.be/CCKxMd_Q8kU

---

### Recipe: First to Flag Race
**Difficulty:** ⭐⭐ Intermediate
**Props needed:** Banner, Timer Rule, Anachronist Billboard

**Goal:** Race to reach the banner first.

**Video Tutorial:** Banner - First to Flag Wins: https://youtu.be/Wo-IXmCvQzg

---

## PART 5: TROUBLESHOOTING

### Common Problems and Solutions

---

**Problem:** Door won't open even though I wired it
**Solutions:**
- Check sender/receiver order - did you click the RIGHT one first?
- Did you click "Confirm" after making the wire?
- Check the wire options - did you select the correct action?
- Is the door's "Locked" property set to Yes? Use a Key Reference or wire Unlock first

---

**Problem:** Counter doesn't seem to be counting
**Solutions:**
- Verify Goal Number is set correctly
- Check that you wired to **ModifyCount**, not something else
- Make sure each item is wired to the counter (each needs its own wire)
- Check if "Loop and Repeat" is resetting your count unexpectedly

---

**Problem:** Timer won't start
**Solutions:**
- Check "Start Immediately" setting
- If Start Immediately is No, you need to wire something to **StartTimer**
- Verify Duration is set (not 0)

---

**Problem:** NPC is stuck and won't move
**Solutions:**
- Check **Pathfinding Range** - try "Global" for largest range
- Make sure path isn't blocked by obstacles
- Check **Idle** setting - is it set to "Stand"?
- Verify there's a valid path between NPC and destination

---

**Problem:** Trigger Volume isn't detecting player
**Solutions:**
- Check volume dimensions (Forward, Backward, Left, Right, Up, Down)
- Use Offset to position the volume correctly
- Check "Only Once" - it may have already triggered
- Make sure volume is large enough (try 5-10 units each direction)

---

**Problem:** Wiring option I need isn't showing
**Solutions:**
- Make sure you selected props in correct order (sender first, receiver second)
- Some props only have Sender options, some only Receiver
- Check if you're looking at the right side of the screen (Sender=left, Receiver=right)

---

**Problem:** Multiple things happening when I only want one
**Solutions:**
- Use **Do Once Rule** to limit to single trigger
- Check if you have duplicate wires
- Check "Only Once" property on Trigger Volumes

---

**Problem:** Sequence/Randomizer not working
**Solutions:**
- Verify Max Sequence/Max Result is set correctly
- Make sure you wired **FireNextEvent** or **FireRandomEvent** from something
- Check that OnResult1, OnResult2, etc. are wired to different things

---

**Problem:** NPC conversation won't start
**Solutions:**
- Check **NPC Reference** is set
- Verify player can reach the interaction point
- Check "On Start" and "Only Once" settings
- Make sure Trigger Volume or Interactable Volume is properly wired

---

## QUICK REFERENCE: Wiring Cheat Sheet

### Most Common Connections

| When this happens... | Wire this... | To make this happen |
|---------------------|--------------|---------------------|
| Player picks up item | Treasure/Key/Resource **OnPickup** | Counter Rule **ModifyCount** |
| Counter reaches goal | Counter Rule **OnCountReached** | Door **Open** |
| Player flips lever | Lever **OnFlipped** | Door **ToggleOpen** |
| Player steps on plate | Pressure Plate **OnPressed** | Door **Open** |
| Timer finishes | Timer Rule **OnTimerFinished** | NPC Spawner **TrySpawnNpc** |
| Player enters area | Trigger Volume **OnTriggered** | Dialogue Node **GiveInstruction** |
| Enemy dies | Medium NPC **OnDefeated** | Counter Rule **ModifyCount** |
| Countdown ticks | Countdown Timer **OutputAsText** | Billboard **SetRawText** |
| Level starts | Level Start Rule **OnLevelStart** | Timer Rule **StartTimer** |

---

## VIDEO TUTORIAL INDEX

| Topic | Video Link |
|-------|------------|
| Counter Rule basics | https://youtu.be/Ey2FRDx7eNc |
| Timer & Countdown | https://youtu.be/1QuOaU2-Qyk |
| Dialogue Node | https://youtu.be/0vbVkD5qLms |
| NPC Death Toll | https://youtu.be/2D8L8fw35Z0 |
| Bridges & Gates | https://youtu.be/buj58tI54xI |
| Portal | https://youtu.be/QwbOPz89h6g |
| Teleport Point | https://youtu.be/cUBOVIw2__o |
| Rising Ocean puzzle | https://youtu.be/fsNwqFtzqsE |
| Checkpoint System | https://youtu.be/ob46I22Qjs8 |
| Banner/Flag race | https://youtu.be/Wo-IXmCvQzg |
| Dynamic Item Spawner | https://youtu.be/60M_UAvr0dI |
| Health UI for NPCs | https://youtu.be/wxPeRg1u6gU |
| Item Quantity Checker (Shop) | https://youtu.be/Yi29pfpJg_4 |
| Teleporter & Health | https://youtu.be/CFBrBYaVHrs |
| Lever trap (kill character) | https://youtu.be/4QoLIxX50XQ |
| Random NPC Spawner | https://youtu.be/CCKxMd_Q8kU |
| Torch on Wall | https://youtu.be/01n5brVV2Xw |
| Glass Prop | https://youtu.be/qjW9nXoQgwo |
| Stained Glass | https://youtu.be/n7TBpTJcrg8 |
| Publishing & Deeplinking | https://youtu.be/FeFfRMLOOTs |

---

*Document Version: 2.0 - Restructured for hands-on student guidance*
*Last Updated: December 2025*
