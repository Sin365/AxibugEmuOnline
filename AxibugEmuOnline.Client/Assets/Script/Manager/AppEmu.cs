using MyNes;
using MyNes.Core;
using System.IO;
using UnityEngine;

namespace AxibugEmuOnline.Client.Manager
{
    public class AppEmu : IFileManager
    {
        public void Init(IVideoProvider videoCom,IAudioProvider audioCom)
        {
            MyNesMain.Initialize(this, videoCom, audioCom);
            NesEmu.LoadGame("E:/kirby.nes", out var successed, true);
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
    }
}
