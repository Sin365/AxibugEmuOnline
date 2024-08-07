using System;

namespace VirtualNes.Core
{


    public class X24C01
    {
        public const int X24C01_IDLE = 0;   // Idle
        public const int X24C01_ADDRESS = 1;    // Address set
        public const int X24C01_READ = 2;       // Read
        public const int X24C01_WRITE = 3;      // Write
        public const int X24C01_ACK = 4;      // Acknowledge
        public const int X24C01_ACK_WAIT = 5;   // Acknowledge wait

        int now_state, next_state;
        int bitcnt;
        byte addr, data;
        byte sda;
        byte scl_old, sda_old;

        ArrayRef<byte> pEEPDATA;

        public X24C01()
        {
            now_state = X24C01_IDLE;
            next_state = X24C01_IDLE;
            addr = 0;
            data = 0;
            sda = 0xFF;
            scl_old = 0;
            sda_old = 0;

            pEEPDATA = null;
        }

        public void Reset(ArrayRef<byte> ptr)
        {
            now_state = X24C01_IDLE;
            next_state = X24C01_IDLE;
            addr = 0;
            data = 0;
            sda = 0xFF;
            scl_old = 0;
            sda_old = 0;

            pEEPDATA = ptr;
        }

        public void Write(byte scl_in, byte sda_in)
        {
            // Clock line
            byte scl_rise = (byte)(~scl_old & scl_in);
            byte scl_fall = (byte)(scl_old & ~scl_in);
            // Data line
            byte sda_rise = (byte)(~sda_old & sda_in);
            byte sda_fall = (byte)(sda_old & ~sda_in);

            byte scl_old_temp = scl_old;
            byte sda_old_temp = sda_old;

            scl_old = scl_in;
            sda_old = sda_in;

            // Start condition?
            if (scl_old_temp != 0 && sda_fall != 0)
            {
                now_state = X24C01_ADDRESS;
                bitcnt = 0;
                addr = 0;
                sda = 0xFF;
                return;
            }

            // Stop condition?
            if (scl_old_temp != 0 && sda_rise != 0)
            {
                now_state = X24C01_IDLE;
                sda = 0xFF;
                return;
            }

            // SCL ____---- RISE
            if (scl_rise != 0)
            {
                switch (now_state)
                {
                    case X24C01_ADDRESS:
                        if (bitcnt < 7)
                        {
                            // 本来はMSB->LSB
                            addr = (byte)(addr & (~(1 << bitcnt)));
                            addr = (byte)(addr | ((sda_in != 0 ? 1 : 0) << bitcnt));
                        }
                        else
                        {
                            if (sda_in != 0)
                            {
                                next_state = X24C01_READ;
                                data = pEEPDATA[addr & 0x7F];
                            }
                            else
                            {
                                next_state = X24C01_WRITE;
                            }
                        }
                        bitcnt++;
                        break;
                    case X24C01_ACK:
                        sda = 0;    // ACK
                        break;
                    case X24C01_READ:
                        if (bitcnt < 8)
                        {
                            // 本来はMSB->LSB
                            sda = (byte)((data & (1 << bitcnt)) != 0 ? 1 : 0);
                        }
                        bitcnt++;
                        break;
                    case X24C01_WRITE:
                        if (bitcnt < 8)
                        {
                            // 本来はMSB->LSB
                            data = (byte)(data & (~(1 << bitcnt)));
                            data = (byte)(data | ((sda_in != 0 ? 1 : 0) << bitcnt));
                        }
                        bitcnt++;
                        break;

                    case X24C01_ACK_WAIT:
                        if (sda_in == 0)
                        {
                            next_state = X24C01_IDLE;
                        }
                        break;
                }
            }

            // SCL ----____ FALL
            if (scl_fall != 0)
            {
                switch (now_state)
                {
                    case X24C01_ADDRESS:
                        if (bitcnt >= 8)
                        {
                            now_state = X24C01_ACK;
                            sda = 0xFF;
                        }
                        break;
                    case X24C01_ACK:
                        now_state = next_state;
                        bitcnt = 0;
                        sda = 0xFF;
                        break;
                    case X24C01_READ:
                        if (bitcnt >= 8)
                        {
                            now_state = X24C01_ACK_WAIT;
                            addr = (byte)((addr + 1) & 0x7F);
                        }
                        break;
                    case X24C01_WRITE:
                        if (bitcnt >= 8)
                        {
                            now_state = X24C01_ACK;
                            next_state = X24C01_IDLE;
                            pEEPDATA[addr & 0x7F] = data;
                            addr = (byte)((addr + 1) & 0x7F);
                        }
                        break;
                }
            }
        }

        public byte Read()
        {
            return sda;
        }

        public void Load(byte[] p)
        {
            //now_state = *((INT*)&p[0]);
            //next_state = *((INT*)&p[4]);
            //bitcnt = *((INT*)&p[8]);
            //addr = p[12];
            //data = p[13];
            //sda = p[14];
            //scl_old = p[15];
            //sda_old = p[16];
        }

        public void Save(byte[] p)
        {
            //*((INT*)&p[0]) = now_state;
            //*((INT*)&p[4]) = next_state;
            //*((INT*)&p[8]) = bitcnt;
            //p[12] = addr;
            //p[13] = data;
            //p[14] = sda;
            //p[15] = scl_old;
            //p[16] = sda_old;
        }
    }

    public class X24C02
    {
        public const int X24C02_IDLE = 0;       	// Idle
        public const int X24C02_DEVADDR = 1;    	// Device address set
        public const int X24C02_ADDRESS = 2;    	// Address set
        public const int X24C02_READ = 3;       	// Read
        public const int X24C02_WRITE = 4;      	// Write
        public const int X24C02_ACK = 5;        // Acknowledge
        public const int X24C02_NAK = 6;        // Not Acknowledge
        public const int X24C02_ACK_WAIT = 7;        // Acknowledge wait

        int now_state, next_state;
        int bitcnt;
        byte addr, data, rw;
        byte sda;
        byte scl_old, sda_old;

        ArrayRef<byte> pEEPDATA;

        public X24C02()
        {
            now_state = X24C02_IDLE;
            next_state = X24C02_IDLE;
            addr = 0;
            data = 0;
            rw = 0;
            sda = 0xFF;
            scl_old = 0;
            sda_old = 0;

            pEEPDATA = null;
        }

        public void Reset(ArrayRef<byte> ptr)
        {
            now_state = X24C02_IDLE;
            next_state = X24C02_IDLE;
            addr = 0;
            data = 0;
            rw = 0;
            sda = 0xFF;
            scl_old = 0;
            sda_old = 0;

            pEEPDATA = ptr;
        }

        public void Write(byte scl_in, byte sda_in)
        {
            // Clock line
            byte scl_rise = (byte)(~scl_old & scl_in);
            byte scl_fall = (byte)(scl_old & ~scl_in);
            // Data line
            byte sda_rise = (byte)(~sda_old & sda_in);
            byte sda_fall = (byte)(sda_old & ~sda_in);

            byte scl_old_temp = scl_old;
            byte sda_old_temp = sda_old;

            scl_old = scl_in;
            sda_old = sda_in;

            // Start condition?
            if (scl_old_temp != 0 && sda_fall != 0)
            {
                now_state = X24C02_DEVADDR;
                bitcnt = 0;
                sda = 0xFF;
                return;
            }

            // Stop condition?
            if (scl_old_temp != 0 && sda_rise != 0)
            {
                now_state = X24C02_IDLE;
                sda = 0xFF;
                return;
            }

            // SCL ____---- RISE
            if (scl_rise != 0)
            {
                switch (now_state)
                {
                    case X24C02_DEVADDR:
                        if (bitcnt < 8)
                        {
                            data = (byte)(data & (~(1 << (7 - bitcnt))));
                            data = (byte)(data | ((sda_in != 0 ? 1 : 0) << (7 - bitcnt)));
                        }
                        bitcnt++;
                        break;
                    case X24C02_ADDRESS:
                        if (bitcnt < 8)
                        {
                            addr = (byte)(addr & (~(1 << (7 - bitcnt))));
                            addr = (byte)(addr | ((sda_in != 0 ? 1 : 0) << (7 - bitcnt)));
                        }
                        bitcnt++;
                        break;
                    case X24C02_READ:
                        if (bitcnt < 8)
                        {
                            sda = (byte)((data & (1 << (7 - bitcnt))) != 0 ? 1 : 0);
                        }
                        bitcnt++;
                        break;
                    case X24C02_WRITE:
                        if (bitcnt < 8)
                        {
                            data = (byte)(data & (~(1 << (7 - bitcnt))));
                            data = (byte)(data | ((sda_in != 0 ? 1 : 0) << (7 - bitcnt)));
                        }
                        bitcnt++;
                        break;
                    case X24C02_NAK:
                        sda = 0xFF; // NAK
                        break;
                    case X24C02_ACK:
                        sda = 0;    // ACK
                        break;
                    case X24C02_ACK_WAIT:
                        if (sda_in == 0)
                        {
                            next_state = X24C02_READ;
                            data = pEEPDATA[addr];
                        }
                        break;
                }
            }

            // SCL ----____ FALL
            if (scl_fall != 0)
            {
                switch (now_state)
                {
                    case X24C02_DEVADDR:
                        if (bitcnt >= 8)
                        {
                            if ((data & 0xA0) == 0xA0)
                            {
                                now_state = X24C02_ACK;
                                rw = (byte)(data & 0x01);
                                sda = 0xFF;
                                if (rw != 0)
                                {
                                    // Now address read
                                    next_state = X24C02_READ;
                                    data = pEEPDATA[addr];
                                }
                                else
                                {
                                    next_state = X24C02_ADDRESS;
                                }
                                bitcnt = 0;
                            }
                            else
                            {
                                now_state = X24C02_NAK;
                                next_state = X24C02_IDLE;
                                sda = 0xFF;
                            }
                        }
                        break;
                    case X24C02_ADDRESS:
                        if (bitcnt >= 8)
                        {
                            now_state = X24C02_ACK;
                            sda = 0xFF;
                            if (rw != 0)
                            {
                                // Readでは絶対来ないが念の為
                                next_state = X24C02_IDLE;
                            }
                            else
                            {
                                // to Data Write
                                next_state = X24C02_WRITE;
                            }
                            bitcnt = 0;
                        }
                        break;
                    case X24C02_READ:
                        if (bitcnt >= 8)
                        {
                            now_state = X24C02_ACK_WAIT;
                            addr = (byte)((addr + 1) & 0xFF);
                        }
                        break;
                    case X24C02_WRITE:
                        if (bitcnt >= 8)
                        {
                            pEEPDATA[addr] = data;
                            now_state = X24C02_ACK;
                            next_state = X24C02_WRITE;
                            addr = (byte)((addr + 1) & 0xFF);
                            bitcnt = 0;
                        }
                        break;
                    case X24C02_NAK:
                        now_state = X24C02_IDLE;
                        bitcnt = 0;
                        sda = 0xFF;
                        break;
                    case X24C02_ACK:
                        now_state = next_state;
                        bitcnt = 0;
                        sda = 0xFF;
                        break;
                    case X24C02_ACK_WAIT:
                        now_state = next_state;
                        bitcnt = 0;
                        sda = 0xFF;
                        break;
                }
            }
        }

        public byte Read()
        {
            return sda;
        }

        public void Load(byte[] p)
        {
            //now_state = *((INT*)&p[0]);
            //next_state = *((INT*)&p[4]);
            //bitcnt = *((INT*)&p[8]);
            //addr = p[12];
            //data = p[13];
            //rw = p[14];
            //sda = p[15];
            //scl_old = p[16];
            //sda_old = p[17];
        }

        public void Save(byte[] p)
        {
            //*((INT*)&p[0]) = now_state;
            //*((INT*)&p[4]) = next_state;
            //*((INT*)&p[8]) = bitcnt;
            //p[12] = addr;
            //p[13] = data;
            //p[14] = rw;
            //p[15] = sda;
            //p[16] = scl_old;
            //p[17] = sda_old;
        }
    }
}