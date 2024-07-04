using MyNes.Core;
using Palmmedia.ReportGenerator.Core;
using SevenZip;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;

namespace AxibugEmuOnline.Client.Manager
{
    public class AppEmu
    {
        public void Init()
        {
            MyNesMain.Initialize(setupRenderers: false);
            MyNesMain.SetVideoProvider();
            MyNesMain.SetAudioProvider();
            MyNesMain.SetRenderingMethods();
        }
    }
}
