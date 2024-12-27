using System;

namespace AxiReplay
{
    internal interface IReplayReader : IDisposable
    {
        bool NextFrame(out ReplayStep data);
        bool TakeFrame(int addFrame, out ReplayStep data);
        bool NextFramebyFrameIdx(int FrameID, out ReplayStep data);
    }
}
