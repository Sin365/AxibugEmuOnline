using System.Collections.Generic;

namespace VirtualNes.Core
{
    public class PAD
    {
        private NES nes;
        private int excontroller_select;
        private EXPAD expad;
        private bool bStrobe;
        private bool bSwapButton;
        private bool bSwapPlayer;
        private bool bZapperMode;
        private VSType nVSSwapType;
        private byte[] padbit = new byte[4];
        private byte micbit;
        private byte[] padbitsync = new byte[4];
        private byte micbitsync;
        private bool bBarcodeWorld;
        private int[][] padcnt = new int[4][]
        {
           new int[2],new int[2],new int[2],new int[2],
        };

        public uint pad1bit, pad2bit, pad3bit, pad4bit;

        private static int[] ren10fps = new int[6] { 1, 1, 1, 0, 0, 0 };
        private static int[] ren15fps = new int[4] { 1, 1, 0, 0 };
        private static int[] ren20fps = new int[3] { 1, 1, 0 };
        private static int[] ren30fps = new int[2] { 1, 0 };
        private static int[] renmask = new int[4] { 6, 4, 3, 2 };
        public static Dictionary<int, int[]> rentbl = new Dictionary<int, int[]>()
        {
            {0,ren10fps },
            {1,ren15fps },
            {2,ren20fps },
            {3,ren30fps },
        };

        public PAD(NES parent)
        {
            nes = parent;
            excontroller_select = 0;
            expad = null;
            bStrobe = false;
            bSwapButton = false;
            bSwapPlayer = false;
            bZapperMode = false;
            nVSSwapType = VSType.VS_TYPE0;

            padbit[0] = padbit[1] = padbit[2] = padbit[3] = 0;
            micbit = 0;

            padbitsync[0] = padbitsync[1] = padbitsync[2] = padbitsync[3] = 0;
            micbitsync = 0;
        }

        internal byte Read(ushort addr)
        {
            byte data = 0x00;

            if (addr == 0x4016)
            {
                data = (byte)(pad1bit & 1);
                pad1bit >>= 1;
                data |= (byte)(((pad3bit & 1)) << 1);
                pad3bit >>= 1;
                // Mic
                if (!nes.rom.IsVSUNISYSTEM())
                {
                    data |= micbitsync;
                }
                if (expad != null)
                {
                    data |= expad.Read4016();
                }
            }
            if (addr == 0x4017)
            {
                data = (byte)(pad2bit & 1);
                pad2bit >>= 1;
                data |= (byte)((pad4bit & 1) << 1);
                pad4bit >>= 1;

                if (expad != null)
                {
                    data |= expad.Read4017();
                }

                if (bBarcodeWorld)
                {
                    data |= nes.Barcode2();
                }
            }

            return data;
        }
        public void Dispose() { }

        internal void Write(ushort addr, byte data)
        {
            if (addr == 0x4016)
            {
                if ((data & 0x01) != 0)
                {
                    bStrobe = true;
                }
                else if (bStrobe)
                {
                    bStrobe = false;

                    Strobe();
                    if (expad != null)
                    {
                        expad.Strobe();
                    }
                }

                if (expad != null)
                {
                    expad.Write4016(data);
                }
            }
            if (addr == 0x4017)
            {
                if (expad != null)
                {
                    expad.Write4017(data);
                }
            }
        }

        private void Strobe()
        {
            // For VS-Unisystem
            if (nes.rom.IsVSUNISYSTEM())
            {
                uint pad1 = (uint)(padbitsync[0] & 0xF3);
                uint pad2 = (uint)(padbitsync[1] & 0xF3);
                uint st1 = (uint)(padbitsync[0] & 0x08) >> 3;
                uint st2 = (uint)(padbitsync[1] & 0x08) >> 3;

                switch (nVSSwapType)
                {
                    case VSType.VS_TYPE0:
                        pad1bit = pad1 | (st1 << 2);
                        pad2bit = pad2 | (st2 << 2);
                        break;
                    case VSType.VS_TYPE1:
                        pad1bit = pad2 | (st1 << 2);
                        pad2bit = pad1 | (st2 << 2);
                        break;
                    case VSType.VS_TYPE2:
                        pad1bit = pad1 | (st1 << 2) | (st2 << 3);
                        pad2bit = pad2;
                        break;
                    case VSType.VS_TYPE3:
                        pad1bit = pad2 | (st1 << 2) | (st2 << 3);
                        pad2bit = pad1;
                        break;
                    case VSType.VS_TYPE4:
                        pad1bit = pad1 | (st1 << 2) | 0x08; // 0x08=Start Protect
                        pad2bit = pad2 | (st2 << 2) | 0x08; // 0x08=Start Protect
                        break;
                    case VSType.VS_TYPE5:
                        pad1bit = pad2 | (st1 << 2) | 0x08; // 0x08=Start Protect
                        pad2bit = pad1 | (st2 << 2) | 0x08; // 0x08=Start Protect
                        break;
                    case VSType.VS_TYPE6:
                        pad1bit = pad1 | (st1 << 2) | (((uint)padbitsync[0] & 0x04) << 1);
                        pad2bit = pad2 | (st2 << 2) | (((uint)padbitsync[1] & 0x04) << 1);
                        break;
                    case VSType.VS_TYPEZ:
                        pad1bit = 0;
                        pad2bit = 0;
                        break;
                }

                // Coin 2偲旐傞堊偵徚偡
                micbit = 0;
            }
            else
            {
                if (Supporter.Config.emulator.bFourPlayer)
                {
                    // NES type
                    pad1bit = padbitsync[0] | ((uint)padbitsync[2] << 8) | 0x00080000;
                    pad2bit = padbitsync[1] | ((uint)padbitsync[3] << 8) | 0x00040000;
                }
                else
                {
                    // Famicom type
                    pad1bit = padbitsync[0];
                    pad2bit = padbitsync[1];
                }
            }
            pad3bit = padbitsync[2];
            pad4bit = padbitsync[3];
        }

        internal void Reset()
        {
            pad1bit = pad2bit = 0;
            bStrobe = false;

            bBarcodeWorld = false;

            for (int x = 0; x < 4; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    padcnt[x][y] = 0;
                }
            }

            // Select Extension Devices
            uint crc = nes.rom.GetPROM_CRC();

            if (crc == 0xfbfc6a6c       // Adventures of Bayou Billy, The(E)
             || crc == 0xcb275051       // Adventures of Bayou Billy, The(U)
             || crc == 0xfb69c131       // Baby Boomer(Unl)(U)
             || crc == 0xf2641ad0       // Barker Bill's Trick Shooting(U)
             || crc == 0xbc1dce96       // Chiller (Unl)(U)
             || crc == 0x90ca616d       // Duck Hunt(JUE)
             || crc == 0x59e3343f       // Freedom Force(U)
             || crc == 0x242a270c       // Gotcha!(U)
             || crc == 0x7b5bd2de       // Gumshoe(UE)
             || crc == 0x255b129c       // Gun Sight(J)
             || crc == 0x8963ae6e       // Hogan's Alley(JU)
             || crc == 0x51d2112f       // Laser Invasion(U)
             || crc == 0x0a866c94       // Lone Ranger, The(U)
                                        //	 || crc == 0xe4c04eea		// Mad City(J)
             || crc == 0x9eef47aa       // Mechanized Attack(U)
             || crc == 0xc2db7551       // Shooting Range(U)
             || crc == 0x163e86c0       // To The Earth(U)
             || crc == 0x42d893e4       // Operation Wolf(J)
             || crc == 0x1388aeb9       // Operation Wolf(U)
             || crc == 0x0d3cf705       // Wild Gunman(J)
             || crc == 0x389960db)
            {   // Wild Gunman(JUE)
                SetExController(EXCONTROLLER.EXCONTROLLER_ZAPPER);
            }
            if (crc == 0x35893b67       // Arkanoid(J)
             || crc == 0x6267fbd1)
            {   // Arkanoid 2(J)
                SetExController(EXCONTROLLER.EXCONTROLLER_PADDLE);
            }
            if (crc == 0xff6621ce       // Hyper Olympic(J)
             || crc == 0xdb9418e8       // Hyper Olympic(Tonosama Ban)(J)
             || crc == 0xac98cd70)
            {   // Hyper Sports(J)
                SetExController(EXCONTROLLER.EXCONTROLLER_HYPERSHOT);
            }
            if (crc == 0xf9def527       // Family BASIC(Ver2.0)
             || crc == 0xde34526e       // Family BASIC(Ver2.1a)
             || crc == 0xf050b611       // Family BASIC(Ver3)
             || crc == 0x3aaeed3f       // Family BASIC(Ver3)(Alt)
             || crc == 0x868FCD89       // Family BASIC(Ver1.0)
             || crc == 0x2D6B7E5A       // PLAYBOX BASIC(J) (Prototype_v0.0)
             || crc == 0xDA03D908)
            {   // PLAYBOX BASIC (J)
                SetExController(EXCONTROLLER.EXCONTROLLER_KEYBOARD);
            }
            if (crc == 0x589b6b0d       // Supor Computer V3.0
             || crc == 0x8b265862       // Supor English
             || crc == 0x41401c6d       // Supor Computer V4.0
             || crc == 0x82F1Fb96       // Supor Computer(Russia) V1.0
             || crc == 0xd5d6eac4)
            {   // EDU(C) Computer
                SetExController(EXCONTROLLER.EXCONTROLLER_SUPOR_KEYBOARD);
                nes.SetVideoMode(true);
            }
            if (crc == 0xc68363f6       // Crazy Climber(J)
             || crc == 0x2989ead6       // Smash TV(U) [!]
             || crc == 0x0b8f8128)
            {   // Smash TV(E) [!]
                SetExController(EXCONTROLLER.EXCONTROLLER_CRAZYCLIMBER);
            }
            if (crc == 0x20d22251)
            {   // Top rider(J)
                SetExController(EXCONTROLLER.EXCONTROLLER_TOPRIDER);
            }
            if (crc == 0x0cd00488)
            {   // Space Shadow(J)
                SetExController(EXCONTROLLER.EXCONTROLLER_SPACESHADOWGUN);
            }

            if (crc == 0x8c8fa83b       // Family Trainer - Athletic World (J)
             || crc == 0x7e704a14       // Family Trainer - Jogging Race (J)
             || crc == 0x2330a5d3)
            {   // Family Trainer - Rairai Kyonshiizu (J)
                SetExController(EXCONTROLLER.EXCONTROLLER_FAMILYTRAINER_A);
            }
            if (crc == 0xf8da2506       // Family Trainer - Aerobics Studio (J)
             || crc == 0xca26a0f1       // Family Trainer - Dai Undoukai (J)
             || crc == 0x28068b8c       // Family Trainer - Fuuun Takeshi Jou 2 (J)
             || crc == 0x10bb8f9a       // Family Trainer - Manhattan Police (J)
             || crc == 0xad3df455       // Family Trainer - Meiro Dai Sakusen (J)
             || crc == 0x8a5b72c0       // Family Trainer - Running Stadium (J)
             || crc == 0x59794f2d)
            {   // Family Trainer - Totsugeki Fuuun Takeshi Jou (J)
                SetExController(EXCONTROLLER.EXCONTROLLER_FAMILYTRAINER_B);
            }
            if (crc == 0x9fae4d46       // Ide Yousuke Meijin no Jissen Mahjong (J)
             || crc == 0x7b44fb2a)
            {   // Ide Yousuke Meijin no Jissen Mahjong 2 (J)
                SetExController(EXCONTROLLER.EXCONTROLLER_MAHJANG);
            }
            if (crc == 0x786148b6)
            {   // Exciting Boxing (J)
                SetExController(EXCONTROLLER.EXCONTROLLER_EXCITINGBOXING);
            }
            if (crc == 0xc3c0811d       // Oeka Kids - Anpanman no Hiragana Daisuki (J)
             || crc == 0x9d048ea4)
            {   // Oeka Kids - Anpanman to Oekaki Shiyou!! (J)
                SetExController(EXCONTROLLER.EXCONTROLLER_OEKAKIDS_TABLET);
            }

            if (crc == 0x67898319)
            {   // Barcode World (J)
                bBarcodeWorld = true;
            }

            // VS-Unisystem
            if (nes.rom.IsVSUNISYSTEM())
            {
                if (crc == 0xff5135a3       // VS Hogan's Alley
                 || crc == 0xed588f00       // VS Duck Hunt
                 || crc == 0x17ae56be)
                {   // VS Freedom Force
                    SetExController(EXCONTROLLER.EXCONTROLLER_VSZAPPER);
                }
                else
                {
                    SetExController(EXCONTROLLER.EXCONTROLLER_VSUNISYSTEM);
                }
            }

            if (crc == 0x21b099f3)
            {   // Gyromite (JUE)
                SetExController(EXCONTROLLER.EXCONTROLLER_GYROMITE);
            }
        }

        internal void SetExController(EXCONTROLLER type)
        {
            excontroller_select = (int)type;

            expad?.Dispose();
            expad = null;

            bZapperMode = false;

            // ExPad Instance create
            switch (type)
            {
                case EXCONTROLLER.EXCONTROLLER_ZAPPER:
                    expad = new EXPAD_Zapper(nes);
                    bZapperMode = true;
                    break;
                case EXCONTROLLER.EXCONTROLLER_PADDLE:
                    expad = new EXPAD_Paddle(nes);
                    break;
                case EXCONTROLLER.EXCONTROLLER_HYPERSHOT:
                    expad = new EXPAD_HyperShot(nes);
                    break;
                case EXCONTROLLER.EXCONTROLLER_KEYBOARD:
                    expad = new EXPAD_Keyboard(nes);
                    break;
                case EXCONTROLLER.EXCONTROLLER_SUPOR_KEYBOARD:
                    expad = new EXPAD_Supor_Keyboard(nes);
                    break;
                case EXCONTROLLER.EXCONTROLLER_CRAZYCLIMBER:
                    expad = new EXPAD_CrazyClimber(nes);
                    break;
                case EXCONTROLLER.EXCONTROLLER_TOPRIDER:
                    expad = new EXPAD_Toprider(nes);
                    break;
                case EXCONTROLLER.EXCONTROLLER_SPACESHADOWGUN:
                    expad = new EXPAD_SpaceShadowGun(nes);
                    bZapperMode = true;
                    break;
                case EXCONTROLLER.EXCONTROLLER_FAMILYTRAINER_A:
                case EXCONTROLLER.EXCONTROLLER_FAMILYTRAINER_B:
                    expad = new EXPAD_FamlyTrainer(nes);
                    break;
                case EXCONTROLLER.EXCONTROLLER_EXCITINGBOXING:
                    expad = new EXPAD_ExcitingBoxing(nes);
                    break;
                case EXCONTROLLER.EXCONTROLLER_MAHJANG:
                    expad = new EXPAD_Mahjang(nes);
                    break;
                case EXCONTROLLER.EXCONTROLLER_OEKAKIDS_TABLET:
                    expad = new EXPAD_OekakidsTablet(nes);
                    break;
                case EXCONTROLLER.EXCONTROLLER_TURBOFILE:
                    expad = new EXPAD_TurboFile(nes);
                    break;
                case EXCONTROLLER.EXCONTROLLER_VSUNISYSTEM:
                    expad = new EXPAD_VSUnisystem(nes);
                    break;
                case EXCONTROLLER.EXCONTROLLER_VSZAPPER:
                    expad = new EXPAD_VSZapper(nes);
                    bZapperMode = true;
                    break;

                case EXCONTROLLER.EXCONTROLLER_GYROMITE:
                    expad = new EXPAD_Gyromite(nes);
                    break;
                default:
                    break;
            }

            if (expad != null)
            {
                expad.Reset();
            }
        }

        public void Sync(ControllerState state)
        {
            padbit[0] = SyncSub(0, state);
            padbit[1] = SyncSub(1, state);
            padbit[2] = SyncSub(2, state);
            padbit[3] = SyncSub(3, state);

            // Mic
            micbit = 0;
            if (state.HasButton(1, EnumButtonType.MIC)) micbit |= 4;

            // For Excontroller
            if (expad != null)
            {
                expad.Sync();
            }
        }

        private byte SyncSub(int no, ControllerState state)
        {
            ushort bit = 0;

            // Up
            if (state.HasButton(no, EnumButtonType.UP))
                bit |= 1 << 4;
            // Down
            if (state.HasButton(no, EnumButtonType.DOWN))
                bit |= 1 << 5;
            // Left
            if (state.HasButton(no, EnumButtonType.LEFT))
                bit |= 1 << 6;
            // Right
            if (state.HasButton(no, EnumButtonType.RIGHT))
                bit |= 1 << 7;

            // 同時入力を禁止する
            //	if( (bit&((1<<4)|(1<<5))) == ((1<<4)|(1<<5)) )
            //		bit &= ~((1<<4)|(1<<5));
            if ((bit & ((1 << 6) | (1 << 7))) == ((1 << 6) | (1 << 7)))
                bit = (byte)(bit & ~((1 << 6) | (1 << 7)));

            // A
            if (state.HasButton(no, EnumButtonType.A)) bit |= 1 << 0;
            // B
            if (state.HasButton(no, EnumButtonType.B)) bit |= 1 << 1;

            // Select
            if (state.HasButton(no, EnumButtonType.SELECT)) bit |= 1 << 2;
            // Start
            if (state.HasButton(no, EnumButtonType.START)) bit |= 1 << 3;


            return (byte)(bit & 0xFF);
        }

        internal bool IsZapperMode()
        {
            return bZapperMode;
        }

        internal void VSync()
        {
            padbitsync[0] = padbit[0];
            padbitsync[1] = padbit[1];
            padbitsync[2] = padbit[2];
            padbitsync[3] = padbit[3];
            micbitsync = micbit;
        }

        internal uint GetSyncData()
        {
            uint ret;
            ret = (uint)(padbit[0] | (padbit[1] << 8) | (padbit[2] << 16) | (padbit[3] << 24));
            ret |= (uint)(micbit << 8);
            return ret;
        }

        internal void SetSyncData(uint data)
        {
            micbit = (byte)((data & 0x00000400) >> 8);
            padbit[0] = (byte)data;
            padbit[1] = (byte)(data >> 8);
            padbit[2] = (byte)(data >> 16);
            padbit[3] = (byte)(data >> 24);
        }

        internal int GetExController()
        {
            return excontroller_select;
        }
    }

    public enum VSType
    {
        VS_TYPE0 = 0,   // SELECT1P=START1P/SELECT2P=START2P 1P/2P No reverse
        VS_TYPE1,   // SELECT1P=START1P/SELECT2P=START2P 1P/2P Reverse
        VS_TYPE2,   // SELECT1P=START1P/START1P =START2P 1P/2P No reverse
        VS_TYPE3,   // SELECT1P=START1P/START1P =START2P 1P/2P Reverse
        VS_TYPE4,   // SELECT1P=START1P/SELECT2P=START2P 1P/2P No reverse (Protection)
        VS_TYPE5,   // SELECT1P=START1P/SELECT2P=START2P 1P/2P Reverse    (Protection)
        VS_TYPE6,   // SELECT1P=START1P/SELECT2P=START2P 1P/2P Reverse	(For Golf)
        VS_TYPEZ,	// ZAPPER
    }

    public enum EXCONTROLLER
    {
        EXCONTROLLER_NONE = 0,
        EXCONTROLLER_PADDLE,
        EXCONTROLLER_HYPERSHOT,
        EXCONTROLLER_ZAPPER,
        EXCONTROLLER_KEYBOARD,
        EXCONTROLLER_CRAZYCLIMBER,
        EXCONTROLLER_TOPRIDER,
        EXCONTROLLER_SPACESHADOWGUN,

        EXCONTROLLER_FAMILYTRAINER_A,
        EXCONTROLLER_FAMILYTRAINER_B,
        EXCONTROLLER_EXCITINGBOXING,
        EXCONTROLLER_MAHJANG,
        EXCONTROLLER_OEKAKIDS_TABLET,
        EXCONTROLLER_TURBOFILE,

        EXCONTROLLER_VSUNISYSTEM,
        EXCONTROLLER_VSZAPPER,

        EXCONTROLLER_GYROMITE,
        EXCONTROLLER_STACKUP,

        EXCONTROLLER_SUPOR_KEYBOARD,
    }
}
