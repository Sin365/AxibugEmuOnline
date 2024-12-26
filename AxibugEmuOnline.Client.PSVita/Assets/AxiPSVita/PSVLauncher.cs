using AxibugEmuOnline.Client.ClientCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using static AxibugEmuOnline.Client.HttpAPI;


public class PSVLauncher : MonoBehaviour
{
	public UnityEngine.UI.Button btnInfo;
	public UnityEngine.UI.Button InitAPP;
	public UnityEngine.UI.Button btnHttpTest;
	public UnityEngine.UI.Button btnHttpTest10;
	public UnityEngine.UI.Button btnTaskTest;
	public UnityEngine.UI.Button btnTaskTest2;
	public UnityEngine.UI.Button btnPSVHUD;
	public UnityEngine.UI.Button btnStart;

	void Awake()
	{

		DontDestroyOnLoad(this);

		btnInfo.onClick.AddListener(() =>
		{
			Debug.Log($"SystemInfo.deviceUniqueIdentifier ->" + SystemInfo.deviceUniqueIdentifier);
			Debug.Log($"systemLanguage ->" + UnityEngine.PSVita.Utility.systemLanguage);
			Debug.Log($"skuFlags ->" + UnityEngine.PSVita.Utility.skuFlags);
		}
		);

		InitAPP.onClick.AddListener(() =>
		{
			App.Init(new AxibugEmuOnline.Client.Initer());
		});
		btnHttpTest.onClick.AddListener(() => StartCoroutine(StartNetInit()));
		btnHttpTest10.onClick.AddListener(() =>
		{
			StartCoroutine(StartNetInit());
			StartCoroutine(StartNetInit());
			StartCoroutine(StartNetInit());
			StartCoroutine(StartNetInit());
			StartCoroutine(StartNetInit());
			StartCoroutine(StartNetInit());
			StartCoroutine(StartNetInit());
			StartCoroutine(StartNetInit());
			StartCoroutine(StartNetInit());
			StartCoroutine(StartNetInit());
		}
		);

		btnTaskTest.onClick.AddListener(() =>
		{
			PSVThread.DoTask(() => DoWork(null));

			//Task task = new Task(() =>
			//{
			//	Thread.Sleep(1000);
			//	Debug.Log($"{Thread.CurrentThread.ManagedThreadId}->{1000}");
			//	Thread.Sleep(1000);
			//	Debug.Log($"{Thread.CurrentThread.ManagedThreadId}->{2000}");
			//});
			//task.Start();
		});

		btnPSVHUD.onClick.AddListener(() => {
			UnityEngine.PSVita.Diagnostics.enableHUD = true;
		});

		btnTaskTest2.onClick.AddListener(() =>
		{
			ThreadPool.QueueUserWorkItem(DoWork);
		}
		);

		btnStart.onClick.AddListener(()
			=>
		{
			SceneManager.LoadScene("AxibugEmuOnline.Client");
		});
	}

	static void DoWork(object state)
	{
		// 这里是线程池中的工作代码
		Debug.Log($"thread id: {Thread.CurrentThread.ManagedThreadId} start");
		// 模拟一些工作
		Thread.Sleep(2000);
		Debug.Log($"thread id: {Thread.CurrentThread.ManagedThreadId} end");
	}

	static IEnumerator StartNetInit()
	{
		int platform = 0;
		AxiHttpProxy.SendWebRequestProxy request = AxiHttpProxy.Get($"{"http://emu.axibug.com/api"}/CheckStandInfo?platform={platform}&version={Application.version}");
		yield return request.SendWebRequest;
		if (!request.downloadHandler.isDone)
			yield break;

		if (request.downloadHandler.Err != null)
		{
			Debug.LogError(request.downloadHandler.Err);
			yield break;
		}

		AxiHttpProxy.ShowAxiHttpDebugInfo(request.downloadHandler);
		Resp_CheckStandInfo resp = JsonUtility.FromJson<Resp_CheckStandInfo>(request.downloadHandler.text);

		//需要更新
		if (resp.needUpdateClient == 1)
		{
			//TODO
		}

		yield return null;

	}
}
