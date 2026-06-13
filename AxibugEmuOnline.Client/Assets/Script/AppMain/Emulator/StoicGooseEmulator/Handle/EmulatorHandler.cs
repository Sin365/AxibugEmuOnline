using StoicGoose.Core.Machines;
using StoicGooseUnity;
using System;
using System.Diagnostics;
using System.Threading;

public class EmulatorHandler
{
    readonly static string threadName = $"Unity_Emulation";

    //Thread thread = default;
    volatile bool threadRunning = false, threadPaused = false;

    volatile bool isResetRequested = false;
    volatile bool isPauseRequested = false, newPauseState = false;
    volatile bool isFpsLimiterChangeRequested = false, limitFps = true, newLimitFps = false;

    public bool IsRunning => threadRunning;
    public bool IsPaused => threadPaused;

    public MachineCommon Machine { get; } = default;
    public int AxiEmuRunFrame;

    public EmulatorHandler(Type machineType)
    {
        StoicGooseUnityAxiMem.Init();
        Machine = Activator.CreateInstance(machineType) as MachineCommon;
        Machine.Initialize();
    }

    public void Startup()
    {
        Machine.Reset();

        threadRunning = true;
        threadPaused = false;

        //thread = new Thread(ThreadMainLoop) { Name = threadName, Priority = ThreadPriority.AboveNormal, IsBackground = false };
        //thread.Start();
        AxiEmuRunFrame = 0;
    }

    public void Reset()
    {
        isResetRequested = true;
    }

    public void Pause()
    {
        isPauseRequested = true;
        newPauseState = true;
    }

    public void Unpause()
    {
        isPauseRequested = true;
        newPauseState = false;
    }

    public void Shutdown()
    {
        threadRunning = false;
        threadPaused = false;
        //thread?.Join();
        Machine.Shutdown();
        StoicGooseUnityAxiMem.FreeAllGCHandle();
    }

    public void SetFpsLimiter(bool value)
    {
        isFpsLimiterChangeRequested = true;
        newLimitFps = value;
    }

    private void ThreadMainLoop()
    {
        var stopWatch = Stopwatch.StartNew();
        var interval = 1000.0 / Machine.RefreshRate;
        var lastTime = 0.0;

        while (true)
        {
            if (!threadRunning) break;

            if (isResetRequested)
            {
                Machine.Reset();
                stopWatch.Restart();
                lastTime = 0.0;

                isResetRequested = false;
            }

            if (isPauseRequested)
            {
                threadPaused = newPauseState;
                isPauseRequested = false;
            }

            if (isFpsLimiterChangeRequested)
            {
                limitFps = newLimitFps;
                isFpsLimiterChangeRequested = false;
            }

            if (!threadPaused)
            {
                if (limitFps)
                {
                    while ((stopWatch.Elapsed.TotalMilliseconds - lastTime) < interval)
                        Thread.Sleep(0);

                    lastTime += interval;
                }
                else
                    lastTime = stopWatch.Elapsed.TotalMilliseconds;

                Machine.RunFrame();
            }
            else
                lastTime = stopWatch.Elapsed.TotalMilliseconds;
        }
    }
    long accumulatedUs = 0;
    long unityFrameUs = 16_666; // 60Hz = 16.6667ms
    public static class WSConstants
    {
        // 3.072 MHz
        public const int MASTER_CLOCK = 3_072_000;

        // 159 lines per frame
        public const int LINES_PER_FRAME = 159;

        // 256 dots per line
        public const int DOTS_PER_LINE = 256;

        // Frame time in microseconds (1ms = 1000us)
        // 13.259ms = 13259us
        public const long FRAME_TIME_US = 13_259;
    }
    public void Frame_Update()
    {
        accumulatedUs += unityFrameUs;

        int runStep = 0;

        while (accumulatedUs >= WSConstants.FRAME_TIME_US)
        {
            accumulatedUs -= WSConstants.FRAME_TIME_US;
            runStep++;
        }

        for (int i = 0; i < runStep; i++)
        {
            Machine.RunFrame();
        }
    }
}
