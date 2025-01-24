using MAME.Core;

namespace MAME.Core
{
    public partial class Neogeo
    {
        public static void loop_inputports_neogeo_standard()
        {
            if (Keyboard.IsPressed(MotionKey.P1_INSERT_COIN))//if (Keyboard.IsPressed(Corekey.D5))
            {
                short3 &= ~0x0001;
            }
            else
            {
                short3 |= 0x0001;
            }
            if (Keyboard.IsPressed(MotionKey.P2_INSERT_COIN))//if (Keyboard.IsPressed(Corekey.D6))
            {
                short3 &= ~0x0002;
            }
            else
            {
                short3 |= 0x0002;
            }
            if (Keyboard.IsPressed(MotionKey.P1_GAMESTART))//if (Keyboard.IsPressed(Corekey.D1))
            {
                short2 &= ~0x0100;
            }
            else
            {
                short2 |= 0x0100;
            }
            if (Keyboard.IsPressed(MotionKey.P2_GAMESTART))//if (Keyboard.IsPressed(Corekey.D2))
            {
                short2 &= ~0x0400;
            }
            else
            {
                short2 |= 0x0400;
            }


            //if (Keyboard.IsPressed(Corekey.D))
            if (Keyboard.IsPressed(MotionKey.P1_RIGHT))
            {
                short0 &= ~0x0800;
            }
            else
            {
                short0 |= 0x0800;
            }
            if (Keyboard.IsPressed(MotionKey.P1_LEFT))//if (Keyboard.IsPressed(Corekey.A))
            {
                short0 &= ~0x0400;
            }
            else
            {
                short0 |= 0x0400;
            }
            if (Keyboard.IsPressed(MotionKey.P1_DOWN))//if (Keyboard.IsPressed(Corekey.S))
            {
                short0 &= ~0x0200;
            }
            else
            {
                short0 |= 0x0200;
            }
            if (Keyboard.IsPressed(MotionKey.P1_UP))//if (Keyboard.IsPressed(Corekey.W))
            {
                short0 &= ~0x0100;
            }
            else
            {
                short0 |= 0x0100;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_1))//if (Keyboard.IsPressed(Corekey.J))
            {
                short0 &= ~0x1000;
            }
            else
            {
                short0 |= 0x1000;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_2))//if (Keyboard.IsPressed(Corekey.K))
            {
                short0 &= ~0x2000;
            }
            else
            {
                short0 |= 0x2000;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_E))//if (Keyboard.IsPressed(Corekey.L))
            {

            }
            else
            {

            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_3))//if (Keyboard.IsPressed(Corekey.U))
            {
                short0 &= ~0x4000;
            }
            else
            {
                short0 |= 0x4000;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_4))//if (Keyboard.IsPressed(Corekey.I))
            {
                short0 &= unchecked((short)~0x8000);
            }
            else
            {
                short0 |= unchecked((short)0x8000);
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_E))//if (Keyboard.IsPressed(Corekey.O))
            {

            }
            else
            {

            }
            if (Keyboard.IsPressed(MotionKey.P2_RIGHT))//if (Keyboard.IsPressed(Corekey.Right))
            {
                short1 &= ~0x0800;
            }
            else
            {
                short1 |= 0x0800;
            }
            if (Keyboard.IsPressed(MotionKey.P2_LEFT))//if (Keyboard.IsPressed(Corekey.Left))
            {
                short1 &= ~0x0400;
            }
            else
            {
                short1 |= 0x0400;
            }
            if (Keyboard.IsPressed(MotionKey.P2_DOWN)) //if (Keyboard.IsPressed(Corekey.Down))
            {
                short1 &= ~0x0200;
            }
            else
            {
                short1 |= 0x0200;
            }
            if (Keyboard.IsPressed(MotionKey.P2_UP))//if (Keyboard.IsPressed(Corekey.Up))
            {
                short1 &= ~0x0100;
            }
            else
            {
                short1 |= 0x0100;
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

            }
            else
            {

            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_3))//if (Keyboard.IsPressed(Corekey.NumPad4))
            {
                short1 &= ~0x4000;
            }
            else
            {
                short1 |= 0x4000;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_4))//if (Keyboard.IsPressed(MotionKey.P2_BTN_4))//if (Keyboard.IsPressed(Corekey.NumPad5))
            {
                short1 &= unchecked((short)~0x8000);
            }
            else
            {
                short1 |= unchecked((short)0x8000);
            }

            if (Keyboard.IsPressed(MotionKey.P2_BTN_F))//if (Keyboard.IsPressed(Corekey.NumPad6))
            {

            }
            else
            {

            }
            if (Keyboard.IsPressed(MotionKey.UNKNOW_R)) //if (Keyboard.IsPressed(Corekey.R))
            {
                short3 &= ~0x0004;
            }
            else
            {
                short3 |= 0x0004;
            }
            if (Keyboard.IsPressed(MotionKey.UNKNOW_T)) //if (Keyboard.IsPressed(Corekey.T))
            {
                short4 &= ~0x0080;
            }
            else
            {
                short4 |= 0x0080;
            }
        }
        public static void loop_inputports_neogeo_irrmaze()
        {
            if (Keyboard.IsPressed(MotionKey.P1_INSERT_COIN))//if (Keyboard.IsPressed(Corekey.D5))
            {
                short3 &= ~0x0001;
            }
            else
            {
                short3 |= 0x0001;
            }
            if (Keyboard.IsPressed(MotionKey.P2_INSERT_COIN))//if (Keyboard.IsPressed(Corekey.D6))
            {
                short3 &= ~0x0002;
            }
            else
            {
                short3 |= 0x0002;
            }
            if (Keyboard.IsPressed(MotionKey.P1_GAMESTART))//if (Keyboard.IsPressed(Corekey.D1))
            {
                short2 &= ~0x0100;
            }
            else
            {
                short2 |= 0x0100;
            }
            if (Keyboard.IsPressed(MotionKey.P2_GAMESTART))//if (Keyboard.IsPressed(Corekey.D2))
            {
                short2 &= ~0x0400;
            }
            else
            {
                short2 |= 0x0400;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_1))//if (Keyboard.IsPressed(Corekey.J))
            {
                short1 &= ~0x1000;
            }
            else
            {
                short1 |= 0x1000;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_2))//if (Keyboard.IsPressed(Corekey.K))
            {
                short1 &= ~0x2000;
            }
            else
            {
                short1 |= 0x2000;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_1))//if (Keyboard.IsPressed(Corekey.NumPad1))
            {
                short1 &= ~0x4000;
            }
            else
            {
                short1 |= 0x4000;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_2))//if (Keyboard.IsPressed(Corekey.NumPad2))
            {
                short1 &= unchecked((short)~0x8000);
            }
            else
            {
                short1 |= unchecked((short)0x8000);
            }
            Inptport.frame_update_analog_field_irrmaze_p0(Inptport.analog_p0);
            Inptport.frame_update_analog_field_irrmaze_p1(Inptport.analog_p1);
        }
        public static void record_port()
        {
            if (short0 != short0_old || short1 != short1_old || short2 != short2_old || short3 != short3_old || short4 != short4_old)
            {
                short0_old = short0;
                short1_old = short1;
                short2_old = short2;
                short3_old = short3;
                short4_old = short4;
                Mame.bwRecord.Write(Video.screenstate.frame_number);
                Mame.bwRecord.Write(short0);
                Mame.bwRecord.Write(short1);
                Mame.bwRecord.Write(short2);
                Mame.bwRecord.Write(short3);
                Mame.bwRecord.Write(short4);
            }
        }
        public static void replay_port()
        {
            if (Inptport.bReplayRead)
            {
                try
                {
                    Video.frame_number_obj = Mame.brRecord.ReadInt64();
                    short0_old = Mame.brRecord.ReadInt16();
                    short1_old = Mame.brRecord.ReadInt16();
                    short2_old = Mame.brRecord.ReadInt16();
                    short3_old = Mame.brRecord.ReadInt16();
                    short4_old = Mame.brRecord.ReadInt16();
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
                short3 = short3_old;
                short4 = short4_old;
                Inptport.bReplayRead = true;
            }
            else
            {
                Inptport.bReplayRead = false;
            }
        }
    }
}
