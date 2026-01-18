-- FlagFilter.lua
-- Simple filter node for PlotFlagManager events
-- For Endstar Engine

-- Key must match PlotFlagManager2's LAST_FLAG_KEY
local LAST_FLAG_KEY = "_PlotFlag_LastTriggered"

-- RECEIVER: Check if last triggered flag matches this filter's FlagName
-- Wire: PlotFlagManager OnFlagTriggered → FlagFilter Check
-- If match: fires OnMatch
function Check(context)
    local gc = MyContext.GameContext
    if not gc then
        LogError("FlagFilter: GameContext not available")
        return
    end

    local lastFlag = gc:GetString(LAST_FLAG_KEY) or ""

    if lastFlag == "" then
        -- No flag has been triggered yet
        return
    end

    if lastFlag == FlagName then
        FireEvent(Events.OnMatch, context)
    end
end

--[[
INSPECTOR PROPERTIES:
- FlagName (String): The flag name to filter for

EVENTS (Outputs):
- OnMatch(): Fires when the last triggered flag matches FlagName

RECEIVERS (Inputs):
- Check(): Compare last triggered flag to FlagName, fire OnMatch if equal

USAGE:
1. Place FlagFilter node in level
2. Set FlagName in inspector (e.g., "LeverFlipped")
3. Wire: PlotFlagManager OnFlagTriggered → FlagFilter Check
4. Wire: FlagFilter OnMatch → YourTargetNode (Level Transition, etc.)

IMPORTANT:
- Only wire Check() from OnFlagTriggered
- Do NOT wire Check() from Level Start or other events (may read stale values)
- Each FlagFilter handles ONE flag - use multiple FlagFilters for multiple flags
]]
