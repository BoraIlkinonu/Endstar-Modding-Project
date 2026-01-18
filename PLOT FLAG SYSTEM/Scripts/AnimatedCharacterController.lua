-- AnimatedCharacterController.lua
-- Controls animated character with Main and ThroughDoor transforms
-- For Endstar Engine

local visuals = nil
local initialized = false
local isMainActive = false
local isThroughDoorActive = false
local currentLoop = 0

function Initialize()
    if initialized then return end

    visuals = MyContext:TryGetComponent(Component.Visuals)
    if visuals then
        print("[AnimChar] Visuals component found")
    else
        print("[AnimChar] ERROR: No Visuals component on this prop")
    end

    initialized = true
end

function DisableAll()
    if visuals then
        if Transforms.Main then
            visuals:SetEnabled(MyContext, Transforms.Main, false)
        end
        if Transforms.ThroughDoor then
            visuals:SetEnabled(MyContext, Transforms.ThroughDoor, false)
        end
    end
    isMainActive = false
    isThroughDoorActive = false
end

-- Called after each animation loop completes
function OnLoopComplete()
    print("[AnimChar] >>> OnLoopComplete CALLED <<<")
    currentLoop = currentLoop + 1
    print("[AnimChar] Loop " .. currentLoop .. "/" .. MainLoopCount .. " completed")

    if currentLoop >= MainLoopCount then
        -- All loops done - switch to ThroughDoor
        print("[AnimChar] All loops complete, switching to ThroughDoor")
        SwitchToThroughDoor()
    else
        -- Schedule next loop check
        print("[AnimChar] Scheduling next loop in " .. AnimationDuration .. " seconds")
        Invoke("OnLoopComplete", AnimationDuration)
    end
end

function SwitchToThroughDoor()
    print("[AnimChar] >>> SwitchToThroughDoor CALLED <<<")
    if visuals then
        if Transforms.Main then
            visuals:SetEnabled(MyContext, Transforms.Main, false)
            print("[AnimChar] Main disabled")
        end
        if Transforms.ThroughDoor then
            visuals:SetEnabled(MyContext, Transforms.ThroughDoor, true)
            print("[AnimChar] ThroughDoor enabled")
        else
            print("[AnimChar] WARNING: Transforms.ThroughDoor is nil")
        end
    end
    isMainActive = false
    isThroughDoorActive = true

    FireEvent(Events.OnMainCompleted, MyContext)
    FireEvent(Events.OnThroughDoorStarted, MyContext)

    -- If ThroughDoor has a duration, schedule its completion
    if ThroughDoorDuration > 0 then
        Invoke("OnThroughDoorComplete", ThroughDoorDuration)
    end
end

function OnThroughDoorComplete()
    print("[AnimChar] ThroughDoor animation complete")
    FireEvent(Events.OnThroughDoorCompleted, MyContext)

    if AutoHideAfterThroughDoor then
        StopThroughDoor(MyContext)
    end
end

-- RECEIVER: Start the Main animation sequence
function StartMain(context)
    Initialize()

    print("[AnimChar] StartMain called")
    print("[AnimChar] MainLoopCount = " .. tostring(MainLoopCount))
    print("[AnimChar] AnimationDuration = " .. tostring(AnimationDuration))

    if not visuals then
        print("[AnimChar] ERROR: No Visuals component")
        return
    end

    -- Cancel any pending invokes
    CancelInvoke("OnLoopComplete")
    CancelInvoke("OnThroughDoorComplete")

    -- Reset state
    currentLoop = 0

    -- Only disable ThroughDoor, don't touch Main to avoid rotation reset
    if Transforms.ThroughDoor then
        visuals:SetEnabled(MyContext, Transforms.ThroughDoor, false)
    end

    -- Enable Main only if not already active (avoid rotation reset)
    if not isMainActive then
        if Transforms.Main then
            visuals:SetEnabled(MyContext, Transforms.Main, true)
            print("[AnimChar] Main transform enabled")
        else
            print("[AnimChar] ERROR: Transforms.Main is nil")
            return
        end
    else
        print("[AnimChar] Main already active, not re-enabling")
    end

    isMainActive = true
    isThroughDoorActive = false

    print("[AnimChar] Starting Main animation, " .. MainLoopCount .. " loops, " .. AnimationDuration .. "s each")
    FireEvent(Events.OnMainStarted, context)

    -- Schedule first loop completion
    if MainLoopCount > 0 and AnimationDuration > 0 then
        print("[AnimChar] Scheduling OnLoopComplete in " .. AnimationDuration .. " seconds")
        Invoke("OnLoopComplete", AnimationDuration)
    else
        print("[AnimChar] WARNING: MainLoopCount or AnimationDuration is 0, no loop scheduled")
    end
end

-- RECEIVER: Stop ThroughDoor and hide character
function StopThroughDoor(context)
    Initialize()

    CancelInvoke("OnLoopComplete")
    CancelInvoke("OnThroughDoorComplete")

    if visuals and Transforms.ThroughDoor then
        visuals:SetEnabled(MyContext, Transforms.ThroughDoor, false)
    end
    isThroughDoorActive = false

    print("[AnimChar] ThroughDoor stopped, character hidden")
    FireEvent(Events.OnCharacterHidden, context)
end

-- RECEIVER: Stop everything and hide character
function StopAll(context)
    Initialize()

    CancelInvoke("OnLoopComplete")
    CancelInvoke("OnThroughDoorComplete")

    DisableAll()

    print("[AnimChar] All animations stopped, character hidden")
    FireEvent(Events.OnCharacterHidden, context)
end

-- RECEIVER: Force switch to ThroughDoor immediately
function ForceToThroughDoor(context)
    Initialize()

    CancelInvoke("OnLoopComplete")
    currentLoop = MainLoopCount  -- Mark as complete

    SwitchToThroughDoor()
end

-- RECEIVER: Enable Main without loop counting (stays on indefinitely)
function EnableMainIndefinitely(context)
    Initialize()

    if not visuals then return end
    if not Transforms.Main then return end

    CancelInvoke("OnLoopComplete")
    CancelInvoke("OnThroughDoorComplete")

    DisableAll()
    visuals:SetEnabled(MyContext, Transforms.Main, true)
    isMainActive = true

    print("[AnimChar] Main enabled indefinitely")
    FireEvent(Events.OnMainStarted, context)
end

-- RECEIVER: Enable ThroughDoor without auto-hide
function EnableThroughDoorIndefinitely(context)
    Initialize()

    if not visuals then return end
    if not Transforms.ThroughDoor then return end

    CancelInvoke("OnLoopComplete")
    CancelInvoke("OnThroughDoorComplete")

    DisableAll()
    visuals:SetEnabled(MyContext, Transforms.ThroughDoor, true)
    isThroughDoorActive = true

    print("[AnimChar] ThroughDoor enabled indefinitely")
    FireEvent(Events.OnThroughDoorStarted, context)
end

function Start()
    Initialize()

    -- Only disable if NOT starting on level load
    -- This avoids the disable/enable cycle that might cause rotation issues
    if StartOnLevelLoad then
        -- Don't disable - just ensure Main is on, ThroughDoor is off
        if visuals then
            if Transforms.ThroughDoor then
                visuals:SetEnabled(MyContext, Transforms.ThroughDoor, false)
            end
            -- Don't touch Main - leave it as-is from Unity
        end
        isMainActive = true
        isThroughDoorActive = false

        currentLoop = 0
        print("[AnimChar] Starting Main animation (no disable), " .. MainLoopCount .. " loops, " .. AnimationDuration .. "s each")
        FireEvent(Events.OnMainStarted, MyContext)

        if MainLoopCount > 0 and AnimationDuration > 0 then
            Invoke("OnLoopComplete", AnimationDuration)
        end
    else
        -- Start hidden
        DisableAll()
    end
end

--[[
INSPECTOR PROPERTIES:
- MainLoopCount (Int): Number of times Main animation should loop before switching
- AnimationDuration (Float): Duration of one Main animation loop in seconds
- ThroughDoorDuration (Float): Duration of ThroughDoor animation (0 = no auto-complete)
- AutoHideAfterThroughDoor (Bool): Hide character after ThroughDoor completes
- StartOnLevelLoad (Bool): Automatically start Main when level loads

TRANSFORMS (set in Visuals component):
- Main: The main walking/idle animation
- ThroughDoor: The exit/through-door animation

EVENTS (Outputs):
- OnMainStarted(): Main animation has started
- OnMainCompleted(): All Main loops finished, switching to ThroughDoor
- OnThroughDoorStarted(): ThroughDoor animation has started
- OnThroughDoorCompleted(): ThroughDoor animation finished
- OnCharacterHidden(): Character is now hidden (both transforms off)

RECEIVERS (Inputs):
- StartMain(): Enable Main and start loop counting
- StopThroughDoor(): Disable ThroughDoor (character hidden)
- StopAll(): Stop everything and hide character
- ForceToThroughDoor(): Skip remaining Main loops, go to ThroughDoor
- EnableMainIndefinitely(): Enable Main without loop counting
- EnableThroughDoorIndefinitely(): Enable ThroughDoor without auto-hide

USAGE EXAMPLE:
1. Set MainLoopCount = 3
2. Set AnimationDuration = 168.28339 (your animation length)
3. Set ThroughDoorDuration = 5 (or 0 to stay visible)
4. Wire: LevelStart → StartMain
5. Wire: OnThroughDoorCompleted → [Next Action]

Or trigger manually:
- Wire: TriggerVolume OnTriggered → StartMain
- Wire: OnCharacterHidden → Door Open
]]
