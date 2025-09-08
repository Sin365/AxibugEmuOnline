using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Event;
using AxibugEmuOnline.Client.Settings;
using StoicGooseUnity;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SGKeyboard : MonoBehaviour
{
    public SGControllerMapper ControllerMapper { get; private set; }
    //Dictionary<KeyCode, StoicGooseKey> dictKey2SGKey = new Dictionary<KeyCode, StoicGooseKey>();
    //KeyCode[] checkKeys;
    long currInput;
    private void Awake()
    {
        ControllerMapper = new SGControllerMapper();
        //SetVerticalOrientation(false);
    }


    internal void PollInput(ref long buttonsHeld)
    {
        buttonsHeld = currInput;
    }

    internal void SetVerticalOrientation(bool isVerticalOrientation)
    {
        //TODO  横屏竖屏

        //dictKey2SGKey[KeyCode.Return] = StoicGooseKey.Start;
        //dictKey2SGKey[KeyCode.W] = StoicGooseKey.X1;
        //dictKey2SGKey[KeyCode.S] = StoicGooseKey.X2;
        //dictKey2SGKey[KeyCode.A] = StoicGooseKey.X3;
        //dictKey2SGKey[KeyCode.D] = StoicGooseKey.X4;
        //dictKey2SGKey[KeyCode.G] = StoicGooseKey.Y1;
        //dictKey2SGKey[KeyCode.V] = StoicGooseKey.Y2;
        //dictKey2SGKey[KeyCode.C] = StoicGooseKey.Y3;
        //dictKey2SGKey[KeyCode.B] = StoicGooseKey.Y4;
        //dictKey2SGKey[KeyCode.Return] = StoicGooseKey.Start;
        //dictKey2SGKey[KeyCode.J] = StoicGooseKey.B;
        //dictKey2SGKey[KeyCode.K] = StoicGooseKey.A;
        //checkKeys = dictKey2SGKey.Keys.ToArray();
    }

    internal void SetInputData(ulong inputData)
    {
        currInput = (long)inputData;
    }

    public ulong Update_InputData()
    {
        ulong tempLocalInputAllData = 0;
        tempLocalInputAllData |= ControllerMapper.Controller0.GetSingleAllInput();
        tempLocalInputAllData |= ControllerMapper.Controller1.GetSingleAllInput();
        tempLocalInputAllData |= ControllerMapper.Controller2.GetSingleAllInput();
        tempLocalInputAllData |= ControllerMapper.Controller3.GetSingleAllInput();
        return tempLocalInputAllData;
        //currInput = tempLocalInputAllData;

        //currInput = 0;
        //for (int i = 0; i < checkKeys.Length; i++)
        //{
        //    KeyCode key = checkKeys[i];
        //    if (Input.GetKey(key))
        //    {
        //        currInput |= (long)dictKey2SGKey[key];
        //    }
        //}
    }

}



public class SGControllerMapper : IControllerSetuper
{
    public SGController Controller0 = new SGController(0);
    public SGController Controller1 = new SGController(1);
    public SGController Controller2 = new SGController(2);
    public SGController Controller3 = new SGController(3);

    ulong mCurrAllInput;

    public void SetConnect(uint? con0ToSlot = null,
            uint? con1ToSlot = null,
            uint? con2ToSlot = null,
            uint? con3ToSlot = null)
    {
        Controller0.ConnectSlot = con0ToSlot;
        Controller1.ConnectSlot = con1ToSlot;
        Controller2.ConnectSlot = con2ToSlot;
        Controller3.ConnectSlot = con3ToSlot;
    }
    public int? GetSlotConnectingControllerIndex(int slotIndex)
    {
        if (Controller0.ConnectSlot.HasValue && Controller0.ConnectSlot.Value == slotIndex) return 0;
        else if (Controller1.ConnectSlot.HasValue && Controller1.ConnectSlot.Value == slotIndex) return 1;
        else if (Controller2.ConnectSlot.HasValue && Controller2.ConnectSlot.Value == slotIndex) return 2;
        else if (Controller3.ConnectSlot.HasValue && Controller3.ConnectSlot.Value == slotIndex) return 3;
        else return null;
    }
    public IController GetSlotConnectingController(int slotIndex)
    {
        if (Controller0.ConnectSlot.HasValue && Controller0.ConnectSlot.Value == slotIndex) return Controller0;
        else if (Controller1.ConnectSlot.HasValue && Controller1.ConnectSlot.Value == slotIndex) return Controller1;
        else if (Controller2.ConnectSlot.HasValue && Controller2.ConnectSlot.Value == slotIndex) return Controller2;
        else if (Controller3.ConnectSlot.HasValue && Controller3.ConnectSlot.Value == slotIndex) return Controller3;
        else return null;
    }
    static HashSet<uint> s_temp = new HashSet<uint>();
    public uint? GetFreeSlotIndex()
    {
        s_temp.Clear();
        s_temp.Add(0);
        s_temp.Add(1);
        s_temp.Add(2);
        s_temp.Add(3);

        if (Controller0.ConnectSlot.HasValue) s_temp.Remove(Controller0.ConnectSlot.Value);
        if (Controller1.ConnectSlot.HasValue) s_temp.Remove(Controller1.ConnectSlot.Value);
        if (Controller2.ConnectSlot.HasValue) s_temp.Remove(Controller2.ConnectSlot.Value);
        if (Controller3.ConnectSlot.HasValue) s_temp.Remove(Controller3.ConnectSlot.Value);

        if (s_temp.Count > 0) return s_temp.First();
        else return null;
    }
    public void LetControllerConnect(int conIndex, uint slotIndex)
    {
        SGController targetController;
        switch (conIndex)
        {
            case 0: targetController = Controller0; break;
            case 1: targetController = Controller1; break;
            case 2: targetController = Controller2; break;
            case 3: targetController = Controller3; break;
            default:
                throw new System.Exception($"Not Allowed conIndex Range: {conIndex}");
                break;
        }
        if (targetController.ConnectSlot.HasValue) return;

        targetController.ConnectSlot = slotIndex;
        Eventer.Instance.PostEvent(EEvent.OnControllerConnectChanged);
    }

}
public class SGController : IController
{
    public ulong CurrLocalSingleAllInput { get; private set; }

    int mControllerIndex;
    uint? mConnectSlot;

    /// <summary>
    /// 指示该手柄连接的手柄插槽
    /// <para><c>这个值代表了该手柄在实际游戏中控制的Player</c></para>
    /// <value>[0,3] 例外:为空代表未连接</value>
    /// </summary>
    public uint? ConnectSlot
    {
        get { return mConnectSlot; }
        set { mConnectSlot = value; }
    }

    /// <summary>
    /// 控制器编号
    /// <para><c>此编号并非对应游戏中的player1,player2,player3,player4,仅仅作为本地4个手柄的实例</c></para>
    /// <value>[0,3]</value>
    /// </summary>
    public int ControllerIndex
    {
        get { return mControllerIndex; }
        set { mControllerIndex = value; /*this.LoadControlKeyForConfig();*/ }
    }

    public SGController(int controllerIndex)
    {
        ControllerIndex = controllerIndex;
    }

    public bool AnyButtonDown()
    {
        return GetKeyMapper().AnyKeyDown(mControllerIndex);
    }

    public StoicGooseBinding GetKeyMapper()
    {
        return App.settings.KeyMapper.GetBinder<StoicGooseBinding>(UStoicGoose.instance.Platform);
    }

    public ulong GetSingleAllInput()
    {
        if (!ConnectSlot.HasValue)
            return 0;
        CurrLocalSingleAllInput = 0;

        StoicGooseBinding essgeeKeys = GetKeyMapper();

        if (essgeeKeys.GetKey(StoicGooseKey.X1, mControllerIndex)) CurrLocalSingleAllInput |= (ulong)StoicGooseKey.X1;
        if (essgeeKeys.GetKey(StoicGooseKey.X2, mControllerIndex)) CurrLocalSingleAllInput |= (ulong)StoicGooseKey.X2;
        if (essgeeKeys.GetKey(StoicGooseKey.X3, mControllerIndex)) CurrLocalSingleAllInput |= (ulong)StoicGooseKey.X3;
        if (essgeeKeys.GetKey(StoicGooseKey.X4, mControllerIndex)) CurrLocalSingleAllInput |= (ulong)StoicGooseKey.X4;
        if (essgeeKeys.GetKey(StoicGooseKey.Y1, mControllerIndex)) CurrLocalSingleAllInput |= (ulong)StoicGooseKey.Y1;
        if (essgeeKeys.GetKey(StoicGooseKey.Y2, mControllerIndex)) CurrLocalSingleAllInput |= (ulong)StoicGooseKey.Y2;
        if (essgeeKeys.GetKey(StoicGooseKey.Y3, mControllerIndex)) CurrLocalSingleAllInput |= (ulong)StoicGooseKey.Y3;
        if (essgeeKeys.GetKey(StoicGooseKey.Y4, mControllerIndex)) CurrLocalSingleAllInput |= (ulong)StoicGooseKey.Y4;
        if (essgeeKeys.GetKey(StoicGooseKey.Start, mControllerIndex)) CurrLocalSingleAllInput |= (ulong)StoicGooseKey.Start;

        return CurrLocalSingleAllInput;
    }
}