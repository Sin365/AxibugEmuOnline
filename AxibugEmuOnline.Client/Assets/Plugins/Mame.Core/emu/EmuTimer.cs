using cpu.m6800;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Threading;
using System.Timers;
using static MAME.Core.EmuTimer;

namespace MAME.Core
{
    public class EmuTimerLister
    {
        public emu_timer this[int index]
        {
            get { return timerlist[index]; }
            set { timerlist[index] = value; } // 如果需要设置的话
        }
        public static void GetNewTimerLister(ref EmuTimerLister tlistObj)
        {
            //如果新旧值替换
            if (tlistObj != null)
            {
                tlistObj.ReleaseLister();
                ObjectPoolAuto.Release(tlistObj);
                tlistObj = null;
            }

            tlistObj = ObjectPoolAuto.Acquire<EmuTimerLister>();
            tlistObj.InitLister();
        }

        List<emu_timer> timerlist;

        public List<emu_timer> GetSrcList()
        {
            return timerlist;
        }
        public int Count
        {
            get { return timerlist.Count; }
        }

        void InitLister()
        {
            ReleaseLister();
            timerlist = ObjectPoolAuto.AcquireList<emu_timer>();
        }
        void ReleaseLister()
        {
            if (timerlist != null)
            {
                Clear();
                ObjectPoolAuto.Release(timerlist);
                timerlist = null;
            }
        }

        public void Clear()
        {
            emu_timer.ClearList(ref timerlist);
        }
        public void Add(emu_timer timer)
        {
            emu_timer.AddList(ref timerlist, ref timer);
        }
        public void Remove(emu_timer timer)
        {
            emu_timer.RemoveToList(ref timerlist, ref timer);
        }

        public void Insert(int index, emu_timer timer)
        {
            emu_timer.InsertToList(ref timerlist, index, ref timer);
        }

        public int IndexOf(emu_timer timer)
        {
            return timerlist.IndexOf(timer);
        }
    }

    public class EmuTimer
    {
        public static EmuTimerLister lt;
        private static List<emu_timer2> lt2;
        public static Atime global_basetime;
        public static Atime global_basetime_obj;
        private static bool callback_timer_modified;
        private static emu_timer callback_timer;
        private static Atime callback_timer_expire_time;
        public delegate void timer_fired_func();
        public static Action setvector;
        /*public class emu_timer
        {
            public TIME_ACT action;
            //public string func;
            public bool enabled;
            public bool temporary;
            public Atime period;
            public Atime start;
            public Atime expire;
        }*/

        public static void instancingTimerList()
        {
            //lt = new List<emu_timer>();
            EmuTimerLister.GetNewTimerLister(ref lt);
        }

        public class emu_timer
        {
            public TIME_ACT action;
            public bool enabled;
            public bool temporary;
            public Atime period;
            public Atime start;
            public Atime expire;

            internal void reset()
            {
                action = default;
                enabled = default;
                temporary = default;
                period = default;
                start = default;
                expire = default;
                _refCount = 1;
            }

            static HashSet<emu_timer> _readyToRelease = new HashSet<emu_timer>();
            /// <summary>
            /// 线程安全队列（因为析构函数是额外线程来的）
            /// </summary>
            static ConcurrentQueue<emu_timer> _failedDeletions = new ConcurrentQueue<emu_timer>();

            static int outTimerAllCount = 0;
            static int newTimerCount = 0;
            public static emu_timer GetEmu_timer()
            {
                emu_timer obj;
                if (!_failedDeletions.TryDequeue(out obj))
                { 
                    obj = new emu_timer();
                    newTimerCount++;
                }
                obj.reset();
                outTimerAllCount++;
                return obj;
            }
            /// <summary>
            /// 释放创建的引用，这个要和GetEmu_timer成对
            /// </summary>
            /// <returns></returns>
            public static void ReleaseCreateRef(emu_timer obj)
            {
                obj.ReleaseRef();
            }

            public static void CheckReadyRelaseAfterRun()
            {
                if (_readyToRelease.Count < 1)
                    return;

                int beforpoolcount = _failedDeletions.Count;
                int releaseCount = 0;
                foreach (var ready in _readyToRelease)
                {
                    if (ready._refCount <= 0)
                    {
                        ready.ReturnToPool();
                        releaseCount++;
                    }
                }
                //UnityEngine.Debug.Log($"CheckReadyRelaseAfterRun 出池数量{outTimerAllCount}，其中new创建的数量{newTimerCount} 回收数量{releaseCount} ,处理前池数量{beforpoolcount}，处理后池数量{_failedDeletions.Count}");
                outTimerAllCount = 0;
                newTimerCount = 0;
                _readyToRelease.Clear();
            }

            // 引用计数字段（线程安全）
            private int _refCount = 1; // 初始为1，表示创建时的引用

            /// <summary>
            /// 增加引用计数
            /// </summary>
            void AddRef()
            {
                int newCount = Interlocked.Increment(ref _refCount);

                //引用计数重新回到1时，移除。
                //但是还是不在这里做把注释了，在每一帧开始之前统一检测
                //if (newCount == 1)
                //{
                //    if (_readyToRelease.Contains(this))
                //    {
                //        //UnityEngine.Debug.Log("移除ReadyToRelease");
                //        _readyToRelease.Remove(this);
                //    }
                //}
            }

            /// <summary>
            /// 减少引用计数，当计数为0时释放对象回池
            /// </summary>
            void ReleaseRef()
            {
                int newCount = Interlocked.Decrement(ref _refCount);
                if (newCount == 0)
                {
                    // 引用计数为0，释放资源并回池
                    ReadyToRelease();
                }
                else if (newCount < 0)
                {
                    // 引用计数异常，不应出现负数
                    throw new InvalidOperationException("引用计数出现负数");
                }
            }

            void ReadyToRelease()
            {
                //UnityEngine.Debug.Log("ReadyToRelease");
                if(!_readyToRelease.Contains(this))
                    _readyToRelease.Add(this);
            }

            /// <summary>
            /// 释放资源并回池
            /// </summary>
            void ReturnToPool()
            {
                reset();
                _failedDeletions.Enqueue(this);
            }

            #region 外部操作 间接影响引用计数
            public static void SetRefUsed(ref emu_timer refattr, ref emu_timer emu_timer)
            {
                if (refattr == emu_timer)
                    return;
                if (emu_timer == null)
                {
                    SetNull(ref refattr);
                    return;
                }
                if (refattr != null)
                    refattr.ReleaseRef();

                refattr = emu_timer;
                refattr.AddRef();
            }
            public static void SetNull(ref emu_timer timer)
            {
                if (timer != null)
                {
                    timer.ReleaseRef();
                    timer = null;
                }
            }
            public static void AddList(ref List<emu_timer> list, ref emu_timer timer)
            {
                list.Add(timer);
                timer.AddRef();
            }
            internal static void InsertToList(ref List<emu_timer> list, int index, ref emu_timer timer)
            {
                list.Insert(index, timer);
                timer.AddRef();
            }
            public static void RemoveToList(ref List<emu_timer> list, ref emu_timer timer)
            {
                list.Remove(timer);
                timer.ReleaseRef();
            }
            internal static void ClearList(ref List<emu_timer> list)
            {
                for (int i = 0; i < list.Count; i++)
                    list[i].ReleaseRef();
                list.Clear();
            }
            #endregion
            /*
            static void EnqueueObj(emu_timer obj)
            {
                _failedDeletions.Enqueue(obj);
            }
            
            ~emu_timer()
            {
                //咱也没办法，这样子来实现emu_timer的回收到对象池。只能这样实现，MAME里面对于emu_timer持有引用比较混沌，在确保没有引用计数时，再安全回池。
                //回池，引用计数+1，使其不被回收。相当于打断CG回收
                //（原本没有析构函数时，GC是直接回收，有析构时，则调用后下一次GC再回收，但是这就有操作空间了。这里引用计数+1
                //GC回收，但是不回收，请回对象池
                //说人话，就是用析构驱动回池，而不破坏现有代码
                EnqueueObj(this);
                GC.ReRegisterForFinalize(this);//手动注册，否则析构函数再也不会回调
            }*/
        }
        public class emu_timer2
        {
            public int index;
            public TIME_ACT action;
            public string func;
            public emu_timer2(int i1, TIME_ACT ac1)
            {
                index = i1;
                action = ac1;
                //func = func1;
            }
        }
        //public static Action getactionbyindex(int index)
        public static TIME_ACT getactionbyindex(int index)
        {
            TIME_ACT action = TIME_ACT.NoneAct;
            foreach (emu_timer2 timer in lt2)
            {
                if (timer.index == index)
                {
                    action = timer.action;
                    if (index == 4)
                    {
                        action = TIME_ACT.Sound_sound_update;
                    }
                    else if (index == 32)
                    {
                        action = TIME_ACT.M6800_action_rx;
                    }
                    else if (index == 33)
                    {
                        action = TIME_ACT.M6800_action_tx;
                    }
                    else if (index == 39)
                    {
                        action = TIME_ACT.setvector;
                    }
                    else if (index == 42)
                    {
                        action = TIME_ACT.Cpuexec_vblank_interrupt2;
                    }
                }
            }
            return action;
        }
        //{
        //    Action action = null;
        //    foreach (emu_timer2 timer in lt2)
        //    {
        //        if (timer.index == index)
        //        {
        //            action = timer.action;
        //            if (index == 4)
        //            {
        //                action = Sound.sound_update;
        //            }
        //            else if (index == 32)
        //            {
        //                action = M6800.action_rx;
        //            }
        //            else if (index == 33)
        //            {
        //                action = M6800.action_tx;
        //            }
        //            else if (index == 39)
        //            {
        //                action = setvector;
        //            }
        //            else if (index == 42)
        //            {
        //                action = Cpuexec.vblank_interrupt2;
        //            }
        //        }
        //    }
        //    return action;
        //}
        public static string getfuncbyindex(int index)
        {
            string func = "";
            foreach (emu_timer2 timer in lt2)
            {
                if (timer.index == index)
                {
                    func = timer.func;
                    break;
                }
            }
            return func;
        }
        //public static int getindexbyfunc(string func)
        //{
        //    int index = 0;
        //    foreach (emu_timer2 timer in lt2)
        //    {
        //        if (timer.func == func)
        //        {
        //            index = timer.index;
        //            break;
        //        }
        //    }
        //    return index;
        //}
        public static int getindexbyaction(TIME_ACT act)
        {
            int index = 0;
            foreach (emu_timer2 timer in lt2)
            {
                if (timer.action == act)
                {
                    index = timer.index;
                    break;
                }
            }
            return index;
        }
        //public static void timer_init()
        //{
        //    global_basetime = Attotime.ATTOTIME_ZERO;
        //    lt = new List<emu_timer>();
        //    lt2 = new List<emu_timer2>();
        //    lt2.Add(new emu_timer2(1, Video.vblank_begin_callback, "vblank_begin_callback"));
        //    lt2.Add(new emu_timer2(2, Mame.soft_reset, "soft_reset"));
        //    lt2.Add(new emu_timer2(3, Cpuint.cpunum_empty_event_queue, "cpunum_empty_event_queue"));
        //    lt2.Add(new emu_timer2(4, Sound.sound_update, "sound_update"));
        //    lt2.Add(new emu_timer2(5, Watchdog.watchdog_callback, "watchdog_callback"));
        //    lt2.Add(new emu_timer2(6, Generic.irq_1_0_line_hold, "irq_1_0_line_hold"));
        //    lt2.Add(new emu_timer2(7, Video.vblank_end_callback, "vblank_end_callback"));

        //    lt2.Add(new emu_timer2(10, YM2151.irqAon_callback, "irqAon_callback"));
        //    lt2.Add(new emu_timer2(11, YM2151.irqBon_callback, "irqBon_callback"));
        //    lt2.Add(new emu_timer2(12, YM2151.irqAoff_callback, "irqAoff_callback"));
        //    lt2.Add(new emu_timer2(13, YM2151.irqBoff_callback, "irqBoff_callback"));
        //    lt2.Add(new emu_timer2(14, YM2151.timer_callback_a, "timer_callback_a"));
        //    lt2.Add(new emu_timer2(15, YM2151.timer_callback_b, "timer_callback_b"));
        //    lt2.Add(new emu_timer2(16, Cpuexec.trigger_partial_frame_interrupt, "trigger_partial_frame_interrupt"));
        //    lt2.Add(new emu_timer2(17, Cpuexec.null_callback, "boost_callback"));
        //    lt2.Add(new emu_timer2(18, Cpuexec.end_interleave_boost, "end_interleave_boost"));
        //    lt2.Add(new emu_timer2(19, Video.scanline0_callback, "scanline0_callback"));
        //    lt2.Add(new emu_timer2(20, Sound.latch_callback, "latch_callback"));
        //    lt2.Add(new emu_timer2(21, Sound.latch_callback2, "latch_callback2"));
        //    lt2.Add(new emu_timer2(22, Sound.latch_callback3, "latch_callback3"));
        //    lt2.Add(new emu_timer2(23, Sound.latch_callback4, "latch_callback4"));
        //    lt2.Add(new emu_timer2(24, Neogeo.display_position_interrupt_callback, "display_position_interrupt_callback"));
        //    lt2.Add(new emu_timer2(25, Neogeo.display_position_vblank_callback, "display_position_vblank_callback"));
        //    lt2.Add(new emu_timer2(26, Neogeo.vblank_interrupt_callback, "vblank_interrupt_callback"));
        //    lt2.Add(new emu_timer2(27, Neogeo.auto_animation_timer_callback, "auto_animation_timer_callback"));
        //    lt2.Add(new emu_timer2(29, YM2610.F2610.timer_callback_0, "timer_callback_0"));
        //    lt2.Add(new emu_timer2(30, YM2610.F2610.timer_callback_1, "timer_callback_1"));
        //    lt2.Add(new emu_timer2(31, Neogeo.sprite_line_timer_callback, "sprite_line_timer_callback"));
        //    lt2.Add(new emu_timer2(32, M6800.action_rx, "m6800_rx_tick"));
        //    lt2.Add(new emu_timer2(33, M6800.action_tx, "m6800_tx_tick"));
        //    lt2.Add(new emu_timer2(34, YM3812.timer_callback_3812_0, "timer_callback_3812_0"));
        //    lt2.Add(new emu_timer2(35, YM3812.timer_callback_3812_1, "timer_callback_3812_1"));
        //    lt2.Add(new emu_timer2(36, ICS2115.timer_cb_0, "timer_cb_0"));
        //    lt2.Add(new emu_timer2(37, ICS2115.timer_cb_1, "timer_cb_1"));
        //    lt2.Add(new emu_timer2(38, M72.m72_scanline_interrupt, "m72_scanline_interrupt"));
        //    lt2.Add(new emu_timer2(39, setvector, "setvector_callback"));
        //    lt2.Add(new emu_timer2(40, M92.m92_scanline_interrupt, "m92_scanline_interrupt"));
        //    lt2.Add(new emu_timer2(41, Cpuexec.cpu_timeslicecallback, "cpu_timeslicecallback"));
        //    lt2.Add(new emu_timer2(42, Cpuexec.vblank_interrupt2, "vblank_interrupt2"));
        //    lt2.Add(new emu_timer2(43, Konami68000.nmi_callback, "nmi_callback"));
        //    lt2.Add(new emu_timer2(44, Upd7759.upd7759_slave_update, "upd7759_slave_update"));
        //    lt2.Add(new emu_timer2(45, Generic.irq_2_0_line_hold, "irq_2_0_line_hold"));
        //    lt2.Add(new emu_timer2(46, MSM5205.MSM5205_vclk_callback0, "msm5205_vclk_callback0"));
        //    lt2.Add(new emu_timer2(47, MSM5205.MSM5205_vclk_callback1, "msm5205_vclk_callback1"));
        //    lt2.Add(new emu_timer2(48, YM2203.timer_callback_2203_0_0, "timer_callback_2203_0_0"));
        //    lt2.Add(new emu_timer2(49, YM2203.timer_callback_2203_0_1, "timer_callback_2203_0_1"));
        //    lt2.Add(new emu_timer2(50, YM2203.timer_callback_2203_1_0, "timer_callback_2203_1_0"));
        //    lt2.Add(new emu_timer2(51, YM2203.timer_callback_2203_1_1, "timer_callback_2203_1_1"));
        //    lt2.Add(new emu_timer2(52, YM3812.timer_callback_3526_0, "timer_callback_3526_0"));
        //    lt2.Add(new emu_timer2(53, YM3812.timer_callback_3526_1, "timer_callback_3526_1"));
        //    lt2.Add(new emu_timer2(54, K054539.k054539_irq, "k054539_irq"));
        //    lt2.Add(new emu_timer2(55, Taito.cchip_timer, "cchip_timer"));
        //}
        public static void timer_init()
        {
            global_basetime = Attotime.ATTOTIME_ZERO;
            //lt = new List<emu_timer>();
            EmuTimerLister.GetNewTimerLister(ref lt);
            lt2 = new List<emu_timer2>();
            lt2.Add(new emu_timer2(1, TIME_ACT.Video_vblank_begin_callback));
            lt2.Add(new emu_timer2(2, TIME_ACT.Mame_soft_reset));
            lt2.Add(new emu_timer2(3, TIME_ACT.Cpuint_cpunum_empty_event_queue));
            lt2.Add(new emu_timer2(4, TIME_ACT.Sound_sound_update));
            lt2.Add(new emu_timer2(5, TIME_ACT.Watchdog_watchdog_callback));
            lt2.Add(new emu_timer2(6, TIME_ACT.Generic_irq_1_0_line_hold));
            lt2.Add(new emu_timer2(7, TIME_ACT.Video_vblank_end_callback));
            lt2.Add(new emu_timer2(10, TIME_ACT.YM2151_irqAon_callback));
            lt2.Add(new emu_timer2(11, TIME_ACT.YM2151_irqBon_callback));
            lt2.Add(new emu_timer2(12, TIME_ACT.YM2151_irqAoff_callback));
            lt2.Add(new emu_timer2(13, TIME_ACT.YM2151_irqBoff_callback));
            lt2.Add(new emu_timer2(14, TIME_ACT.YM2151_timer_callback_a));
            lt2.Add(new emu_timer2(15, TIME_ACT.YM2151_timer_callback_b));
            lt2.Add(new emu_timer2(16, TIME_ACT.Cpuexec_trigger_partial_frame_interrupt));
            lt2.Add(new emu_timer2(17, TIME_ACT.Cpuexec_null_callback));
            lt2.Add(new emu_timer2(18, TIME_ACT.Cpuexec_end_interleave_boost));
            lt2.Add(new emu_timer2(19, TIME_ACT.Video_scanline0_callback));
            lt2.Add(new emu_timer2(20, TIME_ACT.Sound_latch_callback));
            lt2.Add(new emu_timer2(21, TIME_ACT.Sound_latch_callback2));
            lt2.Add(new emu_timer2(22, TIME_ACT.Sound_latch_callback3));
            lt2.Add(new emu_timer2(23, TIME_ACT.Sound_latch_callback4));
            lt2.Add(new emu_timer2(24, TIME_ACT.Neogeo_display_position_interrupt_callback));
            lt2.Add(new emu_timer2(25, TIME_ACT.Neogeo_display_position_vblank_callback));
            lt2.Add(new emu_timer2(26, TIME_ACT.Neogeo_vblank_interrupt_callback));
            lt2.Add(new emu_timer2(27, TIME_ACT.Neogeo_auto_animation_timer_callback));
            lt2.Add(new emu_timer2(29, TIME_ACT.YM2610_F2610_timer_callback_0));
            lt2.Add(new emu_timer2(30, TIME_ACT.YM2610_F2610_timer_callback_1));
            lt2.Add(new emu_timer2(31, TIME_ACT.Neogeo_sprite_line_timer_callback));
            lt2.Add(new emu_timer2(32, TIME_ACT.M6800_action_rx));
            lt2.Add(new emu_timer2(33, TIME_ACT.M6800_action_tx));
            lt2.Add(new emu_timer2(34, TIME_ACT.YM3812_timer_callback_3812_0));
            lt2.Add(new emu_timer2(35, TIME_ACT.YM3812_timer_callback_3812_1));
            lt2.Add(new emu_timer2(36, TIME_ACT.ICS2115_timer_cb_0));
            lt2.Add(new emu_timer2(37, TIME_ACT.ICS2115_timer_cb_1));
            lt2.Add(new emu_timer2(38, TIME_ACT.M72_m72_scanline_interrupt));
            lt2.Add(new emu_timer2(39, TIME_ACT.setvector));
            lt2.Add(new emu_timer2(40, TIME_ACT.M92_m92_scanline_interrupt));
            lt2.Add(new emu_timer2(41, TIME_ACT.Cpuexec_cpu_timeslicecallback));
            lt2.Add(new emu_timer2(42, TIME_ACT.Cpuexec_vblank_interrupt2));
            lt2.Add(new emu_timer2(43, TIME_ACT.Konami68000_nmi_callback));
            lt2.Add(new emu_timer2(44, TIME_ACT.Upd7759_upd7759_slave_update));
            lt2.Add(new emu_timer2(45, TIME_ACT.Generic_irq_2_0_line_hold));
            lt2.Add(new emu_timer2(46, TIME_ACT.MSM5205_MSM5205_vclk_callback0));
            lt2.Add(new emu_timer2(47, TIME_ACT.MSM5205_MSM5205_vclk_callback1));
            lt2.Add(new emu_timer2(48, TIME_ACT.YM2203_timer_callback_2203_0_0));
            lt2.Add(new emu_timer2(49, TIME_ACT.YM2203_timer_callback_2203_0_1));
            lt2.Add(new emu_timer2(50, TIME_ACT.YM2203_timer_callback_2203_1_0));
            lt2.Add(new emu_timer2(51, TIME_ACT.YM2203_timer_callback_2203_1_1));
            lt2.Add(new emu_timer2(52, TIME_ACT.YM3812_timer_callback_3526_0));
            lt2.Add(new emu_timer2(53, TIME_ACT.YM3812_timer_callback_3526_1));
            lt2.Add(new emu_timer2(54, TIME_ACT.K054539_k054539_irq));
            lt2.Add(new emu_timer2(55, TIME_ACT.Taito_cchip_timer));
        }


        #region 更换调度

        public enum TIME_ACT : byte
        {
            NoneAct = 0,
            Video_vblank_begin_callback,
            Mame_soft_reset,
            Cpuint_cpunum_empty_event_queue,
            Sound_sound_update,
            Watchdog_watchdog_callback,
            Generic_irq_1_0_line_hold,
            Video_vblank_end_callback,
            YM2151_irqAon_callback,
            YM2151_irqBon_callback,
            YM2151_irqAoff_callback,
            YM2151_irqBoff_callback,
            YM2151_timer_callback_a,
            YM2151_timer_callback_b,
            Cpuexec_trigger_partial_frame_interrupt,
            Cpuexec_null_callback,
            Cpuexec_end_interleave_boost,
            Video_scanline0_callback,
            Sound_latch_callback,
            Sound_latch_callback2,
            Sound_latch_callback3,
            Sound_latch_callback4,
            Neogeo_display_position_interrupt_callback,
            Neogeo_display_position_vblank_callback,
            Neogeo_vblank_interrupt_callback,
            Neogeo_auto_animation_timer_callback,
            YM2610_F2610_timer_callback_0,
            YM2610_F2610_timer_callback_1,
            Neogeo_sprite_line_timer_callback,
            M6800_action_rx,
            M6800_action_tx,
            YM3812_timer_callback_3812_0,
            YM3812_timer_callback_3812_1,
            ICS2115_timer_cb_0,
            ICS2115_timer_cb_1,
            M72_m72_scanline_interrupt,
            setvector,
            M92_m92_scanline_interrupt,
            Cpuexec_cpu_timeslicecallback,
            Cpuexec_vblank_interrupt2,
            Konami68000_nmi_callback,
            Upd7759_upd7759_slave_update,
            Generic_irq_2_0_line_hold,
            MSM5205_MSM5205_vclk_callback0,
            MSM5205_MSM5205_vclk_callback1,
            YM2203_timer_callback_2203_0_0,
            YM2203_timer_callback_2203_0_1,
            YM2203_timer_callback_2203_1_0,
            YM2203_timer_callback_2203_1_1,
            YM3812_timer_callback_3526_0,
            YM3812_timer_callback_3526_1,
            K054539_k054539_irq,
            Taito_cchip_timer,



            Cpuexec_trigger2,
            Taitob_rsaga2_interrupt2,
            Taitob_crimec_interrupt3,
            Taitob_hitice_interrupt6,
            Taitob_rambo3_interrupt1,
            Taitob_pbobble_interrupt5,
            Taitob_viofight_interrupt1,
            Taitob_masterw_interrupt4,
            Taitob_silentd_interrupt4,
            Taitob_selfeena_interrupt4,
            Taitob_sbm_interrupt5,
            Generic_clear_all_lines,
            M92_spritebuffer_callback,
            Taito_opwolf_timer_callback,
            Taito_nmi_callback,
        }

        public static void DoAct(TIME_ACT act)
        {
            switch (act)
            {
                case TIME_ACT.Video_vblank_begin_callback: Video.vblank_begin_callback(); break;
                case TIME_ACT.Mame_soft_reset: Mame.soft_reset(); break;
                case TIME_ACT.Cpuint_cpunum_empty_event_queue: Cpuint.cpunum_empty_event_queue(); break;
                case TIME_ACT.Sound_sound_update: Sound.sound_update(); break;
                case TIME_ACT.Watchdog_watchdog_callback: Watchdog.watchdog_callback(); break;
                case TIME_ACT.Generic_irq_1_0_line_hold: Generic.irq_1_0_line_hold(); break;
                case TIME_ACT.Video_vblank_end_callback: Video.vblank_end_callback(); break;
                case TIME_ACT.YM2151_irqAon_callback: YM2151.irqAon_callback(); break;
                case TIME_ACT.YM2151_irqBon_callback: YM2151.irqBon_callback(); break;
                case TIME_ACT.YM2151_irqAoff_callback: YM2151.irqAoff_callback(); break;
                case TIME_ACT.YM2151_irqBoff_callback: YM2151.irqBoff_callback(); break;
                case TIME_ACT.YM2151_timer_callback_a: YM2151.timer_callback_a(); break;
                case TIME_ACT.YM2151_timer_callback_b: YM2151.timer_callback_b(); break;
                case TIME_ACT.Cpuexec_trigger_partial_frame_interrupt: Cpuexec.trigger_partial_frame_interrupt(); break;
                case TIME_ACT.Cpuexec_null_callback: Cpuexec.null_callback(); break;
                case TIME_ACT.Cpuexec_end_interleave_boost: Cpuexec.end_interleave_boost(); break;
                case TIME_ACT.Video_scanline0_callback: Video.scanline0_callback(); break;
                case TIME_ACT.Sound_latch_callback: Sound.latch_callback(); break;
                case TIME_ACT.Sound_latch_callback2: Sound.latch_callback2(); break;
                case TIME_ACT.Sound_latch_callback3: Sound.latch_callback3(); break;
                case TIME_ACT.Sound_latch_callback4: Sound.latch_callback4(); break;
                case TIME_ACT.Neogeo_display_position_interrupt_callback: Neogeo.display_position_interrupt_callback(); break;
                case TIME_ACT.Neogeo_display_position_vblank_callback: Neogeo.display_position_vblank_callback(); break;
                case TIME_ACT.Neogeo_vblank_interrupt_callback: Neogeo.vblank_interrupt_callback(); break;
                case TIME_ACT.Neogeo_auto_animation_timer_callback: Neogeo.auto_animation_timer_callback(); break;
                case TIME_ACT.YM2610_F2610_timer_callback_0: YM2610.F2610.timer_callback_0(); break;
                case TIME_ACT.YM2610_F2610_timer_callback_1: YM2610.F2610.timer_callback_1(); break;
                case TIME_ACT.Neogeo_sprite_line_timer_callback: Neogeo.sprite_line_timer_callback(); break;
                case TIME_ACT.M6800_action_rx: M6800.action_rx(); break;
                case TIME_ACT.M6800_action_tx: M6800.action_tx(); break;
                case TIME_ACT.YM3812_timer_callback_3812_0: YM3812.timer_callback_3812_0(); break;
                case TIME_ACT.YM3812_timer_callback_3812_1: YM3812.timer_callback_3812_1(); break;
                case TIME_ACT.ICS2115_timer_cb_0: ICS2115.timer_cb_0(); break;
                case TIME_ACT.ICS2115_timer_cb_1: ICS2115.timer_cb_1(); break;
                case TIME_ACT.M72_m72_scanline_interrupt: M72.m72_scanline_interrupt(); break;
                case TIME_ACT.setvector: setvector(); break;
                case TIME_ACT.M92_m92_scanline_interrupt: M92.m92_scanline_interrupt(); break;
                case TIME_ACT.Cpuexec_cpu_timeslicecallback: Cpuexec.cpu_timeslicecallback(); break;
                case TIME_ACT.Cpuexec_vblank_interrupt2: Cpuexec.vblank_interrupt2(); break;
                case TIME_ACT.Konami68000_nmi_callback: Konami68000.nmi_callback(); break;
                case TIME_ACT.Upd7759_upd7759_slave_update: Upd7759.upd7759_slave_update(); break;
                case TIME_ACT.Generic_irq_2_0_line_hold: Generic.irq_2_0_line_hold(); break;
                case TIME_ACT.MSM5205_MSM5205_vclk_callback0: MSM5205.MSM5205_vclk_callback0(); break;
                case TIME_ACT.MSM5205_MSM5205_vclk_callback1: MSM5205.MSM5205_vclk_callback1(); break;
                case TIME_ACT.YM2203_timer_callback_2203_0_0: YM2203.timer_callback_2203_0_0(); break;
                case TIME_ACT.YM2203_timer_callback_2203_0_1: YM2203.timer_callback_2203_0_1(); break;
                case TIME_ACT.YM2203_timer_callback_2203_1_0: YM2203.timer_callback_2203_1_0(); break;
                case TIME_ACT.YM2203_timer_callback_2203_1_1: YM2203.timer_callback_2203_1_1(); break;
                case TIME_ACT.YM3812_timer_callback_3526_0: YM3812.timer_callback_3526_0(); break;
                case TIME_ACT.YM3812_timer_callback_3526_1: YM3812.timer_callback_3526_1(); break;
                case TIME_ACT.K054539_k054539_irq: K054539.k054539_irq(); break;
                case TIME_ACT.Taito_cchip_timer: Taito.cchip_timer(); break;

                case TIME_ACT.Cpuexec_trigger2: Cpuexec.trigger2(); break;


                case TIME_ACT.Taitob_rsaga2_interrupt2: Taitob.rsaga2_interrupt2(); break;
                case TIME_ACT.Taitob_crimec_interrupt3: Taitob.crimec_interrupt3(); break;
                case TIME_ACT.Taitob_hitice_interrupt6: Taitob.hitice_interrupt6(); break;
                case TIME_ACT.Taitob_rambo3_interrupt1: Taitob.rambo3_interrupt1(); break;
                case TIME_ACT.Taitob_pbobble_interrupt5: Taitob.pbobble_interrupt5(); break;
                case TIME_ACT.Taitob_viofight_interrupt1: Taitob.viofight_interrupt1(); break;
                case TIME_ACT.Taitob_masterw_interrupt4: Taitob.masterw_interrupt4(); break;
                case TIME_ACT.Taitob_silentd_interrupt4: Taitob.silentd_interrupt4(); break;
                case TIME_ACT.Taitob_selfeena_interrupt4: Taitob.selfeena_interrupt4(); break;
                case TIME_ACT.Taitob_sbm_interrupt5: Taitob.sbm_interrupt5(); break;

                case TIME_ACT.Generic_clear_all_lines: Generic.clear_all_lines(); break;
                case TIME_ACT.M92_spritebuffer_callback: M92.spritebuffer_callback(); break;//!!
                case TIME_ACT.Taito_opwolf_timer_callback: Taito.opwolf_timer_callback(); break;
                case TIME_ACT.Taito_nmi_callback: Taito.nmi_callback(); break;
            }
        }
        #endregion
        public static Atime get_current_time()
        {
            if (callback_timer != null)
            {
                return callback_timer_expire_time;
            }
            if (Cpuexec.activecpu >= 0 && Cpuexec.activecpu < Cpuexec.ncpu)
            {
                return Cpuexec.cpunum_get_localtime(Cpuexec.activecpu);
            }
            return global_basetime;
        }
        public static void timer_remove(emu_timer timer1)
        {
            if (timer1 == callback_timer)
            {
                callback_timer_modified = true;
            }
            timer_list_remove(timer1);
        }
        public static void timer_adjust_periodic(emu_timer which, Atime start_delay, Atime period)
        {
            Atime time = get_current_time();
            if (which == callback_timer)
            {
                callback_timer_modified = true;
            }
            which.enabled = true;
            if (start_delay.seconds < 0)
            {
                start_delay = Attotime.ATTOTIME_ZERO;
            }
            which.start = time;
            which.expire = Attotime.attotime_add(time, start_delay);
            which.period = period;
            timer_list_remove(which);
            timer_list_insert(which);
            //if (lt.IndexOf(which) == 0)
            if (lt[0] == which)
            {
                if (Cpuexec.activecpu >= 0 && Cpuexec.activecpu < Cpuexec.ncpu)
                {
                    Cpuexec.activecpu_abort_timeslice(Cpuexec.activecpu);
                }
            }
        }
        public static void timer_pulse_internal(Atime period, TIME_ACT action)
        {
            //emu_timer timer = timer_alloc_common(action, false);
            emu_timer timer = timer_alloc_common_NoRef(action, false);
            timer_adjust_periodic(timer, period, period);
        }
        public static void timer_set_internal(TIME_ACT action)
        {
            //emu_timer timer = timer_alloc_common(action, true);
            emu_timer timer = timer_alloc_common_NoRef(action, true);
            timer_adjust_periodic(timer, Attotime.ATTOTIME_ZERO, Attotime.ATTOTIME_NEVER);
        }
        public static void timer_list_insert(emu_timer timer1)
        {
            int i;
            int i1 = -1;
            if (timer1.action == TIME_ACT.Cpuint_cpunum_empty_event_queue || timer1.action == TIME_ACT.setvector)
            {
                //foreach (emu_timer et in lt)
                foreach (emu_timer et in lt.GetSrcList())
                {
                    if (et.action == timer1.action && Attotime.attotime_compare(et.expire, global_basetime) <= 0)
                    {
                        i1 = lt.IndexOf(et);
                        break;
                    }
                }
            }
            if (i1 == -1)
            {
                Atime expire = timer1.enabled ? timer1.expire : Attotime.ATTOTIME_NEVER;
                for (i = 0; i < lt.Count; i++)
                {
                    if (Attotime.attotime_compare(lt[i].expire, expire) > 0)
                    {
                        break;
                    }
                }
                lt.Insert(i, timer1);
            }
        }

        static List<emu_timer> timer_list_remove_lt1 = new List<emu_timer>();
        public static void timer_list_remove(emu_timer timer1)
        {
            if (timer1.action == TIME_ACT.Cpuint_cpunum_empty_event_queue || timer1.action == TIME_ACT.setvector)
            {
                timer_list_remove_lt1.Clear();
                //foreach (emu_timer et in lt)
                foreach (emu_timer et in lt.GetSrcList())
                {
                    if (et.action == timer1.action && Attotime.attotime_compare(et.expire, timer1.expire) == 0)
                    {
                        timer_list_remove_lt1.Add(et);
                        //lt.Remove(et);
                        //break;
                    }
                    else if (et.action == timer1.action && Attotime.attotime_compare(et.expire, timer1.expire) < 0)
                    {
                        int i1 = 1;
                    }
                    else if (et.action == timer1.action && Attotime.attotime_compare(et.expire, timer1.expire) > 0)
                    {
                        int i1 = 1;
                    }
                }
                foreach (emu_timer et1 in timer_list_remove_lt1)
                {
                    lt.Remove(et1);
                }
            }
            else
            {
                //TODO MAME.NET原来这么foreach写删除是有问题的

                //foreach (emu_timer et in lt)
                foreach (emu_timer et in lt.GetSrcList())
                {
                    if (et.action == timer1.action)
                    {
                        lt.Remove(et);
                        break;
                    }
                }
            }
        }
        /*public static void sort()
        {
            int i1, i2, n1;
            Atime expire1, expire2;
            n1 = lt.Count;
            for (i2 = 1; i2 < n1; i2++)
            {
                for (i1 = 0; i1 < i2; i1++)
                {
                    if (lt[i1].enabled ==true)
                    {
                        expire1 = lt[i1].expire;
                    }
                    else
                    {
                        expire1 = Attotime.ATTOTIME_NEVER;
                    }
                    if (lt[i2].enabled == true)
                    {
                        expire2 = lt[i2].expire;
                    }
                    else
                    {
                        expire2 = Attotime.ATTOTIME_NEVER;
                    }
                    if (Attotime.attotime_compare(expire1, expire2) > 0)
                    {
                        var temp = lt[i1];
                        lt[i1] = lt[i2];
                        lt[i2] = temp;
                    }
                }
            }
        }*/
        public static void timer_set_global_time(Atime newbase)
        {
            emu_timer timer;
            global_basetime = newbase;
            while (Attotime.attotime_compare(lt[0].expire, global_basetime) <= 0)
            {
                bool was_enabled = lt[0].enabled;
                timer = lt[0];
                if (Attotime.attotime_compare(timer.period, Attotime.ATTOTIME_ZERO) == 0 || Attotime.attotime_compare(timer.period, Attotime.ATTOTIME_NEVER) == 0)
                {
                    timer.enabled = false;
                }
                callback_timer_modified = false;
                callback_timer = timer;
                callback_timer_expire_time = timer.expire;
                //if (was_enabled && (timer.action != null && timer.action != Cpuexec.null_callback))
                if (was_enabled && (timer.action != TIME_ACT.NoneAct && timer.action != TIME_ACT.Cpuexec_null_callback))
                {
                    //timer.action();
                    DoAct(timer.action);
                }
                callback_timer = null;
                if (callback_timer_modified == false)
                {
                    if (timer.temporary)
                    {
                        timer_list_remove(timer);
                    }
                    else
                    {
                        timer.start = timer.expire;
                        timer.expire = Attotime.attotime_add(timer.expire, timer.period);
                        timer_list_remove(timer);
                        timer_list_insert(timer);
                    }
                }
            }
        }
        //public static emu_timer timer_alloc_common(TIME_ACT action, bool temp)
        //{
        //    Atime time = get_current_time();
        //    //emu_timer timer = new emu_timer();
        //    emu_timer timer = emu_timer.GetEmu_timerNoRef();
        //    timer.action = action;
        //    timer.enabled = false;
        //    timer.temporary = temp;
        //    timer.period = Attotime.ATTOTIME_ZERO;
        //    //timer.func = func;
        //    timer.start = time;
        //    timer.expire = Attotime.ATTOTIME_NEVER;
        //    timer_list_insert(timer);
        //    return timer;
        //}

        /// <summary>
        /// 申请新的timer，且直接操作引用计数
        /// </summary>
        /// <param name="refattr"></param>
        /// <param name="action"></param>
        /// <param name="temp"></param>
        public static void timer_alloc_common(ref emu_timer refattr, TIME_ACT action, bool temp)
        {
            //Atime time = get_current_time();
            //emu_timer timer = emu_timer.GetEmu_timerNoRef();
            //timer.action = action;
            //timer.enabled = false;
            //timer.temporary = temp;
            //timer.period = Attotime.ATTOTIME_ZERO;
            ////timer.func = func;
            //timer.start = time;
            //timer.expire = Attotime.ATTOTIME_NEVER;
            //timer_list_insert(timer);
            emu_timer timer = timer_alloc_common_NoRef(action, temp);
            emu_timer.SetRefUsed(ref refattr, ref timer);
        }

        /// <summary>
        /// 申请新的timer，不操作额外引用计数，用于外部中间传递
        /// </summary>
        /// <param name="action"></param>
        /// <param name="temp"></param>
        /// <returns></returns>
        public static emu_timer timer_alloc_common_NoRef(TIME_ACT action, bool temp)
        {
            Atime time = get_current_time();
            //创建一个timer
            emu_timer timer = emu_timer.GetEmu_timer();
            timer.action = action;
            timer.enabled = false;
            timer.temporary = temp;
            timer.period = Attotime.ATTOTIME_ZERO;
            //timer.func = func;
            timer.start = time;
            timer.expire = Attotime.ATTOTIME_NEVER;
            timer_list_insert(timer);
            //断开创建的引用计数
            emu_timer.ReleaseCreateRef(timer);
            return timer;
        }

        public static bool timer_enable(emu_timer which, bool enable)
        {
            bool old;
            old = which.enabled;
            which.enabled = enable;
            timer_list_remove(which);
            timer_list_insert(which);
            return old;
        }
        public static bool timer_enabled(emu_timer which)
        {
            return which.enabled;
        }
        public static Atime timer_timeleft(emu_timer which)
        {
            return Attotime.attotime_sub(which.expire, get_current_time());
        }
        public static void SaveStateBinary(System.IO.BinaryWriter writer)
        {
            int i, i1, n;
            n = lt.Count;
            writer.Write(n);
            for (i = 0; i < n; i++)
            {
                i1 = getindexbyaction(lt[i].action);
                writer.Write(i1);
                writer.Write(lt[i].enabled);
                writer.Write(lt[i].temporary);
                writer.Write(lt[i].period.seconds);
                writer.Write(lt[i].period.attoseconds);
                writer.Write(lt[i].start.seconds);
                writer.Write(lt[i].start.attoseconds);
                writer.Write(lt[i].expire.seconds);
                writer.Write(lt[i].expire.attoseconds);
            }
            for (i = n; i < 32; i++)
            {
                writer.Write(0);
                writer.Write(false);
                writer.Write(false);
                writer.Write(0);
                writer.Write((long)0);
                writer.Write(0);
                writer.Write((long)0);
                writer.Write(0);
                writer.Write((long)0);
            }
        }
        public static void LoadStateBinary(System.IO.BinaryReader reader)
        {
            int i, i1, n;
            n = reader.ReadInt32();
            //lt = new List<emu_timer>();
            EmuTimerLister.GetNewTimerLister(ref lt);
            for (i = 0; i < n; i++)
            {
                emu_timer etimer = emu_timer.GetEmu_timer();
                #region
                lt.Add(etimer);
                i1 = reader.ReadInt32();
                etimer.action = getactionbyindex(i1);
                //etimer.func = getfuncbyindex(i1);
                etimer.enabled = reader.ReadBoolean();
                etimer.temporary = reader.ReadBoolean();
                etimer.period.seconds = reader.ReadInt32();
                etimer.period.attoseconds = reader.ReadInt64();
                etimer.start.seconds = reader.ReadInt32();
                etimer.start.attoseconds = reader.ReadInt64();
                etimer.expire.seconds = reader.ReadInt32();
                etimer.expire.attoseconds = reader.ReadInt64();
                //if (etimer.func == "vblank_begin_callback")
                if (etimer.action == TIME_ACT.Video_vblank_begin_callback)
                {
                    emu_timer.SetRefUsed(ref Video.vblank_begin_timer, ref etimer);//Video.vblank_begin_timer = etimer;
                    lt.Remove(etimer);
                    lt.Add(Video.vblank_begin_timer);
                }
                else if (etimer.action == TIME_ACT.Video_vblank_end_callback)
                {
                    //Video.vblank_end_timer = etimer;
                    emu_timer.SetRefUsed(ref Video.vblank_end_timer, ref etimer);
                    lt.Remove(etimer);
                    lt.Add(Video.vblank_end_timer);
                }
                else if (etimer.action == TIME_ACT.Mame_soft_reset)
                {
                    //Mame.soft_reset_timer = etimer;
                    emu_timer.SetRefUsed(ref Mame.soft_reset_timer, ref etimer);
                    lt.Remove(etimer);
                    lt.Add(Mame.soft_reset_timer);
                }
                else if (etimer.action == TIME_ACT.Watchdog_watchdog_callback)
                {
                    //Watchdog.watchdog_timer = etimer;
                    emu_timer.SetRefUsed(ref Watchdog.watchdog_timer, ref etimer);
                    lt.Remove(etimer);
                    lt.Add(Watchdog.watchdog_timer);
                }
                else if (etimer.action == TIME_ACT.Generic_irq_1_0_line_hold)
                {
                    //Cpuexec.timedint_timer = etimer;
                    emu_timer.SetRefUsed(ref Cpuexec.timedint_timer, ref etimer);
                    lt.Remove(etimer);
                    lt.Add(Cpuexec.timedint_timer);
                }
                else if (etimer.action == TIME_ACT.YM2151_timer_callback_a)
                {
                    //YM2151.PSG.timer_A = etimer;
                    emu_timer.SetRefUsed(ref YM2151.PSG.timer_A, ref etimer);
                    lt.Remove(etimer);
                    lt.Add(YM2151.PSG.timer_A);
                }
                else if (etimer.action == TIME_ACT.YM2151_timer_callback_b)
                {
                    //YM2151.PSG.timer_B = etimer;
                    emu_timer.SetRefUsed(ref YM2151.PSG.timer_B, ref etimer);
                    lt.Remove(etimer);
                    lt.Add(YM2151.PSG.timer_B);
                }
                else if (etimer.action == TIME_ACT.Cpuexec_trigger_partial_frame_interrupt)
                {
                    switch (Machine.sBoard)
                    {
                        case "CPS2":
                        case "IGS011":
                        case "Konami68000":
                            //Cpuexec.cpu[0].partial_frame_timer = etimer;
                            emu_timer.SetRefUsed(ref Cpuexec.cpu[0].partial_frame_timer, ref etimer);
                            lt.Remove(etimer);
                            lt.Add(Cpuexec.cpu[0].partial_frame_timer);
                            break;
                        case "M72":
                            //Cpuexec.cpu[1].partial_frame_timer = etimer;
                            emu_timer.SetRefUsed(ref Cpuexec.cpu[1].partial_frame_timer, ref etimer);
                            lt.Remove(etimer);
                            lt.Add(Cpuexec.cpu[1].partial_frame_timer);
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
                                    //Cpuexec.cpu[1].partial_frame_timer = etimer;
                                    emu_timer.SetRefUsed(ref Cpuexec.cpu[1].partial_frame_timer, ref etimer);
                                    lt.Remove(etimer);
                                    lt.Add(Cpuexec.cpu[1].partial_frame_timer);
                                    break;
                            }
                            break;
                    }
                }
                else if (etimer.action == TIME_ACT.Cpuexec_null_callback)
                {
                    //Cpuexec.interleave_boost_timer = etimer;
                    emu_timer.SetRefUsed(ref Cpuexec.interleave_boost_timer, ref etimer);
                    lt.Remove(etimer);
                    lt.Add(Cpuexec.interleave_boost_timer);
                }
                else if (etimer.action == TIME_ACT.Cpuexec_end_interleave_boost)
                {
                    //Cpuexec.interleave_boost_timer_end = etimer;
                    emu_timer.SetRefUsed(ref Cpuexec.interleave_boost_timer_end, ref etimer);
                    lt.Remove(etimer);
                    lt.Add(Cpuexec.interleave_boost_timer_end);
                }
                else if (etimer.action == TIME_ACT.Video_scanline0_callback)
                {
                    //Video.scanline0_timer = etimer;
                    emu_timer.SetRefUsed(ref Video.scanline0_timer, ref etimer);
                    lt.Remove(etimer);
                    lt.Add(Video.scanline0_timer);
                }
                else if (etimer.action == TIME_ACT.Neogeo_display_position_interrupt_callback)
                {
                    //Neogeo.display_position_interrupt_timer = etimer;
                    emu_timer.SetRefUsed(ref Neogeo.display_position_interrupt_timer, ref etimer);
                    lt.Remove(etimer);
                    lt.Add(Neogeo.display_position_interrupt_timer);
                }
                else if (etimer.action == TIME_ACT.Neogeo_display_position_vblank_callback)
                {
                    //Neogeo.display_position_vblank_timer = etimer;
                    emu_timer.SetRefUsed(ref Neogeo.display_position_vblank_timer, ref etimer);
                    lt.Remove(etimer);
                    lt.Add(Neogeo.display_position_vblank_timer);
                }
                else if (etimer.action == TIME_ACT.Neogeo_vblank_interrupt_callback)
                {
                    //Neogeo.vblank_interrupt_timer = etimer;
                    emu_timer.SetRefUsed(ref Neogeo.vblank_interrupt_timer, ref etimer);
                    lt.Remove(etimer);
                    lt.Add(Neogeo.vblank_interrupt_timer);
                }
                else if (etimer.action == TIME_ACT.Neogeo_auto_animation_timer_callback)
                {
                    //Neogeo.auto_animation_timer = etimer;
                    emu_timer.SetRefUsed(ref Neogeo.auto_animation_timer, ref etimer);
                    lt.Remove(etimer);
                    lt.Add(Neogeo.auto_animation_timer);
                }
                else if (etimer.action == TIME_ACT.Neogeo_sprite_line_timer_callback)
                {
                    //Neogeo.sprite_line_timer = etimer;
                    emu_timer.SetRefUsed(ref Neogeo.sprite_line_timer, ref etimer);
                    lt.Remove(etimer);
                    lt.Add(Neogeo.sprite_line_timer);
                }
                else if (etimer.action == TIME_ACT.YM2610_F2610_timer_callback_0)
                {
                    //YM2610.timer[0] = etimer;
                    emu_timer.SetRefUsed(ref YM2610.timer[0], ref etimer);
                    lt.Remove(etimer);
                    lt.Add(YM2610.timer[0]);
                }
                else if (etimer.action == TIME_ACT.YM2610_F2610_timer_callback_1)
                {
                    //YM2610.timer[1] = etimer;
                    emu_timer.SetRefUsed(ref YM2610.timer[1], ref etimer);
                    lt.Remove(etimer);
                    lt.Add(YM2610.timer[1]);
                }
                else if (etimer.action == TIME_ACT.M6800_action_rx)
                {
                    //M6800.m1.m6800_rx_timer = etimer;
                    emu_timer.SetRefUsed(ref M6800.m1.m6800_rx_timer, ref etimer);
                    lt.Remove(etimer);
                    lt.Add(M6800.m1.m6800_rx_timer);
                }
                else if (etimer.action == TIME_ACT.M6800_action_tx)
                {
                    //M6800.m1.m6800_tx_timer = etimer;
                    emu_timer.SetRefUsed(ref M6800.m1.m6800_tx_timer, ref etimer);
                    lt.Remove(etimer);
                    lt.Add(M6800.m1.m6800_tx_timer);
                }
                else if (etimer.action == TIME_ACT.YM3812_timer_callback_3812_0)
                {
                    //YM3812.timer[0] = etimer;
                    emu_timer.SetRefUsed(ref YM3812.timer[0], ref etimer);
                    lt.Remove(etimer);
                    lt.Add(YM3812.timer[0]);
                }
                else if (etimer.action == TIME_ACT.YM3812_timer_callback_3812_1)
                {
                    //YM3812.timer[1] = etimer;
                    emu_timer.SetRefUsed(ref YM3812.timer[1], ref etimer);
                    lt.Remove(etimer);
                    lt.Add(YM3812.timer[1]);
                }
                else if (etimer.action == TIME_ACT.ICS2115_timer_cb_0)
                {
                    //ICS2115.timer[0].timer = etimer;
                    emu_timer.SetRefUsed(ref ICS2115.timer[0].timer, ref etimer);
                    lt.Remove(etimer);
                    lt.Add(ICS2115.timer[0].timer);
                }
                else if (etimer.action == TIME_ACT.ICS2115_timer_cb_1)
                {
                    //ICS2115.timer[1].timer = etimer;
                    emu_timer.SetRefUsed(ref ICS2115.timer[1].timer, ref etimer);
                    lt.Remove(etimer);
                    lt.Add(ICS2115.timer[1].timer);
                }
                else if (etimer.action == TIME_ACT.M72_m72_scanline_interrupt)
                {
                    //M72.scanline_timer = etimer;
                    emu_timer.SetRefUsed(ref M72.scanline_timer, ref etimer);
                    lt.Remove(etimer);
                    lt.Add(M72.scanline_timer);
                }
                else if (etimer.action == TIME_ACT.M92_m92_scanline_interrupt)
                {
                    //M92.scanline_timer = etimer;
                    emu_timer.SetRefUsed(ref M72.scanline_timer, ref etimer);
                    lt.Remove(etimer);
                    lt.Add(M92.scanline_timer);
                }
                else if (etimer.action == TIME_ACT.Cpuexec_cpu_timeslicecallback)
                {
                    //Cpuexec.timeslice_timer = etimer;
                    emu_timer.SetRefUsed(ref Cpuexec.timeslice_timer, ref etimer);
                    lt.Remove(etimer);
                    lt.Add(Cpuexec.timeslice_timer);
                }
                else if (etimer.action == TIME_ACT.Upd7759_upd7759_slave_update)
                {
                    //Upd7759.chip.timer = etimer;
                    emu_timer.SetRefUsed(ref Upd7759.chip.timer, ref etimer);
                    lt.Remove(etimer);
                    lt.Add(Upd7759.chip.timer);
                }
                else if (etimer.action == TIME_ACT.Generic_irq_2_0_line_hold)
                {
                    //Cpuexec.timedint_timer = etimer;
                    emu_timer.SetRefUsed(ref Cpuexec.timedint_timer, ref etimer);
                    lt.Remove(etimer);
                    lt.Add(Cpuexec.timedint_timer);
                }
                else if (etimer.action == TIME_ACT.MSM5205_MSM5205_vclk_callback0)
                {
                    //MSM5205.timer[0] = etimer;
                    emu_timer.SetRefUsed(ref MSM5205.timer[0], ref etimer);
                    lt.Remove(etimer);
                    lt.Add(MSM5205.timer[0]);
                }
                else if (etimer.action == TIME_ACT.MSM5205_MSM5205_vclk_callback1)
                {
                    //MSM5205.timer[1] = etimer;
                    emu_timer.SetRefUsed(ref MSM5205.timer[1], ref etimer);
                    lt.Remove(etimer);
                    lt.Add(MSM5205.timer[1]);
                }
                else if (etimer.action == TIME_ACT.YM2203_timer_callback_2203_0_0)
                {
                    //YM2203.FF2203[0].timer[0] = etimer;
                    emu_timer.SetRefUsed(ref YM2203.FF2203[0].timer[0], ref etimer);
                    lt.Remove(etimer);
                    lt.Add(YM2203.FF2203[0].timer[0]);
                }
                else if (etimer.action == TIME_ACT.YM2203_timer_callback_2203_0_1)
                {
                    //YM2203.FF2203[0].timer[1] = etimer;
                    emu_timer.SetRefUsed(ref YM2203.FF2203[0].timer[1], ref etimer);
                    lt.Remove(etimer);
                    lt.Add(YM2203.FF2203[0].timer[1]);
                }
                else if (etimer.action == TIME_ACT.YM2203_timer_callback_2203_1_0)
                {
                    //YM2203.FF2203[1].timer[0] = etimer;
                    emu_timer.SetRefUsed(ref YM2203.FF2203[1].timer[0], ref etimer);
                    lt.Remove(etimer);
                    lt.Add(YM2203.FF2203[1].timer[0]);
                }
                else if (etimer.action == TIME_ACT.YM2203_timer_callback_2203_1_1)
                {
                    //YM2203.FF2203[1].timer[1] = etimer;
                    emu_timer.SetRefUsed(ref YM2203.FF2203[1].timer[1], ref etimer);
                    lt.Remove(etimer);
                    lt.Add(YM2203.FF2203[1].timer[1]);
                }
                else if (etimer.action == TIME_ACT.YM3812_timer_callback_3526_0)
                {
                    //YM3812.timer[0] = etimer;
                    emu_timer.SetRefUsed(ref YM3812.timer[0], ref etimer);
                    lt.Remove(etimer);
                    lt.Add(YM3812.timer[0]);
                }
                else if (etimer.action == TIME_ACT.YM3812_timer_callback_3526_1)
                {
                    //YM3812.timer[1] = etimer;
                    emu_timer.SetRefUsed(ref YM3812.timer[1], ref etimer);
                    lt.Remove(etimer);
                    lt.Add(YM3812.timer[1]);
                }
                #endregion
                //断开创建的引用计数
                emu_timer.ReleaseCreateRef(etimer);
            }
            for (i = n; i < 32; i++)
            {
                reader.ReadInt32();
                reader.ReadBoolean();
                reader.ReadBoolean();
                reader.ReadInt32();
                reader.ReadInt64();
                reader.ReadInt32();
                reader.ReadInt64();
                reader.ReadInt32();
                reader.ReadInt64();
            }
        }
    }
}
