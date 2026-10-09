namespace RobloxCSharp.Entities
{
    using System;
    using Roblox;
    using RobloxCSharp.Framework;

    // Модификатор супер-прыжка (вертикальный контроллер движения MotionVerticalBase)
    [RegisterModifierBase("SuperJumpModifier")]
    public class SuperJumpModifier : MotionVerticalBase
    {
        private Highlight aura;
        private PointLight flash;

        public override void OnCreated(object customData)
        {
            this.VerticalVelocity = 120; // Сила прыжка вверх
            this.Duration = 0.5; // Длительность импульса

            if (this.Target != null && this.Target.Model != null)
            {
                // Золотистая аура на время прыжка
                this.aura = Instance.New<Highlight>("Highlight");
                this.aura.Name = "SuperJumpAura";
                this.aura.FillColor = Color3.FromRGB(255, 215, 0);
                this.aura.OutlineColor = Color3.FromRGB(255, 255, 255);
                this.aura.FillTransparency = 0.4;
                this.aura.Parent = this.Target.Model;

                // Световая вспышка под персонажем
                if (this.Target.RootPart != null)
                {
                    this.flash = Instance.New<PointLight>("PointLight");
                    this.flash.Color = Color3.FromRGB(255, 230, 100);
                    this.flash.Brightness = 5;
                    this.flash.Range = 20;
                    this.flash.Parent = this.Target.RootPart;
                }

                // Переводим Humanoid в состояние прыжка
                if (this.Target.Humanoid != null)
                {
                    this.Target.Humanoid.Jump = true;
                }
            }

            Console.WriteLine("🚀 [SuperJumpModifier] Наложен импульс вертикального движения на " + this.Target?.Name);
        }

        public override void OnDestroy()
        {
            if (this.aura != null) this.aura.Destroy();
            if (this.flash != null) this.flash.Destroy();
            Console.WriteLine("✨ [SuperJumpModifier] Прыжок завершен, эффекты очищены.");
        }
    }
}
