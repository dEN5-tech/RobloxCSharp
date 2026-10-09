namespace RobloxCSharp.Framework
{
    using System;
    using System.Collections.Generic;
    using Roblox;

    // Базовый класс игрового персонажа Roblox (CharacterBase)
    public class CharacterBase
    {
        public Player Player { get; set; }
        public Model Model { get; set; }
        public Humanoid Humanoid { get; set; }
        public BasePart RootPart { get; set; }
        public string Name { get; set; }

        public double BaseWalkSpeed { get; set; } = 16;
        public double BaseJumpPower { get; set; } = 50;
        public double MaxHealth { get; set; } = 100;
        public double Health { get; set; } = 100;

        public List<ModifierBase> ActiveModifiers { get; } = new List<ModifierBase>();

        public CharacterBase(Player player, Model model, Humanoid humanoid)
        {
            this.Player = player;
            this.Model = model;
            this.Humanoid = humanoid;
            this.RootPart = (BasePart)model.FindFirstChild("HumanoidRootPart");
            this.Name = player != null ? player.Name : model.Name;
        }

        // Инициализация персонажа в игровом пространстве
        public virtual void Initialize()
        {
            if (this.Humanoid != null)
            {
                this.Humanoid.MaxHealth = this.MaxHealth;
                this.Humanoid.Health = this.Health;
                this.Humanoid.WalkSpeed = this.BaseWalkSpeed;
                this.Humanoid.UseJumpPower = true;
                this.Humanoid.JumpPower = this.BaseJumpPower;
            }

            SetupOverheadDisplay();
            Console.WriteLine("[CharacterBase] Персонаж '" + this.Name + "' успешно инициализирован.");
        }

        // Создание плашки над головой (BillboardGui)
        protected virtual void SetupOverheadDisplay()
        {
            var head = (BasePart)this.Model.FindFirstChild("Head");
            if (head == null) return;

            var oldGui = head.FindFirstChild("OverheadDisplay");
            if (oldGui != null) oldGui.Destroy();

            var billboard = Instance.New<BillboardGui>("BillboardGui");
            billboard.Name = "OverheadDisplay";
            billboard.Adornee = head;
            billboard.Size = UDim2.New(0, 160, 0, 40);
            billboard.StudsOffset = Vector3.New(0, 2.5, 0);
            billboard.AlwaysOnTop = true;
            billboard.Parent = head;

            var label = Instance.New<TextLabel>("TextLabel");
            label.Name = "DisplayLabel";
            label.Size = UDim2.New(1, 0, 1, 0);
            label.BackgroundTransparency = 1;
            label.Text = this.Name;
            label.TextColor3 = Color3.FromRGB(255, 255, 255);
            label.TextScaled = true;
            label.Font = Font.GothamBold;
            label.Parent = billboard;
        }

        // Нанесение урона персонажу
        public virtual void TakeDamage(double damage, CharacterBase source = null)
        {
            if (this.Humanoid == null || this.Humanoid.Health <= 0) return;

            this.Humanoid.TakeDamage(damage);
            this.Health = this.Humanoid.Health;
            Console.WriteLine("💥 [CharacterBase] " + this.Name + " получил урон: " + damage + " (Осталось HP: " + this.Health + ")");
        }

        // Лечение персонажа
        public virtual void Heal(double amount)
        {
            if (this.Humanoid == null || this.Humanoid.Health <= 0) return;

            double newHealth = this.Humanoid.Health + amount;
            if (newHealth > this.Humanoid.MaxHealth) newHealth = this.Humanoid.MaxHealth;

            this.Humanoid.Health = newHealth;
            this.Health = newHealth;
            Console.WriteLine("💚 [CharacterBase] " + this.Name + " исцелен на: " + amount + " (HP: " + this.Health + ")");
        }
    }
}
