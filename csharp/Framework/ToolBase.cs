namespace RobloxCSharp.Framework
{
    using System;
    using Roblox;

    // Базовый класс для экипируемых инструментов Roblox (ToolBase)
    public abstract class ToolBase : ActionBase
    {
        public Tool InstanceTool { get; set; }
        public bool IsEquipped { get; private set; } = false;

        public ToolBase(CharacterBase owner, string name, Tool tool = null) : base(owner, name)
        {
            this.InstanceTool = tool;
            if (this.InstanceTool != null)
            {
                this.InstanceTool.Activated.Connect(() =>
                {
                    this.Execute(this.Owner, null);
                });

                this.InstanceTool.Equipped.Connect((mouse) =>
                {
                    this.IsEquipped = true;
                    this.OnEquipped();
                });

                this.InstanceTool.Unequipped.Connect(() =>
                {
                    this.IsEquipped = false;
                    this.OnUnequipped();
                });
            }
        }

        // Хук при взятии инструмента в руки
        public virtual void OnEquipped()
        {
            Console.WriteLine("🗡️ [ToolBase] Инструмент '" + this.Name + "' экипирован персонажем " + this.Owner?.Name);
        }

        // Хук при уборке инструмента
        public virtual void OnUnequipped()
        {
            Console.WriteLine("🗡️ [ToolBase] Инструмент '" + this.Name + "' убран персонажем " + this.Owner?.Name);
        }
    }
}
