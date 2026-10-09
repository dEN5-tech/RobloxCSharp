namespace RobloxCSharp.Framework
{
    using System;

    // Атрибут для регистрации модификаторов Roblox
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class RegisterModifierBaseAttribute : Attribute
    {
        public string ModifierName { get; }
        public RegisterModifierBaseAttribute(string name) => this.ModifierName = name;
    }

    // Атрибут для регистрации действий/способностей Roblox
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class RegisterActionBaseAttribute : Attribute
    {
        public string ActionName { get; }
        public RegisterActionBaseAttribute(string name) => this.ActionName = name;
    }

    // Атрибут для регистрации инструментов Roblox (Tool)
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class RegisterToolBaseAttribute : Attribute
    {
        public string ToolName { get; }
        public RegisterToolBaseAttribute(string name) => this.ToolName = name;
    }

    // Тип физического контроллера движения
    public enum MotionType
    {
        None = 0,
        Horizontal = 1,
        Vertical = 2,
        Both = 3
    }
}
