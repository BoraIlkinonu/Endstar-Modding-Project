# Full Plugin Analysis - Why Prop Tool Doesn't Open

## Critical Finding: TWO PARALLEL INJECTION SYSTEMS

Your plugin has **TWO separate systems** both trying to inject props:

1. **PropSystemPatches** (in `PropIntegration.cs`)
2. **ProperPropInjector** (in `ProperPropInjector.cs`)

Both patch similar methods and both try to manipulate the UI!

---

## Duplicate Harmony Patches

### GetAllRuntimeProps - PATCHED TWICE!

**PropIntegration.cs (line 384-393):**
```csharp
harmony.Patch(getAllMethod, new HarmonyMethod(prefix), new HarmonyMethod(postfix));
// GetAllRuntimePropsPrefix + GetAllRuntimePropsPostfix
```

**ProperPropInjector.cs (line 134-141):**
```csharp
harmony.Patch(getAllPropsMethod, new HarmonyMethod(prefix), null);
// OnGetAllRuntimeProps
```

**Result:** Every call to `GetAllRuntimeProps` runs 3 patches!

---

## Both Systems Have RefreshPropUI!

### PropIntegration.cs (lines 1345-1450):
```csharp
private static void RefreshPropUI()
{
    // Directly manipulates UIRuntimePropInfoListModel
    setMethod.Invoke(model, new object[] { list, true });
}
```

### ProperPropInjector.cs (lines 886-947):
```csharp
private static void RefreshPropUI()
{
    // Also tries to manipulate UIRuntimePropInfoListModel
    // Called from VerifyAndRefreshUI coroutine
}
```

**Result:** Both systems try to manipulate the UI, likely causing conflicts.

---

## Heavy Startup Analysis

**CustomPropsPlugin.Awake() runs:**
1. `AnalyzeGameLibraries()` - iterates ALL types in 3 assemblies
2. `DeepDLLAnalyzer.AnalyzePropSystem()` - extensive DLL analysis
3. `ComprehensiveDLLReader.ReadAllPropRelatedCode()` - comprehensive analysis

This runs EVERY time the plugin loads and could cause:
- Slow startup
- File I/O during initialization
- Potential race conditions

---

## Multiple Update Loops

### PropSearchHelper.Update() (lines 30-56):
```csharp
void Update()
{
    // Every 3 seconds:
    ProperPropInjector.TryInjectNow();
}
```

### CustomPropsPlugin.Update() (lines 485-541):
```csharp
private void Update()
{
    // Every 3 seconds:
    ProperPropInjector.TryInjectNow();
}
```

**Result:** `TryInjectNow()` is called TWICE every 3 seconds from two different Update loops!

---

## OnSceneLoaded Calls Both Systems

**CustomPropsPlugin.OnSceneLoaded() (lines 199-227):**
```csharp
ProperPropInjector.Reset();
ProperPropInjector.TryInjectNow();  // System 1
PropSystemPatches.TryInjectProps(); // System 2
```

---

## Summary of Problems

| Issue | File | Impact |
|-------|------|--------|
| Duplicate GetAllRuntimeProps patch | Both | 3 patches run per call |
| Two RefreshPropUI implementations | Both | UI manipulation conflicts |
| Heavy startup analysis | CustomPropsPlugin | Slow startup |
| Double Update loop | CustomPropsPlugin + Helper | Injection called 2x/interval |
| Both systems patch PropLibrary | Both | Harmony conflicts |
| OnSceneLoaded calls both | CustomPropsPlugin | Race condition |

---

## Root Cause

The prop tool window not opening is most likely caused by:

1. **Harmony patch conflicts** - same method patched multiple times
2. **UI manipulation from both systems** - RefreshPropUI called by both
3. **Race conditions** - both systems trying to inject at the same time
4. **Heavy logging** - slowing down UI updates

---

## Recommended Fix

### Option 1: Keep ONLY ProperPropInjector (Recommended)

1. Remove all PropSystemPatches code from PropIntegration.cs
2. Remove RefreshPropUI from ProperPropInjector
3. Remove duplicate Update loop (keep only one)
4. Disable heavy startup analysis

### Option 2: Keep ONLY PropSystemPatches

1. Remove ProperPropInjector entirely
2. Use the fixed PropIntegration_FIXED.cs
3. Remove duplicate Update loop

### Option 3: Disable Everything Temporarily

Add this at the start of Awake():
```csharp
return; // DISABLE ENTIRE PLUGIN FOR TESTING
```

If prop tool works after this, the plugin is definitely the cause.
