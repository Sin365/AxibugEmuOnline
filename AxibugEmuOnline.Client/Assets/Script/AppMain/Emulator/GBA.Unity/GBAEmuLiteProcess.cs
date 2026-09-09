
using AxibugEmuOnline.Client.ClientCore;
using OptimeGBA;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

namespace AxibugEmuOnline.Client.GBA.Unity
{
    public class GBAEmuLiteProcess : AxiGbaIO
    {
        public Gba gba;
        private bool RomLoaded;
        public void LoadRom(byte[] rom,byte[] savedata)
        {
            LoadSaveFileToCache(savedata);
            byte[] sav;
            if (cache_had_savedata)
            {
                sav = cache_savdata;
                App.log.Info($"save exists, loaded");
            }
            else
            {
                sav = new byte[0];
                App.log.Info(".sav not available");
            }
            LoadRomAndSave(rom, sav);
            App.log.Info("Load Rom Success");
            RomLoaded = true;
        }
        public void LoadRomAndSave(byte[] rom, byte[] sav)
        {
            byte[] bios = Resources.Load<TextAsset>("GBA.Unity/gba_bios.bin").bytes;
            App.log.Debug(bios.Length.ToString());
            gba = new Gba(new ProviderGba(bios, rom, string.Empty, AudioReady, this) { BootBios = GBAEmulator.instance.BootBIOS });
            gba.Mem.SaveProvider.LoadSave(sav);
        }

        #region SaveDataCache

        byte[] cache_savdata;
        bool cache_had_savedata => cache_savdata != null;

        void LoadSaveFileToCache(byte[] savedata)
        {
            cache_savdata = savedata;
        }

        public void DumpSavDataToReady()
        {
            cache_savdata = gba.Mem.SaveProvider.GetSave();
        }

        void ReleaseCache()
        {
            cache_savdata = null;
        }

        public bool SavFileExists()
        {
            return cache_had_savedata;
        }

        public long GetSavFileLength()
        {
            if (!cache_had_savedata)
                return -1;
            return cache_savdata.LongLength;
        }
        #endregion


        #region Audio

        public void AudioReady(float[] data)
        {
        }
        #endregion

        #region Video
        public RawImage m_drawCanvas;
        uint[] wrapTexBuffer = new uint[240 * 160];
        private IntPtr wrapTexBufferPointer;
        public Texture2D wrapTex;
        private int TexBufferSize;
        public void VideoInit(RawImage drawCanvas)
        {
            m_drawCanvas = drawCanvas;
            if (wrapTex == null)
            {
                wrapTex = new Texture2D(240, 160, TextureFormat.RGBA32, false);
                wrapTex.filterMode = FilterMode.Point;
                // 固定数组，防止垃圾回收器移动它  
                GCHandle handle = GCHandle.Alloc(wrapTexBuffer, GCHandleType.Pinned);
                // 获取数组的指针  
                wrapTexBufferPointer = handle.AddrOfPinnedObject();
                m_drawCanvas.texture = wrapTex;
                TexBufferSize = wrapTexBuffer.Length * 4;
            }
        }

        public unsafe void OnRenderFrame()
        {
            if (!gba.EnablePpuRender) return;
            var buf = GBAEmulator.instance.ShowBackBuf ? gba.Ppu.Renderer.ScreenBack : gba.Ppu.Renderer.ScreenFront;
            unsafe
            {
                for (uint i = 0; i < 240 * 160; i++)
                {
                    wrapTexBuffer[i] = PpuRenderer.ColorLutCorrected[buf[i] & 0x7FFF];
                }
            }

            wrapTex.LoadRawTextureData(wrapTexBufferPointer, TexBufferSize);
            wrapTex.Apply();
        }
        #endregion


        #region Input

        public GBAKeyCode DoLocalInputToTest()
        {
            GBAKeyCode tmpCurr = GBAKeyCode.None;
            for (int i = 0; i < GBAInputProvider.CheckList.Length; i++)
            {
                GBAKeyCode key = (GBAKeyCode)GBAInputProvider.CheckList[i];
                bool press = false;
                switch (key)
                {
                    case GBAKeyCode.A: if (Input.GetKey(KeyCode.Keypad2)) tmpCurr |= key; break;
                    case GBAKeyCode.B: if (Input.GetKey(KeyCode.Keypad1)) tmpCurr |= key; break;
                    case GBAKeyCode.L: if (Input.GetKey(KeyCode.Keypad4)) tmpCurr |= key; break;
                    case GBAKeyCode.R: if (Input.GetKey(KeyCode.Keypad5)) tmpCurr |= key; break;
                    case GBAKeyCode.Select: if (Input.GetKey(KeyCode.KeypadPlus)) tmpCurr |= key; break;
                    case GBAKeyCode.Start: if (Input.GetKey(KeyCode.KeypadEnter)) tmpCurr |= key; break;
                    case GBAKeyCode.Right: if (Input.GetKey(KeyCode.RightArrow)) tmpCurr |= key; break;
                    case GBAKeyCode.Left: if (Input.GetKey(KeyCode.LeftArrow)) tmpCurr |= key; break;
                    case GBAKeyCode.Up: if (Input.GetKey(KeyCode.UpArrow)) tmpCurr |= key; break;
                    case GBAKeyCode.Down: if (Input.GetKey(KeyCode.DownArrow)) tmpCurr |= key; break;
                }
            }
            return tmpCurr;
        }
        #endregion
    }
}
