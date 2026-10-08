--[[
  Roblox Server Loader for C#
  Loads CoreSystem, C# Transpiled Assembly, and Launches Server Logic.
--]]

local ServerScriptService = game:GetService("ServerScriptService")
local ReplicatedStorage = game:GetService("ReplicatedStorage")
local StarterPlayer = game:GetService("StarterPlayer")

-- 0. Очистка устаревших папок Server/Client, если они остались в Studio Place
local legacyServer = ServerScriptService:FindFirstChild("Server")
if legacyServer then legacyServer:Destroy() end

local playerScripts = StarterPlayer:FindFirstChild("StarterPlayerScripts")
if playerScripts then
    local legacyClient = playerScripts:FindFirstChild("Client")
    if legacyClient then legacyClient:Destroy() end
end

local System = require(ReplicatedStorage:WaitForChild("CoreSystem"))

-- 1. Подключаем перехватчик ошибок SourceMap
local ErrorHandler = require(ReplicatedStorage:WaitForChild("CoreSystem"):WaitForChild("ErrorHandler"))
ErrorHandler.Install()

-- 2. Загружаем C# сборку через манифест
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

-- 3. Запускаем C# MainServer.Init()
task.defer(function()
    local app = _G.RobloxCSharp
    if app and app.Server and app.Server.MainServer then
        app.Server.MainServer.Init()
    end
end)
