--[[
  Roblox Client Loader for C#
  Loads CoreSystem and Launches Client Logic.
--]]

local ReplicatedStorage = game:GetService("ReplicatedStorage")
local System = require(ReplicatedStorage:WaitForChild("CoreSystem"))

-- Загружаем C# клиентские модули из папки Generated
local generatedFolder = ReplicatedStorage:WaitForChild("RobloxCSharp")
local manifestMod = generatedFolder:WaitForChild("manifest")
local manifestFn = require(manifestMod)
if type(manifestFn) == "function" then
    manifestFn(generatedFolder)
end

-- Гарантируем загрузку всех модулей (fallback)
for _, mod in ipairs(generatedFolder:GetChildren()) do
    if mod:IsA("ModuleScript") and mod.Name ~= "manifest" then
        pcall(require, mod)
    end
end

-- Запуск клиентской C# инициализации
task.defer(function()
    local app = _G.RobloxCSharp
    if app and app.Client and app.Client.MainClient then
        app.Client.MainClient.Init()
    end
end)
