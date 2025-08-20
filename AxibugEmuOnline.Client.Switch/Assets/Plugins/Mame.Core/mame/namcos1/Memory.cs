using cpu.m6800;

namespace MAME.Core
{
    public unsafe partial class Namcos1
    {
        public static byte byte0, byte1, byte2, byte00, byte01, byte02, byte03;
        public static byte byte0_old, byte1_old, byte2_old;
        public static byte strobe;
        public static int input_count, strobe_count;
        public static int stored_input0, stored_input1;
        public static byte N0ReadOpByte(ushort address)
        {
            int reg, offset;
            byte result;
            if (address >= 0x0000 && address <= 0xffff)
            {
                reg = address / 0x2000;
                offset = address & 0x1fff;
                result = user1rom[user1rom_offset[0, reg] + offset];
            }
            else
            {
                result = 0;
            }
            return result;
        }
        public static byte N1ReadOpByte(ushort address)
        {
            int reg, offset;
            byte result;
            if (address >= 0x0000 && address <= 0xffff)
            {
                reg = address / 0x2000;
                offset = address & 0x1fff;
                result = user1rom[user1rom_offset[1, reg] + offset];
            }
            else
            {
                result = 0;
            }
            return result;
        }
        public static byte N2ReadOpByte(ushort address)
        {
            int offset;
            byte result = 0;
            if (address >= 0xc000 && address <= 0xffff)
            {
                offset = address & 0x3fff;
                result = audiorom[offset];
            }
            else
            {
                result = 0;
            }
            return result;
        }
        public static byte N3ReadOpByte(ushort address)
        {
            int offset;
            byte result;
            if (address >= 0x4000 && address <= 0xbfff)
            {
                offset = address - 0x4000;
                result = voicerom[mcurom_offset + offset];
            }
            else if (address >= 0xf000 && address <= 0xffff)
            {
                offset = address & 0xfff;
                result = mcurom[offset];
            }
            else
            {
                result = 0;
            }
            return result;
        }
        public static byte N0ReadMemory(ushort address)
        {
            byte result;
            int offset;
            int reg;
            reg = address / 0x2000;
            offset = address & 0x1fff;
            if (cus117_offset[0, reg] == 0)
            {
                result = user1rom[user1rom_offset[0, reg] + offset];
            }
            else if (cus117_offset[0, reg] >= 0x2e0000 && cus117_offset[0, reg] <= 0x2e7fff)
            {
                result = namcos1_paletteram[cus117_offset[0, reg] - 0x2e0000 + offset];
            }
            else if (cus117_offset[0, reg] >= 0x2f0000 && cus117_offset[0, reg] <= 0x2f7fff)
            {
                result = namcos1_videoram_r(cus117_offset[0, reg] - 0x2f0000 + offset);
            }
            else if (cus117_offset[0, reg] == 0x2f8000)
            {
                result = key_r(offset);
            }
            else if (cus117_offset[0, reg] == 0x2fc000)
            {
                result = namcos1_spriteram_r(offset);
            }
            else if (cus117_offset[0, reg] == 0x2fe000)
            {
                result = soundram_r(offset);
            }
            else if (cus117_offset[0, reg] >= 0x300000 && cus117_offset[0, reg] <= 0x307fff)
            {
                result = s1ram[cus117_offset[0, reg] - 0x300000 + offset];
            }
            else if (cus117_offset[0, reg] >= 0x400000 && cus117_offset[0, reg] <= 0x7fffff)
            {
                result = user1rom[cus117_offset[0, reg] - 0x400000 + offset];
            }
            else
            {
                result = 0;
            }
            return result;
        }
        public static byte N1ReadMemory(ushort address)
        {
            byte result;
            int offset;
            int reg;
            reg = address / 0x2000;
            offset = address & 0x1fff;
            if (cus117_offset[1, reg] >= 0x2e0000 && cus117_offset[1, reg] <= 0x2e7fff)
            {
                result = namcos1_paletteram[cus117_offset[1, reg] - 0x2e0000 + offset];
            }
            else if (cus117_offset[1, reg] >= 0x2f0000 && cus117_offset[1, reg] <= 0x2f7fff)
            {
                result = namcos1_videoram_r(cus117_offset[1, reg] - 0x2f0000 + offset);
            }
            else if (cus117_offset[1, reg] == 0x2f8000)
            {
                result = key_r(offset);
            }
            else if (cus117_offset[1, reg] == 0x2fc000)
            {
                result = namcos1_spriteram_r(offset);
            }
            else if (cus117_offset[1, reg] == 0x2fe000)
            {
                result = soundram_r(offset);
            }
            else if (cus117_offset[1, reg] >= 0x300000 && cus117_offset[1, reg] <= 0x307fff)
            {
                result = s1ram[cus117_offset[1, reg] - 0x300000 + offset];
            }
            else if (cus117_offset[1, reg] >= 0x400000 && cus117_offset[1, reg] <= 0x7fffff)
            {
                result = user1rom[cus117_offset[1, reg] - 0x400000 + offset];
            }
            else if (cus117_offset[0, reg] == 0)
            {
                result = user1rom[user1rom_offset[1, reg] + offset];
            }
            else
            {
                result = 0;
            }
            return result;
        }
        public static byte N2ReadMemory(ushort address)
        {
            byte result;
            int offset;
            if (address >= 0x0000 && address <= 0x3fff)
            {
                offset = address & 0x3fff;
                result = audiorom[audiocpurom_offset + offset];
            }
            else if (address >= 0x4000 && address <= 0x4001)
            {
                result = YM2151.ym2151_status_port_0_r();
            }
            else if (address >= 0x5000 && address <= 0x53ff)
            {
                offset = address & 0x3ff;
                result = Namco.namcos1_cus30_r(offset);
            }
            else if (address >= 0x7000 && address <= 0x77ff)
            {
                offset = address & 0x7ff;
                result = namcos1_triram[offset];
            }
            else if (address >= 0x8000 && address <= 0x9fff)
            {
                offset = address & 0x1fff;
                result = bank_ram20[offset];
            }
            else if (address >= 0xc000 && address <= 0xffff)
            {
                offset = address & 0x3fff;
                result = audiorom[offset];
            }
            else
            {
                result = 0;
            }
            return result;
        }
        public static byte N3ReadMemory(ushort address)
        {
            byte result;
            int offset;
            if (address >= 0x0000 && address <= 0x001f)
            {
                offset = address & 0x1f;
                result = M6800.m1.hd63701_internal_registers_r(offset);
            }
            else if (address >= 0x0080 && address <= 0x00ff)
            {
                offset = address & 0x7f;
                result = bank_ram30[offset];
            }
            else if (address >= 0x1000 && address <= 0x1003)
            {
                offset = address & 0x03;
                result = dsw_r(offset);
            }
            else if (address == 0x1400)
            {
                result = byte0;
            }
            else if (address == 0x1401)
            {
                result = byte1;
            }
            else if (address >= 0x4000 && address <= 0xbfff)
            {
                offset = address - 0x4000;
                result = voicerom[mcurom_offset + offset];
            }
            else if (address >= 0xc000 && address <= 0xc7ff)
            {
                offset = address & 0x7ff;
                result = namcos1_triram[offset];
            }
            else if (address >= 0xc800 && address <= 0xcfff)
            {
                offset = address & 0x7ff;
                result = Generic.generic_nvram[offset];
            }
            else if (address >= 0xf000 && address <= 0xffff)
            {
                offset = address & 0xfff;
                result = mcurom[offset];
            }
            else
            {
                result = 0;
            }
            return result;
        }
        public static byte N3ReadIO(ushort address)
        {
            byte result;
            if (address == 0x100)
            {
                result = byte2;
            }
            else if (address == 0x101)
            {
                result = 0;
            }
            else
            {
                result = 0;
            }
            return result;
        }
        public static void N0WriteMemory(ushort address, byte data)
        {
            int offset;
            int reg;
            reg = address / 0x2000;
            offset = address & 0x1fff;
            if (cus117_offset[0, reg] == 0)
            {
                if (address >= 0x0000 && address <= 0xdfff)
                {
                    user1rom[user1rom_offset[0, reg] + offset] = data;
                }
                if (address >= 0xe000 && address <= 0xefff)
                {
                    namcos1_bankswitch_w(offset, data);
                }
                else if (address == 0xf000)
                {
                    namcos1_cpu_control_w(data);
                }
                else if (address == 0xf200)
                {
                    namcos1_watchdog_w();
                }
                else if (address == 0xf600)
                {
                    irq_ack_w(0);
                }
                else if (address == 0xf800)
                {
                    firq_ack_w(0);
                }
                else if (address == 0xfa00)
                {
                    namcos1_sub_firq_w();
                }
                else if (address >= 0xfc00 && address <= 0xfc01)
                {
                    namcos1_subcpu_bank_w(data);
                }
                else
                {
                    int i1 = 1;
                }
            }
            else if (cus117_offset[0, reg] == 0x2c0000)
            {
                namcos1_3dcs_w(offset);
            }
            else if (cus117_offset[0, reg] >= 0x2e0000 && cus117_offset[0, reg] <= 0x2e7fff)
            {
                namcos1_paletteram_w(cus117_offset[0, reg] - 0x2e0000 + offset, data);
            }
            else if (cus117_offset[0, reg] >= 0x2f0000 && cus117_offset[0, reg] <= 0x2f7fff)
            {
                namcos1_videoram_w(cus117_offset[0, reg] - 0x2f0000 + offset, data);
            }
            else if (cus117_offset[0, reg] == 0x2f8000)
            {
                key_w(offset, data);
            }
            else if (cus117_offset[0, reg] == 0x2fc000)
            {
                namcos1_spriteram_w(offset, data);
            }
            else if (cus117_offset[0, reg] == 0x2fe000)
            {
                soundram_w(offset, data);
            }
            else if (cus117_offset[0, reg] >= 0x300000 && cus117_offset[0, reg] <= 0x307fff)
            {
                s1ram[cus117_offset[0, reg] - 0x300000 + offset] = data;
            }
            else
            {
                int i1 = 1;
            }
        }
        public static void N1WriteMemory(ushort address, byte data)
        {
            int offset;
            int reg;
            reg = address / 0x2000;
            offset = address & 0x1fff;
            if (cus117_offset[1, reg] == 0x2c0000)
            {
                namcos1_3dcs_w(offset);
            }
            else if (cus117_offset[1, reg] >= 0x2e0000 && cus117_offset[1, reg] <= 0x2e7fff)
            {
                namcos1_paletteram_w(cus117_offset[1, reg] - 0x2e0000 + offset, data);
            }
            else if (cus117_offset[1, reg] >= 0x2f0000 && cus117_offset[1, reg] <= 0x2f7fff)
            {
                namcos1_videoram_w(cus117_offset[1, reg] - 0x2f0000 + offset, data);
            }
            else if (cus117_offset[1, reg] == 0x2f8000)
            {
                key_w(offset, data);
            }
            else if (cus117_offset[1, reg] == 0x2fc000)
            {
                namcos1_spriteram_w(offset, data);
            }
            else if (cus117_offset[1, reg] == 0x2fe000)
            {
                soundram_w(offset, data);
            }
            else if (cus117_offset[1, reg] >= 0x300000 && cus117_offset[1, reg] <= 0x307fff)
            {
                s1ram[cus117_offset[1, reg] - 0x300000 + offset] = data;
            }
            else if (cus117_offset[0, reg] == 0)
            {
                if (address >= 0x0000 && address <= 0xdfff)
                {
                    user1rom[user1rom_offset[1, reg] + offset] = data;
                }
                else if (address >= 0xe000 && address <= 0xefff)
                {
                    namcos1_bankswitch_w(offset, data);
                }
                else if (address == 0xf000)
                {
                    int i1 = 1;
                }
                else if (address == 0xf200)
                {
                    namcos1_watchdog_w();
                }
                else if (address == 0xf600)
                {
                    irq_ack_w(1);
                }
                else if (address == 0xf800)
                {
                    firq_ack_w(1);
                }
                else if (address == 0xfa00)
                {
                    int i1 = 1;
                }
                else if (address >= 0xfc00 && address <= 0xfc01)
                {
                    int i1 = 1;
                }
                else
                {
                    int i1 = 1;
                }
            }
            else
            {
                int i1 = 1;
            }
        }
        public static void N2WriteMemory(ushort address, byte data)
        {
            int offset;
            if (address == 0x4000)
            {
                YM2151.ym2151_register_port_0_w(data);
            }
            else if (address == 0x4001)
            {
                YM2151.ym2151_data_port_0_w(data);
            }
            else if (address >= 0x5000 && address <= 0x53ff)
            {
                offset = address & 0x3ff;
                Namco.namcos1_cus30_w(offset, data);
            }
            else if (address >= 0x7000 && address <= 0x77ff)
            {
                offset = address & 0x7ff;
                namcos1_triram[offset] = data;
            }
            else if (address >= 0x8000 && address <= 0x9fff)
            {
                offset = address & 0x1fff;
                bank_ram20[offset] = data;
            }
            else if (address >= 0xc000 && address <= 0xc001)
            {
                namcos1_sound_bankswitch_w(data);
            }
            else if (address == 0xd001)
            {
                namcos1_watchdog_w();
            }
            else if (address == 0xe000)
            {
                irq_ack_w(2);
            }
            else
            {
                int i1 = 1;
            }
        }
        public static void N3WriteMemory(ushort address, byte data)
        {
            int offset;
            if (address >= 0x0000 && address <= 0x001f)
            {
                offset = address & 0x1f;
                M6800.m1.hd63701_internal_registers_w(offset, data);
            }
            else if (address >= 0x0080 && address <= 0x00ff)
            {
                offset = address & 0x7f;
                bank_ram30[offset] = data;
            }
            else if (address == 0xc000)
            {
                namcos1_mcu_patch_w(data);
            }
            else if (address >= 0xc000 && address <= 0xc7ff)
            {
                offset = address & 0x7ff;
                namcos1_triram[offset] = data;
            }
            else if (address >= 0xc800 && address <= 0xcfff)
            {
                offset = address & 0x7ff;
                Generic.generic_nvram[offset] = data;
            }
            else if (address == 0xd000)
            {
                namcos1_dac0_w(data);
            }
            else if (address == 0xd400)
            {
                namcos1_dac1_w(data);
            }
            else if (address == 0xd800)
            {
                namcos1_mcu_bankswitch_w(data);
            }
            else if (address == 0xf000)
            {
                irq_ack_w(3);
            }
            else
            {
                int i1 = 1;
            }
        }
        public static void N3WriteIO(ushort address, byte data)
        {
            if (address == 0x100)
            {
                namcos1_coin_w(data);
            }
            else if (address == 0x101)
            {
                namcos1_dac_gain_w(data);
            }
        }
    }
}
