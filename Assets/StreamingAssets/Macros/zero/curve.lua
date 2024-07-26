local util = require("zero.util")

Folder.new("zero.curve")
    .withParent("zero")
    .withIcon("e7e1")
    .withName("Curves").add()

util.zeroMacro(
    "zero.curve", "followSingle", 
    "Follow single curves", "e5cc", 
    "Follow single curve. Only position is modified.",
    function()
        local batchCommand = Command.create()
        ---@type LuaArc
        local originArc = nil
        ---@type LuaArc
        local targetArc = nil

        local function transform(point, timing)
            local origin = xy(originArc.xAt(timing, true), originArc.yAt(timing, true))
            local target = xy(targetArc.xAt(timing, true), targetArc.yAt(timing, true))
            return point - origin + target
        end

        util.operateOnSelectedArcs(function(arc)
            Event.setSelection({})
            originArc = originArc or util.getArc("Select origin curve")
            targetArc = targetArc or util.getArc("Select target curve")
            arc.startXY = transform(arc.startXY, arc.timing)
            arc.endXY = transform(arc.endXY, arc.endTiming)
            batchCommand.add(arc.save())
        end)

        batchCommand.commit()
end)

util.zeroMacro(
    "zero.curve", "followTwo", 
    "Follow two curves", "eac9", 
    "Follow two curves. Position, scale, rotation of notes are modified.",
    function()
        local batchCommand = Command.create()
        ---@type LuaArc
        local origin1 = nil
        ---@type LuaArc
        local origin2 = nil
        ---@type LuaArc
        local target1 = nil
        ---@type LuaArc
        local target2 = nil

        local function length(vec)
            return math.sqrt((vec.x * vec.x) + (vec.y * vec.y))
        end

        local function dot(vec1, vec2)
            return (vec1.x * vec2.x) + (vec1.y * vec2.y)
        end

        local function unit(vec)
            return vec / length(vec)
        end

        local function normal(vec)
            return unit(xy(vec.y, -vec.x))
        end

        local function sign(num)
            if num > 0 then return 1 elseif num == 0 then return 0 else return -1 end
        end

        local function transform(point, timing)
            local originA = xy(origin1.xAt(timing, true), origin1.yAt(timing, true))
            local originB = xy(origin2.xAt(timing, true), origin2.yAt(timing, true))
            local targetA = xy(target1.xAt(timing, true), target1.yAt(timing, true))
            local targetB = xy(target2.xAt(timing, true), target2.yAt(timing, true))

            local vecOriginAOriginB = originB - originA
            local vecTargetATargetB = targetB - targetA

            local lengthOriginAB = length(vecOriginAOriginB)
            local lengthTargetAB = length(vecTargetATargetB)
            local ratio = lengthTargetAB / lengthOriginAB

            local vecOriginAPoint = point - originA

            local lengthOriginAOriginG = dot(vecOriginAPoint, vecOriginAOriginB) / lengthOriginAB
            local vecTargetATargetG = unit(vecTargetATargetB) * lengthOriginAOriginG * ratio * sign(dot(vecOriginAPoint, vecOriginAOriginB))

            local targetNormal = normal(vecTargetATargetB)
            local originNormal = normal(vecOriginAOriginB)

            local lengthOriginAPoint = length(vecOriginAPoint)
            local lengthOriginGOriginP = math.sqrt(lengthOriginAPoint * lengthOriginAPoint - lengthOriginAOriginG * lengthOriginAOriginG)

            local vecTargetGTargetP = targetNormal * sign(dot(vecOriginAPoint, originNormal)) * lengthOriginGOriginP * ratio

            return targetA + vecTargetATargetG + vecTargetGTargetP
        end

        util.operateOnSelectedArcs(function(arc)
            Event.setSelection({})
            origin1 = origin1 or util.getArc("Select first origin curve")
            origin2 = origin2 or util.getArc("Select second origin curve")
            target1 = target1 or util.getArc("Select first target curve")
            target2 = target2 or util.getArc("Select second target curve")
            arc.startXY = transform(arc.startXY, arc.timing)
            arc.endXY = transform(arc.endXY, arc.endTiming)
            batchCommand.add(arc.save())
        end)

        batchCommand.commit()
end)