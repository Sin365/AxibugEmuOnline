using MAME.Core;

namespace MAME.Core
{
    public partial class IGS011
    {
        public static void loop_inputports_igs011_drgnwrld()
        {
            if (Keyboard.IsPressed(MotionKey.P1_INSERT_COIN))//if (Keyboard.IsPressed(Corekey.D5))
            {
                sbytec &= ~0x01;
            }
            else
            {
                sbytec |= 0x01;
            }
            if (Keyboard.IsPressed(MotionKey.P2_INSERT_COIN))//if (Keyboard.IsPressed(Corekey.D6))
            {
                sbytec &= ~0x02;
            }
            else
            {
                sbytec |= 0x02;
            }
            if (Keyboard.IsPressed(MotionKey.P1_GAMESTART))//if (Keyboard.IsPressed(Corekey.D1))
            {
                sbyte0 &= ~0x01;
            }
            else
            {
                sbyte0 |= 0x01;
            }
            if (Keyboard.IsPressed(MotionKey.P2_GAMESTART))//if (Keyboard.IsPressed(Corekey.D2))
            {
                sbyte2 &= ~0x10;
            }
            else
            {
                sbyte2 |= 0x10;
            }
            //if (Keyboard.IsPressed(Corekey.D))// || Mouse.deltaX > 0)
            if (Keyboard.IsPressed(MotionKey.P1_RIGHT))// || Mouse.deltaX > 0)
            {
                sbyte0 &= ~0x10;
            }
            else
            {
                sbyte0 |= 0x10;
            }
            if (Keyboard.IsPressed(MotionKey.P1_LEFT))//if (Keyboard.IsPressed(Corekey.A))// || Mouse.deltaX < 0)
            {
                sbyte2 &= ~0x02;
            }
            else
            {
                sbyte2 |= 0x02;
            }
            if (Keyboard.IsPressed(MotionKey.P1_DOWN))//if (Keyboard.IsPressed(Corekey.S))// || Mouse.deltaY > 0)
            {
                sbyte0 &= ~0x04;
            }
            else
            {
                sbyte0 |= 0x04;
            }
            if (Keyboard.IsPressed(MotionKey.P1_UP))//if (Keyboard.IsPressed(Corekey.W))// || Mouse.deltaY < 0)
            {
                sbyte2 &= ~0x01;
            }
            else
            {
                sbyte2 |= 0x01;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_1))//if (Keyboard.IsPressed(Corekey.J))// || Mouse.buttons[0] != 0)
            {
                sbyte2 &= ~0x04;
            }
            else
            {
                sbyte2 |= 0x04;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_2))//if (Keyboard.IsPressed(Corekey.K))// || Mouse.buttons[1] != 0)
            {
                sbyte0 &= ~0x40;
            }
            else
            {
                sbyte0 |= 0x40;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_E))//if (Keyboard.IsPressed(Corekey.L))
            {
                sbyte2 &= ~0x08;
            }
            else
            {
                sbyte2 |= 0x08;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_3))//if (Keyboard.IsPressed(Corekey.U))
            {

            }
            else
            {

            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_4))//if (Keyboard.IsPressed(Corekey.I))
            {

            }
            else
            {

            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_E))//if (Keyboard.IsPressed(Corekey.O))
            {

            }
            else
            {

            }
            if (Keyboard.IsPressed(MotionKey.P2_RIGHT))//if (Keyboard.IsPressed(Corekey.Right))
            {
                sbyte2 &= ~0x40;
            }
            else
            {
                sbyte2 |= 0x40;
            }
            if (Keyboard.IsPressed(MotionKey.P2_LEFT))//if (Keyboard.IsPressed(Corekey.Left))
            {
                sbyte1 &= ~0x08;
            }
            else
            {
                sbyte1 |= 0x08;
            }
            if (Keyboard.IsPressed(MotionKey.P2_DOWN)) //if (Keyboard.IsPressed(Corekey.Down))
            {
                sbyte2 &= ~0x20;
            }
            else
            {
                sbyte2 |= 0x20;
            }
            if (Keyboard.IsPressed(MotionKey.P2_UP))//if (Keyboard.IsPressed(Corekey.Up))
            {
                sbyte1 &= ~0x02;
            }
            else
            {
                sbyte1 |= 0x02;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_1))//if (Keyboard.IsPressed(Corekey.NumPad1))
            {
                sbyte1 &= ~0x20;
            }
            else
            {
                sbyte1 |= 0x20;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_2))//if (Keyboard.IsPressed(Corekey.NumPad2))
            {
                sbyte2 &= unchecked((sbyte)~0x80);
            }
            else
            {
                sbyte2 |= unchecked((sbyte)0x80);
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_E))//if (Keyboard.IsPressed(Corekey.NumPad3))
            {
                sbyte1 &= unchecked((sbyte)~0x80);
            }
            else
            {
                sbyte1 |= unchecked((sbyte)0x80);
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_3))//if (Keyboard.IsPressed(Corekey.NumPad4))
            {

            }
            else
            {

            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_4))//if (Keyboard.IsPressed(Corekey.NumPad5))
            {

            }
            else
            {

            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_F))//if (Keyboard.IsPressed(Corekey.NumPad6))
            {

            }
            else
            {

            }
            if (Keyboard.IsPressed(MotionKey.UNKNOW_R)) //if (Keyboard.IsPressed(Corekey.R))
            {
                sbytec &= ~0x08;
            }
            else
            {
                sbytec |= 0x08;
            }
            if (Keyboard.IsPressed(MotionKey.UNKNOW_T)) //if (Keyboard.IsPressed(Corekey.T))
            {
                sbytec &= ~0x10;
            }
            else
            {
                sbytec |= 0x10;
            }
        }
        public static void loop_inputports_igs011_lhb()
        {
            /*
            if (Keyboard.IsPressed(MotionKey.P1_INSERT_COIN))//if (Keyboard.IsPressed(Corekey.D5))
            {
                sbytec &= ~0x10;
            }
            else
            {
                sbytec |= 0x10;
            }
            if (Keyboard.IsPressed(MotionKey.P1_GAMESTART))//if (Keyboard.IsPressed(Corekey.D1))
            {
                bkey0 &= unchecked((byte)~0x20);
            }
            else
            {
                bkey0 |= 0x20;
            }
            if (Keyboard.IsPressed(MotionKey.P2_GAMESTART))//if (Keyboard.IsPressed(Corekey.D2))
            {
                bkey1 &= unchecked((byte)~0x20);
            }
            else
            {
                bkey1 |= 0x20;
            }
            if (Keyboard.IsPressed(Corekey.D3))
            {
                bkey4 &= unchecked((byte)~0x10);
            }
            else
            {
                bkey4 |= 0x10;
            }
            if (Keyboard.IsPressed(Corekey.D4))
            {
                bkey4 &= unchecked((byte)~0x20);
            }
            else
            {
                bkey4 |= 0x20;
            }
            if (Keyboard.IsPressed(MotionKey.P1_LEFT))//if (Keyboard.IsPressed(Corekey.A))
            {
                bkey0 &= unchecked((byte)~0x01);
            }
            else
            {
                bkey0 |= 0x01;
            }
            if (Keyboard.IsPressed(Corekey.B))
            {
                bkey1 &= unchecked((byte)~0x01);
            }
            else
            {
                bkey1 |= 0x01;
            }
            if (Keyboard.IsPressed(Corekey.C))
            {
                bkey2 &= unchecked((byte)~0x01);
            }
            else
            {
                bkey2 |= 0x01;
            }
            if (Keyboard.IsPressed(MotionKey.P1_RIGHT))//if (Keyboard.IsPressed(Corekey.D))
            {
                bkey3 &= unchecked((byte)~0x01);
            }
            else
            {
                bkey3 |= 0x01;
            }
            if (Keyboard.IsPressed(Corekey.E))
            {
                bkey0 &= unchecked((byte)~0x02);
            }
            else
            {
                bkey0 |= 0x02;
            }
            if (Keyboard.IsPressed(Corekey.F))
            {
                bkey1 &= unchecked((byte)~0x02);
            }
            else
            {
                bkey1 |= 0x02;
            }
            if (Keyboard.IsPressed(Corekey.G))
            {
                bkey2 &= unchecked((byte)~0x02);
            }
            else
            {
                bkey2 |= 0x02;
            }
            if (Keyboard.IsPressed(Corekey.H))
            {
                bkey3 &= unchecked((byte)~0x02);
            }
            else
            {
                bkey3 |= 0x02;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_4))//if (Keyboard.IsPressed(Corekey.I))
            {
                bkey0 &= unchecked((byte)~0x04);
            }
            else
            {
                bkey0 |= 0x04;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_1))//if (Keyboard.IsPressed(Corekey.J))
            {
                bkey1 &= unchecked((byte)~0x04);
            }
            else
            {
                bkey1 |= 0x04;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_2))//if (Keyboard.IsPressed(Corekey.K))
            {
                bkey2 &= unchecked((byte)~0x04);
            }
            else
            {
                bkey2 |= 0x04;
            }
            if (Keyboard.IsPressed(MotionKey.P1_UNKNOW_E))//if (Keyboard.IsPressed(Corekey.L))
            {
                bkey3 &= unchecked((byte)~0x04);
            }
            else
            {
                bkey3 |= 0x04;
            }
            if (Keyboard.IsPressed(MotionKey.UNKNOW_M))//if (Keyboard.IsPressed(Corekey.M))
            {
                bkey0 &= unchecked((byte)~0x08);
            }
            else
            {
                bkey0 |= 0x08;
            }
            if (Keyboard.IsPressed(MotionKey.UNKNOW_N))//if (Keyboard.IsPressed(Corekey.N))
            {
                bkey1 &= unchecked((byte)~0x08);
            }
            else
            {
                bkey1 |= 0x08;
            }
            if (Keyboard.IsPressed(MotionKey.P1_UNKNOW_E))//if (Keyboard.IsPressed(Corekey.O))
            {
                bkey4 &= unchecked((byte)~0x04);
            }
            else
            {
                bkey4 |= 0x04;
            }
            if (Keyboard.IsPressed(MotionKey.UNKNOW_Q))//if (Keyboard.IsPressed(Corekey.Q))
            {
                bkey0 &= unchecked((byte)~0x10);
            }
            else
            {
                bkey0 |= 0x10;
            }
            if (Keyboard.IsPressed(MotionKey.UNKNOW_R)) //if (Keyboard.IsPressed(Corekey.R))
            {
                bkey1 &= unchecked((byte)~0x10);
            }
            else
            {
                bkey1 |= 0x10;
            }
            if (Keyboard.IsPressed(MotionKey.UNKNOW_T)) //if (Keyboard.IsPressed(Corekey.T))
            {
                bkey2 &= unchecked((byte)~0x08);
            }
            else
            {
                bkey2 |= 0x08;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_3))//if (Keyboard.IsPressed(Corekey.U))
            {
                bkey4 &= unchecked((byte)~0x02);
            }
            else
            {
                bkey4 |= 0x02;
            }
            if (Keyboard.IsPressed(MotionKey.P1_UP))//if (Keyboard.IsPressed(Corekey.W))
            {
                bkey3 &= unchecked((byte)~0x08);
            }
            else
            {
                bkey3 |= 0x08;
            }
            if (Keyboard.IsPressed(Corekey.Y))
            {
                bkey4 &= unchecked((byte)~0x01);
            }
            else
            {
                bkey4 |= 0x01;
            }
            if (Keyboard.IsPressed(Corekey.Z))
            {
                bkey2 &= unchecked((byte)~0x10);
            }
            else
            {
                bkey2 |= 0x10;
            }
            */
        }
        public static void loop_inputports_igs011_lhb2()
        {
            /*
            if (Keyboard.IsPressed(MotionKey.P1_INSERT_COIN))//if (Keyboard.IsPressed(Corekey.D5))
            {
                sbytec &= ~0x10;
            }
            else
            {
                sbytec |= 0x10;
            }
            if (Keyboard.IsPressed(MotionKey.P1_GAMESTART))//if (Keyboard.IsPressed(Corekey.D1))
            {
                bkey0 &= unchecked((byte)~0x20);
            }
            else
            {
                bkey0 |= 0x20;
            }
            if (Keyboard.IsPressed(MotionKey.P2_GAMESTART))//if (Keyboard.IsPressed(Corekey.D2))
            {
                bkey1 &= unchecked((byte)~0x20);
            }
            else
            {
                bkey1 |= 0x20;
            }
            if (Keyboard.IsPressed(MotionKey.P1_LEFT))//if (Keyboard.IsPressed(Corekey.A))
            {
                bkey0 &= unchecked((byte)~0x01);
            }
            else
            {
                bkey0 |= 0x01;
            }
            if (Keyboard.IsPressed(Corekey.B))
            {
                bkey1 &= unchecked((byte)~0x01);
            }
            else
            {
                bkey1 |= 0x01;
            }
            if (Keyboard.IsPressed(Corekey.C))
            {
                bkey2 &= unchecked((byte)~0x01);
            }
            else
            {
                bkey2 |= 0x01;
            }
            if (Keyboard.IsPressed(MotionKey.P1_RIGHT))//if (Keyboard.IsPressed(Corekey.D))
            {
                bkey3 &= unchecked((byte)~0x01);
            }
            else
            {
                bkey3 |= 0x01;
            }
            if (Keyboard.IsPressed(Corekey.E))
            {
                bkey0 &= unchecked((byte)~0x02);
            }
            else
            {
                bkey0 |= 0x02;
            }
            if (Keyboard.IsPressed(Corekey.F))
            {
                bkey1 &= unchecked((byte)~0x02);
            }
            else
            {
                bkey1 |= 0x02;
            }
            if (Keyboard.IsPressed(Corekey.G))
            {
                bkey2 &= unchecked((byte)~0x02);
            }
            else
            {
                bkey2 |= 0x02;
            }
            if (Keyboard.IsPressed(Corekey.H))
            {
                bkey3 &= unchecked((byte)~0x02);
            }
            else
            {
                bkey3 |= 0x02;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_4))//if (Keyboard.IsPressed(Corekey.I))
            {
                bkey0 &= unchecked((byte)~0x04);
            }
            else
            {
                bkey0 |= 0x04;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_1))//if (Keyboard.IsPressed(Corekey.J))
            {
                bkey1 &= unchecked((byte)~0x04);
            }
            else
            {
                bkey1 |= 0x04;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_2))//if (Keyboard.IsPressed(Corekey.K))
            {
                bkey2 &= unchecked((byte)~0x04);
            }
            else
            {
                bkey2 |= 0x04;
            }
            if (Keyboard.IsPressed(MotionKey.P1_UNKNOW_E))//if (Keyboard.IsPressed(Corekey.L))
            {
                bkey3 &= unchecked((byte)~0x04);
            }
            else
            {
                bkey3 |= 0x04;
            }
            if (Keyboard.IsPressed(MotionKey.UNKNOW_M))//if (Keyboard.IsPressed(Corekey.M))
            {
                bkey0 &= unchecked((byte)~0x08);
            }
            else
            {
                bkey0 |= 0x08;
            }
            if (Keyboard.IsPressed(MotionKey.UNKNOW_N))//if (Keyboard.IsPressed(Corekey.N))
            {
                bkey1 &= unchecked((byte)~0x08);
            }
            else
            {
                bkey1 |= 0x08;
            }
            if (Keyboard.IsPressed(MotionKey.UNKNOW_Q))//if (Keyboard.IsPressed(Corekey.Q))
            {
                bkey0 &= unchecked((byte)~0x10);
            }
            else
            {
                bkey0 |= 0x10;
            }
            if (Keyboard.IsPressed(MotionKey.UNKNOW_R)) //if (Keyboard.IsPressed(Corekey.R))
            {
                bkey1 &= unchecked((byte)~0x10);
            }
            else
            {
                bkey1 |= 0x10;
            }
            if (Keyboard.IsPressed(MotionKey.UNKNOW_T)) //if (Keyboard.IsPressed(Corekey.T))
            {
                bkey2 &= unchecked((byte)~0x08);
            }
            else
            {
                bkey2 |= 0x08;
            }
            if (Keyboard.IsPressed(MotionKey.P1_UP))//if (Keyboard.IsPressed(Corekey.W))
            {
                bkey3 &= unchecked((byte)~0x08);
            }
            else
            {
                bkey3 |= 0x08;
            }
            if (Keyboard.IsPressed(Corekey.Z))
            {
                bkey2 &= unchecked((byte)~0x10);
            }
            else
            {
                bkey2 |= 0x10;
            }
            */
        }
        public static void record_port_drgnwrld()
        {
            if (sbyte0 != sbyte0_old || sbyte1 != sbyte1_old || sbyte2 != sbyte2_old || sbytec != sbytec_old)
            {
                sbyte0_old = sbyte0;
                sbyte1_old = sbyte1;
                sbyte2_old = sbyte2;
                sbytec_old = sbytec;
                Mame.bwRecord.Write(Video.screenstate.frame_number);
                Mame.bwRecord.Write(sbyte0);
                Mame.bwRecord.Write(sbyte1);
                Mame.bwRecord.Write(sbyte2);
                Mame.bwRecord.Write(sbytec);
            }
        }
        public static void replay_port_drgnwrld()
        {
            if (Inptport.bReplayRead)
            {
                try
                {
                    Video.frame_number_obj = Mame.brRecord.ReadInt64();
                    sbyte0_old = Mame.brRecord.ReadSByte();
                    sbyte1_old = Mame.brRecord.ReadSByte();
                    sbyte2_old = Mame.brRecord.ReadSByte();
                    sbytec_old = Mame.brRecord.ReadSByte();
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
                sbytec = sbytec_old;
                Inptport.bReplayRead = true;
            }
            else
            {
                Inptport.bReplayRead = false;
            }
        }
        public static void record_port_lhb()
        {
            if (sbyte0 != sbyte0_old || sbyte1 != sbyte1_old || sbyte2 != sbyte2_old || sbytec != sbytec_old)
            {
                sbyte0_old = sbyte0;
                sbyte1_old = sbyte1;
                sbyte2_old = sbyte2;
                sbytec_old = sbytec;
                Mame.bwRecord.Write(Video.screenstate.frame_number);
                Mame.bwRecord.Write(sbyte0);
                Mame.bwRecord.Write(sbyte1);
                Mame.bwRecord.Write(sbyte2);
                Mame.bwRecord.Write(sbytec);
            }
        }
        public static void replay_port_lhb()
        {
            if (Inptport.bReplayRead)
            {
                try
                {
                    Video.frame_number_obj = Mame.brRecord.ReadInt64();
                    sbyte0_old = Mame.brRecord.ReadSByte();
                    sbyte1_old = Mame.brRecord.ReadSByte();
                    sbyte2_old = Mame.brRecord.ReadSByte();
                    sbytec_old = Mame.brRecord.ReadSByte();
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
                sbytec = sbytec_old;
                Inptport.bReplayRead = true;
            }
            else
            {
                Inptport.bReplayRead = false;
            }
        }
    }
}
