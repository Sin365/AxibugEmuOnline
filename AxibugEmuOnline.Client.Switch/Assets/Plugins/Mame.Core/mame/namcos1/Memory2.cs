namespace MAME.Core
{
    public partial class Namcos1
    {
        public static byte N3ReadMemory_quester(ushort address)
        {
            byte result;
            if (address == 0x1400)
            {
                if ((strobe & 0x20) == 0)
                    result = (byte)((uint)(byte0 & 0x90) | (uint)(strobe & 0x40) | (Inptport.input_port_read_direct(Inptport.analog_p0) & 0x0f));
                else
                    result = (byte)((uint)(byte0 & 0x90) | (uint)(strobe & 0x40) | (Inptport.input_port_read_direct(Inptport.analog_p1) & 0x0f));
                strobe ^= 0x40;
            }
            else if (address == 0x1401)
            {
                if ((strobe & 0x20) == 0)
                    result = (byte)((uint)(byte1 & 0x90) | 0x00 | (Inptport.input_port_read_direct(Inptport.analog_p0) >> 4));
                else
                    result = (byte)((uint)(byte1 & 0x90) | 0x20 | (Inptport.input_port_read_direct(Inptport.analog_p1) >> 4));
                if ((strobe & 0x40) == 0)
                    strobe ^= 0x20;
            }
            else
            {
                result = N3ReadMemory(address);
            }
            return result;
        }
        public static byte N3ReadMemory_berabohm(ushort address)
        {
            byte result;
            if (address == 0x1400)
            {
                int inp = input_count;
                if (inp == 4)
                {
                    result = byte0;
                }
                else
                {
                    if (inp == 0)
                    {
                        result = byte00;
                    }
                    else if (inp == 1)
                    {
                        result = byte01;
                    }
                    else if (inp == 2)
                    {
                        result = byte02;
                    }
                    else if (inp == 3)
                    {
                        result = byte03;
                    }
                    else
                    {
                        result = 0;
                    }
                    if ((result & 1) != 0)
                    {
                        result = 0x7f;
                    }
                    else if ((result & 2) != 0)
                    {
                        result = 0x48;
                    }
                    else if ((result & 4) != 0)
                    {
                        result = 0x40;
                    }
                }
            }
            else if (address == 0x1401)
            {
                result = (byte)(byte1 & 0x8f);
                if (++strobe_count > 4)
                {
                    strobe_count = 0;
                    strobe ^= 0x40;
                    if (strobe == 0)
                    {
                        input_count = (input_count + 1) % 5;
                        if (input_count == 3)
                        {
                            result |= 0x10;
                        }
                    }
                }
                result |= strobe;
            }
            else
            {
                result = N3ReadMemory(address);
            }
            return result;
        }
        public static byte N3ReadMemory_faceoff(ushort address)
        {
            byte result;
            if (address == 0x1400)
            {
                result = (byte)((byte0 & 0x80) | stored_input0);
            }
            else if (address == 0x1401)
            {
                result = (byte)(byte1 & 0x80);
                if (++strobe_count > 8)
                {
                    strobe_count = 0;
                    result |= (byte)input_count;
                    switch (input_count)
                    {
                        case 0:
                            stored_input0 = byte00 & 0x1f;
                            stored_input1 = (byte03 & 0x07) << 3;
                            break;
                        case 3:
                            stored_input0 = byte02 & 0x1f;
                            break;
                        case 4:
                            stored_input0 = byte01 & 0x1f;
                            stored_input1 = byte03 & 0x18;
                            break;
                        default:
                            stored_input0 = 0x1f;
                            stored_input1 = 0x1f;
                            break;
                    }
                    input_count = (input_count + 1) & 7;
                }
                else
                {
                    result |= (byte)(0x40 | stored_input1);
                }
            }
            else
            {
                result = N3ReadMemory(address);
            }
            return result;
        }
    }
}
