namespace RobloxCSharp.Entities
{
    using System;
    using Roblox;
    using RobloxCSharp.Framework;

    // Класс Героя-Прыгуна (SuperJumperHero)
    public class SuperJumperHero : Hero
    {
        public SuperJumpAction JumpAction { get; private set; }

        public SuperJumperHero(Player player, Model character, Humanoid humanoid)
            : base(player, character, humanoid)
        {
            this.JumpAction = new SuperJumpAction(this);
        }

        public override void Initialize()
        {
            base.Initialize();

            // Обновляем бейдж над головой
            UpdateOverheadText();

            // Накладываем пассивный модификатор ускорения на 10 секунд
            ModifierBase.Apply<SpeedBoostModifier>(this, this, 10.0);

            // Выполняем прыжок при спавне
            PerformMainAction();
        }

        public override void PerformMainAction()
        {
            if (this.JumpAction != null)
            {
                this.JumpAction.Execute(this, null);
            }
        }

        private void UpdateOverheadText()
        {
            var head = (BasePart)this.Model.FindFirstChild("Head");
            if (head == null) return;

            var billboard = head.FindFirstChild("OverheadDisplay");
            if (billboard != null)
            {
                var label = (TextLabel)billboard.FindFirstChild("DisplayLabel");
                if (label != null)
                {
                    label.Text = this.Name + " [⚡ Супер Прыгун]";
                    label.TextColor3 = Color3.FromRGB(255, 215, 0);
                }
            }
        }
    }
}
