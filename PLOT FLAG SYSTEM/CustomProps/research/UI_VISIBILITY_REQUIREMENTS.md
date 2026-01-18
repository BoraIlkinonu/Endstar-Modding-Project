# Requirements for Prop to Appear in UI

## Checklist

### Storage Layer Requirements

- [ ] **Prop exists in loadedPropMap**
  - Key: Valid `AssetReference` (use `Prop.ToAssetReference()`)
  - Value: Valid `RuntimePropInfo` struct

- [ ] **RuntimePropInfo is properly populated**
  - [ ] `PropData` is not null (must be valid `Prop` instance)
  - [ ] `Icon` is not null (must be valid `Sprite`)
  - [ ] `EndlessProp` has correct `ReferenceFilter` (see below)
  - [ ] `IsLoading` = false
  - [ ] `IsMissingObject` = false

### Filter Layer Requirements (Option A)

- [ ] **EndlessProp.ReferenceFilter matches UI filter**
  - UI typically uses `InventoryItem (8)`
  - Must NOT be `None (0)`

- [ ] **Prop exists in _referenceFilterMap**
  - `PopulateReferenceFilterMap()` must run AFTER our prop is added
  - Prop must be at key matching UI's filter value

- [ ] **GetReferenceFilteredDefinitionList returns our prop**
  - Called by Synchronize with UI's current filter
  - Our prop must be in the returned list

### UI Layer Requirements

- [ ] **Synchronize() called after our changes**
  - Called by `OnLibraryRepopulated()`
  - Must happen AFTER our prop is in filter map

- [ ] **Prop not in propsToIgnore list**
  - Second parameter to Synchronize
  - Our prop must NOT be in this list

- [ ] **Prop passes IncludeInFilteredResults() check**
  - Any additional UI-side filtering
  - Usually based on search text, category, etc.

- [ ] **List contains our prop**
  - Final destination: `UIRuntimePropInfoListModel.List`
  - This is what the UI renders

## Verification Commands (Runtime)

### Check loadedPropMap (Press F5)
```
Expected output:
[Info] loadedPropMap contains 76 entries
[Info]   [75] Pearl Basket (ID: custom_pearl_basket)
```

### Check _referenceFilterMap (Press F6)
```
Expected output:
[Info] _referenceFilterMap has X filter categories
[Info]   Filter InventoryItem (8): Y props
         ↑ Our prop should be in this count
```

### Check UI List (Press F7)
```
Expected output:
[Info] UI List contains Z props
[Info]   [Z-1] Pearl Basket
         ↑ Our prop should appear here
```

## Quick Diagnostic

If prop is in loadedPropMap but NOT in UI:

1. **Check F6 output** - Is prop in `_referenceFilterMap[8]`?
   - **No**: EndlessProp.ReferenceFilter is wrong OR PopulateReferenceFilterMap not called
   - **Yes**: Continue to step 2

2. **Check F7 output** - Is prop in UI List?
   - **No**: Synchronize didn't include it (check filter parameter)
   - **Yes**: UI should show it (check rendering issues)

## Bypass Solution (Option B)

If fixing filters is too complex, bypass entirely:

```csharp
// After Synchronize completes, add directly:
var panelView = FindObjectOfType<UIPropToolPanelView>();
var listModel = panelView.runtimePropInfoListModel;
listModel.Add(ourRuntimePropInfo, triggerEvents: true);
```

This guarantees visibility regardless of filter configuration.

## Common Failure Modes

| Symptom | Cause | Solution |
|---------|-------|----------|
| Prop in loadedPropMap, not in filterMap | ReferenceFilter=None | Set EndlessProp.ReferenceFilter=8 |
| Prop in filterMap, not in UI List | Wrong filter used by UI | Check what filter Synchronize receives |
| Prop in UI List, not visible | Rendering issue | Check UI scroll, visibility, search filter |
| No UIPropToolPanelView found | Prop tool not opened | Open prop tool first, or hook OnToolChange |
