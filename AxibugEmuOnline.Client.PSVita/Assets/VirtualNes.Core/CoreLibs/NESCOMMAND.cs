namespace VirtualNes.Core
{
    public enum NESCOMMAND
    {
        NESCMD_NONE = 0,
        NESCMD_HWRESET,
        NESCMD_SWRESET,
        NESCMD_EXCONTROLLER,    // Commandparam
        NESCMD_DISK_THROTTLE_ON,
        NESCMD_DISK_THROTTLE_OFF,
        NESCMD_DISK_EJECT,
        NESCMD_DISK_0A,
        NESCMD_DISK_0B,
        NESCMD_DISK_1A,
        NESCMD_DISK_1B,
        NESCMD_DISK_2A,
        NESCMD_DISK_2B,
        NESCMD_DISK_3A,
        NESCMD_DISK_3B,

        NESCMD_SOUND_MUTE,	// CommandParam
    }
}
