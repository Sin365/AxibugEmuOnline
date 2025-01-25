namespace MAME.Core
{
    public class Window
    {
        //private static MameMainMotion _myParentForm;
        //[DllImport("kernel32.dll ")]
        //private static extern uint GetTickCount();

        /// <summary>
        /// 操作系统启动以来的毫秒数
        /// </summary>
        /// <returns></returns>
        private static uint GetTickCount()
        {
            //return (uint)Wintime._stopwatch.ElapsedMilliseconds;
            return AxiTimeSpan.itime.GetTickCount();
        }

        public static bool input_enabled, input_paused, mouse_enabled, lightgun_enabled;
        public static uint last_poll, last_event_check;
        private static bool _CursorShown = true;
        public static void osd_update(bool skip_redraw)
        {
            if (!skip_redraw)
            {
                //winwindow_video_window_update();
            }
            winwindow_process_events(true);
            wininput_poll();
            //check_osd_inputs(machine);
        }
        public static void winwindow_process_events(bool ingame)
        {
            last_event_check = GetTickCount();
        }
        public static void wininput_poll()
        {
            if (input_enabled)
            {
                last_poll = GetTickCount();
                winwindow_process_events_periodic();
            }
        }
        public static void winwindow_process_events_periodic()
        {
            uint currticks = GetTickCount();
            if (currticks - last_event_check < 1000 / 8)
            {
                return;
            }
            winwindow_process_events(true);
        }
        public static void osd_init()
        {
            wininput_init();
        }
        public static void wininput_init()
        {
            input_enabled = true;
            switch (Machine.sName)
            {
                case "opwolf":
                case "opwolfa":
                case "opwolfj":
                case "opwolfu":
                case "opwolfb":
                case "opwolfp":
                    mouse_enabled = true;
                    lightgun_enabled = false;
                    break;
                default:
                    mouse_enabled = false;
                    lightgun_enabled = false;
                    break;
            }
            wininput_poll();
        }
        public static void wininput_pause(bool paused)
        {
            input_paused = paused;
        }
        public static bool wininput_should_hide_mouse()
        {
            if (input_paused || !input_enabled)
                return false;
            if (!mouse_enabled && !lightgun_enabled)
                return false;
            //if (win_window_list != NULL && win_has_menu(win_window_list))
            //    return false;
            return true;
        }
    }
}
