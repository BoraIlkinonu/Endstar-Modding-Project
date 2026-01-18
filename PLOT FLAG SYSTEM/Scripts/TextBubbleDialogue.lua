local currentIndex = 0
local textBubble = nil
local textLength = 0
local initialized = false
local currentTarget = nil
local pendingCloses = 0
local isSitting = false
local visuals = nil
local interactable = nil

function Initialize()
    if initialized then return end

    textBubble = MyContext:TryGetComponent(Component.TextBubble)
    if textBubble then
        textLength = textBubble:GetTextLength()
        print(DebugPrefix .. " TextBubble found with " .. textLength .. " messages")
    else
        print(DebugPrefix .. " ERROR: No TextBubble component found!")
    end


    -- Get Visuals component from this prop
    visuals = MyContext:TryGetComponent(Component.Visuals)
    if visuals then
        print(DebugPrefix .. " Visuals component found")
    else
        print(DebugPrefix .. " ERROR: No Visuals component on this prop")
    end

    -- Get Interactable component
    interactable = MyContext:TryGetComponent(Component.Interactable)
    print(DebugPrefix .. " Interactable component: " .. tostring(interactable))

    -- Disable interaction if no messages
    if textLength == 0 and interactable then
        interactable:SetInteractableEnabled(MyContext, 0, false)
        print(DebugPrefix .. " Interactable disabled (no messages)")
    end

    initialized = true
end

function SwapToSitting()
    if visuals then
        visuals:SetEnabled(MyContext, Transforms.Main, false)
        visuals:SetEnabled(MyContext, Transforms.Sitting, true)
    end
    isSitting = true
end

function SwapToMain()
    if visuals then
        visuals:SetEnabled(MyContext, Transforms.Main, true)
        visuals:SetEnabled(MyContext, Transforms.Sitting, false)
    end
    isSitting = false
end

function OnInteracted(context)
    Initialize()

    if not textBubble then
        print(DebugPrefix .. " ERROR: No TextBubble component")
        return
    end

    if textLength == 0 then
        print(DebugPrefix .. " ERROR: No messages in Text array")
        return
    end

    -- If sitting, revert to main and restart
    if isSitting then
        print(DebugPrefix .. " Reverting to main state")
        SwapToMain()
        currentIndex = 0
    end

    -- Handle loop reset
    if currentIndex >= textLength then
        if Loop then
            print(DebugPrefix .. " Looping back to start")
            currentIndex = 0
        else
            print(DebugPrefix .. " All messages already shown")
            return
        end
    end

    currentTarget = context
    pendingCloses = pendingCloses + 1

    -- Display new message
    print(DebugPrefix .. " Displaying message " .. currentIndex .. " (pending closes: " .. pendingCloses .. ")")
    textBubble:DisplayForTarget(MyContext, context, currentIndex, false, false)

    FireEvent(Events.OnMessageShown, context)

    if currentIndex >= textLength - 1 then
        FireEvent(Events.OnAllMessagesShown, context)
        SwapToSitting()
    end

    currentIndex = currentIndex + 1

    -- Schedule close
    if Duration > 0 then
        Invoke("TryCloseBubble", Duration)
    end
end

function TryCloseBubble()
    pendingCloses = pendingCloses - 1
    print(DebugPrefix .. " TryCloseBubble called (pending closes: " .. pendingCloses .. ")")

    -- Only close if this is the last pending close (no new messages scheduled)
    if pendingCloses == 0 then
        if textBubble and currentTarget then
            print(DebugPrefix .. " Closing bubble")
            textBubble:CloseForTarget(MyContext, currentTarget)
        end
    end
end

function Start()
    print(DebugPrefix .. " Start called, isSitting=" .. tostring(isSitting))
    Initialize()
end
