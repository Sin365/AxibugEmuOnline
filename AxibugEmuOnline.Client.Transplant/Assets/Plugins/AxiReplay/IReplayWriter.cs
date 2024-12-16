using System;

namespace AxiReplay
{
    internal interface IReplayWriter : IDisposable
    {
        void NextFrame(UInt64 frameInput);
        void NextFramebyFrameIdx(int FrameID, UInt64 frameInput);
        void TakeFrame(int addFrame, UInt64 frameInput);
        void SaveData(string path, bool bNeedDump = false, string dumpFilePath = null);
    }
}
