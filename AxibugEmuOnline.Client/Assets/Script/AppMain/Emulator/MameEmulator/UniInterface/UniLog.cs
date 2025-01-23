using MAME.Core;
using UnityEngine;

public class UniLog : ILog
{
    public void Log(string msg)
    {
        Debug.Log(msg);
    }
}
