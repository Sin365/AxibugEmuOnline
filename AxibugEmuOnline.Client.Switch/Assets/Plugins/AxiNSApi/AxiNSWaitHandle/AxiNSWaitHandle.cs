using System;
using System.Collections.Generic;
using System.Threading;

public class AxiNSWaitHandle
{
	static Queue<AxiNSWaitBase> m_QueueReady = new Queue<AxiNSWaitBase>();
	static Queue<AxiNSWaitBase> m_QueueWork = new Queue<AxiNSWaitBase>();
	public void AddWait(AxiNSWaitBase wait)
	{
		lock (m_QueueReady)
		{
			m_QueueReady.Enqueue(wait);
		}

		if (AxiNS.usedmultithreading)
		{
			InitInternalThread();
			autoEvent.Set();
		}
		else
		{
			InitMonoInit();
		}
	}

	#region 多线程实现
	static AutoResetEvent autoEvent = new AutoResetEvent(false);
	static Thread waitThread = new Thread(Loop);
	static bool bSingleInit = false;
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
			Do();
		}
	}
	#endregion

	#region 主线程时间间隔实现
	static bool bMonoInit = false;
	static void InitMonoInit()
	{
		if (bMonoInit) return;
		AxiNSMono.SetInvokeLoop(Do,15);
		bMonoInit = true;
	}
	#endregion

	static void Do()
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