# Endstar Prop System Diagnostic Data Analysis

## Summary of Current Capture (v4.0.0 - COMPLETE PIPELINE)

### What We Captured

#### 1. Type Metadata (Initialization Phase)
- **StageManager**: Full type with 41 fields, 44 properties, 37 methods
- **PropLibrary**: Full type with 8 fields, 2 properties, 22 methods
- **EndlessProp**: Full type with 11 fields, 18 properties, 13 methods
- **BaseTypeList**: Type extending AbstractComponentList<BaseTypeDefinition>

#### 2. BaseTypeDefinitions (37 total)
| GUID | Prefab Name | IsNetworked | IsUserExposed | IBaseType Implementation |
|------|-------------|-------------|---------------|--------------------------|
| b9de8a1e-00b9-44a3-84ac-5936d1a6a5e5 | Static Prop | False | True | StaticProp (Filter=None) |
| b76fb1b6-6619-467c-a8a2-aa92dc530996 | Ambient Settings | True | False | AmbientSettings |
| 86160f4a-83c7-4b39-a9fa-966a0c85fc3f | Assault Rifle | True | False | TwoHandedRangedWeaponItem |
| e8a0df38-8370-4082-a733-fe1f0e7020a4 | Basic Spawn Point | False | True | BasicSpawnPoint |
| db8d96c8-a534-4492-b053-f7bb997e7e58 | BasicLevelGate | True | True | BasicLevelGate |
| 831286a7-0999-41e7-ae45-1e432da318eb | BasicProp | False | True | BasicProp |
| d46a2ac9-066f-4c5d-9e0c-32d41a76a332 | BehaviorNode | False | False | BehaviorNode |
| 7f2b4d01-193d-45c1-b730-a39c45f979ef | Blaster | True | False | OneHandedRangedWeaponItem |
| a9a0225c-6c34-47bc-8649-6c7b33b8473b | Bounce Pad | True | True | BouncePad |
| d42069ba-bcf4-444a-9781-ce0926da6e50 | CommandNode | False | False | CommandNode |
| 9606b939-1576-4544-a6da-1be6fded8152 | DashPack | True | False | DashPackItem |
| 2042f8ff-3f89-4e97-ab49-e336c12a43f5 | Depth Plane | True | False | DepthPlane |
| d7e45813-4679-4839-a725-2748c68ea9d6 | Door | True | True | Door (NavValue=Dynamic) |
| 0c369198-02eb-4967-83d8-7ead9cd0cfea | Filters | True | False | Filter |
| 5f3e9700-8dc5-4ce4-8c80-ef07e1e6e113 | GroupBehaviorNode | False | False | - |
| 3709a1c6-3ead-43c3-80f7-393543ec0a5d | GroupCommandNode | False | False | - |
| befe3c20-10d6-4dfa-8acc-d980aaf2b773 | GroupInteractionNode | False | False | - |
| 44e9d217-dbf9-49a7-a7c7-8952d014f375 | Healing Syringe | True | False | ConsumableHealingItem |
| 9e64496c-1325-4a94-8e2e-55aebc98d863 | InstantPickup | True | True | InstantPickup |
| e281511e-440b-4f83-9a92-e0c3721b6863 | InteractionNode | False | False | InteractionNode |
| ea11af0d-504e-404d-8502-f614ec3e2ddf | Jetpack | True | False | JetpackItem |
| 03af6486-2988-40bf-a424-8cab68c4e5ab | Key | True | False | Key (Filter=Key,InventoryItem) |
| cd7a37f8-3b3e-48d6-ba73-4da63b8ae1e5 | Npc | False | False | NpcEntity |
| 3cc98142-e4c6-47a4-b7d3-83edab4aa870 | PhysicsCube | True | True | DraggablePhysicsCube |
| d7d835d4-c47b-42a5-81f6-ae780bcf9363 | ResourcePickup | True | False | ResourcePickup |
| dec656ba-a4f1-4101-8c29-7d212183e364 | RuleBlock | False | False | RuleBlock |
| dafc866a-364b-4a01-a847-e08f5290f68e | Sentry | True | True | Sentry |
| 7213531a-9128-4b95-b129-aad8a01a6c41 | Spike Trap | True | True | SpikeTrap |
| 3bff8f35-dae5-4c85-bbbb-d9ca9a75ce1d | Sword | True | False | OneHandedMeleeWeaponItem |
| f4caf448-d137-4abc-935a-ec4c1f40aa42 | Sword_2H | True | False | TwoHandedMeleeWeaponItem |
| c7702f56-46b1-425a-bcc4-48d68d289757 | Thrown Bomb | True | False | ThrownBombItem |
| f60832a2-63a9-466d-83e8-df441fbf37c9 | Treasure | True | False | TreasureItem |
| f699aaf0-6232-41c4-9765-8275e9e91e44 | TriggerVolume | False | False | - |
| 8f635db1-6563-465b-a911-d002f067852b | WireTesting | False | False | - |
| bec7eba3-6288-41ed-9f9d-52754fbc2b3d | CutsceneCamera | True | False | - |
| d402ad57-af6a-464d-a3e1-6df66e4775a4 | GameEndRule | False | False | - |
| d2189f00-fad4-4351-ba48-0c23fdd897ca | Ranged_2H_HeavyCrossbow | True | False | - |

#### 3. ComponentDefinitions (15 total)
| GUID | Prefab Name | IsNetworked | Component Class |
|------|-------------|-------------|-----------------|
| 39ae24a0-b3af-4912-8e0d-642d0ee74005 | Simple Interactable | True | Interactable |
| 176bce3f-20f6-403e-9564-225ee2219d8c | HittableComponent | True | HittableComponent + HostilityComponent |
| b6cd3d6f-7c29-406a-b6b8-137896fc4992 | HealthComponent | True | HealthComponent |
| cc67644e-7fc6-4d7d-b73e-c8b3d005e8e7 | TargeterComponent | True | TargeterComponent |
| 2465f533-ee3f-41b3-9664-a0cba5b74b7a | TeamComponent | True | TeamComponent |
| cb841704-b362-4d8e-99be-3ba474902340 | DynamicVisuals | True | DynamicVisuals |
| d96f0900-7320-48ae-a79b-7564b07ffe7a | TextBubbleComponent | True | TextBubble |
| f7d937c8-bfc8-4264-a309-21d56a3f4cba | TriggerComponent | False | TriggerComponent |
| db4b86b1-6689-4a2e-a919-778d43686534 | SensorComponent | False | SensorComponent |
| 0544b79e-05b7-487b-9e3c-450058d44bf5 | ProjectileShooterComponent | True | ProjectileShooterComponent |
| cda23843-25ec-4baf-abbc-8a6255f0ab1c | TextComponent | True | TextComponent |
| 4d2efca1-915c-4d1f-a70b-5819c6a55b82 | PeriodicEffectorComponent | False | PeriodicEffector |
| 8edbd575-c6b3-4821-ae3a-3a5b899c884c | Lockable | True | Lockable |
| 151fb3bb-e21e-4638-a0af-e9221031fedf | DynamicNavigation | False | DynamicNavigationComponent |
| 7aa21324-1cb1-470d-a70f-1ad42da58837 | ResizableVolume | False | ResizableVolume |

#### 4. Prop Object Structure (BuildPrefab Input)
```
Prop {
  BaseTypeId: SerializableGuid           // Required - references BaseTypeDefinition
  ComponentIds: List<SerializableGuid>   // Optional - additional components to attach
  VisualAssets: List<AssetReference>     // Visual asset references
  ScriptAsset: AssetReference            // Lua script reference (can be null)
  PrefabAsset: AssetReference            // Custom prefab override (can be null)
  IconFileInstanceId: int                // Icon asset ID
  PropLocationOffsets: List<?>           // Placement offset configurations
  ApplyXOffset: bool
  ApplyZOffset: bool
  Bounds: Vector3Int
  OpenSource: bool
  HasScript: bool
}
```

#### 5. EndlessProp Output Structure (BuildPrefab Output)
```
EndlessProp {
  ScriptComponent: EndlessScriptComponent  // Lua script handler
  ReferenceFilter: ReferenceFilter         // Combined from base + components
  NavValue: NavType                        // Navigation type
  Prop: Prop                               // Original prop data
  IsNetworked: bool                        // Whether to spawn NetworkObject
  ScriptInjectors: IScriptInjector[]       // Components exposing Lua objects
  WorldObject: WorldObject                 // Runtime tracking object
  TransformMap: Dictionary                 // Named transform references
  GameObject hierarchy:
    - EndlessProp component
    - EndlessScriptComponent
    - WorldObject
    - EndlessVisuals
    - BaseType prefab clone (e.g., BasicProp(Clone))
      - IBaseType implementation component
    - Component prefab clones...
}
```

---

## v4.0.0 CAPTURED DATA - Complete Prop Placement Pipeline

### Tool Selection Events (NOW CAPTURED)
```
[TOOL] ToolManager.Activate: Creator mode activated - tools enabled
[TOOL] SetActiveTool: Type=Empty Name=EmptyTool
```

### PropTool Selection Events (NOW CAPTURED)
```
[PROPTOOL] HandleSelected: PropTool is now active
[PROPTOOL] CurrentSelection: AssetId=00000000-0000-0000-0000-000000000000
[PROPTOOL] UpdateSelectedAssetId: AssetId=e0002f31-53b6-476b-b696-99d2c06a1112
```

### PropTool Placement Events (NOW CAPTURED)
```
[PROPTOOL] ToolPressed: User started click/placement
[PROPTOOL] GhostPosition: Ghost exists: Transform('Nose Blocker(Clone)')
[PROPTOOL] ToolReleased: User released click - checking for placement
[PROPTOOL] PlacementAttempt: AssetId=e0002f31-53b6-476b-b696-99d2c06a1112 HasGhost=True
[PROPTOOL] AttemptPlaceProp: Position=(-19.00, 0.00, -6.00) Rotation=(0.00, 0.00, 0.00) AssetId=e0002f31-53b6-476b-b696-99d2c06a1112
```

### Stage Tracking Events (NOW CAPTURED)
```
[STAGE] PlacementIsValid: Prop=Prop Position=(-19.00, 0.00, -6.00) Rotation=(0.00, 0.00, 0.00, 1.00) Result=True
[STAGE] TrackNonNetworkedObject: AssetId=e0002f31-53b6-476b-b696-99d2c06a1112 InstanceId=61c422f1-bf6b-4e3d-bf75-8139b3d26f96 IsFromEditor=True
[STAGE] TrackedGameObject: Name=Nose Blocker(Clone)
```

### Key Discovery: IsFromEditor Flag
- **Level-loaded props**: `IsFromEditor=False` - Props loaded from level data
- **User-placed props**: `IsFromEditor=True` - Props placed during editing session

### Test Session Props Placed (2026-01-13 21:54)
| Prop Name | AssetId | Position | InstanceId |
|-----------|---------|----------|------------|
| Nose Blocker | e0002f31-53b6-476b-b696-99d2c06a1112 | (-19, 0, -6) | 61c422f1-bf6b-4e3d-bf75-8139b3d26f96 |
| Sack of Rice | 6360123c-7279-4aeb-b009-7446cdb8a980 | (-15, 0, -11) | 683838b3-6628-4737-b0c3-8ae0600e5310 |
| Diver Rock | 6389e014-09f4-4da3-a34c-f5925e852c26 | (-18, 0, -9) | a9a8444a-308a-40cf-8aba-bb2e9023f2e6 |

---

## Complete Prop Placement Pipeline (ALL CAPTURED)

```
┌─────────────────────────────────────────────────────────────────────────┐
│                        INITIALIZATION (CAPTURED)                         │
├─────────────────────────────────────────────────────────────────────────┤
│ 1. BepInEx loads plugin v4.0.0                                          │
│ 2. StageManager.Awake → Initializes BaseTypeList, ComponentList          │
│ 3. LoadLibraryPrefabs → For each prop in level:                         │
│    └── EndlessProp.BuildPrefab(prop) → Creates prefab from definition   │
│    └── Stage.TrackNonNetworkedObject(IsFromEditor=False)                │
└─────────────────────────────────────────────────────────────────────────┘
                                    ↓
┌─────────────────────────────────────────────────────────────────────────┐
│                        TOOL SELECTION (CAPTURED)                         │
├─────────────────────────────────────────────────────────────────────────┤
│ 4. [TOOL] ToolManager.Activate: Creator mode activated                  │
│ 5. [PROPTOOL] HandleSelected: PropTool is now active                    │
│ 6. [PROPTOOL] CurrentSelection: AssetId=00000000-...                    │
└─────────────────────────────────────────────────────────────────────────┘
                                    ↓
┌─────────────────────────────────────────────────────────────────────────┐
│                        PROP SELECTION (CAPTURED)                         │
├─────────────────────────────────────────────────────────────────────────┤
│ 7. User clicks prop in UI panel                                         │
│ 8. [PROPTOOL] UpdateSelectedAssetId: AssetId=<selected prop guid>       │
│ 9. Ghost preview spawns                                                 │
└─────────────────────────────────────────────────────────────────────────┘
                                    ↓
┌─────────────────────────────────────────────────────────────────────────┐
│                       PROP PLACEMENT (CAPTURED)                          │
├─────────────────────────────────────────────────────────────────────────┤
│ 10. [PROPTOOL] ToolPressed: User started click/placement                │
│ 11. [PROPTOOL] GhostPosition: Ghost exists: Transform('<name>')         │
│ 12. [PROPTOOL] ToolReleased: User released click                        │
│ 13. [PROPTOOL] PlacementAttempt: AssetId=<guid> HasGhost=True           │
│ 14. [PROPTOOL] AttemptPlaceProp: Position=(...) Rotation=(...) AssetId  │
└─────────────────────────────────────────────────────────────────────────┘
                                    ↓
┌─────────────────────────────────────────────────────────────────────────┐
│                    SERVER INSTANTIATION (CAPTURED)                       │
├─────────────────────────────────────────────────────────────────────────┤
│ 15. [STAGE] PlacementIsValid: Result=True/False                         │
│ 16. Object.Instantiate<EndlessProp>(metadata.EndlessProp, pos)          │
│ 17. [STAGE] TrackNonNetworkedObject: AssetId=... IsFromEditor=True      │
│ 18. [STAGE] TrackedGameObject: Name=<PropName>(Clone)                   │
└─────────────────────────────────────────────────────────────────────────┘
                                    ↓
┌─────────────────────────────────────────────────────────────────────────┐
│                    TOOL DESELECTION (CAPTURED)                           │
├─────────────────────────────────────────────────────────────────────────┤
│ 19. [PROPTOOL] UpdateSelectedAssetId: AssetId=00000000-... (deselect)   │
│ 20. [TOOL] SetActiveTool: Type=Empty Name=EmptyTool                     │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## Injection Requirements Summary

To inject a custom prop, we need to:

### 1. Register BaseTypeDefinition
- Hook `AbstractComponentList<BaseTypeDefinition>.TryGetDefinition`
- Return custom definition for our custom GUID
- Definition must have valid Prefab with IBaseType component

### 2. Create Prop Data Structure
```csharp
Prop customProp = new Prop {
    AssetID = new SerializableGuid(customGuid),
    Name = "Custom Prop Name",
    BaseTypeId = customBaseTypeGuid,  // Must exist in BaseTypeList
    ComponentIds = new List<SerializableGuid>(),
    // ... other fields
};
```

### 3. Call Injection Method
```csharp
StageManager.Instance.InjectProp(
    customProp,
    testPrefab,     // Can be null for non-testing
    testScript,     // Can be null
    customIcon      // Sprite for UI
);
```

### 4. Timing
- Must call BEFORE `LoadLibraryPrefabs` processes injected props
- Best hook: `StageManager.Awake` postfix, or scene load event

---

## Verified Classes and Hooks (v4.0.0)

| Class | Namespace | DLL | Hooked Methods |
|-------|-----------|-----|----------------|
| StageManager | Endless.Gameplay.LevelEditing.Level | Gameplay.dll | Awake, InjectProp, LoadLibraryPrefabs |
| PropLibrary | Endless.Gameplay.LevelEditing | Gameplay.dll | InjectProp |
| EndlessProp | Endless.Gameplay.Scripting | Gameplay.dll | BuildPrefab |
| BaseTypeList | Endless.Gameplay | Gameplay.dll | TryGetDefinition |
| ComponentList | Endless.Gameplay | Gameplay.dll | TryGetDefinition |
| ToolManager | Endless.Creator.LevelEditing.Runtime | Creator.dll | SetActiveTool, Activate |
| PropTool | Endless.Creator.LevelEditing.Runtime | Creator.dll | HandleSelected, UpdateSelectedAssetId, ToolPressed, ToolReleased, AttemptPlaceProp, PlaceProp |
| Stage | Endless.Gameplay.LevelEditing.Level | Gameplay.dll | TrackNonNetworkedObject, PlacementIsValid |

---

## Action Items (COMPLETED)

1. [x] Update plugin to add Creator.dll reference
2. [x] Implement ToolManager patches (tool selection)
3. [x] Implement PropTool patches (selection + placement)
4. [x] Implement Stage patches (tracking)
5. [x] Test with full user flow: select tool → select prop → place prop
6. [x] Document all captured data

## Next Steps for Prop Injection

1. [ ] Create prototype IBaseType implementation (CustomStaticProp)
2. [ ] Create runtime BaseTypeDefinition factory
3. [ ] Hook TryGetDefinition to return custom definitions
4. [ ] Create test prop and verify it appears in prop panel
5. [ ] Place test prop and verify full pipeline works
