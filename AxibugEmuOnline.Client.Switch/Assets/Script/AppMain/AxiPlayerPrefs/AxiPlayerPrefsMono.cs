using System;
using UnityEngine;

public class AxiPlayerPrefsMono : MonoBehaviour
{
	Action act;
	float waittime;
	float lastinvokeTime;
	public static void SetInvoke(Action _act, int _waitsec)
	{
		GameObject gobj = GameObject.Find($"[{nameof(AxiPlayerPrefsMono)}]");
		if (gobj == null)
		{
			gobj = new GameObject();
			gobj.name = $"[{nameof(AxiPlayerPrefsMono)}]";
			GameObject.DontDestroyOnLoad(gobj);
		}
		AxiPlayerPrefsMono com = gobj.GetComponent<AxiPlayerPrefsMono>();
		if (com == null)
		{
			com = gobj.AddComponent<AxiPlayerPrefsMono>();
		}
		com.act = _act;
		com.waittime = _waitsec;
	}

	public void OnEnable()
	{
		Debug.Log("AxiPlayerPrefsMono Enable");
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