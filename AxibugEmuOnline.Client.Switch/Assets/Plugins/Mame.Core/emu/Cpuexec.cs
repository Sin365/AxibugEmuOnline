using cpu.m6502;
using cpu.m6800;
using cpu.m68000;
using cpu.m6805;
using cpu.m6809;
using cpu.nec;
using cpu.z80;
using System;

namespace MAME.Core
{
    public class cpuexec_data
    {
        public int cpunum;
        public byte suspend;
        public byte nextsuspend;
        public byte eatcycles;
        public byte nexteatcycles;
        public int trigger;
        public ulong totalcycles;			// total CPU cycles executed
        public Atime localtime;				// local time, relative to the timer system's global time
        public int cycles_per_second;
        public long attoseconds_per_cycle;
        public int cycles_running;
        public int cycles_stolen;
        public int icount;
        public EmuTimer.emu_timer partial_frame_timer;
        public Atime partial_frame_period;
        public virtual ulong TotalExecutedCycles { get; set; }
        public virtual int PendingCycles { get; set; }
        public virtual int ExecuteCycles(int cycles) { return 0; }
        public virtual void Reset() { }
        public virtual void set_irq_line(int irqline, LineState state) { }
        public virtual void cpunum_set_input_line_and_vector(int cpunum, int line, LineState state, int vector) { }
    }
    public class Cpuexec
    {
        public static byte SUSPEND_REASON_HALT = 0x01, SUSPEND_REASON_RESET = 0x02, SUSPEND_REASON_SPIN = 0x04, SUSPEND_REASON_TRIGGER = 0x08, SUSPEND_REASON_DISABLE = 0x10, SUSPEND_ANY_REASON = 0xff;
        public static int iType, bLog, bLog0, bLog1, bLog2, bLog3, bLogS;
        public static bool bLog02, bLog12, bLog22, bLog32;
        public static bool b11 = true, b12 = true, b13 = true, b14 = true;
        public static int iloops, activecpu, icpu, ncpu, iloops2;
        public static cpuexec_data[] cpu;
        public static EmuTimer.emu_timer timedint_timer;
        public static Atime timedint_period, timeslice_period;
        public delegate void vblank_delegate();
        public static vblank_delegate vblank_interrupt;
        public static Action vblank_interrupt2;
        public static EmuTimer.emu_timer interleave_boost_timer;
        public static EmuTimer.emu_timer interleave_boost_timer_end;
        public static EmuTimer.emu_timer timeslice_timer;
        public static Atime perfect_interleave;
        public static int vblank_interrupts_per_frame;
        public static void cpuexec_init()
        {
            switch (Machine.sBoard)
            {
                case "CPS-1":
                    MC68000.m1 = new MC68000();
                    Z80A.nZ80 = 1;
                    Z80A.zz1 = new Z80A[Z80A.nZ80];
                    Z80A.zz1[0] = new Z80A();
                    MC68000.m1.cpunum = 0;
                    Z80A.zz1[0].cpunum = 1;
                    ncpu = 2;
                    cpu = new cpuexec_data[ncpu];
                    cpu[0] = MC68000.m1;
                    cpu[1] = Z80A.zz1[0];
                    cpu[0].cycles_per_second = 10000000;
                    switch (Machine.sName)
                    {
                        case "daimakair":
                        case "striderjr":
                        case "dynwarjr":
                        case "area88r":
                        case "sf2ce":
                        case "sf2ceea":
                        case "sf2ceua":
                        case "sf2ceub":
                        case "sf2ceuc":
                        case "sf2ceja":
                        case "sf2cejb":
                        case "sf2cejc":
                        case "sf2bhh":
                        case "sf2rb":
                        case "sf2rb2":
                        case "sf2rb3":
                        case "sf2red":
                        case "sf2v004":
                        case "sf2acc":
                        case "sf2acca":
                        case "sf2accp2":
                        case "sf2amf2":
                        case "sf2dkot2":
                        case "sf2cebltw":
                        case "sf2m2":
                        case "sf2m3":
                        case "sf2m4":
                        case "sf2m5":
                        case "sf2m6":
                        case "sf2m7":
                        case "sf2m8":
                        case "sf2m10":
                        case "sf2yyc":
                        case "sf2koryu":
                        case "sf2dongb":
                        case "cworld2j":
                        case "cworld2ja":
                        case "cworld2jb":
                        case "varth":
                        case "varthr1":
                        case "varthu":
                        case "varthj":
                        case "varthjr":
                        case "qad":
                        case "qadjr":
                        case "wofhfh":
                        case "sf2hf":
                        case "sf2hfu":
                        case "sf2hfj":
                        case "dinohunt":
                        case "punisherbz":
                        case "pnickj":
                        case "qtono2j":
                        case "megaman":
                        case "megamana":
                        case "rockmanj":
                        case "pang3":
                        case "pang3r1":
                        case "pang3j":
                        case "pang3b":
                        case "sfzch":
                        case "sfach":
                        case "sfzbch":
                            cpu[0].cycles_per_second = 12000000;
                            break;
                    }
                    cpu[1].cycles_per_second = 3579545;
                    cpu[0].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[0].cycles_per_second;
                    cpu[1].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[1].cycles_per_second;
                    break;
                case "CPS-1(QSound)":
                    MC68000.m1 = new MC68000();
                    Z80A.nZ80 = 1;
                    Z80A.zz1 = new Z80A[Z80A.nZ80];
                    Z80A.zz1[0] = new Z80A();
                    MC68000.m1.cpunum = 0;
                    Z80A.zz1[0].cpunum = 1;
                    ncpu = 2;
                    cpu = new cpuexec_data[ncpu];
                    cpu[0] = MC68000.m1;
                    cpu[1] = Z80A.zz1[0];
                    cpu[0].cycles_per_second = 12000000;
                    cpu[1].cycles_per_second = 8000000;
                    cpu[0].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[0].cycles_per_second;
                    cpu[1].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[1].cycles_per_second;
                    vblank_interrupts_per_frame = 1;
                    break;
                case "CPS2":
                    MC68000.m1 = new MC68000();
                    Z80A.nZ80 = 1;
                    Z80A.zz1 = new Z80A[Z80A.nZ80];
                    Z80A.zz1[0] = new Z80A();
                    MC68000.m1.cpunum = 0;
                    Z80A.zz1[0].cpunum = 1;
                    ncpu = 2;
                    cpu = new cpuexec_data[ncpu];
                    cpu[0] = MC68000.m1;
                    cpu[1] = Z80A.zz1[0];
                    cpu[0].cycles_per_second = (int)(16000000 * 0.7375f);
                    cpu[1].cycles_per_second = 8000000;
                    cpu[0].attoseconds_per_cycle = (long)((double)Attotime.ATTOSECONDS_PER_SECOND / (16000000 * 0.7375f));
                    cpu[1].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[1].cycles_per_second;
                    vblank_interrupts_per_frame = 262;
                    vblank_interrupt = CPS.cps2_interrupt;
                    break;
                case "Data East":
                    M6502.mm1 = new M6502[2];
                    M6502.mm1[0] = new M6502();
                    M6502.mm1[0].m6502_common_init(Cpuint.cpu_0_irq_callback);
                    M6502.mm1[1] = new M6502();
                    M6502.mm1[1].m6502_common_init(Cpuint.cpu_1_irq_callback);
                    ncpu = 2;
                    cpu = new cpuexec_data[ncpu];
                    cpu[0] = M6502.mm1[0];
                    cpu[1] = M6502.mm1[1];
                    cpu[0].cycles_per_second = 2000000;
                    cpu[1].cycles_per_second = 1500000;
                    cpu[0].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[0].cycles_per_second;
                    cpu[1].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[1].cycles_per_second;
                    vblank_interrupts_per_frame = 1;
                    break;
                case "Tehkan":
                    Z80A.nZ80 = 2;
                    Z80A.zz1 = new Z80A[Z80A.nZ80];
                    Z80A.zz1[0] = new Z80A();
                    Z80A.zz1[1] = new Z80A();
                    Z80A.zz1[0].cpunum = 0;
                    Z80A.zz1[1].cpunum = 1;
                    ncpu = 2;
                    cpu = new cpuexec_data[ncpu];
                    cpu[0] = Z80A.zz1[0];
                    cpu[1] = Z80A.zz1[1];
                    cpu[0].cycles_per_second = 4000000;
                    cpu[1].cycles_per_second = 3072000;
                    cpu[0].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[0].cycles_per_second;
                    cpu[1].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[1].cycles_per_second;
                    vblank_interrupts_per_frame = 2;
                    vblank_interrupt = Generic.nmi_0_line_pulse;
                    break;
                case "Neo Geo":
                    MC68000.m1 = new MC68000();
                    Z80A.nZ80 = 1;
                    Z80A.zz1 = new Z80A[Z80A.nZ80];
                    Z80A.zz1[0] = new Z80A();
                    MC68000.m1.cpunum = 0;
                    Z80A.zz1[0].cpunum = 1;
                    ncpu = 2;
                    cpu = new cpuexec_data[ncpu];
                    cpu[0] = MC68000.m1;
                    cpu[1] = Z80A.zz1[0];
                    cpu[0].cycles_per_second = 12000000;
                    cpu[1].cycles_per_second = 4000000;
                    cpu[0].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[0].cycles_per_second;
                    cpu[1].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[1].cycles_per_second;
                    vblank_interrupts_per_frame = 1;
                    break;
                case "SunA8":
                    Z80A.nZ80 = 2;
                    Z80A.zz1 = new Z80A[Z80A.nZ80];
                    Z80A.zz1[0] = new Z80A();
                    Z80A.zz1[1] = new Z80A();
                    Z80A.zz1[0].cpunum = 0;
                    Z80A.zz1[1].cpunum = 1;
                    ncpu = 2;
                    cpu = new cpuexec_data[ncpu];
                    cpu[0] = Z80A.zz1[0];
                    cpu[1] = Z80A.zz1[1];
                    cpu[0].cycles_per_second = 6000000;
                    cpu[1].cycles_per_second = 6000000;
                    cpu[0].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[0].cycles_per_second;
                    cpu[1].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[1].cycles_per_second;
                    vblank_interrupts_per_frame = 0x100;
                    break;
                case "Namco System 1":
                    M6809.mm1 = new M6809[3];
                    M6809.mm1[0] = new M6809();
                    M6809.mm1[1] = new M6809();
                    M6809.mm1[2] = new M6809();
                    M6809.mm1[0].irq_callback = null;
                    M6809.mm1[1].irq_callback = null;
                    M6809.mm1[2].irq_callback = null;
                    M6800.m1 = new M6800();
                    M6800.action_rx = M6800.m1.m6800_rx_tick;
                    M6800.action_tx = M6800.m1.m6800_tx_tick;
                    M6809.mm1[0].cpunum = 0;
                    M6809.mm1[1].cpunum = 1;
                    M6809.mm1[2].cpunum = 2;
                    M6800.m1.cpunum = 3;
                    ncpu = 4;
                    cpu = new cpuexec_data[ncpu];
                    cpu[0] = M6809.mm1[0];
                    cpu[1] = M6809.mm1[1];
                    cpu[2] = M6809.mm1[2];
                    cpu[3] = M6800.m1;
                    cpu[0].cycles_per_second = 1536000;
                    cpu[1].cycles_per_second = 1536000;
                    cpu[2].cycles_per_second = 1536000;
                    cpu[3].cycles_per_second = 1536000;
                    cpu[0].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[0].cycles_per_second;
                    cpu[1].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[1].cycles_per_second;
                    cpu[2].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[2].cycles_per_second;
                    cpu[3].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[3].cycles_per_second;
                    vblank_interrupts_per_frame = 1;
                    break;
                case "IGS011":
                    MC68000.m1 = new MC68000();
                    MC68000.m1.cpunum = 0;
                    ncpu = 1;
                    cpu = new cpuexec_data[ncpu];
                    cpu[0] = MC68000.m1;
                    cpu[0].cycles_per_second = 7333333;
                    cpu[0].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[0].cycles_per_second;
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
                            vblank_interrupts_per_frame = 5;
                            vblank_interrupt = IGS011.lhb2_interrupt;
                            break;
                        case "lhb":
                        case "lhbv33c":
                        case "dbc":
                        case "ryukobou":
                            vblank_interrupts_per_frame = 4;
                            vblank_interrupt = IGS011.lhb_interrupt;
                            break;
                        case "xymg":
                        case "wlcc":
                            vblank_interrupts_per_frame = 2;
                            vblank_interrupt = IGS011.wlcc_interrupt;
                            break;
                        case "vbowl":
                        case "vbowlj":
                            vblank_interrupts_per_frame = 7;
                            vblank_interrupt = IGS011.vbowl_interrupt;
                            break;
                        case "nkishusp":
                            vblank_interrupts_per_frame = 5;
                            vblank_interrupt = Generic.irq_0_6_line_hold;
                            break;
                    }
                    break;
                case "PGM":
                    MC68000.m1 = new MC68000();
                    Z80A.nZ80 = 1;
                    Z80A.zz1 = new Z80A[Z80A.nZ80];
                    Z80A.zz1[0] = new Z80A();
                    MC68000.m1.cpunum = 0;
                    Z80A.zz1[0].cpunum = 1;
                    ncpu = 2;
                    cpu = new cpuexec_data[ncpu];
                    cpu[0] = MC68000.m1;
                    cpu[1] = Z80A.zz1[0];
                    cpu[0].cycles_per_second = 20000000;
                    cpu[1].cycles_per_second = 8468000;
                    cpu[0].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[0].cycles_per_second;
                    cpu[1].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[1].cycles_per_second;
                    vblank_interrupts_per_frame = 2;
                    vblank_interrupt = PGM.drgw_interrupt;
                    break;
                case "M72":
                    Nec.nn1 = new Nec[1];
                    Nec.nn1[0] = new V30();
                    Z80A.nZ80 = 1;
                    Z80A.zz1 = new Z80A[Z80A.nZ80];
                    Z80A.zz1[0] = new Z80A();
                    Nec.nn1[0].cpunum = 0;
                    Z80A.zz1[0].cpunum = 1;
                    ncpu = 2;
                    cpu = new cpuexec_data[ncpu];
                    cpu[0] = Nec.nn1[0];
                    cpu[1] = Z80A.zz1[0];
                    cpu[0].cycles_per_second = 8000000;
                    cpu[1].cycles_per_second = 3579545;
                    cpu[0].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[0].cycles_per_second;
                    cpu[1].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[1].cycles_per_second;
                    vblank_interrupts_per_frame = 128;
                    switch (Machine.sName)
                    {
                        case "airduel":
                        case "airduelm72":
                            vblank_interrupt = M72.fake_nmi;
                            break;
                        case "ltswords":
                        case "kengo":
                        case "kengoa":
                            vblank_interrupt = Generic.nmi_1_line_pulse;
                            break;
                    }
                    break;
                case "M92":
                    Nec.nn1 = new Nec[2];
                    Nec.nn1[0] = new V33();
                    Nec.nn1[1] = new V30();
                    Nec.nn1[0].cpunum = 0;
                    Nec.nn1[1].cpunum = 1;
                    ncpu = 2;
                    cpu = new cpuexec_data[ncpu];
                    cpu[0] = Nec.nn1[0];
                    cpu[1] = Nec.nn1[1];
                    cpu[0].cycles_per_second = 9000000;
                    cpu[1].cycles_per_second = 7159090;
                    cpu[0].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[0].cycles_per_second;
                    cpu[1].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[1].cycles_per_second;
                    vblank_interrupts_per_frame = 0;
                    break;
                case "Taito":
                    switch (Machine.sName)
                    {
                        case "tokio":
                        case "tokioo":
                        case "tokiou":
                        case "tokiob":
                        case "boblbobl":
                        case "sboblbobl":
                        case "sboblbobla":
                        case "sboblboblb":
                        case "sboblbobld":
                        case "sboblboblc":
                        case "dland":
                        case "bbredux":
                        case "bublboblb":
                        case "boblcave":
                            Z80A.nZ80 = 3;
                            Z80A.zz1 = new Z80A[Z80A.nZ80];
                            Z80A.zz1[0] = new Z80A();
                            Z80A.zz1[1] = new Z80A();
                            Z80A.zz1[2] = new Z80A();
                            Z80A.zz1[0].cpunum = 0;
                            Z80A.zz1[1].cpunum = 1;
                            Z80A.zz1[2].cpunum = 2;
                            ncpu = 3;
                            cpu = new cpuexec_data[ncpu];
                            cpu[0] = Z80A.zz1[0];
                            cpu[1] = Z80A.zz1[1];
                            cpu[2] = Z80A.zz1[2];
                            cpu[0].cycles_per_second = 6000000;
                            cpu[1].cycles_per_second = 6000000;
                            cpu[2].cycles_per_second = 3000000;
                            cpu[0].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[0].cycles_per_second;
                            cpu[1].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[1].cycles_per_second;
                            cpu[2].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[2].cycles_per_second;
                            vblank_interrupts_per_frame = 0;
                            break;
                        case "bublbobl":
                        case "bublbobl1":
                        case "bublboblr":
                        case "bublboblr1":
                        case "bublcave":
                        case "bublcave11":
                        case "bublcave10":
                            Z80A.nZ80 = 3;
                            Z80A.zz1 = new Z80A[Z80A.nZ80];
                            Z80A.zz1[0] = new Z80A();
                            Z80A.zz1[1] = new Z80A();
                            Z80A.zz1[2] = new Z80A();
                            M6800.m1 = new M6801();
                            M6800.action_rx = M6800.m1.m6800_rx_tick;
                            M6800.action_tx = M6800.m1.m6800_tx_tick;
                            Z80A.zz1[0].cpunum = 0;
                            Z80A.zz1[1].cpunum = 1;
                            Z80A.zz1[2].cpunum = 2;
                            M6800.m1.cpunum = 3;
                            ncpu = 4;
                            cpu = new cpuexec_data[ncpu];
                            cpu[0] = Z80A.zz1[0];
                            cpu[1] = Z80A.zz1[1];
                            cpu[2] = Z80A.zz1[2];
                            cpu[3] = M6800.m1;
                            cpu[0].cycles_per_second = 6000000;
                            cpu[1].cycles_per_second = 6000000;
                            cpu[2].cycles_per_second = 3000000;
                            cpu[3].cycles_per_second = 1000000;
                            cpu[0].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[0].cycles_per_second;
                            cpu[1].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[1].cycles_per_second;
                            cpu[2].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[2].cycles_per_second;
                            cpu[3].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[3].cycles_per_second;
                            vblank_interrupts_per_frame = 0;
                            break;
                        case "bub68705":
                            Z80A.nZ80 = 3;
                            Z80A.zz1 = new Z80A[Z80A.nZ80];
                            Z80A.zz1[0] = new Z80A();
                            Z80A.zz1[1] = new Z80A();
                            Z80A.zz1[2] = new Z80A();
                            M6805.m1 = new M68705();
                            Z80A.zz1[0].cpunum = 0;
                            Z80A.zz1[1].cpunum = 1;
                            Z80A.zz1[2].cpunum = 2;
                            M6805.m1.cpunum = 3;
                            ncpu = 4;
                            cpu = new cpuexec_data[ncpu];
                            cpu[0] = Z80A.zz1[0];
                            cpu[1] = Z80A.zz1[1];
                            cpu[2] = Z80A.zz1[2];
                            cpu[3] = M68705.m1;
                            cpu[0].cycles_per_second = 6000000;
                            cpu[1].cycles_per_second = 6000000;
                            cpu[2].cycles_per_second = 3000000;
                            cpu[3].cycles_per_second = 1000000;
                            cpu[0].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[0].cycles_per_second;
                            cpu[1].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[1].cycles_per_second;
                            cpu[2].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[2].cycles_per_second;
                            cpu[3].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[3].cycles_per_second;
                            vblank_interrupts_per_frame = 2;
                            vblank_interrupt = Taito.bublbobl_m68705_interrupt;
                            break;
                        case "opwolf":
                        case "opwolfa":
                        case "opwolfj":
                        case "opwolfu":
                        case "opwolfp":
                            MC68000.m1 = new MC68000();
                            Z80A.nZ80 = 1;
                            Z80A.zz1 = new Z80A[Z80A.nZ80];
                            Z80A.zz1[0] = new Z80A();
                            MC68000.m1.cpunum = 0;
                            Z80A.zz1[0].cpunum = 1;
                            ncpu = 2;
                            cpu = new cpuexec_data[ncpu];
                            cpu[0] = MC68000.m1;
                            cpu[1] = Z80A.zz1[0];
                            cpu[0].cycles_per_second = 8000000;
                            cpu[1].cycles_per_second = 4000000;
                            cpu[0].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[0].cycles_per_second;
                            cpu[1].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[1].cycles_per_second;
                            vblank_interrupts_per_frame = 1;
                            break;
                        case "opwolfb":
                            MC68000.m1 = new MC68000();
                            Z80A.nZ80 = 2;
                            Z80A.zz1 = new Z80A[Z80A.nZ80];
                            Z80A.zz1[0] = new Z80A();
                            Z80A.zz1[1] = new Z80A();
                            MC68000.m1.cpunum = 0;
                            Z80A.zz1[0].cpunum = 1;
                            Z80A.zz1[1].cpunum = 2;
                            ncpu = 3;
                            cpu = new cpuexec_data[ncpu];
                            cpu[0] = MC68000.m1;
                            cpu[1] = Z80A.zz1[0];
                            cpu[2] = Z80A.zz1[1];
                            cpu[0].cycles_per_second = 8000000;
                            cpu[1].cycles_per_second = 4000000;
                            cpu[2].cycles_per_second = 4000000;
                            cpu[0].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[0].cycles_per_second;
                            cpu[1].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[1].cycles_per_second;
                            cpu[2].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[2].cycles_per_second;
                            vblank_interrupts_per_frame = 1;
                            break;
                    }
                    break;
                case "Taito B":
                    MC68000.m1 = new MC68000();
                    Z80A.nZ80 = 1;
                    Z80A.zz1 = new Z80A[Z80A.nZ80];
                    Z80A.zz1[0] = new Z80A();
                    MC68000.m1.cpunum = 0;
                    Z80A.zz1[0].cpunum = 1;
                    ncpu = 2;
                    cpu = new cpuexec_data[ncpu];
                    cpu[0] = MC68000.m1;
                    cpu[1] = Z80A.zz1[0];
                    cpu[0].cycles_per_second = 12000000;
                    switch (Machine.sName)
                    {
                        case "silentd":
                        case "silentdj":
                        case "silentdu":
                            cpu[0].cycles_per_second = 16000000;
                            break;
                    }
                    cpu[1].cycles_per_second = 4000000;
                    cpu[0].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[0].cycles_per_second;
                    cpu[1].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[1].cycles_per_second;
                    vblank_interrupts_per_frame = 1;
                    switch (Machine.sName)
                    {
                        case "pbobble":
                            vblank_interrupt = Taitob.pbobble_interrupt;
                            vblank_interrupt2 = Taitob.pbobble_interrupt5;
                            break;
                        case "silentd":
                        case "silentdj":
                        case "silentdu":
                            vblank_interrupt = Taitob.silentd_interrupt;
                            vblank_interrupt2 = Taitob.silentd_interrupt4;
                            break;
                    }
                    break;
                case "Konami 68000":
                    MC68000.m1 = new MC68000();
                    Z80A.nZ80 = 1;
                    Z80A.zz1 = new Z80A[Z80A.nZ80];
                    Z80A.zz1[0] = new Z80A();
                    MC68000.m1.cpunum = 0;
                    Z80A.zz1[0].cpunum = 1;
                    ncpu = 2;
                    cpu = new cpuexec_data[ncpu];
                    cpu[0] = MC68000.m1;
                    cpu[1] = Z80A.zz1[0];
                    vblank_interrupts_per_frame = 1;
                    switch (Machine.sName)
                    {
                        case "cuebrick":
                            ncpu = 1;
                            cpu[0].cycles_per_second = 8000000;
                            cpu[0].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[0].cycles_per_second;
                            vblank_interrupts_per_frame = 10;
                            vblank_interrupt = Konami68000.cuebrick_interrupt;
                            break;
                        case "mia":
                        case "mia2":
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
                            cpu[0].cycles_per_second = 8000000;
                            cpu[1].cycles_per_second = 3579545;
                            cpu[0].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[0].cycles_per_second;
                            cpu[1].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[1].cycles_per_second;
                            vblank_interrupt = Generic.irq5_line_hold0;
                            break;
                        case "punkshot":
                        case "punkshot2":
                        case "punkshotj":
                        case "thndrx2":
                        case "thndrx2a":
                        case "thndrx2j":
                            cpu[0].cycles_per_second = 12000000;
                            cpu[1].cycles_per_second = 3579545;
                            cpu[0].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[0].cycles_per_second;
                            cpu[1].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[1].cycles_per_second;
                            vblank_interrupt = Konami68000.punkshot_interrupt;
                            break;
                        case "lgtnfght":
                        case "lgtnfghta":
                        case "lgtnfghtu":
                        case "trigon":
                        case "glfgreat":
                        case "glfgreatj":
                            cpu[0].cycles_per_second = 12000000;
                            cpu[1].cycles_per_second = 3579545;
                            cpu[0].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[0].cycles_per_second;
                            cpu[1].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[1].cycles_per_second;
                            vblank_interrupt = Konami68000.lgtnfght_interrupt;
                            break;
                        case "blswhstl":
                        case "blswhstla":
                        case "detatwin":
                            cpu[0].cycles_per_second = 16000000;
                            cpu[1].cycles_per_second = 3579545;
                            cpu[0].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[0].cycles_per_second;
                            cpu[1].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[1].cycles_per_second;
                            vblank_interrupt = Konami68000.punkshot_interrupt;
                            break;
                        case "tmnt2":
                        case "tmnt2a":
                        case "tmht22pe":
                        case "tmht24pe":
                        case "tmnt22pu":
                        case "qgakumon":
                            cpu[0].cycles_per_second = 16000000;
                            cpu[1].cycles_per_second = 8000000;
                            cpu[0].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[0].cycles_per_second;
                            cpu[1].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[1].cycles_per_second;
                            vblank_interrupt = Konami68000.punkshot_interrupt;
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
                            cpu[0].cycles_per_second = 16000000;
                            cpu[1].cycles_per_second = 4000000;
                            cpu[0].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[0].cycles_per_second;
                            cpu[1].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[1].cycles_per_second;
                            vblank_interrupt = Konami68000.punkshot_interrupt;
                            break;
                        case "prmrsocr":
                        case "prmrsocrj":
                            cpu[0].cycles_per_second = 12000000;
                            cpu[1].cycles_per_second = 8000000;
                            cpu[0].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[0].cycles_per_second;
                            cpu[1].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[1].cycles_per_second;
                            vblank_interrupt = Konami68000.lgtnfght_interrupt;
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
                            M6809.mm1 = new M6809[1];
                            M6809.mm1[0] = new M6809();
                            M6809.mm1[0].irq_callback = Cpuint.cpu_0_irq_callback;
                            Z80A.nZ80 = 1;
                            Z80A.zz1 = new Z80A[Z80A.nZ80];
                            Z80A.zz1[0] = new Z80A();
                            M6809.mm1[0].cpunum = 0;
                            Z80A.zz1[0].cpunum = 1;
                            ncpu = 2;
                            cpu = new cpuexec_data[ncpu];
                            cpu[0] = M6809.mm1[0];
                            cpu[1] = Z80A.zz1[0];
                            cpu[0].cycles_per_second = 1500000;
                            cpu[1].cycles_per_second = 3000000;
                            cpu[0].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[0].cycles_per_second;
                            cpu[1].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[1].cycles_per_second;
                            vblank_interrupts_per_frame = 4;
                            vblank_interrupt = Generic.irq0_line_hold1;
                            break;
                        case "sf":
                        case "sfua":
                        case "sfj":
                        case "sfjan":
                        case "sfan":
                        case "sfp":
                            MC68000.m1 = new MC68000();
                            Z80A.nZ80 = 2;
                            Z80A.zz1 = new Z80A[Z80A.nZ80];
                            Z80A.zz1[0] = new Z80A();
                            Z80A.zz1[1] = new Z80A();
                            MC68000.m1.cpunum = 0;
                            Z80A.zz1[0].cpunum = 1;
                            Z80A.zz1[1].cpunum = 2;
                            ncpu = 3;
                            cpu = new cpuexec_data[ncpu];
                            cpu[0] = MC68000.m1;
                            cpu[1] = Z80A.zz1[0];
                            cpu[2] = Z80A.zz1[1];
                            cpu[0].cycles_per_second = 8000000;
                            cpu[1].cycles_per_second = 3579545;
                            cpu[2].cycles_per_second = 3579545;
                            cpu[0].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[0].cycles_per_second;
                            cpu[1].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[1].cycles_per_second;
                            cpu[2].attoseconds_per_cycle = Attotime.ATTOSECONDS_PER_SECOND / cpu[2].cycles_per_second;
                            vblank_interrupts_per_frame = 1;
                            break;
                    }
                    break;
            }
            activecpu = -1;
            for (icpu = 0; icpu < ncpu; icpu++)
            {
                cpu[icpu].suspend = SUSPEND_REASON_RESET;
                cpu[icpu].localtime = Attotime.ATTOTIME_ZERO;
                cpu[icpu].TotalExecutedCycles = 0;
                cpu[icpu].PendingCycles = 0;
            }
            compute_perfect_interleave();
        }
        public static void cpuexec_reset()
        {
            switch (Machine.sBoard)
            {
                case "CPS-1":
                    MC68000.m1.ReadOpByte = CPS.MCReadOpByte;
                    MC68000.m1.ReadByte = CPS.MCReadByte;
                    MC68000.m1.ReadOpWord = CPS.MCReadOpWord;
                    MC68000.m1.ReadWord = MC68000.m1.ReadPcrelWord = CPS.MCReadWord;
                    MC68000.m1.ReadOpLong = CPS.MCReadOpLong;
                    MC68000.m1.ReadLong = MC68000.m1.ReadPcrelLong = CPS.MCReadLong;
                    MC68000.m1.WriteByte = CPS.MCWriteByte;
                    MC68000.m1.WriteWord = CPS.MCWriteWord;
                    MC68000.m1.WriteLong = CPS.MCWriteLong;
                    Z80A.zz1[0].ReadOp = CPS.ZCReadOp;
                    Z80A.zz1[0].ReadOpArg = CPS.ZCReadMemory;
                    Z80A.zz1[0].ReadMemory = CPS.ZCReadMemory;
                    Z80A.zz1[0].WriteMemory = CPS.ZCWriteMemory;
                    Z80A.zz1[0].ReadHardware = CPS.ZCReadHardware;
                    Z80A.zz1[0].WriteHardware = CPS.ZCWriteHardware;
                    Z80A.zz1[0].IRQCallback = CPS.ZIRQCallback;
                    switch (Machine.sName)
                    {
                        case "forgottn":
                        case "forgottna":
                        case "forgottnu":
                        case "forgottnue":
                        case "forgottnuc":
                        case "forgottnua":
                        case "forgottnuaa":
                        case "lostwrld":
                        case "lostwrldo":
                            MC68000.m1.ReadByte = CPS.MCReadByte_forgottn;
                            MC68000.m1.ReadWord = MC68000.m1.ReadPcrelWord = CPS.MCReadWord_forgottn;
                            MC68000.m1.ReadLong = MC68000.m1.ReadPcrelLong = CPS.MCReadLong_forgottn;
                            MC68000.m1.WriteByte = CPS.MCWriteByte_forgottn;
                            MC68000.m1.WriteWord = CPS.MCWriteWord_forgottn;
                            MC68000.m1.WriteLong = CPS.MCWriteLong_forgottn;
                            break;
                        case "sf2ee":
                        case "sf2ue":
                        case "sf2thndr":
                            MC68000.m1.ReadByte = CPS.MCReadByte_sf2thndr;
                            MC68000.m1.ReadWord = MC68000.m1.ReadPcrelWord = CPS.MCReadWord_sf2thndr;
                            MC68000.m1.WriteWord = CPS.MCWriteWord_sf2thndr;
                            break;
                        case "sf2ceblp":
                            MC68000.m1.ReadWord = MC68000.m1.ReadPcrelWord = CPS.MCReadWord_sf2ceblp;
                            MC68000.m1.WriteWord = CPS.MCWriteWord_sf2ceblp;
                            break;
                        case "sf2m3":
                        case "sf2m8":
                            MC68000.m1.ReadByte = CPS.MCReadByte_sf2m3;
                            MC68000.m1.ReadWord = MC68000.m1.ReadPcrelWord = CPS.MCReadWord_sf2m3;
                            MC68000.m1.ReadLong = MC68000.m1.ReadPcrelLong = CPS.MCReadLong_sf2m3;
                            MC68000.m1.WriteByte = CPS.MCWriteByte_sf2m3;
                            MC68000.m1.WriteWord = CPS.MCWriteWord_sf2m3;
                            MC68000.m1.WriteLong = CPS.MCWriteLong_sf2m3;
                            break;
                        case "sf2m10":
                            CPS.mainram2_set = new byte[0x100000];
                            CPS.mainram3_set = new byte[0x100];
                            MC68000.m1.ReadByte = CPS.MCReadByte_sf2m10;
                            MC68000.m1.ReadWord = MC68000.m1.ReadPcrelWord = CPS.MCReadWord_sf2m10;
                            MC68000.m1.ReadLong = MC68000.m1.ReadPcrelLong = CPS.MCReadLong_sf2m10;
                            MC68000.m1.WriteByte = CPS.MCWriteByte_sf2m10;
                            MC68000.m1.WriteWord = CPS.MCWriteWord_sf2m10;
                            MC68000.m1.WriteLong = CPS.MCWriteLong_sf2m10;
                            break;
                        case "sf2dongb":
                            MC68000.m1.ReadWord = MC68000.m1.ReadPcrelWord = CPS.MCReadWord_sf2dongb;
                            break;
                        case "dinohunt":
                            MC68000.m1.ReadByte = CPS.MCReadByte_dinohunt;
                            break;
                        case "pang3":
                        case "pang3r1":
                        case "pang3j":
                        case "pang3b":
                            MC68000.m1.ReadByte = CPS.MCReadByte_pang3;
                            MC68000.m1.ReadWord = MC68000.m1.ReadPcrelWord = CPS.MCReadWord_pang3;
                            MC68000.m1.WriteByte = CPS.MCWriteByte_pang3;
                            MC68000.m1.WriteWord = CPS.MCWriteWord_pang3;
                            break;
                    }
                    break;
                case "CPS-1(QSound)":
                    MC68000.m1.ReadOpByte = CPS.MQReadOpByte;
                    MC68000.m1.ReadByte = CPS.MQReadByte;
                    MC68000.m1.ReadOpWord = CPS.MQReadOpWord;
                    MC68000.m1.ReadWord = MC68000.m1.ReadPcrelWord = CPS.MQReadWord;
                    MC68000.m1.ReadOpLong = CPS.MQReadOpLong;
                    MC68000.m1.ReadLong = MC68000.m1.ReadPcrelLong = CPS.MQReadLong;
                    MC68000.m1.WriteByte = CPS.MQWriteByte;
                    MC68000.m1.WriteWord = CPS.MQWriteWord;
                    MC68000.m1.WriteLong = CPS.MQWriteLong;
                    Z80A.zz1[0].ReadOp = CPS.ZQReadOp;
                    Z80A.zz1[0].ReadOpArg = CPS.ZQReadMemory;
                    Z80A.zz1[0].ReadMemory = CPS.ZQReadMemory;
                    Z80A.zz1[0].WriteMemory = CPS.ZQWriteMemory;
                    Z80A.zz1[0].ReadHardware = CPS.ZCReadHardware;
                    Z80A.zz1[0].WriteHardware = CPS.ZCWriteHardware;
                    Z80A.zz1[0].IRQCallback = CPS.ZIRQCallback;
                    break;
                case "CPS2":
                    MC68000.m1.ReadOpByte = CPS.MC2ReadOpByte;
                    MC68000.m1.ReadByte = CPS.MC2ReadByte;
                    MC68000.m1.ReadOpWord = CPS.MC2ReadOpWord;
                    MC68000.m1.ReadPcrelWord = CPS.MC2ReadPcrelWord;
                    MC68000.m1.ReadWord = CPS.MC2ReadWord;
                    MC68000.m1.ReadOpLong = CPS.MC2ReadOpLong;
                    MC68000.m1.ReadPcrelLong = CPS.MC2ReadPcrelLong;
                    MC68000.m1.ReadLong = CPS.MC2ReadLong;
                    MC68000.m1.WriteByte = CPS.MC2WriteByte;
                    MC68000.m1.WriteWord = CPS.MC2WriteWord;
                    MC68000.m1.WriteLong = CPS.MC2WriteLong;
                    switch (Machine.sName)
                    {
                        case "ddtodd":
                        case "ecofghtrd":
                        case "ssf2ud":
                        case "ssf2tbd":
                        case "armwar1d":
                        case "avspd":
                        case "dstlku1d":
                        case "ringdstd":
                        case "ssf2tad":
                        case "ssf2xjr1d":
                        case "xmcotar1d":
                        case "mshud":
                        case "cybotsud":
                        case "cybotsjd":
                        case "nwarrud":
                        case "sfad":
                        case "19xxd":
                        case "ddsomud":
                        case "gigaman2":
                        case "megamn2d":
                        case "sfz2ad":
                        case "sfz2jd":
                        case "spf2td":
                        case "spf2xjd":
                        case "sfz2ald":
                        case "xmvsfu1d":
                        case "batcird":
                        case "csclub1d":
                        case "mshvsfu1d":
                        case "sgemfd":
                        case "vsavd":
                        case "vhunt2d":
                        case "vsav2d":
                        case "mvscud":
                        case "sfa3ud":
                        case "sfz3jr2d":
                        case "gigawingd":
                        case "gigawingjd":
                        case "1944d":
                        case "dimahoud":
                        case "mmatrixd":
                        case "progearud":
                        case "progearjd":
                        case "progearjbl":
                        case "hsf2d":
                            MC68000.m1.ReadOpByte = CPS.MC2ReadOpByte_dead;
                            MC68000.m1.ReadByte = CPS.MC2ReadByte_dead;
                            MC68000.m1.ReadOpWord = CPS.MC2ReadOpWord_dead;
                            MC68000.m1.ReadWord = MC68000.m1.ReadPcrelWord = CPS.MC2ReadWord_dead;
                            MC68000.m1.ReadOpLong = CPS.MC2ReadOpLong_dead;
                            MC68000.m1.ReadLong = MC68000.m1.ReadPcrelLong = CPS.MC2ReadLong_dead;
                            MC68000.m1.WriteByte = CPS.MC2WriteByte_dead;
                            MC68000.m1.WriteWord = CPS.MC2WriteWord_dead;
                            MC68000.m1.WriteLong = CPS.MC2WriteLong_dead;
                            break;
                    }
                    Z80A.zz1[0].ReadOp = CPS.ZQReadOp;
                    Z80A.zz1[0].ReadOpArg = CPS.ZQReadMemory;
                    Z80A.zz1[0].ReadMemory = CPS.ZQReadMemory;
                    Z80A.zz1[0].WriteMemory = CPS.ZQWriteMemory;
                    Z80A.zz1[0].ReadHardware = CPS.ZCReadHardware;
                    Z80A.zz1[0].WriteHardware = CPS.ZCWriteHardware;
                    Z80A.zz1[0].IRQCallback = CPS.ZIRQCallback;
                    break;
                case "Data East":
                    M6502.mm1[0].ReadOp = Dataeast.D0ReadOp;
                    M6502.mm1[0].ReadOpArg = Dataeast.D0ReadOpArg;
                    M6502.mm1[0].ReadMemory = Dataeast.D0ReadMemory;
                    M6502.mm1[0].WriteMemory = Dataeast.D0WriteMemory;
                    M6502.mm1[1].ReadOpArg = Dataeast.D1ReadOpArg;
                    M6502.mm1[1].ReadMemory = Dataeast.D1ReadMemory;
                    M6502.mm1[1].WriteMemory = Dataeast.D1WriteMemory;
                    switch (Machine.sName)
                    {
                        case "pcktgal":
                        case "pcktgalb":
                            M6502.mm1[1].ReadOp = Dataeast.D1ReadOp;
                            break;
                        case "pcktgal2":
                        case "pcktgal2j":
                        case "spool3":
                        case "spool3i":
                            M6502.mm1[1].ReadOp = Dataeast.D1ReadOp_2;
                            break;
                    }
                    break;
                case "Tehkan":
                    Z80A.zz1[0].ReadOp = Tehkan.Z0ReadOp;
                    Z80A.zz1[0].ReadOpArg = Tehkan.Z0ReadMemory;
                    Z80A.zz1[0].ReadMemory = Tehkan.Z0ReadMemory;
                    Z80A.zz1[0].WriteMemory = Tehkan.Z0WriteMemory;
                    Z80A.zz1[0].ReadHardware = Tehkan.Z0ReadHardware;
                    Z80A.zz1[0].WriteHardware = Tehkan.Z0WriteHardware;
                    Z80A.zz1[0].IRQCallback = Tehkan.Z0IRQCallback;
                    switch (Machine.sName)
                    {
                        case "pbaction3":
                            Z80A.zz1[0].ReadOp = Tehkan.Z0ReadOp_pbaction3;
                            Z80A.zz1[0].ReadOpArg = Tehkan.Z0ReadMemory_pbaction3;
                            Z80A.zz1[0].ReadMemory = Tehkan.Z0ReadMemory_pbaction3;
                            break;
                        case "pbaction4":
                        case "pbaction5":
                            Z80A.zz1[0].ReadOp = Tehkan.Z0ReadOp_pbaction3;
                            break;
                    }
                    Z80A.zz1[1].ReadOp = Tehkan.Z1ReadOp;
                    Z80A.zz1[1].ReadOpArg = Tehkan.Z1ReadMemory;
                    Z80A.zz1[1].ReadMemory = Tehkan.Z1ReadMemory;
                    Z80A.zz1[1].WriteMemory = Tehkan.Z1WriteMemory;
                    Z80A.zz1[1].ReadHardware = Tehkan.Z1ReadHardware;
                    Z80A.zz1[1].WriteHardware = Tehkan.Z1WriteHardware;
                    Z80A.zz1[1].IRQCallback = Tehkan.Z1IRQCallback;
                    break;
                case "Neo Geo":
                    MC68000.m1.ReadOpByte = Neogeo.MReadOpByte;
                    MC68000.m1.ReadByte = Neogeo.MReadByte;
                    MC68000.m1.ReadOpWord = Neogeo.MReadOpWord;
                    MC68000.m1.ReadWord = MC68000.m1.ReadPcrelWord = Neogeo.MReadWord;
                    MC68000.m1.ReadOpLong = Neogeo.MReadOpLong;
                    MC68000.m1.ReadLong = MC68000.m1.ReadPcrelLong = Neogeo.MReadLong;
                    MC68000.m1.WriteByte = Neogeo.MWriteByte;
                    MC68000.m1.WriteWord = Neogeo.MWriteWord;
                    MC68000.m1.WriteLong = Neogeo.MWriteLong;
                    Z80A.zz1[0].ReadOp = Neogeo.ZReadOp;
                    Z80A.zz1[0].ReadOpArg = Neogeo.ZReadOp;
                    Z80A.zz1[0].ReadMemory = Neogeo.ZReadMemory;
                    Z80A.zz1[0].WriteMemory = Neogeo.ZWriteMemory;
                    Z80A.zz1[0].ReadHardware = Neogeo.ZReadHardware;
                    Z80A.zz1[0].WriteHardware = Neogeo.ZWriteHardware;
                    Z80A.zz1[0].IRQCallback = Neogeo.ZIRQCallback;
                    switch (Machine.sName)
                    {
                        case "fatfury2":
                        case "ssideki":
                            MC68000.m1.ReadByte = Neogeo.MReadByte_fatfury2;
                            MC68000.m1.ReadWord = MC68000.m1.ReadPcrelWord = Neogeo.MReadWord_fatfury2;
                            MC68000.m1.ReadLong = MC68000.m1.ReadPcrelLong = Neogeo.MReadLong_fatfury2;
                            MC68000.m1.WriteByte = Neogeo.MWriteByte_fatfury2;
                            MC68000.m1.WriteWord = Neogeo.MWriteWord_fatfury2;
                            MC68000.m1.WriteLong = Neogeo.MWriteLong_fatfury2;
                            break;
                        case "irrmaze":
                            MC68000.m1.ReadByte = Neogeo.MReadByte_irrmaze;
                            break;
                        case "kof98":
                        case "kof98a":
                        case "kof98k":
                        case "kof98ka":
                            MC68000.m1.WriteWord = Neogeo.MWriteWord_kof98;
                            break;
                        case "kof99":
                        case "kof99h":
                        case "kof99e":
                            MC68000.m1.ReadByte = Neogeo.MReadByte_kof99;
                            MC68000.m1.ReadWord = MC68000.m1.ReadPcrelWord = Neogeo.MReadWord_kof99;
                            MC68000.m1.ReadLong = MC68000.m1.ReadPcrelLong = Neogeo.MReadLong_kof99;
                            MC68000.m1.WriteByte = Neogeo.MWriteByte_kof99;
                            MC68000.m1.WriteWord = Neogeo.MWriteWord_kof99;
                            MC68000.m1.WriteLong = Neogeo.MWriteLong_kof99;
                            break;
                        case "garou":
                            MC68000.m1.ReadByte = Neogeo.MReadByte_garou;
                            MC68000.m1.ReadWord = MC68000.m1.ReadPcrelWord = Neogeo.MReadWord_garou;
                            MC68000.m1.ReadLong = MC68000.m1.ReadPcrelLong = Neogeo.MReadLong_garou;
                            MC68000.m1.WriteByte = Neogeo.MWriteByte_garou;
                            MC68000.m1.WriteWord = Neogeo.MWriteWord_garou;
                            MC68000.m1.WriteLong = Neogeo.MWriteLong_garou;
                            break;
                        case "garouh":
                            MC68000.m1.ReadByte = Neogeo.MReadByte_garou;
                            MC68000.m1.ReadWord = MC68000.m1.ReadPcrelWord = Neogeo.MReadWord_garou;
                            MC68000.m1.ReadLong = MC68000.m1.ReadPcrelLong = Neogeo.MReadLong_garou;
                            MC68000.m1.WriteByte = Neogeo.MWriteByte_garouh;
                            MC68000.m1.WriteWord = Neogeo.MWriteWord_garouh;
                            MC68000.m1.WriteLong = Neogeo.MWriteLong_garouh;
                            break;
                        case "mslug3":
                            MC68000.m1.ReadByte = Neogeo.MReadByte_mslug3;
                            MC68000.m1.ReadWord = MC68000.m1.ReadPcrelWord = Neogeo.MReadWord_mslug3;
                            MC68000.m1.ReadLong = MC68000.m1.ReadPcrelLong = Neogeo.MReadLong_mslug3;
                            MC68000.m1.WriteByte = Neogeo.MWriteByte_mslug3;
                            MC68000.m1.WriteWord = Neogeo.MWriteWord_mslug3;
                            MC68000.m1.WriteLong = Neogeo.MWriteLong_mslug3;
                            break;
                        case "kof2000":
                            MC68000.m1.ReadOpByte = MC68000.m1.ReadByte = Neogeo.MReadByte_kof2000;
                            MC68000.m1.ReadOpWord = MC68000.m1.ReadWord = MC68000.m1.ReadPcrelWord = Neogeo.MReadWord_kof2000;
                            MC68000.m1.ReadOpLong = MC68000.m1.ReadLong = MC68000.m1.ReadPcrelLong = Neogeo.MReadLong_kof2000;
                            MC68000.m1.WriteByte = Neogeo.MWriteByte_kof2000;
                            MC68000.m1.WriteWord = Neogeo.MWriteWord_kof2000;
                            MC68000.m1.WriteLong = Neogeo.MWriteLong_kof2000;
                            break;
                        case "mslug5":
                        case "mslug5h":
                        case "svc":
                        case "kof2003":
                        case "kof2003h":
                        case "svcboot":
                        case "svcsplus":
                            MC68000.m1.ReadByte = Neogeo.MReadByte_pvc;
                            MC68000.m1.ReadWord = MC68000.m1.ReadPcrelWord = Neogeo.MReadWord_pvc;
                            MC68000.m1.ReadLong = MC68000.m1.ReadPcrelLong = Neogeo.MReadLong_pvc;
                            MC68000.m1.WriteByte = Neogeo.MWriteByte_pvc;
                            MC68000.m1.WriteWord = Neogeo.MWriteWord_pvc;
                            MC68000.m1.WriteLong = Neogeo.MWriteLong_pvc;
                            break;
                        case "cthd2003":
                        case "ct2k3sp":
                            MC68000.m1.WriteByte = Neogeo.MWriteByte_cthd2003;
                            MC68000.m1.WriteWord = Neogeo.MWriteWord_cthd2003;
                            break;
                        case "ms5plus":
                            MC68000.m1.ReadByte = Neogeo.MReadByte_ms5plus;
                            MC68000.m1.ReadWord = MC68000.m1.ReadPcrelWord = Neogeo.MReadWord_ms5plus;
                            MC68000.m1.ReadLong = MC68000.m1.ReadPcrelLong = Neogeo.MReadLong_ms5plus;
                            MC68000.m1.WriteByte = Neogeo.MWriteByte_ms5plus;
                            MC68000.m1.WriteWord = Neogeo.MWriteWord_ms5plus;
                            MC68000.m1.WriteLong = Neogeo.MWriteLong_ms5plus;
                            break;
                        case "kog":
                            MC68000.m1.ReadByte = Neogeo.MReadByte_kog;
                            MC68000.m1.ReadWord = MC68000.m1.ReadPcrelWord = Neogeo.MReadWord_kog;
                            MC68000.m1.ReadLong = MC68000.m1.ReadPcrelLong = Neogeo.MReadLong_kog;
                            break;
                        case "kf2k3bl":
                        case "kf2k3upl":
                            MC68000.m1.ReadByte = Neogeo.MReadByte_kf2k3bl;
                            MC68000.m1.ReadWord = MC68000.m1.ReadPcrelWord = Neogeo.MReadWord_kf2k3bl;
                            MC68000.m1.ReadLong = MC68000.m1.ReadPcrelLong = Neogeo.MReadLong_kf2k3bl;
                            MC68000.m1.WriteByte = Neogeo.MWriteByte_kf2k3bl;
                            MC68000.m1.WriteWord = Neogeo.MWriteWord_kf2k3bl;
                            MC68000.m1.WriteLong = Neogeo.MWriteLong_kf2k3bl;
                            break;
                        case "kf2k3bla":
                        case "kf2k3pl":
                            MC68000.m1.ReadByte = Neogeo.MReadByte_kf2k3bl;
                            MC68000.m1.ReadWord = MC68000.m1.ReadPcrelWord = Neogeo.MReadWord_kf2k3bl;
                            MC68000.m1.ReadLong = MC68000.m1.ReadPcrelLong = Neogeo.MReadLong_kf2k3bl;
                            MC68000.m1.WriteByte = Neogeo.MWriteByte_kf2k3pl;
                            MC68000.m1.WriteWord = Neogeo.MWriteWord_kf2k3pl;
                            MC68000.m1.WriteLong = Neogeo.MWriteLong_kf2k3pl;
                            break;
                        case "sbp":
                            MC68000.m1.ReadByte = Neogeo.MReadByte_sbp;
                            MC68000.m1.ReadWord = MC68000.m1.ReadPcrelWord = Neogeo.MReadWord_sbp;
                            MC68000.m1.ReadLong = MC68000.m1.ReadPcrelLong = Neogeo.MReadLong_sbp;
                            MC68000.m1.WriteByte = Neogeo.MWriteByte_sbp;
                            MC68000.m1.WriteWord = Neogeo.MWriteWord_sbp;
                            MC68000.m1.WriteLong = Neogeo.MWriteLong_sbp;
                            break;
                        case "kof10th":
                            MC68000.m1.ReadByte = Neogeo.MReadByte_kof10th;
                            MC68000.m1.ReadWord = MC68000.m1.ReadPcrelWord = Neogeo.MReadWord_kof10th;
                            MC68000.m1.ReadLong = MC68000.m1.ReadPcrelLong = Neogeo.MReadLong_kof10th;
                            MC68000.m1.WriteByte = Neogeo.MWriteByte_kof10th;
                            MC68000.m1.WriteWord = Neogeo.MWriteWord_kof10th;
                            MC68000.m1.WriteLong = Neogeo.MWriteLong_kof10th;
                            break;
                        case "jockeygp":
                        case "jockeygpa":
                            MC68000.m1.ReadByte = Neogeo.MReadByte_jockeygp;
                            MC68000.m1.ReadWord = MC68000.m1.ReadPcrelWord = Neogeo.MReadWord_jockeygp;
                            MC68000.m1.ReadLong = MC68000.m1.ReadPcrelLong = Neogeo.MReadLong_jockeygp;
                            MC68000.m1.WriteByte = Neogeo.MWriteByte_jockeygp;
                            MC68000.m1.WriteWord = Neogeo.MWriteWord_jockeygp;
                            MC68000.m1.WriteLong = Neogeo.MWriteLong_jockeygp;
                            break;
                        case "vliner":
                            MC68000.m1.ReadByte = Neogeo.MReadByte_vliner;
                            MC68000.m1.ReadWord = MC68000.m1.ReadPcrelWord = Neogeo.MReadWord_vliner;
                            MC68000.m1.ReadLong = MC68000.m1.ReadPcrelLong = Neogeo.MReadLong_vliner;
                            MC68000.m1.WriteByte = Neogeo.MWriteByte_jockeygp;
                            MC68000.m1.WriteWord = Neogeo.MWriteWord_jockeygp;
                            MC68000.m1.WriteLong = Neogeo.MWriteLong_jockeygp;
                            break;
                    }
                    break;
                case "SunA8":
                    switch (Machine.sName)
                    {
                        case "starfigh":
                            Z80A.zz1[0].ReadOp = SunA8.Z0ReadOp_starfigh;
                            Z80A.zz1[0].ReadOpArg = SunA8.Z0ReadMemory_starfigh;
                            Z80A.zz1[0].ReadMemory = SunA8.Z0ReadMemory_starfigh;
                            Z80A.zz1[0].WriteMemory = SunA8.Z0WriteMemory_starfigh;
                            Z80A.zz1[0].ReadHardware = SunA8.Z0ReadHardware;
                            Z80A.zz1[0].WriteHardware = SunA8.Z0WriteHardware;
                            Z80A.zz1[0].IRQCallback = SunA8.Z0IRQCallback;
                            Z80A.zz1[1].ReadOp = SunA8.Z1ReadOp_hardhead;
                            Z80A.zz1[1].ReadOpArg = SunA8.Z1ReadMemory_hardhead;
                            Z80A.zz1[1].ReadMemory = SunA8.Z1ReadMemory_hardhead;
                            Z80A.zz1[1].WriteMemory = SunA8.Z1Write_Memory_hardhead;
                            Z80A.zz1[1].ReadHardware = SunA8.Z1ReadHardware;
                            Z80A.zz1[1].WriteHardware = SunA8.Z1WriteHardware;
                            Z80A.zz1[1].IRQCallback = SunA8.Z1IRQCallback;
                            break;
                    }
                    break;
                case "Namco System 1":
                    M6809.mm1[0].ReadOp = Namcos1.N0ReadOpByte;
                    M6809.mm1[0].ReadOpArg = Namcos1.N0ReadOpByte;
                    M6809.mm1[0].RM = Namcos1.N0ReadMemory;
                    M6809.mm1[0].WM = Namcos1.N0WriteMemory;
                    M6809.mm1[1].ReadOp = Namcos1.N1ReadOpByte;
                    M6809.mm1[1].ReadOpArg = Namcos1.N1ReadOpByte;
                    M6809.mm1[1].RM = Namcos1.N1ReadMemory;
                    M6809.mm1[1].WM = Namcos1.N1WriteMemory;
                    M6809.mm1[2].ReadOp = Namcos1.N2ReadOpByte;
                    M6809.mm1[2].ReadOpArg = Namcos1.N2ReadOpByte;
                    M6809.mm1[2].RM = Namcos1.N2ReadMemory;
                    M6809.mm1[2].WM = Namcos1.N2WriteMemory;
                    M6800.m1.ReadOp = Namcos1.N3ReadOpByte;
                    M6800.m1.ReadOpArg = Namcos1.N3ReadOpByte;
                    M6800.m1.ReadMemory = Namcos1.N3ReadMemory;
                    M6800.m1.ReadIO = Namcos1.N3ReadIO;
                    M6800.m1.WriteMemory = Namcos1.N3WriteMemory;
                    M6800.m1.WriteIO = Namcos1.N3WriteIO;
                    switch (Machine.sName)
                    {
                        case "quester":
                        case "questers":
                            M6800.m1.ReadMemory = Namcos1.N3ReadMemory_quester;
                            break;
                        case "berabohm":
                            M6800.m1.ReadMemory = Namcos1.N3ReadMemory_berabohm;
                            break;
                        case "faceoff":
                        case "tankfrce4":
                            M6800.m1.ReadMemory = Namcos1.N3ReadMemory_faceoff;
                            break;
                    }
                    break;
                case "IGS011":
                    switch (Machine.sName)
                    {
                        case "drgnwrld":
                        case "drgnwrldv30":
                        case "drgnwrldv10c":
                        case "drgnwrldv11h":
                            MC68000.m1.ReadOpByte = IGS011.MReadOpByte_drgnwrld;
                            MC68000.m1.ReadByte = IGS011.MReadByte_drgnwrld;
                            MC68000.m1.ReadOpWord = IGS011.MReadOpWord_drgnwrld;
                            MC68000.m1.ReadWord = MC68000.m1.ReadPcrelWord = IGS011.MReadWord_drgnwrld;
                            MC68000.m1.ReadOpLong = IGS011.MReadOpLong_drgnwrld;
                            MC68000.m1.ReadLong = MC68000.m1.ReadPcrelLong = IGS011.MReadLong_drgnwrld;
                            MC68000.m1.WriteByte = IGS011.MWriteByte_drgnwrld;
                            MC68000.m1.WriteWord = IGS011.MWriteWord_drgnwrld;
                            MC68000.m1.WriteLong = IGS011.MWriteLong_drgnwrld;
                            break;
                        case "drgnwrldv21":
                            MC68000.m1.ReadOpByte = IGS011.MReadByte_drgnwrld_igs012;
                            MC68000.m1.ReadByte = IGS011.MReadByte_drgnwrld_igs012;
                            MC68000.m1.ReadOpWord = IGS011.MReadWord_drgnwrld_igs012_drgnwrldv21;
                            MC68000.m1.ReadWord = MC68000.m1.ReadPcrelWord = IGS011.MReadWord_drgnwrld_igs012_drgnwrldv21;
                            MC68000.m1.ReadOpLong = IGS011.MReadLong_drgnwrld_igs012;
                            MC68000.m1.ReadLong = MC68000.m1.ReadPcrelLong = IGS011.MReadLong_drgnwrld_igs012;
                            MC68000.m1.WriteByte = IGS011.MWriteByte_drgnwrld_igs012;
                            MC68000.m1.WriteWord = IGS011.MWriteWord_drgnwrld_igs012;
                            MC68000.m1.WriteLong = IGS011.MWriteLong_drgnwrld_igs012;
                            break;
                        case "drgnwrldv21j":
                        case "drgnwrldv20j":
                        case "drgnwrldv40k":
                            MC68000.m1.ReadOpByte = IGS011.MReadByte_drgnwrld_igs012;
                            MC68000.m1.ReadByte = IGS011.MReadByte_drgnwrld_igs012;
                            MC68000.m1.ReadOpWord = IGS011.MReadWord_drgnwrld_igs012;
                            MC68000.m1.ReadWord = MC68000.m1.ReadPcrelWord = IGS011.MReadWord_drgnwrld_igs012;
                            MC68000.m1.ReadOpLong = IGS011.MReadLong_drgnwrld_igs012;
                            MC68000.m1.ReadLong = MC68000.m1.ReadPcrelLong = IGS011.MReadLong_drgnwrld_igs012;
                            MC68000.m1.WriteByte = IGS011.MWriteByte_drgnwrld_igs012;
                            MC68000.m1.WriteWord = IGS011.MWriteWord_drgnwrld_igs012;
                            MC68000.m1.WriteLong = IGS011.MWriteLong_drgnwrld_igs012;
                            break;
                        case "lhb":
                        case "lhbv33c":
                            MC68000.m1.ReadOpByte = IGS011.MReadByte_lhb;
                            MC68000.m1.ReadByte = IGS011.MReadByte_lhb;
                            MC68000.m1.ReadOpWord = IGS011.MReadWord_lhb;
                            MC68000.m1.ReadWord = MC68000.m1.ReadPcrelWord = IGS011.MReadWord_lhb;
                            MC68000.m1.ReadOpLong = IGS011.MReadLong_lhb;
                            MC68000.m1.ReadLong = MC68000.m1.ReadPcrelLong = IGS011.MReadLong_lhb;
                            MC68000.m1.WriteByte = IGS011.MWriteByte_lhb;
                            MC68000.m1.WriteWord = IGS011.MWriteWord_lhb;
                            MC68000.m1.WriteLong = IGS011.MWriteLong_lhb;
                            break;
                        case "dbc":
                            MC68000.m1.ReadOpByte = IGS011.MReadByte_lhb;
                            MC68000.m1.ReadByte = IGS011.MReadByte_lhb;
                            MC68000.m1.ReadOpWord = IGS011.MReadWord_lhb;
                            MC68000.m1.ReadWord = MC68000.m1.ReadPcrelWord = IGS011.MReadWord_dbc;
                            MC68000.m1.ReadOpLong = IGS011.MReadLong_lhb;
                            MC68000.m1.ReadLong = MC68000.m1.ReadPcrelLong = IGS011.MReadLong_lhb;
                            MC68000.m1.WriteByte = IGS011.MWriteByte_lhb;
                            MC68000.m1.WriteWord = IGS011.MWriteWord_lhb;
                            MC68000.m1.WriteLong = IGS011.MWriteLong_lhb;
                            break;
                        case "ryukobou":
                            MC68000.m1.ReadOpByte = IGS011.MReadByte_lhb;
                            MC68000.m1.ReadByte = IGS011.MReadByte_lhb;
                            MC68000.m1.ReadOpWord = IGS011.MReadWord_lhb;
                            MC68000.m1.ReadWord = MC68000.m1.ReadPcrelWord = IGS011.MReadWord_ryukobou;
                            MC68000.m1.ReadOpLong = IGS011.MReadLong_lhb;
                            MC68000.m1.ReadLong = MC68000.m1.ReadPcrelLong = IGS011.MReadLong_lhb;
                            MC68000.m1.WriteByte = IGS011.MWriteByte_lhb;
                            MC68000.m1.WriteWord = IGS011.MWriteWord_lhb;
                            MC68000.m1.WriteLong = IGS011.MWriteLong_lhb;
                            break;
                        case "lhb2":
                            MC68000.m1.ReadOpByte = IGS011.MReadOpByte_lhb2;
                            MC68000.m1.ReadByte = IGS011.MReadByte_lhb2;
                            MC68000.m1.ReadOpWord = IGS011.MReadOpWord_lhb2;
                            MC68000.m1.ReadWord = MC68000.m1.ReadPcrelWord = IGS011.MReadWord_lhb2;
                            MC68000.m1.ReadOpLong = IGS011.MReadOpLong_lhb2;
                            MC68000.m1.ReadLong = MC68000.m1.ReadPcrelLong = IGS011.MReadLong_lhb2;
                            MC68000.m1.WriteByte = IGS011.MWriteByte_lhb2;
                            MC68000.m1.WriteWord = IGS011.MWriteWord_lhb2;
                            MC68000.m1.WriteLong = IGS011.MWriteLong_lhb2;
                            break;
                        case "xymg":
                            MC68000.m1.ReadOpByte = IGS011.MReadOpByte_xymg;
                            MC68000.m1.ReadByte = IGS011.MReadByte_xymg;
                            MC68000.m1.ReadOpWord = IGS011.MReadOpWord_xymg;
                            MC68000.m1.ReadWord = MC68000.m1.ReadPcrelWord = IGS011.MReadWord_xymg;
                            MC68000.m1.ReadOpLong = IGS011.MReadOpLong_xymg;
                            MC68000.m1.ReadLong = MC68000.m1.ReadPcrelLong = IGS011.MReadLong_xymg;
                            MC68000.m1.WriteByte = IGS011.MWriteByte_xymg;
                            MC68000.m1.WriteWord = IGS011.MWriteWord_xymg;
                            MC68000.m1.WriteLong = IGS011.MWriteLong_xymg;
                            break;
                        case "wlcc":
                            MC68000.m1.ReadOpByte = IGS011.MReadOpByte_wlcc;
                            MC68000.m1.ReadByte = IGS011.MReadByte_wlcc;
                            MC68000.m1.ReadOpWord = IGS011.MReadOpWord_wlcc;
                            MC68000.m1.ReadWord = MC68000.m1.ReadPcrelWord = IGS011.MReadWord_wlcc;
                            MC68000.m1.ReadOpLong = IGS011.MReadOpLong_wlcc;
                            MC68000.m1.ReadLong = MC68000.m1.ReadPcrelLong = IGS011.MReadLong_wlcc;
                            MC68000.m1.WriteByte = IGS011.MWriteByte_wlcc;
                            MC68000.m1.WriteWord = IGS011.MWriteWord_wlcc;
                            MC68000.m1.WriteLong = IGS011.MWriteLong_wlcc;
                            break;
                        case "vbowl":
                        case "vbowlj":
                            MC68000.m1.ReadOpByte = IGS011.MReadOpByte_vbowl;
                            MC68000.m1.ReadByte = IGS011.MReadByte_vbowl;
                            MC68000.m1.ReadOpWord = IGS011.MReadOpWord_vbowl;
                            MC68000.m1.ReadWord = MC68000.m1.ReadPcrelWord = IGS011.MReadWord_vbowl;
                            MC68000.m1.ReadOpLong = IGS011.MReadOpLong_vbowl;
                            MC68000.m1.ReadLong = MC68000.m1.ReadPcrelLong = IGS011.MReadLong_vbowl;
                            MC68000.m1.WriteByte = IGS011.MWriteByte_vbowl;
                            MC68000.m1.WriteWord = IGS011.MWriteWord_vbowl;
                            MC68000.m1.WriteLong = IGS011.MWriteLong_vbowl;
                            break;
                        case "nkishusp":
                            MC68000.m1.ReadOpByte = IGS011.MReadOpByte_nkishusp;
                            MC68000.m1.ReadByte = IGS011.MReadByte_nkishusp;
                            MC68000.m1.ReadOpWord = IGS011.MReadOpWord_nkishusp;
                            MC68000.m1.ReadWord = MC68000.m1.ReadPcrelWord = IGS011.MReadWord_nkishusp;
                            MC68000.m1.ReadOpLong = IGS011.MReadOpLong_nkishusp;
                            MC68000.m1.ReadLong = MC68000.m1.ReadPcrelLong = IGS011.MReadLong_nkishusp;
                            MC68000.m1.WriteByte = IGS011.MWriteByte_nkishusp;
                            MC68000.m1.WriteWord = IGS011.MWriteWord_nkishusp;
                            MC68000.m1.WriteLong = IGS011.MWriteLong_nkishusp;
                            break;
                    }
                    break;
                case "PGM":
                    MC68000.m1.ReadOpByte = PGM.MReadOpByte;
                    MC68000.m1.ReadByte = PGM.MReadByte;
                    MC68000.m1.ReadOpWord = PGM.MReadOpWord;
                    MC68000.m1.ReadWord = MC68000.m1.ReadPcrelWord = PGM.MReadWord;
                    MC68000.m1.ReadOpLong = PGM.MReadOpLong;
                    MC68000.m1.ReadLong = MC68000.m1.ReadPcrelLong = PGM.MReadLong;
                    MC68000.m1.WriteByte = PGM.MWriteByte;
                    MC68000.m1.WriteWord = PGM.MWriteWord;
                    MC68000.m1.WriteLong = PGM.MWriteLong;
                    Z80A.zz1[0].ReadOp = PGM.ZReadMemory;
                    Z80A.zz1[0].ReadOpArg = PGM.ZReadMemory;
                    Z80A.zz1[0].ReadMemory = PGM.ZReadMemory;
                    Z80A.zz1[0].WriteMemory = PGM.ZWriteMemory;
                    Z80A.zz1[0].ReadHardware = PGM.ZReadHardware;
                    Z80A.zz1[0].WriteHardware = PGM.ZWriteHardware;
                    Z80A.zz1[0].IRQCallback = PGM.ZIRQCallback;
                    switch (Machine.sName)
                    {
                        case "orlegend":
                        case "orlegende":
                        case "orlegendc":
                        case "orlegendca":
                        case "orlegend111c":
                        case "orlegend111t":
                        case "orlegend111k":
                        case "orlegend105k":
                            MC68000.m1.ReadByte = PGM.MPReadByte_orlegend;
                            MC68000.m1.ReadWord = MC68000.m1.ReadPcrelWord = PGM.MPReadWord_orlegend;
                            MC68000.m1.WriteByte = PGM.MPWriteByte_orlegend;
                            MC68000.m1.WriteWord = PGM.MPWriteWord_orlegend;
                            break;
                            /*case "drgw2":
                                MC68000.m1.ReadByte = PGM.MPReadByte_drgw2;
                                MC68000.m1.ReadWord = MC68000.m1.ReadPcrelWord= PGM.MPReadWord_drgw2;
                                MC68000.m1.ReadLong = MC68000.m1.ReadPcrelLong= PGM.MPReadLong_drgw2;
                                MC68000.m1.WriteByte = PGM.MPWriteByte_drgw2;
                                MC68000.m1.WriteWord = PGM.MPWriteWord_drgw2;
                                MC68000.m1.WriteLong = PGM.MPWriteLong_drgw2;
                                break;*/
                    }
                    break;
                case "M72":
                    Nec.nn1[0].ReadOp = Nec.nn1[0].ReadOpArg = M72.NReadOpByte;
                    Nec.nn1[0].ReadByte = M72.NReadByte_m72;
                    Nec.nn1[0].ReadWord = M72.NReadWord_m72;
                    Nec.nn1[0].WriteByte = M72.NWriteByte_m72;
                    Nec.nn1[0].WriteWord = M72.NWriteWord_m72;
                    Nec.nn1[0].ReadIOByte = M72.NReadIOByte;
                    Nec.nn1[0].ReadIOWord = M72.NReadIOWord;
                    Nec.nn1[0].WriteIOByte = M72.NWriteIOByte_m72;
                    Nec.nn1[0].WriteIOWord = M72.NWriteIOWord_m72;
                    Z80A.zz1[0].ReadOp = M72.ZReadMemory_ram;
                    Z80A.zz1[0].ReadOpArg = M72.ZReadMemory_ram;
                    Z80A.zz1[0].ReadMemory = M72.ZReadMemory_ram;
                    Z80A.zz1[0].WriteMemory = M72.ZWriteMemory_ram;
                    Z80A.zz1[0].ReadHardware = M72.ZReadHardware;
                    Z80A.zz1[0].WriteHardware = M72.ZWriteHardware;
                    Z80A.zz1[0].IRQCallback = M72.ZIRQCallback;
                    switch (Machine.sName)
                    {
                        case "airduel":
                        case "airduelm72":
                            Nec.nn1[0].ReadOp = Nec.nn1[0].ReadOpArg = M72.NReadOpByte_airduel;
                            Nec.nn1[0].ReadByte = M72.NReadByte_m72_airduel;
                            Nec.nn1[0].ReadWord = M72.NReadWord_m72_airduel;
                            Nec.nn1[0].WriteByte = M72.NWriteByte_m72_airduel;
                            Nec.nn1[0].WriteWord = M72.NWriteWord_m72_airduel;
                            Nec.nn1[0].WriteIOByte = M72.NWriteIOByte_m72_airduel;
                            Nec.nn1[0].WriteIOWord = M72.NWriteIOWord_m72_airduel;
                            break;
                        case "ltswords":
                        case "kengo":
                        case "kengoa":
                            Nec.nn1[0].ReadByte = M72.NReadByte_kengo;
                            Nec.nn1[0].ReadWord = M72.NReadWord_kengo;
                            Nec.nn1[0].WriteByte = M72.NWriteByte_kengo;
                            Nec.nn1[0].WriteWord = M72.NWriteWord_kengo;
                            Nec.nn1[0].WriteIOByte = M72.NWriteIOByte_kengo;
                            Nec.nn1[0].WriteIOWord = M72.NWriteIOWord_kengo;
                            Z80A.zz1[0].ReadOp = M72.ZReadMemory_rom;
                            Z80A.zz1[0].ReadOpArg = M72.ZReadMemory_rom;
                            Z80A.zz1[0].ReadMemory = M72.ZReadMemory_rom;
                            Z80A.zz1[0].WriteMemory = M72.ZWriteMemory_rom;
                            Z80A.zz1[0].ReadHardware = M72.ZReadHardware_rtype2;
                            Z80A.zz1[0].WriteHardware = M72.ZWriteHardware_rtype2;
                            Nec.nn1[0].v25v35_decryptiontable = M72.gunforce_decryption_table;
                            break;
                        default:
                            Nec.nn1[0].v25v35_decryptiontable = null;
                            break;
                    }
                    break;
                case "M92":
                    Nec.nn1[0].ReadOp = Nec.nn1[0].ReadOpArg = M92.N0ReadOpByte;
                    Nec.nn1[0].ReadByte = M92.N0ReadByte_m92;
                    Nec.nn1[0].ReadWord = M92.N0ReadWord_m92;
                    Nec.nn1[0].WriteByte = M92.N0WriteByte_m92;
                    Nec.nn1[0].WriteWord = M92.N0WriteWord_m92;
                    Nec.nn1[0].ReadIOByte = M92.N0ReadIOByte_m92;
                    Nec.nn1[0].ReadIOWord = M92.N0ReadIOWord_m92;
                    Nec.nn1[0].WriteIOByte = M92.N0WriteIOByte_m92;
                    Nec.nn1[0].WriteIOWord = M92.N0WriteIOWord_m92;
                    Nec.nn1[1].ReadOp = Nec.nn1[1].ReadOpArg = M92.N1ReadOpByte;
                    Nec.nn1[1].ReadByte = M92.N1ReadByte;
                    Nec.nn1[1].ReadWord = M92.N1ReadWord;
                    Nec.nn1[1].WriteByte = M92.N1WriteByte;
                    Nec.nn1[1].WriteWord = M92.N1WriteWord;
                    Nec.nn1[1].ReadIOByte = null_callback1;
                    Nec.nn1[1].ReadIOWord = null_callback2;
                    Nec.nn1[1].WriteIOByte = null_callback;
                    Nec.nn1[1].WriteIOWord = null_callback;
                    Nec.nn1[0].v25v35_decryptiontable = null;
                    switch (Machine.sName)
                    {
                        case "gunforce":
                        case "gunforcej":
                        case "gunforceu":
                            Nec.nn1[1].v25v35_decryptiontable = M92.gunforce_decryption_table;
                            break;
                        case "bmaster":
                        case "crossbld":
                            Nec.nn1[1].v25v35_decryptiontable = M92.bomberman_decryption_table;
                            break;
                        case "lethalth":
                        case "thndblst":
                            Nec.nn1[0].ReadByte = M92.N0ReadByte_lethalth;
                            Nec.nn1[0].ReadWord = M92.N0ReadWord_lethalth;
                            Nec.nn1[0].WriteByte = M92.N0WriteByte_lethalth;
                            Nec.nn1[0].WriteWord = M92.N0WriteWord_lethalth;
                            Nec.nn1[0].WriteIOByte = M92.N0WriteIOByte_lethalth;
                            Nec.nn1[0].WriteIOWord = M92.N0WriteIOWord_lethalth;
                            Nec.nn1[1].v25v35_decryptiontable = M92.lethalth_decryption_table;
                            break;
                        case "uccops":
                        case "uccopsu":
                        case "uccopsar":
                        case "uccopsj":
                            Nec.nn1[1].v25v35_decryptiontable = M92.dynablaster_decryption_table;
                            break;
                        case "mysticri":
                        case "gunhohki":
                        case "mysticrib":
                            Nec.nn1[1].v25v35_decryptiontable = M92.mysticri_decryption_table;
                            break;
                        case "majtitl2":
                        case "majtitl2a":
                        case "majtitl2b":
                        case "majtitl2j":
                        case "skingame":
                        case "skingame2":
                            Nec.nn1[0].ReadByte = M92.N0ReadByte_majtitl2;
                            Nec.nn1[0].ReadWord = M92.N0ReadWord_majtitl2;
                            Nec.nn1[0].WriteByte = M92.N0WriteByte_majtitl2;
                            Nec.nn1[0].WriteWord = M92.N0WriteWord_majtitl2;
                            Nec.nn1[1].v25v35_decryptiontable = M92.majtitl2_decryption_table;
                            break;
                        case "hook":
                        case "hooku":
                        case "hookj":
                            Nec.nn1[1].v25v35_decryptiontable = M92.hook_decryption_table;
                            break;
                        case "rtypeleo":
                        case "rtypeleoj":
                            Nec.nn1[1].v25v35_decryptiontable = M92.rtypeleo_decryption_table;
                            break;
                        case "inthunt":
                        case "inthuntu":
                        case "kaiteids":
                            Nec.nn1[1].v25v35_decryptiontable = M92.inthunt_decryption_table;
                            break;
                        case "nbbatman":
                        case "nbbatmanu":
                        case "leaguemn":
                            Nec.nn1[1].v25v35_decryptiontable = M92.leagueman_decryption_table;
                            break;
                        case "ssoldier":
                        case "psoldier":
                            Nec.nn1[1].v25v35_decryptiontable = M92.psoldier_decryption_table;
                            break;
                        case "gunforc2":
                        case "geostorm":
                            Nec.nn1[1].v25v35_decryptiontable = M92.lethalth_decryption_table;
                            break;
                        default:
                            Nec.nn1[1].v25v35_decryptiontable = null;
                            break;
                    }
                    break;
                case "Taito":
                    switch (Machine.sName)
                    {
                        case "tokio":
                        case "tokioo":
                        case "tokiou":
                            Z80A.zz1[0].ReadOp = Taito.Z0ReadOp_bublbobl;
                            Z80A.zz1[0].ReadOpArg = Taito.Z0ReadOp_bublbobl;
                            Z80A.zz1[0].ReadMemory = Taito.Z0ReadMemory_tokio;
                            Z80A.zz1[0].WriteMemory = Taito.Z0WriteMemory_tokio;
                            Z80A.zz1[0].ReadHardware = Taito.Z0ReadHardware;
                            Z80A.zz1[0].WriteHardware = Taito.Z0WriteHardware;
                            Z80A.zz1[0].IRQCallback = Taito.Z0IRQCallback;
                            Z80A.zz1[1].ReadOp = Taito.Z1ReadOp_tokio;
                            Z80A.zz1[1].ReadOpArg = Taito.Z1ReadOp_tokio;
                            Z80A.zz1[1].ReadMemory = Taito.Z1ReadMemory_tokio;
                            Z80A.zz1[1].WriteMemory = Taito.Z1WriteMemory_tokio;
                            Z80A.zz1[1].ReadHardware = Taito.Z1ReadHardware;
                            Z80A.zz1[1].WriteHardware = Taito.Z1WriteHardware;
                            Z80A.zz1[1].IRQCallback = Taito.Z1IRQCallback;
                            Z80A.zz1[2].ReadOp = Taito.Z2ReadOp_bublbobl;
                            Z80A.zz1[2].ReadOpArg = Taito.Z2ReadOp_bublbobl;
                            Z80A.zz1[2].ReadMemory = Taito.Z2ReadMemory_tokio;
                            Z80A.zz1[2].WriteMemory = Taito.Z2WriteMemory_tokio;
                            Z80A.zz1[2].ReadHardware = Taito.Z2ReadHardware;
                            Z80A.zz1[2].WriteHardware = Taito.Z2WriteHardware;
                            Z80A.zz1[2].IRQCallback = Taito.Z2IRQCallback;
                            break;
                        case "tokiob":
                            Z80A.zz1[0].ReadOp = Taito.Z0ReadOp_bublbobl;
                            Z80A.zz1[0].ReadOpArg = Taito.Z0ReadOp_bublbobl;
                            Z80A.zz1[0].ReadMemory = Taito.Z0ReadMemory_tokiob;
                            Z80A.zz1[0].WriteMemory = Taito.Z0WriteMemory_tokio;
                            Z80A.zz1[0].ReadHardware = Taito.Z0ReadHardware;
                            Z80A.zz1[0].WriteHardware = Taito.Z0WriteHardware;
                            Z80A.zz1[0].IRQCallback = Taito.Z0IRQCallback;
                            Z80A.zz1[1].ReadOp = Taito.Z1ReadOp_tokio;
                            Z80A.zz1[1].ReadOpArg = Taito.Z1ReadOp_tokio;
                            Z80A.zz1[1].ReadMemory = Taito.Z1ReadMemory_tokio;
                            Z80A.zz1[1].WriteMemory = Taito.Z1WriteMemory_tokio;
                            Z80A.zz1[1].ReadHardware = Taito.Z1ReadHardware;
                            Z80A.zz1[1].WriteHardware = Taito.Z1WriteHardware;
                            Z80A.zz1[1].IRQCallback = Taito.Z1IRQCallback;
                            Z80A.zz1[2].ReadOp = Taito.Z2ReadOp_bublbobl;
                            Z80A.zz1[2].ReadOpArg = Taito.Z2ReadOp_bublbobl;
                            Z80A.zz1[2].ReadMemory = Taito.Z2ReadMemory_tokio;
                            Z80A.zz1[2].WriteMemory = Taito.Z2WriteMemory_tokio;
                            Z80A.zz1[2].ReadHardware = Taito.Z2ReadHardware;
                            Z80A.zz1[2].WriteHardware = Taito.Z2WriteHardware;
                            Z80A.zz1[2].IRQCallback = Taito.Z2IRQCallback;
                            break;
                        case "bublbobl":
                        case "bublbobl1":
                        case "bublboblr":
                        case "bublboblr1":
                        case "bublcave":
                        case "bublcave11":
                        case "bublcave10":
                            Z80A.zz1[0].ReadOp = Taito.Z0ReadOp_bublbobl;
                            Z80A.zz1[0].ReadOpArg = Taito.Z0ReadOp_bublbobl;
                            Z80A.zz1[0].ReadMemory = Taito.Z0ReadMemory_bublbobl;
                            Z80A.zz1[0].WriteMemory = Taito.Z0WriteMemory_bublbobl;
                            Z80A.zz1[0].ReadHardware = Taito.Z0ReadHardware;
                            Z80A.zz1[0].WriteHardware = Taito.Z0WriteHardware;
                            Z80A.zz1[0].IRQCallback = Taito.Z0IRQCallback;
                            Z80A.zz1[1].ReadOp = Taito.Z1ReadOp_bublbobl;
                            Z80A.zz1[1].ReadOpArg = Taito.Z1ReadOp_bublbobl;
                            Z80A.zz1[1].ReadMemory = Taito.Z1ReadMemory_bublbobl;
                            Z80A.zz1[1].WriteMemory = Taito.Z1WriteMemory_bublbobl;
                            Z80A.zz1[1].ReadHardware = Taito.Z1ReadHardware;
                            Z80A.zz1[1].WriteHardware = Taito.Z1WriteHardware;
                            Z80A.zz1[1].IRQCallback = Taito.Z1IRQCallback;
                            Z80A.zz1[2].ReadOp = Taito.Z2ReadOp_bublbobl;
                            Z80A.zz1[2].ReadOpArg = Taito.Z2ReadOp_bublbobl;
                            Z80A.zz1[2].ReadMemory = Taito.Z2ReadMemory_bublbobl;
                            Z80A.zz1[2].WriteMemory = Taito.Z2WriteMemory_bublbobl;
                            Z80A.zz1[2].ReadHardware = Taito.Z2ReadHardware;
                            Z80A.zz1[2].WriteHardware = Taito.Z2WriteHardware;
                            Z80A.zz1[2].IRQCallback = Taito.Z2IRQCallback;
                            M6800.m1.ReadOp = Taito.MReadOp_bublbobl;
                            M6800.m1.ReadOpArg = Taito.MReadOp_bublbobl;
                            M6800.m1.ReadMemory = Taito.MReadMemory_bublbobl;
                            M6800.m1.WriteMemory = Taito.MWriteMemory_bublbobl;
                            M6800.m1.ReadIO = Taito.MReadHardware;
                            M6800.m1.WriteIO = Taito.MWriteHardware;
                            break;
                        case "boblbobl":
                        case "sboblbobl":
                        case "sboblbobla":
                        case "sboblboblb":
                        case "sboblbobld":
                        case "sboblboblc":
                        case "dland":
                        case "bbredux":
                        case "bublboblb":
                        case "boblcave":
                            Z80A.zz1[0].ReadOp = Taito.Z0ReadOp_bublbobl;
                            Z80A.zz1[0].ReadOpArg = Taito.Z0ReadOp_bublbobl;
                            Z80A.zz1[0].ReadMemory = Taito.Z0ReadMemory_bootleg;
                            Z80A.zz1[0].WriteMemory = Taito.Z0WriteMemory_bootleg;
                            Z80A.zz1[0].ReadHardware = Taito.Z0ReadHardware;
                            Z80A.zz1[0].WriteHardware = Taito.Z0WriteHardware;
                            Z80A.zz1[0].IRQCallback = Taito.Z0IRQCallback;
                            Z80A.zz1[1].ReadOp = Taito.Z1ReadOp_bublbobl;
                            Z80A.zz1[1].ReadOpArg = Taito.Z1ReadOp_bublbobl;
                            Z80A.zz1[1].ReadMemory = Taito.Z1ReadMemory_bublbobl;
                            Z80A.zz1[1].WriteMemory = Taito.Z1WriteMemory_bublbobl;
                            Z80A.zz1[1].ReadHardware = Taito.Z1ReadHardware;
                            Z80A.zz1[1].WriteHardware = Taito.Z1WriteHardware;
                            Z80A.zz1[1].IRQCallback = Taito.Z1IRQCallback;
                            Z80A.zz1[2].ReadOp = Taito.Z2ReadOp_bublbobl;
                            Z80A.zz1[2].ReadOpArg = Taito.Z2ReadOp_bublbobl;
                            Z80A.zz1[2].ReadMemory = Taito.Z2ReadMemory_bublbobl;
                            Z80A.zz1[2].WriteMemory = Taito.Z2WriteMemory_bublbobl;
                            Z80A.zz1[2].ReadHardware = Taito.Z2ReadHardware;
                            Z80A.zz1[2].WriteHardware = Taito.Z2WriteHardware;
                            Z80A.zz1[2].IRQCallback = Taito.Z2IRQCallback;
                            break;
                        case "bub68705":
                            Z80A.zz1[0].ReadOp = Taito.Z0ReadOp_bublbobl;
                            Z80A.zz1[0].ReadOpArg = Taito.Z0ReadOp_bublbobl;
                            Z80A.zz1[0].ReadMemory = Taito.Z0ReadMemory_bublbobl;
                            Z80A.zz1[0].WriteMemory = Taito.Z0WriteMemory_bublbobl;
                            Z80A.zz1[0].ReadHardware = Taito.Z0ReadHardware;
                            Z80A.zz1[0].WriteHardware = Taito.Z0WriteHardware;
                            Z80A.zz1[0].IRQCallback = Taito.Z0IRQCallback;
                            Z80A.zz1[1].ReadOp = Taito.Z1ReadOp_bublbobl;
                            Z80A.zz1[1].ReadOpArg = Taito.Z1ReadOp_bublbobl;
                            Z80A.zz1[1].ReadMemory = Taito.Z1ReadMemory_bublbobl;
                            Z80A.zz1[1].WriteMemory = Taito.Z1WriteMemory_bublbobl;
                            Z80A.zz1[1].ReadHardware = Taito.Z1ReadHardware;
                            Z80A.zz1[1].WriteHardware = Taito.Z1WriteHardware;
                            Z80A.zz1[1].IRQCallback = Taito.Z1IRQCallback;
                            Z80A.zz1[2].ReadOp = Taito.Z2ReadOp_bublbobl;
                            Z80A.zz1[2].ReadOpArg = Taito.Z2ReadOp_bublbobl;
                            Z80A.zz1[2].ReadMemory = Taito.Z2ReadMemory_bublbobl;
                            Z80A.zz1[2].WriteMemory = Taito.Z2WriteMemory_bublbobl;
                            Z80A.zz1[2].ReadHardware = Taito.Z2ReadHardware;
                            Z80A.zz1[2].WriteHardware = Taito.Z2WriteHardware;
                            Z80A.zz1[2].IRQCallback = Taito.Z2IRQCallback;
                            M6805.m1.ReadOp = Taito.MReadOp_bootleg;
                            M6805.m1.ReadOpArg = Taito.MReadOp_bootleg;
                            M6805.m1.ReadMemory = Taito.MReadMemory_bootleg;
                            M6805.m1.WriteMemory = Taito.MWriteMemory_bootleg;
                            break;
                        case "opwolf":
                        case "opwolfa":
                        case "opwolfj":
                        case "opwolfu":
                            MC68000.m1.ReadOpByte = Taito.MReadOpByte_opwolf;
                            MC68000.m1.ReadByte = Taito.MReadByte_opwolf;
                            MC68000.m1.ReadOpWord = Taito.MReadOpWord_opwolf;
                            MC68000.m1.ReadWord = MC68000.m1.ReadPcrelWord = Taito.MReadWord_opwolf;
                            MC68000.m1.ReadOpLong = Taito.MReadOpLong_opwolf;
                            MC68000.m1.ReadLong = MC68000.m1.ReadPcrelLong = Taito.MReadLong_opwolf;
                            MC68000.m1.WriteByte = Taito.MWriteByte_opwolf;
                            MC68000.m1.WriteWord = Taito.MWriteWord_opwolf;
                            MC68000.m1.WriteLong = Taito.MWriteLong_opwolf;
                            Z80A.zz1[0].ReadOp = Taito.ZReadOp_opwolf;
                            Z80A.zz1[0].ReadOpArg = Taito.ZReadOp_opwolf;
                            Z80A.zz1[0].ReadMemory = Taito.ZReadMemory_opwolf;
                            Z80A.zz1[0].WriteMemory = Taito.ZWriteMemory_opwolf;
                            Z80A.zz1[0].ReadHardware = Taito.Z0ReadHardware;
                            Z80A.zz1[0].WriteHardware = Taito.Z0WriteHardware;
                            Z80A.zz1[0].IRQCallback = Taito.Z0IRQCallback;
                            break;
                        case "opwolfb":
                            MC68000.m1.ReadOpByte = Taito.MReadOpByte_opwolf;
                            MC68000.m1.ReadByte = Taito.MReadByte_opwolfb;
                            MC68000.m1.ReadOpWord = Taito.MReadOpWord_opwolf;
                            MC68000.m1.ReadWord = MC68000.m1.ReadPcrelWord = Taito.MReadWord_opwolfb;
                            MC68000.m1.ReadOpLong = Taito.MReadOpLong_opwolf;
                            MC68000.m1.ReadLong = MC68000.m1.ReadPcrelLong = Taito.MReadLong_opwolfb;
                            MC68000.m1.WriteByte = Taito.MWriteByte_opwolfb;
                            MC68000.m1.WriteWord = Taito.MWriteWord_opwolfb;
                            MC68000.m1.WriteLong = Taito.MWriteLong_opwolfb;
                            Z80A.zz1[0].ReadOp = Taito.ZReadOp_opwolf;
                            Z80A.zz1[0].ReadOpArg = Taito.ZReadOp_opwolf;
                            Z80A.zz1[0].ReadMemory = Taito.ZReadMemory_opwolf;
                            Z80A.zz1[0].WriteMemory = Taito.ZWriteMemory_opwolf;
                            Z80A.zz1[0].ReadHardware = Taito.Z0ReadHardware;
                            Z80A.zz1[0].WriteHardware = Taito.Z0WriteHardware;
                            Z80A.zz1[0].IRQCallback = Taito.Z0IRQCallback;
                            Z80A.zz1[1].ReadOp = Taito.ZReadOp_opwolf_sub;
                            Z80A.zz1[1].ReadOpArg = Taito.ZReadOp_opwolf_sub;
                            Z80A.zz1[1].ReadMemory = Taito.ZReadMemory_opwolf_sub;
                            Z80A.zz1[1].WriteMemory = Taito.ZWriteMemory_opwolf_sub;
                            Z80A.zz1[1].ReadHardware = Taito.Z0ReadHardware;
                            Z80A.zz1[1].WriteHardware = Taito.Z0WriteHardware;
                            Z80A.zz1[1].IRQCallback = Taito.Z1IRQCallback;
                            break;
                        case "opwolfp":
                            MC68000.m1.ReadOpByte = Taito.MReadOpByte_opwolf;
                            MC68000.m1.ReadByte = Taito.MReadByte_opwolfp;
                            MC68000.m1.ReadOpWord = Taito.MReadOpWord_opwolf;
                            MC68000.m1.ReadWord = MC68000.m1.ReadPcrelWord = Taito.MReadWord_opwolfp;
                            MC68000.m1.ReadOpLong = Taito.MReadOpLong_opwolf;
                            MC68000.m1.ReadLong = MC68000.m1.ReadPcrelLong = Taito.MReadLong_opwolfp;
                            MC68000.m1.WriteByte = Taito.MWriteByte_opwolfp;
                            MC68000.m1.WriteWord = Taito.MWriteWord_opwolfp;
                            MC68000.m1.WriteLong = Taito.MWriteLong_opwolfp;
                            Z80A.zz1[0].ReadOp = Taito.ZReadOp_opwolf;
                            Z80A.zz1[0].ReadOpArg = Taito.ZReadOp_opwolf;
                            Z80A.zz1[0].ReadMemory = Taito.ZReadMemory_opwolf;
                            Z80A.zz1[0].WriteMemory = Taito.ZWriteMemory_opwolf;
                            Z80A.zz1[0].ReadHardware = Taito.Z0ReadHardware;
                            Z80A.zz1[0].WriteHardware = Taito.Z0WriteHardware;
                            Z80A.zz1[0].IRQCallback = Taito.Z0IRQCallback;
                            break;
                    }
                    break;
                case "Taito B":
                    Z80A.zz1[0].ReadOp = Taitob.ZReadOp;
                    Z80A.zz1[0].ReadOpArg = Taitob.ZReadOp;
                    Z80A.zz1[0].ReadMemory = Taitob.ZReadMemory;
                    Z80A.zz1[0].WriteMemory = Taitob.ZWriteMemory;
                    Z80A.zz1[0].ReadHardware = Taitob.ZReadHardware;
                    Z80A.zz1[0].WriteHardware = Taitob.ZWriteHardware;
                    Z80A.zz1[0].IRQCallback = Taitob.ZIRQCallback;
                    switch (Machine.sName)
                    {
                        case "pbobble":
                            MC68000.m1.ReadOpByte = Taitob.MReadOpByte_pbobble;
                            MC68000.m1.ReadByte = Taitob.MReadByte_pbobble;
                            MC68000.m1.ReadOpWord = Taitob.MReadOpWord_pbobble;
                            MC68000.m1.ReadWord = MC68000.m1.ReadPcrelWord = Taitob.MReadWord_pbobble;
                            MC68000.m1.ReadOpLong = Taitob.MReadOpLong_pbobble;
                            MC68000.m1.ReadLong = MC68000.m1.ReadPcrelLong = Taitob.MReadLong_pbobble;
                            MC68000.m1.WriteByte = Taitob.MWriteByte_pbobble;
                            MC68000.m1.WriteWord = Taitob.MWriteWord_pbobble;
                            MC68000.m1.WriteLong = Taitob.MWriteLong_pbobble;
                            break;
                        case "silentd":
                        case "silentdj":
                        case "silentdu":
                            MC68000.m1.ReadOpByte = Taitob.MReadOpByte_silentd;
                            MC68000.m1.ReadByte = Taitob.MReadByte_silentd;
                            MC68000.m1.ReadOpWord = Taitob.MReadOpWord_silentd;
                            MC68000.m1.ReadWord = MC68000.m1.ReadPcrelWord = Taitob.MReadWord_silentd;
                            MC68000.m1.ReadOpLong = Taitob.MReadOpLong_silentd;
                            MC68000.m1.ReadLong = MC68000.m1.ReadPcrelLong = Taitob.MReadLong_silentd;
                            MC68000.m1.WriteByte = Taitob.MWriteByte_silentd;
                            MC68000.m1.WriteWord = Taitob.MWriteWord_silentd;
                            MC68000.m1.WriteLong = Taitob.MWriteLong_silentd;
                            break;
                    }
                    break;
                case "Konami 68000":
                    Z80A.zz1[0].ReadHardware = Konami68000.ZReadHardware;
                    Z80A.zz1[0].WriteHardware = Konami68000.ZWriteHardware;
                    Z80A.zz1[0].IRQCallback = Konami68000.ZIRQCallback;
                    switch (Machine.sName)
                    {
                        case "cuebrick":
                            MC68000.m1.ReadOpByte = Konami68000.MReadOpByte_cuebrick;
                            MC68000.m1.ReadByte = Konami68000.MReadByte_cuebrick;
                            MC68000.m1.ReadOpWord = Konami68000.MReadOpWord_cuebrick;
                            MC68000.m1.ReadWord = MC68000.m1.ReadPcrelWord = Konami68000.MReadWord_cuebrick;
                            MC68000.m1.ReadOpLong = Konami68000.MReadOpLong_cuebrick;
                            MC68000.m1.ReadLong = MC68000.m1.ReadPcrelLong = Konami68000.MReadLong_cuebrick;
                            MC68000.m1.WriteByte = Konami68000.MWriteByte_cuebrick;
                            MC68000.m1.WriteWord = Konami68000.MWriteWord_cuebrick;
                            MC68000.m1.WriteLong = Konami68000.MWriteLong_cuebrick;
                            break;
                        case "mia":
                        case "mia2":
                            MC68000.m1.ReadOpByte = Konami68000.MReadOpByte_mia;
                            MC68000.m1.ReadByte = Konami68000.MReadByte_mia;
                            MC68000.m1.ReadOpWord = Konami68000.MReadOpWord_mia;
                            MC68000.m1.ReadWord = MC68000.m1.ReadPcrelWord = Konami68000.MReadWord_mia;
                            MC68000.m1.ReadOpLong = Konami68000.MReadOpLong_mia;
                            MC68000.m1.ReadLong = MC68000.m1.ReadPcrelLong = Konami68000.MReadLong_mia;
                            MC68000.m1.WriteByte = Konami68000.MWriteByte_mia;
                            MC68000.m1.WriteWord = Konami68000.MWriteWord_mia;
                            MC68000.m1.WriteLong = Konami68000.MWriteLong_mia;
                            Z80A.zz1[0].ReadOp = Konami68000.ZReadOp_mia;
                            Z80A.zz1[0].ReadOpArg = Konami68000.ZReadOp_mia;
                            Z80A.zz1[0].ReadMemory = Konami68000.ZReadMemory_mia;
                            Z80A.zz1[0].WriteMemory = Konami68000.ZWriteMemory_mia;
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
                            MC68000.m1.ReadOpByte = Konami68000.MReadOpByte_tmnt;
                            MC68000.m1.ReadByte = Konami68000.MReadByte_tmnt;
                            MC68000.m1.ReadOpWord = Konami68000.MReadOpWord_tmnt;
                            MC68000.m1.ReadWord = MC68000.m1.ReadPcrelWord = Konami68000.MReadWord_tmnt;
                            MC68000.m1.ReadOpLong = Konami68000.MReadOpLong_tmnt;
                            MC68000.m1.ReadLong = MC68000.m1.ReadPcrelLong = Konami68000.MReadLong_tmnt;
                            MC68000.m1.WriteByte = Konami68000.MWriteByte_tmnt;
                            MC68000.m1.WriteWord = Konami68000.MWriteWord_tmnt;
                            MC68000.m1.WriteLong = Konami68000.MWriteLong_tmnt;
                            Z80A.zz1[0].ReadOp = Konami68000.ZReadOp_tmnt;
                            Z80A.zz1[0].ReadOpArg = Konami68000.ZReadOp_tmnt;
                            Z80A.zz1[0].ReadMemory = Konami68000.ZReadMemory_tmnt;
                            Z80A.zz1[0].WriteMemory = Konami68000.ZWriteMemory_tmnt;
                            break;
                        case "punkshot":
                        case "punkshot2":
                        case "punkshotj":
                            MC68000.m1.ReadOpByte = Konami68000.MReadOpByte_punkshot;
                            MC68000.m1.ReadByte = Konami68000.MReadByte_punkshot;
                            MC68000.m1.ReadOpWord = Konami68000.MReadOpWord_punkshot;
                            MC68000.m1.ReadWord = MC68000.m1.ReadPcrelWord = Konami68000.MReadWord_punkshot;
                            MC68000.m1.ReadOpLong = Konami68000.MReadOpLong_punkshot;
                            MC68000.m1.ReadLong = MC68000.m1.ReadPcrelLong = Konami68000.MReadLong_punkshot;
                            MC68000.m1.WriteByte = Konami68000.MWriteByte_punkshot;
                            MC68000.m1.WriteWord = Konami68000.MWriteWord_punkshot;
                            MC68000.m1.WriteLong = Konami68000.MWriteLong_punkshot;
                            Z80A.zz1[0].ReadOp = Konami68000.ZReadOp_punkshot;
                            Z80A.zz1[0].ReadOpArg = Konami68000.ZReadOp_punkshot;
                            Z80A.zz1[0].ReadMemory = Konami68000.ZReadMemory_punkshot;
                            Z80A.zz1[0].WriteMemory = Konami68000.ZWriteMemory_punkshot;
                            break;
                        case "lgtnfght":
                        case "lgtnfghta":
                        case "lgtnfghtu":
                        case "trigon":
                            MC68000.m1.ReadOpByte = Konami68000.MReadOpByte_lgtnfght;
                            MC68000.m1.ReadByte = Konami68000.MReadByte_lgtnfght;
                            MC68000.m1.ReadOpWord = Konami68000.MReadOpWord_lgtnfght;
                            MC68000.m1.ReadWord = MC68000.m1.ReadPcrelWord = Konami68000.MReadWord_lgtnfght;
                            MC68000.m1.ReadOpLong = Konami68000.MReadOpLong_lgtnfght;
                            MC68000.m1.ReadLong = MC68000.m1.ReadPcrelLong = Konami68000.MReadLong_lgtnfght;
                            MC68000.m1.WriteByte = Konami68000.MWriteByte_lgtnfght;
                            MC68000.m1.WriteWord = Konami68000.MWriteWord_lgtnfght;
                            MC68000.m1.WriteLong = Konami68000.MWriteLong_lgtnfght;
                            Z80A.zz1[0].ReadOp = Konami68000.ZReadOp_lgtnfght;
                            Z80A.zz1[0].ReadOpArg = Konami68000.ZReadOp_lgtnfght;
                            Z80A.zz1[0].ReadMemory = Konami68000.ZReadMemory_lgtnfght;
                            Z80A.zz1[0].WriteMemory = Konami68000.ZWriteMemory_lgtnfght;
                            break;
                        case "blswhstl":
                        case "blswhstla":
                        case "detatwin":
                            MC68000.m1.ReadOpByte = Konami68000.MReadOpByte_blswhstl;
                            MC68000.m1.ReadByte = Konami68000.MReadByte_blswhstl;
                            MC68000.m1.ReadOpWord = Konami68000.MReadOpWord_blswhstl;
                            MC68000.m1.ReadWord = MC68000.m1.ReadPcrelWord = Konami68000.MReadWord_blswhstl;
                            MC68000.m1.ReadOpLong = Konami68000.MReadOpLong_blswhstl;
                            MC68000.m1.ReadLong = MC68000.m1.ReadPcrelLong = Konami68000.MReadLong_blswhstl;
                            MC68000.m1.WriteByte = Konami68000.MWriteByte_blswhstl;
                            MC68000.m1.WriteWord = Konami68000.MWriteWord_blswhstl;
                            MC68000.m1.WriteLong = Konami68000.MWriteLong_blswhstl;
                            Z80A.zz1[0].ReadOp = Konami68000.ZReadOp_ssriders;
                            Z80A.zz1[0].ReadOpArg = Konami68000.ZReadOp_ssriders;
                            Z80A.zz1[0].ReadMemory = Konami68000.ZReadMemory_ssriders;
                            Z80A.zz1[0].WriteMemory = Konami68000.ZWriteMemory_ssriders;
                            break;
                        case "glfgreat":
                        case "glfgreatj":
                            MC68000.m1.ReadOpByte = Konami68000.MReadOpByte_glfgreat;
                            MC68000.m1.ReadByte = Konami68000.MReadByte_glfgreat;
                            MC68000.m1.ReadOpWord = Konami68000.MReadOpWord_glfgreat;
                            MC68000.m1.ReadWord = MC68000.m1.ReadPcrelWord = Konami68000.MReadWord_glfgreat;
                            MC68000.m1.ReadOpLong = Konami68000.MReadOpLong_glfgreat;
                            MC68000.m1.ReadLong = MC68000.m1.ReadPcrelLong = Konami68000.MReadLong_glfgreat;
                            MC68000.m1.WriteByte = Konami68000.MWriteByte_glfgreat;
                            MC68000.m1.WriteWord = Konami68000.MWriteWord_glfgreat;
                            MC68000.m1.WriteLong = Konami68000.MWriteLong_glfgreat;
                            Z80A.zz1[0].ReadOp = Konami68000.ZReadOp_glfgreat;
                            Z80A.zz1[0].ReadOpArg = Konami68000.ZReadOp_glfgreat;
                            Z80A.zz1[0].ReadMemory = Konami68000.ZReadMemory_glfgreat;
                            Z80A.zz1[0].WriteMemory = Konami68000.ZWriteMemory_glfgreat;
                            break;
                        case "tmnt2":
                        case "tmnt2a":
                        case "tmht22pe":
                        case "tmht24pe":
                        case "tmnt22pu":
                        case "qgakumon":
                            MC68000.m1.ReadOpByte = Konami68000.MReadOpByte_tmnt2;
                            MC68000.m1.ReadByte = Konami68000.MReadByte_tmnt2;
                            MC68000.m1.ReadOpWord = Konami68000.MReadOpWord_tmnt2;
                            MC68000.m1.ReadWord = MC68000.m1.ReadPcrelWord = Konami68000.MReadWord_tmnt2;
                            MC68000.m1.ReadOpLong = Konami68000.MReadOpLong_tmnt2;
                            MC68000.m1.ReadLong = MC68000.m1.ReadPcrelLong = Konami68000.MReadLong_tmnt2;
                            MC68000.m1.WriteByte = Konami68000.MWriteByte_tmnt2;
                            MC68000.m1.WriteWord = Konami68000.MWriteWord_tmnt2;
                            MC68000.m1.WriteLong = Konami68000.MWriteLong_tmnt2;
                            Z80A.zz1[0].ReadOp = Konami68000.ZReadOp_ssriders;
                            Z80A.zz1[0].ReadOpArg = Konami68000.ZReadOp_ssriders;
                            Z80A.zz1[0].ReadMemory = Konami68000.ZReadMemory_ssriders;
                            Z80A.zz1[0].WriteMemory = Konami68000.ZWriteMemory_ssriders;
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
                            MC68000.m1.ReadOpByte = Konami68000.MReadOpByte_ssriders;
                            MC68000.m1.ReadByte = Konami68000.MReadByte_ssriders;
                            MC68000.m1.ReadOpWord = Konami68000.MReadOpWord_ssriders;
                            MC68000.m1.ReadWord = MC68000.m1.ReadPcrelWord = Konami68000.MReadWord_ssriders;
                            MC68000.m1.ReadOpLong = Konami68000.MReadOpLong_ssriders;
                            MC68000.m1.ReadLong = MC68000.m1.ReadPcrelLong = Konami68000.MReadLong_ssriders;
                            MC68000.m1.WriteByte = Konami68000.MWriteByte_ssriders;
                            MC68000.m1.WriteWord = Konami68000.MWriteWord_ssriders;
                            MC68000.m1.WriteLong = Konami68000.MWriteLong_ssriders;
                            Z80A.zz1[0].ReadOp = Konami68000.ZReadOp_ssriders;
                            Z80A.zz1[0].ReadOpArg = Konami68000.ZReadOp_ssriders;
                            Z80A.zz1[0].ReadMemory = Konami68000.ZReadMemory_ssriders;
                            Z80A.zz1[0].WriteMemory = Konami68000.ZWriteMemory_ssriders;
                            break;
                        case "thndrx2":
                        case "thndrx2a":
                        case "thndrx2j":
                            MC68000.m1.ReadOpByte = Konami68000.MReadOpByte_thndrx2;
                            MC68000.m1.ReadByte = Konami68000.MReadByte_thndrx2;
                            MC68000.m1.ReadOpWord = Konami68000.MReadOpWord_thndrx2;
                            MC68000.m1.ReadWord = MC68000.m1.ReadPcrelWord = Konami68000.MReadWord_thndrx2;
                            MC68000.m1.ReadOpLong = Konami68000.MReadOpLong_thndrx2;
                            MC68000.m1.ReadLong = MC68000.m1.ReadPcrelLong = Konami68000.MReadLong_thndrx2;
                            MC68000.m1.WriteByte = Konami68000.MWriteByte_thndrx2;
                            MC68000.m1.WriteWord = Konami68000.MWriteWord_thndrx2;
                            MC68000.m1.WriteLong = Konami68000.MWriteLong_thndrx2;
                            Z80A.zz1[0].ReadOp = Konami68000.ZReadOp_thndrx2;
                            Z80A.zz1[0].ReadOpArg = Konami68000.ZReadOp_thndrx2;
                            Z80A.zz1[0].ReadMemory = Konami68000.ZReadMemory_thndrx2;
                            Z80A.zz1[0].WriteMemory = Konami68000.ZWriteMemory_thndrx2;
                            break;
                        case "prmrsocr":
                        case "prmrsocrj":
                            MC68000.m1.ReadOpByte = Konami68000.MReadOpByte_prmrsocr;
                            MC68000.m1.ReadByte = Konami68000.MReadByte_prmrsocr;
                            MC68000.m1.ReadOpWord = Konami68000.MReadOpWord_prmrsocr;
                            MC68000.m1.ReadWord = MC68000.m1.ReadPcrelWord = Konami68000.MReadWord_prmrsocr;
                            MC68000.m1.ReadOpLong = Konami68000.MReadOpLong_prmrsocr;
                            MC68000.m1.ReadLong = MC68000.m1.ReadPcrelLong = Konami68000.MReadLong_prmrsocr;
                            MC68000.m1.WriteByte = Konami68000.MWriteByte_prmrsocr;
                            MC68000.m1.WriteWord = Konami68000.MWriteWord_prmrsocr;
                            MC68000.m1.WriteLong = Konami68000.MWriteLong_prmrsocr;
                            Z80A.zz1[0].ReadOp = Konami68000.ZReadOp_prmrsocr;
                            Z80A.zz1[0].ReadOpArg = Konami68000.ZReadOp_prmrsocr;
                            Z80A.zz1[0].ReadMemory = Konami68000.ZReadMemory_prmrsocr;
                            Z80A.zz1[0].WriteMemory = Konami68000.ZWriteMemory_prmrsocr;
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
                            M6809.mm1[0].ReadOp = Capcom.MReadOpByte_gng;
                            M6809.mm1[0].ReadOpArg = Capcom.MReadOpByte_gng;
                            M6809.mm1[0].RM = Capcom.MReadByte_gng;
                            M6809.mm1[0].WM = Capcom.MWriteByte_gng;
                            Z80A.zz1[0].ReadOp = Capcom.ZReadOp_gng;
                            Z80A.zz1[0].ReadOpArg = Capcom.ZReadOp_gng;
                            Z80A.zz1[0].ReadMemory = Capcom.ZReadMemory_gng;
                            Z80A.zz1[0].WriteMemory = Capcom.ZWriteMemory_gng;
                            Z80A.zz1[0].ReadHardware = Capcom.Z0ReadHardware;
                            Z80A.zz1[0].WriteHardware = Capcom.Z0WriteHardware;
                            Z80A.zz1[0].IRQCallback = Capcom.Z0IRQCallback;
                            break;
                        case "diamond":
                            M6809.mm1[0].ReadOp = Capcom.MReadOpByte_gng;
                            M6809.mm1[0].ReadOpArg = Capcom.MReadOpByte_gng;
                            M6809.mm1[0].RM = Capcom.MReadByte_diamond;
                            M6809.mm1[0].WM = Capcom.MWriteByte_gng;
                            Z80A.zz1[0].ReadOp = Capcom.ZReadOp_gng;
                            Z80A.zz1[0].ReadOpArg = Capcom.ZReadOp_gng;
                            Z80A.zz1[0].ReadMemory = Capcom.ZReadMemory_gng;
                            Z80A.zz1[0].WriteMemory = Capcom.ZWriteMemory_gng;
                            Z80A.zz1[0].ReadHardware = Capcom.Z0ReadHardware;
                            Z80A.zz1[0].WriteHardware = Capcom.Z0WriteHardware;
                            Z80A.zz1[0].IRQCallback = Capcom.Z0IRQCallback;
                            break;
                        case "sf":
                            MC68000.m1.ReadOpByte = Capcom.MReadOpByte_sfus;
                            MC68000.m1.ReadByte = Capcom.MReadByte_sfus;
                            MC68000.m1.ReadOpWord = Capcom.MReadOpWord_sfus;
                            MC68000.m1.ReadWord = MC68000.m1.ReadPcrelWord = Capcom.MReadWord_sfus;
                            MC68000.m1.ReadOpLong = Capcom.MReadOpLong_sfus;
                            MC68000.m1.ReadLong = MC68000.m1.ReadPcrelLong = Capcom.MReadLong_sfus;
                            MC68000.m1.WriteByte = Capcom.MWriteByte_sf;
                            MC68000.m1.WriteWord = Capcom.MWriteWord_sf;
                            MC68000.m1.WriteLong = Capcom.MWriteLong_sf;
                            Z80A.zz1[0].ReadOp = Capcom.Z0ReadOp_sf;
                            Z80A.zz1[0].ReadOpArg = Capcom.Z0ReadOp_sf;
                            Z80A.zz1[0].ReadMemory = Capcom.Z0ReadMemory_sf;
                            Z80A.zz1[0].WriteMemory = Capcom.Z0WriteMemory_sf;
                            Z80A.zz1[0].ReadHardware = Capcom.Z0ReadHardware;
                            Z80A.zz1[0].WriteHardware = Capcom.Z0WriteHardware;
                            Z80A.zz1[0].IRQCallback = Capcom.Z0IRQCallback;
                            Z80A.zz1[1].ReadOp = Capcom.Z1ReadOp_sf;
                            Z80A.zz1[1].ReadOpArg = Capcom.Z1ReadOp_sf;
                            Z80A.zz1[1].ReadMemory = Capcom.Z1ReadMemory_sf;
                            Z80A.zz1[1].WriteMemory = Capcom.Z1WriteMemory_sf;
                            Z80A.zz1[1].ReadHardware = Capcom.Z1ReadHardware;
                            Z80A.zz1[1].WriteHardware = Capcom.Z1WriteHardware;
                            Z80A.zz1[1].IRQCallback = Capcom.Z1IRQCallback;
                            break;
                        case "sfua":
                        case "sfj":
                            MC68000.m1.ReadOpByte = Capcom.MReadByte_sfjp;
                            MC68000.m1.ReadByte = Capcom.MReadByte_sfjp;
                            MC68000.m1.ReadOpWord = Capcom.MReadOpWord_sfus;
                            MC68000.m1.ReadWord = MC68000.m1.ReadPcrelWord = Capcom.MReadWord_sfjp;
                            MC68000.m1.ReadOpLong = Capcom.MReadOpLong_sfus;
                            MC68000.m1.ReadLong = MC68000.m1.ReadPcrelLong = Capcom.MReadLong_sfus;
                            MC68000.m1.WriteByte = Capcom.MWriteByte_sf;
                            MC68000.m1.WriteWord = Capcom.MWriteWord_sf;
                            MC68000.m1.WriteLong = Capcom.MWriteLong_sf;
                            Z80A.zz1[0].ReadOp = Capcom.Z0ReadOp_sf;
                            Z80A.zz1[0].ReadOpArg = Capcom.Z0ReadOp_sf;
                            Z80A.zz1[0].ReadMemory = Capcom.Z0ReadMemory_sf;
                            Z80A.zz1[0].WriteMemory = Capcom.Z0WriteMemory_sf;
                            Z80A.zz1[0].ReadHardware = Capcom.Z0ReadHardware;
                            Z80A.zz1[0].WriteHardware = Capcom.Z0WriteHardware;
                            Z80A.zz1[0].IRQCallback = Capcom.Z0IRQCallback;
                            Z80A.zz1[1].ReadOp = Capcom.Z1ReadOp_sf;
                            Z80A.zz1[1].ReadOpArg = Capcom.Z1ReadOp_sf;
                            Z80A.zz1[1].ReadMemory = Capcom.Z1ReadMemory_sf;
                            Z80A.zz1[1].WriteMemory = Capcom.Z1WriteMemory_sf;
                            Z80A.zz1[1].ReadHardware = Capcom.Z1ReadHardware;
                            Z80A.zz1[1].WriteHardware = Capcom.Z1WriteHardware;
                            Z80A.zz1[1].IRQCallback = Capcom.Z1IRQCallback;
                            break;
                        case "sfjan":
                        case "sfan":
                        case "sfp":
                            MC68000.m1.ReadOpByte = Capcom.MReadByte_sf;
                            MC68000.m1.ReadByte = Capcom.MReadByte_sf;
                            MC68000.m1.ReadOpWord = Capcom.MReadOpWord_sfus;
                            MC68000.m1.ReadWord = MC68000.m1.ReadPcrelWord = Capcom.MReadWord_sf;
                            MC68000.m1.ReadOpLong = Capcom.MReadOpLong_sfus;
                            MC68000.m1.ReadLong = MC68000.m1.ReadPcrelLong = Capcom.MReadLong_sfus;
                            MC68000.m1.WriteByte = Capcom.MWriteByte_sf;
                            MC68000.m1.WriteWord = Capcom.MWriteWord_sf;
                            MC68000.m1.WriteLong = Capcom.MWriteLong_sf;
                            MC68000.m1.WriteByte = Capcom.MWriteByte_sf;
                            MC68000.m1.WriteWord = Capcom.MWriteWord_sf;
                            MC68000.m1.WriteLong = Capcom.MWriteLong_sf;
                            Z80A.zz1[0].ReadOp = Capcom.Z0ReadOp_sf;
                            Z80A.zz1[0].ReadOpArg = Capcom.Z0ReadOp_sf;
                            Z80A.zz1[0].ReadMemory = Capcom.Z0ReadMemory_sf;
                            Z80A.zz1[0].WriteMemory = Capcom.Z0WriteMemory_sf;
                            Z80A.zz1[0].ReadHardware = Capcom.Z0ReadHardware;
                            Z80A.zz1[0].WriteHardware = Capcom.Z0WriteHardware;
                            Z80A.zz1[0].IRQCallback = Capcom.Z0IRQCallback;
                            Z80A.zz1[1].ReadOp = Capcom.Z1ReadOp_sf;
                            Z80A.zz1[1].ReadOpArg = Capcom.Z1ReadOp_sf;
                            Z80A.zz1[1].ReadMemory = Capcom.Z1ReadMemory_sf;
                            Z80A.zz1[1].WriteMemory = Capcom.Z1WriteMemory_sf;
                            Z80A.zz1[1].ReadHardware = Capcom.Z1ReadHardware;
                            Z80A.zz1[1].WriteHardware = Capcom.Z1WriteHardware;
                            Z80A.zz1[1].IRQCallback = Capcom.Z1IRQCallback;
                            break;
                    }
                    break;
            }
            cpu_inittimers();
            switch (Machine.sBoard)
            {
                case "CPS-1":
                case "CPS-1(QSound)":
                case "CPS2":
                case "Neo Geo":
                case "PGM":
                case "Taito B":
                    break;
                case "Tehkan":
                    break;
                case "IGS011":
                    break;
                case "SunA8":
                    break;
                case "Namco System 1":
                    //M6809.mm1[0].DisassemblerInit();
                    break;
                case "M72":
                    break;
                case "M92":
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
                            break;
                        case "opwolf":
                        case "opwolfa":
                        case "opwolfj":
                        case "opwolfu":
                        case "opwolfp":
                            break;
                        case "opwolfb":
                            break;
                    }
                    break;
                case "Konami 68000":
                    switch (Machine.sName)
                    {
                        case "cuebrick":
                        default:
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
                            break;
                        case "sf":
                        case "sfua":
                        case "sfj":
                        case "sfjan":
                        case "sfan":
                        case "sfp":
                            break;
                    }
                    break;
            }
            for (icpu = 0; icpu < ncpu; icpu++)
            {
                cpu[icpu].Reset();
            }
        }
        public static void null_callback()
        {

        }
        public static void trigger2()
        {
            if (iloops2 == 0)
            {
                iloops2 = 4;
            }
            iloops2--;
            Generic.irq_1_0_line_hold();
            if (iloops2 > 1)
            {
                EmuTimer.timer_adjust_periodic(Cpuexec.cpu[1].partial_frame_timer, Cpuexec.cpu[1].partial_frame_period, Attotime.ATTOTIME_NEVER);
            }
        }
        public static byte null_callback1(int address)
        {
            return 0;
        }
        public static ushort null_callback2(int address)
        {
            return 0;
        }
        public static void null_callback(int address, byte value)
        {

        }
        public static void null_callback(int address, ushort value)
        {

        }
        private static void cpu_inittimers()
        {
            switch (Machine.sBoard)
            {
                case "CPS-1(QSound)":
                case "CPS2":
                    timedint_period = new Atime(0, (long)(1e18 / 250));
                    //timedint_timer = EmuTimer.timer_alloc_common(EmuTimer.TIME_ACT.Generic_irq_1_0_line_hold, false);
                    EmuTimer.timer_alloc_common(ref timedint_timer, EmuTimer.TIME_ACT.Generic_irq_1_0_line_hold, false);
                    EmuTimer.timer_adjust_periodic(timedint_timer, timedint_period, timedint_period);
                    break;
                case "Neo Geo":
                    //interleave_boost_timer = EmuTimer.timer_alloc_common(EmuTimer.TIME_ACT.Cpuexec_null_callback, false);
                    EmuTimer.timer_alloc_common(ref interleave_boost_timer, EmuTimer.TIME_ACT.Cpuexec_null_callback, false);
                    //interleave_boost_timer_end = EmuTimer.timer_alloc_common(EmuTimer.TIME_ACT.Cpuexec_end_interleave_boost, false);
                    EmuTimer.timer_alloc_common(ref interleave_boost_timer_end, EmuTimer.TIME_ACT.Cpuexec_end_interleave_boost, false);
                    break;
                case "CPS1":
                case "Namco System 1":
                    break;
                case "IGS011":
                    /*switch (Machine.sName)
                    {
                        case "lhb":
                            timeslice_period = new Atime(0, Video.screenstate.frame_period);
                            timeslice_timer = Timer.timer_alloc_common(cpu_timeslicecallback, "cpu_timeslicecallback", false);
                            Timer.timer_adjust_periodic(timeslice_timer, timeslice_period, timeslice_period);
                            break;
                    }*/
                    break;
                case "PGM":
                case "M72":
                case "M92":
                case "Konami 68000":
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
                            timeslice_period = new Atime(0, Video.screenstate.frame_period / 100);
                            EmuTimer.timer_alloc_common(ref timeslice_timer, EmuTimer.TIME_ACT.Cpuexec_cpu_timeslicecallback, false);
                            EmuTimer.timer_adjust_periodic(timeslice_timer, timeslice_period, timeslice_period);
                            break;
                        case "opwolf":
                        case "opwolfa":
                        case "opwolfj":
                        case "opwolfu":
                        case "opwolfb":
                        case "opwolfp":
                            timeslice_period = new Atime(0, Video.screenstate.frame_period / 10);
                            EmuTimer.timer_alloc_common(ref timeslice_timer, EmuTimer.TIME_ACT.Cpuexec_cpu_timeslicecallback, false);
                            EmuTimer.timer_adjust_periodic(timeslice_timer, timeslice_period, timeslice_period);
                            break;
                    }
                    break;
                case "Taito B":
                    timeslice_period = new Atime(0, Video.screenstate.frame_period / 10);
                    EmuTimer.timer_alloc_common(ref timeslice_timer, EmuTimer.TIME_ACT.Cpuexec_cpu_timeslicecallback, false);
                    EmuTimer.timer_adjust_periodic(timeslice_timer, timeslice_period, timeslice_period);
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

                            break;
                        case "sf":
                        case "sfua":
                        case "sfj":
                        case "sfjan":
                        case "sfan":
                        case "sfp":
                            timedint_period = new Atime(0, (long)(1e18 / 8000));
                            EmuTimer.timer_alloc_common(ref timedint_timer, EmuTimer.TIME_ACT.Generic_irq_2_0_line_hold, false);
                            EmuTimer.timer_adjust_periodic(timedint_timer, timedint_period, timedint_period);
                            break;
                    }
                    break;
            }
        }
        public static void cpuexec_timeslice()
        {
            //Atime target = EmuTimer.lt[0].expire;
            Atime target = EmuTimer.lt._frist_Timer.expire;
            Atime tbase = EmuTimer.global_basetime;
            int ran;
            Atime at;
            int i, j;
            for (icpu = 0; icpu < ncpu; icpu++)
            {
                cpu[icpu].suspend = cpu[icpu].nextsuspend;
                cpu[icpu].eatcycles = cpu[icpu].nexteatcycles;
            }
            for (icpu = 0; icpu < ncpu; icpu++)
            {
                if (cpu[icpu].suspend == 0)
                {
                    at = Attotime.attotime_sub(target, cpu[icpu].localtime);
                    cpu[icpu].cycles_running = (int)(at.seconds * cpu[icpu].cycles_per_second + at.attoseconds / cpu[icpu].attoseconds_per_cycle);
                    if (cpu[icpu].cycles_running > 0)
                    {
                        cpu[icpu].cycles_stolen = 0;
                        activecpu = icpu;
                        ran = cpu[icpu].ExecuteCycles(cpu[icpu].cycles_running);
                        activecpu = -1;
                        ran -= cpu[icpu].cycles_stolen;
                        cpu[icpu].totalcycles += (ulong)ran;
                        cpu[icpu].localtime = Attotime.attotime_add(cpu[icpu].localtime, new Atime(ran / cpu[icpu].cycles_per_second, ran * cpu[icpu].attoseconds_per_cycle));
                        if (Attotime.attotime_compare(cpu[icpu].localtime, target) < 0)
                        {
                            if (Attotime.attotime_compare(cpu[icpu].localtime, tbase) > 0)
                                target = cpu[icpu].localtime;
                            else
                                target = tbase;
                        }
                    }
                }
            }
            for (icpu = 0; icpu < ncpu; icpu++)
            {
                if (cpu[icpu].suspend != 0 && cpu[icpu].eatcycles != 0 && Attotime.attotime_compare(cpu[icpu].localtime, target) < 0)
                {
                    at = Attotime.attotime_sub(target, cpu[icpu].localtime);
                    cpu[icpu].cycles_running = (int)(at.seconds * cpu[icpu].cycles_per_second + at.attoseconds / cpu[icpu].attoseconds_per_cycle);
                    cpu[icpu].totalcycles += (ulong)cpu[icpu].cycles_running;
                    cpu[icpu].localtime = Attotime.attotime_add(cpu[icpu].localtime, new Atime(cpu[icpu].cycles_running / cpu[icpu].cycles_per_second, cpu[icpu].cycles_running * cpu[icpu].attoseconds_per_cycle));
                }
                cpu[icpu].suspend = cpu[icpu].nextsuspend;
                cpu[icpu].eatcycles = cpu[icpu].nexteatcycles;
            }
            EmuTimer.timer_set_global_time(target);
            //if (EmuTimer.global_basetime.attoseconds == 0 && Machine.mainMotion.cheatmotion.lockState == CheatMotion.LockState.LOCK_SECOND)
            //{
            //    Machine.mainMotion.cheatmotion.ApplyCheat();
            //}
        }
        public static void cpu_boost_interleave(Atime timeslice_time, Atime boost_duration)
        {
            if (Attotime.attotime_compare(timeslice_time, perfect_interleave) < 0)
                timeslice_time = perfect_interleave;
            EmuTimer.timer_adjust_periodic(interleave_boost_timer, timeslice_time, timeslice_time);
            if (!EmuTimer.timer_enabled(interleave_boost_timer_end) || Attotime.attotime_compare(EmuTimer.timer_timeleft(interleave_boost_timer_end), boost_duration) < 0)
                EmuTimer.timer_adjust_periodic(interleave_boost_timer_end, boost_duration, Attotime.ATTOTIME_NEVER);
        }
        public static void activecpu_abort_timeslice(int cpunum)
        {
            int current_icount;
            current_icount = cpu[cpunum].PendingCycles + 1;
            cpu[cpunum].cycles_stolen += current_icount;
            cpu[cpunum].cycles_running -= current_icount;
            cpu[cpunum].PendingCycles = -1;
        }
        public static void cpunum_suspend(int cpunum, byte reason, byte eatcycles)
        {
            cpu[cpunum].nextsuspend |= reason;
            cpu[cpunum].nexteatcycles = eatcycles;
            if (Cpuexec.activecpu >= 0)
            {
                activecpu_abort_timeslice(Cpuexec.activecpu);
            }
        }
        public static void cpunum_resume(int cpunum, byte reason)
        {
            cpu[cpunum].nextsuspend &= (byte)(~reason);
            if (Cpuexec.activecpu >= 0)
            {
                activecpu_abort_timeslice(Cpuexec.activecpu);
            }
        }
        public static bool cpunum_is_suspended(int cpunum, byte reason)
        {
            return ((cpu[cpunum].nextsuspend & reason) != 0);
        }
        public static Atime cpunum_get_localtime(int cpunum)
        {
            Atime result;
            result = cpu[cpunum].localtime;
            int cycles;
            cycles = cpu[cpunum].cycles_running - cpu[cpunum].PendingCycles;
            result = Attotime.attotime_add(result, new Atime(cycles / cpu[cpunum].cycles_per_second, cycles * cpu[cpunum].attoseconds_per_cycle));
            return result;
        }
        public static void cpunum_suspend_until_trigger(int cpunum, int trigger, int eatcycles)
        {
            cpunum_suspend(cpunum, (byte)SUSPEND_REASON_TRIGGER, (byte)eatcycles);
            cpu[cpunum].trigger = trigger;
        }
        public static void cpu_spin()
        {
            int cpunum = activecpu;//cpu_getexecutingcpu();
            cpunum_suspend_until_trigger(cpunum, -1000, 1);
        }
        public static void cpu_trigger(int trigger)
        {
            int cpunum;
            if (activecpu >= 0)
            {
                activecpu_abort_timeslice(activecpu);
            }
            for (cpunum = 0; cpunum < ncpu; cpunum++)
            {
                if (cpu[cpunum].suspend != 0 && cpu[cpunum].trigger == trigger)
                {
                    cpunum_resume(cpunum, SUSPEND_REASON_TRIGGER);
                    cpu[cpunum].trigger = 0;
                }
            }
        }
        public static void cpu_triggerint(int cpunum)
        {
            cpu_trigger(-2000 + cpunum);
        }
        public static void on_vblank()
        {
            switch (Machine.sBoard)
            {
                case "CPS-1":
                case "CPS-1(QSound)":
                    CPS.cps1_interrupt();
                    break;
                case "CPS2":
                    iloops = 0;
                    CPS.cps2_interrupt();
                    EmuTimer.timer_adjust_periodic(Cpuexec.cpu[0].partial_frame_timer, Cpuexec.cpu[0].partial_frame_period, Attotime.ATTOTIME_NEVER);
                    break;
                case "Data East":
                    Generic.nmi_line_pulse0();
                    break;
                case "Tehkan":
                    iloops = 0;
                    vblank_interrupt();
                    Tehkan.pbaction_interrupt();
                    EmuTimer.timer_adjust_periodic(Cpuexec.cpu[1].partial_frame_timer, Cpuexec.cpu[1].partial_frame_period, Attotime.ATTOTIME_NEVER);
                    break;
                case "Neo Geo":
                    break;
                case "SunA8":
                    iloops = 0;
                    iloops2 = 0;
                    Generic.irq_1_0_line_hold();
                    EmuTimer.timer_adjust_periodic(Cpuexec.cpu[0].partial_frame_timer, Cpuexec.cpu[0].partial_frame_period, Attotime.ATTOTIME_NEVER);
                    EmuTimer.timer_adjust_periodic(Cpuexec.cpu[1].partial_frame_timer, Cpuexec.cpu[1].partial_frame_period, Attotime.ATTOTIME_NEVER);
                    break;
                case "Namco System 1":
                    for (int cpunum = 0; cpunum < ncpu; cpunum++)
                    {
                        //if (!cpunum_is_suspended(cpunum, (byte)(SUSPEND_REASON_HALT | SUSPEND_REASON_RESET | SUSPEND_REASON_DISABLE)))
                        {
                            Cpuint.cpunum_set_input_line(cpunum, 0, LineState.ASSERT_LINE);
                        }
                    }
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
                            iloops = 0;
                            IGS011.lhb2_interrupt();
                            EmuTimer.timer_adjust_periodic(Cpuexec.cpu[0].partial_frame_timer, Cpuexec.cpu[0].partial_frame_period, Attotime.ATTOTIME_NEVER);
                            break;
                        case "lhb":
                        case "lhbv33c":
                        case "dbc":
                        case "ryukobou":
                            iloops = 0;
                            IGS011.lhb_interrupt();
                            EmuTimer.timer_adjust_periodic(Cpuexec.cpu[0].partial_frame_timer, Cpuexec.cpu[0].partial_frame_period, Attotime.ATTOTIME_NEVER);
                            break;
                    }
                    break;
                case "PGM":
                    PGM.drgw_interrupt();
                    break;
                case "M72":
                    iloops = 0;
                    vblank_interrupt();
                    EmuTimer.timer_adjust_periodic(Cpuexec.cpu[1].partial_frame_timer, Cpuexec.cpu[1].partial_frame_period, Attotime.ATTOTIME_NEVER);
                    break;
                case "M92":
                    break;
                case "Taito":
                    switch (Machine.sName)
                    {
                        case "tokio":
                        case "tokioo":
                        case "tokiou":
                        case "tokiob":
                        case "boblbobl":
                        case "sboblbobl":
                        case "sboblbobla":
                        case "sboblboblb":
                        case "sboblbobld":
                        case "sboblboblc":
                        case "dland":
                        case "bbredux":
                        case "bublboblb":
                        case "boblcave":
                            if (!cpunum_is_suspended(0, (byte)(SUSPEND_REASON_HALT | SUSPEND_REASON_RESET | SUSPEND_REASON_DISABLE)))
                            {
                                Cpuint.cpunum_set_input_line(0, 0, LineState.HOLD_LINE);
                            }
                            if (!cpunum_is_suspended(1, (byte)(SUSPEND_REASON_HALT | SUSPEND_REASON_RESET | SUSPEND_REASON_DISABLE)))
                            {
                                Cpuint.cpunum_set_input_line(1, 0, LineState.HOLD_LINE);
                            }
                            break;
                        case "bublbobl":
                        case "bublbobl1":
                        case "bublboblr":
                        case "bublboblr1":
                        case "bublcave":
                        case "bublcave11":
                        case "bublcave10":
                            if (!cpunum_is_suspended(1, (byte)(SUSPEND_REASON_HALT | SUSPEND_REASON_RESET | SUSPEND_REASON_DISABLE)))
                            {
                                Cpuint.cpunum_set_input_line(1, 0, LineState.HOLD_LINE);
                            }
                            if (!cpunum_is_suspended(3, (byte)(SUSPEND_REASON_HALT | SUSPEND_REASON_RESET | SUSPEND_REASON_DISABLE)))
                            {
                                Cpuint.cpunum_set_input_line(3, 0, LineState.PULSE_LINE);
                            }
                            break;
                        case "bub68705":
                            if (!cpunum_is_suspended(1, (byte)(SUSPEND_REASON_HALT | SUSPEND_REASON_RESET | SUSPEND_REASON_DISABLE)))
                            {
                                Cpuint.cpunum_set_input_line(1, 0, LineState.HOLD_LINE);
                            }
                            iloops = 0;
                            vblank_interrupt();
                            EmuTimer.timer_adjust_periodic(Cpuexec.cpu[3].partial_frame_timer, Cpuexec.cpu[3].partial_frame_period, Attotime.ATTOTIME_NEVER);
                            break;
                        case "opwolf":
                        case "opwolfa":
                        case "opwolfj":
                        case "opwolfu":
                        case "opwolfp":
                            if (!cpunum_is_suspended(0, (byte)(SUSPEND_REASON_HALT | SUSPEND_REASON_RESET | SUSPEND_REASON_DISABLE)))
                            {
                                Generic.irq5_line_hold(0);
                            }
                            Crosshair.animate_opwolf();
                            break;
                        case "opwolfb":
                            if (!cpunum_is_suspended(0, (byte)(SUSPEND_REASON_HALT | SUSPEND_REASON_RESET | SUSPEND_REASON_DISABLE)))
                            {
                                Generic.irq5_line_hold(0);
                            }
                            if (!cpunum_is_suspended(2, (byte)(SUSPEND_REASON_HALT | SUSPEND_REASON_RESET | SUSPEND_REASON_DISABLE)))
                            {
                                Generic.irq0_line_hold(2);
                            }
                            Crosshair.animate_opwolf();
                            break;
                        default:
                            break;
                    }
                    break;
                case "Taito B":
                    vblank_interrupt();
                    break;
                case "Konami 68000":
                    switch (Machine.sName)
                    {
                        case "cuebrick":
                            iloops = 0;
                            vblank_interrupt();
                            EmuTimer.timer_adjust_periodic(Cpuexec.cpu[0].partial_frame_timer, Cpuexec.cpu[0].partial_frame_period, Attotime.ATTOTIME_NEVER);
                            break;
                        default:
                            vblank_interrupt();
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
                            if (!cpunum_is_suspended(0, (byte)(SUSPEND_REASON_HALT | SUSPEND_REASON_RESET | SUSPEND_REASON_DISABLE)))
                            {
                                Generic.irq_0_0_line_hold();
                            }
                            iloops = 0;
                            vblank_interrupt();
                            EmuTimer.timer_adjust_periodic(Cpuexec.cpu[1].partial_frame_timer, Cpuexec.cpu[1].partial_frame_period, Attotime.ATTOTIME_NEVER);
                            break;
                        case "sf":
                        case "sfua":
                        case "sfj":
                        case "sfjan":
                        case "sfan":
                            Generic.irq_0_1_line_hold();
                            break;
                        case "sfp":
                            Generic.irq_0_6_line_hold();
                            break;
                    }
                    break;
            }
        }
        public static void trigger_partial_frame_interrupt()
        {
            switch (Machine.sBoard)
            {
                case "CPS2":
                case "IGS011":
                case "Konami 68000":
                    if (iloops == 0)
                    {
                        iloops = vblank_interrupts_per_frame;
                    }
                    iloops--;
                    vblank_interrupt();
                    if (iloops > 1)
                    {
                        EmuTimer.timer_adjust_periodic(Cpuexec.cpu[0].partial_frame_timer, Cpuexec.cpu[0].partial_frame_period, Attotime.ATTOTIME_NEVER);
                    }
                    break;
                case "Tehkan":
                    if (iloops == 0)
                    {
                        iloops = vblank_interrupts_per_frame;
                    }
                    iloops--;
                    Tehkan.pbaction_interrupt();
                    if (iloops > 1)
                    {
                        EmuTimer.timer_adjust_periodic(Cpuexec.cpu[1].partial_frame_timer, Cpuexec.cpu[1].partial_frame_period, Attotime.ATTOTIME_NEVER);
                    }
                    break;
                case "SunA8":
                    if (iloops == 0)
                    {
                        iloops = vblank_interrupts_per_frame;
                    }
                    iloops--;
                    SunA8.hardhea2_interrupt();
                    if (iloops > 1)
                    {
                        EmuTimer.timer_adjust_periodic(Cpuexec.cpu[0].partial_frame_timer, Cpuexec.cpu[0].partial_frame_period, Attotime.ATTOTIME_NEVER);
                    }
                    break;
                case "M72":
                    if (iloops == 0)
                    {
                        iloops = vblank_interrupts_per_frame;
                    }
                    iloops--;
                    vblank_interrupt();
                    if (iloops > 1)
                    {
                        EmuTimer.timer_adjust_periodic(Cpuexec.cpu[1].partial_frame_timer, Cpuexec.cpu[1].partial_frame_period, Attotime.ATTOTIME_NEVER);
                    }
                    break;
                case "Taito":
                    switch (Machine.sName)
                    {
                        case "bub68705":
                            if (iloops == 0)
                            {
                                iloops = vblank_interrupts_per_frame;
                            }
                            iloops--;
                            vblank_interrupt();
                            if (iloops > 1)
                            {
                                EmuTimer.timer_adjust_periodic(Cpuexec.cpu[3].partial_frame_timer, Cpuexec.cpu[3].partial_frame_period, Attotime.ATTOTIME_NEVER);
                            }
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
                            if (iloops == 0)
                            {
                                iloops = vblank_interrupts_per_frame;
                            }
                            iloops--;
                            vblank_interrupt();
                            if (iloops > 1)
                            {
                                EmuTimer.timer_adjust_periodic(Cpuexec.cpu[1].partial_frame_timer, Cpuexec.cpu[1].partial_frame_period, Attotime.ATTOTIME_NEVER);
                            }
                            break;
                    }
                    break;
            }
        }
        public static void cpu_timeslicecallback()
        {
            cpu_trigger(-1000);
        }
        public static void end_interleave_boost()
        {
            EmuTimer.timer_adjust_periodic(interleave_boost_timer, Attotime.ATTOTIME_NEVER, Attotime.ATTOTIME_NEVER);
        }
        public static void compute_perfect_interleave()
        {
            long smallest = cpu[0].attoseconds_per_cycle;
            int cpunum;
            perfect_interleave = Attotime.ATTOTIME_ZERO;
            perfect_interleave.attoseconds = Attotime.ATTOSECONDS_PER_SECOND - 1;
            for (cpunum = 1; cpunum < ncpu; cpunum++)
            {
                if (cpu[cpunum].attoseconds_per_cycle < smallest)
                {
                    perfect_interleave.attoseconds = smallest;
                    smallest = cpu[cpunum].attoseconds_per_cycle;
                }
                else if (cpu[cpunum].attoseconds_per_cycle < perfect_interleave.attoseconds)
                    perfect_interleave.attoseconds = cpu[cpunum].attoseconds_per_cycle;
            }
            if (perfect_interleave.attoseconds == Attotime.ATTOSECONDS_PER_SECOND - 1)
                perfect_interleave.attoseconds = cpu[0].attoseconds_per_cycle;
        }
        public static void SaveStateBinary(System.IO.BinaryWriter writer)
        {
            int i;
            for (i = 0; i < ncpu; i++)
            {
                writer.Write(Cpuexec.cpu[i].suspend);
                writer.Write(Cpuexec.cpu[i].nextsuspend);
                writer.Write(Cpuexec.cpu[i].eatcycles);
                writer.Write(Cpuexec.cpu[i].nexteatcycles);
                writer.Write(Cpuexec.cpu[i].trigger);
                writer.Write(Cpuexec.cpu[i].localtime.seconds);
                writer.Write(Cpuexec.cpu[i].localtime.attoseconds);
            }
        }
        public static void LoadStateBinary(System.IO.BinaryReader reader)
        {
            int i;
            for (i = 0; i < ncpu; i++)
            {
                Cpuexec.cpu[i].suspend = reader.ReadByte();
                Cpuexec.cpu[i].nextsuspend = reader.ReadByte();
                Cpuexec.cpu[i].eatcycles = reader.ReadByte();
                Cpuexec.cpu[i].nexteatcycles = reader.ReadByte();
                Cpuexec.cpu[i].trigger = reader.ReadInt32();
                Cpuexec.cpu[i].localtime.seconds = reader.ReadInt32();
                Cpuexec.cpu[i].localtime.attoseconds = reader.ReadInt64();
            }
        }
    }
}
