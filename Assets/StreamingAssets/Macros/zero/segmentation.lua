local util = require "zero.util"
require "configtool.config"

addFolderWithIcon("zero", "zero.segmentation",  "e922", "Arc segmentation")
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


configModule:addDescription("<b>Stair arcs config</b>")
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

        local batchCommand = Command.create("zigzag arc conversion (zero.shortcuts)")
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

        local batchCommand = Command.create("ran arc conversion (zero.shortcuts)")
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

addMacroWithIcon(
    "zero.segmentation", "zero.segmentation.config", 
    "Settings", "e8b8", 
    function() configModule:renderDialog() end)