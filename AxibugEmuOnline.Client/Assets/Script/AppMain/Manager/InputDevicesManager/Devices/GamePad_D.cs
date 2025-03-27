using System.Collections.Generic;

namespace AxibugEmuOnline.Client.InputDevices
{
    /// <summary>
    /// 通用游戏控制器
    /// </summary>
    public class GamePad_D : InputDevice_D
    {
        public Button_C Up { get; private set; }
        public Button_C Down { get; private set; }
        public Button_C Left { get; private set; }
        public Button_C Right { get; private set; }
        public Button_C Option { get; private set; }
        public Button_C Start { get; private set; }
        public Button_C North { get; private set; }
        public Button_C South { get; private set; }
        public Button_C West { get; private set; }
        public Button_C East { get; private set; }
        public Button_C LeftShoulder { get; private set; }
        public Button_C RightShoulder { get; private set; }
        public Button_C LeftTrigger { get; private set; }
        public Button_C RightTrigger { get; private set; }

        public GamePad_D(InputResolver resolver) : base(resolver) { }

        protected override List<InputControl_D> DefineControls()
        {
            throw new System.NotImplementedException();
        }


    }
}