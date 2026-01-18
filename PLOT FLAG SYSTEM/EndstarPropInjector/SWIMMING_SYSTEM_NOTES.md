# Swimming System Implementation - Documentation

## Goal
Implement underwater swimming mechanics for Endstar Unity game via BepInEx + HarmonyX plugin.

**Requirements:**
- No death underwater (prevent ProcessFallOffStage from killing player)
- No sinking/gravity underwater
- Q/E vertical swimming controls with bounds
- Surface ceiling (player can swim up until waist above water)
- Space to jump out at water surface
- **CRITICAL**: After exiting water, player must land on ground normally

## Status: UNSOLVED

The core issue of **player floating after exiting water** was never resolved despite multiple attempts.

---

## Versions Attempted

### v4.0.0 - v5.x
- Initial attempts at gravity blocking
- Various approaches to TotalForce manipulation

### v6.0.0
- Diagnostic version confirming SetValue reflection works for struct modification
- NetState is a struct (value type) requiring boxing/unboxing

### v6.1.0
- Swapped Q/E controls
- Changed from adding to TotalForce.y to setting it directly
- **Issue**: E key caused player to rocket into sky

### v6.2.0
- Added transition handling for water exit
- **Issue**: Player floats after gentle exit

### v6.2.1
- Added full state reset on water exit
- Added cooldown after exit
- **Issue**: "Character just floats in the air, doesn't know it's out of water"

### v6.3.0
- Added hard position check
- **Issue**: Same floating problem

### v7.0.0 - Ghost Mode Pattern
- Discovered line 715 in ProcessGroundCalculation_NetFrame:
  ```csharp
  if (this.currentState.TotalForce.y + this.currentState.CalculatedMotion.y < 1f)
  {
      // Ground detection raycasts happen here
  }
  ```
- Theory: When TotalForce.y + CalculatedMotion.y >= 1, ground detection is skipped
- Implemented ghost mode pattern:
  - `BlockGravity = true` (prevent gravity)
  - `TotalForce.y = 0` (keep ground detection working)
  - `CalculatedMotion.y = swimSpeed` (controlled movement)
- **Issue**: Still floating after exit

### v7.0.1
- Fixed water surface threshold bug (was using dynamic fallOffHeight instead of stored WaterSurfaceY)
- **Issue**: Still floating after exit

---

## Technical Findings

### NetState Structure
- `NetState` is a **struct** (value type), not a class
- Requires boxing/unboxing with reflection:
  ```csharp
  object boxedState = _currentStateField.GetValue(__instance);
  // modify fields...
  _currentStateField.SetValue(__instance, boxedState);  // Write back!
  ```

### Key Fields in NetState
| Field | Type | Purpose |
|-------|------|---------|
| `TotalForce` | Vector3 | Accumulated physics velocity |
| `CalculatedMotion` | Vector3 | Input-based movement |
| `BlockGravity` | bool | If true, gravity not applied in ProcessPhysics |
| `FramesSinceStableGround` | int | Frames since last grounded |
| `IsOnGround` | bool | Ground contact state |

### Method Call Order (ProcessNetFrame)
```
Line 335: ProcessInput_NetFrame()
Line 336: ProcessJump_NetFrame()
Line 337: ProcessPhysics_NetFrame()      <- Our postfix runs here
Line 338: ProcessGroundCalculation_NetFrame()
Line 340: vector = TotalForce + CalculatedMotion
Line 341: CharacterController.Move(vector * deltaTime)
```

### Ground Detection Threshold (Line 715)
Ground raycasts only happen when combined Y velocity < 1.0:
```csharp
if (this.currentState.TotalForce.y + this.currentState.CalculatedMotion.y < 1f)
```

### Ghost Mode Pattern (ProcessGhost_NetFrame)
```csharp
this.currentState.TotalForce = Vector3.zero;
this.currentState.CalculatedMotion.y = verticalSpeed;
```

### Water Detection
- `DepthPlane` class controls water surface and death plane
- `FallOffHeight` property = death trigger Y position
- `PlaneObject` = water surface mesh
- `DeeperPlane` = solid color plane below water

---

## What Works
- Death prevention underwater (ProcessFallOffStage_Prefix returning false)
- Swimming activation after falling 1 unit below surface
- Q/E vertical movement while underwater
- Water surface detection
- Shader/volume modifications for underwater visuals

## What Doesn't Work
- **Player lands properly after exiting water**
- Character floats at toe-tip height after swimming up and exiting
- Ground detection appears to not re-engage after swimming
- A "big jump" resets things properly, but gentle exit does not

---

## Theories Not Fully Tested

1. **Animation State**: Swimming animation may lock physics state
2. **CharacterController.isGrounded**: May need explicit reset
3. **Multiple Frame Delay**: Ground detection may need multiple frames to re-engage
4. **ResetTempFrameValues()**: May not be called in our exit path
5. **Other State Fields**: There may be additional fields affecting ground detection beyond what we modified

---

## Potential Alternative Approaches

1. **Replace PlayerController entirely**: Inject custom controller asset at runtime
2. **Patch ProcessGroundCalculation directly**: Force ground detection raycast
3. **Patch CharacterController.Move**: Intercept and modify movement
4. **Use Transpiler**: Modify IL code directly instead of Prefix/Postfix
5. **Force grounded state**: Directly set IsOnGround and FramesSinceStableGround after exit

---

## Files

- **Plugin Source**: `D:\Endstar Plot Flag\PLOT FLAG SYSTEM\EndstarPropInjector\Plugin.cs`
- **Decompiled PlayerController**: `D:\Endstar Plot Flag\DECOMPILED_GAME\Gameplay\Player\PlayerController.cs`
- **Decompiled DepthPlane**: `D:\Endstar Plot Flag\DECOMPILED_GAME\Props\StagePlayerManager\DepthPlane.cs`

---

## Conclusion

The swimming mechanics (movement, bounds, death prevention) work. The unsolved problem is the state transition when exiting water - the game's physics/ground detection system does not properly re-engage, causing the player to float.

The root cause is likely deeper in the PlayerController state machine than what Harmony Postfix patches can reliably control. A more invasive approach (Transpiler, asset replacement, or direct IL modification) may be required.
