-- DebugLogger.lua
-- Debug utility for printing to console and billboard
-- For Endstar Engine

local lastOutput = ""

function OutputMessage(message, context)
    lastOutput = message
    print("[" .. Prefix .. "] " .. message)
    FireEvent(Events.OnOutput, context, message)
end

-- RECEIVER: Log a custom message
function Log(context, message)
    local msg = message or "(no message)"
    OutputMessage(msg, context)
end

-- RECEIVER: Log a trigger event (no parameter needed)
function LogTrigger(context)
    OutputMessage("TRIGGERED", context)
end

-- RECEIVER: Log with a label from inspector
function LogLabel(context)
    OutputMessage(Label, context)
end

function Start()
    if LogOnStart then
        OutputMessage("Started", MyContext)
    end
end

--[[
INSPECTOR PROPERTIES:
- Prefix (String): Prefix for all log messages (e.g., "DEBUG", "GAME", "TEST")
- Label (String): Custom label for LogLabel receiver
- LogOnStart (Bool): Log "Started" when level loads

EVENTS (Outputs):
- OnOutput(message: String): Fires with message text (wire to Billboard SetRawText)

RECEIVERS (Inputs):
- Log(message: String): Print custom message
- LogTrigger(): Print "TRIGGERED" (no parameter needed)
- LogLabel(): Print the Label from inspector (no parameter needed)

USAGE:
1. Place DebugLogger node
2. Set Prefix in inspector (e.g., "DEBUG")
3. Wire: OnOutput → Billboard SetRawText
4. Wire: SomeEvent → DebugLogger Log("Your message")
   OR: SomeEvent → DebugLogger LogTrigger (for events without text box)

EXAMPLE OUTPUT:
- Log("Door opened") → "[DEBUG] Door opened"
- LogTrigger → "[DEBUG] TRIGGERED"
- LogLabel (with Label="LeverFlipped") → "[DEBUG] LeverFlipped"
]]
