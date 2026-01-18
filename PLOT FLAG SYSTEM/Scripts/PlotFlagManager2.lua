-- PlotFlagManager2.lua
-- Enhanced Plot Flag System with FlagFilter support
-- For Endstar Engine

local flagStates = {}
local initialized = false
local lastOutputText = ""
local gameContext = nil
local lastTriggeredFlagName = ""

-- Key for storing last triggered flag in GameContext (for FlagFilter nodes)
local LAST_FLAG_KEY = "_PlotFlag_LastTriggered"

function InitializeFlags()
    if initialized then return end

    -- Get GameContext for cross-level persistence
    if PersistentFlags then
        gameContext = MyContext.GameContext
    end

    -- Load flags from GameContext if persistence enabled
    for i = 0, FlagNames.Length - 1 do
        local flagName = FlagNames[i]
        if gameContext then
            flagStates[flagName] = gameContext:GetBool(flagName) or false
            print(DebugPrefix .. " Loaded " .. flagName .. " = " .. tostring(flagStates[flagName]))
        else
            flagStates[flagName] = false
        end
    end

    initialized = true
end

function IsValidFlag(flagName)
    for i = 0, FlagNames.Length - 1 do
        if FlagNames[i] == flagName then
            return true
        end
    end
    return false
end

function CountCompletedFlags()
    local count = 0
    for i = 0, FlagNames.Length - 1 do
        if flagStates[FlagNames[i]] == true then
            count = count + 1
        end
    end
    return count
end

function GetProgressMessage()
    local completed = CountCompletedFlags()
    local total = FlagNames.Length
    local message = ProgressMessage
    message = string.gsub(message, "{C}", tostring(completed))
    message = string.gsub(message, "{A}", tostring(total))
    return message
end

function OutputText(message, context)
    lastOutputText = message
    print(DebugPrefix .. " " .. message)

    FireEvent(Events.OnDebugOutput, context, message)
end

-- RECEIVER: Set a flag by name
function SetFlagByName(context, flagName)
    InitializeFlags()

    if not flagName or flagName == "" then
        OutputText("ERROR: No flag name provided", context)
        FireEvent(Events.OnError, context)
        return
    end

    if not IsValidFlag(flagName) then
        OutputText("INVALID: '" .. flagName .. "'", context)
        FireEvent(Events.OnInvalidFlag, context)
        return
    end

    if flagStates[flagName] == true then
        return
    end

    flagStates[flagName] = true

    -- Save to GameContext for cross-level persistence
    if gameContext then
        gameContext:SetBool(flagName, true)
        print(DebugPrefix .. " SAVED " .. flagName .. " to GameContext")
    end

    -- Store last triggered flag for FlagFilter nodes
    lastTriggeredFlagName = flagName

    -- Store in GameContext for FlagFilter nodes to read
    local gc = MyContext.GameContext
    if gc then
        gc:SetString(LAST_FLAG_KEY, flagName)
    end

    OutputText(flagName .. "\n" .. GetProgressMessage(), context)
    FireEvent(Events.OnFlagTriggered, context, flagName)
end

-- RECEIVER: Check if a flag is set
function CheckFlagByName(context, flagName)
    InitializeFlags()

    if not flagName or flagName == "" then
        OutputText("ERROR: No flag name provided", context)
        FireEvent(Events.OnError, context)
        return
    end

    if not IsValidFlag(flagName) then
        OutputText("INVALID: '" .. flagName .. "'", context)
        FireEvent(Events.OnInvalidFlag, context)
        return
    end

    local state = flagStates[flagName] or false

    if state then
        FireEvent(Events.OnFlagTrue, context)
    else
        FireEvent(Events.OnFlagFalse, context)
    end
end

-- RECEIVER: Output current state of all flags
function OutputDebugState(context)
    InitializeFlags()

    local output = ""
    for i = 0, FlagNames.Length - 1 do
        local state = flagStates[FlagNames[i]] or false
        local stateStr = state and "1" or "0"
        output = output .. FlagNames[i] .. "=" .. stateStr
        if i < FlagNames.Length - 1 then
            output = output .. ", "
        end
    end
    output = output .. "\n" .. GetProgressMessage()

    OutputText(output, context)
end

-- RECEIVER: Reset all flags to false
function ResetAllFlags(context)
    for i = 0, FlagNames.Length - 1 do
        local flagName = FlagNames[i]
        flagStates[flagName] = false

        -- Clear from GameContext for cross-level persistence
        if gameContext then
            gameContext:SetBool(flagName, false)
        end
    end

    lastTriggeredFlagName = ""
    OutputText("RESET\n" .. GetProgressMessage(), context)
    FireEvent(Events.OnFlagsReset, context)
end

-- RECEIVER: Refresh display on level load
function RefreshDisplay(context)
    InitializeFlags()
    OutputText(GetProgressMessage(), context)
end

function Start()
    InitializeFlags()

    -- Only refresh display if any flags have been set
    if CountCompletedFlags() > 0 then
        OutputText(GetProgressMessage(), MyContext)
    end
end

--[[
INSPECTOR PROPERTIES:
- FlagNames (StringList): List of valid flag names
- ProgressMessage (String): Message template with {C} for completed count and {A} for total
- DebugPrefix (String): Prefix for debug output
- PersistentFlags (Bool): Save flags to GameContext for cross-level persistence

EVENTS (Outputs):
- OnFlagTriggered(flagName: String): Fires when any flag is set
- OnFlagTrue(): Fires from CheckFlagByName when flag is true
- OnFlagFalse(): Fires from CheckFlagByName when flag is false
- OnFlagsReset(): Fires when all flags are reset
- OnDebugOutput(message: String): Fires with text output (for billboard display)
- OnError(): Fires on error conditions
- OnInvalidFlag(): Fires when invalid flag name provided

RECEIVERS (Inputs):
- SetFlagByName(flagName: String): Set a flag to true
- CheckFlagByName(flagName: String): Check if flag is true/false
- OutputDebugState(): Output all flag states
- ResetAllFlags(): Reset all flags to false
- RefreshDisplay(): Refresh billboard with current progress

FLAGFILTER INTEGRATION:
When a flag is set, the flag name is stored in GameContext under "_PlotFlag_LastTriggered".
Use FlagFilter nodes (separate script) to filter OnFlagTriggered by flag name.
Wire: OnFlagTriggered → FlagFilter.Check → OnMatch → YourAction

This allows filtering without requiring the receiving node to accept string parameters.
]]
