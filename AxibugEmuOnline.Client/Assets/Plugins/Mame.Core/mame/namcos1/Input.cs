using MAME.Core;

namespace MAME.Core
{
    public partial class Namcos1
    {
        public static void loop_inputports_ns1_3b()
        {
            if (Keyboard.IsPressed(MotionKey.P1_INSERT_COIN))//if (Keyboard.IsPressed(Corekey.D5))
            {
                byte2 &= unchecked((byte)~0x10);
            }
            else
            {
                byte2 |= 0x10;
            }
            if (Keyboard.IsPressed(MotionKey.P2_INSERT_COIN))//if (Keyboard.IsPressed(Corekey.D6))
            {
                byte2 &= unchecked((byte)~0x08);
            }
            else
            {
                byte2 |= 0x08;
            }
            if (Keyboard.IsPressed(MotionKey.P1_GAMESTART))//if (Keyboard.IsPressed(Corekey.D1))
            {
                byte0 &= unchecked((byte)~0x80);
            }
            else
            {
                byte0 |= 0x80;
            }
            if (Keyboard.IsPressed(MotionKey.P2_GAMESTART))//if (Keyboard.IsPressed(Corekey.D2))
            {
                byte1 &= unchecked((byte)~0x80);
            }
            else
            {
                byte1 |= 0x80;
            }
            if (Keyboard.IsPressed(MotionKey.P1_RIGHT))//if (Keyboard.IsPressed(Corekey.D))
            {
                byte0 &= unchecked((byte)~0x01);
            }
            else
            {
                byte0 |= 0x01;
            }
            if (Keyboard.IsPressed(MotionKey.P1_LEFT))//if (Keyboard.IsPressed(Corekey.A))
            {
                byte0 &= unchecked((byte)~0x02);
            }
            else
            {
                byte0 |= 0x02;
            }
            if (Keyboard.IsPressed(MotionKey.P1_DOWN))//if (Keyboard.IsPressed(Corekey.S))
            {
                byte0 &= unchecked((byte)~0x04);
            }
            else
            {
                byte0 |= 0x04;
            }
            if (Keyboard.IsPressed(MotionKey.P1_UP))//if (Keyboard.IsPressed(Corekey.W))
            {
                byte0 &= unchecked((byte)~0x08);
            }
            else
            {
                byte0 |= 0x08;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_1))//if (Keyboard.IsPressed(Corekey.J))
            {
                byte0 &= unchecked((byte)~0x10);
            }
            else
            {
                byte0 |= 0x10;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_2))//if (Keyboard.IsPressed(Corekey.K))
            {
                byte0 &= unchecked((byte)~0x20);
            }
            else
            {
                byte0 |= 0x20;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_E))//if (Keyboard.IsPressed(Corekey.L))
            {
                byte0 &= unchecked((byte)~0x40);
            }
            else
            {
                byte0 |= 0x40;
            }
            if (Keyboard.IsPressed(MotionKey.P2_RIGHT))//if (Keyboard.IsPressed(Corekey.Right))
            {
                byte1 &= unchecked((byte)~0x01);
            }
            else
            {
                byte1 |= 0x01;
            }
            if (Keyboard.IsPressed(MotionKey.P2_LEFT))//if (Keyboard.IsPressed(Corekey.Left))
            {
                byte1 &= unchecked((byte)~0x02);
            }
            else
            {
                byte1 |= 0x02;
            }
            if (Keyboard.IsPressed(MotionKey.P2_DOWN)) //if (Keyboard.IsPressed(Corekey.Down))
            {
                byte1 &= unchecked((byte)~0x04);
            }
            else
            {
                byte1 |= 0x04;
            }
            if (Keyboard.IsPressed(MotionKey.P2_UP))//if (Keyboard.IsPressed(Corekey.Up))
            {
                byte1 &= unchecked((byte)~0x08);
            }
            else
            {
                byte1 |= 0x08;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_1))//if (Keyboard.IsPressed(Corekey.NumPad1))
            {
                byte1 &= unchecked((byte)~0x10);
            }
            else
            {
                byte1 |= 0x10;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_2))//if (Keyboard.IsPressed(Corekey.NumPad2))
            {
                byte1 &= unchecked((byte)~0x20);
            }
            else
            {
                byte1 |= 0x20;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_E))//if (Keyboard.IsPressed(Corekey.NumPad3))
            {
                byte1 &= unchecked((byte)~0x40);
            }
            else
            {
                byte1 |= 0x40;
            }
            if (Keyboard.IsPressed(MotionKey.UNKNOW_R)) //if (Keyboard.IsPressed(Corekey.R))
            {
                byte2 &= unchecked((byte)~0x20);
            }
            else
            {
                byte2 |= 0x20;
            }
            if (Keyboard.IsPressed(MotionKey.UNKNOW_T)) //if (Keyboard.IsPressed(Corekey.T))
            {
                byte2 &= unchecked((byte)~0x40);
            }
            else
            {
                byte2 |= 0x40;
            }
        }
        public static void loop_inputports_ns1_quester()
        {
            if (Keyboard.IsPressed(MotionKey.P1_INSERT_COIN))//if (Keyboard.IsPressed(Corekey.D5))
            {
                byte2 &= unchecked((byte)~0x10);
            }
            else
            {
                byte2 |= 0x10;
            }
            if (Keyboard.IsPressed(MotionKey.P2_INSERT_COIN))//if (Keyboard.IsPressed(Corekey.D6))
            {
                byte2 &= unchecked((byte)~0x08);
            }
            else
            {
                byte2 |= 0x08;
            }
            if (Keyboard.IsPressed(MotionKey.P1_GAMESTART))//if (Keyboard.IsPressed(Corekey.D1))
            {
                byte0 &= unchecked((byte)~0x80);
            }
            else
            {
                byte0 |= 0x80;
            }
            if (Keyboard.IsPressed(MotionKey.P2_GAMESTART))//if (Keyboard.IsPressed(Corekey.D2))
            {
                byte1 &= unchecked((byte)~0x80);
            }
            else
            {
                byte1 |= 0x80;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_1))//if (Keyboard.IsPressed(Corekey.J))
            {
                byte0 &= unchecked((byte)~0x10);
            }
            else
            {
                byte0 |= 0x10;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_2))//if (Keyboard.IsPressed(Corekey.K))
            {
                byte0 &= unchecked((byte)~0x20);
            }
            else
            {
                byte0 |= 0x20;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_E))//if (Keyboard.IsPressed(Corekey.L))
            {
                byte0 &= unchecked((byte)~0x40);
            }
            else
            {
                byte0 |= 0x40;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_1))//if (Keyboard.IsPressed(Corekey.NumPad1))
            {
                byte1 &= unchecked((byte)~0x10);
            }
            else
            {
                byte1 |= 0x10;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_2))//if (Keyboard.IsPressed(Corekey.NumPad2))
            {
                byte1 &= unchecked((byte)~0x20);
            }
            else
            {
                byte1 |= 0x20;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_E))//if (Keyboard.IsPressed(Corekey.NumPad3))
            {
                byte1 &= unchecked((byte)~0x40);
            }
            else
            {
                byte1 |= 0x40;
            }
            if (Keyboard.IsPressed(MotionKey.UNKNOW_R)) //if (Keyboard.IsPressed(Corekey.R))
            {
                byte2 &= unchecked((byte)~0x20);
            }
            else
            {
                byte2 |= 0x20;
            }
            if (Keyboard.IsPressed(MotionKey.UNKNOW_T)) //if (Keyboard.IsPressed(Corekey.T))
            {
                byte2 &= unchecked((byte)~0x40);
            }
            else
            {
                byte2 |= 0x40;
            }
            Inptport.frame_update_analog_field_quester_p0(Inptport.analog_p0);
            Inptport.frame_update_analog_field_quester_p1(Inptport.analog_p1);
        }
        public static void loop_inputports_ns1_berabohm()
        {
            if (Keyboard.IsPressed(MotionKey.P1_INSERT_COIN))//if (Keyboard.IsPressed(Corekey.D5))
            {
                byte2 &= unchecked((byte)~0x10);
            }
            else
            {
                byte2 |= 0x10;
            }
            if (Keyboard.IsPressed(MotionKey.P2_INSERT_COIN))//if (Keyboard.IsPressed(Corekey.D6))
            {
                byte2 &= unchecked((byte)~0x08);
            }
            else
            {
                byte2 |= 0x08;
            }
            if (Keyboard.IsPressed(MotionKey.P1_GAMESTART))//if (Keyboard.IsPressed(Corekey.D1))
            {
                byte0 &= unchecked((byte)~0x80);
            }
            else
            {
                byte0 |= 0x80;
            }
            if (Keyboard.IsPressed(MotionKey.P2_GAMESTART))//if (Keyboard.IsPressed(Corekey.D2))
            {
                byte1 &= unchecked((byte)~0x80);
            }
            else
            {
                byte1 |= 0x80;
            }
            if (Keyboard.IsPressed(MotionKey.P1_RIGHT))//if (Keyboard.IsPressed(Corekey.D))
            {
                byte0 &= unchecked((byte)~0x01);
            }
            else
            {
                byte0 |= 0x01;
            }
            if (Keyboard.IsPressed(MotionKey.P1_LEFT))//if (Keyboard.IsPressed(Corekey.A))
            {
                byte0 &= unchecked((byte)~0x02);
            }
            else
            {
                byte0 |= 0x02;
            }
            if (Keyboard.IsPressed(MotionKey.P1_DOWN))//if (Keyboard.IsPressed(Corekey.S))
            {
                byte0 &= unchecked((byte)~0x04);
            }
            else
            {
                byte0 |= 0x04;
            }
            if (Keyboard.IsPressed(MotionKey.P1_UP))//if (Keyboard.IsPressed(Corekey.W))
            {
                byte0 &= unchecked((byte)~0x08);
            }
            else
            {
                byte0 |= 0x08;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_1))//if (Keyboard.IsPressed(Corekey.J))
            {
                byte01 |= 0x01;
            }
            else
            {
                byte01 &= unchecked((byte)~0x01);
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_2))//if (Keyboard.IsPressed(Corekey.K))
            {
                byte01 |= 0x02;
            }
            else
            {
                byte01 &= unchecked((byte)~0x02);
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_E))//if (Keyboard.IsPressed(Corekey.L))
            {
                byte01 |= 0x04;
            }
            else
            {
                byte01 &= unchecked((byte)~0x04);
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_3))//if (Keyboard.IsPressed(Corekey.U))
            {
                byte00 |= 0x01;
            }
            else
            {
                byte00 &= unchecked((byte)~0x01);
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_4))//if (Keyboard.IsPressed(Corekey.I))
            {
                byte00 |= 0x02;
            }
            else
            {
                byte00 &= unchecked((byte)~0x02);
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_E))//if (Keyboard.IsPressed(Corekey.O))
            {
                byte00 |= 0x04;
            }
            else
            {
                byte00 &= unchecked((byte)~0x04);
            }
            if (Keyboard.IsPressed(MotionKey.P2_RIGHT))//if (Keyboard.IsPressed(Corekey.Right))
            {
                byte1 &= unchecked((byte)~0x01);
            }
            else
            {
                byte1 |= 0x01;
            }
            if (Keyboard.IsPressed(MotionKey.P2_LEFT))//if (Keyboard.IsPressed(Corekey.Left))
            {
                byte1 &= unchecked((byte)~0x02);
            }
            else
            {
                byte1 |= 0x02;
            }
            if (Keyboard.IsPressed(MotionKey.P2_DOWN)) //if (Keyboard.IsPressed(Corekey.Down))
            {
                byte1 &= unchecked((byte)~0x04);
            }
            else
            {
                byte1 |= 0x04;
            }
            if (Keyboard.IsPressed(MotionKey.P2_UP))//if (Keyboard.IsPressed(Corekey.Up))
            {
                byte1 &= unchecked((byte)~0x08);
            }
            else
            {
                byte1 |= 0x08;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_1))//if (Keyboard.IsPressed(Corekey.NumPad1))
            {
                byte03 |= 0x01;
            }
            else
            {
                byte03 &= unchecked((byte)~0x01);
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_2))//if (Keyboard.IsPressed(Corekey.NumPad2))
            {
                byte03 |= 0x02;
            }
            else
            {
                byte03 &= unchecked((byte)~0x02);
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_E))//if (Keyboard.IsPressed(Corekey.NumPad3))
            {
                byte03 |= 0x04;
            }
            else
            {
                byte03 &= unchecked((byte)~0x04);
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_3))//if (Keyboard.IsPressed(Corekey.NumPad4))
            {
                byte02 |= 0x01;
            }
            else
            {
                byte02 &= unchecked((byte)~0x01);
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_4))//if (Keyboard.IsPressed(Corekey.NumPad5))
            {
                byte02 |= 0x02;
            }
            else
            {
                byte02 &= unchecked((byte)~0x02);
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_F))//if (Keyboard.IsPressed(Corekey.NumPad6))
            {
                byte02 |= 0x04;
            }
            else
            {
                byte02 &= unchecked((byte)~0x04);
            }
            if (Keyboard.IsPressed(MotionKey.UNKNOW_R)) //if (Keyboard.IsPressed(Corekey.R))
            {
                byte2 &= unchecked((byte)~0x20);
            }
            else
            {
                byte2 |= 0x20;
            }
            if (Keyboard.IsPressed(MotionKey.UNKNOW_T)) //if (Keyboard.IsPressed(Corekey.T))
            {
                byte2 &= unchecked((byte)~0x40);
            }
            else
            {
                byte2 |= 0x40;
            }
        }
        public static void loop_inputports_ns1_faceoff()
        {
            if (Keyboard.IsPressed(MotionKey.P1_INSERT_COIN))//if (Keyboard.IsPressed(Corekey.D5))
            {
                byte2 &= unchecked((byte)~0x10);
            }
            else
            {
                byte2 |= 0x10;
            }
            if (Keyboard.IsPressed(MotionKey.P2_INSERT_COIN))//if (Keyboard.IsPressed(Corekey.D6))
            {
                byte2 &= unchecked((byte)~0x08);
            }
            else
            {
                byte2 |= 0x08;
            }
            if (Keyboard.IsPressed(MotionKey.P1_GAMESTART))//if (Keyboard.IsPressed(Corekey.D1))
            {
                byte0 &= unchecked((byte)~0x80);
            }
            else
            {
                byte0 |= 0x80;
            }
            if (Keyboard.IsPressed(MotionKey.P2_GAMESTART))//if (Keyboard.IsPressed(Corekey.D2))
            {
                byte1 &= unchecked((byte)~0x80);
            }
            else
            {
                byte1 |= 0x80;
            }
            if (Keyboard.IsPressed(MotionKey.P1_RIGHT))//if (Keyboard.IsPressed(Corekey.D))
            {
                byte00 &= unchecked((byte)~0x01);
            }
            else
            {
                byte00 |= 0x01;
            }
            if (Keyboard.IsPressed(MotionKey.P1_LEFT))//if (Keyboard.IsPressed(Corekey.A))
            {
                byte00 &= unchecked((byte)~0x02);
            }
            else
            {
                byte00 |= 0x02;
            }
            if (Keyboard.IsPressed(MotionKey.P1_DOWN))//if (Keyboard.IsPressed(Corekey.S))
            {
                byte00 &= unchecked((byte)~0x04);
            }
            else
            {
                byte00 |= 0x04;
            }
            if (Keyboard.IsPressed(MotionKey.P1_UP))//if (Keyboard.IsPressed(Corekey.W))
            {
                byte00 &= unchecked((byte)~0x08);
            }
            else
            {
                byte00 |= 0x08;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_1))//if (Keyboard.IsPressed(Corekey.J))
            {
                byte00 &= unchecked((byte)~0x10);
            }
            else
            {
                byte00 |= 0x10;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_2))//if (Keyboard.IsPressed(Corekey.K))
            {
                byte01 &= unchecked((byte)~0x10);
            }
            else
            {
                byte01 |= 0x10;
            }
            if (Keyboard.IsPressed(MotionKey.P2_RIGHT))//if (Keyboard.IsPressed(Corekey.Right))
            {
                byte01 &= unchecked((byte)~0x01);
            }
            else
            {
                byte01 |= 0x01;
            }
            if (Keyboard.IsPressed(MotionKey.P2_LEFT))//if (Keyboard.IsPressed(Corekey.Left))
            {
                byte01 &= unchecked((byte)~0x02);
            }
            else
            {
                byte01 |= 0x02;
            }
            if (Keyboard.IsPressed(MotionKey.P2_DOWN)) //if (Keyboard.IsPressed(Corekey.Down))
            {
                byte01 &= unchecked((byte)~0x04);
            }
            else
            {
                byte01 |= 0x04;
            }
            if (Keyboard.IsPressed(MotionKey.P2_UP))//if (Keyboard.IsPressed(Corekey.Up))
            {
                byte01 &= unchecked((byte)~0x08);
            }
            else
            {
                byte01 |= 0x08;
            }
            if (Keyboard.IsPressed(MotionKey.UNKNOW_R)) //if (Keyboard.IsPressed(Corekey.R))
            {
                byte2 &= unchecked((byte)~0x20);
            }
            else
            {
                byte2 |= 0x20;
            }
            if (Keyboard.IsPressed(MotionKey.UNKNOW_T)) //if (Keyboard.IsPressed(Corekey.T))
            {
                byte2 &= unchecked((byte)~0x40);
            }
            else
            {
                byte2 |= 0x40;
            }
        }
        public static void loop_inputports_ns1_tankfrce4()
        {
            if (Keyboard.IsPressed(MotionKey.P1_INSERT_COIN))//if (Keyboard.IsPressed(Corekey.D5))
            {
                byte2 &= unchecked((byte)~0x10);
            }
            else
            {
                byte2 |= 0x10;
            }
            if (Keyboard.IsPressed(MotionKey.P2_INSERT_COIN))//if (Keyboard.IsPressed(Corekey.D6))
            {
                byte2 &= unchecked((byte)~0x08);
            }
            else
            {
                byte2 |= 0x08;
            }
            if (Keyboard.IsPressed(MotionKey.P1_RIGHT))//if (Keyboard.IsPressed(Corekey.D))
            {
                byte00 &= unchecked((byte)~0x01);
            }
            else
            {
                byte00 |= 0x01;
            }
            if (Keyboard.IsPressed(MotionKey.P1_LEFT))//if (Keyboard.IsPressed(Corekey.A))
            {
                byte00 &= unchecked((byte)~0x02);
            }
            else
            {
                byte00 |= 0x02;
            }
            if (Keyboard.IsPressed(MotionKey.P1_DOWN))//if (Keyboard.IsPressed(Corekey.S))
            {
                byte00 &= unchecked((byte)~0x04);
            }
            else
            {
                byte00 |= 0x04;
            }
            if (Keyboard.IsPressed(MotionKey.P1_UP))//if (Keyboard.IsPressed(Corekey.W))
            {
                byte00 &= unchecked((byte)~0x08);
            }
            else
            {
                byte00 |= 0x08;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_1))//if (Keyboard.IsPressed(Corekey.J))
            {
                byte00 &= unchecked((byte)~0x10);
            }
            else
            {
                byte00 |= 0x10;
            }
            if (Keyboard.IsPressed(MotionKey.P2_RIGHT))//if (Keyboard.IsPressed(Corekey.Right))
            {
                byte02 &= unchecked((byte)~0x01);
            }
            else
            {
                byte02 |= 0x01;
            }
            if (Keyboard.IsPressed(MotionKey.P2_LEFT))//if (Keyboard.IsPressed(Corekey.Left))
            {
                byte02 &= unchecked((byte)~0x02);
            }
            else
            {
                byte02 |= 0x02;
            }
            if (Keyboard.IsPressed(MotionKey.P2_DOWN)) //if (Keyboard.IsPressed(Corekey.Down))
            {
                byte02 &= unchecked((byte)~0x04);
            }
            else
            {
                byte02 |= 0x04;
            }
            if (Keyboard.IsPressed(MotionKey.P2_UP))//if (Keyboard.IsPressed(Corekey.Up))
            {
                byte02 &= unchecked((byte)~0x08);
            }
            else
            {
                byte02 |= 0x08;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_1))//if (Keyboard.IsPressed(Corekey.NumPad1))
            {
                byte02 &= unchecked((byte)~0x10);
            }
            else
            {
                byte02 |= 0x10;
            }
            /*if (Keyboard.IsPressed(Key.R))
            {
                byte2 &= unchecked((byte)~0x20);
            }
            else
            {
                byte2 |= 0x20;
            }
            if (Keyboard.IsPressed(Key.T))
            {
                byte2 &= unchecked((byte)~0x40);
            }
            else
            {
                byte2 |= 0x40;
            }*/
        }
        public static void record_port()
        {
            if (byte0 != byte0_old || byte1 != byte1_old || byte2 != byte2_old)
            {
                byte0_old = byte0;
                byte1_old = byte1;
                byte2_old = byte2;
                Mame.bwRecord.Write(Video.screenstate.frame_number);
                Mame.bwRecord.Write(byte0);
                Mame.bwRecord.Write(byte1);
                Mame.bwRecord.Write(byte2);
            }
        }
        public static void replay_port()
        {
            if (Inptport.bReplayRead)
            {
                try
                {
                    Video.frame_number_obj = Mame.brRecord.ReadInt64();
                    byte0_old = Mame.brRecord.ReadByte();
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
                byte0 = byte0_old;
                byte1 = byte1_old;
                byte2 = byte2_old;
                Inptport.bReplayRead = true;
            }
            else
            {
                Inptport.bReplayRead = false;
            }
        }
    }
}
