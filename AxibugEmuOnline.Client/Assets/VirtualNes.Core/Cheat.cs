namespace VirtualNes.Core
{
    public class CHEATCODE
    {
        // 埲壓偺俀偮偼OR儅僗僋
        public const int CHEAT_ENABLE = 1 << 0;
        public const int CHEAT_KEYDISABLE = 1 << 1;

        // 彂偒崬傒庬椶
        public const int CHEAT_TYPE_ALWAYS = 0;     // 忢偵彂偒崬傒
        public const int CHEAT_TYPE_ONCE = 1;       // 侾夞偩偗彂偒崬傒
        public const int CHEAT_TYPE_GREATER = 2;    // 僨乕僞傛傝戝偒偄帪
        public const int CHEAT_TYPE_LESS = 3;       // 僨乕僞傛傝彫偝偄帪

        // 僨乕僞挿
        public const int CHEAT_LENGTH_1BYTE = 0;
        public const int CHEAT_LENGTH_2BYTE = 1;
        public const int CHEAT_LENGTH_3BYTE = 2;
        public const int CHEAT_LENGTH_4BYTE = 3;

        public byte enable;
        public byte type;
        public byte length;
        public ushort address;
        public uint data;

        public string comment;
    }

    class GENIECODE
    {
        public ushort address;
        public byte data;
        public byte cmp;
    };
}
