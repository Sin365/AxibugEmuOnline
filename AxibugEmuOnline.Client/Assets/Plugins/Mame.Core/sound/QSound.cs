using System;

namespace MAME.Core
{
    public struct QSOUND_CHANNEL
    {
        public int bank;	   /* bank (x16)    */
        public int address;	/* start address */
        public int pitch;	  /* pitch */
        public int reg3;	   /* unknown (always 0x8000) */
        public int loop;	   /* loop address */
        public int end;		/* end address */
        public int vol;		/* master volume */
        public int pan;		/* Pan value */
        public int reg9;	   /* unknown */
        /* Work variables */
        public int key;		/* Key on / key off */
        public int lvol;	   /* left volume */
        public int rvol;	   /* right volume */
        public int lastdt;	 /* last sample value */
        public int offset;	 /* current offset counter */
    };
    public struct qsound_info
    {
        /* Private variables */
        public QSOUND_CHANNEL[] channel;
        public int data;				  /* register latch data */
        public int sample_rom_length;
        public int[] pan_table;		 /* Pan volume table */
        public float frq_ratio;		   /* Frequency ratio */
    };
    public unsafe class QSound
    {
        public static sbyte[] qsoundrom;
        public static qsound_info QChip;
        public static void qsound_start()
        {
            int i;
            QChip.sample_rom_length = qsoundrom.Length;
            QChip.channel = new QSOUND_CHANNEL[16];
            //QChip.frq_ratio = 16.0;
            QChip.pan_table = new int[33];
            for (i = 0; i < 33; i++)
            {
                QChip.pan_table[i] = (int)((256 / Math.Sqrt(32)) * Math.Sqrt(i));
            }
        }
        public static void qsound_data_h_w(byte data)
        {
            QChip.data = (QChip.data & 0xff) | (data << 8);
        }
        public static void qsound_data_l_w(byte data)
        {
            QChip.data = (QChip.data & 0xff00) | data;
        }
        public static void qsound_cmd_w(byte data)
        {
            qsound_set_command(data, QChip.data);
        }
        public static byte qsound_status_r()
        {
            /* Port ready bit (0x80 if ready) */
            return 0x80;
        }
        private static void qsound_set_command(int data, int value)
        {
            int ch = 0, reg = 0;
            if (data < 0x80)
            {
                ch = data >> 3;
                reg = data & 0x07;
            }
            else
            {
                if (data < 0x90)
                {
                    ch = data - 0x80;
                    reg = 8;
                }
                else
                {
                    if (data >= 0xba && data < 0xca)
                    {
                        ch = data - 0xba;
                        reg = 9;
                    }
                    else
                    {
                        /* Unknown registers */
                        ch = 99;
                        reg = 99;
                    }
                }
            }
            switch (reg)
            {
                case 0: /* Bank */
                    ch = (ch + 1) & 0x0f;	/* strange ... */
                    QChip.channel[ch].bank = (value & 0x7f) << 16;
                    break;
                case 1: /* start */
                    QChip.channel[ch].address = value;
                    break;
                case 2: /* pitch */
                    QChip.channel[ch].pitch = value * 16;
                    if (value == 0)
                    {
                        /* Key off */
                        QChip.channel[ch].key = 0;
                    }
                    break;
                case 3: /* unknown */
                    QChip.channel[ch].reg3 = value;
                    break;
                case 4: /* loop offset */
                    QChip.channel[ch].loop = value;
                    break;
                case 5: /* end */
                    QChip.channel[ch].end = value;
                    break;
                case 6: /* master volume */
                    if (value == 0)
                    {
                        /* Key off */
                        QChip.channel[ch].key = 0;
                    }
                    else if (QChip.channel[ch].key == 0)
                    {
                        /* Key on */
                        QChip.channel[ch].key = 1;
                        QChip.channel[ch].offset = 0;
                        QChip.channel[ch].lastdt = 0;
                    }
                    QChip.channel[ch].vol = value;
                    break;
                case 7:  /* unused */
                    break;
                case 8:
                    {
                        int pandata = (value - 0x10) & 0x3f;
                        if (pandata > 32)
                        {
                            pandata = 32;
                        }
                        QChip.channel[ch].rvol = QChip.pan_table[pandata];
                        QChip.channel[ch].lvol = QChip.pan_table[32 - pandata];
                        QChip.channel[ch].pan = value;
                    }
                    break;
                case 9:
                    QChip.channel[ch].reg9 = value;
                    break;
            }
        }
        public static void qsound_update(int offset, int length)
        {
            int i, j;
            int rvol, lvol, count;
            for (i = 0; i < length; i++)
            {
                Sound.qsoundstream.streamoutput_Ptrs[0][offset + i] = 0;
                Sound.qsoundstream.streamoutput_Ptrs[1][offset + i] = 0;
            }
            for (i = 0; i < 16; i++)
            {
                if (QChip.channel[i].key != 0)
                {
                    rvol = (QChip.channel[i].rvol * QChip.channel[i].vol) >> 8;
                    lvol = (QChip.channel[i].lvol * QChip.channel[i].vol) >> 8;
                    for (j = 0; j < length; j++)
                    {
                        count = (QChip.channel[i].offset) >> 16;
                        QChip.channel[i].offset &= 0xffff;
                        if (count != 0)
                        {
                            QChip.channel[i].address += count;
                            if (QChip.channel[i].address >= QChip.channel[i].end)
                            {
                                if (QChip.channel[i].loop == 0)
                                {
                                    /* Reached the end of a non-looped sample */
                                    QChip.channel[i].key = 0;
                                    break;
                                }
                                /* Reached the end, restart the loop */
                                QChip.channel[i].address = (QChip.channel[i].end - QChip.channel[i].loop) & 0xffff;
                            }
                            QChip.channel[i].lastdt = qsoundrom[(QChip.channel[i].bank + QChip.channel[i].address) % (QChip.sample_rom_length)];
                        }
                        Sound.qsoundstream.streamoutput_Ptrs[0][offset + j] += ((QChip.channel[i].lastdt * lvol) >> 6);
                        Sound.qsoundstream.streamoutput_Ptrs[1][offset + j] += ((QChip.channel[i].lastdt * rvol) >> 6);
                        QChip.channel[i].offset += QChip.channel[i].pitch;
                    }
                }
            }
        }
        public static void SaveStateBinary(System.IO.BinaryWriter writer)
        {
            int i;
            for (i = 0; i < 16; i++)
            {
                writer.Write(QChip.channel[i].bank);
                writer.Write(QChip.channel[i].address);
                writer.Write(QChip.channel[i].pitch);
                writer.Write(QChip.channel[i].loop);
                writer.Write(QChip.channel[i].end);
                writer.Write(QChip.channel[i].vol);
                writer.Write(QChip.channel[i].pan);
                writer.Write(QChip.channel[i].key);
                writer.Write(QChip.channel[i].lvol);
                writer.Write(QChip.channel[i].rvol);
                writer.Write(QChip.channel[i].lastdt);
                writer.Write(QChip.channel[i].offset);
            }
            writer.Write(QChip.data);
        }
        public static void LoadStateBinary(System.IO.BinaryReader reader)
        {
            int i;
            for (i = 0; i < 16; i++)
            {
                QChip.channel[i].bank = reader.ReadInt32();
                QChip.channel[i].address = reader.ReadInt32();
                QChip.channel[i].pitch = reader.ReadInt32();
                QChip.channel[i].loop = reader.ReadInt32();
                QChip.channel[i].end = reader.ReadInt32();
                QChip.channel[i].vol = reader.ReadInt32();
                QChip.channel[i].pan = reader.ReadInt32();
                QChip.channel[i].key = reader.ReadInt32();
                QChip.channel[i].lvol = reader.ReadInt32();
                QChip.channel[i].rvol = reader.ReadInt32();
                QChip.channel[i].lastdt = reader.ReadInt32();
                QChip.channel[i].offset = reader.ReadInt32();
            }
            QChip.data = reader.ReadInt32();
        }
    }
}