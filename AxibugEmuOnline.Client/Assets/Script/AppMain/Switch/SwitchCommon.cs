using AxibugEmuOnline.Client;
using UnityEngine;
public class SwitchCommon : MonoBehaviour
{
    void Start()
    {
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
