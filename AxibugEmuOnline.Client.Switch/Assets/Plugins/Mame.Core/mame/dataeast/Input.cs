using MAME.Core;
using System.Collections.Generic;

namespace MAME.Core
{
    public partial class Dataeast
    {
        public class fr1
        {
            public int fr;
            public byte by;
            public fr1(int i1, byte b1)
            {
                fr = i1;
                by = b1;
            }
        }
        public static int i3 = 70;
        public static List<fr1> lfr = new List<fr1>();
        public static void loop_inputports_dataeast_pcktgal()
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
                byte2 &= unchecked((byte)~0x20);
            }
            else
            {
                byte2 |= 0x20;
            }
            if (Keyboard.IsPressed(MotionKey.P1_GAMESTART))//if (Keyboard.IsPressed(Corekey.D1))
            {
                byte1 &= unchecked((byte)~0x10);
            }
            else
            {
                byte1 |= 0x10;
            }
            if (Keyboard.IsPressed(MotionKey.P2_GAMESTART))//if (Keyboard.IsPressed(Corekey.D2))
            {
                byte1 &= unchecked((byte)~0x20);
            }
            else
            {
                byte1 |= 0x20;
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
                byte1 &= unchecked((byte)~0x80);
            }
            else
            {
                byte1 |= 0x80;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_2))//if (Keyboard.IsPressed(Corekey.K))
            {
                byte1 &= unchecked((byte)~0x40);
            }
            else
            {
                byte1 |= 0x40;
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
                byte2 &= unchecked((byte)~0x80);
            }
            else
            {
                byte2 |= 0x80;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_2))//if (Keyboard.IsPressed(Corekey.NumPad2))
            {
                byte2 &= unchecked((byte)~0x40);
            }
            else
            {
                byte2 |= 0x40;
            }
            if (Keyboard.IsPressed(MotionKey.UNKNOW_N))//if (Keyboard.IsPressed(Corekey.N))
            {
                lfr = new List<fr1>();
                lfr.Add(new fr1((int)(Video.screenstate.frame_number + 1), 0x7f));
                lfr.Add(new fr1((int)(Video.screenstate.frame_number + 2), 0xff));
                lfr.Add(new fr1((int)(Video.screenstate.frame_number + 2 + i3), 0x7f));
                lfr.Add(new fr1((int)(Video.screenstate.frame_number + 2 + i3 + 1), 0xff));
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_3))//if (Keyboard.IsPressed(Corekey.U))
            {
                lfr = new List<fr1>();
                lfr.Add(new fr1((int)(Video.screenstate.frame_number + 1), 0xf7));
                lfr.Add(new fr1((int)(Video.screenstate.frame_number + 2), 0xff));
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_4))//if (Keyboard.IsPressed(Corekey.I))
            {
                lfr = new List<fr1>();
                lfr.Add(new fr1((int)(Video.screenstate.frame_number + 1), 0xfb));
                lfr.Add(new fr1((int)(Video.screenstate.frame_number + 2), 0xff));
            }

            if (Keyboard.IsPressed(MotionKey.UNKNOW_V))//if (Keyboard.IsPressed(Corekey.V))
            {
                lfr = new List<fr1>();
                lfr.Add(new fr1((int)(Video.screenstate.frame_number + 1), 0xfd));
                lfr.Add(new fr1((int)(Video.screenstate.frame_number + 2), 0xff));
            }
            if (Keyboard.IsPressed(MotionKey.UNKNOW_B))//if (Keyboard.IsPressed(Corekey.B))
            {
                lfr = new List<fr1>();
                lfr.Add(new fr1((int)(Video.screenstate.frame_number + 1), 0xfe));
                lfr.Add(new fr1((int)(Video.screenstate.frame_number + 2), 0xff));
            }
            foreach (fr1 f in lfr)
            {
                if (Video.screenstate.frame_number == f.fr)
                {
                    byte1 = f.by;
                    lfr.Remove(f);
                    break;
                }
            }
        }
        public static void record_port_pcktgal()
        {
            if (byte1 != byte1_old || byte2 != byte2_old)
            {
                byte1_old = byte1;
                byte2_old = byte2;
                Mame.bwRecord.Write(Video.screenstate.frame_number);
                Mame.bwRecord.Write(byte1);
                Mame.bwRecord.Write(byte2);
            }
        }
        public static void replay_port_pcktgal()
        {
            if (Inptport.bReplayRead)
            {
                try
                {
                    Video.frame_number_obj = Mame.brRecord.ReadInt64();
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
