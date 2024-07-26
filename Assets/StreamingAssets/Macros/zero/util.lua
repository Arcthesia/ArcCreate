local module = {}

local arcOnlyConstraint = EventSelectionConstraint.create().arc()

---@param message string
---@return LuaArc
function module.getArc(message)
    local req = EventSelectionInput.requestSingleEvent(
        arcOnlyConstraint,
        message or "Select an arc")
    coroutine.yield()
    return req.result.arc[1]
end

---@param showVertical bool
---@param message string
---@return number
function module.getTiming(showVertical, message)
    local req = TrackInput.requestTiming(showVertical or false, message or "Select timing")
    coroutine.yield()
    return req.result.timing
end

---@param startTiming number
---@param message string
---@return XY
function module.getPosition(startTiming, message)
    local req = TrackInput.requestPosition(startTiming, message or "Select position")
    coroutine.yield()
    return req.result.xy
end


XYT = {}
---@class XYT
---@field public x number
---@field public y number
---@field public timing number
---@field public xy number
XYT_inst = {}

---@param message string
---@return XYT
function module.getTimingAndPosition(timingMessage, positionMessage)
    local timing = module.getTiming(true, timingMessage)
    local xy = module.getPosition(timing, positionMessage)
    return {
        x = xy.x,
        y = xy.y,
        timing = timing,
        xy = xy,
    }
end

function module.operateOnSelectedArcs(operation)
    local selection = Event.getCurrentSelection(arcOnlyConstraint)
    if #selection.arc < 1
    then
        operation(module.getArc())
    else
        for _, arc in ipairs(selection.arc) do
            operation(arc)
        end
    end
end

local helpFields = {
    DialogField.create("1").description("Collection of built-in macros. Created by 0thElement\n"),
    DialogField.create("2").description("Included tools:"),
}

---@param parentId string
---@param selfId string
---@param displayName string
---@param icon string
---@param help string
function module.zeroMacro(parentId, selfId, displayName, icon, help, macroDef)
    local id = parentId.."."..selfId
    Macro.new(id)
        .withParent(parentId)
        .withName(displayName)
        .withIcon(icon)
        .withDefinition(macroDef)
        .add()
    module.addHelp(displayName, id, help)
end

---@param displayName string
---@param id string
---@param help string
function module.addHelp(displayName, id, help)
    helpFields[#helpFields + 1] = DialogField
        .create(tostring(#helpFields + 1))
        .description(" - <b>".. displayName .. "</b> <size=8>("..id..")</size>\n".. help)
end

function module.renderHelp()
    DialogInput.withTitle("Help").requestInput(helpFields)
end

---@param bool boolean
function module.choose(bool, vtrue, vfalse)
    if bool then return vtrue else return vfalse end
end

return module