namespace AxibugEmuOnline.Client.InputDevices
{
    /// <summary>
    /// 按键类型的输入控件
    /// </summary>
    public class Button : InputControl
    {
        string m_controlName;

        public Button(InputDevice device, string controlName) : base(device)
        {
            m_controlName = controlName;
        }

        public override string ControlName => m_controlName;
    }
}
