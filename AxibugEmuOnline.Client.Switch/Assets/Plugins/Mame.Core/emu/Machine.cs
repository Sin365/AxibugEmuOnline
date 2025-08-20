using System.Collections.Generic;

namespace MAME.Core
{
    public class Machine
    {
        public static string sName, sParent, sBoard, sDirection, sDescription, sManufacturer;
        public static List<string> lsParents;
        public static MameMainMotion mainMotion;
        public static RomInfo rom;
        public static bool bRom;
        public delegate void machine_delegate();
        public static machine_delegate machine_reset_callback;
        public static void driver_init()
        {
            switch (Machine.sBoard)
            {
                case "Taito":
                    switch (Machine.sName)
                    {
                        case "opwolf":
                        case "opwolfa":
                        case "opwolfj":
                        case "opwolfu":
                            Taito.driver_init_opwolf();
                            break;
                        case "opwolfb":
                            Taito.driver_init_opwolfb();
                            break;
                        case "opwolfp":
                            Taito.driver_init_opwolfp();
                            break;
                    }
                    break;
            }
        }
        public static void machine_start()
        {
            switch (Machine.sBoard)
            {
                case "CPS-1":
                case "CPS-1(QSound)":
                case "CPS2":
                    Eeprom.eeprom_init();
                    CPS.video_start_cps();
                    machine_reset_callback = CPS.machine_reset_cps;
                    break;
                case "Data East":
                    machine_reset_callback = Dataeast.machine_reset_dataeast;
                    break;
                case "Tehkan":
                    Tehkan.video_start_pbaction();
                    machine_reset_callback = Tehkan.machine_reset_tehkan;
                    break;
                case "Neo Geo":
                    Neogeo.nvram_handler_load_neogeo();
                    Neogeo.machine_start_neogeo();
                    Neogeo.video_start_neogeo();
                    machine_reset_callback = Neogeo.machine_reset_neogeo;
                    break;
                case "SunA8":
                    switch (Machine.sName)
                    {
                        case "starfigh":
                            SunA8.video_start_suna8_starfigh();
                            break;
                    }
                    machine_reset_callback = SunA8.machine_reset_suna8;
                    break;
                case "Namco System 1":
                    Namcos1.driver_init();
                    Namcos1.video_start_namcos1();
                    machine_reset_callback = Namcos1.machine_reset_namcos1;
                    break;
                case "IGS011":
                    IGS011.video_start_igs011();
                    machine_reset_callback = IGS011.machine_reset_igs011;
                    break;
                case "PGM":
                    PGM.device_init();
                    PGM.video_start_pgm();
                    machine_reset_callback = PGM.machine_reset_pgm;
                    break;
                case "M72":
                    M72.machine_start_m72();
                    M72.video_start_m72();
                    machine_reset_callback = M72.machine_reset_m72;
                    switch (Machine.sName)
                    {
                        case "ltswords":
                        case "kengo":
                        case "kengoa":
                            machine_reset_callback = M72.machine_reset_kengo;
                            break;
                    }
                    break;
                case "M92":
                    M92.machine_start_m92();
                    M92.video_start_m92();
                    machine_reset_callback = M92.machine_reset_m92;
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
                            Taito.video_start_bublbobl();
                            machine_reset_callback = Taito.machine_reset_null;
                            break;
                        case "opwolf":
                        case "opwolfa":
                        case "opwolfj":
                        case "opwolfu":
                        case "opwolfp":
                            Taito.video_start_opwolf();
                            machine_reset_callback = Taito.machine_reset_opwolf;
                            break;
                        case "opwolfb":
                            Taito.video_start_opwolf();
                            machine_reset_callback = Taito.machine_reset_null;
                            break;
                    }
                    break;
                case "Taito B":
                    Eeprom.eeprom_init();
                    switch (Machine.sName)
                    {
                        case "pbobble":
                            Taitob.video_start_taitob_color_order1();
                            machine_reset_callback = Taitob.machine_reset_mb87078;
                            break;
                        case "silentd":
                        case "silentdj":
                        case "silentdu":
                            Taitob.video_start_taitob_color_order2();
                            machine_reset_callback = Taitob.machine_reset_mb87078;
                            break;
                    }
                    break;
                case "Konami 68000":
                    Eeprom.eeprom_init();
                    machine_reset_callback = Konami68000.machine_reset_konami68000;
                    switch (Machine.sName)
                    {
                        case "cuebrick":
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
                            Konami68000.video_start_tmnt();
                            break;
                        case "punkshot":
                        case "punkshot2":
                        case "punkshotj":
                            Konami68000.video_start_punkshot();
                            break;
                        case "lgtnfght":
                        case "lgtnfghta":
                        case "lgtnfghtu":
                        case "trigon":
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
                            Konami68000.video_start_lgtnfght();
                            break;
                        case "blswhstl":
                        case "blswhstla":
                        case "detatwin":
                            Konami68000.video_start_blswhstl();
                            break;
                        case "glfgreat":
                        case "glfgreatj":
                            Konami68000.video_start_glfgreat();
                            break;
                        case "thndrx2":
                        case "thndrx2a":
                        case "thndrx2j":
                            Konami68000.video_start_thndrx2();
                            break;
                        case "prmrsocr":
                        case "prmrsocrj":
                            Konami68000.video_start_prmrsocr();
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
                            Capcom.video_start_gng();
                            break;
                        case "sf":
                        case "sfua":
                        case "sfj":
                        case "sfjan":
                        case "sfan":
                        case "sfp":
                            Capcom.video_start_sf();
                            break;
                    }
                    machine_reset_callback = Capcom.machine_reset_capcom;
                    break;
            }
        }
        public static byte[] GetNeogeoRom(string sFile)
        {
            byte[] bb1;
            string path = System.IO.Path.Combine(Mame.RomRoot + "/neogeo/", sFile);
            //if (File.Exists(path))
            if (MameMainMotion.IoSupport.File_Exists(path))
            {
                EmuLogger.Log($"Had File => {path}");
                //return File.ReadAllBytes(path);
                return MameMainMotion.IoSupport.File_ReadAllBytes(path);
                //FileStream fs1 = new FileStream(path, FileMode.Open);
                //int n1 = (int)fs1.Length;
                //bb1 = new byte[n1];
                //fs1.Read(bb1, 0, n1);
                //fs1.Close();
            }
            else
            {
                EmuLogger.Log($"Miss File => {path}");
                bb1 = null;
            }
            return bb1;
        }
        public static byte[] GetRom(string sFile)
        {
            foreach (string s1 in lsParents)
            {
                string path = System.IO.Path.Combine(Mame.RomRoot + "/" + s1 + "/", sFile);
                if (MameMainMotion.IoSupport.File_Exists(path))
                {
                    EmuLogger.Log($"Had File => {path}");
                    return MameMainMotion.IoSupport.File_ReadAllBytes(path);
                }
                else
                {
                    EmuLogger.Log($"Miss File => {path}");
                }
            }
            return null;
        }
    }
}
