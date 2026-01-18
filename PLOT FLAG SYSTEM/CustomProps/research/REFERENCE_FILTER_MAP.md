# PropLibrary Reference Filter System Analysis

## Key Fields

```csharp
// The main prop storage
Dictionary<AssetReference, RuntimePropInfo> loadedPropMap;

// Filtered views of props (one list per filter category)
Dictionary<ReferenceFilter, List<RuntimePropInfo>> _referenceFilterMap;

// Which filters are "dynamic" (used by UI)
ReferenceFilter[] dynamicFilters;
```

## How Filtering Works

### 1. PopulateReferenceFilterMap()

Called after props are loaded. It:
1. Iterates through all entries in `loadedPropMap`
2. Reads `EndlessProp.ReferenceFilter` from each RuntimePropInfo
3. Groups props into `_referenceFilterMap` by their filter value

```csharp
void PopulateReferenceFilterMap()
{
    _referenceFilterMap.Clear();

    foreach (var kvp in loadedPropMap)
    {
        var rpi = kvp.Value;
        var filter = rpi.EndlessProp?.ReferenceFilter ?? ReferenceFilter.None;

        if (!_referenceFilterMap.ContainsKey(filter))
            _referenceFilterMap[filter] = new List<RuntimePropInfo>();

        _referenceFilterMap[filter].Add(rpi);
    }
}
```

### 2. GetReferenceFilteredDefinitionList(ReferenceFilter filter)

Returns props matching the requested filter:

```csharp
IReadOnlyList<RuntimePropInfo> GetReferenceFilteredDefinitionList(ReferenceFilter filter)
{
    if (_referenceFilterMap.TryGetValue(filter, out var list))
        return list;
    return Array.Empty<RuntimePropInfo>();
}
```

## ReferenceFilter Enum Values

Based on previous analysis:
```csharp
enum ReferenceFilter
{
    None = 0,           // Props with no filter (may not show in UI!)
    Npc = 2,            // NPC-related props
    InventoryItem = 8,  // Items that can be in inventory
    // ... possibly more
}
```

## Dynamic Filters

The `dynamicFilters` field contains filters that the UI actually uses.

From previous analysis:
```csharp
dynamicFilters = { Npc (2), InventoryItem (8) }
```

**CRITICAL**: If a prop has `ReferenceFilter = None (0)`, it will NOT appear in the UI
because `None` is not in `dynamicFilters`.

## Why Our Prop Doesn't Show

### Problem Chain:
1. We add prop to `loadedPropMap` ✓
2. `PopulateReferenceFilterMap()` runs BEFORE our injection, OR
3. Our `EndlessProp.ReferenceFilter` is `None` or wrong value
4. Prop is not in `_referenceFilterMap[InventoryItem]`
5. `GetReferenceFilteredDefinitionList(InventoryItem)` doesn't include our prop
6. `Synchronize()` never sees our prop
7. UI `List` doesn't contain our prop

### Solution Requirements:

Either:

**A) Ensure correct timing:**
1. Add prop to loadedPropMap
2. Set EndlessProp.ReferenceFilter = InventoryItem (8)
3. Call PopulateReferenceFilterMap() again
4. Call Synchronize() on the list model

**B) Bypass the filter system:**
1. Add prop to loadedPropMap (for other systems)
2. Directly call `listModel.Add(ourProp, true)` on UI

## RuntimePropInfo Structure

```csharp
struct RuntimePropInfo
{
    public Prop PropData;                    // The prop asset data
    public Sprite Icon;                      // UI icon
    public EndlessProp EndlessProp;          // Prefab instance (has ReferenceFilter!)
    public bool IsLoading;                   // Loading state
    public bool IsMissingObject;             // Error state
}
```

## EndlessProp.ReferenceFilter

The `ReferenceFilter` value comes from `EndlessProp`, NOT from `Prop`.

This means:
- Creating just a `Prop` object is insufficient
- We need an `EndlessProp` instance with correct `ReferenceFilter`
- OR we need to bypass the filter system entirely

## Recommended Fix

Since creating a valid `EndlessProp` with correct `ReferenceFilter` is complex,
the **direct Add() approach** is recommended:

```csharp
// After OnLibraryRepopulated triggers Synchronize
var panelView = FindObjectOfType<UIPropToolPanelView>();
var listModel = panelView.runtimePropInfoListModel;
listModel.Add(ourRuntimePropInfo, true);
```

This ensures our prop appears regardless of filter configuration.
