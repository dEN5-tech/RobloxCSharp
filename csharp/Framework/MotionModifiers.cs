namespace RobloxCSharp.Framework
{
    using Roblox;

    // Контроллер горизонтального движения (Рывок / Чардж / Отталкивание / Dash)
    public class BaseModifierMotionHorizontal : BaseModifier
    {
        public override MotionType MotionType => MotionType.Horizontal;
        public Vector3 Direction = Vector3.New(0, 0, 0);
        public double Speed = 60;

        public virtual void UpdateMotion(double deltaTime)
        {
            if (this.Target == null || this.Target.RootPart == null) return;

            // Задаем горизонтальную скорость, сохраняя текущую вертикальную физику (гравитацию)
            this.Target.RootPart.AssemblyLinearVelocity = Vector3.New(
                this.Direction.X * this.Speed,
                this.Target.RootPart.AssemblyLinearVelocity.Y,
                this.Direction.Z * this.Speed
            );
        }
    }

    // Контроллер вертикального движения (Супер Прыжок / Подбрасывание в воздух / Слэм)
    public class BaseModifierMotionVertical : BaseModifier
    {
        public override MotionType MotionType => MotionType.Vertical;
        public double VerticalForce = 120;

        public override void OnCreated(object customData)
        {
            base.OnCreated(customData);
            if (this.Target != null && this.Target.RootPart != null)
            {
                // Мгновенный импульс вверх
                this.Target.RootPart.AssemblyLinearVelocity = Vector3.New(
                    this.Target.RootPart.AssemblyLinearVelocity.X,
                    this.VerticalForce,
                    this.Target.RootPart.AssemblyLinearVelocity.Z
                );

                if (this.Target.Humanoid != null)
                {
                    this.Target.Humanoid.Jump = true;
                }
            }
        }
    }

    // Контроллер движения по 3D-траектории (Параболический прыжок / Leap / Pounce)
    public class BaseModifierMotionBoth : BaseModifier
    {
        public override MotionType MotionType => MotionType.Both;
        public Vector3 TargetPosition;
        public double LeapHeight = 35;
        public double HorizontalSpeed = 50;

        public virtual void UpdateMotion(double progress)
        {
            if (this.Target == null || this.Target.RootPart == null) return;
            // Обновление параболической траектории движения в пространстве
        }
    }
}
