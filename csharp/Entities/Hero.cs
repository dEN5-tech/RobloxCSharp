namespace RobloxCSharp.Entities
{
    using System;
    using Roblox;

    // Простой базовый класс Героя
    public class Hero
    {
        // 1. Основные свойства героя
        public Player Player;
        public Model Character;
        public Humanoid Humanoid;
        public string Name;
        public double Health = 100;
        public double MaxHealth = 100;

        // Конструктор героя
        public Hero(Player player, Model character, Humanoid humanoid)
        {
            this.Player = player;
            this.Character = character;
            this.Humanoid = humanoid;
            this.Name = player != null ? player.Name : "Hero";
        }

        // Базовая инициализация персонажа в мире Roblox
        public virtual void Initialize()
        {
            // Устанавливаем базовое здоровье и скорость
            if (this.Humanoid != null)
            {
                this.Humanoid.MaxHealth = this.MaxHealth;
                this.Humanoid.Health = this.Health;
                this.Humanoid.WalkSpeed = 16;
            }

            // Создаем простой бейдж с именем над головой
            CreateOverheadGui();

            Console.WriteLine("[Герой] Создан базовый герой: " + this.Name);
        }

        // Создание простого текста над головой героя
        protected void CreateOverheadGui()
        {
            var head = (BasePart)this.Character.FindFirstChild("Head");
            if (head == null) return;

            // Удаляем старый GUI если есть
            var oldGui = head.FindFirstChild("HeroOverhead");
            if (oldGui != null) oldGui.Destroy();

            // Создаем новый BillboardGui
            var billboard = Instance.New<BillboardGui>("BillboardGui");
            billboard.Name = "HeroOverhead";
            billboard.Adornee = head;
            billboard.Size = UDim2.New(0, 150, 0, 40);
            billboard.StudsOffset = Vector3.New(0, 2.5, 0);
            billboard.AlwaysOnTop = true;
            billboard.Parent = head;

            // Текстовая плашка с именем и классом
            var nameLabel = Instance.New<TextLabel>("TextLabel");
            nameLabel.Size = UDim2.New(1, 0, 1, 0);
            nameLabel.BackgroundTransparency = 1;
            nameLabel.Text = this.Name + " [Герой]";
            nameLabel.TextColor3 = Color3.FromRGB(255, 255, 255);
            nameLabel.TextScaled = true;
            nameLabel.Font = Font.GothamBold;
            nameLabel.Parent = billboard;
        }

        // Виртуальный метод для способности (будет переопределен в дочернем классе)
        public virtual void UseAbility()
        {
            Console.WriteLine("[Герой] Использована базовая способность героя " + this.Name);
        }
    }
}
