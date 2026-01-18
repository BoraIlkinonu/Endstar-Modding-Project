# Endstar Prop System Event Chain Map

## Event Definitions

### CreatorManager Events

| Event | Type | When Fired |
|-------|------|------------|
| `OnRepopulate` | `Action` | When any repopulation starts |
| `OnTerrainRepopulated` | `Action` | After terrain is repopulated |
| `OnPropsRepopulated` | `Action` | After props are repopulated |
| `OnCreatorStarted` | `UnityEvent` | When creator mode starts |
| `OnCreatorEnded` | `UnityEvent` | When creator mode ends |
| `OnLeavingSession` | `UnityEvent` | When leaving the session |

## Complete Event Chain

```
┌─────────────────────────────────────────────────────────────────────────┐
│                     PROP SYSTEM EVENT CHAIN                             │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  [LEVEL LOAD / CREATOR MODE START]                                      │
│                                                                         │
│  1. StageManager.RegisterStage() or similar                             │
│         │                                                               │
│         ▼                                                               │
│  2. PropLibrary.Repopulate(stage, cancelToken) [ASYNC]                  │
│         │                                                               │
│         ├─> Loads props from game database                              │
│         ├─> Populates loadedPropMap                                     │
│         ├─> Calls PopulateReferenceFilterMap()                          │
│         │                                                               │
│         ▼                                                               │
│  3. CreatorManager.OnPropsRepopulated.Invoke()                          │
│         │                                                               │
│         ├─────────────────────────────────────────┐                     │
│         │                                         │                     │
│         ▼                                         ▼                     │
│  [SUBSCRIBER 1]                            [SUBSCRIBER 2]               │
│  Our Handler (if prepended)                UIPropToolPanelView          │
│         │                                  .OnLibraryRepopulated()      │
│         │                                         │                     │
│         ▼                                         ▼                     │
│  Inject prop to loadedPropMap              listModel.Synchronize()      │
│         │                                         │                     │
│         │                                         ├─> GetReferenceFiltered│
│         │                                         │   DefinitionList()   │
│         │                                         │                     │
│         │                                         ├─> Populate List     │
│         │                                         │                     │
│         ▼                                         ▼                     │
│  [TIMING PROBLEM!]                         UI displays props            │
│  If we inject AFTER Synchronize,                                        │
│  our prop won't be in the UI List                                       │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

## Subscription Order

When multiple handlers subscribe to the same event, execution order matters:

```csharp
// Current subscription (if using +=)
CreatorManager.OnPropsRepopulated += OurHandler;  // Added last = runs last

// If original handler was registered first, sequence is:
// 1. UIPropToolPanelView.OnLibraryRepopulated (runs first)
// 2. OurHandler (runs second - TOO LATE!)

// Solution: Prepend our handler using reflection
var field = typeof(CreatorManager).GetField("OnPropsRepopulated", ...);
var existingAction = (Action)field.GetValue(creatorManager);
var newAction = (Action)OurHandler + existingAction;  // Our handler FIRST
field.SetValue(creatorManager, newAction);
```

## Current Handler Sequence (After Prepending)

```
OnPropsRepopulated.Invoke()
       │
       ├─[1] OurHandler (runs FIRST)
       │        │
       │        ├─> Add prop to loadedPropMap
       │        ├─> Set ReferenceFilter
       │        ├─> Call PopulateReferenceFilterMap()
       │        └─> Return
       │
       └─[2] OnLibraryRepopulated (runs SECOND)
                │
                ├─> Get filter value (InventoryItem?)
                ├─> Call Synchronize(filter, ignoreList)
                │        │
                │        ├─> GetReferenceFilteredDefinitionList(filter)
                │        │        │
                │        │        └─> Should now include our prop!
                │        │
                │        └─> Populate UI List
                │
                └─> UI updates
```

## Why Previous Attempts Failed

### Attempt 1: Simple injection after scene load
- **Problem**: OnPropsRepopulated already fired
- **Result**: Prop in loadedPropMap but Synchronize never re-called

### Attempt 2: Patch GetAllRuntimeProps
- **Problem**: Synchronize doesn't use GetAllRuntimeProps
- **Result**: GetAllRuntimeProps returns our prop, but UI doesn't call it

### Attempt 3: Prepend to OnPropsRepopulated + inject
- **Problem**: ReferenceFilter not set correctly on EndlessProp
- **Result**: Prop not in _referenceFilterMap[InventoryItem]

### Attempt 4: UI patches (disabled)
- **Problem**: Patching UI lifecycle methods broke everything
- **Result**: Prop tool wouldn't open at all

## Correct Solution Path

### Option A: Fix the filter issue
1. Prepend handler to OnPropsRepopulated
2. Inject prop with valid EndlessProp (has ReferenceFilter)
3. Call PopulateReferenceFilterMap()
4. Let OnLibraryRepopulated run normally
5. Synchronize will include our prop

### Option B: Add directly to UI (RECOMMENDED)
1. Let normal flow complete
2. Find UIPropToolPanelView instance
3. Get runtimePropInfoListModel
4. Call `listModel.Add(ourProp, true)`
5. Prop appears immediately

## Hook Points for Option B

```csharp
// After OnLibraryRepopulated completes
[HarmonyPatch(typeof(UIPropToolPanelView), "OnLibraryRepopulated")]
class OnLibraryRepopulatedPatch
{
    static void Postfix(UIPropToolPanelView __instance)
    {
        var listModel = __instance.runtimePropInfoListModel;
        listModel.Add(ourRuntimePropInfo, true);
    }
}
```

This guarantees our prop is added AFTER Synchronize populates the base list.
