using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RobloxAPI.Generator;

public class ApiDump
{
    [JsonPropertyName("Classes")]
    public List<ApiClass> Classes { get; set; } = new();

    [JsonPropertyName("Enums")]
    public List<ApiEnum> Enums { get; set; } = new();

    [JsonPropertyName("Version")]
    public uint Version { get; set; }
}

public class ApiEnum
{
    [JsonPropertyName("Name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("Items")]
    public List<ApiEnumItem> Items { get; set; } = new();
}

public class ApiEnumItem
{
    [JsonPropertyName("Name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("Value")]
    public long Value { get; set; }
}

public class ApiClass
{
    [JsonPropertyName("Name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("Superclass")]
    public string Superclass { get; set; } = "";

    [JsonPropertyName("Members")]
    public List<ApiMember> Members { get; set; } = new();

    [JsonPropertyName("Tags")]
    public List<string>? Tags { get; set; }
}

public class ApiMember
{
    [JsonPropertyName("MemberType")]
    public string MemberType { get; set; } = "";

    [JsonPropertyName("Name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("ValueType")]
    public ApiValueType? ValueType { get; set; }

    [JsonPropertyName("ReturnType")]
    public JsonElement? ReturnType { get; set; }

    [JsonPropertyName("Parameters")]
    public List<ApiParameter>? Parameters { get; set; }

    [JsonPropertyName("Tags")]
    public List<string>? Tags { get; set; }
}

public class ApiParameter
{
    [JsonPropertyName("Name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("Type")]
    public ApiValueType? ParamType { get; set; }

    [JsonPropertyName("Default")]
    public JsonElement? Default { get; set; }
}

public class ApiValueType
{
    [JsonPropertyName("Category")]
    public string Category { get; set; } = "";

    [JsonPropertyName("Name")]
    public string Name { get; set; } = "";
}
