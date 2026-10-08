--[[
  Roblox C# SourceMap Error Interceptor & StackTrace Remapper
  Catches runtime errors and translates Lua stack traces into C# source lines!
--]]

local ScriptContext = game:GetService("ScriptContext")
local ErrorHandler = {}

local SourceMapData = nil
pcall(function()
    SourceMapData = require(script.Parent:WaitForChild("SourceMapData", 5))
end)

local function parseLocation(line)
    -- Format 1: Script 'ServerScriptService.Server.MainServer', Line 73
    local m1, l1 = string.match(line, "Script%s+['\"]?([%w_%.]+)['\"]?,%s+[Ll]ine%s+(%d+)")
    if m1 and l1 then
        local shortName = string.match(m1, "([%w_]+)$") or m1
        return shortName, tonumber(l1)
    end

    -- Format 2: ServerScriptService.Server.MainServer, line 73
    local m2, l2 = string.match(line, "([%w_%.]+),%s+[Ll]ine%s+(%d+)")
    if m2 and l2 then
        local shortName = string.match(m2, "([%w_]+)$") or m2
        return shortName, tonumber(l2)
    end

    -- Format 3: ServerScriptService.Server.MainServer:73
    local m3, l3 = string.match(line, "([%w_%.]+):(%d+)")
    if m3 and l3 then
        local shortName = string.match(m3, "([%w_]+)$") or m3
        return shortName, tonumber(l3)
    end

    return nil, nil
end

function ErrorHandler.RemapLine(traceLine)
    local moduleName, lineNum = parseLocation(traceLine)
    if not moduleName or not lineNum then return traceLine end

    if not SourceMapData then
        pcall(function()
            SourceMapData = require(script.Parent:WaitForChild("SourceMapData", 1))
        end)
    end

    if SourceMapData then
        local fileMap = SourceMapData[moduleName]
        if not fileMap then
            for name, data in pairs(SourceMapData) do
                if string.find(moduleName, name) or string.find(name, moduleName) then
                    fileMap = data
                    break
                end
            end
        end

        if fileMap and fileMap.source then
            local csLine = (fileMap.lines and fileMap.lines[lineNum]) or lineNum
            return string.format("  --> [C# SOURCE] %s:line %d (Lua line %d)", fileMap.source, csLine, lineNum)
        end
    end

    return traceLine
end

function ErrorHandler.FormatStackTrace(message, stackTrace)
    -- Extract location from message if present
    local msgModule, msgLine = parseLocation(message)
    local primaryLocation = nil

    if msgModule and msgLine then
        primaryLocation = ErrorHandler.RemapLine(message)
    end

    local lines = string.split(stackTrace or "", "\n")
    local remappedLines = {}

    for _, line in ipairs(lines) do
        local trimmed = string.match(line, "^%s*(.-)%s*$")
        if trimmed and #trimmed > 0 then
            local remapped = ErrorHandler.RemapLine(trimmed)
            table.insert(remappedLines, remapped)
            if not primaryLocation and string.find(remapped, "%[C%# SOURCE%]") then
                primaryLocation = remapped
            end
        end
    end

    local output = {
        "\n================================================================================",
        "🔥 [ROBLOX C# RUNTIME EXCEPTION CAUGHT BY SOURCEMAP HANDLER]",
        "📌 Error Message: " .. tostring(message),
    }

    if primaryLocation then
        table.insert(output, "📍 Exact C# Origin:" .. primaryLocation)
    end

    table.insert(output, "📜 Remapped C# Stack Trace:")
    if #remappedLines > 0 then
        for _, l in ipairs(remappedLines) do
            table.insert(output, l)
        end
    else
        table.insert(output, "  " .. tostring(primaryLocation or stackTrace))
    end
    table.insert(output, "================================================================================\n")

    return table.concat(output, "\n")
end

local isInstalled = false
function ErrorHandler.Install()
    if isInstalled then return end
    isInstalled = true

    ScriptContext.Error:Connect(function(message, stackTrace, scriptInstance)
        local formatted = ErrorHandler.FormatStackTrace(message, stackTrace)
        warn(formatted)
    end)

    print("[SourceMap ErrorHandler] Global C# Error Interceptor successfully installed.")
end

-- Auto-install on module load
task.defer(function()
    ErrorHandler.Install()
end)

return ErrorHandler
