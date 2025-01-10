namespace AxibugEmuOnline.Client.Manager
{
    public partial class GamePadManager
    {
        /// <summary>
        /// 被Unity所识别的通用GamePad类
        /// </summary>
        public class GamePad
        {
            internal GamePadInfo m_info;
            public int Index => m_info.Index;
            public string Name => m_info.Name;
            public bool Offline { get; internal set; }

            internal GamePad(GamePadInfo info)
            {
                m_info = info;
            }

            public override string ToString()
            {
                return $"{Index}:{Name}{(Offline ? "(Offline)" : string.Empty)}";
            }
        }
    }
}