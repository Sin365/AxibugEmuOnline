using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Event;
using AxibugEmuOnline.Client.Settings;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AxibugEmuOnline.Client.GBA.Unity
{
    public class GBAInputProvider : MonoBehaviour
    {
        public GBAControllerMapper ControllerMapper { get; private set; } = new GBAControllerMapper();

        List<GBAKeyCode> temp = new List<GBAKeyCode>();
        private GBAKeyCode[] mCurrKey;
        public static ushort[] CheckList = new ushort[]
            {
                    (ushort)GBAKeyCode.Start,
                    (ushort)GBAKeyCode.Select,
                    (ushort)GBAKeyCode.Left,
                    (ushort)GBAKeyCode.Right,
                    (ushort)GBAKeyCode.Up,
                    (ushort)GBAKeyCode.Down,
                    (ushort)GBAKeyCode.A,
                    (ushort)GBAKeyCode.B,
                    (ushort)GBAKeyCode.L,
                    (ushort)GBAKeyCode.R,
            };
        public ulong CurrLocalInpuAllData { get; private set; }

        internal void SetCurrKeyArr(GBAKeyCode inputData)
        {
            temp.Clear();
            for (int i = 0; i < CheckList.Length; i++)
            {
                GBAKeyCode key = (GBAKeyCode)CheckList[i];
                bool press = (inputData & key) > 0;
                switch (key)
                {
                    case GBAKeyCode.Start: GBAEmulator.instance.gba.Keypad.Start = press; break;
                    case GBAKeyCode.Select: GBAEmulator.instance.gba.Keypad.Select = press; break;
                    case GBAKeyCode.Left: GBAEmulator.instance.gba.Keypad.Left = press; break;
                    case GBAKeyCode.Right: GBAEmulator.instance.gba.Keypad.Right = press; break;
                    case GBAKeyCode.Up: GBAEmulator.instance.gba.Keypad.Up = press; break;
                    case GBAKeyCode.Down: GBAEmulator.instance.gba.Keypad.Down = press; break;
                    case GBAKeyCode.A: GBAEmulator.instance.gba.Keypad.A = press; break;
                    case GBAKeyCode.B: GBAEmulator.instance.gba.Keypad.B = press; break;
                    case GBAKeyCode.L: GBAEmulator.instance.gba.Keypad.L = press; break;
                    case GBAKeyCode.R: GBAEmulator.instance.gba.Keypad.R = press; break;
                }
                if (press)
                    temp.Add(key);
            }
            mCurrKey = temp.ToArray();
        }

        public ulong DoLocalPressedKeys()
        {
            ulong tempLocalInputAllData = 0;
            tempLocalInputAllData |= ControllerMapper.Controller0.GetSingleAllInput();
            tempLocalInputAllData |= ControllerMapper.Controller1.GetSingleAllInput();
            tempLocalInputAllData |= ControllerMapper.Controller2.GetSingleAllInput();
            tempLocalInputAllData |= ControllerMapper.Controller3.GetSingleAllInput();

#if UNITY_EDITOR
            if (CurrLocalInpuAllData != tempLocalInputAllData)
            {
                string ShowKeyNames = string.Empty;
            }
#endif

            CurrLocalInpuAllData = tempLocalInputAllData;

            CheckPlayerSlotChanged();

            return CurrLocalInpuAllData;
        }


        void CheckPlayerSlotChanged()
        {
            if (!ControllerMapper.Controller0.ConnectSlot.HasValue && ControllerMapper.Controller0.AnyButtonDown())
                Eventer.Instance.PostEvent(EEvent.OnLocalJoyDesireInvert, 0);

            if (!ControllerMapper.Controller1.ConnectSlot.HasValue && ControllerMapper.Controller1.AnyButtonDown())
                Eventer.Instance.PostEvent(EEvent.OnLocalJoyDesireInvert, 1);

            if (!ControllerMapper.Controller2.ConnectSlot.HasValue && ControllerMapper.Controller2.AnyButtonDown())
                Eventer.Instance.PostEvent(EEvent.OnLocalJoyDesireInvert, 2);

            if (!ControllerMapper.Controller3.ConnectSlot.HasValue && ControllerMapper.Controller3.AnyButtonDown())
                Eventer.Instance.PostEvent(EEvent.OnLocalJoyDesireInvert, 3);
        }
    }

    public class GBAControllerMapper : IControllerSetuper
    {
        public GBASingleConoller Controller0 = new GBASingleConoller(0);
        public GBASingleConoller Controller1 = new GBASingleConoller(1);
        public GBASingleConoller Controller2 = new GBASingleConoller(2);
        public GBASingleConoller Controller3 = new GBASingleConoller(3);

        ulong mCurrAllInput;

        public void SetConnect(uint? con0ToSlot = null,
                uint? con1ToSlot = null,
                uint? con2ToSlot = null,
                uint? con3ToSlot = null)
        {
            Controller0.ConnectSlot = con0ToSlot;
            Controller1.ConnectSlot = con1ToSlot;
            Controller2.ConnectSlot = con2ToSlot;
            Controller3.ConnectSlot = con3ToSlot;
        }
        public int? GetSlotConnectingControllerIndex(int slotIndex)
        {
            if (Controller0.ConnectSlot.HasValue && Controller0.ConnectSlot.Value == slotIndex) return 0;
            else if (Controller1.ConnectSlot.HasValue && Controller1.ConnectSlot.Value == slotIndex) return 1;
            else if (Controller2.ConnectSlot.HasValue && Controller2.ConnectSlot.Value == slotIndex) return 2;
            else if (Controller3.ConnectSlot.HasValue && Controller3.ConnectSlot.Value == slotIndex) return 3;
            else return null;
        }
        public IController GetSlotConnectingController(int slotIndex)
        {
            if (Controller0.ConnectSlot.HasValue && Controller0.ConnectSlot.Value == slotIndex) return Controller0;
            else if (Controller1.ConnectSlot.HasValue && Controller1.ConnectSlot.Value == slotIndex) return Controller1;
            else if (Controller2.ConnectSlot.HasValue && Controller2.ConnectSlot.Value == slotIndex) return Controller2;
            else if (Controller3.ConnectSlot.HasValue && Controller3.ConnectSlot.Value == slotIndex) return Controller3;
            else return null;
        }
        static HashSet<uint> s_temp = new HashSet<uint>();
        public uint? GetFreeSlotIndex()
        {
            s_temp.Clear();
            s_temp.Add(0);
            s_temp.Add(1);
            s_temp.Add(2);
            s_temp.Add(3);

            if (Controller0.ConnectSlot.HasValue) s_temp.Remove(Controller0.ConnectSlot.Value);
            if (Controller1.ConnectSlot.HasValue) s_temp.Remove(Controller1.ConnectSlot.Value);
            if (Controller2.ConnectSlot.HasValue) s_temp.Remove(Controller2.ConnectSlot.Value);
            if (Controller3.ConnectSlot.HasValue) s_temp.Remove(Controller3.ConnectSlot.Value);

            if (s_temp.Count > 0) return s_temp.First();
            else return null;
        }
        public void LetControllerConnect(int conIndex, uint slotIndex)
        {
            GBASingleConoller targetController;
            switch (conIndex)
            {
                case 0: targetController = Controller0; break;
                case 1: targetController = Controller1; break;
                case 2: targetController = Controller2; break;
                case 3: targetController = Controller3; break;
                default:
                    throw new System.Exception($"Not Allowed conIndex Range: {conIndex}");
            }
            if (targetController.ConnectSlot.HasValue) return;

            targetController.ConnectSlot = slotIndex;
            Eventer.Instance.PostEvent(EEvent.OnControllerConnectChanged);
        }

    }

    public class GBASingleConoller : IController
    {
        public ulong tg_SELECT,
            tg_GAMESTART,
        tg_UP, tg_DOWN, tg_LEFT, tg_RIGHT,
        tg_BTN_A, tg_BTN_B, tg_BTN_L, tg_BTN_R;

        public ulong CurrLocalSingleAllInput { get; private set; }

        private GBAKeyBinding m_keyMapper;
        int mControllerIndex;
        uint? mConnectSlot;

        /// <summary>
        /// 指示该手柄连接的手柄插槽
        /// <para><c>这个值代表了该手柄在实际游戏中控制的Player</c></para>
        /// <value>[0,3] 例外:为空代表未连接</value>
        /// </summary>
        public uint? ConnectSlot
        {
            get { return mConnectSlot; }
            set { mConnectSlot = value; this.ResetTargetMotionKey(); }
        }

        /// <summary>
        /// 控制器编号
        /// <para><c>此编号并非对应游戏中的player1,player2,player3,player4,仅仅作为本地4个手柄的实例</c></para>
        /// <value>[0,3]</value>
        /// </summary>
        public int ControllerIndex
        {
            get { return mControllerIndex; }
            set
            {
                mControllerIndex = value;
                //this.LoadControlKeyForConfig();
                //走统一配置
            }
        }
        public GBASingleConoller(int controllerIndex)
        {
            ControllerIndex = controllerIndex;
            m_keyMapper = App.settings.KeyMapper.GetBinder<GBAKeyBinding>(AxibugProtobuf.RomPlatformType.GameBoyAdvance);
        }

        public bool AnyButtonDown()
        {
            return m_keyMapper.AnyKeyDown(ControllerIndex);
        }
        public ulong GetSingleAllInput()
        {
            if (!ConnectSlot.HasValue)
                return 0;
            CurrLocalSingleAllInput = 0;

            if (m_keyMapper.GetKey(GBAKeyCode.Select, ControllerIndex)) CurrLocalSingleAllInput |= tg_SELECT;
            if (m_keyMapper.GetKey(GBAKeyCode.Start, ControllerIndex)) CurrLocalSingleAllInput |= tg_GAMESTART;
            if (m_keyMapper.GetKey(GBAKeyCode.Up, ControllerIndex)) CurrLocalSingleAllInput |= tg_UP;
            if (m_keyMapper.GetKey(GBAKeyCode.Down, ControllerIndex)) CurrLocalSingleAllInput |= tg_DOWN;
            if (m_keyMapper.GetKey(GBAKeyCode.Left, ControllerIndex)) CurrLocalSingleAllInput |= tg_LEFT;
            if (m_keyMapper.GetKey(GBAKeyCode.Right, ControllerIndex)) CurrLocalSingleAllInput |= tg_RIGHT;
            if (m_keyMapper.GetKey(GBAKeyCode.A, ControllerIndex)) CurrLocalSingleAllInput |= tg_BTN_A;
            if (m_keyMapper.GetKey(GBAKeyCode.B, ControllerIndex)) CurrLocalSingleAllInput |= tg_BTN_B;
            if (m_keyMapper.GetKey(GBAKeyCode.L, ControllerIndex)) CurrLocalSingleAllInput |= tg_BTN_L;
            if (m_keyMapper.GetKey(GBAKeyCode.R, ControllerIndex)) CurrLocalSingleAllInput |= tg_BTN_R;
            return CurrLocalSingleAllInput;
        }

    }

    public static class GBASingleControllSetter
    {
        public static void ResetTargetMotionKey(this GBASingleConoller singlecontrol)
        {
            if (!singlecontrol.ConnectSlot.HasValue)
            {
                singlecontrol.tg_SELECT
                = singlecontrol.tg_GAMESTART
                = singlecontrol.tg_UP
                = singlecontrol.tg_DOWN
                = singlecontrol.tg_LEFT
                = singlecontrol.tg_RIGHT
                = singlecontrol.tg_BTN_A
                = singlecontrol.tg_BTN_B
                = singlecontrol.tg_BTN_L
                = singlecontrol.tg_BTN_R
                = 10;
                return;
            }
            switch (singlecontrol.ConnectSlot.Value)
            {
                case 0:
                    singlecontrol.tg_SELECT = (ushort)GBAKeyCode.Select;
                    singlecontrol.tg_GAMESTART = (ushort)GBAKeyCode.Start;
                    singlecontrol.tg_UP = (ushort)GBAKeyCode.Up;
                    singlecontrol.tg_DOWN = (ushort)GBAKeyCode.Down;
                    singlecontrol.tg_LEFT = (ushort)GBAKeyCode.Left;
                    singlecontrol.tg_RIGHT = (ushort)GBAKeyCode.Right;
                    singlecontrol.tg_BTN_A = (ushort)GBAKeyCode.A;
                    singlecontrol.tg_BTN_B = (ushort)GBAKeyCode.B;
                    singlecontrol.tg_BTN_L = (ushort)GBAKeyCode.L;
                    singlecontrol.tg_BTN_R = (ushort)GBAKeyCode.R;
                    break;
                    //仅P1吧
            }
        }
    }
}
