# Complete Prop Data Flow: Storage to UI Display

## Overview Diagram

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    ENDSTAR PROP SYSTEM - COMPLETE DATA FLOW                 │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ╔═══════════════════════════════════════════════════════════════════════╗  │
│  ║                         STORAGE LAYER                                 ║  │
│  ╠═══════════════════════════════════════════════════════════════════════╣  │
│  ║                                                                       ║  │
│  ║  PropLibrary (Singleton via StageManager.Instance.activePropLibrary)  ║  │
│  ║  ┌─────────────────────────────────────────────────────────────────┐  ║  │
│  ║  │                                                                 │  ║  │
│  ║  │  loadedPropMap                                                  │  ║  │
│  ║  │  Dictionary<AssetReference, RuntimePropInfo>                    │  ║  │
│  ║  │  ┌─────────────────────────────────────────────────────────┐    │  ║  │
│  ║  │  │ Key: AssetReference (from Prop.ToAssetReference())      │    │  ║  │
│  ║  │  │ Value: RuntimePropInfo {                                │    │  ║  │
│  ║  │  │   PropData: Prop,                                       │    │  ║  │
│  ║  │  │   Icon: Sprite,                                         │    │  ║  │
│  ║  │  │   EndlessProp: EndlessProp (has ReferenceFilter!),      │    │  ║  │
│  ║  │  │   IsLoading: bool,                                      │    │  ║  │
│  ║  │  │   IsMissingObject: bool                                 │    │  ║  │
│  ║  │  │ }                                                       │    │  ║  │
│  ║  │  └─────────────────────────────────────────────────────────┘    │  ║  │
│  ║  │                           │                                     │  ║  │
│  ║  │                           │ PopulateReferenceFilterMap()        │  ║  │
│  ║  │                           ▼                                     │  ║  │
│  ║  │  _referenceFilterMap                                            │  ║  │
│  ║  │  Dictionary<ReferenceFilter, List<RuntimePropInfo>>             │  ║  │
│  ║  │  ┌─────────────────────────────────────────────────────────┐    │  ║  │
│  ║  │  │ Key: ReferenceFilter enum value                         │    │  ║  │
│  ║  │  │   None (0): [props with no filter]                      │    │  ║  │
│  ║  │  │   Npc (2): [NPC props]                                  │    │  ║  │
│  ║  │  │   InventoryItem (8): [inventory props] ← UI USES THIS!  │    │  ║  │
│  ║  │  └─────────────────────────────────────────────────────────┘    │  ║  │
│  ║  │                                                                 │  ║  │
│  ║  └─────────────────────────────────────────────────────────────────┘  ║  │
│  ║                                                                       ║  │
│  ╚═══════════════════════════════════════════════════════════════════════╝  │
│                                      │                                      │
│                                      │ GetReferenceFilteredDefinitionList() │
│                                      ▼                                      │
│  ╔═══════════════════════════════════════════════════════════════════════╗  │
│  ║                          EVENT LAYER                                  ║  │
│  ╠═══════════════════════════════════════════════════════════════════════╣  │
│  ║                                                                       ║  │
│  ║  CreatorManager                                                       ║  │
│  ║  ┌─────────────────────────────────────────────────────────────────┐  ║  │
│  ║  │ OnPropsRepopulated: Action                                      │  ║  │
│  ║  │   └─> Subscribers:                                              │  ║  │
│  ║  │       [1] UIPropToolPanelView.OnLibraryRepopulated              │  ║  │
│  ║  │       [?] Our custom handler (if registered)                    │  ║  │
│  ║  └─────────────────────────────────────────────────────────────────┘  ║  │
│  ║                                                                       ║  │
│  ╚═══════════════════════════════════════════════════════════════════════╝  │
│                                      │                                      │
│                                      │ OnLibraryRepopulated()               │
│                                      ▼                                      │
│  ╔═══════════════════════════════════════════════════════════════════════╗  │
│  ║                           UI LAYER                                    ║  │
│  ╠═══════════════════════════════════════════════════════════════════════╣  │
│  ║                                                                       ║  │
│  ║  UIPropToolPanelView                                                  ║  │
│  ║  ┌─────────────────────────────────────────────────────────────────┐  ║  │
│  ║  │ runtimePropInfoListModel: UIRuntimePropInfoListModel            │  ║  │
│  ║  │                                                                 │  ║  │
│  ║  │ OnLibraryRepopulated():                                         │  ║  │
│  ║  │   filter = GetCurrentFilter()  // e.g., InventoryItem           │  ║  │
│  ║  │   ignoreList = GetIgnoreList() // props to exclude              │  ║  │
│  ║  │   runtimePropInfoListModel.Synchronize(filter, ignoreList)      │  ║  │
│  ║  └─────────────────────────────────────────────────────────────────┘  ║  │
│  ║                                      │                                ║  │
│  ║                                      │ Synchronize()                  ║  │
│  ║                                      ▼                                ║  │
│  ║  UIRuntimePropInfoListModel                                           ║  │
│  ║  ┌─────────────────────────────────────────────────────────────────┐  ║  │
│  ║  │ List: List<RuntimePropInfo>  ← THIS IS WHAT UI DISPLAYS!        │  ║  │
│  ║  │                                                                 │  ║  │
│  ║  │ Synchronize(filter, ignoreList):                                │  ║  │
│  ║  │   props = PropLibrary.GetReferenceFilteredDefinitionList(filter)│  ║  │
│  ║  │   props = props.Except(ignoreList)                              │  ║  │
│  ║  │   List.Clear()                                                  │  ║  │
│  ║  │   List.AddRange(props)                                          │  ║  │
│  ║  │                                                                 │  ║  │
│  ║  │ Add(item, triggerEvents):                                       │  ║  │
│  ║  │   List.Add(item)  ← CAN ADD DIRECTLY!                           │  ║  │
│  ║  │   if (triggerEvents) NotifyObservers()                          │  ║  │
│  ║  └─────────────────────────────────────────────────────────────────┘  ║  │
│  ║                                      │                                ║  │
│  ║                                      ▼                                ║  │
│  ║  UIRuntimePropInfoListView                                            ║  │
│  ║  ┌─────────────────────────────────────────────────────────────────┐  ║  │
│  ║  │ Observes List from UIRuntimePropInfoListModel                   │  ║  │
│  ║  │ Renders visual elements for each RuntimePropInfo                │  ║  │
│  ║  │ User sees props here!                                           │  ║  │
│  ║  └─────────────────────────────────────────────────────────────────┘  ║  │
│  ║                                                                       ║  │
│  ╚═══════════════════════════════════════════════════════════════════════╝  │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

## Data Transformation Steps

| Step | Data Location | Data Format |
|------|---------------|-------------|
| 1 | Game Database | Serialized prop assets |
| 2 | loadedPropMap | Dict<AssetReference, RuntimePropInfo> |
| 3 | _referenceFilterMap | Dict<ReferenceFilter, List<RuntimePropInfo>> |
| 4 | GetReferenceFilteredDefinitionList result | IReadOnlyList<RuntimePropInfo> |
| 5 | UIRuntimePropInfoListModel.List | List<RuntimePropInfo> |
| 6 | UIRuntimePropInfoListView | Visual UI elements |

## Where Our Prop Gets Lost

### Current Flow (BROKEN)
```
Our prop added to loadedPropMap ✓
         │
         │ BUT EndlessProp.ReferenceFilter = None (0)
         │     OR PopulateReferenceFilterMap not called after
         ▼
_referenceFilterMap[InventoryItem] does NOT contain our prop ✗
         │
         ▼
GetReferenceFilteredDefinitionList(InventoryItem) returns list WITHOUT our prop
         │
         ▼
Synchronize populates List WITHOUT our prop
         │
         ▼
UI shows props but NOT ours
```

### Fixed Flow (Option A - Fix Filter)
```
Our prop added to loadedPropMap ✓
         │
         │ WITH EndlessProp.ReferenceFilter = InventoryItem (8)
         │
         ▼
PopulateReferenceFilterMap() called AFTER our injection
         │
         ▼
_referenceFilterMap[InventoryItem] CONTAINS our prop ✓
         │
         ▼
GetReferenceFilteredDefinitionList(InventoryItem) returns list WITH our prop
         │
         ▼
Synchronize populates List WITH our prop ✓
         │
         ▼
UI shows our prop! ✓
```

### Fixed Flow (Option B - Direct Add)
```
Normal flow completes (without our prop)
         │
         ▼
Synchronize populates List (base props only)
         │
         ▼
Our postfix runs: listModel.Add(ourProp, true)
         │
         ▼
List now contains our prop ✓
         │
         ▼
UI shows our prop! ✓
```

## Implementation Recommendation

**Use Option B (Direct Add)** because:

1. Simpler implementation
2. No timing issues
3. No need to create valid EndlessProp with correct ReferenceFilter
4. Single hook point (postfix on OnLibraryRepopulated)
5. Guaranteed to work if UIPropToolPanelView is found

## Code Implementation

```csharp
[HarmonyPatch]
class UIListPatch
{
    static Type _panelViewType;
    static FieldInfo _listModelField;
    static MethodInfo _addMethod;

    static void Initialize()
    {
        var creatorAsm = AppDomain.CurrentDomain.GetAssemblies()
            .First(a => a.GetName().Name == "Creator");

        _panelViewType = creatorAsm.GetTypes()
            .First(t => t.Name == "UIPropToolPanelView");

        _listModelField = _panelViewType.GetField("runtimePropInfoListModel",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        var listModelType = _listModelField.FieldType;
        _addMethod = listModelType.GetMethod("Add",
            BindingFlags.Public | BindingFlags.Instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(UIPropToolPanelView), "OnLibraryRepopulated")]
    static void OnLibraryRepopulated_Postfix(object __instance)
    {
        var listModel = _listModelField.GetValue(__instance);
        _addMethod.Invoke(listModel, new object[] { OurRuntimePropInfo, true });
    }
}
```
