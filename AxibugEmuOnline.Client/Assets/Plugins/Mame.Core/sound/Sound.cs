using MAME.Core;
using System;

namespace MAME.Core
{
    public unsafe partial class Sound
    {
        public static EmuTimer.emu_timer sound_update_timer;
        private static int[] leftmix, rightmix;
        private static byte[] finalmixb;
        private static int sound_muted;
        public static ushort[] latched_value, utempdata;
        public static Action sound_update;
        //public static SecondaryBuffer buf2;
        private static int stream_buffer_in;

        #region 抽象出去
        static Action<int, byte[]> Act_BufferWirte;
        static Action<int> Act_SetVolume;
        static Action<byte[], int> Act_SubmitSamples;
        public delegate void DGetCurrentPosition(out int play_position, out int write_position);
        public static event DGetCurrentPosition Act_DGetCurrentPosition;

        public static void BindFunc(ISoundPlayer Isp)
        {
            Act_BufferWirte -= Act_BufferWirte;
            Act_SetVolume -= Act_SetVolume;
            Act_SubmitSamples -= Act_SubmitSamples;
            Act_DGetCurrentPosition -= Act_DGetCurrentPosition;


            Act_BufferWirte += Isp.BufferWirte;
            Act_SetVolume += Isp.SetVolume;
            Act_SubmitSamples += Isp.SubmitSamples;
            Act_DGetCurrentPosition += Isp.GetCurrentPosition;
        }

        static void BufferWirte(int Off, byte[] Data)
        {
            Act_BufferWirte.Invoke(Off, Data);
        }

        static void SubmitSamples(byte[] buffer, int samples_a)
        {
            Act_SubmitSamples.Invoke(buffer, samples_a);
        }

        public static void SetVolume(int Vol)
        {
            Act_SetVolume.Invoke(Vol);
        }

        static void GetCurrentPosition(out int play_position, out int write_position)
        {
            play_position = 0;
            write_position = 0;
            Act_DGetCurrentPosition?.Invoke(out play_position, out write_position);
        }
        #endregion

        public static void sound_init()
        {
            leftmix = new int[0x3c0];
            rightmix = new int[0x3c0];
            finalmixb = new byte[0xf00];
            sound_muted = 0;
            //buf2.Play(0, BufferPlayFlags.Looping);
            last_update_second = 0;
            //WavWrite.CreateSoundFile(@"\VS2008\compare1\compare1\bin\Debug\2.wav");
            Atime update_frequency = new Atime(0, Attotime.ATTOSECONDS_PER_SECOND / 50);
            switch (Machine.sBoard)
            {
                case "CPS-1":
                    latched_value = new ushort[2];
                    utempdata = new ushort[2];
                    sound_update = sound_updateC;
                    //sound_update_timer = EmuTimer.timer_alloc_common(EmuTimer.TIME_ACT.Sound_sound_update, false);
                    EmuTimer.timer_alloc_common(ref sound_update_timer, EmuTimer.TIME_ACT.Sound_sound_update, false);
                    YM2151.ym2151_init(3579545);
                    OKI6295.okim6295_start();
                    ym2151stream = new sound_stream(55930, 0, 2, YM2151.ym2151_update_one);
                    okistream = new sound_stream(1000000 / 132, 0, 1, OKI6295.okim6295_update);
                    mixerstream = new sound_stream(48000, 3, 0, null);
                    break;
                case "CPS-1(QSound)":
                case "CPS2":
                    sound_update = sound_updateQ;
                    //sound_update_timer = EmuTimer.timer_alloc_common(EmuTimer.TIME_ACT.Sound_sound_update, false);
                    EmuTimer.timer_alloc_common(ref sound_update_timer, EmuTimer.TIME_ACT.Sound_sound_update, false);
                    QSound.qsound_start();
                    qsoundstream = new sound_stream(4000000 / 166, 0, 2, QSound.qsound_update);
                    mixerstream = new sound_stream(48000, 2, 0, null);
                    break;
                case "Data East":
                    latched_value = new ushort[1];
                    utempdata = new ushort[1];
                    sound_update = sound_updateDataeast_pcktgal;
                    //sound_update_timer = EmuTimer.timer_alloc_common(EmuTimer.TIME_ACT.Sound_sound_update, false);
                    EmuTimer.timer_alloc_common(ref sound_update_timer, EmuTimer.TIME_ACT.Sound_sound_update, false);
                    YM2203.ym2203_start(0, 1500000);
                    YM3812.ym3812_start(3000000);
                    MSM5205.msm5205_start(0, 384000, Dataeast.pcktgal_adpcm_int, 5);
                    ym3812stream = new sound_stream(41666, 0, 1, FMOpl.ym3812_update_one);
                    mixerstream = new sound_stream(48000, 6, 0, null);
                    break;
                case "Tehkan":
                    latched_value = new ushort[1];
                    utempdata = new ushort[1];
                    sound_update = sound_updateTehkan_pbaction;
                    //sound_update_timer = EmuTimer.timer_alloc_common(EmuTimer.TIME_ACT.Sound_sound_update, false);
                    EmuTimer.timer_alloc_common(ref sound_update_timer, EmuTimer.TIME_ACT.Sound_sound_update, false);
                    AY8910.ay8910_interface generic_ay8910 = new AY8910.ay8910_interface();
                    generic_ay8910.flags = 1;
                    generic_ay8910.res_load = new int[3] { 1000, 1000, 1000 };
                    generic_ay8910.portAread = null;
                    generic_ay8910.portBread = null;
                    generic_ay8910.portAwrite = null;
                    generic_ay8910.portBwrite = null;
                    AY8910.ay8910_start_ym(6, 0, 1500000, generic_ay8910);
                    AY8910.ay8910_start_ym(6, 1, 1500000, generic_ay8910);
                    AY8910.ay8910_start_ym(6, 2, 1500000, generic_ay8910);
                    mixerstream = new sound_stream(48000, 9, 0, null);
                    break;
                case "Neo Geo":
                    latched_value = new ushort[2];
                    utempdata = new ushort[2];
                    sound_update = sound_updateN;
                    EmuTimer.timer_alloc_common(ref sound_update_timer, EmuTimer.TIME_ACT.Sound_sound_update, false);
                    YM2610.ym2610_start(8000000);
                    ym2610stream = new sound_stream(111111, 0, 2, YM2610.F2610.ym2610_update_one);
                    mixerstream = new sound_stream(48000, 3, 0, null);
                    break;
                case "SunA8":
                    latched_value = new ushort[2];
                    utempdata = new ushort[2];
                    sound_update = sound_updateSunA8;
                    EmuTimer.timer_alloc_common(ref sound_update_timer, EmuTimer.TIME_ACT.Sound_sound_update, false);
                    YM3812.ym3812_start(4000000);
                    AY8910.ay8910_interface starfigh_ay8910_interface = new AY8910.ay8910_interface();
                    starfigh_ay8910_interface.flags = 1;
                    starfigh_ay8910_interface.res_load = new int[3] { 1000, 1000, 1000 };
                    starfigh_ay8910_interface.portAread = null;
                    starfigh_ay8910_interface.portBread = null;
                    starfigh_ay8910_interface.portAwrite = SunA8.suna8_play_samples_w;
                    starfigh_ay8910_interface.portBwrite = SunA8.suna8_samples_number_w;
                    Sample.samples_start();
                    ym3812stream = new sound_stream(55555, 0, 1, FMOpl.ym3812_update_one);
                    AY8910.ay8910_start_ym(6, 0, 1500000, starfigh_ay8910_interface);
                    samplestream = new sound_stream(48000, 0, 1, Sample.sample_update_sound);
                    mixerstream = new sound_stream(48000, 5, 0, null);
                    break;
                case "Namco System 1":
                    sound_update = sound_updateNa;
                    EmuTimer.timer_alloc_common(ref sound_update_timer, EmuTimer.TIME_ACT.Sound_sound_update, false);
                    YM2151.ym2151_init(3579580);
                    Namco.namco_start();
                    DAC.dac_start();
                    ym2151stream = new sound_stream(55930, 0, 2, YM2151.ym2151_update_one);
                    namcostream = new sound_stream(192000, 0, 2, Namco.namco_update_stereo);
                    dacstream = new sound_stream(192000, 0, 1, DAC.DAC_update);
                    mixerstream = new sound_stream(48000, 5, 0, null);
                    break;
                case "IGS011":
                    switch (Machine.sName)
                    {
                        case "drgnwrld":
                        case "drgnwrldv30":
                        case "drgnwrldv21":
                        case "drgnwrldv21j":
                        case "drgnwrldv20j":
                        case "drgnwrldv10c":
                        case "drgnwrldv11h":
                        case "drgnwrldv40k":
                            sound_update = sound_updateIGS011_drgnwrld;
                            EmuTimer.timer_alloc_common(ref sound_update_timer, EmuTimer.TIME_ACT.Sound_sound_update, false);
                            OKI6295.okim6295_start();
                            YM3812.ym3812_start(3579545);
                            okistream = new sound_stream(1047600 / 132, 0, 1, OKI6295.okim6295_update);
                            ym3812stream = new sound_stream(49715, 0, 1, FMOpl.ym3812_update_one);
                            mixerstream = new sound_stream(48000, 2, 0, null);
                            break;
                        case "lhb":
                        case "lhbv33c":
                        case "dbc":
                        case "ryukobou":
                        case "xymg":
                        case "wlcc":
                            sound_update = sound_updateIGS011_lhb;
                            EmuTimer.timer_alloc_common(ref sound_update_timer, EmuTimer.TIME_ACT.Sound_sound_update, false);
                            OKI6295.okim6295_start();
                            okistream = new sound_stream(1047600 / 132, 0, 1, OKI6295.okim6295_update);
                            mixerstream = new sound_stream(48000, 1, 0, null);
                            break;
                        case "lhb2":
                        case "nkishusp":
                            sound_update = sound_updateIGS011_lhb2;
                            EmuTimer.timer_alloc_common(ref sound_update_timer, EmuTimer.TIME_ACT.Sound_sound_update, false);
                            OKI6295.okim6295_start();
                            YM2413.ym2413_start(3579545);
                            okistream = new sound_stream(1047600 / 132, 0, 1, OKI6295.okim6295_update);
                            ym2413stream = new sound_stream(49715, 0, 2, YM2413.ym2413_update_one);
                            mixerstream = new sound_stream(48000, 3, 0, null);
                            break;
                        case "vbowl":
                        case "vbowlj":
                            sound_update = sound_updateIGS011_vbowl;
                            EmuTimer.timer_alloc_common(ref sound_update_timer, EmuTimer.TIME_ACT.Sound_sound_update, false);
                            ICS2115.ics2115_start();
                            ics2115stream = new sound_stream(33075, 0, 2, ICS2115.ics2115_update);
                            mixerstream = new sound_stream(48000, 2, 0, null);
                            break;
                    }
                    break;
                case "PGM":
                    latched_value = new ushort[3];
                    utempdata = new ushort[3];
                    sound_update = sound_updatePGM;
                    EmuTimer.timer_alloc_common(ref sound_update_timer, EmuTimer.TIME_ACT.Sound_sound_update, false);
                    ICS2115.ics2115_start();
                    ics2115stream = new sound_stream(33075, 0, 2, ICS2115.ics2115_update);
                    mixerstream = new sound_stream(48000, 2, 0, null);
                    break;
                case "M72":
                    latched_value = new ushort[1];
                    utempdata = new ushort[1];
                    sound_update = sound_updateM72;
                    EmuTimer.timer_alloc_common(ref sound_update_timer, EmuTimer.TIME_ACT.Sound_sound_update, false);
                    YM2151.ym2151_init(3579545);
                    DAC.dac_start();
                    ym2151stream = new sound_stream(55930, 0, 2, YM2151.ym2151_update_one);
                    dacstream = new sound_stream(192000, 0, 1, DAC.DAC_update);
                    mixerstream = new sound_stream(48000, 3, 0, null);
                    break;
                case "M92":
                    latched_value = new ushort[1];
                    utempdata = new ushort[1];
                    sound_update = sound_updateM92;
                    EmuTimer.timer_alloc_common(ref sound_update_timer, EmuTimer.TIME_ACT.Sound_sound_update, false);
                    YM2151.ym2151_init(3579545);
                    Iremga20.iremga20_start();
                    ym2151stream = new sound_stream(55930, 0, 2, YM2151.ym2151_update_one);
                    iremga20stream = new sound_stream(894886, 0, 2, Iremga20.iremga20_update);
                    mixerstream = new sound_stream(48000, 4, 0, null);
                    break;
                case "Taito":
                    switch (Machine.sName)
                    {
                        case "tokio":
                        case "tokioo":
                        case "tokiou":
                        case "tokiob":
                            latched_value = new ushort[2];
                            utempdata = new ushort[2];
                            sound_update = sound_updateTaito_tokio;
                            EmuTimer.timer_alloc_common(ref sound_update_timer, EmuTimer.TIME_ACT.Sound_sound_update, false);
                            YM2203.ym2203_start(0, 3000000);
                            mixerstream = new sound_stream(48000, 4, 0, null);
                            break;
                        case "bublbobl":
                        case "bublbobl1":
                        case "bublboblr":
                        case "bublboblr1":
                        case "boblbobl":
                        case "sboblbobl":
                        case "sboblbobla":
                        case "sboblboblb":
                        case "sboblbobld":
                        case "sboblboblc":
                        case "bub68705":
                        case "dland":
                        case "bbredux":
                        case "bublboblb":
                        case "bublcave":
                        case "boblcave":
                        case "bublcave11":
                        case "bublcave10":
                            latched_value = new ushort[2];
                            utempdata = new ushort[2];
                            sound_update = sound_updateTaito_bublbobl;
                            EmuTimer.timer_alloc_common(ref sound_update_timer, EmuTimer.TIME_ACT.Sound_sound_update, false);
                            YM2203.ym2203_start(0, 3000000);
                            YM3812.ym3526_start(3000000);
                            ym3526stream = new sound_stream(41666, 0, 1, FMOpl.ym3526_update_one);
                            mixerstream = new sound_stream(48000, 5, 0, null);
                            break;
                        case "opwolf":
                        case "opwolfa":
                        case "opwolfj":
                        case "opwolfu":
                        case "opwolfb":
                        case "opwolfp":
                            latched_value = new ushort[1];
                            utempdata = new ushort[1];
                            sound_update = sound_updateTaito_opwolf;
                            EmuTimer.timer_alloc_common(ref sound_update_timer, EmuTimer.TIME_ACT.Sound_sound_update, false);
                            YM2151.ym2151_init(4000000);
                            ym2151stream = new sound_stream(62500, 0, 2, YM2151.ym2151_update_one);
                            MSM5205.msm5205_start(0, 384000, Taito.opwolf_msm5205_vck, 5);
                            MSM5205.msm5205_start(1, 384000, Taito.opwolf_msm5205_vck, 5);
                            mixerstream = new sound_stream(48000, 4, 0, null);
                            break;
                    }
                    break;
                case "Taito B":
                    latched_value = new ushort[2];
                    utempdata = new ushort[2];
                    YM2610.ym2610_start(8000000);
                    switch (Machine.sName)
                    {
                        case "pbobble":
                            ym2610stream = new sound_stream(111111, 0, 2, YM2610.F2610.ym2610b_update_one);
                            break;
                        case "silentd":
                        case "silentdj":
                        case "silentdu":
                            ym2610stream = new sound_stream(111111, 0, 2, YM2610.F2610.ym2610_update_one);
                            break;
                    }
                    AY8910.AA8910[0].stream.gain = 0x100;
                    ym2610stream.gain = 0x100;
                    sound_update = sound_updateTaitoB;
                    EmuTimer.timer_alloc_common(ref sound_update_timer, EmuTimer.TIME_ACT.Sound_sound_update, false);
                    mixerstream = new sound_stream(48000, 3, 0, null);
                    break;
                case "Konami 68000":
                    switch (Machine.sName)
                    {
                        case "cuebrick":
                            YM2151.ym2151_init(3579545);
                            ym2151stream = new sound_stream(55930, 0, 2, YM2151.ym2151_update_one);
                            sound_update = sound_updateKonami68000_cuebrick;
                            mixerstream = new sound_stream(48000, 2, 0, null);
                            break;
                        case "mia":
                        case "mia2":
                            latched_value = new ushort[1];
                            utempdata = new ushort[1];
                            YM2151.ym2151_init(3579545);
                            K007232.k007232_start(3579545);
                            ym2151stream = new sound_stream(55930, 0, 2, YM2151.ym2151_update_one);
                            k007232stream = new sound_stream(27965, 0, 2, K007232.KDAC_A_update);
                            sound_update = sound_updateKonami68000_mia;
                            mixerstream = new sound_stream(48000, 4, 0, null);
                            break;
                        case "tmnt":
                        case "tmntu":
                        case "tmntua":
                        case "tmntub":
                        case "tmht":
                        case "tmhta":
                        case "tmhtb":
                        case "tmntj":
                        case "tmnta":
                        case "tmht2p":
                        case "tmht2pa":
                        case "tmnt2pj":
                        case "tmnt2po":
                            latched_value = new ushort[1];
                            utempdata = new ushort[1];
                            YM2151.ym2151_init(3579545);
                            K007232.k007232_start(3579545);
                            Upd7759.upd7759_start(640000);
                            Sample.samples_start();
                            ym2151stream = new sound_stream(55930, 0, 2, YM2151.ym2151_update_one);
                            k007232stream = new sound_stream(27965, 0, 2, K007232.KDAC_A_update);
                            upd7759stream = new sound_stream(160000, 0, 1, Upd7759.upd7759_update);
                            samplestream = new sound_stream(48000, 0, 1, Sample.sample_update_sound);
                            sound_update = sound_updateKonami68000_tmnt;
                            mixerstream = new sound_stream(48000, 6, 0, null);
                            break;
                        case "punkshot":
                        case "punkshot2":
                        case "punkshotj":
                        case "lgtnfght":
                        case "lgtnfghta":
                        case "lgtnfghtu":
                        case "trigon":
                        case "ssriders":
                        case "ssriderseaa":
                        case "ssridersebd":
                        case "ssridersebc":
                        case "ssridersuda":
                        case "ssridersuac":
                        case "ssridersuab":
                        case "ssridersubc":
                        case "ssridersadd":
                        case "ssridersabd":
                        case "ssridersjad":
                        case "ssridersjac":
                        case "ssridersjbd":
                            YM2151.ym2151_init(3579545);
                            K053260.k053260_start(3579545);
                            ym2151stream = new sound_stream(55930, 0, 2, YM2151.ym2151_update_one);
                            k053260stream = new sound_stream(111860, 0, 2, K053260.k053260_update);
                            sound_update = sound_updateKonami68000_ssriders;
                            mixerstream = new sound_stream(48000, 4, 0, null);
                            break;
                        case "blswhstl":
                        case "blswhstla":
                        case "detatwin":
                            YM2151.ym2151_init(3579545);
                            K053260.k053260_start(3579545);
                            ym2151stream = new sound_stream(55930, 0, 2, YM2151.ym2151_update_one);
                            k053260stream = new sound_stream(111860, 0, 2, K053260.k053260_update);
                            sound_update = sound_updateKonami68000_blswhstl;
                            mixerstream = new sound_stream(48000, 4, 0, null);
                            break;
                        case "glfgreat":
                        case "glfgreatj":
                            K053260.k053260_start(3579545);
                            k053260stream = new sound_stream(111860, 0, 2, K053260.k053260_update);
                            sound_update = sound_updateKonami68000_glfgreat;
                            mixerstream = new sound_stream(48000, 2, 0, null);
                            break;
                        case "tmnt2":
                        case "tmnt2a":
                        case "tmht22pe":
                        case "tmht24pe":
                        case "tmnt22pu":
                        case "qgakumon":
                            YM2151.ym2151_init(3579545);
                            K053260.k053260_start(3579545);
                            ym2151stream = new sound_stream(55930, 0, 2, YM2151.ym2151_update_one);
                            k053260stream = new sound_stream(111860, 0, 2, K053260.k053260_update);
                            sound_update = sound_updateKonami68000_tmnt2;
                            mixerstream = new sound_stream(48000, 4, 0, null);
                            break;
                        case "thndrx2":
                        case "thndrx2a":
                        case "thndrx2j":
                            YM2151.ym2151_init(3579545);
                            K053260.k053260_start(3579545);
                            ym2151stream = new sound_stream(55930, 0, 2, YM2151.ym2151_update_one);
                            k053260stream = new sound_stream(111860, 0, 2, K053260.k053260_update);
                            sound_update = sound_updateKonami68000_thndrx2;
                            mixerstream = new sound_stream(48000, 4, 0, null);
                            break;
                        case "prmrsocr":
                        case "prmrsocrj":
                            latched_value = new ushort[3];
                            utempdata = new ushort[3];
                            K054539.k054539_start(48000);
                            k054539stream = new sound_stream(48000, 0, 2, K054539.k054539_update);
                            sound_update = sound_updateKonami68000_prmrsocr;
                            mixerstream = new sound_stream(48000, 2, 0, null);
                            break;
                    }
                    EmuTimer.timer_alloc_common(ref sound_update_timer, EmuTimer.TIME_ACT.Sound_sound_update, false);
                    break;
                case "Capcom":
                    latched_value = new ushort[1];
                    utempdata = new ushort[1];
                    switch (Machine.sName)
                    {
                        case "gng":
                        case "gnga":
                        case "gngbl":
                        case "gngprot":
                        case "gngblita":
                        case "gngc":
                        case "gngt":
                        case "makaimur":
                        case "makaimurc":
                        case "makaimurg":
                        case "diamond":
                            latched_value = new ushort[2];
                            utempdata = new ushort[2];
                            YM2203.ym2203_start(0, 1500000);
                            YM2203.ym2203_start(1, 1500000);
                            sound_update = sound_updateCapcom_gng;
                            mixerstream = new sound_stream(48000, 8, 0, null);
                            break;
                        case "sf":
                        case "sfua":
                        case "sfj":
                        case "sfjan":
                        case "sfan":
                        case "sfp":
                            YM2151.ym2151_init(3579545);
                            ym2151stream = new sound_stream(55930, 0, 2, YM2151.ym2151_update_one);
                            MSM5205.msm5205_start(0, 384000, MSM5205.null_vclk, 7);
                            MSM5205.msm5205_start(1, 384000, MSM5205.null_vclk, 7);
                            sound_update = sound_updateCapcom_sf;
                            mixerstream = new sound_stream(48000, 4, 0, null);
                            break;
                    }
                    //sound_update_timer = EmuTimer.timer_alloc_common(EmuTimer.TIME_ACT.Sound_sound_update, false);
                    EmuTimer.timer_alloc_common(ref sound_update_timer, EmuTimer.TIME_ACT.Sound_sound_update, false);
                    break;
            }
            EmuTimer.timer_adjust_periodic(sound_update_timer, update_frequency, update_frequency);
        }
        public static void sound_reset()
        {
            switch (Machine.sBoard)
            {
                case "CPS-1":
                    YM2151.ym2151_reset_chip();
                    OKI6295.okim6295_reset();
                    break;
                case "CPS-1(QSound)":
                case "CPS2":
                    break;
                case "Data East":
                    YM2203.FF2203[0].ym2203_reset_chip();
                    FMOpl.ym3812_reset_chip();
                    break;
                case "Tehkan":
                    AY8910.AA8910[0].ay8910_reset_ym();
                    AY8910.AA8910[1].ay8910_reset_ym();
                    AY8910.AA8910[2].ay8910_reset_ym();
                    break;
                case "Neo Geo":
                    YM2610.F2610.ym2610_reset_chip();
                    break;
                case "SunA8":
                    FMOpl.ym3812_reset_chip();
                    AY8910.AA8910[0].ay8910_reset_ym();
                    break;
                case "Namco System 1":
                    YM2151.ym2151_reset_chip();
                    break;
                case "IGS011":
                    switch (Machine.sName)
                    {
                        case "drgnwrld":
                        case "drgnwrldv30":
                        case "drgnwrldv21":
                        case "drgnwrldv21j":
                        case "drgnwrldv20j":
                        case "drgnwrldv10c":
                        case "drgnwrldv11h":
                        case "drgnwrldv40k":
                            OKI6295.okim6295_reset();
                            FMOpl.ym3812_reset_chip();
                            break;
                        case "lhb":
                        case "lhbv33c":
                        case "dbc":
                        case "ryukobou":
                        case "xymg":
                        case "wlcc":
                            OKI6295.okim6295_reset();
                            break;
                        case "lhb2":
                        case "nkishusp":
                            OKI6295.okim6295_reset();
                            YM2413.ym2413_reset_chip();
                            break;
                        case "vbowl":
                        case "vbowlj":
                            ICS2115.ics2115_reset();
                            break;
                    }
                    break;
                case "PGM":
                    ICS2115.ics2115_reset();
                    break;
                case "M72":
                    YM2151.ym2151_reset_chip();
                    break;
                case "M92":
                    YM2151.ym2151_reset_chip();
                    break;
                case "Taito":
                    switch (Machine.sName)
                    {
                        case "tokio":
                        case "tokioo":
                        case "tokiou":
                        case "tokiob":
                            YM2203.FF2203[0].ym2203_reset_chip();
                            break;
                        case "bublbobl":
                        case "bublbobl1":
                        case "bublboblr":
                        case "bublboblr1":
                        case "boblbobl":
                        case "sboblbobl":
                        case "sboblbobla":
                        case "sboblboblb":
                        case "sboblbobld":
                        case "sboblboblc":
                        case "bub68705":
                        case "dland":
                        case "bbredux":
                        case "bublboblb":
                        case "bublcave":
                        case "boblcave":
                        case "bublcave11":
                        case "bublcave10":
                            YM2203.FF2203[0].ym2203_reset_chip();
                            FMOpl.ym3526_reset_chip();
                            break;
                        case "opwolf":
                        case "opwolfa":
                        case "opwolfj":
                        case "opwolfu":
                        case "opwolfb":
                        case "opwolfp":
                            YM2151.ym2151_reset_chip();
                            break;
                    }
                    break;
                case "Taito B":
                    YM2610.F2610.ym2610_reset_chip();
                    break;
                case "Konami 68000":
                    switch (Machine.sName)
                    {
                        case "cuebrick":
                        case "mia":
                        case "mia2":
                            YM2151.ym2151_reset_chip();
                            break;
                        case "tmnt":
                        case "tmntu":
                        case "tmntua":
                        case "tmntub":
                        case "tmht":
                        case "tmhta":
                        case "tmhtb":
                        case "tmntj":
                        case "tmnta":
                        case "tmht2p":
                        case "tmht2pa":
                        case "tmnt2pj":
                        case "tmnt2po":
                            YM2151.ym2151_reset_chip();
                            Upd7759.upd7759_reset();
                            break;
                        case "punkshot":
                        case "punkshot2":
                        case "punkshotj":
                        case "lgtnfght":
                        case "lgtnfghta":
                        case "lgtnfghtu":
                        case "trigon":
                        case "blswhstl":
                        case "blswhstla":
                        case "detatwin":
                        case "tmnt2":
                        case "tmnt2a":
                        case "tmht22pe":
                        case "tmht24pe":
                        case "tmnt22pu":
                        case "qgakumon":
                        case "ssriders":
                        case "ssriderseaa":
                        case "ssridersebd":
                        case "ssridersebc":
                        case "ssridersuda":
                        case "ssridersuac":
                        case "ssridersuab":
                        case "ssridersubc":
                        case "ssridersadd":
                        case "ssridersabd":
                        case "ssridersjad":
                        case "ssridersjac":
                        case "ssridersjbd":
                        case "thndrx2":
                        case "thndrx2a":
                        case "thndrx2j":
                            YM2151.ym2151_reset_chip();
                            K053260.k053260_reset();
                            break;
                        case "glfgreat":
                        case "glfgreatj":
                            K053260.k053260_reset();
                            break;
                        case "prmrsocr":
                        case "prmrsocrj":
                            break;
                    }
                    break;
                case "Capcom":
                    switch (Machine.sName)
                    {
                        case "gng":
                        case "gnga":
                        case "gngbl":
                        case "gngprot":
                        case "gngblita":
                        case "gngc":
                        case "gngt":
                        case "makaimur":
                        case "makaimurc":
                        case "makaimurg":
                        case "diamond":
                            YM2203.FF2203[0].ym2203_reset_chip();
                            YM2203.FF2203[1].ym2203_reset_chip();
                            break;
                        case "sf":
                        case "sfua":
                        case "sfj":
                        case "sfjan":
                        case "sfan":
                        case "sfp":
                            YM2151.ym2151_reset_chip();
                            break;
                    }
                    break;
            }
        }
        public static void sound_pause(bool pause)
        {
            if (pause)
            {
                sound_muted |= 0x02;
                //buf2.Volume = -10000;
                Sound.SetVolume(-10000);
            }
            else
            {
                sound_muted &= ~0x02;
                //Sound.buf2.Volume = 0;
                Sound.SetVolume(0);
            }
            //osd_set_mastervolume(sound_muted ? -32 : 0);
        }
        public static void sound_updateC()
        {
            int sampindex;
            ym2151stream.stream_update();
            okistream.stream_update();
            generate_resampled_dataY5(0x59);
            generate_resampled_dataO(0x4c, 2);
            mixerstream.output_sampindex += 0x3c0;
            for (sampindex = 0; sampindex < 0x3c0; sampindex++)
            {
                int samp;
                samp = mixerstream.streaminput_Ptrs[0][sampindex] + mixerstream.streaminput_Ptrs[1][sampindex] + mixerstream.streaminput_Ptrs[2][sampindex];
                if (samp < -32768)
                {
                    samp = -32768;
                }
                else if (samp > 32767)
                {
                    samp = 32767;
                }
                finalmixb[sampindex * 4] = (byte)samp;
                finalmixb[sampindex * 4 + 1] = (byte)((samp & 0xff00) >> 8);
                finalmixb[sampindex * 4 + 2] = (byte)samp;
                finalmixb[sampindex * 4 + 3] = (byte)((samp & 0xff00) >> 8);
            }
            osd_update_audio_stream(finalmixb, 0x3c0);
            streams_updateC();
        }
        public static void sound_updateQ()
        {
            int sampindex;
            qsoundstream.stream_update();
            generate_resampled_dataQ();
            mixerstream.output_sampindex += 0x3c0;
            for (sampindex = 0; sampindex < 0x3c0; sampindex++)
            {
                int sampL, sampR;
                sampL = mixerstream.streaminput_Ptrs[0][sampindex];
                if (sampL < -32768)
                {
                    sampL = -32768;
                }
                else if (sampL > 32767)
                {
                    sampL = 32767;
                }
                sampR = mixerstream.streaminput_Ptrs[1][sampindex];
                if (sampR < -32768)
                {
                    sampR = -32768;
                }
                else if (sampR > 32767)
                {
                    sampR = 32767;
                }
                finalmixb[sampindex * 4] = (byte)sampL;
                finalmixb[sampindex * 4 + 1] = (byte)((sampL & 0xff00) >> 8);
                finalmixb[sampindex * 4 + 2] = (byte)sampR;
                finalmixb[sampindex * 4 + 3] = (byte)((sampR & 0xff00) >> 8);
            }
            osd_update_audio_stream(finalmixb, 0x3c0);
            streams_updateQ();
        }
        public static void sound_updateDataeast_pcktgal()
        {
            int sampindex;
            AY8910.AA8910[0].stream.stream_update();
            YM2203.FF2203[0].stream.stream_update();
            ym3812stream.stream_update();
            MSM5205.mm1[0].voice.stream.stream_update();
            generate_resampled_dataA3(0, 0x99, 0);
            generate_resampled_dataYM2203(0, 0x99, 3);
            generate_resampled_dataYM3812(0x100, 4);
            generate_resampled_dataMSM5205_0(0xb3, 5);
            mixerstream.output_sampindex += 0x3c0;
            for (sampindex = 0; sampindex < 0x3c0; sampindex++)
            {
                int samp;
                samp = mixerstream.streaminput_Ptrs[0][sampindex] + mixerstream.streaminput_Ptrs[1][sampindex] + mixerstream.streaminput_Ptrs[2][sampindex] + mixerstream.streaminput_Ptrs[3][sampindex] + mixerstream.streaminput_Ptrs[4][sampindex] + mixerstream.streaminput_Ptrs[5][sampindex];
                if (samp < -32768)
                {
                    samp = -32768;
                }
                else if (samp > 32767)
                {
                    samp = 32767;
                }
                finalmixb[sampindex * 4] = (byte)samp;
                finalmixb[sampindex * 4 + 1] = (byte)((samp & 0xff00) >> 8);
                finalmixb[sampindex * 4 + 2] = (byte)samp;
                finalmixb[sampindex * 4 + 3] = (byte)((samp & 0xff00) >> 8);
            }
            osd_update_audio_stream(finalmixb, 0x3c0);
            streams_updateDataeast_pcktgal();
        }
        public static void sound_updateTehkan_pbaction()
        {
            int sampindex;
            AY8910.AA8910[0].stream.stream_update();
            AY8910.AA8910[1].stream.stream_update();
            AY8910.AA8910[2].stream.stream_update();
            generate_resampled_dataA3(0, 0x40, 0);
            generate_resampled_dataA3(1, 0x40, 3);
            generate_resampled_dataA3(2, 0x40, 6);
            mixerstream.output_sampindex += 0x3c0;
            for (sampindex = 0; sampindex < 0x3c0; sampindex++)
            {
                int samp;
                samp = mixerstream.streaminput_Ptrs[0][sampindex] + mixerstream.streaminput_Ptrs[1][sampindex] + mixerstream.streaminput_Ptrs[2][sampindex] + mixerstream.streaminput_Ptrs[3][sampindex] + mixerstream.streaminput_Ptrs[4][sampindex] + mixerstream.streaminput_Ptrs[5][sampindex] + mixerstream.streaminput_Ptrs[6][sampindex] + mixerstream.streaminput_Ptrs[7][sampindex] + mixerstream.streaminput_Ptrs[8][sampindex];
                if (samp < -32768)
                {
                    samp = -32768;
                }
                else if (samp > 32767)
                {
                    samp = 32767;
                }
                finalmixb[sampindex * 4] = (byte)samp;
                finalmixb[sampindex * 4 + 1] = (byte)((samp & 0xff00) >> 8);
                finalmixb[sampindex * 4 + 2] = (byte)samp;
                finalmixb[sampindex * 4 + 3] = (byte)((samp & 0xff00) >> 8);
            }
            osd_update_audio_stream(finalmixb, 0x3c0);
            streams_updateTehkan();
        }
        public static void sound_updateN()
        {
            int sampindex;
            AY8910.AA8910[0].stream.stream_update();
            ym2610stream.stream_update();
            generate_resampled_dataA_neogeo();
            generate_resampled_dataY6();
            mixerstream.output_sampindex += 0x3c0;
            for (sampindex = 0; sampindex < 0x3c0; sampindex++)
            {
                int sampL, sampR;
                sampL = mixerstream.streaminput_Ptrs[0][sampindex] + mixerstream.streaminput_Ptrs[1][sampindex];
                if (sampL < -32768)
                {
                    sampL = -32768;
                }
                else if (sampL > 32767)
                {
                    sampL = 32767;
                }
                sampR = mixerstream.streaminput_Ptrs[0][sampindex] + mixerstream.streaminput_Ptrs[2][sampindex];
                if (sampR < -32768)
                {
                    sampR = -32768;
                }
                else if (sampR > 32767)
                {
                    sampR = 32767;
                }
                finalmixb[sampindex * 4] = (byte)sampL;
                finalmixb[sampindex * 4 + 1] = (byte)((sampL & 0xff00) >> 8);
                finalmixb[sampindex * 4 + 2] = (byte)sampR;
                finalmixb[sampindex * 4 + 3] = (byte)((sampR & 0xff00) >> 8);
            }
            osd_update_audio_stream(finalmixb, 0x3c0);
            streams_updateN();
        }
        public static void sound_updateSunA8()
        {
            int sampindex;
            ym3812stream.stream_update();
            AY8910.AA8910[0].stream.stream_update();
            samplestream.stream_update();
            generate_resampled_dataYM3812(0x100, 0);
            generate_resampled_dataA3(0, 0x80, 1);
            generate_resampled_dataSample(0x80, 4);
            mixerstream.output_sampindex += 0x3c0;
            for (sampindex = 0; sampindex < 0x3c0; sampindex++)
            {
                int samp;
                samp = mixerstream.streaminput_Ptrs[0][sampindex] + mixerstream.streaminput_Ptrs[1][sampindex] + mixerstream.streaminput_Ptrs[2][sampindex] + mixerstream.streaminput_Ptrs[3][sampindex] + mixerstream.streaminput_Ptrs[4][sampindex];
                if (samp < -32768)
                {
                    samp = -32768;
                }
                else if (samp > 32767)
                {
                    samp = 32767;
                }
                finalmixb[sampindex * 4] = (byte)samp;
                finalmixb[sampindex * 4 + 1] = (byte)((samp & 0xff00) >> 8);
                finalmixb[sampindex * 4 + 2] = (byte)samp;
                finalmixb[sampindex * 4 + 3] = (byte)((samp & 0xff00) >> 8);
            }
            osd_update_audio_stream(finalmixb, 0x3c0);
            streams_updateSunA8();
        }
        public static void sound_updateNa()
        {
            int sampindex;
            ym2151stream.stream_update();
            namcostream.stream_update();
            dacstream.stream_update();
            generate_resampled_dataY5(0x80);
            generate_resampled_dataNa();
            generate_resampled_dataDac(0x100, 4);
            mixerstream.output_sampindex += 0x3c0;
            for (sampindex = 0; sampindex < 0x3c0; sampindex++)
            {
                int sampL, sampR;
                sampL = mixerstream.streaminput_Ptrs[0][sampindex] + mixerstream.streaminput_Ptrs[2][sampindex] + mixerstream.streaminput_Ptrs[4][sampindex];
                if (sampL < -32768)
                {
                    sampL = -32768;
                }
                else if (sampL > 32767)
                {
                    sampL = 32767;
                }
                sampR = mixerstream.streaminput_Ptrs[1][sampindex] + mixerstream.streaminput_Ptrs[3][sampindex] + mixerstream.streaminput_Ptrs[4][sampindex];
                if (sampR < -32768)
                {
                    sampR = -32768;
                }
                else if (sampR > 32767)
                {
                    sampR = 32767;
                }
                finalmixb[sampindex * 4] = (byte)sampL;
                finalmixb[sampindex * 4 + 1] = (byte)((sampL & 0xff00) >> 8);
                finalmixb[sampindex * 4 + 2] = (byte)sampR;
                finalmixb[sampindex * 4 + 3] = (byte)((sampR & 0xff00) >> 8);
            }
            osd_update_audio_stream(finalmixb, 0x3c0);
            streams_updateNa();
        }
        public static void sound_updateIGS011_drgnwrld()
        {
            int sampindex;
            okistream.stream_update();
            ym3812stream.stream_update();
            generate_resampled_dataO(0x100, 0);
            generate_resampled_dataYM3812(0x200, 1);
            mixerstream.output_sampindex += 0x3c0;
            for (sampindex = 0; sampindex < 0x3c0; sampindex++)
            {
                int samp;
                samp = mixerstream.streaminput_Ptrs[0][sampindex] + mixerstream.streaminput_Ptrs[1][sampindex];
                if (samp < -32768)
                {
                    samp = -32768;
                }
                else if (samp > 32767)
                {
                    samp = 32767;
                }
                finalmixb[sampindex * 4] = (byte)samp;
                finalmixb[sampindex * 4 + 1] = (byte)((samp & 0xff00) >> 8);
                finalmixb[sampindex * 4 + 2] = (byte)samp;
                finalmixb[sampindex * 4 + 3] = (byte)((samp & 0xff00) >> 8);
            }
            osd_update_audio_stream(finalmixb, 0x3c0);
            streams_updateIGS011_drgnwrld();
        }
        public static void sound_updateIGS011_lhb()
        {
            int sampindex;
            okistream.stream_update();
            generate_resampled_dataO(0x100, 0);
            mixerstream.output_sampindex += 0x3c0;
            for (sampindex = 0; sampindex < 0x3c0; sampindex++)
            {
                int samp;
                samp = mixerstream.streaminput_Ptrs[0][sampindex];
                if (samp < -32768)
                {
                    samp = -32768;
                }
                else if (samp > 32767)
                {
                    samp = 32767;
                }
                finalmixb[sampindex * 4] = (byte)samp;
                finalmixb[sampindex * 4 + 1] = (byte)((samp & 0xff00) >> 8);
                finalmixb[sampindex * 4 + 2] = (byte)samp;
                finalmixb[sampindex * 4 + 3] = (byte)((samp & 0xff00) >> 8);
            }
            osd_update_audio_stream(finalmixb, 0x3c0);
            streams_updateIGS011_lhb();
        }
        public static void sound_updateIGS011_lhb2()
        {
            int sampindex;
            okistream.stream_update();
            ym2413stream.stream_update();
            generate_resampled_dataO(0x100, 0);
            generate_resampled_dataYM2413(0x200, 1);
            mixerstream.output_sampindex += 0x3c0;
            for (sampindex = 0; sampindex < 0x3c0; sampindex++)
            {
                int samp;
                samp = mixerstream.streaminput_Ptrs[0][sampindex] + mixerstream.streaminput_Ptrs[1][sampindex] + mixerstream.streaminput_Ptrs[2][sampindex];
                if (samp < -32768)
                {
                    samp = -32768;
                }
                else if (samp > 32767)
                {
                    samp = 32767;
                }
                finalmixb[sampindex * 4] = (byte)samp;
                finalmixb[sampindex * 4 + 1] = (byte)((samp & 0xff00) >> 8);
                finalmixb[sampindex * 4 + 2] = (byte)samp;
                finalmixb[sampindex * 4 + 3] = (byte)((samp & 0xff00) >> 8);
            }
            osd_update_audio_stream(finalmixb, 0x3c0);
            streams_updateIGS011_lhb2();
        }
        public static void sound_updateIGS011_vbowl()
        {
            int sampindex;
            ics2115stream.stream_update();
            generate_resampled_dataIcs2115(0x500);
            mixerstream.output_sampindex += 0x3c0;
            for (sampindex = 0; sampindex < 0x3c0; sampindex++)
            {
                int samp;
                samp = mixerstream.streaminput_Ptrs[0][sampindex] + mixerstream.streaminput_Ptrs[1][sampindex];
                if (samp < -32768)
                {
                    samp = -32768;
                }
                else if (samp > 32767)
                {
                    samp = 32767;
                }
                finalmixb[sampindex * 4] = (byte)samp;
                finalmixb[sampindex * 4 + 1] = (byte)((samp & 0xff00) >> 8);
                finalmixb[sampindex * 4 + 2] = (byte)samp;
                finalmixb[sampindex * 4 + 3] = (byte)((samp & 0xff00) >> 8);
            }
            osd_update_audio_stream(finalmixb, 0x3c0);
            streams_updateIGS011_vbowl();
        }
        public static void sound_updatePGM()
        {
            int sampindex;
            ics2115stream.stream_update();
            generate_resampled_dataIcs2115(0x500);
            mixerstream.output_sampindex += 0x3c0;
            for (sampindex = 0; sampindex < 0x3c0; sampindex++)
            {
                int samp;
                samp = mixerstream.streaminput_Ptrs[0][sampindex] + mixerstream.streaminput_Ptrs[1][sampindex];
                if (samp < -32768)
                {
                    samp = -32768;
                }
                else if (samp > 32767)
                {
                    samp = 32767;
                }
                finalmixb[sampindex * 4] = (byte)samp;
                finalmixb[sampindex * 4 + 1] = (byte)((samp & 0xff00) >> 8);
                finalmixb[sampindex * 4 + 2] = (byte)samp;
                finalmixb[sampindex * 4 + 3] = (byte)((samp & 0xff00) >> 8);
            }
            osd_update_audio_stream(finalmixb, 0x3c0);
            streams_updatePGM();
        }
        public static void sound_updateM72()
        {
            int sampindex;
            ym2151stream.stream_update();
            dacstream.stream_update();
            generate_resampled_dataY5(0x100);
            generate_resampled_dataDac(0x66, 2);
            mixerstream.output_sampindex += 0x3c0;
            for (sampindex = 0; sampindex < 0x3c0; sampindex++)
            {
                int sampL, sampR;
                sampL = mixerstream.streaminput_Ptrs[0][sampindex] + mixerstream.streaminput_Ptrs[2][sampindex];
                if (sampL < -32768)
                {
                    sampL = -32768;
                }
                else if (sampL > 32767)
                {
                    sampL = 32767;
                }
                sampR = mixerstream.streaminput_Ptrs[1][sampindex] + mixerstream.streaminput_Ptrs[2][sampindex];
                if (sampR < -32768)
                {
                    sampR = -32768;
                }
                else if (sampR > 32767)
                {
                    sampR = 32767;
                }
                finalmixb[sampindex * 4] = (byte)sampL;
                finalmixb[sampindex * 4 + 1] = (byte)((sampL & 0xff00) >> 8);
                finalmixb[sampindex * 4 + 2] = (byte)sampR;
                finalmixb[sampindex * 4 + 3] = (byte)((sampR & 0xff00) >> 8);
            }
            osd_update_audio_stream(finalmixb, 0x3c0);
            streams_updateM72();
        }
        public static void sound_updateM92()
        {
            int sampindex;
            ym2151stream.stream_update();
            iremga20stream.stream_update();
            generate_resampled_dataY5(0x66);
            generate_resampled_dataIremga20(0x100);
            mixerstream.output_sampindex += 0x3c0;
            for (sampindex = 0; sampindex < 0x3c0; sampindex++)
            {
                int sampL, sampR;
                sampL = mixerstream.streaminput_Ptrs[0][sampindex] + mixerstream.streaminput_Ptrs[2][sampindex];
                if (sampL < -32768)
                {
                    sampL = -32768;
                }
                else if (sampL > 32767)
                {
                    sampL = 32767;
                }
                sampR = mixerstream.streaminput_Ptrs[1][sampindex] + mixerstream.streaminput_Ptrs[3][sampindex];
                if (sampR < -32768)
                {
                    sampR = -32768;
                }
                else if (sampR > 32767)
                {
                    sampR = 32767;
                }
                finalmixb[sampindex * 4] = (byte)sampL;
                finalmixb[sampindex * 4 + 1] = (byte)((sampL & 0xff00) >> 8);
                finalmixb[sampindex * 4 + 2] = (byte)sampR;
                finalmixb[sampindex * 4 + 3] = (byte)((sampR & 0xff00) >> 8);
            }
            osd_update_audio_stream(finalmixb, 0x3c0);
            streams_updateM92();
        }
        public static void sound_updateTaito_tokio()
        {
            int sampindex;
            AY8910.AA8910[0].stream.stream_update();
            YM2203.FF2203[0].stream.stream_update();
            generate_resampled_dataA3(0, 0x14, 0);
            generate_resampled_dataYM2203(0, 0x100, 3);
            mixerstream.output_sampindex += 0x3c0;
            for (sampindex = 0; sampindex < 0x3c0; sampindex++)
            {
                int samp;
                samp = mixerstream.streaminput_Ptrs[0][sampindex] + mixerstream.streaminput_Ptrs[1][sampindex] + mixerstream.streaminput_Ptrs[2][sampindex] + mixerstream.streaminput_Ptrs[3][sampindex];
                if (samp < -32768)
                {
                    samp = -32768;
                }
                else if (samp > 32767)
                {
                    samp = 32767;
                }
                finalmixb[sampindex * 4] = (byte)samp;
                finalmixb[sampindex * 4 + 1] = (byte)((samp & 0xff00) >> 8);
                finalmixb[sampindex * 4 + 2] = (byte)samp;
                finalmixb[sampindex * 4 + 3] = (byte)((samp & 0xff00) >> 8);
            }
            osd_update_audio_stream(finalmixb, 0x3c0);
            streams_updateTaito_tokio();
        }
        public static void sound_updateTaito_bublbobl()
        {
            int sampindex;
            AY8910.AA8910[0].stream.stream_update();
            YM2203.FF2203[0].stream.stream_update();
            ym3526stream.stream_update();
            generate_resampled_dataA3(0, 0x40, 0);
            generate_resampled_dataYM2203(0, 0x40, 3);
            generate_resampled_dataYM3526(0x80, 4);
            mixerstream.output_sampindex += 0x3c0;
            for (sampindex = 0; sampindex < 0x3c0; sampindex++)
            {
                int samp;
                samp = mixerstream.streaminput_Ptrs[0][sampindex] + mixerstream.streaminput_Ptrs[1][sampindex] + mixerstream.streaminput_Ptrs[2][sampindex] + mixerstream.streaminput_Ptrs[3][sampindex] + mixerstream.streaminput_Ptrs[4][sampindex];
                if (samp < -32768)
                {
                    samp = -32768;
                }
                else if (samp > 32767)
                {
                    samp = 32767;
                }
                finalmixb[sampindex * 4] = (byte)samp;
                finalmixb[sampindex * 4 + 1] = (byte)((samp & 0xff00) >> 8);
                finalmixb[sampindex * 4 + 2] = (byte)samp;
                finalmixb[sampindex * 4 + 3] = (byte)((samp & 0xff00) >> 8);
            }
            osd_update_audio_stream(finalmixb, 0x3c0);
            streams_updateTaito_bublbobl();
        }
        public static void sound_updateTaito_opwolf()
        {
            int sampindex;
            ym2151stream.stream_update();
            MSM5205.mm1[0].voice.stream.stream_update();
            MSM5205.mm1[1].voice.stream.stream_update();
            generate_resampled_dataY5(0xc0);
            generate_resampled_dataMSM5205_0(0x99, 2);
            generate_resampled_dataMSM5205_1(0x99, 3);
            mixerstream.output_sampindex += 0x3c0;
            for (sampindex = 0; sampindex < 0x3c0; sampindex++)
            {
                int sampL, sampR;
                sampL = mixerstream.streaminput_Ptrs[0][sampindex] + mixerstream.streaminput_Ptrs[2][sampindex] + mixerstream.streaminput_Ptrs[3][sampindex];
                if (sampL < -32768)
                {
                    sampL = -32768;
                }
                else if (sampL > 32767)
                {
                    sampL = 32767;
                }
                sampR = mixerstream.streaminput_Ptrs[1][sampindex] + mixerstream.streaminput_Ptrs[2][sampindex] + mixerstream.streaminput_Ptrs[3][sampindex];
                if (sampR < -32768)
                {
                    sampR = -32768;
                }
                else if (sampR > 32767)
                {
                    sampR = 32767;
                }
                finalmixb[sampindex * 4] = (byte)sampL;
                finalmixb[sampindex * 4 + 1] = (byte)((sampL & 0xff00) >> 8);
                finalmixb[sampindex * 4 + 2] = (byte)sampR;
                finalmixb[sampindex * 4 + 3] = (byte)((sampR & 0xff00) >> 8);
            }
            osd_update_audio_stream(finalmixb, 0x3c0);
            streams_updateCapcom_sf();
        }
        public static void sound_updateTaitoB()
        {
            int sampindex;
            AY8910.AA8910[0].stream.stream_update();
            ym2610stream.stream_update();
            generate_resampled_dataA_taitob();
            generate_resampled_dataY6();
            mixerstream.output_sampindex += 0x3c0;
            for (sampindex = 0; sampindex < 0x3c0; sampindex++)
            {
                int samp;
                samp = mixerstream.streaminput_Ptrs[0][sampindex] + mixerstream.streaminput_Ptrs[1][sampindex] + mixerstream.streaminput_Ptrs[2][sampindex];
                if (samp < -32768)
                {
                    samp = -32768;
                }
                else if (samp > 32767)
                {
                    samp = 32767;
                }
                finalmixb[sampindex * 4] = (byte)samp;
                finalmixb[sampindex * 4 + 1] = (byte)((samp & 0xff00) >> 8);
                finalmixb[sampindex * 4 + 2] = (byte)samp;
                finalmixb[sampindex * 4 + 3] = (byte)((samp & 0xff00) >> 8);
            }
            osd_update_audio_stream(finalmixb, 0x3c0);
            streams_updateN();
        }
        public static void sound_updateKonami68000_cuebrick()
        {
            int sampindex;
            ym2151stream.stream_update();
            generate_resampled_dataY5(0x100);
            mixerstream.output_sampindex += 0x3c0;
            for (sampindex = 0; sampindex < 0x3c0; sampindex++)
            {
                int sampL, sampR;
                sampL = mixerstream.streaminput_Ptrs[0][sampindex] + mixerstream.streaminput_Ptrs[1][sampindex];
                if (sampL < -32768)
                {
                    sampL = -32768;
                }
                else if (sampL > 32767)
                {
                    sampL = 32767;
                }
                sampR = mixerstream.streaminput_Ptrs[0][sampindex] + mixerstream.streaminput_Ptrs[1][sampindex];
                if (sampR < -32768)
                {
                    sampR = -32768;
                }
                else if (sampR > 32767)
                {
                    sampR = 32767;
                }
                finalmixb[sampindex * 4] = (byte)sampL;
                finalmixb[sampindex * 4 + 1] = (byte)((sampL & 0xff00) >> 8);
                finalmixb[sampindex * 4 + 2] = (byte)sampR;
                finalmixb[sampindex * 4 + 3] = (byte)((sampR & 0xff00) >> 8);
            }
            osd_update_audio_stream(finalmixb, 0x3c0);
            streams_updateKonami68000_cuebrick();
        }
        public static void sound_updateKonami68000_mia()
        {
            int sampindex;
            ym2151stream.stream_update();
            k007232stream.stream_update();
            generate_resampled_dataY5(0x100);
            generate_resampled_dataK007232(0x33);
            mixerstream.output_sampindex += 0x3c0;
            for (sampindex = 0; sampindex < 0x3c0; sampindex++)
            {
                int sampL, sampR;
                sampL = mixerstream.streaminput_Ptrs[0][sampindex] + mixerstream.streaminput_Ptrs[1][sampindex] + mixerstream.streaminput_Ptrs[2][sampindex] + mixerstream.streaminput_Ptrs[3][sampindex];
                if (sampL < -32768)
                {
                    sampL = -32768;
                }
                else if (sampL > 32767)
                {
                    sampL = 32767;
                }
                sampR = mixerstream.streaminput_Ptrs[0][sampindex] + mixerstream.streaminput_Ptrs[1][sampindex] + mixerstream.streaminput_Ptrs[2][sampindex] + mixerstream.streaminput_Ptrs[3][sampindex];
                if (sampR < -32768)
                {
                    sampR = -32768;
                }
                else if (sampR > 32767)
                {
                    sampR = 32767;
                }
                finalmixb[sampindex * 4] = (byte)sampL;
                finalmixb[sampindex * 4 + 1] = (byte)((sampL & 0xff00) >> 8);
                finalmixb[sampindex * 4 + 2] = (byte)sampR;
                finalmixb[sampindex * 4 + 3] = (byte)((sampR & 0xff00) >> 8);
            }
            osd_update_audio_stream(finalmixb, 0x3c0);
            streams_updateKonami68000_mia();
        }
        public static void sound_updateKonami68000_tmnt()
        {
            int sampindex;
            ym2151stream.stream_update();
            k007232stream.stream_update();
            upd7759stream.stream_update();
            samplestream.stream_update();
            generate_resampled_dataY5(0x100);
            generate_resampled_dataK007232(0x33);
            generate_resampled_dataUpd7759(0x99);
            generate_resampled_dataSample(0x100, 5);
            mixerstream.output_sampindex += 0x3c0;
            for (sampindex = 0; sampindex < 0x3c0; sampindex++)
            {
                int sampL, sampR;
                sampL = mixerstream.streaminput_Ptrs[0][sampindex] + mixerstream.streaminput_Ptrs[1][sampindex] + mixerstream.streaminput_Ptrs[2][sampindex] + mixerstream.streaminput_Ptrs[3][sampindex] + mixerstream.streaminput_Ptrs[4][sampindex] + mixerstream.streaminput_Ptrs[5][sampindex];
                if (sampL < -32768)
                {
                    sampL = -32768;
                }
                else if (sampL > 32767)
                {
                    sampL = 32767;
                }
                sampR = mixerstream.streaminput_Ptrs[0][sampindex] + mixerstream.streaminput_Ptrs[1][sampindex] + mixerstream.streaminput_Ptrs[2][sampindex] + mixerstream.streaminput_Ptrs[3][sampindex] + mixerstream.streaminput_Ptrs[4][sampindex] + mixerstream.streaminput_Ptrs[5][sampindex];
                if (sampR < -32768)
                {
                    sampR = -32768;
                }
                else if (sampR > 32767)
                {
                    sampR = 32767;
                }
                finalmixb[sampindex * 4] = (byte)sampL;
                finalmixb[sampindex * 4 + 1] = (byte)((sampL & 0xff00) >> 8);
                finalmixb[sampindex * 4 + 2] = (byte)sampR;
                finalmixb[sampindex * 4 + 3] = (byte)((sampR & 0xff00) >> 8);
            }
            osd_update_audio_stream(finalmixb, 0x3c0);
            streams_updateKonami68000_tmnt();
        }
        public static void sound_updateKonami68000_blswhstl()
        {
            int sampindex;
            ym2151stream.stream_update();
            k053260stream.stream_update();
            generate_resampled_dataY5(0xb3);
            generate_resampled_dataK053260(0x80, 2, 3);
            mixerstream.output_sampindex += 0x3c0;
            for (sampindex = 0; sampindex < 0x3c0; sampindex++)
            {
                int sampL, sampR;
                sampL = mixerstream.streaminput_Ptrs[0][sampindex] + mixerstream.streaminput_Ptrs[2][sampindex];
                if (sampL < -32768)
                {
                    sampL = -32768;
                }
                else if (sampL > 32767)
                {
                    sampL = 32767;
                }
                sampR = mixerstream.streaminput_Ptrs[1][sampindex] + mixerstream.streaminput_Ptrs[3][sampindex];
                if (sampR < -32768)
                {
                    sampR = -32768;
                }
                else if (sampR > 32767)
                {
                    sampR = 32767;
                }
                finalmixb[sampindex * 4] = (byte)sampL;
                finalmixb[sampindex * 4 + 1] = (byte)((sampL & 0xff00) >> 8);
                finalmixb[sampindex * 4 + 2] = (byte)sampR;
                finalmixb[sampindex * 4 + 3] = (byte)((sampR & 0xff00) >> 8);
            }
            osd_update_audio_stream(finalmixb, 0x3c0);
            streams_updateKonami68000_ssriders();
        }
        public static void sound_updateKonami68000_glfgreat()
        {
            int sampindex;
            k053260stream.stream_update();
            generate_resampled_dataK053260(0x100, 0, 1);
            mixerstream.output_sampindex += 0x3c0;
            for (sampindex = 0; sampindex < 0x3c0; sampindex++)
            {
                int sampL, sampR;
                sampL = mixerstream.streaminput_Ptrs[0][sampindex];
                if (sampL < -32768)
                {
                    sampL = -32768;
                }
                else if (sampL > 32767)
                {
                    sampL = 32767;
                }
                sampR = mixerstream.streaminput_Ptrs[1][sampindex];
                if (sampR < -32768)
                {
                    sampR = -32768;
                }
                else if (sampR > 32767)
                {
                    sampR = 32767;
                }
                finalmixb[sampindex * 4] = (byte)sampL;
                finalmixb[sampindex * 4 + 1] = (byte)((sampL & 0xff00) >> 8);
                finalmixb[sampindex * 4 + 2] = (byte)sampR;
                finalmixb[sampindex * 4 + 3] = (byte)((sampR & 0xff00) >> 8);
            }
            osd_update_audio_stream(finalmixb, 0x3c0);
            streams_updateKonami68000_glfgreat();
        }
        public static void sound_updateKonami68000_tmnt2()
        {
            int sampindex;
            ym2151stream.stream_update();
            k053260stream.stream_update();
            generate_resampled_dataY5(0x100);
            generate_resampled_dataK053260(0xc0, 2, 3);
            mixerstream.output_sampindex += 0x3c0;
            for (sampindex = 0; sampindex < 0x3c0; sampindex++)
            {
                int sampL, sampR;
                sampL = mixerstream.streaminput_Ptrs[0][sampindex] + mixerstream.streaminput_Ptrs[2][sampindex];
                if (sampL < -32768)
                {
                    sampL = -32768;
                }
                else if (sampL > 32767)
                {
                    sampL = 32767;
                }
                sampR = mixerstream.streaminput_Ptrs[1][sampindex] + mixerstream.streaminput_Ptrs[3][sampindex];
                if (sampR < -32768)
                {
                    sampR = -32768;
                }
                else if (sampR > 32767)
                {
                    sampR = 32767;
                }
                finalmixb[sampindex * 4] = (byte)sampL;
                finalmixb[sampindex * 4 + 1] = (byte)((sampL & 0xff00) >> 8);
                finalmixb[sampindex * 4 + 2] = (byte)sampR;
                finalmixb[sampindex * 4 + 3] = (byte)((sampR & 0xff00) >> 8);
            }
            osd_update_audio_stream(finalmixb, 0x3c0);
            streams_updateKonami68000_ssriders();
        }
        public static void sound_updateKonami68000_ssriders()
        {
            int sampindex;
            ym2151stream.stream_update();
            k053260stream.stream_update();
            generate_resampled_dataY5(0x100);
            generate_resampled_dataK053260(0xb3, 2, 3);
            mixerstream.output_sampindex += 0x3c0;
            for (sampindex = 0; sampindex < 0x3c0; sampindex++)
            {
                int sampL, sampR;
                sampL = mixerstream.streaminput_Ptrs[0][sampindex] + mixerstream.streaminput_Ptrs[2][sampindex];
                if (sampL < -32768)
                {
                    sampL = -32768;
                }
                else if (sampL > 32767)
                {
                    sampL = 32767;
                }
                sampR = mixerstream.streaminput_Ptrs[1][sampindex] + mixerstream.streaminput_Ptrs[3][sampindex];
                if (sampR < -32768)
                {
                    sampR = -32768;
                }
                else if (sampR > 32767)
                {
                    sampR = 32767;
                }
                finalmixb[sampindex * 4] = (byte)sampL;
                finalmixb[sampindex * 4 + 1] = (byte)((sampL & 0xff00) >> 8);
                finalmixb[sampindex * 4 + 2] = (byte)sampR;
                finalmixb[sampindex * 4 + 3] = (byte)((sampR & 0xff00) >> 8);
            }
            osd_update_audio_stream(finalmixb, 0x3c0);
            streams_updateKonami68000_ssriders();
        }
        public static void sound_updateKonami68000_thndrx2()
        {
            int sampindex;
            ym2151stream.stream_update();
            k053260stream.stream_update();
            generate_resampled_dataY5(0x100);
            generate_resampled_dataK053260(0xc0, 2, 3);
            mixerstream.output_sampindex += 0x3c0;
            for (sampindex = 0; sampindex < 0x3c0; sampindex++)
            {
                int sampL, sampR;
                sampL = mixerstream.streaminput_Ptrs[0][sampindex] + mixerstream.streaminput_Ptrs[2][sampindex];
                if (sampL < -32768)
                {
                    sampL = -32768;
                }
                else if (sampL > 32767)
                {
                    sampL = 32767;
                }
                sampR = mixerstream.streaminput_Ptrs[1][sampindex] + mixerstream.streaminput_Ptrs[3][sampindex];
                if (sampR < -32768)
                {
                    sampR = -32768;
                }
                else if (sampR > 32767)
                {
                    sampR = 32767;
                }
                finalmixb[sampindex * 4] = (byte)sampL;
                finalmixb[sampindex * 4 + 1] = (byte)((sampL & 0xff00) >> 8);
                finalmixb[sampindex * 4 + 2] = (byte)sampR;
                finalmixb[sampindex * 4 + 3] = (byte)((sampR & 0xff00) >> 8);
            }
            osd_update_audio_stream(finalmixb, 0x3c0);
            streams_updateKonami68000_ssriders();
        }
        public static void sound_updateKonami68000_prmrsocr()
        {
            int sampindex;
            k054539stream.stream_update();
            generate_resampled_dataK054539(0x100);
            mixerstream.output_sampindex += 0x3c0;
            for (sampindex = 0; sampindex < 0x3c0; sampindex++)
            {
                int sampL, sampR;
                sampL = mixerstream.streaminput_Ptrs[0][sampindex];
                if (sampL < -32768)
                {
                    sampL = -32768;
                }
                else if (sampL > 32767)
                {
                    sampL = 32767;
                }
                sampR = mixerstream.streaminput_Ptrs[1][sampindex];
                if (sampR < -32768)
                {
                    sampR = -32768;
                }
                else if (sampR > 32767)
                {
                    sampR = 32767;
                }
                finalmixb[sampindex * 4] = (byte)sampL;
                finalmixb[sampindex * 4 + 1] = (byte)((sampL & 0xff00) >> 8);
                finalmixb[sampindex * 4 + 2] = (byte)sampR;
                finalmixb[sampindex * 4 + 3] = (byte)((sampR & 0xff00) >> 8);
            }
            osd_update_audio_stream(finalmixb, 0x3c0);
            streams_updateKonami68000_prmrsocr();
        }
        public static void sound_updateCapcom_gng()
        {
            int sampindex;
            AY8910.AA8910[0].stream.stream_update();
            AY8910.AA8910[1].stream.stream_update();
            YM2203.FF2203[0].stream.stream_update();
            YM2203.FF2203[1].stream.stream_update();
            generate_resampled_dataA3(0, 0x66, 0);
            generate_resampled_dataYM2203(0, 0x33, 3);
            generate_resampled_dataA3(1, 0x66, 4);
            generate_resampled_dataYM2203(1, 0x33, 7);
            mixerstream.output_sampindex += 0x3c0;
            for (sampindex = 0; sampindex < 0x3c0; sampindex++)
            {
                int samp;
                samp = mixerstream.streaminput_Ptrs[0][sampindex] + mixerstream.streaminput_Ptrs[1][sampindex] + mixerstream.streaminput_Ptrs[2][sampindex] + mixerstream.streaminput_Ptrs[3][sampindex] + mixerstream.streaminput_Ptrs[4][sampindex] + mixerstream.streaminput_Ptrs[5][sampindex] + mixerstream.streaminput_Ptrs[6][sampindex] + mixerstream.streaminput_Ptrs[7][sampindex];
                if (samp < -32768)
                {
                    samp = -32768;
                }
                else if (samp > 32767)
                {
                    samp = 32767;
                }
                finalmixb[sampindex * 4] = (byte)samp;
                finalmixb[sampindex * 4 + 1] = (byte)((samp & 0xff00) >> 8);
                finalmixb[sampindex * 4 + 2] = (byte)samp;
                finalmixb[sampindex * 4 + 3] = (byte)((samp & 0xff00) >> 8);
            }
            osd_update_audio_stream(finalmixb, 0x3c0);
            streams_updateCapcom_gng();
        }
        public static void sound_updateCapcom_sf()
        {
            int sampindex;
            ym2151stream.stream_update();
            MSM5205.mm1[0].voice.stream.stream_update();
            MSM5205.mm1[1].voice.stream.stream_update();
            generate_resampled_dataY5(0x99);
            generate_resampled_dataMSM5205_0(0x100, 2);
            generate_resampled_dataMSM5205_1(0x100, 3);
            mixerstream.output_sampindex += 0x3c0;
            for (sampindex = 0; sampindex < 0x3c0; sampindex++)
            {
                int sampL, sampR;
                sampL = mixerstream.streaminput_Ptrs[0][sampindex] + mixerstream.streaminput_Ptrs[2][sampindex] + mixerstream.streaminput_Ptrs[3][sampindex];
                if (sampL < -32768)
                {
                    sampL = -32768;
                }
                else if (sampL > 32767)
                {
                    sampL = 32767;
                }
                sampR = mixerstream.streaminput_Ptrs[1][sampindex] + mixerstream.streaminput_Ptrs[2][sampindex] + mixerstream.streaminput_Ptrs[3][sampindex];
                if (sampR < -32768)
                {
                    sampR = -32768;
                }
                else if (sampR > 32767)
                {
                    sampR = 32767;
                }
                finalmixb[sampindex * 4] = (byte)sampL;
                finalmixb[sampindex * 4 + 1] = (byte)((sampL & 0xff00) >> 8);
                finalmixb[sampindex * 4 + 2] = (byte)sampR;
                finalmixb[sampindex * 4 + 3] = (byte)((sampR & 0xff00) >> 8);
            }
            osd_update_audio_stream(finalmixb, 0x3c0);
            streams_updateCapcom_sf();
        }
        public static void latch_callback()
        {
            latched_value[0] = utempdata[0];
        }
        public static void latch_callback2()
        {
            latched_value[1] = utempdata[1];
        }
        public static void latch_callback3()
        {
            latched_value[2] = utempdata[2];
        }
        public static void latch_callback4()
        {
            latched_value[3] = utempdata[3];
        }
        public static ushort latch_r(int which)
        {
            return latched_value[which];
        }
        public static void soundlatch_w(ushort data)
        {
            utempdata[0] = data;
            EmuTimer.timer_set_internal(EmuTimer.TIME_ACT.Sound_latch_callback);
        }
        public static void soundlatch2_w(ushort data)
        {
            utempdata[1] = data;
            EmuTimer.timer_set_internal(EmuTimer.TIME_ACT.Sound_latch_callback2);
        }
        public static void soundlatch3_w(ushort data)
        {
            utempdata[2] = data;
            EmuTimer.timer_set_internal(EmuTimer.TIME_ACT.Sound_latch_callback3);
        }
        public static void soundlatch4_w(ushort data)
        {
            utempdata[3] = data;
            EmuTimer.timer_set_internal(EmuTimer.TIME_ACT.Sound_latch_callback4);
        }
        public static ushort soundlatch_r()
        {
            return latched_value[0];
        }
        public static ushort soundlatch2_r()
        {
            return latched_value[1];
        }
        public static ushort soundlatch3_r()
        {
            return latched_value[2];
        }
        public static ushort soundlatch4_r()
        {
            return latched_value[3];
        }
        private static void osd_update_audio_stream(byte[] buffer, int samples_this_frame)
        {
            ////只需要这部分逻辑
            SubmitSamples(buffer, samples_this_frame);
        }
    }
}
