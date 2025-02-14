using Essgee.Emulation.Audio;
using Essgee.Emulation.Cartridges.Nintendo;
using Essgee.Emulation.Configuration;
using Essgee.Emulation.CPU;
using Essgee.Emulation.ExtDevices.Nintendo;
using Essgee.Emulation.Video.Nintendo;
using Essgee.EventArguments;
using Essgee.Exceptions;
using Essgee.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace Essgee.Emulation.Machines
{
    [MachineIndex(6)]
    public class GameBoyColor : IMachine
    {
        const double masterClock = 4194304;
        const double refreshRate = 59.727500569606;

        const int wramSize = 8 * 1024;
        const int hramSize = 0x7F;

        const int serialCycleCountNormal = 512;
        const int serialCycleCountFast = 16;

        public event EventHandler<SendLogMessageEventArgs> SendLogMessage;
        protected virtual void OnSendLogMessage(SendLogMessageEventArgs e) { SendLogMessage?.Invoke(this, e); }

        public event EventHandler<EventArgs> EmulationReset;
        protected virtual void OnEmulationReset(EventArgs e) { EmulationReset?.Invoke(this, e); }

        public event EventHandler<RenderScreenEventArgs> RenderScreen
        {
            add { video.RenderScreen += value; }
            remove { video.RenderScreen -= value; }
        }

        public event EventHandler<SizeScreenEventArgs> SizeScreen
        {
            add { video.SizeScreen += value; }
            remove { video.SizeScreen -= value; }
        }

        public event EventHandler<ChangeViewportEventArgs> ChangeViewport;
        protected virtual void OnChangeViewport(ChangeViewportEventArgs e) { ChangeViewport?.Invoke(this, e); }

        public event EventHandler<PollInputEventArgs> PollInput;
        protected virtual void OnPollInput(PollInputEventArgs e) { PollInput?.Invoke(this, e); }

        public event EventHandler<EnqueueSamplesEventArgs> EnqueueSamples
        {
            add { audio.EnqueueSamples += value; }
            remove { audio.EnqueueSamples -= value; }
        }

        public event EventHandler<SaveExtraDataEventArgs> SaveExtraData;
        protected virtual void OnSaveExtraData(SaveExtraDataEventArgs e) { SaveExtraData?.Invoke(this, e); }

        public event EventHandler<EventArgs> EnableRumble;
        protected virtual void OnEnableRumble(EventArgs e) { EnableRumble?.Invoke(this, EventArgs.Empty); }

        public string ManufacturerName => "Nintendo";
        public string ModelName => "Game Boy Color";
        public string DatFilename => "Nintendo - Game Boy Color.dat";
        public (string Extension, string Description) FileFilter => (".gbc", "Game Boy Color ROMs");
        public bool HasBootstrap => true;
        public double RefreshRate => refreshRate;
        public double PixelAspectRatio => 1.0;
        public (string Name, string Description)[] RuntimeOptions => video.RuntimeOptions.Concat(audio.RuntimeOptions).ToArray();

        byte[] bootstrap;
        IGameBoyCartridge cartridge;
        byte[,] wram;
        byte[] hram;
        byte ie;
        SM83CGB cpu;
        CGBVideo video;
        CGBAudio audio;
        ISerialDevice serialDevice;

        // FF00 - P1/JOYP
        byte joypadRegister;

        // FF01 - SB
        byte serialData;
        // FF02 - SC
        bool serialUseInternalClock, serialFastClockSpeed, serialTransferInProgress;

        // FF04 - DIV
        byte divider;
        // FF05 - TIMA
        byte timerCounter;
        //
        ushort clockCycleCount;

        // FF06 - TMA
        byte timerModulo;

        // FF07 - TAC
        bool timerRunning;
        byte timerInputClock;
        //
        bool timerOverflow, timerLoading;

        // FF0F - IF
        bool irqVBlank, irqLCDCStatus, irqTimerOverflow, irqSerialIO, irqKeypad;

        // FF4C

        // FF4D - KEY1
        bool speedIsDouble, speedSwitchPending;

        // FF50
        bool bootstrapDisabled;

        // FF56 - RP
        bool irSendingSignal, irNotReceivingSignal, irReadEnableA, irReadEnableB;

        // FF70 - SVBK
        byte wramBank;

        public enum InfraredSources
        {
            [Description("None")]
            None,
            [Description("Random")]
            Random,
            [Description("Constant Light (Lamp)")]
            ConstantOn,
            [Description("Pocket Pikachu Color")]
            PocketPikachuColor
        }
        ushort[] irDatabase;
        int irDatabaseBaseIndex, irDatabaseStep;
        int irDatabaseCurrentIndex, irCycles;
        bool irExternalTransferActive;

        [Flags]
        enum JoypadInputs : byte
        {
            Right = (1 << 0),
            Left = (1 << 1),
            Up = (1 << 2),
            Down = (1 << 3),
            A = (1 << 4),
            B = (1 << 5),
            Select = (1 << 6),
            Start = (1 << 7)
        }

        JoypadInputs inputsPressed;

        int serialBitsCounter, serialCycles, clockCyclesPerSerialBit;

        int currentMasterClockCyclesInFrame, totalMasterClockCyclesInFrame;

        public Configuration.GameBoyColor configuration;

        public GameBoyColor() { }

        public void Initialize()
        {
            bootstrap = null;
            cartridge = null;

            wram = new byte[8, wramSize];
            hram = new byte[hramSize];
            cpu = new SM83CGB(ReadMemory, WriteMemory);
            video = new CGBVideo(ReadMemory, cpu.RequestInterrupt);
            audio = new CGBAudio();
            serialDevice = null;

            video.EndOfScanline += (s, e) =>
            {
                PollInputEventArgs pollInputEventArgs = PollInputEventArgs.Create();
                OnPollInput(pollInputEventArgs);
                ParseInput(pollInputEventArgs);
                pollInputEventArgs.Release();
            };
        }

        public void SetConfiguration(IConfiguration config)
        {
            configuration = (Configuration.GameBoyColor)config;

            ReconfigureSystem();
        }

        public object GetRuntimeOption(string name)
        {
            if (name.StartsWith("Graphics"))
                return video.GetRuntimeOption(name);
            else if (name.StartsWith("Audio"))
                return audio.GetRuntimeOption(name);
            else
                return null;
        }

        public void SetRuntimeOption(string name, object value)
        {
            if (name.StartsWith("Graphics"))
                video.SetRuntimeOption(name, value);
            else if (name.StartsWith("Audio"))
                audio.SetRuntimeOption(name, value);
        }

        private void ReconfigureSystem()
        {
            /* Video */
            video?.SetClockRate(masterClock);
            video?.SetRefreshRate(refreshRate);
            video?.SetRevision(0);

            /* Audio */
            audio?.SetSampleRate(EmuStandInfo.Configuration.SampleRate);
            audio?.SetOutputChannels(2);
            audio?.SetClockRate(masterClock);
            audio?.SetRefreshRate(refreshRate);

            /* Cartridge */
            if (cartridge is GBCameraCartridge camCartridge)
                camCartridge.SetImageSource(configuration.CameraSource, configuration.CameraImageFile);

            /* Serial */
            if (serialDevice != null)
            {
                serialDevice.SaveExtraData -= SaveExtraData;
                serialDevice.Shutdown();
            }

            serialDevice = (ISerialDevice)Activator.CreateInstance(configuration.SerialDevice);
            serialDevice.SaveExtraData += SaveExtraData;
            serialDevice.Initialize();

            /* Infrared */
            irDatabaseBaseIndex = 0;
            irDatabaseStep = 0;
            irDatabaseCurrentIndex = irCycles = 0;
            irExternalTransferActive = false;

            if (configuration.InfraredSource == InfraredSources.PocketPikachuColor && File.Exists(configuration.InfraredDatabasePikachu))
            {
                using (var reader = new BinaryReader(new FileStream(configuration.InfraredDatabasePikachu, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite)))
                {
                    irDatabase = new ushort[reader.BaseStream.Length / 2];
                    for (var i = 0; i < irDatabase.Length; i++)
                        irDatabase[i] = reader.ReadUInt16();

                    irDatabaseStep = 2007;
                    if ((irDatabaseBaseIndex < 0) || (irDatabaseBaseIndex * irDatabaseStep >= irDatabase.Length))
                        irDatabaseBaseIndex = 0;
                }
            }

            /* Misc timing */
            currentMasterClockCyclesInFrame = 0;
            totalMasterClockCyclesInFrame = (int)Math.Round(masterClock / refreshRate);

            /* Announce viewport */
            var eventArgs = ChangeViewportEventArgs.Create(video.Viewport);
            OnChangeViewport(eventArgs);
            eventArgs.Release();
        }

        private void LoadBootstrap()
        {
            if (configuration.UseBootstrap)
            {
                var (type, bootstrapRomData) = CartridgeLoader.Load(configuration.BootstrapRom, "Game Boy Color Bootstrap");
                bootstrap = new byte[bootstrapRomData.Length];
                Buffer.BlockCopy(bootstrapRomData, 0, bootstrap, 0, bootstrap.Length);
            }
        }

        public void Startup()
        {
            LoadBootstrap();

            cpu.Startup();
            video.Startup();
            audio.Startup();
        }

        public void Reset()
        {
            cpu.Reset();
            video.Reset();
            audio.Reset();

            if (configuration.UseBootstrap)
            {
                cpu.SetProgramCounter(0x0000);
                cpu.SetStackPointer(0x0000);
            }
            else
            {
                cpu.SetProgramCounter(0x0100);
                cpu.SetStackPointer(0xFFFE);
                cpu.SetRegisterAF(0x11B0);
                cpu.SetRegisterBC(0x0000);
                cpu.SetRegisterDE(0xFF56);
                cpu.SetRegisterHL(0x000D);

                video.WritePort(0x40, 0x91);
                video.WritePort(0x42, 0x00);
                video.WritePort(0x43, 0x00);
                video.WritePort(0x45, 0x00);
                video.WritePort(0x47, 0xFC);
                video.WritePort(0x48, 0xFF);
                video.WritePort(0x49, 0xFF);
                video.WritePort(0x4A, 0x00);
                video.WritePort(0x4B, 0x00);
            }

            joypadRegister = 0x0F;

            serialData = 0xFF;
            serialUseInternalClock = serialFastClockSpeed = serialTransferInProgress = false;

            timerCounter = 0;
            clockCycleCount = 0;

            timerModulo = 0;

            timerRunning = false;
            timerInputClock = 0;

            timerOverflow = timerLoading = false;

            irqVBlank = irqLCDCStatus = irqTimerOverflow = irqSerialIO = irqKeypad = false;

            bootstrapDisabled = !configuration.UseBootstrap;

            irSendingSignal = irReadEnableA = irReadEnableB = false;
            irNotReceivingSignal = true;

            wramBank = 0x01;

            inputsPressed = 0;

            serialBitsCounter = serialCycles = clockCyclesPerSerialBit = 0;

            OnEmulationReset(EventArgs.Empty);
        }

        public void Shutdown()
        {
            if (serialDevice != null)
            {
                serialDevice.SaveExtraData -= SaveExtraData;
                serialDevice.Shutdown();
            }

            if (cartridge is MBC5Cartridge mbc5Cartridge)
                mbc5Cartridge.EnableRumble -= EnableRumble;

            cpu?.Shutdown();
            video?.Shutdown();
            audio?.Shutdown();
        }

        public void SetState(Dictionary<string, dynamic> state)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, dynamic> GetState()
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, dynamic> GetDebugInformation()
        {
            var dict = new Dictionary<string, dynamic>
            {
                { "CyclesInFrame", currentMasterClockCyclesInFrame },
            };

            return dict;
        }

        public void Load(byte[] romData, byte[] ramData, Type mapperType)
        {
            cartridge = SpecializedLoader.CreateCartridgeInstance(romData, ramData, mapperType);

            if (cartridge is GBCameraCartridge camCartridge)
                camCartridge.SetImageSource(configuration.CameraSource, configuration.CameraImageFile);

            if (cartridge is MBC5Cartridge mbc5Cartridge)
                mbc5Cartridge.EnableRumble += EnableRumble;
        }

        public byte[] GetCartridgeRam()
        {
            return cartridge?.GetRamData();
        }

        public bool IsCartridgeRamSaveNeeded()
        {
            if (cartridge == null) return false;
            return cartridge.IsRamSaveNeeded();
        }

        public virtual void RunFrame()
        {
            while (currentMasterClockCyclesInFrame < totalMasterClockCyclesInFrame)
                RunStep();

            currentMasterClockCyclesInFrame = 0;
        }

        public void RunStep()
        {
            // TODO: verify if current handling of CGB double speed mode is correct! seems to work but is probably wrong??
            // NOTES:
            //  https://github.com/LIJI32/GBVideoPlayer/blob/master/How%20It%20Works.md#hblank-and-sub-hblank-tricks
            //  https://gbdev.io/pandocs/#ff4d-key1-cgb-mode-only-prepare-speed-switch

            var clockCyclesInStep = RunCpuStep();
            var cycleLength = cpu.IsDoubleSpeed ? 2 : 4;

            video.IsDoubleSpeed = cpu.IsDoubleSpeed;

            for (var s = 0; s < clockCyclesInStep / 4; s++)
            {
                HandleTimerOverflow();
                UpdateCycleCounter((ushort)(clockCycleCount + 4));

                HandleSerialIO(cycleLength);

                HandleIRCommunication(cycleLength);

                video.Step(cycleLength);
                audio.Step(cycleLength);
                cartridge?.Step(cycleLength);

                currentMasterClockCyclesInFrame += cycleLength;
            }
        }

        private int RunCpuStep()
        {
            if (video.GDMAWaitCycles > 0)
            {
                var cycleLength = cpu.IsDoubleSpeed ? 2 : 4;
                video.GDMAWaitCycles -= cycleLength;
                return cycleLength;
            }
            else
                return cpu.Step();
        }

        private void IncrementTimer()
        {
            timerCounter++;
            if (timerCounter == 0) timerOverflow = true;
        }

        private bool GetTimerBit(byte value, ushort cycles)
        {
            switch (value & 0b11)
            {
                case 0: return (cycles & (1 << 9)) != 0;
                case 1: return (cycles & (1 << 3)) != 0;
                case 2: return (cycles & (1 << 5)) != 0;
                case 3: return (cycles & (1 << 7)) != 0;
                default: throw new EmulationException("Unhandled timer state");
            }
        }

        private void UpdateCycleCounter(ushort value)
        {
            if (timerRunning)
            {
                if (!GetTimerBit(timerInputClock, value) && GetTimerBit(timerInputClock, clockCycleCount))
                    IncrementTimer();
            }

            clockCycleCount = value;
            divider = (byte)(clockCycleCount >> 8);
        }

        private void HandleTimerOverflow()
        {
            timerLoading = false;

            if (timerOverflow)
            {
                cpu.RequestInterrupt(SM83.InterruptSource.TimerOverflow);
                timerOverflow = false;

                timerCounter = timerModulo;
                timerLoading = true;
            }
        }

        private void HandleSerialIO(int clockCyclesInStep)
        {
            if (serialTransferInProgress)
            {
                for (var c = 0; c < clockCyclesInStep; c++)
                {
                    serialCycles++;
                    if (serialCycles == clockCyclesPerSerialBit)
                    {
                        serialCycles = 0;

                        serialBitsCounter--;

                        var bitToSend = (byte)((serialData >> 7) & 0b1);
                        var bitReceived = serialDevice.ExchangeBit(serialBitsCounter, bitToSend);
                        serialData = (byte)((serialData << 1) | (bitReceived & 0b1));

                        if (serialBitsCounter == 0)
                        {
                            cpu.RequestInterrupt(SM83.InterruptSource.SerialIO);
                            serialTransferInProgress = false;
                        }
                    }
                }
            }
        }

        private void HandleIRCommunication(int clockCyclesInStep)
        {
            switch (configuration.InfraredSource)
            {
                case InfraredSources.None:
                    irNotReceivingSignal = true;
                    break;

                case InfraredSources.Random:
                    irNotReceivingSignal = (EmuStandInfo.Random.Next(256) % 2) == 0;
                    break;

                case InfraredSources.ConstantOn:
                    irNotReceivingSignal = false;
                    break;

                case InfraredSources.PocketPikachuColor:
                    if (irExternalTransferActive)
                    {
                        for (var c = 0; c < clockCyclesInStep; c++)
                        {
                            irCycles++;
                            if (irCycles == irDatabase[(irDatabaseBaseIndex * irDatabaseStep) + irDatabaseCurrentIndex])
                            {
                                irCycles = 0;

                                irNotReceivingSignal = !irNotReceivingSignal;

                                irDatabaseCurrentIndex++;
                                if (irDatabaseCurrentIndex >= irDatabaseStep)
                                {
                                    irDatabaseCurrentIndex = 0;
                                    irExternalTransferActive = false;
                                    irNotReceivingSignal = true;
                                }
                            }
                        }
                    }
                    break;
            }
        }

        private void ParseInput(PollInputEventArgs eventArgs)
        {
            inputsPressed = 0;

            /* Keyboard */
            if (eventArgs.Keyboard.Contains(configuration.ControlsRight) && !eventArgs.Keyboard.Contains(configuration.ControlsLeft)) inputsPressed |= JoypadInputs.Right;
            if (eventArgs.Keyboard.Contains(configuration.ControlsLeft) && !eventArgs.Keyboard.Contains(configuration.ControlsRight)) inputsPressed |= JoypadInputs.Left;
            if (eventArgs.Keyboard.Contains(configuration.ControlsUp) && !eventArgs.Keyboard.Contains(configuration.ControlsDown)) inputsPressed |= JoypadInputs.Up;
            if (eventArgs.Keyboard.Contains(configuration.ControlsDown) && !eventArgs.Keyboard.Contains(configuration.ControlsUp)) inputsPressed |= JoypadInputs.Down;
            if (eventArgs.Keyboard.Contains(configuration.ControlsA)) inputsPressed |= JoypadInputs.A;
            if (eventArgs.Keyboard.Contains(configuration.ControlsB)) inputsPressed |= JoypadInputs.B;
            if (eventArgs.Keyboard.Contains(configuration.ControlsSelect)) inputsPressed |= JoypadInputs.Select;
            if (eventArgs.Keyboard.Contains(configuration.ControlsStart)) inputsPressed |= JoypadInputs.Start;

            if (eventArgs.Keyboard.Contains(configuration.ControlsSendIR))
            {
                irExternalTransferActive = true;
                irDatabaseCurrentIndex = 0;
                irCycles = 0;

                irNotReceivingSignal = false;
            }

            /* XInput controller */
            //if (eventArgs.ControllerState.IsAnyRightDirectionPressed() && !eventArgs.ControllerState.IsAnyLeftDirectionPressed()) inputsPressed |= JoypadInputs.Right;
            //if (eventArgs.ControllerState.IsAnyLeftDirectionPressed() && !eventArgs.ControllerState.IsAnyRightDirectionPressed()) inputsPressed |= JoypadInputs.Left;
            //if (eventArgs.ControllerState.IsAnyUpDirectionPressed() && !eventArgs.ControllerState.IsAnyDownDirectionPressed()) inputsPressed |= JoypadInputs.Up;
            //if (eventArgs.ControllerState.IsAnyDownDirectionPressed() && !eventArgs.ControllerState.IsAnyUpDirectionPressed()) inputsPressed |= JoypadInputs.Down;
            //if (eventArgs.ControllerState.IsAPressed()) inputsPressed |= JoypadInputs.A;
            //if (eventArgs.ControllerState.IsXPressed() || eventArgs.ControllerState.IsBPressed()) inputsPressed |= JoypadInputs.B;
            //if (eventArgs.ControllerState.IsBackPressed()) inputsPressed |= JoypadInputs.Select;
            //if (eventArgs.ControllerState.IsStartPressed()) inputsPressed |= JoypadInputs.Start;
        }

        private byte ReadMemory(ushort address)
        {
            if (address >= 0x0000 && address <= 0x7FFF)
            {
                if (configuration.UseBootstrap && (address <= 0x00FF || (address >= 0x0200 && address <= 0x08FF)) && !bootstrapDisabled)
                    return bootstrap[address];
                else
                    return (cartridge != null ? cartridge.Read(address) : (byte)0xFF);
            }
            else if (address >= 0x8000 && address <= 0x9FFF)
            {
                return video.ReadVram(address);
            }
            else if (address >= 0xA000 && address <= 0xBFFF)
            {
                return (cartridge != null ? cartridge.Read(address) : (byte)0xFF);
            }
            else if (address >= 0xC000 && address <= 0xFDFF)
            {
                if ((address & 0x1000) == 0)
                    return wram[0, address & 0x0FFF];
                else
                    return wram[wramBank, address & 0x0FFF];
            }
            else if (address >= 0xFE00 && address <= 0xFE9F)
            {
                return video.ReadOam(address);
            }
            else if (address >= 0xFF00 && address <= 0xFF7F)
            {
                return ReadIo(address);
            }
            else if (address >= 0xFF80 && address <= 0xFFFE)
            {
                return hram[address - 0xFF80];
            }
            else if (address == 0xFFFF)
            {
                return ie;
            }

            /* Cannot read from address, return 0 */
            return 0x00;
        }

        private byte ReadIo(ushort address)
        {
            if (((address & 0xFFF0) == 0xFF40 && address != 0xFF4C && address != 0xFF4D) || (address >= 0xFF51 && address <= 0xFF55) || (address >= 0xFF68 && address <= 0xFF6B))
                return video.ReadPort((byte)(address & 0xFF));
            else if ((address & 0xFFF0) == 0xFF10 || (address & 0xFFF0) == 0xFF20 || (address & 0xFFF0) == 0xFF30)
                return audio.ReadPort((byte)(address & 0xFF));
            else
            {
                switch (address)
                {
                    case 0xFF00:
                        // P1/JOYP
                        return joypadRegister;

                    case 0xFF01:
                        // SB
                        return serialData;

                    case 0xFF02:
                        // SC
                        return (byte)(
                            0x7C |
                            (serialUseInternalClock ? (1 << 0) : 0) |
                            (serialFastClockSpeed ? (1 << 1) : 0) |
                            (serialTransferInProgress ? (1 << 7) : 0));

                    case 0xFF04:
                        // DIV
                        return divider;

                    case 0xFF05:
                        // TIMA
                        return timerCounter;

                    case 0xFF06:
                        // TMA
                        return timerModulo;

                    case 0xFF07:
                        // TAC
                        return (byte)(
                            0xF8 |
                            (timerRunning ? (1 << 2) : 0) |
                            (timerInputClock & 0b11));

                    case 0xFF0F:
                        // IF
                        return (byte)(
                            0xE0 |
                            (irqVBlank ? (1 << 0) : 0) |
                            (irqLCDCStatus ? (1 << 1) : 0) |
                            (irqTimerOverflow ? (1 << 2) : 0) |
                            (irqSerialIO ? (1 << 3) : 0) |
                            (irqKeypad ? (1 << 4) : 0));

                    case 0xFF4D:
                        // KEY1
                        return (byte)(
                            0x7E |
                            (speedSwitchPending ? (1 << 0) : 0) |
                            ((speedIsDouble = cpu.IsDoubleSpeed) ? (1 << 7) : 0));

                    case 0xFF50:
                        // Bootstrap disable
                        return (byte)(
                            0xFE |
                            (bootstrapDisabled ? (1 << 0) : 0));

                    case 0xFF56:
                        // RP
                        return (byte)(
                            0x3C |
                            (irSendingSignal ? (1 << 0) : 0) |
                            (!irReadEnableA || !irReadEnableB || irNotReceivingSignal ? (1 << 1) : 0) |
                            (irReadEnableA ? (1 << 6) : 0) |
                            (irReadEnableB ? (1 << 7) : 0));

                    case 0xFF70:
                        // SVBK
                        return (byte)(
                            0xF8 |
                            (wramBank & 0b111));

                    default:
                        return 0xFF;// throw new NotImplementedException();
                }
            }
        }

        private void WriteMemory(ushort address, byte value)
        {
            if (address >= 0x0000 && address <= 0x7FFF)
            {
                cartridge?.Write(address, value);
            }
            else if (address >= 0x8000 && address <= 0x9FFF)
            {
                video.WriteVram(address, value);
            }
            else if (address >= 0xA000 && address <= 0xBFFF)
            {
                cartridge?.Write(address, value);
            }
            else if (address >= 0xC000 && address <= 0xFDFF)
            {
                if ((address & 0x1000) == 0)
                    wram[0, address & 0x0FFF] = value;
                else
                    wram[wramBank, address & 0x0FFF] = value;
            }
            else if (address >= 0xFE00 && address <= 0xFE9F)
            {
                video.WriteOam(address, value);
            }
            else if (address >= 0xFF00 && address <= 0xFF7F)
            {
                WriteIo(address, value);
            }
            else if (address >= 0xFF80 && address <= 0xFFFE)
            {
                hram[address - 0xFF80] = value;
            }
            else if (address == 0xFFFF)
            {
                ie = value;
            }
        }

        private void WriteIo(ushort address, byte value)
        {
            if (((address & 0xFFF0) == 0xFF40 && address != 0xFF4C && address != 0xFF4D) || (address >= 0xFF51 && address <= 0xFF55) || (address >= 0xFF68 && address <= 0xFF6B))
                video.WritePort((byte)(address & 0xFF), value);
            else if ((address & 0xFFF0) == 0xFF10 || (address & 0xFFF0) == 0xFF20 || (address & 0xFFF0) == 0xFF30)
                audio.WritePort((byte)(address & 0xFF), value);
            else
            {
                switch (address)
                {
                    case 0xFF00:
                        joypadRegister = (byte)((joypadRegister & 0xC0) | (value & 0x30));
                        if ((joypadRegister & 0x30) == 0x20)
                            joypadRegister |= (byte)(((byte)inputsPressed & 0x0F) ^ 0x0F);
                        else if ((joypadRegister & 0x30) == 0x10)
                            joypadRegister |= (byte)((((byte)inputsPressed & 0xF0) >> 4) ^ 0x0F);
                        else
                            joypadRegister = 0xFF;
                        break;

                    case 0xFF01:
                        serialData = value;
                        break;

                    case 0xFF02:
                        serialUseInternalClock = (value & (1 << 0)) != 0;
                        serialFastClockSpeed = (value & (1 << 1)) != 0;
                        serialTransferInProgress = (value & (1 << 7)) != 0;

                        clockCyclesPerSerialBit = (serialFastClockSpeed ? serialCycleCountFast : serialCycleCountNormal) >> (cpu.IsDoubleSpeed ? 1 : 0);

                        if (serialTransferInProgress) serialCycles = 0;
                        serialBitsCounter = 8;
                        break;

                    case 0xFF04:
                        UpdateCycleCounter(0);
                        break;

                    case 0xFF05:
                        if (!timerLoading)
                        {
                            timerCounter = value;
                            timerOverflow = false;
                        }
                        break;

                    case 0xFF06:
                        timerModulo = value;
                        if (timerLoading)
                            timerCounter = value;
                        break;

                    case 0xFF07:
                        {
                            var newTimerRunning = (value & (1 << 2)) != 0;
                            var newTimerInputClock = (byte)(value & 0b11);

                            var oldBit = timerRunning && GetTimerBit(timerInputClock, clockCycleCount);
                            var newBit = newTimerRunning && GetTimerBit(newTimerInputClock, clockCycleCount);

                            if (oldBit && !newBit)
                                IncrementTimer();

                            timerRunning = newTimerRunning;
                            timerInputClock = newTimerInputClock;
                        }
                        break;

                    case 0xFF0F:
                        irqVBlank = (value & (1 << 0)) != 0;
                        irqLCDCStatus = (value & (1 << 1)) != 0;
                        irqTimerOverflow = (value & (1 << 2)) != 0;
                        irqSerialIO = (value & (1 << 3)) != 0;
                        irqKeypad = (value & (1 << 4)) != 0;
                        break;

                    case 0xFF4D:
                        speedSwitchPending = (value & (1 << 0)) != 0;
                        break;

                    case 0xFF50:
                        if (!bootstrapDisabled)
                            bootstrapDisabled = (value & (1 << 0)) != 0;
                        break;

                    case 0xFF56:
                        irSendingSignal = (value & (1 << 0)) != 0;
                        irReadEnableA = (value & (1 << 6)) != 0;
                        irReadEnableB = (value & (1 << 7)) != 0;
                        break;

                    case 0xFF70:
                        wramBank = (byte)(value & 0b111);
                        if (wramBank == 0x00) wramBank = 0x01;
                        break;
                }
            }
        }
    }
}
