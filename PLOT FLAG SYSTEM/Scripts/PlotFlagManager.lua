local flagStates = {}
local initialized = false
local lastOutputText = ""
local gameContext = nil

function InitializeFlags()
    if initialized then return end

    -- Get GameContext for cross-level persistence
    if PersistentFlags then
        gameContext = MyContext.GameContext
    end

    -- Load flags - check GameContext first if persistence enabled
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
print("RAW ProgressMessage: [" .. message .. "]")
    message = string.gsub(message, '"C"', tostring(completed))
    message = string.gsub(message, '"A"', tostring(total))
print("AFTER gsub: [" .. message .. "]")
    return message
end

function OutputText(message, context)
    lastOutputText = message
    print(DebugPrefix .. " " .. message)

    FireEvent(Events.OnDebugOutput, context, message)
end

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
    end

    OutputText(flagName .. "\n" .. GetProgressMessage(), context)
    FireEvent(Events.OnFlagTriggered, context)
end

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

function ResetAllFlags(context)
    for i = 0, FlagNames.Length - 1 do
        local flagName = FlagNames[i]
        flagStates[flagName] = false

        -- Clear from GameContext for cross-level persistence
        if gameContext then
            gameContext:SetBool(flagName, false)
        end
    end

    OutputText("RESET\n" .. GetProgressMessage(), context)
    FireEvent(Events.OnFlagsReset, context)
end

function Start()
    InitializeFlags()
end
