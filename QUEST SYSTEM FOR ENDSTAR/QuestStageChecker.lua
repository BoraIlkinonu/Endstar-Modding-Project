QuestStageRegistry = QuestStageRegistry or {}

local stageNameToIndex = {}
local stageIndexToName = {}
local maxStageIndex = 0

function Awake()
    if StageNames and StageNames.Length > 0 then
        for i = 0, StageNames.Length - 1 do
            local name = StageNames[i]
            stageNameToIndex[name] = i
            stageIndexToName[i] = name
            if i > maxStageIndex then
                maxStageIndex = i
            end
        end
        QuestStageRegistry[QuestName] = {
            nameToIndex = stageNameToIndex,
            indexToName = stageIndexToName,
            maxIndex = maxStageIndex
        }
    elseif QuestStageRegistry[QuestName] then
        stageNameToIndex = QuestStageRegistry[QuestName].nameToIndex
        stageIndexToName = QuestStageRegistry[QuestName].indexToName
        maxStageIndex = QuestStageRegistry[QuestName].maxIndex
    end
end

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

local function GetStageIndex(stageName)
    return stageNameToIndex[stageName] or -1
end

local function GetStageName(index)
    return stageIndexToName[index] or "Unknown"
end

local function GetCurrentIndex(context)
    local ctx = GetStorageContext(context)
    local key = "Quest_" .. QuestName .. "_Stage"
    if ctx:HasMember(key) then
        return ctx:GetInt(key)
    end
    return 0
end

local function SetCurrentIndex(context, index)
    local ctx = GetStorageContext(context)
    local key = "Quest_" .. QuestName .. "_Stage"
    local oldIndex = GetCurrentIndex(context)
    ctx:SetInt(key, index)

    if oldIndex ~= index then
        local stageName = GetStageName(index)
        FireEvent(Events.OnStageChanged, context, stageName)
    end
end

function SetStage(context, stageName)
    local index = GetStageIndex(stageName)
    if index >= 0 then
        SetCurrentIndex(context, index)
    end
end

function CheckStageEquals(context, stageName)
    local currentIndex = GetCurrentIndex(context)
    local targetIndex = GetStageIndex(stageName)
    local currentName = GetStageName(currentIndex)

    if currentIndex == targetIndex then
        FireEvent(Events.OnPassed, context, currentName)
        FireEvent(Events.OnSendDisplayMessage, context, TrueMessage)
    else
        FireEvent(Events.OnFailed, context, currentName)
        FireEvent(Events.OnSendDisplayMessage, context, FailedMessage)
    end
end

function CheckStageAtLeast(context, stageName)
    local currentIndex = GetCurrentIndex(context)
    local targetIndex = GetStageIndex(stageName)
    local currentName = GetStageName(currentIndex)

    if currentIndex >= targetIndex then
        FireEvent(Events.OnPassed, context, currentName)
        FireEvent(Events.OnSendDisplayMessage, context, TrueMessage)
    else
        FireEvent(Events.OnFailed, context, currentName)
        FireEvent(Events.OnSendDisplayMessage, context, FailedMessage)
    end
end

function CheckStageBefore(context, stageName)
    local currentIndex = GetCurrentIndex(context)
    local targetIndex = GetStageIndex(stageName)
    local currentName = GetStageName(currentIndex)

    if currentIndex < targetIndex then
        FireEvent(Events.OnPassed, context, currentName)
        FireEvent(Events.OnSendDisplayMessage, context, TrueMessage)
    else
        FireEvent(Events.OnFailed, context, currentName)
        FireEvent(Events.OnSendDisplayMessage, context, FailedMessage)
    end
end

function AdvanceStage(context)
    local currentIndex = GetCurrentIndex(context)
    if currentIndex < maxStageIndex then
        SetCurrentIndex(context, currentIndex + 1)
    end
end

function ResetStage(context)
    SetCurrentIndex(context, 0)
end
