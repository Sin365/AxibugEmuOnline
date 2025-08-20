public abstract class AxiNSWaitBase : UnityEngine.CustomYieldInstruction
{
	protected bool IsDone;
	public abstract void Invoke();
	public string errmsg = string.Empty;
	public AxiNSWaitBase()
	{
		this.IsDone = false;
	}

	public void SetDone()
	{
		this.IsDone = true;
	}

	~AxiNSWaitBase()
	{
	}

	public override bool keepWaiting
	{
		get { return !IsDone; }
	}
}



public struct S_NSWAIT_PathWithBytes
{
	public string filePath;
	public byte[] data;
}

public class AxiNSWait_FileToSaveWithCreate : AxiNSWaitBase
{
	S_NSWAIT_PathWithBytes req;
	public bool result;
	public AxiNSWait_FileToSaveWithCreate(string filePath, byte[] data)
	{
		req = new S_NSWAIT_PathWithBytes() { filePath = filePath, data = data };
	}

	public override void Invoke()
	{
		result = AxiNS.instance.io.FileToSaveWithCreate(req.filePath, req.data);
	}
}

public struct S_NSWAIT_PathWithMS
{
	public string filePath;
	public System.IO.MemoryStream ms;
}

public class AxiNSWait_FileToSaveByMSWithCreate : AxiNSWaitBase
{
	S_NSWAIT_PathWithMS req;
	public bool result;
	public AxiNSWait_FileToSaveByMSWithCreate(string filePath, System.IO.MemoryStream ms)
	{
		req = new S_NSWAIT_PathWithMS() { filePath = filePath, ms = ms };
	}

	public override void Invoke()
	{
		result = AxiNS.instance.io.FileToSaveWithCreate(req.filePath, req.ms);
	}
}

public struct S_NSWAIT_Path
{
	public string filePath;
}

public class AxiNSWait_LoadSwitchDataFile : AxiNSWaitBase
{
	S_NSWAIT_Path req;
	public bool result;
	public byte[] outputData;
	public AxiNSWait_LoadSwitchDataFile(string filePath)
	{
		req = new S_NSWAIT_Path() { filePath = filePath};
	}

	public override void Invoke()
	{
		result = AxiNS.instance.io.LoadSwitchDataFile(req.filePath, out outputData);
	}
}

public class AxiNSWait_DeletePathFile : AxiNSWaitBase
{
	S_NSWAIT_Path req;
	public bool result;
	public AxiNSWait_DeletePathFile(string filePath)
	{
		req = new S_NSWAIT_Path() { filePath = filePath };
	}

	public override void Invoke()
	{
		result = AxiNS.instance.io.DeletePathFile(req.filePath);
	}
}

public class AxiNSWait_DeletePathDir : AxiNSWaitBase
{
	S_NSWAIT_Path req;
	public bool result;
	public AxiNSWait_DeletePathDir(string filePath)
	{
		req = new S_NSWAIT_Path() { filePath = filePath };
	}

	public override void Invoke()
	{
		result = AxiNS.instance.io.DeletePathDir(req.filePath);
	}
}

public class AxiNSWait_DeletePathDirRecursively : AxiNSWaitBase
{
	S_NSWAIT_Path req;
	public bool result;
	public AxiNSWait_DeletePathDirRecursively(string filePath)
	{
		req = new S_NSWAIT_Path() { filePath = filePath };
	}

	public override void Invoke()
	{
		result = AxiNS.instance.io.DeletePathDirRecursively(req.filePath);
	}
}