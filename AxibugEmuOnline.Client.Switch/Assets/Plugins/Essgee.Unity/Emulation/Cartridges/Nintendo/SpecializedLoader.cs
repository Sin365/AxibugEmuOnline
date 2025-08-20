using Essgee.Exceptions;
using System;

namespace Essgee.Emulation.Cartridges.Nintendo
{
    public static class SpecializedLoader
    {
        internal static IGameBoyCartridge CreateCartridgeInstance(byte[] romData, byte[] ramData, Type mapperType)
        {
            var romSize = -1;
            switch (romData[0x0148])
            {
                case 0x00: romSize = 32 * 1024; break;
                case 0x01: romSize = 64 * 1024; break;
                case 0x02: romSize = 128 * 1024; break;
                case 0x03: romSize = 256 * 1024; break;
                case 0x04: romSize = 512 * 1024; break;
                case 0x05: romSize = 1024 * 1024; break;
                case 0x06: romSize = 2048 * 1024; break;
                case 0x07: romSize = 4096 * 1024; break;
                case 0x08: romSize = 8192 * 1024; break;
                case 0x52: romSize = 1152 * 1024; break;
                case 0x53: romSize = 1280 * 1024; break;
                case 0x54: romSize = 1536 * 1024; break;

                default: romSize = romData.Length; break;
            }

            var ramSize = -1;
            switch (romData[0x0149])
            {
                case 0x00: ramSize = 0 * 1024; break;
                case 0x01: ramSize = 2 * 1024; break;
                case 0x02: ramSize = 8 * 1024; break;
                case 0x03: ramSize = 32 * 1024; break;
                case 0x04: ramSize = 128 * 1024; break;
                case 0x05: ramSize = 64 * 1024; break;

                default: ramSize = 0; break;
            }

            /* NOTES:
			 *  MBC2 internal RAM is not given in header, 512*4b == 256 bytes 
			 *  GB Camera internal RAM ~seems~ to not be given in header? 128 kbytes
			 */

            var mapperTypeFromHeader = typeof(NoMapperCartridge);
            var hasBattery = false;
            var hasRtc = false;
            var hasRumble = false;
            switch (romData[0x0147])
            {
                case 0x00: mapperType = typeof(NoMapperCartridge); break;
                case 0x01: mapperType = typeof(MBC1Cartridge); break;
                case 0x02: mapperType = typeof(MBC1Cartridge); break;
                case 0x03: mapperType = typeof(MBC1Cartridge); hasBattery = true; break;
                case 0x05: mapperType = typeof(MBC2Cartridge); ramSize = 0x100; break;
                case 0x06: mapperType = typeof(MBC2Cartridge); ramSize = 0x100; hasBattery = true; break;
                case 0x08: mapperType = typeof(NoMapperCartridge); break;
                case 0x09: mapperType = typeof(NoMapperCartridge); hasBattery = true; break;
                // 0B-0D, MMM01
                case 0x0F: mapperType = typeof(MBC3Cartridge); hasBattery = true; hasRtc = true; break;
                case 0x10: mapperType = typeof(MBC3Cartridge); hasBattery = true; hasRtc = true; break;
                case 0x11: mapperType = typeof(MBC3Cartridge); break;
                case 0x12: mapperType = typeof(MBC3Cartridge); break;
                case 0x13: mapperType = typeof(MBC3Cartridge); hasBattery = true; break;
                case 0x19: mapperType = typeof(MBC5Cartridge); break;
                case 0x1A: mapperType = typeof(MBC5Cartridge); break;
                case 0x1B: mapperType = typeof(MBC5Cartridge); hasBattery = true; break;
                case 0x1C: mapperType = typeof(MBC5Cartridge); hasRumble = true; break;
                case 0x1D: mapperType = typeof(MBC5Cartridge); hasRumble = true; break;
                case 0x1E: mapperType = typeof(MBC5Cartridge); hasBattery = true; hasRumble = true; break;
                // 20, MBC6
                // 22, MBC7
                case 0xFC: mapperType = typeof(GBCameraCartridge); ramSize = 128 * 1024; break;
                // FD, BANDAI TAMA5
                // FE, HuC3
                // FF, HuC1

                default: throw new EmulationException($"Unimplemented cartridge type 0x{romData[0x0147]:X2}");
            }

            if (mapperType == null)
                mapperType = mapperTypeFromHeader;

            if (romSize != romData.Length)
            {
                var romSizePadded = 1;
                while (romSizePadded < romData.Length) romSizePadded <<= 1;
                romSize = Math.Max(romSizePadded, romData.Length);
            }

            var cartridge = (IGameBoyCartridge)Activator.CreateInstance(mapperType, new object[] { romSize, ramSize });
            cartridge.LoadRom(romData);
            cartridge.LoadRam(ramData);
            cartridge.SetCartridgeConfig(hasBattery, hasRtc, hasRumble);

            return cartridge;
        }
    }
}
