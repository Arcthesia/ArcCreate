---Utility class for converting between types
Convert = {}

---Utility class for converting between types
---@class Convert
---@param r number
---@param g number
---@param b number
---@param a number
---@return string
function Convert.RGBAToHex(r, g, b, a) end

---@param rgba RGBA
---@return string
function Convert.RGBAToHex(rgba) end

---@param hsva HSVA
---@return string
function Convert.HSVAToHex(hsva) end

---@param hex string
---@return RGBA
function Convert.HexToRGBA(hex) end

---@param hex string
---@return HSVA
function Convert.HexToHSVA(hex) end

---@param r number
---@param g number
---@param b number
---@param a number
---@return HSVA
function Convert.RGBAToHSVA(r, g, b, a) end

---@param rgba RGBA
---@return HSVA
function Convert.RGBAToHSVA(rgba) end

---@param h number
---@param s number
---@param v number
---@param a number
---@return RGBA
function Convert.HSVAToRGBA(h, s, v, a) end

---@param hsva HSVA
---@return RGBA
function Convert.HSVAToRGBA(hsva) end

HSVA = {}

---@class HSVA
---@field public h number
---@field public s number
---@field public v number
---@field public a number
HSVA__inst = {}

---@return string
function HSVA__inst.toString() end

RGBA = {}

---@class RGBA
---@field public r number
---@field public g number
---@field public b number
---@field public a number
RGBA__inst = {}

---@return string
function RGBA__inst.toString() end

XY = {}

---@class XY
---@field public x number
---@field public y number
XY__inst = {}

---@param axis number
---@return XY
function XY__inst.mirrorX(axis) end

---@param axis number
---@return XY
function XY__inst.mirrorY(axis) end

---@return string
function XY__inst.toString() end

XYZ = {}

---@class XYZ
---@field public x number
---@field public y number
---@field public z number
XYZ__inst = {}

---@param axis number
---@return XYZ
function XYZ__inst.mirrorX(axis) end

---@param axis number
---@return XYZ
function XYZ__inst.mirrorY(axis) end

---@param axis number
---@return XYZ
function XYZ__inst.mirrorZ(axis) end

---@return string
function XYZ__inst.toString() end

---@param x number
---@param y number
---@return XY
function xy(x, y) end

---@param x number
---@param y number
---@param z number
---@return XYZ
function xyz(x, y, z) end

---@param h number
---@param s number
---@param v number
---@param a number
---@return HSVA
function hsva(h, s, v, a) end

---@param r number
---@param g number
---@param b number
---@param a number
---@return RGBA
function rgba(r, g, b, a) end

---@param content any
function log(content) end

---@param value any
---@return number
function toNumber(value) end

---@param value any
---@return boolean
function toBool(value) end

---Constraint for filtering events selection.
EventSelectionConstraint = {}

---Constraint for filtering events selection.
---@class EventSelectionConstraint
EventSelectionConstraint__inst = {}

---Create a new constraint.
---@return EventSelectionConstraint
function EventSelectionConstraint.create() end

---Set the event type to filter for
---@param type ('any' | 'tap' | 'hold' | 'arc' | 'solidarc' | 'voidarc' | 'trace' | 'arctap' | 'timing' | 'camera' | 'floor' | 'sky' | 'short' | 'long' | 'judgeable')
---@return EventSelectionConstraint
function EventSelectionConstraint__inst.setType(type) end

---Filter for all event types.
---@return EventSelectionConstraint
function EventSelectionConstraint__inst.any() end

---Filter for timing events.
---@return EventSelectionConstraint
function EventSelectionConstraint__inst.timing() end

---Filter for camera events.
---@return EventSelectionConstraint
function EventSelectionConstraint__inst.camera() end

---Filter for scenecontrol events.
---@return EventSelectionConstraint
function EventSelectionConstraint__inst.scenecontrol() end

---Filter for tap events.
---@return EventSelectionConstraint
function EventSelectionConstraint__inst.tap() end

---Filter for hold events.
---@return EventSelectionConstraint
function EventSelectionConstraint__inst.hold() end

---Filter for arc and trace events.
---@return EventSelectionConstraint
function EventSelectionConstraint__inst.arc() end

---Filter for trace events.
---@return EventSelectionConstraint
function EventSelectionConstraint__inst.voidArc() end

---Filter for trace events.
---@return EventSelectionConstraint
function EventSelectionConstraint__inst.trace() end

---Filter for arc events (without trace events).
---@return EventSelectionConstraint
function EventSelectionConstraint__inst.solidArc() end

---Filter for arctap events.
---@return EventSelectionConstraint
function EventSelectionConstraint__inst.arctap() end

---Filter for arctap events.
---@return EventSelectionConstraint
function EventSelectionConstraint__inst.arcTap() end

---Filter for tap and hold events.
---@return EventSelectionConstraint
function EventSelectionConstraint__inst.floor() end

---Filter for hold, arc and trace events.
---@return EventSelectionConstraint
function EventSelectionConstraint__inst.long() end

---Filter for tap and arctap events.
---@return EventSelectionConstraint
function EventSelectionConstraint__inst.short() end

---Filter for arctap, arc and trace events.
---@return EventSelectionConstraint
function EventSelectionConstraint__inst.sky() end

---Filter for tap, arctap, hold, arc events (without trace events).
---@return EventSelectionConstraint
function EventSelectionConstraint__inst.judgeable() end

---Filter for notes with timing larger than the provided value.
---@param timing integer
---@return EventSelectionConstraint
function EventSelectionConstraint__inst.fromTiming(timing) end

---Filter for notes with smaller than the provided value.
---@param timing integer
---@return EventSelectionConstraint
function EventSelectionConstraint__inst.toTiming(timing) end

---Filter for notes belonging to the group-th timing group.
---@param group integer
---@return EventSelectionConstraint
function EventSelectionConstraint__inst.ofTimingGroup(group) end

---Set a custom filter.
---@param function any
---@param message string
---@return EventSelectionConstraint
function EventSelectionConstraint__inst.custom(_function, message) end

---Also include notes that passes the provided constraint.
---@param constraint EventSelectionConstraint
---@return EventSelectionConstraint
function EventSelectionConstraint__inst.union(constraint) end

---Get the constraint description.
---@return string
function EventSelectionConstraint__inst.getConstraintDescription() end

---Check if an event satisfy this constraint.
---@param value LuaChartEvent
---@return boolean
function EventSelectionConstraint__inst.checkEvent(value) end

---Constraint for checking text input.
FieldConstraint = {}

---Constraint for checking text input.
---@class FieldConstraint
FieldConstraint__inst = {}

---Create a new constraint.
---@return FieldConstraint
function FieldConstraint.create() end

---Filter for all texts.
---@return FieldConstraint
function FieldConstraint__inst.any() end

---Filter for integers.
---@return FieldConstraint
function FieldConstraint__inst.integer() end

---Filter for float numbers (i.e decimal numbers).
---@return FieldConstraint
function FieldConstraint__inst.float() end

---Filter for number greater or equal than the provided value.
---@param value number
---@return FieldConstraint
function FieldConstraint__inst.gEqual(value) end

---Filter for number less or equal than the provided value.
---@param value number
---@return FieldConstraint
function FieldConstraint__inst.lEqual(value) end

---Filter for number greater than the provided value.
---@param value number
---@return FieldConstraint
function FieldConstraint__inst.greater(value) end

---Filter for number less than the provided value.
---@param value number
---@return FieldConstraint
function FieldConstraint__inst.less(value) end

---Set a custom filter.
---@param function any
---@param message string
---@return FieldConstraint
function FieldConstraint__inst.custom(_function, message) end

---Also include texts that passes the provided constraint.
---@param constraint FieldConstraint
---@return FieldConstraint
function FieldConstraint__inst.union(constraint) end

---Get the constraint description.
---@return string
function FieldConstraint__inst.getConstraintDescription() end

Event = {}

---@class Event
---Create an arc event data.
---@param timing number
---@param startX number
---@param startY number
---@param endTiming number
---@param endX number
---@param endY number
---@param isTrace boolean
---@param color integer
---@param type ('b' | 's' | 'si' | 'so' | 'sisi' | 'soso' | 'siso' | 'sosi')
---@param timingGroup integer
---@param sfx string
---@return LuaArc
function Event.arc(timing, startX, startY, endTiming, endX, endY, isTrace, color, type, timingGroup, sfx) end

---Create an arc event data.
---@param timing number
---@param startXY XY
---@param endTiming number
---@param endXY XY
---@param isTrace boolean
---@param color integer
---@param type ('b' | 's' | 'si' | 'so' | 'sisi' | 'soso' | 'siso' | 'sosi')
---@param timingGroup integer
---@param sfx string
---@return LuaArc
function Event.arc(timing, startXY, endTiming, endXY, isTrace, color, type, timingGroup, sfx) end

---Create a tap event data.
---@param timing integer
---@param lane integer
---@param timingGroup integer
---@return LuaTap
function Event.tap(timing, lane, timingGroup) end

---Create a hold event data.
---@param timing integer
---@param endTiming integer
---@param lane integer
---@param timingGroup integer
---@return LuaHold
function Event.hold(timing, endTiming, lane, timingGroup) end

---Create an arctap event data.
---@param timing integer
---@param arc LuaArc
---@return LuaArcTap
function Event.arcTap(timing, arc) end

---Create an arctap event data.
---@param timing integer
---@param arc LuaArc
---@return LuaArcTap
function Event.arctap(timing, arc) end

---Create a timing event data.
---@param timing integer
---@param bpm number
---@param divisor number
---@param timingGroup integer
---@return LuaTiming
function Event.timing(timing, bpm, divisor, timingGroup) end

---Create a camera event data.
---@param timing integer
---@param move XYZ
---@param rotate XYZ
---@param type ('l' | 'qi' | 'qo' | 'reset' | 's')
---@param duration integer
---@param timingGroup integer
---@return LuaCamera
function Event.camera(timing, move, rotate, type, duration, timingGroup) end

---Create a camera event data.
---@param timing integer
---@param x number
---@param y number
---@param z number
---@param rx number
---@param ry number
---@param rz number
---@param type ('l' | 'qi' | 'qo' | 'reset' | 's')
---@param duration integer
---@param timingGroup integer
---@return LuaCamera
function Event.camera(timing, x, y, z, rx, ry, rz, type, duration, timingGroup) end

---Create a scenecontrol event data.
---@param timing integer
---@param type string
---@param timingGroup integer
---@param args any[]
---@return LuaScenecontrol
function Event.scenecontrol(timing, type, timingGroup, args) end

---Create a timing group object. Properties are defined with a string whose format is the same as .aff chart format.You must use LuaTimingGroup.save() in order to add this timing group to the chart.
---@param properties string
---@return LuaTimingGroup
function Event.createTimingGroup(properties) end

---Deprecated, use Event.CreateTimingGroup() instead. Create a timing group object. Properties are defined with a string whose format is the same as .aff chart format.
---@param properties string
---@return LuaTimingGroup
function Event.createTimingGroupProperty(properties) end

---Deprecated. This method does not support the latest timing group properties, use Event.CreateTimingGroup() instead.Create a timing group object. Properties are defined with a table.
---@param properties table<string, any>
---@return LuaTimingGroup
function Event.createTimingGroupProperty(properties) end

---Not recommended, use LuaTimingGroup.save() instead. Add a timing group to the current chart.
---@param properties LuaTimingGroup
function Event.addTimingGroup(properties) end

---Not recommended, use LuaTimingGroup.save() instead. Change an existing timing group's properties. Properties are defined with a property string, similar to that of .aff chart format
---@param group integer
---@param properties LuaTimingGroup
function Event.setTimingGroupProperty(group, properties) end

---Get the n-th timing group of the current chart.
---@param group integer
---@return LuaTimingGroup
function Event.getTimingGroup(group) end

---Get the current note selection. If a constraint is provided, then only notes that fit the constraint is returned.
---@param constraint EventSelectionConstraint
---@return EventSelectionRequest
function Event.getCurrentSelection(constraint) end

---Set the selections to the provided notes. Non-selectable events (such as timing events) are ignored.
---@param notes any
function Event.setSelection(notes) end

---Query for all notes. If a constraint is provided, then only notes that fit the constraint is returned.
---@param constraint EventSelectionConstraint
---@return EventSelectionRequest
function Event.query(constraint) end

---Description for constructing a dialog field.
DialogField = {}

---Description for constructing a dialog field.
---@class DialogField
---@field public key string
---@field public label string
---@field public tooltip string
---@field public hint string
---@field public dropdownOptions any[]
---@field public defaultValue any
---@field public fieldConstraint FieldConstraint
---@field public fieldType any
DialogField__inst = {}

---Create a new dialog field. The provided key is used for accessing this field's result value.
---@param key string
---@return DialogField
function DialogField.create(key) end

---Set the field's label.
---@param label string
---@return DialogField
function DialogField__inst.setLabel(label) end

---Set the field's tooltip, which is shown when user hovers over the field.
---@param tooltip string
---@return DialogField
function DialogField__inst.setTooltip(tooltip) end

---Set the field's hint, which is shown when the field is empty.
---@param hint string
---@return DialogField
function DialogField__inst.setHint(hint) end

---Set the field's default value.
---@param value any
---@return DialogField
function DialogField__inst.defaultTo(value) end

---Change the field type to a text input field. Only accepts text input that fits the provided constraint.
---@param constraint FieldConstraint
---@return DialogField
function DialogField__inst.textField(constraint) end

---Change the field type to a dropdown field.
---@param options any[]
---@return DialogField
function DialogField__inst.dropdownMenu(options) end

---Change the field type to a checkbox field.
---@return DialogField
function DialogField__inst.checkbox() end

---Change the field type to a description field.
---@param description string
---@return DialogField
function DialogField__inst.description(description) end

---Class for requesting input from user through a dialog.
DialogInput = {}

---Class for requesting input from user through a dialog.
---@class DialogInput
---@field public dialogTitle string
---@field public dialogFields DialogField[]
DialogInput__inst = {}

---Create a dialog with the provided title.
---@param message string
---@return DialogInput
function DialogInput.withTitle(message) end

---Start the request. Returned value of each field is access through the key of the field.
---@param fields DialogField[]
---@return MacroRequest
function DialogInput__inst.requestInput(fields) end

---Class for requesting note selection input from user.
EventSelectionInput = {}

---Class for requesting note selection input from user.
---@class EventSelectionInput
EventSelectionInput__inst = {}

---Request for selection of a single event.
---@param constraint EventSelectionConstraint
---@param notification string
---@return EventSelectionRequest
function EventSelectionInput.requestSingleEvent(constraint, notification) end

---Request for selection of multiple events.
---@param constraint EventSelectionConstraint
---@param notification string
---@return EventSelectionRequest
function EventSelectionInput.requestEvents(constraint, notification) end

---Containment of chart events data.
EventSelectionRequest = {}

---Containment of chart events data.
---@class EventSelectionRequest
---Access the lists of events separated by event types, each sorted by timing (e.g. result["tap"] contains all tap notes). All event types are: tap, hold, arc, arctap, timing, camera, scenecontrol
---@field public result table<string, LuaChartEvent[]>
---Get returned tap notes
---@field public tap LuaChartEvent[]
---Get returned hold notes
---@field public hold LuaChartEvent[]
---Get returned arc notes
---@field public arc LuaChartEvent[]
---Get returned arctap notes
---@field public arctap LuaChartEvent[]
---Get returned timing events
---@field public timing LuaChartEvent[]
---Get returned scenecontrol events
---@field public scenecontrol LuaChartEvent[]
---Get returned camera events
---@field public camera LuaChartEvent[]
---Access the combined list of events sorted by timing.
---@field public resultCombined LuaChartEvent[]
EventSelectionRequest__inst = {}

---Containment of data retuend from user input requests.
MacroRequest = {}

---Containment of data retuend from user input requests.
---@class MacroRequest
---@field public result table<string, any>
MacroRequest__inst = {}

---Class for requesting input from user through interacting with the track.
TrackInput = {}

---Class for requesting input from user through interacting with the track.
---@class TrackInput
TrackInput__inst = {}

---DEPRECATED. showVertical is no longer supported
---@param showVertical boolean
---@param notification string
---@return MacroRequest
function TrackInput.requestTiming(showVertical, notification) end

---Request a timing selection. Returned value is accessed through the key "timing"
---@param notification string
---@return MacroRequest
function TrackInput.requestTiming(notification) end

---Request a vertical position selection. Returned value is accessed through the key "x", "y" or "xy"
---@param timing integer
---@param notification string
---@return MacroRequest
function TrackInput.requestPosition(timing, notification) end

---Request a lane selection. Returned value is accessed through the key "lane"
---@param notification string
---@return MacroRequest
function TrackInput.requestLane(notification) end

Macro = {}

---@class Macro
---Create a new macro builder. The id should be unique.
---@param id string
---@return MacroBuilder
function Macro.new(id) end

MacroBuilder = {}

---@class MacroBuilder
MacroBuilder__inst = {}

---Set the macro display name.
---@param name string
---@return MacroBuilder
function MacroBuilder__inst.withName(name) end

---Set the display icon display code. Should be a Material icon unicode (example: e1666).
---@param icon string
---@return MacroBuilder
function MacroBuilder__inst.withIcon(icon) end

---Set the parent node. The value should be the id of the parent node.
---@param parent string
---@return MacroBuilder
function MacroBuilder__inst.withParent(parent) end

---Set the macro definition. The value is a function that will be executed every time the macro is run (example: `funcion() ... end`)
---@param function any
---@return MacroBuilder
function MacroBuilder__inst.withDefinition(_function) end

---Add the macro to the application.
---@return MacroBuilder
function MacroBuilder__inst.add() end

---Create a new macro builder inheriting the same settings from the current instance
---@param id string
---@return MacroBuilder
function MacroBuilder__inst.new(id) end

Folder = {}

---@class Folder
---@param id string
---@return FolderBuilder
function Folder.new(id) end

FolderBuilder = {}

---@class FolderBuilder
FolderBuilder__inst = {}

---Set the macro display name.
---@param name string
---@return FolderBuilder
function FolderBuilder__inst.withName(name) end

---Set the display icon display code. Should be a Material icon unicode (example: e1666).
---@param icon string
---@return FolderBuilder
function FolderBuilder__inst.withIcon(icon) end

---Set the parent node. The value should be the id of the parent node.
---@param parent string
---@return FolderBuilder
function FolderBuilder__inst.withParent(parent) end

---Add the folder to the application.
---@return FolderBuilder
function FolderBuilder__inst.add() end

---Create a new folder builder inheriting the same settings from the current instance
---@param id string
---@return FolderBuilder
function FolderBuilder__inst.new(id) end

Context = {}

---@type number
Context.dropRate = nil

---@type number
Context.offset = nil

---@type number
Context.timingPointDensityFactor = nil

---@type number
Context.beatlineDensity = nil

---@type string
Context.language = nil

---@type number
Context.baseBpm = nil

---@type integer
Context.songLength = nil

---@type integer
Context.maxCombo = nil

---@type integer
Context.noteCount = nil

---@type string
Context.title = nil

---@type string
Context.composer = nil

---@type string
Context.charter = nil

---@type string
Context.alias = nil

---@type string
Context.difficulty = nil

---@type string
Context.difficultyColor = nil

---@type string
Context.chartPath = nil

---@type string
Context.side = nil

---@type boolean
Context.isLight = nil

---@type integer
Context.currentArcColor = nil

---@type integer
Context.maxArcColor = nil

---@type string[]
Context.allArcTypes = nil

---@type string
Context.currentArcType = nil

---@type boolean
Context.currentIsVoidMode = nil

---@type boolean
Context.currentIsTraceMode = nil

---@type integer
Context.currentTimingGroup = nil

---@type integer
Context.timingGroupCount = nil

---@type integer
Context.currentTiming = nil

---@type integer
Context.screenWidth = nil

---@type integer
Context.screenHeight = nil

---@type number
Context.screenAspectRatio = nil

---@type XY
Context.screenMiddle = nil

---@type string
Context.systemClipboard = nil

---@class Context
---@param timing integer
---@param timingGroup integer
---@return number
function Context.beatLengthAt(timing, timingGroup) end

---@param timing integer
---@param timingGroup integer
---@return number
function Context.bpmAt(timing, timingGroup) end

---@param timing integer
---@param timingGroup integer
---@return number
function Context.divisorAt(timing, timingGroup) end

---@param timing integer
---@param timingGroup integer
---@return number
function Context.floorPositionAt(timing, timingGroup) end

Persistent = {}

---@class Persistent
---Get the string data for a given key. Returns defaultValue if valid data can not be found.
---@param key string
---@param defaultValue string
---@return string
function Persistent.getString(key, defaultValue) end

---Set the string data for a given key.
---@param key string
---@param value string
function Persistent.setString(key, value) end

---Save all configuration. This is also done automatically on application shutdown.
function Persistent.save() end

Command = {}

---@class Command
---Create a new command. The provided name will be displayed to the user.
---@param name string
---@param save LuaChartEvent[]
---@param delete LuaChartEvent[]
---@return LuaChartCommand
function Command.create(name, save, delete) end

LuaArc = {}

---@class LuaArc : LuaChartEvent
---@field public startXY XY
---@field public endXY XY
---@field public color integer
---@field public isTrace boolean
---@field public endTiming number
---@field public sfx string
---@field public startX number
---@field public startY number
---@field public endX number
---@field public endY number
---@field public type string
---@field public isVoid boolean
---@field public effect string
LuaArc__inst = {}

---@return LuaChartEvent
function LuaArc__inst.copy() end

---@param timing integer
---@param clamp boolean
---@return XY
function LuaArc__inst.positionAt(timing, clamp) end

---@param timing integer
---@param clamp boolean
---@return number
function LuaArc__inst.xAt(timing, clamp) end

---@param timing integer
---@param clamp boolean
---@return number
function LuaArc__inst.yAt(timing, clamp) end

LuaArcTap = {}

---@class LuaArcTap : LuaChartEvent
---@field public arc LuaArc
---@field public traceTimingGroup integer
LuaArcTap__inst = {}

---@return LuaChartEvent
function LuaArcTap__inst.copy() end

LuaCamera = {}

---@class LuaCamera : LuaChartEvent
---@field public move XYZ
---@field public rotate XYZ
---@field public x number
---@field public y number
---@field public z number
---@field public rx number
---@field public ry number
---@field public rz number
---@field public duration integer
---@field public type string
LuaCamera__inst = {}

---@return LuaChartEvent
function LuaCamera__inst.copy() end

---An editing command that can be undone / redone
LuaChartCommand = {}

---An editing command that can be undone / redone
---@class LuaChartCommand
---The command name which is displayed to the user
---@field public name string
LuaChartCommand__inst = {}

---Combine both command to be executed at once.
---@param c LuaChartCommand
---@return LuaChartCommand
function LuaChartCommand__inst.add(c) end

---Assign all events under this command to a timing group on command execution. Useful for assign to newly created timing group.
---@param timingGroup LuaTimingGroup
---@return LuaChartCommand
function LuaChartCommand__inst.withTimingGroup(timingGroup) end

---Execute the command.
function LuaChartCommand__inst.commit() end

LuaChartEvent = {}

---@class LuaChartEvent
---@field public timing number
---@field public timingGroup integer
---@field public attached boolean
LuaChartEvent__inst = {}

---Create a copy of an event.
---@return LuaChartEvent
function LuaChartEvent__inst.copy() end

---Create a command that saves changes made to this event.
---@return LuaChartCommand
function LuaChartEvent__inst.save() end

---Create a command that delete current event, if it's connected to a real event in the chart.
---@return LuaChartCommand
function LuaChartEvent__inst.delete() end

---Check if the event matches the event type
---@param type ('any' | 'tap' | 'hold' | 'arc' | 'solidarc' | 'voidarc' | 'trace' | 'arctap' | 'timing' | 'camera' | 'floor' | 'sky' | 'short' | 'long' | 'judgeable')
---@return boolean
function LuaChartEvent__inst.is(type) end

---Check if the attached instance equals that of another event
---@param ev LuaChartEvent
---@return boolean
function LuaChartEvent__inst.instanceEquals(ev) end

LuaHold = {}

---@class LuaHold : LuaChartEvent
---@field public lane integer
---@field public endTiming number
LuaHold__inst = {}

---@return LuaChartEvent
function LuaHold__inst.copy() end

LuaScenecontrol = {}

---@class LuaScenecontrol : LuaChartEvent
---@field public type string
---@field public args any[]
LuaScenecontrol__inst = {}

---@return LuaChartEvent
function LuaScenecontrol__inst.copy() end

LuaTap = {}

---@class LuaTap : LuaChartEvent
---@field public lane integer
LuaTap__inst = {}

---@return LuaChartEvent
function LuaTap__inst.copy() end

LuaTiming = {}

---@class LuaTiming : LuaChartEvent
---@field public bpm number
---@field public divisor number
LuaTiming__inst = {}

---@return LuaChartEvent
function LuaTiming__inst.copy() end

LuaTimingGroup = {}

---@class LuaTimingGroup
---@field public num integer
---@field public name string
---@field public noInput boolean
---@field public noClip boolean
---@field public noHeightIndicator boolean
---@field public noShadow boolean
---@field public noHead boolean
---@field public noArcCap boolean
---@field public noConnection boolean
---@field public fadingHolds boolean
---@field public ignoreMirror boolean
---@field public autoplay boolean
---@field public arcResolution number
---@field public angleX number
---@field public angleY number
---@field public judgementSizeX number
---@field public judgementSizeY number
---@field public judgementOffsetX number
---@field public judgementOffsetY number
---@field public judgementOffsetZ number
---@field public side string
---@field public file string
---@field public judgementMaps table<string, string>
---@field public attached boolean
LuaTimingGroup__inst = {}

---Set the properties of this group using aff timing group property syntax.
---@param props string
function LuaTimingGroup__inst.setProperties(props) end

---@return string
function LuaTimingGroup__inst.toString() end

---Create a command that saves changes made to this timing group.
---@return LuaChartCommand
function LuaTimingGroup__inst.save() end

---Create a command that delete current timing group, if it's connected to a real group in the chart.
---@return LuaChartCommand
function LuaTimingGroup__inst.delete() end

---Check if the attached instance equals that of another group
---@param group LuaTimingGroup
---@return boolean
function LuaTimingGroup__inst.instanceEquals(group) end

---Remove a macro or folder. Does nothing if the macro has not been added.
---@param id string
function removeMacro(id) end

---@param content any
function notify(content) end

---@param content any
function notifyWarn(content) end

---@param content any
function notifyError(content) end

