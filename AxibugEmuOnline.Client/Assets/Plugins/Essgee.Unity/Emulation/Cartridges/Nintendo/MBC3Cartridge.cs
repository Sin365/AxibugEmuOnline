using System;
using System.Linq;

namespace Essgee.Emulation.Cartridges.Nintendo
{
    public class MBC3Cartridge : IGameBoyCartridge
    {
        // https://thomas.spurden.name/gameboy/#mbc3-real-time-clock-rtc

        public class RTC
        {
            public const int NumRegisters = 0x05;

            public byte[] BaseRegisters { get; private set; }
            public byte[] LatchedRegisters { get; private set; }

            public DateTime BaseTime { get; set; }

            public bool IsSelected { get; set; }
            public byte SelectedRegister { get; set; }
            public bool IsLatched { get; set; }

            public RTC()
            {
                BaseRegisters = new byte[NumRegisters];
                LatchedRegisters = new byte[NumRegisters];

                BaseTime = DateTime.Now;

                IsSelected = false;
                SelectedRegister = 0;
                IsLatched = false;
            }



            public void FromSaveData(byte[] ramData)
            {
                var rtcOffset = ramData.Length - 0x30;

                // Time
                BaseRegisters[0x00] = ramData[rtcOffset + 0];
                BaseRegisters[0x01] = ramData[rtcOffset + 4];
                BaseRegisters[0x02] = ramData[rtcOffset + 8];
                BaseRegisters[0x03] = ramData[rtcOffset + 12];
                BaseRegisters[0x04] = ramData[rtcOffset + 16];

                // Latched time
                LatchedRegisters[0x00] = ramData[rtcOffset + 20];
                LatchedRegisters[0x01] = ramData[rtcOffset + 24];
                LatchedRegisters[0x02] = ramData[rtcOffset + 28];
                LatchedRegisters[0x03] = ramData[rtcOffset + 32];
                LatchedRegisters[0x04] = ramData[rtcOffset + 36];

                // Timestamp
                BaseTime = DateTimeOffset.FromUnixTimeSeconds((long)BitConverter.ToUInt64(ramData, rtcOffset + 40)).UtcDateTime;
            }

            public byte[] ToSaveData()
            {
                var appendData = new byte[0x30];

                // Time
                appendData[0] = BaseRegisters[0x00];
                appendData[4] = BaseRegisters[0x01];
                appendData[8] = BaseRegisters[0x02];
                appendData[12] = BaseRegisters[0x03];
                appendData[16] = BaseRegisters[0x04];

                // Latched time
                appendData[20] = LatchedRegisters[0x00];
                appendData[24] = LatchedRegisters[0x01];
                appendData[28] = LatchedRegisters[0x02];
                appendData[32] = LatchedRegisters[0x03];
                appendData[36] = LatchedRegisters[0x04];

                // Timestamp
                var timestamp = BitConverter.GetBytes(((DateTimeOffset)BaseTime).ToUnixTimeSeconds());
                for (var i = 0; i < timestamp.Length; i++) appendData[40 + i] = timestamp[i];

                return appendData;
            }

            public void Update()
            {
                // GOLD,38695,3000 == 00931


                var currentTime = DateTime.Now;
                var newTime = currentTime;

                if (((BaseRegisters[0x04] >> 6) & 0b1) == 0 && currentTime > BaseTime)
                    newTime.Add(currentTime - BaseTime);

                newTime.AddSeconds(BaseRegisters[0x00]);
                newTime.AddMinutes(BaseRegisters[0x01]);
                newTime.AddHours(BaseRegisters[0x02]);
                newTime.AddDays(BaseRegisters[0x03]);
                newTime.AddDays((BaseRegisters[0x04] & 0b1) << 8);

                BaseRegisters[0x00] = (byte)newTime.Second;
                BaseRegisters[0x01] = (byte)newTime.Minute;
                BaseRegisters[0x02] = (byte)newTime.Hour;
                BaseRegisters[0x03] = (byte)(newTime.Day & 0xFF);
                BaseRegisters[0x04] = (byte)((BaseRegisters[0x04] & 0xFE) | ((newTime.Day >> 8) & 0b1) | ((newTime.Day >> 8) & 0b1) << 7);

                BaseTime = currentTime;
            }
        }

        byte[] romData, ramData;
        bool hasBattery, hasRTC;

        byte romBank, ramBank;
        bool ramEnable;

        RTC rtc;

        public MBC3Cartridge(int romSize, int ramSize)
        {
            romData = new byte[romSize];
            ramData = new byte[ramSize];

            hasBattery = false;
            hasRTC = false;

            romBank = 1;
            ramBank = 0;
            ramEnable = false;

            rtc = new RTC();
        }

        #region AxiState

        public void LoadAxiStatus(AxiEssgssStatusData data)
        {
        }

        public AxiEssgssStatusData SaveAxiStatus()
        {
            AxiEssgssStatusData data = new AxiEssgssStatusData();
            return data;
        }
        #endregion
        public void LoadRom(byte[] data)
        {
            Buffer.BlockCopy(data, 0, romData, 0, Math.Min(data.Length, romData.Length));
        }

        public void LoadRam(byte[] data)
        {
            /* Has appended RTC state data? */
            if ((data.Length & 0x30) == 0x30) rtc.FromSaveData(data);

            Buffer.BlockCopy(data, 0, ramData, 0, Math.Min(data.Length, ramData.Length));
        }

        public byte[] GetRomData()
        {
            return romData;
        }

        public byte[] GetRamData()
        {
            if (hasRTC)
                return ramData.Concat(rtc.ToSaveData()).ToArray();
            else
                return ramData;
        }

        public bool IsRamSaveNeeded()
        {
            return hasBattery;
        }

        public ushort GetLowerBound()
        {
            return 0x0000;
        }

        public ushort GetUpperBound()
        {
            return 0x7FFF;
        }

        public void SetCartridgeConfig(bool battery, bool rtc, bool rumble)
        {
            hasBattery = battery;
            hasRTC = rtc;
        }

        public void Step(int clockCyclesInStep)
        {
            /* Nothing to do */
        }

        public byte Read(ushort address)
        {
            if (address >= 0x0000 && address <= 0x3FFF)
            {
                return romData[address & 0x3FFF];
            }
            else if (address >= 0x4000 && address <= 0x7FFF)
            {
                return romData[(romBank << 14) | (address & 0x3FFF)];
            }
            else if (address >= 0xA000 && address <= 0xBFFF)
            {
                if (rtc.IsSelected)
                {
                    if (rtc.IsLatched)
                        return rtc.LatchedRegisters[rtc.SelectedRegister];
                    else
                        return rtc.BaseRegisters[rtc.SelectedRegister];
                }
                else if (ramEnable)
                    return ramData[(ramBank << 13) | (address & 0x1FFF)];
                else
                    return 0xFF;
            }
            else
                return 0xFF;
        }

        public void Write(ushort address, byte value)
        {
            if (address >= 0x0000 && address <= 0x1FFF)
            {
                ramEnable = (value & 0x0F) == 0x0A;
            }
            else if (address >= 0x2000 && address <= 0x3FFF)
            {
                romBank = (byte)((romBank & 0x80) | (value & 0x7F));
                romBank &= (byte)((romData.Length >> 14) - 1);
                if (romBank == 0x00) romBank = 0x01;
            }
            else if (address >= 0x4000 && address <= 0x5FFF)
            {
                if (value >= 0x00 && value <= 0x07)
                {
                    rtc.IsSelected = false;
                    ramBank = (byte)(value & 0x07);
                }
                else if (value >= 0x08 && value <= 0x0C)
                {
                    rtc.IsSelected = true;
                    rtc.SelectedRegister = (byte)(value - 0x08);
                }
            }
            else if (address >= 0x6000 && address <= 0x7FFF)
            {
                if (value == 0x00 && rtc.IsLatched)
                    rtc.IsLatched = false;
                else if (value == 0x01 && !rtc.IsLatched)
                {
                    rtc.Update();
                    for (var i = 0; i < RTC.NumRegisters; i++)
                        rtc.LatchedRegisters[i] = rtc.BaseRegisters[i];
                    rtc.IsLatched = true;
                }
            }
            else if (address >= 0xA000 && address <= 0xBFFF)
            {
                if (rtc.IsSelected)
                {
                    rtc.Update();
                    rtc.BaseRegisters[rtc.SelectedRegister] = value;
                }
                else if (ramEnable)
                    ramData[(ramBank << 13) | (address & 0x1FFF)] = value;
            }
        }

    }
}
