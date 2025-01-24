using System.IO;

namespace MAME.Core
{
    public class Pd4900a
    {
        public struct pd4990a_s
        {
            public int seconds;
            public int minutes;
            public int hours;
            public int days;
            public int month;
            public int year;
            public int weekday;
        }
        public static pd4990a_s pd4990a;
        public static uint shiftlo, shifthi;
        public static int retraces = 0;	/* Assumes 60 retraces a second */
        public static int testwaits = 0;
        public static int maxwaits = 1;	/*switch test every frame*/
        public static int testbit = 0;	/* Pulses a bit in order to simulate */
        public static int outputbit = 0;
        public static int bitno = 0;
        public static byte reading = 0;
        public static byte writting = 0;
        public static int clock_line = 0;
        public static int command_line = 0;	//??
        public static void pd4990a_init()
        {
            pd4990a.seconds = 0x00;	/* seconds BCD */
            pd4990a.minutes = 0x00;	/* minutes BCD */
            pd4990a.hours = 0x00;	/* hours   BCD */
            pd4990a.days = 0x09;	/* days    BCD */
            pd4990a.month = 9;		/* month   Hexadecimal form */
            pd4990a.year = 0x73;	/* year    BCD */
            pd4990a.weekday = 1;		/* weekday BCD */
        }
        public static void pd4990a_addretrace()
        {
            ++testwaits;
            if (testwaits >= maxwaits)
            {
                testbit ^= 1;
                testwaits = 0;
            }
            retraces++;
            if (retraces < 60)
                return;
            retraces = 0;
            pd4990a.seconds++;
            if ((pd4990a.seconds & 0x0f) < 10)
                return;
            pd4990a.seconds &= 0xf0;
            pd4990a.seconds += 0x10;
            if (pd4990a.seconds < 0x60)
                return;
            pd4990a.seconds = 0;
            pd4990a.minutes++;
            if ((pd4990a.minutes & 0x0f) < 10)
                return;
            pd4990a.minutes &= 0xf0;
            pd4990a.minutes += 0x10;
            if (pd4990a.minutes < 0x60)
                return;
            pd4990a.minutes = 0;
            pd4990a.hours++;
            if ((pd4990a.hours & 0x0f) < 10)
                return;
            pd4990a.hours &= 0xf0;
            pd4990a.hours += 0x10;
            if (pd4990a.hours < 0x24)
                return;
            pd4990a.hours = 0;
            pd4990a_increment_day();
        }
        private static void pd4990a_increment_day()
        {
            int real_year;
            pd4990a.days++;
            if ((pd4990a.days & 0x0f) >= 10)
            {
                pd4990a.days &= 0xf0;
                pd4990a.days += 0x10;
            }
            pd4990a.weekday++;
            if (pd4990a.weekday == 7)
                pd4990a.weekday = 0;

            switch (pd4990a.month)
            {
                case 1:
                case 3:
                case 5:
                case 7:
                case 8:
                case 10:
                case 12:
                    if (pd4990a.days == 0x32)
                    {
                        pd4990a.days = 1;
                        pd4990a_increment_month();
                    }
                    break;
                case 2:
                    real_year = (pd4990a.year >> 4) * 10 + (pd4990a.year & 0xf);
                    if ((real_year % 4) != 0 && ((real_year % 100) == 0 || (real_year % 400) != 0))
                    {
                        if (pd4990a.days == 0x29)
                        {
                            pd4990a.days = 1;
                            pd4990a_increment_month();
                        }
                    }
                    else
                    {
                        if (pd4990a.days == 0x30)
                        {
                            pd4990a.days = 1;
                            pd4990a_increment_month();
                        }
                    }
                    break;
                case 4:
                case 6:
                case 9:
                case 11:
                    if (pd4990a.days == 0x31)
                    {
                        pd4990a.days = 1;
                        pd4990a_increment_month();
                    }
                    break;
            }
        }
        private static void pd4990a_increment_month()
        {
            pd4990a.month++;
            if (pd4990a.month == 13)
            {
                pd4990a.month = 1;
                pd4990a.year++;
                if ((pd4990a.year & 0x0f) >= 10)
                {
                    pd4990a.year &= 0xf0;
                    pd4990a.year += 0x10;
                }
                if (pd4990a.year == 0xA0)
                    pd4990a.year = 0;
            }
        }
        private static void pd4990a_readbit()
        {
            switch (bitno)
            {
                case 0x00:
                case 0x01:
                case 0x02:
                case 0x03:
                case 0x04:
                case 0x05:
                case 0x06:
                case 0x07:
                    outputbit = (pd4990a.seconds >> bitno) & 0x01;
                    break;
                case 0x08:
                case 0x09:
                case 0x0a:
                case 0x0b:
                case 0x0c:
                case 0x0d:
                case 0x0e:
                case 0x0f:
                    outputbit = (pd4990a.minutes >> (bitno - 0x08)) & 0x01;
                    break;
                case 0x10:
                case 0x11:
                case 0x12:
                case 0x13:
                case 0x14:
                case 0x15:
                case 0x16:
                case 0x17:
                    outputbit = (pd4990a.hours >> (bitno - 0x10)) & 0x01;
                    break;
                case 0x18:
                case 0x19:
                case 0x1a:
                case 0x1b:
                case 0x1c:
                case 0x1d:
                case 0x1e:
                case 0x1f:
                    outputbit = (pd4990a.days >> (bitno - 0x18)) & 0x01;
                    break;
                case 0x20:
                case 0x21:
                case 0x22:
                case 0x23:
                    outputbit = (pd4990a.weekday >> (bitno - 0x20)) & 0x01;
                    break;
                case 0x24:
                case 0x25:
                case 0x26:
                case 0x27:
                    outputbit = (pd4990a.month >> (bitno - 0x24)) & 0x01;
                    break;
                case 0x28:
                case 0x29:
                case 0x2a:
                case 0x2b:
                case 0x2c:
                case 0x2d:
                case 0x2e:
                case 0x2f:
                    outputbit = (pd4990a.year >> (bitno - 0x28)) & 0x01;
                    break;
                case 0x30:
                case 0x31:
                case 0x32:
                case 0x33:
                    //unknown
                    break;
            }
        }
        private static void pd4990a_resetbitstream()
        {
            shiftlo = 0;
            shifthi = 0;
            bitno = 0;
        }
        private static void pd4990a_writebit(byte bit)
        {
            if (bitno <= 31)	//low part
                shiftlo |= (uint)(bit << bitno);
            else	//high part
                shifthi |= (uint)(bit << (bitno - 32));
        }
        private static void pd4990a_nextbit()
        {
            ++bitno;
            if (reading != 0)
                pd4990a_readbit();
            if (reading != 0 && bitno == 0x34)
            {
                reading = 0;
                pd4990a_resetbitstream();
            }
        }
        private static byte pd4990a_getcommand()
        {
            //Warning: problems if the 4 bits are in different
            //parts, It's very strange that this case could happen.
            if (bitno <= 31)
                return (byte)(shiftlo >> (bitno - 4));
            else
                return (byte)(shifthi >> (bitno - 32 - 4));
        }
        private static void pd4990a_update_date()
        {
            pd4990a.seconds = (int)((shiftlo >> 0) & 0xff);
            pd4990a.minutes = (int)((shiftlo >> 8) & 0xff);
            pd4990a.hours = (int)((shiftlo >> 16) & 0xff);
            pd4990a.days = (int)((shiftlo >> 24) & 0xff);
            pd4990a.weekday = (int)((shifthi >> 0) & 0x0f);
            pd4990a.month = (int)((shifthi >> 4) & 0x0f);
            pd4990a.year = (int)((shifthi >> 8) & 0xff);
        }
        private static void pd4990a_process_command()
        {
            switch (pd4990a_getcommand())
            {
                case 0x1:	//load output register
                    bitno = 0;
                    if (reading != 0)
                        pd4990a_readbit();	//prepare first bit
                    shiftlo = 0;
                    shifthi = 0;
                    break;
                case 0x2:
                    writting = 0;	//store register to current date
                    pd4990a_update_date();
                    break;
                case 0x3:	//start reading
                    reading = 1;
                    break;
                case 0x7:	//switch testbit every frame
                    maxwaits = 1;
                    break;
                case 0x8:	//switch testbit every half-second
                    maxwaits = 30;
                    break;
            }
            pd4990a_resetbitstream();
        }
        private static void pd4990a_serial_control(byte data)
        {
            //Check for command end
            if (command_line != 0 && (data & 4) == 0) //end of command
            {
                pd4990a_process_command();
            }
            command_line = data & 4;
            if (clock_line != 0 && (data & 2) == 0)	//clock lower edge
            {
                pd4990a_writebit((byte)(data & 1));
                pd4990a_nextbit();
            }
            clock_line = data & 2;
        }
        public static void pd4990a_control_16_w(byte data)
        {
            pd4990a_serial_control((byte)(data & 0x7));
        }
        public static void SaveStateBinary(BinaryWriter writer)
        {
            writer.Write(pd4990a.seconds);
            writer.Write(pd4990a.minutes);
            writer.Write(pd4990a.hours);
            writer.Write(pd4990a.days);
            writer.Write(pd4990a.month);
            writer.Write(pd4990a.year);
            writer.Write(pd4990a.weekday);
            writer.Write(shiftlo);
            writer.Write(shifthi);
            writer.Write(retraces);
            writer.Write(testwaits);
            writer.Write(maxwaits);
            writer.Write(testbit);
            writer.Write(outputbit);
            writer.Write(bitno);
            writer.Write(reading);
            writer.Write(writting);
            writer.Write(clock_line);
            writer.Write(command_line);
        }
        public static void LoadStateBinary(BinaryReader reader)
        {
            pd4990a.seconds = reader.ReadInt32();
            pd4990a.minutes = reader.ReadInt32();
            pd4990a.hours = reader.ReadInt32();
            pd4990a.days = reader.ReadInt32();
            pd4990a.month = reader.ReadInt32();
            pd4990a.year = reader.ReadInt32();
            pd4990a.weekday = reader.ReadInt32();
            shiftlo = reader.ReadUInt32();
            shifthi = reader.ReadUInt32();
            retraces = reader.ReadInt32();
            testwaits = reader.ReadInt32();
            maxwaits = reader.ReadInt32();
            testbit = reader.ReadInt32();
            outputbit = reader.ReadInt32();
            bitno = reader.ReadInt32();
            reading = reader.ReadByte();
            writting = reader.ReadByte();
            clock_line = reader.ReadInt32();
            command_line = reader.ReadInt32();
        }
    }
}