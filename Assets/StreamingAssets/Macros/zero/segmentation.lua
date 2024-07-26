local util = require "zero.util"
require "configtool.config"

Folder.new("zero.segmentation")
    .withParent("zero")
    .withIcon("e922")
    .withName("Arc segmentation").add()
local configModule = ConfigModule.new("zero.segmentation")

util.zeroMacro(
    "zero.segmentation", "normal",
    "Normal split", "f108",
    "Select an arc or trace to split it into segments, based on the current Density setting.\n"
    .."Alternatively run this macro after selecting multple arcs or traces to split all of them.",
    function()
        local batchCommand = Command.create("split arc (zero.segmentation)")

        util.operateOnSelectedArcs(function(arc)
            local arcLength = Context.beatLengthAt(arc.timing) / Context.beatlineDensity
            for timing = arc.timing, arc.endTiming, arcLength do
                local endTiming = math.min(timing + arcLength, arc.endTiming)
                if (math.abs(endTiming - timing) <= 1) then break end

                local startXY = arc.positionAt(timing)
                local endXY = arc.positionAt(endTiming)

                batchCommand.add(
                    Event.arc(
                        timing, startXY,
                        endTiming, endXY,
                        arc.isVoid,
                        arc.color,
                        's',
                        arc.timingGroup
                    ).save()
                )
            end

            batchCommand.add(arc.delete())
        end)

        batchCommand.commit()
    end)

util.zeroMacro(
    "zero.segmentation", "amyg",
    "Amygdata",  "e25a",
    "Similar to <i>zero.segmentation.split</i>, but guarantees generation of height indicators.",
    function()
        local batchCommand = Command.create("split arc with height indicators (zero.segmentation)")

        util.operateOnSelectedArcs(function(arc)
            local arcLength = Context.beatLengthAt(arc.timing) / Context.beatlineDensity

            local dy = 0
            for timing = arc.timing, arc.endTiming, arcLength do
                local endTiming = math.min(timing + arcLength, arc.endTiming)
                if math.abs(endTiming - timing) <= 1 then break end

                local startXY = arc.positionAt(timing)
                startXY.y = startXY.y + dy

                if dy == 0 then dy = -0.01 else dy = 0 end
                local endXY = arc.positionAt(endTiming)
                endXY.y = endXY.y + dy

                batchCommand.add(
                    Event.arc(
                        timing, startXY,
                        endTiming, endXY,
                        arc.isVoid,
                        arc.color,
                        's',
                        arc.timingGroup
                    ).save()
                )
            end

            batchCommand.add(arc.delete())
        end)

        batchCommand.commit()
    end)

configModule:addDescription("<b>Rice & Stasis arcs config</b>")
local multiplerConfig = configModule:addField(1, DialogField.create("multiplier")
    .textField(FieldConstraint.float().greater(0).lEqual(1))
    .setLabel("Multiplier")
    .setTooltip("How much to shorten each segment when converting an arc to rice arcs"))
util.zeroMacro(
    "zero.segmentation", "rice",
    "Rice arcs", "e5d4",
    "Splits selected arc into disconnected segments",
    function()
        local batchCommand = Command.create("create rice arcs (zero.segmentation)")

        local multiplier = multiplerConfig.value
        util.operateOnSelectedArcs(function(arc)
            local arcLength = Context.beatLengthAt(arc.timing) / Context.beatlineDensity

            for timing = arc.timing, arc.endTiming, arcLength do
                local endTiming = math.min(timing + arcLength, arc.endTiming)
                endTiming = timing * (1 - multiplier) + endTiming * multiplier

                if math.abs(endTiming - timing) <= 1 then break end

                local startXY = arc.positionAt(timing)
                batchCommand.add(
                    Event.arc(
                        timing, startXY,
                        endTiming, startXY,
                        arc.isVoid,
                        arc.color,
                        's',
                        arc.timingGroup
                    ).save())
            end

            batchCommand.add(arc.delete())
        end)

        batchCommand.commit()
    end)

util.zeroMacro(
    "zero.segmentation", "stasis",
    "Stasis arcs", "e5d4",
    "Splits selected arc into disconnected segments, each following the curve",
    function()
        local batchCommand = Command.create("create rice arcs (zero.segmentation)")

        local multiplier = multiplerConfig.value
        util.operateOnSelectedArcs(function(arc)
            local arcLength = Context.beatLengthAt(arc.timing) / Context.beatlineDensity

            for timing = arc.timing, arc.endTiming, arcLength do
                local endTiming = math.min(timing + arcLength, arc.endTiming)
                endTiming = timing * (1 - multiplier) + endTiming * multiplier

                if math.abs(endTiming - timing) <= 1 then break end

                local startXY = arc.positionAt(timing)
                local endXY = arc.positionAt(endTiming)
                batchCommand.add(
                    Event.arc(
                        timing, startXY,
                        endTiming, endXY,
                        arc.isVoid,
                        arc.color,
                        's',
                        arc.timingGroup
                    ).save())
            end

            batchCommand.add(arc.delete())
        end)

        batchCommand.commit()
    end)


configModule:addDescription("<b>Stair & sdvx arcs config</b>")
local stairAtConfig = configModule:addField(1, DialogField.create("stairAt")
    .textField(FieldConstraint.float().gEqual(0).lEqual(1))
    .setLabel("Bend point")
    .setTooltip("0: L / 1: Γ"))
util.zeroMacro(
    "zero.segmentation", "stair",
    "Staircase",  "ebaa",
    "Splits selected arc into staircase",
    function()
        local batchCommand = Command.create("create staircase arc (zero.segmentation)")

        util.operateOnSelectedArcs(function(arc)
            local arcLength = Context.beatLengthAt(arc.timing) / Context.beatlineDensity
            for timing = arc.timing, arc.endTiming, arcLength do
                local endTiming = math.min(timing + arcLength, arc.endTiming)
                if math.abs(endTiming - timing) <= 1 then break end

                local startXY = arc.positionAt(timing)
                local endXY = arc.positionAt(endTiming)

                local midTiming = timing
                local midXY = endXY
                local vertFirst = tonumber(stairAtConfig.value) >= 0.5
                if vertFirst then
                    midTiming = endTiming
                    midXY = startXY
                end

                batchCommand.add(Event.arc(
                        timing, startXY,
                        midTiming, midXY,
                        arc.isVoid,
                        arc.color,
                        's',
                        arc.timingGroup
                    ).save())
                batchCommand.add(Event.arc(
                        midTiming, midXY,
                        endTiming, endXY,
                        arc.isVoid,
                        arc.color,
                        's',
                        arc.timingGroup
                    ).save())
            end

            batchCommand.add(arc.delete())
        end)

        batchCommand.commit()
    end)

util.zeroMacro(
    "zero.segmentation", "zigzag",
    "Zigzag", "e6e1",
    "Select two arcs to generate zigzag arcs between them.\n"
    .."Generated pattern looks like /\\/\\/\\/\\/\\/\\",
    function()
        local arc1 = util.getArc("Select the dominant arc")
        local arc2 = util.getArc("Select the extension arc")

        local batchCommand = Command.create("zigzag arc conversion (zero.segmentation)")
        local arcLength = Context.beatLengthAt(arc1.timing) / Context.beatlineDensity

        local arc1ToArc2 = true
        for timing = arc1.timing, arc1.endTiming, arcLength do
            local endtiming = math.min(timing + arcLength, arc1.endTiming)
            if math.abs(endtiming - timing) <= 1 then break end

            local p1, p2
            if (arc1ToArc2) then
                p1 = arc1.positionAt(timing)
                p2 = arc2.positionAt(endtiming)
            else
                p1 = arc2.positionAt(timing)
                p2 = arc1.positionAt(endtiming)
            end
            arc1ToArc2 = not arc1ToArc2

            batchCommand.add(
                Event.arc(
                    timing, p1,
                    endtiming, p2,
                    arc1.isVoid,
                    arc1.color,
                    's',
                    arc1.timingGroup
                ).save()
            )
        end

        batchCommand.add(arc1.delete())
        batchCommand.add(arc2.delete())
        batchCommand.commit();
    end)

util.zeroMacro(
    "zero.segmentation", "ran",
    "Ran arcs", "ec1c",
    "Select two arcs to generate zigzag arcs between them.\n"
    .."Generated pattern looks like /|/|/|/|/|/|",
    function()
        local arc1 = util.getArc("Select the dominant arc")
        local arc2 = util.getArc("Select the extension arc")

        local batchCommand = Command.create("ran arc conversion (zero.segmentation)")
        local arcLength = Context.beatLengthAt(arc1.timing) / Context.beatlineDensity

        for timing = arc1.timing, arc1.endTiming, arcLength do
            local endtiming = math.min(timing + arcLength, arc1.endTiming)
            if math.abs(endtiming - timing) <= 1 then break end

            -- p3--p2
            --    /
            -- p1/

            local p1 = arc1.positionAt(timing)
            local p3 = arc1.positionAt(endtiming)
            local p2 = arc2.positionAt(endtiming)

            batchCommand.add(
                Event.arc(
                    timing, p1,
                    endtiming, p2,
                    arc1.isVoid,
                    arc1.color,
                    's',
                    arc1.timingGroup
                ).save()
            )

            batchCommand.add(
                Event.arc(
                    endtiming, p2,
                    endtiming, p3,
                    arc1.isVoid,
                    arc1.color,
                    's',
                    arc1.timingGroup
                ).save()
            )
        end

        batchCommand.add(arc1.delete())
        batchCommand.add(arc2.delete())
        batchCommand.commit();
    end)

local function genSdvx(bend)
    local arc1 = util.getArc("Select the dominant arc")
    local arc2 = util.getArc("Select the extension arc")

    local batchCommand = Command.create("sdvx arc conversion (zero.segmentation)")
    local arcLength = Context.beatLengthAt(arc1.timing) / Context.beatlineDensity

    local oneToTwo = true
    for timing = arc1.timing, arc1.endTiming, arcLength do
        local endtiming = math.min(timing + arcLength, arc1.endTiming)
        if math.abs(endtiming - timing) <= 1 then break end

        local from = util.choose(oneToTwo, arc1, arc2)
        local to = util.choose(oneToTwo, arc2, arc1)
        oneToTwo = not oneToTwo

        local vertFirst = tonumber(stairAtConfig.value) >= 0.5
        --  true  | false
        -- p2--p3 |     p3
        -- |      |     |
        -- p1     | p1--p2
        local p1 = from.positionAt(timing)
        local p3 = to.positionAt(endtiming)
        local p2 = util.choose(vertFirst,
            util.choose(bend, from.positionAt(endtiming), p1),
            util.choose(bend, to.positionAt(timing), p3))

        batchCommand.add(
            Event.arc(
                timing, p1,
                endtiming, p2,
                arc1.isVoid,
                arc1.color,
                's',
                arc1.timingGroup
            ).save()
        )

        batchCommand.add(
            Event.arc(
                endtiming, p2,
                endtiming, p3,
                arc1.isVoid,
                arc1.color,
                's',
                arc1.timingGroup
            ).save()
        )
    end

    batchCommand.add(arc1.delete())
    batchCommand.add(arc2.delete())
    batchCommand.commit();
end

util.zeroMacro(
    "zero.segmentation", "sdvx",
    "Sdvx arcs (straight)", "ebaa",
    "Select two arcs to generate sdvx arcs between them.\n"
    .."Generated pattern looks like LΓLΓLΓLΓ. Each segment is straight.",
    function() genSdvx(false) end)

util.zeroMacro(
    "zero.segmentation", "sdvx_bend",
    "Sdvx arcs (bend)", "ebaa",
    "Select two arcs to generate sdvx arcs between them.\n"
    .."Generated pattern looks like LΓLΓLΓLΓ. Each segment follows the original curve.",
    function() genSdvx(true) end)

configModule:addDescription("<b>Random spread range</b>")
local spreadXConfig = configModule:addField(0.125, DialogField.create("spreadX")
    .textField(FieldConstraint.float().gEqual(0))
    .setLabel("Spread X")
    .setTooltip("Maximum x distance"))
local spreadYConfig = configModule:addField(0.25, DialogField.create("spreadY")
    .textField(FieldConstraint.float().gEqual(0))
    .setLabel("Spread Y")
    .setTooltip("Maximum y distance"))
local spreadClampConfig = configModule:addField(true, DialogField.create("spreadClmp")
    .checkbox()
    .setLabel("Clamp")
    .setTooltip("Clamp x and y coordinates to playable range"))
util.zeroMacro(
    "zero.segmentation", "random",
    "Random spread",  "e074",
    "Splits selected arc into segments, each offsetted by a random distance.",
    function()
        local batchCommand = Command.create("create random split arc (zero.segmentation)")

        util.operateOnSelectedArcs(function(arc)
            local arcLength = Context.beatLengthAt(arc.timing) / Context.beatlineDensity
            for timing = arc.timing, arc.endTiming, arcLength do
                local endTiming = math.min(timing + arcLength, arc.endTiming)
                if math.abs(endTiming - timing) <= 1 then break end

                local spreadX = tonumber(spreadXConfig.value)
                local spreadY = tonumber(spreadYConfig.value)
                local startXY = arc.positionAt(timing)
                local newXY = xy(
                    startXY.x + (math.random() - 0.5) * spreadX * 2,
                    startXY.y + (math.random() - 0.5) * spreadY * 2)

                log(spreadClampConfig.value)
                if toBool(spreadClampConfig.value) then
                    newXY.x = math.max(newXY.x, -0.5)
                    newXY.x = math.min(newXY.x, 1.5)
                    newXY.y = math.max(newXY.y, 0)
                    newXY.y = math.min(newXY.y, 1)
                end

                batchCommand.add(Event.arc(
                    timing, newXY,
                    endTiming, newXY,
                    arc.isVoid,
                    arc.color,
                    's',
                    arc.timingGroup
                ).save())
            end

            batchCommand.add(arc.delete())
        end)

        batchCommand.commit()
    end)

Macro.new("zero.segmentation.config")
    .withName("Settings")
    .withParent("zero.segmentation") 
    .withIcon("e8b8")
    .withDefinition(function() configModule:renderDialog() end)
    .add()