# Experimentation Log - Custom Props Injection

## Session: 2026-01-06

### Experiment 1: Basic Prop Creation with ScriptableObject
**What was tried:** Creating Prop using `ScriptableObject.CreateInstance<Prop>()`
**Result:** FAILED - null returned
**Root cause discovered:** Prop is NOT a ScriptableObject. It extends `Asset → AssetCore → System.Object`
**Fix:** Use `Activator.CreateInstance(_propType)` instead

### Experiment 2: Setting Fields on Prop
**What was tried:** Setting `AssetID` directly on Prop instance
**Result:** FAILED - "Non-static field requires a target"
**Root cause discovered:** `AssetID` is in base class `Asset`, not `Prop`. Simple `GetField()` doesn't find inherited fields.
**Fix:** Created `SetFieldValue` helper that walks up the inheritance chain

### Experiment 3: Adding to injectedPropIds
**What was tried:** Adding string GUID to `injectedPropIds` assuming it was `HashSet<string>`
**Result:** FAILED - Type mismatch
**Root cause discovered:** `injectedPropIds` is `List<SerializableGuid>`, not `HashSet<string>`
**Fix:** Create `SerializableGuid` using string constructor, then add to list

### Experiment 4: Stub Prop Injection
**What was tried:** Created minimal Prop with only AssetID, Name, Description set
**Result:** FAILED - NullReferenceException in `PropLibrary.PopulateReferenceFilterMap()`
**Root cause:** Stub prop was missing required fields that `PopulateReferenceFilterMap` accesses:
- baseTypeId
- prefabBundle
- componentIds
- etc.
**Fix:** Clone a REAL existing prop and only change ID/Name (HOOK E created)

### Experiment 5: Clone-based Prop Injection with Real Assets
**What was tried:**
1. Load asset bundle with PearlBasket.prefab and icon
2. Clone existing prop's data to get ALL required fields
3. Create EndlessProp by instantiating basePropPrefab
4. Only change ID, Name, Description on cloned prop

**Result:** FAILED - "The Object you want to instantiate is null"
**Root cause discovered:** `basePropPrefab` is an `EndlessProp` component (which is a MonoBehaviour), not a GameObject. Casting to `GameObject` returns null.
**Fix applied:** Get `.gameObject` from the component before instantiating:
```csharp
var basePropComponent = basePropPrefab as Component;
var basePropGO = basePropComponent.gameObject;
endlessPropGO = UnityEngine.Object.Instantiate(basePropGO);
```
**Status:** Code edited but NOT YET TESTED

---

## Key Learnings

1. **Prop hierarchy:** `Prop → Asset → AssetCore → Object` (NOT ScriptableObject)
2. **injectedPropIds type:** `List<SerializableGuid>` (NOT HashSet<string>)
3. **basePropPrefab type:** `EndlessProp` component (NOT GameObject)
4. **Critical rule:** Never create stub objects - always clone real ones

---

## Outstanding Questions (Need DLL Research)

1. **What does PopulateReferenceFilterMap() access on the Prop?**
   - Need to decompile or analyze this method
   - It crashed on our stub prop - what fields did it try to read?

2. **How does the game's own InjectProp work?**
   - StageManager.InjectProp(Prop prop, GameObject testPrefab, Script testScript, Sprite icon)
   - What does this method actually do internally?

3. **What is ReferenceFilter and how is it determined from a Prop?**
   - EndlessProp has `ReferenceFilter ReferenceFilter { get; }`
   - How does the prop data connect to this?

4. **What is the correct structure for EndlessProp to work?**
   - basePropPrefab has certain components
   - What's the minimum required structure?

---

## Next Steps

1. Run DLL analysis on:
   - `PropLibrary.PopulateReferenceFilterMap()`
   - `StageManager.InjectProp()` internal implementation
   - `EndlessProp` structure and requirements

2. Understand the relationship between:
   - Prop (data class)
   - EndlessProp (MonoBehaviour component)
   - RuntimePropInfo (wrapper for both + icon)
