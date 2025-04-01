using AxibugProtobuf;
using System.Net.Sockets;

namespace AxibugEmuOnline.Server.Manager.Client
{
    public class ClientInfo
    {
        public long UID { get; set; }
        public string NickName { get; set; } = string.Empty;
        public string Account { get; set; } = string.Empty;
        internal DeviceType deviceType { get; set; } = DeviceType.Default;
        public Socket _socket { get; set; }
        public bool IsOffline { get; set; } = false;
        public DateTime RegisterDT { get; set; }
        public DateTime LogOutDT { get; set; }
        public DateTime LogInDT { get; set; }
        public DateTime LastLogInDT { get; set; }
        public UserRoomState RoomState { get; set; } = new UserRoomState();
        public TimeSpan LastStartPingTime { get; set; }
        public int LastPingSeed { get; set; }
        public double AveNetDelay { get; set; }
        public double MinNetDelay { get; set; }
        public double MaxNetDelay { get; set; }
        public List<double> NetDelays { get; set; } = new List<double>();
        public const int NetAveDelayCount = 3;
    }

    public class UserRoomState
    {
        public int RoomID { get; private set; }
        public UserRoomState()
        {
            ClearRoomData();
        }
        public void SetRoomData(int roomID)
        {
            RoomID = roomID;
        }
        public void ClearRoomData()
        {
            RoomID = -1;
        }
    }
}
