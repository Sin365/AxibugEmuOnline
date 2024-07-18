using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Xml;

namespace MyNes.Core
{
    public class NesEmu
    {
        [StructLayout(LayoutKind.Explicit)]
        private struct CPURegister
        {
            [FieldOffset(0)]
            internal byte l;

            [FieldOffset(1)]
            internal byte h;

            [FieldOffset(0)]
            internal ushort v;
        }

        private enum RequestMode
        {
            None,
            HardReset,
            SoftReset,
            LoadState,
            SaveState,
            TakeSnapshot
        }

        private static int[][] dmc_freq_table = new int[3][]
        {
            new int[16]
            {
                428, 380, 340, 320, 286, 254, 226, 214, 190, 160,
                142, 128, 106, 84, 72, 54
            },
            new int[16]
            {
                398, 354, 316, 298, 276, 236, 210, 198, 176, 148,
                132, 118, 98, 78, 66, 50
            },
            new int[16]
            {
                428, 380, 340, 320, 286, 254, 226, 214, 190, 160,
                142, 128, 106, 84, 72, 54
            }
        };

        private static int dmc_output_a;

        private static int dmc_output;

        private static int dmc_period_devider;

        private static bool dmc_irq_enabled;

        private static bool dmc_loop_flag;

        private static byte dmc_rate_index;

        private static ushort dmc_addr_refresh;

        private static int dmc_size_refresh;

        private static bool dmc_dmaEnabled;

        private static byte dmc_dmaByte;

        private static int dmc_dmaBits;

        private static bool dmc_bufferFull;

        private static byte dmc_dmaBuffer;

        private static int dmc_dmaSize;

        private static ushort dmc_dmaAddr;

        private static ushort[][] nos_freq_table = new ushort[3][]
        {
            new ushort[16]
            {
                4, 8, 16, 32, 64, 96, 128, 160, 202, 254,
                380, 508, 762, 1016, 2034, 4068
            },
            new ushort[16]
            {
                4, 7, 14, 30, 60, 88, 118, 148, 188, 236,
                354, 472, 708, 944, 1890, 3778
            },
            new ushort[16]
            {
                4, 8, 16, 32, 64, 96, 128, 160, 202, 254,
                380, 508, 762, 1016, 2034, 4068
            }
        };

        private static bool nos_length_halt;

        private static bool nos_constant_volume_envelope;

        private static byte nos_volume_devider_period;

        private static ushort nos_timer;

        private static bool nos_mode;

        private static int nos_period_devider;

        private static bool nos_length_enabled;

        private static int nos_length_counter;

        private static bool nos_envelope_start_flag;

        private static byte nos_envelope_devider;

        private static byte nos_envelope_decay_level_counter;

        private static byte nos_envelope;

        private static int nos_output;

        private static int nos_shift_reg;

        private static int nos_feedback;

        private static bool nos_ignore_reload;

        private static byte[][] sq_duty_cycle_sequences = new byte[4][]
        {
            new byte[8] { 0, 0, 0, 0, 0, 0, 0, 1 },
            new byte[8] { 0, 0, 0, 0, 0, 0, 1, 1 },
            new byte[8] { 0, 0, 0, 0, 1, 1, 1, 1 },
            new byte[8] { 1, 1, 1, 1, 1, 1, 0, 0 }
        };

        private static byte[] sq_duration_table = new byte[32]
        {
            10, 254, 20, 2, 40, 4, 80, 6, 160, 8,
            60, 10, 14, 12, 26, 14, 12, 16, 24, 18,
            48, 20, 96, 22, 192, 24, 72, 26, 16, 28,
            32, 30
        };

        private static byte sq1_duty_cycle;

        private static bool sq1_length_halt;

        private static bool sq1_constant_volume_envelope;

        private static byte sq1_volume_devider_period;

        private static bool sq1_sweep_enable;

        private static byte sq1_sweep_devider_period;

        private static bool sq1_sweep_negate;

        private static byte sq1_sweep_shift_count;

        private static int sq1_timer;

        private static int sq1_period_devider;

        private static byte sq1_seqencer;

        private static bool sq1_length_enabled;

        private static int sq1_length_counter;

        private static bool sq1_envelope_start_flag;

        private static byte sq1_envelope_devider;

        private static byte sq1_envelope_decay_level_counter;

        private static byte sq1_envelope;

        private static int sq1_sweep_counter;

        private static bool sq1_sweep_reload;

        private static int sq1_sweep_change;

        private static bool sq1_valid_freq;

        private static int sq1_output;

        private static bool sq1_ignore_reload;

        private static byte[] trl_step_seq = new byte[32]
        {
            15, 14, 13, 12, 11, 10, 9, 8, 7, 6,
            5, 4, 3, 2, 1, 0, 0, 1, 2, 3,
            4, 5, 6, 7, 8, 9, 10, 11, 12, 13,
            14, 15
        };

        private static bool trl_liner_control_flag;

        private static byte trl_liner_control_reload;

        private static ushort trl_timer;

        private static bool trl_length_enabled;

        private static byte trl_length_counter;

        private static bool trl_liner_control_reload_flag;

        private static byte trl_liner_counter;

        private static int trl_output;

        private static int trl_period_devider;

        private static int trl_step;

        private static bool trl_ignore_reload;

        private static byte apu_reg_io_db;

        private static byte apu_reg_io_addr;

        private static bool apu_reg_access_happened;

        private static bool apu_reg_access_w;

        private static Action[] apu_reg_update_func;

        private static Action[] apu_reg_read_func;

        private static Action[] apu_reg_write_func;

        private static Action apu_update_playback_func;

        private static bool apu_odd_cycle;

        private static bool apu_irq_enabled;

        private static bool apu_irq_flag;

        private static bool apu_irq_do_it;

        internal static bool apu_irq_delta_occur;

        private static bool apu_seq_mode;

        private static int apu_ferq_f;

        private static int apu_ferq_l;

        private static int apu_ferq_e;

        private static int apu_cycle_f;

        private static int apu_cycle_f_t;

        private static int apu_cycle_e;

        private static int apu_cycle_l;

        private static bool apu_odd_l;

        private static bool apu_check_irq;

        private static bool apu_do_env;

        private static bool apu_do_length;

        public static bool SoundEnabled;

        public static double audio_playback_amplitude = 1.5;

        public static int audio_playback_peek_limit = 124;

        private static bool audio_playback_dac_initialized;

        public static int cpu_speed;

        private static short[] audio_samples;

        private static int audio_w_pos;

        private static int audio_samples_added;

        internal static int audio_samples_count;

        private static int[][][][][] mix_table;

        private static double audio_x;

        private static double audio_y;

        private static double audio_y_av;

        private static double audio_y_timer;

        public static double audio_timer_ratio = 40.0;

        private static double audio_timer;

        private static SoundLowPassFilter audio_low_pass_filter_14K;

        private static SoundHighPassFilter audio_high_pass_filter_90;

        private static SoundHighPassFilter audio_high_pass_filter_440;

        private static SoundDCBlockerFilter audio_dc_blocker_filter;

        private static bool audio_sq1_outputable;

        private static bool audio_sq2_outputable;

        private static bool audio_nos_outputable;

        private static bool audio_trl_outputable;

        private static bool audio_dmc_outputable;

        private static bool audio_signal_outputed;

        private static bool apu_use_external_sound;

        private static CPURegister cpu_reg_pc;

        private static CPURegister cpu_reg_sp;

        private static CPURegister cpu_reg_ea;

        private static byte cpu_reg_a;

        private static byte cpu_reg_x;

        private static byte cpu_reg_y;

        private static bool cpu_flag_n;

        private static bool cpu_flag_v;

        private static bool cpu_flag_d;

        private static bool cpu_flag_i;

        private static bool cpu_flag_z;

        private static bool cpu_flag_c;

        private static byte cpu_m;

        private static byte cpu_opcode;

        private static byte cpu_byte_temp;

        private static int cpu_int_temp;

        private static int cpu_int_temp1;

        private static byte cpu_dummy;

        private static bool cpu_bool_tmp;

        private static CPURegister temp_add;

        private static bool CPU_IRQ_PIN;

        private static bool CPU_NMI_PIN;

        private static bool cpu_suspend_nmi;

        private static bool cpu_suspend_irq;

        private static Action[] cpu_addressings;

        private static Action[] cpu_instructions;

        private static int dma_DMCDMAWaitCycles;

        private static int dma_OAMDMAWaitCycles;

        private static bool dma_isOamDma;

        private static int dma_oamdma_i;

        private static bool dma_DMCOn;

        private static bool dma_OAMOn;

        private static bool dma_DMC_occurring;

        private static bool dma_OAM_occurring;

        private static int dma_OAMFinishCounter;

        private static ushort dma_Oamaddress;

        private static int dma_OAMCYCLE;

        private static byte dma_latch;

        private static byte dma_dummy;

        private static ushort reg_2004;

        internal static int IRQFlags = 0;

        private static bool PPU_NMI_Current;

        private static bool PPU_NMI_Old;

        private const int IRQ_APU = 1;

        internal const int IRQ_DMC = 2;

        internal const int IRQ_BOARD = 8;

        private static ushort InterruptVector;

        private static byte[] mem_wram;

        internal static Board mem_board;

        private static MemReadAccess[] mem_read_accesses;

        private static MemWriteAccess[] mem_write_accesses;

        private static bool BUS_RW;

        private static ushort BUS_ADDRESS;

        private static string SRAMFileName;

        public static string GMFileName;

        private static int PORT0;

        private static int PORT1;

        private static int inputStrobe;

        private static IJoypadConnecter joypad1;

        private static IJoypadConnecter joypad2;

        private static IJoypadConnecter joypad3;

        private static IJoypadConnecter joypad4;

        public static bool IsFourPlayers;

        private static byte[] reverseLookup = new byte[256]
        {
            0, 128, 64, 192, 32, 160, 96, 224, 16, 144,
            80, 208, 48, 176, 112, 240, 8, 136, 72, 200,
            40, 168, 104, 232, 24, 152, 88, 216, 56, 184,
            120, 248, 4, 132, 68, 196, 36, 164, 100, 228,
            20, 148, 84, 212, 52, 180, 116, 244, 12, 140,
            76, 204, 44, 172, 108, 236, 28, 156, 92, 220,
            60, 188, 124, 252, 2, 130, 66, 194, 34, 162,
            98, 226, 18, 146, 82, 210, 50, 178, 114, 242,
            10, 138, 74, 202, 42, 170, 106, 234, 26, 154,
            90, 218, 58, 186, 122, 250, 6, 134, 70, 198,
            38, 166, 102, 230, 22, 150, 86, 214, 54, 182,
            118, 246, 14, 142, 78, 206, 46, 174, 110, 238,
            30, 158, 94, 222, 62, 190, 126, 254, 1, 129,
            65, 193, 33, 161, 97, 225, 17, 145, 81, 209,
            49, 177, 113, 241, 9, 137, 73, 201, 41, 169,
            105, 233, 25, 153, 89, 217, 57, 185, 121, 249,
            5, 133, 69, 197, 37, 165, 101, 229, 21, 149,
            85, 213, 53, 181, 117, 245, 13, 141, 77, 205,
            45, 173, 109, 237, 29, 157, 93, 221, 61, 189,
            125, 253, 3, 131, 67, 195, 35, 163, 99, 227,
            19, 147, 83, 211, 51, 179, 115, 243, 11, 139,
            75, 203, 43, 171, 107, 235, 27, 155, 91, 219,
            59, 187, 123, 251, 7, 135, 71, 199, 39, 167,
            103, 231, 23, 151, 87, 215, 55, 183, 119, 247,
            15, 143, 79, 207, 47, 175, 111, 239, 31, 159,
            95, 223, 63, 191, 127, 255
        };

        private static Action[] ppu_v_clocks;

        private static Action[] ppu_h_clocks;

        private static Action[] ppu_bkg_fetches;

        private static Action[] ppu_spr_fetches;

        private static Action[] ppu_oam_phases;

        private static int[] ppu_bkg_pixels;

        private static int[] ppu_spr_pixels;

        private static int[] ppu_screen_pixels;

        private static int[] ppu_palette;

        private static int ppu_clock_h;

        internal static ushort ppu_clock_v;

        private static ushort ppu_clock_vblank_start;

        private static ushort ppu_clock_vblank_end;

        private static bool ppu_use_odd_cycle;

        private static bool ppu_use_odd_swap;

        private static bool ppu_odd_swap_done;

        private static bool ppu_is_nmi_time;

        private static bool ppu_frame_finished;

        private static byte[] ppu_oam_bank;

        private static byte[] ppu_oam_bank_secondary;

        private static byte[] ppu_palette_bank;

        private static byte ppu_reg_io_db;

        private static byte ppu_reg_io_addr;

        private static bool ppu_reg_access_happened;

        private static bool ppu_reg_access_w;

        private static Action[] ppu_reg_update_func;

        private static Action[] ppu_reg_read_func;

        private static byte ppu_reg_2000_vram_address_increament;

        private static ushort ppu_reg_2000_sprite_pattern_table_address_for_8x8_sprites;

        private static ushort ppu_reg_2000_background_pattern_table_address;

        internal static byte ppu_reg_2000_Sprite_size;

        private static bool ppu_reg_2000_VBI;

        private static bool ppu_reg_2001_show_background_in_leftmost_8_pixels_of_screen;

        private static bool ppu_reg_2001_show_sprites_in_leftmost_8_pixels_of_screen;

        private static bool ppu_reg_2001_show_background;

        private static bool ppu_reg_2001_show_sprites;

        private static int ppu_reg_2001_grayscale;

        private static int ppu_reg_2001_emphasis;

        private static bool ppu_reg_2002_SpriteOverflow;

        private static bool ppu_reg_2002_Sprite0Hit;

        private static bool ppu_reg_2002_VblankStartedFlag;

        private static byte ppu_reg_2003_oam_addr;

        private static ushort ppu_vram_addr;

        private static byte ppu_vram_data;

        private static ushort ppu_vram_addr_temp;

        private static ushort ppu_vram_addr_access_temp;

        private static bool ppu_vram_flip_flop;

        private static byte ppu_vram_finex;

        private static ushort ppu_bkgfetch_nt_addr;

        private static byte ppu_bkgfetch_nt_data;

        private static ushort ppu_bkgfetch_at_addr;

        private static byte ppu_bkgfetch_at_data;

        private static ushort ppu_bkgfetch_lb_addr;

        private static byte ppu_bkgfetch_lb_data;

        private static ushort ppu_bkgfetch_hb_addr;

        private static byte ppu_bkgfetch_hb_data;

        private static int ppu_sprfetch_slot;

        private static byte ppu_sprfetch_y_data;

        private static byte ppu_sprfetch_t_data;

        private static byte ppu_sprfetch_at_data;

        private static byte ppu_sprfetch_x_data;

        private static ushort ppu_sprfetch_lb_addr;

        private static byte ppu_sprfetch_lb_data;

        private static ushort ppu_sprfetch_hb_addr;

        private static byte ppu_sprfetch_hb_data;

        internal static bool ppu_is_sprfetch;

        private static int ppu_bkg_render_i;

        private static int ppu_bkg_render_pos;

        private static int ppu_bkg_render_tmp_val;

        private static int ppu_bkg_current_pixel;

        private static int ppu_spr_current_pixel;

        private static int ppu_current_pixel;

        private static int ppu_render_x;

        private static int ppu_render_y;

        private static byte ppu_oamev_n;

        private static byte ppu_oamev_m;

        private static bool ppu_oamev_compare;

        private static byte ppu_oamev_slot;

        private static byte ppu_fetch_data;

        private static byte ppu_phase_index;

        private static bool ppu_sprite0_should_hit;

        private static int ppu_temp_comparator;

        public static bool ON;

        public static bool PAUSED;

        public static bool isPaused;

        public static string CurrentFilePath;

        public static bool FrameLimiterEnabled;

        private static Thread mainThread;

        private static double fps_time_period;

        private static double emu_time_target_fps = 60.0;

        private static bool render_initialized;

        private static RenderVideoFrame render_video;

        private static RenderAudioSamples render_audio;

        private static TogglePause render_audio_toggle_pause;

        private static GetIsPlaying render_audio_get_is_playing;

        private static bool render_audio_is_playing;

        public static EmuRegion Region;

        private static int SystemIndex;

        private static RequestMode emu_request_mode = RequestMode.None;

        public static bool FrameSkipEnabled;

        public static int FrameSkipInterval;

        private static int FrameSkipCounter;

        private static byte sq2_duty_cycle;

        private static bool sq2_length_halt;

        private static bool sq2_constant_volume_envelope;

        private static byte sq2_volume_devider_period;

        private static bool sq2_sweep_enable;

        private static byte sq2_sweep_devider_period;

        private static bool sq2_sweep_negate;

        private static byte sq2_sweep_shift_count;

        private static int sq2_timer;

        private static int sq2_period_devider;

        private static byte sq2_seqencer;

        private static bool sq2_length_enabled;

        private static int sq2_length_counter;

        private static bool sq2_envelope_start_flag;

        private static byte sq2_envelope_devider;

        private static byte sq2_envelope_decay_level_counter;

        private static byte sq2_envelope;

        private static int sq2_sweep_counter;

        private static bool sq2_sweep_reload;

        private static int sq2_sweep_change;

        private static bool sq2_valid_freq;

        private static int sq2_output;

        private static bool sq2_ignore_reload;

        private static byte register_p
        {
            get
            {
                return (byte)((cpu_flag_n ? 128u : 0u) | (cpu_flag_v ? 64u : 0u) | (cpu_flag_d ? 8u : 0u) | (cpu_flag_i ? 4u : 0u) | (cpu_flag_z ? 2u : 0u) | (cpu_flag_c ? 1u : 0u) | 0x20u);
            }
            set
            {
                cpu_flag_n = (value & 0x80) != 0;
                cpu_flag_v = (value & 0x40) != 0;
                cpu_flag_d = (value & 8) != 0;
                cpu_flag_i = (value & 4) != 0;
                cpu_flag_z = (value & 2) != 0;
                cpu_flag_c = (value & 1) != 0;
            }
        }

        public static GameGenieCode[] GameGenieCodes
        {
            get
            {
                if (mem_board != null)
                {
                    return mem_board.GameGenieCodes;
                }
                return null;
            }
        }

        public static bool IsGameGenieActive
        {
            get
            {
                if (mem_board != null)
                {
                    return mem_board.IsGameGenieActive;
                }
                return false;
            }
            set
            {
                if (mem_board != null)
                {
                    mem_board.IsGameGenieActive = value;
                }
            }
        }

        public static bool IsGameFoundOnDB
        {
            get
            {
                if (mem_board != null)
                {
                    return mem_board.IsGameFoundOnDB;
                }
                return false;
            }
        }

        public static NesCartDatabaseGameInfo GameInfo
        {
            get
            {
                if (mem_board != null)
                {
                    return mem_board.GameInfo;
                }
                return NesCartDatabaseGameInfo.Empty;
            }
        }

        public static NesCartDatabaseCartridgeInfo GameCartInfo
        {
            get
            {
                if (mem_board != null)
                {
                    return mem_board.GameCartInfo;
                }
                return new NesCartDatabaseCartridgeInfo();
            }
        }

        public static string SHA1 => mem_board.SHA1;

        public static event EventHandler EmuShutdown;

        private static void DMCHardReset()
        {
            dmc_output_a = 0;
            dmc_output = 0;
            dmc_period_devider = 0;
            dmc_loop_flag = false;
            dmc_rate_index = 0;
            dmc_irq_enabled = false;
            dmc_dmaAddr = 49152;
            dmc_addr_refresh = 49152;
            dmc_size_refresh = 0;
            dmc_dmaBits = 1;
            dmc_dmaByte = 1;
            dmc_period_devider = 0;
            dmc_dmaEnabled = false;
            dmc_bufferFull = false;
            dmc_dmaSize = 0;
        }

        private static void DMCSoftReset()
        {
            DMCHardReset();
        }

        private static void DMCClock()
        {
            dmc_period_devider--;
            if (dmc_period_devider > 0)
            {
                return;
            }
            dmc_period_devider = dmc_freq_table[SystemIndex][dmc_rate_index];
            if (dmc_dmaEnabled)
            {
                if (((uint)dmc_dmaByte & (true ? 1u : 0u)) != 0)
                {
                    if (dmc_output_a <= 125)
                    {
                        dmc_output_a += 2;
                    }
                }
                else if (dmc_output_a >= 2)
                {
                    dmc_output_a -= 2;
                }
                dmc_dmaByte >>= 1;
            }
            dmc_dmaBits--;
            if (dmc_dmaBits == 0)
            {
                dmc_dmaBits = 8;
                if (dmc_bufferFull)
                {
                    dmc_bufferFull = false;
                    dmc_dmaEnabled = true;
                    dmc_dmaByte = dmc_dmaBuffer;
                    if (dmc_dmaSize > 0)
                    {
                        AssertDMCDMA();
                    }
                }
                else
                {
                    dmc_dmaEnabled = false;
                }
            }
            if (audio_dmc_outputable)
            {
                dmc_output = dmc_output_a;
            }
            audio_signal_outputed = true;
        }

        private static void DMCDoDMA()
        {
            dmc_bufferFull = true;
            Read(ref dmc_dmaAddr, out dmc_dmaBuffer);
            if (dmc_dmaAddr == ushort.MaxValue)
            {
                dmc_dmaAddr = 32768;
            }
            else
            {
                dmc_dmaAddr++;
            }
            if (dmc_dmaSize > 0)
            {
                dmc_dmaSize--;
            }
            if (dmc_dmaSize == 0)
            {
                if (dmc_loop_flag)
                {
                    dmc_dmaSize = dmc_size_refresh;
                    dmc_dmaAddr = dmc_addr_refresh;
                }
                else if (dmc_irq_enabled)
                {
                    IRQFlags |= 2;
                    apu_irq_delta_occur = true;
                }
            }
        }

        private static void APUOnRegister4010()
        {
            if (apu_reg_access_w)
            {
                dmc_irq_enabled = (apu_reg_io_db & 0x80) != 0;
                dmc_loop_flag = (apu_reg_io_db & 0x40) != 0;
                if (!dmc_irq_enabled)
                {
                    apu_irq_delta_occur = false;
                    IRQFlags &= -3;
                }
                dmc_rate_index = (byte)(apu_reg_io_db & 0xFu);
            }
        }

        private static void APUOnRegister4011()
        {
            if (apu_reg_access_w)
            {
                dmc_output_a = (byte)(apu_reg_io_db & 0x7F);
            }
        }

        private static void APUOnRegister4012()
        {
            if (apu_reg_access_w)
            {
                dmc_addr_refresh = (ushort)((uint)(apu_reg_io_db << 6) | 0xC000u);
            }
        }

        private static void APUOnRegister4013()
        {
            if (apu_reg_access_w)
            {
                dmc_size_refresh = (apu_reg_io_db << 4) | 1;
            }
        }

        private static void DMCOn4015()
        {
            apu_irq_delta_occur = false;
            IRQFlags &= -3;
        }

        private static void DMCRead4015()
        {
            if (dmc_dmaSize > 0)
            {
                apu_reg_io_db = (byte)((apu_reg_io_db & 0xEFu) | 0x10u);
            }
        }

        private static void DMCWriteState(ref BinaryWriter bin)
        {
            bin.Write(dmc_output_a);
            bin.Write(dmc_output);
            bin.Write(dmc_period_devider);
            bin.Write(dmc_irq_enabled);
            bin.Write(dmc_loop_flag);
            bin.Write(dmc_rate_index);
            bin.Write(dmc_addr_refresh);
            bin.Write(dmc_size_refresh);
            bin.Write(dmc_dmaEnabled);
            bin.Write(dmc_dmaByte);
            bin.Write(dmc_dmaBits);
            bin.Write(dmc_bufferFull);
            bin.Write(dmc_dmaBuffer);
            bin.Write(dmc_dmaSize);
            bin.Write(dmc_dmaAddr);
        }

        private static void DMCReadState(ref BinaryReader bin)
        {
            dmc_output_a = bin.ReadInt32();
            dmc_output = bin.ReadInt32();
            dmc_period_devider = bin.ReadInt32();
            dmc_irq_enabled = bin.ReadBoolean();
            dmc_loop_flag = bin.ReadBoolean();
            dmc_rate_index = bin.ReadByte();
            dmc_addr_refresh = bin.ReadUInt16();
            dmc_size_refresh = bin.ReadInt32();
            dmc_dmaEnabled = bin.ReadBoolean();
            dmc_dmaByte = bin.ReadByte();
            dmc_dmaBits = bin.ReadInt32();
            dmc_bufferFull = bin.ReadBoolean();
            dmc_dmaBuffer = bin.ReadByte();
            dmc_dmaSize = bin.ReadInt32();
            dmc_dmaAddr = bin.ReadUInt16();
        }

        private static void NOSHardReset()
        {
            nos_length_halt = false;
            nos_constant_volume_envelope = false;
            nos_volume_devider_period = 0;
            nos_shift_reg = 1;
            nos_timer = 0;
            nos_mode = false;
            nos_period_devider = 0;
            nos_length_enabled = false;
            nos_length_counter = 0;
            nos_envelope_start_flag = false;
            nos_envelope_devider = 0;
            nos_envelope_decay_level_counter = 0;
            nos_envelope = 0;
            nos_output = 0;
            nos_feedback = 0;
            nos_ignore_reload = false;
        }

        private static void NOSSoftReset()
        {
            NOSHardReset();
        }

        private static void NOSClock()
        {
            nos_period_devider--;
            if (nos_period_devider > 0)
            {
                return;
            }
            nos_period_devider = nos_timer;
            if (nos_mode)
            {
                nos_feedback = ((nos_shift_reg >> 6) & 1) ^ (nos_shift_reg & 1);
            }
            else
            {
                nos_feedback = ((nos_shift_reg >> 1) & 1) ^ (nos_shift_reg & 1);
            }
            nos_shift_reg >>= 1;
            nos_shift_reg = (nos_shift_reg & 0x3FFF) | ((nos_feedback & 1) << 14);
            if (nos_length_counter > 0 && (nos_shift_reg & 1) == 0)
            {
                if (audio_nos_outputable)
                {
                    nos_output = nos_envelope;
                }
            }
            else
            {
                nos_output = 0;
            }
            audio_signal_outputed = true;
        }

        private static void NOSClockLength()
        {
            if (nos_length_counter > 0 && !nos_length_halt)
            {
                nos_length_counter--;
                if (apu_reg_access_happened && apu_reg_io_addr == 15 && apu_reg_access_w)
                {
                    nos_ignore_reload = true;
                }
            }
        }

        private static void NOSClockEnvelope()
        {
            if (nos_envelope_start_flag)
            {
                nos_envelope_start_flag = false;
                nos_envelope_decay_level_counter = 15;
                nos_envelope_devider = (byte)(nos_volume_devider_period + 1);
            }
            else if (nos_envelope_devider > 0)
            {
                nos_envelope_devider--;
            }
            else
            {
                nos_envelope_devider = (byte)(nos_volume_devider_period + 1);
                if (nos_envelope_decay_level_counter > 0)
                {
                    nos_envelope_decay_level_counter--;
                }
                else if (nos_length_halt)
                {
                    nos_envelope_decay_level_counter = 15;
                }
            }
            nos_envelope = (nos_constant_volume_envelope ? nos_volume_devider_period : nos_envelope_decay_level_counter);
        }

        private static void APUOnRegister400C()
        {
            if (apu_reg_access_w)
            {
                nos_volume_devider_period = (byte)(apu_reg_io_db & 0xFu);
                nos_length_halt = (apu_reg_io_db & 0x20) != 0;
                nos_constant_volume_envelope = (apu_reg_io_db & 0x10) != 0;
                nos_envelope = (nos_constant_volume_envelope ? nos_volume_devider_period : nos_envelope_decay_level_counter);
            }
        }

        private static void APUOnRegister400D()
        {
        }

        private static void APUOnRegister400E()
        {
            if (apu_reg_access_w)
            {
                nos_timer = (ushort)(nos_freq_table[SystemIndex][apu_reg_io_db & 0xF] / 2);
                nos_mode = (apu_reg_io_db & 0x80) == 128;
            }
        }

        private static void APUOnRegister400F()
        {
            if (apu_reg_access_w)
            {
                if (nos_length_enabled && !nos_ignore_reload)
                {
                    nos_length_counter = sq_duration_table[apu_reg_io_db >> 3];
                }
                if (nos_ignore_reload)
                {
                    nos_ignore_reload = false;
                }
                nos_envelope_start_flag = true;
            }
        }

        private static void NOSOn4015()
        {
            nos_length_enabled = (apu_reg_io_db & 8) != 0;
            if (!nos_length_enabled)
            {
                nos_length_counter = 0;
            }
        }

        private static void NOSRead4015()
        {
            if (nos_length_counter > 0)
            {
                apu_reg_io_db = (byte)((apu_reg_io_db & 0xF7u) | 8u);
            }
        }

        private static void NOSWriteState(ref BinaryWriter bin)
        {
            bin.Write(nos_length_halt);
            bin.Write(nos_constant_volume_envelope);
            bin.Write(nos_volume_devider_period);
            bin.Write(nos_timer);
            bin.Write(nos_mode);
            bin.Write(nos_period_devider);
            bin.Write(nos_length_enabled);
            bin.Write(nos_length_counter);
            bin.Write(nos_envelope_start_flag);
            bin.Write(nos_envelope_devider);
            bin.Write(nos_envelope_decay_level_counter);
            bin.Write(nos_envelope);
            bin.Write(nos_output);
            bin.Write(nos_shift_reg);
            bin.Write(nos_feedback);
            bin.Write(nos_ignore_reload);
        }

        private static void NOSReadState(ref BinaryReader bin)
        {
            nos_length_halt = bin.ReadBoolean();
            nos_constant_volume_envelope = bin.ReadBoolean();
            nos_volume_devider_period = bin.ReadByte();
            nos_timer = bin.ReadUInt16();
            nos_mode = bin.ReadBoolean();
            nos_period_devider = bin.ReadInt32();
            nos_length_enabled = bin.ReadBoolean();
            nos_length_counter = bin.ReadInt32();
            nos_envelope_start_flag = bin.ReadBoolean();
            nos_envelope_devider = bin.ReadByte();
            nos_envelope_decay_level_counter = bin.ReadByte();
            nos_envelope = bin.ReadByte();
            nos_output = bin.ReadInt32();
            nos_shift_reg = bin.ReadInt32();
            nos_feedback = bin.ReadInt32();
            nos_ignore_reload = bin.ReadBoolean();
        }

        private static void SQ1HardReset()
        {
            sq1_duty_cycle = 0;
            sq1_length_halt = false;
            sq1_constant_volume_envelope = false;
            sq1_volume_devider_period = 0;
            sq1_sweep_enable = false;
            sq1_sweep_devider_period = 0;
            sq1_sweep_negate = false;
            sq1_sweep_shift_count = 0;
            sq1_timer = 0;
            sq1_period_devider = 0;
            sq1_seqencer = 0;
            sq1_length_enabled = false;
            sq1_length_counter = 0;
            sq1_envelope_start_flag = false;
            sq1_envelope_devider = 0;
            sq1_envelope_decay_level_counter = 0;
            sq1_envelope = 0;
            sq1_sweep_counter = 0;
            sq1_sweep_reload = false;
            sq1_sweep_change = 0;
            sq1_valid_freq = false;
            sq1_output = 0;
            sq1_ignore_reload = false;
        }

        private static void SQ1SoftReset()
        {
            SQ1HardReset();
        }

        private static void SQ1Clock()
        {
            sq1_period_devider--;
            if (sq1_period_devider > 0)
            {
                return;
            }
            sq1_period_devider = sq1_timer + 1;
            sq1_seqencer = (byte)((uint)(sq1_seqencer + 1) & 7u);
            if (sq1_length_counter > 0 && sq1_valid_freq)
            {
                if (audio_sq1_outputable)
                {
                    sq1_output = sq_duty_cycle_sequences[sq1_duty_cycle][sq1_seqencer] * sq1_envelope;
                }
            }
            else
            {
                sq1_output = 0;
            }
            audio_signal_outputed = true;
        }

        private static void SQ1ClockLength()
        {
            if (sq1_length_counter > 0 && !sq1_length_halt)
            {
                sq1_length_counter--;
                if (apu_reg_access_happened && apu_reg_io_addr == 3 && apu_reg_access_w)
                {
                    sq1_ignore_reload = true;
                }
            }
            sq1_sweep_counter--;
            if (sq1_sweep_counter == 0)
            {
                sq1_sweep_counter = sq1_sweep_devider_period + 1;
                if (sq1_sweep_enable && sq1_sweep_shift_count > 0 && sq1_valid_freq)
                {
                    sq1_sweep_change = sq1_timer >> (int)sq1_sweep_shift_count;
                    sq1_timer += (sq1_sweep_negate ? (~sq1_sweep_change) : sq1_sweep_change);
                    SQ1CalculateValidFreq();
                }
            }
            if (sq1_sweep_reload)
            {
                sq1_sweep_counter = sq1_sweep_devider_period + 1;
                sq1_sweep_reload = false;
            }
        }

        private static void SQ1ClockEnvelope()
        {
            if (sq1_envelope_start_flag)
            {
                sq1_envelope_start_flag = false;
                sq1_envelope_decay_level_counter = 15;
                sq1_envelope_devider = (byte)(sq1_volume_devider_period + 1);
            }
            else if (sq1_envelope_devider > 0)
            {
                sq1_envelope_devider--;
            }
            else
            {
                sq1_envelope_devider = (byte)(sq1_volume_devider_period + 1);
                if (sq1_envelope_decay_level_counter > 0)
                {
                    sq1_envelope_decay_level_counter--;
                }
                else if (sq1_length_halt)
                {
                    sq1_envelope_decay_level_counter = 15;
                }
            }
            sq1_envelope = (sq1_constant_volume_envelope ? sq1_volume_devider_period : sq1_envelope_decay_level_counter);
        }

        private static void APUOnRegister4000()
        {
            if (apu_reg_access_w)
            {
                sq1_duty_cycle = (byte)((apu_reg_io_db & 0xC0) >> 6);
                sq1_volume_devider_period = (byte)(apu_reg_io_db & 0xFu);
                sq1_length_halt = (apu_reg_io_db & 0x20) != 0;
                sq1_constant_volume_envelope = (apu_reg_io_db & 0x10) != 0;
                sq1_envelope = (sq1_constant_volume_envelope ? sq1_volume_devider_period : sq1_envelope_decay_level_counter);
            }
        }

        private static void APUOnRegister4001()
        {
            if (apu_reg_access_w)
            {
                sq1_sweep_enable = (apu_reg_io_db & 0x80) == 128;
                sq1_sweep_devider_period = (byte)((uint)(apu_reg_io_db >> 4) & 7u);
                sq1_sweep_negate = (apu_reg_io_db & 8) == 8;
                sq1_sweep_shift_count = (byte)(apu_reg_io_db & 7u);
                sq1_sweep_reload = true;
                SQ1CalculateValidFreq();
            }
        }

        private static void APUOnRegister4002()
        {
            if (apu_reg_access_w)
            {
                sq1_timer = (sq1_timer & 0xFF00) | apu_reg_io_db;
                SQ1CalculateValidFreq();
            }
        }

        private static void APUOnRegister4003()
        {
            if (apu_reg_access_w)
            {
                sq1_timer = (sq1_timer & 0xFF) | ((apu_reg_io_db & 7) << 8);
                if (sq1_length_enabled && !sq1_ignore_reload)
                {
                    sq1_length_counter = sq_duration_table[apu_reg_io_db >> 3];
                }
                if (sq1_ignore_reload)
                {
                    sq1_ignore_reload = false;
                }
                sq1_seqencer = 0;
                sq1_envelope_start_flag = true;
                SQ1CalculateValidFreq();
            }
        }

        private static void SQ1On4015()
        {
            sq1_length_enabled = (apu_reg_io_db & 1) != 0;
            if (!sq1_length_enabled)
            {
                sq1_length_counter = 0;
            }
        }

        private static void SQ1Read4015()
        {
            if (sq1_length_counter > 0)
            {
                apu_reg_io_db = (byte)((apu_reg_io_db & 0xFEu) | 1u);
            }
        }

        private static void SQ1CalculateValidFreq()
        {
            sq1_valid_freq = sq1_timer >= 8 && (sq1_sweep_negate || ((sq1_timer + (sq1_timer >> (int)sq1_sweep_shift_count)) & 0x800) == 0);
        }

        private static void SQ1WriteState(ref BinaryWriter bin)
        {
            bin.Write(sq1_duty_cycle);
            bin.Write(sq1_length_halt);
            bin.Write(sq1_constant_volume_envelope);
            bin.Write(sq1_volume_devider_period);
            bin.Write(sq1_sweep_enable);
            bin.Write(sq1_sweep_devider_period);
            bin.Write(sq1_sweep_negate);
            bin.Write(sq1_sweep_shift_count);
            bin.Write(sq1_timer);
            bin.Write(sq1_period_devider);
            bin.Write(sq1_seqencer);
            bin.Write(sq1_length_enabled);
            bin.Write(sq1_length_counter);
            bin.Write(sq1_envelope_start_flag);
            bin.Write(sq1_envelope_devider);
            bin.Write(sq1_envelope_decay_level_counter);
            bin.Write(sq1_envelope);
            bin.Write(sq1_sweep_counter);
            bin.Write(sq1_sweep_reload);
            bin.Write(sq1_sweep_change);
            bin.Write(sq1_valid_freq);
            bin.Write(sq1_output);
            bin.Write(sq1_ignore_reload);
        }

        private static void SQ1ReadState(ref BinaryReader bin)
        {
            sq1_duty_cycle = bin.ReadByte();
            sq1_length_halt = bin.ReadBoolean();
            sq1_constant_volume_envelope = bin.ReadBoolean();
            sq1_volume_devider_period = bin.ReadByte();
            sq1_sweep_enable = bin.ReadBoolean();
            sq1_sweep_devider_period = bin.ReadByte();
            sq1_sweep_negate = bin.ReadBoolean();
            sq1_sweep_shift_count = bin.ReadByte();
            sq1_timer = bin.ReadInt32();
            sq1_period_devider = bin.ReadInt32();
            sq1_seqencer = bin.ReadByte();
            sq1_length_enabled = bin.ReadBoolean();
            sq1_length_counter = bin.ReadInt32();
            sq1_envelope_start_flag = bin.ReadBoolean();
            sq1_envelope_devider = bin.ReadByte();
            sq1_envelope_decay_level_counter = bin.ReadByte();
            sq1_envelope = bin.ReadByte();
            sq1_sweep_counter = bin.ReadInt32();
            sq1_sweep_reload = bin.ReadBoolean();
            sq1_sweep_change = bin.ReadInt32();
            sq1_valid_freq = bin.ReadBoolean();
            sq1_output = bin.ReadInt32();
            sq1_ignore_reload = bin.ReadBoolean();
        }

        private static void TRLHardReset()
        {
            trl_liner_control_flag = false;
            trl_liner_control_reload = 0;
            trl_timer = 0;
            trl_length_enabled = false;
            trl_length_counter = 0;
            trl_liner_control_reload_flag = false;
            trl_liner_counter = 0;
            trl_output = 0;
            trl_period_devider = 0;
            trl_step = 0;
            trl_ignore_reload = false;
        }

        private static void TRLSoftReset()
        {
            TRLHardReset();
        }

        private static void TRLClock()
        {
            trl_period_devider--;
            if (trl_period_devider > 0)
            {
                return;
            }
            trl_period_devider = trl_timer + 1;
            if (trl_length_counter > 0 && trl_liner_counter > 0 && trl_timer >= 4)
            {
                trl_step++;
                trl_step &= 31;
                if (audio_trl_outputable)
                {
                    trl_output = trl_step_seq[trl_step];
                }
            }
            audio_signal_outputed = true;
        }

        private static void TRLClockLength()
        {
            if (trl_length_counter > 0 && !trl_liner_control_flag)
            {
                trl_length_counter--;
                if (apu_reg_access_happened && apu_reg_io_addr == 11 && apu_reg_access_w)
                {
                    trl_ignore_reload = true;
                }
            }
        }

        private static void TRLClockEnvelope()
        {
            if (trl_liner_control_reload_flag)
            {
                trl_liner_counter = trl_liner_control_reload;
            }
            else if (trl_liner_counter > 0)
            {
                trl_liner_counter--;
            }
            if (!trl_liner_control_flag)
            {
                trl_liner_control_reload_flag = false;
            }
        }

        private static void APUOnRegister4008()
        {
            if (apu_reg_access_w)
            {
                trl_liner_control_flag = (apu_reg_io_db & 0x80) == 128;
                trl_liner_control_reload = (byte)(apu_reg_io_db & 0x7Fu);
            }
        }

        private static void APUOnRegister4009()
        {
        }

        private static void APUOnRegister400A()
        {
            if (apu_reg_access_w)
            {
                trl_timer = (ushort)((trl_timer & 0x7F00u) | apu_reg_io_db);
            }
        }

        private static void APUOnRegister400B()
        {
            if (apu_reg_access_w)
            {
                trl_timer = (ushort)((trl_timer & 0xFFu) | (uint)((apu_reg_io_db & 7) << 8));
                if (trl_length_enabled && !trl_ignore_reload)
                {
                    trl_length_counter = sq_duration_table[apu_reg_io_db >> 3];
                }
                if (trl_ignore_reload)
                {
                    trl_ignore_reload = false;
                }
                trl_liner_control_reload_flag = true;
            }
        }

        private static void TRLOn4015()
        {
            trl_length_enabled = (apu_reg_io_db & 4) != 0;
            if (!trl_length_enabled)
            {
                trl_length_counter = 0;
            }
        }

        private static void TRLRead4015()
        {
            if (trl_length_counter > 0)
            {
                apu_reg_io_db = (byte)((apu_reg_io_db & 0xFBu) | 4u);
            }
        }

        private static void TRLWriteState(ref BinaryWriter bin)
        {
            bin.Write(trl_liner_control_flag);
            bin.Write(trl_liner_control_reload);
            bin.Write(trl_timer);
            bin.Write(trl_length_enabled);
            bin.Write(trl_length_counter);
            bin.Write(trl_liner_control_reload_flag);
            bin.Write(trl_liner_counter);
            bin.Write(trl_output);
            bin.Write(trl_period_devider);
            bin.Write(trl_step);
            bin.Write(trl_ignore_reload);
        }

        private static void TRLReadState(ref BinaryReader bin)
        {
            trl_liner_control_flag = bin.ReadBoolean();
            trl_liner_control_reload = bin.ReadByte();
            trl_timer = bin.ReadUInt16();
            trl_length_enabled = bin.ReadBoolean();
            trl_length_counter = bin.ReadByte();
            trl_liner_control_reload_flag = bin.ReadBoolean();
            trl_liner_counter = bin.ReadByte();
            trl_output = bin.ReadInt32();
            trl_period_devider = bin.ReadInt32();
            trl_step = bin.ReadInt32();
            trl_ignore_reload = bin.ReadBoolean();
        }

        private static void APUInitialize()
        {
            apu_reg_update_func = new Action[32];
            apu_reg_read_func = new Action[32];
            apu_reg_write_func = new Action[32];
            for (int i = 0; i < 32; i++)
            {
                apu_reg_update_func[i] = APUBlankAccess;
                apu_reg_read_func[i] = APUBlankAccess;
                apu_reg_write_func[i] = APUBlankAccess;
            }
            apu_reg_update_func[0] = APUOnRegister4000;
            apu_reg_update_func[1] = APUOnRegister4001;
            apu_reg_update_func[2] = APUOnRegister4002;
            apu_reg_update_func[3] = APUOnRegister4003;
            apu_reg_update_func[4] = APUOnRegister4004;
            apu_reg_update_func[5] = APUOnRegister4005;
            apu_reg_update_func[6] = APUOnRegister4006;
            apu_reg_update_func[7] = APUOnRegister4007;
            apu_reg_update_func[8] = APUOnRegister4008;
            apu_reg_update_func[9] = APUOnRegister4009;
            apu_reg_update_func[10] = APUOnRegister400A;
            apu_reg_update_func[11] = APUOnRegister400B;
            apu_reg_update_func[12] = APUOnRegister400C;
            apu_reg_update_func[13] = APUOnRegister400D;
            apu_reg_update_func[14] = APUOnRegister400E;
            apu_reg_update_func[15] = APUOnRegister400F;
            apu_reg_update_func[16] = APUOnRegister4010;
            apu_reg_update_func[17] = APUOnRegister4011;
            apu_reg_update_func[18] = APUOnRegister4012;
            apu_reg_update_func[19] = APUOnRegister4013;
            apu_reg_update_func[21] = APUOnRegister4015;
            apu_reg_update_func[22] = APUOnRegister4016;
            apu_reg_update_func[23] = APUOnRegister4017;
            apu_reg_read_func[21] = APURead4015;
            apu_reg_read_func[22] = APURead4016;
            apu_reg_read_func[23] = APURead4017;
            apu_reg_write_func[20] = APUWrite4014;
            apu_reg_write_func[21] = APUWrite4015;
            audio_low_pass_filter_14K = new SoundLowPassFilter(0.00815686);
            audio_high_pass_filter_90 = new SoundHighPassFilter(0.999835);
            audio_high_pass_filter_440 = new SoundHighPassFilter(0.996039);
            audio_dc_blocker_filter = new SoundDCBlockerFilter(0.995);
            apu_update_playback_func = APUUpdatePlaybackWithFilters;
        }

        public static void ApplyAudioSettings(bool all = true)
        {
            SoundEnabled = MyNesMain.RendererSettings.Audio_SoundEnabled;
            audio_sq1_outputable = MyNesMain.RendererSettings.Audio_ChannelEnabled_SQ1;
            audio_sq2_outputable = MyNesMain.RendererSettings.Audio_ChannelEnabled_SQ2;
            audio_nos_outputable = MyNesMain.RendererSettings.Audio_ChannelEnabled_NOZ;
            audio_trl_outputable = MyNesMain.RendererSettings.Audio_ChannelEnabled_TRL;
            audio_dmc_outputable = MyNesMain.RendererSettings.Audio_ChannelEnabled_DMC;
            if (apu_use_external_sound)
            {
                mem_board.APUApplyChannelsSettings();
            }
            if (all)
            {
                CalculateAudioPlaybackValues();
            }
        }

        private static void APUHardReset()
        {
            apu_reg_io_db = 0;
            apu_reg_io_addr = 0;
            apu_reg_access_happened = false;
            apu_reg_access_w = false;
            apu_seq_mode = false;
            apu_odd_cycle = true;
            apu_cycle_f_t = 0;
            apu_cycle_e = 4;
            apu_cycle_f = 4;
            apu_cycle_l = 4;
            apu_odd_l = false;
            apu_check_irq = false;
            apu_do_env = false;
            apu_do_length = false;
            switch (Region)
            {
                case EmuRegion.NTSC:
                    cpu_speed = 1789773;
                    apu_ferq_f = 14914;
                    apu_ferq_e = 3728;
                    apu_ferq_l = 7456;
                    break;
                case EmuRegion.PALB:
                    cpu_speed = 1662607;
                    apu_ferq_f = 14914;
                    apu_ferq_e = 3728;
                    apu_ferq_l = 7456;
                    break;
                case EmuRegion.DENDY:
                    cpu_speed = 1773448;
                    apu_ferq_f = 14914;
                    apu_ferq_e = 3728;
                    apu_ferq_l = 7456;
                    break;
            }
            Tracer.WriteLine("NES: cpu speed = " + cpu_speed);
            SQ1HardReset();
            SQ2HardReset();
            NOSHardReset();
            DMCHardReset();
            TRLHardReset();
            apu_irq_enabled = true;
            apu_irq_flag = false;
            reg_2004 = 8196;
            CalculateAudioPlaybackValues();
            apu_use_external_sound = mem_board.enable_external_sound;
            if (apu_use_external_sound)
            {
                Tracer.WriteInformation("External sound channels has been enabled on apu.");
            }
        }

        private static void APUSoftReset()
        {
            apu_reg_io_db = 0;
            apu_reg_io_addr = 0;
            apu_reg_access_happened = false;
            apu_reg_access_w = false;
            apu_seq_mode = false;
            apu_odd_cycle = false;
            apu_cycle_f_t = 0;
            apu_cycle_e = 4;
            apu_cycle_f = 4;
            apu_cycle_l = 4;
            apu_odd_l = false;
            apu_check_irq = false;
            apu_do_env = false;
            apu_do_length = false;
            apu_irq_enabled = true;
            apu_irq_flag = false;
            SQ1SoftReset();
            SQ2SoftReset();
            TRLSoftReset();
            NOSSoftReset();
            DMCSoftReset();
        }

        private static void APUIORead(ref ushort addr, out byte value)
        {
            if (addr >= 16416)
            {
                mem_board.ReadEX(ref addr, out value);
                return;
            }
            apu_reg_io_addr = (byte)(addr & 0x1Fu);
            apu_reg_access_happened = true;
            apu_reg_access_w = false;
            apu_reg_read_func[apu_reg_io_addr]();
            value = apu_reg_io_db;
        }

        private static void APUIOWrite(ref ushort addr, ref byte value)
        {
            if (addr >= 16416)
            {
                mem_board.WriteEX(ref addr, ref value);
                return;
            }
            apu_reg_io_addr = (byte)(addr & 0x1Fu);
            apu_reg_io_db = value;
            apu_reg_access_w = true;
            apu_reg_access_happened = true;
            apu_reg_write_func[apu_reg_io_addr]();
        }

        private static void APUBlankAccess()
        {
        }

        private static void APUWrite4014()
        {
            dma_Oamaddress = (ushort)(apu_reg_io_db << 8);
            AssertOAMDMA();
        }

        private static void APUWrite4015()
        {
            if ((apu_reg_io_db & 0x10u) != 0)
            {
                if (dmc_dmaSize == 0)
                {
                    dmc_dmaSize = dmc_size_refresh;
                    dmc_dmaAddr = dmc_addr_refresh;
                }
            }
            else
            {
                dmc_dmaSize = 0;
            }
            if (!dmc_bufferFull && dmc_dmaSize > 0)
            {
                AssertDMCDMA();
            }
        }

        private static void APUOnRegister4015()
        {
            if (apu_reg_access_w)
            {
                SQ1On4015();
                SQ2On4015();
                NOSOn4015();
                TRLOn4015();
                DMCOn4015();
            }
            else
            {
                apu_irq_flag = false;
                IRQFlags &= -2;
            }
        }

        private static void APUOnRegister4016()
        {
            if (!apu_reg_access_w)
            {
                return;
            }
            if (inputStrobe > (apu_reg_io_db & 1))
            {
                if (IsFourPlayers)
                {
                    PORT0 = (joypad3.GetData() << 8) | joypad1.GetData() | 0x1010000;
                    PORT1 = (joypad4.GetData() << 8) | joypad2.GetData() | 0x2020000;
                }
                else
                {
                    PORT0 = joypad1.GetData() | 0x1010100;
                    PORT1 = joypad2.GetData() | 0x2020200;
                }
            }
            inputStrobe = apu_reg_io_db & 1;
        }

        private static void APUOnRegister4017()
        {
            if (apu_reg_access_w)
            {
                apu_seq_mode = (apu_reg_io_db & 0x80) != 0;
                apu_irq_enabled = (apu_reg_io_db & 0x40) == 0;
                apu_cycle_e = -1;
                apu_cycle_l = -1;
                apu_cycle_f = -1;
                apu_odd_l = false;
                apu_do_length = apu_seq_mode;
                apu_do_env = apu_seq_mode;
                apu_check_irq = false;
                if (!apu_irq_enabled)
                {
                    apu_irq_flag = false;
                    IRQFlags &= -2;
                }
            }
        }

        private static void APURead4015()
        {
            apu_reg_io_db &= 32;
            SQ1Read4015();
            SQ2Read4015();
            NOSRead4015();
            TRLRead4015();
            DMCRead4015();
            if (apu_irq_flag)
            {
                apu_reg_io_db = (byte)((apu_reg_io_db & 0xBFu) | 0x40u);
            }
            if (apu_irq_delta_occur)
            {
                apu_reg_io_db = (byte)((apu_reg_io_db & 0x7Fu) | 0x80u);
            }
        }

        private static void APURead4016()
        {
            apu_reg_io_db = (byte)((uint)PORT0 & 1u);
            PORT0 >>= 1;
        }

        private static void APURead4017()
        {
            apu_reg_io_db = (byte)((uint)PORT1 & 1u);
            PORT1 >>= 1;
        }

        private static void APUClock()
        {
            apu_odd_cycle = !apu_odd_cycle;
            if (apu_do_env)
            {
                APUClockEnvelope();
            }
            if (apu_do_length)
            {
                APUClockDuration();
            }
            if (apu_odd_cycle)
            {
                apu_cycle_f++;
                if (apu_cycle_f >= apu_ferq_f)
                {
                    apu_cycle_f = -1;
                    apu_check_irq = true;
                    apu_cycle_f_t = 3;
                }
                apu_cycle_e++;
                if (apu_cycle_e >= apu_ferq_e)
                {
                    apu_cycle_e = -1;
                    if (apu_check_irq)
                    {
                        if (!apu_seq_mode)
                        {
                            apu_do_env = true;
                        }
                        else
                        {
                            apu_cycle_e = 4;
                        }
                    }
                    else
                    {
                        apu_do_env = true;
                    }
                }
                apu_cycle_l++;
                if (apu_cycle_l >= apu_ferq_l)
                {
                    apu_odd_l = !apu_odd_l;
                    apu_cycle_l = (apu_odd_l ? (-2) : (-1));
                    if (apu_check_irq && apu_seq_mode)
                    {
                        apu_cycle_l = 3730;
                        apu_odd_l = true;
                    }
                    else
                    {
                        apu_do_length = true;
                    }
                }
                SQ1Clock();
                SQ2Clock();
                NOSClock();
                if (apu_use_external_sound)
                {
                    mem_board.OnAPUClock();
                }
                if (apu_reg_access_happened)
                {
                    apu_reg_access_happened = false;
                    apu_reg_update_func[apu_reg_io_addr]();
                }
            }
            TRLClock();
            DMCClock();
            if (apu_check_irq)
            {
                if (!apu_seq_mode)
                {
                    APUCheckIRQ();
                }
                apu_cycle_f_t--;
                if (apu_cycle_f_t == 0)
                {
                    apu_check_irq = false;
                }
            }
            if (apu_use_external_sound)
            {
                mem_board.OnAPUClockSingle();
            }
            apu_update_playback_func();
        }

        private static void APUClockDuration()
        {
            SQ1ClockLength();
            SQ2ClockLength();
            NOSClockLength();
            TRLClockLength();
            if (apu_use_external_sound)
            {
                mem_board.OnAPUClockDuration();
            }
            apu_do_length = false;
        }

        private static void APUClockEnvelope()
        {
            SQ1ClockEnvelope();
            SQ2ClockEnvelope();
            NOSClockEnvelope();
            TRLClockEnvelope();
            if (apu_use_external_sound)
            {
                mem_board.OnAPUClockEnvelope();
            }
            apu_do_env = false;
        }

        private static void APUCheckIRQ()
        {
            if (apu_irq_enabled)
            {
                apu_irq_flag = true;
            }
            if (apu_irq_flag)
            {
                IRQFlags |= 1;
            }
        }

        private static void CalculateAudioPlaybackValues()
        {
            audio_timer_ratio = (double)cpu_speed / (double)MyNesMain.RendererSettings.Audio_Frequency;
            audio_playback_peek_limit = MyNesMain.RendererSettings.Audio_InternalPeekLimit;
            audio_samples_count = MyNesMain.RendererSettings.Audio_InternalSamplesCount;
            audio_playback_amplitude = MyNesMain.RendererSettings.Audio_PlaybackAmplitude;
            audio_samples = new short[audio_samples_count];
            audio_w_pos = 0;
            audio_samples_added = 0;
            audio_timer = 0.0;
            audio_x = (audio_y = 0.0);
            Tracer.WriteLine("AUDIO: frequency = " + MyNesMain.RendererSettings.Audio_Frequency);
            Tracer.WriteLine("AUDIO: timer ratio = " + audio_timer_ratio);
            Tracer.WriteLine("AUDIO: internal samples count = " + audio_samples_count);
            Tracer.WriteLine("AUDIO: amplitude = " + audio_playback_amplitude);
            if (MyNesMain.RendererSettings.Audio_EnableFilters)
            {
                apu_update_playback_func = APUUpdatePlaybackWithFilters;
                audio_low_pass_filter_14K = new SoundLowPassFilter(SoundLowPassFilter.GetK((double)cpu_speed / 14000.0, 14000.0));
                audio_high_pass_filter_90 = new SoundHighPassFilter(SoundHighPassFilter.GetK((double)cpu_speed / 90.0, 90.0));
                audio_high_pass_filter_440 = new SoundHighPassFilter(SoundHighPassFilter.GetK((double)cpu_speed / 440.0, 440.0));
            }
            else
            {
                apu_update_playback_func = APUUpdatePlaybackWithoutFilters;
            }
            InitializeDACTables(force_intitialize: false);
        }

        public static void InitializeDACTables(bool force_intitialize)
        {
            if (audio_playback_dac_initialized && !force_intitialize)
            {
                return;
            }
            int[] array = new int[5];
            mix_table = new int[16][][][][];
            for (int i = 0; i < 16; i++)
            {
                mix_table[i] = new int[16][][][];
                for (int j = 0; j < 16; j++)
                {
                    mix_table[i][j] = new int[16][][];
                    for (int k = 0; k < 16; k++)
                    {
                        mix_table[i][j][k] = new int[16][];
                        for (int l = 0; l < 16; l++)
                        {
                            mix_table[i][j][k][l] = new int[128];
                            for (int m = 0; m < 128; m++)
                            {
                                if (MyNesMain.RendererSettings.Audio_UseDefaultMixer)
                                {
                                    double num = 95.88 / (8128.0 / (double)(i + j) + 100.0);
                                    double num2 = 159.79 / (1.0 / ((double)k / 8227.0 + (double)l / 12241.0 + (double)m / 22638.0) + 100.0);
                                    mix_table[i][j][k][l][m] = (int)Math.Ceiling((num + num2) * audio_playback_amplitude);
                                    continue;
                                }
                                GetPrec(i, 255, 2048, out array[0]);
                                GetPrec(j, 255, 2048, out array[1]);
                                GetPrec(l, 255, 2048, out array[2]);
                                GetPrec(k, 255, 2048, out array[3]);
                                GetPrec(m, 255, 2048, out array[4]);
                                array[4] /= 2;
                                int num3 = array[0] + array[1] + array[2] + array[3] + array[4];
                                num3 /= 5;
                                mix_table[i][j][k][l][m] = num3;
                            }
                        }
                    }
                }
            }
            audio_playback_dac_initialized = true;
        }

        private static void APUUpdatePlaybackWithFilters()
        {
            if (!SoundEnabled)
            {
                return;
            }
            audio_x = mix_table[sq1_output][sq2_output][trl_output][nos_output][dmc_output];
            if (apu_use_external_sound)
            {
                audio_x = (audio_x + mem_board.APUGetSample() * audio_playback_amplitude) / 2.0;
            }
            audio_high_pass_filter_90.DoFiltering(audio_x, out audio_y);
            audio_high_pass_filter_440.DoFiltering(audio_y, out audio_y);
            audio_low_pass_filter_14K.DoFiltering(audio_y, out audio_y);
            audio_y_av += audio_y;
            audio_y_timer += 1.0;
            audio_timer += 1.0;
            if (!(audio_timer >= audio_timer_ratio))
            {
                return;
            }
            if (audio_y_timer > 0.0)
            {
                audio_y = audio_y_av / audio_y_timer;
            }
            else
            {
                audio_y = 0.0;
            }
            audio_y_av = 0.0;
            audio_y_timer = 0.0;
            audio_timer -= audio_timer_ratio;
            if (audio_w_pos < audio_samples_count)
            {
                if (audio_y > (double)audio_playback_peek_limit)
                {
                    audio_y = audio_playback_peek_limit;
                }
                if (audio_y < (double)(-audio_playback_peek_limit))
                {
                    audio_y = -audio_playback_peek_limit;
                }
                audio_samples[audio_w_pos] = (short)audio_y;
                if (MyNesMain.WaveRecorder.IsRecording)
                {
                    MyNesMain.WaveRecorder.AddSample((short)audio_y);
                }
                audio_w_pos++;
                audio_samples_added++;
            }
            audio_y = 0.0;
        }

        private static void APUUpdatePlaybackWithoutFilters()
        {
            if (!SoundEnabled)
            {
                return;
            }
            audio_y = mix_table[sq1_output][sq2_output][trl_output][nos_output][dmc_output] / 2;
            if (apu_use_external_sound)
            {
                audio_y = (audio_y + mem_board.APUGetSample() * audio_playback_amplitude) / 2.0;
            }
            audio_y_av += audio_y;
            audio_y_timer += 1.0;
            audio_timer += 1.0;
            if (!(audio_timer >= audio_timer_ratio))
            {
                return;
            }
            if (audio_y_timer > 0.0)
            {
                audio_y = audio_y_av / audio_y_timer;
            }
            else
            {
                audio_y = 0.0;
            }
            audio_y_av = 0.0;
            audio_y_timer = 0.0;
            audio_timer -= audio_timer_ratio;
            if (audio_w_pos < audio_samples_count)
            {
                if (audio_y > (double)audio_playback_peek_limit)
                {
                    audio_y = audio_playback_peek_limit;
                }
                if (audio_y < (double)(-audio_playback_peek_limit))
                {
                    audio_y = -audio_playback_peek_limit;
                }
                audio_samples[audio_w_pos] = (short)audio_y;
                if (MyNesMain.WaveRecorder.IsRecording)
                {
                    MyNesMain.WaveRecorder.AddSample((short)audio_y);
                }
                audio_w_pos++;
                audio_samples_added++;
            }
            audio_y = 0.0;
        }

        private static void GetPrec(int inVal, int inMax, int outMax, out int val)
        {
            val = outMax * inVal / inMax;
        }

        private static void APUWriteState(ref BinaryWriter bin)
        {
            bin.Write(apu_reg_io_db);
            bin.Write(apu_reg_io_addr);
            bin.Write(apu_reg_access_happened);
            bin.Write(apu_reg_access_w);
            bin.Write(apu_odd_cycle);
            bin.Write(apu_irq_enabled);
            bin.Write(apu_irq_flag);
            bin.Write(apu_irq_delta_occur);
            bin.Write(apu_seq_mode);
            bin.Write(apu_ferq_f);
            bin.Write(apu_ferq_l);
            bin.Write(apu_ferq_e);
            bin.Write(apu_cycle_f);
            bin.Write(apu_cycle_e);
            bin.Write(apu_cycle_l);
            bin.Write(apu_odd_l);
            bin.Write(apu_cycle_f_t);
            bin.Write(apu_check_irq);
            bin.Write(apu_do_env);
            bin.Write(apu_do_length);
            SQ1WriteState(ref bin);
            SQ2WriteState(ref bin);
            NOSWriteState(ref bin);
            TRLWriteState(ref bin);
            DMCWriteState(ref bin);
        }

        private static void APUReadState(ref BinaryReader bin)
        {
            apu_reg_io_db = bin.ReadByte();
            apu_reg_io_addr = bin.ReadByte();
            apu_reg_access_happened = bin.ReadBoolean();
            apu_reg_access_w = bin.ReadBoolean();
            apu_odd_cycle = bin.ReadBoolean();
            apu_irq_enabled = bin.ReadBoolean();
            apu_irq_flag = bin.ReadBoolean();
            apu_irq_delta_occur = bin.ReadBoolean();
            apu_seq_mode = bin.ReadBoolean();
            apu_ferq_f = bin.ReadInt32();
            apu_ferq_l = bin.ReadInt32();
            apu_ferq_e = bin.ReadInt32();
            apu_cycle_f = bin.ReadInt32();
            apu_cycle_e = bin.ReadInt32();
            apu_cycle_l = bin.ReadInt32();
            apu_odd_l = bin.ReadBoolean();
            apu_cycle_f_t = bin.ReadInt32();
            apu_check_irq = bin.ReadBoolean();
            apu_do_env = bin.ReadBoolean();
            apu_do_length = bin.ReadBoolean();
            SQ1ReadState(ref bin);
            SQ2ReadState(ref bin);
            NOSReadState(ref bin);
            TRLReadState(ref bin);
            DMCReadState(ref bin);
        }

        private static byte register_pb()
        {
            return (byte)((cpu_flag_n ? 128u : 0u) | (cpu_flag_v ? 64u : 0u) | (cpu_flag_d ? 8u : 0u) | (cpu_flag_i ? 4u : 0u) | (cpu_flag_z ? 2u : 0u) | (cpu_flag_c ? 1u : 0u) | 0x30u);
        }

        private static void CPUInitialize()
        {
            cpu_addressings = new Action[256]
            {
                Imp____, IndX_R_, ImA____, IndX_W_, Zpg_R__, Zpg_R__, Zpg_RW_, Zpg_W__, ImA____, Imm____,
                ImA____, Imm____, Abs_R__, Abs_R__, Abs_RW_, Abs_W__, Imp____, IndY_R_, Imp____, IndY_W_,
                ZpgX_R_, ZpgX_R_, ZpgX_RW, ZpgX_W_, ImA____, AbsY_R_, ImA____, AbsY_W_, AbsX_R_, AbsX_R_,
                AbsX_RW, AbsX_W_, Imp____, IndX_R_, ImA____, IndX_W_, Zpg_R__, Zpg_R__, Zpg_RW_, Zpg_W__,
                ImA____, Imm____, ImA____, Imm____, Abs_R__, Abs_R__, Abs_RW_, Abs_W__, Imp____, IndY_R_,
                Imp____, IndY_W_, ZpgX_R_, ZpgX_R_, ZpgX_RW, ZpgX_W_, ImA____, AbsY_R_, ImA____, AbsY_W_,
                AbsX_R_, AbsX_R_, AbsX_RW, AbsX_W_, ImA____, IndX_R_, ImA____, IndX_W_, Zpg_R__, Zpg_R__,
                Zpg_RW_, Zpg_W__, ImA____, Imm____, ImA____, Imm____, Abs_W__, Abs_R__, Abs_RW_, Abs_W__,
                Imp____, IndY_R_, Imp____, IndY_W_, ZpgX_R_, ZpgX_R_, ZpgX_RW, ZpgX_W_, ImA____, AbsY_R_,
                ImA____, AbsY_W_, AbsX_R_, AbsX_R_, AbsX_RW, AbsX_W_, ImA____, IndX_R_, ImA____, IndX_W_,
                Zpg_R__, Zpg_R__, Zpg_RW_, Zpg_W__, ImA____, Imm____, ImA____, Imm____, Imp____, Abs_R__,
                Abs_RW_, Abs_W__, Imp____, IndY_R_, Imp____, IndY_W_, ZpgX_R_, ZpgX_R_, ZpgX_RW, ZpgX_W_,
                ImA____, AbsY_R_, ImA____, AbsY_W_, AbsX_R_, AbsX_R_, AbsX_RW, AbsX_W_, Imm____, IndX_W_,
                Imm____, IndX_W_, Zpg_W__, Zpg_W__, Zpg_W__, Zpg_W__, ImA____, Imm____, ImA____, Imm____,
                Abs_W__, Abs_W__, Abs_W__, Abs_W__, Imp____, IndY_W_, Imp____, IndY_W_, ZpgX_W_, ZpgX_W_,
                ZpgY_W_, ZpgY_W_, ImA____, AbsY_W_, ImA____, AbsY_W_, Abs_W__, AbsX_W_, Abs_W__, AbsY_W_,
                Imm____, IndX_R_, Imm____, IndX_R_, Zpg_R__, Zpg_R__, Zpg_R__, Zpg_R__, ImA____, Imm____,
                ImA____, Imm____, Abs_R__, Abs_R__, Abs_R__, Abs_R__, Imp____, IndY_R_, Imp____, IndY_R_,
                ZpgX_R_, ZpgX_R_, ZpgY_R_, ZpgY_R_, ImA____, AbsY_R_, ImA____, AbsY_R_, AbsX_R_, AbsX_R_,
                AbsY_R_, AbsY_R_, Imm____, IndX_R_, Imm____, IndX_R_, Zpg_R__, Zpg_R__, Zpg_RW_, Zpg_R__,
                ImA____, Imm____, ImA____, Imm____, Abs_R__, Abs_R__, Abs_RW_, Abs_R__, Imp____, IndY_R_,
                Imp____, IndY_RW, ZpgX_R_, ZpgX_R_, ZpgX_RW, ZpgX_RW, ImA____, AbsY_R_, ImA____, AbsY_RW,
                AbsX_R_, AbsX_R_, AbsX_RW, AbsX_RW, Imm____, IndX_R_, Imm____, IndX_W_, Zpg_R__, Zpg_R__,
                Zpg_RW_, Zpg_W__, ImA____, Imm____, ImA____, Imm____, Abs_R__, Abs_R__, Abs_RW_, Abs_W__,
                Imp____, IndY_R_, Imp____, IndY_W_, ZpgX_R_, ZpgX_R_, ZpgX_RW, ZpgX_W_, ImA____, AbsY_R_,
                ImA____, AbsY_W_, AbsX_R_, AbsX_R_, AbsX_RW, AbsX_W_
            };
            cpu_instructions = new Action[256]
            {
                BRK__, ORA__, NOP__, SLO__, NOP__, ORA__, ASL_M, SLO__, PHP__, ORA__,
                ASL_A, ANC__, NOP__, ORA__, ASL_M, SLO__, BPL__, ORA__, NOP__, SLO__,
                NOP__, ORA__, ASL_M, SLO__, CLC__, ORA__, NOP__, SLO__, NOP__, ORA__,
                ASL_M, SLO__, JSR__, AND__, NOP__, RLA__, BIT__, AND__, ROL_M, RLA__,
                PLP__, AND__, ROL_A, ANC__, BIT__, AND__, ROL_M, RLA__, BMI__, AND__,
                NOP__, RLA__, NOP__, AND__, ROL_M, RLA__, SEC__, AND__, NOP__, RLA__,
                NOP__, AND__, ROL_M, RLA__, RTI__, EOR__, NOP__, SRE__, NOP__, EOR__,
                LSR_M, SRE__, PHA__, EOR__, LSR_A, ALR__, JMP__, EOR__, LSR_M, SRE__,
                BVC__, EOR__, NOP__, SRE__, NOP__, EOR__, LSR_M, SRE__, CLI__, EOR__,
                NOP__, SRE__, NOP__, EOR__, LSR_M, SRE__, RTS__, ADC__, NOP__, RRA__,
                NOP__, ADC__, ROR_M, RRA__, PLA__, ADC__, ROR_A, ARR__, JMP_I, ADC__,
                ROR_M, RRA__, BVS__, ADC__, NOP__, RRA__, NOP__, ADC__, ROR_M, RRA__,
                SEI__, ADC__, NOP__, RRA__, NOP__, ADC__, ROR_M, RRA__, NOP__, STA__,
                NOP__, SAX__, STY__, STA__, STX__, SAX__, DEY__, NOP__, TXA__, XAA__,
                STY__, STA__, STX__, SAX__, BCC__, STA__, NOP__, AHX__, STY__, STA__,
                STX__, SAX__, TYA__, STA__, TXS__, XAS__, SHY__, STA__, SHX__, AHX__,
                LDY__, LDA__, LDX__, LAX__, LDY__, LDA__, LDX__, LAX__, TAY__, LDA__,
                TAX__, LAX__, LDY__, LDA__, LDX__, LAX__, BCS__, LDA__, NOP__, LAX__,
                LDY__, LDA__, LDX__, LAX__, CLV__, LDA__, TSX__, LAR__, LDY__, LDA__,
                LDX__, LAX__, CPY__, CMP__, NOP__, DCP__, CPY__, CMP__, DEC__, DCP__,
                INY__, CMP__, DEX__, AXS__, CPY__, CMP__, DEC__, DCP__, BNE__, CMP__,
                NOP__, DCP__, NOP__, CMP__, DEC__, DCP__, CLD__, CMP__, NOP__, DCP__,
                NOP__, CMP__, DEC__, DCP__, CPX__, SBC__, NOP__, ISC__, CPX__, SBC__,
                INC__, ISC__, INX__, SBC__, NOP__, SBC__, CPX__, SBC__, INC__, ISC__,
                BEQ__, SBC__, NOP__, ISC__, NOP__, SBC__, INC__, ISC__, SED__, SBC__,
                NOP__, ISC__, NOP__, SBC__, INC__, ISC__
            };
        }

        private static void CPUClock()
        {
            Read(ref cpu_reg_pc.v, out cpu_opcode);
            cpu_reg_pc.v++;
            cpu_addressings[cpu_opcode]();
            cpu_instructions[cpu_opcode]();
            if (CPU_IRQ_PIN || CPU_NMI_PIN)
            {
                Read(ref cpu_reg_pc.v, out cpu_dummy);
                Read(ref cpu_reg_pc.v, out cpu_dummy);
                Interrupt();
            }
        }

        private static void CPUHardReset()
        {
            cpu_reg_a = 0;
            cpu_reg_x = 0;
            cpu_reg_y = 0;
            cpu_reg_sp.l = 253;
            cpu_reg_sp.h = 1;
            ushort addr = 65532;
            mem_board.ReadPRG(ref addr, out cpu_reg_pc.l);
            addr++;
            mem_board.ReadPRG(ref addr, out cpu_reg_pc.h);
            register_p = 0;
            cpu_flag_i = true;
            cpu_reg_ea.v = 0;
            cpu_opcode = 0;
            CPU_IRQ_PIN = false;
            CPU_NMI_PIN = false;
            cpu_suspend_nmi = false;
            cpu_suspend_irq = false;
            IRQFlags = 0;
        }

        private static void CPUSoftReset()
        {
            cpu_flag_i = true;
            cpu_reg_sp.v -= 3;
            ushort addr = 65532;
            Read(ref addr, out cpu_reg_pc.l);
            addr++;
            Read(ref addr, out cpu_reg_pc.h);
        }

        private static void Imp____()
        {
        }

        private static void IndX_R_()
        {
            temp_add.h = 0;
            Read(ref cpu_reg_pc.v, out temp_add.l);
            cpu_reg_pc.v++;
            Read(ref temp_add.v, out cpu_dummy);
            temp_add.l += cpu_reg_x;
            Read(ref temp_add.v, out cpu_reg_ea.l);
            temp_add.l++;
            Read(ref temp_add.v, out cpu_reg_ea.h);
            Read(ref cpu_reg_ea.v, out cpu_m);
        }

        private static void IndX_W_()
        {
            temp_add.h = 0;
            Read(ref cpu_reg_pc.v, out temp_add.l);
            cpu_reg_pc.v++;
            Read(ref temp_add.v, out cpu_dummy);
            temp_add.l += cpu_reg_x;
            Read(ref temp_add.v, out cpu_reg_ea.l);
            temp_add.l++;
            Read(ref temp_add.v, out cpu_reg_ea.h);
        }

        private static void IndX_RW()
        {
            temp_add.h = 0;
            Read(ref cpu_reg_pc.v, out temp_add.l);
            cpu_reg_pc.v++;
            Read(ref temp_add.v, out cpu_dummy);
            temp_add.l += cpu_reg_x;
            Read(ref temp_add.v, out cpu_reg_ea.l);
            temp_add.l++;
            Read(ref temp_add.v, out cpu_reg_ea.h);
            Read(ref cpu_reg_ea.v, out cpu_m);
        }

        private static void IndY_R_()
        {
            temp_add.h = 0;
            Read(ref cpu_reg_pc.v, out temp_add.l);
            cpu_reg_pc.v++;
            Read(ref temp_add.v, out cpu_reg_ea.l);
            temp_add.l++;
            Read(ref temp_add.v, out cpu_reg_ea.h);
            cpu_reg_ea.l += cpu_reg_y;
            Read(ref cpu_reg_ea.v, out cpu_m);
            if (cpu_reg_ea.l < cpu_reg_y)
            {
                cpu_reg_ea.h++;
                Read(ref cpu_reg_ea.v, out cpu_m);
            }
        }

        private static void IndY_W_()
        {
            temp_add.h = 0;
            Read(ref cpu_reg_pc.v, out temp_add.l);
            cpu_reg_pc.v++;
            Read(ref temp_add.v, out cpu_reg_ea.l);
            temp_add.l++;
            Read(ref temp_add.v, out cpu_reg_ea.h);
            cpu_reg_ea.l += cpu_reg_y;
            Read(ref cpu_reg_ea.v, out cpu_m);
            if (cpu_reg_ea.l < cpu_reg_y)
            {
                cpu_reg_ea.h++;
            }
        }

        private static void IndY_RW()
        {
            temp_add.h = 0;
            Read(ref cpu_reg_pc.v, out temp_add.l);
            cpu_reg_pc.v++;
            Read(ref temp_add.v, out cpu_reg_ea.l);
            temp_add.l++;
            Read(ref temp_add.v, out cpu_reg_ea.h);
            cpu_reg_ea.l += cpu_reg_y;
            Read(ref cpu_reg_ea.v, out cpu_dummy);
            if (cpu_reg_ea.l < cpu_reg_y)
            {
                cpu_reg_ea.h++;
            }
            Read(ref cpu_reg_ea.v, out cpu_m);
        }

        private static void Zpg_R__()
        {
            cpu_reg_ea.h = 0;
            Read(ref cpu_reg_pc.v, out cpu_reg_ea.l);
            cpu_reg_pc.v++;
            Read(ref cpu_reg_ea.v, out cpu_m);
        }

        private static void Zpg_W__()
        {
            cpu_reg_ea.h = 0;
            Read(ref cpu_reg_pc.v, out cpu_reg_ea.l);
            cpu_reg_pc.v++;
        }

        private static void Zpg_RW_()
        {
            cpu_reg_ea.h = 0;
            Read(ref cpu_reg_pc.v, out cpu_reg_ea.l);
            cpu_reg_pc.v++;
            Read(ref cpu_reg_ea.v, out cpu_m);
        }

        private static void ZpgX_R_()
        {
            cpu_reg_ea.h = 0;
            Read(ref cpu_reg_pc.v, out cpu_reg_ea.l);
            cpu_reg_pc.v++;
            Read(ref cpu_reg_ea.v, out cpu_dummy);
            cpu_reg_ea.l += cpu_reg_x;
            Read(ref cpu_reg_ea.v, out cpu_m);
        }

        private static void ZpgX_W_()
        {
            cpu_reg_ea.h = 0;
            Read(ref cpu_reg_pc.v, out cpu_reg_ea.l);
            cpu_reg_pc.v++;
            Read(ref cpu_reg_ea.v, out cpu_dummy);
            cpu_reg_ea.l += cpu_reg_x;
        }

        private static void ZpgX_RW()
        {
            cpu_reg_ea.h = 0;
            Read(ref cpu_reg_pc.v, out cpu_reg_ea.l);
            cpu_reg_pc.v++;
            Read(ref cpu_reg_ea.v, out cpu_dummy);
            cpu_reg_ea.l += cpu_reg_x;
            Read(ref cpu_reg_ea.v, out cpu_m);
        }

        private static void ZpgY_R_()
        {
            cpu_reg_ea.h = 0;
            Read(ref cpu_reg_pc.v, out cpu_reg_ea.l);
            cpu_reg_pc.v++;
            Read(ref cpu_reg_ea.v, out cpu_dummy);
            cpu_reg_ea.l += cpu_reg_y;
            Read(ref cpu_reg_ea.v, out cpu_m);
        }

        private static void ZpgY_W_()
        {
            cpu_reg_ea.h = 0;
            Read(ref cpu_reg_pc.v, out cpu_reg_ea.l);
            cpu_reg_pc.v++;
            Read(ref cpu_reg_ea.v, out cpu_dummy);
            cpu_reg_ea.l += cpu_reg_y;
        }

        private static void ZpgY_RW()
        {
            cpu_reg_ea.h = 0;
            Read(ref cpu_reg_pc.v, out cpu_reg_ea.l);
            cpu_reg_pc.v++;
            Read(ref cpu_reg_ea.v, out cpu_dummy);
            cpu_reg_ea.l += cpu_reg_y;
            Read(ref cpu_reg_ea.v, out cpu_m);
        }

        private static void Imm____()
        {
            Read(ref cpu_reg_pc.v, out cpu_m);
            cpu_reg_pc.v++;
        }

        private static void ImA____()
        {
            Read(ref cpu_reg_pc.v, out cpu_dummy);
        }

        private static void Abs_R__()
        {
            Read(ref cpu_reg_pc.v, out cpu_reg_ea.l);
            cpu_reg_pc.v++;
            Read(ref cpu_reg_pc.v, out cpu_reg_ea.h);
            cpu_reg_pc.v++;
            Read(ref cpu_reg_ea.v, out cpu_m);
        }

        private static void Abs_W__()
        {
            Read(ref cpu_reg_pc.v, out cpu_reg_ea.l);
            cpu_reg_pc.v++;
            Read(ref cpu_reg_pc.v, out cpu_reg_ea.h);
            cpu_reg_pc.v++;
        }

        private static void Abs_RW_()
        {
            Read(ref cpu_reg_pc.v, out cpu_reg_ea.l);
            cpu_reg_pc.v++;
            Read(ref cpu_reg_pc.v, out cpu_reg_ea.h);
            cpu_reg_pc.v++;
            Read(ref cpu_reg_ea.v, out cpu_m);
        }

        private static void AbsX_R_()
        {
            Read(ref cpu_reg_pc.v, out cpu_reg_ea.l);
            cpu_reg_pc.v++;
            Read(ref cpu_reg_pc.v, out cpu_reg_ea.h);
            cpu_reg_pc.v++;
            cpu_reg_ea.l += cpu_reg_x;
            Read(ref cpu_reg_ea.v, out cpu_m);
            if (cpu_reg_ea.l < cpu_reg_x)
            {
                cpu_reg_ea.h++;
                Read(ref cpu_reg_ea.v, out cpu_m);
            }
        }

        private static void AbsX_W_()
        {
            Read(ref cpu_reg_pc.v, out cpu_reg_ea.l);
            cpu_reg_pc.v++;
            Read(ref cpu_reg_pc.v, out cpu_reg_ea.h);
            cpu_reg_pc.v++;
            cpu_reg_ea.l += cpu_reg_x;
            Read(ref cpu_reg_ea.v, out cpu_m);
            if (cpu_reg_ea.l < cpu_reg_x)
            {
                cpu_reg_ea.h++;
            }
        }

        private static void AbsX_RW()
        {
            Read(ref cpu_reg_pc.v, out cpu_reg_ea.l);
            cpu_reg_pc.v++;
            Read(ref cpu_reg_pc.v, out cpu_reg_ea.h);
            cpu_reg_pc.v++;
            cpu_reg_ea.l += cpu_reg_x;
            Read(ref cpu_reg_ea.v, out cpu_dummy);
            if (cpu_reg_ea.l < cpu_reg_x)
            {
                cpu_reg_ea.h++;
            }
            Read(ref cpu_reg_ea.v, out cpu_m);
        }

        private static void AbsY_R_()
        {
            Read(ref cpu_reg_pc.v, out cpu_reg_ea.l);
            cpu_reg_pc.v++;
            Read(ref cpu_reg_pc.v, out cpu_reg_ea.h);
            cpu_reg_pc.v++;
            cpu_reg_ea.l += cpu_reg_y;
            Read(ref cpu_reg_ea.v, out cpu_m);
            if (cpu_reg_ea.l < cpu_reg_y)
            {
                cpu_reg_ea.h++;
                Read(ref cpu_reg_ea.v, out cpu_m);
            }
        }

        private static void AbsY_W_()
        {
            Read(ref cpu_reg_pc.v, out cpu_reg_ea.l);
            cpu_reg_pc.v++;
            Read(ref cpu_reg_pc.v, out cpu_reg_ea.h);
            cpu_reg_pc.v++;
            cpu_reg_ea.l += cpu_reg_y;
            Read(ref cpu_reg_ea.v, out cpu_m);
            if (cpu_reg_ea.l < cpu_reg_y)
            {
                cpu_reg_ea.h++;
            }
        }

        private static void AbsY_RW()
        {
            Read(ref cpu_reg_pc.v, out cpu_reg_ea.l);
            cpu_reg_pc.v++;
            Read(ref cpu_reg_pc.v, out cpu_reg_ea.h);
            cpu_reg_pc.v++;
            cpu_reg_ea.l += cpu_reg_y;
            Read(ref cpu_reg_ea.v, out cpu_m);
            if (cpu_reg_ea.l < cpu_reg_y)
            {
                cpu_reg_ea.h++;
            }
            Read(ref cpu_reg_ea.v, out cpu_m);
        }

        private static void Interrupt()
        {
            Push(ref cpu_reg_pc.h);
            Push(ref cpu_reg_pc.l);
            cpu_dummy = ((cpu_opcode == 0) ? register_pb() : register_p);
            Push(ref cpu_dummy);
            temp_add.v = InterruptVector;
            cpu_suspend_nmi = true;
            cpu_flag_i = true;
            CPU_NMI_PIN = false;
            Read(ref temp_add.v, out cpu_reg_pc.l);
            temp_add.v++;
            Read(ref temp_add.v, out cpu_reg_pc.h);
            cpu_suspend_nmi = false;
        }

        private static void Branch(ref bool condition)
        {
            Read(ref cpu_reg_pc.v, out cpu_byte_temp);
            cpu_reg_pc.v++;
            if (!condition)
            {
                return;
            }
            cpu_suspend_irq = true;
            Read(ref cpu_reg_pc.v, out cpu_dummy);
            cpu_reg_pc.l += cpu_byte_temp;
            cpu_suspend_irq = false;
            if (cpu_byte_temp >= 128)
            {
                if (cpu_reg_pc.l >= cpu_byte_temp)
                {
                    Read(ref cpu_reg_pc.v, out cpu_dummy);
                    cpu_reg_pc.h--;
                }
            }
            else if (cpu_reg_pc.l < cpu_byte_temp)
            {
                Read(ref cpu_reg_pc.v, out cpu_dummy);
                cpu_reg_pc.h++;
            }
        }

        private static void Push(ref byte val)
        {
            Write(ref cpu_reg_sp.v, ref val);
            cpu_reg_sp.l--;
        }

        private static void Pull(out byte val)
        {
            cpu_reg_sp.l++;
            Read(ref cpu_reg_sp.v, out val);
        }

        private static void ADC__()
        {
            cpu_int_temp = cpu_reg_a + cpu_m + (cpu_flag_c ? 1 : 0);
            cpu_flag_v = ((cpu_int_temp ^ cpu_reg_a) & (cpu_int_temp ^ cpu_m) & 0x80) != 0;
            cpu_flag_n = (cpu_int_temp & 0x80) != 0;
            cpu_flag_z = (cpu_int_temp & 0xFF) == 0;
            cpu_flag_c = cpu_int_temp >> 8 != 0;
            cpu_reg_a = (byte)((uint)cpu_int_temp & 0xFFu);
        }

        private static void AHX__()
        {
            cpu_byte_temp = (byte)((uint)(cpu_reg_a & cpu_reg_x) & 7u);
            Write(ref cpu_reg_ea.v, ref cpu_byte_temp);
        }

        private static void ALR__()
        {
            cpu_reg_a &= cpu_m;
            cpu_flag_c = (cpu_reg_a & 1) != 0;
            cpu_reg_a >>= 1;
            cpu_flag_n = (cpu_reg_a & 0x80) != 0;
            cpu_flag_z = cpu_reg_a == 0;
        }

        private static void ANC__()
        {
            cpu_reg_a &= cpu_m;
            cpu_flag_n = (cpu_reg_a & 0x80) != 0;
            cpu_flag_z = cpu_reg_a == 0;
            cpu_flag_c = (cpu_reg_a & 0x80) != 0;
        }

        private static void AND__()
        {
            cpu_reg_a &= cpu_m;
            cpu_flag_n = (cpu_reg_a & 0x80) == 128;
            cpu_flag_z = cpu_reg_a == 0;
        }

        private static void ARR__()
        {
            cpu_reg_a = (byte)((uint)((cpu_m & cpu_reg_a) >> 1) | (cpu_flag_c ? 128u : 0u));
            cpu_flag_z = (cpu_reg_a & 0xFF) == 0;
            cpu_flag_n = (cpu_reg_a & 0x80) != 0;
            cpu_flag_c = (cpu_reg_a & 0x40) != 0;
            cpu_flag_v = (((cpu_reg_a << 1) ^ cpu_reg_a) & 0x40) != 0;
        }

        private static void AXS__()
        {
            cpu_int_temp = (cpu_reg_a & cpu_reg_x) - cpu_m;
            cpu_flag_n = (cpu_int_temp & 0x80) != 0;
            cpu_flag_z = (cpu_int_temp & 0xFF) == 0;
            cpu_flag_c = ~cpu_int_temp >> 8 != 0;
            cpu_reg_x = (byte)((uint)cpu_int_temp & 0xFFu);
        }

        private static void ASL_M()
        {
            cpu_flag_c = (cpu_m & 0x80) == 128;
            Write(ref cpu_reg_ea.v, ref cpu_m);
            cpu_m = (byte)((uint)(cpu_m << 1) & 0xFEu);
            Write(ref cpu_reg_ea.v, ref cpu_m);
            cpu_flag_n = (cpu_m & 0x80) == 128;
            cpu_flag_z = cpu_m == 0;
        }

        private static void ASL_A()
        {
            cpu_flag_c = (cpu_reg_a & 0x80) == 128;
            cpu_reg_a = (byte)((uint)(cpu_reg_a << 1) & 0xFEu);
            cpu_flag_n = (cpu_reg_a & 0x80) == 128;
            cpu_flag_z = cpu_reg_a == 0;
        }

        private static void BCC__()
        {
            cpu_bool_tmp = !cpu_flag_c;
            Branch(ref cpu_bool_tmp);
        }

        private static void BCS__()
        {
            Branch(ref cpu_flag_c);
        }

        private static void BEQ__()
        {
            Branch(ref cpu_flag_z);
        }

        private static void BIT__()
        {
            cpu_flag_n = (cpu_m & 0x80) != 0;
            cpu_flag_v = (cpu_m & 0x40) != 0;
            cpu_flag_z = (cpu_m & cpu_reg_a) == 0;
        }

        private static void BRK__()
        {
            Read(ref cpu_reg_pc.v, out cpu_dummy);
            cpu_reg_pc.v++;
            Interrupt();
        }

        private static void BPL__()
        {
            cpu_bool_tmp = !cpu_flag_n;
            Branch(ref cpu_bool_tmp);
        }

        private static void BNE__()
        {
            cpu_bool_tmp = !cpu_flag_z;
            Branch(ref cpu_bool_tmp);
        }

        private static void BMI__()
        {
            Branch(ref cpu_flag_n);
        }

        private static void BVC__()
        {
            cpu_bool_tmp = !cpu_flag_v;
            Branch(ref cpu_bool_tmp);
        }

        private static void BVS__()
        {
            Branch(ref cpu_flag_v);
        }

        private static void SED__()
        {
            cpu_flag_d = true;
        }

        private static void CLC__()
        {
            cpu_flag_c = false;
        }

        private static void CLD__()
        {
            cpu_flag_d = false;
        }

        private static void CLV__()
        {
            cpu_flag_v = false;
        }

        private static void CMP__()
        {
            cpu_int_temp = cpu_reg_a - cpu_m;
            cpu_flag_n = (cpu_int_temp & 0x80) == 128;
            cpu_flag_c = cpu_reg_a >= cpu_m;
            cpu_flag_z = cpu_int_temp == 0;
        }

        private static void CPX__()
        {
            cpu_int_temp = cpu_reg_x - cpu_m;
            cpu_flag_n = (cpu_int_temp & 0x80) == 128;
            cpu_flag_c = cpu_reg_x >= cpu_m;
            cpu_flag_z = cpu_int_temp == 0;
        }

        private static void CPY__()
        {
            cpu_int_temp = cpu_reg_y - cpu_m;
            cpu_flag_n = (cpu_int_temp & 0x80) == 128;
            cpu_flag_c = cpu_reg_y >= cpu_m;
            cpu_flag_z = cpu_int_temp == 0;
        }

        private static void CLI__()
        {
            cpu_flag_i = false;
        }

        private static void DCP__()
        {
            Write(ref cpu_reg_ea.v, ref cpu_m);
            cpu_m--;
            Write(ref cpu_reg_ea.v, ref cpu_m);
            cpu_int_temp = cpu_reg_a - cpu_m;
            cpu_flag_n = (cpu_int_temp & 0x80) != 0;
            cpu_flag_z = cpu_int_temp == 0;
            cpu_flag_c = ~cpu_int_temp >> 8 != 0;
        }

        private static void DEC__()
        {
            Write(ref cpu_reg_ea.v, ref cpu_m);
            cpu_m--;
            Write(ref cpu_reg_ea.v, ref cpu_m);
            cpu_flag_n = (cpu_m & 0x80) == 128;
            cpu_flag_z = cpu_m == 0;
        }

        private static void DEY__()
        {
            cpu_reg_y--;
            cpu_flag_z = cpu_reg_y == 0;
            cpu_flag_n = (cpu_reg_y & 0x80) == 128;
        }

        private static void DEX__()
        {
            cpu_reg_x--;
            cpu_flag_z = cpu_reg_x == 0;
            cpu_flag_n = (cpu_reg_x & 0x80) == 128;
        }

        private static void EOR__()
        {
            cpu_reg_a ^= cpu_m;
            cpu_flag_n = (cpu_reg_a & 0x80) == 128;
            cpu_flag_z = cpu_reg_a == 0;
        }

        private static void INC__()
        {
            Write(ref cpu_reg_ea.v, ref cpu_m);
            cpu_m++;
            Write(ref cpu_reg_ea.v, ref cpu_m);
            cpu_flag_n = (cpu_m & 0x80) == 128;
            cpu_flag_z = cpu_m == 0;
        }

        private static void INX__()
        {
            cpu_reg_x++;
            cpu_flag_z = cpu_reg_x == 0;
            cpu_flag_n = (cpu_reg_x & 0x80) == 128;
        }

        private static void INY__()
        {
            cpu_reg_y++;
            cpu_flag_n = (cpu_reg_y & 0x80) == 128;
            cpu_flag_z = cpu_reg_y == 0;
        }

        private static void ISC__()
        {
            Read(ref cpu_reg_ea.v, out cpu_byte_temp);
            Write(ref cpu_reg_ea.v, ref cpu_byte_temp);
            cpu_byte_temp++;
            Write(ref cpu_reg_ea.v, ref cpu_byte_temp);
            cpu_int_temp = cpu_byte_temp ^ 0xFF;
            cpu_int_temp1 = cpu_reg_a + cpu_int_temp + (cpu_flag_c ? 1 : 0);
            cpu_flag_n = (cpu_int_temp1 & 0x80) != 0;
            cpu_flag_v = ((cpu_int_temp1 ^ cpu_reg_a) & (cpu_int_temp1 ^ cpu_int_temp) & 0x80) != 0;
            cpu_flag_z = (cpu_int_temp1 & 0xFF) == 0;
            cpu_flag_c = cpu_int_temp1 >> 8 != 0;
            cpu_reg_a = (byte)((uint)cpu_int_temp1 & 0xFFu);
        }

        private static void JMP__()
        {
            cpu_reg_pc.v = cpu_reg_ea.v;
        }

        private static void JMP_I()
        {
            Read(ref cpu_reg_pc.v, out cpu_reg_ea.l);
            cpu_reg_pc.v++;
            Read(ref cpu_reg_pc.v, out cpu_reg_ea.h);
            Read(ref cpu_reg_ea.v, out cpu_reg_pc.l);
            cpu_reg_ea.l++;
            Read(ref cpu_reg_ea.v, out cpu_reg_pc.h);
        }

        private static void JSR__()
        {
            Read(ref cpu_reg_pc.v, out cpu_reg_ea.l);
            cpu_reg_pc.v++;
            Write(ref cpu_reg_sp.v, ref cpu_reg_ea.l);
            Push(ref cpu_reg_pc.h);
            Push(ref cpu_reg_pc.l);
            Read(ref cpu_reg_pc.v, out cpu_reg_ea.h);
            cpu_reg_pc.v = cpu_reg_ea.v;
        }

        private static void LAR__()
        {
            cpu_reg_sp.l &= cpu_m;
            cpu_reg_a = cpu_reg_sp.l;
            cpu_reg_x = cpu_reg_sp.l;
            cpu_flag_n = (cpu_reg_sp.l & 0x80) != 0;
            cpu_flag_z = (cpu_reg_sp.l & 0xFF) == 0;
        }

        private static void LAX__()
        {
            cpu_reg_x = (cpu_reg_a = cpu_m);
            cpu_flag_n = (cpu_reg_x & 0x80) != 0;
            cpu_flag_z = (cpu_reg_x & 0xFF) == 0;
        }

        private static void LDA__()
        {
            cpu_reg_a = cpu_m;
            cpu_flag_n = (cpu_reg_a & 0x80) == 128;
            cpu_flag_z = cpu_reg_a == 0;
        }

        private static void LDX__()
        {
            cpu_reg_x = cpu_m;
            cpu_flag_n = (cpu_reg_x & 0x80) == 128;
            cpu_flag_z = cpu_reg_x == 0;
        }

        private static void LDY__()
        {
            cpu_reg_y = cpu_m;
            cpu_flag_n = (cpu_reg_y & 0x80) == 128;
            cpu_flag_z = cpu_reg_y == 0;
        }

        private static void LSR_A()
        {
            cpu_flag_c = (cpu_reg_a & 1) == 1;
            cpu_reg_a >>= 1;
            cpu_flag_z = cpu_reg_a == 0;
            cpu_flag_n = (cpu_reg_a & 0x80) != 0;
        }

        private static void LSR_M()
        {
            cpu_flag_c = (cpu_m & 1) == 1;
            Write(ref cpu_reg_ea.v, ref cpu_m);
            cpu_m >>= 1;
            Write(ref cpu_reg_ea.v, ref cpu_m);
            cpu_flag_z = cpu_m == 0;
            cpu_flag_n = (cpu_m & 0x80) != 0;
        }

        private static void NOP__()
        {
        }

        private static void ORA__()
        {
            cpu_reg_a |= cpu_m;
            cpu_flag_n = (cpu_reg_a & 0x80) == 128;
            cpu_flag_z = cpu_reg_a == 0;
        }

        private static void PHA__()
        {
            Push(ref cpu_reg_a);
        }

        private static void PHP__()
        {
            cpu_dummy = register_pb();
            Push(ref cpu_dummy);
        }

        private static void PLA__()
        {
            Read(ref cpu_reg_sp.v, out cpu_dummy);
            Pull(out cpu_reg_a);
            cpu_flag_n = (cpu_reg_a & 0x80) == 128;
            cpu_flag_z = cpu_reg_a == 0;
        }

        private static void PLP__()
        {
            Read(ref cpu_reg_sp.v, out cpu_dummy);
            Pull(out cpu_dummy);
            register_p = cpu_dummy;
        }

        private static void RLA__()
        {
            Read(ref cpu_reg_ea.v, out cpu_byte_temp);
            Write(ref cpu_reg_ea.v, ref cpu_byte_temp);
            cpu_dummy = (byte)((uint)(cpu_byte_temp << 1) | (cpu_flag_c ? 1u : 0u));
            Write(ref cpu_reg_ea.v, ref cpu_dummy);
            cpu_flag_n = (cpu_dummy & 0x80) != 0;
            cpu_flag_z = (cpu_dummy & 0xFF) == 0;
            cpu_flag_c = (cpu_byte_temp & 0x80) != 0;
            cpu_reg_a &= cpu_dummy;
            cpu_flag_n = (cpu_reg_a & 0x80) != 0;
            cpu_flag_z = (cpu_reg_a & 0xFF) == 0;
        }

        private static void ROL_A()
        {
            cpu_byte_temp = (byte)((uint)(cpu_reg_a << 1) | (cpu_flag_c ? 1u : 0u));
            cpu_flag_n = (cpu_byte_temp & 0x80) != 0;
            cpu_flag_z = (cpu_byte_temp & 0xFF) == 0;
            cpu_flag_c = (cpu_reg_a & 0x80) != 0;
            cpu_reg_a = cpu_byte_temp;
        }

        private static void ROL_M()
        {
            Write(ref cpu_reg_ea.v, ref cpu_m);
            cpu_byte_temp = (byte)((uint)(cpu_m << 1) | (cpu_flag_c ? 1u : 0u));
            Write(ref cpu_reg_ea.v, ref cpu_byte_temp);
            cpu_flag_n = (cpu_byte_temp & 0x80) != 0;
            cpu_flag_z = (cpu_byte_temp & 0xFF) == 0;
            cpu_flag_c = (cpu_m & 0x80) != 0;
        }

        private static void ROR_A()
        {
            cpu_byte_temp = (byte)((uint)(cpu_reg_a >> 1) | (cpu_flag_c ? 128u : 0u));
            cpu_flag_n = (cpu_byte_temp & 0x80) != 0;
            cpu_flag_z = (cpu_byte_temp & 0xFF) == 0;
            cpu_flag_c = (cpu_reg_a & 1) != 0;
            cpu_reg_a = cpu_byte_temp;
        }

        private static void ROR_M()
        {
            Write(ref cpu_reg_ea.v, ref cpu_m);
            cpu_byte_temp = (byte)((uint)(cpu_m >> 1) | (cpu_flag_c ? 128u : 0u));
            Write(ref cpu_reg_ea.v, ref cpu_byte_temp);
            cpu_flag_n = (cpu_byte_temp & 0x80) != 0;
            cpu_flag_z = (cpu_byte_temp & 0xFF) == 0;
            cpu_flag_c = (cpu_m & 1) != 0;
        }

        private static void RRA__()
        {
            Read(ref cpu_reg_ea.v, out cpu_byte_temp);
            Write(ref cpu_reg_ea.v, ref cpu_byte_temp);
            cpu_dummy = (byte)((uint)(cpu_byte_temp >> 1) | (cpu_flag_c ? 128u : 0u));
            Write(ref cpu_reg_ea.v, ref cpu_dummy);
            cpu_flag_n = (cpu_dummy & 0x80) != 0;
            cpu_flag_z = (cpu_dummy & 0xFF) == 0;
            cpu_flag_c = (cpu_byte_temp & 1) != 0;
            cpu_byte_temp = cpu_dummy;
            cpu_int_temp = cpu_reg_a + cpu_byte_temp + (cpu_flag_c ? 1 : 0);
            cpu_flag_n = (cpu_int_temp & 0x80) != 0;
            cpu_flag_v = ((cpu_int_temp ^ cpu_reg_a) & (cpu_int_temp ^ cpu_byte_temp) & 0x80) != 0;
            cpu_flag_z = (cpu_int_temp & 0xFF) == 0;
            cpu_flag_c = cpu_int_temp >> 8 != 0;
            cpu_reg_a = (byte)cpu_int_temp;
        }

        private static void RTI__()
        {
            Read(ref cpu_reg_sp.v, out cpu_dummy);
            Pull(out cpu_dummy);
            register_p = cpu_dummy;
            Pull(out cpu_reg_pc.l);
            Pull(out cpu_reg_pc.h);
        }

        private static void RTS__()
        {
            Read(ref cpu_reg_sp.v, out cpu_dummy);
            Pull(out cpu_reg_pc.l);
            Pull(out cpu_reg_pc.h);
            cpu_reg_pc.v++;
            Read(ref cpu_reg_pc.v, out cpu_dummy);
        }

        private static void SAX__()
        {
            cpu_dummy = (byte)(cpu_reg_x & cpu_reg_a);
            Write(ref cpu_reg_ea.v, ref cpu_dummy);
        }

        private static void SBC__()
        {
            cpu_m ^= byte.MaxValue;
            cpu_int_temp = cpu_reg_a + cpu_m + (cpu_flag_c ? 1 : 0);
            cpu_flag_n = (cpu_int_temp & 0x80) != 0;
            cpu_flag_v = ((cpu_int_temp ^ cpu_reg_a) & (cpu_int_temp ^ cpu_m) & 0x80) != 0;
            cpu_flag_z = (cpu_int_temp & 0xFF) == 0;
            cpu_flag_c = cpu_int_temp >> 8 != 0;
            cpu_reg_a = (byte)cpu_int_temp;
        }

        private static void SEC__()
        {
            cpu_flag_c = true;
        }

        private static void SEI__()
        {
            cpu_flag_i = true;
        }

        private static void SHX__()
        {
            cpu_byte_temp = (byte)(cpu_reg_x & (cpu_reg_ea.h + 1));
            Read(ref cpu_reg_ea.v, out cpu_dummy);
            cpu_reg_ea.l += cpu_reg_y;
            if (cpu_reg_ea.l < cpu_reg_y)
            {
                cpu_reg_ea.h = cpu_byte_temp;
            }
            Write(ref cpu_reg_ea.v, ref cpu_byte_temp);
        }

        private static void SHY__()
        {
            cpu_byte_temp = (byte)(cpu_reg_y & (cpu_reg_ea.h + 1));
            Read(ref cpu_reg_ea.v, out cpu_dummy);
            cpu_reg_ea.l += cpu_reg_x;
            if (cpu_reg_ea.l < cpu_reg_x)
            {
                cpu_reg_ea.h = cpu_byte_temp;
            }
            Write(ref cpu_reg_ea.v, ref cpu_byte_temp);
        }

        private static void SLO__()
        {
            Read(ref cpu_reg_ea.v, out cpu_byte_temp);
            cpu_flag_c = (cpu_byte_temp & 0x80) != 0;
            Write(ref cpu_reg_ea.v, ref cpu_byte_temp);
            cpu_byte_temp <<= 1;
            Write(ref cpu_reg_ea.v, ref cpu_byte_temp);
            cpu_flag_n = (cpu_byte_temp & 0x80) != 0;
            cpu_flag_z = (cpu_byte_temp & 0xFF) == 0;
            cpu_reg_a |= cpu_byte_temp;
            cpu_flag_n = (cpu_reg_a & 0x80) != 0;
            cpu_flag_z = (cpu_reg_a & 0xFF) == 0;
        }

        private static void SRE__()
        {
            Read(ref cpu_reg_ea.v, out cpu_byte_temp);
            cpu_flag_c = (cpu_byte_temp & 1) != 0;
            Write(ref cpu_reg_ea.v, ref cpu_byte_temp);
            cpu_byte_temp >>= 1;
            Write(ref cpu_reg_ea.v, ref cpu_byte_temp);
            cpu_flag_n = (cpu_byte_temp & 0x80) != 0;
            cpu_flag_z = (cpu_byte_temp & 0xFF) == 0;
            cpu_reg_a ^= cpu_byte_temp;
            cpu_flag_n = (cpu_reg_a & 0x80) != 0;
            cpu_flag_z = (cpu_reg_a & 0xFF) == 0;
        }

        private static void STA__()
        {
            Write(ref cpu_reg_ea.v, ref cpu_reg_a);
        }

        private static void STX__()
        {
            Write(ref cpu_reg_ea.v, ref cpu_reg_x);
        }

        private static void STY__()
        {
            Write(ref cpu_reg_ea.v, ref cpu_reg_y);
        }

        private static void TAX__()
        {
            cpu_reg_x = cpu_reg_a;
            cpu_flag_n = (cpu_reg_x & 0x80) == 128;
            cpu_flag_z = cpu_reg_x == 0;
        }

        private static void TAY__()
        {
            cpu_reg_y = cpu_reg_a;
            cpu_flag_n = (cpu_reg_y & 0x80) == 128;
            cpu_flag_z = cpu_reg_y == 0;
        }

        private static void TSX__()
        {
            cpu_reg_x = cpu_reg_sp.l;
            cpu_flag_n = (cpu_reg_x & 0x80) != 0;
            cpu_flag_z = cpu_reg_x == 0;
        }

        private static void TXA__()
        {
            cpu_reg_a = cpu_reg_x;
            cpu_flag_n = (cpu_reg_a & 0x80) == 128;
            cpu_flag_z = cpu_reg_a == 0;
        }

        private static void TXS__()
        {
            cpu_reg_sp.l = cpu_reg_x;
        }

        private static void TYA__()
        {
            cpu_reg_a = cpu_reg_y;
            cpu_flag_n = (cpu_reg_a & 0x80) == 128;
            cpu_flag_z = cpu_reg_a == 0;
        }

        private static void XAA__()
        {
            cpu_reg_a = (byte)(cpu_reg_x & cpu_m);
            cpu_flag_n = (cpu_reg_a & 0x80) != 0;
            cpu_flag_z = (cpu_reg_a & 0xFF) == 0;
        }

        private static void XAS__()
        {
            cpu_reg_sp.l = (byte)(cpu_reg_a & cpu_reg_x);
            Write(ref cpu_reg_ea.v, ref cpu_reg_sp.l);
        }

        private static void CPUWriteState(ref BinaryWriter bin)
        {
            bin.Write(cpu_reg_pc.v);
            bin.Write(cpu_reg_sp.v);
            bin.Write(cpu_reg_ea.v);
            bin.Write(cpu_reg_a);
            bin.Write(cpu_reg_x);
            bin.Write(cpu_reg_y);
            bin.Write(cpu_flag_n);
            bin.Write(cpu_flag_v);
            bin.Write(cpu_flag_d);
            bin.Write(cpu_flag_i);
            bin.Write(cpu_flag_z);
            bin.Write(cpu_flag_c);
            bin.Write(cpu_m);
            bin.Write(cpu_opcode);
            bin.Write(cpu_byte_temp);
            bin.Write(cpu_int_temp);
            bin.Write(cpu_int_temp1);
            bin.Write(cpu_dummy);
            bin.Write(cpu_bool_tmp);
            bin.Write(temp_add.v);
            bin.Write(CPU_IRQ_PIN);
            bin.Write(CPU_NMI_PIN);
            bin.Write(cpu_suspend_nmi);
            bin.Write(cpu_suspend_irq);
        }

        private static void CPUReadState(ref BinaryReader bin)
        {
            cpu_reg_pc.v = bin.ReadUInt16();
            cpu_reg_sp.v = bin.ReadUInt16();
            cpu_reg_ea.v = bin.ReadUInt16();
            cpu_reg_a = bin.ReadByte();
            cpu_reg_x = bin.ReadByte();
            cpu_reg_y = bin.ReadByte();
            cpu_flag_n = bin.ReadBoolean();
            cpu_flag_v = bin.ReadBoolean();
            cpu_flag_d = bin.ReadBoolean();
            cpu_flag_i = bin.ReadBoolean();
            cpu_flag_z = bin.ReadBoolean();
            cpu_flag_c = bin.ReadBoolean();
            cpu_m = bin.ReadByte();
            cpu_opcode = bin.ReadByte();
            cpu_byte_temp = bin.ReadByte();
            cpu_int_temp = bin.ReadInt32();
            cpu_int_temp1 = bin.ReadInt32();
            cpu_dummy = bin.ReadByte();
            cpu_bool_tmp = bin.ReadBoolean();
            temp_add.v = bin.ReadUInt16();
            CPU_IRQ_PIN = bin.ReadBoolean();
            CPU_NMI_PIN = bin.ReadBoolean();
            cpu_suspend_nmi = bin.ReadBoolean();
            cpu_suspend_irq = bin.ReadBoolean();
        }

        private static void DMAHardReset()
        {
            dma_DMCDMAWaitCycles = 0;
            dma_OAMDMAWaitCycles = 0;
            dma_isOamDma = false;
            dma_oamdma_i = 0;
            dma_DMCOn = false;
            dma_OAMOn = false;
            dma_DMC_occurring = false;
            dma_OAM_occurring = false;
            dma_OAMFinishCounter = 0;
            dma_Oamaddress = 0;
            dma_OAMCYCLE = 0;
            dma_latch = 0;
            reg_2004 = 8196;
        }

        private static void DMASoftReset()
        {
            dma_DMCDMAWaitCycles = 0;
            dma_OAMDMAWaitCycles = 0;
            dma_isOamDma = false;
            dma_oamdma_i = 0;
            dma_DMCOn = false;
            dma_OAMOn = false;
            dma_DMC_occurring = false;
            dma_OAM_occurring = false;
            dma_OAMFinishCounter = 0;
            dma_Oamaddress = 0;
            dma_OAMCYCLE = 0;
            dma_latch = 0;
        }

        internal static void AssertDMCDMA()
        {
            if (dma_OAM_occurring)
            {
                if (dma_OAMCYCLE < 508)
                {
                    dma_DMCDMAWaitCycles = (BUS_RW ? 1 : 0);
                }
                else
                {
                    dma_DMCDMAWaitCycles = 4 - (512 - dma_OAMCYCLE);
                }
            }
            else
            {
                if (dma_DMC_occurring)
                {
                    return;
                }
                dma_DMCDMAWaitCycles = (BUS_RW ? 3 : 2);
                if (dma_OAMFinishCounter == 3)
                {
                    dma_DMCDMAWaitCycles++;
                }
            }
            dma_isOamDma = false;
            dma_DMCOn = true;
        }

        private static void AssertOAMDMA()
        {
            if (!dma_OAM_occurring)
            {
                dma_OAMDMAWaitCycles = (apu_odd_cycle ? 1 : 2);
                dma_isOamDma = true;
                dma_OAMOn = true;
            }
        }

        private static void DMAClock()
        {
            if (dma_OAMFinishCounter > 0)
            {
                dma_OAMFinishCounter--;
            }
            if (!BUS_RW)
            {
                if (dma_DMCDMAWaitCycles > 0)
                {
                    dma_DMCDMAWaitCycles--;
                }
                if (dma_OAMDMAWaitCycles > 0)
                {
                    dma_OAMDMAWaitCycles--;
                }
                return;
            }
            if (dma_DMCOn)
            {
                dma_DMC_occurring = true;
                dma_DMCOn = false;
                if (dma_DMCDMAWaitCycles > 0)
                {
                    if (BUS_ADDRESS == 16406 || BUS_ADDRESS == 16407)
                    {
                        Read(ref BUS_ADDRESS, out dma_dummy);
                        dma_DMCDMAWaitCycles--;
                        while (dma_DMCDMAWaitCycles > 0)
                        {
                            EmuClockComponents();
                            dma_DMCDMAWaitCycles--;
                        }
                    }
                    else
                    {
                        if (dma_DMCDMAWaitCycles > 0)
                        {
                            EmuClockComponents();
                            dma_DMCDMAWaitCycles--;
                        }
                        while (dma_DMCDMAWaitCycles > 0)
                        {
                            Read(ref BUS_ADDRESS, out dma_dummy);
                            dma_DMCDMAWaitCycles--;
                        }
                    }
                }
                DMCDoDMA();
                dma_DMC_occurring = false;
            }
            if (!dma_OAMOn)
            {
                return;
            }
            dma_OAM_occurring = true;
            dma_OAMOn = false;
            if (dma_OAMDMAWaitCycles > 0)
            {
                if (BUS_ADDRESS == 16406 || BUS_ADDRESS == 16407)
                {
                    Read(ref BUS_ADDRESS, out dma_dummy);
                    dma_OAMDMAWaitCycles--;
                    while (dma_OAMDMAWaitCycles > 0)
                    {
                        EmuClockComponents();
                        dma_OAMDMAWaitCycles--;
                    }
                }
                else
                {
                    if (dma_OAMDMAWaitCycles > 0)
                    {
                        EmuClockComponents();
                        dma_OAMDMAWaitCycles--;
                    }
                    while (dma_OAMDMAWaitCycles > 0)
                    {
                        Read(ref BUS_ADDRESS, out dma_dummy);
                        dma_OAMDMAWaitCycles--;
                    }
                }
            }
            dma_OAMCYCLE = 0;
            for (dma_oamdma_i = 0; dma_oamdma_i < 256; dma_oamdma_i++)
            {
                Read(ref dma_Oamaddress, out dma_latch);
                dma_OAMCYCLE++;
                Write(ref reg_2004, ref dma_latch);
                dma_OAMCYCLE++;
                dma_Oamaddress = (ushort)(++dma_Oamaddress & 0xFFFFu);
            }
            dma_OAMCYCLE = 0;
            dma_OAMFinishCounter = 5;
            dma_OAM_occurring = false;
        }

        private static void DMAWriteState(ref BinaryWriter bin)
        {
            bin.Write(dma_DMCDMAWaitCycles);
            bin.Write(dma_OAMDMAWaitCycles);
            bin.Write(dma_isOamDma);
            bin.Write(dma_oamdma_i);
            bin.Write(dma_DMCOn);
            bin.Write(dma_OAMOn);
            bin.Write(dma_DMC_occurring);
            bin.Write(dma_OAM_occurring);
            bin.Write(dma_OAMFinishCounter);
            bin.Write(dma_Oamaddress);
            bin.Write(dma_OAMCYCLE);
            bin.Write(dma_latch);
            bin.Write(dma_dummy);
        }

        private static void DMAReadState(ref BinaryReader bin)
        {
            dma_DMCDMAWaitCycles = bin.ReadInt32();
            dma_OAMDMAWaitCycles = bin.ReadInt32();
            dma_isOamDma = bin.ReadBoolean();
            dma_oamdma_i = bin.ReadInt32();
            dma_DMCOn = bin.ReadBoolean();
            dma_OAMOn = bin.ReadBoolean();
            dma_DMC_occurring = bin.ReadBoolean();
            dma_OAM_occurring = bin.ReadBoolean();
            dma_OAMFinishCounter = bin.ReadInt32();
            dma_Oamaddress = bin.ReadUInt16();
            dma_OAMCYCLE = bin.ReadInt32();
            dma_latch = bin.ReadByte();
            dma_dummy = bin.ReadByte();
        }

        private static void PollInterruptStatus()
        {
            if (!cpu_suspend_nmi)
            {
                if (PPU_NMI_Current & !PPU_NMI_Old)
                {
                    CPU_NMI_PIN = true;
                }
                PPU_NMI_Old = (PPU_NMI_Current = false);
            }
            if (!cpu_suspend_irq)
            {
                CPU_IRQ_PIN = !cpu_flag_i && IRQFlags != 0;
            }
            if (CPU_NMI_PIN)
            {
                InterruptVector = 65530;
            }
            else
            {
                InterruptVector = 65534;
            }
        }

        private static void InterruptsWriteState(ref BinaryWriter bin)
        {
            bin.Write(IRQFlags);
            bin.Write(PPU_NMI_Current);
            bin.Write(PPU_NMI_Old);
            bin.Write(InterruptVector);
        }

        private static void InterruptsReadState(ref BinaryReader bin)
        {
            IRQFlags = bin.ReadInt32();
            PPU_NMI_Current = bin.ReadBoolean();
            PPU_NMI_Old = bin.ReadBoolean();
            InterruptVector = bin.ReadUInt16();
        }

        public static void SetupGameGenie(bool IsGameGenieActive, GameGenieCode[] GameGenieCodes)
        {
            if (mem_board != null)
            {
                mem_board.SetupGameGenie(IsGameGenieActive, GameGenieCodes);
            }
        }

        private static void MEMInitialize(IRom rom)
        {
            Tracer.WriteLine("Looking for mapper # " + rom.MapperNumber + "....");
            if (MyNesMain.IsBoardExist(rom.MapperNumber))
            {
                Tracer.WriteLine("Mapper # " + rom.MapperNumber + " located, assigning...");
                mem_board = MyNesMain.GetBoard(rom.MapperNumber);
                Tracer.WriteInformation("Mapper # " + rom.MapperNumber + " assigned successfully.");
                if (mem_board.HasIssues)
                {
                    Tracer.WriteWarning(MNInterfaceLanguage.Mapper + " # " + mem_board.MapperNumber + " [" + mem_board.Name + "] " + MNInterfaceLanguage.Message_Error17);
                    MyNesMain.VideoProvider.WriteWarningNotification(MNInterfaceLanguage.Mapper + " # " + mem_board.MapperNumber + " [" + mem_board.Name + "] " + MNInterfaceLanguage.Message_Error17, instant: false);
                }
            }
            else
            {
                Tracer.WriteError("Mapper # " + rom.MapperNumber + " IS NOT LOCATED, mapper is not supported or unable to find it.");
                MyNesMain.VideoProvider.WriteErrorNotification(MNInterfaceLanguage.Mapper + " # " + rom.MapperNumber + " " + MNInterfaceLanguage.Message_Error14, instant: false);
                mem_board = MyNesMain.GetBoard(0);
                Tracer.WriteWarning("Mapper # 0 [NROM] will be used instead, assigned successfully.");
                MyNesMain.VideoProvider.WriteErrorNotification(MNInterfaceLanguage.Mapper + " # 0 [NROM] " + MNInterfaceLanguage.Message_Error15, instant: false);
            }
            mem_read_accesses = new MemReadAccess[65536];
            mem_write_accesses = new MemWriteAccess[65536];
            MEMMap(MEMReadWRAM, new ushort[2] { 0, 4096 });
            MEMMap(MEMWriteWRAM, new ushort[2] { 0, 4096 });
            MEMMap(PPUIORead, new ushort[2] { 8192, 12288 });
            MEMMap(PPUIOWrite, new ushort[2] { 8192, 12288 });
            MEMMap(APUIORead, new ushort[1] { 16384 });
            MEMMap(APUIOWrite, new ushort[1] { 16384 });
            MEMMap(mem_board.ReadEX, new ushort[1] { 20480 });
            MEMMap(mem_board.WriteEX, new ushort[1] { 20480 });
            MEMMap(mem_board.ReadSRM, new ushort[2] { 24576, 28672 });
            MEMMap(mem_board.WriteSRM, new ushort[2] { 24576, 28672 });
            MEMMap(mem_board.ReadPRG, new ushort[8] { 32768, 36864, 40960, 45056, 49152, 53248, 57344, 61440 });
            MEMMap(mem_board.WritePRG, new ushort[8] { 32768, 36864, 40960, 45056, 49152, 53248, 57344, 61440 });
            mem_board.Initialize(rom);
            mem_wram = new byte[2048];
        }

        private static void MEMHardReset()
        {
            mem_wram = new byte[2048];
            mem_wram[8] = 247;
            mem_wram[9] = 239;
            mem_wram[10] = 223;
            mem_wram[15] = 191;
            Tracer.WriteLine("Reading SRAM ...");
            SRAMFileName = Path.Combine(MyNesMain.EmuSettings.SRAMFolder, Path.GetFileNameWithoutExtension(CurrentFilePath) + ".srm");
            if (File.Exists(SRAMFileName))
            {
                FileStream fileStream = new FileStream(SRAMFileName, FileMode.Open, FileAccess.Read);
                byte[] array = new byte[fileStream.Length];
                fileStream.Read(array, 0, array.Length);
                fileStream.Flush();
                fileStream.Close();
                byte[] outData = new byte[0];
                ZlipWrapper.DecompressData(array, out outData);
                mem_board.LoadSRAM(outData);
                Tracer.WriteLine("SRAM read successfully.");
            }
            else
            {
                Tracer.WriteLine("SRAM file not found; rom has no SRAM or file not exist.");
            }
            ReloadGameGenieCodes();
            mem_board.HardReset();
        }

        public static void ReloadGameGenieCodes()
        {
            Tracer.WriteLine("Reading game genie codes (if available)....");
            GMFileName = Path.Combine(MyNesMain.EmuSettings.GameGenieFolder, Path.GetFileNameWithoutExtension(CurrentFilePath) + ".txt");
            mem_board.GameGenieCodes = new GameGenieCode[0];
            if (File.Exists(GMFileName))
            {
                XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
                xmlReaderSettings.DtdProcessing = DtdProcessing.Ignore;
                xmlReaderSettings.IgnoreWhitespace = true;
                XmlReader xmlReader = XmlReader.Create(GMFileName, xmlReaderSettings);
                xmlReader.Read();
                xmlReader.Read();
                if (xmlReader.Name != "MyNesGameGenieCodesList")
                {
                    xmlReader.Close();
                    return;
                }
                GameGenie gameGenie = new GameGenie();
                List<GameGenieCode> list = new List<GameGenieCode>();
                while (xmlReader.Read())
                {
                    if (xmlReader.Name == "Code")
                    {
                        GameGenieCode item = default(GameGenieCode);
                        item.Enabled = true;
                        xmlReader.MoveToAttribute("code");
                        item.Name = xmlReader.Value.ToString();
                        if (item.Name.Length == 6)
                        {
                            item.Address = gameGenie.GetGGAddress(gameGenie.GetCodeAsHEX(item.Name), 6) | 0x8000;
                            item.Value = gameGenie.GetGGValue(gameGenie.GetCodeAsHEX(item.Name), 6);
                            item.IsCompare = false;
                        }
                        else
                        {
                            item.Address = gameGenie.GetGGAddress(gameGenie.GetCodeAsHEX(item.Name), 8) | 0x8000;
                            item.Value = gameGenie.GetGGValue(gameGenie.GetCodeAsHEX(item.Name), 8);
                            item.Compare = gameGenie.GetGGCompareValue(gameGenie.GetCodeAsHEX(item.Name));
                            item.IsCompare = true;
                        }
                        list.Add(item);
                    }
                }
                xmlReader.Close();
                if (list.Count > 0)
                {
                    mem_board.GameGenieCodes = list.ToArray();
                    Tracer.WriteInformation("Game Genie codes loaded successfully, total of " + list.Count);
                }
                else
                {
                    Tracer.WriteError("There is no Game Genie code in the file to load.");
                }
            }
            else
            {
                Tracer.WriteWarning("No Game Genie file found for this game.");
            }
        }

        private static void MEMMap(MemReadAccess readAccess, ushort[] addresses)
        {
            for (int i = 0; i < addresses.Length; i++)
            {
                mem_read_accesses[(addresses[i] & 0xF000) >> 12] = readAccess;
            }
        }

        private static void MEMMap(MemWriteAccess writeAccess, ushort[] addresses)
        {
            for (int i = 0; i < addresses.Length; i++)
            {
                mem_write_accesses[(addresses[i] & 0xF000) >> 12] = writeAccess;
            }
        }

        private static void MEMReadWRAM(ref ushort addr, out byte value)
        {
            value = mem_wram[addr & 0x7FF];
        }

        private static void MEMWriteWRAM(ref ushort addr, ref byte value)
        {
            mem_wram[addr & 0x7FF] = value;
        }

        internal static void Read(ref ushort addr, out byte value)
        {
            BUS_RW = true;
            BUS_ADDRESS = addr;
            EmuClockComponents();
            mem_read_accesses[(addr & 0xF000) >> 12](ref addr, out value);
        }

        private static void Write(ref ushort addr, ref byte value)
        {
            BUS_RW = false;
            BUS_ADDRESS = addr;
            EmuClockComponents();
            mem_write_accesses[(addr & 0xF000) >> 12](ref addr, ref value);
        }

        internal static void SaveSRAM()
        {
            if (mem_board != null && MyNesMain.EmuSettings.SaveSRAMAtEmuShutdown && mem_board.SRAMSaveRequired)
            {
                Tracer.WriteLine("Saving SRAM ...");
                byte[] outData = new byte[0];
                ZlipWrapper.CompressData(mem_board.GetSRAMBuffer(), out outData);
                FileStream fileStream = new FileStream(SRAMFileName, FileMode.Create, FileAccess.Write);
                fileStream.Write(outData, 0, outData.Length);
                fileStream.Flush();
                fileStream.Close();
                Tracer.WriteLine("SRAM saved successfully.");
            }
        }

        private static void MEMWriteState(ref BinaryWriter bin)
        {
            mem_board.WriteStateData(ref bin);
            bin.Write(mem_wram);
            bin.Write(BUS_RW);
            bin.Write(BUS_ADDRESS);
        }

        private static void MEMReadState(ref BinaryReader bin)
        {
            mem_board.ReadStateData(ref bin);
            bin.Read(mem_wram, 0, mem_wram.Length);
            BUS_RW = bin.ReadBoolean();
            BUS_ADDRESS = bin.ReadUInt16();
        }

        private static void PORTSInitialize()
        {
            if (joypad1 == null)
            {
                joypad1 = new BlankJoypad();
            }
            if (joypad2 == null)
            {
                joypad2 = new BlankJoypad();
            }
            if (joypad3 == null)
            {
                joypad3 = new BlankJoypad();
            }
            if (joypad4 == null)
            {
                joypad4 = new BlankJoypad();
            }
        }

        public static void SetupControllers(IJoypadConnecter joy1, IJoypadConnecter joy2, IJoypadConnecter joy3, IJoypadConnecter joy4)
        {
            joypad1 = joy1;
            joypad2 = joy2;
            joypad3 = joy3;
            joypad4 = joy4;
        }

        public static void SetupVSUnisystemDIP(IVSUnisystemDIPConnecter uni)
        {
        }

        public static void SetupControllersP1(IJoypadConnecter joy)
        {
            joypad1 = joy;
        }

        public static void SetupControllersP2(IJoypadConnecter joy)
        {
            joypad2 = joy;
        }

        public static void SetupControllersP3(IJoypadConnecter joy)
        {
            joypad3 = joy;
        }

        public static void SetupControllersP4(IJoypadConnecter joy)
        {
            joypad4 = joy;
        }

        public static void DestroyJoypads()
        {
            if (joypad1 == null)
            {
                joypad1 = new BlankJoypad();
            }
            else
            {
                joypad1.Destroy();
            }
            if (joypad2 == null)
            {
                joypad2 = new BlankJoypad();
            }
            else
            {
                joypad1.Destroy();
            }
            if (joypad3 == null)
            {
                joypad3 = new BlankJoypad();
            }
            else
            {
                joypad1.Destroy();
            }
            if (joypad4 == null)
            {
                joypad4 = new BlankJoypad();
            }
            else
            {
                joypad1.Destroy();
            }
        }

        private static void PORTWriteState(ref BinaryWriter bin)
        {
            bin.Write(PORT0);
            bin.Write(PORT1);
            bin.Write(inputStrobe);
        }

        private static void PORTReadState(ref BinaryReader bin)
        {
            PORT0 = bin.ReadInt32();
            PORT1 = bin.ReadInt32();
            inputStrobe = bin.ReadInt32();
        }

        public static void SetupPalette(int[] pal)
        {
            ppu_palette = pal;
        }

        private static void PPUInitialize()
        {
            ppu_reg_update_func = new Action[8] { PPUOnRegister2000, PPUOnRegister2001, PPUOnRegister2002, PPUOnRegister2003, PPUOnRegister2004, PPUOnRegister2005, PPUOnRegister2006, PPUOnRegister2007 };
            ppu_reg_read_func = new Action[8] { PPURead2000, PPURead2001, PPURead2002, PPURead2003, PPURead2004, PPURead2005, PPURead2006, PPURead2007 };
            ppu_bkg_fetches = new Action[8] { PPUBKFetch0, PPUBKFetch1, PPUBKFetch2, PPUBKFetch3, PPUBKFetch4, PPUBKFetch5, PPUBKFetch6, PPUBKFetch7 };
            ppu_spr_fetches = new Action[8] { PPUBKFetch0, PPUBKFetch1, PPUBKFetch2, PPUBKFetch3, PPUSPRFetch0, PPUSPRFetch1, PPUSPRFetch2, PPUSPRFetch3 };
            ppu_oam_phases = new Action[9] { PPUOamPhase0, PPUOamPhase1, PPUOamPhase2, PPUOamPhase3, PPUOamPhase4, PPUOamPhase5, PPUOamPhase6, PPUOamPhase7, PPUOamPhase8 };
            ppu_h_clocks = new Action[341];
            ppu_h_clocks[0] = PPUHClock_000_Idle;
            for (int i = 1; i < 257; i++)
            {
                ppu_h_clocks[i] = PPUHClock_1_256_BKGClocks;
            }
            for (int j = 257; j < 321; j++)
            {
                ppu_h_clocks[j] = PPUHClock_257_320_SPRClocks;
            }
            for (int k = 321; k < 337; k++)
            {
                ppu_h_clocks[k] = PPUHClock_321_336_DUMClocks;
            }
            for (int l = 337; l < 341; l++)
            {
                ppu_h_clocks[l] = PPUHClock_337_340_DUMClocks;
            }
            ppu_v_clocks = new Action[320];
            for (int m = 0; m < 240; m++)
            {
                ppu_v_clocks[m] = PPUScanlineRender;
            }
            ppu_v_clocks[240] = PPUScanlineVBLANK;
            ppu_oam_bank = new byte[256];
            ppu_oam_bank_secondary = new byte[32];
            ppu_palette_bank = new byte[32];
            ppu_bkg_pixels = new int[512];
            ppu_spr_pixels = new int[512];
            ppu_screen_pixels = new int[61440];
            ppu_palette = NTSCPaletteGenerator.GeneratePalette();
        }

        private static void PPUHardReset()
        {
            ppu_reg_2001_grayscale = 243;
            switch (Region)
            {
                case EmuRegion.NTSC:
                    ppu_clock_vblank_start = 241;
                    ppu_clock_vblank_end = 261;
                    ppu_use_odd_cycle = true;
                    break;
                case EmuRegion.PALB:
                    ppu_clock_vblank_start = 241;
                    ppu_clock_vblank_end = 311;
                    ppu_use_odd_cycle = false;
                    break;
                case EmuRegion.DENDY:
                    {
                        ppu_clock_vblank_start = 291;
                        ppu_clock_vblank_end = 311;
                        for (int i = 241; i <= 290; i++)
                        {
                            ppu_v_clocks[i] = PPUScanlineVBLANK;
                        }
                        ppu_use_odd_cycle = false;
                        break;
                    }
            }
            ppu_v_clocks[ppu_clock_vblank_start] = PPUScanlineVBLANKStart;
            for (int j = ppu_clock_vblank_start + 1; j <= ppu_clock_vblank_end - 1; j++)
            {
                ppu_v_clocks[j] = PPUScanlineVBLANK;
            }
            ppu_v_clocks[ppu_clock_vblank_end] = PPUScanlineVBLANKEnd;
            ppu_oam_bank = new byte[256];
            ppu_oam_bank_secondary = new byte[32];
            PPUOamReset();
            ppu_palette_bank = new byte[32]
            {
                9, 1, 0, 1, 0, 2, 2, 13, 8, 16,
                8, 36, 0, 0, 4, 44, 9, 1, 52, 3,
                0, 4, 0, 20, 8, 58, 0, 2, 0, 32,
                44, 8
            };
            ppu_reg_io_db = 0;
            ppu_reg_io_addr = 0;
            ppu_reg_access_happened = false;
            ppu_reg_access_w = false;
            ppu_reg_2000_vram_address_increament = 1;
            ppu_reg_2000_sprite_pattern_table_address_for_8x8_sprites = 0;
            ppu_reg_2000_background_pattern_table_address = 0;
            ppu_reg_2000_Sprite_size = 0;
            ppu_reg_2000_VBI = false;
            ppu_reg_2001_show_background_in_leftmost_8_pixels_of_screen = false;
            ppu_reg_2001_show_sprites_in_leftmost_8_pixels_of_screen = false;
            ppu_reg_2001_show_background = false;
            ppu_reg_2001_show_sprites = false;
            ppu_reg_2001_grayscale = 63;
            ppu_reg_2001_emphasis = 0;
            ppu_reg_2002_SpriteOverflow = false;
            ppu_reg_2002_Sprite0Hit = false;
            ppu_reg_2002_VblankStartedFlag = false;
            ppu_reg_2003_oam_addr = 0;
            ppu_is_sprfetch = false;
            ppu_use_odd_swap = false;
            ppu_clock_h = 0;
            ppu_clock_v = 0;
        }

        private static void PPUClock()
        {
            mem_board.OnPPUClock();
            ppu_v_clocks[ppu_clock_v]();
            ppu_clock_h++;
            if (ppu_clock_h >= 341)
            {
                mem_board.OnPPUScanlineTick();
                if (ppu_clock_v == ppu_clock_vblank_end)
                {
                    ppu_clock_v = 0;
                    ppu_frame_finished = true;
                }
                else
                {
                    ppu_clock_v++;
                }
                ppu_clock_h -= 341;
            }
            if (ppu_reg_access_happened)
            {
                ppu_reg_access_happened = false;
                ppu_reg_update_func[ppu_reg_io_addr]();
            }
        }

        public static int GetPixel(int x, int y)
        {
            return ppu_screen_pixels[y * 256 + x];
        }

        private static void PPUScanlineRender()
        {
            ppu_h_clocks[ppu_clock_h]();
        }

        private static void PPUScanlineVBLANKStart()
        {
            ppu_is_nmi_time = (ppu_clock_h >= 1) & (ppu_clock_h <= 3);
            if (ppu_is_nmi_time)
            {
                if (ppu_clock_h == 1)
                {
                    ppu_reg_2002_VblankStartedFlag = true;
                }
                PPU_NMI_Current = ppu_reg_2002_VblankStartedFlag & ppu_reg_2000_VBI;
            }
        }

        private static void PPUScanlineVBLANKEnd()
        {
            ppu_is_nmi_time = (ppu_clock_h >= 1) & (ppu_clock_h <= 3);
            if (ppu_clock_h == 1)
            {
                ppu_reg_2002_Sprite0Hit = false;
                ppu_reg_2002_VblankStartedFlag = false;
                ppu_reg_2002_SpriteOverflow = false;
            }
            PPUScanlineRender();
            if (ppu_use_odd_cycle && ppu_clock_h == 339)
            {
                ppu_use_odd_swap = !ppu_use_odd_swap;
                if (!ppu_use_odd_swap & (ppu_reg_2001_show_background || ppu_reg_2001_show_sprites))
                {
                    ppu_odd_swap_done = true;
                    ppu_clock_h++;
                }
            }
        }

        private static void PPUScanlineVBLANK()
        {
        }

        private static void PPUHClock_000_Idle()
        {
            if (ppu_odd_swap_done)
            {
                ppu_bkg_fetches[1]();
                ppu_odd_swap_done = false;
            }
        }

        private static void PPUHClock_1_256_BKGClocks()
        {
            if (ppu_reg_2001_show_background || ppu_reg_2001_show_sprites)
            {
                if (ppu_clock_v != ppu_clock_vblank_end)
                {
                    if (ppu_clock_h > 0 && ppu_clock_h < 65)
                    {
                        ppu_oam_bank_secondary[(ppu_clock_h - 1) & 0x1F] = byte.MaxValue;
                    }
                    else
                    {
                        if (ppu_clock_h == 65)
                        {
                            PPUOamReset();
                        }
                        if (((ppu_clock_h - 1) & 1) == 0)
                        {
                            PPUOamEvFetch();
                        }
                        else
                        {
                            ppu_oam_phases[ppu_phase_index]();
                        }
                        if (ppu_clock_h == 256)
                        {
                            PPUOamClear();
                        }
                    }
                }
                ppu_bkg_fetches[(ppu_clock_h - 1) & 7]();
                if (ppu_clock_v < 240)
                {
                    RenderPixel();
                }
            }
            else
            {
                if (ppu_clock_v >= 240)
                {
                    return;
                }
                if ((ppu_vram_addr & 0x3F00) == 16128)
                {
                    if ((ppu_vram_addr & 3) == 0)
                    {
                        ppu_screen_pixels[ppu_clock_h - 1 + ppu_clock_v * 256] = ppu_palette[(ppu_palette_bank[ppu_vram_addr & 0xC] & ppu_reg_2001_grayscale) | ppu_reg_2001_emphasis];
                    }
                    else
                    {
                        ppu_screen_pixels[ppu_clock_h - 1 + ppu_clock_v * 256] = ppu_palette[(ppu_palette_bank[ppu_vram_addr & 0x1F] & ppu_reg_2001_grayscale) | ppu_reg_2001_emphasis];
                    }
                }
                else
                {
                    ppu_screen_pixels[ppu_clock_h - 1 + ppu_clock_v * 256] = ppu_palette[(ppu_palette_bank[0] & ppu_reg_2001_grayscale) | ppu_reg_2001_emphasis];
                }
            }
        }

        private static void PPUHClock_257_320_SPRClocks()
        {
            if (ppu_reg_2001_show_background || ppu_reg_2001_show_sprites)
            {
                ppu_spr_fetches[(ppu_clock_h - 1) & 7]();
                if (ppu_clock_h == 257)
                {
                    ppu_vram_addr = (ushort)((ppu_vram_addr & 0x7BE0u) | (ppu_vram_addr_temp & 0x41Fu));
                }
                if (ppu_clock_v == ppu_clock_vblank_end && ppu_clock_h >= 280 && ppu_clock_h <= 304)
                {
                    ppu_vram_addr = (ushort)((ppu_vram_addr & 0x41Fu) | (ppu_vram_addr_temp & 0x7BE0u));
                }
            }
        }

        private static void PPUHClock_321_336_DUMClocks()
        {
            if (ppu_reg_2001_show_background || ppu_reg_2001_show_sprites)
            {
                ppu_bkg_fetches[(ppu_clock_h - 1) & 7]();
            }
        }

        private static void PPUHClock_337_340_DUMClocks()
        {
            if (ppu_reg_2001_show_background || ppu_reg_2001_show_sprites)
            {
                ppu_bkg_fetches[(ppu_clock_h - 1) & 1]();
            }
        }

        private static void PPUBKFetch0()
        {
            ppu_bkgfetch_nt_addr = (ushort)(0x2000u | (ppu_vram_addr & 0xFFFu));
            mem_board.OnPPUAddressUpdate(ref ppu_bkgfetch_nt_addr);
        }

        private static void PPUBKFetch1()
        {
            mem_board.ReadNMT(ref ppu_bkgfetch_nt_addr, out ppu_bkgfetch_nt_data);
        }

        private static void PPUBKFetch2()
        {
            ppu_bkgfetch_at_addr = (ushort)(0x23C0u | (ppu_vram_addr & 0xC00u) | ((uint)(ppu_vram_addr >> 4) & 0x38u) | ((uint)(ppu_vram_addr >> 2) & 7u));
            mem_board.OnPPUAddressUpdate(ref ppu_bkgfetch_at_addr);
        }

        private static void PPUBKFetch3()
        {
            mem_board.ReadNMT(ref ppu_bkgfetch_at_addr, out ppu_bkgfetch_at_data);
            ppu_bkgfetch_at_data = (byte)(ppu_bkgfetch_at_data >> (((ppu_vram_addr >> 4) & 4) | (ppu_vram_addr & 2)));
        }

        private static void PPUBKFetch4()
        {
            ppu_bkgfetch_lb_addr = (ushort)((uint)(ppu_reg_2000_background_pattern_table_address | (ppu_bkgfetch_nt_data << 4)) | ((uint)(ppu_vram_addr >> 12) & 7u));
            mem_board.OnPPUAddressUpdate(ref ppu_bkgfetch_lb_addr);
        }

        private static void PPUBKFetch5()
        {
            mem_board.ReadCHR(ref ppu_bkgfetch_lb_addr, out ppu_bkgfetch_lb_data);
        }

        private static void PPUBKFetch6()
        {
            ppu_bkgfetch_hb_addr = (ushort)((uint)(ppu_reg_2000_background_pattern_table_address | (ppu_bkgfetch_nt_data << 4)) | 8u | ((uint)(ppu_vram_addr >> 12) & 7u));
            mem_board.OnPPUAddressUpdate(ref ppu_bkgfetch_hb_addr);
        }

        private static void PPUBKFetch7()
        {
            mem_board.ReadCHR(ref ppu_bkgfetch_hb_addr, out ppu_bkgfetch_hb_data);
            ppu_bkg_render_pos = ppu_clock_h + 8;
            ppu_bkg_render_pos %= 336;
            if (ppu_clock_h == 256)
            {
                if ((ppu_vram_addr & 0x7000) != 28672)
                {
                    ppu_vram_addr += 4096;
                }
                else
                {
                    ppu_vram_addr ^= 28672;
                    switch (ppu_vram_addr & 0x3E0)
                    {
                        case 928:
                            ppu_vram_addr ^= 2976;
                            break;
                        case 992:
                            ppu_vram_addr ^= 992;
                            break;
                        default:
                            ppu_vram_addr += 32;
                            break;
                    }
                }
            }
            else if ((ppu_vram_addr & 0x1F) == 31)
            {
                ppu_vram_addr ^= 1055;
            }
            else
            {
                ppu_vram_addr++;
            }
            for (ppu_bkg_render_i = 0; ppu_bkg_render_i < 8; ppu_bkg_render_i++)
            {
                ppu_bkg_render_tmp_val = ((ppu_bkgfetch_at_data << 2) & 0xC) | ((ppu_bkgfetch_lb_data >> 7) & 1) | ((ppu_bkgfetch_hb_data >> 6) & 2);
                ppu_bkg_pixels[ppu_bkg_render_i + ppu_bkg_render_pos] = ppu_bkg_render_tmp_val;
                ppu_bkgfetch_lb_data <<= 1;
                ppu_bkgfetch_hb_data <<= 1;
            }
        }

        private static void PPUSPRFetch0()
        {
            ppu_sprfetch_slot = (ppu_clock_h - 1 >> 3) & 7;
            ppu_sprfetch_slot = 7 - ppu_sprfetch_slot;
            ppu_sprfetch_y_data = ppu_oam_bank_secondary[ppu_sprfetch_slot * 4];
            ppu_sprfetch_t_data = ppu_oam_bank_secondary[ppu_sprfetch_slot * 4 + 1];
            ppu_sprfetch_at_data = ppu_oam_bank_secondary[ppu_sprfetch_slot * 4 + 2];
            ppu_sprfetch_x_data = ppu_oam_bank_secondary[ppu_sprfetch_slot * 4 + 3];
            ppu_temp_comparator = (ppu_clock_v - ppu_sprfetch_y_data) ^ (((ppu_sprfetch_at_data & 0x80u) != 0) ? 15 : 0);
            if (ppu_reg_2000_Sprite_size == 16)
            {
                ppu_sprfetch_lb_addr = (ushort)(((uint)(ppu_sprfetch_t_data << 12) & 0x1000u) | ((uint)(ppu_sprfetch_t_data << 4) & 0xFE0u) | ((uint)(ppu_temp_comparator << 1) & 0x10u) | ((uint)ppu_temp_comparator & 7u));
            }
            else
            {
                ppu_sprfetch_lb_addr = (ushort)((uint)(ppu_reg_2000_sprite_pattern_table_address_for_8x8_sprites | (ppu_sprfetch_t_data << 4)) | ((uint)ppu_temp_comparator & 7u));
            }
            mem_board.OnPPUAddressUpdate(ref ppu_sprfetch_lb_addr);
        }

        private static void PPUSPRFetch1()
        {
            ppu_is_sprfetch = true;
            mem_board.ReadCHR(ref ppu_sprfetch_lb_addr, out ppu_sprfetch_lb_data);
            ppu_is_sprfetch = false;
            if ((ppu_sprfetch_at_data & 0x40u) != 0)
            {
                ppu_sprfetch_lb_data = reverseLookup[ppu_sprfetch_lb_data];
            }
        }

        private static void PPUSPRFetch2()
        {
            ppu_sprfetch_hb_addr = (ushort)(ppu_sprfetch_lb_addr | 8u);
            mem_board.OnPPUAddressUpdate(ref ppu_sprfetch_hb_addr);
        }

        private static void PPUSPRFetch3()
        {
            ppu_is_sprfetch = true;
            mem_board.ReadCHR(ref ppu_sprfetch_hb_addr, out ppu_sprfetch_hb_data);
            ppu_is_sprfetch = false;
            if ((ppu_sprfetch_at_data & 0x40u) != 0)
            {
                ppu_sprfetch_hb_data = reverseLookup[ppu_sprfetch_hb_data];
            }
            if (ppu_sprfetch_x_data == byte.MaxValue)
            {
                return;
            }
            for (ppu_bkg_render_i = 0; ppu_bkg_render_i < 8; ppu_bkg_render_i++)
            {
                if (ppu_sprfetch_x_data < byte.MaxValue)
                {
                    ppu_bkg_render_tmp_val = ((ppu_sprfetch_at_data << 2) & 0xC) | ((ppu_sprfetch_lb_data >> 7) & 1) | ((ppu_sprfetch_hb_data >> 6) & 2);
                    if (((uint)ppu_bkg_render_tmp_val & 3u) != 0)
                    {
                        ppu_spr_pixels[ppu_sprfetch_x_data] = ppu_bkg_render_tmp_val;
                        if (ppu_sprfetch_slot == 0 && ppu_sprite0_should_hit)
                        {
                            ppu_spr_pixels[ppu_sprfetch_x_data] |= 16384;
                        }
                        if ((ppu_sprfetch_at_data & 0x20) == 0)
                        {
                            ppu_spr_pixels[ppu_sprfetch_x_data] |= 32768;
                        }
                    }
                    ppu_sprfetch_lb_data <<= 1;
                    ppu_sprfetch_hb_data <<= 1;
                    ppu_sprfetch_x_data++;
                }
            }
        }

        private static void PPUOamReset()
        {
            ppu_oamev_n = 0;
            ppu_oamev_m = 0;
            ppu_oamev_slot = 0;
            ppu_phase_index = 0;
            ppu_sprite0_should_hit = false;
        }

        private static void PPUOamClear()
        {
            for (int i = 0; i < ppu_spr_pixels.Length; i++)
            {
                ppu_spr_pixels[i] = 0;
            }
        }

        private static void PPUOamEvFetch()
        {
            ppu_fetch_data = ppu_oam_bank[ppu_oamev_n * 4 + ppu_oamev_m];
        }

        private static void PPUOamPhase0()
        {
            ppu_oamev_compare = ppu_clock_v >= ppu_fetch_data && ppu_clock_v < ppu_fetch_data + ppu_reg_2000_Sprite_size;
            if (ppu_oamev_compare)
            {
                ppu_oam_bank_secondary[ppu_oamev_slot * 4] = ppu_fetch_data;
                ppu_oamev_m = 1;
                ppu_phase_index++;
                if (ppu_oamev_n == 0)
                {
                    ppu_sprite0_should_hit = true;
                }
            }
            else
            {
                ppu_oamev_m = 0;
                ppu_oamev_n++;
                if (ppu_oamev_n == 64)
                {
                    ppu_oamev_n = 0;
                    ppu_phase_index = 8;
                }
            }
        }

        private static void PPUOamPhase1()
        {
            ppu_oam_bank_secondary[ppu_oamev_slot * 4 + ppu_oamev_m] = ppu_fetch_data;
            ppu_oamev_m = 2;
            ppu_phase_index++;
        }

        private static void PPUOamPhase2()
        {
            ppu_oam_bank_secondary[ppu_oamev_slot * 4 + ppu_oamev_m] = ppu_fetch_data;
            ppu_oamev_m = 3;
            ppu_phase_index++;
        }

        private static void PPUOamPhase3()
        {
            ppu_oam_bank_secondary[ppu_oamev_slot * 4 + ppu_oamev_m] = ppu_fetch_data;
            ppu_oamev_m = 0;
            ppu_oamev_n++;
            ppu_oamev_slot++;
            if (ppu_oamev_n == 64)
            {
                ppu_oamev_n = 0;
                ppu_phase_index = 8;
            }
            else if (ppu_oamev_slot < 8)
            {
                ppu_phase_index = 0;
            }
            else if (ppu_oamev_slot == 8)
            {
                ppu_phase_index = 4;
            }
        }

        private static void PPUOamPhase4()
        {
            ppu_oamev_compare = ppu_clock_v >= ppu_fetch_data && ppu_clock_v < ppu_fetch_data + ppu_reg_2000_Sprite_size;
            if (ppu_oamev_compare)
            {
                ppu_oamev_m = 1;
                ppu_phase_index++;
                ppu_reg_2002_SpriteOverflow = true;
                return;
            }
            ppu_oamev_m++;
            if (ppu_oamev_m == 4)
            {
                ppu_oamev_m = 0;
            }
            ppu_oamev_n++;
            if (ppu_oamev_n == 64)
            {
                ppu_oamev_n = 0;
                ppu_phase_index = 8;
            }
            else
            {
                ppu_phase_index = 4;
            }
        }

        private static void PPUOamPhase5()
        {
            ppu_oamev_m = 2;
            ppu_phase_index++;
        }

        private static void PPUOamPhase6()
        {
            ppu_oamev_m = 3;
            ppu_phase_index++;
        }

        private static void PPUOamPhase7()
        {
            ppu_oamev_m = 0;
            ppu_oamev_n++;
            if (ppu_oamev_n == 64)
            {
                ppu_oamev_n = 0;
            }
            ppu_phase_index = 8;
        }

        private static void PPUOamPhase8()
        {
            ppu_oamev_n++;
            if (ppu_oamev_n >= 64)
            {
                ppu_oamev_n = 0;
            }
        }

        private static void RenderPixel()
        {
            if (ppu_clock_v == ppu_clock_vblank_end)
            {
                return;
            }
            ppu_render_x = ppu_clock_h - 1;
            ppu_render_y = ppu_clock_v * 256;
            if (ppu_render_x < 8)
            {
                if (ppu_reg_2001_show_background_in_leftmost_8_pixels_of_screen)
                {
                    ppu_bkg_current_pixel = 0x3F00 | ppu_bkg_pixels[ppu_render_x + ppu_vram_finex];
                }
                else
                {
                    ppu_bkg_current_pixel = 16128;
                }
                if (ppu_reg_2001_show_sprites_in_leftmost_8_pixels_of_screen)
                {
                    ppu_spr_current_pixel = 0x3F10 | ppu_spr_pixels[ppu_render_x];
                }
                else
                {
                    ppu_spr_current_pixel = 16144;
                }
            }
            else
            {
                if (!ppu_reg_2001_show_background)
                {
                    ppu_bkg_current_pixel = 16128;
                }
                else
                {
                    ppu_bkg_current_pixel = 0x3F00 | ppu_bkg_pixels[ppu_render_x + ppu_vram_finex];
                }
                if (!ppu_reg_2001_show_sprites || ppu_clock_v == 0)
                {
                    ppu_spr_current_pixel = 16144;
                }
                else
                {
                    ppu_spr_current_pixel = 0x3F10 | ppu_spr_pixels[ppu_render_x];
                }
            }
            ppu_current_pixel = 0;
            if (((uint)ppu_spr_current_pixel & 0x8000u) != 0)
            {
                ppu_current_pixel = ppu_spr_current_pixel;
            }
            else
            {
                ppu_current_pixel = ppu_bkg_current_pixel;
            }
            if ((ppu_bkg_current_pixel & 3) == 0)
            {
                ppu_current_pixel = ppu_spr_current_pixel;
            }
            else if ((ppu_spr_current_pixel & 3) == 0)
            {
                ppu_current_pixel = ppu_bkg_current_pixel;
            }
            else if (((uint)ppu_spr_pixels[ppu_render_x] & 0x4000u) != 0)
            {
                ppu_reg_2002_Sprite0Hit = true;
            }
            if ((ppu_current_pixel & 3) == 0)
            {
                ppu_screen_pixels[ppu_render_x + ppu_render_y] = ppu_palette[(ppu_palette_bank[ppu_current_pixel & 0xC] & ppu_reg_2001_grayscale) | ppu_reg_2001_emphasis];
            }
            else
            {
                ppu_screen_pixels[ppu_render_x + ppu_render_y] = ppu_palette[(ppu_palette_bank[ppu_current_pixel & 0x1F] & ppu_reg_2001_grayscale) | ppu_reg_2001_emphasis];
            }
        }

        private static void PPUIORead(ref ushort addr, out byte value)
        {
            ppu_reg_io_addr = (byte)(addr & 7u);
            ppu_reg_access_happened = true;
            ppu_reg_access_w = false;
            ppu_reg_read_func[ppu_reg_io_addr]();
            value = ppu_reg_io_db;
        }

        private static void PPUIOWrite(ref ushort addr, ref byte value)
        {
            ppu_reg_io_addr = (byte)(addr & 7u);
            ppu_reg_io_db = value;
            ppu_reg_access_w = true;
            ppu_reg_access_happened = true;
        }

        private static void PPUOnRegister2000()
        {
            if (ppu_reg_access_w)
            {
                ppu_vram_addr_temp = (ushort)((ppu_vram_addr_temp & 0x73FFu) | (uint)((ppu_reg_io_db & 3) << 10));
                if ((ppu_reg_io_db & 4u) != 0)
                {
                    ppu_reg_2000_vram_address_increament = 32;
                }
                else
                {
                    ppu_reg_2000_vram_address_increament = 1;
                }
                if ((ppu_reg_io_db & 8u) != 0)
                {
                    ppu_reg_2000_sprite_pattern_table_address_for_8x8_sprites = 4096;
                }
                else
                {
                    ppu_reg_2000_sprite_pattern_table_address_for_8x8_sprites = 0;
                }
                if ((ppu_reg_io_db & 0x10u) != 0)
                {
                    ppu_reg_2000_background_pattern_table_address = 4096;
                }
                else
                {
                    ppu_reg_2000_background_pattern_table_address = 0;
                }
                if ((ppu_reg_io_db & 0x20u) != 0)
                {
                    ppu_reg_2000_Sprite_size = 16;
                }
                else
                {
                    ppu_reg_2000_Sprite_size = 8;
                }
                if (!ppu_reg_2000_VBI && (ppu_reg_io_db & 0x80u) != 0 && ppu_reg_2002_VblankStartedFlag)
                {
                    PPU_NMI_Current = true;
                }
                ppu_reg_2000_VBI = (ppu_reg_io_db & 0x80) != 0;
                if (!ppu_reg_2000_VBI && ppu_is_nmi_time)
                {
                    PPU_NMI_Current = false;
                }
            }
        }

        private static void PPUOnRegister2001()
        {
            if (ppu_reg_access_w)
            {
                ppu_reg_2001_show_background_in_leftmost_8_pixels_of_screen = (ppu_reg_io_db & 2) != 0;
                ppu_reg_2001_show_sprites_in_leftmost_8_pixels_of_screen = (ppu_reg_io_db & 4) != 0;
                ppu_reg_2001_show_background = (ppu_reg_io_db & 8) != 0;
                ppu_reg_2001_show_sprites = (ppu_reg_io_db & 0x10) != 0;
                ppu_reg_2001_grayscale = ((((uint)ppu_reg_io_db & (true ? 1u : 0u)) != 0) ? 48 : 63);
                ppu_reg_2001_emphasis = (ppu_reg_io_db & 0xE0) << 1;
            }
        }

        private static void PPUOnRegister2002()
        {
            if (!ppu_reg_access_w)
            {
                ppu_vram_flip_flop = false;
                ppu_reg_2002_VblankStartedFlag = false;
                if (ppu_clock_v == ppu_clock_vblank_start)
                {
                    PPU_NMI_Current = ppu_reg_2002_VblankStartedFlag & ppu_reg_2000_VBI;
                }
            }
        }

        private static void PPUOnRegister2003()
        {
            if (ppu_reg_access_w)
            {
                ppu_reg_2003_oam_addr = ppu_reg_io_db;
            }
        }

        private static void PPUOnRegister2004()
        {
            if (ppu_reg_access_w)
            {
                if (ppu_clock_v < 240 && IsRenderingOn())
                {
                    ppu_reg_io_db = byte.MaxValue;
                }
                if ((ppu_reg_2003_oam_addr & 3) == 2)
                {
                    ppu_reg_io_db &= 227;
                }
                ppu_oam_bank[ppu_reg_2003_oam_addr] = ppu_reg_io_db;
                ppu_reg_2003_oam_addr = (byte)((uint)(ppu_reg_2003_oam_addr + 1) & 0xFFu);
            }
        }

        private static void PPUOnRegister2005()
        {
            if (ppu_reg_access_w)
            {
                if (!ppu_vram_flip_flop)
                {
                    ppu_vram_addr_temp = (ushort)((ppu_vram_addr_temp & 0x7FE0u) | (uint)((ppu_reg_io_db & 0xF8) >> 3));
                    ppu_vram_finex = (byte)(ppu_reg_io_db & 7u);
                }
                else
                {
                    ppu_vram_addr_temp = (ushort)((ppu_vram_addr_temp & 0xC1Fu) | (uint)((ppu_reg_io_db & 7) << 12) | (uint)((ppu_reg_io_db & 0xF8) << 2));
                }
                ppu_vram_flip_flop = !ppu_vram_flip_flop;
            }
        }

        private static void PPUOnRegister2006()
        {
            if (ppu_reg_access_w)
            {
                if (!ppu_vram_flip_flop)
                {
                    ppu_vram_addr_temp = (ushort)((ppu_vram_addr_temp & 0xFFu) | (uint)((ppu_reg_io_db & 0x3F) << 8));
                }
                else
                {
                    ppu_vram_addr_temp = (ushort)((ppu_vram_addr_temp & 0x7F00u) | ppu_reg_io_db);
                    ppu_vram_addr = ppu_vram_addr_temp;
                    mem_board.OnPPUAddressUpdate(ref ppu_vram_addr);
                }
                ppu_vram_flip_flop = !ppu_vram_flip_flop;
            }
        }

        private static void PPUOnRegister2007()
        {
            if (ppu_reg_access_w)
            {
                ppu_vram_addr_access_temp = (ushort)(ppu_vram_addr & 0x3FFFu);
                if (ppu_vram_addr_access_temp < 8192)
                {
                    mem_board.WriteCHR(ref ppu_vram_addr_access_temp, ref ppu_reg_io_db);
                }
                else if (ppu_vram_addr_access_temp < 16128)
                {
                    mem_board.WriteNMT(ref ppu_vram_addr_access_temp, ref ppu_reg_io_db);
                }
                else if ((ppu_vram_addr_access_temp & 3u) != 0)
                {
                    ppu_palette_bank[ppu_vram_addr_access_temp & 0x1F] = ppu_reg_io_db;
                }
                else
                {
                    ppu_palette_bank[ppu_vram_addr_access_temp & 0xC] = ppu_reg_io_db;
                }
            }
            else
            {
                if ((ppu_vram_addr & 0x3F00) == 16128)
                {
                    ppu_vram_addr_access_temp = (ushort)(ppu_vram_addr & 0x2FFFu);
                }
                else
                {
                    ppu_vram_addr_access_temp = (ushort)(ppu_vram_addr & 0x3FFFu);
                }
                if (ppu_vram_addr_access_temp < 8192)
                {
                    mem_board.ReadCHR(ref ppu_vram_addr_access_temp, out ppu_vram_data);
                }
                else if (ppu_vram_addr_access_temp < 16128)
                {
                    mem_board.ReadNMT(ref ppu_vram_addr_access_temp, out ppu_vram_data);
                }
            }
            ppu_vram_addr = (ushort)((uint)(ppu_vram_addr + ppu_reg_2000_vram_address_increament) & 0x7FFFu);
            mem_board.OnPPUAddressUpdate(ref ppu_vram_addr);
        }

        private static void PPURead2000()
        {
        }

        private static void PPURead2001()
        {
        }

        private static void PPURead2002()
        {
            ppu_reg_io_db = (byte)((ppu_reg_io_db & 0xDFu) | (ppu_reg_2002_SpriteOverflow ? 32u : 0u));
            ppu_reg_io_db = (byte)((ppu_reg_io_db & 0xBFu) | (ppu_reg_2002_Sprite0Hit ? 64u : 0u));
            ppu_reg_io_db = (byte)((ppu_reg_io_db & 0x7Fu) | (ppu_reg_2002_VblankStartedFlag ? 128u : 0u));
        }

        private static void PPURead2003()
        {
        }

        private static void PPURead2004()
        {
            ppu_reg_io_db = ppu_oam_bank[ppu_reg_2003_oam_addr];
            if (ppu_clock_v < 240 && IsRenderingOn())
            {
                if (ppu_clock_h < 64)
                {
                    ppu_reg_io_db = byte.MaxValue;
                }
                else if (ppu_clock_h < 192)
                {
                    ppu_reg_io_db = ppu_oam_bank[(ppu_clock_h - 64 << 1) & 0xFC];
                }
                else if (ppu_clock_h < 256)
                {
                    ppu_reg_io_db = (((ppu_clock_h & 1) == 1) ? ppu_oam_bank[252] : ppu_oam_bank[(ppu_clock_h - 192 << 1) & 0xFC]);
                }
                else if (ppu_clock_h < 320)
                {
                    ppu_reg_io_db = byte.MaxValue;
                }
                else
                {
                    ppu_reg_io_db = ppu_oam_bank[0];
                }
            }
        }

        private static void PPURead2005()
        {
        }

        private static void PPURead2006()
        {
        }

        private static void PPURead2007()
        {
            ppu_vram_addr_access_temp = (ushort)(ppu_vram_addr & 0x3FFFu);
            if (ppu_vram_addr_access_temp < 16128)
            {
                ppu_reg_io_db = ppu_vram_data;
            }
            else if ((ppu_vram_addr_access_temp & 3u) != 0)
            {
                ppu_reg_io_db = ppu_palette_bank[ppu_vram_addr_access_temp & 0x1F];
            }
            else
            {
                ppu_reg_io_db = ppu_palette_bank[ppu_vram_addr_access_temp & 0xC];
            }
        }

        internal static bool IsRenderingOn()
        {
            if (!ppu_reg_2001_show_background)
            {
                return ppu_reg_2001_show_sprites;
            }
            return true;
        }

        internal static bool IsInRender()
        {
            if (ppu_clock_v >= 240)
            {
                return ppu_clock_v == ppu_clock_vblank_end;
            }
            return true;
        }

        private static void PPUWriteState(ref BinaryWriter bin)
        {
            bin.Write(ppu_clock_h);
            bin.Write(ppu_clock_v);
            bin.Write(ppu_clock_vblank_start);
            bin.Write(ppu_clock_vblank_end);
            bin.Write(ppu_use_odd_cycle);
            bin.Write(ppu_use_odd_swap);
            bin.Write(ppu_is_nmi_time);
            bin.Write(ppu_frame_finished);
            bin.Write(ppu_oam_bank);
            bin.Write(ppu_oam_bank_secondary);
            bin.Write(ppu_palette_bank);
            bin.Write(ppu_reg_io_db);
            bin.Write(ppu_reg_io_addr);
            bin.Write(ppu_reg_access_happened);
            bin.Write(ppu_reg_access_w);
            bin.Write(ppu_reg_2000_vram_address_increament);
            bin.Write(ppu_reg_2000_sprite_pattern_table_address_for_8x8_sprites);
            bin.Write(ppu_reg_2000_background_pattern_table_address);
            bin.Write(ppu_reg_2000_Sprite_size);
            bin.Write(ppu_reg_2000_VBI);
            bin.Write(ppu_reg_2001_show_background_in_leftmost_8_pixels_of_screen);
            bin.Write(ppu_reg_2001_show_sprites_in_leftmost_8_pixels_of_screen);
            bin.Write(ppu_reg_2001_show_background);
            bin.Write(ppu_reg_2001_show_sprites);
            bin.Write(ppu_reg_2001_grayscale);
            bin.Write(ppu_reg_2001_emphasis);
            bin.Write(ppu_reg_2002_SpriteOverflow);
            bin.Write(ppu_reg_2002_Sprite0Hit);
            bin.Write(ppu_reg_2002_VblankStartedFlag);
            bin.Write(ppu_reg_2003_oam_addr);
            bin.Write(ppu_vram_addr);
            bin.Write(ppu_vram_data);
            bin.Write(ppu_vram_addr_temp);
            bin.Write(ppu_vram_addr_access_temp);
            bin.Write(ppu_vram_flip_flop);
            bin.Write(ppu_vram_finex);
            bin.Write(ppu_bkgfetch_nt_addr);
            bin.Write(ppu_bkgfetch_nt_data);
            bin.Write(ppu_bkgfetch_at_addr);
            bin.Write(ppu_bkgfetch_at_data);
            bin.Write(ppu_bkgfetch_lb_addr);
            bin.Write(ppu_bkgfetch_lb_data);
            bin.Write(ppu_bkgfetch_hb_addr);
            bin.Write(ppu_bkgfetch_hb_data);
            bin.Write(ppu_sprfetch_slot);
            bin.Write(ppu_sprfetch_y_data);
            bin.Write(ppu_sprfetch_t_data);
            bin.Write(ppu_sprfetch_at_data);
            bin.Write(ppu_sprfetch_x_data);
            bin.Write(ppu_sprfetch_lb_addr);
            bin.Write(ppu_sprfetch_lb_data);
            bin.Write(ppu_sprfetch_hb_addr);
            bin.Write(ppu_sprfetch_hb_data);
            bin.Write(ppu_bkg_render_i);
            bin.Write(ppu_bkg_render_pos);
            bin.Write(ppu_bkg_render_tmp_val);
            bin.Write(ppu_bkg_current_pixel);
            bin.Write(ppu_spr_current_pixel);
            bin.Write(ppu_current_pixel);
            bin.Write(ppu_render_x);
            bin.Write(0);
            bin.Write(ppu_oamev_n);
            bin.Write(ppu_oamev_m);
            bin.Write(ppu_oamev_compare);
            bin.Write(ppu_oamev_slot);
            bin.Write(ppu_fetch_data);
            bin.Write(ppu_phase_index);
            bin.Write(ppu_sprite0_should_hit);
        }

        private static void PPUReadState(ref BinaryReader bin)
        {
            ppu_clock_h = bin.ReadInt32();
            ppu_clock_v = bin.ReadUInt16();
            ppu_clock_vblank_start = bin.ReadUInt16();
            ppu_clock_vblank_end = bin.ReadUInt16();
            ppu_use_odd_cycle = bin.ReadBoolean();
            ppu_use_odd_swap = bin.ReadBoolean();
            ppu_is_nmi_time = bin.ReadBoolean();
            ppu_frame_finished = bin.ReadBoolean();
            bin.Read(ppu_oam_bank, 0, ppu_oam_bank.Length);
            bin.Read(ppu_oam_bank_secondary, 0, ppu_oam_bank_secondary.Length);
            bin.Read(ppu_palette_bank, 0, ppu_palette_bank.Length);
            ppu_reg_io_db = bin.ReadByte();
            ppu_reg_io_addr = bin.ReadByte();
            ppu_reg_access_happened = bin.ReadBoolean();
            ppu_reg_access_w = bin.ReadBoolean();
            ppu_reg_2000_vram_address_increament = bin.ReadByte();
            ppu_reg_2000_sprite_pattern_table_address_for_8x8_sprites = bin.ReadUInt16();
            ppu_reg_2000_background_pattern_table_address = bin.ReadUInt16();
            ppu_reg_2000_Sprite_size = bin.ReadByte();
            ppu_reg_2000_VBI = bin.ReadBoolean();
            ppu_reg_2001_show_background_in_leftmost_8_pixels_of_screen = bin.ReadBoolean();
            ppu_reg_2001_show_sprites_in_leftmost_8_pixels_of_screen = bin.ReadBoolean();
            ppu_reg_2001_show_background = bin.ReadBoolean();
            ppu_reg_2001_show_sprites = bin.ReadBoolean();
            ppu_reg_2001_grayscale = bin.ReadInt32();
            ppu_reg_2001_emphasis = bin.ReadInt32();
            ppu_reg_2002_SpriteOverflow = bin.ReadBoolean();
            ppu_reg_2002_Sprite0Hit = bin.ReadBoolean();
            ppu_reg_2002_VblankStartedFlag = bin.ReadBoolean();
            ppu_reg_2003_oam_addr = bin.ReadByte();
            ppu_vram_addr = bin.ReadUInt16();
            ppu_vram_data = bin.ReadByte();
            ppu_vram_addr_temp = bin.ReadUInt16();
            ppu_vram_addr_access_temp = bin.ReadUInt16();
            ppu_vram_flip_flop = bin.ReadBoolean();
            ppu_vram_finex = bin.ReadByte();
            ppu_bkgfetch_nt_addr = bin.ReadUInt16();
            ppu_bkgfetch_nt_data = bin.ReadByte();
            ppu_bkgfetch_at_addr = bin.ReadUInt16();
            ppu_bkgfetch_at_data = bin.ReadByte();
            ppu_bkgfetch_lb_addr = bin.ReadUInt16();
            ppu_bkgfetch_lb_data = bin.ReadByte();
            ppu_bkgfetch_hb_addr = bin.ReadUInt16();
            ppu_bkgfetch_hb_data = bin.ReadByte();
            ppu_sprfetch_slot = bin.ReadInt32();
            ppu_sprfetch_y_data = bin.ReadByte();
            ppu_sprfetch_t_data = bin.ReadByte();
            ppu_sprfetch_at_data = bin.ReadByte();
            ppu_sprfetch_x_data = bin.ReadByte();
            ppu_sprfetch_lb_addr = bin.ReadUInt16();
            ppu_sprfetch_lb_data = bin.ReadByte();
            ppu_sprfetch_hb_addr = bin.ReadUInt16();
            ppu_sprfetch_hb_data = bin.ReadByte();
            ppu_bkg_render_i = bin.ReadInt32();
            ppu_bkg_render_pos = bin.ReadInt32();
            ppu_bkg_render_tmp_val = bin.ReadInt32();
            ppu_bkg_current_pixel = bin.ReadInt32();
            ppu_spr_current_pixel = bin.ReadInt32();
            ppu_current_pixel = bin.ReadInt32();
            ppu_render_x = bin.ReadInt32();
            bin.ReadInt32();
            ppu_oamev_n = bin.ReadByte();
            ppu_oamev_m = bin.ReadByte();
            ppu_oamev_compare = bin.ReadBoolean();
            ppu_oamev_slot = bin.ReadByte();
            ppu_fetch_data = bin.ReadByte();
            ppu_phase_index = bin.ReadByte();
            ppu_sprite0_should_hit = bin.ReadBoolean();
        }

        internal static void CheckGame(string fileName, out bool valid)
        {
            string text = Path.GetExtension(fileName).ToLower();
            if (text != null && text == ".nes")
            {
                Tracer.WriteLine("Checking INES header ...");
                INes nes = new INes();
                nes.Load(fileName, loadDumps: false);
                valid = nes.IsValid;
                Tracer.WriteLine("INES header is valid.");
            }
            else
            {
                Tracer.WriteWarning("File format is not supported. Format: " + Path.GetExtension(fileName));
                valid = false;
            }
        }

        internal static void Initialize()
        {
            Tracer.WriteLine("Loading database file ...");
            NesCartDatabase.LoadDatabase(out bool success);

            if (success)
            {
                Tracer.WriteInformation("Nes Cart database file loaded successfully.");
            }
            else
            {
                Tracer.WriteError("Error loading Nes Cart database file.");
            }

            FrameLimiterEnabled = true;
            CPUInitialize();
            PPUInitialize();
            APUInitialize();
            PORTSInitialize();
        }

        internal static void SetupRenderingMethods(RenderVideoFrame renderVideo, RenderAudioSamples renderAudio, TogglePause renderTogglePause, GetIsPlaying renderGetIsPlaying)
        {
            render_initialized = false;
            render_video = renderVideo;
            render_audio = renderAudio;
            render_audio_toggle_pause = renderTogglePause;
            render_audio_get_is_playing = renderGetIsPlaying;
            render_initialized = render_video != null && render_audio != null && render_audio_toggle_pause != null && render_audio_get_is_playing != null;
            if (render_initialized)
            {
                Tracer.WriteInformation("Renderer methods initialized successfully.");
                return;
            }
            Tracer.WriteError("ERROR RENDERER INITIALIZING !!");
            Tracer.WriteError("Faild to initialize the renderers methods. Please use the method 'SetupRenderingMethods' to initialize the renderers methods before you can run the emulation.");
        }

        public static void LoadGame(string fileName, out bool success, bool useThread)
        {
            if (!render_initialized)
            {
                Tracer.WriteError("NO RENDERER INITIALIZED !! EMU CANNOT BE INTIALIZED WITHOUT A RENDERER !!");
                Tracer.WriteError("Please use the method 'SetupRenderingMethods' to initialize the renderers methods before you can run the emulation.");
                success = false;
                return;
            }
            Tracer.WriteLine("Checking INES header ...");
            INes nes = new INes();
            nes.Load(fileName, loadDumps: true);
            if (nes.IsValid)
            {
                emu_request_mode = RequestMode.None;
                CurrentFilePath = fileName;
                if (ON)
                {
                    ShutDown();
                }
                Tracer.WriteLine("INES header is valid, loading game ...");
                ApplyRegionSetting();
                MEMInitialize(nes);
                ApplyAudioSettings();
                ApplyFrameSkipSettings();
                ApplyPaletteSetting();
                PORTSInitialize();
                hardReset();
                Tracer.WriteLine("EMU is ready.");
                success = true;
                ON = true;
                PAUSED = false;
                if (useThread)
                {
                    Tracer.WriteLine("Running in a thread ... using custom frame limiter.");
                    FrameLimiterEnabled = true;
                    currentFrame = 0;
                    mainThread = new Thread(EmuClock);
                    mainThread.Start();
                }
                MyNesMain.VideoProvider.SignalToggle(started: true);
                MyNesMain.AudioProvider.SignalToggle(started: true);
            }
            else
            {
                success = false;
            }
        }

        public static void HardReset()
        {
            PAUSED = true;
            emu_request_mode = RequestMode.HardReset;
        }

        private static void hardReset()
        {
            if (MyNesMain.WaveRecorder.IsRecording)
            {
                MyNesMain.WaveRecorder.Stop();
            }
            render_audio_toggle_pause(paused: true);
            switch (Region)
            {
                case EmuRegion.NTSC:
                    emu_time_target_fps = 60.0988;
                    break;
                case EmuRegion.PALB:
                case EmuRegion.DENDY:
                    emu_time_target_fps = 50.0;
                    break;
            }
            fps_time_period = 1.0 / emu_time_target_fps;
            MEMHardReset();
            CPUHardReset();
            PPUHardReset();
            APUHardReset();
            DMAHardReset();
            render_audio_toggle_pause(paused: false);
            MyNesMain.VideoProvider.WriteWarningNotification(MNInterfaceLanguage.Message_HardReset, instant: false);
        }

        public static void SoftReset()
        {
            PAUSED = true;
            emu_request_mode = RequestMode.SoftReset;
        }

        private static void softReset()
        {
            CPUSoftReset();
            APUSoftReset();
            MyNesMain.VideoProvider.WriteWarningNotification(MNInterfaceLanguage.Message_SoftReset, instant: false);
        }

        public static void SaveState()
        {
            PAUSED = true;
            emu_request_mode = RequestMode.SaveState;
        }

        public static void LoadState()
        {
            PAUSED = true;
            emu_request_mode = RequestMode.LoadState;
        }

        internal static void TakeSnapshot()
        {
            PAUSED = true;
            emu_request_mode = RequestMode.TakeSnapshot;
        }

        public static void ShutDown()
        {
            MyNesMain.VideoProvider.SignalToggle(started: false);
            MyNesMain.AudioProvider.SignalToggle(started: false);
            if (MyNesMain.WaveRecorder.IsRecording)
            {
                MyNesMain.WaveRecorder.Stop();
            }
            render_audio_get_is_playing(out render_audio_is_playing);
            if (render_audio_is_playing)
            {
                render_audio_toggle_pause(paused: true);
            }
            Tracer.WriteLine("Shutting down the emulation core...");
            ON = false;
            if (mainThread != null)
            {
                Tracer.WriteLine("Aborting thread ..");
                mainThread.Abort();
                mainThread = null;
            }
            SaveSRAM();
            Tracer.WriteInformation("Emulation core shutdown successfully.");
            NesEmu.EmuShutdown?.Invoke(null, new EventArgs());
        }

        private static Stopwatch sw = new Stopwatch();
        private static double fixTime;
        public static ulong currentFrame;
        private static void EmuClock()
        {
            while (ON)
            {
                if (!PAUSED)
                {
                    var waitTime = GetTime() + fps_time_period + fixTime;

                    while (!ppu_frame_finished)
                        CPUClock();

                    FrameFinished();

                    fixTime = waitTime - GetTime();
                    while (fixTime > 0)
                    {
                        fixTime = waitTime - GetTime();
                    };

                    currentFrame++;

                    continue;
                }
                render_audio_get_is_playing(out render_audio_is_playing);
                if (render_audio_is_playing)
                {
                    render_audio_toggle_pause(paused: true);
                }
                Thread.Sleep(100);
                switch (emu_request_mode)
                {
                    case RequestMode.HardReset:
                        hardReset();
                        PAUSED = false;
                        emu_request_mode = RequestMode.None;
                        break;
                    case RequestMode.SoftReset:
                        softReset();
                        PAUSED = false;
                        emu_request_mode = RequestMode.None;
                        break;
                    case RequestMode.SaveState:
                        StateHandler.SaveState();
                        PAUSED = false;
                        emu_request_mode = RequestMode.None;
                        break;
                    case RequestMode.LoadState:
                        StateHandler.LoadState();
                        PAUSED = false;
                        emu_request_mode = RequestMode.None;
                        break;
                    case RequestMode.TakeSnapshot:
                        MyNesMain.VideoProvider.TakeSnapshot();
                        PAUSED = false;
                        emu_request_mode = RequestMode.None;
                        break;
                }
                isPaused = true;
            }
        }

        internal static void EmuClockComponents()
        {
            PPUClock();
            PollInterruptStatus();
            PPUClock();
            PPUClock();
            APUClock();
            DMAClock();
            mem_board.OnCPUClock();
        }

        internal static void ApplyFrameSkipSettings()
        {
            FrameSkipEnabled = MyNesMain.RendererSettings.FrameSkipEnabled;
            FrameSkipInterval = MyNesMain.RendererSettings.FrameSkipInterval;
        }

        private static void FrameFinished()
        {
            if (!FrameSkipEnabled)
            {
                render_video(ref ppu_screen_pixels);
            }
            else
            {
                FrameSkipCounter++;
                if (FrameSkipCounter >= FrameSkipInterval)
                {
                    render_video(ref ppu_screen_pixels);
                    FrameSkipCounter = 0;
                }
            }
            isPaused = false;
            ppu_frame_finished = false;
            joypad1.Update();
            joypad2.Update();
            if (IsFourPlayers)
            {
                joypad3.Update();
                joypad4.Update();
            }
            if (SoundEnabled)
            {
                render_audio_get_is_playing(out render_audio_is_playing);
                if (!render_audio_is_playing)
                {
                    render_audio_toggle_pause(paused: false);
                }
                render_audio(ref audio_samples, ref audio_samples_added);
                audio_w_pos = 0;
                audio_samples_added = 0;
                audio_timer = 0.0;
            }
        }

        private static double GetTime()
        {
            return (double)Stopwatch.GetTimestamp() / (double)Stopwatch.Frequency;
        }


        public static void SetFramePeriod(ref double period)
        {
            fps_time_period = period;
        }

        public static void RevertFramePeriod()
        {
            fps_time_period = 1 / emu_time_target_fps;
        }

        public static void ApplyRegionSetting()
        {
            switch ((RegionSetting)MyNesMain.EmuSettings.RegionSetting)
            {
                case RegionSetting.AUTO:
                    Tracer.WriteLine("REGION = AUTO");
                    Region = EmuRegion.NTSC;
                    if (CurrentFilePath.Contains("(E)"))
                    {
                        Region = EmuRegion.PALB;
                    }
                    Tracer.WriteLine("REGION SELECTED: " + Region);
                    break;
                case RegionSetting.ForceNTSC:
                    Tracer.WriteLine("REGION: FORCE NTSC");
                    Region = EmuRegion.NTSC;
                    break;
                case RegionSetting.ForcePALB:
                    Tracer.WriteLine("REGION: FORCE PALB");
                    Region = EmuRegion.PALB;
                    break;
                case RegionSetting.ForceDENDY:
                    Tracer.WriteLine("REGION: FORCE DENDY");
                    Region = EmuRegion.DENDY;
                    break;
            }
            SystemIndex = (int)Region;
        }

        public static void ApplyPaletteSetting()
        {
            Tracer.WriteLine("Loading palette generators values from settings...");
            NTSCPaletteGenerator.brightness = MyNesMain.RendererSettings.Palette_NTSC_brightness;
            NTSCPaletteGenerator.contrast = MyNesMain.RendererSettings.Palette_NTSC_contrast;
            NTSCPaletteGenerator.gamma = MyNesMain.RendererSettings.Palette_NTSC_gamma;
            NTSCPaletteGenerator.hue_tweak = MyNesMain.RendererSettings.Palette_NTSC_hue_tweak;
            NTSCPaletteGenerator.saturation = MyNesMain.RendererSettings.Palette_NTSC_saturation;
            PALBPaletteGenerator.brightness = MyNesMain.RendererSettings.Palette_PALB_brightness;
            PALBPaletteGenerator.contrast = MyNesMain.RendererSettings.Palette_PALB_contrast;
            PALBPaletteGenerator.gamma = MyNesMain.RendererSettings.Palette_PALB_gamma;
            PALBPaletteGenerator.hue_tweak = MyNesMain.RendererSettings.Palette_PALB_hue_tweak;
            PALBPaletteGenerator.saturation = MyNesMain.RendererSettings.Palette_PALB_saturation;
            Tracer.WriteLine("Setting up palette ....");
            switch ((PaletteSelectSetting)MyNesMain.RendererSettings.Palette_PaletteSetting)
            {
                case PaletteSelectSetting.AUTO:
                    Tracer.WriteLine("Palette set to auto detect depending on region.");
                    switch (Region)
                    {
                        case EmuRegion.NTSC:
                            SetupPalette(NTSCPaletteGenerator.GeneratePalette());
                            Tracer.WriteLine("Region is NTSC, Palette set from NTSC generator.");
                            break;
                        case EmuRegion.PALB:
                        case EmuRegion.DENDY:
                            SetupPalette(PALBPaletteGenerator.GeneratePalette());
                            Tracer.WriteLine("Region is PALB/DENDY, Palette set from PALB generator.");
                            break;
                    }
                    break;
                case PaletteSelectSetting.ForceNTSC:
                    Tracer.WriteLine("Palette set to always use NTSC palette generator.");
                    SetupPalette(NTSCPaletteGenerator.GeneratePalette());
                    Tracer.WriteLine("Palette set from NTSC generator.");
                    break;
                case PaletteSelectSetting.ForcePALB:
                    Tracer.WriteLine("Palette set to always use PALB palette generator.");
                    SetupPalette(NTSCPaletteGenerator.GeneratePalette());
                    Tracer.WriteLine("Palette set from PALB generator.");
                    break;
                case PaletteSelectSetting.File:
                    {
                        Tracer.WriteLine("Palette set to load from file.");

                        var paletteFileStream = MyNesMain.Supporter.OpenPaletteFile();
                        if (paletteFileStream != null)
                        {
                            PaletteFileWrapper.LoadFile(paletteFileStream, out var palette);
                            SetupPalette(palette);
                            Tracer.WriteLine("Palette set from file");
                            break;
                        }
                        Tracer.WriteError("Palette from file is not exist is not exist. Setting up palette from generators.");
                        switch (Region)
                        {
                            case EmuRegion.NTSC:
                                SetupPalette(NTSCPaletteGenerator.GeneratePalette());
                                Tracer.WriteLine("Region is NTSC, Palette set from NTSC generator.");
                                break;
                            case EmuRegion.PALB:
                            case EmuRegion.DENDY:
                                SetupPalette(PALBPaletteGenerator.GeneratePalette());
                                Tracer.WriteLine("Region is PALB/DENDY, Palette set from PALB generator.");
                                break;
                        }
                        break;
                    }
            }
        }

        internal static void WriteStateData(ref BinaryWriter bin)
        {
            APUWriteState(ref bin);
            CPUWriteState(ref bin);
            DMAWriteState(ref bin);
            InterruptsWriteState(ref bin);
            MEMWriteState(ref bin);
            PORTWriteState(ref bin);
            PPUWriteState(ref bin);
        }

        internal static void ReadStateData(ref BinaryReader bin)
        {
            APUReadState(ref bin);
            CPUReadState(ref bin);
            DMAReadState(ref bin);
            InterruptsReadState(ref bin);
            MEMReadState(ref bin);
            PORTReadState(ref bin);
            PPUReadState(ref bin);
        }

        private static void SQ2HardReset()
        {
            sq2_duty_cycle = 0;
            sq2_length_halt = false;
            sq2_constant_volume_envelope = false;
            sq2_volume_devider_period = 0;
            sq2_sweep_enable = false;
            sq2_sweep_devider_period = 0;
            sq2_sweep_negate = false;
            sq2_sweep_shift_count = 0;
            sq2_timer = 0;
            sq2_period_devider = 0;
            sq2_seqencer = 0;
            sq2_length_enabled = false;
            sq2_length_counter = 0;
            sq2_envelope_start_flag = false;
            sq2_envelope_devider = 0;
            sq2_envelope_decay_level_counter = 0;
            sq2_envelope = 0;
            sq2_sweep_counter = 0;
            sq2_sweep_reload = false;
            sq2_sweep_change = 0;
            sq2_valid_freq = false;
            sq2_output = 0;
            sq2_ignore_reload = false;
        }

        private static void SQ2SoftReset()
        {
            SQ2HardReset();
        }

        private static void SQ2Clock()
        {
            sq2_period_devider--;
            if (sq2_period_devider > 0)
            {
                return;
            }
            sq2_period_devider = sq2_timer + 1;
            sq2_seqencer = (byte)((uint)(sq2_seqencer + 1) & 7u);
            if (sq2_length_counter > 0 && sq2_valid_freq)
            {
                if (audio_sq2_outputable)
                {
                    sq2_output = sq_duty_cycle_sequences[sq2_duty_cycle][sq2_seqencer] * sq2_envelope;
                }
            }
            else
            {
                sq2_output = 0;
            }
            audio_signal_outputed = true;
        }

        private static void SQ2ClockLength()
        {
            if (sq2_length_counter > 0 && !sq2_length_halt)
            {
                sq2_length_counter--;
                if (apu_reg_access_happened && apu_reg_io_addr == 7 && apu_reg_access_w)
                {
                    sq2_ignore_reload = true;
                }
            }
            sq2_sweep_counter--;
            if (sq2_sweep_counter == 0)
            {
                sq2_sweep_counter = sq2_sweep_devider_period + 1;
                if (sq2_sweep_enable && sq2_sweep_shift_count > 0 && sq2_valid_freq)
                {
                    sq2_sweep_change = sq2_timer >> (int)sq2_sweep_shift_count;
                    sq2_timer += (sq2_sweep_negate ? (-sq2_sweep_change) : sq2_sweep_change);
                    SQ2CalculateValidFreq();
                }
            }
            else if (sq2_sweep_reload)
            {
                sq2_sweep_counter = sq2_sweep_devider_period + 1;
                sq2_sweep_reload = false;
            }
        }

        private static void SQ2ClockEnvelope()
        {
            if (sq2_envelope_start_flag)
            {
                sq2_envelope_start_flag = false;
                sq2_envelope_decay_level_counter = 15;
                sq2_envelope_devider = (byte)(sq2_volume_devider_period + 1);
            }
            else if (sq2_envelope_devider > 0)
            {
                sq2_envelope_devider--;
            }
            else
            {
                sq2_envelope_devider = (byte)(sq2_volume_devider_period + 1);
                if (sq2_envelope_decay_level_counter > 0)
                {
                    sq2_envelope_decay_level_counter--;
                }
                else if (sq2_length_halt)
                {
                    sq2_envelope_decay_level_counter = 15;
                }
            }
            sq2_envelope = (sq2_constant_volume_envelope ? sq2_volume_devider_period : sq2_envelope_decay_level_counter);
        }

        private static void APUOnRegister4004()
        {
            if (apu_reg_access_w)
            {
                sq2_duty_cycle = (byte)((apu_reg_io_db & 0xC0) >> 6);
                sq2_volume_devider_period = (byte)(apu_reg_io_db & 0xFu);
                sq2_length_halt = (apu_reg_io_db & 0x20) != 0;
                sq2_constant_volume_envelope = (apu_reg_io_db & 0x10) != 0;
                sq2_envelope = (sq2_constant_volume_envelope ? sq2_volume_devider_period : sq2_envelope_decay_level_counter);
            }
        }

        private static void APUOnRegister4005()
        {
            if (apu_reg_access_w)
            {
                sq2_sweep_enable = (apu_reg_io_db & 0x80) == 128;
                sq2_sweep_devider_period = (byte)((uint)(apu_reg_io_db >> 4) & 7u);
                sq2_sweep_negate = (apu_reg_io_db & 8) == 8;
                sq2_sweep_shift_count = (byte)(apu_reg_io_db & 7u);
                sq2_sweep_reload = true;
                SQ2CalculateValidFreq();
            }
        }

        private static void APUOnRegister4006()
        {
            if (apu_reg_access_w)
            {
                sq2_timer = (sq2_timer & 0xFF00) | apu_reg_io_db;
                SQ2CalculateValidFreq();
            }
        }

        private static void APUOnRegister4007()
        {
            if (apu_reg_access_w)
            {
                sq2_timer = (sq2_timer & 0xFF) | ((apu_reg_io_db & 7) << 8);
                if (sq2_length_enabled && !sq2_ignore_reload)
                {
                    sq2_length_counter = sq_duration_table[apu_reg_io_db >> 3];
                }
                if (sq2_ignore_reload)
                {
                    sq2_ignore_reload = false;
                }
                sq2_seqencer = 0;
                sq2_envelope_start_flag = true;
                SQ2CalculateValidFreq();
            }
        }

        private static void SQ2On4015()
        {
            sq2_length_enabled = (apu_reg_io_db & 2) != 0;
            if (!sq2_length_enabled)
            {
                sq2_length_counter = 0;
            }
        }

        private static void SQ2Read4015()
        {
            if (sq2_length_counter > 0)
            {
                apu_reg_io_db = (byte)((apu_reg_io_db & 0xFDu) | 2u);
            }
        }

        private static void SQ2CalculateValidFreq()
        {
            sq2_valid_freq = sq2_timer >= 8 && (sq2_sweep_negate || ((sq2_timer + (sq2_timer >> (int)sq2_sweep_shift_count)) & 0x800) == 0);
        }

        private static void SQ2WriteState(ref BinaryWriter bin)
        {
            bin.Write(sq2_duty_cycle);
            bin.Write(sq2_length_halt);
            bin.Write(sq2_constant_volume_envelope);
            bin.Write(sq2_volume_devider_period);
            bin.Write(sq2_sweep_enable);
            bin.Write(sq2_sweep_devider_period);
            bin.Write(sq2_sweep_negate);
            bin.Write(sq2_sweep_shift_count);
            bin.Write(sq2_timer);
            bin.Write(sq2_period_devider);
            bin.Write(sq2_seqencer);
            bin.Write(sq2_length_enabled);
            bin.Write(sq2_length_counter);
            bin.Write(sq2_envelope_start_flag);
            bin.Write(sq2_envelope_devider);
            bin.Write(sq2_envelope_decay_level_counter);
            bin.Write(sq2_envelope);
            bin.Write(sq2_sweep_counter);
            bin.Write(sq2_sweep_reload);
            bin.Write(sq2_sweep_change);
            bin.Write(sq2_valid_freq);
            bin.Write(sq2_output);
            bin.Write(sq2_ignore_reload);
        }

        private static void SQ2ReadState(ref BinaryReader bin)
        {
            sq2_duty_cycle = bin.ReadByte();
            sq2_length_halt = bin.ReadBoolean();
            sq2_constant_volume_envelope = bin.ReadBoolean();
            sq2_volume_devider_period = bin.ReadByte();
            sq2_sweep_enable = bin.ReadBoolean();
            sq2_sweep_devider_period = bin.ReadByte();
            sq2_sweep_negate = bin.ReadBoolean();
            sq2_sweep_shift_count = bin.ReadByte();
            sq2_timer = bin.ReadInt32();
            sq2_period_devider = bin.ReadInt32();
            sq2_seqencer = bin.ReadByte();
            sq2_length_enabled = bin.ReadBoolean();
            sq2_length_counter = bin.ReadInt32();
            sq2_envelope_start_flag = bin.ReadBoolean();
            sq2_envelope_devider = bin.ReadByte();
            sq2_envelope_decay_level_counter = bin.ReadByte();
            sq2_envelope = bin.ReadByte();
            sq2_sweep_counter = bin.ReadInt32();
            sq2_sweep_reload = bin.ReadBoolean();
            sq2_sweep_change = bin.ReadInt32();
            sq2_valid_freq = bin.ReadBoolean();
            sq2_output = bin.ReadInt32();
            sq2_ignore_reload = bin.ReadBoolean();
        }
    }
}
