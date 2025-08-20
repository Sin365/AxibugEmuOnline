using System.ComponentModel;

namespace Essgee.Emulation
{
    // TODO change all these b/c gameboy is a thing now

    public enum TVStandard
    {
        //todo unity [ValueIgnored(true)]
        Auto = -1,
        [Description("NTSC (60 Hz)")]
        NTSC = 0,
        [Description("PAL (50 Hz)")]
        PAL
    }
    public enum Region
    {
        //todo unity [ValueIgnored(true)]
        Auto = -1,
        [Description("Domestic (Japan)")]
        Domestic = 0,
        [Description("Export")]
        Export
    }

    public enum InputDevice
    {
        [Description("None")]
        None = 0,
        [Description("Standard Controller")]
        Controller,
        [Description("Light Phaser")]
        Lightgun
    }

    public enum VDPTypes
    {
        [Description("Mark III / Master System")]
        Mk3SMS1 = 0,
        [Description("Master System II / Game Gear")]
        SMS2GG = 1
    }

    public enum InterruptType
    {
        Maskable,
        NonMaskable
    }

    public enum InterruptState
    {
        Clear,
        Assert
    }
}
