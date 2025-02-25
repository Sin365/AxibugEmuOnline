using Essgee.Emulation.Audio;
using Essgee.Emulation.Cartridges;
using Essgee.Emulation.Cartridges.Sega;
using Essgee.Emulation.Configuration;
using Essgee.Emulation.CPU;
using Essgee.Emulation.Video;
using Essgee.EventArguments;
using Essgee.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using static Essgee.Emulation.Utilities;

namespace Essgee.Emulation.Machines
{
    [MachineIndex(3)]
    public class GameGear : IMachine
    {
        const double masterClock = 10738635;
        const double refreshRate = 59.922743;

        const int ramSize = 1 * 8192;

        double vdpClock, psgClock;

        public event EventHandler<SendLogMessageEventArgs> SendLogMessage;
        protected virtual void OnSendLogMessage(SendLogMessageEventArgs e) { SendLogMessage?.Invoke(this, e); }

        public event EventHandler<EventArgs> EmulationReset;
        protected virtual void OnEmulationReset(EventArgs e) { EmulationReset?.Invoke(this, e); }

        public event EventHandler<RenderScreenEventArgs> RenderScreen
        {
            add { vdp.RenderScreen += value; }
            remove { vdp.RenderScreen -= value; }
        }

        public event EventHandler<SizeScreenEventArgs> SizeScreen
        {
            add { vdp.SizeScreen += value; }
            remove { vdp.SizeScreen -= value; }
        }

        public event EventHandler<ChangeViewportEventArgs> ChangeViewport;
        protected virtual void OnChangeViewport(ChangeViewportEventArgs e) { ChangeViewport?.Invoke(this, e); }

        public event EventHandler<PollInputEventArgs> PollInput;
        protected virtual void OnPollInput(PollInputEventArgs e) { PollInput?.Invoke(this, e); }

        public event EventHandler<EnqueueSamplesEventArgs> EnqueueSamples
        {
            add { psg.EnqueueSamples += value; }
            remove { psg.EnqueueSamples -= value; }
        }

        public event EventHandler<SaveExtraDataEventArgs> SaveExtraData;
        protected virtual void OnSaveExtraData(SaveExtraDataEventArgs e) { SaveExtraData?.Invoke(this, e); }

        public event EventHandler<EventArgs> EnableRumble { add { } remove { } }

        public string ManufacturerName => "Sega";
        public string ModelName => "Game Gear";
        public string DatFilename => "Sega - Game Gear.dat";
        public (string Extension, string Description) FileFilter => (".gg", "Game Gear ROMs");
        public bool HasBootstrap => true;
        public double RefreshRate => refreshRate;
        public double PixelAspectRatio => 8.0 / 7.0;
        public (string Name, string Description)[] RuntimeOptions => vdp.RuntimeOptions.Concat(psg.RuntimeOptions).ToArray();

        ICartridge bootstrap, cartridge;
        byte[] wram;
        Z80A cpu;
        SegaGGVDP vdp;
        SegaGGPSG psg;

        [Flags]
        enum IOPortABInputs : byte
        {
            PortAUp = (1 << 0),
            PortADown = (1 << 1),
            PortALeft = (1 << 2),
            PortARight = (1 << 3),
            PortATL = (1 << 4),
            PortATR = (1 << 5),
            PortBUp = (1 << 6),
            PortBDown = (1 << 7)
        }

        [Flags]
        enum IOPortBMiscInputs : byte
        {
            PortBLeft = (1 << 0),
            PortBRight = (1 << 1),
            PortBTL = (1 << 2),
            PortBTR = (1 << 3),
            Reset = (1 << 4),
            CartSlotCONTPin = (1 << 5),
            PortATH = (1 << 6),
            PortBTH = (1 << 7)
        }

        [Flags]
        enum IOPortCInputs : byte
        {
            Start = (1 << 7)
        }

        IOPortABInputs portAInputsPressed;
        IOPortBMiscInputs portBInputsPressed;
        IOPortCInputs portCInputsPressed;

        byte portMemoryControl, portIoControl, hCounterLatched, portIoAB, portIoBMisc;
        byte portIoC, portParallelData, portDataDirNMI, portTxBuffer, portRxBuffer, portSerialControl;

        bool isWorkRamEnabled { get { return !IsBitSet(portMemoryControl, 4); } }
        bool isBootstrapRomEnabled { get { return !IsBitSet(portMemoryControl, 3); } }

        int currentMasterClockCyclesInFrame, totalMasterClockCyclesInFrame;

        public Configuration.GameGear configuration;

        public GameGear() { }

        public void Initialize()
        {
            bootstrap = null;
            cartridge = null;

            wram = new byte[ramSize];
            cpu = new Z80A(ReadMemory, WriteMemory, ReadPort, WritePort);
            vdp = new SegaGGVDP();
            psg = new SegaGGPSG();

            vdp.EndOfScanline += (s, e) =>
            {
                PollInputEventArgs pollInputEventArgs = PollInputEventArgs.Create();
                OnPollInput(pollInputEventArgs);
                ParseInput(pollInputEventArgs);
                pollInputEventArgs.Release();
            };
        }

        public void SetConfiguration(IConfiguration config)
        {
            configuration = (Configuration.GameGear)config;

            ReconfigureSystem();
        }

        public object GetRuntimeOption(string name)
        {
            if (name.StartsWith("Graphics"))
                return vdp.GetRuntimeOption(name);
            else if (name.StartsWith("Audio"))
                return psg.GetRuntimeOption(name);
            else
                return null;
        }

        public void SetRuntimeOption(string name, object value)
        {
            if (name.StartsWith("Graphics"))
                vdp.SetRuntimeOption(name, value);
            else if (name.StartsWith("Audio"))
                psg.SetRuntimeOption(name, value);
        }

        private void ReconfigureSystem()
        {
            vdpClock = (masterClock / 1.0);
            psgClock = (masterClock / 3.0);

            vdp?.SetClockRate(vdpClock);
            vdp?.SetRefreshRate(refreshRate);
            vdp?.SetRevision(1);

            psg?.SetSampleRate(EmuStandInfo.Configuration.SampleRate);
            psg?.SetOutputChannels(2);
            psg?.SetClockRate(psgClock);
            psg?.SetRefreshRate(refreshRate);

            currentMasterClockCyclesInFrame = 0;
            totalMasterClockCyclesInFrame = (int)Math.Round(masterClock / refreshRate);

            var eventArgs = ChangeViewportEventArgs.Create(vdp.Viewport);
            OnChangeViewport(eventArgs);
            eventArgs.Release();
        }

        private void LoadBootstrap()
        {
            if (configuration.UseBootstrap)
            {
                var (type, bootstrapRomData) = CartridgeLoader.Load(configuration.BootstrapRom, "GameGear Bootstrap");
                bootstrap = new SegaMapperCartridge(bootstrapRomData.Length, 0);
                bootstrap.LoadRom(bootstrapRomData);
            }
        }

        public void Startup()
        {
            LoadBootstrap();

            cpu.Startup();
            vdp.Startup();
            psg.Startup();
        }

        public void Reset()
        {
            cpu.Reset();
            cpu.SetStackPointer(0xDFF0);
            vdp.Reset();
            psg.Reset();

            portAInputsPressed = 0;
            portBInputsPressed = 0;
            portCInputsPressed = 0;

            portMemoryControl = (byte)(bootstrap != null ? 0xA3 : 0x00);
            portIoControl = 0x0F;
            hCounterLatched = 0x00;
            portIoAB = portIoBMisc = 0xFF;

            portIoC = (byte)(0x80 | (configuration.Region == Region.Export ? 0x40 : 0x00));
            portParallelData = 0x00;
            portDataDirNMI = 0xFF;
            portTxBuffer = 0x00;
            portRxBuffer = 0xFF;
            portSerialControl = 0x00;
            psg.WritePort(SegaGGPSG.PortStereoControl, 0xFF);

            OnEmulationReset(EventArgs.Empty);
        }

        public void Shutdown()
        {
            cpu?.Shutdown();
            vdp?.Shutdown();
            psg?.Shutdown();
        }

        //public void SetState(Dictionary<string, dynamic> state)
        //public void SetState(Dictionary<string, object> state)
        //{
        //    configuration.Region = (Region)state[nameof(configuration.Region)];

        //    SaveStateHandler.PerformSetState(bootstrap, (Dictionary<string, object>)state[nameof(bootstrap)]);
        //    SaveStateHandler.PerformSetState(cartridge, (Dictionary<string, object>)state[nameof(cartridge)]);
        //    wram = (byte[])state[nameof(wram)];
        //    SaveStateHandler.PerformSetState(cpu, (Dictionary<string, object>)state[nameof(cpu)]);
        //    SaveStateHandler.PerformSetState(vdp, (Dictionary<string, object>)state[nameof(vdp)]);
        //    SaveStateHandler.PerformSetState(psg, (Dictionary<string, object>)state[nameof(psg)]);

        //    portMemoryControl = (byte)state[nameof(portMemoryControl)];
        //    portIoControl = (byte)state[nameof(portIoControl)];
        //    hCounterLatched = (byte)state[nameof(hCounterLatched)];
        //    portIoAB = (byte)state[nameof(portIoAB)];
        //    portIoBMisc = (byte)state[nameof(portIoBMisc)];

        //    portIoC = (byte)state[nameof(portIoC)];
        //    portParallelData = (byte)state[nameof(portParallelData)];
        //    portDataDirNMI = (byte)state[nameof(portDataDirNMI)];
        //    portTxBuffer = (byte)state[nameof(portTxBuffer)];
        //    portRxBuffer = (byte)state[nameof(portRxBuffer)];
        //    portSerialControl = (byte)state[nameof(portSerialControl)];

        //    ReconfigureSystem();
        //}

        //public Dictionary<string, object> GetState()
        //{
        //    return new Dictionary<string, object>
        //    {
        //        [nameof(configuration.Region)] = configuration.Region,

        //        [nameof(bootstrap)] = SaveStateHandler.PerformGetState(bootstrap),
        //        [nameof(cartridge)] = SaveStateHandler.PerformGetState(cartridge),
        //        [nameof(wram)] = wram,
        //        [nameof(cpu)] = SaveStateHandler.PerformGetState(cpu),
        //        [nameof(vdp)] = SaveStateHandler.PerformGetState(vdp),
        //        [nameof(psg)] = SaveStateHandler.PerformGetState(psg),

        //        [nameof(portMemoryControl)] = portMemoryControl,
        //        [nameof(portIoControl)] = portIoControl,
        //        [nameof(hCounterLatched)] = hCounterLatched,
        //        [nameof(portIoAB)] = portIoAB,
        //        [nameof(portIoBMisc)] = portIoBMisc,

        //        [nameof(portIoC)] = portIoC,
        //        [nameof(portParallelData)] = portParallelData,
        //        [nameof(portDataDirNMI)] = portDataDirNMI,
        //        [nameof(portTxBuffer)] = portTxBuffer,
        //        [nameof(portRxBuffer)] = portRxBuffer,
        //        [nameof(portSerialControl)] = portSerialControl
        //    };
        //}


        public void LoadAxiStatus(AxiEssgssStatusData data)
        {
            configuration.Region = data.MemberData[nameof(configuration.Region)].ToEnum<Region>();

            if (data.ClassData.ContainsKey(nameof(bootstrap)))
                bootstrap.LoadAxiStatus(data.ClassData[nameof(bootstrap)]);

            cartridge.LoadAxiStatus(data.ClassData[nameof(cartridge)]);
            wram = data.MemberData[nameof(wram)];
            cpu.LoadAxiStatus(data.ClassData[nameof(cpu)]);
            vdp.LoadAxiStatus(data.ClassData[nameof(vdp)]);
            psg.LoadAxiStatus(data.ClassData[nameof(psg)]);

            portMemoryControl = data.MemberData[nameof(portMemoryControl)].First();
            portIoControl = data.MemberData[nameof(portIoControl)].First();
            hCounterLatched = data.MemberData[nameof(hCounterLatched)].First();
            portIoAB = data.MemberData[nameof(portIoAB)].First();
            portIoBMisc = data.MemberData[nameof(portIoBMisc)].First();

            portIoC = data.MemberData[nameof(portIoC)].First();
            portParallelData = data.MemberData[nameof(portParallelData)].First();
            portDataDirNMI = data.MemberData[nameof(portDataDirNMI)].First();
            portTxBuffer = data.MemberData[nameof(portTxBuffer)].First();
            portRxBuffer = data.MemberData[nameof(portRxBuffer)].First();
            portSerialControl = data.MemberData[nameof(portSerialControl)].First();
        }

        public AxiEssgssStatusData SaveAxiStatus()
        {
            AxiEssgssStatusData data = new AxiEssgssStatusData();
            data.MemberData[nameof(configuration.Region)] = configuration.Region.ToByteArray();

            if (bootstrap != null)
                data.ClassData[nameof(bootstrap)] = bootstrap.SaveAxiStatus();

            data.ClassData[nameof(cartridge)] = cartridge.SaveAxiStatus();
            data.MemberData[nameof(wram)] = wram;
            data.ClassData[nameof(cpu)] = cpu.SaveAxiStatus();
            data.ClassData[nameof(vdp)] = vdp.SaveAxiStatus();
            data.ClassData[nameof(psg)] = psg.SaveAxiStatus();

            data.MemberData[nameof(portMemoryControl)] = BitConverter.GetBytes(portMemoryControl);
            data.MemberData[nameof(portIoControl)] = BitConverter.GetBytes(portIoControl);
            data.MemberData[nameof(hCounterLatched)] = BitConverter.GetBytes(hCounterLatched);
            data.MemberData[nameof(portIoAB)] = BitConverter.GetBytes(portIoAB);
            data.MemberData[nameof(portIoBMisc)] = BitConverter.GetBytes(portIoBMisc);

            data.MemberData[nameof(portIoC)] = BitConverter.GetBytes(portIoC);
            data.MemberData[nameof(portParallelData)] = BitConverter.GetBytes(portParallelData);
            data.MemberData[nameof(portDataDirNMI)] = BitConverter.GetBytes(portDataDirNMI);
            data.MemberData[nameof(portTxBuffer)] = BitConverter.GetBytes(portTxBuffer);
            data.MemberData[nameof(portRxBuffer)] = BitConverter.GetBytes(portRxBuffer);
            data.MemberData[nameof(portSerialControl)] = BitConverter.GetBytes(portSerialControl);

            return data;

        }

        public Dictionary<string, object> GetDebugInformation()
        {
            var dict = new Dictionary<string, object>
            {
                { "CyclesInFrame", currentMasterClockCyclesInFrame },
            };

            return dict;
        }

        public void Load(byte[] romData, byte[] ramData, Type mapperType)
        {
            if (mapperType == null)
                mapperType = typeof(SegaMapperCartridge);
            if (ramData.Length == 0)
                ramData = new byte[32768];

            cartridge = (ICartridge)Activator.CreateInstance(mapperType, new object[] { romData.Length, ramData.Length });
            cartridge.LoadRom(romData);
            cartridge.LoadRam(ramData);
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

            currentMasterClockCyclesInFrame -= totalMasterClockCyclesInFrame;
        }

        public void RunStep()
        {
            double currentCpuClockCycles = 0.0;
            currentCpuClockCycles += cpu.Step();

            double currentMasterClockCycles = (currentCpuClockCycles * 3.0);

            vdp.Step((int)Math.Round(currentMasterClockCycles));

            cpu.SetInterruptLine(InterruptType.Maskable, vdp.InterruptLine);

            psg.Step((int)Math.Round(currentCpuClockCycles));

            cartridge?.Step((int)Math.Round(currentCpuClockCycles));

            currentMasterClockCyclesInFrame += (int)Math.Round(currentMasterClockCycles);
        }

        private void ParseInput(PollInputEventArgs eventArgs)
        {
            portAInputsPressed = 0;
            portBInputsPressed = 0;
            portCInputsPressed = 0;

            /* Keyboard */
            if (eventArgs.Keyboard.Contains(configuration.ControlsUp)) portAInputsPressed |= IOPortABInputs.PortAUp;
            if (eventArgs.Keyboard.Contains(configuration.ControlsDown)) portAInputsPressed |= IOPortABInputs.PortADown;
            if (eventArgs.Keyboard.Contains(configuration.ControlsLeft)) portAInputsPressed |= IOPortABInputs.PortALeft;
            if (eventArgs.Keyboard.Contains(configuration.ControlsRight)) portAInputsPressed |= IOPortABInputs.PortARight;
            if (eventArgs.Keyboard.Contains(configuration.ControlsButton1)) portAInputsPressed |= IOPortABInputs.PortATL;
            if (eventArgs.Keyboard.Contains(configuration.ControlsButton2)) portAInputsPressed |= IOPortABInputs.PortATR;
            if (eventArgs.Keyboard.Contains(configuration.ControlsStart)) portCInputsPressed |= IOPortCInputs.Start;

            /* XInput controller */
            //if (eventArgs.ControllerState.IsAnyUpDirectionPressed() && !eventArgs.ControllerState.IsAnyDownDirectionPressed()) portAInputsPressed |= IOPortABInputs.PortAUp;
            //if (eventArgs.ControllerState.IsAnyDownDirectionPressed() && !eventArgs.ControllerState.IsAnyUpDirectionPressed()) portAInputsPressed |= IOPortABInputs.PortADown;
            //if (eventArgs.ControllerState.IsAnyLeftDirectionPressed() && !eventArgs.ControllerState.IsAnyRightDirectionPressed()) portAInputsPressed |= IOPortABInputs.PortALeft;
            //if (eventArgs.ControllerState.IsAnyRightDirectionPressed() && !eventArgs.ControllerState.IsAnyLeftDirectionPressed()) portAInputsPressed |= IOPortABInputs.PortARight;
            //if (eventArgs.ControllerState.IsAPressed()) portAInputsPressed |= IOPortABInputs.PortATL;
            //if (eventArgs.ControllerState.IsXPressed() || eventArgs.ControllerState.IsBPressed()) portAInputsPressed |= IOPortABInputs.PortATR;
            //if (eventArgs.ControllerState.IsStartPressed()) portCInputsPressed |= IOPortCInputs.Start;

            portIoAB |= (byte)(IOPortABInputs.PortAUp | IOPortABInputs.PortADown | IOPortABInputs.PortALeft | IOPortABInputs.PortARight | IOPortABInputs.PortATL | IOPortABInputs.PortATR | IOPortABInputs.PortBUp | IOPortABInputs.PortBDown);
            portIoBMisc |= (byte)(IOPortBMiscInputs.PortBLeft | IOPortBMiscInputs.PortBRight | IOPortBMiscInputs.PortBTL | IOPortBMiscInputs.PortBTR | IOPortBMiscInputs.Reset | IOPortBMiscInputs.CartSlotCONTPin | IOPortBMiscInputs.PortATH | IOPortBMiscInputs.PortBTH);
            portIoC |= (byte)IOPortCInputs.Start;

            portIoAB &= (byte)~portAInputsPressed;
            portIoBMisc &= (byte)~portBInputsPressed;
            portIoC &= (byte)~portCInputsPressed;
        }

        private byte ReadMemory(ushort address)
        {
            if (address >= 0x0000 && address <= 0xBFFF)
            {
                if (address <= 0x0400 && isBootstrapRomEnabled && bootstrap != null)
                    return bootstrap.Read(address);

                if (cartridge != null)
                    return cartridge.Read(address);
            }
            else if (address >= 0xC000 && address <= 0xFFFF)
            {
                if (isWorkRamEnabled)
                    return wram[address & (ramSize - 1)];
            }

            /* Cannot read from address, return 0 */
            return 0x00;
        }

        private void WriteMemory(ushort address, byte value)
        {
            if (isBootstrapRomEnabled) bootstrap?.Write(address, value);
            cartridge?.Write(address, value);

            if (isWorkRamEnabled && address >= 0xC000 && address <= 0xFFFF)
                wram[address & (ramSize - 1)] = value;
        }

        private byte ReadPort(byte port)
        {
            var maskedPort = (byte)(port & 0xC1);

            switch (maskedPort & 0xF0)
            {
                case 0x00:
                    /* GG-specific ports */
                    switch (port)
                    {
                        case 0x00: return (byte)((portIoC & 0xBF) | (configuration.Region == Region.Export ? 0x40 : 0x00));
                        case 0x01: return portParallelData;
                        case 0x02: return portDataDirNMI;
                        case 0x03: return portTxBuffer;
                        case 0x04: return portRxBuffer;
                        case 0x05: return portSerialControl;
                        case 0x06: return 0xFF;
                    }
                    return 0xFF;

                case 0x40:                                  /* Counters */
                case 0x80:                                  /* VDP ports */
                    return vdp.ReadPort(maskedPort);

                case 0xC0:
                    if (port == 0xC0 || port == 0xDC)
                        return portIoAB;                    /* IO port A/B register */
                    else if (port == 0xC1 || port == 0xDD)
                        return portIoBMisc;                 /* IO port B/misc register */
                    else
                        return 0xFF;

                default:
                    // TODO: handle properly
                    return 0x00;
            }
        }

        public void WritePort(byte port, byte value)
        {
            var maskedPort = (byte)(port & 0xC1);

            switch (maskedPort & 0xF0)
            {
                case 0x00:
                    switch (port)
                    {
                        case 0x00: /* Read-only */ break;
                        case 0x01: portParallelData = value; break;
                        case 0x02: portDataDirNMI = value; break;
                        case 0x03: portTxBuffer = value; break;
                        case 0x04: /* Read-only? */; break;
                        case 0x05: portSerialControl = (byte)(value & 0xF8); break;
                        case 0x06: psg.WritePort(port, value); break;
                        default:
                            /* System stuff */
                            if ((maskedPort & 0x01) == 0)
                            {
                                /* Memory control */
                                if (configuration.AllowMemoryControl)
                                    portMemoryControl = value;
                            }
                            else
                            {
                                /* I/O control */
                                if ((portIoControl & 0x0A) == 0x00 && ((value & 0x02) == 0x02 || (value & 0x08) == 0x08))
                                    hCounterLatched = vdp.ReadPort(SegaSMSVDP.PortHCounter);
                                portIoControl = value;
                            }
                            break;
                    }
                    break;

                case 0x40:
                    /* PSG */
                    psg.WritePort(maskedPort, value);
                    break;

                case 0x80:
                    /* VDP */
                    vdp.WritePort(maskedPort, value);
                    break;

                case 0xC0:
                    /* No effect */
                    break;

                default:
                    // TODO: handle properly
                    break;
            }
        }

    }
}
