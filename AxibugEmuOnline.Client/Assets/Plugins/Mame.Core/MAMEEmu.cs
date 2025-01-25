using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace MAME.Core
{
    public class MAMEEmu : IDisposable
    {
        public MameMainMotion mameMainMotion { get; private set; }
        //byte[] mGameTileData;
        //byte[] mtileListData;
        public MAMEEmu()
        {
            mameMainMotion = new MameMainMotion();
        }

        public bool bRom => mameMainMotion.bRom;

        public void Init(
            string RomDir,
            ILog ilog,
            IResources iRes,
            IVideoPlayer ivp,
            ISoundPlayer isp,
            IKeyboard ikb,
            IMouse imou,
            ITimeSpan itime
            ) => mameMainMotion.Init(RomDir, ilog, iRes, ivp, isp, ikb, imou, itime);

        public void ResetRomRoot(string RomDir) => mameMainMotion.ResetRomRoot(RomDir);

        public Dictionary<string, RomInfo> GetGameList() => mameMainMotion.GetGameList();
        public void LoadRom(string Name) => mameMainMotion.LoadRom(Name);
        public void GetGameScreenSize(out int _width, out int _height, out IntPtr _framePtr) => mameMainMotion.GetGameScreenSize(out _width, out _height, out _framePtr);
        public void StartGame() => mameMainMotion.StartGame();
        public void StartGame_WithNewThread() => mameMainMotion.StartGame_WithNewThread();
        public void UpdateFrame() => Mame.mame_execute_UpdateMode_NextFrame();
        public void UnlockNextFreme(int moreTick = 1) => mameMainMotion.UnlockNextFreme(moreTick);
        public void StopGame() => mameMainMotion.StopGame();
        public long currEmuFrame => Video.screenstate.frame_number;
        public bool IsPaused => Mame.paused;
        public void LoadState(BinaryReader sr)
        {
            Mame.paused = true;
            Thread.Sleep(20);
            State.loadstate_callback.Invoke(sr);
            Mame.postload();
            Thread.Sleep(20);
            Mame.paused = false;
        }

        public void SaveState(BinaryWriter sw)
        {
            Mame.paused = true;
            Thread.Sleep(20);
            State.savestate_callback.Invoke(sw);
            Thread.Sleep(20);
            Mame.paused = false;
        }

        public void Dispose()
        {
            mameMainMotion.StopGame();
            mameMainMotion = null;
            GC.Collect();
            AxiMemoryEx.FreeAllGCHandle();
        }

        public void SetPaused(bool ispaused)
        {
            Mame.paused = ispaused;
        }
    }
}
