//using System;
//using Unity.Android.Gradle;
//using Unity.Android.Gradle.Manifest;
//using VirtualNes.Core;

//namespace VirtualNes.Core
//{

//    public class map0_3208cn : Mapper
//    {
//        public map0_3208cn(NES parent) : base(parent) { }
//        //void map0_3208cn::Reset()
//        public override void Reset()
//        {
//            SetPROM_32K_Bank(0);
//        }


//        //void map0_3208cn::Write(WORD addr, BYTE data)
//        public override void Write(ushort addr, byte data)
//        {
//            //addr = 0x8000,ÇÐ»» PROM
//            if (addr == 0x8032)
//                SetPROM_32K_Bank(data);
//            if (addr == 0x8416)
//                SetPROM_16K_Bank(4, data);
//            if (addr == 0x8616)
//                SetPROM_16K_Bank(6, data);
//            if (addr == 0x8408)
//                SetPROM_8K_Bank(4, data);
//            if (addr == 0x8508)
//                SetPROM_8K_Bank(5, data);
//            if (addr == 0x8608)
//                SetPROM_8K_Bank(6, data);
//            if (addr == 0x8708)
//                SetPROM_8K_Bank(7, data);

//            //addr = 0x8001,ÇÐ»» VROM
//            if (addr == 0x9008)
//                SetVROM_8K_Bank(data);
//            if (addr == 0x9004)
//                SetVROM_4K_Bank(0, data);
//            if (addr == 0x9404)
//                SetVROM_4K_Bank(4, data);
//            if (addr == 0x9001)
//                SetVROM_1K_Bank(0, data);
//            if (addr == 0x9101)
//                SetVROM_1K_Bank(1, data);
//            if (addr == 0x9201)
//                SetVROM_1K_Bank(2, data);
//            if (addr == 0x9301)
//                SetVROM_1K_Bank(3, data);
//            if (addr == 0x9401)
//                SetVROM_1K_Bank(4, data);
//            if (addr == 0x9501)
//                SetVROM_1K_Bank(5, data);
//            if (addr == 0x9601)
//                SetVROM_1K_Bank(6, data);
//            if (addr == 0x9701)
//                SetVROM_1K_Bank(7, data);
//        }
//    }

//    public class GeniusMerioBros : Mapper
//    {

//        public GeniusMerioBros(NES parent) : base(parent) { }
//        //void GeniusMerioBros::Reset()
//        public override void Reset()
//        {
//            SetPROM_32K_Bank(0);
//            memcpy(WRAM, PROM + 0x2000 * 4, 0x800);
//        }

//        //BYTE GeniusMerioBros::ReadLow(WORD A)
//        public override byte ReadLow(ushort addr)
//        {
//            if (A >= 0x6000 && A <= 0x6FFF)
//            {
//                return CPU_MEM_BANK[0][A & 0x7FF];
//            }
//            else if (A >= 0x7000 && A <= 0x7FFF)
//            {
//                return XRAM[A & 0x7FF];
//            }
//            else
//                return (BYTE)(A >> 8);

//        }
//        //void GeniusMerioBros::WriteLow(WORD A, BYTE V)
//        public override void WriteLow(ushort addr, byte data)
//        {
//            if (A >= 0x7000 && A <= 0x7FFF)
//            {
//                XRAM[A & 0x7FF] = V;
//            }
//            else
//            if (A >= 0x6000 && A <= 0x6FFF)
//            {
//                CPU_MEM_BANK[A >> 13][A & 0x1FFF] = V;
//            }
//        }
//    }

//    public class smb2j : Mapper
//    {

//        BYTE prg, IRQa;
//        WORD IRQCount;
//        public smb2j(NES parent) : base(parent) { }
//        //Super Mario Bros. 2j (Unl) [U][!]
//        //void smb2j::Reset()
//        public override void Reset()
//        {
//            prg = 0;

//            memcpy(MRAM, &PROMPTR[1][0x1000], 0x1000);
//            SetPrg8r(1, 0x6000, 1);
//            SetPROM_32K_Bank(prg);
//            SetVROM_8K_Bank(0);
//            SetVRAM_Mirror(VRAM_VMIRROR);
//            IRQa = 0;
//            IRQCount = 0;
//        }

//        //void smb2j::Write(WORD A, BYTE V)// (0x4020,0xffff)
//        public override void Write(ushort addr, byte data)
//        {
//            if (A == 0x4022)
//            {
//                prg = V & 1;
//                SetPROM_32K_Bank(prg);
//            }
//            if (A == 0x4122)
//            {
//                IRQa = V;
//                IRQCount = 0;
//                nes->cpu->ClrIRQ(IRQ_MAPPER);
//            }
//        }

//        //void smb2j::WriteLow(WORD A, BYTE V)
//        public override void WriteLow(ushort addr, byte data)
//        {
//            Write(A, V);
//        }

//        //void smb2j::Clock(INT a)
//        public override void Clock(int cycles)
//        {
//            if (IRQa)
//            {
//                IRQCount += a * 3;
//                if ((IRQCount >> 12) == IRQa)
//                    nes->cpu->SetIRQ(IRQ_MAPPER);
//            }
//        }

//        //BYTE smb2j::ReadLow(WORD A)
//        public override byte ReadLow(ushort addr)
//        {
//            if ((A >= 0x5000) && (A < 0x6000))
//                return MRAM[A - 0x5000];
//            return Mapper::ReadLow(A);
//        }
//    }

//    //public class smb2j : Mapper
//    //{

//    //    WORD cmdreg;
//    //    BYTE invalid_data;
//    //    public smb2j(NES parent) : base(parent) { }

//    //    void Reset();
//    //    void SoftReset();
//    //    void Write(WORD addr, BYTE data);
//    //    BYTE Read(WORD addr);
//    //    void Sync(void);
//    //}


//    //void Mapper8157::SoftReset()
//    //    {
//    //        cmdreg = 0x200;
//    //        invalid_data ^= 1;
//    //        Sync();
//    //    }

//    //    void Mapper8157::Sync(void)
//    //    {
//    //        SetPrg16r((cmdreg & 0x060) >> 5, 0x8000, (cmdreg & 0x01C) >> 2);
//    //        SetPrg16r((cmdreg & 0x060) >> 5, 0xC000, (cmdreg & 0x200) ? (PROM_16K_SIZE / 4 - 1) : 0);
//    //        SetVRAM_Mirror(((cmdreg & 2) >> 1) ^ 1);
//    //    }

//    //    BYTE Mapper8157::Read(WORD A)
//    //    {
//    //        if (invalid_data && cmdreg & 0x100)
//    //            return 0xFF;
//    //        else
//    //            return Mapper::Read(A);
//    //    }

//    //    void Mapper8157::Write(WORD A, BYTE V)
//    //    {
//    //        cmdreg = A;
//    //        Sync();
//    //    }




//    public class MapperT262 : Mapper
//    {

//        uint16 addrreg;
//        uint8 datareg;
//        uint8 busy;
//        public MapperT262(NES parent) : base(parent) { }

//        //void MapperT262::Reset()
//        public override void Reset()
//        {
//            SetVROM_8K_Bank(0);
//            busy = 0;
//            addrreg = 0;
//            datareg = 0;

//            uint16 base = ((addrreg & 0x60) >> 2) | ((addrreg & 0x100) >> 3);
//            SetPROM_16K_Bank(0x8000 >> 13, (datareg & 7) | base);
//            SetPROM_16K_Bank(0xC000 >> 13, 7 | base);
//            SetVRAM_Mirror(((addrreg & 2) >> 1) ^ 1);
//        }
//        //void MapperT262::Write(WORD A, BYTE V)
//        public override void Write(ushort addr, byte data)
//        {
//            if (busy || (A == 0x8000))
//                datareg = V;
//            else
//            {
//                addrreg = A;
//                busy = 1;
//            }

//            uint16 base = ((addrreg & 0x60) >> 2) | ((addrreg & 0x100) >> 3);
//            SetPROM_16K_Bank(0x8000 >> 13, (datareg & 7) | base);
//            SetPROM_16K_Bank(0xC000 >> 13, 7 | base);
//            SetVRAM_Mirror(((addrreg & 2) >> 1) ^ 1);
//        }
//    }
//}

