using cpu.m68000;

namespace MAME.Core
{
    public unsafe partial class Neogeo
    {
        public static short short0, short1, short2, short3, short4, short5, short6;
        public static short short0_old, short1_old, short2_old, short3_old, short4_old, short5_old, short6_old;
        public static sbyte MReadOpByte(int address)
        {
            address &= 0xffffff;
            sbyte result = 0;
            if (address >= 0x000000 && address <= 0x00007f)
            {
                if (main_cpu_vector_table_source == 0)
                {
                    result = (sbyte)mainbiosrom[address];
                }
                else if (main_cpu_vector_table_source == 1)
                {
                    result = (sbyte)Memory.mainrom[address];
                }
            }
            else if (address >= 0x000080 && address <= 0x0fffff)
            {
                result = (sbyte)Memory.mainrom[address];
            }
            else if (address >= 0x100000 && address <= 0x1fffff)
            {
                result = (sbyte)Memory.mainram[address & 0xffff];
            }
            else if (address >= 0x200000 && address <= 0x2fffff)
            {
                result = (sbyte)Memory.mainrom[main_cpu_bank_address + (address - 0x200000)];
            }
            else if (address >= 0xc00000 && address <= 0xcfffff)
            {
                result = (sbyte)mainbiosrom[address & 0x1ffff];
            }
            else
            {
                result = 0;
            }
            return result;
        }
        public static sbyte MReadByte(int address)
        {
            address &= 0xffffff;
            sbyte result = 0;
            if (address >= 0x000000 && address <= 0x00007f)
            {
                if (main_cpu_vector_table_source == 0)
                {
                    result = (sbyte)mainbiosrom[address];
                }
                else if (main_cpu_vector_table_source == 1)
                {
                    result = (sbyte)Memory.mainrom[address];
                }
            }
            else if (address >= 0x000080 && address <= 0x0fffff)
            {
                result = (sbyte)Memory.mainrom[address];
            }
            else if (address >= 0x100000 && address <= 0x1fffff)
            {
                result = (sbyte)Memory.mainram[address & 0xffff];
            }
            else if (address >= 0x200000 && address <= 0x2fffff)
            {
                result = (sbyte)Memory.mainrom[main_cpu_bank_address + (address - 0x200000)];
            }
            /*else if (address >= 0x300000 && address <= 0x300001)
            {
                if (address == 0x300000)
                {
                    result = (sbyte)(short0 >> 8);
                }
                else if (address == 0x300001)
                {
                    result = (sbyte)dsw;
                }
            }
            else if (address >= 0x300080 && address <= 0x300081)
            {
                if (address == 0x300080)
                {
                    result = (sbyte)(short4 >> 8);
                }
                else if (address == 0x300081)
                {
                    result = (sbyte)short4;
                }
            }*/
            else if (address >= 0x300000 && address <= 0x31ffff)
            {
                int add = address & 0x81;
                if (add == 0x00)
                {
                    result = (sbyte)(short0 >> 8);
                }
                else if (add == 0x01)
                {
                    result = (sbyte)dsw;
                }
                else if (add == 0x80)
                {
                    result = (sbyte)(short4 >> 8);
                }
                else if (add == 0x81)
                {
                    result = (sbyte)short4;
                }
            }
            else if (address >= 0x320000 && address <= 0x33ffff)
            {
                if ((address & 0x01) == 0)
                {
                    result = (sbyte)((short3 >> 8) | get_audio_result());
                }
                else if ((address & 0x01) == 1)
                {
                    result = (sbyte)((ushort)short3 | ((get_calendar_status() & 0x03) << 6));
                }
            }
            else if (address >= 0x340000 && address <= 0x35ffff)
            {
                if ((address & 0x01) == 0)
                {
                    result = (sbyte)(short1 >> 8);
                }
                else if ((address & 0x01) == 1)
                {
                    result = (sbyte)short1;
                }
            }
            else if (address >= 0x380000 && address <= 0x39ffff)
            {
                if ((address & 0x01) == 0)
                {
                    result = (sbyte)(short2 >> 8);
                }
                else if ((address & 0x01) == 1)
                {
                    result = (sbyte)short2;
                }
            }
            else if (address >= 0x3c0000 && address <= 0x3dffff)
            {
                if ((address & 0x01) == 0)
                {
                    result = (sbyte)(neogeo_video_register_r((address & 0x07) >> 1) >> 8);
                }
                else if ((address & 0x01) == 1)
                {
                    int i1 = 1;
                }
            }
            else if (address >= 0x400000 && address <= 0x7fffff)
            {
                int i1 = 1;
                //result = palettes[palette_bank][(address &0x1fff) >> 1];
            }
            else if (address >= 0xc00000 && address <= 0xcfffff)
            {
                result = (sbyte)mainbiosrom[address & 0x1ffff];
            }
            else if (address >= 0xd00000 && address <= 0xdfffff)
            {
                result = (sbyte)mainram2[address & 0xffff];
            }
            else
            {
                int i1 = 1;
            }
            return result;
        }
        //public static short MReadOpWord(int address)
        //{
        //    address &= 0xffffff;
        //    short result = 0;
        //    if (address >= 0x000000 && address + 1 <= 0x00007f)
        //    {
        //        if (main_cpu_vector_table_source == 0)
        //        {
        //            result = (short)(mainbiosrom[address] * 0x100 + mainbiosrom[address + 1]);
        //        }
        //        else if (main_cpu_vector_table_source == 1)
        //        {
        //            result = (short)(Memory.mainrom[address] * 0x100 + Memory.mainrom[address + 1]);
        //        }
        //    }
        //    else if (address >= 0x000080 && address + 1 <= 0x0fffff)
        //    {
        //        if (address >= 0x142B9 && address <= 0x142C9)
        //        {
        //            //m68000Form.iStatus = 1;
        //        }
        //        result = (short)(Memory.mainrom[address] * 0x100 + Memory.mainrom[address + 1]);
        //    }
        //    else if (address >= 0x100000 && address + 1 <= 0x1fffff)
        //    {
        //        result = (short)(Memory.mainram[address & 0xffff] * 0x100 + Memory.mainram[(address & 0xffff) + 1]);
        //    }
        //    else if (address >= 0x200000 && address + 1 <= 0x2fffff)
        //    {
        //        result = (short)(Memory.mainrom[main_cpu_bank_address + (address & 0xfffff)] * 0x100 + Memory.mainrom[main_cpu_bank_address + (address & 0xfffff) + 1]);
        //    }
        //    else if (address >= 0xc00000 && address + 1 <= 0xcfffff)
        //    {
        //        result = (short)(mainbiosrom[address & 0x1ffff] * 0x100 + mainbiosrom[(address & 0x1ffff) + 1]);
        //    }
        //    else
        //    {
        //        result = 0;
        //    }
        //    return result;
        //}

        //手动优化
        public static short MReadOpWord(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address >= 0x000000 && address + 1 <= 0x00007f)
            {
                if (main_cpu_vector_table_source == 0)
                {
                    result = (short)(mainbiosrom[address] * 0x100 + mainbiosrom[address + 1]);
                }
                else if (main_cpu_vector_table_source == 1)
                {
                    result = (short)(Memory.mainrom[address] * 0x100 + Memory.mainrom[address + 1]);
                }
            }
            else if (address >= 0x000080 && address + 1 <= 0x0fffff)
            {
                //if (address >= 0x142B9 && address <= 0x142C9)
                //{
                //    //m68000Form.iStatus = 1;
                //}
                result = (short)(Memory.mainrom[address] * 0x100 + Memory.mainrom[address + 1]);
            }
            else if (address >= 0x100000 && address + 1 <= 0x1fffff)
            {
                result = (short)(Memory.mainram[address & 0xffff] * 0x100 + Memory.mainram[(address & 0xffff) + 1]);
            }
            else if (address >= 0x200000 && address + 1 <= 0x2fffff)
            {
                result = (short)(Memory.mainrom[main_cpu_bank_address + (address & 0xfffff)] * 0x100 + Memory.mainrom[main_cpu_bank_address + (address & 0xfffff) + 1]);
            }
            else if (address >= 0xc00000 && address + 1 <= 0xcfffff)
            {
                result = (short)(mainbiosrom[address & 0x1ffff] * 0x100 + mainbiosrom[(address & 0x1ffff) + 1]);
            }
            else
            {
                result = 0;
            }
            return result;
        }
        public static short MReadWord(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address >= 0x000000 && address + 1 <= 0x00007f)
            {
                if (main_cpu_vector_table_source == 0)
                {
                    result = (short)(mainbiosrom[address] * 0x100 + mainbiosrom[address + 1]);
                }
                else if (main_cpu_vector_table_source == 1)
                {
                    result = (short)(Memory.mainrom[address] * 0x100 + Memory.mainrom[address + 1]);
                }
            }
            else if (address >= 0x000080 && address + 1 <= 0x0fffff)
            {
                if (address >= 0x142B9 && address <= 0x142C9)
                {
                    //m68000Form.iStatus = 1;
                }
                result = (short)(Memory.mainrom[address] * 0x100 + Memory.mainrom[address + 1]);
            }
            else if (address >= 0x100000 && address + 1 <= 0x1fffff)
            {
                if (address == 0x101410)
                {
                    int i1 = 1;
                }
                result = (short)(Memory.mainram[address & 0xffff] * 0x100 + Memory.mainram[(address & 0xffff) + 1]);
            }
            else if (address >= 0x200000 && address <= 0x2fffff)
            {
                result = (short)(Memory.mainrom[main_cpu_bank_address + (address & 0xfffff)] * 0x100 + Memory.mainrom[main_cpu_bank_address + (address & 0xfffff) + 1]);
            }
            /*else if (address >= 0x300000 && address <= 0x300001)
            {
                result = (short)((ushort)short0 | dsw);
            }
            else if (address >= 0x300080 && address <= 0x300081)
            {
                result = short4;
            }*/
            else if (address >= 0x300000 && address <= 0x31ffff)
            {
                int add = address & 0x81;
                if (add >= 0x00 && add + 1 <= 0x01)
                {
                    result = (short)((ushort)short0 | dsw);
                }
                else if (add >= 0x80 && add + 1 <= 0x81)
                {
                    result = short4;
                }
            }
            else if (address >= 0x320000 && address <= 0x33ffff)
            {
                result = (short)((ushort)short3 | (ushort)((get_calendar_status() & 0x03) << 6) | (get_audio_result() << 8));
            }
            else if (address >= 0x340000 && address <= 0x35ffff)
            {
                result = short1;
            }
            else if (address >= 0x380000 && address <= 0x39ffff)
            {
                result = short2;
            }
            else if (address >= 0x3c0000 && address + 1 <= 0x3dffff)
            {
                result = (short)neogeo_video_register_r((address & 0x07) >> 1);
            }
            else if (address >= 0x400000 && address + 1 <= 0x7fffff)
            {
                result = (short)palettes[palette_bank, (address & 0x1fff) >> 1];
            }
            else if (address >= 0xc00000 && address + 1 <= 0xcfffff)
            {
                result = (short)(mainbiosrom[address & 0x1ffff] * 0x100 + mainbiosrom[(address & 0x1ffff) + 1]);
            }
            else if (address >= 0xd00000 && address + 1 <= 0xdfffff)
            {
                result = (short)(mainram2[address & 0xffff] * 0x100 + mainram2[(address & 0xffff) + 1]);
            }
            else
            {
                int i1 = 1;
            }
            return result;
        }
        public static int MReadOpLong(int address)
        {
            address &= 0xffffff;
            int result = 0;
            if (address >= 0x000000 && address + 3 < 0x00007f)
            {
                if (main_cpu_vector_table_source == 0)
                {
                    result = mainbiosrom[address] * 0x1000000 + mainbiosrom[address + 1] * 0x10000 + mainbiosrom[address + 2] * 0x100 + mainbiosrom[address + 3];
                }
                else if (main_cpu_vector_table_source == 1)
                {
                    result = Memory.mainrom[address] * 0x1000000 + Memory.mainrom[address + 1] * 0x10000 + Memory.mainrom[address + 2] * 0x100 + Memory.mainrom[address + 3];
                }
            }
            else if (address >= 0x000080 && address + 3 <= 0x0fffff)
            {
                if (address >= 0x1387a && address <= 0x1387a)
                {
                    //m68000Form.iStatus = 1;
                }
                result = Memory.mainrom[address] * 0x1000000 + Memory.mainrom[address + 1] * 0x10000 + Memory.mainrom[address + 2] * 0x100 + Memory.mainrom[address + 3];
            }
            else if (address >= 0x100000 && address + 3 <= 0x1fffff)
            {
                result = Memory.mainram[address & 0xffff] * 0x1000000 + Memory.mainram[(address & 0xffff) + 1] * 0x10000 + Memory.mainram[(address & 0xffff) + 2] * 0x100 + Memory.mainram[(address & 0xffff) + 3];
            }
            else if (address >= 0x200000 && address + 3 <= 0x2fffff)
            {
                result = Memory.mainrom[main_cpu_bank_address + (address & 0xfffff)] * 0x1000000 + Memory.mainrom[main_cpu_bank_address + (address & 0xfffff) + 1] * 0x10000 + Memory.mainrom[main_cpu_bank_address + (address & 0xfffff) + 2] * 0x100 + Memory.mainrom[main_cpu_bank_address + (address & 0xfffff) + 3];
            }
            else if (address >= 0xc00000 && address + 3 <= 0xcfffff)
            {
                result = mainbiosrom[address & 0x1ffff] * 0x1000000 + mainbiosrom[(address & 0x1ffff) + 1] * 0x10000 + mainbiosrom[(address & 0x1ffff) + 2] * 0x100 + mainbiosrom[(address & 0x1ffff) + 3];
            }
            else
            {
                result = 0;
            }
            return result;
        }
        public static int MReadLong(int address)
        {
            address &= 0xffffff;
            int result = 0;
            if (address >= 0x000000 && address + 3 <= 0x00007f)
            {
                if (main_cpu_vector_table_source == 0)
                {
                    result = mainbiosrom[address] * 0x1000000 + mainbiosrom[address + 1] * 0x10000 + mainbiosrom[address + 2] * 0x100 + mainbiosrom[address + 3];
                }
                else if (main_cpu_vector_table_source == 1)
                {
                    result = Memory.mainrom[address] * 0x1000000 + Memory.mainrom[address + 1] * 0x10000 + Memory.mainrom[address + 2] * 0x100 + Memory.mainrom[address + 3];
                }
            }
            else if (address >= 0x000080 && address + 3 <= 0x0fffff)
            {
                if (address >= 0x1387a && address <= 0x1387a)
                {
                    //m68000Form.iStatus = 1;
                }
                result = Memory.mainrom[address] * 0x1000000 + Memory.mainrom[address + 1] * 0x10000 + Memory.mainrom[address + 2] * 0x100 + Memory.mainrom[address + 3];
            }
            else if (address >= 0x100000 && address + 3 <= 0x1fffff)
            {
                result = Memory.mainram[address & 0xffff] * 0x1000000 + Memory.mainram[(address & 0xffff) + 1] * 0x10000 + Memory.mainram[(address & 0xffff) + 2] * 0x100 + Memory.mainram[(address & 0xffff) + 3];
            }
            else if (address >= 0x200000 && address + 3 <= 0x2fffff)
            {
                result = Memory.mainrom[main_cpu_bank_address + (address & 0xfffff)] * 0x1000000 + Memory.mainrom[main_cpu_bank_address + (address & 0xfffff) + 1] * 0x10000 + Memory.mainrom[main_cpu_bank_address + (address & 0xfffff) + 2] * 0x100 + Memory.mainrom[main_cpu_bank_address + (address & 0xfffff) + 3];
            }
            else if (address >= 0x300000 && address <= 0x31ffff)
            {
                result = 0;
            }
            else if (address >= 0x320000 && address <= 0x33ffff)
            {
                result = 0;
            }
            else if (address >= 0x340000 && address <= 0x35ffff)
            {
                result = 0;
            }
            else if (address >= 0x380000 && address <= 0x39ffff)
            {
                result = 0;
            }
            else if (address >= 0x3c0000 && address + 3 <= 0x3dffff)
            {
                int i1 = 1;
                //result =neogeo_video_register_r((address &0x07) >> 1, mem_mask);
            }
            else if (address >= 0x400000 && address + 3 <= 0x7fffff)
            {
                result = palettes[palette_bank, (address & 0x1fff) / 2] * 0x10000 + palettes[palette_bank, ((address & 0x1fff) / 2) + 1];
            }
            else if (address >= 0xc00000 && address + 3 <= 0xcfffff)
            {
                result = mainbiosrom[address & 0x1ffff] * 0x1000000 + mainbiosrom[(address & 0x1ffff) + 1] * 0x10000 + mainbiosrom[(address & 0x1ffff) + 2] * 0x100 + mainbiosrom[(address & 0x1ffff) + 3];
            }
            else if (address >= 0xd00000 && address + 3 <= 0xdfffff)
            {
                result = mainram2[address & 0xffff] * 0x1000000 + mainram2[(address & 0xffff) + 1] * 0x10000 + mainram2[(address & 0xffff) + 2] * 0x100 + mainram2[(address & 0xffff) + 3];
            }
            else
            {
                int i1 = 1;
            }
            return result;
        }
        //public static void MWriteByte(int address, sbyte value)
        //{
        //    address &= 0xffffff;
        //    if (address >= 0x100000 && address <= 0x1fffff)
        //    {
        //        if (address == 0x100d0b && value == 0x06)//&&MC68000.m1.TotalExecutedCycles>0x3F6FC8C)
        //        {
        //            ulong l1 = MC68000.m1.TotalExecutedCycles;
        //            int i2 = 1;
        //            //m68000Form.iStatus = 1;
        //        }
        //        Memory.mainram[address & 0xffff] = (byte)value;
        //    }
        //    else if (address >= 0x2ffff0 && address <= 0x2fffff)
        //    {
        //        main_cpu_bank_select_w(value);
        //    }
        //    else if (address >= 0x300000 && address <= 0x31ffff)
        //    {
        //        if ((address & 0x01) == 0)
        //        {
        //            int i1 = 1;
        //        }
        //        else if ((address & 0x01) == 1)
        //        {
        //            watchdog_w();
        //        }
        //    }
        //    else if (address >= 0x320000 && address <= 0x33ffff)
        //    {
        //        if ((address & 0x01) == 0)
        //        {
        //            audio_command_w((byte)value);
        //        }
        //        else if ((address & 0x01) == 1)
        //        {
        //            int i1 = 1;
        //        }
        //    }
        //    else if (address >= 0x380000 && address <= 0x39ffff)
        //    {
        //        io_control_w((address & 0x7f) >> 1, (byte)value);
        //    }
        //    else if (address >= 0x3a0000 && address <= 0x3bffff)
        //    {
        //        if ((address & 0x01) == 1)
        //        {
        //            system_control_w((address & 0x1f) >> 1);
        //        }
        //    }
        //    else if (address >= 0x3c0000 && address <= 0x3dffff)
        //    {
        //        if ((address & 0x01) == 0)
        //        {
        //            neogeo_video_register_w((address & 0x0f) >> 1, (ushort)((value << 8) | (byte)value));
        //        }
        //        else if ((address & 0x01) == 1)
        //        {
        //            int i1 = 1;
        //        }
        //    }
        //    else if (address >= 0x400000 && address <= 0x7fffff)
        //    {
        //        int i1 = 1;
        //        //neogeo_paletteram_w((address - 0x400000) >> 1, data, mem_mask);
        //    }
        //    else if (address >= 0xd00000 && address <= 0xdfffff)
        //    {
        //        save_ram_w(address & 0xffff, (byte)value);
        //    }
        //    else
        //    {
        //        int i1 = 1;
        //    }
        //}
        
        //手动优化
        public static void MWriteByte(int address, sbyte value)
        {
            address &= 0xffffff;
            if (address >= 0x100000 && address <= 0x1fffff)
            {
                //if (address == 0x100d0b && value == 0x06)//&&MC68000.m1.TotalExecutedCycles>0x3F6FC8C)
                //{
                //    ulong l1 = MC68000.m1.TotalExecutedCycles;
                //    int i2 = 1;
                //    //m68000Form.iStatus = 1;
                //}
                Memory.mainram[address & 0xffff] = (byte)value;
            }
            else if (address >= 0x2ffff0 && address <= 0x2fffff)
            {
                main_cpu_bank_select_w(value);
            }
            else if (address >= 0x300000 && address <= 0x31ffff)
            {
                /*if ((address & 0x01) == 0)
                {
                    int i1 = 1;
                }
                else */if ((address & 0x01) == 1)
                {
                    //watchdog_w();
                    //减少一次堆栈 无意义套娃
                    Watchdog.watchdog_reset();
                }
            }
            else if (address >= 0x320000 && address <= 0x33ffff)
            {
                if ((address & 0x01) == 0)
                {
                    audio_command_w((byte)value);
                }
                //无意义
                //else if ((address & 0x01) == 1)
                //{
                //    int i1 = 1;
                //}
            }
            else if (address >= 0x380000 && address <= 0x39ffff)
            {
                io_control_w((address & 0x7f) >> 1, (byte)value);
            }
            else if (address >= 0x3a0000 && address <= 0x3bffff)
            {
                if ((address & 0x01) == 1)
                {
                    system_control_w((address & 0x1f) >> 1);
                }
            }
            else if (address >= 0x3c0000 && address <= 0x3dffff)
            {
                if ((address & 0x01) == 0)
                {
                    neogeo_video_register_w((address & 0x0f) >> 1, (ushort)((value << 8) | (byte)value));
                }
                //无意义
                //else if ((address & 0x01) == 1)
                //{
                //    int i1 = 1;
                //}
            }

            //无意义
            //else if (address >= 0x400000 && address <= 0x7fffff)
            //{
            //    int i1 = 1;
            //    //neogeo_paletteram_w((address - 0x400000) >> 1, data, mem_mask);
            //}
            else if (address >= 0xd00000 && address <= 0xdfffff)
            {
                save_ram_w(address & 0xffff, (byte)value);
            }
            //无意义
            //else
            //{
            //    int i1 = 1;
            //}
        }
        public static void MWriteWord(int address, short value)
        {
            address &= 0xffffff;
            if (address >= 0x100000 && address + 1 <= 0x1fffff)
            {
                if (address == 0x1007c4 && value == unchecked((short)0xb102))
                {
                    int i1 = 1;
                }
                Memory.mainram[address & 0xffff] = (byte)(value >> 8);
                Memory.mainram[(address & 0xffff) + 1] = (byte)value;
            }
            else if (address >= 0x2ffff0 && address <= 0x2fffff)
            {
                main_cpu_bank_select_w(value);
            }
            else if (address >= 0x300000 && address <= 0x31ffff)
            {
                int i1 = 1;
                //watchdog_w();
            }
            else if (address >= 0x320000 && address <= 0x33ffff)
            {
                audio_command_w((byte)(value >> 8));
            }
            else if (address >= 0x380000 && address <= 0x39ffff)
            {
                io_control_w((address & 0x7f) >> 1, (byte)value);
            }
            else if (address >= 0x3a0000 && address <= 0x3bffff)
            {
                system_control_w((address & 0x1f) >> 1);
            }
            else if (address >= 0x3c0000 && address <= 0x3dffff)
            {
                neogeo_video_register_w((address & 0x0f) >> 1, (ushort)value);
            }
            else if (address >= 0x400000 && address <= 0x7fffff)
            {
                neogeo_paletteram_w((address & 0x1fff) >> 1, (ushort)value);
            }
            else if (address >= 0xd00000 && address + 1 <= 0xdfffff)
            {
                save_ram_w(address & 0xffff, (byte)(value >> 8));
                save_ram_w((address & 0xffff) + 1, (byte)value);
            }
            else
            {
                int i1 = 1;
            }
        }
        public static void MWriteLong(int address, int value)
        {
            address &= 0xffffff;
            if (address >= 0x100000 && address + 3 <= 0x1fffff)
            {
                if (address == 0x1051e4 && value == 0x00130070)
                {
                    int i1 = 1;
                }
                Memory.mainram[address & 0xffff] = (byte)(value >> 24);
                Memory.mainram[(address & 0xffff) + 1] = (byte)(value >> 16);
                Memory.mainram[(address & 0xffff) + 2] = (byte)(value >> 8);
                Memory.mainram[(address & 0xffff) + 3] = (byte)value;
            }
            else if (address >= 0x2ffff0 && address <= 0x2fffff)
            {
                main_cpu_bank_select_w(value);
            }
            else if (address >= 0x300000 && address <= 0x31ffff)
            {
                int i1 = 1;
                //watchdog_w();
            }
            else if (address >= 0x320000 && address <= 0x33ffff)
            {
                int i1 = 1;
                //audio_command_w
            }
            else if (address >= 0x380000 && address <= 0x39ffff)
            {
                int i1 = 1;
                //io_control_w((address & 0x7f) >> 1, value);
            }
            else if (address >= 0x3a0000 && address <= 0x3bffff)
            {
                //system_control_w((address &0x1f) >> 1, mem_mask);
                int i1 = 1;
            }
            else if (address >= 0x3c0000 && address + 3 <= 0x3dffff)
            {
                neogeo_video_register_w((address & 0x0f) >> 1, (ushort)(value >> 16));
                neogeo_video_register_w(((address & 0x0f) >> 1) + 1, (ushort)value);
            }
            else if (address >= 0x400000 && address + 3 <= 0x7fffff)
            {
                neogeo_paletteram_w((address & 0x1fff) >> 1, (ushort)(value >> 16));
                neogeo_paletteram_w(((address & 0x1fff) >> 1) + 1, (ushort)value);
            }
            else if (address >= 0xd00000 && address + 3 <= 0xdfffff)
            {
                save_ram_w(address & 0xffff, (byte)(value >> 24));
                save_ram_w((address & 0xffff) + 1, (byte)(value >> 16));
                save_ram_w((address & 0xffff) + 2, (byte)(value >> 8));
                save_ram_w((address & 0xffff) + 3, (byte)value);
            }
            else
            {
                int i1 = 1;
            }
        }
        public static sbyte MReadByte_fatfury2(int address)
        {
            address &= 0xffffff;
            sbyte result = 0;
            if (address >= 0x200000 && address <= 0x2fffff)
            {
                int offset = (address - 0x200000) / 2;
                result = (sbyte)fatfury2_protection_16_r(offset);
            }
            else
            {
                result = MReadByte(address);
            }
            return result;
        }
        public static short MReadWord_fatfury2(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address >= 0x200000 && address + 1 <= 0x2fffff)
            {
                int offset = (address - 0x200000) / 2;
                result = (short)fatfury2_protection_16_r(offset);
            }
            else
            {
                result = MReadWord(address);
            }
            return result;
        }
        public static int MReadLong_fatfury2(int address)
        {
            address &= 0xffffff;
            int result = 0;
            if (address >= 0x200000 && address + 3 <= 0x2fffff)
            {
                result = 0;
            }
            else
            {
                result = MReadLong(address);
            }
            return result;
        }
        public static void MWriteByte_fatfury2(int address, sbyte value)
        {
            address &= 0xffffff;
            if (address >= 0x200000 && address <= 0x2fffff)
            {
                int offset = (address - 0x200000) / 2;
                fatfury2_protection_16_w(offset);
            }
            else
            {
                MWriteByte(address, value);
            }
        }
        public static void MWriteWord_fatfury2(int address, short value)
        {
            address &= 0xffffff;
            if (address >= 0x200000 && address + 1 <= 0x2fffff)
            {
                int offset = (address - 0x200000) / 2;
                fatfury2_protection_16_w(offset);
            }
            else
            {
                MWriteWord(address, value);
            }
        }
        public static void MWriteLong_fatfury2(int address, int value)
        {
            address &= 0xffffff;
            if (address >= 0x200000 && address + 3 <= 0x2fffff)
            {
                int i1 = 1;
            }
            else
            {
                MWriteLong(address, value);
            }
        }
        public static sbyte MReadByte_irrmaze(int address)
        {
            address &= 0xffffff;
            sbyte result = 0;
            if (address >= 0x300000 && address <= 0x300001)
            {
                if (address == 0x300000)
                {
                    result = (sbyte)dsw;
                }
                else if (address == 0x300001)
                {
                    if ((controller_select & 0x01) == 0)
                    {
                        result = (sbyte)(Inptport.input_port_read_direct(Inptport.analog_p0) ^ 0xff);
                    }
                    else if ((controller_select & 0x01) == 1)
                    {
                        result = (sbyte)(Inptport.input_port_read_direct(Inptport.analog_p1) ^ 0xff);
                    }
                }
            }
            else
            {
                result = MReadByte(address);
            }
            return result;
        }
        public static void MWriteWord_kof98(int address, short value)
        {
            address &= 0xffffff;
            if (address >= 0x20aaaa && address + 1 <= 0x20aaab)
            {
                kof98_prot_w(value);
            }
            else
            {
                MWriteWord(address, value);
            }
        }
        public static sbyte MReadByte_kof99(int address)
        {
            address &= 0xffffff;
            sbyte result = 0;
            if (address >= 0x2fe446 && address <= 0x2fe447)
            {
                result = (sbyte)prot_9a37_r();
            }
            else if (address >= 0x2ffff8 && address <= 0x2ffffb)
            {
                result = (sbyte)sma_random_r();
            }
            else
            {
                result = MReadByte(address);
            }
            return result;
        }
        public static short MReadWord_kof99(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address >= 0x2fe446 && address + 1 <= 0x2fe447)
            {
                result = (short)prot_9a37_r();
            }
            else if (address >= 0x2ffff8 && address + 1 <= 0x2ffffb)
            {
                result = (short)sma_random_r();
            }
            else
            {
                result = MReadWord(address);
            }
            return result;
        }
        public static int MReadLong_kof99(int address)
        {
            address &= 0xffffff;
            int result = 0;
            if (address >= 0x2fe446 && address <= 0x2fe447)
            {
                result = 0;
            }
            else if (address >= 0x2ffff8 && address <= 0x2ffffb)
            {
                result = 0;
            }
            else
            {
                result = MReadLong(address);
            }
            return result;
        }
        public static void MWriteByte_kof99(int address, sbyte value)
        {
            address &= 0xffffff;
            if (address >= 0x2ffff0 && address <= 0x2ffff1)
            {
                kof99_bankswitch_w(value);
            }
            else
            {
                MWriteByte(address, value);
            }
        }
        public static void MWriteWord_kof99(int address, short value)
        {
            address &= 0xffffff;
            if (address >= 0x2ffff0 && address + 1 <= 0x2ffff1)
            {
                kof99_bankswitch_w(value);
            }
            else
            {
                MWriteWord(address, value);
            }
        }
        public static void MWriteLong_kof99(int address, int value)
        {
            address &= 0xffffff;
            if (address >= 0x2ffff0 && address <= 0x2ffff1)
            {
                int i1 = 1;
            }
            else
            {
                MWriteLong(address, value);
            }
        }
        public static sbyte MReadByte_garou(int address)
        {
            address &= 0xffffff;
            sbyte result = 0;
            if (address >= 0x2fe446 && address <= 0x2fe447)
            {
                result = (sbyte)prot_9a37_r();
            }
            else if (address >= 0x2fffcc && address <= 0x2fffcd)
            {
                result = (sbyte)sma_random_r();
            }
            else if (address >= 0x2ffff0 && address <= 0x2ffff1)
            {
                result = (sbyte)sma_random_r();
            }
            else
            {
                result = MReadByte(address);
            }
            return result;
        }
        public static short MReadWord_garou(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address >= 0x2fe446 && address + 1 <= 0x2fe447)
            {
                result = (short)prot_9a37_r();
            }
            else if (address >= 0x2fffcc && address + 1 <= 0x2fffcd)
            {
                result = (short)sma_random_r();
            }
            else if (address >= 0x2ffff0 && address + 1 <= 0x2ffff1)
            {
                result = (short)sma_random_r();
            }
            else
            {
                result = MReadWord(address);
            }
            return result;
        }
        public static int MReadLong_garou(int address)
        {
            address &= 0xffffff;
            int result = 0;
            if (address >= 0x2fe446 && address <= 0x2fe447)
            {
                result = 0;
            }
            else if (address >= 0x2fffcc && address <= 0x2fffcd)
            {
                result = 0;
            }
            else if (address >= 0x2ffff0 && address <= 0x2ffff1)
            {
                result = 0;
            }
            else
            {
                result = MReadLong(address);
            }
            return result;
        }
        public static void MWriteByte_garou(int address, sbyte value)
        {
            address &= 0xffffff;
            if (address >= 0x2fffc0 && address <= 0x2fffc1)
            {
                garou_bankswitch_w(value);
            }
            else
            {
                MWriteByte(address, value);
            }
        }
        public static void MWriteWord_garou(int address, short value)
        {
            address &= 0xffffff;
            if (address >= 0x2fffc0 && address + 1 <= 0x2fffc1)
            {
                garou_bankswitch_w(value);
            }
            else
            {
                MWriteWord(address, value);
            }
        }
        public static void MWriteLong_garou(int address, int value)
        {
            address &= 0xffffff;
            if (address >= 0x2fffc0 && address <= 0x2fffc1)
            {
                int i1 = 1;
            }
            else
            {
                MWriteLong(address, value);
            }
        }
        public static void MWriteByte_garouh(int address, sbyte value)
        {
            if (address >= 0x2fffc0 && address <= 0x2fffc1)
            {
                garouh_bankswitch_w(value);
            }
            else
            {
                MWriteByte(address, value);
            }
        }
        public static void MWriteWord_garouh(int address, short value)
        {
            address &= 0xffffff;
            if (address >= 0x2fffc0 && address + 1 <= 0x2fffc1)
            {
                garouh_bankswitch_w(value);
            }
            else
            {
                MWriteWord(address, value);
            }
        }
        public static void MWriteLong_garouh(int address, int value)
        {
            address &= 0xffffff;
            if (address >= 0x2fffc0 && address <= 0x2fffc1)
            {
                int i1 = 1;
            }
            else
            {
                MWriteLong(address, value);
            }
        }
        public static sbyte MReadByte_mslug3(int address)
        {
            address &= 0xffffff;
            sbyte result = 0;
            if (address >= 0x2fe446 && address <= 0x2fe447)
            {
                result = (sbyte)prot_9a37_r();
            }
            else
            {
                result = MReadByte(address);
            }
            return result;
        }
        public static short MReadWord_mslug3(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address >= 0x2fe446 && address + 1 <= 0x2fe447)
            {
                result = (short)prot_9a37_r();
            }
            else
            {
                result = MReadWord(address);
            }
            return result;
        }
        public static int MReadLong_mslug3(int address)
        {
            address &= 0xffffff;
            int result = 0;
            if (address >= 0x2fe446 && address <= 0x2fe447)
            {
                result = 0;
            }
            else
            {
                result = MReadLong(address);
            }
            return result;
        }
        public static void MWriteByte_mslug3(int address, sbyte value)
        {
            address &= 0xffffff;
            if (address >= 0x2fffe4 && address <= 0x2fffe5)
            {
                mslug3_bankswitch_w(value);
            }
            else
            {
                MWriteByte(address, value);
            }
        }
        public static void MWriteWord_mslug3(int address, short value)
        {
            address &= 0xffffff;
            if (address >= 0x2fffe4 && address + 1 <= 0x2fffe5)
            {
                mslug3_bankswitch_w(value);
            }
            else
            {
                MWriteWord(address, value);
            }
        }
        public static void MWriteLong_mslug3(int address, int value)
        {
            address &= 0xffffff;
            if (address >= 0x2fffe4 && address <= 0x2fffe5)
            {
                int i1 = 1;
            }
            else
            {
                MWriteLong(address, value);
            }
        }
        public static sbyte MReadByte_kof2000(int address)
        {
            address &= 0xffffff;
            sbyte result = 0;
            if (address >= 0x2fe446 && address <= 0x2fe447)
            {
                if (address == 0x2fe446)
                {
                    result = (sbyte)(prot_9a37_r() >> 8);
                }
                else if (address == 0x2fe447)
                {
                    result = (sbyte)prot_9a37_r();
                }
            }
            else if (address >= 0x2fffd8 && address <= 0x2fffd9)
            {
                result = (sbyte)sma_random_r();
            }
            else if (address >= 0x2fffda && address <= 0x2fffdb)
            {
                result = (sbyte)sma_random_r();
            }
            else
            {
                result = MReadByte(address);
            }
            return result;
        }
        public static short MReadWord_kof2000(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address >= 0x2fe446 && address + 1 <= 0x2fe447)
            {
                result = (short)prot_9a37_r();
            }
            else if (address >= 0x2fffd8 && address + 1 <= 0x2fffd9)
            {
                result = (short)sma_random_r();
            }
            else if (address >= 0x2fffda && address + 1 <= 0x2fffdb)
            {
                result = (short)sma_random_r();
            }
            else
            {
                result = MReadWord(address);
            }
            return result;
        }
        public static int MReadLong_kof2000(int address)
        {
            address &= 0xffffff;
            int result = 0;
            if (address >= 0x2fe446 && address <= 0x2fe447)
            {
                result = 0;
            }
            else if (address >= 0x2fffd8 && address <= 0x2fffd9)
            {
                result = 0;
            }
            else if (address >= 0x2fffda && address <= 0x2fffdb)
            {
                result = 0;
            }
            else
            {
                result = MReadLong(address);
            }
            return result;
        }
        public static void MWriteByte_kof2000(int address, sbyte value)
        {
            address &= 0xffffff;
            if (address >= 0x2fffec && address <= 0x2fffed)
            {
                kof2000_bankswitch_w(value);
            }
            else
            {
                MWriteByte(address, value);
            }
        }
        public static void MWriteWord_kof2000(int address, short value)
        {
            address &= 0xffffff;
            if (address >= 0x2fffec && address + 1 <= 0x2fffed)
            {
                kof2000_bankswitch_w(value);
            }
            else
            {
                MWriteWord(address, value);
            }
        }
        public static void MWriteLong_kof2000(int address, int value)
        {
            address &= 0xffffff;
            if (address >= 0x2fffec && address <= 0x2fffed)
            {
                kof2000_bankswitch_w(value);
            }
            else
            {
                MWriteLong(address, value);
            }
        }
        public static sbyte MReadByte_pvc(int address)
        {
            address &= 0xffffff;
            sbyte result = 0;
            if (address >= 0x2fe000 && address <= 0x2fffff)
            {
                result = (sbyte)pvc_cartridge_ram[address - 0x2fe000];
            }
            else
            {
                result = MReadByte(address);
            }
            return result;
        }
        public static short MReadWord_pvc(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address >= 0x2fe000 && address + 1 <= 0x2fffff)
            {
                result = (short)(pvc_cartridge_ram[address - 0x2fe000] * 0x100 + pvc_cartridge_ram[address - 0x2fe000 + 1]);
            }
            else
            {
                result = MReadWord(address);
            }
            return result;
        }
        public static int MReadLong_pvc(int address)
        {
            address &= 0xffffff;
            int result = 0;
            if (address >= 0x2fe000 && address + 3 <= 0x2fffff)
            {
                result = pvc_cartridge_ram[address - 0x2fe000] * 0x1000000 + pvc_cartridge_ram[address - 0x2fe000 + 1] * 0x10000 + pvc_cartridge_ram[address - 0x2fe000 + 2] * 0x100 + pvc_cartridge_ram[address - 0x2fe000 + 3];
            }
            else
            {
                result = MReadLong(address);
            }
            return result;
        }
        public static void MWriteByte_pvc(int address, sbyte value)
        {
            address &= 0xffffff;
            if (address >= 0x2fe000 && address <= 0x2fffff)
            {
                pvc_cartridge_ram[address - 0x2fe000] = (byte)value;
                int offset = (address - 0x2fe000) / 2;
                if (offset == 0xff0)
                    pvc_prot1();
                else if (offset >= 0xff4 && offset <= 0xff5)
                    pvc_prot2();
                else if (offset >= 0xff8)
                    pvc_write_bankswitch();
            }
            else
            {
                MWriteByte(address, value);
            }
        }
        public static void MWriteWord_pvc(int address, short value)
        {
            address &= 0xffffff;
            if (address >= 0x2fe000 && address + 1 <= 0x2fffff)
            {
                pvc_cartridge_ram[address - 0x2fe000] = (byte)(value >> 8);
                pvc_cartridge_ram[address - 0x2fe000 + 1] = (byte)(value);
                int offset = (address - 0x2fe000) / 2;
                if (offset == 0xff0)
                    pvc_prot1();
                else if (offset >= 0xff4 && offset <= 0xff5)
                    pvc_prot2();
                else if (offset >= 0xff8)
                    pvc_write_bankswitch();
            }
            else
            {
                MWriteWord(address, value);
            }
        }
        public static void MWriteLong_pvc(int address, int value)
        {
            address &= 0xffffff;
            if (address >= 0x2fe000 && address + 3 <= 0x2fffff)
            {
                pvc_cartridge_ram[address - 0x2fe000] = (byte)(value >> 24);
                pvc_cartridge_ram[address - 0x2fe000 + 1] = (byte)(value >> 16);
                pvc_cartridge_ram[address - 0x2fe000 + 2] = (byte)(value >> 8);
                pvc_cartridge_ram[address - 0x2fe000 + 3] = (byte)(value);
                int offset = (address - 0x2fe000) / 2;
                if (offset == 0xff0)
                    pvc_prot1();
                else if (offset >= 0xff4 && offset <= 0xff5)
                    pvc_prot2();
                else if (offset >= 0xff8)
                    pvc_write_bankswitch();

                if (offset + 1 == 0xff0)
                    pvc_prot1();
                else if (offset + 1 >= 0xff4 && offset + 1 <= 0xff5)
                    pvc_prot2();
                else if (offset + 1 >= 0xff8)
                    pvc_write_bankswitch();
            }
            else
            {
                MWriteLong(address, value);
            }
        }
        public static void MWriteByte_cthd2003(int address, sbyte value)
        {
            address &= 0xffffff;
            if (address >= 0x2ffff0 && address <= 0x2ffff1)
            {
                cthd2003_bankswitch_w(value);
            }
            else
            {
                MWriteByte(address, value);
            }
        }
        public static void MWriteWord_cthd2003(int address, short value)
        {
            address &= 0xffffff;
            if (address >= 0x2ffff0 && address + 1 <= 0x2ffff1)
            {
                cthd2003_bankswitch_w(value);
            }
            else
            {
                MWriteWord(address, value);
            }
        }
        public static sbyte MReadByte_ms5plus(int address)
        {
            address &= 0xffffff;
            sbyte result = 0;
            if (address >= 0x2ffff0 && address <= 0x2fffff)
            {
                result = (sbyte)mslug5_prot_r();
            }
            else
            {
                result = MReadByte(address);
            }
            return result;
        }
        public static short MReadWord_ms5plus(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address >= 0x2ffff0 && address + 1 <= 0x2fffff)
            {
                result = (short)mslug5_prot_r();
            }
            else
            {
                result = MReadWord(address);
            }
            return result;
        }
        public static int MReadLong_ms5plus(int address)
        {
            address &= 0xffffff;
            int result = 0;
            if (address >= 0x2ffff0 && address + 3 <= 0x2fffff)
            {
                result = 0;
            }
            else
            {
                result = MReadLong(address);
            }
            return result;
        }
        public static void MWriteByte_ms5plus(int address, sbyte value)
        {
            address &= 0xffffff;
            if (address >= 0x2ffff0 && address <= 0x2fffff)
            {
                int offset = (address - 0x2ffff0) / 2;
                ms5plus_bankswitch_w(offset, value);
            }
            else
            {
                MWriteByte(address, value);
            }
        }
        public static void MWriteWord_ms5plus(int address, short value)
        {
            address &= 0xffffff;
            if (address >= 0x2ffff0 && address + 1 <= 0x2fffff)
            {
                int offset = (address - 0x2ffff0) / 2;
                ms5plus_bankswitch_w(offset, value);
            }
            else
            {
                MWriteWord(address, value);
            }
        }
        public static void MWriteLong_ms5plus(int address, int value)
        {
            address &= 0xffffff;
            if (address >= 0x2ffff0 && address + 3 <= 0x2fffff)
            {
                int i1 = 1;
            }
            else
            {
                MWriteLong(address, value);
            }
        }
        public static sbyte MReadByte_kog(int address)
        {
            address &= 0xffffff;
            sbyte result = 0;
            if (address >= 0x0ffffe && address <= 0x0fffff)
            {
                if (address == 0x0ffffe)
                {
                    result = unchecked((sbyte)0xff);
                }
                else
                {
                    result = 0x01;
                }
            }
            else
            {
                result = MReadByte(address);
            }
            return result;
        }
        public static short MReadWord_kog(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address >= 0x0ffffe && address + 1 <= 0x0fffff)
            {
                result = unchecked((short)0xff01);
            }
            else
            {
                result = MReadWord(address);
            }
            return result;
        }
        public static int MReadLong_kog(int address)
        {
            address &= 0xffffff;
            int result = 0;
            if (address >= 0x0ffffe && address <= 0x0fffff)
            {
                result = 0;
            }
            else
            {
                result = MReadLong(address);
            }
            return result;
        }
        public static sbyte MReadByte_kf2k3bl(int address)
        {
            address &= 0xffffff;
            sbyte result = 0;
            if (address >= 0x2fe000 && address <= 0x2fffff)
            {
                int offset = address - 0x2fe000;
                result = (sbyte)(extra_ram[offset]);
            }
            else
            {
                result = MReadByte(address);
            }
            return result;
        }
        public static short MReadWord_kf2k3bl(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address >= 0x2fe000 && address + 1 <= 0x2fffff)
            {
                int offset = address - 0x2fe000;
                result = (short)(extra_ram[offset] * 0x100 + extra_ram[offset + 1]);
            }
            else
            {
                result = MReadWord(address);
            }
            return result;
        }
        public static int MReadLong_kf2k3bl(int address)
        {
            address &= 0xffffff;
            int result = 0;
            if (address >= 0x2fe000 && address + 3 <= 0x2fffff)
            {
                int offset = address - 0x2fe000;
                result = extra_ram[offset] * 0x1000000 + extra_ram[offset + 1] * 0x10000 + extra_ram[offset + 2] * 0x100 + extra_ram[offset + 3];
            }
            else
            {
                result = MReadLong(address);
            }
            return result;
        }
        public static void MWriteByte_kf2k3bl(int address, sbyte value)
        {
            address &= 0xffffff;
            if (address >= 0x2fe000 && address <= 0x2fffff)
            {
                int offset = address - 0x2fe000;
                extra_ram[offset] = (byte)value;
                kof2003_w(offset / 2);
            }
            else
            {
                MWriteByte(address, value);
            }
        }
        public static void MWriteWord_kf2k3bl(int address, short value)
        {
            address &= 0xffffff;
            if (address >= 0x2fe000 && address + 1 <= 0x2fffff)
            {
                int offset = address - 0x2fe000;
                extra_ram[offset] = (byte)(value >> 8);
                extra_ram[offset + 1] = (byte)value;
                kof2003_w(offset / 2);
            }
            else
            {
                MWriteWord(address, value);
            }
        }
        public static void MWriteLong_kf2k3bl(int address, int value)
        {
            address &= 0xffffff;
            if (address >= 0x2fe000 && address + 3 <= 0x2fffff)
            {
                int offset = address - 0x2fe000;
                extra_ram[offset] = (byte)(value >> 24);
                extra_ram[offset + 1] = (byte)(value >> 16);
                extra_ram[offset + 2] = (byte)(value >> 8);
                extra_ram[offset + 3] = (byte)value;
                kof2003_w(offset / 2);
                kof2003_w((offset + 2) / 2);
            }
            else
            {
                MWriteLong(address, value);
            }
        }
        public static void MWriteByte_kf2k3pl(int address, sbyte value)
        {
            address &= 0xffffff;
            if (address >= 0x2fe000 && address <= 0x2fffff)
            {
                int offset = address - 0x2fe000;
                extra_ram[offset] = (byte)value;
                kof2003p_w(offset / 2);
            }
            else
            {
                MWriteByte(address, value);
            }
        }
        public static void MWriteWord_kf2k3pl(int address, short value)
        {
            address &= 0xffffff;
            if (address >= 0x2fe000 && address + 1 <= 0x2fffff)
            {
                int offset = address - 0x2fe000;
                extra_ram[offset] = (byte)(value >> 8);
                extra_ram[offset + 1] = (byte)value;
                kof2003p_w(offset / 2);
            }
            else
            {
                MWriteWord(address, value);
            }
        }
        public static void MWriteLong_kf2k3pl(int address, int value)
        {
            address &= 0xffffff;
            if (address >= 0x2fe000 && address + 3 <= 0x2fffff)
            {
                int offset = address - 0x2fe000;
                extra_ram[offset] = (byte)(value >> 24);
                extra_ram[offset + 1] = (byte)(value >> 16);
                extra_ram[offset + 2] = (byte)(value >> 8);
                extra_ram[offset + 3] = (byte)value;
                kof2003p_w(offset / 2);
                kof2003p_w((offset + 2) / 2);
            }
            else
            {
                MWriteLong(address, value);
            }
        }
        public static sbyte MReadByte_sbp(int address)
        {
            address &= 0xffffff;
            sbyte result = 0;
            if (address >= 0x00200 && address <= 0x001fff)
            {
                int offset = address - 0x200;
                result = (sbyte)sbp_protection_r(offset);
            }
            else
            {
                result = MReadByte(address);
            }
            return result;
        }
        public static short MReadWord_sbp(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address >= 0x00200 && address + 1 <= 0x001fff)
            {
                int offset = address - 0x200;
                result = (short)(sbp_protection_r(offset) * 0x100 + sbp_protection_r(offset + 1));
            }
            else
            {
                result = MReadWord(address);
            }
            return result;
        }
        public static int MReadLong_sbp(int address)
        {
            address &= 0xffffff;
            int result = 0;
            if (address >= 0x00200 && address + 3 <= 0x001fff)
            {
                int offset = address - 0x200;
                result = sbp_protection_r(offset) * 0x1000000 + sbp_protection_r(offset + 1) * 0x10000 + sbp_protection_r(offset + 2) * 0x100 + sbp_protection_r(offset + 3);
            }
            else
            {
                result = MReadLong(address);
            }
            return result;
        }
        public static sbyte MReadByte_kof10th(int address)
        {
            address &= 0xffffff;
            sbyte result = 0;
            if (address >= 0x2fe000 && address <= 0x2fffff)
            {
                int offset = address - 0x2fe000;
                result = (sbyte)extra_ram[offset];
            }
            else
            {
                result = MReadByte(address);
            }
            return result;
        }
        public static short MReadWord_kof10th(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address >= 0x2fe000 && address + 1 <= 0x2fffff)
            {
                int offset = address - 0x2fe000;
                result = (short)(extra_ram[offset] * 0x100 + extra_ram[offset + 1]);
            }
            else
            {
                result = MReadWord(address);
            }
            return result;
        }
        public static int MReadLong_kof10th(int address)
        {
            address &= 0xffffff;
            int result = 0;
            if (address >= 0x2fe000 && address + 3 <= 0x2fffff)
            {
                int offset = address - 0x2fe000;
                result = extra_ram[offset] * 0x1000000 + extra_ram[offset + 1] * 0x10000 + extra_ram[offset + 2] * 0x100 + extra_ram[offset + 3];
            }
            else
            {
                result = MReadLong(address);
            }
            return result;
        }
        public static void MWriteByte_sbp(int address, sbyte value)
        {
            address &= 0xffffff;
            if (address >= 0x200 && address <= 0x1fff)
            {
                int offset = (address - 0x200) / 2;
                sbp_protection_w(offset, value);
            }
            else
            {
                MWriteByte(address, value);
            }
        }
        public static void MWriteWord_sbp(int address, short value)
        {
            address &= 0xffffff;
            if (address >= 0x200 && address + 1 <= 0x1fff)
            {
                int offset = (address - 0x200) / 2;
                sbp_protection_w(offset, value);
            }
            else
            {
                MWriteWord(address, value);
            }
        }
        public static void MWriteLong_sbp(int address, int value)
        {
            address &= 0xffffff;
            if (address >= 0x200 && address + 3 <= 0x1fff)
            {
                int offset = (address - 0x200) / 2;
                sbp_protection_w(offset, (ushort)(value >> 16));
                sbp_protection_w(offset + 1, (byte)value);
            }
            else
            {
                MWriteLong(address, value);
            }
        }
        public static void MWriteByte_kof10th(int address, sbyte value)
        {
            address &= 0xffffff;
            if (address >= 0x200000 && address <= 0x23ffff)
            {
                int offset = address - 0x200000;
                kof10th_custom_w(offset, (byte)value);
            }
            else if (address >= 0x240000 && address <= 0x2fffff)
            {
                int offset = address - 0x240000;
                kof10th_bankswitch_w(offset, (byte)value);
            }
            else
            {
                MWriteByte(address, value);
            }
        }
        public static void MWriteWord_kof10th(int address, short value)
        {
            address &= 0xffffff;
            if (address >= 0x200000 && address + 1 <= 0x23ffff)
            {
                int offset = address - 0x200000;
                kof10th_custom_w(offset, (ushort)value);
            }
            else if (address >= 0x240000 && address + 1 <= 0x2fffff)
            {
                int offset = address - 0x240000;
                kof10th_bankswitch_w(offset, (ushort)value);
            }
            else
            {
                MWriteWord(address, value);
            }
        }
        public static void MWriteLong_kof10th(int address, int value)
        {
            address &= 0xffffff;
            if (address >= 0x200000 && address + 3 <= 0x23ffff)
            {
                int i1 = 1;
            }
            else if (address >= 0x240000 && address + 3 <= 0x2fffff)
            {
                int offset = address - 0x240000;
                kof10th_bankswitch_w(offset, (ushort)(value >> 16));
                kof10th_bankswitch_w(offset + 2, (ushort)value);
            }
            else
            {
                MWriteLong(address, value);
            }
        }
        public static sbyte MReadByte_jockeygp(int address)
        {
            address &= 0xffffff;
            sbyte result = 0;
            if (address >= 0x200000 && address <= 0x201fff)
            {
                int offset = address - 0x200000;
                result = (sbyte)extra_ram[offset];
            }
            else
            {
                result = MReadByte(address);
            }
            return result;
        }
        public static short MReadWord_jockeygp(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address >= 0x200000 && address + 1 <= 0x201fff)
            {
                int offset = address - 0x200000;
                result = (short)(extra_ram[offset] * 0x100 + extra_ram[offset + 1]);
            }
            else
            {
                result = MReadWord(address);
            }
            return result;
        }
        public static int MReadLong_jockeygp(int address)
        {
            address &= 0xffffff;
            int result = 0;
            if (address >= 0x200000 && address + 3 <= 0x201fff)
            {
                int offset = address - 0x200000;
                result = extra_ram[offset] * 0x1000000 + extra_ram[offset + 1] * 0x10000 + extra_ram[offset + 2] * 0x100 + extra_ram[offset + 3];
            }
            else
            {
                result = MReadLong(address);
            }
            return result;
        }
        public static void MWriteByte_jockeygp(int address, sbyte value)
        {
            address &= 0xffffff;
            if (address >= 0x200000 && address <= 0x201fff)
            {
                int offset = address - 0x200000;
                extra_ram[offset] = (byte)value;
            }
            else
            {
                MWriteByte(address, value);
            }
        }
        public static void MWriteWord_jockeygp(int address, short value)
        {
            address &= 0xffffff;
            if (address >= 0x200000 && address + 1 <= 0x201fff)
            {
                int offset = address - 0x200000;
                extra_ram[offset] = (byte)(value >> 8);
                extra_ram[offset + 1] = (byte)(value);
            }
            else
            {
                MWriteWord(address, value);
            }
        }
        public static void MWriteLong_jockeygp(int address, int value)
        {
            address &= 0xffffff;
            if (address >= 0x200000 && address + 3 <= 0x201fff)
            {
                int offset = address - 0x200000;
                extra_ram[offset] = (byte)(value >> 24);
                extra_ram[offset + 1] = (byte)(value >> 16);
                extra_ram[offset + 2] = (byte)(value >> 8);
                extra_ram[offset + 3] = (byte)(value);
            }
            else
            {
                MWriteLong(address, value);
            }
        }
        public static sbyte MReadByte_vliner(int address)
        {
            address &= 0xffffff;
            sbyte result = 0;
            if (address >= 0x200000 && address <= 0x201fff)
            {
                int offset = address - 0x200000;
                result = (sbyte)extra_ram[offset];
            }
            else if (address >= 0x280000 && address <= 0x280001)
            {
                if (address == 0x280000)
                {
                    result = (sbyte)(short5 >> 8);
                }
                else
                {
                    result = (sbyte)(short5);
                }
            }
            else if (address >= 0x2c0000 && address <= 0x2c0001)
            {
                if (address == 0x2c0000)
                {
                    result = (sbyte)(short6 >> 8);
                }
                else
                {
                    result = (sbyte)(short6);
                }
            }
            else
            {
                result = MReadByte(address);
            }
            return result;
        }
        public static short MReadWord_vliner(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address >= 0x200000 && address + 1 <= 0x201fff)
            {
                int offset = address - 0x200000;
                result = (short)(extra_ram[offset] * 0x100 + extra_ram[offset + 1]);
            }
            else if (address >= 0x280000 && address + 1 <= 0x280001)
            {
                result = short5;
            }
            else if (address >= 0x2c0000 && address + 1 <= 0x2c0001)
            {
                result = short6;
            }
            else
            {
                result = MReadWord(address);
            }
            return result;
        }
        public static int MReadLong_vliner(int address)
        {
            address &= 0xffffff;
            int result = 0;
            if (address >= 0x200000 && address + 3 <= 0x201fff)
            {
                int offset = address - 0x200000;
                result = extra_ram[offset] * 0x1000000 + extra_ram[offset + 1] * 0x10000 + extra_ram[offset + 2] * 0x100 + extra_ram[offset + 3];
            }
            else if (address >= 0x280000 && address <= 0x280001)
            {
                result = 0;
            }
            else if (address >= 0x2c0000 && address <= 0x2c0001)
            {
                result = 0;
            }
            else
            {
                result = MReadLong(address);
            }
            return result;
        }
        public static sbyte MReadByte_(int address)
        {
            address &= 0xffffff;
            sbyte result = 0;
            if (address >= 0 && address <= 0)
            {
                result = 0;
            }
            else
            {
                result = MReadByte(address);
            }
            return result;
        }
        public static short MReadWord_(int address)
        {
            address &= 0xffffff;
            short result = 0;
            if (address >= 0 && address <= 0)
            {
                result = 0;
            }
            else
            {
                result = MReadWord(address);
            }
            return result;
        }
        public static int MReadLong_(int address)
        {
            address &= 0xffffff;
            int result = 0;
            if (address >= 0 && address <= 0)
            {
                result = 0;
            }
            else
            {
                result = MReadLong(address);
            }
            return result;
        }
        public static void MWriteByte_(int address, sbyte value)
        {
            if (address >= 0 && address <= 0)
            {

            }
            else
            {
                MWriteByte(address, value);
            }
        }
        public static void MWriteWord_(int address, short value)
        {
            if (address >= 0 && address <= 0)
            {

            }
            else
            {
                MWriteWord(address, value);
            }
        }
        public static void MWriteLong_(int address, int value)
        {
            if (address >= 0 && address <= 0)
            {

            }
            else
            {
                MWriteLong(address, value);
            }
        }
        public static byte ZReadOp(ushort address)
        {
            byte result = 0;
            if (address >= 0x0000 && address <= 0x7fff)
            {
                result = Memory.audiorom[address];
            }
            else if (address >= 0x8000 && address <= 0xbfff)
            {
                result = Memory.audiorom[audio_cpu_banks[3] * 0x4000 + address - 0x8000];
            }
            else if (address >= 0xc000 && address <= 0xdfff)
            {
                result = Memory.audiorom[audio_cpu_banks[2] * 0x2000 + address - 0xc000];
            }
            else if (address >= 0xe000 && address <= 0xefff)
            {
                result = Memory.audiorom[audio_cpu_banks[1] * 0x1000 + address - 0xe000];
            }
            else if (address >= 0xf000 && address <= 0xf7ff)
            {
                result = Memory.audiorom[audio_cpu_banks[0] * 0x800 + address - 0xf000];
            }
            else if (address >= 0xf800 && address <= 0xffff)
            {
                result = Memory.audioram[address - 0xf800];
            }
            return result;
        }
        public static byte ZReadMemory(ushort address)
        {
            byte result = 0;
            if (address >= 0x0000 && address <= 0x7fff)
            {
                result = Memory.audiorom[address];
            }
            else if (address >= 0x8000 && address <= 0xbfff)
            {
                result = Memory.audiorom[audio_cpu_banks[3] * 0x4000 + address - 0x8000];
            }
            else if (address >= 0xc000 && address <= 0xdfff)
            {
                result = Memory.audiorom[audio_cpu_banks[2] * 0x2000 + address - 0xc000];
            }
            else if (address >= 0xe000 && address <= 0xefff)
            {
                result = Memory.audiorom[audio_cpu_banks[1] * 0x1000 + address - 0xe000];
            }
            else if (address >= 0xf000 && address <= 0xf7ff)
            {
                result = Memory.audiorom[audio_cpu_banks[0] * 0x800 + address - 0xf000];
            }
            else if (address >= 0xf800 && address <= 0xffff)
            {
                result = Memory.audioram[address - 0xf800];
            }
            return result;
        }
        public static void ZWriteMemory(ushort address, byte value)
        {
            if (address >= 0xf800 && address <= 0xffff)
            {
                Memory.audioram[address - 0xf800] = value;
            }
            else
            {
                int i1 = 1;
            }
        }
        public static byte ZReadHardware(ushort address)
        {
            byte result = 0;
            int add1, add2;
            address &= 0xffff;
            add1 = address & 0xff;
            if (add1 == 0)
            {
                result = audio_command_r();
            }
            else if (add1 >= 0x04 && add1 <= 0x07)
            {
                result = YM2610.F2610.ym2610_read(add1 - 0x04);
            }
            else if (add1 >= 0x08 && add1 <= 0xfb)
            {
                add2 = add1 & 0x0f;
                if (add2 >= 0x08 && add2 <= 0x0b)
                {
                    audio_cpu_banks[add2 - 0x08] = (byte)(address >> 8);
                }
                else
                {
                    int i1 = 1;
                }
                result = 0;
            }
            else
            {
                int i1 = 1;
            }
            return result;
        }
        public static void ZWriteHardware(ushort address, byte value)
        {
            int add1;
            add1 = address & 0xff;
            if (add1 == 0x00)
            {
                Sound.soundlatch_w(0);
            }
            else if (add1 >= 0x04 && add1 <= 0x07)
            {
                YM2610.F2610.ym2610_write(add1 - 0x04, value);
            }
            else if (add1 == 0x08)
            {
                audio_cpu_enable_nmi_w(0);
            }
            else if (add1 == 0x0c)
            {
                audio_result_w(value);
            }
            else if (add1 == 0x18)
            {
                audio_cpu_enable_nmi_w(0x10);
            }
            else
            {
                int i1 = 1;
            }
        }
        public static int ZIRQCallback()
        {
            return 0;
        }
    }
}
