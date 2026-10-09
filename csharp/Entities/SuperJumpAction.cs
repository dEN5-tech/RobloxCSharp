namespace RobloxCSharp.Entities
{
    using System;
    using Roblox;
    using RobloxCSharp.Framework;

    // Действие супер-прыжка персонажа (ActionBase)
    [RegisterActionBase("SuperJumpAction")]
    public class SuperJumpAction : ActionBase
    {
        public SuperJumpAction(CharacterBase owner) : base(owner, "SuperJump")
        {
            this.Cooldown = 3.0; // 3 секунды перезарядки
        }

        public override void OnActionStart(CharacterBase target, Vector3 targetPosition)
        {
            Console.WriteLine("⚡ [SuperJumpAction] Персонаж " + this.Owner?.Name + " активирует супер-прыжок вверх!");

            // Накладываем модификатор движения MotionVerticalBase через статичный Apply<T>
            ModifierBase.Apply<SuperJumpModifier>(this.Owner, this.Owner, 0.5);
        }
    }
}
