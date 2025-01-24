using System;
using System.IO;

namespace MAME.Core
{
    public class YM2151
    {
        public struct YM2151Operator
        {
            public uint phase;
            public uint freq;
            public int dt1;
            public uint mul;
            public uint dt1_i;
            public uint dt2;

            public int mem_value;

            public uint fb_shift;
            public int fb_out_curr;
            public int fb_out_prev;
            public uint kc;
            public uint kc_i;
            public uint pms;
            public uint ams;

            public uint AMmask;
            public uint state;
            public byte eg_sh_ar;
            public byte eg_sel_ar;
            public uint tl;
            public int volume;
            public byte eg_sh_d1r;
            public byte eg_sel_d1r;
            public uint d1l;
            public byte eg_sh_d2r;
            public byte eg_sel_d2r;
            public byte eg_sh_rr;
            public byte eg_sel_rr;

            public uint key;

            public uint ks;
            public uint ar;
            public uint d1r;
            public uint d2r;
            public uint rr;

            public uint reserved0;
            public uint reserved1;
        };
        public struct YM2151Struct
        {
            public YM2151Operator[] oper;

            public uint[] pan;

            public int lastreg0;
            public uint eg_cnt;
            public uint eg_timer;
            public uint eg_timer_add;
            public uint eg_timer_overflow;

            public uint lfo_phase;
            public uint lfo_timer;
            public uint lfo_timer_add;
            public uint lfo_overflow;
            public uint lfo_counter;
            public uint lfo_counter_add;
            public byte lfo_wsel;
            public byte amd;
            public sbyte pmd;
            public uint lfa;
            public int lfp;

            public byte test;
            public byte ct;

            public uint noise;
            public uint noise_rng;
            public uint noise_p;
            public uint noise_f;

            public uint csm_req;

            public uint irq_enable;
            public uint status;
            public byte[] connect;

            public EmuTimer.emu_timer timer_A;
            public EmuTimer.emu_timer timer_B;
            public Atime[] timer_A_time;
            public Atime[] timer_B_time;
            public int irqlinestate;

            public uint timer_A_index;
            public uint timer_B_index;
            public uint timer_A_index_old;
            public uint timer_B_index_old;

            public uint[] freq;

            public int[] dt1_freq;

            public uint[] noise_tab;

            public int clock;
            public int sampfreq;
            public irqhandler irqhandler;
            public porthandler porthandler;
        };
        private static int[] iconnect = new int[32], imem = new int[32];//m2=8,c1=9,c2=10,mem=11,null=12
        private static int[] tl_tab = new int[13 * 2 * 0x100];
        private static uint[] sin_tab = new uint[0x400];
        private static uint[] d1l_tab = new uint[16];

        private static byte[] eg_inc = new byte[19 * 8]{
            0,1, 0,1, 0,1, 0,1,
            0,1, 0,1, 1,1, 0,1,
            0,1, 1,1, 0,1, 1,1,
            0,1, 1,1, 1,1, 1,1,

            1,1, 1,1, 1,1, 1,1,
            1,1, 1,2, 1,1, 1,2,
            1,2, 1,2, 1,2, 1,2,
            1,2, 2,2, 1,2, 2,2,

            2,2, 2,2, 2,2, 2,2,
            2,2, 2,4, 2,2, 2,4,
            2,4, 2,4, 2,4, 2,4,
            2,4, 4,4, 2,4, 4,4,

            4,4, 4,4, 4,4, 4,4,
            4,4, 4,8, 4,4, 4,8,
            4,8, 4,8, 4,8, 4,8,
            4,8, 8,8, 4,8, 8,8,

            8,8, 8,8, 8,8, 8,8,
            16,16,16,16,16,16,16,16,
            0,0, 0,0, 0,0, 0,0,
        };

        private static uint[] dt2_tab = new uint[4] { 0, 384, 500, 608 };

        private static ushort[] phaseinc_rom = new ushort[768]{
            1299,1300,1301,1302,1303,1304,1305,1306,1308,1309,1310,1311,1313,1314,1315,1316,
            1318,1319,1320,1321,1322,1323,1324,1325,1327,1328,1329,1330,1332,1333,1334,1335,
            1337,1338,1339,1340,1341,1342,1343,1344,1346,1347,1348,1349,1351,1352,1353,1354,
            1356,1357,1358,1359,1361,1362,1363,1364,1366,1367,1368,1369,1371,1372,1373,1374,
            1376,1377,1378,1379,1381,1382,1383,1384,1386,1387,1388,1389,1391,1392,1393,1394,
            1396,1397,1398,1399,1401,1402,1403,1404,1406,1407,1408,1409,1411,1412,1413,1414,
            1416,1417,1418,1419,1421,1422,1423,1424,1426,1427,1429,1430,1431,1432,1434,1435,
            1437,1438,1439,1440,1442,1443,1444,1445,1447,1448,1449,1450,1452,1453,1454,1455,
            1458,1459,1460,1461,1463,1464,1465,1466,1468,1469,1471,1472,1473,1474,1476,1477,
            1479,1480,1481,1482,1484,1485,1486,1487,1489,1490,1492,1493,1494,1495,1497,1498,
            1501,1502,1503,1504,1506,1507,1509,1510,1512,1513,1514,1515,1517,1518,1520,1521,
            1523,1524,1525,1526,1528,1529,1531,1532,1534,1535,1536,1537,1539,1540,1542,1543,
            1545,1546,1547,1548,1550,1551,1553,1554,1556,1557,1558,1559,1561,1562,1564,1565,
            1567,1568,1569,1570,1572,1573,1575,1576,1578,1579,1580,1581,1583,1584,1586,1587,
            1590,1591,1592,1593,1595,1596,1598,1599,1601,1602,1604,1605,1607,1608,1609,1610,
            1613,1614,1615,1616,1618,1619,1621,1622,1624,1625,1627,1628,1630,1631,1632,1633,
            1637,1638,1639,1640,1642,1643,1645,1646,1648,1649,1651,1652,1654,1655,1656,1657,
            1660,1661,1663,1664,1666,1667,1669,1670,1672,1673,1675,1676,1678,1679,1681,1682,
            1685,1686,1688,1689,1691,1692,1694,1695,1697,1698,1700,1701,1703,1704,1706,1707,
            1709,1710,1712,1713,1715,1716,1718,1719,1721,1722,1724,1725,1727,1728,1730,1731,
            1734,1735,1737,1738,1740,1741,1743,1744,1746,1748,1749,1751,1752,1754,1755,1757,
            1759,1760,1762,1763,1765,1766,1768,1769,1771,1773,1774,1776,1777,1779,1780,1782,
            1785,1786,1788,1789,1791,1793,1794,1796,1798,1799,1801,1802,1804,1806,1807,1809,
            1811,1812,1814,1815,1817,1819,1820,1822,1824,1825,1827,1828,1830,1832,1833,1835,
            1837,1838,1840,1841,1843,1845,1846,1848,1850,1851,1853,1854,1856,1858,1859,1861,
            1864,1865,1867,1868,1870,1872,1873,1875,1877,1879,1880,1882,1884,1885,1887,1888,
            1891,1892,1894,1895,1897,1899,1900,1902,1904,1906,1907,1909,1911,1912,1914,1915,
            1918,1919,1921,1923,1925,1926,1928,1930,1932,1933,1935,1937,1939,1940,1942,1944,
            1946,1947,1949,1951,1953,1954,1956,1958,1960,1961,1963,1965,1967,1968,1970,1972,
            1975,1976,1978,1980,1982,1983,1985,1987,1989,1990,1992,1994,1996,1997,1999,2001,
            2003,2004,2006,2008,2010,2011,2013,2015,2017,2019,2021,2022,2024,2026,2028,2029,
            2032,2033,2035,2037,2039,2041,2043,2044,2047,2048,2050,2052,2054,2056,2058,2059,
            2062,2063,2065,2067,2069,2071,2073,2074,2077,2078,2080,2082,2084,2086,2088,2089,
            2092,2093,2095,2097,2099,2101,2103,2104,2107,2108,2110,2112,2114,2116,2118,2119,
            2122,2123,2125,2127,2129,2131,2133,2134,2137,2139,2141,2142,2145,2146,2148,2150,
            2153,2154,2156,2158,2160,2162,2164,2165,2168,2170,2172,2173,2176,2177,2179,2181,
            2185,2186,2188,2190,2192,2194,2196,2197,2200,2202,2204,2205,2208,2209,2211,2213,
            2216,2218,2220,2222,2223,2226,2227,2230,2232,2234,2236,2238,2239,2242,2243,2246,
            2249,2251,2253,2255,2256,2259,2260,2263,2265,2267,2269,2271,2272,2275,2276,2279,
            2281,2283,2285,2287,2288,2291,2292,2295,2297,2299,2301,2303,2304,2307,2308,2311,
            2315,2317,2319,2321,2322,2325,2326,2329,2331,2333,2335,2337,2338,2341,2342,2345,
            2348,2350,2352,2354,2355,2358,2359,2362,2364,2366,2368,2370,2371,2374,2375,2378,
            2382,2384,2386,2388,2389,2392,2393,2396,2398,2400,2402,2404,2407,2410,2411,2414,
            2417,2419,2421,2423,2424,2427,2428,2431,2433,2435,2437,2439,2442,2445,2446,2449,
            2452,2454,2456,2458,2459,2462,2463,2466,2468,2470,2472,2474,2477,2480,2481,2484,
            2488,2490,2492,2494,2495,2498,2499,2502,2504,2506,2508,2510,2513,2516,2517,2520,
            2524,2526,2528,2530,2531,2534,2535,2538,2540,2542,2544,2546,2549,2552,2553,2556,
            2561,2563,2565,2567,2568,2571,2572,2575,2577,2579,2581,2583,2586,2589,2590,2593
        };
        public static byte[] lfo_noise_waveform = new byte[256]{
            0xFF,0xEE,0xD3,0x80,0x58,0xDA,0x7F,0x94,0x9E,0xE3,0xFA,0x00,0x4D,0xFA,0xFF,0x6A,
            0x7A,0xDE,0x49,0xF6,0x00,0x33,0xBB,0x63,0x91,0x60,0x51,0xFF,0x00,0xD8,0x7F,0xDE,
            0xDC,0x73,0x21,0x85,0xB2,0x9C,0x5D,0x24,0xCD,0x91,0x9E,0x76,0x7F,0x20,0xFB,0xF3,
            0x00,0xA6,0x3E,0x42,0x27,0x69,0xAE,0x33,0x45,0x44,0x11,0x41,0x72,0x73,0xDF,0xA2,

            0x32,0xBD,0x7E,0xA8,0x13,0xEB,0xD3,0x15,0xDD,0xFB,0xC9,0x9D,0x61,0x2F,0xBE,0x9D,
            0x23,0x65,0x51,0x6A,0x84,0xF9,0xC9,0xD7,0x23,0xBF,0x65,0x19,0xDC,0x03,0xF3,0x24,
            0x33,0xB6,0x1E,0x57,0x5C,0xAC,0x25,0x89,0x4D,0xC5,0x9C,0x99,0x15,0x07,0xCF,0xBA,
            0xC5,0x9B,0x15,0x4D,0x8D,0x2A,0x1E,0x1F,0xEA,0x2B,0x2F,0x64,0xA9,0x50,0x3D,0xAB,

            0x50,0x77,0xE9,0xC0,0xAC,0x6D,0x3F,0xCA,0xCF,0x71,0x7D,0x80,0xA6,0xFD,0xFF,0xB5,
            0xBD,0x6F,0x24,0x7B,0x00,0x99,0x5D,0xB1,0x48,0xB0,0x28,0x7F,0x80,0xEC,0xBF,0x6F,
            0x6E,0x39,0x90,0x42,0xD9,0x4E,0x2E,0x12,0x66,0xC8,0xCF,0x3B,0x3F,0x10,0x7D,0x79,
            0x00,0xD3,0x1F,0x21,0x93,0x34,0xD7,0x19,0x22,0xA2,0x08,0x20,0xB9,0xB9,0xEF,0x51,

            0x99,0xDE,0xBF,0xD4,0x09,0x75,0xE9,0x8A,0xEE,0xFD,0xE4,0x4E,0x30,0x17,0xDF,0xCE,
            0x11,0xB2,0x28,0x35,0xC2,0x7C,0x64,0xEB,0x91,0x5F,0x32,0x0C,0x6E,0x00,0xF9,0x92,
            0x19,0xDB,0x8F,0xAB,0xAE,0xD6,0x12,0xC4,0x26,0x62,0xCE,0xCC,0x0A,0x03,0xE7,0xDD,
            0xE2,0x4D,0x8A,0xA6,0x46,0x95,0x0F,0x8F,0xF5,0x15,0x97,0x32,0xD4,0x28,0x1E,0x55
        };
        public static YM2151Struct PSG;
        public static int[] chanout = new int[12];
        public delegate void irqhandler(int irq);
        public delegate void porthandler(int offset, byte data);

        private static void init_tables()
        {
            int i, x, n;
            double o, m;
            for (x = 0; x < 0x100; x++)
            {
                m = (1 << 16) / Math.Pow(2, (x + 1) * (1.0 / 256));
                m = Math.Floor(m);
                n = (int)m;
                n >>= 4;
                if ((n & 1) != 0)
                {
                    n = (n >> 1) + 1;
                }
                else
                {
                    n = n >> 1;
                }
                n <<= 2;
                tl_tab[x * 2] = n;
                tl_tab[x * 2 + 1] = -tl_tab[x * 2];
                for (i = 1; i < 13; i++)
                {
                    tl_tab[x * 2 + i * 2 * 0x100] = tl_tab[x * 2] >> i;
                    tl_tab[x * 2 + 1 + i * 2 * 0x100] = -tl_tab[x * 2 + i * 2 * 0x100];
                }
            }
            for (i = 0; i < 0x400; i++)
            {
                m = Math.Sin(((i * 2) + 1) * Math.PI / 0x400);
                if (m > 0.0)
                {
                    o = 8 * Math.Log(1.0 / m) / Math.Log(2);
                }
                else
                {
                    o = 8 * Math.Log(-1.0 / m) / Math.Log(2);
                }
                o = o / (1.0 / 32);
                n = (int)(2.0 * o);
                if ((n & 1) != 0)
                {
                    n = (n >> 1) + 1;
                }
                else
                {
                    n = n >> 1;
                }
                sin_tab[i] = (uint)(n * 2 + (m >= 0.0 ? 0 : 1));
            }
            for (i = 0; i < 16; i++)
            {
                m = (i != 15 ? i : i + 16) * 32;
                d1l_tab[i] = (uint)m;
            }
        }
        private static void init_chip_tables()
        {
            int i, j;
            double mult, phaseinc, Hz;
            double scaler;
            Atime pom;
            scaler = ((double)PSG.clock / 64.0) / ((double)PSG.sampfreq);
            /*logerror("scaler    = %20.15f\n", scaler);*/
            /* this loop calculates Hertz values for notes from c-0 to b-7 */
            /* including 64 'cents' (100/64 that is 1.5625 of real cent) per note */
            /* i*100/64/1200 is equal to i/768 */
            /* real chip works with 10 bits fixed point values (10.10) */
            mult = (1 << 6); /* -10 because phaseinc_rom table values are already in 10.10 format */
            for (i = 0; i < 768; i++)
            {
                /* 3.4375 Hz is note A; C# is 4 semitones higher */
                Hz = 1000;

                phaseinc = phaseinc_rom[i];	/* real chip phase increment */
                phaseinc *= scaler;			/* adjust */

                /* octave 2 - reference octave */
                PSG.freq[768 + 2 * 768 + i] = (uint)(((int)(phaseinc * mult)) & 0xffffffc0); /* adjust to X.10 fixed point */
                /* octave 0 and octave 1 */
                for (j = 0; j < 2; j++)
                {
                    PSG.freq[768 + j * 768 + i] = (PSG.freq[768 + 2 * 768 + i] >> (2 - j)) & 0xffffffc0; /* adjust to X.10 fixed point */
                }
                /* octave 3 to 7 */
                for (j = 3; j < 8; j++)
                {
                    PSG.freq[768 + j * 768 + i] = PSG.freq[768 + 2 * 768 + i] << (j - 2);
                }
            }
            /* octave -1 (all equal to: oct 0, _KC_00_, _KF_00_) */
            for (i = 0; i < 768; i++)
            {
                PSG.freq[i] = PSG.freq[768];
            }
            /* octave 8 and 9 (all equal to: oct 7, _KC_14_, _KF_63_) */
            for (j = 8; j < 10; j++)
            {
                for (i = 0; i < 768; i++)
                {
                    PSG.freq[768 + j * 768 + i] = PSG.freq[768 + 8 * 768 - 1];
                }
            }
            mult = (1 << 16);
            for (j = 0; j < 4; j++)
            {
                for (i = 0; i < 32; i++)
                {
                    Hz = ((double)FM.dt_tab[j * 32 + i] * ((double)PSG.clock / 64.0)) / (double)(1 << 20);

                    /*calculate phase increment*/
                    phaseinc = (Hz * 0x400) / (double)PSG.sampfreq;

                    /*positive and negative values*/
                    PSG.dt1_freq[j * 32 + i] = (int)(phaseinc * mult);
                    PSG.dt1_freq[(j + 4) * 32 + i] = -PSG.dt1_freq[j * 32 + i];
                }
            }
            /* calculate timers' deltas */
            /* User's Manual pages 15,16  */
            mult = (1 << 16);
            for (i = 0; i < 1024; i++)
            {
                /* ASG 980324: changed to compute both tim_A_tab and timer_A_time */
                pom = Attotime.attotime_mul(new Atime(0, Attotime.ATTOSECONDS_PER_SECOND / PSG.clock), (uint)(64 * (1024 - i)));
                PSG.timer_A_time[i] = pom;//(long)Math.Pow(10, 18) 
            }
            for (i = 0; i < 256; i++)
            {
                /* ASG 980324: changed to compute both tim_B_tab and timer_B_time */
                pom = Attotime.attotime_mul(new Atime(0, Attotime.ATTOSECONDS_PER_SECOND / PSG.clock), (uint)(1024 * (256 - i)));
                PSG.timer_B_time[i] = pom;
            }
            /* calculate noise periods table */
            //scaler = ((double)PSG.clock / 64.0) / ((double)PSG.sampfreq);
            for (i = 0; i < 32; i++)
            {
                j = (i != 31 ? i : 30);				/* rate 30 and 31 are the same */
                j = 32 - j;
                j = (int)(65536.0 / (double)(j * 32.0));	/* number of samples per one shift of the shift register */
                /*chip->noise_tab[i] = j * 64;*/
                /* number of chip clock cycles per one shift */
                PSG.noise_tab[i] = (uint)(j * 64 * scaler);
                /*logerror("noise_tab[%02x]=%08x\n", i, chip->noise_tab[i]);*/
            }
        }
        private static void KEY_ON(uint op1, uint key_set)//YM2151Operator op, uint key_set)
        {
            if (PSG.oper[op1].key == 0)
            {
                PSG.oper[op1].phase = 0;			/* clear phase */
                PSG.oper[op1].state = 4;		/* KEY ON = attack */
                PSG.oper[op1].volume += (~PSG.oper[op1].volume *
                               (eg_inc[PSG.oper[op1].eg_sel_ar + ((PSG.eg_cnt >> PSG.oper[op1].eg_sh_ar) & 7)])
                              ) >> 4;
                if (PSG.oper[op1].volume <= 0)
                {
                    PSG.oper[op1].volume = 0;
                    PSG.oper[op1].state = 3;
                }
            }
            PSG.oper[op1].key |= key_set;
        }
        private static void KEY_OFF(uint op1, uint key_clr)//YM2151Operator op, uint key_clr)
        {
            if (PSG.oper[op1].key != 0)
            {
                PSG.oper[op1].key &= key_clr;
                if (PSG.oper[op1].key == 0)
                {
                    if (PSG.oper[op1].state > 1)
                        PSG.oper[op1].state = 1;/* KEY OFF = release */
                }
            }
        }
        private static void envelope_KONKOFF(uint i, int v)
        {
            if ((v & 0x08) != 0)	/* M1 */
                KEY_ON(i, 1);
            else
                KEY_OFF(i, 0xfffffffe);
            if ((v & 0x20) != 0)	/* M2 */
                KEY_ON(i + 1, 1);
            else
                KEY_OFF(i + 1, 0xfffffffe);
            if ((v & 0x10) != 0)	/* C1 */
                KEY_ON(i + 2, 1);
            else
                KEY_OFF(i + 2, 0xfffffffe);
            if ((v & 0x40) != 0)	/* C2 */
                KEY_ON(i + 3, 1);
            else
                KEY_OFF(i + 3, 0xfffffffe);
        }
        public static void irqAon_callback()
        {
            int oldstate = PSG.irqlinestate;
            PSG.irqlinestate |= 1;
            if (oldstate == 0)
            {
                PSG.irqhandler(1);
            }
        }
        public static void irqBon_callback()
        {
            int oldstate = PSG.irqlinestate;
            PSG.irqlinestate |= 2;
            if (oldstate == 0)
            {
                PSG.irqhandler(1);
            }
        }
        public static void irqAoff_callback()
        {
            int oldstate = PSG.irqlinestate;
            PSG.irqlinestate &= ~1;
            if (oldstate == 1)
            {
                PSG.irqhandler(0);
            }
        }
        public static void irqBoff_callback()
        {
            int oldstate = PSG.irqlinestate;
            PSG.irqlinestate &= ~2;
            if (oldstate == 2)
            {
                PSG.irqhandler(0);
            }
        }
        public static void timer_callback_a()
        {
            EmuTimer.timer_adjust_periodic(PSG.timer_A, PSG.timer_A_time[PSG.timer_A_index], Attotime.ATTOTIME_NEVER);
            PSG.timer_A_index_old = PSG.timer_A_index;
            if ((PSG.irq_enable & 0x04) != 0)
            {
                PSG.status |= 1;
                EmuTimer.timer_set_internal(EmuTimer.TIME_ACT.YM2151_irqAon_callback);
            }
            if ((PSG.irq_enable & 0x80) != 0)
            {
                PSG.csm_req = 2;		/* request KEY ON / KEY OFF sequence */
            }
        }
        public static void timer_callback_b()
        {
            EmuTimer.timer_adjust_periodic(PSG.timer_B, PSG.timer_B_time[PSG.timer_B_index], Attotime.ATTOTIME_NEVER);
            PSG.timer_B_index_old = PSG.timer_B_index;
            if ((PSG.irq_enable & 0x08) != 0)
            {
                PSG.status |= 2;
                EmuTimer.timer_set_internal(EmuTimer.TIME_ACT.YM2151_irqBon_callback);
            }
        }
        private static void set_connect(int cha, int v)
        {
            /* set connect algorithm */
            /* MEM is simply one sample delay */
            switch (v & 7)
            {
                case 0:
                    /* M1---C1---MEM---M2---C2---OUT */
                    //PSG.oper[i1].connect = c1;
                    //oc1.connect = mem;
                    //om2.connect = c2;
                    //PSG.oper[i1].mem_connect = m2;
                    iconnect[cha * 4] = 9;
                    iconnect[cha * 4 + 2] = 11;
                    iconnect[cha * 4 + 1] = 10;
                    imem[cha * 4] = 8;
                    break;

                case 1:
                    /* M1------+-MEM---M2---C2---OUT */
                    /*      C1-+                     */
                    //PSG.oper[i1].connect = mem;
                    //oc1.connect = mem;
                    //om2.connect = c2;
                    //PSG.oper[i1].mem_connect = m2;
                    iconnect[cha * 4] = 11;
                    iconnect[cha * 4 + 2] = 11;
                    iconnect[cha * 4 + 1] = 10;
                    imem[cha * 4] = 8;
                    break;

                case 2:
                    /* M1-----------------+-C2---OUT */
                    /*      C1---MEM---M2-+          */
                    //PSG.oper[i1].connect = c2;
                    //oc1.connect = mem;
                    //om2.connect = c2;
                    //PSG.oper[i1].mem_connect = m2;
                    iconnect[cha * 4] = 10;
                    iconnect[cha * 4 + 2] = 11;
                    iconnect[cha * 4 + 1] = 10;
                    imem[cha * 4] = 8;
                    break;

                case 3:
                    /* M1---C1---MEM------+-C2---OUT */
                    /*                 M2-+          */
                    //PSG.oper[i1].connect = c1;
                    //oc1.connect = mem;
                    //om2.connect = c2;
                    //PSG.oper[i1].mem_connect = c2;
                    iconnect[cha * 4] = 9;
                    iconnect[cha * 4 + 2] = 11;
                    iconnect[cha * 4 + 1] = 10;
                    imem[cha * 4] = 10;
                    break;

                case 4:
                    /* M1---C1-+-OUT */
                    /* M2---C2-+     */
                    /* MEM: not used */
                    //PSG.oper[i1].connect = c1;
                    //oc1.connect = chanout[cha];
                    //om2.connect = c2;
                    //PSG.oper[i1].mem_connect = mem;	/* store it anywhere where it will not be used */
                    iconnect[cha * 4] = 9;
                    iconnect[cha * 4 + 2] = cha;
                    iconnect[cha * 4 + 1] = 10;
                    imem[cha * 4] = 11;
                    break;

                case 5:
                    /*    +----C1----+     */
                    /* M1-+-MEM---M2-+-OUT */
                    /*    +----C2----+     */
                    //PSG.oper[i1].connect = 0;	/* special mark */
                    //oc1.connect = chanout[cha];
                    //om2.connect = chanout[cha];
                    //PSG.oper[i1].mem_connect = m2;
                    iconnect[cha * 4] = 12;
                    iconnect[cha * 4 + 2] = cha;
                    iconnect[cha * 4 + 1] = cha;
                    imem[cha * 4] = 8;
                    break;

                case 6:
                    /* M1---C1-+     */
                    /*      M2-+-OUT */
                    /*      C2-+     */
                    /* MEM: not used */
                    //PSG.oper[i1].connect = c1;
                    //oc1.connect = chanout[cha];
                    //om2.connect = chanout[cha];
                    //PSG.oper[i1].mem_connect = mem;	/* store it anywhere where it will not be used */
                    iconnect[cha * 4] = 9;
                    iconnect[cha * 4 + 2] = cha;
                    iconnect[cha * 4 + 1] = cha;
                    imem[cha * 4] = 11;
                    break;

                case 7:
                    /* M1-+     */
                    /* C1-+-OUT */
                    /* M2-+     */
                    /* C2-+     */
                    /* MEM: not used*/
                    //PSG.oper[i1].connect = chanout[cha];
                    //oc1.connect = chanout[cha];
                    //om2.connect = chanout[cha];
                    //PSG.oper[i1].mem_connect = mem;	/* store it anywhere where it will not be used */
                    iconnect[cha * 4] = cha;
                    iconnect[cha * 4 + 2] = cha;
                    iconnect[cha * 4 + 1] = cha;
                    imem[cha * 4] = 11;
                    break;
            }
        }
        private static void refresh_EG(int i1)
        {
            uint kc;
            uint v;
            kc = PSG.oper[i1].kc;
            /* v = 32 + 2*RATE + RKS = max 126 */
            v = kc >> (int)PSG.oper[i1].ks;
            if ((PSG.oper[i1].ar + v) < 94)
            {
                PSG.oper[i1].eg_sh_ar = FM.eg_rate_shift[PSG.oper[i1].ar + v];
                PSG.oper[i1].eg_sel_ar = FM.eg_rate_select[PSG.oper[i1].ar + v];
            }
            else
            {
                PSG.oper[i1].eg_sh_ar = 0;
                PSG.oper[i1].eg_sel_ar = 17 * 8;
            }
            PSG.oper[i1].eg_sh_d1r = FM.eg_rate_shift[PSG.oper[i1].d1r + v];
            PSG.oper[i1].eg_sel_d1r = FM.eg_rate_select[PSG.oper[i1].d1r + v];
            PSG.oper[i1].eg_sh_d2r = FM.eg_rate_shift[PSG.oper[i1].d2r + v];
            PSG.oper[i1].eg_sel_d2r = FM.eg_rate_select[PSG.oper[i1].d2r + v];
            PSG.oper[i1].eg_sh_rr = FM.eg_rate_shift[PSG.oper[i1].rr + v];
            PSG.oper[i1].eg_sel_rr = FM.eg_rate_select[PSG.oper[i1].rr + v];
            i1 += 1;
            v = kc >> (int)PSG.oper[i1].ks;
            if ((PSG.oper[i1].ar + v) < 94)
            {
                PSG.oper[i1].eg_sh_ar = FM.eg_rate_shift[PSG.oper[i1].ar + v];
                PSG.oper[i1].eg_sel_ar = FM.eg_rate_select[PSG.oper[i1].ar + v];
            }
            else
            {
                PSG.oper[i1].eg_sh_ar = 0;
                PSG.oper[i1].eg_sel_ar = 17 * 8;
            }
            PSG.oper[i1].eg_sh_d1r = FM.eg_rate_shift[PSG.oper[i1].d1r + v];
            PSG.oper[i1].eg_sel_d1r = FM.eg_rate_select[PSG.oper[i1].d1r + v];
            PSG.oper[i1].eg_sh_d2r = FM.eg_rate_shift[PSG.oper[i1].d2r + v];
            PSG.oper[i1].eg_sel_d2r = FM.eg_rate_select[PSG.oper[i1].d2r + v];
            PSG.oper[i1].eg_sh_rr = FM.eg_rate_shift[PSG.oper[i1].rr + v];
            PSG.oper[i1].eg_sel_rr = FM.eg_rate_select[PSG.oper[i1].rr + v];
            i1 += 1;
            v = kc >> (int)PSG.oper[i1].ks;
            if ((PSG.oper[i1].ar + v) < 94)
            {
                PSG.oper[i1].eg_sh_ar = FM.eg_rate_shift[PSG.oper[i1].ar + v];
                PSG.oper[i1].eg_sel_ar = FM.eg_rate_select[PSG.oper[i1].ar + v];
            }
            else
            {
                PSG.oper[i1].eg_sh_ar = 0;
                PSG.oper[i1].eg_sel_ar = 17 * 8;
            }
            PSG.oper[i1].eg_sh_d1r = FM.eg_rate_shift[PSG.oper[i1].d1r + v];
            PSG.oper[i1].eg_sel_d1r = FM.eg_rate_select[PSG.oper[i1].d1r + v];
            PSG.oper[i1].eg_sh_d2r = FM.eg_rate_shift[PSG.oper[i1].d2r + v];
            PSG.oper[i1].eg_sel_d2r = FM.eg_rate_select[PSG.oper[i1].d2r + v];
            PSG.oper[i1].eg_sh_rr = FM.eg_rate_shift[PSG.oper[i1].rr + v];
            PSG.oper[i1].eg_sel_rr = FM.eg_rate_select[PSG.oper[i1].rr + v];
            i1 += 1;
            v = kc >> (int)PSG.oper[i1].ks;
            if ((PSG.oper[i1].ar + v) < 94)
            {
                PSG.oper[i1].eg_sh_ar = FM.eg_rate_shift[PSG.oper[i1].ar + v];
                PSG.oper[i1].eg_sel_ar = FM.eg_rate_select[PSG.oper[i1].ar + v];
            }
            else
            {
                PSG.oper[i1].eg_sh_ar = 0;
                PSG.oper[i1].eg_sel_ar = 17 * 8;
            }
            PSG.oper[i1].eg_sh_d1r = FM.eg_rate_shift[PSG.oper[i1].d1r + v];
            PSG.oper[i1].eg_sel_d1r = FM.eg_rate_select[PSG.oper[i1].d1r + v];
            PSG.oper[i1].eg_sh_d2r = FM.eg_rate_shift[PSG.oper[i1].d2r + v];
            PSG.oper[i1].eg_sel_d2r = FM.eg_rate_select[PSG.oper[i1].d2r + v];
            PSG.oper[i1].eg_sh_rr = FM.eg_rate_shift[PSG.oper[i1].rr + v];
            PSG.oper[i1].eg_sel_rr = FM.eg_rate_select[PSG.oper[i1].rr + v];
        }
        private static void ym2151_write_reg(int r, int v)
        {
            int opIndex = (r & 0x07) * 4 + ((r & 0x18) >> 3);
            /* adjust bus to 8 bits */
            r &= 0xff;
            v &= 0xff;
            switch (r & 0xe0)
            {
                case 0x00:
                    switch (r)
                    {
                        case 0x01:	/* LFO reset(bit 1), Test Register (other bits) */
                            PSG.test = (byte)v;
                            if ((v & 2) != 0)
                            {
                                PSG.lfo_phase = 0;
                            }
                            break;
                        case 0x08:
                            envelope_KONKOFF((uint)((v & 7) * 4), v);
                            break;
                        case 0x0f:	/* noise mode enable, noise period */
                            PSG.noise = (uint)v;
                            PSG.noise_f = PSG.noise_tab[v & 0x1f];
                            break;
                        case 0x10:	/* timer A hi */
                            PSG.timer_A_index = (PSG.timer_A_index & 0x003) | (uint)(v << 2);
                            break;
                        case 0x11:	/* timer A low */
                            PSG.timer_A_index = (PSG.timer_A_index & 0x3fc) | (uint)(v & 3);
                            break;
                        case 0x12:	/* timer B */
                            PSG.timer_B_index = (uint)v;
                            break;
                        case 0x14:	/* CSM, irq flag reset, irq enable, timer start/stop */
                            PSG.irq_enable = (uint)v;	/* bit 3-timer B, bit 2-timer A, bit 7 - CSM */
                            if ((v & 0x10) != 0)	/* reset timer A irq flag */
                            {
                                PSG.status &= 0xfffffffe;
                                EmuTimer.timer_set_internal(EmuTimer.TIME_ACT.YM2151_irqAoff_callback);
                            }
                            if ((v & 0x20) != 0)	/* reset timer B irq flag */
                            {
                                PSG.status &= 0xfffffffd;
                                EmuTimer.timer_set_internal(EmuTimer.TIME_ACT.YM2151_irqBoff_callback);
                            }
                            if ((v & 0x02) != 0)
                            {	/* load and start timer B */
                                /* ASG 980324: added a real timer */
                                /* start timer _only_ if it wasn't already started (it will reload time value next round) */
                                if (!EmuTimer.timer_enable(PSG.timer_B, true))
                                {
                                    EmuTimer.timer_adjust_periodic(PSG.timer_B, PSG.timer_B_time[PSG.timer_B_index], Attotime.ATTOTIME_NEVER);
                                    PSG.timer_B_index_old = PSG.timer_B_index;
                                }
                            }
                            else
                            {		/* stop timer B */
                                /* ASG 980324: added a real timer */
                                EmuTimer.timer_enable(PSG.timer_B, false);
                            }
                            if ((v & 0x01) != 0)
                            {	/* load and start timer A */
                                /* ASG 980324: added a real timer */
                                /* start timer _only_ if it wasn't already started (it will reload time value next round) */
                                if (!EmuTimer.timer_enable(PSG.timer_A, true))
                                {
                                    EmuTimer.timer_adjust_periodic(PSG.timer_A, PSG.timer_A_time[PSG.timer_A_index], Attotime.ATTOTIME_NEVER);
                                    PSG.timer_A_index_old = PSG.timer_A_index;
                                }
                            }
                            else
                            {		/* stop timer A */
                                /* ASG 980324: added a real timer */
                                EmuTimer.timer_enable(PSG.timer_A, false);
                            }
                            break;
                        case 0x18:	/* LFO frequency */
                            {
                                PSG.lfo_overflow = (uint)((1 << ((15 - (v >> 4)) + 3)) * (1 << 10));
                                PSG.lfo_counter_add = (uint)(0x10 + (v & 0x0f));
                            }
                            break;
                        case 0x19:	/* PMD (bit 7==1) or AMD (bit 7==0) */
                            if ((v & 0x80) != 0)
                                PSG.pmd = (sbyte)(v & 0x7f);
                            else
                                PSG.amd = (byte)(v & 0x7f);
                            break;
                        case 0x1b:	/* CT2, CT1, LFO waveform */
                            PSG.ct = (byte)(v >> 6);
                            PSG.lfo_wsel = (byte)(v & 3);
                            if (PSG.porthandler != null)
                            {
                                PSG.porthandler(0, PSG.ct);
                            }
                            break;
                        default:
                            //logerror("YM2151 Write %02x to undocumented register #%02x\n", v, r);
                            break;
                    }
                    break;
                case 0x20:
                    int op1 = (r & 7) * 4;
                    //op = PSG.oper[(r & 7) * 4];
                    switch (r & 0x18)
                    {
                        case 0x00:	/* RL enable, Feedback, Connection */
                            PSG.oper[op1].fb_shift = (uint)((((v >> 3) & 7) != 0) ? ((v >> 3) & 7) + 6 : 0);
                            PSG.pan[(r & 7) * 2] = (uint)(((v & 0x40) != 0) ? ~0 : 0);
                            PSG.pan[(r & 7) * 2 + 1] = (uint)(((v & 0x80) != 0) ? ~0 : 0);
                            PSG.connect[r & 7] = (byte)(v & 7);
                            set_connect(r & 7, v & 7);
                            break;
                        case 0x08:	/* Key Code */
                            v &= 0x7f;
                            if (v != PSG.oper[op1].kc)
                            {
                                uint kc, kc_channel;
                                kc_channel = (uint)((v - (v >> 2)) * 64);
                                kc_channel += 768;
                                kc_channel |= (PSG.oper[op1].kc_i & 63);
                                PSG.oper[(r & 7) * 4].kc = (uint)v;
                                PSG.oper[(r & 7) * 4].kc_i = kc_channel;
                                PSG.oper[(r & 7) * 4 + 1].kc = (uint)v;
                                PSG.oper[(r & 7) * 4 + 1].kc_i = kc_channel;
                                PSG.oper[(r & 7) * 4 + 2].kc = (uint)v;
                                PSG.oper[(r & 7) * 4 + 2].kc_i = kc_channel;
                                PSG.oper[(r & 7) * 4 + 3].kc = (uint)v;
                                PSG.oper[(r & 7) * 4 + 3].kc_i = kc_channel;
                                kc = (uint)(v >> 2);
                                PSG.oper[(r & 7) * 4].dt1 = PSG.dt1_freq[PSG.oper[(r & 7) * 4].dt1_i + kc];
                                PSG.oper[(r & 7) * 4].freq = (uint)(((PSG.freq[kc_channel + PSG.oper[(r & 7) * 4].dt2] + PSG.oper[(r & 7) * 4].dt1) * PSG.oper[(r & 7) * 4].mul) >> 1);
                                PSG.oper[(r & 7) * 4 + 1].dt1 = PSG.dt1_freq[PSG.oper[(r & 7) * 4 + 1].dt1_i + kc];
                                PSG.oper[(r & 7) * 4 + 1].freq = (uint)(((PSG.freq[kc_channel + PSG.oper[(r & 7) * 4 + 1].dt2] + PSG.oper[(r & 7) * 4 + 1].dt1) * PSG.oper[(r & 7) * 4 + 1].mul) >> 1);
                                PSG.oper[(r & 7) * 4 + 2].dt1 = PSG.dt1_freq[PSG.oper[(r & 7) * 4 + 2].dt1_i + kc];
                                PSG.oper[(r & 7) * 4 + 2].freq = (uint)(((PSG.freq[kc_channel + PSG.oper[(r & 7) * 4 + 2].dt2] + PSG.oper[(r & 7) * 4 + 2].dt1) * PSG.oper[(r & 7) * 4 + 2].mul) >> 1);
                                PSG.oper[(r & 7) * 4 + 3].dt1 = PSG.dt1_freq[PSG.oper[(r & 7) * 4 + 3].dt1_i + kc];
                                PSG.oper[(r & 7) * 4 + 3].freq = (uint)(((PSG.freq[kc_channel + PSG.oper[(r & 7) * 4 + 3].dt2] + PSG.oper[(r & 7) * 4 + 3].dt1) * PSG.oper[(r & 7) * 4 + 3].mul) >> 1);
                                refresh_EG(op1);
                            }
                            break;
                        case 0x10:	/* Key Fraction */
                            v >>= 2;
                            if (v != (PSG.oper[(r & 7) * 4].kc_i & 63))
                            {
                                uint kc_channel;
                                kc_channel = (uint)v;
                                kc_channel |= (uint)(PSG.oper[(r & 7) * 4].kc_i & ~63);
                                PSG.oper[(r & 7) * 4].kc_i = kc_channel;
                                PSG.oper[(r & 7) * 4 + 1].kc_i = kc_channel;
                                PSG.oper[(r & 7) * 4 + 2].kc_i = kc_channel;
                                PSG.oper[(r & 7) * 4 + 3].kc_i = kc_channel;
                                PSG.oper[(r & 7) * 4].freq = (uint)(((PSG.freq[kc_channel + PSG.oper[(r & 7) * 4].dt2] + PSG.oper[(r & 7) * 4].dt1) * PSG.oper[(r & 7) * 4].mul) >> 1);
                                PSG.oper[(r & 7) * 4 + 1].freq = (uint)(((PSG.freq[kc_channel + PSG.oper[(r & 7) * 4 + 1].dt2] + PSG.oper[(r & 7) * 4 + 1].dt1) * PSG.oper[(r & 7) * 4 + 1].mul) >> 1);
                                PSG.oper[(r & 7) * 4 + 2].freq = (uint)(((PSG.freq[kc_channel + PSG.oper[(r & 7) * 4 + 2].dt2] + PSG.oper[(r & 7) * 4 + 2].dt1) * PSG.oper[(r & 7) * 4 + 2].mul) >> 1);
                                PSG.oper[(r & 7) * 4 + 3].freq = (uint)(((PSG.freq[kc_channel + PSG.oper[(r & 7) * 4 + 3].dt2] + PSG.oper[(r & 7) * 4 + 3].dt1) * PSG.oper[(r & 7) * 4 + 3].mul) >> 1);
                            }
                            break;
                        case 0x18:	/* PMS, AMS */
                            PSG.oper[op1].pms = (uint)((v >> 4) & 7);
                            PSG.oper[op1].ams = (uint)(v & 3);
                            break;
                    }
                    break;
                case 0x40:		/* DT1, MUL */
                    {
                        uint olddt1_i = PSG.oper[opIndex].dt1_i;
                        uint oldmul = PSG.oper[opIndex].mul;
                        PSG.oper[opIndex].dt1_i = (uint)((v & 0x70) << 1);
                        PSG.oper[opIndex].mul = (uint)(((v & 0x0f) != 0) ? (v & 0x0f) << 1 : 1);
                        if (olddt1_i != PSG.oper[opIndex].dt1_i)
                            PSG.oper[opIndex].dt1 = PSG.dt1_freq[PSG.oper[opIndex].dt1_i + (PSG.oper[opIndex].kc >> 2)];
                        if ((olddt1_i != PSG.oper[opIndex].dt1_i) || (oldmul != PSG.oper[opIndex].mul))
                            PSG.oper[opIndex].freq = (uint)(((PSG.freq[PSG.oper[opIndex].kc_i + PSG.oper[opIndex].dt2] + PSG.oper[opIndex].dt1) * PSG.oper[opIndex].mul) >> 1);
                    }
                    break;
                case 0x60:		/* TL */
                    PSG.oper[opIndex].tl = (uint)((v & 0x7f) << 3); /* 7bit TL */
                    break;
                case 0x80:		/* KS, AR */
                    {
                        uint oldks = PSG.oper[opIndex].ks;
                        uint oldar = PSG.oper[opIndex].ar;
                        PSG.oper[opIndex].ks = (uint)(5 - (v >> 6));
                        PSG.oper[opIndex].ar = (uint)(((v & 0x1f) != 0) ? 32 + ((v & 0x1f) << 1) : 0);
                        if ((PSG.oper[opIndex].ar != oldar) || (PSG.oper[opIndex].ks != oldks))
                        {
                            if ((PSG.oper[opIndex].ar + (PSG.oper[opIndex].kc >> (int)PSG.oper[opIndex].ks)) < 32 + 62)
                            {
                                PSG.oper[opIndex].eg_sh_ar = FM.eg_rate_shift[PSG.oper[opIndex].ar + (PSG.oper[opIndex].kc >> (int)PSG.oper[opIndex].ks)];
                                PSG.oper[opIndex].eg_sel_ar = FM.eg_rate_select[PSG.oper[opIndex].ar + (PSG.oper[opIndex].kc >> (int)PSG.oper[opIndex].ks)];
                            }
                            else
                            {
                                PSG.oper[opIndex].eg_sh_ar = 0;
                                PSG.oper[opIndex].eg_sel_ar = 17 * 8;
                            }
                        }
                        if (PSG.oper[opIndex].ks != oldks)
                        {
                            PSG.oper[opIndex].eg_sh_d1r = FM.eg_rate_shift[PSG.oper[opIndex].d1r + (PSG.oper[opIndex].kc >> (int)PSG.oper[opIndex].ks)];
                            PSG.oper[opIndex].eg_sel_d1r = FM.eg_rate_select[PSG.oper[opIndex].d1r + (PSG.oper[opIndex].kc >> (int)PSG.oper[opIndex].ks)];
                            PSG.oper[opIndex].eg_sh_d2r = FM.eg_rate_shift[PSG.oper[opIndex].d2r + (PSG.oper[opIndex].kc >> (int)PSG.oper[opIndex].ks)];
                            PSG.oper[opIndex].eg_sel_d2r = FM.eg_rate_select[PSG.oper[opIndex].d2r + (PSG.oper[opIndex].kc >> (int)PSG.oper[opIndex].ks)];
                            PSG.oper[opIndex].eg_sh_rr = FM.eg_rate_shift[PSG.oper[opIndex].rr + (PSG.oper[opIndex].kc >> (int)PSG.oper[opIndex].ks)];
                            PSG.oper[opIndex].eg_sel_rr = FM.eg_rate_select[PSG.oper[opIndex].rr + (PSG.oper[opIndex].kc >> (int)PSG.oper[opIndex].ks)];
                        }
                    }
                    break;
                case 0xa0:		/* LFO AM enable, D1R */
                    PSG.oper[opIndex].AMmask = (uint)(((v & 0x80) != 0) ? ~0 : 0);
                    PSG.oper[opIndex].d1r = (uint)(((v & 0x1f) != 0) ? 32 + ((v & 0x1f) << 1) : 0);
                    PSG.oper[opIndex].eg_sh_d1r = FM.eg_rate_shift[PSG.oper[opIndex].d1r + (PSG.oper[opIndex].kc >> (int)PSG.oper[opIndex].ks)];
                    PSG.oper[opIndex].eg_sel_d1r = FM.eg_rate_select[PSG.oper[opIndex].d1r + (PSG.oper[opIndex].kc >> (int)PSG.oper[opIndex].ks)];
                    break;
                case 0xc0:		/* DT2, D2R */
                    {
                        uint olddt2 = PSG.oper[opIndex].dt2;
                        PSG.oper[opIndex].dt2 = dt2_tab[v >> 6];
                        if (PSG.oper[opIndex].dt2 != olddt2)
                            PSG.oper[opIndex].freq = (uint)(((PSG.freq[PSG.oper[opIndex].kc_i + PSG.oper[opIndex].dt2] + PSG.oper[opIndex].dt1) * PSG.oper[opIndex].mul) >> 1);
                    }
                    PSG.oper[opIndex].d2r = (uint)(((v & 0x1f) != 0) ? 32 + ((v & 0x1f) << 1) : 0);
                    PSG.oper[opIndex].eg_sh_d2r = FM.eg_rate_shift[PSG.oper[opIndex].d2r + (PSG.oper[opIndex].kc >> (int)PSG.oper[opIndex].ks)];
                    PSG.oper[opIndex].eg_sel_d2r = FM.eg_rate_select[PSG.oper[opIndex].d2r + (PSG.oper[opIndex].kc >> (int)PSG.oper[opIndex].ks)];
                    break;
                case 0xe0:		/* D1L, RR */
                    PSG.oper[opIndex].d1l = d1l_tab[v >> 4];
                    PSG.oper[opIndex].rr = (uint)(34 + ((v & 0x0f) << 2));
                    PSG.oper[opIndex].eg_sh_rr = FM.eg_rate_shift[PSG.oper[opIndex].rr + (PSG.oper[opIndex].kc >> (int)PSG.oper[opIndex].ks)];
                    PSG.oper[opIndex].eg_sel_rr = FM.eg_rate_select[PSG.oper[opIndex].rr + (PSG.oper[opIndex].kc >> (int)PSG.oper[opIndex].ks)];
                    break;
            }
        }
        public static void ym2151_postload()
        {
            int j;
            for (j = 0; j < 8; j++)
            {
                set_connect(j, PSG.connect[j]);
            }
        }
        public static void ym2151_init(int clock)
        {
            PSG.connect = new byte[8];
            PSG.dt1_freq = new int[256];
            PSG.freq = new uint[11 * 768];
            PSG.noise_tab = new uint[32];
            PSG.oper = new YM2151Operator[32];
            PSG.pan = new uint[16];
            PSG.timer_A_time = new Atime[1024];
            PSG.timer_B_time = new Atime[256];
            for (int i1 = 0; i1 < 32; i1++)
            {
                iconnect[i1] = 12;
            }
            init_tables();
            PSG.clock = clock;//rate = clock/64
            PSG.sampfreq = clock / 64;
            PSG.irqhandler = null;
            PSG.porthandler = null;
            init_chip_tables();
            PSG.lfo_timer_add = (uint)(0x400 * (clock / 64.0) / PSG.sampfreq);
            PSG.eg_timer_add = (uint)(0x10000 * (clock / 64.0) / PSG.sampfreq);
            PSG.eg_timer_overflow = 0x30000;
            /* this must be done _before_ a call to ym2151_reset_chip() */
            PSG.timer_A = EmuTimer.timer_alloc_common(EmuTimer.TIME_ACT.YM2151_timer_callback_a, false);
            PSG.timer_B = EmuTimer.timer_alloc_common(EmuTimer.TIME_ACT.YM2151_timer_callback_b, false);
            ym2151_reset_chip();
            switch (Machine.sBoard)
            {
                case "CPS-1":
                    PSG.irqhandler = Cpuint.cps1_irq_handler_mus;
                    break;
                case "Namco System 1":
                    PSG.irqhandler = Cpuint.namcos1_sound_interrupt;
                    break;
                case "M72":
                    PSG.irqhandler = M72.m72_ym2151_irq_handler;
                    break;
                case "M92":
                    PSG.irqhandler = M92.sound_irq;
                    break;
                case "Taito":
                    switch (Machine.sName)
                    {
                        case "opwolf":
                        case "opwolfa":
                        case "opwolfj":
                        case "opwolfu":
                        case "opwolfb":
                        case "opwolfp":
                            PSG.irqhandler = Taito.irq_handler;
                            PSG.porthandler = Taito.sound_bankswitch_w;
                            break;
                    }
                    break;
                case "Konami 68000":
                    switch (Machine.sName)
                    {
                        case "cuebrick":
                            PSG.irqhandler = Konami68000.cuebrick_irq_handler;
                            break;
                        default:
                            PSG.irqhandler = Konami68000.konami68000_ym2151_irq_handler;
                            break;
                    }
                    break;
                case "Capcom":
                    switch (Machine.sName)
                    {
                        case "sf":
                        case "sfua":
                        case "sfj":
                        case "sfjan":
                        case "sfan":
                        case "sfp":
                            PSG.irqhandler = Capcom.irq_handler;
                            break;
                    }
                    break;
            }
        }
        public static void ym2151_reset_chip()
        {
            int i;
            /* initialize hardware registers */
            for (i = 0; i < 32; i++)
            {
                PSG.oper[i].volume = 0x3ff;
                PSG.oper[i].kc_i = 768; /* min kc_i value */
            }
            PSG.eg_timer = 0;
            PSG.eg_cnt = 0;
            PSG.lfo_timer = 0;
            PSG.lfo_counter = 0;
            PSG.lfo_phase = 0;
            PSG.lfo_wsel = 0;
            PSG.pmd = 0;
            PSG.amd = 0;
            PSG.lfa = 0;
            PSG.lfp = 0;
            PSG.test = 0;
            PSG.irq_enable = 0;
            /* ASG 980324 -- reset the timers before writing to the registers */
            EmuTimer.timer_enable(PSG.timer_A, false);
            EmuTimer.timer_enable(PSG.timer_B, false);
            PSG.timer_A_index = 0;
            PSG.timer_B_index = 0;
            PSG.timer_A_index_old = 0;
            PSG.timer_B_index_old = 0;
            PSG.noise = 0;
            PSG.noise_rng = 0;
            PSG.noise_p = 0;
            PSG.noise_f = PSG.noise_tab[0];
            PSG.csm_req = 0;
            PSG.status = 0;
            ym2151_write_reg(0x1b, 0);	/* only because of CT1, CT2 output pins */
            ym2151_write_reg(0x18, 0);	/* set LFO frequency */
            for (i = 0x20; i < 0x100; i++)		/* set the operators */
            {
                ym2151_write_reg(i, 0);
            }
        }

        unsafe static int op_calc(YM2151Operator* PSGoper, int i1, uint env, int pm)
        {
            uint p;
            p = (env << 3) + sin_tab[(((int)((PSGoper[i1].phase & 0xffff0000) + (pm << 15))) >> 16) & 0x3ff];
            if (p >= 13 * 2 * 0x100)
            {
                return 0;
            }
            return tl_tab[p];
        }

        //private static int op_calc(int i1, uint env, int pm)
        //{
        //    uint p;
        //    p = (env << 3) + sin_tab[(((int)((PSG.oper[i1].phase & 0xffff0000) + (pm << 15))) >> 16) & 0x3ff];
        //    if (p >= 13 * 2 * 0x100)
        //    {
        //        return 0;
        //    }
        //    return tl_tab[p];
        //}


        unsafe static int op_calc1(YM2151Operator* PSGoper, int i1, uint env, int pm)
        {
            uint p;
            int i;
            i = (int)((PSGoper[i1].phase & 0xffff0000) + pm);
            p = (env << 3) + sin_tab[(i >> 16) & 0x3ff];
            if (p >= 13 * 2 * 0x100)
            {
                return 0;
            }
            return tl_tab[p];
        }

        //private static int op_calc1_src(int i1, uint env, int pm)
        //{
        //    uint p;
        //    int i;
        //    i = (int)((PSG.oper[i1].phase & 0xffff0000) + pm);
        //    p = (env << 3) + sin_tab[(i >> 16) & 0x3ff];
        //    if (p >= 13 * 2 * 0x100)
        //    {
        //        return 0;
        //    }
        //    return tl_tab[p];
        //}

        unsafe static uint volume_calc(YM2151Operator* PSGoper, int i1, uint AM)
        {
            uint i11;
            i11 = PSGoper[i1].tl + ((uint)PSGoper[i1].volume) + (AM & PSGoper[i1].AMmask);
            return i11;
        }
        //private static uint volume_calc_old(int i1, uint AM)
        //{
        //    uint i11;
        //    i11 = PSG.oper[i1].tl + ((uint)PSG.oper[i1].volume) + (AM & PSG.oper[i1].AMmask);
        //    return i11;
        //}

        //chan_calc高频调用 单次Update 5467次 左右，下级堆栈volume_calc 20000+次 op_calc 10000+次

        unsafe static void chan_calc(YM2151Operator* PSGoper, int* chanout, int* imem, int chan)
        {

            //fixed (YM2151Operator* PSGoperPtr = &PSG.oper[0])
            {
                //YM2151Operator* PSGoper = PSGoperPtr;
                uint env;
                uint AM = 0;
                //m2 = c1 = c2 = mem = 0;
                chanout[8] = chanout[9] = chanout[10] = chanout[11] = 0;
                //op = PSGoper[chan * 4];	/* M1 */
                //op.mem_connect = op.mem_value;	/* restore delayed sample (MEM) value to m2 or c2 */
                set_mem(PSGoper, chanout, imem, chan * 4);
                if (PSGoper[chan * 4].ams != 0)
                {
                    AM = PSG.lfa << (int)(PSGoper[chan * 4].ams - 1);
                }

                env = volume_calc(PSGoper, (chan * 4), AM);
                //env = volume_calc_planB(PSGoper[chan * 4], AM);
                {
                    int iout = PSGoper[chan * 4].fb_out_prev + PSGoper[chan * 4].fb_out_curr;
                    PSGoper[chan * 4].fb_out_prev = PSGoper[chan * 4].fb_out_curr;

                    set_value1(chanout, PSGoper, chan * 4);

                    PSGoper[chan * 4].fb_out_curr = 0;
                    if (env < 13 * 64)
                    {
                        if (PSGoper[chan * 4].fb_shift == 0)
                        {
                            iout = 0;
                        }
                        PSGoper[chan * 4].fb_out_curr = op_calc1(PSGoper, (chan * 4), env, (iout << (int)PSGoper[chan * 4].fb_shift));
                    }
                }
                env = volume_calc(PSGoper, (chan * 4 + 1), AM); /* M2 */
                //env = volume_calc_planB(PSGoper[chan * 4 + 1], AM);/* M2 */
                if (env < 13 * 64)
                {
                    //PSGoper[chan * 4 + 1].connect += op_calc((int)(chan * 4 + 1), env, m2);
                    set_value2(chanout, chan * 4 + 1, op_calc(PSGoper, (chan * 4 + 1), env, chanout[8]));// m2));
                }
                env = volume_calc(PSGoper, (chan * 4 + 2), AM); /* C1 */
                //env = volume_calc_planB(PSGoper[chan * 4 + 2], AM); /* C1 */
                if (env < 13 * 64)
                {
                    //PSGoper[chan * 4 + 2].connect += op_calc((int)(chan * 4 + 2), env, c1);
                    set_value2(chanout, chan * 4 + 2, op_calc(PSGoper, (chan * 4 + 2), env, chanout[9]));// c1));
                }
                env = volume_calc(PSGoper, (chan * 4 + 3), AM); /* C2 */
                //env = volume_calc_planB(PSGoper[chan * 4 + 3], AM); /* C2 */
                if (env < 13 * 64)
                {
                    chanout[chan] += op_calc(PSGoper, (chan * 4 + 3), env, chanout[10]);// c2);
                }
                /* M1 */
                PSGoper[chan * 4].mem_value = chanout[11];//mem;
            }

        }

        //private static void chan_calc_src(int chan)
        //{
        //    uint env;
        //    uint AM = 0;
        //    //m2 = c1 = c2 = mem = 0;
        //    chanout[8] = chanout[9] = chanout[10] = chanout[11] = 0;
        //    //op = PSG.oper[chan * 4];	/* M1 */
        //    //op.mem_connect = op.mem_value;	/* restore delayed sample (MEM) value to m2 or c2 */
        //    set_mem(chan * 4);
        //    if (PSG.oper[chan * 4].ams != 0)
        //    {
        //        AM = PSG.lfa << (int)(PSG.oper[chan * 4].ams - 1);
        //    }


        //    env = volume_calc((int)(chan * 4), AM);
        //    //env = volume_calc_planB(PSG.oper[chan * 4], AM);
        //    {
        //        int iout = PSG.oper[chan * 4].fb_out_prev + PSG.oper[chan * 4].fb_out_curr;
        //        PSG.oper[chan * 4].fb_out_prev = PSG.oper[chan * 4].fb_out_curr;

        //        set_value1(chan * 4);

        //        PSG.oper[chan * 4].fb_out_curr = 0;
        //        if (env < 13 * 64)
        //        {
        //            if (PSG.oper[chan * 4].fb_shift == 0)
        //            {
        //                iout = 0;
        //            }
        //            PSG.oper[chan * 4].fb_out_curr = op_calc1((int)(chan * 4), env, (int)(iout << (int)PSG.oper[chan * 4].fb_shift));
        //        }
        //    }
        //    env = volume_calc((int)(chan * 4 + 1), AM);	/* M2 */
        //    //env = volume_calc_planB(PSG.oper[chan * 4 + 1], AM);/* M2 */
        //    if (env < 13 * 64)
        //    {
        //        //PSG.oper[chan * 4 + 1].connect += op_calc((int)(chan * 4 + 1), env, m2);
        //        set_value2(chan * 4 + 1, op_calc((int)(chan * 4 + 1), env, chanout[8]));// m2));
        //    }
        //    env = volume_calc((int)(chan * 4 + 2), AM);	/* C1 */
        //    //env = volume_calc_planB(PSG.oper[chan * 4 + 2], AM); /* C1 */
        //    if (env < 13 * 64)
        //    {
        //        //PSG.oper[chan * 4 + 2].connect += op_calc((int)(chan * 4 + 2), env, c1);
        //        set_value2(chan * 4 + 2, op_calc((int)(chan * 4 + 2), env, chanout[9]));// c1));
        //    }
        //    env = volume_calc((int)(chan * 4 + 3), AM);	/* C2 */
        //    //env = volume_calc_planB(PSG.oper[chan * 4 + 3], AM); /* C2 */
        //    if (env < 13 * 64)
        //    {
        //        chanout[chan] += op_calc((int)(chan * 4 + 3), env, chanout[10]);// c2);
        //    }
        //    /* M1 */
        //    PSG.oper[chan * 4].mem_value = chanout[11];//mem;
        //}

        unsafe static void chan7_calc(YM2151Operator* PSGoper, int* chanout, int* imem)
        {
            uint env;
            uint AM = 0;
            //m2 = c1 = c2 = mem = 0;
            chanout[8] = chanout[9] = chanout[10] = chanout[11] = 0;
            //op = PSG.oper[7 * 4];		/* M1 */
            //op.mem_connect = op.mem_value;	/* restore delayed sample (MEM) value to m2 or c2 */
            set_mem(PSGoper, chanout, imem, 7 * 4);
            if (PSGoper[7 * 4].ams != 0)
            {
                AM = PSG.lfa << (int)(PSGoper[7 * 4].ams - 1);
            }
            env = volume_calc(PSGoper, 7 * 4, AM);
            //env = volume_calc_planB(PSGoper[7*4], AM);
            int iout = PSGoper[7 * 4].fb_out_prev + PSGoper[7 * 4].fb_out_curr;
            PSGoper[7 * 4].fb_out_prev = PSGoper[7 * 4].fb_out_curr;
            set_value1(chanout, PSGoper, 7 * 4);
            PSGoper[7 * 4].fb_out_curr = 0;
            if (env < 13 * 64)
            {
                if (PSGoper[7 * 4].fb_shift == 0)
                {
                    iout = 0;
                }
                PSGoper[7 * 4].fb_out_curr = op_calc1(PSGoper, 7 * 4, env, (iout << (int)PSGoper[7 * 4].fb_shift));
            }
            env = volume_calc(PSGoper, 7 * 4 + 1, AM);	/* M2 */
            //env = volume_calc_planB(PSGoper[7 * 4 + 1], AM);/* M2 */
            if (env < 13 * 64)
            {
                //PSGoper[7 * 4 + 1].connect += op_calc(7 * 4 + 1, env, m2);
                set_value2(chanout, 7 * 4 + 1, op_calc(PSGoper, 7 * 4 + 1, env, chanout[8]));// m2));
            }
            env = volume_calc(PSGoper, 7 * 4 + 2, AM);	/* C1 */
            //env = volume_calc_planB(PSGoper[7 * 4 + 2], AM);/* C1 */
            if (env < 13 * 64)
            {
                //PSGoper[7 * 4 + 2].connect += op_calc(7 * 4 + 2, env, c1);
                set_value2(chanout, 7 * 4 + 2, op_calc(PSGoper, 7 * 4 + 2, env, chanout[9]));// c1));
            }
            env = volume_calc(PSGoper, 7 * 4 + 3, AM);	/* C2 */
            //env = volume_calc_planB(PSGoper[7 * 4 + 3], AM);/* C2 */
            if ((PSG.noise & 0x80) != 0)
            {
                uint noiseout;

                noiseout = 0;
                if (env < 0x3ff)
                    noiseout = (env ^ 0x3ff) * 2;	/* range of the YM2151 noise output is -2044 to 2040 */
                chanout[7] += (int)(((PSG.noise_rng & 0x10000) != 0) ? noiseout : -noiseout); /* bit 16 -> output */
            }
            else
            {
                if (env < 13 * 64)
                    chanout[7] += op_calc(PSGoper, 7 * 4 + 3, env, chanout[10]);// c2);
            }
            /* M1 */
            PSGoper[7 * 4].mem_value = chanout[11];//mem;
        }

        //private static void chan7_calc_src()
        //{
        //    uint env;
        //    uint AM = 0;
        //    //m2 = c1 = c2 = mem = 0;
        //    chanout[8] = chanout[9] = chanout[10] = chanout[11] = 0;
        //    //op = PSG.oper[7 * 4];		/* M1 */
        //    //op.mem_connect = op.mem_value;	/* restore delayed sample (MEM) value to m2 or c2 */
        //    set_mem(7 * 4);
        //    if (PSG.oper[7 * 4].ams != 0)
        //    {
        //        AM = PSG.lfa << (int)(PSG.oper[7 * 4].ams - 1);
        //    }
        //    env = volume_calc(7 * 4, AM);
        //    //env = volume_calc_planB(PSG.oper[7*4], AM);
        //    int iout = PSG.oper[7 * 4].fb_out_prev + PSG.oper[7 * 4].fb_out_curr;
        //    PSG.oper[7 * 4].fb_out_prev = PSG.oper[7 * 4].fb_out_curr;
        //    set_value1(7 * 4);
        //    PSG.oper[7 * 4].fb_out_curr = 0;
        //    if (env < 13 * 64)
        //    {
        //        if (PSG.oper[7 * 4].fb_shift == 0)
        //        {
        //            iout = 0;
        //        }
        //        PSG.oper[7 * 4].fb_out_curr = op_calc1(7 * 4, env, (iout << (int)PSG.oper[7 * 4].fb_shift));
        //    }
        //    env = volume_calc(7 * 4 + 1, AM);	/* M2 */
        //    //env = volume_calc_planB(PSG.oper[7 * 4 + 1], AM);/* M2 */
        //    if (env < 13 * 64)
        //    {
        //        //PSG.oper[7 * 4 + 1].connect += op_calc(7 * 4 + 1, env, m2);
        //        set_value2(7 * 4 + 1, op_calc(7 * 4 + 1, env, chanout[8]));// m2));
        //    }
        //    env = volume_calc(7 * 4 + 2, AM);	/* C1 */
        //    //env = volume_calc_planB(PSG.oper[7 * 4 + 2], AM);/* C1 */
        //    if (env < 13 * 64)
        //    {
        //        //PSG.oper[7 * 4 + 2].connect += op_calc(7 * 4 + 2, env, c1);
        //        set_value2(7 * 4 + 2, op_calc(7 * 4 + 2, env, chanout[9]));// c1));
        //    }
        //    env = volume_calc(7 * 4 + 3, AM);	/* C2 */
        //    //env = volume_calc_planB(PSG.oper[7 * 4 + 3], AM);/* C2 */
        //    if ((PSG.noise & 0x80) != 0)
        //    {
        //        uint noiseout;

        //        noiseout = 0;
        //        if (env < 0x3ff)
        //            noiseout = (env ^ 0x3ff) * 2;	/* range of the YM2151 noise output is -2044 to 2040 */
        //        chanout[7] += (int)(((PSG.noise_rng & 0x10000) != 0) ? noiseout : -noiseout); /* bit 16 -> output */
        //    }
        //    else
        //    {
        //        if (env < 13 * 64)
        //            chanout[7] += op_calc(7 * 4 + 3, env, chanout[10]);// c2);
        //    }
        //    /* M1 */
        //    PSG.oper[7 * 4].mem_value = chanout[11];//mem;
        //}

        unsafe static void advance_eg(YM2151Operator* PSGoper)
        {
            uint i;
            int i1 = 0;
            PSG.eg_timer += PSG.eg_timer_add;
            while (PSG.eg_timer >= PSG.eg_timer_overflow)
            {
                PSG.eg_timer -= PSG.eg_timer_overflow;
                PSG.eg_cnt++;
                /* envelope generator */
                //op = PSGoper[i1];	/* CH 0 M1 */
                i = 32;
                do
                {
                    switch (PSGoper[i1].state)
                    {
                        case 4:	/* attack phase */
                            if ((PSG.eg_cnt & ((1 << PSGoper[i1].eg_sh_ar) - 1)) == 0)
                            {
                                PSGoper[i1].volume += (~PSGoper[i1].volume *
                                               (eg_inc[PSGoper[i1].eg_sel_ar + ((PSG.eg_cnt >> PSGoper[i1].eg_sh_ar) & 7)])
                                              ) >> 4;

                                if (PSGoper[i1].volume <= 0)
                                {
                                    PSGoper[i1].volume = 0;
                                    PSGoper[i1].state = 3;
                                }
                            }
                            break;
                        case 3:	/* decay phase */
                            if ((PSG.eg_cnt & ((1 << PSGoper[i1].eg_sh_d1r) - 1)) == 0)
                            {
                                PSGoper[i1].volume += eg_inc[PSGoper[i1].eg_sel_d1r + ((PSG.eg_cnt >> PSGoper[i1].eg_sh_d1r) & 7)];
                                if (PSGoper[i1].volume >= PSGoper[i1].d1l)
                                    PSGoper[i1].state = 2;
                            }
                            break;
                        case 2:	/* sustain phase */
                            if ((PSG.eg_cnt & ((1 << PSGoper[i1].eg_sh_d2r) - 1)) == 0)
                            {
                                PSGoper[i1].volume += eg_inc[PSGoper[i1].eg_sel_d2r + ((PSG.eg_cnt >> PSGoper[i1].eg_sh_d2r) & 7)];
                                if (PSGoper[i1].volume >= 0x3ff)
                                {
                                    PSGoper[i1].volume = 0x3ff;
                                    PSGoper[i1].state = 0;
                                }
                            }
                            break;
                        case 1:	/* release phase */
                            if ((PSG.eg_cnt & ((1 << PSGoper[i1].eg_sh_rr) - 1)) == 0)
                            {
                                PSGoper[i1].volume += eg_inc[PSGoper[i1].eg_sel_rr + ((PSG.eg_cnt >> PSGoper[i1].eg_sh_rr) & 7)];
                                if (PSGoper[i1].volume >= 0x3ff)
                                {
                                    PSGoper[i1].volume = 0x3ff;
                                    PSGoper[i1].state = 0;
                                }
                            }
                            break;
                    }
                    i1++;
                    i--;
                } while (i != 0);
            }
        }

        //private static void advance_eg()
        //{
        //    uint i;
        //    int i1 = 0;
        //    PSG.eg_timer += PSG.eg_timer_add;
        //    while (PSG.eg_timer >= PSG.eg_timer_overflow)
        //    {
        //        PSG.eg_timer -= PSG.eg_timer_overflow;
        //        PSG.eg_cnt++;
        //        /* envelope generator */
        //        //op = PSG.oper[i1];	/* CH 0 M1 */
        //        i = 32;
        //        do
        //        {
        //            switch (PSG.oper[i1].state)
        //            {
        //                case 4:	/* attack phase */
        //                    if ((PSG.eg_cnt & ((1 << PSG.oper[i1].eg_sh_ar) - 1)) == 0)
        //                    {
        //                        PSG.oper[i1].volume += (~PSG.oper[i1].volume *
        //                                       (eg_inc[PSG.oper[i1].eg_sel_ar + ((PSG.eg_cnt >> PSG.oper[i1].eg_sh_ar) & 7)])
        //                                      ) >> 4;

        //                        if (PSG.oper[i1].volume <= 0)
        //                        {
        //                            PSG.oper[i1].volume = 0;
        //                            PSG.oper[i1].state = 3;
        //                        }
        //                    }
        //                    break;
        //                case 3:	/* decay phase */
        //                    if ((PSG.eg_cnt & ((1 << PSG.oper[i1].eg_sh_d1r) - 1)) == 0)
        //                    {
        //                        PSG.oper[i1].volume += eg_inc[PSG.oper[i1].eg_sel_d1r + ((PSG.eg_cnt >> PSG.oper[i1].eg_sh_d1r) & 7)];
        //                        if (PSG.oper[i1].volume >= PSG.oper[i1].d1l)
        //                            PSG.oper[i1].state = 2;
        //                    }
        //                    break;
        //                case 2:	/* sustain phase */
        //                    if ((PSG.eg_cnt & ((1 << PSG.oper[i1].eg_sh_d2r) - 1)) == 0)
        //                    {
        //                        PSG.oper[i1].volume += eg_inc[PSG.oper[i1].eg_sel_d2r + ((PSG.eg_cnt >> PSG.oper[i1].eg_sh_d2r) & 7)];
        //                        if (PSG.oper[i1].volume >= 0x3ff)
        //                        {
        //                            PSG.oper[i1].volume = 0x3ff;
        //                            PSG.oper[i1].state = 0;
        //                        }
        //                    }
        //                    break;
        //                case 1:	/* release phase */
        //                    if ((PSG.eg_cnt & ((1 << PSG.oper[i1].eg_sh_rr) - 1)) == 0)
        //                    {
        //                        PSG.oper[i1].volume += eg_inc[PSG.oper[i1].eg_sel_rr + ((PSG.eg_cnt >> PSG.oper[i1].eg_sh_rr) & 7)];
        //                        if (PSG.oper[i1].volume >= 0x3ff)
        //                        {
        //                            PSG.oper[i1].volume = 0x3ff;
        //                            PSG.oper[i1].state = 0;
        //                        }
        //                    }
        //                    break;
        //            }
        //            i1++;
        //            i--;
        //        } while (i != 0);
        //    }
        //}
        unsafe static void advance(YM2151Operator* PSGoper, uint* PSGfreq)
        {
            uint i;
            int a, p;
            /* LFO */
            if ((PSG.test & 2) != 0)
            {
                PSG.lfo_phase = 0;
            }
            else
            {
                PSG.lfo_timer += PSG.lfo_timer_add;
                if (PSG.lfo_timer >= PSG.lfo_overflow)
                {
                    PSG.lfo_timer -= PSG.lfo_overflow;
                    PSG.lfo_counter += PSG.lfo_counter_add;
                    PSG.lfo_phase += (PSG.lfo_counter >> 4);
                    PSG.lfo_phase &= 255;
                    PSG.lfo_counter &= 15;
                }
            }
            i = PSG.lfo_phase;
            /* calculate LFO AM and PM waveform value (all verified on real chip, except for noise algorithm which is impossible to analyse)*/
            switch (PSG.lfo_wsel)
            {
                case 0:
                    /* saw */
                    /* AM: 255 down to 0 */
                    /* PM: 0 to 127, -127 to 0 (at PMD=127: LFP = 0 to 126, -126 to 0) */
                    a = (int)(255 - i);
                    if (i < 128)
                        p = (int)i;
                    else
                        p = (int)(i - 255);
                    break;
                case 1:
                    /* square */
                    /* AM: 255, 0 */
                    /* PM: 128,-128 (LFP = exactly +PMD, -PMD) */
                    if (i < 128)
                    {
                        a = 255;
                        p = 128;
                    }
                    else
                    {
                        a = 0;
                        p = -128;
                    }
                    break;
                case 2:
                    /* triangle */
                    /* AM: 255 down to 1 step -2; 0 up to 254 step +2 */
                    /* PM: 0 to 126 step +2, 127 to 1 step -2, 0 to -126 step -2, -127 to -1 step +2*/
                    if (i < 128)
                        a = (int)(255 - (i * 2));
                    else
                        a = (int)((i * 2) - 256);
                    if (i < 64)						/* i = 0..63 */
                        p = (int)(i * 2);					/* 0 to 126 step +2 */
                    else if (i < 128)					/* i = 64..127 */
                        p = (int)(255 - i * 2);			/* 127 to 1 step -2 */
                    else if (i < 192)				/* i = 128..191 */
                        p = (int)(256 - i * 2);		/* 0 to -126 step -2*/
                    else					/* i = 192..255 */
                        p = (int)(i * 2 - 511);		/*-127 to -1 step +2*/
                    break;
                case 3:
                default:	/*keep the compiler happy*/
                    a = lfo_noise_waveform[i];
                    p = a - 128;
                    break;
            }
            PSG.lfa = (uint)(a * PSG.amd / 128);
            PSG.lfp = p * PSG.pmd / 128;
            PSG.noise_p += PSG.noise_f;
            i = (PSG.noise_p >> 16);		/* number of events (shifts of the shift register) */
            PSG.noise_p &= 0xffff;
            while (i != 0)
            {
                uint j;
                j = ((PSG.noise_rng ^ (PSG.noise_rng >> 3)) & 1) ^ 1;
                PSG.noise_rng = (j << 16) | (PSG.noise_rng >> 1);
                i--;
            }
            /* phase generator */
            uint i1 = 0;
            //op = PSGoper[i1];	/* CH 0 M1 */
            i = 8;
            do
            {
                if (PSGoper[i1].pms != 0)	/* only when phase modulation from LFO is enabled for this channel */
                {
                    int mod_ind = PSG.lfp;		/* -128..+127 (8bits signed) */
                    if (PSGoper[i1].pms < 6)
                        mod_ind >>= (int)(6 - PSGoper[i1].pms);
                    else
                        mod_ind <<= (int)(PSGoper[i1].pms - 5);
                    if (mod_ind != 0)
                    {
                        uint kc_channel = (uint)(PSGoper[i1].kc_i + mod_ind);
                        PSGoper[i1].phase += (uint)(((PSGfreq[kc_channel + PSGoper[i1].dt2] + PSGoper[i1].dt1) * PSGoper[i1].mul) >> 1);
                        PSGoper[i1 + 1].phase += (uint)(((PSGfreq[kc_channel + PSGoper[i1 + 1].dt2] + PSGoper[i1 + 1].dt1) * PSGoper[i1 + 1].mul) >> 1);
                        PSGoper[i1 + 2].phase += (uint)(((PSGfreq[kc_channel + PSGoper[i1 + 2].dt2] + PSGoper[i1 + 2].dt1) * PSGoper[i1 + 2].mul) >> 1);
                        PSGoper[i1 + 3].phase += (uint)(((PSGfreq[kc_channel + PSGoper[i1 + 3].dt2] + PSGoper[i1 + 3].dt1) * PSGoper[i1 + 3].mul) >> 1);
                    }
                    else		/* phase modulation from LFO is equal to zero */
                    {
                        PSGoper[i1].phase += PSGoper[i1].freq;
                        PSGoper[i1 + 1].phase += PSGoper[i1 + 1].freq;
                        PSGoper[i1 + 2].phase += PSGoper[i1 + 2].freq;
                        PSGoper[i1 + 3].phase += PSGoper[i1 + 3].freq;
                    }
                }
                else			/* phase modulation from LFO is disabled */
                {
                    PSGoper[i1].phase += PSGoper[i1].freq;
                    PSGoper[i1 + 1].phase += PSGoper[i1 + 1].freq;
                    PSGoper[i1 + 2].phase += PSGoper[i1 + 2].freq;
                    PSGoper[i1 + 3].phase += PSGoper[i1 + 3].freq;
                }
                i1 += 4;
                i--;
            } while (i != 0);
            if (PSG.csm_req != 0)			/* CSM KEYON/KEYOFF seqeunce request */
            {
                if (PSG.csm_req == 2)	/* KEY ON */
                {
                    i1 = 0;
                    PSGoper[i1] = PSGoper[i1];	/* CH 0 M1 */
                    i = 32;
                    do
                    {
                        KEY_ON(i1, 2);
                        i1++;
                        i--;
                    } while (i != 0);
                    PSG.csm_req = 1;
                }
                else					/* KEY OFF */
                {
                    i1 = 0;
                    PSGoper[i1] = PSGoper[i1];	/* CH 0 M1 */
                    i = 32;
                    do
                    {
                        KEY_OFF(i1, 0xfffffffe);
                        i1++;
                        i--;
                    } while (i != 0);
                    PSG.csm_req = 0;
                }
            }
        }
        //private static void advance()
        //{
        //    uint i;
        //    int a, p;
        //    /* LFO */
        //    if ((PSG.test & 2) != 0)
        //    {
        //        PSG.lfo_phase = 0;
        //    }
        //    else
        //    {
        //        PSG.lfo_timer += PSG.lfo_timer_add;
        //        if (PSG.lfo_timer >= PSG.lfo_overflow)
        //        {
        //            PSG.lfo_timer -= PSG.lfo_overflow;
        //            PSG.lfo_counter += PSG.lfo_counter_add;
        //            PSG.lfo_phase += (PSG.lfo_counter >> 4);
        //            PSG.lfo_phase &= 255;
        //            PSG.lfo_counter &= 15;
        //        }
        //    }
        //    i = PSG.lfo_phase;
        //    /* calculate LFO AM and PM waveform value (all verified on real chip, except for noise algorithm which is impossible to analyse)*/
        //    switch (PSG.lfo_wsel)
        //    {
        //        case 0:
        //            /* saw */
        //            /* AM: 255 down to 0 */
        //            /* PM: 0 to 127, -127 to 0 (at PMD=127: LFP = 0 to 126, -126 to 0) */
        //            a = (int)(255 - i);
        //            if (i < 128)
        //                p = (int)i;
        //            else
        //                p = (int)(i - 255);
        //            break;
        //        case 1:
        //            /* square */
        //            /* AM: 255, 0 */
        //            /* PM: 128,-128 (LFP = exactly +PMD, -PMD) */
        //            if (i < 128)
        //            {
        //                a = 255;
        //                p = 128;
        //            }
        //            else
        //            {
        //                a = 0;
        //                p = -128;
        //            }
        //            break;
        //        case 2:
        //            /* triangle */
        //            /* AM: 255 down to 1 step -2; 0 up to 254 step +2 */
        //            /* PM: 0 to 126 step +2, 127 to 1 step -2, 0 to -126 step -2, -127 to -1 step +2*/
        //            if (i < 128)
        //                a = (int)(255 - (i * 2));
        //            else
        //                a = (int)((i * 2) - 256);
        //            if (i < 64)						/* i = 0..63 */
        //                p = (int)(i * 2);					/* 0 to 126 step +2 */
        //            else if (i < 128)					/* i = 64..127 */
        //                p = (int)(255 - i * 2);			/* 127 to 1 step -2 */
        //            else if (i < 192)				/* i = 128..191 */
        //                p = (int)(256 - i * 2);		/* 0 to -126 step -2*/
        //            else					/* i = 192..255 */
        //                p = (int)(i * 2 - 511);		/*-127 to -1 step +2*/
        //            break;
        //        case 3:
        //        default:	/*keep the compiler happy*/
        //            a = lfo_noise_waveform[i];
        //            p = a - 128;
        //            break;
        //    }
        //    PSG.lfa = (uint)(a * PSG.amd / 128);
        //    PSG.lfp = p * PSG.pmd / 128;
        //    PSG.noise_p += PSG.noise_f;
        //    i = (PSG.noise_p >> 16);		/* number of events (shifts of the shift register) */
        //    PSG.noise_p &= 0xffff;
        //    while (i != 0)
        //    {
        //        uint j;
        //        j = ((PSG.noise_rng ^ (PSG.noise_rng >> 3)) & 1) ^ 1;
        //        PSG.noise_rng = (j << 16) | (PSG.noise_rng >> 1);
        //        i--;
        //    }
        //    /* phase generator */
        //    uint i1 = 0;
        //    //op = PSG.oper[i1];	/* CH 0 M1 */
        //    i = 8;
        //    do
        //    {
        //        if (PSG.oper[i1].pms != 0)	/* only when phase modulation from LFO is enabled for this channel */
        //        {
        //            int mod_ind = PSG.lfp;		/* -128..+127 (8bits signed) */
        //            if (PSG.oper[i1].pms < 6)
        //                mod_ind >>= (int)(6 - PSG.oper[i1].pms);
        //            else
        //                mod_ind <<= (int)(PSG.oper[i1].pms - 5);
        //            if (mod_ind != 0)
        //            {
        //                uint kc_channel = (uint)(PSG.oper[i1].kc_i + mod_ind);
        //                PSG.oper[i1].phase += (uint)(((PSG.freq[kc_channel + PSG.oper[i1].dt2] + PSG.oper[i1].dt1) * PSG.oper[i1].mul) >> 1);
        //                PSG.oper[i1 + 1].phase += (uint)(((PSG.freq[kc_channel + PSG.oper[i1 + 1].dt2] + PSG.oper[i1 + 1].dt1) * PSG.oper[i1 + 1].mul) >> 1);
        //                PSG.oper[i1 + 2].phase += (uint)(((PSG.freq[kc_channel + PSG.oper[i1 + 2].dt2] + PSG.oper[i1 + 2].dt1) * PSG.oper[i1 + 2].mul) >> 1);
        //                PSG.oper[i1 + 3].phase += (uint)(((PSG.freq[kc_channel + PSG.oper[i1 + 3].dt2] + PSG.oper[i1 + 3].dt1) * PSG.oper[i1 + 3].mul) >> 1);
        //            }
        //            else		/* phase modulation from LFO is equal to zero */
        //            {
        //                PSG.oper[i1].phase += PSG.oper[i1].freq;
        //                PSG.oper[i1 + 1].phase += PSG.oper[i1 + 1].freq;
        //                PSG.oper[i1 + 2].phase += PSG.oper[i1 + 2].freq;
        //                PSG.oper[i1 + 3].phase += PSG.oper[i1 + 3].freq;
        //            }
        //        }
        //        else			/* phase modulation from LFO is disabled */
        //        {
        //            PSG.oper[i1].phase += PSG.oper[i1].freq;
        //            PSG.oper[i1 + 1].phase += PSG.oper[i1 + 1].freq;
        //            PSG.oper[i1 + 2].phase += PSG.oper[i1 + 2].freq;
        //            PSG.oper[i1 + 3].phase += PSG.oper[i1 + 3].freq;
        //        }
        //        i1 += 4;
        //        i--;
        //    } while (i != 0);
        //    if (PSG.csm_req != 0)			/* CSM KEYON/KEYOFF seqeunce request */
        //    {
        //        if (PSG.csm_req == 2)	/* KEY ON */
        //        {
        //            i1 = 0;
        //            PSG.oper[i1] = PSG.oper[i1];	/* CH 0 M1 */
        //            i = 32;
        //            do
        //            {
        //                KEY_ON(i1, 2);
        //                i1++;
        //                i--;
        //            } while (i != 0);
        //            PSG.csm_req = 1;
        //        }
        //        else					/* KEY OFF */
        //        {
        //            i1 = 0;
        //            PSG.oper[i1] = PSG.oper[i1];	/* CH 0 M1 */
        //            i = 32;
        //            do
        //            {
        //                KEY_OFF(i1, 0xfffffffe);
        //                i1++;
        //                i--;
        //            } while (i != 0);
        //            PSG.csm_req = 0;
        //        }
        //    }
        //}

        static bool skiprun_update_one;
        static int run_update_one_Index;
        public unsafe static void ym2151_update_one(int offset, int length)
        {
            fixed (uint* PSGpanPtr = &PSG.pan[0])
            fixed (uint* PSGfreqPtr = &PSG.freq[0])
            fixed (YM2151Operator* PSGoperPtr = &PSG.oper[0])
            fixed (int* chanoutPtr = &chanout[0])
            //fixed (int* streamoutput0Ptr = &Sound.ym2151stream.streamoutput_Ptrs[0][0])
            //fixed (int* streamoutput1Ptr = &Sound.ym2151stream.streamoutput_Ptrs[0][1])
            fixed (int* imemPtr = &imem[0])
            {
                YM2151Operator* PSGoper = PSGoperPtr;
                uint* PSGpan = PSGpanPtr;
                uint* PSGfreq = PSGfreqPtr;
                int* chanout = chanoutPtr;
                //int* streamoutput0 = streamoutput0Ptr;
                //int* streamoutput1 = streamoutput1Ptr;
                int* streamoutput0 = &Sound.ym2151stream.streamoutput_Ptrs[0][0];
                int* streamoutput1 = &Sound.ym2151stream.streamoutput_Ptrs[0][1];
                int* imem = imemPtr;

                int i;
                int outl, outr;
                for (i = 0; i < length; i++)
                {
                    advance_eg(PSGoper);
                    chanout[0] = 0;
                    chanout[1] = 0;
                    chanout[2] = 0;
                    chanout[3] = 0;
                    chanout[4] = 0;
                    chanout[5] = 0;
                    chanout[6] = 0;
                    chanout[7] = 0;

                    //二分交替混音
                    //skiprun_update_one = !skiprun_update_one;
                    //if (!skiprun_update_one)
                    //{
                    //    chan_calc(PSGoper, chanout, imem, 0);
                    //    chan_calc(PSGoper, chanout, imem, 2);
                    //    chan_calc(PSGoper, chanout, imem, 4);
                    //    chan_calc(PSGoper, chanout, imem, 6);
                    //}
                    //else
                    //{
                    //    chan_calc(PSGoper, chanout, imem, 1);
                    //    chan_calc(PSGoper, chanout, imem, 3);
                    //    chan_calc(PSGoper, chanout, imem, 5);
                    //    chan7_calc(PSGoper, chanout, imem);
                    //}

                    //三分交替混音
                    //run_update_one_Index++;
                    //if (run_update_one_Index > 2)
                    //    run_update_one_Index = 0;
                    //switch (run_update_one_Index)
                    //{
                    //    case 0:
                    //        chan_calc(PSGoper, chanout, imem, 0);
                    //        chan_calc(PSGoper, chanout, imem, 2);
                    //        chan_calc(PSGoper, chanout, imem, 4);
                    //        break;
                    //    case 1:
                    //        chan_calc(PSGoper, chanout, imem, 1);
                    //        chan_calc(PSGoper, chanout, imem, 3);
                    //        chan7_calc(PSGoper, chanout, imem);
                    //        break;
                    //    case 2:
                    //        chan_calc(PSGoper, chanout, imem, 0);
                    //        chan_calc(PSGoper, chanout, imem, 5);
                    //        chan_calc(PSGoper, chanout, imem, 6);
                    //        break;
                    //}

                    //完全渲染混音
                    chan_calc(PSGoper, chanout, imem, 0);
                    chan_calc(PSGoper, chanout, imem, 1);
                    chan_calc(PSGoper, chanout, imem, 2);
                    chan_calc(PSGoper, chanout, imem, 3);
                    chan_calc(PSGoper, chanout, imem, 4);
                    chan_calc(PSGoper, chanout, imem, 5);
                    chan_calc(PSGoper, chanout, imem, 6);
                    chan7_calc(PSGoper, chanout, imem);

                    outl = (int)(chanout[0] & PSGpan[0]);
                    outr = (int)(chanout[0] & PSGpan[1]);
                    outl += (int)(chanout[1] & PSGpan[2]);
                    outr += (int)(chanout[1] & PSGpan[3]);
                    outl += (int)(chanout[2] & PSGpan[4]);
                    outr += (int)(chanout[2] & PSGpan[5]);
                    outl += (int)(chanout[3] & PSGpan[6]);
                    outr += (int)(chanout[3] & PSGpan[7]);
                    outl += (int)(chanout[4] & PSGpan[8]);
                    outr += (int)(chanout[4] & PSGpan[9]);
                    outl += (int)(chanout[5] & PSGpan[10]);
                    outr += (int)(chanout[5] & PSGpan[11]);
                    outl += (int)(chanout[6] & PSGpan[12]);
                    outr += (int)(chanout[6] & PSGpan[13]);
                    outl += (int)(chanout[7] & PSGpan[14]);
                    outr += (int)(chanout[7] & PSGpan[15]);
                    if (outl > 32767)
                    {
                        outl = 32767;
                    }
                    else if (outl < -32768)
                    {
                        outl = -32768;
                    }
                    if (outr > 32767)
                    {
                        outr = 32767;
                    }
                    else if (outr < -32768)
                    {
                        outr = -32768;
                    }
                    streamoutput0[offset + i] = outl;
                    streamoutput1[offset + i] = outr;
                    advance(PSGoper, PSGfreq);
                }
            }
        }
        //public static void ym2151_update_one(int offset, int length)
        //{
        //    int i;
        //    int outl, outr;
        //    for (i = 0; i < length; i++)
        //    {
        //        advance_eg();
        //        chanout[0] = 0;
        //        chanout[1] = 0;
        //        chanout[2] = 0;
        //        chanout[3] = 0;
        //        chanout[4] = 0;
        //        chanout[5] = 0;
        //        chanout[6] = 0;
        //        chanout[7] = 0;
        //        chan_calc(0);
        //        chan_calc(1);
        //        chan_calc(2);
        //        chan_calc(3);
        //        chan_calc(4);
        //        chan_calc(5);
        //        chan_calc(6);
        //        chan7_calc();
        //        outl = (int)(chanout[0] & PSG.pan[0]);
        //        outr = (int)(chanout[0] & PSG.pan[1]);
        //        outl += (int)(chanout[1] & PSG.pan[2]);
        //        outr += (int)(chanout[1] & PSG.pan[3]);
        //        outl += (int)(chanout[2] & PSG.pan[4]);
        //        outr += (int)(chanout[2] & PSG.pan[5]);
        //        outl += (int)(chanout[3] & PSG.pan[6]);
        //        outr += (int)(chanout[3] & PSG.pan[7]);
        //        outl += (int)(chanout[4] & PSG.pan[8]);
        //        outr += (int)(chanout[4] & PSG.pan[9]);
        //        outl += (int)(chanout[5] & PSG.pan[10]);
        //        outr += (int)(chanout[5] & PSG.pan[11]);
        //        outl += (int)(chanout[6] & PSG.pan[12]);
        //        outr += (int)(chanout[6] & PSG.pan[13]);
        //        outl += (int)(chanout[7] & PSG.pan[14]);
        //        outr += (int)(chanout[7] & PSG.pan[15]);
        //        if (outl > 32767)
        //        {
        //            outl = 32767;
        //        }
        //        else if (outl < -32768)
        //        {
        //            outl = -32768;
        //        }
        //        if (outr > 32767)
        //        {
        //            outr = 32767;
        //        }
        //        else if (outr < -32768)
        //        {
        //            outr = -32768;
        //        }
        //        Sound.ym2151stream.streamoutput_Ptrs[0][offset + i] = outl;
        //        Sound.ym2151stream.streamoutput_Ptrs[1][offset + i] = outr;
        //        advance();
        //    }
        //}
        public static byte ym2151_status_port_0_r()
        {
            Sound.ym2151stream.stream_update();
            return (byte)PSG.status;
        }
        public static void ym2151_register_port_0_w(byte data)
        {
            PSG.lastreg0 = data;
        }
        public static void ym2151_data_port_0_w(byte data)
        {
            Sound.ym2151stream.stream_update();
            ym2151_write_reg(PSG.lastreg0, data);
        }

        unsafe static void set_value1(int* chanout, YM2151Operator* PSGoper, int op1)
        {
            if (iconnect[op1] == 12)
            {
                chanout[9] = chanout[10] = chanout[11] = PSGoper[op1].fb_out_prev;
            }
            else
            {
                chanout[iconnect[op1]] = PSGoper[op1].fb_out_prev;
            }
        }
        //private static void set_value1(int op1)
        //{
        //    if (iconnect[op1] == 12)
        //    {
        //        chanout[9] = chanout[10] = chanout[11] = PSG.oper[op1].fb_out_prev;
        //    }
        //    else
        //    {
        //        chanout[iconnect[op1]] = PSG.oper[op1].fb_out_prev;
        //    }
        //}
        unsafe static void set_value2(int* chanout, int op1, int i)
        {
            if (iconnect[op1] == 12)
            {
                return;
            }
            else
            {
                chanout[iconnect[op1]] += i;
            }
        }

        //private static void set_value2(int op1, int i)
        //{
        //    if (iconnect[op1] == 12)
        //    {
        //        return;
        //    }
        //    else
        //    {
        //        chanout[iconnect[op1]] += i;
        //    }
        //}
        unsafe static void set_mem(YM2151Operator* PSGoper, int* chanout, int* imem, int op1)
        {
            if (imem[op1] == 8 || imem[op1] == 10 || imem[op1] == 11)
            {
                chanout[imem[op1]] = PSGoper[op1].mem_value;
            }
        }
        //private static void set_mem(int op1)
        //{
        //    if (imem[op1] == 8 || imem[op1] == 10 || imem[op1] == 11)
        //    {
        //        chanout[imem[op1]] = PSG.oper[op1].mem_value;
        //    }
        //}
        public static void SaveStateBinary(BinaryWriter writer)
        {
            int i;
            for (i = 0; i < 32; i++)
            {
                writer.Write(PSG.oper[i].phase);
                writer.Write(PSG.oper[i].freq);
                writer.Write(PSG.oper[i].dt1);
                writer.Write(PSG.oper[i].mul);
                writer.Write(PSG.oper[i].dt1_i);
                writer.Write(PSG.oper[i].dt2);
                writer.Write(PSG.oper[i].mem_value);
                writer.Write(PSG.oper[i].fb_shift);
                writer.Write(PSG.oper[i].fb_out_curr);
                writer.Write(PSG.oper[i].fb_out_prev);
                writer.Write(PSG.oper[i].kc);
                writer.Write(PSG.oper[i].kc_i);
                writer.Write(PSG.oper[i].pms);
                writer.Write(PSG.oper[i].ams);
                writer.Write(PSG.oper[i].AMmask);
                writer.Write(PSG.oper[i].state);
                writer.Write(PSG.oper[i].eg_sh_ar);
                writer.Write(PSG.oper[i].eg_sel_ar);
                writer.Write(PSG.oper[i].tl);
                writer.Write(PSG.oper[i].volume);
                writer.Write(PSG.oper[i].eg_sh_d1r);
                writer.Write(PSG.oper[i].eg_sel_d1r);
                writer.Write(PSG.oper[i].d1l);
                writer.Write(PSG.oper[i].eg_sh_d2r);
                writer.Write(PSG.oper[i].eg_sel_d2r);
                writer.Write(PSG.oper[i].eg_sh_rr);
                writer.Write(PSG.oper[i].eg_sel_rr);
                writer.Write(PSG.oper[i].key);
                writer.Write(PSG.oper[i].ks);
                writer.Write(PSG.oper[i].ar);
                writer.Write(PSG.oper[i].d1r);
                writer.Write(PSG.oper[i].d2r);
                writer.Write(PSG.oper[i].rr);
                writer.Write(PSG.oper[i].reserved0);
                writer.Write(PSG.oper[i].reserved1);
            }
            for (i = 0; i < 16; i++)
            {
                writer.Write(PSG.pan[i]);
            }
            writer.Write(PSG.lastreg0);
            writer.Write(PSG.eg_cnt);
            writer.Write(PSG.eg_timer);
            writer.Write(PSG.eg_timer_add);
            writer.Write(PSG.eg_timer_overflow);
            writer.Write(PSG.lfo_phase);
            writer.Write(PSG.lfo_timer);
            writer.Write(PSG.lfo_timer_add);
            writer.Write(PSG.lfo_overflow);
            writer.Write(PSG.lfo_counter);
            writer.Write(PSG.lfo_counter_add);
            writer.Write(PSG.lfo_wsel);
            writer.Write(PSG.amd);
            writer.Write(PSG.pmd);
            writer.Write(PSG.lfa);
            writer.Write(PSG.lfp);
            writer.Write(PSG.test);
            writer.Write(PSG.ct);
            writer.Write(PSG.noise);
            writer.Write(PSG.noise_rng);
            writer.Write(PSG.noise_p);
            writer.Write(PSG.noise_f);
            writer.Write(PSG.csm_req);
            writer.Write(PSG.irq_enable);
            writer.Write(PSG.status);
            writer.Write(PSG.timer_A_index);
            writer.Write(PSG.timer_B_index);
            writer.Write(PSG.timer_A_index_old);
            writer.Write(PSG.timer_B_index_old);
            writer.Write(PSG.irqlinestate);
            writer.Write(PSG.connect);
        }
        public static void LoadStateBinary(BinaryReader reader)
        {
            int i;
            for (i = 0; i < 32; i++)
            {
                PSG.oper[i].phase = reader.ReadUInt32();
                PSG.oper[i].freq = reader.ReadUInt32();
                PSG.oper[i].dt1 = reader.ReadInt32();
                PSG.oper[i].mul = reader.ReadUInt32();
                PSG.oper[i].dt1_i = reader.ReadUInt32();
                PSG.oper[i].dt2 = reader.ReadUInt32();
                PSG.oper[i].mem_value = reader.ReadInt32();
                PSG.oper[i].fb_shift = reader.ReadUInt32();
                PSG.oper[i].fb_out_curr = reader.ReadInt32();
                PSG.oper[i].fb_out_prev = reader.ReadInt32();
                PSG.oper[i].kc = reader.ReadUInt32();
                PSG.oper[i].kc_i = reader.ReadUInt32();
                PSG.oper[i].pms = reader.ReadUInt32();
                PSG.oper[i].ams = reader.ReadUInt32();
                PSG.oper[i].AMmask = reader.ReadUInt32();
                PSG.oper[i].state = reader.ReadUInt32();
                PSG.oper[i].eg_sh_ar = reader.ReadByte();
                PSG.oper[i].eg_sel_ar = reader.ReadByte();
                PSG.oper[i].tl = reader.ReadUInt32();
                PSG.oper[i].volume = reader.ReadInt32();
                PSG.oper[i].eg_sh_d1r = reader.ReadByte();
                PSG.oper[i].eg_sel_d1r = reader.ReadByte();
                PSG.oper[i].d1l = reader.ReadUInt32();
                PSG.oper[i].eg_sh_d2r = reader.ReadByte();
                PSG.oper[i].eg_sel_d2r = reader.ReadByte();
                PSG.oper[i].eg_sh_rr = reader.ReadByte();
                PSG.oper[i].eg_sel_rr = reader.ReadByte();
                PSG.oper[i].key = reader.ReadUInt32();
                PSG.oper[i].ks = reader.ReadUInt32();
                PSG.oper[i].ar = reader.ReadUInt32();
                PSG.oper[i].d1r = reader.ReadUInt32();
                PSG.oper[i].d2r = reader.ReadUInt32();
                PSG.oper[i].rr = reader.ReadUInt32();
                PSG.oper[i].reserved0 = reader.ReadUInt32();
                PSG.oper[i].reserved1 = reader.ReadUInt32();
            }
            for (i = 0; i < 16; i++)
            {
                PSG.pan[i] = reader.ReadUInt32();
            }
            PSG.lastreg0 = reader.ReadInt32();
            PSG.eg_cnt = reader.ReadUInt32();
            PSG.eg_timer = reader.ReadUInt32();
            PSG.eg_timer_add = reader.ReadUInt32();
            PSG.eg_timer_overflow = reader.ReadUInt32();
            PSG.lfo_phase = reader.ReadUInt32();
            PSG.lfo_timer = reader.ReadUInt32();
            PSG.lfo_timer_add = reader.ReadUInt32();
            PSG.lfo_overflow = reader.ReadUInt32();
            PSG.lfo_counter = reader.ReadUInt32();
            PSG.lfo_counter_add = reader.ReadUInt32();
            PSG.lfo_wsel = reader.ReadByte();
            PSG.amd = reader.ReadByte();
            PSG.pmd = reader.ReadSByte();
            PSG.lfa = reader.ReadUInt32();
            PSG.lfp = reader.ReadInt32();
            PSG.test = reader.ReadByte();
            PSG.ct = reader.ReadByte();
            PSG.noise = reader.ReadUInt32();
            PSG.noise_rng = reader.ReadUInt32();
            PSG.noise_p = reader.ReadUInt32();
            PSG.noise_f = reader.ReadUInt32();
            PSG.csm_req = reader.ReadUInt32();
            PSG.irq_enable = reader.ReadUInt32();
            PSG.status = reader.ReadUInt32();
            PSG.timer_A_index = reader.ReadUInt32();
            PSG.timer_B_index = reader.ReadUInt32();
            PSG.timer_A_index_old = reader.ReadUInt32();
            PSG.timer_B_index_old = reader.ReadUInt32();
            PSG.irqlinestate = reader.ReadInt32();
            PSG.connect = reader.ReadBytes(8);
        }
    }
}