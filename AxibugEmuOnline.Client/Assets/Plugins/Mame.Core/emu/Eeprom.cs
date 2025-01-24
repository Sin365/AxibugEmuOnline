using System.IO;

namespace MAME.Core
{
    /*public enum eeprom_command
    {
        COMMAND_INVALID,
        COMMAND_READ,
        COMMAND_WRITE,
        COMMAND_ERASE,
        COMMAND_LOCK,
        COMMAND_UNLOCK,
        COMMAND_WRITEALL,
        COMMAND_ERASEALL,
        COMMAND_COPY_EEPROM_TO_RAM,
        COMMAND_COPY_RAM_TO_EEPROM
    }
    public enum eeprom_state
    {
        STATE_IN_RESET,
        STATE_WAIT_FOR_START_BIT,
        STATE_WAIT_FOR_COMMAND,
        STATE_READING_DATA,
        STATE_WAIT_FOR_DATA,
        STATE_WAIT_FOR_COMPLETION
    };
    public enum eeprom_event
    {
        EVENT_CS_RISING_EDGE = 1 << 0,
        EVENT_CS_FALLING_EDGE = 1 << 1,
        EVENT_CLK_RISING_EDGE = 1 << 2,
        EVENT_CLK_FALLING_EDGE = 1 << 3
    };*/
    public class Eeprom
    {
        public static int serial_count;
        public static byte[] serial_buffer = new byte[40];
        public static byte[] eeprom_data = new byte[0x80];
        private static byte[] cmd_read;
        private static byte[] cmd_write;
        private static byte[] cmd_erase;
        private static byte[] cmd_lock, cmd_unlock;
        public static int eeprom_data_bits;
        public static int eeprom_read_address;
        public static int eeprom_clock_count;
        public static int latch, sending;
        public static int address_bits, data_bits;
        public static LineState reset_line, clock_line;
        public static int locked;
        public static int enable_multi_read;
        public static int reset_delay, reset_delay1;
        private static bool eeprom_command_match(byte[] buf, byte[] cmd, int len)
        {
            int ibuf = 0, idx = 0;
            if (cmd.Length == 0)
            {
                return false;
            }
            if (len == 0)
            {
                return false;
            }
            for (; len > 0;)
            {
                byte b = buf[ibuf];
                byte c = cmd[idx];
                if ((b == 0) || (c == 0))
                    return (b == c);
                switch (c)
                {
                    case (byte)'0':
                    case (byte)'1':
                        if (b != c)
                        {
                            return false;
                        }
                        ibuf++;
                        len--;
                        idx++;
                        break;
                    case (byte)'X':
                    case (byte)'x':
                        ibuf++;
                        len--;
                        idx++;
                        break;
                    case (byte)'*':
                        break;
                }
            }
            return (idx >= cmd.Length);
        }
        public static void eeprom_init()
        {
            serial_count = 0;
            latch = 0;
            reset_line = LineState.ASSERT_LINE;
            clock_line = LineState.ASSERT_LINE;
            eeprom_read_address = 0;
            sending = 0;
            reset_delay1 = 0;
            switch (Machine.sBoard)
            {
                case "CPS-1"://pang3
                    cmd_read = new byte[] { (byte)'0', (byte)'1', (byte)'1', (byte)'0' };
                    cmd_write = new byte[] { (byte)'0', (byte)'1', (byte)'0', (byte)'1' };
                    cmd_erase = new byte[] { (byte)'0', (byte)'1', (byte)'1', (byte)'1' };
                    cmd_lock = new byte[] { };
                    cmd_unlock = new byte[] { };
                    for (int i = 0; i < 0x80; i++)
                    {
                        eeprom_data[i] = 0xff;
                    }
                    locked = 0;
                    address_bits = 6;
                    data_bits = 16;
                    break;
                case "CPS-1(QSound)":
                    cmd_read = new byte[] { (byte)'0', (byte)'1', (byte)'1', (byte)'0' };
                    cmd_write = new byte[] { (byte)'0', (byte)'1', (byte)'0', (byte)'1' };
                    cmd_erase = new byte[] { (byte)'0', (byte)'1', (byte)'1', (byte)'1' };
                    cmd_lock = new byte[] { };
                    cmd_unlock = new byte[] { };
                    for (int i = 0; i < 0x80; i++)
                    {
                        eeprom_data[i] = 0xff;
                    }
                    locked = 0;
                    address_bits = 7;
                    data_bits = 8;
                    break;
                case "CPS2":
                    cmd_read = new byte[] { (byte)'0', (byte)'1', (byte)'1', (byte)'0' };
                    cmd_write = new byte[] { (byte)'0', (byte)'1', (byte)'0', (byte)'1' };
                    cmd_erase = new byte[] { (byte)'0', (byte)'1', (byte)'1', (byte)'1' };
                    cmd_lock = new byte[] { };
                    cmd_unlock = new byte[] { };
                    for (int i = 0; i < 0x80; i++)
                    {
                        eeprom_data[i] = 0xff;
                    }
                    locked = 0;
                    address_bits = 6;
                    data_bits = 16;
                    break;
                case "Taito B":
                    cmd_read = new byte[] { (byte)'0', (byte)'1', (byte)'1', (byte)'0' };
                    cmd_write = new byte[] { (byte)'0', (byte)'1', (byte)'0', (byte)'1' };
                    cmd_erase = new byte[] { (byte)'0', (byte)'1', (byte)'1', (byte)'1' };
                    cmd_lock = new byte[] { (byte)'0', (byte)'1', (byte)'0', (byte)'0', (byte)'0', (byte)'0', (byte)'0', (byte)'0', (byte)'0', (byte)'0' };
                    cmd_unlock = new byte[] { (byte)'0', (byte)'1', (byte)'0', (byte)'0', (byte)'1', (byte)'1', (byte)'0', (byte)'0', (byte)'0', (byte)'0' };
                    for (int i = 0; i < 0x80; i++)
                    {
                        eeprom_data[i] = 0xff;
                    }
                    locked = 1;
                    address_bits = 6;
                    data_bits = 16;
                    break;
                case "Konami 68000":
                    cmd_read = new byte[] { (byte)'0', (byte)'1', (byte)'1', (byte)'0', (byte)'0', (byte)'0' };
                    cmd_write = new byte[] { (byte)'0', (byte)'1', (byte)'1', (byte)'1', (byte)'0', (byte)'0' };
                    cmd_erase = new byte[] { };
                    cmd_lock = new byte[] { (byte)'0', (byte)'1', (byte)'0', (byte)'0', (byte)'0', (byte)'0', (byte)'0', (byte)'0', (byte)'0', (byte)'0', (byte)'0', (byte)'0', (byte)'0' };
                    cmd_unlock = new byte[] { (byte)'0', (byte)'1', (byte)'0', (byte)'0', (byte)'1', (byte)'1', (byte)'0', (byte)'0', (byte)'0', (byte)'0', (byte)'0', (byte)'0', (byte)'0' };
                    for (int i = 0; i < 0x80; i++)
                    {
                        eeprom_data[i] = 0xff;
                    }
                    locked = 1;
                    address_bits = 7;
                    data_bits = 8;
                    switch (Machine.sName)
                    {
                        case "thndrx2":
                        case "thndrx2a":
                        case "thndrx2j":
                        case "prmrsocr":
                        case "prmrsocrj":
                            cmd_write = new byte[] { (byte)'0', (byte)'1', (byte)'0', (byte)'1', (byte)'0', (byte)'0' };
                            break;
                    }
                    break;
            }
            switch (Machine.sName)
            {
                case "pang3":
                case "pang3r1":
                case "pang3j":
                case "pang3b":
                    for (int i = 0; i < 0x80; i++)
                    {
                        eeprom_data[i] = 0xff;
                    }
                    locked = 0;
                    address_bits = 6;
                    data_bits = 16;
                    break;
            }
        }
        private static void eeprom_write(int bit)
        {
            if (serial_count >= 40 - 1)
            {
                return;
            }
            serial_buffer[serial_count++] = (bit != 0 ? (byte)'1' : (byte)'0');
            serial_buffer[serial_count] = 0;	/* nul terminate so we can treat it as a string */
            if ((serial_count > address_bits) && eeprom_command_match(serial_buffer, cmd_read, serial_count - address_bits))
            {
                int i, address;
                address = 0;
                for (i = serial_count - address_bits; i < serial_count; i++)
                {
                    address <<= 1;
                    if (serial_buffer[i] == (byte)'1')
                    {
                        address |= 1;
                    }
                }
                if (data_bits == 16)
                {
                    eeprom_data_bits = (eeprom_data[2 * address + 0] << 8) + eeprom_data[2 * address + 1];
                }
                else
                {
                    eeprom_data_bits = eeprom_data[address];
                }
                eeprom_read_address = address;
                eeprom_clock_count = 0;
                sending = 1;
                serial_count = 0;
            }
            else if ((serial_count > address_bits) && eeprom_command_match(serial_buffer, cmd_erase, serial_count - address_bits))
            {
                int i, address;
                address = 0;
                for (i = serial_count - address_bits; i < serial_count; i++)
                {
                    address <<= 1;
                    if (serial_buffer[i] == '1')
                    {
                        address |= 1;
                    }
                }
                if (locked == 0)
                {
                    if (data_bits == 16)
                    {
                        eeprom_data[2 * address + 0] = 0x00;
                        eeprom_data[2 * address + 1] = 0x00;
                    }
                    else
                    {
                        eeprom_data[address] = 0x00;
                    }
                }
                serial_count = 0;
            }
            else if ((serial_count > (address_bits + data_bits)) && eeprom_command_match(serial_buffer, cmd_write, serial_count - (address_bits + data_bits)))
            {
                int i, address, data;
                address = 0;
                for (i = serial_count - data_bits - address_bits; i < (serial_count - data_bits); i++)
                {
                    address <<= 1;
                    if (serial_buffer[i] == '1')
                    {
                        address |= 1;
                    }
                }
                data = 0;
                for (i = serial_count - data_bits; i < serial_count; i++)
                {
                    data <<= 1;
                    if (serial_buffer[i] == '1')
                    {
                        data |= 1;
                    }
                }
                if (locked == 0)
                {
                    if (data_bits == 16)
                    {
                        eeprom_data[2 * address + 0] = (byte)(data >> 8);
                        eeprom_data[2 * address + 1] = (byte)(data & 0xff);
                    }
                    else
                    {
                        eeprom_data[address] = (byte)data;
                    }
                }
                serial_count = 0;
            }
            else if (eeprom_command_match(serial_buffer, cmd_lock, serial_count))
            {
                locked = 1;
                serial_count = 0;
            }
            else if (eeprom_command_match(serial_buffer, cmd_unlock, serial_count))
            {
                locked = 0;
                serial_count = 0;
            }
        }
        private static void eeprom_reset()
        {
            serial_count = 0;
            sending = 0;
            reset_delay = reset_delay1;
        }
        public static void eeprom_write_bit(int bit)
        {
            latch = bit;
        }
        public static int eeprom_read_bit()
        {
            int res;
            if (sending == 1)
            {
                res = (eeprom_data_bits >> data_bits) & 1;
            }
            else
            {
                if (reset_delay > 0)
                {
                    reset_delay--;
                    res = 0;
                }
                else
                {
                    res = 1;
                }
            }
            return res;
        }
        public static int eeprom_bit_r()
        {
            return eeprom_read_bit();
        }
        public static void eeprom_set_cs_line(LineState state)
        {
            reset_line = state;
            if (reset_line != LineState.CLEAR_LINE)
            {
                eeprom_reset();
            }
        }
        public static void eeprom_set_clock_line(LineState state)
        {
            if (state == LineState.PULSE_LINE || (clock_line == LineState.CLEAR_LINE && state != LineState.CLEAR_LINE))
            {
                if (reset_line == LineState.CLEAR_LINE)
                {
                    if (sending == 1)
                    {
                        if (eeprom_clock_count == data_bits && enable_multi_read != 0)
                        {
                            eeprom_read_address = (eeprom_read_address + 1) & ((1 << address_bits) - 1);
                            if (data_bits == 16)
                            {
                                eeprom_data_bits = (eeprom_data[2 * eeprom_read_address + 0] << 8) + eeprom_data[2 * eeprom_read_address + 1];
                            }
                            else
                            {
                                eeprom_data_bits = eeprom_data[eeprom_read_address];
                            }
                            eeprom_clock_count = 0;
                        }
                        eeprom_data_bits = (eeprom_data_bits << 1) | 1;
                        eeprom_clock_count++;
                    }
                    else
                    {
                        eeprom_write(latch);
                    }
                }
            }
            clock_line = state;
        }
        public static void SaveStateBinary(BinaryWriter writer)
        {
            writer.Write(eeprom_data);
            writer.Write(serial_buffer);
            writer.Write((int)clock_line);
            writer.Write((int)reset_line);
            writer.Write(locked);
            writer.Write(serial_count);
            writer.Write(latch);
            writer.Write(reset_delay);
            writer.Write(sending);
            writer.Write(eeprom_clock_count);
            writer.Write(eeprom_data_bits);
            writer.Write(eeprom_read_address);
        }
        public static void LoadStateBinary(BinaryReader reader)
        {
            eeprom_data = reader.ReadBytes(0x80);
            serial_buffer = reader.ReadBytes(40);
            clock_line = (LineState)reader.ReadInt32();
            reset_line = (LineState)reader.ReadInt32();
            locked = reader.ReadInt32();
            serial_count = reader.ReadInt32();
            latch = reader.ReadInt32();
            reset_delay = reader.ReadInt32();
            sending = reader.ReadInt32();
            eeprom_clock_count = reader.ReadInt32();
            eeprom_data_bits = reader.ReadInt32();
            eeprom_read_address = reader.ReadInt32();
        }
    }
}
