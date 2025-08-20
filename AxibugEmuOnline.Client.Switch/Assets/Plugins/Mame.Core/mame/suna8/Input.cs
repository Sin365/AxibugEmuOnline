using MAME.Core;

namespace MAME.Core
{
    public partial class SunA8
    {
        public static void loop_inputports_suna8_starfigh()
        {
            if (Keyboard.IsPressed(MotionKey.P1_INSERT_COIN))//if (Keyboard.IsPressed(Corekey.D5))
            {
                byte1 &= unchecked((byte)~0x80);
            }
            else
            {
                byte1 |= 0x80;
            }
            if (Keyboard.IsPressed(MotionKey.P2_INSERT_COIN))//if (Keyboard.IsPressed(Corekey.D6))
            {
                byte2 &= unchecked((byte)~0x80);
            }
            else
            {
                byte2 |= 0x80;
            }
            if (Keyboard.IsPressed(MotionKey.P1_GAMESTART))//if (Keyboard.IsPressed(Corekey.D1))
            {
                byte1 &= unchecked((byte)~0x40);
            }
            else
            {
                byte1 |= 0x40;
            }
            if (Keyboard.IsPressed(MotionKey.P2_GAMESTART))//if (Keyboard.IsPressed(Corekey.D2))
            {
                byte2 &= unchecked((byte)~0x40);
            }
            else
            {
                byte2 |= 0x40;
            }
            if (Keyboard.IsPressed(MotionKey.P1_RIGHT))//if (Keyboard.IsPressed(Corekey.D))
            {
                byte1 &= unchecked((byte)~0x08);
            }
            else
            {
                byte1 |= 0x08;
            }
            if (Keyboard.IsPressed(MotionKey.P1_LEFT))//if (Keyboard.IsPressed(Corekey.A))
            {
                byte1 &= unchecked((byte)~0x04);
            }
            else
            {
                byte1 |= 0x04;
            }
            if (Keyboard.IsPressed(MotionKey.P1_DOWN))//if (Keyboard.IsPressed(Corekey.S))
            {
                byte1 &= unchecked((byte)~0x02);
            }
            else
            {
                byte1 |= 0x02;
            }
            if (Keyboard.IsPressed(MotionKey.P1_UP))//if (Keyboard.IsPressed(Corekey.W))
            {
                byte1 &= unchecked((byte)~0x01);
            }
            else
            {
                byte1 |= 0x01;
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
                byte2 &= unchecked((byte)~0x08);
            }
            else
            {
                byte2 |= 0x08;
            }
            if (Keyboard.IsPressed(MotionKey.P2_LEFT))//if (Keyboard.IsPressed(Corekey.Left))
            {
                byte2 &= unchecked((byte)~0x04);
            }
            else
            {
                byte2 |= 0x04;
            }
            if (Keyboard.IsPressed(MotionKey.P2_DOWN)) //if (Keyboard.IsPressed(Corekey.Down))
            {
                byte2 &= unchecked((byte)~0x02);
            }
            else
            {
                byte2 |= 0x02;
            }
            if (Keyboard.IsPressed(MotionKey.P2_UP))//if (Keyboard.IsPressed(Corekey.Up))
            {
                byte2 &= unchecked((byte)~0x01);
            }
            else
            {
                byte2 |= 0x01;
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
        }
        public static void record_port_starfigh()
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
        public static void replay_port_starfigh()
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
