addFolder(nil, "zero", "Built-in macros")

require "zero.creation"
require "zero.arcmod"
require "zero.segmentation"
require "zero.curve"

local util = require "zero.util"
addMacroWithIcon("zero", "zero.help", "Help", "e887", function() util.renderHelp() end)