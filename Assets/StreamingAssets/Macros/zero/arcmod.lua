local util = require "zero.util"
require "configtool.config"

Folder.new("zero.arcmod")
    .withParent("zero")
    .withIcon("e922")
    .withName("Arc modification").add()
local configModule = ConfigModule.new("zero.arcmod")

util.zeroMacro(
    "zero.arcmod", "cut",
    "Cut at timing", "e14e",
    "Select an arc or trace and cut it into two at the specified timing",
    function()
        local arc = util.getArc()
        local timing = util.getTiming(true)
        if timing <= arc.timing or timing >= arc.endTiming then return end

        local midpoint = arc.positionAt(timing)
        local arc1 = Event.arc(
            arc.timing, arc.startXY,
            timing, midpoint,
            arc.isVoid,
            arc.color,
            arc.type,
            arc.timingGroup
        )
        local arc2 = Event.arc(
            timing, midpoint,
            arc.endTiming, arc.endXY,
            arc.isVoid,
            arc.color,
            arc.type,
            arc.timingGroup
        )

        local batchCommand = Command.create("cutting arc (zero.shortcuts)")
        batchCommand.add(arc.delete())
        batchCommand.add(arc1.save())
        batchCommand.add(arc2.save())

        local arctaps = Event.query(EventSelectionConstraint.create()
            .arctap()
            .fromTiming(arc.timing)
            .toTiming(arc.endTiming)
            .ofTimingGroup(arc.timingGroup)).arctap
        for _, arctap in ipairs(arctaps) do
            if arctap.arc.instanceEquals(arc) then
                if arctap.timing >= arc1.timing and arctap.timing < arc1.endTiming then
                    arctap.arc = arc1
                elseif arctap.timing >= arc2.timing and arctap.timing <= arc2.endTiming then
                    arctap.arc = arc2
                end

                batchCommand.add(arctap.save())
            end
        end
        batchCommand.commit()
    end)

local hpi = math.pi / 2
local derivatives = {
    s = function(s, e, t) return e - s end,
    si = function(s, e, t) return hpi * (e - s) * math.cos(hpi * t) end,
    so = function(s, e, t) return hpi * (e - s) * math.sin(hpi * t) end,
    b = function(s, e, t) return -6 * (e - s) * (t - 1) * t end,
}

local xEasings = {
    s = "s",
    b = "b",
    si = "si",
    so = "so",
    sisi = "si",
    soso = "so",
    siso = "si",
    sosi = "so",
}

local yEasings = {
    s = "s",
    b = "b",
    si = "s",
    so = "s",
    sisi = "si",
    soso = "so",
    siso = "so",
    sosi = "si",
}

local function smoothTwoFunc(easing1, easing2, duration1, duration2, s, e, startVal)
    local function zeroFunc(x)
        return duration2 * derivatives[easing1](s, x, 1) - duration1 * derivatives[easing2](x, e, 0)
    end

    local step = 5
    local dv = 0.01
    local val = startVal or 0
    for _ = 1, step, 1 do
        local funcVal = zeroFunc(val)
        if funcVal < dv then return val end
        local funcDVal = zeroFunc(val + dv)
        val = val - funcVal * dv / (funcDVal - funcVal)
    end
    return val
end

local function smoothTwoArc(arc1, arc2)
    local midX = smoothTwoFunc(
        xEasings[arc1.type], xEasings[arc2.type],
        arc1.endTiming - arc1.timing, arc2.endTiming - arc2.timing,
        arc1.startX, arc2.endX,
        arc1.endX)
    local midY = smoothTwoFunc(
        yEasings[arc1.type], yEasings[arc2.type],
        arc1.endTiming - arc1.timing, arc2.endTiming - arc2.timing,
        arc1.startY, arc2.endY,
        arc1.endY)
    local midPoint = xy(midX, midY)

    arc1.endXY = midPoint
    arc2.startXY = midPoint
end

configModule:addDescription("<b>Smoothen</b>")
local stepCountConfig = configModule:addField(1, DialogField.create("stepCount")
    .textField(FieldConstraint.create().integer().greater(0))
    .setLabel("Step count"))
util.zeroMacro(
    "zero.arcmod", "smoothen",
    "Smoothen arc chain", "e335",
    "Select an arc chain and smoothen it by moving arc's end points",
    function()
        local selection = Event.getCurrentSelection(EventSelectionConstraint.create().arc())
        if #selection.arc <= 1 then return end

        for i = 1, #selection.arc - 1, 1 do
            local arc1 = selection.arc[i]
            local arc2 = selection.arc[i + 1]
            smoothTwoArc(arc1, arc2)
        end

        local batchCommand = Command.create("smooth arc (zero.smooth)")
        for _, arc in ipairs(selection.arc) do
            batchCommand.add(arc.save())
        end
        batchCommand.commit()
    end)