# UIRuntimePropInfoListModel.Synchronize Analysis

## Method Signature

```csharp
void Synchronize(ReferenceFilter referenceFilter, IReadOnlyList<RuntimePropInfo> propsToIgnore)
```

## Parameters

| Parameter | Type | Purpose |
|-----------|------|---------|
| `referenceFilter` | `Endless.Gameplay.ReferenceFilter` | Filter to select which props to show |
| `propsToIgnore` | `IReadOnlyList<RuntimePropInfo>` | Props to EXCLUDE from the list |

## CRITICAL DISCOVERY

**`propsToIgnore` is an EXCLUDE list, not an include list!**

This means Synchronize:
1. Gets ALL props matching `referenceFilter` from PropLibrary
2. Removes any props in `propsToIgnore`
3. Populates the internal `List` field

## Class Fields

```csharp
// Internal list that UI displays
List<RuntimePropInfo> List;

// Context info
Contexts Context;
```

## Available Methods

```csharp
// Can add props directly to the list!
virtual void Add(RuntimePropInfo item, bool triggerEvents);

// Synchronize the list
void Synchronize(ReferenceFilter referenceFilter, IReadOnlyList<RuntimePropInfo> propsToIgnore);
```

## Inheritance Chain

```
UIRuntimePropInfoListModel
  └─> UIBaseLocalFilterableListModel<RuntimePropInfo>
      └─> UIBaseListModel<RuntimePropInfo>
          └─> UIGameObject
              └─> MonoBehaviour
```

## Data Flow

```
OnLibraryRepopulated() called
       │
       ▼
Synchronize(filter, ignoreList)
       │
       ├─> Calls PropLibrary.GetReferenceFilteredDefinitionList(filter)
       │         │
       │         ▼
       │   Returns List<RuntimePropInfo> matching filter
       │
       ├─> Removes props in ignoreList
       │
       └─> Populates internal List field
              │
              ▼
       UI renders items from List
```

## Solution Approaches

### Option 1: Ensure prop is in filtered list
- Our prop must be in `_referenceFilterMap[filter]`
- Must have correct ReferenceFilter on EndlessProp
- PopulateReferenceFilterMap() must run AFTER our prop is added

### Option 2: Call Add() directly after Synchronize
```csharp
// After Synchronize completes
listModel.Add(ourRuntimePropInfo, true);
```
This bypasses the filter entirely.

### Option 3: Patch GetReferenceFilteredDefinitionList
Postfix to append our prop to the result before Synchronize processes it.

## Recommended Approach

**Option 2 (Direct Add) is safest** because:
- Doesn't depend on timing
- Doesn't require correct ReferenceFilter setup
- Single point of modification
- Guaranteed to work if we have the list model instance

## Next Steps

1. Verify we can get UIPropToolPanelView instance at runtime
2. Access runtimePropInfoListModel field
3. Call Add() after each Synchronize, or
4. Hook into OnLibraryRepopulated to add after sync
