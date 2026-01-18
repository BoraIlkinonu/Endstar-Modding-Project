-- SimpleVisualToggle.lua
-- Lightweight controller to enable/disable Main visual via receivers

local visuals = nil
local initialized = false

function Initialize()
    if initialized then return end

    visuals = MyContext:TryGetComponent(Component.Visuals)
    if not visuals then
        print("[VisualToggle] ERROR: No Visuals component on this prop")
    end

    initialized = true
end

-- RECEIVER: Enable Main visual
function EnableMain(context)
    Initialize()

    if visuals and Transforms.Main then
        visuals:SetEnabled(MyContext, Transforms.Main, true)
        print("[VisualToggle] Main enabled")
    end
end

-- RECEIVER: Disable Main visual
function DisableMain(context)
    Initialize()

    if visuals and Transforms.Main then
        visuals:SetEnabled(MyContext, Transforms.Main, false)
        print("[VisualToggle] Main disabled")
    end
end

--[[
TRANSFORMS (set in Visuals component):
- Main: The visual to toggle

RECEIVERS (Inputs):
- EnableMain(): Set Main visual enabled
- DisableMain(): Set Main visual disabled
]]
