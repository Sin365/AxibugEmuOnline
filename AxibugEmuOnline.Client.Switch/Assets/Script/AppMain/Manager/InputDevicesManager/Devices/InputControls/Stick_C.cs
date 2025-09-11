using UnityEngine;

namespace AxibugEmuOnline.Client.InputDevices
{
    /// <summary>
    /// 摇杆类型的输入控件,支持的返回值为Vector2
    /// </summary>
    public class Stick_C : InputControl_C
    {
        public VirtualButton Up;
        public VirtualButton Down;
        public VirtualButton Left;
        public VirtualButton Right;

        public Stick_C(InputDevice_D device, string controlName) : base(device, controlName) { }

        protected override void OnUpdate()
        {
            var axis = GetVector2();

            var dir = GetDirection(axis, 0.15f);
            Up.m_performing = dir == Direction.Up;
            Up.Update();

            Down.m_performing = dir == Direction.Down;
            Down.Update();

            Left.m_performing = dir == Direction.Left;
            Left.Update();

            Right.m_performing = dir == Direction.Right;
            Right.Update();
        }

        

        public class VirtualButton : InputControl_C
        {
            internal bool m_performing;

            public VirtualButton(InputDevice_D device, string controlName) : base(device, controlName) { }

            public override bool Performing
            {
                get => m_performing;
            }

            public override Vector2 GetVector2()
            {
                return default;
            }

            public override float GetFlaot()
            {
                return Performing ? 1 : 0;
            }
        }

        enum Direction
        {
            None,
            Up,
            Down,
            Left,
            Right
        }

        static Direction GetDirection(Vector2 input, float deadzone)
        {
            // 检查死区：如果点在死区半径内，返回无
            if (input.magnitude <= deadzone)
            {
                return Direction.None;
            }

            // 标准化向量（确保在单位圆上）
            Vector2 normalized = input.normalized;

            // 计算点与四个方向基准向量的点积
            float dotUp = Vector2.Dot(normalized, Vector2.up);        // (0, 1)
            float dotDown = Vector2.Dot(normalized, Vector2.down);    // (0, -1)
            float dotRight = Vector2.Dot(normalized, Vector2.right);  // (1, 0)
            float dotLeft = Vector2.Dot(normalized, Vector2.left);    // (-1, 0)

            // 找出最大点积对应的方向
            Direction bestDirection = Direction.None;
            float maxDot = -1f;  // 初始化为最小值

            // 检查上方向
            if (dotUp > maxDot)
            {
                maxDot = dotUp;
                bestDirection = Direction.Up;
            }

            // 检查下方向
            if (dotDown > maxDot)
            {
                maxDot = dotDown;
                bestDirection = Direction.Down;
            }

            // 检查右方向
            if (dotRight > maxDot)
            {
                maxDot = dotRight;
                bestDirection = Direction.Right;
            }

            // 检查左方向
            if (dotLeft > maxDot)
            {
                bestDirection = Direction.Left;
            }

            return bestDirection;
        }

    }
}
