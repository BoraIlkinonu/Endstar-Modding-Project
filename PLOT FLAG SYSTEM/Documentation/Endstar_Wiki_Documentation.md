# Endstar Wiki Documentation

> Scraped from https://endstar.wiki.gg/wiki/Special:AllPages
> Last Updated: December 2025

---

## Table of Contents

1. [Creator Mode](#creator-mode)
2. [Editor Tools](#editor-tools)
3. [Rule Blocks](#rule-blocks)
4. [NPC System](#npc-system)
5. [Available Wiki Pages Index](#available-wiki-pages-index)

---

## Creator Mode

### What is the Creator?

The Creator is the editing interface that allows players to modify individual levels within games in Endstar.

### Accessing the Creator

To enter Creator mode, players must initiate an "Edit" session for a specific level. For new content, users navigate to the "Create/Play" tab, select the "Create" subtab, and click "Create New Game" to establish a new game and accompanying level.

### Two-Phase Workflow

**Creating Phase:** Players control an avatar that can move and jump throughout the environment, enabling real-time platforming tests during development. However, interactive gameplay elements—such as doors, signs, and NPCs—remain non-functional during this phase.

**Testing Phase:** By selecting the "Play" button at the screen's top center, users transition to a testing mode where they can fully interact with all gameplay mechanics. Players return to creation mode by clicking "Stop" or (on desktop) pressing F6 while holding Alt.

### Toolset

The interface features two tool categories:

**Bottom Toolbar** includes eight creation tools: Empty, Terrain, Prop, Erase, Wiring, Inspector, Copy, and Move tools—each focused on level construction.

**Upper-Right Meta Tools** comprise three management options: Screenshot, Level Editor, and Game Editor tools for handling metadata and project organization.

---

## Editor Tools

### Overview of the 8 Editor Tools

| Tool | Keyboard | Purpose |
|------|----------|---------|
| **1. Clear/Empty Tool** | 1 | Clears active menus, lets you move without changing anything |
| **2. Terrain Tool** | 2 | Select and paint terrain tiles to build levels |
| **3. Prop Tool** | 3 | Place interactive objects (doors, NPCs, rule blocks) |
| **4. Eraser Tool** | 4 | Remove props and terrain (configurable filters) |
| **5. Wiring Tool** | 5 | Connect props to create game logic |
| **6. Inspector Tool** | 6 | View and change properties of placed objects |
| **7. Copy Tool** | 7 | Duplicate props (includes wiring) |
| **8. Move Tool** | 8 | Move and rotate placed props |

---

### Wiring Tool

The Wiring Tool is described as "one of the most powerful tools in all of Endstar." It enables creators to connect two props together to execute custom gameplay without requiring any scripting.

#### Basic Usage Steps

The tool follows a four-step process:

1. **Select Source Prop**: Choose a prop that can send an event, such as a lever that triggers when flipped.

2. **Select Destination Prop**: Pick a prop capable of receiving events, like a door that opens when signaled.

3. **Connect the Wire**: Left-click the desired event on the source prop and the corresponding receiver on the destination prop, then confirm the connection.

4. **Visual Connection**: The wired connection appears visually in the game world between the two props.

#### Wire Parameters

Some connections support parameter passing. The documentation provides a Boolean value example: users can check a box to send a True value or leave it unchecked to send False.

#### Removing Wires

To delete a connection, left-click the wire itself in the game world to select both connected props. Then left-click the wire between the panels and select the Delete button in the confirmation popup.

---

### Terrain Tool

The Terrain Tool allows you to "place sections of terrain tiles on a grid." All terrain is "smart terrain" and automatically deforms based on neighboring blocks while spawning decorative elements like flowers and rocks.

#### Basic Usage

**Accessing the Tool:**
Selecting the Terrain Tool opens the Tilesets panel where you can choose which terrain to work with.

**Painting Process:**
1. Pick a tileset from the Tilesets panel
2. Hover over existing terrain where you want placement
3. Left-click to place individual tiles

**Area Filling:**
You can also drag the mouse to fill larger regions. Use the Q and E keys to cycle between three fill shapes: Horizontal Plane, Vertical Long Plane, and Vertical Facing Plane. Release the mouse to complete the fill.

#### Advanced Tips

**Selecting Existing Tilesets:**
Hold ALT and left-click any terrain already on the map to quickly select that tileset for painting.

**Adding New Tiles:**
Click the plus button in the Tilesets Panel to browse available terrain assets. Select an asset you like, view its details, then click ADD to incorporate it into your game.

---

### Inspector Tool

The Inspector Tool in Creator mode allows you to modify object properties in scenes. As stated in the documentation, "The Creator modes Inspector tool allows you to change the properties of an object in the scene."

#### How to Use It

**Basic Steps:**
1. Select the Wiring Tool and follow the on-screen prompts
2. Left-click any prop to open a properties panel
3. Edit available fields for that object

**Example:** The documentation illustrates this with a door that has a "Locked" property—checking this box locks the door at game start.

#### Key Features

**Property Tooltips:** Some properties are underlined, indicating they contain helpful tooltips. Hovering over underlined text reveals additional guidance.

**Dropdown Menus:** The tool includes dropdown properties with descriptions to help creators "comprehend" the expected behavior of each option.

---

### Erase Tool

The Erase Tool is a Creator mode feature that removes props and terrain from game levels with customizable filtering options.

#### Filter Options

- "Terrain allows terrain tiles to be erased when checked"
- "Props allows prop objects to be erased when checked"
- "Include Wired Props allows props with wired connections to be erased when checked"

#### Usage Instructions

**Visual Indicators:**
- Red cube = object can be erased
- Black cube = object is blocked from erasure due to filter settings

**Erasing Single Terrain Tiles:**
Hover over a tile to select it, then left-click to remove it.

**Erasing Multiple Terrain Tiles:**
Left-click and drag to select a range. Press Q or E to change selection direction (horizontal, vertical facing, or vertical long).

**Erasing Props:**
Navigate to the prop's location, hover to select it (turns red if erasable), then left-click to delete. If selection turns black, adjust the Erase Panel filters—particularly enabling "Include Wired Props" for connected objects.

---

### Level Editor Tool

The Level Editor Tool is a Creator mode feature that enables control over level aspects including name, description, screenshots, and player spawn mechanics.

#### Key Features

**Level Info Tab:**
Allows editing of "the name and description of the level" and arranging "level images (acquired using the screenshot tool) by dragging them into the desired order." Images can be removed by dragging them onto a red X icon.

**Collaborators Tab:**
Enables adding or editing level collaborators. Important constraint: collaborators must first be added to the game's collaborator list via the Game Editor Tool before they can be assigned to individual levels.

**Spawn Point Order Tab:**
Manages multiple basic spawn points where player characters appear when gameplay begins. This tab allows adjustment of spawn point ordering and enables/disables individual points.

**Revisions Tab:**
Displays all level changes with reversion capability. The tool maintains full history—reverting to a previous state creates a new revision rather than erasing records.

---

### Game Editor Tool

The Game Editor Tool allows creators to manage their entire game within Endstar's Creator mode, handling level management, collaborations, publishing, and asset library integration.

#### Key Features

**Info Tab:**
Creators can "edit the name and description of the game as well as arrange the order of game levels by dragging them to the desired order." Users select screenshots from existing level captures to display in the main menu.

**Levels Tab:**
This section enables creation of new levels and organization of their sequence. A popup window appears when adding levels, allowing creators to name them and choose a starting level template.

**Collaborators Tab:**
Manage team members by adding, editing, or removing contributors. The role will be inherited for all levels in your game. Individual level permissions can be customized separately through the Level Editor Tool.

**Publish Tab:**
Once ready, creators can publish versions publicly through the "Play" tab. Updates made after publishing won't be visible until a new version is published.

**Game Library Tab:**
Browse and integrate assets from a shared library, including both official Endstar assets and community-created content.

---

### Prop Tool

The Prop Tool allows you to place "props"—non-terrain objects that often have in-game functionality—into your scenes.

#### Main Capability
"The Prop tool allows you to paint 'props' into the scene." You can also add new props by searching published options or creating custom ones like Rule Blocks or NPC Nodes.

**Scripting:** Props with sufficient permissions include an "Edit Script" button for adding Lua code, wiring events, or inspector fields.

#### Basic Usage Steps

1. **Select a Prop:** Choose from the Prop panel that opens when you activate the tool
2. **Choose Location:** Hover over terrain to find a valid placement spot
3. **Place It:** Left-click to place the selected prop

**Pro Tip:** "To select a prop that is already in the game world, hold the ALT key and Left-Click the existing desired prop."

---

### Copy Tool

The Copy Tool allows duplicating objects already placed in a scene. It automatically copies attached wires and maintains a limited history of previously selected objects for quick access.

#### Usage Instructions

**Initial Copying:**
1. Left-click the prop to select it as the source
2. Hover over your destination to preview placement
3. Left-click to place the copy
4. Rotate during placement by left-clicking and dragging before release

**Switching Objects:**
- Click "Select More" button in the Copy Panel, or right-click to deselect the current prop
- Choose a different prop and repeat the copying process

**Reusing Previous Props:**
- Check the desired prop from the Copy Panel's history list
- The previously selected prop becomes active for additional copies

---

### Move Tool

The Move Tool enables users to relocate props to new positions within the game environment.

#### How to Use

**Step 1: Select the Source Prop**
"Left-Click the prop to be moved." This identifies which object you want to relocate.

**Step 2: Choose Destination Location**
"Hover over the terrain, or other props, to preview the outcome of the move operation." Then execute a left-click to finalize placement.

**Step 3: Rotate During Placement (Optional)**
"Left-Click and Drag to adjust the orientation of the prop," then release the mouse button to confirm final rotation and position.

---

### Empty Tool

The Empty Tool allows users to navigate the environment without any active menus or tools. It "doesn't have any features" but provides a clean state where "clicking wont accidentally change anything."

"Selecting the Empty Tool will close all tools." This makes it the quickest way to deactivate all other tools and return to a neutral state.

---

### Screenshot Tool

The Screenshot Tool allows you to capture up to 9 screenshots per level.

#### Taking Screenshots
1. Select the Screenshot Tool to open the screenshot UI
2. Take multiple photos as needed
3. Click the X button to proceed to the management panel

#### Managing Your Captures
After taking screenshots, a review panel appears where you can decide which images to keep or remove. You have the option to download individual screenshots before finalizing your selection. Click Done to save the selected images.

---

## Rule Blocks

### Definition

"A Rule Block is a modular piece of logic" that serves as a foundational building component for game structure and interactions. These are Lua scripts designed for purely logical operations like storing state or processing data.

### Core Purpose

Rule Blocks enable code reuse across multiple configurations. Rather than embedding logic directly into a prop's script, developers can create a reusable Rule Block that applies the same behavior with different settings.

### Common Applications

Rule Blocks handle various gameplay mechanics:
- Countdown timers with environmental changes
- Score tracking systems
- Item distribution to players
- Enemy wave management and NPC tracking
- Dynamic level hazards

---

### Counter Rule Block

The Counter is a rule block designed to track numerical values. It functions as "A rule block that keeps track of a number."

#### Inspector Fields

| Field | Type | Description |
|-------|------|-------------|
| Goal Number | Int | At what number should this fire its On Goal Reached event? Values greater than this threshold also fire the event. |
| Loop And Repeat | Bool | After reaching its goal, should it reset to 0 and wait until the goal is reached again? |

#### Wiring Options

**Event Output:**
- **On Count Reached** - Activates when the counter reaches or surpasses the goal value. The counter requires resetting before retriggering. It passes context from the object that initiated the count modification.

**Receivers (Inputs):**
- **Modify Count** - Accepts an integer delta to increment or decrement the current count
- **Set Goal** - Updates the target goal value via integer parameter
- **Reset And Clear Counter** - Returns count to zero and resets completion status

---

### Timer Rule Block

The Timer is a mechanism that triggers an event after a specified delay period.

#### Inspector Fields

| Field | Type | Description |
|-------|------|-------------|
| Duration | Float | Determines how long the timer runs |
| Loop | Bool | Enables automatic restart upon completion |
| Start Immediately | Bool | Begins timing when the level loads |

#### Wiring Options

**Event Output:**
- **On Timer Finished** - Activates when countdown completes, passing along the context from the Start Timer receiver

**Receivers (Inputs):**
- **Start Timer** - Initiates the countdown with optional restart parameter
- **Stop Timer** - Halts the current timer operation

---

### Bool Gate Rule Block

The Bool Gate is a rule block component that "holds a true/false state and executes either success or failure depending" on that state.

#### Inspector Fields

| Field | Type | Description |
|-------|------|-------------|
| Initial State | Bool | Determines the gate's starting true/false value |

#### Wiring Options

**Events (Outputs):**
- **On True** - Triggers when EvaluateGate is called and the gate state is True
- **On False** - Triggers when EvaluateGate is called and the gate state is False

**Receivers (Inputs):**
- **Set Gate State** - Accepts a Bool value to update the gate's current state
- **Evaluate Gate** - Fires either OnTrue or OnFalse based on the current state
- **Flip Gate State** - Inverts the state (True becomes False, and vice versa)

---

### Sequencer Rule Block

The Sequencer is a rule block that "fires events in order when told to." It functions as an event manager for sequential execution.

#### Inspector Fields

| Field | Type | Description |
|-------|------|-------------|
| Max Sequence | Integer | Events above this value won't fire |
| Loop | Boolean | Determines whether the sequence restarts upon completion |

#### Wiring Options

**Input Receiver:**
- **Fire Next Event** - Triggers the next event in the sequence

**Output Events (On Result 1-10):**
Ten distinct output events that activate based on the sequence progression. Each fires "when FireNextEvent is called if it is this event's turn in the sequence."

---

### Randomizer Rule Block

The Randomizer is a rule block used to trigger random events. You can "Wire to this block to fire random events."

#### Inspector Fields

| Field | Type | Description |
|-------|------|-------------|
| Max Result | Integer | Sets the upper limit for random selection; events exceeding this value will not be triggered |

#### Wiring Options

**Event Outputs:**
- **On Result 1 through On Result 10** - Each fires when randomly selected after the FireRandomEvent receiver is activated

**Input Receiver:**
- **Fire Random Event** - Activates the randomization process

---

### Event Relay Rule Block

The Event Relay functions as a simple event relay mechanism. It's "A simple block that fires an event when it receives an event."

#### Purpose

This block is particularly useful for scenarios where multiple input wires need to trigger the same sequence of output wires, serving as a consolidation point for event routing.

#### Wiring Options

**Events (Outputs):**
- **On Event Received** - Activates whenever the Execute Event receiver is triggered

**Receivers (Inputs):**
- **Execute Event** - The input that triggers the On Event Received event emission

---

### Do Once Rule Block

The Do Once is a Rule Block that "will fire an event, only if it has not done so before."

#### Wiring Options

**Events:**
- **On Triggered** - Activates the first time the Trigger receiver is called. The event cannot fire again until Reset is invoked. It passes along the context provided to the Trigger receiver.

**Receivers:**
- **Trigger** - Initiates the OnTriggered event if it hasn't previously fired
- **Reset** - Restores the block's ability to trigger again

This block's functionality can be reset through wiring, allowing it to fire multiple times across different cycles when Reset is called between triggers.

---

### Do Once Per Context Rule Block

"Filters events to ensure it only fires an event once for each incoming context" - this is a Rule Block component designed to manage event firing based on context uniqueness.

#### Wiring Options

**Events (Outputs):**
- **On Filter Success** - Activates when `FireFilteredEvent` executes and the provided context hasn't been processed previously
- **On Filter Failed** - Activates when `FireFilteredEvent` executes but the context was already processed before

**Receivers (Inputs):**
- **Fire Filtered Event** - Evaluates incoming context and routes it to either success or failure event based on prior occurrence
- **Reset Context** - Clears the stored context history, enabling previously-filtered contexts to trigger events again

This component prevents duplicate event triggering by tracking which contexts have already fired.

---

## NPC System

### NPC Node Overview

An NPC Node is "a way to interact with any NPC generically without having to code a fully custom NPC," allowing developers to create varied NPC behaviors through modular components rather than custom scripts.

### Three Node Types

**Command Nodes:** Execute immediate actions with optional combat engagement. Examples include moving to locations while either engaging or ignoring enemies en route.

**Interaction Nodes:** Activate when players engage NPCs. They can trigger responses like "When Interacted with, run away!" or initiate dialogue sequences.

**Behavior Nodes:** Define NPC downtime activities when not executing commands or fighting—standing idle, wandering, or fidgeting.

**Active Limitation:** NPCs can have one active command, interaction, and behavior node simultaneously (three total).

### Example Scenario

The wiki illustrates a sequence: NPC runs to hilltop, waves, moves to gate, waits for player dialogue, opens gate, then transitions to wandering. This combines: MoveTo → PlayAnimation → MoveTo → Dialogue → UseInteractable → Wander.

### Implementation Requirements

Custom nodes must include these functions:
- `ConfigureNpc()` - assigns node to NPC
- `GetWorldState()` - defines desired outcome state
- `Priority()` - rates goal importance (0-100 scale)
- `Activate()` - executes when goal becomes active
- `Deactivate()` - cleanup when goal ends

### Expansion Capability

The system uses GOAP (Goal Oriented Action Planning) internally, allowing developers to create custom nodes within the AI system's current capabilities.

---

## Available Wiki Pages Index

Based on the wiki's "All Pages" directory, here are the available pages organized by category:

### Game Elements & Assets
- All Item Remover
- All Player Broadcaster
- Ambient Settings
- Asset Browser
- Billboard
- Blender
- Building a Prop
- Collaborator
- Collaborators

### Editor Tools
- Copy Tool
- Create Menu
- Creator
- Erase Tool
- Empty Tool
- Game Editor Tool
- Game Library
- Game Tool
- Inspector Tool
- Level Editor Tool
- Level Tool
- Move Tool
- Prop Tool
- Screenshot Tool
- Terrain Tool
- Wiring Tool

### Components & Systems
- Component: Dynamic Navigation
- Component: Dynamic Visuals
- Component: Health
- Component: Hittable
- Component: Interactable
- Component: Lockable
- Component: Periodic Effector
- Component: Projectile Shooter
- Component: Sensor
- Component: Targeter
- Component: Team
- Component: Text
- Component: Text Bubble
- Component: Trigger
- Component: Visuals

### NPCs & Behavior
- NPC Node
- NPC Spawner
- NPC Idle
- NPC Dialogue
- NPC Group Dialogue
- NPC Group Idle
- NPC Group Move To
- NPC Group Rove
- NPC Move To Position
- NPC Rove
- NPC Stand
- NPC Wander

### Logic & Mechanics (Rule Blocks)
- Bool Gate
- Command Node
- Counter
- Do Once
- Do Once Per Context
- Event Relay
- Randomizer
- Rule Block
- Sequencer
- Timer

### World Objects
- Doors, gates, pressure plates
- Teleporters
- Traps
- Lighting
- Various environmental props (100+ individual item entries)

### Installation & Guides
- Installation Guide
- Endstar SDK
- Endstar SDK Install
- Git Install For Windows
- Unity HUB Install
- How To: Create a Game
- Guide: Managing Large Projects

---

## Default Rule Blocks Summary

| Rule Block | Purpose |
|------------|---------|
| **Counter** | Modifies an internal counter and fires an event when reaching a goal number |
| **Level Transition** | Loads players into another level when triggered |
| **Randomizer** | Randomly selects one event from multiple options to broadcast |
| **Sequence** | Fires the next event in a predetermined order with each trigger |
| **Timer** | Executes an event after a specified delay, with looping capability |
| **Bool Gate** | Holds a true/false state and executes either success or failure |
| **Event Relay** | Simple pass-through that fires an event when receiving an event |
| **Do Once** | Ensures something only happens one time |

---

## Creating Custom Rule Blocks

Users create Rule Blocks by:
1. Adding Inspector attributes (text fields, events, receivers)
2. Writing Lua code with functions matching receiver names
3. Using `FireEvent()` to broadcast events
4. Accessing Inspector attributes as Lua variables
5. Saving and adding to game levels

### Debugging

Log messages are viewable through the Rule editor's inspection icon after running a level.

---

## Props Reference

### All Item Remover

The All Item Remover is a Rule Block component that enables removal of items from players in a game. "Allows you to remove all the items from a specific player, or from all players in a game. Great for resetting inventories between levels!"

#### Inspector Fields

| Field | Type | Description |
|-------|------|-------------|
| Player Target | Player Reference | Specifies which player should have items removed |
| Remove Inventory | Bool | Controls whether to remove held items |
| Remove Resources | Bool | Controls whether to remove resources like coins |

#### Receivers

- **Remove Items** - Removes all items from the designated PlayerTarget; if set to 'Use Context,' removes items from the passed context player
- **Remove Items For All Players** - Removes items from every player in the game

---

### All Player Broadcaster

The All Player Broadcaster is a rule block component that "emits an event for each player when told to do so."

#### Wiring Options

**Events (Outputs):**
- **On Player Broadcast** - Activates whenever the Broadcast Players receiver is triggered, firing individually for each player in the game
- **On Player Count Broadcast** - Activates when Broadcast Player Count is called, passing the prop's context and player count as an integer value

**Receivers (Inputs):**
- **Broadcast Players** - Triggers the On Player Broadcast event to fire for all active players
- **Broadcast Player Count** - Triggers the On Player Count Broadcast event

---

### Billboard

The Billboard is a basic prop object that displays text content. "A billboard that displays text. Inspect the object to change its text. Wire to the object to change its text at runtime."

**Base Type:** Basic Prop

**Components:** Text, Hittable

#### Inspector Fields

| Field | Type | Description |
|-------|------|-------------|
| Text | Localized String | Allows customization of displayed content |

#### Wiring Options

**Receivers (Input):**
- **Set Text** - Accepts a localized string parameter to update the displayed message
- **Set Raw Text** - Accepts a plain string parameter for text updates

---

### Ambient Settings

This object allows players to modify skyboxes and environmental settings within a game world. "Lets you swap between some skyboxes and environment settings."

#### Inspector Properties

| Field | Type | Description |
|-------|------|-------------|
| Is Default | Boolean | Whether this is the default ambient setting |
| Current Skybox | Skybox | Determines which skybox displays |

#### Wiring Options

**Receivers:**
- **Change Skybox** - Accepts a new skybox parameter, enabling real-time environmental modifications through connected systems

---

### Trigger Volume

"A invisible resizable volume that triggers when entered by players, NPCs or Physics objects!"

**Components:** Trigger Component, Resizable Volume

#### Inspector Fields

| Field | Type | Description |
|-------|------|-------------|
| Context Filter | Filter | Specifies which context types can activate the volume |
| Only Once | Bool | Fire events solely on first valid entry |
| Forward, Backward, Left, Right, Up, Down | Int | Zone size in each direction (units) |
| Offset | Vector3 | Position offset for volume root |

#### Wiring Options

**Events:**
- **On Triggered** - Activates when compatible objects enter, passing the triggering object's context
- **On Exited** - Fires when qualifying objects leave the volume

---

### Door

"A two block wide door" that can be opened, closed, locked, and controlled via wiring. NPCs cannot open locked doors but may attempt to if permitted.

**Components:** Interactable, Lockable, Navigation

#### Inspector Fields

| Field | Type | Description |
|-------|------|-------------|
| NPC Door Interaction | Dropdown | Determines whether NPCs can interact with the door |
| Key Reference | Key | Specifies which key unlocks the door |

#### Wiring Options

**Events:**
- **On Interact Failed** - Triggered when a locked door blocks entry
- **On Door Opened** - Fired after opening completes
- **On Door Closed** - Fired after closing completes

**Receivers:**
- **Open** - Opens closed doors (blocked by locks)
- **Close** - Closes open doors
- **Toggle Open** - Switches door state (ignores locks)
- **Unlock** - Removes the lock

---

### Lever

"Strange emerald crystals pulse with energy" - allows players to interact to change its position.

**Components:** Interactable, Visuals

#### Inspector Fields

| Field | Type | Description |
|-------|------|-------------|
| Start In Position 2 | Boolean | Determines if lever begins in second position |

#### Wiring Options

**Events:**
- **On Flipped** - Activates whenever the lever changes position
- **On Flipped Position 1** - Fires specifically when moved to position 1
- **On Flipped Position 2** - Fires specifically when moved to position 2

**Receivers:**
- **Flip Lever** - Toggles to the opposite position
- **Set To Position 1** - Moves to position 1
- **Set To Position 2** - Moves to position 2

---

### Pressure Plate

"A metal pressure plate. Wire to this pressure plate to receive events when it is depressed or released."

**Components:** Trigger Component, Visuals

#### Wiring Options

**Events:**
- **On Pressed** - Fires whenever an object depresses the pressure plate
- **On Released** - Fires whenever no object is depressing the pressure plate

---

### Teleporter

"Teleport things in the level!" - functions through wiring to transport characters.

#### Inspector Fields

| Field | Type | Description |
|-------|------|-------------|
| Teleport Target | Instance Reference | Accepts context to teleport |
| Destination | Cell Reference | Specifies teleportation destination |
| Teleport Effect | Teleport Type | Visual/mechanical style of teleportation |

#### Wiring Options

**Events:**
- **On Teleport Success** - Activates when object teleports successfully
- **On Teleport Failure** - Activates when teleportation fails

**Receivers:**
- **Trigger Teleport** - Initiates teleportation of target to destination

---

### Bounce Pad

"Whether it's arcane energies, or technology, when you want to go up, fast... you use a bounce pad."

#### Inspector Fields

| Field | Type | Description |
|-------|------|-------------|
| Start Active | Boolean | Whether pad starts active |
| Starting Bounce Height | Integer | Initial bounce height value |

#### Wiring Options

**Events:**
- **On Entity Bounced** - Triggers when an object uses the pad

**Receivers:**
- **Activate** - Turns the pad on
- **Deactivate** - Turns the pad off
- **Toggle Active** - Switches active state
- **Set Bounce Height** - Sets exact height (integer)
- **Modify Bounce Height** - Adjusts current height (float)

---

### Key

A special collectible that can unlock locked doors.

#### Wiring Options

**Events:**
- **On Pickup** - Fires the first time this object is picked up

---

### Treasure

A collectible item players can pick up. Great for coins, gems, or any reward.

#### Wiring Options

**Events:**
- **On Pickup** - Activates when the object is picked up for the first time

---

## SDK & Development

### Blender Integration

Endstar Studios selected Blender specifically because "it is free for everyone to use." This choice enables them to create all their content within the software and share source files with their community without requiring expensive third-party software licenses.

---

### Building a Prop in Endstar SDK

"When creating a prop in the SDK a user first chooses from one of the available BaseTypes, and then selects any number of Components" to build interactive or static objects for the Endstar game.

#### Key Steps

1. **Select a BaseType** - Choose from specialized pre-built prop types offering different functionality levels
2. **Add Components** - Layer in optional behaviors like health, interactability, or visual effects
3. **Configure Visuals** - Set up models, textures, materials, and appearance
4. **Run Validation** - Automated tests ensure proper setup
5. **Export to Cloud** - Make the prop available for use in levels or community sharing

#### Available BaseTypes

**Simple Props:**
- Static Prop (no components, visual-only objects)
- Basic Prop (empty interactive base)
- Basic Spawn Point

**Specialized Types:**
- Bounce Pad, Door, Instant Pickup, Draggable Physics Cube, Sentry, Spike Trap, Basic Level Gate

#### Key Components

Props can include: visual dynamics, health systems, hittable behaviors, lockable mechanics, projectile shooting, sensors, interactables, targeting systems, team allegiances, text displays, and trigger colliders.

---

### Command Node Functions

The default Command Node includes several key functions:

| Function | Description |
|----------|-------------|
| `Start()` | Initializes the command when the OnStart parameter is enabled |
| `ConfigureNpc(context)` | Executes when the command is assigned to an NPC |
| `Priority(context, goalNumber)` | Returns a float value (clamped between 40 and 70) for command importance |
| `GetWorldState(goalNumber)` | Returns the desired WorldState for a goal |
| `Activate(context, goalNumber)` | Triggered when an NPC activates the command |
| `Deactivate(context, goalNumber)` | Triggered when an NPC deactivates the command |

---

## Collaborator System

### Permission Hierarchy

Permission levels are cumulative, with each tier gaining abilities from ranks below it:

| Role | Capabilities |
|------|-------------|
| **Owner** (Highest) | Complete control over assets; Can add/remove Viewers, Editors, and Publishers; Cannot be revoked |
| **Publisher** | Modifies assets and publishes them; Adds or removes Viewers and Editors |
| **Editor** | Makes asset changes; Adds or removes Viewers only |
| **Viewer** (Lowest) | Opens and uses assets without editing; Cannot invite collaborators |

**Note:** For Props/Terrain, Viewers can add to Game Libraries. For Games, Viewers can join Creator sessions without editing. Viewers can still edit Levels if they have separate Level permissions.

---

*Document compiled from Endstar Wiki (https://endstar.wiki.gg)*
*Last Updated: December 2025*
