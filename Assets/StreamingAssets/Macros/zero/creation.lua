local util = require("zero.util")
require "configtool.config"

Folder.new("zero.creation")
    .withParent("zero")
    .withIcon("e145")
    .withName("Create elements").add()
local configModule = ConfigModule.new("zero.creation")

util.zeroMacro(
    "zero.creation", "beamred",
    "Create red arc beam", "e015",
    "Select a timing and position to create a vertical red arc beam.",
    function()
        local xyt = util.getTimingAndPosition()
        Event.arc(xyt.timing, xyt.x, 10, xyt.timing, xyt.x, 10.1, false, 1, "s", Context.currentTimingGroup, "none").save().commit()
    end)

util.zeroMacro(
    "zero.creation", "beamblue",
    "Create blue arc beam", "e015",
    "Select a timing and position to create a vertical blue arc beam.",
    function()
        local xyt = util.getTimingAndPosition()
        Event.arc(xyt.timing, xyt.x, 10, xyt.timing, xyt.x, 10.1, false, 0, "s", Context.currentTimingGroup, "none").save().commit()
    end)

---@param name string
---@param command Command
---@return LuaTimingGroup
local function detectTimingGroupName(name, command)
    for i = 1, Context.timingGroupCount - 1, 1 do
        local group = Event.getTimingGroup(i)
        if group.name == name then return group end
    end

    local tg = Event.createTimingGroup('name="'..name..'"')
    command.add(tg.save())
    return tg
end
    
util.zeroMacro(
    "zero.creation", "blink", 
    "Create blink traces", "e3e4",
    "Create blinking trace - signature element of 0thElement's charts.\n"..
    "Simply select the starting timing, position, and ending timing.\n"
    .. "Timing groups and timing events are generated automatically.\n"
    .. "All blink traces are generated at timing group named '<i>$zero.blink</i>'.",
    function()
        local xyt = util.getTimingAndPosition("Select start timing", "Select center position")
        local startTiming = xyt.timing
        local startXY = xyt.xy
        local endTiming = util.getTiming(true, "Select end timing")
        if endTiming < startTiming then return end

        local batchCommand = Command.create("creating blink trace (zero.creation)")

        local timingGroup = detectTimingGroupName("$zero.blink", batchCommand)
        local halfTiming = (endTiming + startTiming) / 2

        local bpm = Context.baseBpm
        local interferingTiming = Event.query(
            EventSelectionConstraint.create()
                .timing()
                .fromTiming(startTiming-1)
                .toTiming(endTiming)
                .ofTimingGroup(timingGroup.num)
            ).timing
        for i = 1, #interferingTiming, 1 do
            batchCommand.add(interferingTiming[i].delete())
        end

        batchCommand.add(Event.arc(
            startTiming, startXY + xy(-0.03, 0),
            halfTiming, startXY,
            true, 0, 'si' 
        ).save().withTimingGroup(timingGroup))

        batchCommand.add(Event.arc(
            startTiming, startXY + xy(0.03, 0),
            halfTiming, startXY,
            true, 0, 'si'
        ).save().withTimingGroup(timingGroup))

        batchCommand.add(Event.arc(
            startTiming, startXY,
            endTiming, startXY,
            true, 0, 's'
        ).save().withTimingGroup(timingGroup))

        batchCommand.add(Event.timing(
            startTiming-1, 999999, 9999
        ).save().withTimingGroup(timingGroup))
        batchCommand.add(Event.timing(
            startTiming, bpm * 10, 9999
        ).save().withTimingGroup(timingGroup))
        batchCommand.add(Event.timing(
            endTiming, bpm, 9999
        ).save().withTimingGroup(timingGroup))
        
        batchCommand.commit()
    end)

configModule:addDescription("<b>Create fading arccap:<b>")
configModule:addDescription(" - Base timing multiplier")
local baseMultConfig = configModule:addField(3, DialogField.create("baseMult")
    .textField(FieldConstraint.create().float().greater(0))
    .setLabel("Value"))
configModule:addDescription(" - Multiply value for each step")
local incrMultConfig = configModule:addField(0.75, DialogField.create("incrMult")
    .textField(FieldConstraint.create().float().greater(0))
    .setLabel("Value"))
configModule:addDescription(" - Number of timing events")
local timingStepCountConfig = configModule:addField(3, DialogField.create("stepCount")
    .textField(FieldConstraint.create().integer().greater(0))
    .setLabel("Count"))
util.zeroMacro(
    "zero.creation", "arccap", 
    "Create fading arccap", "f1d1",
    "Create fading arccap.\n"..
    "Select the starting timing, position.\n"
    .. "A new timing group is generated automatically.\n",
    function()
        local xyt = util.getTimingAndPosition()
        local tg = Event.createTimingGroup("name=\"$zero.arccap\",noinput,noheightindicator,nohead")
        local tgNum = Context.timingGroupCount - 1
        local baseBpm = Context.baseBpm
        
        local cmd = Command.create("Fading arccap")
        cmd.add(tg.save())
        cmd.add(Event.timing(0, baseBpm, 4, tgNum).save())

        local mult = baseMultConfig.value
        local inc = incrMultConfig.value
        local step = timingStepCountConfig.value

        local dist = 0
        for i = 0, step, 1 do
            cmd.add(Event.timing(xyt.timing + 100 * i, -baseBpm * mult, 4, tgNum).save())
            dist = dist + 100 * baseBpm * mult
            mult = mult * inc
        end

        dist = dist + (999996 - xyt.timing - 100 * (step + 1)) * baseBpm * mult / inc
        cmd.add(Event.arc(999997, xyt.xy, 999999, xyt.xy, false, 0, "s", tgNum, "none").save())
        cmd.add(Event.timing(999996, dist, 9999, tgNum).save())
        cmd.add(Event.timing(999997, 0.01, 4, tgNum).save())
        cmd.withTimingGroup(tg)

        cmd.commit()
    end)

Macro.new("zero.creation.config")
    .withName("Settings")
    .withParent("zero.creation") 
    .withIcon("e8b8")
    .withDefinition(function() configModule:renderDialog() end)
    .add()