Folder.new("zero").withName("Built-in macros").add()

require "zero.creation"
require "zero.arcmod"
require "zero.segmentation"
require "zero.curve"
require "zero.progress"

local util = require "zero.util"
Macro.new("zero.help")
    .withParent("zero")
    .withName("Help")
    .withIcon("e887")
    .withDefinition(function() util.renderHelp() end)
    .add()