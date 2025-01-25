using MAME.Core;
using System.IO;

namespace MAME.Core
{
    public class Mame
    {
        public enum PlayState
        {
            PLAY_RUNNING = 0,
            PLAY_SAVE,
            PLAY_LOAD,
            PLAY_RESET,
            PLAY_RECORDSTART,
            PLAY_RECORDRUNNING,
            PLAY_RECORDEND,
            PLAY_REPLAYSTART,
            PLAY_REPLAYRUNNING,
            PLAY_REPLAYEND,
        }
        public static PlayState playState;
        public static bool is_foreground;
        public static bool paused;
        public static bool exit_pending;
        public static EmuTimer.emu_timer soft_reset_timer;
        public static BinaryReader brRecord = null;
        public static BinaryWriter bwRecord = null;
        public static bool bPP = true;
        public static string RomRoot = string.Empty;
        public class AA
        {
            public int fr;
            public string fi;
            public AA(int i1, string s1)
            {
                fr = i1;
                fi = s1;
            }
        }
        public static AA[] aa = new AA[]{
            new AA(1055,"1"),
            new AA(6547,"2"),
            new AA(13955,"3")
        };
        private static FileStream fsRecord = null;

        public static void mame_execute()
        {
            soft_reset();
            //mame_pause(true);
            //开始不暂停
            mame_pause(false);
            while (!exit_pending)
            {
                if (!paused)
                {
                    Cpuexec.cpuexec_timeslice();
                }
                else
                {
                    //TODO 暂停时，不应该更新画面帧
                    //Video.video_frame_update();
                }
                /*if (bPP)
                {
                    foreach (AA a in aa)
                    {
                        if (Video.screenstate.frame_number == a.fr)
                        {
                            mame_pause(true);
                            State.savestate_callback(new BinaryWriter(new FileStream(@"G:\VS2008\compare1\compare1\bin\Debug\sta1\silentd\" + a.fi + ".sta", FileMode.Create)));
                            bPP = false;
                        }
                    }                    
                }
                else
                {
                    foreach (AA a in aa)
                    {
                        if (Video.screenstate.frame_number == a.fr + 1)
                        {
                            bPP = true;
                        }
                    }
                }*/
                handlestate();
            }
        }

        #region Update 推进方式

        public static void mame_execute_UpdateMode_Start()
        {
            soft_reset();
            //mame_pause(true);
            //开始不暂停
            mame_pause(false);
        }

        public static void mame_execute_UpdateMode_NextFrame()
        {
            if (exit_pending)
                return;

            long lastframe = Video.screenstate.frame_number;
            //执行CPU命令，直到一次画面更新
            while (lastframe == Video.screenstate.frame_number)
            {
                if (!paused)
                {
                    Cpuexec.cpuexec_timeslice();
                }
                else
                {
                    Video.video_frame_update();
                }
                handlestate();
            }
        }
        #endregion

        public static void mame_schedule_soft_reset()
        {
            EmuTimer.timer_adjust_periodic(soft_reset_timer, Attotime.ATTOTIME_ZERO, Attotime.ATTOTIME_NEVER);
            mame_pause(false);
            if (Cpuexec.activecpu >= 0)
            {
                Cpuexec.cpu[Cpuexec.activecpu].PendingCycles = -1;
            }
        }
        private static void handlestate()
        {
            if (playState == PlayState.PLAY_SAVE)
            {
                mame_pause(true);
                Motion.motion_handler_callback = handle_save;
            }
            else if (playState == PlayState.PLAY_LOAD)
            {
                mame_pause(true);
                Motion.motion_handler_callback = handle_load;
            }
            else if (playState == PlayState.PLAY_RESET)
            {
                soft_reset();
                playState = PlayState.PLAY_RUNNING;
            }
            else if (playState == PlayState.PLAY_RECORDSTART)
            {
                mame_pause(true);
                Motion.motion_handler_callback = handle_record;
            }
            else if (playState == PlayState.PLAY_RECORDEND)
            {
                handle_record();
            }
            else if (playState == PlayState.PLAY_REPLAYSTART)
            {
                mame_pause(true);
                Motion.motion_handler_callback = handle_replay;
            }
            else if (playState == PlayState.PLAY_REPLAYEND)
            {
                handle_replay();
            }
        }
        public static void init_machine()
        {
            Inptport.input_init();
            Palette.palette_init();
            Generic.generic_machine_init();
            EmuTimer.timer_init();
            soft_reset_timer = EmuTimer.timer_alloc_common(EmuTimer.TIME_ACT.Mame_soft_reset, false);
            Window.osd_init();
            Inptport.input_port_init();
            Cpuexec.cpuexec_init();
            Watchdog.watchdog_init();
            Cpuint.cpuint_init();
            Machine.driver_init();
            Video.video_init();
            Tilemap.tilemap_init();
            Crosshair.crosshair_init();
            Sound.sound_init();
            State.state_init();
            Machine.machine_start();
        }
        public static void mame_pause(bool pause)
        {
            if (paused == pause)
                return;
            EmuLogger.Log($"mame_pause->{pause}");
            paused = pause;
            Window.wininput_pause(paused);
            Sound.sound_pause(paused);
        }
        public static bool mame_is_paused()
        {
            return (playState != PlayState.PLAY_RUNNING && playState != PlayState.PLAY_RECORDRUNNING && playState != PlayState.PLAY_REPLAYRUNNING) || paused;
        }
        public static void soft_reset()
        {
            Memory.memory_reset();
            Cpuint.cpuint_reset();
            Machine.machine_reset_callback();
            Generic.interrupt_reset();
            Cpuexec.cpuexec_reset();
            Watchdog.watchdog_internal_reset();
            Sound.sound_reset();
            playState = PlayState.PLAY_RUNNING;
            EmuTimer.timer_set_global_time(EmuTimer.get_current_time());
        }
        private static void handle_save()
        {
            //是否焦点
            is_foreground = true;
            if (is_foreground)
            {
                Video.sDrawText = "Select position to save to";
                Video.popup_text_end = Wintime.osd_ticks() + Wintime.ticks_per_second * 1000;
                if (Keyboard.IsTriggered(MotionKey.Escape))
                {
                    Video.sDrawText = "Save cancelled";
                    Video.popup_text_end = Wintime.osd_ticks() + Wintime.ticks_per_second * 2;
                    playState = PlayState.PLAY_RUNNING;
                    mame_pause(false);
                    Motion.motion_handler_callback = Motion.handler_ingame;
                    return;
                }
            }
        }
        private static void handle_load()
        {
            //是否焦点
            bool is_foreground = true;
            if (is_foreground)
            {
                Video.sDrawText = "Select position to load from";
                Video.popup_text_end = Wintime.osd_ticks() + Wintime.ticks_per_second * 1000;
                if (Keyboard.IsTriggered(MotionKey.Escape))
                {
                    Video.sDrawText = "Load cancelled";
                    Video.popup_text_end = Wintime.osd_ticks() + Wintime.ticks_per_second * 2;
                    playState = PlayState.PLAY_RUNNING;
                    mame_pause(false);
                    Motion.motion_handler_callback = Motion.handler_ingame;
                    return;
                }
            }
        }
        private static void handle_record()
        {
            //是否焦点
            bool is_foreground = true;
            if (is_foreground)
            {
                if (playState == PlayState.PLAY_RECORDSTART)
                {
                    Video.sDrawText = "Select position to record to";
                    Video.popup_text_end = Wintime.osd_ticks() + Wintime.ticks_per_second * 1000;
                    if (Keyboard.IsTriggered(MotionKey.Escape))
                    {
                        Video.sDrawText = "Record cancelled";
                        Video.popup_text_end = Wintime.osd_ticks() + Wintime.ticks_per_second * 2;
                        playState = PlayState.PLAY_RUNNING;
                        mame_pause(false);
                        Motion.motion_handler_callback = Motion.handler_ingame;
                        return;
                    }
                }
                else if (playState == PlayState.PLAY_RECORDEND)
                {
                    Video.sDrawText = "Record end";
                    Video.popup_text_end = Wintime.osd_ticks() + Wintime.ticks_per_second * 2;
                    bwRecord.Close();
                    bwRecord = null;
                    playState = PlayState.PLAY_RUNNING;
                }
            }
        }
        private static void handle_replay()
        {
            //是否焦点
            bool is_foreground = true;
            if (is_foreground)
            {
                if (playState == PlayState.PLAY_REPLAYSTART)
                {
                    Video.sDrawText = "Select position to replay from";
                    Video.popup_text_end = Wintime.osd_ticks() + Wintime.ticks_per_second * 1000;
                    if (Keyboard.IsTriggered(MotionKey.Escape))
                    {
                        Video.sDrawText = "Replay cancelled";
                        Video.popup_text_end = Wintime.osd_ticks() + Wintime.ticks_per_second * 2;
                        playState = PlayState.PLAY_RUNNING;
                        mame_pause(false);
                        Motion.motion_handler_callback = Motion.handler_ingame;
                        return;
                    }
                }
            }
            if (playState == PlayState.PLAY_REPLAYEND)
            {
                Video.sDrawText = "Replay end";
                Video.popup_text_end = Wintime.osd_ticks() + Wintime.ticks_per_second * 2;
                brRecord.Close();
                brRecord = null;
                //mame_pause(true);
                playState = PlayState.PLAY_RUNNING;
            }
        }
        public unsafe static void postload()
        {
            int i;
            switch (Machine.sBoard)
            {
                case "CPS-1":
                    for (i = 0; i < 3; i++)
                    {
                        CPS.ttmap[i].all_tiles_dirty = true;
                    }
                    YM2151.ym2151_postload();
                    break;
                case "CPS-1(QSound)":
                case "CPS2":
                    for (i = 0; i < 3; i++)
                    {
                        CPS.ttmap[i].all_tiles_dirty = true;
                    }
                    break;
                case "Data East":
                    Dataeast.bg_tilemap.all_tiles_dirty = true;
                    YM2203.FF2203[0].ym2203_postload();
                    break;
                case "Tehkan":
                    Tehkan.bg_tilemap.all_tiles_dirty = true;
                    Tehkan.fg_tilemap.all_tiles_dirty = true;
                    break;
                case "Neo Geo":
                    Neogeo.regenerate_pens();
                    YM2610.F2610.ym2610_postload();
                    break;
                case "Namco System 1":
                    for (i = 0; i < 6; i++)
                    {
                        Namcos1.ttmap[i].all_tiles_dirty = true;
                    }
                    YM2151.ym2151_postload();
                    break;
                case "IGS011":
                    break;
                case "PGM":
                    PGM.pgm_tx_tilemap.all_tiles_dirty = true;
                    PGM.pgm_bg_tilemap.all_tiles_dirty = true;
                    break;
                case "M72":
                    M72.bg_tilemap.all_tiles_dirty = true;
                    M72.fg_tilemap.all_tiles_dirty = true;
                    break;
                case "M92":
                    for (i = 0; i < 3; i++)
                    {
                        M92.pf_layer[i].tmap.all_tiles_dirty = true;
                        M92.pf_layer[i].wide_tmap.all_tiles_dirty = true;
                    }
                    break;
                case "Taito":
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
                            YM2203.FF2203[0].ym2203_postload();
                            break;
                        case "opwolf":
                        case "opwolfa":
                        case "opwolfj":
                        case "opwolfu":
                        case "opwolfb":
                        case "opwolfp":
                            Taito.PC080SN_tilemap[0][0].all_tiles_dirty = true;
                            Taito.PC080SN_tilemap[0][1].all_tiles_dirty = true;
                            break;
                    }
                    break;
                case "Taito B":
                    Taitob.bg_tilemap.all_tiles_dirty = true;
                    Taitob.fg_tilemap.all_tiles_dirty = true;
                    Taitob.tx_tilemap.all_tiles_dirty = true;
                    YM2610.F2610.ym2610_postload();
                    break;
                case "Konami 68000":
                    Konami68000.K052109_tilemap[0].all_tiles_dirty = true;
                    Konami68000.K052109_tilemap[1].all_tiles_dirty = true;
                    Konami68000.K052109_tilemap[2].all_tiles_dirty = true;
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
                            Capcom.bg_tilemap.all_tiles_dirty = true;
                            Capcom.fg_tilemap.all_tiles_dirty = true;
                            Capcom.bg_tilemap.tilemap_set_scrollx(0, Capcom.scrollx[0] + 256 * Capcom.scrollx[1]);
                            Capcom.fg_tilemap.tilemap_set_scrollx(0, Capcom.scrolly[0] + 256 * Capcom.scrolly[1]);
                            YM2203.FF2203[0].ym2203_postload();
                            YM2203.FF2203[1].ym2203_postload();
                            break;
                        case "sf":
                        case "sfua":
                        case "sfj":
                        case "sfjan":
                        case "sfan":
                        case "sfp":
                            Capcom.bg_tilemap.all_tiles_dirty = true;
                            Capcom.fg_tilemap.all_tiles_dirty = true;
                            Capcom.tx_tilemap.all_tiles_dirty = true;
                            Capcom.bg_tilemap.tilemap_set_scrollx(0, Capcom.bg_scrollx);
                            Capcom.fg_tilemap.tilemap_set_scrollx(0, Capcom.fg_scrollx);
                            break;
                    }
                    break;
            }
        }
    }
}
