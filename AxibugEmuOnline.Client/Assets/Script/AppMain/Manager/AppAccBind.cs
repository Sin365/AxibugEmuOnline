using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Network;
using AxibugProtobuf;
using System;
using System.Collections;
using UnityEngine;

namespace AxibugEmuOnline.Client.Manager
{
    public class AppAccBind
    {
        float LastSendBindTime = 0;
        public AppAccBind()
        {
            NetMsg.Instance.RegNetMsgEvent<Protobuf_Bind_RESP>((int)CommandID.CmdBindAcc, RecvBindMsg);
        }
        Protobuf_Bind _Protobuf_Bind = new Protobuf_Bind();
        public void SendBind(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                OverlayManager.PopTip("绑定码不可为空");
                return;
            }
            if (Time.time - LastSendBindTime < 3f)
            {
                OverlayManager.PopTip("操作太快，稍后重试");
                return;
            }
            if (AfterBindCorout.bHadCoroutine)
            {
                OverlayManager.PopTip("有正在进行的绑定");
                return;
            }
            _Protobuf_Bind.BindCode = code;
            LastSendBindTime = Time.time;
            App.network.SendToServer((int)CommandID.CmdBindAcc, _Protobuf_Bind);
        }

        public void RecvBindMsg(Protobuf_Bind_RESP msg)
        {
            App.StartCoroutine(AfterBind(msg));
        }

        class AfterBindCorout : IDisposable
        {
            public static bool bHadCoroutine;
            public static AfterBindCorout Acquire()
            {
                return new AfterBindCorout();
            }
            public AfterBindCorout()
            {
                bHadCoroutine = true;
            }
            void IDisposable.Dispose()
            {
                bHadCoroutine = false;
            }
        }
        IEnumerator AfterBind(Protobuf_Bind_RESP msg)
        {
            using (AfterBindCorout.Acquire())
            {
                OverlayManager.PopTip("成功绑定到:" + msg.MoveToNickName);
                OverlayManager.PopTip("迁移游戏收藏" + msg.MoveStarCount + "个");
                OverlayManager.PopTip("迁移游戏存档" + msg.MoveStarCount + "个");
                OverlayManager.PopTip("等待3秒，自动重新登录");
                yield return null;
                yield return new WaitForSeconds(3f);
                App.network.CloseConntect();
            }
        }
    }
}