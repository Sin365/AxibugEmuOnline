namespace MAME.Core
{
    public class Watchdog
    {
        public static bool watchdog_enabled;
        public static EmuTimer.emu_timer watchdog_timer;
        public static Atime watchdog_time;
        public static void watchdog_init()
        {
            //watchdog_timer = EmuTimer.timer_alloc_common(EmuTimer.TIME_ACT.Watchdog_watchdog_callback, false);
            EmuTimer.timer_alloc_common(ref watchdog_timer, EmuTimer.TIME_ACT.Watchdog_watchdog_callback, false);
            switch (Machine.sBoard)
            {
                case "CPS-1":
                case "CPS-1(QSound)":
                case "CPS2":
                case "Data East":
                case "Tehkan":
                case "Namco System 1":
                case "IGS011":
                case "PGM":
                case "M72":
                case "M92":
                case "Taito":
                case "Taito B":
                case "Konami 68000":
                case "Capcom":
                    watchdog_time = Attotime.ATTOTIME_ZERO;
                    break;
                case "Neo Geo":
                    watchdog_time = new Atime(0, (long)128762e12);
                    break;
            }
        }
        public static void watchdog_internal_reset()
        {
            watchdog_enabled = false;
            watchdog_reset();
            watchdog_enabled = true;
        }
        public static void watchdog_callback()
        {
            Mame.mame_schedule_soft_reset();
        }
        static long LastCheckFrame;
        static int frame_reset_count;
        const byte maxLimitReset_everyFrame = 1;
        public static void watchdog_reset()
        {
            if (LastCheckFrame != Video.screenstate.frame_number)
            {
                LastCheckFrame = Video.screenstate.frame_number;
                //UnityEngine.Debug.Log($"上一帧数跳过watchdog_reset:{frame_reset_count}次");
                frame_reset_count = 0;
            }

            frame_reset_count++;
            if (frame_reset_count > maxLimitReset_everyFrame)
                return;

            if (!watchdog_enabled)
            {
                EmuTimer.timer_adjust_periodic(watchdog_timer, Attotime.ATTOTIME_NEVER, Attotime.ATTOTIME_NEVER);
            }
            else if (Attotime.attotime_compare(watchdog_time, Attotime.ATTOTIME_ZERO) != 0)
            {
                EmuTimer.timer_adjust_periodic(watchdog_timer, watchdog_time, Attotime.ATTOTIME_NEVER);
            }
            else
            {
                EmuTimer.timer_adjust_periodic(watchdog_timer, new Atime(3, 0), Attotime.ATTOTIME_NEVER);
            }
        }
        //public static void watchdog_reset()
        //{
        //    if (!watchdog_enabled)
        //    {
        //        EmuTimer.timer_adjust_periodic(watchdog_timer, Attotime.ATTOTIME_NEVER, Attotime.ATTOTIME_NEVER);
        //    }
        //    else if (Attotime.attotime_compare(watchdog_time, Attotime.ATTOTIME_ZERO) != 0)
        //    {
        //        EmuTimer.timer_adjust_periodic(watchdog_timer, watchdog_time, Attotime.ATTOTIME_NEVER);
        //    }
        //    else
        //    {
        //        EmuTimer.timer_adjust_periodic(watchdog_timer, new Atime(3, 0), Attotime.ATTOTIME_NEVER);
        //    }
        //}
    }
}
