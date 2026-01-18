local messageQueue = {}
local isDisplaying = false

function ReceiveDisplayMessage(context, message)
    if message and message ~= "" then
        table.insert(messageQueue, message)
        if not isDisplaying then
            DisplayNextMessage(context)
        end
    end
end

function DisplayNextMessage(context)
    if #messageQueue > 0 then
        isDisplaying = true
        local message = table.remove(messageQueue, 1)
        FireEvent(Events.OnDisplayUpdate, context, message)
        Invoke("ContinueQueue", MessageDuration or 2.0)
    else
        isDisplaying = false
    end
end

function ContinueQueue()
    DisplayNextMessage(MyContext)
end

function ClearMessages(context)
    messageQueue = {}
    isDisplaying = false
    FireEvent(Events.OnDisplayUpdate, context, "")
end
