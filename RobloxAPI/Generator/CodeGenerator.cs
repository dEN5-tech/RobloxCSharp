using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace RobloxAPI.Generator;

public static class CodeGenerator
{
    private static readonly HashSet<string> ReservedKeywords = new HashSet<string>(StringComparer.Ordinal)
    {
        "as", "break", "const", "continue", "crate", "else", "enum", "extern", "false",
        "fn", "for", "if", "impl", "in", "let", "loop", "match", "mod", "move",
        "mut", "pub", "ref", "return", "self", "Self", "static", "struct", "super",
        "trait", "true", "type", "unsafe", "use", "where", "while", "async", "await",
        "dyn", "abstract", "become", "box", "do", "final", "macro", "override",
        "priv", "typeof", "unsized", "virtual", "yield", "try",
        "base", "bool", "byte", "case", "catch", "char", "checked", "class",
        "decimal", "default", "delegate", "double", "event", "explicit", "fixed", "float",
        "foreach", "goto", "implicit", "int", "interface", "internal", "is", "lock",
        "long", "namespace", "new", "null", "object", "operator", "out", "params",
        "private", "protected", "public", "readonly", "sbyte", "sealed", "short",
        "sizeof", "stackalloc", "string", "switch", "this", "throw", "uint", "ulong",
        "unchecked", "ushort", "using", "volatile", "void"
    };

    private static bool IsAsciiLetterOrDigit(char c) =>
        (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9');

    private static bool IsAsciiDigit(char c) =>
        c >= '0' && c <= '9';

    public static string SanitizeIdent(string name)
    {
        if (string.IsNullOrEmpty(name)) return "Unknown";

        var sb = new StringBuilder(name.Length);
        foreach (var c in name)
        {
            if (IsAsciiLetterOrDigit(c) || c == '_')
            {
                sb.Append(c);
            }
        }

        var s = sb.ToString();
        if (s.Length == 0) return "Unknown";

        if (IsAsciiDigit(s[0]))
        {
            s = "_" + s;
        }

        if (ReservedKeywords.Contains(s))
        {
            return "_" + s;
        }

        return s;
    }

    public static string MapCSharpType(ApiValueType? vt, HashSet<string> classNames)
    {
        if (vt == null) return "object";
        var rawName = vt.Name.TrimEnd('?');
        return (vt.Category, rawName) switch
        {
            ("Primitive", "string") => "string",
            ("Primitive", "bool") => "bool",
            ("Primitive", "int") or ("Primitive", "int64") => "int",
            ("Primitive", "float") or ("Primitive", "double") => "double",
            ("Primitive", "null") or ("Primitive", "void") => "void",
            ("DataType", "Vector3") => "Vector3",
            ("DataType", "CFrame") => "CFrame",
            ("DataType", "Color3") => "Color3",
            ("DataType", "UDim") => "UDim",
            ("DataType", "UDim2") => "UDim2",
            ("DataType", "NumberRange") => "NumberRange",
            ("DataType", "Objects") or ("DataType", "Array") => "object[]",
            ("DataType", "RBXScriptSignal") or ("DataType", "RBXScriptConnection") => "object",
            ("Enum", var enumName) => classNames.Contains(enumName) ? $"{enumName}Enum" : enumName,
            ("Class", var clsName) => classNames.Contains(clsName) ? clsName : "Instance",
            _ => "object"
        };
    }

    public static string GetReturnType(JsonElement? returnTypeElement, HashSet<string> classNames)
    {
        if (!returnTypeElement.HasValue) return "void";
        var el = returnTypeElement.Value;

        if (el.ValueKind == JsonValueKind.Object)
        {
            var cat = el.TryGetProperty("Category", out var cp) ? cp.GetString() ?? "" : "";
            var name = el.TryGetProperty("Name", out var np) ? np.GetString() ?? "" : "";
            return MapCSharpType(new ApiValueType { Category = cat, Name = name }, classNames);
        }
        else if (el.ValueKind == JsonValueKind.Array)
        {
            var list = new List<string>();
            foreach (var item in el.EnumerateArray())
            {
                var cat = item.TryGetProperty("Category", out var cp) ? cp.GetString() ?? "" : "";
                var name = item.TryGetProperty("Name", out var np) ? np.GetString() ?? "" : "";
                list.Add(MapCSharpType(new ApiValueType { Category = cat, Name = name }, classNames));
            }

            if (list.Count == 0) return "void";
            if (list.Count == 1) return list[0];
            return $"({string.Join(", ", list)})";
        }

        return "void";
    }

    private static string? FormatDefaultValue(JsonElement? defElement, string pType, bool isValueType)
    {
        if (!defElement.HasValue) return null;
        var el = defElement.Value;

        switch (el.ValueKind)
        {
            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                return isValueType ? " = default" : " = null";
            case JsonValueKind.True:
                return " = true";
            case JsonValueKind.False:
                return " = false";
            case JsonValueKind.Number:
                return $" = {el.GetRawText()}";
            case JsonValueKind.String:
                var s = el.GetString() ?? "";
                if (s == "null" || s == "nil" || s.Length == 0)
                {
                    return isValueType ? " = default" : " = null";
                }
                if (pType == "string")
                {
                    return $" = \"{s}\"";
                }
                if (pType == "bool")
                {
                    return $" = {s.ToLowerInvariant()}";
                }
                if (pType == "int" || pType == "double" || pType == "float")
                {
                    return $" = {s}";
                }
                return " = default";
            default:
                return null;
        }
    }

    public static string Generate(ApiDump dump)
    {
        var sb = new StringBuilder(1024 * 1024);

        sb.Append("// 🤖 АВТОМАТИЧЕСКИ СГЕНЕРИРОВАННЫЕ БАЙНДИНГИ C# ИЗ ROBLOX API-DUMP (ROSLYN INCREMENTAL GENERATOR)\n");
        sb.Append("// ⚠️ НЕ РЕДАКТИРУЙТЕ ВРУЧНУЮ: Генерируется во время компиляции через RobloxAPI.Generator\n\n");
        sb.Append("#pragma warning disable CS0108 // Member hides inherited member\n");
        sb.Append("#pragma warning disable CS0626 // External method warning\n");
        sb.Append("#pragma warning disable CS8618 // Non-nullable uninitialized\n");
        sb.Append("#pragma warning disable CS8625 // Null literal conversion\n");
        sb.Append("using System;\nusing System.Collections.Generic;\nusing CSharpLua;\n\nnamespace Roblox\n{\n");

        sb.Append(@"    /// @CSharpLua.Ignore
    [External]
    public class RBXScriptConnection
    {
        /// @CSharpLua.Template = ""{this}:Disconnect()""
        [Template(""{this}:Disconnect()"")]
        public extern void Disconnect();
    }

    /// @CSharpLua.Ignore
    [External]
    public class RBXScriptSignal
    {
        /// @CSharpLua.Template = ""{this}:Connect({0})""
        [Template(""{this}:Connect({0})"")]
        public extern RBXScriptConnection Connect(Action<Instance> callback);

        /// @CSharpLua.Template = ""{this}:Connect({0})""
        [Template(""{this}:Connect({0})"")]
        public extern RBXScriptConnection Connect(Action callback);

        /// @CSharpLua.Template = ""{this}:Wait()""
        [Template(""{this}:Wait()"")]
        public extern Instance Wait();
    }

    /// @CSharpLua.Ignore
    [External]
    public class Vector3
    {
        public double X, Y, Z;
        /// @CSharpLua.Template = ""Vector3.new({0}, {1}, {2})""
        [Template(""Vector3.new({0}, {1}, {2})"")]
        public static extern Vector3 New(double x, double y, double z);
    }

    /// @CSharpLua.Ignore
    [External]
    public class CFrame
    {
        /// @CSharpLua.Template = ""CFrame.new({0}, {1}, {2})""
        [Template(""CFrame.new({0}, {1}, {2})"")]
        public static extern CFrame New(double x, double y, double z);
    }

    /// @CSharpLua.Ignore
    [External]
    public class Color3
    {
        public double R, G, B;
        /// @CSharpLua.Template = ""Color3.fromRGB({0}, {1}, {2})""
        [Template(""Color3.fromRGB({0}, {1}, {2})"")]
        public static extern Color3 FromRGB(int r, int g, int b);
        /// @CSharpLua.Template = ""Color3.new({0}, {1}, {2})""
        [Template(""Color3.new({0}, {1}, {2})"")]
        public static extern Color3 New(double r, double g, double b);
    }

    /// @CSharpLua.Ignore
    [External]
    public class UDim
    {
        public double Scale;
        public int Offset;
        /// @CSharpLua.Template = ""UDim.new({0}, {1})""
        [Template(""UDim.new({0}, {1})"")]
        public static extern UDim New(double scale, int offset);
    }

    /// @CSharpLua.Ignore
    [External]
    public class UDim2
    {
        public UDim X, Y;
        /// @CSharpLua.Template = ""UDim2.new({0}, {1}, {2}, {3})""
        [Template(""UDim2.new({0}, {1}, {2}, {3})"")]
        public static extern UDim2 New(double xScale, int xOffset, double yScale, int yOffset);
    }

    /// @CSharpLua.Ignore
    [External]
    public class NumberRange
    {
        public double Min, Max;
        /// @CSharpLua.Template = ""NumberRange.new({0}, {1})""
        [Template(""NumberRange.new({0}, {1})"")]
        public static extern NumberRange New(double min, double max);
    }

    /// @CSharpLua.Ignore
    [External]
    public static class Game
    {
        /// @CSharpLua.Template = ""game:GetService({0})""
        [Template(""game:GetService({0})"")]
        public static extern object GetService(string serviceName);

        /// @CSharpLua.Get = ""workspace""
        [Get(""workspace"")]
        public static extern Instance Workspace { get; }

        /// @CSharpLua.Get = ""game:GetService('Players')""
        [Get(""game:GetService('Players')"")]
        public static extern Players Players { get; }

        /// @CSharpLua.Get = ""game:GetService('Lighting')""
        [Get(""game:GetService('Lighting')"")]
        public static extern Lighting Lighting { get; }

        /// @CSharpLua.Get = ""game:GetService('ReplicatedStorage')""
        [Get(""game:GetService('ReplicatedStorage')"")]
        public static extern ReplicatedStorage ReplicatedStorage { get; }

        /// @CSharpLua.Get = ""game:GetService('ServerScriptService')""
        [Get(""game:GetService('ServerScriptService')"")]
        public static extern ServerScriptService ServerScriptService { get; }

        /// @CSharpLua.Get = ""game:GetService('StarterPlayer')""
        [Get(""game:GetService('StarterPlayer')"")]
        public static extern StarterPlayer StarterPlayer { get; }

        /// @CSharpLua.Get = ""game:GetService('SoundService')""
        [Get(""game:GetService('SoundService')"")]
        public static extern SoundService SoundService { get; }
    }

    /// @CSharpLua.Ignore
    [External]
    public static class ErrorHandler
    {
        /// @CSharpLua.Template = ""require(game:GetService('ReplicatedStorage'):WaitForChild('CoreSystem'):WaitForChild('ErrorHandler')):Install()""
        [Template(""require(game:GetService('ReplicatedStorage'):WaitForChild('CoreSystem'):WaitForChild('ErrorHandler')):Install()"")]
        public static extern void Install();

        /// @CSharpLua.Template = ""require(game:GetService('ReplicatedStorage'):WaitForChild('CoreSystem'):WaitForChild('ErrorHandler')):RemapTrace({0})""
        [Template(""require(game:GetService('ReplicatedStorage'):WaitForChild('CoreSystem'):WaitForChild('ErrorHandler')):RemapTrace({0})"")]
        public static extern string RemapTrace(string originalTrace);
    }

");

        var classNames = new HashSet<string>(dump.Classes.Select(c => c.Name), StringComparer.Ordinal);

        // 1. Enums
        var seenEnums = new HashSet<string>(StringComparer.Ordinal);
        foreach (var e in dump.Enums)
        {
            if (e.Items.Count == 0) continue;
            if (!seenEnums.Add(e.Name)) continue;

            var enumNameRaw = e.Name;
            if (classNames.Contains(e.Name))
            {
                enumNameRaw += "Enum";
            }
            var enumName = SanitizeIdent(enumNameRaw);
            sb.Append($"    public enum {enumName}\n    {{\n");

            var seenItems = new HashSet<string>(StringComparer.Ordinal);
            foreach (var item in e.Items)
            {
                var itemName = SanitizeIdent(item.Name);
                if (seenItems.Add(itemName))
                {
                    sb.Append($"        {itemName} = {item.Value},\n");
                }
            }
            sb.Append("    }\n\n");
        }

        // 2. Classes
        var seenClasses = new HashSet<string>(StringComparer.Ordinal);
        foreach (var c in dump.Classes)
        {
            if (!seenClasses.Add(c.Name)) continue;

            var className = SanitizeIdent(c.Name);
            var superclassSanitized = SanitizeIdent(c.Superclass);
            var baseClassStr = "";
            if (!string.IsNullOrEmpty(superclassSanitized) && superclassSanitized != "ROOT" && superclassSanitized != "Unknown")
            {
                baseClassStr = $" : {superclassSanitized}";
            }

            sb.Append("    /// @CSharpLua.Ignore\n");
            sb.Append("    [External]\n");
            sb.Append($"    public partial class {className}{baseClassStr}\n    {{\n");

            if (className == "Instance")
            {
                sb.Append(@"        /// @CSharpLua.Template = ""Instance.new({0})""
        [Template(""Instance.new({0})"")]
        public static extern Instance New(string className);

        /// @CSharpLua.Template = ""Instance.new({0}, {1})""
        [Template(""Instance.new({0}, {1})"")]
        public static extern Instance New(string className, Instance parent);

        /// @CSharpLua.Template = ""Instance.new({0})""
        [Template(""Instance.new({0})"")]
        public static extern T New<T>(string className) where T : Instance;
");
            }

            var seenMembers = new HashSet<string>(StringComparer.Ordinal);

            foreach (var m in c.Members)
            {
                if (!seenMembers.Add(m.Name)) continue;

                bool isDeprecated = m.Tags?.Contains("Deprecated") ?? false;
                var obsoleteAttr = isDeprecated ? "        [Obsolete]\n" : "";

                switch (m.MemberType)
                {
                    case "Property":
                        if (m.ValueType != null)
                        {
                            var csType = MapCSharpType(m.ValueType, classNames);
                            var propName = SanitizeIdent(m.Name);
                            if (propName == className)
                            {
                                propName = $"{propName}Property";
                            }
                            sb.Append($"{obsoleteAttr}        public {csType} {propName};\n");
                        }
                        break;

                    case "Function":
                        var retType = GetReturnType(m.ReturnType, classNames);
                        var paramDecls = new List<string>();
                        var templateArgs = new List<string>();
                        var argIdx = 0;

                        if (m.Parameters != null && m.Parameters.Count > 0)
                        {
                            int n = m.Parameters.Count;
                            var hasDefault = new bool[n];
                            var defaultVals = new string[n];

                            for (int i = 0; i < n; i++)
                            {
                                var p = m.Parameters[i];
                                var pType = MapCSharpType(p.ParamType, classNames);
                                bool isValueType = pType is "int" or "double" or "float" or "bool" or "Vector3" or "CFrame" or "Color3" or "UDim" or "UDim2" or "NumberRange" || pType.EndsWith("Enum");

                                var def = FormatDefaultValue(p.Default, pType, isValueType);
                                if (def != null)
                                {
                                    hasDefault[i] = true;
                                    defaultVals[i] = def;
                                }
                            }

                            int validDefaultStart = n;
                            for (int i = n - 1; i >= 0; i--)
                            {
                                if (hasDefault[i])
                                {
                                    validDefaultStart = i;
                                }
                                else
                                {
                                    break;
                                }
                            }

                            for (int i = 0; i < n; i++)
                            {
                                var p = m.Parameters[i];
                                var pType = MapCSharpType(p.ParamType, classNames);
                                var pName = SanitizeIdent(p.Name);
                                var defaultStr = i >= validDefaultStart ? defaultVals[i] : "";

                                paramDecls.Add($"{pType} {pName}{defaultStr}");
                                templateArgs.Add($"{{{argIdx}}}");
                                argIdx++;
                            }
                        }

                        var methodName = SanitizeIdent(m.Name);
                        if (methodName != className)
                        {
                            var argsStr = string.Join(", ", templateArgs);
                            sb.Append(obsoleteAttr);
                            sb.Append($"        /// @CSharpLua.Template = \"{{this}}:{m.Name}({argsStr})\"\n");
                            sb.Append($"        [Template(\"{{this}}:{m.Name}({argsStr})\")]\n");
                            sb.Append($"        public extern {retType} {methodName}({string.Join(", ", paramDecls)});\n");
                        }
                        break;

                    case "Event":
                        var eventName = SanitizeIdent(m.Name);
                        sb.Append(obsoleteAttr);
                        sb.Append($"        public RBXScriptSignal {eventName};\n");
                        break;
                }
            }

            sb.Append("    }\n\n");
        }

        sb.Append("}\n");
        return sb.ToString();
    }
}
