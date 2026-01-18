# CUSTOM PROP INJECTION - CURRENT STATUS

**Date:** 2026-01-06
**Status:** PARTIAL SUCCESS - Prop injected into PropLibrary but NOT visible in UI

---

## WHAT WORKS

### Direct Injection into loadedPropMap
- Successfully added prop to `PropLibrary.loadedPropMap` dictionary
- PropLibrary count increased from 75 to 76 props
- Custom prop "Pearl Basket" found in library via `GetAllRuntimeProps()`

### Log Evidence of Success:
```
[Info] DIRECT INJECTION: Added custom_pearl_basket to loadedPropMap!
[Info] PropLibrary now has 76 props
[Info] FOUND CUSTOM PROP: Pearl Basket (ID: custom_pearl_basket)
```

---

## WHAT DOESN'T WORK

### Prop Not Visible in UI
- Prop tool panel does not show "Pearl Basket"
- UI refresh attempts failed
- `UIRuntimePropInfoListModel` instances: 0 found when trying to refresh

### InjectProp Methods Fail Silently
- `PropLibrary.InjectProp()` - async method, silently rejects custom props
- `StageManager.InjectProp()` - also fails silently
- No exceptions thrown, but prop not added

---

## KEY TECHNICAL DISCOVERIES

### 1. PropLibrary Structure
```
PropLibrary fields:
  - loadedPropMap (Dictionary<AssetReference, RuntimePropInfo>)
  - injectedPropIds (List<SerializableGuid>)  // NOT List<String>!
  - basePropPrefab (EndlessProp)
  - prefabSpawnRoot (Transform)
  - missingObjectPrefab (EndlessProp)
```

### 2. Dictionary Key Type
- `loadedPropMap` key is `AssetReference`, NOT `String`
- Must use `Prop.ToAssetReference()` to create proper key
- Error when using String: "Object of type 'System.String' cannot be converted to type 'Endless.Assets.AssetReference'"

### 3. injectedPropIds List Type
- Expects `SerializableGuid`, NOT `String`
- Error when using String: "Object of type 'System.String' cannot be converted to type 'Endless.Shared.DataTypes.SerializableGuid'"

### 4. RuntimePropInfo Structure
```csharp
RuntimePropInfo {
    public Prop PropData;        // The prop asset data
    public Sprite Icon;          // UI icon
    public EndlessProp EndlessProp;  // The prefab instance (can be null initially)
}
```

### 5. Prop Inheritance Chain
```
Prop -> Asset -> AssetCore
```
- `AssetCore` has: Name, AssetID, AssetVersion, AssetType
- `Asset` adds: Description, InternalVersion, RevisionMetaData
- `Prop` adds: baseTypeId, bounds, componentIds, etc.

### 6. Valid baseTypeId
- Extracted from existing props: `f60832a2-63a9-466d-83e8-df441fbf37c9`
- This is the baseTypeId used by most props in the game

---

## INJECTION PROCESS THAT WORKS

```csharp
// 1. Clone existing prop (ensures valid internal structure)
var existingProp = GetAllRuntimeProps()[0].PropData;
var newProp = existingProp.Clone();

// 2. Set custom values
newProp.Name = "Pearl Basket";
newProp.AssetID = "custom_pearl_basket";
newProp.Description = "Custom prop: Pearl Basket";
newProp.baseTypeId = "f60832a2-63a9-466d-83e8-df441fbf37c9";

// 3. Create RuntimePropInfo
var runtimePropInfo = new RuntimePropInfo();
runtimePropInfo.PropData = newProp;
runtimePropInfo.Icon = customIcon;
// runtimePropInfo.EndlessProp = null; // No prefab yet

// 4. Create AssetReference key
var assetRefKey = newProp.ToAssetReference();

// 5. Add to loadedPropMap
loadedPropMap.Add(assetRefKey, runtimePropInfo);
```

---

## UNSOLVED PROBLEMS

### 1. UI Not Showing Injected Prop
**Hypothesis:** The UI has its own filtered list that doesn't automatically refresh when loadedPropMap changes.

**Attempted solutions:**
- Call `UIRuntimePropInfoListModel.Synchronize()` - couldn't find instances
- Patch `UIRuntimePropInfoListController.Start` - broke the UI entirely

### 2. Prop Tool Window Not Opening
**Cause:** Our Harmony patches on UIRuntimePropInfoListController were crashing the UI.

**Solution:** Disabled these patches. Prop tool now opens but custom prop still not visible.

### 3. EndlessProp Prefab Missing
- RuntimePropInfo.EndlessProp is null for our injected prop
- Game may require a valid prefab to display the prop
- Need to clone/create an EndlessProp prefab

---

## FILES MODIFIED

### ProperPropInjector.cs
- Main injection logic
- Direct injection into loadedPropMap
- Creates RuntimePropInfo with PropData and Icon
- Uses ToAssetReference() for dictionary key

### PropIntegration.cs
- Harmony patches (some disabled)
- UIRuntimePropInfoListController patches DISABLED (were breaking UI)

### CustomPropsPlugin.cs
- Plugin entry point
- PropSearchHelper for periodic injection attempts
- Scene load handling

---

## NEXT STEPS TO TRY

1. **Create/Clone EndlessProp Prefab**
   - The RuntimePropInfo.EndlessProp field is null
   - May need a valid prefab for UI to display

2. **Hook UI List Population**
   - Find where UI gets its prop list
   - Ensure our prop is included in that list

3. **Check ReferenceFilter**
   - UI may filter props by ReferenceFilter
   - Our prop may be filtered out

4. **Investigate Synchronize Method**
   - `UIRuntimePropInfoListModel.Synchronize(ReferenceFilter, IReadOnlyList<RuntimePropInfo>)`
   - Need to call this with our prop included

5. **Check if GetAllRuntimeProps Patch Works**
   - We patched this method
   - Verify our prop is returned when UI calls it

---

## PATCHES CURRENTLY ACTIVE

```
- StageManager.RegisterStage (Postfix) - triggers injection
- StageManager.LoadLevel (Prefix) - detects level loading
- PropLibrary.GetAllRuntimeProps (Prefix + Postfix)
- PropLibrary.FetchAndSpawnPropPrefab (Postfix)
- PropLibrary.InjectProp (Postfix)
- PropLibraryReference.GetReference (Postfix)
```

### DISABLED PATCHES
```
- UIRuntimePropInfoListController.Start (was breaking prop tool UI)
- UIRuntimePropInfoListController.Awake
- UIRuntimePropInfoListController.OnEnable
- UIRuntimePropInfoListController.Initialize
```

---

## CONCLUSION

The prop IS in the PropLibrary (verified by GetAllRuntimeProps returning 76 props and finding "Pearl Basket"). The problem is the UI layer - specifically how `UIRuntimePropInfoListModel` gets and displays the prop list. Direct manipulation of the UI list model may be required.
