# CUSTOM PROP INJECTION - CODE-BACKED IMPLEMENTATION PLAN

Based on comprehensive DLL analysis of Endstar's prop system.

**UPDATED: 2026-01-06** - See CURRENT_STATUS.md for latest findings.

---

## CURRENT STATUS: PARTIAL SUCCESS
- Prop successfully injected into PropLibrary (76 props, was 75)
- Prop NOT visible in UI prop tool panel
- Root cause: UI layer not refreshing/including injected prop

---

## OBJECTIVE
Inject unlimited custom props into Endstar's native prop system with full UI integration (visible, selectable, placeable in level editor).

---

## PHASE 1: PROPER TIMING (Hook Stage Creation)

### Problem
Previous attempts failed because injection happened before Stage existed.
Log showed: `"No Stage in scene - not in a level yet, skipping injection"`

### Solution
Hook into `StageManager.RegisterStage(Stage stage)` method - this is called when a stage is created and registered.

### Code Implementation
```csharp
// Harmony patch for StageManager.RegisterStage
[HarmonyPatch]
public class StageRegistrationPatch
{
    static MethodBase TargetMethod()
    {
        var stageManagerType = AccessTools.TypeByName("Endless.Gameplay.LevelEditing.Level.StageManager");
        return AccessTools.Method(stageManagerType, "RegisterStage");
    }

    static void Postfix(object __instance)
    {
        // __instance is StageManager
        // Now Stage exists, safe to inject props
        CustomPropsPlugin.Log.LogInfo("Stage registered - now injecting custom props");
        PropInjector.InjectAllCustomProps(__instance);
    }
}
```

### Why This Works
- `RegisterStage` is called after Stage is fully created
- At this point, `activePropLibrary` is valid
- All required references (basePropPrefab, prefabSpawnRoot) are available

---

## PHASE 2: RETRIEVE REQUIRED REFERENCES

### Required References (from DLL analysis)
1. `StageManager.Instance` - singleton access
2. `StageManager.activePropLibrary` - PropLibrary instance
3. `StageManager.basePropPrefab` - EndlessProp template
4. `PropLibrary.prefabSpawnRoot` - Transform for spawning

### Code Implementation
```csharp
public static class PropInjector
{
    public static void InjectAllCustomProps(object stageManager)
    {
        // Get activePropLibrary field
        var propLibField = stageManager.GetType().GetField("activePropLibrary",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var propLibrary = propLibField.GetValue(stageManager);

        if (propLibrary == null)
        {
            Log.LogWarning("activePropLibrary is null!");
            return;
        }

        // Get basePropPrefab from StageManager
        var basePrefabField = stageManager.GetType().GetField("basePropPrefab",
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
        var basePropPrefab = basePrefabField.GetValue(stageManager);

        // Get prefabSpawnRoot from PropLibrary
        var spawnRootField = propLibrary.GetType().GetField("prefabSpawnRoot",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var prefabSpawnRoot = spawnRootField.GetValue(propLibrary) as Transform;

        Log.LogInfo($"Got references: PropLibrary={propLibrary != null}, BasePrefab={basePropPrefab != null}, SpawnRoot={prefabSpawnRoot != null}");

        // Now inject props
        foreach (var customProp in PropRegistry.GetAll())
        {
            InjectSingleProp(propLibrary, customProp, prefabSpawnRoot, basePropPrefab);
        }
    }
}
```

---

## PHASE 3: CREATE PROPER PROP INSTANCE

### Prop Type Details (from DLL analysis)
- Class: `Endless.Props.Assets.Prop`
- Inherits: `Asset -> AssetCore` (NOT ScriptableObject)
- Has default constructor: `public Prop()`

### Required Fields
```
baseTypeId (String) - needs valid ID from existing props
Name (String) - display name (inherited from AssetCore)
AssetID (String) - unique GUID (inherited from AssetCore)
bounds (Vector3Int) - bounding box size
```

### Code Implementation
```csharp
private static object CreatePropInstance(PropData customProp)
{
    // Get Prop type
    var propAssembly = AppDomain.CurrentDomain.GetAssemblies()
        .First(a => a.GetName().Name == "Props");
    var propType = propAssembly.GetType("Endless.Props.Assets.Prop");

    // Create instance using default constructor
    var prop = Activator.CreateInstance(propType);

    // Set Name (inherited from AssetCore)
    var nameField = propType.GetField("Name", BindingFlags.Public | BindingFlags.Instance);
    if (nameField != null)
        nameField.SetValue(prop, customProp.DisplayName);

    // Set AssetID (inherited from AssetCore)
    var assetIdField = propType.GetField("AssetID", BindingFlags.Public | BindingFlags.Instance);
    if (assetIdField != null)
        assetIdField.SetValue(prop, customProp.PropId);

    // Set baseTypeId - CRITICAL: need to use a valid base type
    // Options: copy from existing prop, or use a known valid ID
    var baseTypeField = propType.GetField("baseTypeId",
        BindingFlags.NonPublic | BindingFlags.Instance);
    if (baseTypeField != null)
        baseTypeField.SetValue(prop, "custom_prop"); // May need valid ID

    // Set bounds
    var boundsField = propType.GetField("bounds",
        BindingFlags.NonPublic | BindingFlags.Instance);
    if (boundsField != null)
        boundsField.SetValue(prop, new Vector3Int(1, 1, 1));

    return prop;
}
```

### Finding Valid baseTypeId
Need to examine existing props to get a valid baseTypeId:
```csharp
private static string GetValidBaseTypeId(object propLibrary)
{
    // Call GetAllRuntimeProps to get existing props
    var getAllMethod = propLibrary.GetType().GetMethod("GetAllRuntimeProps");
    var allProps = getAllMethod.Invoke(propLibrary, null) as Array;

    if (allProps != null && allProps.Length > 0)
    {
        var firstProp = allProps.GetValue(0);
        var propDataField = firstProp.GetType().GetField("PropData");
        var propData = propDataField.GetValue(firstProp);

        var baseTypeField = propData.GetType().GetField("baseTypeId",
            BindingFlags.NonPublic | BindingFlags.Instance);
        return baseTypeField.GetValue(propData) as string;
    }

    return null;
}
```

---

## PHASE 4: CALL INJECT METHOD

### Two Options

**Option A: StageManager.InjectProp (4 params)**
```csharp
// Signature: Void InjectProp(Prop prop, GameObject testPrefab, Script testScript, Sprite icon)
var injectMethod = stageManager.GetType().GetMethod("InjectProp",
    BindingFlags.Public | BindingFlags.Instance);

injectMethod.Invoke(stageManager, new object[] {
    prop,           // Endless.Props.Assets.Prop
    testPrefab,     // GameObject (can be null for testing)
    null,           // Script (can be null)
    icon            // Sprite
});
```

**Option B: PropLibrary.InjectProp (6 params)**
```csharp
// Signature: Void InjectProp(Prop prop, GameObject testPrefab, Script testScript, Sprite icon, Transform prefabSpawnTransform, EndlessProp propPrefab)
var injectMethod = propLibrary.GetType().GetMethod("InjectProp",
    BindingFlags.Public | BindingFlags.Instance);

injectMethod.Invoke(propLibrary, new object[] {
    prop,               // Endless.Props.Assets.Prop
    testPrefab,         // GameObject
    null,               // Script
    icon,               // Sprite
    prefabSpawnRoot,    // Transform
    basePropPrefab      // EndlessProp
});
```

### CRITICAL: InjectProp is ASYNC
The method is async internally. Need to handle this:
```csharp
// After calling InjectProp, wait or use coroutine
private static IEnumerator InjectAndWait(object propLibrary, object[] args)
{
    var injectMethod = propLibrary.GetType().GetMethod("InjectProp");
    injectMethod.Invoke(propLibrary, args);

    // Wait for async to complete
    yield return new WaitForSeconds(0.5f);

    // Verify injection
    var getAllMethod = propLibrary.GetType().GetMethod("GetAllRuntimeProps");
    var allProps = getAllMethod.Invoke(propLibrary, null) as Array;
    Log.LogInfo($"After injection: {allProps?.Length} props in library");
}
```

---

## PHASE 5: FORCE UI REFRESH

### Problem
Even if injection succeeds, UI may not update automatically.

### Solution
Call `UIRuntimePropInfoListModel.Synchronize` or trigger refresh.

### Code Implementation
```csharp
private static void RefreshPropUI()
{
    // Find UIRuntimePropInfoListModel in scene
    var creatorAssembly = AppDomain.CurrentDomain.GetAssemblies()
        .First(a => a.GetName().Name == "Creator");
    var listModelType = creatorAssembly.GetType("Endless.Creator.UI.UIRuntimePropInfoListModel");

    var listModel = UnityEngine.Object.FindObjectOfType(listModelType);
    if (listModel == null)
    {
        Log.LogWarning("UIRuntimePropInfoListModel not found in scene");
        return;
    }

    // Get PropLibrary's props
    var propLibrary = GetActivePropLibrary();
    var getAllMethod = propLibrary.GetType().GetMethod("GetAllRuntimeProps");
    var allProps = getAllMethod.Invoke(propLibrary, null);

    // Call Synchronize
    // Signature: Synchronize(ReferenceFilter, IReadOnlyList<RuntimePropInfo>)
    var syncMethod = listModelType.GetMethod("Synchronize");

    // May need to convert array to list and get ReferenceFilter
    // This needs more investigation on exact call signature
}
```

### Alternative: Hook UI Update
```csharp
// Patch GetAllRuntimeProps to include our props
[HarmonyPatch]
public class GetAllRuntimePropsPatch
{
    static MethodBase TargetMethod()
    {
        var propLibType = AccessTools.TypeByName("Endless.Gameplay.LevelEditing.PropLibrary");
        return AccessTools.Method(propLibType, "GetAllRuntimeProps");
    }

    static void Postfix(ref Array __result)
    {
        // After game returns props, append our custom props
        // This ensures our props always appear in the list
    }
}
```

---

## PHASE 6: COMPLETE INJECTION FLOW

```csharp
public static class CustomPropInjector
{
    private static bool _injected = false;

    public static void OnStageRegistered(object stageManager)
    {
        if (_injected) return; // Only inject once per stage
        _injected = true;

        try
        {
            // Step 1: Get references
            var propLibrary = GetField(stageManager, "activePropLibrary");
            var basePrefab = GetField(stageManager, "basePropPrefab");
            var spawnRoot = GetField(propLibrary, "prefabSpawnRoot") as Transform;

            // Step 2: Get valid baseTypeId from existing prop
            var baseTypeId = GetValidBaseTypeIdFromExistingProp(propLibrary);

            // Step 3: Create and inject each custom prop
            foreach (var customProp in PropRegistry.GetAll())
            {
                // Create Prop instance
                var prop = CreatePropInstance(customProp, baseTypeId);

                // Create icon
                var icon = customProp.Icon ?? CreateDefaultIcon(customProp.DisplayName);

                // Inject via StageManager (simpler)
                var injectMethod = stageManager.GetType().GetMethod("InjectProp");
                injectMethod.Invoke(stageManager, new object[] {
                    prop,
                    customProp.Prefab, // Test prefab (our custom mesh)
                    null,              // Script (optional)
                    icon
                });

                Log.LogInfo($"Injected: {customProp.PropId}");
            }

            // Step 4: Wait and verify
            CoroutineRunner.Instance.StartCoroutine(VerifyInjection(propLibrary));
        }
        catch (Exception ex)
        {
            Log.LogError($"Injection failed: {ex}");
        }
    }

    private static IEnumerator VerifyInjection(object propLibrary)
    {
        yield return new WaitForSeconds(1f);

        var getAllMethod = propLibrary.GetType().GetMethod("GetAllRuntimeProps");
        var allProps = getAllMethod.Invoke(propLibrary, null) as Array;

        Log.LogInfo($"Verification: {allProps?.Length} props now in PropLibrary");

        // Force UI refresh if needed
        RefreshPropUI();
    }
}
```

---

## IMPLEMENTATION CHECKLIST

- [ ] 1. Patch `StageManager.RegisterStage` to trigger injection at right time
- [ ] 2. Retrieve `activePropLibrary`, `basePropPrefab`, `prefabSpawnRoot`
- [ ] 3. Get valid `baseTypeId` from existing prop
- [ ] 4. Create `Prop` instance with correct field values
- [ ] 5. Create icon `Sprite` for UI
- [ ] 6. Call `InjectProp` via StageManager or PropLibrary
- [ ] 7. Wait for async completion
- [ ] 8. Verify prop appears in `GetAllRuntimeProps()`
- [ ] 9. Force UI refresh via `UIRuntimePropInfoListModel.Synchronize` if needed
- [ ] 10. Test prop is visible/selectable in prop tool panel

---

## RISKS AND MITIGATIONS

### Risk 1: baseTypeId validation
- **Problem**: Invalid baseTypeId may cause issues
- **Mitigation**: Copy from existing prop, or experiment with values

### Risk 2: Async completion
- **Problem**: InjectProp is async, prop not immediately available
- **Mitigation**: Use coroutine to wait and verify

### Risk 3: UI not refreshing
- **Problem**: Prop injected but UI doesn't show it
- **Mitigation**: Force call Synchronize or patch GetAllRuntimeProps

### Risk 4: Prefab requirements
- **Problem**: EndlessProp component may have specific requirements
- **Mitigation**: Clone from basePropPrefab and modify

---

## NEXT STEPS

1. **User approval** of this plan
2. **Implement Phase 1**: Patch StageManager.RegisterStage
3. **Implement Phase 2**: Get all required references
4. **Implement Phase 3**: Create proper Prop instances
5. **Implement Phase 4**: Call InjectProp
6. **Implement Phase 5**: Handle UI refresh
7. **Test and iterate**

---

This plan is based on actual DLL analysis, not assumptions.
All code snippets reference actual types and methods found in the game's assemblies.
