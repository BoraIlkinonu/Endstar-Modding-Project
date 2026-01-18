local visuals = nil
local isVisible = false
local revealCount = 0
local initialized = false

function Initialize()
    if initialized then return end

    visuals = MyContext:TryGetComponent(Component.Visuals)
    if visuals then
        print("TimedReveal: Visuals component found")
    else
        print("TimedReveal: ERROR: No Visuals component on this prop")
    end

    initialized = true
end

function Start()
    Initialize()

    -- Start hidden
    if visuals then
        visuals:SetEnabled(MyContext, Transforms.Displayer, false)
    end
    isVisible = false
end

-- RECEIVER: Show the prop for Duration seconds
function Reveal(context)
    Initialize()

    if not isVisible then
        -- First reveal
        revealCount = revealCount + 1
        if visuals then
            visuals:SetEnabled(MyContext, Transforms.Displayer, true)
        end
        isVisible = true
        FireEvent(Events.OnRevealed, context)
        Invoke("TryHide", Duration)
    elseif ResetOnSignal then
        -- Already visible and reset enabled - restart timer
        revealCount = revealCount + 1
        FireEvent(Events.OnTimerReset, context)
        Invoke("TryHide", Duration)
    end
    -- If visible and ResetOnSignal is false, ignore signal
end

function TryHide()
    revealCount = revealCount - 1

    -- Only hide if no new reveals came in
    if revealCount == 0 and isVisible then
        if visuals then
            visuals:SetEnabled(MyContext, Transforms.Displayer, false)
        end
        isVisible = false
        FireEvent(Events.OnHidden, MyContext)
    end
end

-- RECEIVER: Force hide immediately
function Hide(context)
    Initialize()

    revealCount = 0
    if isVisible then
        if visuals then
            visuals:SetEnabled(MyContext, Transforms.Displayer, false)
        end
        isVisible = false
        FireEvent(Events.OnHidden, context)
    end
end

-- RECEIVER: Toggle visibility
function Toggle(context)
    if isVisible then
        Hide(context)
    else
        Reveal(context)
    end
end


