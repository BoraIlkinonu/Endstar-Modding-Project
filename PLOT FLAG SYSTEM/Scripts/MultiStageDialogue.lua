local currentInteractor = nil
local currentStage = 0
local currentMessageIndex = -1
local textBubble = nil
local stages = {}  -- Array of {startIndex, endIndex} per stage
local stageCount = 0
local dialogueComplete = false
local gameContext = nil
local lastDebugOutput = ""

function DebugOutput(message, context)
    if not DebugMode then return end
	lastDebugOutput = message
    print("[MultiStageDialogue:" .. DialogueId .. "] " .. message)
    FireEvent(Events.OnDebugOutput, context, message)
end

function ExtractStringFromLocalized(localizedString)
    -- LocalizedString tostring format:
    -- "ActiveLanguage: English, originalLanguage: English, localizedStrings: Language: English, String: ACTUAL_TEXT: NUMBER"
    -- We need to extract ACTUAL_TEXT
    local str = tostring(localizedString)

    -- Find "String: " and extract everything after it until the last colon
    local pattern = "String: (.+): %d"
    local match = string.match(str, pattern)

    if match then
        return match
    end

    -- Fallback: try simpler pattern
    local startPos = string.find(str, "String: ", 1, true)
    if startPos then
        local afterString = string.sub(str, startPos + 8)
        -- Remove trailing ": NUMBER"
        local lastColon = string.find(afterString, ":[^:]*$")
        if lastColon then
            return string.sub(afterString, 1, lastColon - 1)
        end
        return afterString
    end

    -- Last fallback: return original
    return str
end

function ParseStages()
    stages = {}
    stageCount = 0
    local currentStageStart = 0
print("MultiStageDialogue: Parsing " .. Text.Length .. " text entries")
 print("MultiStageDialogue: StageDelimiter = [" .. tostring(StageDelimiter) .. "]")

    for i = 0, Text.Length - 1 do
	local rawEntry = tostring(Text[i])
        local textEntry = ExtractStringFromLocalized(Text[i])
	print("MultiStageDialogue: Text[" .. i .. "] = [" .. textEntry .. "]")
        if textEntry == StageDelimiter then
	 print("MultiStageDialogue: Found delimiter at index " .. i)
            -- Only add stage if there's content (skip empty stages)
            if i > currentStageStart then
                stages[stageCount] = {
                    startIndex = currentStageStart,
                    endIndex = i - 1
                }
                stageCount = stageCount + 1
            end
            currentStageStart = i + 1
        end
    end

    -- Last stage (no trailing delimiter)
    if currentStageStart < Text.Length then
        stages[stageCount] = {
            startIndex = currentStageStart,
            endIndex = Text.Length - 1
        }
        stageCount = stageCount + 1
    end

    -- Validate stage count matches StageNames
    if StageNames.Length ~= stageCount then
        LogError("MultiStageDialogue: StageNames count (" .. StageNames.Length ..
                 ") doesn't match dialogue sections (" .. stageCount .. ")")
    end

    print("MultiStageDialogue: Parsed " .. stageCount .. " stages")
end

function LoadProgress()
    if PersistentProgress then
        gameContext = MyContext.GameContext
        if gameContext then
            currentStage = gameContext:GetInt(DialogueId .. "_stage") or 0
            dialogueComplete = gameContext:GetBool(DialogueId .. "_complete") or false
            print("MultiStageDialogue: Loaded stage " .. currentStage ..
                  ", complete=" .. tostring(dialogueComplete))
        end
    end
end

function SaveProgress()
    if PersistentProgress and gameContext then
        gameContext:SetInt(DialogueId .. "_stage", currentStage)
        gameContext:SetBool(DialogueId .. "_complete", dialogueComplete)
    end
end

function GetCurrentStageName()
    if currentStage >= 0 and currentStage < StageNames.Length then
        return StageNames[currentStage]
    end
    return "Unknown"
end

function Start()
    ParseStages()
    LoadProgress()

    if OnStart and not dialogueComplete then
        local npc = Interaction:GetNpcReference()
        if npc ~= nil then
            local context = Interaction:GetContext()
            if npc:IsNpc() then
                Interaction:GiveInstruction(context, npc)
            end
        end
    end
end

function ConfigureNpc(context)
    if context:IsNpc() then
        textBubble = context:TryGetComponent(Component.TextBubble)
        textBubble:SetLocalizedText(context, Text)
    end
end

function AttemptInteraction(interactorContext, npcContext, colliderIndex)
    -- Block interaction if dialogue is complete (OnlyRunOnce)
    if dialogueComplete then
        return false
    end

    if currentInteractor == nil then
        return true
    end
    return currentInteractor == interactorContext
end

function OnInteracted(interactorContext, npcContext, colliderIndex)
    if npcContext:IsNpc() == false then
        LogError("OnInteracted: npcContext is not an Npc")
        return
    end

    currentMessageIndex = currentMessageIndex + 1

    -- Calculate global index in the Text array
    local stage = stages[currentStage]
    if stage == nil then
        LogError("OnInteracted: Invalid stage " .. currentStage)
        return
    end

    local globalIndex = stage.startIndex + currentMessageIndex

    -- Check if we've completed this stage's messages
    if globalIndex > stage.endIndex then
        -- Stage complete
        local stageName = GetCurrentStageName()
   			
DebugOutput("STAGE COMPLETED: " .. stageName .. " (" .. (currentStage + 1) .. "/" .. stageCount .. ")", npcContext)
        FireEvent(Events.OnStageCompleted, npcContext, stageName)

        Interaction:StopInteraction(interactorContext, npcContext)

        -- Advance to next stage
        currentStage = currentStage + 1
        currentMessageIndex = -1

        -- Check if all stages complete
        if currentStage >= stageCount then
	DebugOutput("DIALOGUE COMPLETED: " .. DialogueId, npcContext)
            FireEvent(Events.OnDialogueCompleted, npcContext)

            if OnlyRunOnce then
                dialogueComplete = true
		DebugOutput("Dialogue marked as COMPLETE (OnlyRunOnce)", npcContext)
            else
                -- Loop back to first stage
                currentStage = 0
		DebugOutput("Looping back to stage 0", npcContext)
            end
        end

        SaveProgress()

        if OnlyOnce then
            Interaction:RescindInstruction(interactorContext, npcContext)
        end

        return
    end

    -- Fire events on first message of stage
    if currentMessageIndex == 0 then
        local stageName = GetCurrentStageName()
	DebugOutput("STAGE STARTED: " .. stageName .. " (" .. (currentStage + 1) .. "/" .. stageCount .. ")", npcContext)
        FireEvent(Events.OnStageStarted, npcContext, stageName)

        -- Fire OnDialogueStarted only on first stage
        if currentStage == 0 then
	DebugOutput("DIALOGUE STARTED: " .. DialogueId, npcContext)
            FireEvent(Events.OnDialogueStarted, npcContext)
        end
    end

    -- Display the message
    textBubble:Display(interactorContext, globalIndex)
end

function OnInteractionStopped(interactorContext, npcContext)
    if npcContext:IsNpc() == false then
        LogError("OnInteractionStopped: npcContext is not an Npc")
        return
    end

    currentInteractor = nil
    textBubble:Close(interactorContext)

    -- Check if stage was completed (all messages shown)
    local stage = stages[currentStage]
    local expectedMessages = stage.endIndex - stage.startIndex + 1

    if currentMessageIndex >= expectedMessages then
        Interaction:InteractionCompleted(interactorContext)
    else
        -- Player walked away mid-dialogue
	DebugOutput("INTERACTION CANCELED at stage: " .. GetCurrentStageName() .. ", message: " ..currentMessageIndex, npcContext)
        Interaction:InteractionCanceled(interactorContext)
        FireEvent(Events.OnInteractionCanceled, npcContext)
    end

    -- Reset message index for next interaction (same stage if canceled)
    currentMessageIndex = -1
end

-- RECEIVER: Set dialogue to a specific stage by name
function SetStage(context, stageName)
    if not stageName or stageName == "" then
        LogError("SetStage: No stage name provided")
        return
    end

    for i = 0, StageNames.Length - 1 do
        if StageNames[i] == stageName then
            currentStage = i
            currentMessageIndex = -1
            dialogueComplete = false
            SaveProgress()
            FireEvent(Events.OnStageChanged, context, stageName)
            print("MultiStageDialogue: Set to stage " .. stageName .. " (index " .. i .. ")")
            return
        end
    end

    LogError("SetStage: Invalid stage name '" .. stageName .. "'")
end

-- RECEIVER: Advance to next stage
function AdvanceStage(context)
    if currentStage < stageCount - 1 then
        currentStage = currentStage + 1
        currentMessageIndex = -1
        SaveProgress()
        FireEvent(Events.OnStageChanged, context, GetCurrentStageName())
        print("MultiStageDialogue: Advanced to stage " .. GetCurrentStageName())
    elseif not OnlyRunOnce then
        -- Loop back to first stage
        currentStage = 0
        currentMessageIndex = -1
        SaveProgress()
        FireEvent(Events.OnStageChanged, context, GetCurrentStageName())
        print("MultiStageDialogue: Looped back to stage " .. GetCurrentStageName())
    else
        -- Already at last stage and OnlyRunOnce is true
        print("MultiStageDialogue: Already at final stage")
    end
end

-- RECEIVER: Reset dialogue to initial state
function ResetDialogue(context)
    currentStage = 0
    currentMessageIndex = -1
    dialogueComplete = false
    SaveProgress()
    FireEvent(Events.OnDialogueReset, context)
    print("MultiStageDialogue: Reset to initial state")
end

-- RECEIVER: Get current stage (outputs via event)
function GetCurrentStage(context)
    FireEvent(Events.OnCurrentStage, context, GetCurrentStageName())
end

-- RECEIVER: Output current dialogue state for debugging
function OutputDebugState(context)
    local status = dialogueComplete and "COMPLETE" or "ACTIVE"
    local output = "Stage: " .. GetCurrentStageName() ..
                   " (" .. (currentStage + 1) .. "/" .. stageCount .. ")" ..
                   " | Status: " .. status
    DebugOutput(output, context)
end

-- RECEIVER: Log a custom debug message
function DebugLog(context, message)
    local msg = message or "(no message)"
    DebugOutput(msg, context)
end