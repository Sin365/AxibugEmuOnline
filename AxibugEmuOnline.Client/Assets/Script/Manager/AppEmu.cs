using MyNes.Core;
using System.IO;
using UnityEngine;

namespace AxibugEmuOnline.Client.Manager
{
    public class AppEmu : IFileManager
    {
        public UguiVideoProvider UguiVideo { get; private set; }
        public AudioProvider Audio { get; private set; }

        public void Init()
        {
            MyNesMain.Initialize(this);
            NesEmu.LoadGame("E:/rzg4.nes", out var successed, true);
            UguiVideo = MyNesMain.VideoProvider as UguiVideoProvider;
            Audio = MyNesMain.AudioProvider as AudioProvider;

            var fps_nes_missle = 1.0 / 59.0;
            NesEmu.SetFramePeriod(ref fps_nes_missle);
        }

        public void Update()
        {
            UguiVideo.Draw();

            double t = Time.deltaTime;
            NesEmu.SetFramePeriod(ref t);
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
