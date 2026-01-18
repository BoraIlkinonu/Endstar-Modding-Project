# Endstar Props & Rule Blocks - Quick Reference

This document contains information about all Endstar props, rule blocks, wiring, recipes, and troubleshooting.

---

## What is the difference between Props and Rule Blocks in Endstar?

In Endstar, Props and Rule Blocks serve different purposes:

**Props:**
- Visible objects in your game
- Players can see and interact with them
- Have physical presence in the world
- Examples: Doors, Keys, Treasure, NPCs, Levers, Weapons

**Rule Blocks:**
- Invisible logic controllers
- Work behind the scenes
- Define behaviors and rules
- Examples: Timer Rule, Counter Rule, Trigger Volume, Sequence Rule

Think of it this way: Props are the *actors* in your game, while Rule Blocks are the *directors* telling them what to do.

---

## How do I wire props and rule blocks together in Endstar?

Here's how to wire props and rule blocks in Endstar:

**Step-by-step:**
1. Press **Key 5** on your keyboard to see wiring options
2. **Click first** on the prop/rule block you want as **Sender** (appears on LEFT side)
3. **Click second** on the prop/rule block you want as **Receiver** (appears on RIGHT side)
4. Click and drag from Sender's connector point to Receiver's connector point (or single-click each)
5. A **colored wire** appears connecting them
6. Choose options in the **pop-up menu** that appears
7. **Click 'Confirm'** to finalize - this is important, don't skip it!

**Key concepts:**
- **Sender** = 'When THIS happens...' (the trigger)
- **Receiver** = '...do THAT' (the action)
- Example: When player picks up a Key (Sender: OnPickup) → Open the Door (Receiver: Open)

---

## How do I delete a wire connection in Endstar?

To delete a wire connection in Endstar:
1. Click on the wire/line between the two connected props or rule blocks
2. A pop-up will appear
3. Click the **'Delete' button** in the pop-up

The connection will be removed and the props/rule blocks will no longer communicate through that wire.

---

## What does 'On' mean in Endstar wiring options like OnPickup or OnTimerFinished?

When you see wiring options starting with **'On...'** in Endstar (like OnPickup, OnTimerFinished, OnTriggered), the 'On' simply means **'When'**.

Examples:
- **OnPickup** = 'When picked up'
- **OnTimerFinished** = 'When timer finishes'
- **OnTriggered** = 'When triggered'
- **OnDoorOpened** = 'When door opens'
- **OnDefeated** = 'When defeated'

This is an old programming convention - nothing special, just a way of naming events!

---

## What is a Treasure prop in Endstar?

**Treasure** is a collectible prop in Endstar that players can pick up.

**When to use it:**
- Creating collectible coins or gems
- Reward items scattered in levels
- Objectives like 'collect all treasures'

**Inspector Properties:**
- Name: Unique identifier for this treasure

**Wiring Options (Sender):**
- **OnPickup**: Triggers when player collects this treasure

**Works well with:**
- **Counter Rule** - Track how many treasures collected
- **Door - Mechanical** - Open door after collecting
- **Anachronist Billboard** - Display collection count
- **Do Once Rule** - Ensure pickup only triggers once

---

## What is a Key prop in Endstar?

**Key** is a special collectible prop in Endstar that can unlock locked doors.

**When to use it:**
- Classic key-and-lock puzzles
- Gating progress until player explores
- Multi-key door puzzles

**Inspector Properties:**
- Name: Unique identifier for this key

**Wiring Options (Sender):**
- **OnPickup**: Triggers when player picks up the key

**Works well with:**
- **Door - Locked** - Assign this key to unlock specific door
- **Counter Rule** - Track multiple keys collected
- **Anachronist Billboard** - Show 'Key collected!' message

---

## What is a Resource prop in Endstar?

**Resource** is a collectible prop in Endstar with a quantity value.

**When to use it:**
- Currency (coins, gold)
- Crafting materials
- Score pickups

**Inspector Properties:**
- Name: Unique identifier
- Quantity: How many resources this gives (1-10)

**Wiring Options (Sender):**
- **OnCollected**: Triggers when player collects this resource

**Works well with:**
- **Item Quantity Checker** - Check if player has enough
- **Counter Rule** - Track total collected
- **Anachronist Billboard** - Display resource count

---

## What is an Instant Health Pickup in Endstar?

**Instant Health Pickup** is a prop that immediately restores player health when collected.

**When to use it:**
- Health restoration in combat areas
- Rewards after defeating enemies
- Hidden health bonuses

**Inspector Properties:**
- Name: Unique identifier

**Wiring Options (Sender):**
- **OnCollected**: Triggers when player picks up the health

**Works well with:**
- **Dynamic Item Spawner** - Drop health when enemy dies
- **Timer Rule** - Respawn health pickup after delay

---

## What is a Healing Injector in Endstar?

**Healing Injector** is a stackable healing item that players carry in their inventory.

**When to use it:**
- Healing items player saves for later
- Limited healing resources (survival games)
- Inventory-based health system

**Inspector Properties:**
- Name: Unique identifier
- Starting Stack Count: How many uses (1-10)

**Wiring Options (Sender):**
- **OnPickup**: Triggers when player picks up the injector

---

## What is a Healing Fountain in Endstar?

**Healing Fountain** is a stationary healing source that activates periodically.

**When to use it:**
- Safe zones where players can heal
- Checkpoints with healing
- Strategic healing locations in combat areas

**Inspector Properties:**
- Name: Unique identifier
- Initial Interval: Seconds before first activation
- Interval Scalar: Seconds between each heal after first
- Healing Amount: Health restored per activation

**Note:** This prop has no wiring options - it works automatically based on its settings.

---

## What is a Thrown Bomb in Endstar?

**Thrown Bomb** is an explosive weapon prop that players can pick up and throw.

**When to use it:**
- Combat weapon pickup
- Puzzle element (destroy walls)
- Limited-use powerful attack

**Inspector Properties:**
- Name: Unique identifier
- Starting Stack Count: How many bombs (1-10)

**Wiring Options (Sender):**
- **OnPickup**: Triggers when player picks up bombs

---

## What weapons are available in Endstar?

Endstar has several weapon pickup props:

| Weapon | Description |
|--------|-------------|
| **One Handed Sword** | Fast melee weapon, can use shield |
| **Two Handed Sword Pickup** | Powerful but slower melee |
| **1H Range** | Single-handed ranged weapon |
| **2H Ranged** | Two-handed ranged weapon |

**All weapons have:**
- Inspector Property: Name (unique identifier)
- Wiring Option (Sender): OnPickup - triggers when player picks up weapon

**Work well with:**
- **Dynamic Item Spawner** - Drop weapons from enemies
- **Trigger Volume** - Entering area grants weapon

---

## What is a Dash Pack in Endstar?

**Dash Pack** is a prop that gives the player a dash/dodge ability.

**When to use it:**
- Movement upgrade pickup
- Combat mobility enhancement
- Exploration reward

**Inspector Properties:**
- Name: Unique identifier

**Wiring Options (Sender):**
- **OnPickup**: Triggers when player picks up dash pack

---

## What is a Jetpack in Endstar?

**Jetpack** is a prop that gives the player flight/hover ability.

**When to use it:**
- Vertical exploration
- Movement upgrade
- Accessing high areas

**Inspector Properties:**
- Name: Unique identifier

**Wiring Options (Sender):**
- **OnPickup**: Triggers when player picks up jetpack

---

## What is a Door - Locked in Endstar?

**Door - Locked** is a door prop that requires a specific key to open.

**When to use it:**
- Classic key-and-lock puzzles
- Gating important areas
- Exploration rewards

**Inspector Properties:**
- Name: Unique identifier
- Key Reference: Which key unlocks this door
- Locked: Start locked or unlocked (Yes/No)
- NPC Door Interaction: Can NPCs use this door? (Not Openable, Openable, Open and Close Behind)

**Wiring Options (Sender):**
- **OnInteractFailed**: Player tried to open without key
- **OnDoorOpened**: Door finished opening
- **OnDoorClosed**: Door finished closing

**Wiring Options (Receiver):**
- **Open**: Opens the door (with Forward Direction option)
- **Close**: Closes the door
- **ToggleOpen**: Switches open/closed state
- **Unlock**: Unlocks without opening

**Works well with:**
- Key, Lever, Pressure Plate, Counter Rule

**Video Tutorial:** Bridges video for gate mechanics: https://youtu.be/buj58tI54xI

---

## What is a Door - Mechanical in Endstar?

**Door - Mechanical** is a door controlled purely by wiring (no key required).

**When to use it:**
- Puzzle-triggered doors
- Doors opened by switches/levers
- Automatic doors

**Inspector Properties:**
- Name: Unique identifier
- NPC Door Interaction: Can NPCs use this door? (Not Openable, Openable, Open and Close Behind)

**Wiring Options (Sender):**
- **OnDoorOpened**: Door finished opening
- **OnDoorClosed**: Door finished closing

**Wiring Options (Receiver):**
- **Open**: Opens the door (with Forward Direction option)
- **Close**: Closes the door
- **ToggleOpen**: Switches open/closed state

**Works well with:**
- **Lever** - Pull lever to open
- **Pressure Plate** - Step to open
- **Counter Rule** - Open after X items collected
- **Timer Rule** - Auto-close after delay
- **Trigger Volume** - Open when player approaches

---

## What is a Basic Level Gate in Endstar?

**Basic Level Gate** is a gate prop that transitions players to another level when entered.

**When to use it:**
- End of level exits
- Doorways between areas
- Level selection hubs

**Inspector Properties:**
- Name: Unique identifier
- Level: Which level to go to
- Target Spawn Points: Where players appear in new level

**Wiring Options (Sender):**
- **OnLevelEntered**: Player entered the gate
- **OnTransitionStarted**: Level change began

**Wiring Options (Receiver):**
- **SetText**: Change displayed text
- **SetRawText**: Set text from another source

---

## What is a Lever in Endstar?

**Lever** is a switch prop that players can flip between two positions.

**When to use it:**
- Activating mechanisms
- Opening doors
- Turning systems on/off
- Puzzle elements

**Inspector Properties:**
- Name: Unique identifier
- Start in Position 2: Should lever start flipped? (No action on initial flip)

**Wiring Options (Sender):**
- **OnFlipped**: Triggers every time lever is flipped
- **OnFlippedPosition1**: Triggers when flipped to position 1
- **OnFlippedPosition2**: Triggers when flipped to position 2

**Wiring Options (Receiver):**
- **FlipLever**: Flip the lever (from another trigger)
- **SetToPosition1**: Force lever to position 1
- **SetToPosition2**: Force lever to position 2

**Works well with:**
- Door - Mechanical, Timer Rule, NPC Spawner, Sensor Spike Trap, Health Modifier

**Video Tutorials:**
- Countdown Timer & Timer Rule: https://youtu.be/1QuOaU2-Qyk
- Pull Lever to Kill Character: https://youtu.be/4QoLIxX50XQ

---

## What is a Pressure Plate in Endstar?

**Pressure Plate** is a floor switch activated by standing on it.

**When to use it:**
- Weight-triggered puzzles
- Automatic doors
- Trap triggers
- 'Stand here' mechanics

**Inspector Properties:**
- Name: Unique identifier

**Wiring Options (Sender):**
- **OnPressed**: Player stepped on plate
- **OnReleased**: Player stepped off plate

**Works well with:**
- **Door - Mechanical** - Step on to open
- **Sensor Spike Trap** - Step on to trigger trap
- **Bool Gate Rule** - Set true when pressed
- **Counter Rule** - Track how many plates pressed

**Tip:** Use OnPressed for 'step on to activate' and OnReleased for 'hold down to keep active'

---

## What is a Trigger Volume in Endstar?

**Trigger Volume** is an invisible zone that detects when players enter or exit.

**When to use it:**
- Area-based triggers (enter room → something happens)
- Trap zones
- Checkpoint detection
- Cutscene triggers
- Proximity-based events

**Inspector Properties:**
- Name: Unique identifier
- Forward, Backward, Left, Right, Up, Down: Zone size in each direction (units)
- Offset (X,Y,Z): Move the zone from the rule block position
- Only Once: Trigger only first time?

**Wiring Options (Sender):**
- **OnTriggered**: Player entered the volume
- **OnExit**: Player left the volume

**Works well with:**
- Dialogue Node, NPC Spawner, Timer Rule, Health Modifier, Door - Mechanical, Ambient Settings

**Video Tutorials:**
- Rising Ocean: https://youtu.be/fsNwqFtzqsE
- Item Quantity Checker: https://youtu.be/Yi29pfpJg_4
- Pull Lever to Kill NPC: https://youtu.be/4QoLIxX50XQ

---

## What is an Interactable Volume in Endstar?

**Interactable Volume** is an invisible zone that shows an interaction prompt and triggers when player interacts.

**When to use it:**
- 'Press E to interact' areas
- Custom interaction points
- Object examination
- Dialogue triggers

**Inspector Properties:**
- Name: Unique identifier
- Forward, Backward, Left, Right, Up, Down: Zone dimensions
- Offset (X,Y,Z): Position offset
- Anchor Position: Where to show the interaction prompt
- Only Once: Allow interaction only once?

**Wiring Options (Sender):**
- **OnInteracted**: Player pressed interact in the volume

**Works well with:**
- Dialogue Node, Door - Mechanical, Item Spawner

---

## What is a Bool Gate Rule in Endstar?

**Bool Gate Rule** is a true/false switch that can trigger different actions based on its state.

**When to use it:**
- On/off state tracking
- Conditional logic (if this, then that)
- Toggle systems
- Requirement checking

**Inspector Properties:**
- Name: Unique identifier
- Initial State: Starting state (Active/true or Inactive/false)

**Wiring Options (Sender):**
- **OnTrue**: Triggers when gate evaluates to true
- **OnFalse**: Triggers when gate evaluates to false

**Wiring Options (Receiver):**
- **SetGateState**: Set to true or false (Completed)
- **EvaluateGate**: Check current state and trigger OnTrue/OnFalse
- **FlipGateState**: Toggle between true and false

**Works well with:**
- Lever, Counter Rule, Door - Mechanical, Multiple Bool Gates for complex logic chains

**Example Use:** 'Player has key' boolean - set true OnPickup, check before allowing door open

---

## What is a Do Once Rule in Endstar?

**Do Once Rule** ensures something only happens one time, then blocks further triggers.

**When to use it:**
- One-time events (cutscenes, tutorials)
- Preventing exploit/spam
- First-time pickups
- Single-use triggers

**Inspector Properties:**
- Name: Unique identifier

**Wiring Options (Sender):**
- **OnTriggered**: Fires the FIRST time trigger received

**Wiring Options (Receiver):**
- **Trigger**: Attempt to trigger (only works once)
- **Reset**: Allow the Do Once to trigger again

**Works well with:**
- Trigger Volume, Dialogue Node, Key

**Video Tutorial:** Item Quantity Checker (Do Once example): https://youtu.be/Yi29pfpJg_4

---

## What is a Relay Rule in Endstar?

**Relay Rule** receives an event and passes it on to multiple receivers. Like a signal splitter.

**When to use it:**
- One trigger → multiple actions
- Organizing complex wiring
- Signal distribution

**Inspector Properties:**
- Name: Unique identifier

**Wiring Options (Sender):**
- **OnEventReceived**: Triggers when relay receives an event

**Wiring Options (Receiver):**
- **ExecuteEvent**: Send event to this relay

**Example:** Lever flip → Relay → (Door opens AND lights turn on AND sound plays)

---

## What is a Sequence Rule in Endstar?

**Sequence Rule** fires events in order, one after another. Each trigger advances to the next step.

**When to use it:**
- Multi-step puzzles
- Sequential events
- Wave-based spawning
- Combo systems

**Inspector Properties:**
- Name: Unique identifier
- Max Sequence: How many steps (1-10)
- Loop: Restart after reaching end? (Yes/No)

**Wiring Options (Sender):**
- **OnResult1** through **OnResult10**: Events for each step in sequence

**Wiring Options (Receiver):**
- **FireNextEvent**: Advance to next step and fire it

**Works well with:**
- Timer Rule, NPC Spawner, Door - Mechanical

**Example:** FireNextEvent → OnResult1 (spawn easy enemy) → FireNextEvent → OnResult2 (spawn medium enemy) → ...

---

## What is a Randomizer Rule in Endstar?

**Randomizer Rule** randomly fires ONE of up to 10 possible events when triggered.

**When to use it:**
- Random rewards
- Unpredictable enemy spawns
- Loot variety
- Random events

**Inspector Properties:**
- Name: Unique identifier
- Max Result: How many possible outcomes (1-10)

**Wiring Options (Sender):**
- **OnResult1** through **OnResult10**: Random outcome options

**Wiring Options (Receiver):**
- **FireRandomEvent**: Trigger a random result

**Works well with:**
- Item Spawner, NPC Spawner, Dynamic Item Spawner

---

## What is a Level Start Rule in Endstar?

**Level Start Rule** triggers automatically when the level/game starts.

**When to use it:**
- Initialize game state
- Start background music
- Spawn initial enemies
- Show intro dialogue
- Set initial conditions

**Inspector Properties:**
- Name: Unique identifier
- Only Fire On First Load: Only trigger first time level loads (not on respawn)

**Wiring Options (Sender):**
- **OnLevelStart**: Level has started

**Works well with:**
- Timer Rule, NPC Spawner, Countdown Timer, Dialogue Node

---

## What is a Timer Rule in Endstar?

**Timer Rule** waits for a specified duration, then triggers an event. Can loop.

**When to use it:**
- Delayed events
- Repeating spawns
- Timed puzzles
- Cooldowns
- Wave intervals

**Inspector Properties:**
- Name: Unique identifier
- Duration: How long to wait (seconds)
- Loop: Restart timer after finishing? (Yes/No)
- Start Immediately: Begin timing when level starts? (Yes/No)

**Wiring Options (Sender):**
- **OnTimerFinished**: Timer completed its duration

**Wiring Options (Receiver):**
- **StartTimer**: Begin the countdown
- **StopTimer**: Pause/stop the timer

**Works well with:**
- NPC Spawner, Counter Rule, Door - Mechanical, Lever

**Video Tutorial:** Countdown Timer & Timer Rule: https://youtu.be/1QuOaU2-Qyk

---

## What is a Countdown Timer in Endstar?

**Countdown Timer** is a visible countdown that can display remaining time.

**When to use it:**
- Race against time challenges
- Bomb defusal timers
- Visible countdowns for players
- Time limits

**Inspector Properties:**
- Name: Unique identifier
- Time Step: Countdown interval (seconds)
- Start Immediately: Begin when level starts? (Yes/No)
- Countdown Value: Starting number (seconds)

**Wiring Options (Sender):**
- **OutputAsText**: Sends countdown number as text data

**Wiring Options (Receiver):**
- **StartTimer**: Begin countdown
- **ResetTimer**: Reset to starting value
- **PauseTimer**: Pause countdown

**Works well with:**
- **Anachronist Billboard** - Wire OutputAsText → SetRawText to display countdown
- **Level Transition Rule** - End level when time runs out
- **Lever** - Start/pause timer

**Video Tutorials:**
- Countdown Timer & Timer Rule: https://youtu.be/1QuOaU2-Qyk
- Rising Ocean: https://youtu.be/fsNwqFtzqsE
- Teleport Point: https://youtu.be/cUBOVIw2__o

---

## What is a Counter Rule in Endstar?

**Counter Rule** counts events and triggers when a goal number is reached.

**When to use it:**
- 'Collect 5 coins to open door'
- 'Kill 10 enemies to win'
- 'Press 3 buttons in any order'
- Tracking player progress

**Inspector Properties:**
- Name: Unique identifier
- Goal Number: Target count to reach (whole number)
- Loop and Repeat: Reset to 0 after reaching goal? (Yes/No)

**Wiring Options (Sender):**
- **OnCountReached**: Counter reached the goal number

**Wiring Options (Receiver):**
- **ModifyCount**: Add to counter (usually +1)
- **SetGoal**: Change the goal number
- **ResetAndClearCounter**: Reset count to 0

**Works well with:**
- Treasure/Key/Resource (OnPickup → ModifyCount)
- Medium NPC (OnDefeated → ModifyCount)
- Door - Mechanical (OnCountReached → Open)
- Anachronist Billboard (display count)

**Video Tutorial:** Counter Rule: https://youtu.be/Ey2FRDx7eNc

---

## What is a Medium NPC in Endstar?

**Medium NPC** is a pre-placed NPC character (enemy, ally, or neutral).

**When to use it:**
- Pre-positioned enemies
- Quest NPCs
- Allies/companions
- Shop keepers

**Inspector Properties:**
- Name, NPC Name (displayed in game)
- Character Visuals: Appearance options
- NPC Class: Blank (no weapon), Grunt (sword), Rifleman (ranged), Zombie
- Starting Health: 1-20
- Team: Player's team or others
- Group: Group for shared behavior
- Combat: Passive, Defensive, Aggressive
- Damage: Take Damage or Ignore Damage
- Physics: Take Physics or Ignore Physics
- Idle: Stand, Idle, Wander, Rove
- NPC Movement Mode: Walk or Run
- Pathfinding Range: Global, Area, Island

**Wiring Options (Sender):**
- **OnEntityHealthZeroed**: This NPC's health reached 0
- **OnTargetHealthZeroed**: NPC's target's health reached 0
- **OnDefeated**: This NPC was killed

**Wiring Options (Receiver):**
- **AttemptSwitchTarget**, **SetDisplayName**, **SetLocalizedText**, **SetText**

**Works well with:**
- Counter Rule, Dynamic Item Spawner, Dialogue Node

**Video Tutorials:**
- Dynamic Item Spawner: https://youtu.be/60M_UAvr0dI
- Health UI for NPCs: https://youtu.be/wxPeRg1u6gU
- Item Quantity Checker (NPC shop): https://youtu.be/Yi29pfpJg_4

---

## What is an NPC Spawner in Endstar?

**NPC Spawner** spawns an NPC when triggered (not pre-placed).

**When to use it:**
- Wave-based enemy spawning
- Triggered enemy encounters
- Dynamic ally spawning
- Boss fights

**Inspector Properties:**
- Name, Spawn on Start, Position
- Visuals: NPC appearance
- Class: Blank, Grunt, Rifleman, Zombie
- Health: 1-100
- NPC Team, Group, Range, Combat, Damage, Physics, Idle, NPC Movement Mode

**Wiring Options (Sender):**
- **OnNpcSpawned**: NPC was successfully spawned

**Wiring Options (Receiver):**
- **TrySpawnNpc**: Attempt to spawn the NPC

**Works well with:**
- **Timer Rule** - Spawn enemies every X seconds
- **Trigger Volume** - Spawn when player enters area
- **Counter Rule** - Track spawned/killed NPCs
- **Lever** - Player-controlled spawning

**Video Tutorial:** Countdown Timer & Timer Rule (spawning): https://youtu.be/1QuOaU2-Qyk

---

## What is a Random Multiple NPC Spawner in Endstar?

**Random Multiple NPC Spawner** spawns random NPCs from a pool at defined locations.

**When to use it:**
- Varied enemy encounters
- Surprise enemy spawns
- Emergent gameplay
- Arena modes

This rule block is helpful to create emergent gameplay and surprise players with unpredictable enemy encounters.

**Video Tutorials:**
- Random Multiple NPC Spawner: https://youtu.be/CCKxMd_Q8kU
- NPC Death Toll: https://youtu.be/2D8L8fw35Z0
- Health UI for NPCs: https://youtu.be/wxPeRg1u6gU

---

## What is a Sensor Spike Trap in Endstar?

**Sensor Spike Trap** is a trap that damages entities when triggered by proximity.

**When to use it:**
- Dungeon traps
- Hazard areas
- Defensive mechanisms
- Puzzle obstacles

**Inspector Properties:**
- Name: Unique identifier
- Start Active: Begin active when level starts?

**Wiring Options (Sender):**
- **OnEntityHit**: Something was damaged by trap
- **OnTriggered**: Trap was activated

**Wiring Options (Receiver):**
- **Activate**: Turn trap on
- **Deactivate**: Turn trap off
- **ToggleActive**: Switch on/off state

**Works well with:**
- Lever, Pressure Plate, Timer Rule

---

## What is a Mechanical Spike Trap in Endstar?

**Mechanical Spike Trap** is a manually triggered spike trap (doesn't auto-sense).

**When to use it:**
- Script-controlled traps
- Puzzle traps
- Timed hazards

**Inspector Properties:**
- Name: Unique identifier

**Wiring Options (Sender):**
- **OnEntityHit**: Something was damaged
- **OnTriggered**: Trap was triggered

**Wiring Options (Receiver):**
- **Trigger**: Activate the trap

---

## What is a Sensor Arrow Trap in Endstar?

**Sensor Arrow Trap** fires arrows when it detects entities.

**When to use it:**
- Ranged trap hazards
- Dungeon corridors
- Defensive systems

**Inspector Properties:**
- Name: Unique identifier
- Set Active: Start active? (Yes/No)
- Fire Delay: Cooldown between shots (0.25-5 seconds)

**Wiring Options (Sender):**
- **OnFired**: Arrow was shot
- **OnFiringStarted**: Trap began firing
- **OnFiringStopped**: Trap stopped firing

**Wiring Options (Receiver):**
- **Activate**: Turn on
- **Deactivate**: Turn off
- **ToggleActive**: Switch state

---

## What is a Sentry Turret in Endstar?

**Sentry Turret** is an automated turret that shoots enemies.

**When to use it:**
- Defensive positions
- Allied fire support
- Enemy hazards

**Inspector Properties:**
- Name: Unique identifier
- Team: Whose side is it on?

**Note:** Sentry Turret has no wiring options - it operates automatically based on team settings.

---

## What is a Damager Block in Endstar?

**Damager Block** deals damage to a specified target when triggered.

**When to use it:**
- Custom damage traps
- Scripted damage events
- Invisible hazards

**Inspector Properties:**
- Name: Unique identifier
- Target Object: What to damage

**Wiring Options (Receiver):**
- **DamageTarget**: Deal damage to target

**Works well with:**
- Trigger Volume, Timer Rule, Lever

---

## What is a Health Modifier in Endstar?

**Health Modifier** modifies health of connected character (damage or heal).

**When to use it:**
- Damage zones
- Healing areas
- Custom death mechanics
- Fake respawn systems

**Video Tutorial:** Teleporter & Health Utilities & Health Modifier: https://youtu.be/CFBrBYaVHrs

---

## What is Health Utilities in Endstar?

**Health Utilities** restores health to connected character.

**When to use it:**
- Healing zones
- Checkpoint healing
- Triggered healing

**Video Tutorial:** Teleporter & Health Utilities & Health Modifier: https://youtu.be/CFBrBYaVHrs

---

## What is a Dynamic Item Spawner in Endstar?

**Dynamic Item Spawner** spawns items at a location when triggered (like enemy death drops).

**When to use it:**
- Loot drops from enemies
- Reward spawning
- Key drops from bosses

This rule block grants items to players by spawning them when an NPC dies. Item is spawned at the NPC's location of death.

**Works well with:**
- Medium NPC (OnDefeated → spawn item)
- Randomizer Rule (random loot type)

**Video Tutorial:** Dynamic Item Spawner: https://youtu.be/60M_UAvr0dI

---

## What is an Item Spawner in Endstar?

**Item Spawner** spawns a specific item when triggered.

**When to use it:**
- Shop item giving
- Reward distribution
- Puzzle rewards

**Video Tutorial:** Item Quantity Checker & Item Spawner: https://youtu.be/Yi29pfpJg_4

---

## What is an Item Quantity Checker in Endstar?

**Item Quantity Checker** checks if player has enough of a specific item.

**When to use it:**
- Shop requirements
- Crafting checks
- Gate conditions

This is useful for creating shop scenarios where the player needs to have enough resources before they can get an item.

**Video Tutorial:** Item Quantity Checker & Item Spawner: https://youtu.be/Yi29pfpJg_4

---

## What is an NPC Death Toll in Endstar?

**NPC Death Toll** tracks total NPCs, deaths, and survivors automatically.

This rule block keeps track of all NPCs spawned and placed, the total death toll, and also NPCs which are still alive.

**Video Tutorial:** NPC Death Toll: https://youtu.be/2D8L8fw35Z0

---

## What is Health UI for NPCs in Endstar?

**Health UI for NPCs** shows health bars above NPCs when they take damage.

This rule block doesn't require much of a setup. It automatically keeps track of all NPCs placed or spawned in the level, and when they get hit, a text bubble bark appears above them showing how much health each NPC has left.

**Video Tutorial:** Health UI for NPCs: https://youtu.be/wxPeRg1u6gU

---

## What is a Dialogue Node in Endstar?

**Dialogue Node** creates conversations with NPCs.

**When to use it:**
- NPC dialogues
- Quest giving
- Story elements
- Shop interactions

**Inspector Properties:**
- Name: Unique identifier
- On Start: Active when level starts?
- NPC Reference: Which NPC talks
- Text: Dialogue text
- Only Once: One-time conversation?
- NPC Behavior Overrides: Change NPC behavior during dialogue (Combat Mode, Damage Mode, Physics Mode)

**Wiring Options (Sender):**
- **OnFinishedTalking**: Dialogue completed
- **OnInteractionCompleted**: Player finished interaction
- **OnInteractionCanceled**: Player left early
- **OnInteractionFinished**: Any interaction end

**Wiring Options (Receiver):**
- **GiveInstruction**: Start this dialogue
- **RescindInstruction**: Cancel this instruction

**Video Tutorial:** Dialogue Node: https://youtu.be/0vbVkD5qLms

---

## What is a Move To Position Node in Endstar?

**Move To Position Node** commands an NPC to move to a specific location.

**When to use it:**
- Scripted NPC movement
- Patrol waypoints
- Quest NPC positioning

**Inspector Properties:**
- Name: Unique identifier
- NPC Reference: Which NPC to command
- Position: Destination
- Destination Tolerance: How close is 'arrived'
- NPC Behavior Overrides: Behavior during movement

**Wiring Options (Sender):**
- **OnCommandSucceeded**: NPC reached destination
- **OnCommandFailed**: NPC couldn't reach
- **OnCommandFinished**: Movement ended (either way)

**Wiring Options (Receiver):**
- **GiveInstruction**: Start movement
- **RescindInstruction**: Cancel movement

---

## What are Idle Node, Stand Node, Wander Node, and Rove Node in Endstar?

These are NPC behavior nodes that set idle behaviors:

| Node | Behavior |
|------|----------|
| **Idle Node** | NPC does idle animations in place |
| **Stand Node** | NPC stands still |
| **Wander Node** | NPC wanders within max distance |
| **Rove Node** | NPC roams around |

**Common Properties:**
- Name, NPC Reference, NPC Behavior Overrides
- Wander/Rove have max distance settings

**Wiring Options (Receiver):**
- **GiveInstruction**: Start this behavior
- **RescindInstruction**: Cancel this behavior

---

## What is Ambient Settings in Endstar?

**Ambient Settings** controls skybox and atmosphere settings.

**When to use it:**
- Day/night transitions
- Weather changes
- Mood shifts

**Inspector Properties:**
- Name: Unique identifier
- Is Default: Is this the starting setting?
- Current Skybox: Which skybox to use

**Wiring Options (Receiver):**
- **ChangeSkybox**: Switch to this ambient setting

**Works well with:**
- Trigger Volume, Lever, Timer Rule

---

## What is a Filters rule block in Endstar?

**Filters** applies visual filters/effects to the screen.

**When to use it:**
- Damage effects (red flash)
- Underwater effects
- Dream sequences
- Status effects

**Inspector Properties:**
- Name: Unique identifier
- Visible in Creator: Show in editor?
- Transition Time: Fade duration (seconds)

**Wiring Options (Receiver):**
- **SetFilterType**: Change to a filter
- **SetTransitionTime**: Change fade speed
- **SetFilterTypeForPlayer**: Apply filter to player only

---

## What is an Ocean Plane in Endstar?

**Ocean Plane** is a water plane that can rise, fall, and detect levels.

**When to use it:**
- Rising water puzzles
- Flooding mechanics
- Water level changes

**Inspector Properties:**
- Name: Unique identifier
- Plane Type: Empty or Ocean
- Move Speed: How fast it moves (0.1-10)

**Wiring Options (Sender):**
- **OnWaterLevelReached**: Water reached set level

**Wiring Options (Receiver):**
- **ModifyHeight**: Change height by amount
- **SetHeight**: Set specific height
- **ModifyMoveSpeed**: Change speed
- **SetMoveSpeed**: Set specific speed

**Video Tutorial:** Rising Ocean: https://youtu.be/fsNwqFtzqsE

---

## What is a Torch on the Wall in Endstar?

**Torch on the Wall** is a light-emitting wall torch prop for decoration and lighting.

**When to use it:**
- Dungeon lighting
- Medieval atmosphere
- Wall decorations

**Video Tutorial:** Torch on the Wall: https://youtu.be/01n5brVV2Xw

---

## What glass props are available in Endstar?

Endstar has several glass props:

| Prop | Description |
|------|-------------|
| **Glass** | Transparent cube that may work as a glass |
| **Glass 1X1** | Stained glass, 1x1 size |
| **Glass 1X2** | Stained glass, 1x2 size |
| **Glass 2X1** | Stained glass, 2x1 size |
| **Glass 2X2** | Stained glass, 2x2 size |

**Video Tutorials:**
- Glass Prop: https://youtu.be/qjW9nXoQgwo
- Stained Glass: https://youtu.be/n7TBpTJcrg8

---

## What bridge and gate props are available in Endstar?

Endstar has these bridge and gate structures:

| Prop | Description |
|------|-------------|
| **Bridge** | Static stone bridge |
| **Opening Bridge** | Tower Bridge-style with opening wings |
| **Gate** | Medieval lifting gate that can block or allow passage |

**Video Tutorial:** Bridges: https://youtu.be/buj58tI54xI

---

## What is a Physics Cube in Endstar?

**Physics Cube** is a cube affected by physics (can be pushed, falls).

**When to use it:**
- Physics puzzles
- Pushable objects
- Weight mechanics

**Video Tutorial:** Rising Ocean (physics cube example): https://youtu.be/fsNwqFtzqsE

---

## What is a Portal in Endstar?

**Portal** teleports players/NPCs to destination points within the same level.

**When to use it:**
- Fast travel points
- Shortcuts
- Puzzle teleportation

**Inspector Properties:**
- Name: Unique identifier
- Destination Points: Where to teleport to (one or more)
- Destination Offset: Randomization around destination
- Cooldown: Seconds before can use again

**Wiring Options (Receiver):**
- **AttemptSwitchTarget**: Change destination

**Video Tutorial:** Portal: https://youtu.be/QwbOPz89h6g

---

## What is a Teleporter in Endstar?

**Teleporter** is a rule block that teleports a target to a destination.

**When to use it:**
- Scripted teleportation
- Triggered warps
- Trap teleporters

**Inspector Properties:**
- Name: Unique identifier
- Teleport Target: Who to teleport
- Destination: Where to send them
- Teleport Effect: Visual style (Materialize or Instant)

**Wiring Options (Sender):**
- **OnTeleportSuccess**: Teleport worked
- **OnTeleportFailure**: Teleport failed

**Wiring Options (Receiver):**
- **TriggerTeleport**: Execute teleport

**Video Tutorial:** Teleporter & Health Utilities: https://youtu.be/CFBrBYaVHrs

---

## What is a Teleport Point in Endstar?

**Teleport Point** is a teleporter with visual countdown display and range detection.

**When to use it:**
- Star Trek-style beam pads
- Visible teleport areas
- Range-based teleportation

It has a countdown display above it. You can define a range and if characters are within this range, they are being beamed up.

**Video Tutorial:** Teleport Point: https://youtu.be/cUBOVIw2__o

---

## What is an Anachronist Bounce Pad in Endstar?

**Anachronist Bounce Pad** launches players/entities into the air.

**When to use it:**
- Platforming mechanics
- Reaching high areas
- Fun movement

**Inspector Properties:**
- Name: Unique identifier
- Start Active: Active when level starts? (Yes/No)
- Starting Bounce Height: Launch power (1-35)

**Wiring Options (Sender):**
- **OnEntityBounced**: Something bounced

**Wiring Options (Receiver):**
- **Activate**: Turn on
- **Deactivate**: Turn off
- **ToggleActive**: Switch state
- **SetBounceHeight**: Set exact height
- **ModifyBounceHeight**: Change height by amount

**Video Tutorial:** Rising Ocean (bounce pad example): https://youtu.be/fsNwqFtzqsE

---

## What is a Level Transition Rule in Endstar?

**Level Transition Rule** changes to a different level when triggered.

**When to use it:**
- Win conditions
- Fail conditions
- Level progression

**Inspector Properties:**
- Name: Unique identifier
- Level: Destination level
- Target Spawn Points: Where to spawn in new level

**Wiring Options (Receiver):**
- **ChangeLevel**: Execute level transition

**Works well with:**
- Counter Rule (win after collecting all items)
- Countdown Timer (fail when time runs out)
- Trigger Volume (enter area to win)

---

## What is an Anachronist Billboard in Endstar?

**Anachronist Billboard** is a display board that shows text to players.

**When to use it:**
- Score displays
- Countdown displays
- Instructions
- Messages

**Inspector Properties:**
- Name: Unique identifier
- Text: Initial displayed text

**Wiring Options (Receiver):**
- **SetText**: Display specific text
- **SetRawText**: Display data from sender (auto or manual override)

**Works well with:**
- **Countdown Timer** - OutputAsText → SetRawText shows countdown
- **Counter Rule** - Display count
- **Timer Rule** - Display remaining time

**Video Tutorials:**
- NPC Death Toll (billboard display): https://youtu.be/2D8L8fw35Z0
- Banner race (billboard): https://youtu.be/Wo-IXmCvQzg
- Countdown Timer: https://youtu.be/1QuOaU2-Qyk

---

## What is an Anachronist Floor Sign in Endstar?

**Anachronist Floor Sign** is a sign players can read with sequential text pages.

**When to use it:**
- Tutorial signs
- Lore/story
- Instructions
- Multi-page info

**Inspector Properties:**
- Name: Unique identifier
- Text: Text pages (shown sequentially when player interacts)
- Sign Name: Displayed name of sign

**Wiring Options (Sender):**
- **OnFinishedReading**: Player read all pages

**Wiring Options (Receiver):**
- **Generated**, **SetDisplayName**, **SetLocalizedText**, **SetText**

---

## What is a Banner in Endstar?

**Banner** is a flag/banner that detects when players reach it (includes built-in teleport).

**When to use it:**
- Race finish lines
- Capture points
- Goal markers

This prop has a built-in Teleport and detects players when they reach them. Great for scenarios where players race against each other to arrive at a specific destination.

**Video Tutorial:** Banner - First to Flag Wins: https://youtu.be/Wo-IXmCvQzg

---

## What is a Basic Spawn Point in Endstar?

**Basic Spawn Point** is where players spawn/respawn in the level.

**When to use it:**
- Initial player spawn
- Respawn points
- Checkpoint spawns

**Inspector Properties:**
- Name: Unique identifier
- Starting Health: Player health on spawn (1-20)
- Inventory Size: Inventory slots (1-10)

**Wiring Options (Sender):**
- **OnPlayerSpawned**: Player spawned here
- **OnLevelEntered**: Level loaded with this spawn

---

## What is a Checkpoint System in Endstar?

**Checkpoint System** automatically respawns the player at the closest spawn point when they die.

**When to use it:**
- Automatic respawning
- Progress-saving checkpoints
- Player-friendly death handling

**Video Tutorial:** Checkpoint System: https://youtu.be/ob46I22Qjs8

---

## How do I make a door that opens with a key in Endstar?

**Recipe: Key and Locked Door**
Difficulty: Beginner

**Goal:** Player must find key to open door.

**Props needed:** Key, Door - Locked

**Steps:**
1. Place a **Door - Locked** where you want the barrier
2. Place a **Key** somewhere in the level
3. Select the Door and in Inspector, set **Key Reference** to your Key
4. Done! Door automatically unlocks when player has the key

**No wiring required** - the key reference handles it automatically.

---

## How do I make a door that opens after collecting items in Endstar?

**Recipe: Collect X Items to Open Door**
Difficulty: Intermediate

**Goal:** Door opens after collecting 3 treasures.

**Props needed:** Treasure (x3 or more), Counter Rule, Door - Mechanical

**Steps:**
1. Place 3 (or more) **Treasure** props in your level
2. Place a **Counter Rule** - set Goal Number = 3
3. Place a **Door - Mechanical** where you want the barrier
4. Wire each Treasure to Counter:
   - Select Treasure first (sender)
   - Select Counter Rule second (receiver)
   - Connect **OnPickup** → **ModifyCount**
   - Click Confirm
5. Wire Counter to Door:
   - Select Counter Rule first (sender)
   - Select Door second (receiver)
   - Connect **OnCountReached** → **Open**
   - Click Confirm

**Result:** Door opens when 3rd treasure is collected!

**Video Reference:** Counter Rule: https://youtu.be/Ey2FRDx7eNc

---

## How do I make a lever-controlled door in Endstar?

**Recipe: Lever-Controlled Door**
Difficulty: Beginner

**Goal:** Flip lever to open/close door.

**Props needed:** Lever, Door - Mechanical

**Steps:**
1. Place a **Lever** near the door location
2. Place a **Door - Mechanical**
3. Wire:
   - Select Lever first (sender)
   - Select Door second (receiver)
   - Connect **OnFlipped** → **ToggleOpen**
   - Click Confirm

**Result:** Each lever flip toggles the door open or closed!

---

## How do I make a pressure plate door in Endstar?

**Recipe: Pressure Plate Door (Hold to Keep Open)**
Difficulty: Beginner

**Goal:** Door opens when standing on plate, closes when stepping off.

**Props needed:** Pressure Plate, Door - Mechanical

**Steps:**
1. Place **Pressure Plate** where players will step
2. Place **Door - Mechanical**
3. Wire for opening:
   - Pressure Plate → Door
   - **OnPressed** → **Open**
   - Click Confirm
4. Wire for closing:
   - Pressure Plate → Door
   - **OnReleased** → **Close**
   - Click Confirm

**Result:** Door stays open only while standing on the plate!

---

## How do I create enemy waves that spawn over time in Endstar?

**Recipe: Wave-Based Enemy Spawner**
Difficulty: Intermediate

**Goal:** Enemies spawn every 10 seconds.

**Props needed:** Timer Rule, NPC Spawner (multiple)

**Steps:**
1. Place **Timer Rule** - set Duration = 10, Loop = Yes, Start Immediately = Yes
2. Place multiple **NPC Spawner** rule blocks with different positions
3. Wire Timer to each spawner:
   - Timer Rule → NPC Spawner
   - **OnTimerFinished** → **TrySpawnNpc**
   - Click Confirm
4. Repeat for each spawner

**Result:** Every 10 seconds, NPCs spawn at all connected spawners!

**Enhancement:** Use **Sequence Rule** instead to spawn different enemy types each wave.

---

## How do I make a door open after killing all enemies in Endstar?

**Recipe: Kill All Enemies to Proceed**
Difficulty: Intermediate

**Goal:** Door opens after killing all enemies.

**Props needed:** Medium NPC (multiple) OR NPC Spawner, Counter Rule, Door - Mechanical

**Steps:**
1. Place your **Medium NPCs** (e.g., 5 enemies)
2. Place **Counter Rule** - set Goal Number = 5 (number of enemies)
3. Place **Door - Mechanical**
4. Wire each NPC to counter:
   - Medium NPC → Counter Rule
   - **OnDefeated** → **ModifyCount**
   - Click Confirm
5. Wire counter to door:
   - Counter Rule → Door
   - **OnCountReached** → **Open**
   - Click Confirm

**Result:** Door opens when all 5 enemies are defeated!

---

## How do I create a timed challenge with countdown display in Endstar?

**Recipe: Timed Challenge with Countdown Display**
Difficulty: Intermediate

**Goal:** Player has 60 seconds to reach goal or fail.

**Props needed:** Countdown Timer, Anachronist Billboard, Level Transition Rule (x2)

**Steps:**
1. Place **Countdown Timer** - set Countdown Value = 60, Start Immediately = Yes
2. Place **Anachronist Billboard** to display time
3. Wire countdown to billboard:
   - Countdown Timer → Billboard
   - **OutputAsText** → **SetRawText**
   - Click Confirm
4. Place **Level Transition Rule** for failure (links to fail/retry level)
5. Use additional logic (Bool Gate, Timer reaching 0) to trigger failure

**For win condition:** Use **Trigger Volume** at goal location → Level Transition Rule to win level

**Video Reference:** Countdown Timer & Timer Rule: https://youtu.be/1QuOaU2-Qyk

---

## How do I create an NPC shop in Endstar?

**Recipe: NPC Shop System**
Difficulty: Advanced

**Goal:** Player talks to NPC, if they have enough coins, they get an item.

**Props needed:** Medium NPC, Dialogue Node, Item Quantity Checker, Item Spawner, Do Once Rule, Trigger Volume

**Steps:**
1. Place **Medium NPC** (the shopkeeper)
2. Place **Trigger Volume** around NPC
3. Place **Dialogue Node** - set NPC Reference, Text = 'Want to buy a sword? (2 coins)'
4. Place **Item Quantity Checker** - set required item and quantity
5. Place **Item Spawner** - set item to give
6. Place **Do Once Rule** (prevents buying multiple times per interaction)
7. Wire:
   - Trigger Volume → Dialogue Node (OnTriggered → GiveInstruction)
   - Dialogue Node → Item Quantity Checker (OnInteractionCompleted → check)
   - Item Quantity Checker (if has enough) → Item Spawner
   - Use Do Once to prevent spam

**Video Tutorial:** Item Quantity Checker & Item Spawner: https://youtu.be/Yi29pfpJg_4

---

## How do I create a fake respawn system in Endstar?

**Recipe: Fake Respawn (Teleport + Heal Instead of Death)**
Difficulty: Advanced

**Goal:** When player would die, teleport them and heal instead of actual death.

**Props needed:** Health Modifier, Health Utilities, Teleporter, Trigger Volume

This creates a system where instead of the normal death/respawn, the player is teleported back and healed, maintaining their progress.

**Video Tutorial:** Teleporter & Health Utilities & Health Modifier: https://youtu.be/CFBrBYaVHrs

---

## How do I create a rising water escape puzzle in Endstar?

**Recipe: Rising Water Escape**
Difficulty: Advanced

**Goal:** Water rises over time. Player must solve puzzle before drowning.

**Props needed:** Ocean Plane, Timer Rule, Countdown Timer, Anachronist Billboard, Physics Cube, Lever, Door

This creates an exciting race-against-time scenario where the player interacts with Physics Cubes to complete a mechanism that would stop rising waters before everybody drowns.

**Video Tutorial:** Rising Ocean: https://youtu.be/fsNwqFtzqsE

---

## How do I spawn random enemy types in Endstar?

**Recipe: Random Enemy Types**
Difficulty: Advanced

**Goal:** Random enemy spawns from pool of possibilities.

**Props needed:** Randomizer Rule, NPC Spawner (multiple with different enemy types)

**Steps:**
1. Place **Randomizer Rule** - set Max Result = 3 (for 3 enemy types)
2. Place 3 **NPC Spawner** rule blocks with different enemy types
3. Wire randomizer to spawners:
   - Randomizer → Spawner 1: **OnResult1** → **TrySpawnNpc**
   - Randomizer → Spawner 2: **OnResult2** → **TrySpawnNpc**
   - Randomizer → Spawner 3: **OnResult3** → **TrySpawnNpc**
4. Wire trigger to randomizer:
   - (Timer Rule or Trigger Volume) → Randomizer: → **FireRandomEvent**

**Video Tutorial:** Random Multiple NPC Spawner: https://youtu.be/CCKxMd_Q8kU

---

## How do I create a race to the flag in Endstar?

**Recipe: First to Flag Race**
Difficulty: Intermediate

**Goal:** Race to reach the banner first.

**Props needed:** Banner, Timer Rule, Anachronist Billboard

The Banner prop has a built-in teleport and detects players when they reach it. This can be used for scenarios where players race against each other to arrive at a specific destination.

**Video Tutorial:** Banner - First to Flag Wins: https://youtu.be/Wo-IXmCvQzg

---

## My door won't open even though I wired it in Endstar. What's wrong?

**Troubleshooting: Door Won't Open**

Check these common issues:

1. **Sender/receiver order** - Did you click the props in the RIGHT order? Sender (trigger) must be clicked FIRST, receiver (action) SECOND.

2. **Confirm button** - Did you click 'Confirm' after making the wire? The connection isn't finalized until you confirm!

3. **Wrong action selected** - Check the wire options - did you select the correct action? Make sure it's 'Open' or 'ToggleOpen'.

4. **Door is locked** - Is the door's 'Locked' property set to Yes? If so, you need to either:
   - Assign a Key Reference and have the player collect that key
   - Wire an 'Unlock' action first before 'Open'

---

## My Counter Rule doesn't seem to be counting in Endstar. What's wrong?

**Troubleshooting: Counter Not Working**

Check these common issues:

1. **Goal Number** - Verify Goal Number is set correctly (not 0 or too high)

2. **Wrong receiver action** - Check that you wired to **ModifyCount**, not something else like SetGoal or ResetAndClearCounter

3. **Missing wires** - Make sure EACH item is wired to the counter. Each treasure/key needs its own wire - they don't share automatically!

4. **Loop setting** - Check if 'Loop and Repeat' is enabled - this resets your count to 0 after reaching the goal, which might make it seem like it's not working

---

## My Timer Rule won't start in Endstar. What's wrong?

**Troubleshooting: Timer Won't Start**

Check these common issues:

1. **Start Immediately setting** - Check the 'Start Immediately' property. If it's set to No, you need to wire something to **StartTimer** to begin it.

2. **Duration is zero** - Verify Duration is set to a value (not 0). A zero duration won't produce visible results.

3. **Missing trigger** - If Start Immediately is No, make sure you have something wired to StartTimer (like a Lever, Level Start Rule, or Trigger Volume)

---

## My NPC is stuck and won't move in Endstar. What's wrong?

**Troubleshooting: NPC Stuck / Won't Move**

Check these common issues:

1. **Pathfinding Range** - Check the NPC's **Pathfinding Range** setting. Try 'Global' for the largest navigation range. 'Area' and 'Island' have smaller ranges.

2. **Blocked path** - Make sure the path isn't blocked by obstacles. NPCs can't walk through walls or props!

3. **Idle setting** - Check the **Idle** property - is it set to 'Stand'? If so, the NPC is meant to stand still. Change to 'Wander' or 'Rove' for movement.

4. **Valid path** - Verify there's a valid walkable path between NPC and destination. Some terrain may not be navigable.

---

## My Trigger Volume isn't detecting the player in Endstar. What's wrong?

**Troubleshooting: Trigger Volume Not Detecting**

Check these common issues:

1. **Volume dimensions** - Check the size values (Forward, Backward, Left, Right, Up, Down). They might be too small! Try 5-10 units in each direction.

2. **Offset position** - Use the Offset property to position the volume correctly. It might be offset away from where the player walks.

3. **Only Once setting** - Check the 'Only Once' property - it may have already triggered once and won't trigger again.

4. **Volume too small** - Make sure the volume is large enough for the player to actually enter it.

---

## The wiring option I need isn't showing in Endstar. What's wrong?

**Troubleshooting: Missing Wiring Options**

Check these common issues:

1. **Selection order** - Make sure you selected props in the correct order: sender FIRST, receiver SECOND.

2. **Sender vs Receiver** - Some props only have Sender options (like Treasure which only has OnPickup). Some only have Receiver options (like Door's Open action). Check if the prop supports what you need.

3. **Wrong side of screen** - Remember: Sender options appear on the LEFT side, Receiver options on the RIGHT side. Look on the correct side!

4. **Wrong prop type** - Not all props/rule blocks can do everything. Check if this prop actually supports the action you need.

---

## Multiple things are happening when I only want one in Endstar. How do I fix this?

**Troubleshooting: Multiple Unwanted Triggers**

Check these common issues:

1. **Use Do Once Rule** - Add a **Do Once Rule** between your trigger and action to limit it to a single occurrence.

2. **Duplicate wires** - Check if you accidentally have multiple wires connecting the same things. Delete extra wires.

3. **Only Once property** - Check the 'Only Once' property on Trigger Volumes and other props that have it.

---

## My Sequence Rule or Randomizer Rule isn't working in Endstar. What's wrong?

**Troubleshooting: Sequence/Randomizer Not Working**

Check these common issues:

1. **Max setting** - Verify Max Sequence or Max Result is set correctly. If you want 3 outcomes, set it to 3.

2. **Missing trigger** - Make sure you wired **FireNextEvent** (for Sequence) or **FireRandomEvent** (for Randomizer) FROM something. They need to be triggered!

3. **Result wiring** - Check that OnResult1, OnResult2, etc. are wired TO different things. Each result needs to trigger something.

---

## My NPC conversation won't start in Endstar. What's wrong?

**Troubleshooting: NPC Dialogue Not Starting**

Check these common issues:

1. **NPC Reference** - Check that **NPC Reference** is set in the Dialogue Node properties. It must point to an NPC.

2. **Interaction point** - Verify the player can reach the interaction point. The NPC might be behind a barrier.

3. **Settings** - Check 'On Start' and 'Only Once' settings. If 'Only Once' is enabled and it already triggered, it won't trigger again.

4. **Trigger wiring** - Make sure the Trigger Volume or Interactable Volume is properly wired to the Dialogue Node with GiveInstruction.

---

## What are the most common wiring connections in Endstar?

**Quick Reference: Most Common Wiring Connections**

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

## What video tutorials are available for Endstar?

**Endstar Video Tutorial Index**

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

## How do I use a Counter Rule in Endstar?

**How to Use Counter Rule**

The Counter Rule tracks events and triggers when a goal is reached.

**Setup:**
1. Place a Counter Rule in your level
2. Set **Goal Number** (e.g., 3 for 'collect 3 items')
3. Optionally enable **Loop and Repeat** to reset after reaching goal

**Wiring pattern:**
1. Wire collectibles/events TO the Counter:
   - Treasure/Key **OnPickup** → Counter **ModifyCount**
   - Or NPC **OnDefeated** → Counter **ModifyCount**

2. Wire Counter TO what should happen:
   - Counter **OnCountReached** → Door **Open**
   - Or Counter **OnCountReached** → Level Transition **ChangeLevel**

**Video Tutorial:** https://youtu.be/Ey2FRDx7eNc

---

## How do I use a Timer Rule in Endstar?

**How to Use Timer Rule**

The Timer Rule waits for a duration, then triggers an event.

**Setup:**
1. Place a Timer Rule in your level
2. Set **Duration** in seconds (e.g., 10 for 10-second timer)
3. Enable **Loop** if you want it to repeat
4. Enable **Start Immediately** to begin when level loads

**Wiring pattern:**
1. If NOT starting immediately, wire something to START it:
   - Lever **OnFlipped** → Timer **StartTimer**
   - Or Trigger Volume **OnTriggered** → Timer **StartTimer**

2. Wire Timer TO what should happen:
   - Timer **OnTimerFinished** → NPC Spawner **TrySpawnNpc**
   - Or Timer **OnTimerFinished** → Door **Close**

**Video Tutorial:** https://youtu.be/1QuOaU2-Qyk

---

## What categories of props and rule blocks are in Endstar?

**Endstar Props & Rule Blocks by Category**

| Category | What It's For | Examples |
|----------|---------------|----------|
| 🎒 **Collectibles & Items** | Things players pick up | Treasure, Key, Resource, Weapons, Health |
| 🚪 **Doors & Barriers** | Controlling access | Door-Locked, Door-Mechanical, Level Gate |
| 🔘 **Triggers & Switches** | Player-activated controls | Lever, Pressure Plate, Trigger Volume |
| 🧠 **Logic & Flow Control** | Game rules and conditions | Bool Gate, Do Once, Relay, Sequence, Randomizer |
| ⏱️ **Timers & Counters** | Tracking time and counts | Timer Rule, Countdown Timer, Counter Rule |
| 👾 **NPCs & Combat** | Enemies, allies, fighting | Medium NPC, NPC Spawner, Traps, Damager |
| 🗣️ **NPC Behavior Nodes** | Controlling NPC actions | Dialogue Node, Move To, Idle, Wander |
| 🌍 **Environment & Effects** | World settings, visuals | Ambient Settings, Filters, Ocean Plane |
| 🚀 **Movement & Teleportation** | Moving players around | Portal, Teleporter, Bounce Pad |
| 📺 **UI & Feedback** | Showing info to players | Billboard, Floor Sign, Banner |
| 🎮 **Level & Spawn Management** | Game flow and respawning | Spawn Point, Checkpoint, Level Transition |

---

## What beginner-friendly props should I start with in Endstar?

**Beginner-Friendly Props in Endstar**

Start with these simple props that are easy to understand:

**Collectibles (⭐ Beginner):**
- **Treasure** - Simple pickup, just has OnPickup
- **Key** - Works automatically with locked doors
- **Instant Health Pickup** - Auto-heals player

**Doors (⭐ Beginner):**
- **Door - Locked** - Just assign a Key Reference, no wiring needed!
- **Door - Mechanical** - Simple open/close wiring

**Switches (⭐ Beginner):**
- **Lever** - Easy OnFlipped trigger
- **Pressure Plate** - OnPressed/OnReleased

**First project ideas:**
1. Key + Locked Door (no wiring needed)
2. Lever + Mechanical Door (one simple wire)
3. 3 Treasures + Counter + Door (learn counting)

Once comfortable, move to Timer Rule and Counter Rule!

---

## What are the different NPC classes in Endstar?

**NPC Classes in Endstar**

When setting up NPCs (Medium NPC or NPC Spawner), you can choose from these classes:

| Class | Description |
|-------|-------------|
| **Blank** | Ordinary NPC without any weapon - won't attack |
| **Grunt** | Has a sword - close-distance melee fighter |
| **Rifleman** | Has a projectile-firing weapon - ranged attacker |
| **Zombie** | Zombie-type enemy |

**Combat behavior settings:**
- **Passive** - NPC doesn't defend itself or attack
- **Defensive** - NPC responds when attacked
- **Aggressive** - NPC is always hostile and attacks on sight

---

## What does Pathfinding Range mean for NPCs in Endstar?

**NPC Pathfinding Range in Endstar**

Pathfinding Range controls how far an NPC can navigate/move:

| Range | Description |
|-------|-------------|
| **Global** | Largest range - NPC can navigate the entire level |
| **Area** | Medium range - NPC limited to a defined area |
| **Island** | Smallest range - NPC limited to immediate vicinity |

**Troubleshooting tip:** If your NPC is stuck or won't move to a destination, try changing Pathfinding Range to 'Global' to give it maximum navigation ability.

---

## How do I display a countdown timer on screen in Endstar?

**Displaying Countdown Timer on Screen**

**Props needed:** Countdown Timer, Anachronist Billboard

**Steps:**
1. Place a **Countdown Timer** and set:
   - Countdown Value (starting number in seconds)
   - Start Immediately (Yes to begin when level loads)

2. Place an **Anachronist Billboard** where you want to display the time

3. Wire them together:
   - Select Countdown Timer first (sender)
   - Select Billboard second (receiver)
   - Connect **OutputAsText** → **SetRawText**
   - Click Confirm

**Result:** The billboard will automatically update to show the remaining time!

**Video Tutorial:** https://youtu.be/1QuOaU2-Qyk

---

## How do I make items drop when enemies die in Endstar?

**Making Enemies Drop Items on Death**

**Props needed:** Medium NPC, Dynamic Item Spawner

**How it works:**
The Dynamic Item Spawner grants items to players by spawning them when an NPC dies. The item spawns at the NPC's location of death.

**Steps:**
1. Place your **Medium NPC** (the enemy)
2. Place a **Dynamic Item Spawner** and configure what item to drop
3. Wire:
   - Medium NPC **OnDefeated** → Dynamic Item Spawner (trigger spawn)

**For random loot:** Use a Randomizer Rule between the NPC death and multiple Item Spawners to create random loot drops.

**Video Tutorial:** https://youtu.be/60M_UAvr0dI

---

## How do I publish my Endstar game?

**Publishing Your Endstar Game**

You can easily publish your game to be played by everyone in the Endstar community.

**Features:**
- Share your game with the community
- Create a **deeplink** that can be shared and directly takes players to your game
- Anyone with the link can play your creation

**Video Tutorial:** Publishing and Deeplinking: https://youtu.be/FeFfRMLOOTs

---

## How do I create a lever trap that kills the player in Endstar?

**Creating a Lever Death Trap**

**Goal:** Pull lever to deal damage (can kill player or NPC)

**Props needed:** Health Modifier, Lever, Trigger Volume, Medium NPC (optional)

The video tutorial shows how to create a simple rule block combination. It demonstrates using a Health Modifier triggered with a Lever to kill the player. This type of scenario can be used as a trap.

**Bonus:** The video also shows using a Trigger Volume with an NPC to kill them when they enter a specific zone.

**Video Tutorial:** Pull Lever to Kill Character: https://youtu.be/4QoLIxX50XQ

---

## What is the difference between Portal and Teleporter in Endstar?

**Portal vs Teleporter in Endstar**

| Feature | Portal (Prop) | Teleporter (Rule Block) |
|---------|---------------|------------------------|
| **Visibility** | Visible in game | Invisible |
| **Activation** | Player walks into it | Must be triggered by wiring |
| **Cooldown** | Has cooldown setting | No cooldown |
| **Control** | Automatic | Script-controlled |
| **Best for** | Fast travel points, shortcuts | Triggered warps, traps |

**Portal** is a visible prop players walk into. It has destination points and cooldown.

**Teleporter** is an invisible rule block that teleports a target when triggered via wiring. It has success/failure events.

**There's also Teleport Point** - a hybrid with visual countdown display and range detection (Star Trek beam-up style).

**Video Tutorials:**
- Portal: https://youtu.be/QwbOPz89h6g
- Teleport Point: https://youtu.be/cUBOVIw2__o
- Teleporter: https://youtu.be/CFBrBYaVHrs

