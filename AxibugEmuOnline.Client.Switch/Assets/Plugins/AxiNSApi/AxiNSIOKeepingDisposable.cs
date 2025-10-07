#if UNITY_SWITCH
using nn.fs;
#endif

using System;

public class AxiNSIOKeepingDisposable : IDisposable
{
    static object m_CurrLiveHandleLock = new object();
    static int m_CurrLiveHandleCounter = 0;
    static bool hadCounter { get { return m_CurrLiveHandleCounter > 0; } }
    public static AxiNSIOKeepingDisposable Acquire()
    {
        return new AxiNSIOKeepingDisposable();
    }
    static void UpdateKeepingState(bool add)
    {
        lock (m_CurrLiveHandleLock)
        {
            bool lasthadCounter = hadCounter;
            if (add)
                m_CurrLiveHandleCounter++;
            else
                m_CurrLiveHandleCounter--;

            if (lasthadCounter == hadCounter)
                return;

            if (hadCounter)
            {
                // This next line prevents the user from quitting the game while saving. 
                // This is required for Nintendo Switch Guideline 0080
                // ��������ֹ�û��ڱ���ʱ���˳���Ϸ Switch ���� 0080
                UnityEngine.Switch.Notification.EnterExitRequestHandlingSection();
                UnityEngine.Debug.Log("��������ֹ�û��ڱ���ʱ���˳���Ϸ Switch ���� 0080");
            }
            else
            {
                // ȡ������ֹ�û��ڱ���ʱ���˳���Ϸ Switch ���� 0080
                // End preventing the user from quitting the game while saving.
                UnityEngine.Switch.Notification.LeaveExitRequestHandlingSection();
                UnityEngine.Debug.Log("ȡ������ֹ�û��ڱ���ʱ���˳���Ϸ Switch ���� 0080");
            }
        }
    }
    private AxiNSIOKeepingDisposable()
    {
        UpdateKeepingState(true);
    }
    void IDisposable.Dispose()
    {
        UpdateKeepingState(false);
    }
}
