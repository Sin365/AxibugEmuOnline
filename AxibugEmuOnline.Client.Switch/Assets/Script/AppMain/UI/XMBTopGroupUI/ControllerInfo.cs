using AxibugEmuOnline.Client;
using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Event;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ControllerInfo : MonoBehaviour
{
    [Header("手柄槽位序号[0,3]")]
    [SerializeField]
    int m_slotIndex;

    [SerializeField]
    GameObject m_connectInfoNode;
    [SerializeField]
    Image m_indexIcon;
    [SerializeField]
    Text m_playerName;
    private bool m_islocal;
    private Tweener m_tween;

    public int SlotIndex
    {
        get => m_slotIndex;
        set
        {
            if (m_slotIndex == value) return;
            m_slotIndex = value;
            UpdateIndexIcon();
        }
    }

    private void Awake()
    {
        UpdateIndexIcon();
    }


    private void OnEnable()
    {
        Eventer.Instance.RegisterEvent(EEvent.OnMineJoinRoom, OnJoinRoom);
        Eventer.Instance.RegisterEvent(EEvent.OnMineRoomCreated, OnMineRoomCreated);
        Eventer.Instance.RegisterEvent(EEvent.OnMineLeavnRoom, OnLeaveRoom);
        Eventer.Instance.RegisterEvent(EEvent.OnLoginSucceed, OnLoginSuccess);
        Eventer.Instance.RegisterEvent(EEvent.OnLossLoginState, OnLossLoginState);
        Eventer.Instance.RegisterEvent(EEvent.OnControllerConnectChanged, OnControlConnectChanged);
        UpdateConnectInfo();

        m_indexIcon.rectTransform.anchoredPosition = Vector3.zero;
    }

    private void OnDisable()
    {
        Eventer.Instance.UnregisterEvent(EEvent.OnMineJoinRoom, OnJoinRoom);
        Eventer.Instance.UnregisterEvent(EEvent.OnMineRoomCreated, OnMineRoomCreated);
        Eventer.Instance.UnregisterEvent(EEvent.OnMineLeavnRoom, OnLeaveRoom);
        Eventer.Instance.UnregisterEvent(EEvent.OnLoginSucceed, OnLoginSuccess);
        Eventer.Instance.UnregisterEvent(EEvent.OnLossLoginState, OnLossLoginState);
        Eventer.Instance.UnregisterEvent(EEvent.OnControllerConnectChanged, OnControlConnectChanged);
    }

    private void Update()
    {
        if (m_islocal)
        {
            var controller = App.emu.Core.GetControllerSetuper().GetSlotConnectingController(m_slotIndex);
            if (controller == null) return;
            if (!controller.AnyButtonDown()) return;

            if (m_tween != null)
            {
                m_indexIcon.rectTransform.anchoredPosition = Vector2.zero;
                m_tween.Kill();
                m_tween = null;
            }
            m_tween = m_indexIcon.rectTransform.DOShakePosition(0.1f).SetLink(gameObject).OnComplete(() =>
            {
                m_tween = null;
            });
        }
    }

    private void OnMineRoomCreated() => UpdateConnectInfo();
    private void OnJoinRoom() => UpdateConnectInfo();
    private void OnLeaveRoom() => UpdateConnectInfo();
    private void OnLoginSuccess() => UpdateConnectInfo();
    private void OnLossLoginState() => UpdateConnectInfo();
    private void OnControlConnectChanged() => UpdateConnectInfo();

    private void UpdateConnectInfo()
    {
        if (App.roomMgr.InRoom)
        {
            var slotInfo = App.roomMgr.mineRoomMiniInfo.GamePlaySlotList[SlotIndex];
            if (slotInfo.PlayerUID <= 0)
                SetDisconnect();
            else
                UpdateStateView(App.user.userdata.UID == slotInfo.PlayerUID, slotInfo.PlayerNickName, slotInfo.PlayerLocalJoyIdx);
        }
        else
        {
            if (App.emu.Core == null)
            {
                SetDisconnect();
                return;
            }
            var connecter = App.emu.Core.GetControllerSetuper();

            var localControlIndex = connecter.GetSlotConnectingControllerIndex(SlotIndex);
            if (localControlIndex == null)
                SetDisconnect();
            else
            {
                if (App.user.IsLoggedIn)
                    UpdateStateView(true, App.user.userdata.NickName, localControlIndex.Value);
                else
                    UpdateStateView(true, "Player", localControlIndex.Value);
            }
        }
    }

    private void UpdateStateView(bool isLocal, string playerName, int slotIndex)
    {
        m_islocal = isLocal;

        m_connectInfoNode.SetActiveEx(true);
        m_playerName.text = playerName;
    }

    private void SetDisconnect()
    {
        m_connectInfoNode.SetActiveEx(false);
        m_playerName.text = null;
        m_islocal = false;
    }

    private void UpdateIndexIcon()
    {
        switch (SlotIndex)
        {
            case 0: m_indexIcon.sprite = Resources.Load<Sprite>("UIImage/JoyImg/P1"); break;
            case 1: m_indexIcon.sprite = Resources.Load<Sprite>("UIImage/JoyImg/P2"); break;
            case 2: m_indexIcon.sprite = Resources.Load<Sprite>("UIImage/JoyImg/P3"); break;
            case 3: m_indexIcon.sprite = Resources.Load<Sprite>("UIImage/JoyImg/P4"); break;
        }
    }
}
