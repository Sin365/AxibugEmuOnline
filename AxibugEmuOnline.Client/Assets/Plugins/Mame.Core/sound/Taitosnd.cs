using System.IO;

namespace MAME.Core
{
    public class Taitosnd
    {
        public static byte TC0140SYT_PORT01_FULL, TC0140SYT_PORT23_FULL, TC0140SYT_PORT01_FULL_MASTER, TC0140SYT_PORT23_FULL_MASTER;
        public struct TC0140SYT
        {
            public byte[] slavedata;
            public byte[] masterdata;
            public byte mainmode;
            public byte submode;
            public byte status;
            public byte nmi_enabled;
            public byte nmi_req;
        }
        public static TC0140SYT tc0140syt;
        public static void taitosnd_start()
        {
            tc0140syt = new TC0140SYT();
            tc0140syt.slavedata = new byte[4];
            tc0140syt.masterdata = new byte[4];
            tc0140syt.mainmode = 0;
            tc0140syt.submode = 0;
            tc0140syt.status = 0;
            tc0140syt.nmi_enabled = 0;
            tc0140syt.nmi_req = 0;
            TC0140SYT_PORT01_FULL = 0x01;
            TC0140SYT_PORT23_FULL = 0x02;
            TC0140SYT_PORT01_FULL_MASTER = 0x04;
            TC0140SYT_PORT23_FULL_MASTER = 0x08;
        }
        public static void Interrupt_Controller()
        {
            if ((tc0140syt.nmi_req != 0) && (tc0140syt.nmi_enabled != 0))
            {
                Cpuint.cpunum_set_input_line(1, (int)LineState.INPUT_LINE_NMI, LineState.PULSE_LINE);
                tc0140syt.nmi_req = 0;
            }
        }
        public static void taitosound_port_w(int offset, byte data)
        {
            data &= 0x0f;
            tc0140syt.mainmode = data;
        }
        public static void taitosound_comm_w(int offset, byte data)
        {
            data &= 0x0f;	/*this is important, otherwise ballbros won't work*/
            switch (tc0140syt.mainmode)
            {
                case 0x00:		// mode #0
                    tc0140syt.slavedata[tc0140syt.mainmode++] = data;
                    break;
                case 0x01:		// mode #1
                    tc0140syt.slavedata[tc0140syt.mainmode++] = data;
                    tc0140syt.status |= TC0140SYT_PORT01_FULL;
                    tc0140syt.nmi_req = 1;
                    break;
                case 0x02:		// mode #2
                    tc0140syt.slavedata[tc0140syt.mainmode++] = data;
                    break;
                case 0x03:		// mode #3
                    tc0140syt.slavedata[tc0140syt.mainmode++] = data;
                    tc0140syt.status |= TC0140SYT_PORT23_FULL;
                    tc0140syt.nmi_req = 1;
                    break;
                case 0x04:		// port status
                    /* this does a hi-lo transition to reset the sound cpu */
                    if (data != 0)
                    {
                        Cpuint.cpunum_set_input_line(1, (int)LineState.INPUT_LINE_RESET, LineState.ASSERT_LINE);
                    }
                    else
                    {
                        Cpuint.cpunum_set_input_line(1, (int)LineState.INPUT_LINE_RESET, LineState.CLEAR_LINE);
                        Cpuexec.cpu_spin(); /* otherwise no sound in driftout */
                    }
                    break;
                default:
                    break;
            }
        }
        public static byte taitosound_comm_r(int offset)
        {
            byte result;
            switch (tc0140syt.mainmode)
            {
                case 0x00:		// mode #0
                    result = tc0140syt.masterdata[tc0140syt.mainmode++];
                    break;
                case 0x01:		// mode #1
                    tc0140syt.status &= (byte)(~TC0140SYT_PORT01_FULL_MASTER);
                    result = tc0140syt.masterdata[tc0140syt.mainmode++];
                    break;
                case 0x02:		// mode #2
                    result = tc0140syt.masterdata[tc0140syt.mainmode++];
                    break;
                case 0x03:		// mode #3
                    tc0140syt.status &= (byte)(~TC0140SYT_PORT23_FULL_MASTER);
                    result = tc0140syt.masterdata[tc0140syt.mainmode++];
                    break;
                case 0x04:		// port status
                    result = tc0140syt.status;
                    break;
                default:
                    result = 0;
                    break;
            }
            return result;
        }
        public static void taitosound_slave_port_w(byte data)
        {
            data &= 0x0f;
            tc0140syt.submode = data;
        }
        public static void taitosound_slave_comm_w(byte data)
        {
            data &= 0x0f;
            switch (tc0140syt.submode)
            {
                case 0x00:		// mode #0
                    tc0140syt.masterdata[tc0140syt.submode++] = data;
                    break;
                case 0x01:		// mode #1
                    tc0140syt.masterdata[tc0140syt.submode++] = data;
                    tc0140syt.status |= TC0140SYT_PORT01_FULL_MASTER;
                    Cpuexec.cpu_spin(); /* writing should take longer than emulated, so spin */
                    break;
                case 0x02:		// mode #2
                    tc0140syt.masterdata[tc0140syt.submode++] = data;
                    break;
                case 0x03:		// mode #3
                    tc0140syt.masterdata[tc0140syt.submode++] = data;
                    tc0140syt.status |= TC0140SYT_PORT23_FULL_MASTER;
                    Cpuexec.cpu_spin(); /* writing should take longer than emulated, so spin */
                    break;
                case 0x04:		// port status
                    break;
                case 0x05:		// nmi disable
                    tc0140syt.nmi_enabled = 0;
                    break;
                case 0x06:		// nmi enable
                    tc0140syt.nmi_enabled = 1;
                    break;
                default:
                    break;
            }
            Interrupt_Controller();
        }
        public static byte taitosound_slave_comm_r()
        {
            byte res = 0;
            switch (tc0140syt.submode)
            {
                case 0x00:		// mode #0
                    res = tc0140syt.slavedata[tc0140syt.submode++];
                    break;
                case 0x01:		// mode #1
                    tc0140syt.status &= unchecked((byte)(~0x01));
                    res = tc0140syt.slavedata[tc0140syt.submode++];
                    break;
                case 0x02:		// mode #2
                    res = tc0140syt.slavedata[tc0140syt.submode++];
                    break;
                case 0x03:		// mode #3
                    tc0140syt.status &= unchecked((byte)(~0x02));
                    res = tc0140syt.slavedata[tc0140syt.submode++];
                    break;
                case 0x04:		// port status
                    res = tc0140syt.status;
                    break;
                default:
                    res = 0;
                    break;
            }
            Interrupt_Controller();
            return res;
        }
        public static void taitosound_port16_msb_w(ushort data)
        {
            //if (ACCESSING_BITS_8_15)
            taitosound_port_w(0, (byte)(data >> 8));
        }
        public static void taitosound_port16_msb_w1(byte data)
        {
            //if (ACCESSING_BITS_8_15)
            taitosound_port_w(0, data);
        }
        public static void taitosound_comm16_msb_w(ushort data)
        {
            //if (ACCESSING_BITS_8_15)
            taitosound_comm_w(0, (byte)(data >> 8));
        }
        public static void taitosound_comm16_msb_w1(byte data)
        {
            //if (ACCESSING_BITS_8_15)
            taitosound_comm_w(0, data);
        }
        public static ushort taitosound_comm16_msb_r()
        {
            return (ushort)(taitosound_comm_r(0) << 8);
        }
        public static void SaveStateBinary(BinaryWriter writer)
        {
            writer.Write(tc0140syt.slavedata, 0, 4);
            writer.Write(tc0140syt.masterdata, 0, 4);
            writer.Write(tc0140syt.mainmode);
            writer.Write(tc0140syt.submode);
            writer.Write(tc0140syt.status);
            writer.Write(tc0140syt.nmi_enabled);
            writer.Write(tc0140syt.nmi_req);
        }
        public static void LoadStateBinary(BinaryReader reader)
        {
            tc0140syt.slavedata = reader.ReadBytes(4);
            tc0140syt.masterdata = reader.ReadBytes(4);
            tc0140syt.mainmode = reader.ReadByte();
            tc0140syt.submode = reader.ReadByte();
            tc0140syt.status = reader.ReadByte();
            tc0140syt.nmi_enabled = reader.ReadByte();
            tc0140syt.nmi_req = reader.ReadByte();
        }
    }
}
