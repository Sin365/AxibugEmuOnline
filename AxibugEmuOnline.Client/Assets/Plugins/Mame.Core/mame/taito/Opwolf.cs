using System;

namespace MAME.Core
{
    public unsafe partial class Taito
    {
        public static int opwolf_region;
        public static byte[] cchip_ram, adpcmrom;
        public static byte[] adpcm_b = new byte[0x08];
        public static byte[] adpcm_c = new byte[0x08];
        public static int opwolf_gun_xoffs, opwolf_gun_yoffs;
        public static byte dswa, dswb, p1x, p1y;
        public static int[] adpcm_pos = new int[2], adpcm_end = new int[2];
        public static int[] adpcm_data = new int[2];
        public static ushort m_sprite_ctrl;
        public static ushort m_sprites_flipscreen;
        public static byte current_bank = 0;
        public static byte current_cmd = 0;
        public static byte cchip_last_7a = 0;
        public static byte cchip_last_04 = 0;
        public static byte cchip_last_05 = 0;
        public static byte[] cchip_coins_for_credit = new byte[2];
        public static byte[] cchip_credits_for_coin = new byte[2];
        public static byte[] cchip_coins = new byte[2];
        public static byte c588 = 0, c589 = 0, c58a = 0;
        public static byte m_triggeredLevel1b; // These variables derived from comparison to unprotection version
        public static byte m_triggeredLevel2;
        public static byte m_triggeredLevel2b;
        public static byte m_triggeredLevel2c;
        public static byte m_triggeredLevel3b;
        public static byte m_triggeredLevel13b;
        public static byte m_triggeredLevel4;
        public static byte m_triggeredLevel5;
        public static byte m_triggeredLevel7;
        public static byte m_triggeredLevel8;
        public static byte m_triggeredLevel9;
        public static ushort[] level_data_00 = new ushort[]
        {
            0x0480, 0x1008, 0x0300,   0x5701, 0x0001, 0x0010,
            0x0480, 0x1008, 0x0300,   0x5701, 0x0001, 0x002b,
            0x0780, 0x0009, 0x0300,   0x4a01, 0x0004, 0x0020,
            0x0780, 0x1208, 0x0300,   0x5d01, 0x0004, 0x0030,
            0x0780, 0x0209, 0x0300,   0x4c01, 0x0004, 0x0038,
            0x0780, 0x0309, 0x0300,   0x4d01, 0x0004, 0x0048,
            0x0980, 0x1108, 0x0300,   0x5a01, 0xc005, 0x0018,
            0x0980, 0x0109, 0x0300,   0x4b01, 0xc005, 0x0028,
            0x0b80, 0x020a, 0x0000,   0x6401, 0x8006, 0x0004,
            0x0c80, 0x010b, 0x0000,   0xf201, 0x8006, 0x8002,
            0x0b80, 0x020a, 0x0000,   0x6401, 0x8006, 0x0017,
            0x0c80, 0x010b, 0x0000,   0xf201, 0x8006, 0x8015,
            0x0b80, 0x020a, 0x0000,   0x6401, 0x0007, 0x0034,
            0x0c80, 0x010b, 0x0000,   0xf201, 0x0007, 0x8032,
            0x0b80, 0x020a, 0x0000,   0x6401, 0x8006, 0x803e,
            0x0c80, 0x010b, 0x0000,   0xf201, 0x8006, 0x803d,
            0x0b80, 0x100a, 0x0000,   0x6001, 0x0007, 0x0008,
            0x0b80, 0x100a, 0x0000,   0x6001, 0x0007, 0x000b,
            0x0b80, 0x100a, 0x0000,   0x6001, 0x0007, 0x001b,
            0x0b80, 0x100a, 0x0000,   0x6001, 0x0007, 0x001e,
            0x0b80, 0x100a, 0x0000,   0x6001, 0x8007, 0x0038,
            0x0b80, 0x100a, 0x0000,   0x6001, 0x8007, 0x003b,
            0x0b80, 0x100a, 0x0000,   0x6001, 0x0007, 0x8042,
            0x0b80, 0x100a, 0x0000,   0x6001, 0x0007, 0x8045,
            0x0c80, 0x000b, 0x0000,   0xf101, 0x800b, 0x8007,
            0x0c80, 0x000b, 0x0000,   0xf101, 0x800b, 0x801a,
            0x0c80, 0x000b, 0x0000,   0xf101, 0x000c, 0x8037,
            0x0c80, 0x000b, 0x0000,   0xf101, 0x800b, 0x0042,
            0x0c80, 0xd04b, 0x0000,   0xf301, 0x8006, 0x8009,
            0x0c80, 0xd04b, 0x0000,   0xf301, 0x8006, 0x801c,
            0x0c80, 0xd04b, 0x0000,   0xf301, 0x8006, 0x0044,
            0x0c80, 0x030b, 0x0000,   0xf401, 0x0008, 0x0028,
            0x0c80, 0x030b, 0x0000,   0xf401, 0x0008, 0x804b,
            0x0c00, 0x040b, 0x0000,   0xf501, 0x0008, 0x8026,
            0xffff
        };
        public static ushort[] level_data_01 = new ushort[]
        {
            0x0780, 0x0209, 0x0300,   0x4c01, 0x0004, 0x0010,
            0x0780, 0x0209, 0x0300,   0x4c01, 0x4004, 0x0020,
            0x0780, 0x0309, 0x0300,   0x4d01, 0xe003, 0x0030,
            0x0780, 0x0309, 0x0300,   0x4d01, 0x8003, 0x0040,
            0x0780, 0x0209, 0x0300,   0x4c01, 0x8004, 0x0018,
            0x0780, 0x0309, 0x0300,   0x4d01, 0xc003, 0x0028,
            0x0b80, 0x000b, 0x0000,   0x0b02, 0x8009, 0x0029,
            0x0b80, 0x0409, 0x0000,   0x0f02, 0x8008, 0x8028,
            0x0b80, 0x040a, 0x0000,   0x3502, 0x000a, 0x8028,
            0x0b80, 0x050a, 0x0000,   0x1002, 0x8006, 0x8028,
            0x0b80, 0x120a, 0x0000,   0x3602, 0x0008, 0x004d,
            0x0b80, 0x120a, 0x0000,   0x3602, 0x0008, 0x004f,
            0x0b80, 0x120a, 0x0000,   0x3602, 0x0008, 0x0001,
            0x0b80, 0x120a, 0x0000,   0x3602, 0x0008, 0x0003,
            0x0b80, 0x130a, 0x0000,   0x3a02, 0x0007, 0x0023,
            0x0b80, 0x130a, 0x0000,   0x3a02, 0x0007, 0x8025,
            0x0b80, 0x130a, 0x0000,   0x3a02, 0x8009, 0x0023,
            0x0b80, 0x130a, 0x0000,   0x3a02, 0x8009, 0x8025,
            0x0b80, 0x140a, 0x0000,   0x3e02, 0x0007, 0x000d,
            0x0b80, 0x140a, 0x0000,   0x3e02, 0x0007, 0x800f,
            0x0b80, 0x000b, 0x0000,   0x0102, 0x0007, 0x804e,
            0x0b80, 0xd24b, 0x0000,   0x0302, 0x0007, 0x000e,
            0x0b80, 0x000b, 0x0000,   0x0402, 0x8006, 0x0020,
            0x0b80, 0xd34b, 0x0000,   0x0502, 0x8006, 0x0024,
            0x0b80, 0x000b, 0x0000,   0x0602, 0x8009, 0x0001,
            0x0b80, 0xd44b, 0x0000,   0x0702, 0x800b, 0x800b,
            0x0b80, 0xd54b, 0x0000,   0x0802, 0x800b, 0x000e,
            0x0b80, 0x000b, 0x0000,   0x0902, 0x800b, 0x0010,
            0x0b80, 0x000b, 0x0000,   0x0a02, 0x0009, 0x0024,
            0x0b80, 0xd64b, 0x0000,   0x0c02, 0x000c, 0x8021,
            0x0b80, 0x000b, 0x0000,   0x0d02, 0x000c, 0x0025,
            0x0b80, 0x000b, 0x0000,   0x0e02, 0x8009, 0x004e,
            0x0b80, 0x000b, 0x0300,   0x4e01, 0x8006, 0x8012,
            0x0b80, 0x000b, 0x0300,   0x4e01, 0x0007, 0x8007,
            0xffff
        };
        public static ushort[] level_data_02 = new ushort[]
        {
            0x0480, 0x000b, 0x0300,   0x4501, 0x0001, 0x0018,
            0x0480, 0x000b, 0x0300,   0x4501, 0x2001, 0x0030,
            0x0780, 0x1208, 0x0300,   0x5d01, 0x0004, 0x0010,
            0x0780, 0x1208, 0x0300,   0x5d01, 0x2004, 0x001c,
            0x0780, 0x1208, 0x0300,   0x5d01, 0xe003, 0x0026,
            0x0780, 0x1208, 0x0300,   0x5d01, 0x8003, 0x0034,
            0x0780, 0x1208, 0x0300,   0x5d01, 0x3004, 0x0040,
            0x0780, 0x010c, 0x0300,   0x4601, 0x4004, 0x0022,
            0x0780, 0x010c, 0x0300,   0x4601, 0x6004, 0x0042,
            0x0780, 0x000c, 0x0500,   0x7b01, 0x800b, 0x0008,
            0x0780, 0x010c, 0x0300,   0x4601, 0x2004, 0x0008,
            0x0000, 0x0000, 0x0000,   0xf001, 0x0000, 0x0000,
            0x0000, 0x0000, 0x0000,   0xf001, 0x0000, 0x0000,
            0x0000, 0x0000, 0x0000,   0xf001, 0x0000, 0x0000,
            0x0b80, 0x000b, 0x0000,   0x1902, 0x000b, 0x0004,
            0x0b80, 0x000b, 0x0000,   0x1a02, 0x0009, 0x8003,
            0x0b80, 0x000b, 0x0000,   0x1902, 0x000b, 0x000c,
            0x0b80, 0x000b, 0x0000,   0x1a02, 0x0009, 0x800b,
            0x0b80, 0x000b, 0x0000,   0x1902, 0x000b, 0x001c,
            0x0b80, 0x000b, 0x0000,   0x1a02, 0x0009, 0x801b,
            0x0b80, 0x000b, 0x0000,   0x1902, 0x000b, 0x002c,
            0x0b80, 0x000b, 0x0000,   0x1a02, 0x0009, 0x802b,
            0x0b80, 0x000b, 0x0000,   0x1902, 0x000b, 0x0044,
            0x0b80, 0x000b, 0x0000,   0x1a02, 0x0009, 0x8043,
            0x0b80, 0x000b, 0x0000,   0x1902, 0x000b, 0x004c,
            0x0b80, 0x000b, 0x0000,   0x1a02, 0x0009, 0x804b,
            0x0b80, 0x020c, 0x0300,   0x4801, 0xa009, 0x0010,
            0x0b80, 0x020c, 0x0300,   0x4801, 0xa009, 0x0028,
            0x0b80, 0x020c, 0x0300,   0x4801, 0xa009, 0x0036,
            0x0000, 0x0000, 0x0000,   0xf001, 0x0000, 0x0000,
            0x0000, 0x0000, 0x0000,   0xf001, 0x0000, 0x0000,
            0x0000, 0x0000, 0x0000,   0xf001, 0x0000, 0x0000,
            0x0000, 0x0000, 0x0000,   0xf001, 0x0000, 0x0000,
            0x0000, 0x0000, 0x0000,   0xf001, 0x0000, 0x0000,
            0xffff
        };
        public static ushort[] level_data_03 = new ushort[]
        {
            0x0480, 0x000b, 0x0300,   0x4501, 0x0001, 0x0018,
            0x0480, 0x000b, 0x0300,   0x4501, 0x2001, 0x002b,
            0x0780, 0x010c, 0x0300,   0x4601, 0x0004, 0x000d,
            0x0780, 0x000c, 0x0500,   0x7b01, 0x800b, 0x0020,
            0x0780, 0x010c, 0x0300,   0x4601, 0x2004, 0x0020,
            0x0780, 0x010c, 0x0300,   0x4601, 0x8003, 0x0033,
            0x0780, 0x010c, 0x0300,   0x4601, 0x0004, 0x003c,
            0x0780, 0x010c, 0x0300,   0x4601, 0xd003, 0x0045,
            0x0780, 0x000c, 0x0500,   0x7b01, 0x900b, 0x0041,
            0x0780, 0x010c, 0x0300,   0x4601, 0x3004, 0x0041,
            0x0b80, 0x020c, 0x0300,   0x4801, 0x0007, 0x0000,
            0x0b80, 0x410a, 0x0000,   0x2b02, 0xe006, 0x4049,
            0x0b80, 0x020c, 0x0300,   0x4801, 0x8007, 0x000b,
            0x0b80, 0x000b, 0x0000,   0x2702, 0x800a, 0x8005,
            0x0b80, 0x000b, 0x0000,   0x1e02, 0x0008, 0x800e,
            0x0b80, 0x000b, 0x0000,   0x1f02, 0x8007, 0x0011,
            0x0b80, 0x000b, 0x0000,   0x2802, 0x000b, 0x0012,
            0x0b80, 0x000b, 0x0000,   0x2002, 0x0007, 0x8015,
            0x0b80, 0x000b, 0x0000,   0x2102, 0x0007, 0x801b,
            0x0b80, 0x000b, 0x0000,   0x2902, 0x800a, 0x001a,
            0x0b80, 0x000b, 0x0000,   0x2202, 0x8007, 0x001e,
            0x0b80, 0x000b, 0x0000,   0x1e02, 0x0008, 0x0025,
            0x0b80, 0x000b, 0x0000,   0x2302, 0x8007, 0x802c,
            0x0b80, 0x000b, 0x0000,   0x2802, 0x000b, 0x8028,
            0x0b80, 0x020c, 0x0300,   0x4801, 0x0007, 0x0030,
            0x0b80, 0x400a, 0x0000,   0x2e02, 0x4007, 0x002d,
            0x0b80, 0x000b, 0x0000,   0x2702, 0x800a, 0x8035,
            0x0b80, 0x020c, 0x0300,   0x4801, 0x8007, 0x0022,
            0x0b80, 0x000b, 0x0000,   0x2402, 0x8007, 0x0047,
            0x0b80, 0x000b, 0x0000,   0x2a02, 0x800a, 0x004b,
            0x0b80, 0x000b, 0x0000,   0x2502, 0x0007, 0x804b,
            0x0b80, 0x000b, 0x0000,   0x2602, 0x0007, 0x004e,
            0x0b80, 0x020c, 0x0300,   0x4801, 0x0007, 0x8043,
            0x0b80, 0x020c, 0x0300,   0x4801, 0x8007, 0x803d,
            0xffff
        };
        public static ushort[] level_data_04 = new ushort[]
        {
            0x0780, 0x0209, 0x0300,   0x4c01, 0x0004, 0x0010,
            0x0780, 0x0209, 0x0300,   0x4c01, 0x4004, 0x0020,
            0x0780, 0x0309, 0x0300,   0x4d01, 0xe003, 0x0030,
            0x0780, 0x0309, 0x0300,   0x4d01, 0x8003, 0x0040,
            0x0780, 0x0209, 0x0300,   0x4c01, 0x8004, 0x0018,
            0x0780, 0x0309, 0x0300,   0x4d01, 0xc003, 0x0028,
            0x0780, 0x000b, 0x0300,   0x5601, 0x8004, 0x0008,
            0x0780, 0x000b, 0x0300,   0x5601, 0x8004, 0x0038,
            0x0780, 0x000b, 0x0300,   0x5501, 0x8004, 0x0048,
            0x0980, 0x0509, 0x0f00,   0x0f01, 0x4005, 0x4007,
            0x0980, 0x0509, 0x0f00,   0x0f01, 0x4005, 0x4037,
            0x0b80, 0x030a, 0x0000,   0x1302, 0x8006, 0x0040,
            0x0b80, 0x110a, 0x0000,   0x1502, 0x8008, 0x8048,
            0x0b80, 0x110a, 0x0000,   0x1502, 0x8008, 0x8049,
            0x0b80, 0x000b, 0x0000,   0xf601, 0x0007, 0x8003,
            0x0b80, 0x000b, 0x0000,   0xf701, 0x0007, 0x0005,
            0x0b80, 0x000b, 0x0000,   0xf901, 0x0007, 0x8008,
            0x0b80, 0x000b, 0x0000,   0xf901, 0x0007, 0x0010,
            0x0b80, 0x000b, 0x0000,   0xfa01, 0x0007, 0x8013,
            0x0b80, 0x000b, 0x0000,   0xf801, 0x800b, 0x800b,
            0x0b80, 0x000b, 0x0000,   0x0002, 0x800b, 0x801a,
            0x0b80, 0x000b, 0x0000,   0xf901, 0x0007, 0x8017,
            0x0b80, 0x000b, 0x0000,   0xfa01, 0x0007, 0x001b,
            0x0b80, 0x000b, 0x0000,   0xf801, 0x800b, 0x0013,
            0x0b80, 0x000b, 0x0000,   0x4202, 0x800b, 0x0016,
            0x0b80, 0x000b, 0x0000,   0xfb01, 0x8007, 0x8020,
            0x0b80, 0x000b, 0x0000,   0xf601, 0x0007, 0x8023,
            0x0b80, 0x000b, 0x0000,   0x4202, 0x800b, 0x800e,
            0x0b80, 0x000b, 0x0000,   0x4302, 0x800b, 0x801d,
            0x0b80, 0x000b, 0x0000,   0xf701, 0x0007, 0x0025,
            0x0b80, 0x000b, 0x0000,   0xfd01, 0x8006, 0x003f,
            0x0b80, 0x000b, 0x0000,   0xfe01, 0x0007, 0x0046,
            0x0b80, 0x000b, 0x0000,   0xff01, 0x8007, 0x8049,
            0x0b80, 0x000b, 0x0000,   0xfc01, 0x8009, 0x0042,
            0xffff
        };
        public static ushort[] level_data_05 = new ushort[]
        {
            0x0480, 0x1008, 0x0300,   0x5701, 0x0001, 0x0010,
            0x0480, 0x1008, 0x0300,   0x5701, 0x0001, 0x002b,
            0x0780, 0x0009, 0x0300,   0x4a01, 0x0004, 0x0020,
            0x0780, 0x1208, 0x0300,   0x5d01, 0x0004, 0x0030,
            0x0780, 0x0209, 0x0300,   0x4c01, 0x0004, 0x0038,
            0x0780, 0x0309, 0x0300,   0x4d01, 0x0004, 0x0048,
            0x0980, 0x1108, 0x0300,   0x5a01, 0xc005, 0x0018,
            0x0980, 0x0109, 0x0300,   0x4b01, 0xc005, 0x0028,
            0x0b80, 0x020a, 0x0000,   0x6401, 0x8006, 0x0004,
            0x0c80, 0x010b, 0x0000,   0xf201, 0x8006, 0x8002,
            0x0b80, 0x020a, 0x0000,   0x6401, 0x8006, 0x0017,
            0x0c80, 0x010b, 0x0000,   0xf201, 0x8006, 0x8015,
            0x0b80, 0x020a, 0x0000,   0x6401, 0x0007, 0x0034,
            0x0c80, 0x010b, 0x0000,   0xf201, 0x0007, 0x8032,
            0x0b80, 0x020a, 0x0000,   0x6401, 0x8006, 0x803e,
            0x0c80, 0x010b, 0x0000,   0xf201, 0x8006, 0x803d,
            0x0b80, 0x100a, 0x0000,   0x6001, 0x0007, 0x0008,
            0x0b80, 0x100a, 0x0000,   0x6001, 0x0007, 0x000b,
            0x0b80, 0x100a, 0x0000,   0x6001, 0x0007, 0x001b,
            0x0b80, 0x100a, 0x0000,   0x6001, 0x0007, 0x001e,
            0x0b80, 0x100a, 0x0000,   0x6001, 0x8007, 0x0038,
            0x0b80, 0x100a, 0x0000,   0x6001, 0x8007, 0x003b,
            0x0b80, 0x100a, 0x0000,   0x6001, 0x0007, 0x8042,
            0x0b80, 0x100a, 0x0000,   0x6001, 0x0007, 0x8045,
            0x0c80, 0x000b, 0x0000,   0xf101, 0x800b, 0x8007,
            0x0c80, 0x000b, 0x0000,   0xf101, 0x800b, 0x801a,
            0x0c80, 0x000b, 0x0000,   0xf101, 0x000c, 0x8037,
            0x0c80, 0x000b, 0x0000,   0xf101, 0x800b, 0x0042,
            0x0c80, 0xd04b, 0x0000,   0xf301, 0x8006, 0x8009,
            0x0c80, 0xd04b, 0x0000,   0xf301, 0x8006, 0x801c,
            0x0c80, 0xd04b, 0x0000,   0xf301, 0x8006, 0x0044,
            0x0c80, 0x030b, 0x0000,   0xf401, 0x0008, 0x0028,
            0x0c80, 0x030b, 0x0000,   0xf401, 0x0008, 0x804b,
            0x0c00, 0x040b, 0x0000,   0xf501, 0x0008, 0x8026,
            0xffff
        };
        public static ushort[] level_data_06 = new ushort[]
        {
            0x0000, 0x1008, 0x0300,   0x5701, 0x0001, 0x0010,
            0x0000, 0x1008, 0x0300,   0x5701, 0x0001, 0x002b,
            0x0000, 0x0000, 0x0000,   0x0000, 0x0000, 0x0000,
            0x0700, 0x0009, 0x0300,   0x4a01, 0x0004, 0x0020,
            0x0700, 0x1208, 0x0300,   0x5d01, 0x0004, 0x0030,
            0x0700, 0x0209, 0x0300,   0x4c01, 0x0004, 0x0038,
            0x0700, 0x0309, 0x0300,   0x4d01, 0x0004, 0x0048,
            0x0900, 0x1108, 0x0300,   0x5a01, 0xc005, 0x0018,
            0x0900, 0x0109, 0x0300,   0x4b01, 0xc005, 0x0028,
            0x0000, 0x000b, 0x0000,   0x0000, 0x0018, 0x0000,
            0x0000, 0x000b, 0x0000,   0x0000, 0x0018, 0x0000,
            0x0000, 0x000b, 0x0000,   0x0000, 0x0018, 0x0000,
            0x0000, 0x000b, 0x0000,   0x0000, 0x0018, 0x0000,
            0x0000, 0x000b, 0x0000,   0x0000, 0x0018, 0x0000,
            0x0000, 0x000b, 0x0000,   0x0000, 0x0018, 0x0000,
            0x0000, 0x000b, 0x0000,   0x0000, 0x0018, 0x0000,
            0x0000, 0x000b, 0x0000,   0x0000, 0x0018, 0x0000,
            0x0000, 0x000b, 0x0000,   0x0000, 0x0018, 0x0000,
            0x0980, 0xdb4c, 0x0000,   0x3202, 0x0006, 0x0004,
            0x0000, 0x000b, 0x0000,   0x0000, 0x0018, 0x0000,
            0x0000, 0x000b, 0x0000,   0x0000, 0x0018, 0x0000,
            0x0000, 0x000b, 0x0000,   0x0000, 0x0018, 0x0000,
            0x0000, 0x000b, 0x0000,   0x0000, 0x0018, 0x0000,
            0x0000, 0x000b, 0x0000,   0x0000, 0x0018, 0x0000,
            0x0000, 0x000b, 0x0000,   0x0000, 0x0018, 0x0000,
            0x0000, 0x000b, 0x0000,   0x0000, 0x0018, 0x0000,
            0x0000, 0x000b, 0x0000,   0x0000, 0x0018, 0x0000,
            0x0000, 0x000b, 0x0000,   0x0000, 0x0018, 0x0000,
            0x0000, 0x000b, 0x0000,   0x0000, 0x0018, 0x0000,
            0x0000, 0x000b, 0x0000,   0x0000, 0x0018, 0x0000,
            0x0000, 0x000b, 0x0000,   0x0000, 0x0018, 0x0000,
            0x0000, 0x000b, 0x0000,   0x0000, 0x0018, 0x0000,
            0x0000, 0x000b, 0x0000,   0x0000, 0x0018, 0x0000,
            0x0000, 0x000b, 0x0000,   0x0000, 0x0018, 0x0000,
            0xffff
        };
        public static ushort[] level_data_07 = new ushort[]{
            0x0480, 0x000b, 0x0300,   0x4501, 0x0001, 0x0001,
            0x0000, 0x0000, 0x0000,   0xf001, 0x0000, 0x0000,
            0x0780, 0x0109, 0x0300,   0x4a01, 0x0004, 0x0004,
            0x0780, 0x0009, 0x0300,   0x4a01, 0x0004, 0x000d,
            0x0780, 0x000c, 0x0500,   0x7b01, 0x000c, 0x0005,
            0x0780, 0x000c, 0x0540,   0x7b01, 0x000c, 0x0005,
            0x0780, 0x010c, 0x0300,   0x4601, 0x0005, 0x0005,
            0x0780, 0x000c, 0x0500,   0x7b01, 0x800b, 0xc00d,
            0x0780, 0x000c, 0x0540,   0x7b01, 0x800b, 0xc00d,
            0x0780, 0x010c, 0x0300,   0x4601, 0x8004, 0xc00d,
            0x0900, 0x0109, 0x0340,   0x4b01, 0x2006, 0x400c,
            0x0780, 0x020c, 0x0300,   0x4801, 0x8007, 0x0008,
            0x0780, 0x020c, 0x0300,   0x4801, 0x4007, 0xc00b,
            0x0980, 0x0109, 0x0300,   0x4b01, 0xc006, 0x8007,
            0x0980, 0x0109, 0x0300,   0x4b01, 0x8007, 0x8008,
            0x0980, 0x0109, 0x0300,   0x4b01, 0xc006, 0x800c,
            0x0000, 0x0000, 0x0000,   0xf001, 0x0000, 0x0000,
            0x0000, 0x0000, 0x0000,   0xf001, 0x0000, 0x0000,
            0x0000, 0x0000, 0x0000,   0xf001, 0x0000, 0x0000,
            0x0000, 0x0000, 0x0000,   0xf001, 0x0000, 0x0000,
            0x0000, 0x0000, 0x0000,   0xf001, 0x0000, 0x0000,
            0x0000, 0x0000, 0x0000,   0xf001, 0x0000, 0x0000,
            0x0000, 0x0000, 0x0000,   0xf001, 0x0000, 0x0000,
            0x0000, 0x0000, 0x0000,   0xf001, 0x0000, 0x0000,
            0x0000, 0x0000, 0x0000,   0xf001, 0x0000, 0x0000,
            0x0000, 0x0000, 0x0000,   0xf001, 0x0000, 0x0000,
            0x0000, 0x0000, 0x0000,   0xf001, 0x0000, 0x0000,
            0x0000, 0x0000, 0x0000,   0xf001, 0x0000, 0x0000,
            0x0000, 0x0000, 0x0000,   0xf001, 0x0000, 0x0000,
            0x0000, 0x0000, 0x0000,   0xf001, 0x0000, 0x0000,
            0x0000, 0x0000, 0x0000,   0xf001, 0x0000, 0x0000,
            0x0000, 0x0000, 0x0000,   0xf001, 0x0000, 0x0000,
            0xffff
        };
        public static ushort[] level_data_08 = new ushort[]{
            0xffff
        };
        public static ushort[] level_data_09 = new ushort[]{
            0x0000, 0x0000, 0x0000,   0xf001, 0x0000, 0x0000,
            0x0000, 0x0000, 0x0000,   0xf001, 0x0000, 0x0000,
            0x0780, 0x0109, 0x0300,   0x4a01, 0x8003, 0x8003,
            0x0780, 0x0009, 0x0300,   0x4a01, 0x0004, 0x800e,
            0x0780, 0x000c, 0x0500,   0x7b01, 0x000c, 0x0005,
            0x0780, 0x000c, 0x0540,   0x7b01, 0x000c, 0x0005,
            0x0780, 0x010c, 0x0300,   0x4601, 0x0005, 0x0005,
            0x0780, 0x000c, 0x0500,   0x7b01, 0x800b, 0xc00d,
            0x0780, 0x000c, 0x0540,   0x7b01, 0x800b, 0xc00d,
            0x0780, 0x010c, 0x0300,   0x4601, 0x8004, 0xc00d,
            0x0900, 0x0109, 0x0340,   0x4b01, 0x2006, 0x400c,
            0x0780, 0x020c, 0x0300,   0x4801, 0x8007, 0x0008,
            0x0780, 0x020c, 0x0300,   0x4801, 0x4007, 0xc00b,
            0x0980, 0x0109, 0x0300,   0x4b01, 0xc006, 0x8007,
            0x0980, 0x0109, 0x0300,   0x4b01, 0x8007, 0x8008,
            0x0980, 0x0109, 0x0300,   0x4b01, 0xc006, 0x800c,
            0x0000, 0x0000, 0x0000,   0xf001, 0x0000, 0x0000,
            0x0000, 0x0000, 0x0000,   0xf001, 0x0000, 0x0000,
            0x0000, 0x0000, 0x0000,   0xf001, 0x0000, 0x0000,
            0x0000, 0x0000, 0x0000,   0xf001, 0x0000, 0x0000,
            0x0000, 0x0000, 0x0000,   0xf001, 0x0000, 0x0000,
            0x0000, 0x0000, 0x0000,   0xf001, 0x0000, 0x0000,
            0x0000, 0x0000, 0x0000,   0xf001, 0x0000, 0x0000,
            0x0000, 0x0000, 0x0000,   0xf001, 0x0000, 0x0000,
            0x0000, 0x0000, 0x0000,   0xf001, 0x0000, 0x0000,
            0x0000, 0x0000, 0x0000,   0xf001, 0x0000, 0x0000,
            0x0000, 0x0000, 0x0000,   0xf001, 0x0000, 0x0000,
            0x0000, 0x0000, 0x0000,   0xf001, 0x0000, 0x0000,
            0x0000, 0x0000, 0x0000,   0xf001, 0x0000, 0x0000,
            0x0000, 0x0000, 0x0000,   0xf001, 0x0000, 0x0000,
            0x0000, 0x0000, 0x0000,   0xf001, 0x0000, 0x0000,
            0x0000, 0x0000, 0x0000,   0xf001, 0x0000, 0x0000,
            0xffff
        };
        public static ushort[][] level_data_lookup = new ushort[][]
        {
            level_data_00,
            level_data_01,
            level_data_02,
            level_data_03,
            level_data_04,
            level_data_05,
            level_data_06,
            level_data_07,
            level_data_08,
            level_data_09
        };
        public static byte cchip_r(int offset)
        {
            return cchip_ram[offset];
        }
        public static void cchip_w(int offset, byte data)
        {
            cchip_ram[offset] = data;
        }
        public static byte opwolf_in_r(int offset)
        {
            byte result = 0;
            if (offset == 0)
            {
                result = (byte)sbyte0;
            }
            else if (offset == 1)
            {
                result = (byte)sbyte1;
            }
            return result;
        }
        public static byte opwolf_dsw_r(int offset)
        {
            byte result = 0;
            if (offset == 0)
            {
                result = dswa;
            }
            else if (offset == 1)
            {
                result = dswb;
            }
            return result;
        }
        public static ushort opwolf_lightgun_r(int offset)
        {
            ushort result = 0;
            if (offset == 0)
            {
                result = (ushort)(0xfe00 | opwolf_gun_x_r());
            }
            else if (offset == 1)
            {
                result = (ushort)(0xfe00 | opwolf_gun_y_r());
            }
            return result;
        }
        public static ushort opwolf_lightgun_r_p(int offset)
        {
            ushort result = 0;
            if (offset == 0)
            {
                result = (ushort)((sbyte2 << 8) | opwolf_gun_x_r());
            }
            else if (offset == 1)
            {
                result = (ushort)((sbyte3 << 8) | opwolf_gun_y_r());
            }
            return result;
        }
        public static byte z80_input1_r()
        {
            byte result;
            result = (byte)sbyte0;
            return result;
        }
        public static byte z80_input2_r()
        {
            byte result;
            result = (byte)sbyte0;
            return result;
        }
        public static void sound_bankswitch_w(int offset, byte data)
        {
            basebanksnd = 0x10000 + 0x4000 * ((data - 1) & 0x03);
        }
        public static void machine_reset_opwolf()
        {
            adpcm_b[0] = adpcm_b[1] = 0;
            adpcm_c[0] = adpcm_c[1] = 0;
            adpcm_pos[0] = adpcm_pos[1] = 0;
            adpcm_end[0] = adpcm_end[1] = 0;
            adpcm_data[0] = adpcm_data[1] = -1;
            m_sprite_ctrl = 0;
            m_sprites_flipscreen = 0;
            MSM5205.msm5205_reset_w(0, 1);
            MSM5205.msm5205_reset_w(1, 1);
        }
        public static void opwolf_msm5205_vck(int chip)
        {
            if (adpcm_data[chip] != -1)
            {
                MSM5205.msm5205_data_w(chip, adpcm_data[chip] & 0x0f);
                adpcm_data[chip] = -1;
                if (adpcm_pos[chip] == adpcm_end[chip])
                {
                    MSM5205.msm5205_reset_w(chip, 1);
                }
            }
            else
            {
                adpcm_data[chip] = adpcmrom[adpcm_pos[chip]];
                adpcm_pos[chip] = (adpcm_pos[chip] + 1) & 0x7ffff;
                MSM5205.msm5205_data_w(chip, adpcm_data[chip] >> 4);
            }
        }
        public static void opwolf_adpcm_b_w(int offset, byte data)
        {
            int start;
            int end;
            adpcm_b[offset] = data;
            if (offset == 0x04) //trigger ?
            {
                start = adpcm_b[0] + adpcm_b[1] * 256;
                end = adpcm_b[2] + adpcm_b[3] * 256;
                start *= 16;
                end *= 16;
                adpcm_pos[0] = start;
                adpcm_end[0] = end;
                MSM5205.msm5205_reset_w(0, 0);
            }
        }
        public static void opwolf_adpcm_c_w(int offset, byte data)
        {
            int start;
            int end;
            adpcm_c[offset] = data;
            if (offset == 0x04) //trigger ?
            {
                start = adpcm_c[0] + adpcm_c[1] * 256;
                end = adpcm_c[2] + adpcm_c[3] * 256;
                start *= 16;
                end *= 16;
                adpcm_pos[1] = start;
                adpcm_end[1] = end;
                MSM5205.msm5205_reset_w(1, 0);
            }
        }
        public static void opwolf_adpcm_d_w()
        {

        }
        public static void opwolf_adpcm_e_w()
        {

        }
        public static int opwolf_gun_x_r()
        {
            p1x = (byte)Inptport.input_port_read_direct(Inptport.analog_p1x);
            int scaled = (p1x * 320) / 256;
            return (scaled + 0x15 + opwolf_gun_xoffs);
        }
        public static int opwolf_gun_y_r()
        {
            p1y = (byte)Inptport.input_port_read_direct(Inptport.analog_p1y);
            return (p1y - 0x24 + opwolf_gun_yoffs);
        }
        public static void irq_handler(int irq)
        {
            Cpuint.cpunum_set_input_line(1, 0, irq != 0 ? LineState.ASSERT_LINE : LineState.CLEAR_LINE);
        }
        public static void opwolf_timer_callback()
        {
            if (current_cmd == 0xf5)
            {
                int level = cchip_ram[0x1b] % 10;
                int i = 0;
                Array.Clear(cchip_ram, 0x200, 0x200);
                for (i = 0; (i < 0x200) && (level_data_lookup[level][i] != 0xffff); i += 3)
                {
                    cchip_ram[0x200 + i * 2 + 0] = (byte)(level_data_lookup[level][i] >> 8);
                    cchip_ram[0x200 + i * 2 + 1] = (byte)(level_data_lookup[level][i] & 0xff);
                    cchip_ram[0x200 + i * 2 + 2] = (byte)(level_data_lookup[level][i + 1] >> 8);
                    cchip_ram[0x200 + i * 2 + 3] = (byte)(level_data_lookup[level][i + 1] & 0xff);
                    cchip_ram[0x200 + i * 2 + 4] = (byte)(level_data_lookup[level][i + 2] >> 8);
                    cchip_ram[0x200 + i * 2 + 5] = (byte)(level_data_lookup[level][i + 2] & 0xff);
                }
                // The bootleg cchip writes 0 to these locations - hard to tell what the real one writes
                cchip_ram[0x0] = 0;
                cchip_ram[0x76] = 0;
                cchip_ram[0x75] = 0;
                cchip_ram[0x74] = 0;
                cchip_ram[0x72] = 0;
                cchip_ram[0x71] = 0;
                //cchip_ram[0x70]=0;
                cchip_ram[0x66] = 0;
                cchip_ram[0x2b] = 0;
                cchip_ram[0x30] = 0;
                cchip_ram[0x31] = 0;
                cchip_ram[0x32] = 0;
                cchip_ram[0x27] = 0;
                c588 = 0;
                c589 = 0;
                c58a = 0;
                m_triggeredLevel1b = 0;
                m_triggeredLevel13b = 0;
                m_triggeredLevel2 = 0;
                m_triggeredLevel2b = 0;
                m_triggeredLevel2c = 0;
                m_triggeredLevel3b = 0;
                m_triggeredLevel4 = 0;
                m_triggeredLevel5 = 0;
                m_triggeredLevel7 = 0;
                m_triggeredLevel8 = 0;
                m_triggeredLevel9 = 0;
                cchip_ram[0x1a] = 0;
                cchip_ram[0x7a] = 1; // Signal command complete
            }
            current_cmd = 0;
        }
        public static void updateDifficulty(int mode)
        {
            // The game is made up of 6 rounds, when you complete the
            // sixth you return to the start but with harder difficulty.
            if (mode == 0)
            {
                switch (cchip_ram[0x15] & 3) // Dipswitch B
                {
                    case 3:
                        cchip_ram[0x2c] = 0x31;
                        cchip_ram[0x77] = 0x05;
                        cchip_ram[0x25] = 0x0f;
                        cchip_ram[0x26] = 0x0b;
                        break;
                    case 0:
                        cchip_ram[0x2c] = 0x20;
                        cchip_ram[0x77] = 0x06;
                        cchip_ram[0x25] = 0x07;
                        cchip_ram[0x26] = 0x03;
                        break;
                    case 1:
                        cchip_ram[0x2c] = 0x31;
                        cchip_ram[0x77] = 0x05;
                        cchip_ram[0x25] = 0x0f;
                        cchip_ram[0x26] = 0x0b;
                        break;
                    case 2:
                        cchip_ram[0x2c] = 0x3c;
                        cchip_ram[0x77] = 0x04;
                        cchip_ram[0x25] = 0x13;
                        cchip_ram[0x26] = 0x0f;
                        break;
                }
            }
            else
            {
                switch (cchip_ram[0x15] & 3) // Dipswitch B
                {
                    case 3:
                        cchip_ram[0x2c] = 0x46;
                        cchip_ram[0x77] = 0x05;
                        cchip_ram[0x25] = 0x11;
                        cchip_ram[0x26] = 0x0e;
                        break;
                    case 0:
                        cchip_ram[0x2c] = 0x30;
                        cchip_ram[0x77] = 0x06;
                        cchip_ram[0x25] = 0x0b;
                        cchip_ram[0x26] = 0x03;
                        break;
                    case 1:
                        cchip_ram[0x2c] = 0x3a;
                        cchip_ram[0x77] = 0x05;
                        cchip_ram[0x25] = 0x0f;
                        cchip_ram[0x26] = 0x09;
                        break;
                    case 2:
                        cchip_ram[0x2c] = 0x4c;
                        cchip_ram[0x77] = 0x04;
                        cchip_ram[0x25] = 0x19;
                        cchip_ram[0x26] = 0x11;
                        break;
                }
            }
        }
        public static void opwolf_cchip_status_w()
        {
            cchip_ram[0x3d] = 1;
            cchip_ram[0x7a] = 1;
            updateDifficulty(0);
        }
        public static void opwolf_cchip_bank_w(byte data)
        {
            current_bank = (byte)(data & 7);
        }
        public static void opwolf_cchip_data_w(int offset, ushort data)
        {
            cchip_ram[(current_bank * 0x400) + offset] = (byte)(data & 0xff);
            if (current_bank == 0)
            {
                // Dip switch A is written here by the 68k - precalculate the coinage values
                // Shouldn't we directly read the values from the ROM area ?
                if (offset == 0x14)
                {
                    int[] coin_table = new int[] { 0, 0 };
                    byte[] coin_offset = new byte[2];
                    int slot;

                    if ((opwolf_region == 1) || (opwolf_region == 2))
                    {
                        coin_table[0] = 0x03ffce;
                        coin_table[1] = 0x03ffce;
                    }
                    if ((opwolf_region == 3) || (opwolf_region == 4))
                    {
                        coin_table[0] = 0x03ffde;
                        coin_table[1] = 0x03ffee;
                    }
                    coin_offset[0] = (byte)(12 - (4 * ((data & 0x30) >> 4)));
                    coin_offset[1] = (byte)(12 - (4 * ((data & 0xc0) >> 6)));
                    for (slot = 0; slot < 2; slot++)
                    {
                        if (coin_table[slot] != 0)
                        {
                            cchip_coins_for_credit[slot] = (byte)((Memory.mainrom[(coin_table[slot] + coin_offset[slot] + 0) / 2 * 2] * 0x100 + Memory.mainrom[(coin_table[slot] + coin_offset[slot] + 0) / 2 * 2 + 1]) & 0xff);
                            cchip_credits_for_coin[slot] = (byte)((Memory.mainrom[(coin_table[slot] + coin_offset[slot] + 2) / 2 * 2] * 0x100 + Memory.mainrom[(coin_table[slot] + coin_offset[slot] + 2) / 2 * 2 + 1]) & 0xff);
                        }
                    }
                }
                // Dip switch B
                if (offset == 0x15)
                {
                    updateDifficulty(0);
                }
            }
        }
        public static void opwolf_cchip_data_w2(int offset, byte data)
        {
            cchip_ram[(current_bank * 0x400) + offset] = (byte)(data & 0xff);
            if (current_bank == 0)
            {
                // Dip switch A is written here by the 68k - precalculate the coinage values
                // Shouldn't we directly read the values from the ROM area ?
                if (offset == 0x14)
                {
                    int[] coin_table = new int[] { 0, 0 };
                    byte[] coin_offset = new byte[2];
                    int slot;

                    if ((opwolf_region == 1) || (opwolf_region == 2))
                    {
                        coin_table[0] = 0x03ffce;
                        coin_table[1] = 0x03ffce;
                    }
                    if ((opwolf_region == 3) || (opwolf_region == 4))
                    {
                        coin_table[0] = 0x03ffde;
                        coin_table[1] = 0x03ffee;
                    }
                    coin_offset[0] = (byte)(12 - (4 * ((data & 0x30) >> 4)));
                    coin_offset[1] = (byte)(12 - (4 * ((data & 0xc0) >> 6)));
                    for (slot = 0; slot < 2; slot++)
                    {
                        if (coin_table[slot] != 0)
                        {
                            cchip_coins_for_credit[slot] = (byte)((Memory.mainrom[(coin_table[slot] + coin_offset[slot] + 0) / 2 * 2] * 0x100 + Memory.mainrom[(coin_table[slot] + coin_offset[slot] + 0) / 2 * 2 + 1]) & 0xff);
                            cchip_credits_for_coin[slot] = (byte)((Memory.mainrom[(coin_table[slot] + coin_offset[slot] + 2) / 2 * 2] * 0x100 + Memory.mainrom[(coin_table[slot] + coin_offset[slot] + 2) / 2 * 2 + 1]) & 0xff);
                        }
                    }
                }
                // Dip switch B
                if (offset == 0x15)
                {
                    updateDifficulty(0);
                }
            }
        }
        public static ushort opwolf_cchip_status_r()
        {
            return 0x1;
        }
        public static ushort opwolf_cchip_data_r(int offset)
        {
            return cchip_ram[(current_bank * 0x400) + offset];
        }
        public static void cchip_timer()
        {
            cchip_ram[0x4] = (byte)sbyte0;
            cchip_ram[0x5] = (byte)sbyte1;
            if (cchip_ram[0x4] != cchip_last_04)
            {
                int slot = -1;
                if ((cchip_ram[0x4] & 1) != 0)
                {
                    slot = 0;
                }
                if ((cchip_ram[0x4] & 2) != 0)
                {
                    slot = 1;
                }
                if (slot != -1)
                {
                    cchip_coins[slot]++;
                    if (cchip_coins[slot] >= cchip_coins_for_credit[slot])
                    {
                        cchip_ram[0x53] += cchip_credits_for_coin[slot];
                        cchip_ram[0x51] = 0x55;
                        cchip_ram[0x52] = 0x55;
                        cchip_coins[slot] -= cchip_coins_for_credit[slot];
                    }
                    Generic.coin_counter_w(slot, 1);
                }
                if (cchip_ram[0x53] > 9)
                {
                    cchip_ram[0x53] = 9;
                }
            }
            cchip_last_04 = cchip_ram[0x4];
            if (cchip_ram[0x5] != cchip_last_05)
            {
                if ((cchip_ram[0x5] & 4) == 0)
                {
                    cchip_ram[0x53]++;
                    cchip_ram[0x51] = 0x55;
                    cchip_ram[0x52] = 0x55;
                }
            }
            cchip_last_05 = cchip_ram[0x5];
            Generic.coin_lockout_w(1, cchip_ram[0x53] == 9 ? 1 : 0);
            Generic.coin_lockout_w(0, cchip_ram[0x53] == 9 ? 1 : 0);
            Generic.coin_counter_w(0, 0);
            Generic.coin_counter_w(1, 0);
            if (cchip_ram[0x34] < 2)
            {
                updateDifficulty(0);
                cchip_ram[0x76] = 0;
                cchip_ram[0x75] = 0;
                cchip_ram[0x74] = 0;
                cchip_ram[0x72] = 0;
                cchip_ram[0x71] = 0;
                cchip_ram[0x70] = 0;
                cchip_ram[0x66] = 0;
                cchip_ram[0x2b] = 0;
                cchip_ram[0x30] = 0;
                cchip_ram[0x31] = 0;
                cchip_ram[0x32] = 0;
                cchip_ram[0x27] = 0;
                c588 = 0;
                c589 = 0;
                c58a = 0;
            }
            if (cchip_ram[0x1c] == 0 && cchip_ram[0x1d] == 0 && cchip_ram[0x1e] == 0 && cchip_ram[0x1f] == 0 && cchip_ram[0x20] == 0)
            {
                if (cchip_ram[0x1b] == 0x6)
                {
                    if (cchip_ram[0x27] == 0x1)
                        cchip_ram[0x32] = 1;
                }
                else if (cchip_ram[0x1b] == 0x2)
                {
                    if (m_triggeredLevel2 == 0 && cchip_ram[0x5f] == 0) // Don't write unless 68K is ready (0 at 0x5f)
                    {
                        cchip_ram[0x5f] = 4; // 0xBE at 68K side
                        m_triggeredLevel2 = 1;
                    }
                    if (m_triggeredLevel2 != 0 && cchip_ram[0x5d] != 0)
                    {
                        cchip_ram[0x32] = 1;
                        cchip_ram[0x5d] = 0; // acknowledge 68K command
                    }
                }
                else if (cchip_ram[0x1b] == 0x4)
                {
                    cchip_ram[0x32] = 1;
                    if (m_triggeredLevel4 == 0 && cchip_ram[0x5f] == 0) // Don't write unless 68K is ready (0 at 0x5f))
                    {
                        cchip_ram[0x5f] = 10;
                        m_triggeredLevel4 = 1;
                    }
                }
                else
                {
                    cchip_ram[0x32] = 1;
                }
            }
            if (cchip_ram[0x1c] == 0 && cchip_ram[0x1d] == 0)
            {
                if (cchip_ram[0x1b] == 0x1 && m_triggeredLevel1b == 0 && cchip_ram[0x5f] == 0) // Don't write unless 68K is ready (0 at 0x5f))
                {
                    cchip_ram[0x5f] = 7;
                    m_triggeredLevel1b = 1;
                }
                if (cchip_ram[0x1b] == 0x3 && m_triggeredLevel3b == 0 && cchip_ram[0x5f] == 0) // Don't write unless 68K is ready (0 at 0x5f))
                {
                    cchip_ram[0x5f] = 8;
                    m_triggeredLevel3b = 1;
                }
                if ((cchip_ram[0x1b] != 0x1 && cchip_ram[0x1b] != 0x3) && m_triggeredLevel13b == 0 && cchip_ram[0x5f] == 0) // Don't write unless 68K is ready (0 at 0x5f))
                {
                    cchip_ram[0x5f] = 9;
                    m_triggeredLevel13b = 1;
                }
            }
            if (cchip_ram[0x1b] == 0x2)
            {
                int numMen = (cchip_ram[0x1d] << 8) + cchip_ram[0x1c];
                if (numMen < 0x25 && m_triggeredLevel2b == 1 && m_triggeredLevel2c == 0 && cchip_ram[0x5f] == 0) // Don't write unless 68K is ready (0 at 0x5f))
                {
                    cchip_ram[0x5f] = 6;
                    m_triggeredLevel2c = 1;
                }
                if (numMen < 0x45 && m_triggeredLevel2b == 0 && cchip_ram[0x5f] == 0) // Don't write unless 68K is ready (0 at 0x5f))
                {
                    cchip_ram[0x5f] = 5;
                    m_triggeredLevel2b = 1;
                }
            }
            if (cchip_ram[0x1b] == 0x5)
            {
                if (cchip_ram[0x1c] == 0 && cchip_ram[0x1d] == 0 && m_triggeredLevel5 == 0)
                {
                    cchip_ram[0x2f] = 1;
                    m_triggeredLevel5 = 1;
                }
            }
            if (cchip_ram[0x1b] == 0x6)
            {
                if (c58a == 0)
                {
                    if ((cchip_ram[0x72] & 0x7f) >= 8 && cchip_ram[0x74] == 0 && cchip_ram[0x1c] == 0 && cchip_ram[0x1d] == 0 && cchip_ram[0x1f] == 0)
                    {
                        cchip_ram[0x30] = 1;
                        cchip_ram[0x74] = 1;
                        c58a = 1;
                    }
                }
                if (cchip_ram[0x1a] == 0x90)
                {
                    cchip_ram[0x74] = 0;
                }
                if (c58a != 0)
                {
                    if (c589 == 0 && cchip_ram[0x27] == 0 && cchip_ram[0x75] == 0 && cchip_ram[0x1c] == 0 && cchip_ram[0x1d] == 0 && cchip_ram[0x1e] == 0 && cchip_ram[0x1f] == 0)
                    {
                        cchip_ram[0x31] = 1;
                        cchip_ram[0x75] = 1;
                        c589 = 1;
                    }
                }
                if (cchip_ram[0x2b] == 0x1)
                {
                    cchip_ram[0x2b] = 0;
                    if (cchip_ram[0x30] == 0x1)
                    {
                        if (cchip_ram[0x1a] != 0x90)
                        {
                            cchip_ram[0x1a]--;
                        }
                    }
                    if (cchip_ram[0x72] == 0x9)
                    {
                        if (cchip_ram[0x76] != 0x4)
                        {
                            cchip_ram[0x76] = 3;
                        }
                    }
                    else
                    {
                        c588 |= 0x80;
                        cchip_ram[0x72] = c588;
                        c588++;
                        cchip_ram[0x1a]--;
                        cchip_ram[0x1a]--;
                        cchip_ram[0x1a]--;
                    }
                }
                if (cchip_ram[0x76] == 0)
                {
                    cchip_ram[0x76] = 1;
                    updateDifficulty(1);
                }
            }
            if (cchip_ram[0x1b] == 0x7 && m_triggeredLevel7 == 0 && cchip_ram[0x5f] == 0) // Don't write unless 68K is ready (0 at 0x5f))
            {
                m_triggeredLevel7 = 1;
                cchip_ram[0x5f] = 1;
            }
            if (cchip_ram[0x1b] == 0x8 && m_triggeredLevel8 == 0 && cchip_ram[0x5f] == 0) // Don't write unless 68K is ready (0 at 0x5f))
            {
                m_triggeredLevel8 = 1;
                cchip_ram[0x5f] = 2;
            }
            if (cchip_ram[0x1b] == 0x9 && m_triggeredLevel9 == 0 && cchip_ram[0x5f] == 0) // Don't write unless 68K is ready (0 at 0x5f))
            {
                m_triggeredLevel9 = 1;
                cchip_ram[0x5f] = 3;
            }
            if (cchip_ram[0xe] == 1)
            {
                cchip_ram[0xe] = 0xfd;
                cchip_ram[0x61] = 0x04;
            }
            if (cchip_ram[0x7a] == 0 && cchip_last_7a != 0 && current_cmd != 0xf5)
            {
                current_cmd = 0xf5;
                EmuTimer.emu_timer timer = EmuTimer.timer_alloc_common(EmuTimer.TIME_ACT.Taito_opwolf_timer_callback, true);
                EmuTimer.timer_adjust_periodic(timer, new Atime(0, (long)(80000 * Cpuexec.cpu[0].attoseconds_per_cycle)), Attotime.ATTOTIME_NEVER);
            }
            cchip_last_7a = cchip_ram[0x7a];
            if (cchip_ram[0x7f] == 0xa)
            {
                cchip_ram[0xfe] = 0xf7;
                cchip_ram[0xff] = 0x6e;
            }
            cchip_ram[0x64] = 0;
            cchip_ram[0x66] = 0;
        }
        public static void opwolf_cchip_init()
        {
            m_triggeredLevel1b = 0;
            m_triggeredLevel2 = 0;
            m_triggeredLevel2b = 0;
            m_triggeredLevel2c = 0;
            m_triggeredLevel3b = 0;
            m_triggeredLevel13b = 0;
            m_triggeredLevel4 = 0;
            m_triggeredLevel5 = 0;
            m_triggeredLevel7 = 0;
            m_triggeredLevel8 = 0;
            m_triggeredLevel9 = 0;
            current_bank = 0;
            current_cmd = 0;
            cchip_last_7a = 0;
            cchip_last_04 = 0xfc;
            cchip_last_05 = 0xff;
            c588 = 0;
            c589 = 0;
            c58a = 0;
            cchip_coins[0] = 0;
            cchip_coins[1] = 0;
            cchip_coins_for_credit[0] = 1;
            cchip_credits_for_coin[0] = 1;
            cchip_coins_for_credit[1] = 1;
            cchip_credits_for_coin[1] = 1;
            EmuTimer.timer_pulse_internal(new Atime(0, (long)(1e18 / 60)), EmuTimer.TIME_ACT.Taito_cchip_timer);
        }
    }
}
