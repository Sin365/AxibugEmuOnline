using MAME.Core;

namespace MAME.Core
{
    public partial class Capcom
    {
        public static void loop_inputports_gng()
        {
            if (Keyboard.IsPressed(MotionKey.P1_INSERT_COIN))//if (Keyboard.IsPressed(Corekey.D5))
            {
                bytes &= unchecked((byte)~0x40);
            }
            else
            {
                bytes |= 0x40;
            }
            if (Keyboard.IsPressed(MotionKey.P2_INSERT_COIN))//if (Keyboard.IsPressed(Corekey.D6))
            {
                bytes &= unchecked((byte)~0x80);
            }
            else
            {
                bytes |= 0x80;
            }
            if (Keyboard.IsPressed(MotionKey.P1_GAMESTART))//if (Keyboard.IsPressed(Corekey.D1))
            {
                bytes &= unchecked((byte)~0x01);
            }
            else
            {
                bytes |= 0x01;
            }
            if (Keyboard.IsPressed(MotionKey.P2_GAMESTART))//if (Keyboard.IsPressed(Corekey.D2))
            {
                bytes &= unchecked((byte)~0x02);
            }
            else
            {
                bytes |= 0x02;
            }
            if (Keyboard.IsPressed(MotionKey.P1_RIGHT))//if (Keyboard.IsPressed(Corekey.D))
            {
                byte1 &= unchecked((byte)~0x01);
            }
            else
            {
                byte1 |= 0x01;
            }
            if (Keyboard.IsPressed(MotionKey.P1_LEFT))//if (Keyboard.IsPressed(Corekey.A))
            {
                byte1 &= unchecked((byte)~0x02);
            }
            else
            {
                byte1 |= 0x02;
            }
            if (Keyboard.IsPressed(MotionKey.P1_DOWN))//if (Keyboard.IsPressed(Corekey.S))
            {
                byte1 &= unchecked((byte)~0x04);
            }
            else
            {
                byte1 |= 0x04;
            }
            if (Keyboard.IsPressed(MotionKey.P1_UP))//if (Keyboard.IsPressed(Corekey.W))
            {
                byte1 &= unchecked((byte)~0x08);
            }
            else
            {
                byte1 |= 0x08;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_1))//if (Keyboard.IsPressed(Corekey.J))
            {
                byte1 &= unchecked((byte)~0x10);
            }
            else
            {
                byte1 |= 0x10;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_2))//if (Keyboard.IsPressed(Corekey.K))
            {
                byte1 &= unchecked((byte)~0x20);
            }
            else
            {
                byte1 |= 0x20;
            }
            if (Keyboard.IsPressed(MotionKey.P2_RIGHT))//if (Keyboard.IsPressed(Corekey.Right))
            {
                byte2 &= unchecked((byte)~0x01);
            }
            else
            {
                byte2 |= 0x01;
            }
            if (Keyboard.IsPressed(MotionKey.P2_LEFT))//if (Keyboard.IsPressed(Corekey.Left))
            {
                byte2 &= unchecked((byte)~0x02);
            }
            else
            {
                byte2 |= 0x02;
            }
            if (Keyboard.IsPressed(MotionKey.P2_DOWN)) //if (Keyboard.IsPressed(Corekey.Down))
            {
                byte2 &= unchecked((byte)~0x04);
            }
            else
            {
                byte2 |= 0x04;
            }
            if (Keyboard.IsPressed(MotionKey.P2_UP))//if (Keyboard.IsPressed(Corekey.Up))
            {
                byte2 &= unchecked((byte)~0x08);
            }
            else
            {
                byte2 |= 0x08;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_1))//if (Keyboard.IsPressed(Corekey.NumPad1))
            {
                byte2 &= unchecked((byte)~0x10);
            }
            else
            {
                byte2 |= 0x10;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_2))//if (Keyboard.IsPressed(Corekey.NumPad2))
            {
                byte2 &= unchecked((byte)~0x20);
            }
            else
            {
                byte2 |= 0x20;
            }
            if (Keyboard.IsPressed(MotionKey.UNKNOW_R)) //if (Keyboard.IsPressed(Corekey.R))
            {
                bytes &= unchecked((byte)~0x20);
            }
            else
            {
                bytes |= 0x20;
            }
        }
        public static void loop_inputports_diamond()
        {
            if (Keyboard.IsPressed(MotionKey.P1_INSERT_COIN))//if (Keyboard.IsPressed(Corekey.D5))
            {
                bytes &= unchecked((byte)~0x40);
            }
            else
            {
                bytes |= 0x40;
            }
            if (Keyboard.IsPressed(MotionKey.P2_INSERT_COIN))//if (Keyboard.IsPressed(Corekey.D6))
            {
                bytes &= unchecked((byte)~0x80);
            }
            else
            {
                bytes |= 0x80;
            }
            if (Keyboard.IsPressed(MotionKey.P1_GAMESTART))//if (Keyboard.IsPressed(Corekey.D1))
            {
                bytes &= unchecked((byte)~0x01);
            }
            else
            {
                bytes |= 0x01;
            }
            if (Keyboard.IsPressed(MotionKey.P2_GAMESTART))//if (Keyboard.IsPressed(Corekey.D2))
            {
                bytes &= unchecked((byte)~0x02);
            }
            else
            {
                bytes |= 0x02;
            }
            if (Keyboard.IsPressed(MotionKey.P1_RIGHT))//if (Keyboard.IsPressed(Corekey.D))
            {
                byte1 &= unchecked((byte)~0x01);
            }
            else
            {
                byte1 |= 0x01;
            }
            if (Keyboard.IsPressed(MotionKey.P1_LEFT))//if (Keyboard.IsPressed(Corekey.A))
            {
                byte1 &= unchecked((byte)~0x02);
            }
            else
            {
                byte1 |= 0x02;
            }
            if (Keyboard.IsPressed(MotionKey.P1_DOWN))//if (Keyboard.IsPressed(Corekey.S))
            {
                byte1 &= unchecked((byte)~0x04);
            }
            else
            {
                byte1 |= 0x04;
            }
            if (Keyboard.IsPressed(MotionKey.P1_UP))//if (Keyboard.IsPressed(Corekey.W))
            {
                byte1 &= unchecked((byte)~0x08);
            }
            else
            {
                byte1 |= 0x08;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_1))//if (Keyboard.IsPressed(Corekey.J))
            {
                byte1 &= unchecked((byte)~0x10);
            }
            else
            {
                byte1 |= 0x10;
            }
        }
        public static void loop_inputports_sfus()
        {
            if (Keyboard.IsPressed(MotionKey.P1_INSERT_COIN))//if (Keyboard.IsPressed(Corekey.D5))
            {
                short0 &= ~0x0001;
            }
            else
            {
                short0 |= 0x0001;
            }
            if (Keyboard.IsPressed(MotionKey.P2_INSERT_COIN))//if (Keyboard.IsPressed(Corekey.D6))
            {
                short0 &= ~0x0002;
            }
            else
            {
                short0 |= 0x0002;
            }
            if (Keyboard.IsPressed(MotionKey.P1_GAMESTART))//if (Keyboard.IsPressed(Corekey.D1))
            {
                shorts &= ~0x0001;
            }
            else
            {
                shorts |= 0x0001;
            }
            if (Keyboard.IsPressed(MotionKey.P2_GAMESTART))//if (Keyboard.IsPressed(Corekey.D2))
            {
                shorts &= ~0x0002;
            }
            else
            {
                shorts |= 0x0002;
            }
            if (Keyboard.IsPressed(MotionKey.P1_RIGHT))//if (Keyboard.IsPressed(Corekey.D))
            {
                short1 &= ~0x0001;
            }
            else
            {
                short1 |= 0x0001;
            }
            if (Keyboard.IsPressed(MotionKey.P1_LEFT))//if (Keyboard.IsPressed(Corekey.A))
            {
                short1 &= ~0x0002;
            }
            else
            {
                short1 |= 0x0002;
            }
            if (Keyboard.IsPressed(MotionKey.P1_DOWN))//if (Keyboard.IsPressed(Corekey.S))
            {
                short1 &= ~0x0004;
            }
            else
            {
                short1 |= 0x0004;
            }
            if (Keyboard.IsPressed(MotionKey.P1_UP))//if (Keyboard.IsPressed(Corekey.W))
            {
                short1 &= ~0x0008;
            }
            else
            {
                short1 |= 0x0008;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_1))//if (Keyboard.IsPressed(Corekey.J))
            {
                short1 &= ~0x0010;
            }
            else
            {
                short1 |= 0x0010;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_2))//if (Keyboard.IsPressed(Corekey.K))
            {
                short1 &= ~0x0020;
            }
            else
            {
                short1 |= 0x0020;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_E))//if (Keyboard.IsPressed(Corekey.L))
            {
                short0 &= ~0x0200;
            }
            else
            {
                short0 |= 0x0200;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_3))//if (Keyboard.IsPressed(Corekey.U))
            {
                short1 &= ~0x0040;
            }
            else
            {
                short1 |= 0x0040;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_4))//if (Keyboard.IsPressed(Corekey.I))
            {
                short1 &= ~0x0080;
            }
            else
            {
                short1 |= 0x0080;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_E))//if (Keyboard.IsPressed(Corekey.O))
            {
                short0 &= ~0x0004;
            }
            else
            {
                short0 |= 0x0004;
            }
            if (Keyboard.IsPressed(MotionKey.P2_RIGHT))//if (Keyboard.IsPressed(Corekey.Right))
            {
                short1 &= ~0x0100;
            }
            else
            {
                short1 |= 0x0100;
            }
            if (Keyboard.IsPressed(MotionKey.P2_LEFT))//if (Keyboard.IsPressed(Corekey.Left))
            {
                short1 &= ~0x0200;
            }
            else
            {
                short1 |= 0x0200;
            }
            if (Keyboard.IsPressed(MotionKey.P2_DOWN)) //if (Keyboard.IsPressed(Corekey.Down))
            {
                short1 &= ~0x0400;
            }
            else
            {
                short1 |= 0x0400;
            }
            if (Keyboard.IsPressed(MotionKey.P2_UP))//if (Keyboard.IsPressed(Corekey.Up))
            {
                short1 &= ~0x0800;
            }
            else
            {
                short1 |= 0x0800;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_1))//if (Keyboard.IsPressed(Corekey.NumPad1))
            {
                short1 &= ~0x1000;
            }
            else
            {
                short1 |= 0x1000;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_2))//if (Keyboard.IsPressed(Corekey.NumPad2))
            {
                short1 &= ~0x2000;
            }
            else
            {
                short1 |= 0x2000;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_E))//if (Keyboard.IsPressed(Corekey.NumPad3))
            {
                short0 &= ~0x0400;
            }
            else
            {
                short0 |= 0x0400;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_3))//if (Keyboard.IsPressed(Corekey.NumPad4))
            {
                short1 &= ~0x4000;
            }
            else
            {
                short1 |= 0x4000;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_4))//if (Keyboard.IsPressed(Corekey.NumPad5))
            {
                short1 &= unchecked((short)~0x8000);
            }
            else
            {
                short1 |= unchecked((short)0x8000);
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_F))//if (Keyboard.IsPressed(Corekey.NumPad6))
            {
                short0 &= ~0x0100;
            }
            else
            {
                short0 |= 0x0100;
            }
            if (Keyboard.IsPressed(MotionKey.UNKNOW_R)) //if (Keyboard.IsPressed(Corekey.R))
            {
                shorts &= ~0x0004;
            }
            else
            {
                shorts |= 0x0004;
            }
            if (Keyboard.IsPressed(MotionKey.UNKNOW_T)) //if (Keyboard.IsPressed(Corekey.T))
            {
                //sbyte0 &= ~0x40;
            }
            else
            {
                //sbyte0 |= 0x40;
            }
        }
        public static void loop_inputports_sfjp()
        {
            if (Keyboard.IsPressed(MotionKey.P1_INSERT_COIN))//if (Keyboard.IsPressed(Corekey.D5))
            {
                shortc &= ~0x0001;
            }
            else
            {
                shortc |= 0x0001;
            }
            if (Keyboard.IsPressed(MotionKey.P2_INSERT_COIN))//if (Keyboard.IsPressed(Corekey.D6))
            {
                shortc &= ~0x0002;
            }
            else
            {
                shortc |= 0x0002;
            }
            if (Keyboard.IsPressed(MotionKey.P1_GAMESTART))//if (Keyboard.IsPressed(Corekey.D1))
            {
                shorts &= ~0x0001;
            }
            else
            {
                shorts |= 0x0001;
            }
            if (Keyboard.IsPressed(MotionKey.P2_GAMESTART))//if (Keyboard.IsPressed(Corekey.D2))
            {
                shorts &= ~0x0002;
            }
            else
            {
                shorts |= 0x0002;
            }
            if (Keyboard.IsPressed(MotionKey.P1_RIGHT))//if (Keyboard.IsPressed(Corekey.D))
            {
                short1 &= ~0x0001;
            }
            else
            {
                short1 |= 0x0001;
            }
            if (Keyboard.IsPressed(MotionKey.P1_LEFT))//if (Keyboard.IsPressed(Corekey.A))
            {
                short1 &= ~0x0002;
            }
            else
            {
                short1 |= 0x0002;
            }
            if (Keyboard.IsPressed(MotionKey.P1_DOWN))//if (Keyboard.IsPressed(Corekey.S))
            {
                short1 &= ~0x0004;
            }
            else
            {
                short1 |= 0x0004;
            }
            if (Keyboard.IsPressed(MotionKey.P1_UP))//if (Keyboard.IsPressed(Corekey.W))
            {
                short1 &= ~0x0008;
            }
            else
            {
                short1 |= 0x0008;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_1))//if (Keyboard.IsPressed(Corekey.J))
            {
                short1 &= ~0x0100;
            }
            else
            {
                short1 |= 0x0100;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_2))//if (Keyboard.IsPressed(Corekey.K))
            {
                short1 &= ~0x0200;
            }
            else
            {
                short1 |= 0x0200;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_E))//if (Keyboard.IsPressed(Corekey.L))
            {
                short1 &= ~0x0400;
            }
            else
            {
                short1 |= 0x0400;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_3))//if (Keyboard.IsPressed(Corekey.U))
            {
                short1 &= ~0x1000;
            }
            else
            {
                short1 |= 0x1000;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_4))//if (Keyboard.IsPressed(Corekey.I))
            {
                short1 &= ~0x2000;
            }
            else
            {
                short1 |= 0x2000;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_E))//if (Keyboard.IsPressed(Corekey.O))
            {
                short1 &= ~0x4000;
            }
            else
            {
                short1 |= 0x4000;
            }
            if (Keyboard.IsPressed(MotionKey.P2_RIGHT))//if (Keyboard.IsPressed(Corekey.Right))
            {
                short2 &= ~0x0001;
            }
            else
            {
                short2 |= 0x0001;
            }
            if (Keyboard.IsPressed(MotionKey.P2_LEFT))//if (Keyboard.IsPressed(Corekey.Left))
            {
                short2 &= ~0x0002;
            }
            else
            {
                short2 |= 0x0002;
            }
            if (Keyboard.IsPressed(MotionKey.P2_DOWN)) //if (Keyboard.IsPressed(Corekey.Down))
            {
                short2 &= ~0x0004;
            }
            else
            {
                short2 |= 0x0004;
            }
            if (Keyboard.IsPressed(MotionKey.P2_UP))//if (Keyboard.IsPressed(Corekey.Up))
            {
                short2 &= ~0x0008;
            }
            else
            {
                short2 |= 0x0008;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_1))//if (Keyboard.IsPressed(Corekey.NumPad1))
            {
                short2 &= ~0x0100;
            }
            else
            {
                short2 |= 0x0100;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_2))//if (Keyboard.IsPressed(Corekey.NumPad2))
            {
                short2 &= ~0x0200;
            }
            else
            {
                short2 |= 0x0200;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_E))//if (Keyboard.IsPressed(Corekey.NumPad3))
            {
                short2 &= ~0x0400;
            }
            else
            {
                short2 |= 0x0400;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_3))//if (Keyboard.IsPressed(Corekey.NumPad4))
            {
                short2 &= ~0x1000;
            }
            else
            {
                short2 |= 0x1000;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_4))//if (Keyboard.IsPressed(Corekey.NumPad5))
            {
                short2 &= ~0x2000;
            }
            else
            {
                short2 |= 0x2000;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_F))//if (Keyboard.IsPressed(Corekey.NumPad6))
            {
                short2 &= ~0x4000;
            }
            else
            {
                short2 |= 0x4000;
            }
            if (Keyboard.IsPressed(MotionKey.UNKNOW_R)) //if (Keyboard.IsPressed(Corekey.R))
            {
                shorts &= ~0x0004;
            }
            else
            {
                shorts |= 0x0004;
            }
            if (Keyboard.IsPressed(MotionKey.UNKNOW_T)) //if (Keyboard.IsPressed(Corekey.T))
            {
                //sbyte0 &= ~0x40;
            }
            else
            {
                //sbyte0 |= 0x40;
            }
        }
        public static void loop_inputports_sfan()
        {
            if (Keyboard.IsPressed(MotionKey.P1_INSERT_COIN))//if (Keyboard.IsPressed(Corekey.D5))
            {
                shortc &= ~0x0001;
            }
            else
            {
                shortc |= 0x0001;
            }
            if (Keyboard.IsPressed(MotionKey.P2_INSERT_COIN))//if (Keyboard.IsPressed(Corekey.D6))
            {
                shortc &= ~0x0002;
            }
            else
            {
                shortc |= 0x0002;
            }
            if (Keyboard.IsPressed(MotionKey.P1_GAMESTART))//if (Keyboard.IsPressed(Corekey.D1))
            {
                shorts &= ~0x0001;
            }
            else
            {
                shorts |= 0x0001;
            }
            if (Keyboard.IsPressed(MotionKey.P2_GAMESTART))//if (Keyboard.IsPressed(Corekey.D2))
            {
                shorts &= ~0x0002;
            }
            else
            {
                shorts |= 0x0002;
            }
            if (Keyboard.IsPressed(MotionKey.P1_RIGHT))//if (Keyboard.IsPressed(Corekey.D))
            {
                short0 &= ~0x0001;
            }
            else
            {
                short0 |= 0x0001;
            }
            if (Keyboard.IsPressed(MotionKey.P1_LEFT))//if (Keyboard.IsPressed(Corekey.A))
            {
                short0 &= ~0x0002;
            }
            else
            {
                short0 |= 0x0002;
            }
            if (Keyboard.IsPressed(MotionKey.P1_DOWN))//if (Keyboard.IsPressed(Corekey.S))
            {
                short0 &= ~0x0004;
            }
            else
            {
                short0 |= 0x0004;
            }
            if (Keyboard.IsPressed(MotionKey.P1_UP))//if (Keyboard.IsPressed(Corekey.W))
            {
                short0 &= ~0x0008;
            }
            else
            {
                short0 |= 0x0008;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_1))//if (Keyboard.IsPressed(Corekey.J))
            {
                sbyte1 |= 0x01;
            }
            else
            {
                sbyte1 &= ~0x01;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_2))//if (Keyboard.IsPressed(Corekey.K))
            {
                sbyte1 |= 0x02;
            }
            else
            {
                sbyte1 &= ~0x02;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_E))//if (Keyboard.IsPressed(Corekey.L))
            {
                sbyte1 |= 0x04;
            }
            else
            {
                sbyte1 &= ~0x04;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_3))//if (Keyboard.IsPressed(Corekey.U))
            {
                sbyte2 |= 0x01;
            }
            else
            {
                sbyte2 &= ~0x01;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_4))//if (Keyboard.IsPressed(Corekey.I))
            {
                sbyte2 |= 0x02;
            }
            else
            {
                sbyte2 &= ~0x02;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_E))//if (Keyboard.IsPressed(Corekey.O))
            {
                sbyte2 |= 0x04;
            }
            else
            {
                sbyte2 &= ~0x04;
            }
            if (Keyboard.IsPressed(MotionKey.P2_RIGHT))//if (Keyboard.IsPressed(Corekey.Right))
            {
                short0 &= ~0x0100;
            }
            else
            {
                short0 |= 0x0100;
            }
            if (Keyboard.IsPressed(MotionKey.P2_LEFT))//if (Keyboard.IsPressed(Corekey.Left))
            {
                short0 &= ~0x0200;
            }
            else
            {
                short0 |= 0x0200;
            }
            if (Keyboard.IsPressed(MotionKey.P2_DOWN)) //if (Keyboard.IsPressed(Corekey.Down))
            {
                short0 &= ~0x0400;
            }
            else
            {
                short0 |= 0x0400;
            }
            if (Keyboard.IsPressed(MotionKey.P2_UP))//if (Keyboard.IsPressed(Corekey.Up))
            {
                short0 &= ~0x0800;
            }
            else
            {
                short0 |= 0x0800;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_1))//if (Keyboard.IsPressed(Corekey.NumPad1))
            {
                sbyte3 |= 0x01;
            }
            else
            {
                sbyte3 &= ~0x01;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_2))//if (Keyboard.IsPressed(Corekey.NumPad2))
            {
                sbyte3 |= 0x02;
            }
            else
            {
                sbyte3 &= ~0x02;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_E))//if (Keyboard.IsPressed(Corekey.NumPad3))
            {
                sbyte3 |= 0x04;
            }
            else
            {
                sbyte3 &= ~0x04;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_3))//if (Keyboard.IsPressed(Corekey.NumPad4))
            {
                sbyte4 |= 0x01;
            }
            else
            {
                sbyte4 &= ~0x01;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_4))//if (Keyboard.IsPressed(Corekey.NumPad5))
            {
                sbyte4 &= ~0x02;
            }
            else
            {
                sbyte4 |= 0x02;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_F))//if (Keyboard.IsPressed(Corekey.NumPad6))
            {
                sbyte4 |= 0x04;
            }
            else
            {
                sbyte4 &= ~0x04;
            }
            if (Keyboard.IsPressed(MotionKey.UNKNOW_R)) //if (Keyboard.IsPressed(Corekey.R))
            {
                shorts &= ~0x0004;
            }
            else
            {
                shorts |= 0x0004;
            }
            if (Keyboard.IsPressed(MotionKey.UNKNOW_T)) //if (Keyboard.IsPressed(Corekey.T))
            {
                //sbyte0 &= ~0x40;
            }
            else
            {
                //sbyte0 |= 0x40;
            }
        }
        public static void record_port_gng()
        {
            if (bytes != bytes_old || byte1 != byte1_old || byte2 != byte2_old)
            {
                bytes_old = bytes;
                byte1_old = byte1;
                byte2_old = byte2;
                Mame.bwRecord.Write(Video.screenstate.frame_number);
                Mame.bwRecord.Write(bytes);
                Mame.bwRecord.Write(byte1);
                Mame.bwRecord.Write(byte2);
            }
        }
        public static void replay_port_gng()
        {
            if (Inptport.bReplayRead)
            {
                try
                {
                    Video.frame_number_obj = Mame.brRecord.ReadInt64();
                    bytes_old = Mame.brRecord.ReadByte();
                    byte1_old = Mame.brRecord.ReadByte();
                    byte2_old = Mame.brRecord.ReadByte();
                }
                catch
                {
                    Mame.playState = Mame.PlayState.PLAY_REPLAYEND;
                }
                Inptport.bReplayRead = false;
            }
            if (Video.screenstate.frame_number == Video.frame_number_obj)
            {
                bytes = bytes_old;
                byte1 = byte1_old;
                byte2 = byte2_old;
                Inptport.bReplayRead = true;
            }
            else
            {
                Inptport.bReplayRead = false;
            }
        }
        public static void record_port_sf()
        {
            if (short0 != short0_old || short1 != short1_old || short2 != short2_old || shorts != shorts_old || shortc != shortc_old || sbyte1 != sbyte1_old || sbyte2 != sbyte2_old || sbyte3 != sbyte3_old || sbyte4 != sbyte4_old)
            {
                short0_old = short0;
                short1_old = short1;
                short2_old = short2;
                shorts_old = shorts;
                shortc_old = shortc;
                sbyte1_old = sbyte1;
                sbyte2_old = sbyte2;
                sbyte3_old = sbyte3;
                sbyte4_old = sbyte4;
                Mame.bwRecord.Write(Video.screenstate.frame_number);
                Mame.bwRecord.Write(short0);
                Mame.bwRecord.Write(short1);
                Mame.bwRecord.Write(short2);
                Mame.bwRecord.Write(shorts);
                Mame.bwRecord.Write(shortc);
                Mame.bwRecord.Write(sbyte1);
                Mame.bwRecord.Write(sbyte2);
                Mame.bwRecord.Write(sbyte3);
                Mame.bwRecord.Write(sbyte4);
            }
        }
        public static void replay_port_sf()
        {
            if (Inptport.bReplayRead)
            {
                try
                {
                    Video.frame_number_obj = Mame.brRecord.ReadInt64();
                    short0_old = Mame.brRecord.ReadInt16();
                    short1_old = Mame.brRecord.ReadInt16();
                    short2_old = Mame.brRecord.ReadInt16();
                    shorts_old = Mame.brRecord.ReadInt16();
                    shortc_old = Mame.brRecord.ReadInt16();
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
                short0 = short0_old;
                short1 = short1_old;
                short2 = short2_old;
                shorts = shorts_old;
                shortc = shortc_old;
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
    }
}
