//using System;
//using VirtualNes.Core;

//------------------------------.h-------------------------------

//    //////////////////////////////////////////////////////////////////////////
//    // MapperFk23c  WaiXing San Guo Zhi - Xiong Ba Tian Xia (C)             //
//    //////////////////////////////////////////////////////////////////////////
//class MapperFk23c : public MMC3
//{
//public:
//	MapperFk23c(NES * parent);


//void Reset();
//void Write(WORD addr, BYTE data);
//void WriteLow(WORD addr, BYTE data);

//// For state save
//BOOL IsStateSave() { return TRUE; }
//void SaveState(LPBYTE p);
//void LoadState(LPBYTE p);

//protected:
//	BYTE exRegs[8];
//BYTE unromChr, mode;

//private:
//	void fk23c_UpdatePrg2p(unsigned int addr, unsigned int bank);
//void fk23c_UpdateChr2p(unsigned int addr, unsigned int bank);
//virtual void fk23c_UpdatePrg();
//void fk23c_UpdateChr();
//};

//class MapperFk23ca : public MapperFk23c
//{
//public:
//	MapperFk23ca(NES * parent);
//void Reset();

//private:
//	void fk23c_UpdatePrg();
//};

//-----------------------------CPP-------------------------------

////////////////////////////////////////////////////////////////////////////
//// MapperFk23c  WaiXing San Guo Zhi - Xiong Ba Tian Xia (C)             //
////////////////////////////////////////////////////////////////////////////

//using System;
//using VirtualNes.Core;

//MapperFk23c::MapperFk23c(NES * parent) : MMC3(parent)
//{
//    UpdateChr = (void(__thiscall MMC3::* )(unsigned int, unsigned int)) & MapperFk23c::fk23c_UpdateChr2p;
//    UpdatePrg = (void(__thiscall MMC3::* )(unsigned int, unsigned int)) & MapperFk23c::fk23c_UpdatePrg2p;
//}

//void MapperFk23c::Reset()
//{
//    uint8 i;
//    for (i = 0; i < 8; ++i)
//        exRegs[i] = 0xFF;

//    if (PROM_16K_SIZE <= 32)
//    {
//        for (i = 0; i < 4; ++i)
//            exRegs[i] = 0x00;
//    }

//    unromChr = 0x0;
//    mode = 0;

//    MMC3::Reset();
//    Mmc3_UpdatePrg();
//    Mmc3_UpdateChr();
//}

//void MapperFk23c::WriteLow(WORD A, BYTE V)
//{
//    if ((A >= 0x5000) && (A <= 0x5fff))
//    {
//        if (A & (1U << (mode + 4)))
//        {
//            exRegs[A & 0x3] = V;

//            fk23c_UpdatePrg();
//            fk23c_UpdateChr();
//        }
//    }
//    else
//        Mapper::WriteLow(A, V);
//}


//void MapperFk23c::Write(WORD A, BYTE V)
//{
//    if (exRegs[0] & 0x40U)
//    {
//        unromChr = (exRegs[0] & 0x30U) ? 0x0 : V & 0x3;
//        fk23c_UpdateChr();
//    }
//    else switch (A & 0xE001)
//        {
//            case 0x8000: Poke_Mmc3_8000(A, V); break;
//            case 0x8001:

//                if (exRegs[3] << 2 & (ctrl0 & 0x8))
//                {
//                    exRegs[4 | (ctrl0 & 0x3)] = V;

//                    fk23c_UpdatePrg();
//                    fk23c_UpdateChr();
//                }
//                else
//                {
//                    Poke_Mmc3_8001(A, V);
//                }
//                break;

//            case 0xA000:
//                if (V)
//                    SetVRAM_Mirror(VRAM_HMIRROR);
//                else
//                    SetVRAM_Mirror(VRAM_VMIRROR);
//                break;
//            case 0xA001: Poke_Mmc3_A001(A, V); break;
//            case 0xC000: Poke_Mmc3_C000(A, V); break;
//            case 0xC001: Poke_Mmc3_C001(A, V); break;
//            case 0xE000: Poke_Mmc3_E000(A, V); break;
//            case 0xE001: Poke_Mmc3_E001(A, V); break;

//            default:;
//        }
//}


//void MapperFk23c::fk23c_UpdatePrg2p(unsigned int A, unsigned int V)
//{
//    if ((exRegs[0] & 0x7U) - 3 > 1 && (!(exRegs[3] & 0x2U) || A < 0x4000))
//    {
//        if (exRegs[0] & 0x3U)
//            V = (V & (0x3FU >> (exRegs[0] & 0x3U))) | (exRegs[1] << 1);

//        SetPROM_8K_Bank((A >> 13) + 4, V);
//    }
//}

//void MapperFk23c::fk23c_UpdateChr2p(unsigned int A, unsigned int V)
//{
//    if (VROM_1K_SIZE == 0) return;//ÔÝÊ±Ã»ÓÐºÃµÄ½â¾ö·½·¨

//    if (!(exRegs[0] & 0x40U) && (!(exRegs[3] & 0x2U) || (A != 0x400 && A != 0xC00)))
//        SetVROM_1K_Bank(A >> 10, (exRegs[2] & 0x7FU) << 3 | V);
//}


//void MapperFk23c::fk23c_UpdatePrg()
//{
//    if ((exRegs[0] & 0x7U) == 4)
//    {
//        SetPROM_32K_Bank(exRegs[1] >> 1);
//    }
//    else if ((exRegs[0] & 0x7U) == 3)
//    {
//        SetPROM_16K_Bank(4, exRegs[1]);
//        SetPROM_16K_Bank(6, exRegs[1]);
//    }
//    else
//    {
//        if (exRegs[3] & 0x2U)
//        {
//            SetPROM_8K_Bank(6, exRegs[4]);
//            SetPROM_8K_Bank(7, exRegs[5]);
//        }

//        Mmc3_UpdatePrg();
//    }
//}

//void MapperFk23c::fk23c_UpdateChr()
//{
//    if (exRegs[0] & 0x40U)
//    {
//        SetVROM_8K_Bank(exRegs[2] | unromChr);
//    }
//    else
//    {
//        if (exRegs[3] & 0x2U)
//        {
//            const unsigned int base = (exRegs[2] & 0x7FU) << 3;

//            SetVROM_1K_Bank(1, base | exRegs[6]);
//            SetVROM_1K_Bank(3, base | exRegs[7]);
//        }

//        Mmc3_UpdateChr();
//    }
//}

//void MapperFk23c::SaveState(LPBYTE p)
//{/*
//	for( INT i = 0; i < 8; i++ ) {
//		p[i]   = reg[i];
//		p[10+i]  = chr[i];
//	}

//	p[ 8] = prg[0];
//	p[ 9] = prg[1];	
//	p[18] = irq_enable;
//	p[19] = irq_counter;
//	p[20] = irq_latch;
//	p[21] = irq_request;
//	p[22] = prg[2];
//	p[23] = prg[3];	
//	*/
//}

//void MapperFk23c::LoadState(LPBYTE p)
//{
//    /*for( INT i = 0; i < 8; i++ ) {
//		reg[i] = p[i];
//		chr[i] = p[10+i];
//	}
//	prg[0]  = p[ 8];
//	prg[1]  = p[ 9];
//	irq_enable  = p[18];
//	irq_counter = p[19];
//	irq_latch   = p[20];
//	irq_request = p[21];
//	prg[2]  = p[ 22];
//	prg[3]  = p[ 23];*/
//}



//MapperFk23ca::MapperFk23ca(NES * parent) : MapperFk23c(parent)
//{
//}

//void MapperFk23ca::Reset()
//{
//    uint8 i;
//    for (i = 0; i < 8; ++i)
//        MapperFk23c::exRegs[i] = 0xFF;

//    for (i = 0; i < 4; ++i)
//        exRegs[i] = 0x00;

//    MapperFk23c::unromChr = 0x0;
//    MapperFk23c::mode = 0;

//    MMC3::Reset();
//    Mmc3_UpdatePrg();
//    Mmc3_UpdateChr();
//}

//void MapperFk23ca::fk23c_UpdatePrg()
//{
//    uint32 bank = (exRegs[1] & 0x1F);
//    uint32 block = (exRegs[1] & 0x60);
//    uint32 extra = (exRegs[3] & 2);

//    if ((exRegs[0] & 0x7U) == 4)
//    {
//        SetPROM_32K_Bank((bank + block) >> 1);
//    }
//    else if ((exRegs[0] & 0x7U) == 3)
//    {
//        SetPROM_16K_Bank(4, (bank + block));
//        SetPROM_16K_Bank(6, (bank + block));
//    }
//    else
//    {
//        if (extra)
//        {
//            SetPROM_8K_Bank(6, exRegs[4]);
//            SetPROM_8K_Bank(7, exRegs[5]);
//        }
//    }
//}