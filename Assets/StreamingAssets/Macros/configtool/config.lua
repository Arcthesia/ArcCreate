---@class Config
---@field value any
---@field field DialogField
Config = {}

---@class ConfigModule
---@field public moduleName string
---@field public fields DialogField[]
---@field protected configsLookup table<string, Config>
ConfigModule = {
    moduleName = "",
    fields = {},
    configsLookup = {},
}
ConfigModule.__index = ConfigModule

---@param moduleName string
---@return ConfigModule
function ConfigModule.new(moduleName)
    local self = setmetatable({}, ConfigModule)
    self.moduleName = moduleName or "Unknown Module"
    self.fields = {}
    self.configsLookup = {}
    return self
end

function ConfigModule:addDescription(text)
    self.fields[#self.fields + 1] = DialogField.create(text).description(text)
end

---@param defaultValue any
---@param field DialogField
---@return Config
function ConfigModule:addField(defaultValue, field)
    ---@type Config
    local newConfig = {
        value = defaultValue,
        field = field
    }

    field.defaultTo(defaultValue)
    self.fields[#self.fields + 1] = field
    self.configsLookup[field.key] = newConfig
    return newConfig
end

function ConfigModule:renderDialog()
    for _, config in pairs(self.configsLookup) do
        config.field.defaultTo(config.value)
    end
    local req = DialogInput.withTitle("Configuration ("..self.moduleName..")").requestInput(self.fields)
    coroutine.yield()
    for key, value in pairs(req.result) do
        self.configsLookup[key].value = value
        self.configsLookup[key].field.defaultTo(value)
    end
end