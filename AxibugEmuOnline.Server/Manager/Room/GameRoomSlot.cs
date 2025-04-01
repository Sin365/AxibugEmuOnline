using AxibugProtobuf;

namespace AxibugEmuOnline.Server.Manager.Room
{
    public class GameRoomSlot
    {
        public uint SlotIdx { get; set; }
        public long UID { get; set; }
        public uint LocalJoyIdx { get; set; }
        public GamePadType LocalGamePadType { get; set; }
        public bool Ready = false;
        public void Init(uint SlotIdx)
        {
            this.SlotIdx = SlotIdx;
            UID = -1;
            LocalJoyIdx = 0;
            Ready = false;
        }
    }
}
