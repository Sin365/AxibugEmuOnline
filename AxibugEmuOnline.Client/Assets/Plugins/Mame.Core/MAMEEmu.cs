using System;
using System.Collections.Generic;
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
            ITimeSpan itime,
            IMAMEIOSupport io
            ) => mameMainMotion.Init(RomDir, ilog, iRes, ivp, isp, ikb, imou, itime,io);

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
        public void LoadState(System.IO.BinaryReader sr)
        {
            //热机逻辑：主要解决NEOGEO问题，避免加入其他人房间自动联机时，加载流程，cpu一次都没执行，部分逻辑没有初始化。
            //再加载数据之前，推若干帧，确保所有组件充分初始化
            for (int i = 0; i < 5; i++)
            {
                UpdateFrame();
            }
            Mame.paused = true;
            Thread.Sleep(20);
            Mame.soft_reset();//软重启一次，确保没有脏数据
            State.loadstate_callback(sr);
            Mame.postload();
            Video.popup_text_end = Wintime.osd_ticks() + Wintime.ticks_per_second * 2;
            mameMainMotion.ResetFreameIndex();
            Thread.Sleep(20);
            Mame.paused = false;
        }

        public void SaveState(System.IO.BinaryWriter sw)
        {
            Mame.paused = true;
            Thread.Sleep(20);
            State.savestate_callback(sw);
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
