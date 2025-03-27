namespace AxibugEmuOnline.Client.InputDevices
{
    /// <summary>
    /// 按键类型的输入控件
    /// </summary>
    public class Button_C : InputControl_D
    {
        string m_controlName;

        public Button_C(InputDevice_D device, string controlName) : base(device)
        {
            m_controlName = controlName;
        }

        public override string ControlName => m_controlName;
    }
}
