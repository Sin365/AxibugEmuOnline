using AxibugEmuOnline.Client.ClientCore;
using AxibugProtobuf;
using System;
using System.Collections;
using System.Collections.Generic;

namespace AxibugEmuOnline.Client
{
    public class RoomListMenuItem : VirtualSubMenuItem
    {
        protected override void GetVirtualListDatas(Action<object> datas)
        {
            App.StartCoroutine(Test(datas));
        }

        private IEnumerator Test(Action<object> datas)
        {
            yield return null;

            List<Protobuf_Room_MiniInfo> fakeData = new List<Protobuf_Room_MiniInfo>()
            {
                new Protobuf_Room_MiniInfo{ GameRomID = 1, RoomID = 1, HostPlayerUID = 1, Player1UID = 1, Player1NickName = "Test1"},
                new Protobuf_Room_MiniInfo{ GameRomID = 2, RoomID = 2, HostPlayerUID = 2, Player1UID = 2, Player1NickName = "Test2"},
                new Protobuf_Room_MiniInfo{ GameRomID = 3, RoomID = 3, HostPlayerUID = 3, Player1UID = 3, Player1NickName = "Test3"},
                new Protobuf_Room_MiniInfo{ GameRomID = 4, RoomID = 4, HostPlayerUID = 4, Player1UID = 4, Player1NickName = "Test4"},
                new Protobuf_Room_MiniInfo{ GameRomID = 5, RoomID = 5, HostPlayerUID = 5, Player1UID = 5, Player1NickName = "Test5"},
            };

            datas.Invoke(fakeData);

            yield break;
        }
    }
}
