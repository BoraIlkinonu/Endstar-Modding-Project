# PropIntegration.cs Fix Changelog

## Files
- **Original:** `PropIntegration.cs` (1835 lines)
- **Fixed:** `PropIntegration_FIXED.cs` (470 lines)

---

## Summary of Changes

The fixed version removes all patches and code that were interfering with the prop tool UI.

---

## Removed (Breaking UI)

### 1. `PatchPropLibraryReference()` - REMOVED
- Was patching `GetReference` method
- Could intercept UI's access to PropLibrary

### 2. `PatchPropUIComponents()` - REMOVED ENTIRELY
- Was patching methods containing "PropLibrary" or "SetLibrary"
- Was analyzing `UIRuntimePropInfoListModel`
- Was analyzing `UIRuntimePropInfoListController`
- **This was the main culprit for breaking the prop tool UI**

### 3. `GetReferencePostfix()` - REMOVED
- Was capturing PropLibrary from UI reference calls
- Called `ScheduleInjection()` which triggered immediate injection

### 4. `SetPropLibraryPrefix()` - REMOVED
- Was patching UI model methods
- Logged extensively on every call

### 5. `UIControllerInitPostfix()` - REMOVED
- Was hooking into UI controller initialization
- Could interfere with proper UI setup

### 6. `GetAllRuntimePropsPostfix()` - REMOVED
- Was iterating through ALL props on EVERY call
- Heavy reflection and logging on every UI update
- Now only using lightweight prefix for capture

### 7. `RefreshPropUI()` - REMOVED
- Was directly calling `Set()` on UI models
- Bypassed normal UI initialization flow
- **Let the game handle UI refresh naturally**

### 8. Immediate injection in `ScheduleInjection()` - REPLACED
- Was injecting immediately when PropLibrary captured
- Now delays injection by 2 seconds to let UI initialize first

---

## Simplified Patches (Kept)

| Patch | Target | Purpose |
|-------|--------|---------|
| `GetAllRuntimePropsPrefix` | PropLibrary | Capture instance (no logging) |
| `InjectPropPostfix` | PropLibrary | Log when injection happens |

---

## New Features

### Delayed Injection
```csharp
private static float _injectionDelaySeconds = 2.0f;
```
- Waits 2 seconds after capturing PropLibrary before injecting
- Ensures UI is fully initialized first

### Reset Methods
```csharp
ResetInjectionState()  // Soft reset for scene changes
FullReset()            // Complete reset
```

---

## How to Use

### Option 1: Replace the file
```
1. Backup original: PropIntegration.cs → PropIntegration_ORIGINAL.cs
2. Rename: PropIntegration_FIXED.cs → PropIntegration.cs
3. Rebuild plugin
```

### Option 2: Test first
```
1. Keep both files
2. In CustomPropsPlugin.cs, change the using/reference to use _FIXED version
3. Test if prop tool opens
4. If works, replace permanently
```

---

## Testing

After applying the fix:

1. Launch Endstar
2. Enter Creator mode in a level
3. Click the Prop Tool button
4. **Expected:** Prop tool panel should open and show props
5. Check BepInEx log for injection messages

---

## If Prop Tool Still Doesn't Open

The issue might not be in PropIntegration.cs. Check:

1. Other plugins/patches you have active
2. The main plugin file `CustomPropsPlugin.cs`
3. BepInEx log for exceptions during level load
4. Unity console for errors

---

## Code Size Comparison

| Version | Lines | Patches | UI Manipulation |
|---------|-------|---------|-----------------|
| Original | 1835 | 8+ | Yes (RefreshPropUI) |
| Fixed | 470 | 2 | No |

The fixed version is **74% smaller** and much safer.
