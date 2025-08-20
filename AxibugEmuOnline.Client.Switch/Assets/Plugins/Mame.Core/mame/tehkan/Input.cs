using MAME.Core;

namespace MAME.Core
{
    public partial class Tehkan
    {
        public static void loop_inputports_tehkan_pbaction()
        {
            if (Keyboard.IsPressed(MotionKey.P1_INSERT_COIN))//if (Keyboard.IsPressed(Corekey.D5))
            {
                byte2 |= 0x01;
            }
            else
            {
                byte2 &= unchecked((byte)~0x01);
            }
            if (Keyboard.IsPressed(MotionKey.P2_INSERT_COIN))//if (Keyboard.IsPressed(Corekey.D6))
            {
                byte2 |= 0x02;
            }
            else
            {
                byte2 &= unchecked((byte)~0x02);
            }
            if (Keyboard.IsPressed(MotionKey.P1_GAMESTART))//if (Keyboard.IsPressed(Corekey.D1))
            {
                byte2 |= 0x04;
            }
            else
            {
                byte2 &= unchecked((byte)~0x04);
            }
            if (Keyboard.IsPressed(MotionKey.P2_GAMESTART))//if (Keyboard.IsPressed(Corekey.D2))
            {
                byte2 |= 0x08;
            }
            else
            {
                byte2 &= unchecked((byte)~0x08);
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_1))//if (Keyboard.IsPressed(Corekey.J))
            {
                byte0 |= 0x08;
            }
            else
            {
                byte0 &= unchecked((byte)~0x08);
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_2))//if (Keyboard.IsPressed(Corekey.K))
            {
                byte0 |= 0x10;
            }
            else
            {
                byte0 &= unchecked((byte)~0x10);
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_E))//if (Keyboard.IsPressed(Corekey.L))
            {
                byte0 |= 0x01;
            }
            else
            {
                byte0 &= unchecked((byte)~0x01);
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_3))//if (Keyboard.IsPressed(Corekey.U))
            {
                byte0 |= 0x04;
            }
            else
            {
                byte0 &= unchecked((byte)~0x04);
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_1))//if (Keyboard.IsPressed(Corekey.NumPad1))
            {
                byte1 |= 0x08;
            }
            else
            {
                byte1 &= unchecked((byte)~0x08);
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_2))//if (Keyboard.IsPressed(Corekey.NumPad2))
            {
                byte1 |= 0x10;
            }
            else
            {
                byte1 &= unchecked((byte)~0x10);
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_E))//if (Keyboard.IsPressed(Corekey.NumPad3))
            {
                byte1 |= 0x01;
            }
            else
            {
                byte1 &= unchecked((byte)~0x01);
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_3))//if (Keyboard.IsPressed(Corekey.NumPad4))
            {
                byte1 |= 0x04;
            }
            else
            {
                byte1 &= unchecked((byte)~0x04);
            }
        }
        public static void record_port_pbaction()
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
        public static void replay_port_pbaction()
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
