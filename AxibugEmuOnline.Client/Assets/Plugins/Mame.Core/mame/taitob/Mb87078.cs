namespace MAME.Core
{
    public partial class Taitob
    {
        public struct MB87078
        {
            public int[] gain;		/* gain index 0-63,64,65 */
            public int channel_latch;	/* current channel */
            public byte[] latch;	/* 6bit+3bit 4 data latches */
            public byte reset_comp;
        };
        public static MB87078[] c;
        public static int[] MB87078_gain_percent = new int[66]{
           100,94,89,84,79,74,70,66,
            63,59,56,53,50,47,44,42,
            39,37,35,33,31,29,28,26,
            25,23,22,21,19,18,17,16,
            15,14,14,13,12,11,11,10,
            10, 9, 8, 8, 7, 7, 7, 6,
             6, 5, 5, 5, 5, 4, 4, 4,
             3, 3, 3, 3, 3, 2, 2, 2,
           2, 0
        };
        public static int calc_gain_index(int data0, int data1)
        {
            if ((data1 & 4) == 0)
            {
                return 65;
            }
            else
            {
                if ((data1 & 16) != 0)
                {
                    return 64;
                }
                else
                {
                    if ((data1 & 8) != 0)
                    {
                        return 0;
                    }
                    else
                    {
                        return (data0 ^ 0x3f);
                    }
                }
            }
        }
        public static void gain_recalc(int which)
        {
            int i;
            for (i = 0; i < 4; i++)
            {
                int old_index = c[which].gain[i];
                c[which].gain[i] = calc_gain_index(c[which].latch[i], c[which].latch[4 + i]);
                if (old_index != c[which].gain[i])
                {
                    mb87078_gain_changed(i, MB87078_gain_percent[c[which].gain[i]]);
                }
            }
        }
        public static void MB87078_start(int which)
        {
            c = new MB87078[1];
            c[0] = new MB87078();
            c[0].gain = new int[4];
            c[0].latch = new byte[8];
            if (which >= 4)
            {
                return;
            }
            MB87078_reset_comp_w(which, 0);
            MB87078_reset_comp_w(which, 1);
        }
        public static void MB87078_reset_comp_w(int which, int level)
        {
            c[which].reset_comp = (byte)level;
            if (level == 0)
            {
                c[which].latch[0] = 0x3f;
                c[which].latch[1] = 0x3f;
                c[which].latch[2] = 0x3f;
                c[which].latch[3] = 0x3f;
                c[which].latch[4] = 0x0 | 0x4;
                c[which].latch[5] = 0x1 | 0x4;
                c[which].latch[6] = 0x2 | 0x4;
                c[which].latch[7] = 0x3 | 0x4;
            }
            gain_recalc(which);
        }
        public static void MB87078_data_w(int which, int data, int dsel)
        {
            if (c[which].reset_comp == 0)
            {
                return;
            }
            if (dsel == 0)
            {
                c[which].latch[0 + c[which].channel_latch] = (byte)(data & 0x3f);
            }
            else
            {
                c[which].channel_latch = data & 3;
                c[which].latch[4 + c[which].channel_latch] = (byte)(data & 0x1f);
            }
            gain_recalc(which);
        }
    }
}
