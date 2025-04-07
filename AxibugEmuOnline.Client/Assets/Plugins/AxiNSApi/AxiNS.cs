#if UNITY_SWITCH
using nn.account;
#endif

public class AxiNS
{
	static AxiNS _instance;
	public static AxiNS instance
	{
		get
		{
			if (_instance == null)
				_instance = new AxiNS();
			return _instance;
		}
	}

	public AxiNSUser user;
	public AxiNSMount mount;
	public AxiNSIO io;
	AxiNS()
	{
		user = new AxiNSUser();
		mount = new AxiNSMount();
		io = new AxiNSIO();
	}

	/// <summary>
	/// 初始化（最好在项目第一时间初始化，保证先初始化再使用某些东西，才不闪退）
	/// </summary>
	public void Init()
	{
#if UNITY_SWITCH
		if (!user.GetUserID(out Uid uid))
			return;
		mount.MountSave(uid);
#endif
	}
}
