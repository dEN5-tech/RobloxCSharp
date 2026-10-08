--[[
  Roblox Luau CoreSystem Runtime for C# Transpilation
  Supports OOP Classes, Inheritance, Constructors, Properties, and Collections.
--]]

local System = {}
_G.System = System
_G.RobloxCSharp = _G.RobloxCSharp or {}

local GameObj = {
    Workspace = workspace,
    Players = game:GetService("Players"),
    Lighting = game:GetService("Lighting"),
    ReplicatedStorage = game:GetService("ReplicatedStorage"),
    ServerScriptService = game:GetService("ServerScriptService"),
    StarterPlayer = game:GetService("StarterPlayer"),
    SoundService = game:GetService("SoundService"),
}
setmetatable(GameObj, {
    __index = function(tbl, key)
        if key == "GetService" then
            return function(selfOrName, maybeName)
                local name = maybeName or selfOrName
                return game:GetService(name)
            end
        end
        local ok, s = pcall(function() return game:GetService(key) end)
        if ok and s then return s end
        return nil
    end
})

_G.Roblox = _G.Roblox or {}
_G.Roblox.Game = GameObj
_G.Roblox.Vector3 = Vector3
_G.Roblox.CFrame = CFrame
_G.Roblox.Color3 = Color3
_G.Roblox.UDim = UDim
_G.Roblox.UDim2 = UDim2
_G.Roblox.NumberRange = NumberRange
_G.Roblox.Instance = Instance

local globalScope = _G
local classes = {}
local importQueue = {}

-- Helper to set/get nested table in global scope
local function getOrCreateNamespace(fullName)
    local current = globalScope
    for segment in string.gmatch(fullName, "([^.]+)") do
        if not current[segment] then
            current[segment] = {}
        end
        current = current[segment]
    end
    return current
end

-- 1. Namespace registration
function System.namespace(namespaceName, bodyFn)
    local ns = getOrCreateNamespace(namespaceName)
    local builder = {
        class = function(className, classDefFn)
            local classMembers = classDefFn(ns) or {}
            
            -- Set up base class inheritance if declared
            local baseClasses = {}
            if classMembers.base and type(classMembers.base) == "function" then
                local res = classMembers.base(globalScope)
                if type(res) == "table" then
                    baseClasses = res
                end
            end

            local baseClass = baseClasses[1]
            if baseClass then
                setmetatable(classMembers, { __index = baseClass })
            end
            classMembers.__index = classMembers
            classMembers.__base = baseClass

            -- Constructor wrapper: ClassName(...) -> new instance
            local classCallable = setmetatable({}, {
                __index = classMembers,
                __call = function(tbl, ...)
                    local instance = setmetatable({}, classMembers)
                    if classMembers.__ctor__ then
                        classMembers.__ctor__(instance, ...)
                    end
                    return instance
                end
            })

            -- Static constructor/initializer
            if classMembers.static then
                -- Safe execute static initializer
                pcall(function()
                    classMembers.static(classCallable)
                end)
            end

            ns[className] = classCallable
            classes[namespaceName .. "." .. className] = classCallable
            return classCallable
        end,
        struct = function(structName, structDefFn)
            local st = structDefFn(ns) or {}
            st.__index = st
            ns[structName] = st
            return st
        end
    }

    bodyFn(builder)
    return ns
end

-- 2. Base class access for super calls: System.base(this)
function System.base(instance)
    local meta = getmetatable(instance)
    if meta and meta.__base then
        return meta.__base
    end
    return {
        __ctor__ = function() end
    }
end

-- 3. Import deferred hooks (System.import)
function System.import(importFn)
    table.insert(importQueue, importFn)
    pcall(importFn, globalScope)
end

-- 4. Collections: System.List & System.Dictionary & System.Array
function System.Array(elementType)
    return function(sizeOrTable)
        if type(sizeOrTable) == "table" then
            return sizeOrTable
        end
        return {}
    end
end

function System.each(collection)
    if type(collection) == "table" then
        if collection.items then
            return ipairs(collection.items)
        else
            return ipairs(collection)
        end
    end
    return function() return nil end
end

function System.List(elementType)
    local ListMeta = {}
    ListMeta.__index = ListMeta

    function ListMeta:Add(item)
        table.insert(self.items, item)
    end

    function ListMeta:Remove(item)
        for i, val in ipairs(self.items) do
            if val == item then
                table.remove(self.items, i)
                return true
            end
        end
        return false
    end

    function ListMeta:Count()
        return #self.items
    end

    function ListMeta:Get(index)
        return self.items[index + 1]
    end

    local constructor = function()
        local list = setmetatable({ items = {} }, ListMeta)
        return list
    end

    return constructor
end

function System.Dictionary(keyType, valueType)
    local DictMeta = {}
    DictMeta.__index = DictMeta

    function DictMeta:Add(k, v)
        self.map[k] = v
    end

    function DictMeta:Remove(k)
        self.map[k] = nil
    end

    function DictMeta:ContainsKey(k)
        return self.map[k] ~= nil
    end

    function DictMeta:Get(k)
        return self.map[k]
    end

    return function()
        return setmetatable({ map = {} }, DictMeta)
    end
end

-- 5. Type casting and primitives
System.Object = {}
System.String = {}
System.Int32 = {}
System.Double = {}
System.Boolean = {}
System.Action = function(...) return function() end end
System.Func = function(...) return function() end end
System.Exception = function(msg) return { Message = msg } end
System.NullReferenceException = function(msg) return { Message = msg or "Null reference" } end

function System.cast(targetType, obj)
    return obj
end

function System.as(obj, targetType)
    return obj
end

function System.is(obj, targetType)
    return obj ~= nil
end

function System.toString(val)
    if val == nil then return "" end
    return tostring(val)
end

-- 6. Console logging
System.Console = {
    WriteLine = function(...)
        local args = { ... }
        for i, v in ipairs(args) do
            args[i] = tostring(v)
        end
        print(table.concat(args, " "))
    end,
    Write = function(...)
        local args = { ... }
        for i, v in ipairs(args) do
            args[i] = tostring(v)
        end
        print(table.concat(args, " "))
    end
}

-- 7. Assembly bootstrap (System.init)
function System.init(config)
    if type(config) == "table" and config.path then
        local folder = config.path
        if config.files and type(config.files) == "table" then
            for _, fileName in ipairs(config.files) do
                local child = folder:FindFirstChild(fileName)
                if child and child:IsA("ModuleScript") then
                    pcall(require, child)
                end
            end
        else
            for _, child in ipairs(folder:GetChildren()) do
                if child:IsA("ModuleScript") and child.Name ~= "manifest" then
                    pcall(require, child)
                end
            end
        end
    end

    for _, fn in ipairs(importQueue) do
        pcall(fn, globalScope)
    end
    return System
end

return System

