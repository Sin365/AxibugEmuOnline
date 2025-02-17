using Essgee.EventArguments;
using Essgee.Utilities;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using static Essgee.Emulation.Utilities;

namespace Essgee.Emulation.Video
{
    /* Sega 315-5124 (Mark III, SMS) and 315-5246 (SMS 2); differences see 'VDPDIFF' comments */
    public unsafe class SegaSMSVDP : TMS99xxA
    {
        /* VDPDIFF: switch for Mk3/SMS1 vs SMS2/GG behavior; configurable via SetRevision, maybe still split into separate classes instead? */
        protected VDPTypes vdpType = VDPTypes.Mk3SMS1;

        public const int NumActiveScanlinesLow = 192;
        public const int NumActiveScanlinesMed = 224;
        public const int NumActiveScanlinesHigh = 240;

        protected const int NumSpritesMode4 = 64;
        protected const int NumSpritesPerLineMode4 = 8;

        public const int PortVCounter = 0x40;       // 0x7E canonically, but mirrored across bus
        public const int PortHCounter = 0x41;       // 0x7F canonically, but mirrored across bus

        //[StateRequired]
        //protected byte[] cram;

        #region //指针化 cram
        static byte[] cram_src;
        static GCHandle cram_handle;
        public static byte* cram;
        public static int cramLength;
        public static bool cram_IsNull => cram == null;
        public static byte[] cram_set
        {
            set
            {
                cram_handle.ReleaseGCHandle();
                cram_src = value;
                cramLength = value.Length;
                cram_src.GetObjectPtr(ref cram_handle, ref cram);
            }
        }
        #endregion


        [StateRequired]
        protected int vCounter, hCounter;
        protected int nametableHeight, vCounterTableIndex;
        [StateRequired]
        protected int lineInterruptCounter;
        protected int screenHeight;

        bool isLineInterruptEnabled => IsBitSet(registers[0x00], 4);
        [StateRequired]
        bool isLineInterruptPending;

        bool isColumn0MaskEnabled => IsBitSet(registers[0x00], 5);
        bool isVScrollPartiallyDisabled => IsBitSet(registers[0x00], 7);                /* Columns 24-31, i.e. pixels 192-255 */
        bool isHScrollPartiallyDisabled => IsBitSet(registers[0x00], 6);                /* Rows 0-1, i.e. pixels 0-15 */

        bool isBitM4Set => IsBitSet(registers[0x00], 2);

        protected override bool isModeGraphics1 => !(isBitM1Set || isBitM2Set || isBitM3Set || isBitM4Set);
        protected override bool isModeText => (isBitM1Set && !(isBitM2Set || isBitM3Set || isBitM4Set));
        protected override bool isModeGraphics2 => (isBitM2Set && !(isBitM1Set || isBitM3Set || isBitM4Set));
        protected override bool isModeMulticolor => (isBitM3Set && !(isBitM1Set || isBitM2Set || isBitM4Set));

        protected bool isSMS240LineMode => (!isBitM1Set && isBitM2Set && isBitM3Set && isBitM4Set);
        protected bool isSMS224LineMode => (isBitM1Set && isBitM2Set && !isBitM3Set && isBitM4Set);

        bool isSpriteShiftLeft8 => IsBitSet(registers[0x00], 3);

        protected override ushort nametableBaseAddress
        {
            get
            {
                if (isBitM4Set)
                {
                    if (isSMS224LineMode || isSMS240LineMode)
                        return (ushort)(((registers[0x02] & 0x0C) << 10) | 0x700);
                    else
                        return (ushort)((registers[0x02] & 0x0E) << 10);
                }
                else
                    return (ushort)((registers[0x02] & 0x0F) << 10);
            }
        }
        protected override ushort spriteAttribTableBaseAddress => (ushort)((registers[0x05] & (isBitM4Set ? 0x7E : 0x7F)) << 7);
        protected override ushort spritePatternGenBaseAddress => (ushort)((registers[0x06] & (isBitM4Set ? 0x04 : 0x07)) << 11);

        /* http://www.smspower.org/Development/Palette */
        // TODO: verify these, SMSPower has some mistakes (RGB approx correct, palette value wrong)
        // (not that we'll really use this, aside from for F-16 Fighting Falcon, as SG1000 games should always be loaded into the SG1000 core...)
        readonly byte[] legacyColorMap = new byte[]
        {
            0x00,	/* Transparent */
			0x00,	/* Black */
			0x08,	/* Medium green */
			0x0C,	/* Light green */
			0x10,	/* Dark blue */
			0x30,	/* Light blue */
			0x01,	/* Dark red */
			0x3C,	/* Cyan */
			0x02,	/* Medium red */
			0x03,	/* Light red */
			0x05,	/* Dark yellow */
			0x0F,	/* Light yellow */
			0x04,	/* Dark green */
			0x33,	/* Magenta */
			0x15,	/* Gray */
			0x3F	/* White */
        };

        readonly byte[][] vCounterTables = new byte[][]
        {
			/* NTSC, 192 lines */
			new byte[]
            {
				/* Top blanking */
				0xD8, 0xD9, 0xDA, 0xDB, 0xDC, 0xDD, 0xDE, 0xDF, 0xE0, 0xE1, 0xE2, 0xE3, 0xE4,

				/* Top border */
				0xE5, 0xE6, 0xE7, 0xE8, 0xE9, 0xEA, 0xEB, 0xEC, 0xED, 0xEE, 0xEF, 0xF0, 0xF1, 0xF2, 0xF3, 0xF4,
                0xF5, 0xF6, 0xF7, 0xF8, 0xF9, 0xFA, 0xFB, 0xFC, 0xFD, 0xFE, 0xFF,

				/* Active display */
				0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F,
                0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F,
                0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28, 0x29, 0x2A, 0x2B, 0x2C, 0x2D, 0x2E, 0x2F,
                0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3A, 0x3B, 0x3C, 0x3D, 0x3E, 0x3F,
                0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E, 0x4F,
                0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59, 0x5A, 0x5B, 0x5C, 0x5D, 0x5E, 0x5F,
                0x60, 0x61, 0x62, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68, 0x69, 0x6A, 0x6B, 0x6C, 0x6D, 0x6E, 0x6F,
                0x70, 0x71, 0x72, 0x73, 0x74, 0x75, 0x76, 0x77, 0x78, 0x79, 0x7A, 0x7B, 0x7C, 0x7D, 0x7E, 0x7F,
                0x80, 0x81, 0x82, 0x83, 0x84, 0x85, 0x86, 0x87, 0x88, 0x89, 0x8A, 0x8B, 0x8C, 0x8D, 0x8E, 0x8F,
                0x90, 0x91, 0x92, 0x93, 0x94, 0x95, 0x96, 0x97, 0x98, 0x99, 0x9A, 0x9B, 0x9C, 0x9D, 0x9E, 0x9F,
                0xA0, 0xA1, 0xA2, 0xA3, 0xA4, 0xA5, 0xA6, 0xA7, 0xA8, 0xA9, 0xAA, 0xAB, 0xAC, 0xAD, 0xAE, 0xAF,
                0xB0, 0xB1, 0xB2, 0xB3, 0xB4, 0xB5, 0xB6, 0xB7, 0xB8, 0xB9, 0xBA, 0xBB, 0xBC, 0xBD, 0xBE, 0xBF,

				/* Bottom border */
				0xC0, 0xC1, 0xC2, 0xC3, 0xC4, 0xC5, 0xC6, 0xC7, 0xC8, 0xC9, 0xCA, 0xCB, 0xCC, 0xCD, 0xCE, 0xCF,
                0xD0, 0xD1, 0xD2, 0xD3, 0xD4, 0xD5, 0xD6, 0xD7,

				/* Bottom blanking */
				0xD8, 0xD9, 0xDA,

				/* Vertical blanking */
				0xD5, 0xD6, 0xD7
            },
			/* NTSC, 224 lines */
			new byte[]
            {
				/* Top blanking */
				0xE8, 0xE9, 0xEA, 0xEB, 0xEC, 0xED, 0xEE, 0xEF, 0xF0, 0xF1, 0xF2, 0xF3, 0xF4,

				/* Top border */
				0xF5, 0xF6, 0xF7, 0xF8, 0xF9, 0xFA, 0xFB, 0xFC, 0xFD, 0xFE, 0xFF,

				/* Active display */
				0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F,
                0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F,
                0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28, 0x29, 0x2A, 0x2B, 0x2C, 0x2D, 0x2E, 0x2F,
                0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3A, 0x3B, 0x3C, 0x3D, 0x3E, 0x3F,
                0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E, 0x4F,
                0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59, 0x5A, 0x5B, 0x5C, 0x5D, 0x5E, 0x5F,
                0x60, 0x61, 0x62, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68, 0x69, 0x6A, 0x6B, 0x6C, 0x6D, 0x6E, 0x6F,
                0x70, 0x71, 0x72, 0x73, 0x74, 0x75, 0x76, 0x77, 0x78, 0x79, 0x7A, 0x7B, 0x7C, 0x7D, 0x7E, 0x7F,
                0x80, 0x81, 0x82, 0x83, 0x84, 0x85, 0x86, 0x87, 0x88, 0x89, 0x8A, 0x8B, 0x8C, 0x8D, 0x8E, 0x8F,
                0x90, 0x91, 0x92, 0x93, 0x94, 0x95, 0x96, 0x97, 0x98, 0x99, 0x9A, 0x9B, 0x9C, 0x9D, 0x9E, 0x9F,
                0xA0, 0xA1, 0xA2, 0xA3, 0xA4, 0xA5, 0xA6, 0xA7, 0xA8, 0xA9, 0xAA, 0xAB, 0xAC, 0xAD, 0xAE, 0xAF,
                0xB0, 0xB1, 0xB2, 0xB3, 0xB4, 0xB5, 0xB6, 0xB7, 0xB8, 0xB9, 0xBA, 0xBB, 0xBC, 0xBD, 0xBE, 0xBF,
                0xC0, 0xC1, 0xC2, 0xC3, 0xC4, 0xC5, 0xC6, 0xC7, 0xC8, 0xC9, 0xCA, 0xCB, 0xCC, 0xCD, 0xCE, 0xCF,
                0xD0, 0xD1, 0xD2, 0xD3, 0xD4, 0xD5, 0xD6, 0xD7, 0xD8, 0xD9, 0xDA, 0xDB, 0xDC, 0xDD, 0xDE, 0xDF,

				/* Bottom border */
				0xE0, 0xE1, 0xE2, 0xE3, 0xE4, 0xE5, 0xE6, 0xE7,

				/* Bottom blanking */
				0xE8, 0xE9, 0xEA,

				/* Vertical blanking */
				0xE5, 0xE6, 0xE7,
            },
			/* NTSC, 240 lines (invalid) */
			new byte[]
            {
                0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F,
                0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F,
                0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28, 0x29, 0x2A, 0x2B, 0x2C, 0x2D, 0x2E, 0x2F,
                0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3A, 0x3B, 0x3C, 0x3D, 0x3E, 0x3F,
                0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E, 0x4F,
                0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59, 0x5A, 0x5B, 0x5C, 0x5D, 0x5E, 0x5F,
                0x60, 0x61, 0x62, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68, 0x69, 0x6A, 0x6B, 0x6C, 0x6D, 0x6E, 0x6F,
                0x70, 0x71, 0x72, 0x73, 0x74, 0x75, 0x76, 0x77, 0x78, 0x79, 0x7A, 0x7B, 0x7C, 0x7D, 0x7E, 0x7F,
                0x80, 0x81, 0x82, 0x83, 0x84, 0x85, 0x86, 0x87, 0x88, 0x89, 0x8A, 0x8B, 0x8C, 0x8D, 0x8E, 0x8F,
                0x90, 0x91, 0x92, 0x93, 0x94, 0x95, 0x96, 0x97, 0x98, 0x99, 0x9A, 0x9B, 0x9C, 0x9D, 0x9E, 0x9F,
                0xA0, 0xA1, 0xA2, 0xA3, 0xA4, 0xA5, 0xA6, 0xA7, 0xA8, 0xA9, 0xAA, 0xAB, 0xAC, 0xAD, 0xAE, 0xAF,
                0xB0, 0xB1, 0xB2, 0xB3, 0xB4, 0xB5, 0xB6, 0xB7, 0xB8, 0xB9, 0xBA, 0xBB, 0xBC, 0xBD, 0xBE, 0xBF,
                0xC0, 0xC1, 0xC2, 0xC3, 0xC4, 0xC5, 0xC6, 0xC7, 0xC8, 0xC9, 0xCA, 0xCB, 0xCC, 0xCD, 0xCE, 0xCF,
                0xD0, 0xD1, 0xD2, 0xD3, 0xD4, 0xD5, 0xD6, 0xD7, 0xD8, 0xD9, 0xDA, 0xDB, 0xDC, 0xDD, 0xDE, 0xDF,
                0xE0, 0xE1, 0xE2, 0xE3, 0xE4, 0xE5, 0xE6, 0xE7, 0xE8, 0xE9, 0xEA, 0xEB, 0xEC, 0xED, 0xEE, 0xEF,
                0xF0, 0xF1, 0xF2, 0xF3, 0xF4, 0xF5, 0xF6, 0xF7, 0xF8, 0xF9, 0xFA, 0xFB, 0xFC, 0xFD, 0xFE, 0xFF,
                0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06
            },
			/* PAL, 192 lines */
			new byte[]
            {
				/* Top blanking */
				0xBD, 0xBE, 0xBF, 0xC0, 0xC1, 0xC2, 0xC3, 0xC4, 0xC5, 0xC6, 0xC7, 0xC8, 0xC9,

				/* Top border */
				0xCA, 0xCB, 0xCC, 0xCD, 0xCE, 0xCF, 0xD0, 0xD1, 0xD2, 0xD3, 0xD4, 0xD5, 0xD6, 0xD7, 0xD8, 0xD9,
                0xDA, 0xDB, 0xDC, 0xDD, 0xDE, 0xDF, 0xE0, 0xE1, 0xE2, 0xE3, 0xE4, 0xE5, 0xE6, 0xE7, 0xE8, 0xE9,
                0xEA, 0xEB, 0xEC, 0xED, 0xEE, 0xEF, 0xF0, 0xF1, 0xF2, 0xF3, 0xF4, 0xF5, 0xF6, 0xF7, 0xF8, 0xF9,
                0xFA, 0xFB, 0xFC, 0xFD, 0xFE, 0xFF,

				/* Active display */
				0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F,
                0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F,
                0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28, 0x29, 0x2A, 0x2B, 0x2C, 0x2D, 0x2E, 0x2F,
                0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3A, 0x3B, 0x3C, 0x3D, 0x3E, 0x3F,
                0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E, 0x4F,
                0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59, 0x5A, 0x5B, 0x5C, 0x5D, 0x5E, 0x5F,
                0x60, 0x61, 0x62, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68, 0x69, 0x6A, 0x6B, 0x6C, 0x6D, 0x6E, 0x6F,
                0x70, 0x71, 0x72, 0x73, 0x74, 0x75, 0x76, 0x77, 0x78, 0x79, 0x7A, 0x7B, 0x7C, 0x7D, 0x7E, 0x7F,
                0x80, 0x81, 0x82, 0x83, 0x84, 0x85, 0x86, 0x87, 0x88, 0x89, 0x8A, 0x8B, 0x8C, 0x8D, 0x8E, 0x8F,
                0x90, 0x91, 0x92, 0x93, 0x94, 0x95, 0x96, 0x97, 0x98, 0x99, 0x9A, 0x9B, 0x9C, 0x9D, 0x9E, 0x9F,
                0xA0, 0xA1, 0xA2, 0xA3, 0xA4, 0xA5, 0xA6, 0xA7, 0xA8, 0xA9, 0xAA, 0xAB, 0xAC, 0xAD, 0xAE, 0xAF,
                0xB0, 0xB1, 0xB2, 0xB3, 0xB4, 0xB5, 0xB6, 0xB7, 0xB8, 0xB9, 0xBA, 0xBB, 0xBC, 0xBD, 0xBE, 0xBF,

				/* Bottom border */
				0xC0, 0xC1, 0xC2, 0xC3, 0xC4, 0xC5, 0xC6, 0xC7, 0xC8, 0xC9, 0xCA, 0xCB, 0xCC, 0xCD, 0xCE, 0xCF,
                0xD0, 0xD1, 0xD2, 0xD3, 0xD4, 0xD5, 0xD6, 0xD7, 0xD8, 0xD9, 0xDA, 0xDB, 0xDC, 0xDD, 0xDE, 0xDF,
                0xE0, 0xE1, 0xE2, 0xE3, 0xE4, 0xE5, 0xE6, 0xE7, 0xE8, 0xE9, 0xEA, 0xEB, 0xEC, 0xED, 0xEE, 0xEF,

				/* Bottom blanking */
				0xF0, 0xF1, 0xF2,

				/* Vertical blanking */
				0xBA, 0xBB, 0xBC
            },
			/* PAL, 224 lines */
			new byte[]
            {
				/* Top blanking */
				0xCD, 0xCE, 0xCF, 0xD0, 0xD1, 0xD2, 0xD3, 0xD4, 0xD5, 0xD6, 0xD7, 0xD8, 0xD9,

				/* Top border */
				0xDA, 0xDB, 0xDC, 0xDD, 0xDE, 0xDF, 0xE0, 0xE1, 0xE2, 0xE3, 0xE4, 0xE5, 0xE6, 0xE7, 0xE8, 0xE9,
                0xEA, 0xEB, 0xEC, 0xED, 0xEE, 0xEF, 0xF0, 0xF1, 0xF2, 0xF3, 0xF4, 0xF5, 0xF6, 0xF7, 0xF8, 0xF9,
                0xFA, 0xFB, 0xFC, 0xFD, 0xFE, 0xFF,

				/* Active display */
				0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F,
                0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F,
                0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28, 0x29, 0x2A, 0x2B, 0x2C, 0x2D, 0x2E, 0x2F,
                0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3A, 0x3B, 0x3C, 0x3D, 0x3E, 0x3F,
                0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E, 0x4F,
                0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59, 0x5A, 0x5B, 0x5C, 0x5D, 0x5E, 0x5F,
                0x60, 0x61, 0x62, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68, 0x69, 0x6A, 0x6B, 0x6C, 0x6D, 0x6E, 0x6F,
                0x70, 0x71, 0x72, 0x73, 0x74, 0x75, 0x76, 0x77, 0x78, 0x79, 0x7A, 0x7B, 0x7C, 0x7D, 0x7E, 0x7F,
                0x80, 0x81, 0x82, 0x83, 0x84, 0x85, 0x86, 0x87, 0x88, 0x89, 0x8A, 0x8B, 0x8C, 0x8D, 0x8E, 0x8F,
                0x90, 0x91, 0x92, 0x93, 0x94, 0x95, 0x96, 0x97, 0x98, 0x99, 0x9A, 0x9B, 0x9C, 0x9D, 0x9E, 0x9F,
                0xA0, 0xA1, 0xA2, 0xA3, 0xA4, 0xA5, 0xA6, 0xA7, 0xA8, 0xA9, 0xAA, 0xAB, 0xAC, 0xAD, 0xAE, 0xAF,
                0xB0, 0xB1, 0xB2, 0xB3, 0xB4, 0xB5, 0xB6, 0xB7, 0xB8, 0xB9, 0xBA, 0xBB, 0xBC, 0xBD, 0xBE, 0xBF,
                0xC0, 0xC1, 0xC2, 0xC3, 0xC4, 0xC5, 0xC6, 0xC7, 0xC8, 0xC9, 0xCA, 0xCB, 0xCC, 0xCD, 0xCE, 0xCF,
                0xD0, 0xD1, 0xD2, 0xD3, 0xD4, 0xD5, 0xD6, 0xD7, 0xD8, 0xD9, 0xDA, 0xDB, 0xDC, 0xDD, 0xDE, 0xDF,

				/* Bottom border */
				0xE0, 0xE1, 0xE2, 0xE3, 0xE4, 0xE5, 0xE6, 0xE7, 0xE8, 0xE9, 0xEA, 0xEB, 0xEC, 0xED, 0xEE, 0xEF,
                0xF0, 0xF1, 0xF2, 0xF3, 0xF4, 0xF5, 0xF6, 0xF7, 0xF8, 0xF9, 0xFA, 0xFB, 0xFC, 0xFD, 0xFE, 0xFF,

				/* Bottom blanking */
				0x00, 0x01, 0x02,

				/* Vertical blanking */
				0xCA, 0xCB, 0xCC,
            },
			/* PAL, 240 lines */
			new byte[]
            {
				/* Top blanking */
				0xD5, 0xD6, 0xD7, 0xD8, 0xD9, 0xDA, 0xDB, 0xDC, 0xDD, 0xDE, 0xDF, 0xE0, 0xE1,

				/* Top border */
				0xE2, 0xE3, 0xE4, 0xE5, 0xE6, 0xE7, 0xE8, 0xE9, 0xEA, 0xEB, 0xEC, 0xED, 0xEE, 0xEF, 0xF0, 0xF1,
                0xF2, 0xF3, 0xF4, 0xF5, 0xF6, 0xF7, 0xF8, 0xF9, 0xFA, 0xFB, 0xFC, 0xFD, 0xFE, 0xFF,

				/* Active display */
				0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F,
                0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F,
                0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28, 0x29, 0x2A, 0x2B, 0x2C, 0x2D, 0x2E, 0x2F,
                0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3A, 0x3B, 0x3C, 0x3D, 0x3E, 0x3F,
                0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E, 0x4F,
                0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59, 0x5A, 0x5B, 0x5C, 0x5D, 0x5E, 0x5F,
                0x60, 0x61, 0x62, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68, 0x69, 0x6A, 0x6B, 0x6C, 0x6D, 0x6E, 0x6F,
                0x70, 0x71, 0x72, 0x73, 0x74, 0x75, 0x76, 0x77, 0x78, 0x79, 0x7A, 0x7B, 0x7C, 0x7D, 0x7E, 0x7F,
                0x80, 0x81, 0x82, 0x83, 0x84, 0x85, 0x86, 0x87, 0x88, 0x89, 0x8A, 0x8B, 0x8C, 0x8D, 0x8E, 0x8F,
                0x90, 0x91, 0x92, 0x93, 0x94, 0x95, 0x96, 0x97, 0x98, 0x99, 0x9A, 0x9B, 0x9C, 0x9D, 0x9E, 0x9F,
                0xA0, 0xA1, 0xA2, 0xA3, 0xA4, 0xA5, 0xA6, 0xA7, 0xA8, 0xA9, 0xAA, 0xAB, 0xAC, 0xAD, 0xAE, 0xAF,
                0xB0, 0xB1, 0xB2, 0xB3, 0xB4, 0xB5, 0xB6, 0xB7, 0xB8, 0xB9, 0xBA, 0xBB, 0xBC, 0xBD, 0xBE, 0xBF,
                0xC0, 0xC1, 0xC2, 0xC3, 0xC4, 0xC5, 0xC6, 0xC7, 0xC8, 0xC9, 0xCA, 0xCB, 0xCC, 0xCD, 0xCE, 0xCF,
                0xD0, 0xD1, 0xD2, 0xD3, 0xD4, 0xD5, 0xD6, 0xD7, 0xD8, 0xD9, 0xDA, 0xDB, 0xDC, 0xDD, 0xDE, 0xDF,
                0xE0, 0xE1, 0xE2, 0xE3, 0xE4, 0xE5, 0xE6, 0xE7, 0xE8, 0xE9, 0xEA, 0xEB, 0xEC, 0xED, 0xEE, 0xEF,

				/* Bottom border */
				0xF0, 0xF1, 0xF2, 0xF3, 0xF4, 0xF5, 0xF6, 0xF7, 0xF8, 0xF9, 0xFA, 0xFB, 0xFC, 0xFD, 0xFE, 0xFF,
                0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07,

				/* Bottom blanking */
				0x08, 0x09, 0x0A,

				/* Vertical blanking */
				0xD2, 0xD3, 0xD4,
            }
        };

        /* For H-counter emulation */
        readonly byte[] hCounterTable = new byte[]
        {
            0x00, 0x01, 0x02, 0x02, 0x03, 0x04, 0x05, 0x05, 0x06, 0x07, 0x08, 0x08, 0x09, 0x0A, 0x0B, 0x0B,
            0x0C, 0x0D, 0x0E, 0x0E, 0x0F, 0x10, 0x11, 0x11, 0x12, 0x13, 0x14, 0x14, 0x15, 0x16, 0x17, 0x17,
            0x18, 0x19, 0x1A, 0x1A, 0x1B, 0x1C, 0x1D, 0x1D, 0x1E, 0x1F, 0x20, 0x20, 0x21, 0x22, 0x23, 0x23,
            0x24, 0x25, 0x26, 0x26, 0x27, 0x28, 0x29, 0x29, 0x2A, 0x2B, 0x2C, 0x2C, 0x2D, 0x2E, 0x2F, 0x2F,
            0x30, 0x31, 0x32, 0x32, 0x33, 0x34, 0x35, 0x35, 0x36, 0x37, 0x38, 0x38, 0x39, 0x3A, 0x3B, 0x3B,
            0x3C, 0x3D, 0x3E, 0x3E, 0x3F, 0x40, 0x41, 0x41, 0x42, 0x43, 0x44, 0x44, 0x45, 0x46, 0x47, 0x47,
            0x48, 0x49, 0x4A, 0x4A, 0x4B, 0x4C, 0x4D, 0x4D, 0x4E, 0x4F, 0x50, 0x50, 0x51, 0x52, 0x53, 0x53,
            0x54, 0x55, 0x56, 0x56, 0x57, 0x58, 0x59, 0x59, 0x5A, 0x5B, 0x5C, 0x5C, 0x5D, 0x5E, 0x5F, 0x5F,
            0x60, 0x61, 0x62, 0x62, 0x63, 0x64, 0x65, 0x65, 0x66, 0x67, 0x68, 0x68, 0x69, 0x6A, 0x6B, 0x6B,
            0x6C, 0x6D, 0x6E, 0x6E, 0x6F, 0x70, 0x71, 0x71, 0x72, 0x73, 0x74, 0x74, 0x75, 0x76, 0x77, 0x77,
            0x78, 0x79, 0x7A, 0x7A, 0x7B, 0x7C, 0x7D, 0x7D, 0x7E, 0x7F, 0x80, 0x80, 0x81, 0x82, 0x83, 0x83,
            0x84, 0x85, 0x86, 0x86, 0x87, 0x88, 0x89, 0x89, 0x8A, 0x8B, 0x8C, 0x8C, 0x8D, 0x8E, 0x8F, 0x8F,
            0x90, 0x91, 0x92, 0x92, 0x93,

            0xE9, 0xEA, 0xEA, 0xEB, 0xEC, 0xED, 0xED, 0xEE, 0xEF, 0xF0, 0xF0, 0xF1, 0xF2, 0xF3, 0xF3, 0xF4,
            0xF5, 0xF6, 0xF6, 0xF7, 0xF8, 0xF9, 0xF9, 0xFA, 0xFB, 0xFC, 0xFC, 0xFD, 0xFE, 0xFF, 0xFF
        };

        [StateRequired]
        byte horizontalScrollLatched, verticalScrollLatched;

        const byte screenUsageBgLowPriority = screenUsageBackground;
        const byte screenUsageBgHighPriority = (1 << 2);

        public int ScreenHeight => screenHeight;
        public int CurrentScanline => currentScanline;

        public SegaSMSVDP() : base()
        {
            //registers = new byte[0x0B];
            //cram = new byte[0x20];
            registers_set = new byte[0x0B];
            cram_set = new byte[0x20];

            spriteBuffer = new (int Number, int Y, int X, int Pattern, int Attribute)[NumActiveScanlinesHigh][];
            for (int i = 0; i < spriteBuffer.Length; i++) spriteBuffer[i] = new (int Number, int Y, int X, int Pattern, int Attribute)[NumSpritesPerLineMode4];
        }

        #region AxiState

        public void LoadAxiStatus(AxiEssgssStatusData data)
        {
            base.LoadAxiStatus(data);
            cram_set = data.MemberData[nameof(cram)];
            vCounter = BitConverter.ToInt32(data.MemberData[nameof(vCounter)]);
            hCounter = BitConverter.ToInt32(data.MemberData[nameof(hCounter)]);
            lineInterruptCounter = BitConverter.ToInt32(data.MemberData[nameof(lineInterruptCounter)]);
            isLineInterruptPending = BitConverter.ToBoolean(data.MemberData[nameof(isLineInterruptPending)]);
            horizontalScrollLatched = data.MemberData[nameof(horizontalScrollLatched)].First();
            verticalScrollLatched = data.MemberData[nameof(verticalScrollLatched)].First();
        }

        public AxiEssgssStatusData SaveAxiStatus()
        {
            AxiEssgssStatusData data = base.SaveAxiStatus();

            data.MemberData[nameof(cram)] = cram_src;
            data.MemberData[nameof(vCounter)] = BitConverter.GetBytes(vCounter);
            data.MemberData[nameof(hCounter)] = BitConverter.GetBytes(hCounter);

            data.MemberData[nameof(lineInterruptCounter)] = BitConverter.GetBytes(lineInterruptCounter);

            data.MemberData[nameof(isLineInterruptPending)] = BitConverter.GetBytes(isLineInterruptPending);

            data.MemberData[nameof(horizontalScrollLatched)] = BitConverter.GetBytes(horizontalScrollLatched);
            data.MemberData[nameof(verticalScrollLatched)] = BitConverter.GetBytes(verticalScrollLatched);

            return data;
        }
        #endregion

        public override void Reset()
        {
            base.Reset();

            WriteRegister(0x00, 0x36);
            WriteRegister(0x01, 0x80);
            WriteRegister(0x02, 0xFF);
            WriteRegister(0x03, 0xFF);
            WriteRegister(0x04, 0xFF);
            WriteRegister(0x05, 0xFF);
            WriteRegister(0x06, 0xFB);
            WriteRegister(0x07, 0x00);
            WriteRegister(0x08, 0x00);
            WriteRegister(0x09, 0x00);
            WriteRegister(0x0A, 0xFF);

            //for (int i = 0; i < cram.Length; i++) cram[i] = 0;
            for (int i = 0; i < cramLength; i++) cram[i] = 0;

            vCounter = hCounter = 0;
            lineInterruptCounter = registers[0x0A];

            isLineInterruptPending = false;

            horizontalScrollLatched = verticalScrollLatched = 0;

            UpdateResolution();
        }

        public override void SetRevision(int rev)
        {
            VDPTypes type = (VDPTypes)rev;
            Debug.Assert(Enum.IsDefined(typeof(VDPTypes), type), "Invalid revision", "{0} revision is invalid; only rev 0 (MK3/SMS1) or 1 (SMS2/GG) is valid", GetType().FullName);
            vdpType = type;
        }

        protected override void ReconfigureTimings()
        {
            /* Calculate cycles/line */
            clockCyclesPerLine = (int)Math.Round((clockRate / refreshRate) / numTotalScanlines);

            /* Create arrays */
            screenUsage = new byte[numVisiblePixels * numVisibleScanlines];
            //outputFramebuffer = new byte[(numVisiblePixels * numVisibleScanlines) * 4];
            outputFramebuffer_set = new byte[(numVisiblePixels * numVisibleScanlines) * 4];

            /* Update resolution/display timing */
            UpdateResolution();
        }

        public override void Step(int clockCyclesInStep)
        {
            InterruptLine = (((isFrameInterruptEnabled && isFrameInterruptPending) || (isLineInterruptEnabled && isLineInterruptPending)) ? InterruptState.Assert : InterruptState.Clear);

            cycleCount += clockCyclesInStep;

            hCounter = hCounterTable[(int)Math.Round((cycleCount + 578) / 3.0) % hCounterTable.Length];

            if (cycleCount >= clockCyclesPerLine)
            {
                OnEndOfScanline(EventArgs.Empty);

                horizontalScrollLatched = registers[0x08];

                if (currentScanline == scanlineActiveDisplay)
                    verticalScrollLatched = registers[0x09];

                CheckSpriteOverflow(currentScanline);

                RenderLine(currentScanline);

                if (currentScanline >= scanlineActiveDisplay && currentScanline <= scanlineBottomBorder)
                {
                    lineInterruptCounter--;
                    if (lineInterruptCounter < 0)
                    {
                        lineInterruptCounter = registers[0x0A];
                        isLineInterruptPending = true;
                    }
                }
                else
                    lineInterruptCounter = registers[0x0A];

                if (currentScanline == (scanlineBottomBorder + 1))
                    isFrameInterruptPending = true;

                vCounter = vCounterTables[vCounterTableIndex][currentScanline];

                currentScanline++;
                if (currentScanline == numTotalScanlines)
                {
                    currentScanline = 0;
                    ClearScreenUsage();

                    PrepareRenderScreen();
                }

                ParseSpriteTable(currentScanline);

                cycleCount -= clockCyclesPerLine;
                if (cycleCount <= -clockCyclesPerLine) cycleCount = 0;
            }
        }

        //GCHandle? lasyRenderHandle;
        protected override void PrepareRenderScreen()
        {
            // 固定数组，防止垃圾回收器移动它  
            //var bitmapcolorRect_handle = GCHandle.Alloc(outputFramebuffer.Clone() as byte[], GCHandleType.Pinned);
            //var bitmapcolorRect_handle = GCHandle.Alloc(outputFramebuffer, GCHandleType.Pinned);
            //// 获取数组的指针  
            //IntPtr mFrameDataPtr = bitmapcolorRect_handle.AddrOfPinnedObject();

            var eventArgs = RenderScreenEventArgs.Create(numVisiblePixels, numVisibleScanlines, outputFramebuffer_Ptr);
            OnRenderScreen(eventArgs);
            eventArgs.Release();
            //if (lasyRenderHandle != null)
            //    lasyRenderHandle.Value.Free();
            //lasyRenderHandle = bitmapcolorRect_handle;

            //OnRenderScreen(new RenderScreenEventArgs(numVisiblePixels, numVisibleScanlines, outputFramebuffer.Clone() as byte[]));
        }

        protected override byte ReadVram(ushort address)
        {
            return vram[address & vramMask16k];
        }

        protected override void WriteVram(ushort address, byte value)
        {
            vram[address & vramMask16k] = value;
        }

        protected override void RenderLine(int y)
        {
            if (y >= scanlineTopBorder && y < scanlineActiveDisplay)
            {
                if (layerBordersForceEnable) SetLine(y, 1, backgroundColor);
                else SetLine(y, 0x00, 0x00, 0x00);
            }
            else if (y >= scanlineActiveDisplay && y < scanlineBottomBorder)
            {
                if (layerBackgroundForceEnable)
                {
                    if (isBitM4Set)
                        RenderLineMode4Background(y);
                    else if (isModeGraphics1)
                        RenderLineGraphics1Background(y);
                    else if (isModeGraphics2)
                        RenderLineGraphics2Background(y);
                    else if (isModeMulticolor)
                        RenderLineMulticolorBackground(y);
                    else if (isModeText)
                        RenderLineTextBackground(y);
                }
                else
                    SetLine(y, 0x00, 0x00, 0x00);

                if (layerSpritesForceEnable)
                {
                    if (isBitM4Set)
                        RenderLineMode4Sprites(y);
                    else if (!isModeText && !isBitM4Set)
                        RenderLineSprites(y);
                }

                RenderBorders(y);
            }
            else if (y >= scanlineBottomBorder && y < numVisibleScanlines)
            {
                if (layerBordersForceEnable) SetLine(y, 1, backgroundColor);
                else SetLine(y, 0x00, 0x00, 0x00);
            }
        }

        protected override void RenderBorders(int y)
        {
            for (int x = pixelLeftBorder; x < pixelActiveDisplay; x++)
            {
                if (layerBordersForceEnable) SetPixel(y, x, 1, backgroundColor);
                else SetPixel(y, x, 0x00, 0x00, 0x00);
            }
            for (int x = pixelRightBorder; x < numVisiblePixels; x++)
            {
                if (layerBordersForceEnable) SetPixel(y, x, 1, backgroundColor);
                else SetPixel(y, x, 0x00, 0x00, 0x00);
            }
        }

        protected void SetLine(int y, int palette, int color)
        {
            for (int x = 0; x < numVisiblePixels; x++)
                SetPixel(y, x, palette, color);
        }

        protected virtual void SetPixel(int y, int x, int palette, int color)
        {
            WriteColorToFramebuffer(palette, color, ((y * numVisiblePixels) + (x % numVisiblePixels)) * 4);
        }

        private void RenderLineMode4Background(int y)
        {
            /* Determine coordinates in active display */
            int activeDisplayY = (y - scanlineActiveDisplay);

            /* Determine H scrolling parameters */
            int currentHorizontalScroll = ((isHScrollPartiallyDisabled && activeDisplayY < 16) ? 0 : horizontalScrollLatched);
            int horizontalScrollCoarse = (currentHorizontalScroll >> 3);
            int horizontalScrollFine = (currentHorizontalScroll & 0x07);

            ushort currentNametableBaseAddress = nametableBaseAddress;
            bool currentIsVScrollPartiallyDisabled = isVScrollPartiallyDisabled;
            bool currentIsColumn0MaskEnabled = isColumn0MaskEnabled;

            for (int x = 0; x < numTotalPixelsPerScanline; x++)
            {
                int activeDisplayX = (x - pixelActiveDisplay);
                if (activeDisplayX < 0 || activeDisplayX >= NumActivePixelsPerScanline) continue;

                /* Determine V scrolling parameters */
                int currentVerticalScroll = ((currentIsVScrollPartiallyDisabled && activeDisplayX >= 192) ? 0 : verticalScrollLatched);
                int verticalScrollCoarse = (currentVerticalScroll >> 3);
                int verticalScrollFine = (currentVerticalScroll & 0x07);

                /* Calculate current scrolled column and row */
                int numColumns = 32;
                int currentColumn = (((activeDisplayX - horizontalScrollFine) / 8) - horizontalScrollCoarse) & (numColumns - 1);
                int currentRow = (((activeDisplayY + verticalScrollFine) / 8) + verticalScrollCoarse) % (nametableHeight / 8);

                /* VDPDIFF: Mk3/SMS1 VDP only, adjust current row according to mask bit; http://www.smspower.org/Development/TilemapMirroring
				 * NOTE: Emulating this breaks 224/240-line mode games (ex. Fantastic Dizzy, Cosmic Spacehead, Micro Machines)?
				 */
                if (vdpType == VDPTypes.Mk3SMS1)
                    currentRow &= (((registers[0x02] & 0x01) << 4) | 0x0F);

                /* Fetch data from nametable & extract properties */
                ushort nametableAddress = (ushort)(currentNametableBaseAddress + (currentRow * (numColumns * 2)) + (currentColumn * 2));
                ushort nametableData = (ushort)(ReadVram((ushort)(nametableAddress + 1)) << 8 | ReadVram(nametableAddress));

                int tileIndex = (nametableData & 0x01FF);
                bool hFlip = ((nametableData & 0x0200) == 0x0200);
                bool vFlip = ((nametableData & 0x0400) == 0x0400);
                int palette = (((nametableData & 0x0800) >> 11) & 0x0001);
                bool priority = ((nametableData & 0x1000) == 0x1000);

                /* Fetch pixel data for current pixel line */
                int hPixel = ((activeDisplayX - horizontalScrollFine) % 8);
                hPixel = (hFlip ? hPixel : (7 - hPixel));
                int vPixel = ((activeDisplayY + verticalScrollFine) % 8);
                vPixel = (!vFlip ? vPixel : (7 - vPixel));

                ushort tileAddress = (ushort)((tileIndex << 5) + (vPixel << 2));
                int c = (((ReadVram((ushort)(tileAddress + 0)) >> hPixel) & 0x01) << 0);
                c |= (((ReadVram((ushort)(tileAddress + 1)) >> hPixel) & 0x01) << 1);
                c |= (((ReadVram((ushort)(tileAddress + 2)) >> hPixel) & 0x01) << 2);
                c |= (((ReadVram((ushort)(tileAddress + 3)) >> hPixel) & 0x01) << 3);

                /* Record screen usage, write to framebuffer */
                if (GetScreenUsageFlag(y, x) == screenUsageEmpty)
                {
                    if ((currentIsColumn0MaskEnabled && (activeDisplayX / 8) == 0) || isDisplayBlanked)
                        SetPixel(y, x, 1, backgroundColor);
                    else
                        SetPixel(y, x, palette, c);

                    SetScreenUsageFlag(y, x, (c != 0 && priority) ? screenUsageBgHighPriority : screenUsageBgLowPriority);
                }
            }
        }

        protected override void CheckSpriteOverflow(int y)
        {
            if (!isBitM4Set)
            {
                /* Not in Master System video mode */
                base.CheckSpriteOverflow(y);
            }
            else
            {
                /* Ensure current scanline is within active display */
                if (y >= scanlineActiveDisplay && y < scanlineBottomBorder)
                {
                    int activeDisplayY = (y - scanlineActiveDisplay);

                    /* If last sprite in buffer is valid, sprite overflow occured */
                    int lastSpriteInBuffer = spriteBuffer[activeDisplayY][NumSpritesPerLineMode4 - 1].Number;
                    if (lastSpriteInBuffer != -1)
                    {
                        isSpriteOverflow = true;

                        /* Store sprite number in status register */
                        /* NOTE: the last illegal sprite is *technically* only stored here in TMS99xxA modes, but still emulating it in Mode 4 should be fine */
                        WriteSpriteNumberToStatus(lastSpriteInBuffer);
                    }
                }
            }
        }

        protected override void ParseSpriteTable(int y)
        {
            if (!isBitM4Set)
            {
                /* Not in Master System video mode */
                base.ParseSpriteTable(y);
            }
            else
            {
                if (y < scanlineActiveDisplay || y >= scanlineBottomBorder) return;

                /* Determine coordinates in active display */
                int activeDisplayY = (y - scanlineActiveDisplay);

                /* Clear sprite list for current line */
                for (int i = 0; i < spriteBuffer[activeDisplayY].Length; i++) spriteBuffer[activeDisplayY][i] = (-1, 0, 0, 0, 0);

                /* Determine sprite size & get zoomed sprites adjustment */
                int zoomShift = (isZoomedSprites ? 1 : 0);
                int spriteHeight = ((isLargeSprites ? 16 : 8) << zoomShift);

                int numValidSprites = 0;
                for (int sprite = 0; sprite < NumSpritesMode4; sprite++)
                {
                    int yCoordinate = ReadVram((ushort)(spriteAttribTableBaseAddress + sprite));

                    /* Ignore following if Y coord is 208 in 192-line mode */
                    if (yCoordinate == 208 && screenHeight == NumActiveScanlinesLow)
                    {
                        /* Store first "illegal sprite" number in status register */
                        WriteSpriteNumberToStatus(sprite);
                        return;
                    }

                    /* Modify Y coord as needed */
                    yCoordinate++;
                    if (yCoordinate > screenHeight + 32) yCoordinate -= 256;

                    /* Ignore this sprite if on incorrect lines */
                    if (activeDisplayY < yCoordinate || activeDisplayY >= (yCoordinate + spriteHeight)) continue;

                    /* Check if maximum number of sprites per line is reached */
                    numValidSprites++;
                    if (numValidSprites > NumSpritesPerLineMode4) return;

                    /* Mark sprite for rendering */
                    int xCoordinate = ReadVram((ushort)(spriteAttribTableBaseAddress + 0x80 + (sprite * 2)));
                    int patternNumber = ReadVram((ushort)(spriteAttribTableBaseAddress + 0x80 + (sprite * 2) + 1));
                    int unusedData = ReadVram((ushort)(spriteAttribTableBaseAddress + 0x40 + (sprite * 2)));

                    spriteBuffer[activeDisplayY][numValidSprites - 1] = (sprite, yCoordinate, xCoordinate, patternNumber, unusedData);
                }

                /* Because we didn't bow out before already, store total number of sprites in status register */
                WriteSpriteNumberToStatus(NumSprites - 1);
            }
        }

        private void RenderLineMode4Sprites(int y)
        {
            if (y < scanlineActiveDisplay || y >= scanlineBottomBorder) return;

            /* Determine coordinates in active display */
            int activeDisplayY = (y - scanlineActiveDisplay);

            /* Determine sprite size & get zoomed sprites adjustment */
            int zoomShift = (isZoomedSprites ? 1 : 0);
            int spriteHeight = ((isLargeSprites ? 16 : 8) << zoomShift);

            for (int s = 0; s < spriteBuffer[activeDisplayY].Length; s++)
            {
                var sprite = spriteBuffer[activeDisplayY][s];

                if (sprite.Number == -1) continue;

                /* VDPDIFF: Mk3/SMS1 VDP zoomed sprites bug, only first four sprites (same as max sprites/line on TMS99xxA) can be zoomed horizontally
				 * Zoom works normally on SMS2/GG */
                int spriteWidth = (8 << zoomShift);
                if (vdpType == VDPTypes.Mk3SMS1 && s >= NumSpritesPerLine) spriteWidth = 8;

                if (!isDisplayBlanked)
                {
                    int yCoordinate = sprite.Y;
                    int xCoordinate = sprite.X;
                    int patternNumber = sprite.Pattern;
                    int unusedData = sprite.Attribute;

                    if (isSpriteShiftLeft8) xCoordinate -= 8;

                    for (int pixel = 0; pixel < spriteWidth; pixel++)
                    {
                        /* Ignore pixel if column 0 masking is enabled and sprite pixel is in column 0 */
                        if (isColumn0MaskEnabled && (xCoordinate + pixel) < 8) continue;

                        /* Check if sprite is outside active display, else continue to next sprite */
                        if ((xCoordinate + pixel) < 0 || (xCoordinate + pixel) >= NumActivePixelsPerScanline) continue;

                        /* Determine coordinate inside sprite */
                        int inSpriteXCoord = (pixel >> zoomShift) % spriteWidth;
                        int inSpriteYCoord = ((activeDisplayY - yCoordinate) >> zoomShift) % spriteHeight;

                        /* Calculate address and fetch pixel data */
                        int tileIndex = patternNumber;
                        if (isLargeSprites) tileIndex &= ~0x01;
                        ushort tileAddress = (ushort)(spritePatternGenBaseAddress + (tileIndex << 5) + (inSpriteYCoord << 2));

                        /* Get color & check transparency and position */
                        int c = (((ReadVram((ushort)(tileAddress + 0)) >> (7 - inSpriteXCoord)) & 0x1) << 0);
                        c |= (((ReadVram((ushort)(tileAddress + 1)) >> (7 - inSpriteXCoord)) & 0x1) << 1);
                        c |= (((ReadVram((ushort)(tileAddress + 2)) >> (7 - inSpriteXCoord)) & 0x1) << 2);
                        c |= (((ReadVram((ushort)(tileAddress + 3)) >> (7 - inSpriteXCoord)) & 0x1) << 3);

                        if (c == 0) continue;

                        int x = pixelActiveDisplay + (xCoordinate + pixel);
                        if (IsScreenUsageFlagSet(y, x, screenUsageSprite))
                        {
                            /* If sprite was already at this location, set sprite collision flag */
                            isSpriteCollision = true;
                        }
                        else if (!IsScreenUsageFlagSet(y, x, screenUsageBgHighPriority))
                        {
                            /* Draw if pixel isn't occupied by high-priority BG */
                            SetPixel(y, x, 1, c);
                        }

                        /* Note that there is a sprite here regardless */

                        /* VDPDIFF: Mk3 / SMS1 VDP zoomed sprites bug, horizontally zoomed area of sprite (i.e. pixels 9-16) is ignored by collision; do not mark location as containing a sprite
						 * https://www.smspower.org/forums/post109677#109677 
						 * TODO: verify behavior somehow?
						 */
                        if ((vdpType == VDPTypes.Mk3SMS1 && isZoomedSprites && pixel < 8) || (vdpType == VDPTypes.Mk3SMS1 && !isZoomedSprites) || vdpType == VDPTypes.SMS2GG)
                            SetScreenUsageFlag(y, x, screenUsageSprite);
                    }
                }
            }
        }

        protected void UpdateResolution()
        {
            /* Check screenmode */
            if (isSMS240LineMode)
            {
                screenHeight = NumActiveScanlinesHigh;
                nametableHeight = 256;
                vCounterTableIndex = (isPalChip ? 5 : 2);
            }
            else if (isSMS224LineMode)
            {
                screenHeight = NumActiveScanlinesMed;
                nametableHeight = 256;
                vCounterTableIndex = (isPalChip ? 4 : 1);
            }
            else
            {
                screenHeight = NumActiveScanlinesLow;
                nametableHeight = 224;
                vCounterTableIndex = (isPalChip ? 3 : 0);
            }

            /* Scanline parameters */
            if (!isPalChip)
            {
                /* NTSC */
                if (screenHeight == NumActiveScanlinesHigh)
                {
                    /* 240 active lines, invalid on NTSC (dummy values); "Line Interrupt Test #1" mode 1 will show blue screen */
                    topBorderSize = 0;
                    verticalActiveDisplaySize = 0;
                    bottomBorderSize = 0;
                }
                else if (screenHeight == NumActiveScanlinesMed)
                {
                    /* 224 active lines */
                    topBorderSize = 11;
                    verticalActiveDisplaySize = 224;
                    bottomBorderSize = 8;
                }
                else
                {
                    /* 192 active lines */
                    topBorderSize = 27;
                    verticalActiveDisplaySize = 192;
                    bottomBorderSize = 24;
                }
            }
            else
            {
                /* PAL */
                if (screenHeight == NumActiveScanlinesHigh)
                {
                    /* 240 active lines */
                    topBorderSize = 30;
                    verticalActiveDisplaySize = 240;
                    bottomBorderSize = 24;
                }
                else if (screenHeight == NumActiveScanlinesMed)
                {
                    /* 224 active lines */
                    topBorderSize = 38;
                    verticalActiveDisplaySize = 224;
                    bottomBorderSize = 32;
                }
                else
                {
                    /* 192 active lines */
                    topBorderSize = 54;
                    verticalActiveDisplaySize = 192;
                    bottomBorderSize = 48;
                }
            }

            scanlineTopBorder = 0;
            scanlineActiveDisplay = (scanlineTopBorder + topBorderSize);
            scanlineBottomBorder = (scanlineActiveDisplay + verticalActiveDisplaySize);

            numVisibleScanlines = (topBorderSize + verticalActiveDisplaySize + bottomBorderSize);

            /* Pixel parameters */
            leftBorderSize = 13;
            horizontalActiveDisplaySize = 256;
            rightBorderSize = 15;

            pixelLeftBorder = 0;
            pixelActiveDisplay = (pixelLeftBorder + leftBorderSize);
            pixelRightBorder = (pixelActiveDisplay + horizontalActiveDisplaySize);

            numVisiblePixels = (leftBorderSize + horizontalActiveDisplaySize + rightBorderSize);
        }

        protected virtual void WriteColorToFramebuffer(int palette, int color, int address)
        {
            WriteColorToFramebuffer(cram[((palette * 16) + color)], address);
        }

        protected unsafe override void WriteColorToFramebuffer(ushort colorValue, int address)
        {
            /* If not in Master System video mode, color value is index into legacy colormap */
            if (!isBitM4Set)
                colorValue = (legacyColorMap[colorValue & 0x000F]);

            RGB222toBGRA8888(colorValue, ref outputFramebuffer, address);
        }

        protected override void WriteDataPort(byte value)
        {
            isSecondControlWrite = false;

            readBuffer = value;

            switch (codeRegister)
            {
                case 0x00:
                case 0x01:
                case 0x02:
                    WriteVram(addressRegister, value);
                    break;
                case 0x03:
                    cram[(addressRegister & 0x001F)] = value;
                    break;
            }

            addressRegister++;
        }

        protected override byte ReadControlPort()
        {
            byte statusCurrent = (byte)statusFlags;

            statusFlags = StatusFlags.None;
            isSecondControlWrite = false;

            isLineInterruptPending = false;

            InterruptLine = InterruptState.Clear;

            return statusCurrent;
        }

        public override byte ReadPort(byte port)
        {
            if ((port & 0x40) == 0x40)
                if ((port & 0x01) == 0)
                    return (byte)vCounter;      /* V counter */
                else
                    return (byte)hCounter;      /* H counter */
            else
                return base.ReadPort(port);
        }

        protected override void WriteRegister(byte register, byte value)
        {
            //if (register < registers.Length)
            if (register < registersLength)
                registers[register] = value;

            if (register == 0x00 || register == 0x01)
                UpdateResolution();
        }
    }
}
