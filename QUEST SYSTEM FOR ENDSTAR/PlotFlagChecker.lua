local function GetStorageContext(context)
    if UseGlobalScope then
        return MyContext
    end
    if context and context:IsPlayer() then
        return context
    end
    local playerCount = Game:GetPlayerCount()
    if playerCount > 0 then
        return Game:GetPlayerByIndex(0)
    end
    return MyContext
end

local function GetFlagKey()
    return "PlotFlag_" .. FlagType .. "_" .. FlagName
end

local function GetValue(context)
    local ctx = GetStorageContext(context)
    local key = GetFlagKey()
    if not ctx:HasMember(key) then
        return nil
    end
    if FlagType == "Bool" then
        return ctx:GetBool(key)
    elseif FlagType == "Int" then
        return ctx:GetInt(key)
    elseif FlagType == "Float" then
        return ctx:GetFloat(key)
    elseif FlagType == "String" then
        return ctx:GetString(key)
    end
    return nil
end

local function SetValue(context, value)
    local ctx = GetStorageContext(context)
    local key = GetFlagKey()
    if FlagType == "Bool" then
        ctx:SetBool(key, value)
    elseif FlagType == "Int" then
        ctx:SetInt(key, value)
    elseif FlagType == "Float" then
        ctx:SetFloat(key, value)
    elseif FlagType == "String" then
        ctx:SetString(key, value)
    end
end

local function CheckValue(context)
    local value = GetValue(context)
    if value == nil then
        return false
    end
    if FlagType == "Bool" then
        return value == true
    elseif FlagType == "Int" then
        if CompareMode == "Equals" then return value == CompareValue end
        if CompareMode == "NotEquals" then return value ~= CompareValue end
        if CompareMode == "AtLeast" then return value >= CompareValue end
        if CompareMode == "AtMost" then return value <= CompareValue end
    elseif FlagType == "Float" then
        if CompareMode == "Equals" then return value == CompareValue end
        if CompareMode == "NotEquals" then return value ~= CompareValue end
        if CompareMode == "AtLeast" then return value >= CompareValue end
        if CompareMode == "AtMost" then return value <= CompareValue end
    elseif FlagType == "String" then
        if CompareMode == "Equals" then return value == CompareValue end
        if CompareMode == "NotEquals" then return value ~= CompareValue end
    end
    return false
end

function CheckFlag(context)
    if CheckValue(context) then
        FireEvent(Events.OnPassed, context, FlagName)
        FireEvent(Events.OnSendDisplayMessage, context, TrueMessage)
    else
        FireEvent(Events.OnFailed, context, FlagName)
        FireEvent(Events.OnSendDisplayMessage, context, FailedMessage)
    end
end

function SetFlag(context, value)
    SetValue(context, value)
    FireEvent(Events.OnFlagSet, context, FlagName)
    FireEvent(Events.OnSendDisplayMessage, context, TrueMessage)
end

function SetFlagTrue(context)
    SetFlag(context, true)
end

function SetFlagFalse(context)
    SetValue(context, false)
    FireEvent(Events.OnFlagSet, context, FlagName)
    FireEvent(Events.OnSendDisplayMessage, context, FailedMessage)
end

function IncrementFlag(context)
    local current = GetValue(context) or 0
    SetFlag(context, current + 1)
end

function DecrementFlag(context)
    local current = GetValue(context) or 0
    SetValue(context, current - 1)
    FireEvent(Events.OnFlagSet, context, FlagName)
    FireEvent(Events.OnSendDisplayMessage, context, FailedMessage)
end
