using System;
using UnityEngine;

public class AxiNSMono : MonoBehaviour
{
	Action act;
	float waittime;
	float lastinvokeTime;

	public static void SetInvoke(Action _act, int _waitsec)
	{
		GameObject gobj = GameObject.Find($"[{nameof(AxiNSMono)}]");
		if (gobj == null)
		{
			gobj = new GameObject();
			gobj.name = $"[{nameof(AxiNSMono)}]";
			GameObject.DontDestroyOnLoad(gobj);
		}
		AxiNSMono com = gobj.GetComponent<AxiNSMono>();
		if (com == null)
		{
			com = gobj.AddComponent<AxiNSMono>();
		}
		com.act = _act;
		com.waittime = _waitsec;
	}

	public void OnEnable()
	{
		Debug.Log("AxiNSMono Enable");
	}

	public void Update()
	{
		if (Time.time - lastinvokeTime < waittime)
			return;
		lastinvokeTime = Time.time;
		if (act != null)
			act.Invoke();
	}
}