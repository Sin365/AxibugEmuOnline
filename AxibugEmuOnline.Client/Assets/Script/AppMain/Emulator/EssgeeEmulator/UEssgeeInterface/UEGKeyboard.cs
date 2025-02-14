using UnityEngine;

public class UEGKeyboard : MonoBehaviour
{
    public KeyCodeCore mKeyCodeCore = new KeyCodeCore();
    #region
    //public UILongClickButton btnP1;
    //public UILongClickButton btnCoin1;
    //public UILongClickButton btnA;
    //public UILongClickButton btnB;
    //public UILongClickButton btnC;
    //public UILongClickButton btnD;
    ////public UILongClickButton btnE;
    ////public UILongClickButton btnF;
    //public UILongClickButton btnAB;
    //public UILongClickButton btnCD;
    //public UILongClickButton btnABC;
    //public Transform tfKeyPad;
    //public FloatingJoystick mJoystick;
    #endregion

    //public List<UILongClickButton> mUIBtns = new List<UILongClickButton>();

    void Awake()
    {
        //mJoystick = GameObject.Find("tfJoystick").GetComponent<FloatingJoystick>();
        //tfKeyPad = GameObject.Find("tfKeyPad").transform;
        //btnP1 = GameObject.Find("btnP1").GetComponent<UILongClickButton>();
        //btnCoin1 = GameObject.Find("btnCoin1").GetComponent<UILongClickButton>();
        //btnA = GameObject.Find("btnA").GetComponent<UILongClickButton>();
        //btnB = GameObject.Find("btnB").GetComponent<UILongClickButton>();
        //btnC = GameObject.Find("btnC").GetComponent<UILongClickButton>();
        //btnD = GameObject.Find("btnD").GetComponent<UILongClickButton>();
        ////btnE = GameObject.Find("btnE")?.GetComponent<UILongClickButton>();
        ////btnF = GameObject.Find("btnF")?.GetComponent<UILongClickButton>();
        //btnAB = GameObject.Find("btnAB").GetComponent<UILongClickButton>();
        //btnCD = GameObject.Find("btnCD").GetComponent<UILongClickButton>();
        //btnABC = GameObject.Find("btnABC").GetComponent<UILongClickButton>();

        //mUIBtns.Add(btnP1);
        //mUIBtns.Add(btnCoin1);
        //mUIBtns.Add(btnA);
        //mUIBtns.Add(btnB);
        //mUIBtns.Add(btnC);
        //mUIBtns.Add(btnD);
        //mUIBtns.Add(btnAB);
        //mUIBtns.Add(btnCD);
        //mUIBtns.Add(btnABC);

        //if (btnE != null)
        //{
        //    mUIBtns.Add(btnE);
        //    btnE.gameObject.SetActive(false);
        //}
        //else
        //{ 
        //    mUIBtns.Add(btnF);
        //    btnF.gameObject.SetActive(false);
        //}


#if UNITY_STANDALONE_WIN || UNITY_EDITOR
        //tfKeyPad.gameObject.SetActive(false);
#endif
    }

    public void Init(Essgee.Emulation.Machines.IMachine Machine)
    {
        mKeyCodeCore.Init(Machine, this, false);
    }


    public void UpdateInputKey()
    {
        mKeyCodeCore.UpdateLogic();
    }
}