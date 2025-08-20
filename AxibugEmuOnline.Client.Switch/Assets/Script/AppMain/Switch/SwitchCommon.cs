using AxibugEmuOnline.Client;
using AxibugEmuOnline.Client.ClientCore;
using UnityEngine;
public class SwitchCommon : MonoBehaviour
{
    void Start()
    {
        Debug.Log("SwitchCommon Start");
        TickLoop.LoopAction_15s += ApplyCommit;
    }

    void OnDisable()
    {
        TickLoop.LoopAction_15s -= ApplyCommit;
    }

    private void ApplyCommit()
    {
#if UNITY_SWITCH
        AxiNS.instance.io.ApplyAutoCommit();
#endif
    }
}
