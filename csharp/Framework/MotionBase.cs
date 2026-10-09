namespace RobloxCSharp.Framework
{
    using System;
    using Roblox;

    // Базовый контроллер физического движения персонажа в Roblox (MotionBase)
    public abstract class MotionBase : ModifierBase
    {
        // Вызывается на каждом тике физики RunService.Heartbeat
        public abstract void UpdateMotion(double deltaTime);
    }

    // Горизонтальное движение (Рывок / Чардж / Отталкивание / Dash)
    public class MotionHorizontalBase : MotionBase
    {
        public override MotionType MotionType => MotionType.Horizontal;

        public Vector3 Direction { get; set; } = Vector3.New(0, 0, 0);
        public double Speed { get; set; } = 50;

        public override void UpdateMotion(double deltaTime)
        {
            if (this.Target == null || this.Target.RootPart == null) return;

            var currentVel = this.Target.RootPart.AssemblyLinearVelocity;
            // Устанавливаем горизонтальную скорость, сохраняя текущую вертикальную
            this.Target.RootPart.AssemblyLinearVelocity = Vector3.New(
                this.Direction.X * this.Speed,
                currentVel.Y,
                this.Direction.Z * this.Speed
            );
        }
    }

    // Вертикальное движение (Супер прыжок / Подбрасывание / Слэм / JumpBoost)
    public class MotionVerticalBase : MotionBase
    {
        public override MotionType MotionType => MotionType.Vertical;

        public double VerticalVelocity { get; set; } = 100;

        public override void UpdateMotion(double deltaTime)
        {
            if (this.Target == null || this.Target.RootPart == null) return;

            var currentVel = this.Target.RootPart.AssemblyLinearVelocity;
            // Задаем точную вертикальную скорость
            this.Target.RootPart.AssemblyLinearVelocity = Vector3.New(
                currentVel.X,
                this.VerticalVelocity,
                currentVel.Z
            );
        }
    }

    // Траекторное 3D движение (Парабола / Прыжок по дуге / Leap)
    public class MotionBothBase : MotionBase
    {
        public override MotionType MotionType => MotionType.Both;

        public Vector3 TargetPosition { get; set; }
        public Vector3 StartPosition { get; set; }
        public double ArcHeight { get; set; } = 25;

        public override void OnCreated(object customData)
        {
            if (this.Target != null && this.Target.RootPart != null)
            {
                this.StartPosition = this.Target.RootPart.Position;
            }
        }

        public override void UpdateMotion(double deltaTime)
        {
            if (this.Target == null || this.Target.RootPart == null || this.Duration <= 0) return;

            double progress = this.ElapsedTime / this.Duration;
            if (progress > 1.0) progress = 1.0;

            // Расчет интерполяции X/Z
            double currentX = this.StartPosition.X + (this.TargetPosition.X - this.StartPosition.X) * progress;
            double currentZ = this.StartPosition.Z + (this.TargetPosition.Z - this.StartPosition.Z) * progress;

            // Параболическая высота Y: 4 * ArcHeight * progress * (1 - progress)
            double heightOffset = 4.0 * this.ArcHeight * progress * (1.0 - progress);
            double currentY = this.StartPosition.Y + (this.TargetPosition.Y - this.StartPosition.Y) * progress + heightOffset;

            this.Target.RootPart.CFrame = CFrame.New(currentX, currentY, currentZ);
        }
    }
}
