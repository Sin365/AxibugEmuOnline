#if UNITY_SWITCH
using nn.account;
#endif

public class AxiNSUser
{
	bool m_bInit = false;
	bool m_bGotOpenPreselectedUser = false;

#if UNITY_SWITCH
	Uid m_UserId;
	nn.account.UserHandle mUserHandle;
	nn.account.Nickname m_NickName;
#endif

	#region 对外公开接口，确保内部安全处理，避免崩溃

#if UNITY_SWITCH
	public bool GetUserID(out Uid uid)
	{
		InitPreselectedUserInfo();
		if (!m_bGotOpenPreselectedUser)
		{
			uid = Uid.Invalid;
			return false;
		}
		uid = m_UserId;
		return true;
	}
#endif
	public bool GetNickName(out string NickName)
	{
#if !UNITY_SWITCH
		NickName = "";
		return true;
#else
		InitPreselectedUserInfo();
		if (!m_bGotOpenPreselectedUser)
		{
			NickName = string.Empty;
			return false;
		}
		NickName = m_NickName.ToString();
		return true;
#endif
	}
#endregion
	/// <summary>
	/// 初始化Account模块儿
	/// </summary>
	void InitNSAccount()
    {
#if UNITY_SWITCH
		if (m_bInit)
			return;
		//必须先初始化NS的Account 不然调用即崩
		nn.account.Account.Initialize();
		m_bInit = true;
#endif
	}

	/// <summary>
	/// 获取预选用户
	/// </summary>
	void InitPreselectedUserInfo()
    {
#if UNITY_SWITCH
		if (m_bGotOpenPreselectedUser)
			return;

		InitNSAccount();
		nn.Result result;
		mUserHandle = new nn.account.UserHandle();
		if (!nn.account.Account.TryOpenPreselectedUser(ref mUserHandle))
		{
			UnityEngine.Debug.LogError("打开预选的用户失败.");
			return;
		}
		UnityEngine.Debug.Log("打开预选用户成功.");
		result = nn.account.Account.GetUserId(ref m_UserId, mUserHandle);
		//result.abortUnlessSuccess();
		if (!result.IsSuccess())
		{
			UnityEngine.Debug.LogError($"GetUserId失败: {result.ToString()}");
			return;
		}

		if (m_UserId == Uid.Invalid)
		{
			UnityEngine.Debug.LogError("无法获取用户 ID");
			return;
		}
		UnityEngine.Debug.Log($"获取用户 ID:{m_UserId.ToString()}");

		result = nn.account.Account.GetNickname(ref m_NickName, m_UserId);
		//result.abortUnlessSuccess();
		if (!result.IsSuccess())
		{
			UnityEngine.Debug.LogError($"GetNickname失败: {result.ToString()}");
			return;
		}
		UnityEngine.Debug.Log($"获取用户 NickName ID:{m_NickName.ToString()}");
		m_bGotOpenPreselectedUser = true;
#endif
	}
}
