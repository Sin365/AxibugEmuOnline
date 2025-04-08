using System;
using System.Collections.Generic;
using System.Threading;

public class AxiNSWaitHandle
{
	static AutoResetEvent autoEvent = new AutoResetEvent(false);
	static Thread waitThread = new Thread(Loop);
	static bool bSingleInit = false;
	static Queue<AxiNSWaitBase> m_QueueReady = new Queue<AxiNSWaitBase>();
	static Queue<AxiNSWaitBase> m_QueueWork = new Queue<AxiNSWaitBase>();
	public void AddWait(AxiNSWaitBase wait)
	{
		InitInternalThread();
		lock (m_QueueReady)
		{
			m_QueueReady.Enqueue(wait);
		}
		autoEvent.Set();
	}

	static void InitInternalThread()
	{
		if (bSingleInit) return;
		waitThread.Start();
		bSingleInit = true;
	}

	static void Loop()
	{
		while (autoEvent.WaitOne())
		{
			lock (m_QueueReady)
			{
				while (m_QueueReady.Count > 0)
				{
					m_QueueWork.Enqueue(m_QueueReady.Dequeue());
				}
			}
			while (m_QueueWork.Count > 0)
			{
				AxiNSWaitBase wait = m_QueueWork.Dequeue();
				try
				{
					wait.Invoke();
				}
				catch (Exception ex)
				{
					wait.errmsg = ex.ToString();
					UnityEngine.Debug.Log(ex.ToString());
				}
				wait.SetDone();
			}
		}
	}
}