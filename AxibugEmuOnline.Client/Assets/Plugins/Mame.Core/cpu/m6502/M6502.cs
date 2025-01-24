using MAME.Core;
using System;
using System.IO;
//using System.IO;

namespace cpu.m6502
{
    public partial class M6502 : cpuexec_data
    {
        public static M6502[] mm1;
        public Action[] insn, insn6502;
        public byte subtype;
        public Register ppc, pc, sp, zp, ea;
        public byte p, a, x, y, pending_irq, after_cli, nmi_state, irq_state, so_state;
        public delegate int irq_delegate(int i);
        public irq_delegate irq_callback;
        public delegate byte read8handler(ushort offset);
        public delegate void write8handler(ushort offset, byte value);
        public read8handler rdmem_id;
        public write8handler wrmem_id;
        private ushort M6502_NMI_VEC = 0xfffa, M6502_RST_VEC = 0xfffc, M6502_IRQ_VEC = 0xfffe;
        private int M6502_SET_OVERFLOW = 1;
        private int m6502_IntOccured;
        protected ulong totalExecutedCycles;
        protected int pendingCycles;
        public override ulong TotalExecutedCycles
        {
            get
            {
                return totalExecutedCycles;
            }
            set
            {
                totalExecutedCycles = value;
            }
        }
        public override int PendingCycles
        {
            get
            {
                return pendingCycles;
            }
            set
            {
                pendingCycles = value;
            }
        }
        public Func<ushort, byte> ReadOp, ReadOpArg;
        public Func<ushort, byte> ReadMemory;
        public Action<ushort, byte> WriteMemory;
        public byte default_rdmem_id(ushort offset)
        {
            return ReadMemory(offset);
        }
        private void default_wdmem_id(ushort offset, byte data)
        {
            WriteMemory(offset, data);
        }
        public M6502()
        {
            insn6502 = new Action[]{
                m6502_00,m6502_01,m6502_02,m6502_03,m6502_04,m6502_05,m6502_06,m6502_07,
                m6502_08,m6502_09,m6502_0a,m6502_0b,m6502_0c,m6502_0d,m6502_0e,m6502_0f,
                m6502_10,m6502_11,m6502_12,m6502_13,m6502_14,m6502_15,m6502_16,m6502_17,
                m6502_18,m6502_19,m6502_1a,m6502_1b,m6502_1c,m6502_1d,m6502_1e,m6502_1f,
                m6502_20,m6502_21,m6502_22,m6502_23,m6502_24,m6502_25,m6502_26,m6502_27,
                m6502_28,m6502_29,m6502_2a,m6502_2b,m6502_2c,m6502_2d,m6502_2e,m6502_2f,
                m6502_30,m6502_31,m6502_32,m6502_33,m6502_34,m6502_35,m6502_36,m6502_37,
                m6502_38,m6502_39,m6502_3a,m6502_3b,m6502_3c,m6502_3d,m6502_3e,m6502_3f,
                m6502_40,m6502_41,m6502_42,m6502_43,m6502_44,m6502_45,m6502_46,m6502_47,
                m6502_48,m6502_49,m6502_4a,m6502_4b,m6502_4c,m6502_4d,m6502_4e,m6502_4f,
                m6502_50,m6502_51,m6502_52,m6502_53,m6502_54,m6502_55,m6502_56,m6502_57,
                m6502_58,m6502_59,m6502_5a,m6502_5b,m6502_5c,m6502_5d,m6502_5e,m6502_5f,
                m6502_60,m6502_61,m6502_62,m6502_63,m6502_64,m6502_65,m6502_66,m6502_67,
                m6502_68,m6502_69,m6502_6a,m6502_6b,m6502_6c,m6502_6d,m6502_6e,m6502_6f,
                m6502_70,m6502_71,m6502_72,m6502_73,m6502_74,m6502_75,m6502_76,m6502_77,
                m6502_78,m6502_79,m6502_7a,m6502_7b,m6502_7c,m6502_7d,m6502_7e,m6502_7f,
                m6502_80,m6502_81,m6502_82,m6502_83,m6502_84,m6502_85,m6502_86,m6502_87,
                m6502_88,m6502_89,m6502_8a,m6502_8b,m6502_8c,m6502_8d,m6502_8e,m6502_8f,
                m6502_90,m6502_91,m6502_92,m6502_93,m6502_94,m6502_95,m6502_96,m6502_97,
                m6502_98,m6502_99,m6502_9a,m6502_9b,m6502_9c,m6502_9d,m6502_9e,m6502_9f,
                m6502_a0,m6502_a1,m6502_a2,m6502_a3,m6502_a4,m6502_a5,m6502_a6,m6502_a7,
                m6502_a8,m6502_a9,m6502_aa,m6502_ab,m6502_ac,m6502_ad,m6502_ae,m6502_af,
                m6502_b0,m6502_b1,m6502_b2,m6502_b3,m6502_b4,m6502_b5,m6502_b6,m6502_b7,
                m6502_b8,m6502_b9,m6502_ba,m6502_bb,m6502_bc,m6502_bd,m6502_be,m6502_bf,
                m6502_c0,m6502_c1,m6502_c2,m6502_c3,m6502_c4,m6502_c5,m6502_c6,m6502_c7,
                m6502_c8,m6502_c9,m6502_ca,m6502_cb,m6502_cc,m6502_cd,m6502_ce,m6502_cf,
                m6502_d0,m6502_d1,m6502_d2,m6502_d3,m6502_d4,m6502_d5,m6502_d6,m6502_d7,
                m6502_d8,m6502_d9,m6502_da,m6502_db,m6502_dc,m6502_dd,m6502_de,m6502_df,
                m6502_e0,m6502_e1,m6502_e2,m6502_e3,m6502_e4,m6502_e5,m6502_e6,m6502_e7,
                m6502_e8,m6502_e9,m6502_ea,m6502_eb,m6502_ec,m6502_ed,m6502_ee,m6502_ef,
                m6502_f0,m6502_f1,m6502_f2,m6502_f3,m6502_f4,m6502_f5,m6502_f6,m6502_f7,
                m6502_f8,m6502_f9,m6502_fa,m6502_fb,m6502_fc,m6502_fd,m6502_fe,m6502_ff
            };
            insn = insn6502;
        }
        public void m6502_common_init(irq_delegate irqcallback)
        {
            irq_callback = irqcallback;
            subtype = 0;
            rdmem_id = default_rdmem_id;
            wrmem_id = default_wdmem_id;
        }
        public override void Reset()
        {
            m6502_reset();
        }
        public void m6502_reset()
        {
            pc.LowByte = RDMEM(M6502_RST_VEC);
            pc.HighByte = RDMEM((ushort)(M6502_RST_VEC + 1));
            sp.d = 0x01ff;
            p = (byte)(F_T | F_I | F_Z | F_B | (p & F_D));
            pending_irq = 0;
            after_cli = 0;
            irq_state = 0;
            nmi_state = 0;
        }
        public void m6502_take_irq()
        {
            if ((p & F_I) == 0)
            {
                ea.d = M6502_IRQ_VEC;
                pendingCycles -= 2;
                PUSH(pc.HighByte);
                PUSH(pc.LowByte);
                PUSH((byte)(p & ~F_B));
                p |= F_I;
                pc.LowByte = RDMEM((ushort)ea.d);
                pc.HighByte = RDMEM((ushort)(ea.d + 1));
                if (irq_callback != null)
                {
                    irq_callback(0);
                }
            }
            pending_irq = 0;
        }
        public override int ExecuteCycles(int cycles)
        {
            return m6502_execute(cycles);
        }
        public int m6502_execute(int cycles)
        {
            pendingCycles = cycles;
            do
            {
                byte op;
                ppc.d = pc.d;
                if (pending_irq != 0)
                {
                    m6502_take_irq();
                }
                op = ReadOp(pc.LowWord);
                pc.LowWord++;
                pendingCycles -= 1;
                insn[op]();
                if (after_cli != 0)
                {
                    after_cli = 0;
                    if (irq_state != (byte)LineState.CLEAR_LINE)
                    {
                        pending_irq = 1;
                    }
                }
                else
                {
                    if (pending_irq == 2)
                    {
                        if (m6502_IntOccured - pendingCycles > 1)
                        {
                            pending_irq = 1;
                        }
                    }
                    if (pending_irq == 1)
                    {
                        m6502_take_irq();
                    }
                    if (pending_irq == 2)
                    {
                        pending_irq = 1;
                    }
                }
            }
            while (pendingCycles > 0);
            return cycles - pendingCycles;
        }
        public override void set_irq_line(int irqline, LineState state)
        {
            m6502_set_irq_line(irqline, state);
        }
        public override void cpunum_set_input_line_and_vector(int cpunum, int line, LineState state, int vector)
        {
            EmuTimer.timer_set_internal(EmuTimer.TIME_ACT.Cpuint_cpunum_empty_event_queue);
        }
        private void m6502_set_irq_line(int irqline, LineState state)
        {
            if (irqline == (byte)LineState.INPUT_LINE_NMI)
            {
                if (nmi_state == (byte)state)
                {
                    return;
                }
                nmi_state = (byte)state;
                if (state != LineState.CLEAR_LINE)
                {
                    ea.d = M6502_NMI_VEC;
                    pendingCycles -= 2;
                    PUSH(pc.HighByte);
                    PUSH(pc.LowByte);
                    PUSH((byte)(p & ~F_B));
                    p |= F_I;
                    pc.LowByte = RDMEM((ushort)ea.d);
                    pc.HighByte = RDMEM((ushort)(ea.d + 1));
                }
            }
            else
            {
                if (irqline == M6502_SET_OVERFLOW)
                {
                    if (so_state != 0 && state == 0)
                    {
                        p |= F_V;
                    }
                    so_state = (byte)state;
                    return;
                }
                irq_state = (byte)state;
                if (state != LineState.CLEAR_LINE)
                {
                    pending_irq = 1;
                    m6502_IntOccured = pendingCycles;
                }
            }
        }
        public void SaveStateBinary(BinaryWriter writer)
        {
            writer.Write(subtype);
            writer.Write(ppc.LowWord);
            writer.Write(pc.LowWord);
            writer.Write(sp.LowWord);
            writer.Write(p);
            writer.Write(a);
            writer.Write(x);
            writer.Write(y);
            writer.Write(pending_irq);
            writer.Write(after_cli);
            writer.Write(nmi_state);
            writer.Write(irq_state);
            writer.Write(so_state);
            writer.Write(TotalExecutedCycles);
            writer.Write(PendingCycles);
        }
        public void LoadStateBinary(BinaryReader reader)
        {
            subtype = reader.ReadByte();
            ppc.LowWord = reader.ReadUInt16();
            pc.LowWord = reader.ReadUInt16();
            sp.LowWord = reader.ReadUInt16();
            p = reader.ReadByte();
            a = reader.ReadByte();
            x = reader.ReadByte();
            y = reader.ReadByte();
            pending_irq = reader.ReadByte();
            after_cli = reader.ReadByte();
            nmi_state = reader.ReadByte();
            irq_state = reader.ReadByte();
            so_state = reader.ReadByte();
            TotalExecutedCycles = reader.ReadUInt64();
            PendingCycles = reader.ReadInt32();
        }
    }
}
