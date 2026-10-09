namespace RobloxCSharp.Entities
{
    using System;
    using Roblox;

    // Класс Героя-Прыгуна: расширяет базового Героя и добавляет способность "Супер Прыжок Вверх"
    public class SuperJumperHero : Hero
    {
        // Сила прыжка
        public double SuperJumpForce = 120;

        // Конструктор
        public SuperJumperHero(Player player, Model character, Humanoid humanoid)
            : base(player, character, humanoid)
        {
        }

        // Переопределяем инициализацию героя
        public override void Initialize()
        {
            base.Initialize();

            // 1. Увеличиваем стандартную высоту прыжка персонажа
            if (this.Humanoid != null)
            {
                this.Humanoid.UseJumpPower = true;
                this.Humanoid.JumpPower = this.SuperJumpForce;
            }

            // 2. Добавляем золотистое свечение вокруг героя
            CreateJumpAura();

            // 3. Обновляем надпись над головой
            UpdateOverheadText();

            Console.WriteLine("[Супер Прыгун] Герой " + this.Name + " готов к супер прыжкам!");

            // 4. Сразу выполняем демонстрационный супер прыжок вверх при появлении
            SuperJump();
        }

        // Логика способности "Супер Прыжок Вверх"
        public void SuperJump()
        {
            var rootPart = (BasePart)this.Character.FindFirstChild("HumanoidRootPart");
            if (rootPart == null || this.Humanoid == null) return;

            Console.WriteLine("🚀 [Способность] Герой " + this.Name + " выполняет СУПЕР ПРЫЖОК ВВЕРХ!");

            // 1. Даем физический импульс вверх персонажу через скорость
            rootPart.AssemblyLinearVelocity = Vector3.New(0, this.SuperJumpForce, 0);

            // 2. Активируем состояние прыжка у Humanoid
            this.Humanoid.Jump = true;

            // 3. Создаем яркую световую вспышку при прыжке
            CreateJumpFlashEffect(rootPart);
        }

        // Переопределяем метод базового класса
        public override void UseAbility()
        {
            SuperJump();
        }

        // Создание эффекта золотистой ауры вокруг персонажа
        private void CreateJumpAura()
        {
            var oldHighlight = this.Character.FindFirstChild("JumpAura");
            if (oldHighlight != null) oldHighlight.Destroy();

            var aura = Instance.New<Highlight>("Highlight");
            aura.Name = "JumpAura";
            aura.FillColor = Color3.FromRGB(255, 215, 0); // Золотой цвет
            aura.OutlineColor = Color3.FromRGB(255, 255, 255);
            aura.FillTransparency = 0.5;
            aura.OutlineTransparency = 0.1;
            aura.Parent = this.Character;
        }

        // Создание световой вспышки под ногами при супер прыжке
        private void CreateJumpFlashEffect(BasePart rootPart)
        {
            var light = Instance.New<PointLight>("PointLight");
            light.Name = "JumpFlash";
            light.Color = Color3.FromRGB(255, 230, 100);
            light.Brightness = 5;
            light.Range = 20;
            light.Parent = rootPart;
        }

        // Обновление бейджа над головой с новым именем класса
        private void UpdateOverheadText()
        {
            var head = (BasePart)this.Character.FindFirstChild("Head");
            if (head == null) return;

            var billboard = head.FindFirstChild("HeroOverhead");
            if (billboard != null)
            {
                var label = (TextLabel)billboard.FindFirstChild("TextLabel");
                if (label != null)
                {
                    label.Text = this.Name + " [⚡ Супер Прыгун]";
                    label.TextColor3 = Color3.FromRGB(255, 215, 0);
                }
            }
        }
    }
}
