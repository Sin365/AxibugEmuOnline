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

namespace Essgee.Emulation.Machines
{
    [MachineIndex(0)]
    public class SG1000 : IMachine
    {
        // TODO: verify port 0xC0-0xFF behavior wrt the lack of a PPI and the SG-1000 Test Cartridge Extension Port test?

        const double masterClockNtsc = 10738635;
        const double masterClockPal = 10640684;
        const double refreshRateNtsc = 59.922743;
        const double refreshRatePal = 49.701459;

        const int ramSize = 1 * 1024;

        double masterClock;
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
        public string ModelName => "SG-1000";
        public string DatFilename => "Sega - SG-1000.dat";
        public (string Extension, string Description) FileFilter => (".sg", "SG-1000 ROMs");
        public bool HasBootstrap => false;
        public double RefreshRate { get; private set; }
        public double PixelAspectRatio => 8.0 / 7.0;
        public (string Name, string Description)[] RuntimeOptions => vdp.RuntimeOptions.Concat(psg.RuntimeOptions).ToArray();

        ICartridge cartridge;
        byte[] wram;
        Z80A cpu;
        TMS99xxA vdp;
        SN76489 psg;

        [Flags]
        enum PortIoABValues : byte
        {
            P1Up = 0b00000001,
            P1Down = 0b00000010,
            P1Left = 0b00000100,
            P1Right = 0b00001000,
            P1Button1 = 0b00010000,
            P1Button2 = 0b00100000,
            P2Up = 0b01000000,
            P2Down = 0b10000000,
            Mask = 0b11111111
        }

        [Flags]
        enum PortIoBMiscValues : byte
        {
            P2Left = 0b00000001,
            P2Right = 0b00000010,
            P2Button1 = 0b00000100,
            P2Button2 = 0b00001000,
            CON = 0b00010000,
            IC21Pin6 = 0b00100000,
            IC21Pin10 = 0b01000000,
            IC21Pin13 = 0b10000000,
            Mask = 0b11111111
        }

        PortIoABValues portIoABPressed;
        PortIoBMiscValues portIoBMiscPressed;

        bool pauseButtonPressed, pauseButtonToggle;

        int currentMasterClockCyclesInFrame, totalMasterClockCyclesInFrame;

        public Configuration.SG1000 configuration;

        public SG1000() { }

        public void Initialize()
        {
            cartridge = null;
            wram = new byte[ramSize];
            cpu = new Z80A(ReadMemory, WriteMemory, ReadPort, WritePort);
            vdp = new TMS99xxA();
            psg = new SN76489();

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
            configuration = (Configuration.SG1000)config;

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
            if (configuration.TVStandard == TVStandard.NTSC)
            {
                masterClock = masterClockNtsc;
                RefreshRate = refreshRateNtsc;
            }
            else
            {
                masterClock = masterClockPal;
                RefreshRate = refreshRatePal;
            }

            vdpClock = (masterClock / 1.0);
            psgClock = (masterClock / 3.0);

            vdp?.SetClockRate(vdpClock);
            vdp?.SetRefreshRate(RefreshRate);
            vdp?.SetRevision(0);

            psg?.SetSampleRate(EmuStandInfo.Configuration.SampleRate);
            psg?.SetOutputChannels(2);
            psg?.SetClockRate(psgClock);
            psg?.SetRefreshRate(RefreshRate);

            currentMasterClockCyclesInFrame = 0;
            totalMasterClockCyclesInFrame = (int)Math.Round(masterClock / RefreshRate);

            var eventArgs = ChangeViewportEventArgs.Create(vdp.Viewport);
            OnChangeViewport(eventArgs);
            eventArgs.Release();

        }

        public void Startup()
        {
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

            portIoABPressed = 0;
            portIoBMiscPressed = 0;

            pauseButtonPressed = pauseButtonToggle = false;

            OnEmulationReset(EventArgs.Empty);
        }

        public void Shutdown()
        {
            cpu?.Shutdown();
            vdp?.Shutdown();
            psg?.Shutdown();
        }

        ////public void SetState(Dictionary<string, dynamic> state)
        //public void SetState(Dictionary<string, object> state)
        //{
        //    configuration.TVStandard = (TVStandard)state[nameof(configuration.TVStandard)];

        //    SaveStateHandler.PerformSetState(cartridge, (Dictionary<string, object>)state[nameof(cartridge)]);
        //    wram = (byte[])state[nameof(wram)];
        //    SaveStateHandler.PerformSetState(cpu, (Dictionary<string, object>)state[nameof(cpu)]);
        //    SaveStateHandler.PerformSetState(vdp, (Dictionary<string, object>)state[nameof(vdp)]);
        //    SaveStateHandler.PerformSetState(psg, (Dictionary<string, object>)state[nameof(psg)]);

        //    ReconfigureSystem();
        //}

        //public Dictionary<string, object> GetState()
        //{
        //    return new Dictionary<string, object>
        //    {
        //        [nameof(configuration.TVStandard)] = configuration.TVStandard,

        //        [nameof(cartridge)] = SaveStateHandler.PerformGetState(cartridge),
        //        [nameof(wram)] = wram,
        //        [nameof(cpu)] = SaveStateHandler.PerformGetState(cpu),
        //        [nameof(vdp)] = SaveStateHandler.PerformGetState(vdp),
        //        [nameof(psg)] = SaveStateHandler.PerformGetState(psg)
        //    };
        //}

        #region
        public void LoadAxiStatus(AxiEssgssStatusData data)
        {
            configuration.TVStandard = data.MemberData[nameof(configuration.TVStandard)].ToEnum<TVStandard>();

            cartridge.LoadAxiStatus(data.ClassData[nameof(cartridge)]);
            wram = data.MemberData[nameof(wram)];
            cpu.LoadAxiStatus(data.ClassData[nameof(cpu)]);
            vdp.LoadAxiStatus(data.ClassData[nameof(vdp)]);
            psg.LoadAxiStatus(data.ClassData[nameof(psg)]);

        }

        public AxiEssgssStatusData SaveAxiStatus()
        {
            AxiEssgssStatusData data = new AxiEssgssStatusData();
            data.MemberData[nameof(configuration.TVStandard)] = configuration.TVStandard.ToByteArray();

            data.ClassData[nameof(cartridge)] = cartridge.SaveAxiStatus();
            data.MemberData[nameof(wram)] = wram;
            data.ClassData[nameof(cpu)] = cpu.SaveAxiStatus();
            data.ClassData[nameof(vdp)] = vdp.SaveAxiStatus();
            data.ClassData[nameof(psg)] = psg.SaveAxiStatus();

            return data;

        }
        #endregion

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
                mapperType = typeof(SegaSGCartridge);

            cartridge = (ICartridge)Activator.CreateInstance(mapperType, new object[] { romData.Length, ramData.Length });
            cartridge.LoadRom(romData);
            cartridge.LoadRam(ramData);
        }

        public byte[] GetCartridgeRam()
        {
            return cartridge.GetRamData();
        }

        public bool IsCartridgeRamSaveNeeded()
        {
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

            if (pauseButtonPressed)
            {
                pauseButtonPressed = false;
                cpu.SetInterruptLine(InterruptType.NonMaskable, InterruptState.Assert);
            }

            cpu.SetInterruptLine(InterruptType.Maskable, vdp.InterruptLine);

            psg.Step((int)Math.Round(currentCpuClockCycles));

            cartridge?.Step((int)Math.Round(currentCpuClockCycles));

            currentMasterClockCyclesInFrame += (int)Math.Round(currentMasterClockCycles);
        }

        private void ParseInput(PollInputEventArgs eventArgs)
        {
            /* Get variables */
            var keysDown = eventArgs.Keyboard;

            /* Handle Pause button */
            var pausePressed = keysDown.Contains(configuration.InputPause);
            var pauseButtonHeld = (pauseButtonToggle && pausePressed);
            if (pausePressed)
            {
                if (!pauseButtonHeld) pauseButtonPressed = true;
                pauseButtonToggle = true;
            }
            else if (pauseButtonToggle)
                pauseButtonToggle = false;

            /* Handle controllers */
            portIoABPressed = 0;
            portIoBMiscPressed = 0;

            /* Keyboard */
            if (keysDown.Contains(configuration.Joypad1Up)) portIoABPressed |= PortIoABValues.P1Up;
            if (keysDown.Contains(configuration.Joypad1Down)) portIoABPressed |= PortIoABValues.P1Down;
            if (keysDown.Contains(configuration.Joypad1Left)) portIoABPressed |= PortIoABValues.P1Left;
            if (keysDown.Contains(configuration.Joypad1Right)) portIoABPressed |= PortIoABValues.P1Right;
            if (keysDown.Contains(configuration.Joypad1Button1)) portIoABPressed |= PortIoABValues.P1Button1;
            if (keysDown.Contains(configuration.Joypad1Button2)) portIoABPressed |= PortIoABValues.P1Button2;
            if (keysDown.Contains(configuration.Joypad2Up)) portIoABPressed |= PortIoABValues.P2Up;
            if (keysDown.Contains(configuration.Joypad2Down)) portIoABPressed |= PortIoABValues.P2Down;
            if (keysDown.Contains(configuration.Joypad2Left)) portIoBMiscPressed |= PortIoBMiscValues.P2Left;
            if (keysDown.Contains(configuration.Joypad2Right)) portIoBMiscPressed |= PortIoBMiscValues.P2Right;
            if (keysDown.Contains(configuration.Joypad2Button1)) portIoBMiscPressed |= PortIoBMiscValues.P2Button1;
            if (keysDown.Contains(configuration.Joypad2Button2)) portIoBMiscPressed |= PortIoBMiscValues.P2Button2;

            /* XInput controller */
            //if (eventArgs.ControllerState.IsAnyUpDirectionPressed() && !eventArgs.ControllerState.IsAnyDownDirectionPressed()) portIoABPressed |= PortIoABValues.P1Up;
            //if (eventArgs.ControllerState.IsAnyDownDirectionPressed() && !eventArgs.ControllerState.IsAnyUpDirectionPressed()) portIoABPressed |= PortIoABValues.P1Down;
            //if (eventArgs.ControllerState.IsAnyLeftDirectionPressed() && !eventArgs.ControllerState.IsAnyRightDirectionPressed()) portIoABPressed |= PortIoABValues.P1Left;
            //if (eventArgs.ControllerState.IsAnyRightDirectionPressed() && !eventArgs.ControllerState.IsAnyLeftDirectionPressed()) portIoABPressed |= PortIoABValues.P1Right;
            //if (eventArgs.ControllerState.IsAPressed()) portIoABPressed |= PortIoABValues.P1Button1;
            //if (eventArgs.ControllerState.IsXPressed() || eventArgs.ControllerState.IsBPressed()) portIoABPressed |= PortIoABValues.P1Button2;

            portIoBMiscPressed |= (PortIoBMiscValues.IC21Pin6 | PortIoBMiscValues.IC21Pin10 | PortIoBMiscValues.IC21Pin13);       /* Unused, always 1 */
        }

        private byte ReadMemory(ushort address)
        {
            if (address >= 0x0000 && address <= 0xBFFF)
            {
                return (cartridge != null ? cartridge.Read(address) : (byte)0x00);
            }
            else if (address >= 0xC000 && address <= 0xFFFF)
            {
                return wram[address & (ramSize - 1)];
            }

            /* Cannot read from address, return 0 */
            return 0x00;
        }

        private void WriteMemory(ushort address, byte value)
        {
            if (address >= 0x0000 && address <= 0xBFFF)
            {
                cartridge?.Write(address, value);
            }
            else if (address >= 0xC000 && address <= 0xFFFF)
            {
                wram[address & (ramSize - 1)] = value;
            }
        }

        private byte ReadPort(byte port)
        {
            switch (port & 0xC0)
            {
                case 0x80:
                    return vdp.ReadPort(port);

                case 0xC0:
                    if ((port & 0x01) == 0)
                        return (byte)(PortIoABValues.Mask & ~portIoABPressed);
                    else
                        return (byte)(PortIoBMiscValues.Mask & ~portIoBMiscPressed);

                default:
                    // TODO: handle properly
                    return 0x00;
            }
        }

        public void WritePort(byte port, byte value)
        {
            switch (port & 0xC0)
            {
                case 0x40: psg.WritePort(port, value); break;
                case 0x80: vdp.WritePort(port, value); break;
                default: break; // TODO: handle properly
            }
        }
    }
}
