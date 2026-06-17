//MMC3.h


////////////////////////////////////////////////////////////////////////////
//// Nintendo MMC3                                                        //
////////////////////////////////////////////////////////////////////////////
//class MMC3 : public Mapper
//{
//public:
//	MMC3( NES* parent );
//	//virtual	~MMC3();

//	// For state save
//	BOOL	IsStateSave() { return TRUE; }
//	void	SaveState( LPBYTE p );
//	void	LoadState( LPBYTE p );


//	void	Reset();

//	void	Write( WORD addr, BYTE data );
//	void	HSync( INT scanline );

//	//////////////////////////////////////////////////////////////////////////
//	// MMC3
//	//////////////////////////////////////////////////////////////////////////	
//	unsigned int count;
//	unsigned int latch;
//	unsigned int reload;
//	unsigned int enabled;				

//	unsigned int ctrl0;
//	unsigned int ctrl1;

//	unsigned int chr[8];
//	unsigned int prg[4];	

//	//Memory Write 8000~FFFF
//	//void Mmc3_MemoryWrite(uint32 address, uint8 data);
//	void Poke_Mmc3_8000(uint32 address,uint8 data);
//	void Poke_Mmc3_8001(uint32 address,uint8 data);
//	void Poke_Mmc3_A000(uint32 address,uint8 data);
//	void Poke_Mmc3_A001(uint32 address,uint8 data);
//	void Poke_Mmc3_C000(uint32 address,uint8 data);
//	void Poke_Mmc3_C001(uint32 address,uint8 data);
//	void Poke_Mmc3_E000(uint32 address,uint8 data);
//	void Poke_Mmc3_E001(uint32 address,uint8 data);
//	void Poke_Nop(uint32,uint8);

//	//³õÊ¼»¯Mmc3º¯ÊýÖ¸Õë
//	//void Mmc3_Init();
//	//void Mmc3_Reset();
//	void Mmc3_UpdatePrg();
//	void Mmc3_UpdateChr();
//	void Mmc3_UpdatePrg2p(unsigned int addr,unsigned int bank);
//	void Mmc3_UpdateChr2p(unsigned int addr,unsigned int bank);
//	unsigned int Mmc3_GetChrSource(unsigned int);
//	void Mmc3_HSync(uint32 scanline);

//	void Mmc3_SwapChr1K(uint8 page,uint32 bank);
//	void Mmc3_SwapChr2K(uint8 page,uint32 bank);

//	void (MMC3::*UpdateChr)(unsigned int,unsigned int);
//	void (MMC3::*UpdatePrg)(unsigned int,unsigned int);
//	unsigned int (MMC3::*GetChrSource)(unsigned int);

//	void (MMC3::*Poke_8000)(uint32 address,uint8 data);
//	void (MMC3::*Poke_8001)(uint32 address,uint8 data);
//	void (MMC3::*Poke_A000)(uint32 address,uint8 data);
//	void (MMC3::*Poke_A001)(uint32 address,uint8 data);
//	void (MMC3::*Poke_C000)(uint32 address,uint8 data);
//	void (MMC3::*Poke_C001)(uint32 address,uint8 data);
//	void (MMC3::*Poke_E000)(uint32 address,uint8 data);
//	void (MMC3::*Poke_E001)(uint32 address,uint8 data);
//};//////////////////////////////////////////////////////////////////////////
//// Nintendo MMC3                                                        //
////////////////////////////////////////////////////////////////////////////
//class MMC3 : public Mapper
//{
//public:
//	MMC3( NES* parent );
//	//virtual	~MMC3();

//	// For state save
//	BOOL	IsStateSave() { return TRUE; }
//	void	SaveState( LPBYTE p );
//	void	LoadState( LPBYTE p );


//	void	Reset();

//	void	Write( WORD addr, BYTE data );
//	void	HSync( INT scanline );

//	//////////////////////////////////////////////////////////////////////////
//	// MMC3
//	//////////////////////////////////////////////////////////////////////////	
//	unsigned int count;
//	unsigned int latch;
//	unsigned int reload;
//	unsigned int enabled;				

//	unsigned int ctrl0;
//	unsigned int ctrl1;

//	unsigned int chr[8];
//	unsigned int prg[4];	

//	//Memory Write 8000~FFFF
//	//void Mmc3_MemoryWrite(uint32 address, uint8 data);
//	void Poke_Mmc3_8000(uint32 address,uint8 data);
//	void Poke_Mmc3_8001(uint32 address,uint8 data);
//	void Poke_Mmc3_A000(uint32 address,uint8 data);
//	void Poke_Mmc3_A001(uint32 address,uint8 data);
//	void Poke_Mmc3_C000(uint32 address,uint8 data);
//	void Poke_Mmc3_C001(uint32 address,uint8 data);
//	void Poke_Mmc3_E000(uint32 address,uint8 data);
//	void Poke_Mmc3_E001(uint32 address,uint8 data);
//	void Poke_Nop(uint32,uint8);

//	//³õÊ¼»¯Mmc3º¯ÊýÖ¸Õë
//	//void Mmc3_Init();
//	//void Mmc3_Reset();
//	void Mmc3_UpdatePrg();
//	void Mmc3_UpdateChr();
//	void Mmc3_UpdatePrg2p(unsigned int addr,unsigned int bank);
//	void Mmc3_UpdateChr2p(unsigned int addr,unsigned int bank);
//	unsigned int Mmc3_GetChrSource(unsigned int);
//	void Mmc3_HSync(uint32 scanline);

//	void Mmc3_SwapChr1K(uint8 page,uint32 bank);
//	void Mmc3_SwapChr2K(uint8 page,uint32 bank);

//	void (MMC3::*UpdateChr)(unsigned int,unsigned int);
//	void (MMC3::*UpdatePrg)(unsigned int,unsigned int);
//	unsigned int (MMC3::*GetChrSource)(unsigned int);

//	void (MMC3::*Poke_8000)(uint32 address,uint8 data);
//	void (MMC3::*Poke_8001)(uint32 address,uint8 data);
//	void (MMC3::*Poke_A000)(uint32 address,uint8 data);
//	void (MMC3::*Poke_A001)(uint32 address,uint8 data);
//	void (MMC3::*Poke_C000)(uint32 address,uint8 data);
//	void (MMC3::*Poke_C001)(uint32 address,uint8 data);
//	void (MMC3::*Poke_E000)(uint32 address,uint8 data);
//	void (MMC3::*Poke_E001)(uint32 address,uint8 data);
//};




//MMC3.cpp


////////////////////////////////////////////////////////////////////////////
//// Nintendo MMC3                                                        //
////////////////////////////////////////////////////////////////////////////
//using System.Net;
//using System;
//using Unity.Android.Gradle.Manifest;
//using VirtualNes.Core;

//MMC3::MMC3(NES * parent) : Mapper(parent)
//{
//    UpdateChr = &MMC3::Mmc3_UpdateChr2p;
//    UpdatePrg = &MMC3::Mmc3_UpdatePrg2p;
//    GetChrSource = &MMC3::Mmc3_GetChrSource;

//    Poke_8000 = &MMC3::Poke_Mmc3_8000;
//    Poke_8001 = &MMC3::Poke_Mmc3_8001;
//    Poke_A000 = &MMC3::Poke_Mmc3_A000;
//    Poke_A001 = &MMC3::Poke_Mmc3_A001;
//    Poke_C000 = &MMC3::Poke_Mmc3_C000;
//    Poke_C001 = &MMC3::Poke_Mmc3_C001;
//    Poke_E000 = &MMC3::Poke_Mmc3_E000;
//    Poke_E001 = &MMC3::Poke_Mmc3_E001;
//}

//void MMC3::Reset()
//{
//    int i;

//    ctrl0 = 0;
//    ctrl1 = 0;

//    for (i = 0; i < 8; ++i)
//        chr[i] = i;

//    prg[0] = 0x00;
//    prg[1] = 0x01;
//    prg[2] = 0x3E;
//    prg[3] = 0x3F;

//    count = 0;
//    latch = 0;
//    reload = 0;
//    enabled = 0;

//    Mmc3_UpdatePrg();
//    Mmc3_UpdateChr();
//}

//void MMC3::Write(WORD A, BYTE V)
//{
//    switch (A & 0xE001)
//    {
//        case 0x8000: (this->* Poke_8000)(A, V); break;
//        case 0x8001: (this->* Poke_8001)(A, V); break;
//        case 0xA000: (this->* Poke_A000)(A, V); break;
//        case 0xA001: (this->* Poke_A001)(A, V); break;
//        case 0xC000: (this->* Poke_C000)(A, V); break;
//        case 0xC001: (this->* Poke_C001)(A, V); break;
//        case 0xE000: (this->* Poke_E000)(A, V); break;
//        case 0xE001: (this->* Poke_E001)(A, V); break;
//    }
//}

//void MMC3::Poke_Mmc3_8000(uint32 address, uint8 data)
//{
//    const unsigned int diff = ctrl0 ^ data;
//    ctrl0 = data;

//    if (diff & 0x40)
//    {
//        const unsigned int v[2] =
//        {
//            prg[(data >> 5 & 0x2) ^ 0],
//            prg[(data >> 5 & 0x2) ^ 2]
//        };

//        (this->* UpdatePrg)(0x0000, v[0]);
//        (this->* UpdatePrg)(0x4000, v[1]);
//    }

//    if (diff & 0x80)
//        Mmc3_UpdateChr();
//}
//void MMC3::Poke_Mmc3_8001(uint32 address, uint8 data)
//{
//    unsigned int addr = ctrl0 & 0x7;

//    if (addr < 6)
//    {
//        unsigned int base = ctrl0 << 5 & 0x1000;

//        if (addr < 2)
//        {
//            addr <<= 1;
//            base |= addr << 10;
//            (this->* UpdateChr)(base | 0x0000, (chr[addr + 0] = data & 0xFE));
//            (this->* UpdateChr)(base | 0x0400, (chr[addr + 1] = data | 0x01));
//        }
//        else
//        {
//            (this->* UpdateChr)((base ^ 0x1000) | (addr - 2) << 10, (chr[addr + 2] = data));
//        }
//    }
//    else
//    {
//        (this->* UpdatePrg)((addr == 6) ? (ctrl0 << 8 & 0x4000) : 0x2000, (prg[addr - 6] = data & 0x3F));
//    }
//}
//void MMC3::Poke_Mmc3_A000(uint32 address, uint8 data)
//{
//    if (!nes->rom->Is4SCREEN())
//    {
//        if (data & 0x01)
//            SetVRAM_Mirror(VRAM_HMIRROR);
//        else
//            SetVRAM_Mirror(VRAM_VMIRROR);
//    }
//}

//void MMC3::Poke_Mmc3_A001(uint32 address, uint8 data)
//{
//    ctrl1 = data;
//}

////////////////////////////////////////////////////////////////////////////
//// MMC3 IRQ
////////////////////////////////////////////////////////////////////////////
//void MMC3::Poke_Mmc3_C000(uint32 address, uint8 data)
//{
//    count = data;
//    reload = 0;
//}
//void MMC3::Poke_Mmc3_C001(uint32 address, uint8 data)
//{
//    latch = data;
//    reload = 0;
//}
//void MMC3::Poke_Mmc3_E000(uint32 address, uint8 data)
//{
//enabled = 0;
//reload = 0;
//    nes->cpu->ClrIRQ(IRQ_MAPPER);
//}
//void MMC3::Poke_Mmc3_E001(uint32 address, uint8 data)
//{
//    enabled = 1;
//    reload = 0;
//}

//void MMC3::HSync(int scanline)
////void MMC3::Mmc3_HSync(uint32 scanline)
//{
//    if ((scanline >= 0 && scanline <= 239))
//    {
//        if (nes->ppu->IsDispON())
//        {
//            if (enabled && !reload)
//            {
//                if (scanline == 0)
//                {
//                    if (count)
//                    {
//                        count -= 1;
//                    }
//                }
//                if (!(count))
//                {
//                    reload = 0xFF;
//                    count = latch;
//                    nes->cpu->SetIRQ(IRQ_MAPPER);
//                }
//                count--;
//            }
//        }
//    }
//}

//void MMC3::Poke_Nop(uint32 addr, uint8 data)
//{
//    return;
//}



//void MMC3::Mmc3_SwapChr1K(uint8 page, uint32 bank)
//{
//    if ((this->* GetChrSource)(bank))
//    {
//        SetCRAM_1K_Bank(page, bank);
//    }
//    else
//    {
//        SetVROM_1K_Bank(page, bank);
//    }
//}

//void MMC3::Mmc3_SwapChr2K(uint8 page, uint32 bank)
//{
//    if ((this->* GetChrSource)(bank))
//    {
//        SetCRAM_2K_Bank(page, bank);
//    }
//    else
//    {
//        SetVROM_2K_Bank(page, bank);
//    }
//}

//void MMC3::Mmc3_UpdatePrg2p(unsigned int addr, unsigned int bank)
//{
//    SetPROM_8K_Bank((addr >> 13) + 4, bank);
//}

//void MMC3::Mmc3_UpdateChr2p(unsigned int addr, unsigned int bank)
//{
//    Mmc3_SwapChr1K(addr >> 10, bank);
//}


//void MMC3::Mmc3_UpdatePrg()
//{
//    const unsigned int x = ctrl0 >> 5 & 0x2;

//    (this->* UpdatePrg)(0x0000, prg[0 ^ x]);
//    (this->* UpdatePrg)(0x2000, prg[1 ^ 0]);
//    (this->* UpdatePrg)(0x4000, prg[2 ^ x]);
//    (this->* UpdatePrg)(0x6000, prg[3 ^ 0]);
//}

//void MMC3::Mmc3_UpdateChr()
//{
//    const unsigned int x = ctrl0 >> 5 & 0x4;
//    unsigned int i = 0;
//    for (i = 0; i < 8; ++i)
//        (this->* UpdateChr)(i * 0x400, chr[i ^ x]);
//}

//unsigned int MMC3::Mmc3_GetChrSource(unsigned int dummy){return 0;}


//void MMC3::SaveState(LPBYTE p)
//{
//    //Ã»Ð´ºÃ
//}
//void MMC3::LoadState(LPBYTE p)
//{
//    //Ã»Ð´ºÃ
//}