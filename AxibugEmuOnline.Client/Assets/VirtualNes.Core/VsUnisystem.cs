namespace VirtualNes.Core
{
    public class VSDIPSWITCH
    {
        public string name;
        public ushort value;
    }


    public static class VsUnisystem
    {
        public static VSDIPSWITCH[] vsdip_default = new VSDIPSWITCH[]
        {
            new VSDIPSWITCH{name="Unknown", value=         0x0100},
            new VSDIPSWITCH{name="Off",     value=         0x00},
            new VSDIPSWITCH{name="On",      value=         0x01},
            new VSDIPSWITCH{name=null,      value=         0xFF},
            new VSDIPSWITCH{name="Unknown", value=         0x0200},
            new VSDIPSWITCH{name="Off",     value=         0x00},
            new VSDIPSWITCH{name="On",      value=         0x02},
            new VSDIPSWITCH{name=null,      value=         0xFF},
            new VSDIPSWITCH{name="Unknown", value=         0x0400},
            new VSDIPSWITCH{name="Off",     value=         0x00},
            new VSDIPSWITCH{name="On",      value=         0x04},
            new VSDIPSWITCH{name=null,      value=         0xFF},
            new VSDIPSWITCH{name="Unknown", value=         0x0800},
            new VSDIPSWITCH{name="Off",     value=         0x00},
            new VSDIPSWITCH{name="On",      value=         0x08},
            new VSDIPSWITCH{name=null,      value=         0xFF},
            new VSDIPSWITCH{name="Unknown", value=         0x1000},
            new VSDIPSWITCH{name="Off",     value=         0x00},
            new VSDIPSWITCH{name="On",      value=         0x10},
            new VSDIPSWITCH{name=null,      value=         0xFF},
            new VSDIPSWITCH{name="Unknown", value=         0x2000},
            new VSDIPSWITCH{name="Off",     value=         0x00},
            new VSDIPSWITCH{name="On",      value=         0x20},
            new VSDIPSWITCH{name=null,      value=         0xFF},
            new VSDIPSWITCH{name="Unknown", value=         0x4000},
            new VSDIPSWITCH{name="Off",     value=         0x00},
            new VSDIPSWITCH{name="On",      value=         0x40},
            new VSDIPSWITCH{name=null,      value=         0xFF},
            new VSDIPSWITCH{name="Unknown", value=         0x8000},
            new VSDIPSWITCH{name="Off",     value=         0x00},
            new VSDIPSWITCH{name="On",      value=         0x80},
            new VSDIPSWITCH{name=null,      value=         0xFF},
            new VSDIPSWITCH{name=null,      value=         0 },
        };
    }
}
