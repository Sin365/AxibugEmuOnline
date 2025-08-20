using Essgee.EventArguments;
using System;

namespace Essgee.Emulation.Video
{
    interface IVideo : IAxiEssgssStatus
    {
        (int X, int Y, int Width, int Height) Viewport { get; }

        event EventHandler<RenderScreenEventArgs> RenderScreen;
        void OnRenderScreen(RenderScreenEventArgs e);

        event EventHandler<EventArgs> EndOfScanline;
        void OnEndOfScanline(EventArgs e);

        event EventHandler<SizeScreenEventArgs> SizeScreen;
        void OnSizeScreen(SizeScreenEventArgs e);

        (string Name, string Description)[] RuntimeOptions { get; }

        object GetRuntimeOption(string name);
        void SetRuntimeOption(string name, object value);

        void Startup();
        void Shutdown();
        void Reset();
        void Step(int clockCyclesInStep);

        void SetClockRate(double clock);
        void SetRefreshRate(double refresh);
    }
}
