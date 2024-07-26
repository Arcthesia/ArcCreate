addFolder(nil, "zero", "Built-in macros")

require "zero.creation"
require "zero.arcmod"
require "zero.segmentation"
require "zero.curve"

local util = require "zero.util"
addMacroWithIcon("zero", "zero.help", "Help", "e887", function() util.renderHelp() end)

Folder.new("testFolder")
    .withName("Test Folder")
    .add()

Macro.new("test").withIcon("e887").withName("Test Macro").withParent("testFolder")
    .withDefinition(function ()
        local tg = Event.createTimingGroup(
            'name="TestGroup",noinput,'..
            'noclip,noheightindicator,noshadow,nohead,noarccap,noconnection,'..
            'fadingholds,ignoremirror,autoplay,arcresolution=2,'..
            'anglex=45,angley=45,'..
            'judgesizex=2,judgesizey=2,'..
            'judgeoffsetx=1,judgeoffsety=1,judgeoffsetz=1,'..
            'conflict,'..
            'perfect=miss,goodearly=goodlate,goodlate=goodearly,misslate=missearly,missearly=misslate')
        local cmd = Command.create();
        cmd.add(tg.save())
        cmd.add(Event.tap(6969, 1).save().withTimingGroup(tg))
        cmd.commit()
    end).add()
    .new("test2").withName("Test Macro 2")
    .withDefinition(function()
        local tg1 = Event.getTimingGroup(2)
        local tg2 = Event.getTimingGroup(3)
        local tg = Event.createTimingGroup('name="rmthenadd"')
        local cmd = Command.create();
        tg2.name = "Edited";
        tg2.judgementMaps = {perfect = "miss"};
        cmd.add(tg.save())
        cmd.add(Event.tap(6969, 1).save().withTimingGroup(tg))
        cmd.add(tg1.delete())
        cmd.add(tg2.save())
        cmd.commit()
        local s = ''
        for k,v in pairs(tg2.judgementMaps) do
            s = s.." + ".. k .. "->" .. v
        end
        notify(s)
    end).add()