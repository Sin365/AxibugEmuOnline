using MAME.Core;

namespace MAME.Core
{
    public partial class Konami68000
    {
        public static void loop_inputports_konami68000_common()
        {

        }
        public static void loop_inputports_konami68000_cuebrick()
        {
            if (Keyboard.IsPressed(MotionKey.P1_INSERT_COIN))//if (Keyboard.IsPressed(Corekey.D5))
            {
                sbyte0 &= ~0x01;
            }
            else
            {
                sbyte0 |= 0x01;
            }
            if (Keyboard.IsPressed(MotionKey.P2_INSERT_COIN))//if (Keyboard.IsPressed(Corekey.D6))
            {
                sbyte0 &= ~0x02;
            }
            else
            {
                sbyte0 |= 0x02;
            }
            if (Keyboard.IsPressed(MotionKey.P1_GAMESTART))//if (Keyboard.IsPressed(Corekey.D1))
            {
                sbyte0 &= ~0x08;
            }
            else
            {
                sbyte0 |= 0x08;
            }
            if (Keyboard.IsPressed(MotionKey.P2_GAMESTART))//if (Keyboard.IsPressed(Corekey.D2))
            {
                sbyte0 &= ~0x10;
            }
            else
            {
                sbyte0 |= 0x10;
            }
            if (Keyboard.IsPressed(MotionKey.P1_RIGHT))//if (Keyboard.IsPressed(Corekey.D))
            {
                sbyte1 &= ~0x02;
            }
            else
            {
                sbyte1 |= 0x02;
            }
            if (Keyboard.IsPressed(MotionKey.P1_LEFT))//if (Keyboard.IsPressed(Corekey.A))
            {
                sbyte1 &= ~0x01;
            }
            else
            {
                sbyte1 |= 0x01;
            }
            if (Keyboard.IsPressed(MotionKey.P1_DOWN))//if (Keyboard.IsPressed(Corekey.S))
            {
                sbyte1 &= ~0x08;
            }
            else
            {
                sbyte1 |= 0x08;
            }
            if (Keyboard.IsPressed(MotionKey.P1_UP))//if (Keyboard.IsPressed(Corekey.W))
            {
                sbyte1 &= ~0x04;
            }
            else
            {
                sbyte1 |= 0x04;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_1))//if (Keyboard.IsPressed(Corekey.J))
            {
                sbyte1 &= ~0x10;
            }
            else
            {
                sbyte1 |= 0x10;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_2))//if (Keyboard.IsPressed(Corekey.K))
            {
                sbyte1 &= ~0x20;
            }
            else
            {
                sbyte1 |= 0x20;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_E))//if (Keyboard.IsPressed(Corekey.L))
            {
                sbyte1 &= ~0x40;
            }
            else
            {
                sbyte1 |= 0x40;
            }
            if (Keyboard.IsPressed(MotionKey.P2_RIGHT))//if (Keyboard.IsPressed(Corekey.Right))
            {
                sbyte2 &= ~0x02;
            }
            else
            {
                sbyte2 |= 0x02;
            }
            if (Keyboard.IsPressed(MotionKey.P2_LEFT))//if (Keyboard.IsPressed(Corekey.Left))
            {
                sbyte2 &= ~0x01;
            }
            else
            {
                sbyte2 |= 0x01;
            }
            if (Keyboard.IsPressed(MotionKey.P2_DOWN)) //if (Keyboard.IsPressed(Corekey.Down))
            {
                sbyte2 &= ~0x08;
            }
            else
            {
                sbyte2 |= 0x08;
            }
            if (Keyboard.IsPressed(MotionKey.P2_UP))//if (Keyboard.IsPressed(Corekey.Up))
            {
                sbyte2 &= ~0x04;
            }
            else
            {
                sbyte2 |= 0x04;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_1))//if (Keyboard.IsPressed(Corekey.NumPad1))
            {
                sbyte2 &= ~0x10;
            }
            else
            {
                sbyte2 |= 0x10;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_2))//if (Keyboard.IsPressed(Corekey.NumPad2))
            {
                sbyte2 &= ~0x20;
            }
            else
            {
                sbyte2 |= 0x20;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_E))//if (Keyboard.IsPressed(Corekey.NumPad3))
            {
                sbyte2 &= ~0x40;
            }
            else
            {
                sbyte2 |= 0x40;
            }
            if (Keyboard.IsPressed(MotionKey.UNKNOW_R)) //if (Keyboard.IsPressed(Corekey.R))
            {
                sbyte0 &= ~0x40;
            }
            else
            {
                sbyte0 |= 0x40;
            }
            if (Keyboard.IsPressed(MotionKey.UNKNOW_T)) //if (Keyboard.IsPressed(Corekey.T))
            {
                //sbyte0 &= ~0x20;
            }
            else
            {
                //sbyte0 |= 0x20;
            }
        }
        public static void loop_inputports_konami68000_tmnt()
        {
            if (Keyboard.IsPressed(MotionKey.P1_INSERT_COIN))//if (Keyboard.IsPressed(Corekey.D5))
            {
                sbyte0 &= ~0x01;
            }
            else
            {
                sbyte0 |= 0x01;
            }
            if (Keyboard.IsPressed(MotionKey.P2_INSERT_COIN))//if (Keyboard.IsPressed(Corekey.D6))
            {
                sbyte0 &= ~0x02;
            }
            else
            {
                sbyte0 |= 0x02;
            }
            if (Keyboard.IsPressed(MotionKey.P1_RIGHT))//if (Keyboard.IsPressed(Corekey.D))
            {
                sbyte1 &= ~0x02;
            }
            else
            {
                sbyte1 |= 0x02;
            }
            if (Keyboard.IsPressed(MotionKey.P1_LEFT))//if (Keyboard.IsPressed(Corekey.A))
            {
                sbyte1 &= ~0x01;
            }
            else
            {
                sbyte1 |= 0x01;
            }
            if (Keyboard.IsPressed(MotionKey.P1_DOWN))//if (Keyboard.IsPressed(Corekey.S))
            {
                sbyte1 &= ~0x08;
            }
            else
            {
                sbyte1 |= 0x08;
            }
            if (Keyboard.IsPressed(MotionKey.P1_UP))//if (Keyboard.IsPressed(Corekey.W))
            {
                sbyte1 &= ~0x04;
            }
            else
            {
                sbyte1 |= 0x04;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_1))//if (Keyboard.IsPressed(Corekey.J))
            {
                sbyte1 &= ~0x10;
            }
            else
            {
                sbyte1 |= 0x10;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_2))//if (Keyboard.IsPressed(Corekey.K))
            {
                sbyte1 &= ~0x20;
            }
            else
            {
                sbyte1 |= 0x20;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_E))//if (Keyboard.IsPressed(Corekey.L))
            {
                //sbyte1 &= ~0x04;
            }
            else
            {
                //sbyte1 |= 0x04;
            }
            if (Keyboard.IsPressed(MotionKey.P2_RIGHT))//if (Keyboard.IsPressed(Corekey.Right))
            {
                sbyte2 &= ~0x02;
            }
            else
            {
                sbyte2 |= 0x02;
            }
            if (Keyboard.IsPressed(MotionKey.P2_LEFT))//if (Keyboard.IsPressed(Corekey.Left))
            {
                sbyte2 &= ~0x01;
            }
            else
            {
                sbyte2 |= 0x01;
            }
            if (Keyboard.IsPressed(MotionKey.P2_DOWN)) //if (Keyboard.IsPressed(Corekey.Down))
            {
                sbyte2 &= ~0x08;
            }
            else
            {
                sbyte2 |= 0x08;
            }
            if (Keyboard.IsPressed(MotionKey.P2_UP))//if (Keyboard.IsPressed(Corekey.Up))
            {
                sbyte2 &= ~0x04;
            }
            else
            {
                sbyte2 |= 0x04;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_1))//if (Keyboard.IsPressed(Corekey.NumPad1))
            {
                sbyte2 &= ~0x10;
            }
            else
            {
                sbyte2 |= 0x10;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_2))//if (Keyboard.IsPressed(Corekey.NumPad2))
            {
                sbyte2 &= ~0x20;
            }
            else
            {
                sbyte2 |= 0x20;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_E))//if (Keyboard.IsPressed(Corekey.NumPad3))
            {
                //sbyte1 &= ~0x40;
            }
            else
            {
                //sbyte1 |= 0x40;
            }
            if (Keyboard.IsPressed(MotionKey.UNKNOW_R)) //if (Keyboard.IsPressed(Corekey.R))
            {
                sbyte0 &= ~0x10;
            }
            else
            {
                sbyte0 |= 0x10;
            }
            if (Keyboard.IsPressed(MotionKey.UNKNOW_T)) //if (Keyboard.IsPressed(Corekey.T))
            {
                sbyte0 &= ~0x20;
            }
            else
            {
                sbyte0 |= 0x20;
            }
        }
        public static void loop_inputports_konami68000_blswhstl()
        {
            if (Keyboard.IsPressed(MotionKey.P1_INSERT_COIN))//if (Keyboard.IsPressed(Corekey.D5))
            {
                sbyte0 &= ~0x01;
            }
            else
            {
                sbyte0 |= 0x01;
            }
            if (Keyboard.IsPressed(MotionKey.P2_INSERT_COIN))//if (Keyboard.IsPressed(Corekey.D6))
            {
                sbyte0 &= ~0x02;
            }
            else
            {
                sbyte0 |= 0x02;
            }
            if (Keyboard.IsPressed(MotionKey.P1_GAMESTART))//if (Keyboard.IsPressed(Corekey.D1))
            {
                sbyte0 &= ~0x10;
            }
            else
            {
                sbyte0 |= 0x10;
            }
            if (Keyboard.IsPressed(MotionKey.P2_GAMESTART))//if (Keyboard.IsPressed(Corekey.D2))
            {
                sbyte0 &= ~0x20;
            }
            else
            {
                sbyte0 |= 0x20;
            }
            if (Keyboard.IsPressed(MotionKey.P1_RIGHT))//if (Keyboard.IsPressed(Corekey.D))
            {
                sbyte1 &= ~0x02;
            }
            else
            {
                sbyte1 |= 0x02;
            }
            if (Keyboard.IsPressed(MotionKey.P1_LEFT))//if (Keyboard.IsPressed(Corekey.A))
            {
                sbyte1 &= ~0x01;
            }
            else
            {
                sbyte1 |= 0x01;
            }
            if (Keyboard.IsPressed(MotionKey.P1_DOWN))//if (Keyboard.IsPressed(Corekey.S))
            {
                sbyte1 &= ~0x08;
            }
            else
            {
                sbyte1 |= 0x08;
            }
            if (Keyboard.IsPressed(MotionKey.P1_UP))//if (Keyboard.IsPressed(Corekey.W))
            {
                sbyte1 &= ~0x04;
            }
            else
            {
                sbyte1 |= 0x04;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_1))//if (Keyboard.IsPressed(Corekey.J))
            {
                sbyte1 &= ~0x10;
            }
            else
            {
                sbyte1 |= 0x10;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_2))//if (Keyboard.IsPressed(Corekey.K))
            {
                sbyte1 &= ~0x20;
            }
            else
            {
                sbyte1 |= 0x20;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_E))//if (Keyboard.IsPressed(Corekey.L))
            {
                //sbyte1 &= ~0x04;
            }
            else
            {
                //sbyte1 |= 0x04;
            }
            if (Keyboard.IsPressed(MotionKey.P2_RIGHT))//if (Keyboard.IsPressed(Corekey.Right))
            {
                sbyte2 &= ~0x02;
            }
            else
            {
                sbyte2 |= 0x02;
            }
            if (Keyboard.IsPressed(MotionKey.P2_LEFT))//if (Keyboard.IsPressed(Corekey.Left))
            {
                sbyte2 &= ~0x01;
            }
            else
            {
                sbyte2 |= 0x01;
            }
            if (Keyboard.IsPressed(MotionKey.P2_DOWN)) //if (Keyboard.IsPressed(Corekey.Down))
            {
                sbyte2 &= ~0x08;
            }
            else
            {
                sbyte2 |= 0x08;
            }
            if (Keyboard.IsPressed(MotionKey.P2_UP))//if (Keyboard.IsPressed(Corekey.Up))
            {
                sbyte2 &= ~0x04;
            }
            else
            {
                sbyte2 |= 0x04;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_1))//if (Keyboard.IsPressed(Corekey.NumPad1))
            {
                sbyte2 &= ~0x10;
            }
            else
            {
                sbyte2 |= 0x10;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_2))//if (Keyboard.IsPressed(Corekey.NumPad2))
            {
                sbyte2 &= ~0x20;
            }
            else
            {
                sbyte2 |= 0x20;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_E))//if (Keyboard.IsPressed(Corekey.NumPad3))
            {
                //sbyte1 &= ~0x40;
            }
            else
            {
                //sbyte1 |= 0x40;
            }
            if (Keyboard.IsPressed(MotionKey.UNKNOW_R)) //if (Keyboard.IsPressed(Corekey.R))
            {
                sbyte0 &= ~0x04;
            }
            else
            {
                sbyte0 |= 0x04;
            }
            if (Keyboard.IsPressed(MotionKey.UNKNOW_T)) //if (Keyboard.IsPressed(Corekey.T))
            {
                //sbyte0 &= ~0x20;
            }
            else
            {
                //sbyte0 |= 0x20;
            }
        }
        public static void loop_inputports_konami68000_glfgreat()
        {
            if (Keyboard.IsPressed(MotionKey.P1_INSERT_COIN))//if (Keyboard.IsPressed(Corekey.D5))
            {
                sbyte0 &= ~0x01;
            }
            else
            {
                sbyte0 |= 0x01;
            }
            if (Keyboard.IsPressed(MotionKey.P2_INSERT_COIN))//if (Keyboard.IsPressed(Corekey.D6))
            {
                sbyte0 &= ~0x02;
            }
            else
            {
                sbyte0 |= 0x02;
            }
            if (Keyboard.IsPressed(MotionKey.P1_GAMESTART))//if (Keyboard.IsPressed(Corekey.D1))
            {
                dsw3 &= unchecked((byte)~0x01);
            }
            else
            {
                dsw3 |= 0x01;
            }
            if (Keyboard.IsPressed(MotionKey.P2_GAMESTART))//if (Keyboard.IsPressed(Corekey.D2))
            {
                dsw3 &= unchecked((byte)~0x02);
            }
            else
            {
                dsw3 |= 0x02;
            }
            if (Keyboard.IsPressed(MotionKey.P1_RIGHT))//if (Keyboard.IsPressed(Corekey.D))
            {
                sbyte1 &= ~0x02;
            }
            else
            {
                sbyte1 |= 0x02;
            }
            if (Keyboard.IsPressed(MotionKey.P1_LEFT))//if (Keyboard.IsPressed(Corekey.A))
            {
                sbyte1 &= ~0x01;
            }
            else
            {
                sbyte1 |= 0x01;
            }
            if (Keyboard.IsPressed(MotionKey.P1_DOWN))//if (Keyboard.IsPressed(Corekey.S))
            {
                sbyte1 &= ~0x08;
            }
            else
            {
                sbyte1 |= 0x08;
            }
            if (Keyboard.IsPressed(MotionKey.P1_UP))//if (Keyboard.IsPressed(Corekey.W))
            {
                sbyte1 &= ~0x04;
            }
            else
            {
                sbyte1 |= 0x04;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_1))//if (Keyboard.IsPressed(Corekey.J))
            {
                sbyte1 &= ~0x10;
            }
            else
            {
                sbyte1 |= 0x10;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_2))//if (Keyboard.IsPressed(Corekey.K))
            {
                sbyte1 &= ~0x20;
            }
            else
            {
                sbyte1 |= 0x20;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_E))//if (Keyboard.IsPressed(Corekey.L))
            {
                sbyte1 &= ~0x40;
            }
            else
            {
                sbyte1 |= 0x40;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_3))//if (Keyboard.IsPressed(Corekey.U))
            {
                sbyte1 &= unchecked((sbyte)~0x80);
            }
            else
            {
                sbyte1 |= unchecked((sbyte)0x80);
            }
            if (Keyboard.IsPressed(MotionKey.P2_RIGHT))//if (Keyboard.IsPressed(Corekey.Right))
            {
                sbyte2 &= ~0x02;
            }
            else
            {
                sbyte2 |= 0x02;
            }
            if (Keyboard.IsPressed(MotionKey.P2_LEFT))//if (Keyboard.IsPressed(Corekey.Left))
            {
                sbyte2 &= ~0x01;
            }
            else
            {
                sbyte2 |= 0x01;
            }
            if (Keyboard.IsPressed(MotionKey.P2_DOWN)) //if (Keyboard.IsPressed(Corekey.Down))
            {
                sbyte2 &= ~0x08;
            }
            else
            {
                sbyte2 |= 0x08;
            }
            if (Keyboard.IsPressed(MotionKey.P2_UP))//if (Keyboard.IsPressed(Corekey.Up))
            {
                sbyte2 &= ~0x04;
            }
            else
            {
                sbyte2 |= 0x04;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_1))//if (Keyboard.IsPressed(Corekey.NumPad1))
            {
                sbyte2 &= ~0x10;
            }
            else
            {
                sbyte2 |= 0x10;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_2))//if (Keyboard.IsPressed(Corekey.NumPad2))
            {
                sbyte2 &= ~0x20;
            }
            else
            {
                sbyte2 |= 0x20;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_E))//if (Keyboard.IsPressed(Corekey.NumPad3))
            {
                sbyte2 &= ~0x40;
            }
            else
            {
                sbyte2 |= 0x40;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_3))//if (Keyboard.IsPressed(Corekey.NumPad4))
            {
                sbyte2 &= unchecked((sbyte)~0x80);
            }
            else
            {
                sbyte2 |= unchecked((sbyte)0x80);
            }
            if (Keyboard.IsPressed(MotionKey.UNKNOW_R)) //if (Keyboard.IsPressed(Corekey.R))
            {
                sbyte0 &= ~0x10;
            }
            else
            {
                sbyte0 |= 0x10;
            }
            if (Keyboard.IsPressed(MotionKey.UNKNOW_T)) //if (Keyboard.IsPressed(Corekey.T))
            {
                //sbyte0 &= ~0x20;
            }
            else
            {
                //sbyte0 |= 0x20;
            }
        }
        public static void loop_inputports_konami68000_qgakumon()
        {
            if (Keyboard.IsPressed(MotionKey.P1_INSERT_COIN))//if (Keyboard.IsPressed(Corekey.D5))
            {
                sbyte0 &= ~0x01;
            }
            else
            {
                sbyte0 |= 0x01;
            }
            if (Keyboard.IsPressed(MotionKey.P2_INSERT_COIN))//if (Keyboard.IsPressed(Corekey.D6))
            {
                sbyte0 &= ~0x02;
            }
            else
            {
                sbyte0 |= 0x02;
            }
            if (Keyboard.IsPressed(MotionKey.P1_GAMESTART))//if (Keyboard.IsPressed(Corekey.D1))
            {
                sbyte1 &= unchecked((sbyte)~0x80);
            }
            else
            {
                sbyte1 |= unchecked((sbyte)0x80);
            }
            if (Keyboard.IsPressed(MotionKey.P2_GAMESTART))//if (Keyboard.IsPressed(Corekey.D2))
            {
                sbyte2 &= unchecked((sbyte)~0x80);
            }
            else
            {
                sbyte2 |= unchecked((sbyte)0x80);
            }
            if (Keyboard.IsPressed(MotionKey.P1_RIGHT))//if (Keyboard.IsPressed(Corekey.D))
            {
                sbyte1 &= ~0x02;
            }
            else
            {
                sbyte1 |= 0x02;
            }
            if (Keyboard.IsPressed(MotionKey.P1_LEFT))//if (Keyboard.IsPressed(Corekey.A))
            {
                sbyte1 &= ~0x01;
            }
            else
            {
                sbyte1 |= 0x01;
            }
            if (Keyboard.IsPressed(MotionKey.P1_DOWN))//if (Keyboard.IsPressed(Corekey.S))
            {
                sbyte1 &= ~0x08;
            }
            else
            {
                sbyte1 |= 0x08;
            }
            if (Keyboard.IsPressed(MotionKey.P1_UP))//if (Keyboard.IsPressed(Corekey.W))
            {
                sbyte1 &= ~0x04;
            }
            else
            {
                sbyte1 |= 0x04;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_1))//if (Keyboard.IsPressed(Corekey.J))
            {
                sbyte1 &= ~0x10;
            }
            else
            {
                sbyte1 |= 0x10;
            }
            /*if (Keyboard.IsPressed(Key.K))
            {
                sbyte1 &= ~0x20;
            }
            else
            {
                sbyte1 |= 0x20;
            }
            if (Keyboard.IsPressed(Key.L))
            {
                sbyte1 &= ~0x40;
            }
            else
            {
                sbyte1 |= 0x40;
            }*/
            if (Keyboard.IsPressed(MotionKey.P2_RIGHT))//if (Keyboard.IsPressed(Corekey.Right))
            {
                sbyte2 &= ~0x02;
            }
            else
            {
                sbyte2 |= 0x02;
            }
            if (Keyboard.IsPressed(MotionKey.P2_LEFT))//if (Keyboard.IsPressed(Corekey.Left))
            {
                sbyte2 &= ~0x01;
            }
            else
            {
                sbyte2 |= 0x01;
            }
            if (Keyboard.IsPressed(MotionKey.P2_DOWN)) //if (Keyboard.IsPressed(Corekey.Down))
            {
                sbyte2 &= ~0x08;
            }
            else
            {
                sbyte2 |= 0x08;
            }
            if (Keyboard.IsPressed(MotionKey.P2_UP))//if (Keyboard.IsPressed(Corekey.Up))
            {
                sbyte2 &= ~0x04;
            }
            else
            {
                sbyte2 |= 0x04;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_1))//if (Keyboard.IsPressed(Corekey.NumPad1))
            {
                sbyte2 &= ~0x10;
            }
            else
            {
                sbyte2 |= 0x10;
            }
            /*if (Keyboard.IsPressed(Key.NumPad2))
            {
                sbyte2 &= ~0x20;
            }
            else
            {
                sbyte2 |= 0x20;
            }
            if (Keyboard.IsPressed(Key.NumPad3))
            {
                sbyte2 &= ~0x40;
            }
            else
            {
                sbyte2 |= 0x40;
            }*/
            if (Keyboard.IsPressed(MotionKey.UNKNOW_R)) //if (Keyboard.IsPressed(Corekey.R))
            {
                //sbyte0 &= ~0x40;
            }
            else
            {
                //sbyte0 |= 0x40;
            }
            if (Keyboard.IsPressed(MotionKey.UNKNOW_T)) //if (Keyboard.IsPressed(Corekey.T))
            {
                //sbyte0 &= ~0x20;
            }
            else
            {
                //sbyte0 |= 0x20;
            }
        }
        public static void loop_inputports_konami68000_ssriders()
        {
            if (Keyboard.IsPressed(MotionKey.P1_INSERT_COIN))//if (Keyboard.IsPressed(Corekey.D5))
            {
                sbyte0 &= ~0x01;
            }
            else
            {
                sbyte0 |= 0x01;
            }
            if (Keyboard.IsPressed(MotionKey.P2_INSERT_COIN))//if (Keyboard.IsPressed(Corekey.D6))
            {
                sbyte0 &= ~0x02;
            }
            else
            {
                sbyte0 |= 0x02;
            }
            if (Keyboard.IsPressed(MotionKey.P1_GAMESTART))//if (Keyboard.IsPressed(Corekey.D1))
            {
                sbyte1 &= unchecked((sbyte)~0x80);
            }
            else
            {
                sbyte1 |= unchecked((sbyte)0x80);
            }
            if (Keyboard.IsPressed(MotionKey.P2_GAMESTART))//if (Keyboard.IsPressed(Corekey.D2))
            {
                sbyte2 &= unchecked((sbyte)~0x80);
            }
            else
            {
                sbyte2 |= unchecked((sbyte)0x80);
            }
            if (Keyboard.IsPressed(MotionKey.P1_RIGHT))//if (Keyboard.IsPressed(Corekey.D))
            {
                sbyte1 &= ~0x02;
            }
            else
            {
                sbyte1 |= 0x02;
            }
            if (Keyboard.IsPressed(MotionKey.P1_LEFT))//if (Keyboard.IsPressed(Corekey.A))
            {
                sbyte1 &= ~0x01;
            }
            else
            {
                sbyte1 |= 0x01;
            }
            if (Keyboard.IsPressed(MotionKey.P1_DOWN))//if (Keyboard.IsPressed(Corekey.S))
            {
                sbyte1 &= ~0x08;
            }
            else
            {
                sbyte1 |= 0x08;
            }
            if (Keyboard.IsPressed(MotionKey.P1_UP))//if (Keyboard.IsPressed(Corekey.W))
            {
                sbyte1 &= ~0x04;
            }
            else
            {
                sbyte1 |= 0x04;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_1))//if (Keyboard.IsPressed(Corekey.J))
            {
                sbyte1 &= ~0x10;
            }
            else
            {
                sbyte1 |= 0x10;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_2))//if (Keyboard.IsPressed(Corekey.K))
            {
                sbyte1 &= ~0x20;
            }
            else
            {
                sbyte1 |= 0x20;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_E))//if (Keyboard.IsPressed(Corekey.L))
            {
                //sbyte1 &= ~0x04;
            }
            else
            {
                //sbyte1 |= 0x04;
            }
            if (Keyboard.IsPressed(MotionKey.P2_RIGHT))//if (Keyboard.IsPressed(Corekey.Right))
            {
                sbyte2 &= ~0x02;
            }
            else
            {
                sbyte2 |= 0x02;
            }
            if (Keyboard.IsPressed(MotionKey.P2_LEFT))//if (Keyboard.IsPressed(Corekey.Left))
            {
                sbyte2 &= ~0x01;
            }
            else
            {
                sbyte2 |= 0x01;
            }
            if (Keyboard.IsPressed(MotionKey.P2_DOWN)) //if (Keyboard.IsPressed(Corekey.Down))
            {
                sbyte2 &= ~0x08;
            }
            else
            {
                sbyte2 |= 0x08;
            }
            if (Keyboard.IsPressed(MotionKey.P2_UP))//if (Keyboard.IsPressed(Corekey.Up))
            {
                sbyte2 &= ~0x04;
            }
            else
            {
                sbyte2 |= 0x04;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_1))//if (Keyboard.IsPressed(Corekey.NumPad1))
            {
                sbyte2 &= ~0x10;
            }
            else
            {
                sbyte2 |= 0x10;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_2))//if (Keyboard.IsPressed(Corekey.NumPad2))
            {
                sbyte2 &= ~0x20;
            }
            else
            {
                sbyte2 |= 0x20;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_E))//if (Keyboard.IsPressed(Corekey.NumPad3))
            {
                //sbyte1 &= ~0x40;
            }
            else
            {
                //sbyte1 |= 0x40;
            }
            if (Keyboard.IsPressed(MotionKey.UNKNOW_R)) //if (Keyboard.IsPressed(Corekey.R))
            {
                sbyte0 &= ~0x10;
            }
            else
            {
                sbyte0 |= 0x10;
            }
            if (Keyboard.IsPressed(MotionKey.UNKNOW_T)) //if (Keyboard.IsPressed(Corekey.T))
            {
                sbyte0 &= ~0x20;
            }
            else
            {
                sbyte0 |= 0x20;
            }
        }
        public static void loop_inputports_konami68000_thndrx2()
        {
            if (Keyboard.IsPressed(MotionKey.P1_INSERT_COIN))//if (Keyboard.IsPressed(Corekey.D5))
            {
                sbyte0 &= ~0x01;
            }
            else
            {
                sbyte0 |= 0x01;
            }
            if (Keyboard.IsPressed(MotionKey.P2_INSERT_COIN))//if (Keyboard.IsPressed(Corekey.D6))
            {
                sbyte0 &= ~0x02;
            }
            else
            {
                sbyte0 |= 0x02;
            }
            if (Keyboard.IsPressed(MotionKey.P1_GAMESTART))//if (Keyboard.IsPressed(Corekey.D1))
            {
                sbyte1 &= unchecked((sbyte)~0x80);
            }
            else
            {
                sbyte1 |= unchecked((sbyte)0x80);
            }
            if (Keyboard.IsPressed(MotionKey.P2_GAMESTART))//if (Keyboard.IsPressed(Corekey.D2))
            {
                sbyte2 &= unchecked((sbyte)~0x80);
            }
            else
            {
                sbyte2 |= unchecked((sbyte)0x80);
            }
            if (Keyboard.IsPressed(MotionKey.P1_RIGHT))//if (Keyboard.IsPressed(Corekey.D))
            {
                sbyte1 &= ~0x02;
            }
            else
            {
                sbyte1 |= 0x02;
            }
            if (Keyboard.IsPressed(MotionKey.P1_LEFT))//if (Keyboard.IsPressed(Corekey.A))
            {
                sbyte1 &= ~0x01;
            }
            else
            {
                sbyte1 |= 0x01;
            }
            if (Keyboard.IsPressed(MotionKey.P1_DOWN))//if (Keyboard.IsPressed(Corekey.S))
            {
                sbyte1 &= ~0x08;
            }
            else
            {
                sbyte1 |= 0x08;
            }
            if (Keyboard.IsPressed(MotionKey.P1_UP))//if (Keyboard.IsPressed(Corekey.W))
            {
                sbyte1 &= ~0x04;
            }
            else
            {
                sbyte1 |= 0x04;
            }
            if (((byte)sbyte1 & 0x03) == 0)
            {
                sbyte1 |= 0x03;
            }
            if (((byte)sbyte1 & 0x0c) == 0)
            {
                sbyte1 |= 0x0c;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_1))//if (Keyboard.IsPressed(Corekey.J))
            {
                sbyte1 &= ~0x10;
            }
            else
            {
                sbyte1 |= 0x10;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_2))//if (Keyboard.IsPressed(Corekey.K))
            {
                sbyte1 &= ~0x20;
            }
            else
            {
                sbyte1 |= 0x20;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_E))//if (Keyboard.IsPressed(Corekey.L))
            {
                //sbyte1 &= ~0x04;
            }
            else
            {
                //sbyte1 |= 0x04;
            }
            if (Keyboard.IsPressed(MotionKey.P2_RIGHT))//if (Keyboard.IsPressed(Corekey.Right))
            {
                sbyte2 &= ~0x02;
            }
            else
            {
                sbyte2 |= 0x02;
            }
            if (Keyboard.IsPressed(MotionKey.P2_LEFT))//if (Keyboard.IsPressed(Corekey.Left))
            {
                sbyte2 &= ~0x01;
            }
            else
            {
                sbyte2 |= 0x01;
            }
            if (Keyboard.IsPressed(MotionKey.P2_DOWN)) //if (Keyboard.IsPressed(Corekey.Down))
            {
                sbyte2 &= ~0x08;
            }
            else
            {
                sbyte2 |= 0x08;
            }
            if (Keyboard.IsPressed(MotionKey.P2_UP))//if (Keyboard.IsPressed(Corekey.Up))
            {
                sbyte2 &= ~0x04;
            }
            else
            {
                sbyte2 |= 0x04;
            }
            if (((byte)sbyte2 & 0x03) == 0)
            {
                sbyte2 |= 0x03;
            }
            if (((byte)sbyte2 & 0x0c) == 0)
            {
                sbyte2 |= 0x0c;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_1))//if (Keyboard.IsPressed(Corekey.NumPad1))
            {
                sbyte2 &= ~0x10;
            }
            else
            {
                sbyte2 |= 0x10;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_2))//if (Keyboard.IsPressed(Corekey.NumPad2))
            {
                sbyte2 &= ~0x20;
            }
            else
            {
                sbyte2 |= 0x20;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_E))//if (Keyboard.IsPressed(Corekey.NumPad3))
            {
                //sbyte1 &= ~0x40;
            }
            else
            {
                //sbyte1 |= 0x40;
            }
            if (Keyboard.IsPressed(MotionKey.UNKNOW_R)) //if (Keyboard.IsPressed(Corekey.R))
            {
                sbyte0 &= ~0x04;
            }
            else
            {
                sbyte0 |= 0x04;
            }
            if (Keyboard.IsPressed(MotionKey.UNKNOW_T)) //if (Keyboard.IsPressed(Corekey.T))
            {
                //sbyte0 &= ~0x20;
            }
            else
            {
                //sbyte0 |= 0x20;
            }
        }
        public static void loop_inputports_konami68000_prmrsocr()
        {
            if (Keyboard.IsPressed(MotionKey.P1_INSERT_COIN))//if (Keyboard.IsPressed(Corekey.D5))
            {
                sbyte0 &= ~0x04;
            }
            else
            {
                sbyte0 |= 0x04;
            }
            if (Keyboard.IsPressed(MotionKey.P2_INSERT_COIN))//if (Keyboard.IsPressed(Corekey.D6))
            {
                bytee &= unchecked((byte)~0x04);
            }
            else
            {
                bytee |= 0x04;
            }
            if (Keyboard.IsPressed(MotionKey.P1_GAMESTART))//if (Keyboard.IsPressed(Corekey.D1))
            {
                sbyte1 &= unchecked((sbyte)~0x80);
            }
            else
            {
                sbyte1 |= unchecked((sbyte)0x80);
            }
            if (Keyboard.IsPressed(MotionKey.P2_GAMESTART))//if (Keyboard.IsPressed(Corekey.D2))
            {
                sbyte2 &= unchecked((sbyte)~0x80);
            }
            else
            {
                sbyte2 |= unchecked((sbyte)0x80);
            }
            if (Keyboard.IsPressed(MotionKey.P1_RIGHT))//if (Keyboard.IsPressed(Corekey.D))
            {
                sbyte1 &= ~0x02;
            }
            else
            {
                sbyte1 |= 0x02;
            }
            if (Keyboard.IsPressed(MotionKey.P1_LEFT))//if (Keyboard.IsPressed(Corekey.A))
            {
                sbyte1 &= ~0x01;
            }
            else
            {
                sbyte1 |= 0x01;
            }
            if (Keyboard.IsPressed(MotionKey.P1_DOWN))//if (Keyboard.IsPressed(Corekey.S))
            {
                sbyte1 &= ~0x08;
            }
            else
            {
                sbyte1 |= 0x08;
            }
            if (Keyboard.IsPressed(MotionKey.P1_UP))//if (Keyboard.IsPressed(Corekey.W))
            {
                sbyte1 &= ~0x04;
            }
            else
            {
                sbyte1 |= 0x04;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_1))//if (Keyboard.IsPressed(Corekey.J))
            {
                sbyte1 &= ~0x10;
            }
            else
            {
                sbyte1 |= 0x10;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_2))//if (Keyboard.IsPressed(Corekey.K))
            {
                sbyte1 &= ~0x20;
            }
            else
            {
                sbyte1 |= 0x20;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_E))//if (Keyboard.IsPressed(Corekey.L))
            {
                //sbyte1 &= ~0x04;
            }
            else
            {
                //sbyte1 |= 0x04;
            }
            if (Keyboard.IsPressed(MotionKey.P2_RIGHT))//if (Keyboard.IsPressed(Corekey.Right))
            {
                sbyte2 &= ~0x02;
            }
            else
            {
                sbyte2 |= 0x02;
            }
            if (Keyboard.IsPressed(MotionKey.P2_LEFT))//if (Keyboard.IsPressed(Corekey.Left))
            {
                sbyte2 &= ~0x01;
            }
            else
            {
                sbyte2 |= 0x01;
            }
            if (Keyboard.IsPressed(MotionKey.P2_DOWN)) //if (Keyboard.IsPressed(Corekey.Down))
            {
                sbyte2 &= ~0x08;
            }
            else
            {
                sbyte2 |= 0x08;
            }
            if (Keyboard.IsPressed(MotionKey.P2_UP))//if (Keyboard.IsPressed(Corekey.Up))
            {
                sbyte2 &= ~0x04;
            }
            else
            {
                sbyte2 |= 0x04;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_1))//if (Keyboard.IsPressed(Corekey.NumPad1))
            {
                sbyte2 &= ~0x10;
            }
            else
            {
                sbyte2 |= 0x10;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_2))//if (Keyboard.IsPressed(Corekey.NumPad2))
            {
                sbyte2 &= ~0x20;
            }
            else
            {
                sbyte2 |= 0x20;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_E))//if (Keyboard.IsPressed(Corekey.NumPad3))
            {
                //sbyte1 &= ~0x40;
            }
            else
            {
                //sbyte1 |= 0x40;
            }
            if (Keyboard.IsPressed(MotionKey.UNKNOW_R)) //if (Keyboard.IsPressed(Corekey.R))
            {
                sbyte0 &= ~0x01;
            }
            else
            {
                sbyte0 |= 0x01;
            }
            if (Keyboard.IsPressed(MotionKey.UNKNOW_T)) //if (Keyboard.IsPressed(Corekey.T))
            {
                //sbyte0 &= ~0x20;
            }
            else
            {
                //sbyte0 |= 0x20;
            }
        }
        public static void record_port()
        {
            if (sbyte0 != sbyte0_old || sbyte1 != sbyte1_old || sbyte2 != sbyte2_old || sbyte3 != sbyte3_old || sbyte4 != sbyte4_old)
            {
                sbyte0_old = sbyte0;
                sbyte1_old = sbyte1;
                sbyte2_old = sbyte2;
                sbyte3_old = sbyte3;
                sbyte4_old = sbyte4;
                Mame.bwRecord.Write(Video.screenstate.frame_number);
                Mame.bwRecord.Write(sbyte0);
                Mame.bwRecord.Write(sbyte1);
                Mame.bwRecord.Write(sbyte2);
                Mame.bwRecord.Write(sbyte3);
                Mame.bwRecord.Write(sbyte4);
            }
        }
        public static void replay_port()
        {
            if (Inptport.bReplayRead)
            {
                try
                {
                    Video.frame_number_obj = Mame.brRecord.ReadInt64();
                    sbyte0_old = Mame.brRecord.ReadSByte();
                    sbyte1_old = Mame.brRecord.ReadSByte();
                    sbyte2_old = Mame.brRecord.ReadSByte();
                    sbyte3_old = Mame.brRecord.ReadSByte();
                    sbyte4_old = Mame.brRecord.ReadSByte();
                }
                catch
                {
                    Mame.playState = Mame.PlayState.PLAY_REPLAYEND;
                }
                Inptport.bReplayRead = false;
            }
            if (Video.screenstate.frame_number == Video.frame_number_obj)
            {
                sbyte0 = sbyte0_old;
                sbyte1 = sbyte1_old;
                sbyte2 = sbyte2_old;
                sbyte3 = sbyte3_old;
                sbyte4 = sbyte4_old;
                Inptport.bReplayRead = true;
            }
            else
            {
                Inptport.bReplayRead = false;
            }
        }
        public static void record_port_prmrsocr()
        {
            if (sbyte0 != sbyte0_old || sbyte1 != sbyte1_old || sbyte2 != sbyte2_old || sbyte3 != sbyte3_old || sbyte4 != sbyte4_old || bytee != bytee_old)
            {
                sbyte0_old = sbyte0;
                sbyte1_old = sbyte1;
                sbyte2_old = sbyte2;
                sbyte3_old = sbyte3;
                sbyte4_old = sbyte4;
                bytee_old = bytee;
                Mame.bwRecord.Write(Video.screenstate.frame_number);
                Mame.bwRecord.Write(sbyte0);
                Mame.bwRecord.Write(sbyte1);
                Mame.bwRecord.Write(sbyte2);
                Mame.bwRecord.Write(sbyte3);
                Mame.bwRecord.Write(sbyte4);
                Mame.bwRecord.Write(bytee);
            }
        }
        public static void replay_port_prmrsocr()
        {
            if (Inptport.bReplayRead)
            {
                try
                {
                    Video.frame_number_obj = Mame.brRecord.ReadInt64();
                    sbyte0_old = Mame.brRecord.ReadSByte();
                    sbyte1_old = Mame.brRecord.ReadSByte();
                    sbyte2_old = Mame.brRecord.ReadSByte();
                    sbyte3_old = Mame.brRecord.ReadSByte();
                    sbyte4_old = Mame.brRecord.ReadSByte();
                    bytee_old = Mame.brRecord.ReadByte();
                }
                catch
                {
                    Mame.playState = Mame.PlayState.PLAY_REPLAYEND;
                }
                Inptport.bReplayRead = false;
            }
            if (Video.screenstate.frame_number == Video.frame_number_obj)
            {
                sbyte0 = sbyte0_old;
                sbyte1 = sbyte1_old;
                sbyte2 = sbyte2_old;
                sbyte3 = sbyte3_old;
                sbyte4 = sbyte4_old;
                bytee = bytee_old;
                Inptport.bReplayRead = true;
            }
            else
            {
                Inptport.bReplayRead = false;
            }
        }
    }
}
