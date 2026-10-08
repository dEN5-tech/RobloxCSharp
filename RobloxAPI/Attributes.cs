#pragma warning disable CS8618
using System;

namespace CSharpLua
{
    [AttributeUsage(AttributeTargets.All)]
    public class TemplateAttribute : Attribute
    {
        public string Template { get; set; }
        public TemplateAttribute() { }
        public TemplateAttribute(string template) { Template = template; }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Field)]
    public class GetAttribute : Attribute
    {
        public string Template { get; set; }
        public GetAttribute(string template) { Template = template; }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Field)]
    public class SetAttribute : Attribute
    {
        public string Template { get; set; }
        public SetAttribute(string template) { Template = template; }
    }

    [AttributeUsage(AttributeTargets.All)]
    public class ExternalAttribute : Attribute { }
    
    [AttributeUsage(AttributeTargets.All)]
    public class IgnoreAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyAttribute : Attribute { }
}
