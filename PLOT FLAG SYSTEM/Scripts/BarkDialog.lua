-- BarkDialog.lua
-- NPC bark dialog with start/complete events
-- For Endstar Engine

local currentContext = nil
local textSet = false
local isBusy = false

function GetContext(context)
    local inspectorContext = TargetNpc:GetNpc()
    if inspectorContext ~= nil then
        return inspectorContext
    else
        return context
    end
end

function HandleBark(context, index, duration)
    -- Block if already showing a bark
    if isBusy then return end

    context = GetContext(context)
    currentContext = context
    local tb = context:TryGetComponent(Component.TextBubble)

    if index > DialogOptions.Length - 1 then
        index = DialogOptions.Length - 1
    elseif index < 0 then
        index = 0
    end

    if tb ~= nil and DialogOptions[index] ~= nil then
        isBusy = true
        tb:Close(MyContext)

        -- Set text once, then display at index
        if not textSet then
            tb:SetLocalizedText(MyContext, DialogOptions)
            textSet = true
        end
        tb:Display(MyContext, index)

        FireEvent(Events.OnBarkStarted, context)

        CancelInvoke("StopBark")
        Invoke("StopBark", duration, context)
    end
end

function StopBark(context)
    local ctx = context or currentContext or MyContext
    local tb = ctx:TryGetComponent(Component.TextBubble)
    if tb ~= nil then
        tb:Close(MyContext)
    end

    isBusy = false
    FireEvent(Events.OnBarkCompleted, ctx)
    currentContext = nil
end

-- RECEIVER: Random bark with default duration
function Bark(context)
    HandleBark(context, math.random(DialogOptions.Length) - 1, DefaultDuration)
end

-- RECEIVER: Bark at specific index with default duration
function BarkAtIndex(context, index)
    HandleBark(context, index, DefaultDuration)
end

-- RECEIVER: Bark at specific index with custom duration
function BarkAtIndexWithDuration(context, index, duration)
    HandleBark(context, index, duration)
end

-- RECEIVER: Stop bark immediately
function Silence(context)
    if not isBusy then return end
    CancelInvoke("StopBark")
    StopBark(GetContext(context))
end

--[[
INSPECTOR PROPERTIES:
- TargetNpc (NpcReference): NPC to display bark on (optional, uses context if empty)
- DialogOptions (LocalizedStringList): List of bark messages
- DefaultDuration (Float): Seconds to display bark

EVENTS (Outputs):
- OnBarkStarted(): Fires when bark text appears
- OnBarkCompleted(): Fires when bark closes (after duration or Silence)

RECEIVERS (Inputs):
- Bark(): Show random message for DefaultDuration
- BarkAtIndex(index: Int): Show specific message for DefaultDuration
- BarkAtIndexWithDuration(index: Int, duration: Float): Show specific message for custom duration
- Silence(): Stop bark immediately

WIRING EXAMPLE:
  TriggerZone OnEnter → BarkDialog Bark
  BarkDialog OnBarkCompleted → [Next Action]
]]
