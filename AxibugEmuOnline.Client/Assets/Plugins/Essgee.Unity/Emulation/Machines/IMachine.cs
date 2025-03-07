using Essgee.Emulation.Configuration;
using Essgee.EventArguments;
using System;
using System.Collections.Generic;

namespace Essgee.Emulation.Machines
{
    public interface IMachine : IAxiEssgssStatus
    {
        event EventHandler<SendLogMessageEventArgs> SendLogMessage;
        event EventHandler<EventArgs> EmulationReset;
        event EventHandler<RenderScreenEventArgs> RenderScreen;
        event EventHandler<SizeScreenEventArgs> SizeScreen;
        event EventHandler<ChangeViewportEventArgs> ChangeViewport;
        event EventHandler<PollInputEventArgs> PollInput;
        event EventHandler<EnqueueSamplesEventArgs> EnqueueSamples;
        event EventHandler<SaveExtraDataEventArgs> SaveExtraData;
        event EventHandler<EventArgs> EnableRumble;

        string ManufacturerName { get; }
        string ModelName { get; }
        string DatFilename { get; }
        (string Extension, string Description) FileFilter { get; }
        bool HasBootstrap { get; }
        double RefreshRate { get; }
        double PixelAspectRatio { get; }
        (string Name, string Description)[] RuntimeOptions { get; }

        Dictionary<string, object> GetDebugInformation();

        void SetConfiguration(IConfiguration config);

        object GetRuntimeOption(string name);
        void SetRuntimeOption(string name, object value);

        void Initialize();
        void Startup();
        void Reset();
        void Shutdown();

        //void SetState(Dictionary<string, object> state);
        //Dictionary<string, object> GetState();


        void Load(byte[] romData, byte[] ramData, Type mapperType);
        byte[] GetCartridgeRam();
        bool IsCartridgeRamSaveNeeded();

        void RunFrame();
    }
}
