local util = require('zero.util')
local min_color = { 65,65,65 }
local max_color = { 99,183,209 }

local function allocate(events, count, length, counter)
    local table = {}
    for i = 1, count do table[i] = 0 end

    local max = 0;
    local slot_length = length / count;
    for _, e in ipairs(events.resultCombined) do
        counter(e, function(timing)
            local slot = math.floor(timing / slot_length)
            if slot > 0 and slot <= #table then
                table[slot] = table[slot] + timing
                if table[slot] > max then max = table[slot] end
            end
        end)
    end

    for i = 1, #table do
        table[i] = table[i] / max
    end
    return table
end

local function allocation_to_str(allocation)
    local t = {}
    for i = 1, #allocation do
        local num = allocation[i]
        local r = min_color[1] + (max_color[1] - min_color[1]) * num;
        local g = min_color[2] + (max_color[2] - min_color[2]) * num;
        local b = min_color[3] + (max_color[3] - min_color[3]) * num;
        t[i] = string.format("<color=#%x%x%x>■</color>", r, g, b)
    end
    return "["..table.concat(t, "").."<color=#ffffff>]</color>"
end

util.zeroMacro(
    "zero", "progress",
    "Progress report", "f071",
    "View statistics about the current chart.",
    function()
        local tap = Event.query(EventSelectionConstraint.create().tap()).tap;
        local hold = Event.query(EventSelectionConstraint.create().tap()).hold;
        local arc = Event.query(EventSelectionConstraint.create().solidArc()).arc;
        local trace = Event.query(EventSelectionConstraint.create().voidArc()).arc;
        local arctap = Event.query(EventSelectionConstraint.create().arctap()).arc;
        local timing = Event.query(EventSelectionConstraint.create().timing()).timing;
        local camera = Event.query(EventSelectionConstraint.create().camera()).camera;
        local sc = Event.query(EventSelectionConstraint.create().scenecontrol()).scenecontrol;

        local all = Event.query(EventSelectionConstraint.create().any());
        local length = Context.songLength;
        local char_count = 42;
        local by_count = allocate(all, char_count, length, 
            function(e, adder)
                adder(e.timing)
            end)

        local by_combo = allocate(all, char_count, length, 
            function(e, adder) 
                if not Event.getTimingGroup(e.timingGroup).noInput then
                    if e.is("tap") or e.is("arctap") then adder(e.timing) end
                    if e.is("arc") or e.is("hold") then 
                        local bpm = Context.bpmAt(e.timing, e.timingGroup)
                        local dur = e.endTiming - e.timing
                        if bpm ~= 0 and dur > 0 then
                            bpm = math.abs(bpm)
                            local x = 30000
                            if bpm >= 255 then x = 60000 end
                            local increment = x / bpm / Context.timingPointDensityFactor
                            if math.floor(dur / increment) <= 1 then
                                adder(e.timing + dur / 2)
                            else
                                local first = e.timing + increment;
                                local count = math.floor(dur / increment) - 1;
                                for i = 0, count - 1 do
                                    adder(first + count * i)
                                end
                            end
                        end
                    end
                end
            end)

        local dialog = DialogInput.withTitle("Progress report")
            .requestInput({
                DialogField.create("1").description("<b>Note count:</b> "..#all.resultCombined.." <color=#808080>(" .. Context.maxCombo.. " combo)</color>"),
                DialogField.create("2").description(""..
                    "<color=#63b7d1>Tap:</color> "..#tap..
                    "<color=#63b7d1> | Hold:</color> "..#hold..
                    "<color=#63b7d1> | Sky:</color> "..#arctap..
                    "<color=#63b7d1> | Trace:</color> "..#trace..
                    "<color=#63b7d1> | Arc:</color> "..#arc..
                    "<color=#63b7d1> | Timing:</color> "..#timing..
                    "<color=#63b7d1> | Camera:</color> "..#camera..
                    "<color=#63b7d1> | SC:</color> "..#sc
                ),

                DialogField.create("3").description("- Event count:"),
                DialogField.create("4").description(allocation_to_str(by_count)),
                DialogField.create("5").description("- Combo:"),
                DialogField.create("6").description(allocation_to_str(by_combo)),
            })
end)