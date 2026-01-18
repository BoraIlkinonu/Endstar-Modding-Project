# Endstar LUA API Documentation

> Scraped from https://endstar-api-docs.endlessstudios.com/
> Generated with Doxygen 1.11.0
> Last Updated: December 2025

---

## Table of Contents

1. [Namespaces Overview](#namespaces-overview)
2. [LuaInterfaces Classes](#luainterfaces-classes)
3. [Scripting Classes](#scripting-classes)
4. [Class Index](#class-index)
5. [Methods Reference](#methods-reference)

---

## Namespaces Overview

The Endstar LUA API is organized into three main namespaces:

| Namespace | Purpose |
|-----------|---------|
| **Endless.Gameplay** | Core gameplay references (AudioReference, CellReference, PlayerReference, etc.) |
| **Endless.Gameplay.LuaInterfaces** | Interactive game elements (Player, NPC, weapons, items, mechanics) |
| **Endless.Gameplay.Scripting** | Scripting utilities (Game, Context, Color, Vector3, managers) |
| **Runtime.Gameplay.LuaClasses** | Runtime utilities (LocalizedStringFactory) |

---

## LuaInterfaces Classes

### BasicLevelGate

"Basic Level Gate contains several methods for triggering and handling a player's transition to another level."

**Key Methods:**
- `TriggerGate()` - Triggers level transition
- `ToggleReadyParticles()` - Toggles ready visual effects

---

### Behavior

"Used to create behavior nodes that can modify an NPC's idle behavior"

---

### BouncePad

"Base Type that applies a physics force to objects that overlap it"

**Key Methods:**
- `Activate()` - Enables the bounce pad
- `Deactivate()` - Disables the bounce pad
- `GetActive()` - Returns current active state
- `GetBounceHeight()` - Returns bounce height value
- `SetBounceHeight()` - Sets bounce height value

---

### Camera

Provides camera functionality for cutscenes with follow target and look-at capabilities.

**Key Methods:**
- `SetCameraPitch()` - Sets camera pitch angle
- `SetCameraYaw()` - Sets camera yaw angle
- `SetFieldOfView()` - Sets camera field of view
- `EnterShot()` - Enters a camera shot
- `EnterShotManaged()` - Enters a managed camera shot
- `ExitShot()` - Exits a camera shot
- `ExitShotManaged()` - Exits a managed camera shot

---

### CameraFadeManager

Manages camera fade effects for transitions.

**Key Methods:**
- `FadeAllIn()` - Fades in for all players
- `FadeAllOut()` - Fades out for all players
- `FadePlayerIn()` - Fades in for specific player
- `FadePlayerOut()` - Fades out for specific player

---

### ConsumableHealing

"Consumable Inventory Item base type that provides a heal when used"

**Constructor:**
- `ConsumableHealing()` - Creates a new consumable healing item

---

### DashPack

"Item base type that thrusts the character in a direction when used"

---

### Door

Handles open/closed state and rotating one or two physical doors.

**Key Methods:**
- `Open()` - Opens the door
- `OpenFromUser()` - Opens door from user interaction
- `Close()` - Closes the door
- `ToggleOpenFromUser()` - Toggles door state from user interaction

---

### Effector

"Periodically applies some effect to all the contexts in its effect list, such as damage or healing"

**Key Methods:**
- `Activate()` - Enables the effector
- `Deactivate()` - Disables the effector
- `AddContext()` - Adds a context to effect list

---

### Health

Tracks object health; can auto-destroy at zero health; processes damage/healing through related components.

**Key Methods:**
- `ChangeHealth()` - Modifies current health value
- `ChangeMaxHealth()` - Modifies maximum health value
- `GetIsDamageable()` - Returns if entity can be damaged
- `GetIsDowned()` - Returns if entity is downed
- `GetIsFullHealth()` - Returns if at full health

**Events:**
- `OnDefeated` - Fires when entity health reaches zero
- `OnHealthChanged` - Fires when health value changes

---

### Hittable

"This component makes something able to be targeted by attacks from the player, NPCs, and other objects."

**Key Methods:**
- `ChangeThreatLevel()` - Modifies threat level
- `GetIsTargetable()` - Returns if entity can be targeted

---

### Interactable

Allows players to interact with objects via the interact key.

**Key Methods:**
- `SetAnchorOverride()` - Sets interaction anchor position
- `ClearAnchorOverride()` - Clears anchor override
- `StopInteraction()` - Stops current interaction

---

### InstantPickup

Handles instant pickup items.

**Key Methods:**
- `ForceCollect()` - Forces collection of pickup

---

### MeleeWeapon

"Inventory Item base type for melee weapons"

---

### Npc

Contains methods to modify NPCs during gameplay.

**Key Methods:**
- `ClearDestination()` - Clears NPC destination
- `SetDestination()` - Sets NPC destination
- `SetDestinationToCell()` - Sets destination to specific cell
- `SetDestinationToPosition()` - Sets destination to position
- `GetNpcPosition()` - Returns NPC position
- `GiveInstruction()` - Gives instruction to NPC
- `GiveGroupInstruction()` - Gives instruction to NPC group

---

### NpcConfiguration

Configuration class for NPC properties.

**Properties:**
- `CombatMode` - NPC combat behavior setting
- `DamageMode` - NPC damage handling setting
- `Team` - NPC team assignment

**Methods:**
- `SetCombatMode()` - Sets combat mode
- `SetDamageMode()` - Sets damage mode

---

### NpcManager

Manages NPC instances and configurations.

**Key Methods:**
- `SpawnNpc()` - Spawns a new NPC
- `CopyNpcConfiguration()` - Copies NPC configuration
- `CreateNewConfiguration()` - Creates new NPC configuration
- `GetNpc()` - Gets NPC by reference
- `GetNpcInGroupByIndex()` - Gets NPC in group by index
- `GetNpcReference()` - Gets NPC reference

---

### Player

Provides access to player-specific functionality like inventory.

**Key Methods:**
- `AttemptGiveItem()` - Attempts to give item to player
- `ClearAllItems()` - Clears player inventory
- `ConsumeItem()` - Consumes an item from inventory

---

### PhysicsComponent

Handles physics interactions.

**Key Methods:**
- `AddImpulse()` - Adds physics impulse to object

---

### ProjectileShooter

Component for shooting projectiles.

**Key Methods:**
- `Shoot()` - Fires a projectile

---

### RangedWeapon

"Inventory Item base type for ranged weapons"

---

### ResourcePickup

"This base type is similar to instant pickup, but is directly tied to a resource."

---

### ResourceManager

Manages game resources.

**Key Methods:**
- `ClearAllResources()` - Clears all resources
- `ClearAllResourcesForPlayer()` - Clears resources for specific player
- `ClearResource()` - Clears specific resource
- `ClearResourceForPlayer()` - Clears resource for player

---

### Sentry

Automated turret system.

**Key Methods:**
- `Shoot()` - Fires the sentry weapon
- `DisableTrackingLaser()` - Disables the tracking laser

---

### SpikeTrap

Trap that damages entities.

**Key Methods:**
- `Activate()` - Enables the trap
- `Deactivate()` - Disables the trap
- `Trigger()` - Manually triggers the trap
- `GetActive()` - Returns active state

---

### Text

"Allows the display and updating of 3D text in the game world."

**Key Methods:**
- `DisplayText()` - Displays text content
- `GetRawText()` - Gets raw text content
- `GetLocalizedText()` - Gets localized text
- `GetTextMeshString()` - Gets text mesh string
- `SetColor()` - Sets text color
- `SetFontSize()` - Sets font size
- `SetLineSpacing()` - Sets line spacing
- `SetAlpha()` - Sets text transparency
- `SetAutoSizingEnabled()` - Enables/disables auto sizing
- `SetAutoSizingMaximum()` - Sets maximum auto size
- `SetAutoSizingMinimum()` - Sets minimum auto size

---

### TextBubble

Displays text bubbles above objects.

**Key Methods:**
- `Display()` - Shows the text bubble
- `DisplayForTarget()` - Shows bubble for specific target
- `DisplayForTargetWithDuration()` - Shows bubble with duration
- `DisplayWithDuration()` - Shows bubble with timed duration
- `Close()` - Closes the text bubble
- `CloseForTarget()` - Closes bubble for target
- `ShakeForAll()` - Shakes bubble for all players
- `ShakeForTarget()` - Shakes bubble for target
- `ShowAlert()` - Shows alert
- `ShowAlertForTarget()` - Shows alert for target

**Properties:**
- `ShakeDuration` - Duration of shake effect

---

### ThrownBomb

Throwable explosive item.

**Key Methods:**
- `SetCenterBlastForce()` - Sets center blast force
- `SetCenterRadius()` - Sets center blast radius
- `SetDamageAtCenter()` - Sets damage at blast center
- `SetDamageAtEdge()` - Sets damage at blast edge
- `SetTotalBlastRadius()` - Sets total blast radius
- `GetCenterBlastForce()` - Gets center blast force
- `GetCenterRadius()` - Gets center radius
- `GetDamageAtCenter()` - Gets center damage
- `GetDamageAtEdge()` - Gets edge damage

---

### TradeInfo

Handles trade/shop information.

**Key Methods:**
- `AttemptTrade()` - Attempts to execute trade
- `CanPlayerAfford()` - Checks if player can afford item
- `CanPlayerMakeTrade()` - Checks if trade is possible

**Properties:**
- `Cost1` - First cost item
- `Cost2` - Second cost item

---

### TriggerVolume

Handles filtering and firing events when dynamic objects overlap.

---

### Visuals

"This class allows you to change visual aspects of an object, like rotation, color, etc"

**Key Methods:**
- `SetAlbedoColor()` - Sets albedo color
- `SetEnabled()` - Enables/disables visuals
- `SetLocalPosition()` - Sets local position
- `SetLocalRotation()` - Sets local rotation
- `StopContinuousRotation()` - Stops continuous rotation

---

## Scripting Classes

### Context

"Every object in the world that has a script has a context...A context is the generic reference to an object."

**Key Methods:**
- `GetBool()` - Gets boolean value
- `GetFloat()` - Gets float value
- `GetInt()` - Gets integer value
- `GetString()` - Gets string value
- `SetBool()` - Sets boolean value
- `SetFloat()` - Sets float value
- `SetInt()` - Sets integer value
- `SetString()` - Sets string value
- `SetTable()` - Sets table value
- `GetContext()` - Gets context reference
- `TryGetComponent()` - Attempts to get component
- `ToString()` - Converts to string

**Events:**
- `OnBoolSet` - Fires when boolean is set
- `OnFloatSet` - Fires when float is set
- `OnIntSet` - Fires when integer is set
- `OnStringSet` - Fires when string is set
- `OnTableSet` - Fires when table is set

**Properties:**
- `GameContext` - Reference to game context

---

### Game

Provides access to current session information including players and level data.

**Key Methods:**
- `Game()` - Constructor
- `GetGameContext()` - Gets the game context
- `GetGameDescription()` - Gets game description
- `GetGameTitle()` - Gets game title
- `GetCurrentLevelContext()` - Gets current level context
- `Teleport()` - Teleports entity

**Events:**
- `OnPlayerCountChanged` - Fires when player count changes
- `OnPlayerJoined` - Fires when player joins
- `OnPlayerLeft` - Fires when player leaves

---

### GameEndBlock

Handles game ending logic.

**Key Methods:**
- `GameEndBlock()` - Constructor
- `TriggerEndGame()` - Triggers game end

---

### Vector3

"A utility class for generating Vector3s...Often used for position or rotation."

**Key Methods:**
- `Create()` - Creates a new Vector3
- `Add()` - Adds two vectors
- `Scale()` - Scales a vector
- `Distance()` - Calculates distance between vectors
- `Dot()` - Calculates dot product
- `Cross()` - Calculates cross product
- `Angle()` - Calculates angle between vectors
- `SignedAngle()` - Calculates signed angle
- `SqrMagnitude()` - Gets squared magnitude

---

### Color

Color utility class.

**Key Methods:**
- `Create()` - Creates a new color

**Color Constants:**
- `Black` - Black color
- `Blue` - Blue color
- `Clear` - Transparent color
- `Cyan` - Cyan color
- `Gray` - Gray color
- `Green` - Green color

---

### LocalizedStringFactory

Runtime utility for localized strings.

**Key Methods:**
- `Create()` - Creates a localized string

---

## Class Index

The API contains approximately **70+ classes** distributed across the namespaces:

### Character & NPC Systems
- Player
- Npc
- NpcConfiguration
- NpcManager
- NpcInstanceReference

### Combat & Weapons
- MeleeWeapon
- RangedWeapon
- ProjectileShooter
- ThrownBomb
- Sentry

### Environment & Interaction
- Door
- TriggerVolume
- Interactable
- Sensor
- TextBubble

### Items & Inventory
- InstantPickup
- ResourcePickup
- ConsumableHealing
- DashPack
- InventoryLibraryReference

### Mechanics
- Physics
- Health
- Navigation
- Camera
- CameraFadeManager

### Utilities
- Game
- Context
- Color
- Vector3
- ResourceManager

---

## Methods Reference (Alphabetical)

### A
- `Activate()` - BouncePad, Effector, SpikeTrap
- `Add()` - Vector3
- `AddContext()` - Effector
- `AddImpulse()` - PhysicsComponent
- `Angle()` - Vector3
- `AttemptGiveItem()` - Player
- `AttemptTrade()` - TradeInfo

### B
- `Black` - Color (constant)
- `Blue` - Color (constant)

### C
- `CanPlayerAfford()` - TradeInfo
- `CanPlayerMakeTrade()` - TradeInfo
- `ChangeHealth()` - Health
- `ChangeMaxHealth()` - Health
- `ChangeThreatLevel()` - Hittable
- `ClearAllItems()` - Player
- `ClearAllResources()` - ResourceManager
- `ClearAnchorOverride()` - Interactable
- `ClearDestination()` - Npc
- `Close()` - Door, TextBubble
- `CloseForTarget()` - TextBubble
- `ConsumeItem()` - Player
- `CopyNpcConfiguration()` - NpcManager
- `Create()` - Color, Vector3, LocalizedStringFactory
- `CreateNewConfiguration()` - NpcManager
- `Cross()` - Vector3

### D
- `Deactivate()` - BouncePad, Effector, SpikeTrap
- `DisableTrackingLaser()` - Sentry
- `Display()` - TextBubble
- `DisplayForTarget()` - TextBubble
- `DisplayForTargetWithDuration()` - TextBubble
- `DisplayText()` - Text
- `DisplayWithDuration()` - TextBubble
- `Distance()` - Vector3
- `Dot()` - Vector3

### E
- `EnableTrackingLaser()` - Sentry
- `EnterShot()` - Camera
- `EnterShotManaged()` - Camera
- `ExitShot()` - Camera
- `ExitShotManaged()` - Camera

### F
- `FadeAllIn()` - CameraFadeManager
- `FadeAllOut()` - CameraFadeManager
- `FadePlayerIn()` - CameraFadeManager
- `FadePlayerOut()` - CameraFadeManager
- `ForceCollect()` - InstantPickup

### G
- `Game()` - Game
- `GetActive()` - BouncePad, SpikeTrap
- `GetBool()` - Context
- `GetBounceHeight()` - BouncePad
- `GetCellPosition()` - CellReference
- `GetCellPositionAsVector3Int()` - CellReference
- `GetCellReference()` - Context
- `GetContext()` - Multiple reference classes
- `GetCurrentLevelContext()` - Game
- `GetFloat()` - Context
- `GetGameContext()` - Game
- `GetGameDescription()` - Game
- `GetGameTitle()` - Game
- `GetInt()` - Context
- `GetIsDamageable()` - Health
- `GetIsDowned()` - Health
- `GetIsFullHealth()` - Health
- `GetIsLocked()` - Door
- `GetIsTargetable()` - Hittable
- `GetIsValidDestination()` - Navigation
- `GetLocalizedText()` - Text
- `GetNpc()` - NpcManager
- `GetNpcInGroupByIndex()` - NpcManager
- `GetNpcPosition()` - Npc
- `GetNpcReference()` - NpcManager
- `GetRawText()` - Text
- `GetString()` - Context
- `GetTextMeshString()` - Text
- `GiveGroupInstruction()` - Npc
- `GiveInstruction()` - Npc
- `Gray` - Color (constant)
- `Green` - Color (constant)

### H
- `HasMember()` - Context
- `HasValue` - CellReference (property)
- `Health` - NpcConfiguration (property)

### I
- `IdleBehavior` - NpcConfiguration (property)
- `InteractionCanceled()` - GroupInteraction, Interaction
- `InteractionCompleted()` - GroupInteraction, Interaction
- `InventoryAndQuantityReference()` - TradeInfo.InventoryAndQuantityReference
- `IsNavigationDependent()` - Targeter
- `IsNpc()` - Context
- `IsPlayer()` - Context
- `IsStackable()` - InventoryLibraryReference
- `IsTradeValid()` - TradeInfo

### L
- `LastContext` - Context (property)
- `Lerp()` - Vector3
- `LevelContext` - Context (property)
- `LoadBasicStat()` - Game
- `LoadComparativeStat()` - Game
- `LoadPerPlayerStat()` - Game
- `LookBackward()` - Sentry
- `LookDown()` - Sentry
- `LookForward()` - Sentry
- `LookLeft()` - Sentry
- `LookRight()` - Sentry
- `LookUp()` - Sentry

### M
- `Magenta` - Color (constant)
- `Magnitude()` - Vector3
- `ManagedNode` - InstructionNode (property)
- `Max()` - Vector3
- `Min()` - Vector3
- `ModifyHealth()` - Hittable
- `MovementMode` - NpcConfiguration (property)
- `MoveTowards()` - Vector3

### K
- `Kill()` - Npc

### N
- `Normalize()` - Vector3
- `NpcClass` - NpcConfiguration (property)
- `NpcConfiguration()` - NpcConfiguration (constructor)
- `NpcVisuals` - NpcConfiguration (property)

### O
- `OnBoolSet` - Context (Event)
- `OnDefeated` - Health (Event)
- `OnFloatSet` - Context (Event)
- `OnHealthChanged` - Health (Event)
- `OnIntSet` - Context (Event)
- `OnPlayerCountChanged` - Game (Event)
- `OnPlayerJoined` - Game (Event)
- `OnPlayerLeft` - Game (Event)
- `OnStringSet` - Context (Event)
- `OnTableSet` - Context (Event)
- `Open()` - Door
- `OpenFromUser()` - Door

### P
- `PathfindingRange` - NpcConfiguration (property)
- `PhysicsMode` - NpcConfiguration (property)
- `PlayerHasItem()` - Player
- `PlayerReady()` - BasicLevelGate
- `PlayerUnready()` - BasicLevelGate
- `Project()` - Vector3

### R
- `RecordBasicStat()` - GameEndBlock
- `RecordComparativeStat()` - GameEndBlock
- `RecordPerPlayerStat()` - GameEndBlock
- `RecordStat()` - Game
- `Red` - Color (constant)
- `Reflect()` - Vector3
- `RemoveContext()` - Effector
- `RescindGroupInstruction()` - GroupInstruction
- `RescindInstruction()` - InstructionNode, Interaction
- `ResetFontSize()` - TextBubble
- `Respawn()` - InstantPickup
- `Reward1` - TradeInfo (property)
- `Reward2` - TradeInfo (property)
- `RotateTowardsDegrees()` - Vector3
- `RotateTowardsRadians()` - Vector3
- `RotationHasValue` - CellReference (property)

### S
- `Scale()` - Vector3
- `SetAlbedoColor()` - Visuals
- `SetAlpha()` - Text
- `SetAnchorOverride()` - Interactable
- `SetAutoSizingEnabled()` - Text
- `SetAutoSizingMaximum()` - Text
- `SetAutoSizingMinimum()` - Text
- `SetBool()` - Context
- `SetBounceHeight()` - BouncePad
- `SetCameraPitch()` - Camera
- `SetCameraYaw()` - Camera
- `SetCenterBlastForce()` - ThrownBomb
- `SetCenterRadius()` - ThrownBomb
- `SetColor()` - Text
- `SetCombatMode()` - NpcConfiguration
- `SetDamageAtCenter()` - ThrownBomb
- `SetDamageAtEdge()` - ThrownBomb
- `SetDamageMode()` - NpcConfiguration
- `SetDestination()` - Npc
- `SetDestinationToCell()` - Npc
- `SetDestinationToPosition()` - Npc
- `SetEnabled()` - Visuals
- `SetFieldOfView()` - Camera
- `SetFloat()` - Context
- `SetFontSize()` - Text
- `SetInt()` - Context
- `SetLineSpacing()` - Text
- `SetLocalPosition()` - Visuals
- `SetLocalRotation()` - Visuals
- `SetString()` - Context
- `SetTable()` - Context
- `SetTotalBlastRadius()` - ThrownBomb
- `ShakeForAll()` - TextBubble
- `ShakeForTarget()` - TextBubble
- `Shoot()` - ProjectileShooter, Sentry
- `ShowAlert()` - TextBubble
- `ShowAlertForTarget()` - TextBubble
- `SignedAngle()` - Vector3
- `SpawnNpc()` - NpcManager
- `SqrMagnitude()` - Vector3
- `StopContinuousRotation()` - Visuals
- `StopInteraction()` - Interactable

### T
- `Team` - NpcConfiguration (property)
- `Teleport()` - Game
- `ToggleOpenFromUser()` - Door
- `ToggleReadyParticles()` - BasicLevelGate
- `ToString()` - Multiple classes
- `Trigger()` - SpikeTrap
- `TriggerEndGame()` - GameEndBlock
- `TriggerGate()` - BasicLevelGate
- `TryGetComponent()` - Context

### U
- `UniqueId` - Context (property)
- `Unlock()` - Lockable
- `useContext` - InstanceReference (property)
- `UseXRayLos()` - Targeter

### W
- `White` - Color (constant)

### Y
- `Yellow` - Color (constant)

---

## Events Summary

| Event | Class | Description |
|-------|-------|-------------|
| `OnBoolSet` | Context | Fires when boolean value is set |
| `OnFloatSet` | Context | Fires when float value is set |
| `OnIntSet` | Context | Fires when integer value is set |
| `OnStringSet` | Context | Fires when string value is set |
| `OnTableSet` | Context | Fires when table value is set |
| `OnDefeated` | Health | Fires when entity health reaches zero |
| `OnHealthChanged` | Health | Fires when health value changes |
| `OnPlayerCountChanged` | Game | Fires when player count changes |
| `OnPlayerJoined` | Game | Fires when a player joins |
| `OnPlayerLeft` | Game | Fires when a player leaves |

---

## Usage Notes

### Context System

Every object in the world that has a script has a context. A context is the generic reference to an object. Use `GetContext()` to retrieve the context of an object, and use `TryGetComponent()` to access specific components.

### Vector3 Operations

Vector3 is commonly used for position and rotation. Create vectors with `Vector3.Create(x, y, z)` and use mathematical operations like `Add()`, `Scale()`, `Distance()`, `Dot()`, and `Cross()` for spatial calculations.

### NPC Management

Use `NpcManager` to spawn and configure NPCs programmatically. Create configurations with `CreateNewConfiguration()`, modify them with methods like `SetCombatMode()` and `SetDamageMode()`, then spawn with `SpawnNpc()`.

### Health System

The Health component tracks entity health with events for damage and defeat. Use `ChangeHealth()` for damage/healing, and listen to `OnHealthChanged` and `OnDefeated` events for game logic.

---

*Documentation compiled from Endstar API Docs (https://endstar-api-docs.endlessstudios.com/)*
*Generated with Doxygen 1.11.0*
*Last Updated: December 2025*
