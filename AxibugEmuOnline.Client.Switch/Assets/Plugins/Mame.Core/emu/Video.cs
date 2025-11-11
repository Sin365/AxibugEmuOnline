using System;
using System.Runtime.InteropServices;

namespace MAME.Core
{
    public struct screen_state
    {
        public int width;					/* current width (HTOTAL) */
        public int height;					/* current height (VTOTAL) */
        public RECT visarea;    			/* current visible area (HBLANK end/start, VBLANK end/start) */
        public int last_partial_scan;       /* scanline of last partial update */
        public long frame_period;			/* attoseconds per frame */
        public long vblank_period;			/* attoseconds per VBLANK period */
        public long scantime;				/* attoseconds per scanline */
        public long pixeltime;				/* attoseconds per pixel */
        public Atime vblank_start_time;
        public Atime vblank_end_time;
        public long frame_number;
    };
    unsafe partial class Video
    {
        public static bool flip_screen_x, flip_screen_y;
        public static long frame_number_obj;
        public static Atime frame_update_time;
        public static screen_state screenstate;
        public static int video_attributes;
        private static int PAUSED_REFRESH_RATE = 30, VIDEO_UPDATE_AFTER_VBLANK = 4;
        public static EmuTimer.emu_timer vblank_begin_timer, vblank_end_timer;
        public static EmuTimer.emu_timer scanline0_timer, scanline_timer;
        private static Atime speed_last_emutime, overall_emutime;
        private static long speed_last_realtime, overall_real_ticks;
        private static double speed_percent;
        private static uint throttle_history, overall_valid_counter, overall_real_seconds;
        private static int[] popcount;
        //public static ushort[][] bitmapbase;
        //public static int[][] bitmapbaseN;
        //public static int[] bitmapcolor;

        /**  bitmapcolor的指针管理  **/
        public static ushort[][] bitmapbase; //还有 部分 Array.Copy 在引用
        static GCHandle[] bitmapbase_handles;
        public static ushort*[] bitmapbase_Ptrs;
        /**  end **/

        /**  bitmapbaseN的指针管理  **/
        public static int[][] bitmapbaseN; //还有 部分 Array.Copy 在引用
        static GCHandle[] bitmapbaseN_handles;
        public static int*[] bitmapbaseN_Ptrs;
        /**  end **/

        /**  bitmapcolor的指针管理  **/
        //不再拷贝完整画布
        //public static int[] bitmapcolor;
        //static GCHandle bitmapcolor_handle;
        //public static IntPtr bitmapcolor_Ptr;

        public static int[] bitmapcolorRect;
        static GCHandle bitmapcolorRect_handle;
        public static IntPtr bitmapcolorRect_Ptr;
        public static int* bitmapcolorRect_Ptrunsafe;
        /**  end **/

        public static int fullwidth, fullheight;
        public static bool global_throttle;
        public static int scanline_param;
        public static RECT new_clip;
        public static int curbitmap;
        public static string sDrawText;
        public static long popup_text_end;
        public static int iMode, nMode;
        //private static BitmapData bitmapData;
        public static int offsetx, offsety, width, height;
        public delegate void video_delegate();
        public static video_delegate video_update_callback, video_eof_callback;
        private static int NEOGEO_HBEND = 0x01e;//30	/* this should really be 29.5 */
        private static int NEOGEO_HBSTART = 0x15e;//350 /* this should really be 349.5 */
        private static int NEOGEO_VTOTAL = 0x108;//264
        private static int NEOGEO_VBEND = 0x010;
        private static int NEOGEO_VBSTART = 0x0f0;//240
        private static int NEOGEO_VBLANK_RELOAD_HPOS = 0x11f;//287


        #region 抽象出去
        static Action<int[], long> Act_SubmitVideo;

        public static void BindFunc(IVideoPlayer Ivp)
        {
            Act_SubmitVideo -= Act_SubmitVideo;

            Act_SubmitVideo += Ivp.SubmitVideo;
        }

        static void SubmitVideo(int[] Bitmap, long frame_number)
        {
            Act_SubmitVideo.Invoke(Bitmap, frame_number);
        }
        #endregion

        public static void video_init()
        {
            Wintime.wintime_init();
            global_throttle = true;
            Motion.motion_handler_callback = Motion.handler_ingame;
            sDrawText = "";
            popup_text_end = 0;
            popcount = new int[256]{
                0,1,1,2,1,2,2,3, 1,2,2,3,2,3,3,4, 1,2,2,3,2,3,3,4, 2,3,3,4,3,4,4,5,
                1,2,2,3,2,3,3,4, 2,3,3,4,3,4,4,5, 2,3,3,4,3,4,4,5, 3,4,4,5,4,5,5,6,
                1,2,2,3,2,3,3,4, 2,3,3,4,3,4,4,5, 2,3,3,4,3,4,4,5, 3,4,4,5,4,5,5,6,
                2,3,3,4,3,4,4,5, 3,4,4,5,4,5,5,6, 3,4,4,5,4,5,5,6, 4,5,5,6,5,6,6,7,
                1,2,2,3,2,3,3,4, 2,3,3,4,3,4,4,5, 2,3,3,4,3,4,4,5, 3,4,4,5,4,5,5,6,
                2,3,3,4,3,4,4,5, 3,4,4,5,4,5,5,6, 3,4,4,5,4,5,5,6, 4,5,5,6,5,6,6,7,
                2,3,3,4,3,4,4,5, 3,4,4,5,4,5,5,6, 3,4,4,5,4,5,5,6, 4,5,5,6,5,6,6,7,
                3,4,4,5,4,5,5,6, 4,5,5,6,5,6,6,7, 4,5,5,6,5,6,6,7, 5,6,6,7,6,7,7,8
            };
            switch (Machine.sBoard)
            {
                case "CPS-1":
                case "CPS-1(QSound)":
                    screenstate.width = 0x200;
                    screenstate.height = 0x100;
                    screenstate.visarea.min_x = 0;
                    screenstate.visarea.max_x = 0x1ff;
                    screenstate.visarea.min_y = 0;
                    screenstate.visarea.max_y = 0x1ff;
                    fullwidth = 0x200;
                    fullheight = 0x200;
                    frame_update_time = new Atime(0, (long)(1e18 / 59.61));//59.61Hz
                    screenstate.vblank_period = 0;
                    video_attributes = 0;
                    Motion.motion_update_callback = Motion.ui_updateC;
                    bitmapbase = new ushort[2][];
                    bitmapbase[0] = new ushort[0x200 * 0x200];
                    bitmapbase[1] = new ushort[0x200 * 0x200];
                    video_update_callback = CPS.video_update_cps1;
                    video_eof_callback = CPS.video_eof_cps1;
                    break;
                case "CPS2":
                    screenstate.width = 0x200;
                    screenstate.height = 0x100;
                    screenstate.visarea.min_x = 0;
                    screenstate.visarea.max_x = 0x1ff;
                    screenstate.visarea.min_y = 0;
                    screenstate.visarea.max_y = 0x1ff;
                    fullwidth = 0x200;
                    fullheight = 0x200;
                    frame_update_time = new Atime(0, (long)(1e18 / 8000000) * 512 * 262);//59.637404580152669Hz
                    screenstate.vblank_period = 0;
                    video_attributes = 0;
                    Motion.motion_update_callback = Motion.ui_updateC;
                    bitmapbase = new ushort[2][];
                    bitmapbase[0] = new ushort[0x200 * 0x200];
                    bitmapbase[1] = new ushort[0x200 * 0x200];
                    video_update_callback = CPS.video_update_cps1;
                    video_eof_callback = CPS.video_eof_cps1;
                    break;
                case "Data East":
                    screenstate.width = 0x100;
                    screenstate.height = 0x100;
                    screenstate.visarea.min_x = 0;
                    screenstate.visarea.max_x = 0xff;
                    screenstate.visarea.min_y = 0x10;
                    screenstate.visarea.max_y = 0xef;
                    fullwidth = 0x100;
                    fullheight = 0x100;
                    frame_update_time = new Atime(0, (long)(1e18 / 60));
                    screenstate.vblank_period = 0;
                    video_attributes = 0;
                    Motion.motion_update_callback = Motion.ui_updateC;
                    bitmapbase = new ushort[2][];
                    bitmapbase[0] = new ushort[0x100 * 0x100];
                    bitmapbase[1] = new ushort[0x100 * 0x100];
                    video_update_callback = Dataeast.video_update_pcktgal;
                    video_eof_callback = Dataeast.video_eof_pcktgal;
                    switch (Machine.sName)
                    {
                        case "pcktgal":
                        case "pcktgalb":
                        case "pcktgal2":
                        case "pcktgal2j":
                        case "spool3":
                        case "spool3i":
                            Dataeast.palette_init_pcktgal(Dataeast.prom);
                            break;
                    }
                    break;
                case "Tehkan":
                    screenstate.width = 0x100;
                    screenstate.height = 0x100;
                    screenstate.visarea.min_x = 0;
                    screenstate.visarea.max_x = 0xff;
                    screenstate.visarea.min_y = 0x10;
                    screenstate.visarea.max_y = 0xef;
                    fullwidth = 0x100;
                    fullheight = 0x100;
                    frame_update_time = new Atime(0, (long)(1e18 / 60));
                    screenstate.vblank_period = 0;
                    video_attributes = 0;
                    Motion.motion_update_callback = Motion.ui_updateTehkan;
                    bitmapbase = new ushort[2][];
                    bitmapbase[0] = new ushort[0x100 * 0x100];
                    bitmapbase[1] = new ushort[0x100 * 0x100];
                    video_update_callback = Tehkan.video_update_pbaction;
                    video_eof_callback = Tehkan.video_eof_pbaction;
                    break;
                case "Neo Geo":
                    screenstate.width = 384;
                    screenstate.height = 264;
                    screenstate.visarea.min_x = NEOGEO_HBEND;//30
                    screenstate.visarea.max_x = NEOGEO_HBSTART - 1;//349
                    screenstate.visarea.min_y = NEOGEO_VBEND;//16
                    screenstate.visarea.max_y = NEOGEO_VBSTART - 1;//239
                    fullwidth = 384;
                    fullheight = 264;
                    frame_update_time = new Atime(0, (long)(1e18 / 6000000) * screenstate.width * screenstate.height);//59.1856060608428Hz
                    screenstate.vblank_period = (long)(1e18 / 6000000) * 384 * (264 - 224);
                    video_attributes = 0;
                    Motion.motion_update_callback = Motion.ui_updateN;
                    bitmapbaseN = new int[2][];
                    bitmapbaseN[0] = new int[384 * 264];
                    bitmapbaseN[1] = new int[384 * 264];
                    video_update_callback = Neogeo.video_update_neogeo;
                    video_eof_callback = Neogeo.video_eof_neogeo;
                    break;
                case "SunA8":
                    screenstate.width = 0x100;
                    screenstate.height = 0x100;
                    screenstate.visarea.min_x = 0;
                    screenstate.visarea.min_x = 0xff;
                    screenstate.visarea.min_y = 0x10;
                    screenstate.visarea.max_y = 0xef;
                    fullwidth = 0x100;
                    fullheight = 0x100;
                    frame_update_time = new Atime(0, (long)(1e18 / 60));
                    screenstate.vblank_period = (long)(1e12 * 2500);
                    video_attributes = 0;
                    Motion.motion_update_callback = Motion.ui_updatePGM;
                    bitmapbase = new ushort[2][];
                    bitmapbase[0] = new ushort[0x100 * 0x100];
                    bitmapbase[1] = new ushort[0x100 * 0x100];
                    video_update_callback = SunA8.video_update_suna8;
                    video_eof_callback = SunA8.video_eof_suna8;
                    break;
                case "Namco System 1":
                    screenstate.width = 0x200;
                    screenstate.height = 0x200;
                    screenstate.visarea.min_x = 0;
                    screenstate.visarea.max_x = 0x1ff;
                    screenstate.visarea.min_y = 0;
                    screenstate.visarea.max_y = 0x1ff;
                    fullwidth = 0x200;
                    fullheight = 0x200;
                    frame_update_time = new Atime(0, (long)(1e18 / 60.606060));
                    screenstate.vblank_period = 0;
                    video_attributes = 0;
                    Motion.motion_update_callback = Motion.ui_updateNa;
                    bitmapbase = new ushort[2][];
                    bitmapbase[0] = new ushort[0x200 * 0x200];
                    bitmapbase[1] = new ushort[0x200 * 0x200];
                    video_update_callback = Namcos1.video_update_namcos1;
                    video_eof_callback = Namcos1.video_eof_namcos1;
                    break;
                case "IGS011":
                    screenstate.width = 0x200;
                    screenstate.height = 0x100;
                    screenstate.visarea.min_x = 0;
                    screenstate.visarea.max_x = 0x1ff;
                    screenstate.visarea.min_y = 0;
                    screenstate.visarea.max_y = 0xef;
                    fullwidth = 0x200;
                    fullheight = 0x200;
                    frame_update_time = new Atime(0, (long)(1e18 / 60));
                    screenstate.vblank_period = 0;
                    video_attributes = 0;
                    Motion.motion_update_callback = Motion.ui_updateIGS011;
                    bitmapbase = new ushort[2][];
                    bitmapbase[0] = new ushort[0x200 * 0x200];
                    bitmapbase[1] = new ushort[0x200 * 0x200];
                    video_update_callback = IGS011.video_update_igs011;
                    video_eof_callback = IGS011.video_eof_igs011;
                    break;
                case "PGM":
                    screenstate.width = 0x200;
                    screenstate.height = 0x200;
                    screenstate.visarea.min_x = 0;
                    screenstate.visarea.max_x = 0x1bf;
                    screenstate.visarea.min_y = 0;
                    screenstate.visarea.max_y = 0xdf;
                    fullwidth = 0x200;
                    fullheight = 0x200;
                    frame_update_time = new Atime(0, (long)(1e18 / 60));
                    screenstate.vblank_period = 0;
                    video_attributes = 0;
                    Motion.motion_update_callback = Motion.ui_updatePGM;
                    bitmapbase = new ushort[2][];
                    bitmapbase[0] = new ushort[0x200 * 0x200];
                    bitmapbase[1] = new ushort[0x200 * 0x200];
                    video_update_callback = PGM.video_update_pgm;
                    video_eof_callback = PGM.video_eof_pgm;
                    break;
                case "M72":
                    screenstate.width = 0x200;
                    screenstate.height = 0x11c;
                    screenstate.visarea.min_x = 0x40;
                    screenstate.visarea.max_x = 0x1bf;
                    screenstate.visarea.min_y = 0;
                    screenstate.visarea.max_y = 0xff;
                    fullwidth = 0x200;
                    fullheight = 0x200;
                    frame_update_time = new Atime(0, (long)(1e18 / 8000000) * screenstate.width * screenstate.height);
                    screenstate.vblank_period = (long)(1e18 / 8000000) * 512 * (284 - 256);
                    video_attributes = 0;
                    Motion.motion_update_callback = Motion.ui_updatePGM;
                    bitmapbase = new ushort[2][];
                    bitmapbase[0] = new ushort[0x200 * 0x200];//0x11c
                    bitmapbase[1] = new ushort[0x200 * 0x200];//0x11c
                    video_update_callback = M72.video_update_m72;
                    video_eof_callback = M72.video_eof_m72;
                    break;
                case "M92":
                    screenstate.width = 0x200;
                    screenstate.height = 0x100;
                    screenstate.visarea.min_x = 0x50;
                    screenstate.visarea.max_x = 0x18f;
                    screenstate.visarea.min_y = 0x8;
                    screenstate.visarea.max_y = 0xf7;
                    fullwidth = 0x200;
                    fullheight = 0x200;
                    frame_update_time = new Atime(0, (long)(1e18 / 60));
                    screenstate.vblank_period = 0;
                    video_attributes = 0;
                    Motion.motion_update_callback = Motion.ui_updatePGM;
                    bitmapbase = new ushort[2][];
                    bitmapbase[0] = new ushort[0x200 * 0x200];
                    bitmapbase[1] = new ushort[0x200 * 0x200];
                    video_update_callback = M92.video_update_m92;
                    video_eof_callback = M92.video_eof_m92;
                    break;
                case "Taito":
                    video_attributes = 0;
                    Motion.motion_update_callback = Motion.ui_updatePGM;
                    switch (Machine.sName)
                    {
                        case "tokio":
                        case "tokioo":
                        case "tokiou":
                        case "tokiob":
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
                            screenstate.width = 0x100;
                            screenstate.height = 0x100;
                            screenstate.visarea.min_x = 0;
                            screenstate.visarea.max_x = 255;
                            screenstate.visarea.min_y = 16;
                            screenstate.visarea.max_y = 240 - 1;
                            fullwidth = 0x100;
                            fullheight = 0x100;
                            frame_update_time = new Atime(0, 0x003c372a18883411);
                            screenstate.vblank_period = 0;// (long)(1e18 * 0.00256);
                            bitmapbase = new ushort[2][];
                            bitmapbase[0] = new ushort[0x100 * 0x100];
                            bitmapbase[1] = new ushort[0x100 * 0x100];
                            video_update_callback = Taito.video_update_bublbobl;
                            video_eof_callback = Taito.video_eof_taito;
                            break;
                        case "opwolf":
                        case "opwolfa":
                        case "opwolfj":
                        case "opwolfu":
                        case "opwolfb":
                        case "opwolfp":
                            screenstate.width = 0x140;
                            screenstate.height = 0x100;
                            screenstate.visarea.min_x = 0;
                            screenstate.visarea.max_x = 0x13f;
                            screenstate.visarea.min_y = 8;
                            screenstate.visarea.max_y = 0xf7;
                            fullwidth = 0x140;
                            fullheight = 0x100;
                            frame_update_time = new Atime(0, (long)(1e18 / 60));
                            screenstate.vblank_period = 0;// (long)(1e18 * 0.00256);
                            bitmapbase = new ushort[2][];
                            bitmapbase[0] = new ushort[0x140 * 0x100];
                            bitmapbase[1] = new ushort[0x140 * 0x100];
                            video_update_callback = Taito.video_update_opwolf;
                            video_eof_callback = Taito.video_eof_taito;
                            break;
                    }
                    break;
                case "Taito B":
                    screenstate.width = 0x200;
                    screenstate.height = 0x100;
                    screenstate.visarea.min_x = 0;//0
                    screenstate.visarea.max_x = 320 - 1;//319
                    screenstate.visarea.min_y = 16;//16
                    screenstate.visarea.max_y = 240 - 1;//239
                    fullwidth = 0x200;
                    fullheight = 0x100;
                    frame_update_time = new Atime(0, (long)(1e18 / 60));
                    screenstate.vblank_period = 0;
                    video_attributes = 0;
                    Motion.motion_update_callback = Motion.ui_updatePGM;
                    bitmapbase = new ushort[2][];
                    bitmapbase[0] = new ushort[0x200 * 0x100];
                    bitmapbase[1] = new ushort[0x200 * 0x100];
                    video_update_callback = Taitob.video_update_taitob;
                    video_eof_callback = Taitob.video_eof_taitob;
                    break;
                case "Konami 68000":
                    screenstate.width = 0x200;
                    screenstate.height = 0x100;
                    fullwidth = 0x200;
                    fullheight = 0x100;
                    frame_update_time = new Atime(0, (long)(1e18 / 60));
                    screenstate.vblank_period = (long)(1e12 * 2500);
                    video_attributes = 0x34;
                    Motion.motion_update_callback = Motion.ui_updatePGM;
                    bitmapbase = new ushort[2][];
                    bitmapbase[0] = new ushort[0x200 * 0x100];
                    bitmapbase[1] = new ushort[0x200 * 0x100];
                    video_eof_callback = Konami68000.video_eof;
                    switch (Machine.sName)
                    {
                        case "cuebrick":
                            screenstate.visarea.min_x = 0x68;
                            screenstate.visarea.max_x = 0x197;
                            screenstate.visarea.min_y = 0x10;
                            screenstate.visarea.max_y = 0xef;
                            video_update_callback = Konami68000.video_update_mia;
                            break;
                        case "mia":
                        case "mia2":
                            screenstate.visarea.min_x = 0x68;
                            screenstate.visarea.max_x = 0x197;
                            screenstate.visarea.min_y = 0x10;
                            screenstate.visarea.max_y = 0xef;
                            video_attributes = 0x30;
                            video_update_callback = Konami68000.video_update_mia;
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
                            screenstate.visarea.min_x = 0x60;
                            screenstate.visarea.max_x = 0x19f;
                            screenstate.visarea.min_y = 0x10;
                            screenstate.visarea.max_y = 0xef;
                            video_attributes = 0x30;
                            video_update_callback = Konami68000.video_update_mia;
                            break;
                        case "punkshot":
                        case "punkshot2":
                        case "punkshotj":
                            screenstate.visarea.min_x = 0x70;
                            screenstate.visarea.max_x = 0x18f;
                            screenstate.visarea.min_y = 0x10;
                            screenstate.visarea.max_y = 0xef;
                            video_update_callback = Konami68000.video_update_punkshot;
                            break;
                        case "lgtnfght":
                        case "lgtnfghta":
                        case "lgtnfghtu":
                        case "trigon":
                            screenstate.visarea.min_x = 0x60;
                            screenstate.visarea.max_x = 0x19f;
                            screenstate.visarea.min_y = 0x10;
                            screenstate.visarea.max_y = 0xef;
                            video_update_callback = Konami68000.video_update_lgtnfght;
                            break;
                        case "blswhstl":
                        case "blswhstla":
                        case "detatwin":
                            screenstate.visarea.min_x = 0x60;
                            screenstate.visarea.max_x = 0x19f;
                            screenstate.visarea.min_y = 0x10;
                            screenstate.visarea.max_y = 0xef;
                            video_update_callback = Konami68000.video_update_lgtnfght;
                            video_eof_callback = Konami68000.video_eof_blswhstl;
                            break;
                        case "glfgreat":
                        case "glfgreatj":
                        case "prmrsocr":
                        case "prmrsocrj":
                            screenstate.visarea.min_x = 0x70;
                            screenstate.visarea.max_x = 0x18f;
                            screenstate.visarea.min_y = 0x10;
                            screenstate.visarea.max_y = 0xef;
                            video_update_callback = Konami68000.video_update_glfgreat;
                            break;
                        case "tmnt2":
                        case "tmnt2a":
                        case "tmht22pe":
                        case "tmht24pe":
                        case "tmnt22pu":
                        case "qgakumon":
                            screenstate.visarea.min_x = 0x68;
                            screenstate.visarea.max_x = 0x197;
                            screenstate.visarea.min_y = 0x10;
                            screenstate.visarea.max_y = 0xef;
                            video_update_callback = Konami68000.video_update_tmnt2;
                            break;
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
                            screenstate.visarea.min_x = 0x70;
                            screenstate.visarea.max_x = 0x18f;
                            screenstate.visarea.min_y = 0x10;
                            screenstate.visarea.max_y = 0xef;
                            video_update_callback = Konami68000.video_update_tmnt2;
                            break;
                        case "thndrx2":
                        case "thndrx2a":
                        case "thndrx2j":
                            screenstate.visarea.min_x = 0x70;
                            screenstate.visarea.max_x = 0x18f;
                            screenstate.visarea.min_y = 0x10;
                            screenstate.visarea.max_y = 0xef;
                            video_attributes = 0x30;
                            video_update_callback = Konami68000.video_update_thndrx2;
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
                            screenstate.width = 0x100;
                            screenstate.height = 0x100;
                            screenstate.visarea.min_x = 0;
                            screenstate.visarea.max_x = 0xff;
                            screenstate.visarea.min_y = 0x10;
                            screenstate.visarea.max_y = 0xef;
                            fullwidth = 0x100;
                            fullheight = 0x100;
                            frame_update_time = new Atime(0, (long)(1e18 / 60));
                            screenstate.vblank_period = 0;
                            video_attributes = 0;
                            Motion.motion_update_callback = Motion.ui_updatePGM;
                            bitmapbase = new ushort[2][];
                            bitmapbase[0] = new ushort[0x100 * 0x100];
                            bitmapbase[1] = new ushort[0x100 * 0x100];
                            video_update_callback = Capcom.video_update_gng;
                            video_eof_callback = Capcom.video_eof_gng;
                            break;
                        case "sf":
                        case "sfua":
                        case "sfj":
                        case "sfjan":
                        case "sfan":
                        case "sfp":
                            screenstate.width = 0x200;
                            screenstate.height = 0x100;
                            screenstate.visarea.min_x = 0x40;
                            screenstate.visarea.max_x = 0x1bf;
                            screenstate.visarea.min_y = 0x10;
                            screenstate.visarea.max_y = 0xef;
                            fullwidth = 0x200;
                            fullheight = 0x100;
                            frame_update_time = new Atime(0, (long)(1e18 / 60));
                            screenstate.vblank_period = 0;
                            video_attributes = 0;
                            Motion.motion_update_callback = Motion.ui_updatePGM;
                            bitmapbase = new ushort[2][];
                            bitmapbase[0] = new ushort[0x200 * 0x100];
                            bitmapbase[1] = new ushort[0x200 * 0x100];
                            video_update_callback = Capcom.video_update_sf;
                            video_eof_callback = Capcom.video_eof;
                            break;
                    }
                    break;
            }
            screenstate.frame_period = frame_update_time.attoseconds;
            screenstate.scantime = screenstate.frame_period / screenstate.height;
            screenstate.pixeltime = screenstate.frame_period / (screenstate.height * screenstate.width);
            screenstate.frame_number = 0;


            /**  bitmapbase的指针管理  **/
            // 释放句柄
            if (bitmapbase_handles != null)
            {
                for (int i = 0; i < bitmapbase_handles.Length; i++)
                {
                    if (bitmapbase_handles[i].IsAllocated)
                        bitmapbase_handles[i].Free();
                }
                bitmapbase_handles = null;
                bitmapbase_Ptrs = null;
            }

            if (bitmapbase != null)
            {
                bitmapbase_handles = new GCHandle[bitmapbase.Length];
                bitmapbase_Ptrs = new ushort*[bitmapbase.Length];
                for (int i = 0; i < bitmapbase.Length; i++)
                {
                    bitmapbase_handles[i] = GCHandle.Alloc(bitmapbase[i], GCHandleType.Pinned);
                    bitmapbase_Ptrs[i] = (ushort*)bitmapbase_handles[i].AddrOfPinnedObject();
                }
            }


            if (bitmapbaseN_handles != null)
            {
                for (int i = 0; i < bitmapbaseN_handles.Length; i++)
                {
                    if (bitmapbaseN_handles[i].IsAllocated)
                        bitmapbaseN_handles[i].Free();
                }
                bitmapbaseN_handles = null;
                bitmapbaseN_Ptrs = null;
            }

            if (bitmapbaseN != null)
            {
                bitmapbaseN_handles = new GCHandle[bitmapbaseN.Length];
                bitmapbaseN_Ptrs = new int*[bitmapbaseN.Length];
                for (int i = 0; i < bitmapbaseN.Length; i++)
                {
                    bitmapbaseN_handles[i] = GCHandle.Alloc(bitmapbaseN[i], GCHandleType.Pinned);
                    bitmapbaseN_Ptrs[i] = (int*)bitmapbaseN_handles[i].AddrOfPinnedObject();
                }
            }

            /**  end **/

            //bitmapcolor = new int[Video.fullwidth * Video.fullheight];
            /**  bitmapcolor的指针管理  **/
            //不再拷贝完整画布
            //if (bitmapcolor != null)
            //{
            //    // 释放句柄  
            //    if (bitmapcolor_handle.IsAllocated)
            //    {
            //        bitmapcolor_handle.Free();
            //    }
            //}
            //bitmapcolor = new int[Video.fullwidth * Video.fullheight];
            //// 固定数组，防止垃圾回收器移动它  
            //bitmapcolor_handle = GCHandle.Alloc(bitmapcolor, GCHandleType.Pinned);
            //// 获取数组的指针  
            //bitmapcolor_Ptr = bitmapcolor_handle.AddrOfPinnedObject();




            // 释放句柄
            if (bitmapcolorRect != null && bitmapcolorRect_handle.IsAllocated)
                bitmapcolorRect_handle.Free();

            bitmapcolorRect = new int[width * height];
            // 固定数组，防止垃圾回收器移动它  
            bitmapcolorRect_handle = GCHandle.Alloc(bitmapcolorRect, GCHandleType.Pinned);
            // 获取数组的指针  
            bitmapcolorRect_Ptr = bitmapcolorRect_handle.AddrOfPinnedObject();

            bitmapcolorRect_Ptrunsafe = (int*)bitmapcolorRect_Ptr;
            /**  end **/





            //vblank_begin_timer = EmuTimer.timer_alloc_common(EmuTimer.TIME_ACT.Video_vblank_begin_callback, false);
            EmuTimer.timer_alloc_common(ref vblank_begin_timer, EmuTimer.TIME_ACT.Video_vblank_begin_callback, false);
            EmuTimer.timer_adjust_periodic(vblank_begin_timer, video_screen_get_time_until_vblank_start(), Attotime.ATTOTIME_NEVER);
            //scanline0_timer = EmuTimer.timer_alloc_common(EmuTimer.TIME_ACT.Video_scanline0_callback, false);
            EmuTimer.timer_alloc_common(ref scanline0_timer, EmuTimer.TIME_ACT.Video_scanline0_callback, false);
            EmuTimer.timer_adjust_periodic(scanline0_timer, video_screen_get_time_until_pos(0, 0), Attotime.ATTOTIME_NEVER);
            //vblank_end_timer = EmuTimer.timer_alloc_common(EmuTimer.TIME_ACT.Video_vblank_end_callback, false);
            EmuTimer.timer_alloc_common(ref vblank_end_timer, EmuTimer.TIME_ACT.Video_vblank_end_callback, false);
            switch (Machine.sBoard)
            {
                case "CPS-1":
                case "CPS-1(QSound)":
                case "Namco System 1":
                case "M92":
                case "Taito B":
                    break;
                case "CPS2":
                    Cpuexec.cpu[0].partial_frame_period = Attotime.attotime_div(Video.frame_update_time, 262);
                    EmuTimer.timer_alloc_common(ref Cpuexec.cpu[0].partial_frame_timer, EmuTimer.TIME_ACT.Cpuexec_trigger_partial_frame_interrupt, false);
                    break;
                case "Tehkan":
                    Cpuexec.cpu[1].partial_frame_period = Attotime.attotime_div(Video.frame_update_time, 2);
                    EmuTimer.timer_alloc_common(ref Cpuexec.cpu[1].partial_frame_timer, EmuTimer.TIME_ACT.Cpuexec_trigger_partial_frame_interrupt, false);
                    break;
                case "Neo Geo":
                    break;
                case "SunA8":
                    Cpuexec.cpu[0].partial_frame_period = Attotime.attotime_div(Video.frame_update_time, 0x100);
                    EmuTimer.timer_alloc_common(ref Cpuexec.cpu[0].partial_frame_timer, EmuTimer.TIME_ACT.Cpuexec_trigger_partial_frame_interrupt, false);
                    Cpuexec.cpu[1].partial_frame_period = Attotime.attotime_div(Video.frame_update_time, 4);
                    EmuTimer.timer_alloc_common(ref Cpuexec.cpu[1].partial_frame_timer, EmuTimer.TIME_ACT.Cpuexec_trigger2, false);
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
                        case "lhb2":
                            Cpuexec.cpu[0].partial_frame_period = Attotime.attotime_div(Video.frame_update_time, 5);
                            EmuTimer.timer_alloc_common(ref Cpuexec.cpu[0].partial_frame_timer, EmuTimer.TIME_ACT.Cpuexec_trigger_partial_frame_interrupt, false);
                            break;
                        case "lhb":
                        case "lhbv33c":
                        case "dbc":
                        case "ryukobou":
                            Cpuexec.cpu[0].partial_frame_period = Attotime.attotime_div(Video.frame_update_time, 4);
                            EmuTimer.timer_alloc_common(ref Cpuexec.cpu[0].partial_frame_timer, EmuTimer.TIME_ACT.Cpuexec_trigger_partial_frame_interrupt, false);
                            break;
                    }
                    break;
                case "M72":
                    Cpuexec.cpu[1].partial_frame_period = Attotime.attotime_div(Video.frame_update_time, 128);
                    EmuTimer.timer_alloc_common(ref Cpuexec.cpu[1].partial_frame_timer, EmuTimer.TIME_ACT.Cpuexec_trigger_partial_frame_interrupt, false);
                    break;
                case "Taito":
                    switch (Machine.sName)
                    {
                        case "bub68705":
                            Cpuexec.cpu[3].partial_frame_period = Attotime.attotime_div(Video.frame_update_time, 2);
                            EmuTimer.timer_alloc_common(ref Cpuexec.cpu[3].partial_frame_timer, EmuTimer.TIME_ACT.Cpuexec_trigger_partial_frame_interrupt, false);
                            break;
                    }
                    break;
                case "Konami 68000":
                    switch (Machine.sName)
                    {
                        case "cuebrick":
                            Cpuexec.cpu[0].partial_frame_period = Attotime.attotime_div(Video.frame_update_time, 10);
                            //Cpuexec.cpu[0].partial_frame_timer = EmuTimer.timer_alloc_common(EmuTimer.TIME_ACT.Cpuexec_trigger_partial_frame_interrupt, false);
                            EmuTimer.timer_alloc_common(ref Cpuexec.cpu[0].partial_frame_timer, EmuTimer.TIME_ACT.Cpuexec_trigger_partial_frame_interrupt, false);
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
                            Cpuexec.cpu[1].partial_frame_period = Attotime.attotime_div(Video.frame_update_time, 4);
                            //Cpuexec.cpu[1].partial_frame_timer = EmuTimer.timer_alloc_common(EmuTimer.TIME_ACT.Cpuexec_trigger_partial_frame_interrupt, false);
                            EmuTimer.timer_alloc_common(ref Cpuexec.cpu[1].partial_frame_timer, EmuTimer.TIME_ACT.Cpuexec_trigger_partial_frame_interrupt, false);
                            break;
                    }
                    break;
            }
            screenstate.vblank_start_time = Attotime.ATTOTIME_ZERO;
            screenstate.vblank_end_time = new Atime(0, screenstate.vblank_period);
        }
        public static void video_screen_configure(int width, int height, RECT visarea, long frame_period)
        {
            screenstate.width = width;
            screenstate.height = height;
            screenstate.visarea = visarea;
            //realloc_screen_bitmaps(screen);
            screenstate.frame_period = frame_period;
            screenstate.scantime = frame_period / height;
            screenstate.pixeltime = frame_period / (height * width);
            /*if (config->vblank == 0 && !config->oldstyle_vblank_supplied)
                state->vblank_period = state->scantime * (height - (visarea->max_y + 1 - visarea->min_y));
            else
                state->vblank_period = config->vblank;
            if (video_screen_get_vpos(screen) == 0)
                timer_adjust_oneshot(state->scanline0_timer, attotime_zero, 0);
            else
                timer_adjust_oneshot(state->scanline0_timer, video_screen_get_time_until_pos(screen, 0, 0), 0);
            timer_adjust_oneshot(state->vblank_begin_timer, video_screen_get_time_until_vblank_start(screen), 0);
            update_refresh_speed(screen->machine);*/
        }
        public static bool video_screen_update_partial(int scanline)
        {
            new_clip = screenstate.visarea;
            bool result = false;
            if (screenstate.last_partial_scan > new_clip.min_y)
            {
                new_clip.min_y = screenstate.last_partial_scan;
            }
            if (scanline < new_clip.max_y)
            {
                new_clip.max_y = scanline;
            }
            if (new_clip.min_y <= new_clip.max_y)
            {
                video_update_callback();
                result = true;
            }
            screenstate.last_partial_scan = scanline + 1;
            return result;
        }
        public static int video_screen_get_vpos()
        {
            long delta = Attotime.attotime_to_attoseconds(Attotime.attotime_sub(EmuTimer.get_current_time(), screenstate.vblank_start_time));
            int vpos;
            delta += screenstate.pixeltime / 2;
            vpos = (int)(delta / screenstate.scantime);
            return (screenstate.visarea.max_y + 1 + vpos) % screenstate.height;
        }
        public static bool video_screen_get_vblank()
        {
            return (Attotime.attotime_compare(EmuTimer.get_current_time(), screenstate.vblank_end_time) < 0);
        }
        public static Atime video_screen_get_time_until_pos(int vpos, int hpos)
        {
            long curdelta = Attotime.attotime_to_attoseconds(Attotime.attotime_sub(EmuTimer.get_current_time(), screenstate.vblank_start_time));
            long targetdelta;
            vpos += screenstate.height - (screenstate.visarea.max_y + 1);
            vpos %= screenstate.height;
            targetdelta = vpos * screenstate.scantime + hpos * screenstate.pixeltime;
            if (targetdelta <= curdelta + screenstate.pixeltime / 2)
                targetdelta += screenstate.frame_period;
            while (targetdelta <= curdelta)
                targetdelta += screenstate.frame_period;
            return new Atime(0, targetdelta - curdelta);
        }
        public static Atime video_screen_get_time_until_vblank_start()
        {
            return video_screen_get_time_until_pos(Video.screenstate.visarea.max_y + 1, 0);
        }
        public static Atime video_screen_get_time_until_vblank_end()
        {
            Atime ret;
            Atime current_time = EmuTimer.get_current_time();
            if (video_screen_get_vblank())
            {
                ret = Attotime.attotime_sub(screenstate.vblank_end_time, current_time);
            }
            else
            {
                ret = Attotime.attotime_sub(Attotime.attotime_add_attoseconds(screenstate.vblank_end_time, screenstate.frame_period), current_time);
            }
            return ret;
        }
        private static bool effective_throttle()
        {
            //	if (mame_is_paused(machine) || ui_is_menu_active())
            //		return true;
            //	if (global.fastforward)
            //		return false;
            return global_throttle;
        }
        public static void vblank_begin_callback()
        {
            screenstate.vblank_start_time = EmuTimer.global_basetime;// Timer.get_current_time();
            screenstate.vblank_end_time = Attotime.attotime_add_attoseconds(screenstate.vblank_start_time, screenstate.vblank_period);
            Cpuexec.on_vblank();
            //垂直同步
            if ((video_attributes & VIDEO_UPDATE_AFTER_VBLANK) == 0)
            {
                video_frame_update();
            }
            EmuTimer.timer_adjust_periodic(vblank_begin_timer, video_screen_get_time_until_vblank_start(), Attotime.ATTOTIME_NEVER);
            if (screenstate.vblank_period == 0)
            {
                vblank_end_callback();
            }
            else
            {
                EmuTimer.timer_adjust_periodic(vblank_end_timer, video_screen_get_time_until_vblank_end(), Attotime.ATTOTIME_NEVER);
            }
        }
        public static void vblank_end_callback()
        {
            int i;
            //垂直同步
            if ((video_attributes & VIDEO_UPDATE_AFTER_VBLANK) != 0)
            {
                video_frame_update();
            }
        }
        public static void scanline0_callback()
        {
            screenstate.last_partial_scan = 0;
            EmuTimer.timer_adjust_periodic(scanline0_timer, video_screen_get_time_until_pos(0, 0), Attotime.ATTOTIME_NEVER);
        }
        public static void scanline_update_callback()
        {
            int scanline = scanline_param;
            video_screen_update_partial(scanline);
            scanline++;
            if (scanline > screenstate.visarea.max_y)
            {
                scanline = screenstate.visarea.min_y;
            }
            scanline_param = scanline;
            EmuTimer.timer_adjust_periodic(scanline_timer, video_screen_get_time_until_pos(scanline, 0), Attotime.ATTOTIME_NEVER);
        }
        public static void video_frame_update()
        {
            Atime current_time = EmuTimer.global_basetime;
            if (!Mame.paused)
            {
                finish_screen_updates();
            }
            Keyboard.Update();
            Mouse.Update();
            Inptport.frame_update_callback();
            Motion.ui_update_and_render();
            //if (Machine.mainMotion.cheatmotion.lockState == CheatMotion.LockState.LOCK_FRAME)
            //{
            //    Machine.mainMotion.cheatmotion.ApplyCheat();
            //}
            GDIDraw();

            if (effective_throttle())
            {
                //不执行该函数，避免Thread.Sleep （暂时确认无逻辑依赖）
                //废弃
                //update_throttle(current_time);
            }

            Window.osd_update(false);
            //UI.ui_input_frame_update();
            recompute_speed(current_time);
            if (Mame.paused)
            {
                //Thread.Sleep(5);
            }
            else
            {
                video_eof_callback();
            }
        }
        private static void finish_screen_updates()
        {
            video_screen_update_partial(screenstate.visarea.max_y);
            curbitmap = 1 - curbitmap;
        }
        //废弃
        //private static void update_throttle(Atime emutime)
        //{
        //    long real_delta_attoseconds;
        //    long emu_delta_attoseconds;
        //    long real_is_ahead_attoseconds;
        //    long attoseconds_per_tick;
        //    long ticks_per_second;
        //    long target_ticks;
        //    long diff_ticks;
        //    ticks_per_second = Wintime.ticks_per_second;
        //    attoseconds_per_tick = Attotime.ATTOSECONDS_PER_SECOND / ticks_per_second;
        //    if (Mame.mame_is_paused())
        //    {
        //        throttle_emutime = Attotime.attotime_sub_attoseconds(emutime, Attotime.ATTOSECONDS_PER_SECOND / PAUSED_REFRESH_RATE);
        //        throttle_realtime = throttle_emutime;
        //    }
        //    emu_delta_attoseconds = Attotime.attotime_to_attoseconds(Attotime.attotime_sub(emutime, throttle_emutime));
        //    if (emu_delta_attoseconds < 0 || emu_delta_attoseconds > Attotime.ATTOSECONDS_PER_SECOND / 10)
        //    {
        //        goto resync;
        //    }
        //    diff_ticks = Wintime.osd_ticks() - throttle_last_ticks;
        //    throttle_last_ticks += diff_ticks;
        //    if (diff_ticks >= ticks_per_second)
        //    {
        //        goto resync;
        //    }
        //    real_delta_attoseconds = diff_ticks * attoseconds_per_tick;
        //    throttle_emutime = emutime;
        //    throttle_realtime = Attotime.attotime_add_attoseconds(throttle_realtime, real_delta_attoseconds);
        //    throttle_history = (throttle_history << 1) | Convert.ToUInt32(emu_delta_attoseconds > real_delta_attoseconds);
        //    real_is_ahead_attoseconds = Attotime.attotime_to_attoseconds(Attotime.attotime_sub(throttle_emutime, throttle_realtime));
        //    if ((real_is_ahead_attoseconds < -Attotime.ATTOSECONDS_PER_SECOND / 10) || (real_is_ahead_attoseconds < 0 && popcount[throttle_history & 0xff] < 6))
        //    {
        //        goto resync;
        //    }
        //    if (real_is_ahead_attoseconds < 0)
        //    {
        //        return;
        //    }
        //    target_ticks = throttle_last_ticks + real_is_ahead_attoseconds / attoseconds_per_tick;
        //    diff_ticks = throttle_until_ticks(target_ticks) - throttle_last_ticks;
        //    throttle_last_ticks += diff_ticks;
        //    throttle_realtime = Attotime.attotime_add_attoseconds(throttle_realtime, diff_ticks * attoseconds_per_tick);
        //    return;
        //resync:
        //    throttle_realtime = throttle_emutime = emutime;
        //}
        //废弃
        //private static long throttle_until_ticks(long target_ticks)
        //{
        //    long minimum_sleep = Wintime.ticks_per_second / 1000;
        //    long current_ticks = Wintime.osd_ticks();
        //    long new_ticks;
        //    while (current_ticks < target_ticks)
        //    {
        //        long delta;
        //        bool slept = false;
        //        delta = (target_ticks - current_ticks) * 1000 / (1000 + average_oversleep);
        //        if (delta >= minimum_sleep)
        //        {
        //            Wintime.osd_sleep(delta);
        //            slept = true;
        //        }
        //        new_ticks = Wintime.osd_ticks();
        //        if (slept)
        //        {
        //            long actual_ticks = new_ticks - current_ticks;
        //            if (actual_ticks > delta)
        //            {
        //                long oversleep_milliticks = 1000 * (actual_ticks - delta) / delta;
        //                average_oversleep = (average_oversleep * 99 + oversleep_milliticks) / 100;

        //            }
        //        }
        //        current_ticks = new_ticks;
        //    }
        //    return current_ticks;
        //}
        private static void recompute_speed(Atime emutime)
        {
            long delta_emutime;
            if (speed_last_realtime == 0 || Mame.mame_is_paused())
            {
                speed_last_realtime = Wintime.osd_ticks();
                speed_last_emutime = emutime;
            }
            delta_emutime = Attotime.attotime_to_attoseconds(Attotime.attotime_sub(emutime, speed_last_emutime));
            if (delta_emutime > Attotime.ATTOSECONDS_PER_SECOND / 4)
            {
                long realtime = Wintime.osd_ticks();
                long delta_realtime = realtime - speed_last_realtime;
                long tps = Wintime.ticks_per_second;
                speed_percent = (double)delta_emutime * (double)tps / ((double)delta_realtime * (double)Attotime.ATTOSECONDS_PER_SECOND);
                speed_last_realtime = realtime;
                speed_last_emutime = emutime;
                overall_valid_counter++;
                if (overall_valid_counter >= 4)
                {
                    overall_real_ticks += delta_realtime;
                    while (overall_real_ticks >= tps)
                    {
                        overall_real_ticks -= tps;
                        overall_real_seconds++;
                    }
                    overall_emutime = Attotime.attotime_add_attoseconds(overall_emutime, delta_emutime);
                }
            }
        }
        public static void flip_screen_set_no_update(bool on)
        {
            flip_screen_x = on;
        }
        public static bool flip_screen_get()
        {
            return flip_screen_x;
        }
        public static void SaveStateBinary(System.IO.BinaryWriter writer)
        {
            writer.Write(scanline_param);
            writer.Write(screenstate.last_partial_scan);
            writer.Write(screenstate.vblank_start_time.seconds);
            writer.Write(screenstate.vblank_start_time.attoseconds);
            writer.Write(screenstate.vblank_end_time.seconds);
            writer.Write(screenstate.vblank_end_time.attoseconds);
            writer.Write(screenstate.frame_number);
        }
        public static void LoadStateBinary(System.IO.BinaryReader reader)
        {
            scanline_param = reader.ReadInt32();
            screenstate.last_partial_scan = reader.ReadInt32();
            screenstate.vblank_start_time.seconds = reader.ReadInt32();
            screenstate.vblank_start_time.attoseconds = reader.ReadInt64();
            screenstate.vblank_end_time.seconds = reader.ReadInt32();
            screenstate.vblank_end_time.attoseconds = reader.ReadInt64();
            screenstate.frame_number = reader.ReadInt64();
        }
    }

}
