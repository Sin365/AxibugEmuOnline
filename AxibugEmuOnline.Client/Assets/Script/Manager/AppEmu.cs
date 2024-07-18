using AxibugEmuOnline.Client.Input;
using MyNes.Core;
using System.IO;
using UnityEngine;

namespace AxibugEmuOnline.Client.Manager
{
    public class AppEmu : IExternalSupporter
    {
        private InputManager m_inputMgr;

        public void Init(IVideoProvider videoCom, IAudioProvider audioCom, InputManager inputManager)
        {
            m_inputMgr = inputManager;

            MyNesMain.Initialize(this, videoCom, audioCom);
            NesEmu.SetupControllers(
                new NesJoyController(EnumJoyIndex.P1),
                new NesJoyController(EnumJoyIndex.P2),
                new NesJoyController(EnumJoyIndex.P3),
                new NesJoyController(EnumJoyIndex.P4));

        }

        public bool LoadGame(string romName)
        {
            NesEmu.LoadGame(romName, out var successed, true);
            return successed;
        }

        public void Dispose()
        {
            MyNesMain.Shutdown();
        }

        public Stream OpenDatabaseFile()
        {
            var databaseFile = Resources.Load<TextAsset>("NesCoreRes/database");
            MemoryStream ms = new MemoryStream(databaseFile.bytes);
            return ms;
        }

        public Stream OpenPaletteFile()
        {
            var defaultPalett = Resources.Load<TextAsset>("NesCoreRes/Palettes/default_ntsc.pal");
            MemoryStream ms = new MemoryStream(defaultPalett.bytes);
            return ms;
        }

        public string GetWorkingFolderPath()
        {
            return $"{Application.persistentDataPath}/MyNes";
        }

        public Stream OpenRomFile(string path)
        {
            var ta = Resources.Load<TextAsset>($"Roms/{path}");
            MemoryStream ms = new MemoryStream(ta.bytes);
            return ms;
        }

        public bool IsKeyPressing(EnumJoyIndex index, EnumKeyKind key)
        {
            return m_inputMgr.IsKeyPress(index, key);
        }
    }
}
