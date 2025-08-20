using MAME.Core;

namespace MAME.Core
{
    public partial class M72
    {
        public static void loop_inputports_m72_common()
        {
            if (Keyboard.IsPressed(MotionKey.P1_INSERT_COIN))//if (Keyboard.IsPressed(Corekey.D5))
            {
                ushort1 &= unchecked((ushort)~0x0004);
            }
            else
            {
                ushort1 |= 0x0004;
            }
            if (Keyboard.IsPressed(MotionKey.P2_INSERT_COIN))//if (Keyboard.IsPressed(Corekey.D6))
            {
                ushort1 &= unchecked((ushort)~0x0008);
            }
            else
            {
                ushort1 |= 0x0008;
            }
            if (Keyboard.IsPressed(MotionKey.P1_GAMESTART))//if (Keyboard.IsPressed(Corekey.D1))
            {
                ushort1 &= unchecked((ushort)~0x0001);
            }
            else
            {
                ushort1 |= 0x0001;
            }
            if (Keyboard.IsPressed(MotionKey.P2_GAMESTART))//if (Keyboard.IsPressed(Corekey.D2))
            {
                ushort1 &= unchecked((ushort)~0x0002);
            }
            else
            {
                ushort1 |= 0x0002;
            }
            if (Keyboard.IsPressed(MotionKey.P1_RIGHT))//if (Keyboard.IsPressed(Corekey.D))
            {
                ushort0 &= unchecked((ushort)~0x0001);
            }
            else
            {
                ushort0 |= 0x0001;
            }
            if (Keyboard.IsPressed(MotionKey.P1_LEFT))//if (Keyboard.IsPressed(Corekey.A))
            {
                ushort0 &= unchecked((ushort)~0x0002);
            }
            else
            {
                ushort0 |= 0x0002;
            }
            if (Keyboard.IsPressed(MotionKey.P1_DOWN))//if (Keyboard.IsPressed(Corekey.S))
            {
                ushort0 &= unchecked((ushort)~0x0004);
            }
            else
            {
                ushort0 |= 0x0004;
            }
            if (Keyboard.IsPressed(MotionKey.P1_UP))//if (Keyboard.IsPressed(Corekey.W))
            {
                ushort0 &= unchecked((ushort)~0x0008);
            }
            else
            {
                ushort0 |= 0x0008;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_1))//if (Keyboard.IsPressed(Corekey.J))
            {
                ushort0 &= unchecked((ushort)~0x0080);
            }
            else
            {
                ushort0 |= 0x0080;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_2))//if (Keyboard.IsPressed(Corekey.K))
            {
                ushort0 &= unchecked((ushort)~0x0040);
            }
            else
            {
                ushort0 |= 0x0040;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_3))//if (Keyboard.IsPressed(Corekey.U))
            {
                ushort0 &= unchecked((ushort)~0x0020);
            }
            else
            {
                ushort0 |= 0x0020;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_4))//if (Keyboard.IsPressed(Corekey.I))
            {
                ushort0 &= unchecked((ushort)~0x0010);
            }
            else
            {
                ushort0 |= 0x0010;
            }
            if (Keyboard.IsPressed(MotionKey.P2_RIGHT))//if (Keyboard.IsPressed(Corekey.Right))
            {
                ushort0 &= unchecked((ushort)~0x0100);
            }
            else
            {
                ushort0 |= 0x0100;
            }
            if (Keyboard.IsPressed(MotionKey.P2_LEFT))//if (Keyboard.IsPressed(Corekey.Left))
            {
                ushort0 &= unchecked((ushort)~0x0200);
            }
            else
            {
                ushort0 |= 0x0200;
            }
            if (Keyboard.IsPressed(MotionKey.P2_DOWN)) //if (Keyboard.IsPressed(Corekey.Down))
            {
                ushort0 &= unchecked((ushort)~0x0400);
            }
            else
            {
                ushort0 |= 0x0400;
            }
            if (Keyboard.IsPressed(MotionKey.P2_UP))//if (Keyboard.IsPressed(Corekey.Up))
            {
                ushort0 &= unchecked((ushort)~0x0800);
            }
            else
            {
                ushort0 |= 0x0800;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_1))//if (Keyboard.IsPressed(Corekey.NumPad1))
            {
                ushort0 &= unchecked((ushort)~0x8000);
            }
            else
            {
                ushort0 |= 0x8000;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_2))//if (Keyboard.IsPressed(Corekey.NumPad2))
            {
                ushort0 &= unchecked((ushort)~0x4000);
            }
            else
            {
                ushort0 |= 0x4000;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_3))//if (Keyboard.IsPressed(Corekey.NumPad4))
            {
                ushort0 &= unchecked((ushort)~0x2000);
            }
            else
            {
                ushort0 |= 0x2000;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_4))//if (Keyboard.IsPressed(Corekey.NumPad5))
            {
                ushort0 &= unchecked((ushort)~0x1000);
            }
            else
            {
                ushort0 |= 0x1000;
            }
            if (Keyboard.IsPressed(MotionKey.UNKNOW_R)) //if (Keyboard.IsPressed(Corekey.R))
            {
                ushort1 &= unchecked((ushort)~0x0010);
            }
            else
            {
                ushort1 |= 0x0010;
            }
            if (Keyboard.IsPressed(MotionKey.UNKNOW_T)) //if (Keyboard.IsPressed(Corekey.T))
            {
                ushort1 &= unchecked((ushort)~0x0020);
            }
            else
            {
                ushort1 |= 0x0020;
            }
        }
        public static void record_port()
        {
            if (ushort0 != ushort0_old || ushort1 != ushort1_old)
            {
                ushort0_old = ushort0;
                ushort1_old = ushort1;
                Mame.bwRecord.Write(Video.screenstate.frame_number);
                Mame.bwRecord.Write(ushort0);
                Mame.bwRecord.Write(ushort1);
            }
        }
        public static void replay_port()
        {
            if (Inptport.bReplayRead)
            {
                try
                {
                    Video.frame_number_obj = Mame.brRecord.ReadInt64();
                    ushort0_old = Mame.brRecord.ReadUInt16();
                    ushort1_old = Mame.brRecord.ReadUInt16();
                }
                catch
                {
                    Mame.playState = Mame.PlayState.PLAY_REPLAYEND;
                }
                Inptport.bReplayRead = false;
            }
            if (Video.screenstate.frame_number == Video.frame_number_obj)
            {
                ushort0 = ushort0_old;
                ushort1 = ushort1_old;
                Inptport.bReplayRead = true;
            }
            else
            {
                Inptport.bReplayRead = false;
            }
        }
    }
}
