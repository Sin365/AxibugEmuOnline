using MAME.Core;

namespace MAME.Core
{
    public class analog_field_state
    {
        //public byte shift;
        public int adjdefvalue;
        public int adjmin;
        public int adjmax;

        public int sensitivity;
        public bool reverse;
        public int delta;
        //public int centerdelta;

        public int accum;
        public int previous;
        //public int previousanalog;

        public int minimum;
        public int maximum;
        //public int center;
        public int reverse_val;

        public long scalepos;
        public long scaleneg;
        public long keyscalepos;
        public long keyscaleneg;
        //public long positionalscale;

        public bool absolute;
        public bool wraps;
        //public byte autocenter;
        public byte single_scale;
        public bool interpolate;
        public byte lastdigital;
    }
    public class input_port_private
    {
        public Atime last_frame_time;
        public long last_delta_nsec;
    }
    public partial class Inptport
    {
        public static bool bReplayRead;
        public delegate void loop_delegate();
        public static loop_delegate loop_inputports_callback, record_port_callback, replay_port_callback;
        public static analog_field_state analog_p0, analog_p1, analog_p1x, analog_p1y;
        public static input_port_private portdata;
        public static void input_port_init()
        {
            portdata = new input_port_private();
            switch (Machine.sBoard)
            {
                case "CPS-1":
                case "CPS-1(QSound)":
                    loop_inputports_callback = CPS.loop_inputports_cps1_6b;
                    record_port_callback = CPS.record_portC;
                    replay_port_callback = CPS.replay_portC;
                    analog_p0 = new analog_field_state();
                    analog_p1 = new analog_field_state();
                    analog_p0.adjdefvalue = 0;
                    analog_p1.adjdefvalue = 0;
                    analog_p0.sensitivity = 100;
                    analog_p1.sensitivity = 100;
                    analog_p0.reverse = false;
                    analog_p1.reverse = false;
                    analog_p0.delta = 20;
                    analog_p1.delta = 20;
                    analog_p0.minimum = 0;
                    analog_p1.minimum = 0;
                    analog_p0.maximum = 0x1ffe00;
                    analog_p1.maximum = 0x1ffe00;
                    analog_p0.reverse_val = 0x200000;
                    analog_p1.reverse_val = 0x200000;
                    analog_p0.scalepos = 0x8000;
                    analog_p1.scalepos = 0x8000;
                    analog_p0.scaleneg = 0x8000;
                    analog_p1.scaleneg = 0x8000;
                    analog_p0.absolute = false;
                    analog_p0.wraps = true;
                    analog_p0.interpolate = true;
                    analog_p1.absolute = false;
                    analog_p1.wraps = true;
                    analog_p1.interpolate = true;
                    break;
                case "CPS2":
                    loop_inputports_callback = CPS.loop_inputports_cps2_2p6b;
                    record_port_callback = CPS.record_portC2;
                    replay_port_callback = CPS.replay_portC2;
                    analog_p0 = new analog_field_state();
                    analog_p1 = new analog_field_state();
                    analog_p0.adjdefvalue = 0;
                    analog_p1.adjdefvalue = 0;
                    analog_p0.sensitivity = 100;
                    analog_p1.sensitivity = 100;
                    analog_p0.reverse = false;
                    analog_p1.reverse = false;
                    analog_p0.delta = 20;
                    analog_p1.delta = 20;
                    analog_p0.minimum = 0;
                    analog_p1.minimum = 0;
                    analog_p0.maximum = 0x1ffe00;
                    analog_p1.maximum = 0x1ffe00;
                    analog_p0.reverse_val = 0x200000;
                    analog_p1.reverse_val = 0x200000;
                    analog_p0.scalepos = 0x8000;
                    analog_p1.scalepos = 0x8000;
                    analog_p0.scaleneg = 0x8000;
                    analog_p1.scaleneg = 0x8000;
                    analog_p0.absolute = false;
                    analog_p0.wraps = true;
                    analog_p0.interpolate = true;
                    analog_p1.absolute = false;
                    analog_p1.wraps = true;
                    analog_p1.interpolate = true;
                    break;
                case "Data East":
                    loop_inputports_callback = Dataeast.loop_inputports_dataeast_pcktgal;
                    record_port_callback = Dataeast.record_port_pcktgal;
                    replay_port_callback = Dataeast.replay_port_pcktgal;
                    break;
                case "Tehkan":
                    break;
                case "Neo Geo":
                    loop_inputports_callback = Neogeo.loop_inputports_neogeo_standard;
                    record_port_callback = Neogeo.record_port;
                    replay_port_callback = Neogeo.replay_port;
                    analog_p0 = new analog_field_state();
                    analog_p1 = new analog_field_state();
                    analog_p0.adjdefvalue = 0;
                    analog_p1.adjdefvalue = 0;
                    analog_p0.sensitivity = 10;
                    analog_p1.sensitivity = 10;
                    analog_p0.reverse = true;
                    analog_p1.reverse = true;
                    analog_p0.delta = 20;
                    analog_p1.delta = 20;
                    analog_p0.minimum = 0;
                    analog_p1.minimum = 0;
                    analog_p0.maximum = 0x1fe00;
                    analog_p1.maximum = 0x1fe00;
                    analog_p0.reverse_val = 0x20000;
                    analog_p1.reverse_val = 0x20000;
                    analog_p0.scalepos = 0x8000;
                    analog_p1.scalepos = 0x8000;
                    analog_p0.scaleneg = 0x8000;
                    analog_p1.scaleneg = 0x8000;
                    analog_p0.absolute = false;
                    analog_p0.wraps = true;
                    analog_p0.interpolate = true;
                    analog_p1.absolute = false;
                    analog_p1.wraps = true;
                    analog_p1.interpolate = true;
                    break;
                case "SunA8":
                    loop_inputports_callback = SunA8.loop_inputports_suna8_starfigh;
                    record_port_callback = SunA8.record_port_starfigh;
                    replay_port_callback = SunA8.replay_port_starfigh;
                    break;
                case "Namco System 1":
                    loop_inputports_callback = Namcos1.loop_inputports_ns1_3b;
                    record_port_callback = Namcos1.record_port;
                    replay_port_callback = Namcos1.replay_port;
                    analog_p0 = new analog_field_state();
                    analog_p1 = new analog_field_state();
                    analog_p0.adjdefvalue = 0;
                    analog_p1.adjdefvalue = 0;
                    analog_p0.sensitivity = 30;
                    analog_p1.sensitivity = 30;
                    analog_p0.reverse = false;
                    analog_p1.reverse = false;
                    analog_p0.delta = 15;
                    analog_p1.delta = 15;
                    analog_p0.minimum = 0;
                    analog_p1.minimum = 0;
                    analog_p0.maximum = 0x1fe00;
                    analog_p1.maximum = 0x1fe00;
                    analog_p0.reverse_val = 0x20000;
                    analog_p1.reverse_val = 0x20000;
                    analog_p0.scalepos = 0x8000;
                    analog_p1.scalepos = 0x8000;
                    analog_p0.scaleneg = 0x8000;
                    analog_p1.scaleneg = 0x8000;
                    analog_p0.absolute = false;
                    analog_p0.wraps = true;
                    analog_p0.interpolate = true;
                    analog_p1.absolute = false;
                    analog_p1.wraps = true;
                    analog_p1.interpolate = true;
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
                            loop_inputports_callback = IGS011.loop_inputports_igs011_drgnwrld;
                            record_port_callback = IGS011.record_port_drgnwrld;
                            replay_port_callback = IGS011.replay_port_drgnwrld;
                            break;
                        case "lhb":
                        case "lhbv33c":
                        case "dbc":
                        case "ryukobou":
                            loop_inputports_callback = IGS011.loop_inputports_igs011_lhb;
                            record_port_callback = IGS011.record_port_lhb;
                            replay_port_callback = IGS011.replay_port_lhb;
                            break;
                        case "lhb2":
                            loop_inputports_callback = IGS011.loop_inputports_igs011_lhb2;
                            record_port_callback = IGS011.record_port_lhb;
                            replay_port_callback = IGS011.replay_port_lhb;
                            break;
                    }
                    break;
                case "PGM":
                    loop_inputports_callback = PGM.loop_inputports_pgm_standard;
                    record_port_callback = PGM.record_port;
                    replay_port_callback = PGM.replay_port;
                    break;
                case "M72":
                    loop_inputports_callback = M72.loop_inputports_m72_common;
                    record_port_callback = M72.record_port;
                    replay_port_callback = M72.replay_port;
                    break;
                case "M92":
                    loop_inputports_callback = M92.loop_inputports_m92_common;
                    record_port_callback = M92.record_port;
                    replay_port_callback = M92.replay_port;
                    break;
                case "Taito":
                    record_port_callback = Taito.record_port_bublbobl;
                    replay_port_callback = Taito.replay_port_bublbobl;
                    analog_p1x = new analog_field_state();
                    analog_p1x.adjdefvalue = 0x80;
                    analog_p1x.adjmin = 0;
                    analog_p1x.adjmax = 0xff;
                    analog_p1x.sensitivity = 25;
                    analog_p1x.reverse = false;
                    analog_p1x.delta = 15;
                    analog_p1x.minimum = -0x10000;
                    analog_p1x.maximum = 0x10000;
                    analog_p1x.absolute = true;
                    analog_p1x.wraps = false;
                    analog_p1x.interpolate = false;
                    analog_p1x.single_scale = 0;
                    analog_p1x.scalepos = 0x7f00;
                    analog_p1x.scaleneg = 0x8000;
                    analog_p1x.reverse_val = 0x200000;
                    analog_p1x.keyscalepos = 0x0000000204081020;
                    analog_p1x.keyscaleneg = 0x0000000200000000;
                    analog_p1y = new analog_field_state();
                    analog_p1y.adjdefvalue = 0x80;
                    analog_p1y.adjmin = 0;
                    analog_p1y.adjmax = 0xff;
                    analog_p1y.sensitivity = 25;
                    analog_p1y.reverse = false;
                    analog_p1y.delta = 15;
                    analog_p1y.minimum = -0x10000;
                    analog_p1y.maximum = 0x10000;
                    analog_p1y.absolute = true;
                    analog_p1y.wraps = false;
                    analog_p1y.interpolate = false;
                    analog_p1y.single_scale = 0;
                    analog_p1y.scalepos = 0x7f00;
                    analog_p1y.scaleneg = 0x8000;
                    analog_p1y.reverse_val = 0x200000;
                    analog_p1y.keyscalepos = 0x0000000204081020;
                    analog_p1y.keyscaleneg = 0x0000000200000000;
                    break;
                case "Taito B":
                    //loop_inputports_callback = Taitob.loop_inputports_taitob_pbobble;
                    record_port_callback = Taitob.record_port;
                    replay_port_callback = Taitob.replay_port;
                    break;
                case "Konami 68000":
                    //loop_inputports_callback = Konami68000.loop_inputports_konami68000_ssriders;
                    record_port_callback = Konami68000.record_port;
                    replay_port_callback = Konami68000.replay_port;
                    break;
                case "Capcom":
                    break;
            }
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
                    loop_inputports_callback = CPS.loop_inputports_cps1_forgottn;
                    break;
                case "sf2ebbl":
                case "sf2ebbl2":
                case "sf2ebbl3":
                case "sf2amf2":
                case "sf2m2":
                case "sf2m4":
                case "sf2m5":
                case "sf2m6":
                case "sf2m7":
                case "sf2yyc":
                case "sf2koryu":
                    loop_inputports_callback = CPS.loop_inputports_cps1_sf2hack;
                    break;
                case "cworld2j":
                case "cworld2ja":
                case "cworld2jb":
                case "qad":
                case "qadjr":
                case "qtono2j":
                    loop_inputports_callback = CPS.loop_inputports_cps1_cworld2j;
                    break;
                case "pzloop2":
                case "pzloop2j":
                case "pzloop2jr1":
                    loop_inputports_callback = CPS.loop_inputports_cps2_pzloop2;
                    break;
                case "ecofghtr":
                    loop_inputports_callback = CPS.loop_inputports_cps2_ecofghtr;
                    break;
                case "qndream":
                    loop_inputports_callback = CPS.loop_inputports_cps2_qndream;
                    break;
                case "pbaction":
                case "pbaction2":
                case "pbaction3":
                case "pbaction4":
                case "pbaction5":
                    loop_inputports_callback = Tehkan.loop_inputports_tehkan_pbaction;
                    record_port_callback = Tehkan.record_port_pbaction;
                    replay_port_callback = Tehkan.replay_port_pbaction;
                    break;
                case "irrmaze":
                    loop_inputports_callback = Neogeo.loop_inputports_neogeo_irrmaze;
                    break;
                case "quester":
                case "questers":
                    loop_inputports_callback = Namcos1.loop_inputports_ns1_quester;
                    break;
                case "berabohm":
                    loop_inputports_callback = Namcos1.loop_inputports_ns1_berabohm;
                    break;
                case "faceoff":
                    loop_inputports_callback = Namcos1.loop_inputports_ns1_faceoff;
                    break;
                case "tankfrce4":
                    loop_inputports_callback = Namcos1.loop_inputports_ns1_tankfrce4;
                    break;
                /*case "":
                case "":
                    loop_inputports_callback = IGS011.loop_inputports_igs011_drgnwrldj;
                    break;*/

                case "tokio":
                case "tokioo":
                case "tokiou":
                case "tokiob":
                    loop_inputports_callback = Taito.loop_inputports_taito_tokio;
                    break;
                case "bublbobl":
                case "bublbobl1":
                case "bublboblr":
                case "bublboblr1":
                case "bub68705":
                case "bublcave":
                case "bublcave11":
                case "bublcave10":
                    loop_inputports_callback = Taito.loop_inputports_taito_bublbobl;
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
                    loop_inputports_callback = Taito.loop_inputports_taito_boblbobl;
                    break;
                case "opwolf":
                case "opwolfa":
                case "opwolfj":
                case "opwolfu":
                case "opwolfb":
                    loop_inputports_callback = Taito.loop_inputports_taito_opwolf;
                    record_port_callback = Taito.record_port_opwolf;
                    replay_port_callback = Taito.replay_port_opwolf;
                    break;
                case "opwolfp":
                    loop_inputports_callback = Taito.loop_inputports_taito_opwolfp;
                    record_port_callback = Taito.record_port_opwolfp;
                    replay_port_callback = Taito.replay_port_opwolfp;
                    break;
                case "pbobble":
                    loop_inputports_callback = Taitob.loop_inputports_taitob_pbobble;
                    record_port_callback = Taitob.record_port_pbobble;
                    replay_port_callback = Taitob.replay_port_pbobble;
                    break;
                case "silentd":
                case "silentdj":
                case "silentdu":
                    loop_inputports_callback = Taitob.loop_inputports_taitob_silentd;
                    break;
                case "cuebrick":
                case "mia":
                case "mia2":
                case "lgtnfght":
                case "lgtnfghta":
                case "lgtnfghtu":
                case "trigon":
                    loop_inputports_callback = Konami68000.loop_inputports_konami68000_cuebrick;
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
                case "punkshot":
                case "punkshot2":
                case "punkshotj":
                case "tmnt2":
                case "ssriders":
                    loop_inputports_callback = Konami68000.loop_inputports_konami68000_tmnt;
                    break;
                case "blswhstl":
                case "blswhstla":
                case "detatwin":
                    loop_inputports_callback = Konami68000.loop_inputports_konami68000_blswhstl;
                    break;
                case "glfgreat":
                case "glfgreatj":
                    loop_inputports_callback = Konami68000.loop_inputports_konami68000_glfgreat;
                    break;
                case "tmht2p":
                case "tmht2pa":
                case "tmnt2pj":
                case "tmnt2po":
                case "tmnt2a":
                case "tmht22pe":
                case "tmht24pe":
                case "tmnt22pu":
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
                    loop_inputports_callback = Konami68000.loop_inputports_konami68000_ssriders;
                    break;
                case "qgakumon":
                    loop_inputports_callback = Konami68000.loop_inputports_konami68000_qgakumon;
                    break;
                case "thndrx2":
                case "thndrx2a":
                case "thndrx2j":
                    loop_inputports_callback = Konami68000.loop_inputports_konami68000_thndrx2;
                    break;
                case "prmrsocr":
                case "prmrsocrj":
                    loop_inputports_callback = Konami68000.loop_inputports_konami68000_prmrsocr;
                    record_port_callback = Konami68000.record_port_prmrsocr;
                    replay_port_callback = Konami68000.replay_port_prmrsocr;
                    break;
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
                    loop_inputports_callback = Capcom.loop_inputports_gng;
                    record_port_callback = Capcom.record_port_gng;
                    replay_port_callback = Capcom.replay_port_gng;
                    break;
                case "diamond":
                    loop_inputports_callback = Capcom.loop_inputports_diamond;
                    record_port_callback = Capcom.record_port_gng;
                    replay_port_callback = Capcom.replay_port_gng;
                    break;
                case "sf":
                    loop_inputports_callback = Capcom.loop_inputports_sfus;
                    record_port_callback = Capcom.record_port_sf;
                    replay_port_callback = Capcom.replay_port_sf;
                    break;
                case "sfua":
                case "sfj":
                    loop_inputports_callback = Capcom.loop_inputports_sfjp;
                    record_port_callback = Capcom.record_port_sf;
                    replay_port_callback = Capcom.replay_port_sf;
                    break;
                case "sfjan":
                case "sfan":
                case "sfp":
                    loop_inputports_callback = Capcom.loop_inputports_sfan;
                    record_port_callback = Capcom.record_port_sf;
                    replay_port_callback = Capcom.replay_port_sf;
                    break;
            }
        }
        public static int apply_analog_min_max(analog_field_state analog, int value)
        {
            int adjmin = (analog.minimum * 100) / analog.sensitivity;
            int adjmax = (analog.maximum * 100) / analog.sensitivity;
            if (!analog.wraps)
            {
                if (value > adjmax)
                    value = adjmax;
                else if (value < adjmin)
                    value = adjmin;
            }
            else
            {
                int adj1 = (512 * 100) / analog.sensitivity;
                int adjdif = adjmax - adjmin + adj1;
                if (analog.reverse)
                {
                    while (value <= adjmin - adj1)
                        value += adjdif;
                    while (value > adjmax)
                        value -= adjdif;
                }
                else
                {
                    while (value >= adjmax + adj1)
                        value -= adjdif;
                    while (value < adjmin)
                        value += adjdif;
                }
            }
            return value;
        }
        public static uint input_port_read_direct(analog_field_state analog)
        {
            uint result;
            int value;
            long nsec_since_last;
            value = analog.accum;
            if (analog.interpolate && portdata.last_delta_nsec != 0)
            {
                nsec_since_last = Attotime.attotime_to_attoseconds(Attotime.attotime_sub(EmuTimer.get_current_time(), portdata.last_frame_time)) / Attotime.ATTOSECONDS_PER_NANOSECOND;
                value = (int)(analog.previous + ((long)(analog.accum - analog.previous) * nsec_since_last / portdata.last_delta_nsec));
            }
            result = (uint)apply_analog_settings(value, analog);
            return result;
        }
        public static int apply_analog_settings(int value, analog_field_state analog)
        {
            value = apply_analog_min_max(analog, value);
            value = (int)((long)value * analog.sensitivity / 100);
            if (analog.reverse)
            {
                value = analog.reverse_val - value;
            }
            if (value >= 0)
            {
                value = (int)((long)(value * analog.scalepos) >> 24);
            }
            else
            {
                value = (int)((long)(value * analog.scaleneg) >> 24);
            }
            value += analog.adjdefvalue;
            return value;
        }
        public static void frame_update_callback()
        {
            if (Mame.mame_is_paused())
            {
                return;
            }
            frame_update();
            Video.screenstate.frame_number++;
        }
        private static void frame_update()
        {
            Atime curtime = EmuTimer.get_current_time();
            portdata.last_delta_nsec = Attotime.attotime_to_attoseconds(Attotime.attotime_sub(curtime, portdata.last_frame_time)) / Attotime.ATTOSECONDS_PER_NANOSECOND;
            portdata.last_frame_time = curtime;
            if (Mame.playState != Mame.PlayState.PLAY_REPLAYRUNNING)
            {
                if (Mame.is_foreground)
                {
                    loop_inputports_callback();
                }
                /*int i1 = (int)(Video.screenstate.frame_number % 4);
                if (i1 == 0)
                {
                    CPS.short1 = unchecked((short)0xfffb);
                }
                else if (i1 == 1)
                {
                    CPS.short1 = unchecked((short)0xffce);
                }
                else if (i1 == 2)
                {
                    CPS.short1 = unchecked((short)0xfff7);
                }
                else if (i1 == 3)
                {
                    CPS.short1 = unchecked((short)0xffcd);
                }*/
            }
            if (Mame.playState == Mame.PlayState.PLAY_RECORDRUNNING)
            {
                record_port_callback();
            }
            else if (Mame.playState == Mame.PlayState.PLAY_REPLAYRUNNING)
            {
                replay_port_callback();
            }
        }
        public static void frame_update_analog_field_forgottn_p0(analog_field_state analog)
        {
            bool keypressed = false;
            int delta = 0;
            int value2;
            value2 = apply_analog_min_max(analog, analog.accum);
            analog.previous = analog.accum = value2;
            if (Keyboard.IsPressed(MotionKey.P1_BTN_2))//if (Keyboard.IsPressed(Corekey.K))
            {
                keypressed = true;
                delta -= analog.delta * 0x200;
                analog.lastdigital = 1;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_E))//if (Keyboard.IsPressed(Corekey.L))
            {
                keypressed = true;
                delta += analog.delta * 0x200;
                analog.lastdigital = 2;
            }
            if (Mouse.deltaY < 0)
            {
                keypressed = true;
                delta += Mouse.deltaY * 0x200;
                analog.lastdigital = 1;
            }
            if (Mouse.deltaY > 0)
            {
                keypressed = true;
                delta += Mouse.deltaY * 0x200;
                analog.lastdigital = 2;
            }
            analog.accum += delta;
            if (!keypressed)
                analog.lastdigital = 0;
        }
        public static void frame_update_analog_field_forgottn_p1(analog_field_state analog)
        {
            bool keypressed = false;
            int delta = 0;
            int value2;
            value2 = apply_analog_min_max(analog, analog.accum);
            analog.previous = analog.accum = value2;
            if (Keyboard.IsPressed(MotionKey.P2_BTN_2))//if (Keyboard.IsPressed(Corekey.NumPad2))
            {
                keypressed = true;
                delta -= analog.delta * 0x200;
                analog.lastdigital = 1;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_E))//if (Keyboard.IsPressed(Corekey.NumPad3))
            {
                keypressed = true;
                delta += analog.delta * 0x200;
                analog.lastdigital = 2;
            }
            analog.accum += delta;
            if (!keypressed)
                analog.lastdigital = 0;
        }
        public static void frame_update_analog_field_ecofghtr_p0(analog_field_state analog)
        {
            bool keypressed = false;
            int delta = 0;
            int value2;
            value2 = apply_analog_min_max(analog, analog.accum);
            analog.previous = analog.accum = value2;
            if (Keyboard.IsPressed(MotionKey.P1_BTN_3))//if (Keyboard.IsPressed(MotionKey.P1_BTN_3))//if (Keyboard.IsPressed(Corekey.U))
            {
                keypressed = true;
                delta -= analog_p0.delta * 0x200;
                analog.lastdigital = 1;
            }
            if (Keyboard.IsPressed(MotionKey.P1_BTN_4))//if (Keyboard.IsPressed(Corekey.I))
            {
                keypressed = true;
                delta += analog_p0.delta * 0x200;
                analog.lastdigital = 2;
            }
            analog.accum += delta;
            if (!keypressed)
                analog.lastdigital = 0;
        }
        public static void frame_update_analog_field_ecofghtr_p1(analog_field_state analog)
        {
            bool keypressed = false;
            int delta = 0;
            int value2;
            value2 = apply_analog_min_max(analog, analog.accum);
            analog.previous = analog.accum = value2;
            if (Keyboard.IsPressed(MotionKey.P2_BTN_3))//if (Keyboard.IsPressed(Corekey.NumPad4))
            {
                keypressed = true;
                delta -= analog.delta * 0x200;
                analog.lastdigital = 1;
            }
            if (Keyboard.IsPressed(MotionKey.P2_BTN_4))//if (Keyboard.IsPressed(MotionKey.P2_BTN_4))//if (Keyboard.IsPressed(Corekey.NumPad5))
            {
                keypressed = true;
                delta += analog.delta * 0x200;
                analog.lastdigital = 2;
            }
            analog.accum += delta;
            if (!keypressed)
                analog.lastdigital = 0;
        }
        public static void frame_update_analog_field_irrmaze_p0(analog_field_state analog)
        {
            bool keypressed = false;
            int delta = 0;
            int value2;
            value2 = apply_analog_min_max(analog, analog.accum);
            analog.previous = analog.accum = value2;
            if (Keyboard.IsPressed(MotionKey.P1_LEFT))//if (Keyboard.IsPressed(Corekey.A))
            {
                keypressed = true;
                delta -= analog.delta * 0x200;
                analog.lastdigital = 1;
            }
            if (Keyboard.IsPressed(MotionKey.P1_RIGHT))//if (Keyboard.IsPressed(Corekey.D))
            {
                keypressed = true;
                delta += analog.delta * 0x200;
                analog.lastdigital = 2;
            }
            analog.accum += delta;
            if (!keypressed)
                analog.lastdigital = 0;
        }
        public static void frame_update_analog_field_irrmaze_p1(analog_field_state analog)
        {
            bool keypressed = false;
            int delta = 0;
            int value2;
            value2 = apply_analog_min_max(analog, analog.accum);
            analog.previous = analog.accum = value2;
            if (Keyboard.IsPressed(MotionKey.P1_DOWN))//if (Keyboard.IsPressed(Corekey.S))
            {
                keypressed = true;
                delta -= analog.delta * 0x200;
                analog.lastdigital = 1;
            }
            if (Keyboard.IsPressed(MotionKey.P1_UP))//if (Keyboard.IsPressed(Corekey.W))
            {
                keypressed = true;
                delta += analog.delta * 0x200;
                analog.lastdigital = 2;
            }
            analog.accum += delta;
            if (!keypressed)
                analog.lastdigital = 0;
        }
        public static void frame_update_analog_field_quester_p0(analog_field_state analog)
        {
            bool keypressed = false;
            int delta = 0;
            int value2;
            value2 = apply_analog_min_max(analog, analog.accum);
            analog.previous = analog.accum = value2;
            if (Keyboard.IsPressed(MotionKey.P1_LEFT))//if (Keyboard.IsPressed(Corekey.A))
            {
                keypressed = true;
                delta -= analog.delta * 0x200;
                analog.lastdigital = 1;
            }
            if (Keyboard.IsPressed(MotionKey.P1_RIGHT))//if (Keyboard.IsPressed(Corekey.D))
            {
                keypressed = true;
                delta += analog.delta * 0x200;
                analog.lastdigital = 2;
            }
            analog.accum += delta;
            if (!keypressed)
                analog.lastdigital = 0;
        }
        public static void frame_update_analog_field_quester_p1(analog_field_state analog)
        {
            bool keypressed = false;
            int delta = 0;
            int value2;
            value2 = apply_analog_min_max(analog, analog.accum);
            analog.previous = analog.accum = value2;
            if (Keyboard.IsPressed(MotionKey.P2_LEFT))//if (Keyboard.IsPressed(Corekey.Left))
            {
                keypressed = true;
                delta -= analog.delta * 0x200;
                analog.lastdigital = 1;
            }
            if (Keyboard.IsPressed(MotionKey.P2_RIGHT))//if (Keyboard.IsPressed(Corekey.Right))
            {
                keypressed = true;
                delta += analog.delta * 0x200;
                analog.lastdigital = 2;
            }
            analog.accum += delta;
            if (!keypressed)
                analog.lastdigital = 0;
        }
        public static void frame_update_analog_field_opwolf_p1x(analog_field_state analog)
        {
            bool keypressed = false;
            long keyscale;
            int rawvalue;
            int delta = 0;
            int value2;
            value2 = apply_analog_min_max(analog, analog.accum);
            analog.previous = analog.accum = value2;
            rawvalue = Mouse.deltaX;
            if (rawvalue != 0)
            {
                delta = rawvalue;
                analog.lastdigital = 0;
            }
            if (Keyboard.IsPressed(MotionKey.P1_LEFT))//if (Keyboard.IsPressed(Corekey.A))
            {
                keypressed = true;
                delta -= analog.delta * 0x200;
                analog.lastdigital = 1;
            }
            if (Keyboard.IsPressed(MotionKey.P1_RIGHT))//if (Keyboard.IsPressed(Corekey.D))
            {
                keypressed = true;
                delta += analog.delta * 0x200;
                analog.lastdigital = 2;
            }
            if (Mouse.deltaX < 0)
            {
                keypressed = true;
                delta += Mouse.deltaX * 0x200;
                analog.lastdigital = 1;
            }
            if (Mouse.deltaX > 0)
            {
                keypressed = true;
                delta += Mouse.deltaX * 0x200;
                analog.lastdigital = 2;
            }
            analog.accum += delta;
            if (!keypressed)
            {
                analog.lastdigital = 0;
            }
        }
        public static void frame_update_analog_field_opwolf_p1y(analog_field_state analog)
        {
            bool keypressed = false;
            long keyscale;
            int rawvalue;
            int delta = 0;
            int value2;
            value2 = apply_analog_min_max(analog, analog.accum);
            analog.previous = analog.accum = value2;
            rawvalue = Mouse.deltaY;
            if (rawvalue != 0)
            {
                delta = rawvalue;
                analog.lastdigital = 0;
            }
            if (Keyboard.IsPressed(MotionKey.P1_UP))//if (Keyboard.IsPressed(Corekey.W))
            {
                keypressed = true;
                delta -= analog.delta * 0x200;
                analog.lastdigital = 1;
            }
            if (Keyboard.IsPressed(MotionKey.P1_DOWN))//if (Keyboard.IsPressed(Corekey.S))
            {
                keypressed = true;
                delta += analog.delta * 0x200;
                analog.lastdigital = 2;
            }
            if (Mouse.deltaY < 0)
            {
                keypressed = true;
                delta += Mouse.deltaY * 0x200;
                analog.lastdigital = 1;
            }
            if (Mouse.deltaY > 0)
            {
                keypressed = true;
                delta += Mouse.deltaY * 0x200;
                analog.lastdigital = 2;
            }
            analog.accum += delta;
            if (!keypressed)
            {
                analog.lastdigital = 0;
            }
        }
    }
}
