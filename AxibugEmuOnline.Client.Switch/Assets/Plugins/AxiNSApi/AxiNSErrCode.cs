public static class AxiNSErrCode
{
#if UNITY_SWITCH
	public static string GetErrorInfo(this nn.Result result)
	{
		if (result.IsSuccess())
			return "NoErr";
		return GetErrorDetails(result.GetModule(), result.GetDescription());
	}
#endif
	/// <summary>
	/// 根据模块 ID 和描述 ID 返回任天堂 Switch 错误码的含义、可能原因及解决办法。
	/// </summary>
	/// <param name="moduleId">模块 ID</param>
	/// <param name="descriptionId">描述 ID</param>
	/// <returns>包含错误码、含义、可能原因及解决办法的字符串</returns>
	public static string GetErrorDetails(int moduleId, int descriptionId)
	{
		string errorCode = $"2{moduleId:D3}-{descriptionId:D4}"; // 格式化为 2XXX-YYYY
		string meaning = "未知错误";
		string causeAndSolution = "未知错误，请检查日志或联系任天堂支持。";

		switch (moduleId)
		{
			case 2: // nn::fs (文件系统)
				switch (descriptionId)
				{
					case 1:
						meaning = "ResultPathNotFound";
						causeAndSolution = "路径未找到。检查路径是否正确，确保父目录存在。使用 nn.fs.Directory.Create 创建父目录。";
						break;
					case 2:
						meaning = "ResultPermissionDenied";
						causeAndSolution = "权限被拒绝。可能是 Atmosphere 限制了 save:/ 的写权限。尝试调整 Atmosphere 配置或使用 sd:/ 挂载点。";
						break;
					case 3:
						meaning = "ResultPathAlreadyExists";
						causeAndSolution = "路径已存在。检查路径是否被占用，删除或重命名现有文件/目录。";
						break;
					case 5:
						meaning = "ResultTargetLocked";
						causeAndSolution = "目标被锁定。可能是文件正在使用中，关闭相关程序后重试。";
						break;
					case 7:
						meaning = "ResultTargetNotFound";
						causeAndSolution = "目标未找到。确认目标文件/目录是否存在，检查路径拼写。";
						break;
				}
				break;

			case 5: // nn::err (错误处理)
				switch (descriptionId)
				{
					case 3:
						meaning = "microSD 卡相关错误";
						causeAndSolution = "无法下载软件，可能是 microSD 卡损坏。移除 microSD 卡，重新插入，或更换卡。";
						break;
				}
				break;

			case 16: // nn::oe (操作系统错误)
				switch (descriptionId)
				{
					case 247:
						meaning = "microSD 卡存储问题";
						causeAndSolution = "microSD 卡损坏或不兼容。更换 microSD 卡，或将默认存储位置设置为系统内存。";
						break;
					case 390:
						meaning = "不支持的 microSD 卡";
						causeAndSolution = "microSD 卡格式不受支持。格式化为 exFAT 或 FAT32 后重试。";
						break;
					case 601:
						meaning = "microSD 卡数据损坏";
						causeAndSolution = "microSD 卡数据损坏。备份数据后格式化卡，或更换新卡。";
						break;
				}
				break;

			case 21: // nn::settings (设置)
				switch (descriptionId)
				{
					case 3:
						meaning = "系统软件未更新或损坏";
						causeAndSolution = "系统固件版本过旧或损坏。更新系统固件，或重新安装系统。";
						break;
				}
				break;

			case 101: // nn::fssrv (文件系统服务)
				switch (descriptionId)
				{
					case 1:
						meaning = "系统错误";
						causeAndSolution = "通用系统错误。重启 Switch，若问题持续，联系任天堂支持。";
						break;
					case 2:
						meaning = "固件损坏";
						causeAndSolution = "系统固件损坏。尝试更新系统，或恢复出厂设置（注意备份数据）。";
						break;
				}
				break;

			case 107: // nn::nim (网络安装管理)
				switch (descriptionId)
				{
					case 427:
						meaning = "固件损坏";
						causeAndSolution = "系统固件损坏。更新系统固件，或重新安装系统。";
						break;
					case 445:
						meaning = "硬件损坏或盗版内容";
						causeAndSolution = "可能是硬件损坏或存在盗版内容。删除盗版内容，重启 Switch，若无效联系任天堂支持。";
						break;
				}
				break;

			case 110: // nn::socket (网络套接字)
				switch (descriptionId)
				{
					case 1000:
						meaning = "网络连接失败";
						causeAndSolution = "无法连接到网络。检查网络连接，重启路由器，尝试切换到其他网络频段（2.4GHz/5GHz）。";
						break;
					case 2003:
						meaning = "无线网络连接失败";
						causeAndSolution = "无线信号弱或不稳定。靠近路由器，移除干扰物，或重启路由器。";
						break;
					case 2004:
						meaning = "网络设置不支持";
						causeAndSolution = "网络安全类型不受支持。Switch 支持 WEP/WPA/WPA2，调整路由器设置后重试。";
						break;
					case 2091:
						meaning = "有线网络连接失败";
						causeAndSolution = "有线连接问题。检查网线是否插好，重启 Switch 和路由器。";
						break;
					case 3127:
						meaning = "网络连接失败";
						causeAndSolution = "网络不稳定。测试网络连接，重启路由器，或联系网络提供商。";
						break;
				}
				break;

			case 115: // nn::mii (Mii 相关)
				switch (descriptionId)
				{
					case 96:
						meaning = "Mii 数据错误";
						causeAndSolution = "Mii 数据损坏。删除并重新创建 Mii，或更新系统。";
						break;
				}
				break;

			case 139: // nn::nfp (NFC/Amiibo)
				switch (descriptionId)
				{
					case 6:
						meaning = "Amiibo 读取错误";
						causeAndSolution = "Amiibo 读取失败。检查 Amiibo 是否损坏，更新系统，或尝试其他 Amiibo。";
						break;
				}
				break;

			case 153: // nn::ir (红外摄像头)
				switch (descriptionId)
				{
					case 321:
						meaning = "红外摄像头读取错误";
						causeAndSolution = "红外摄像头无法读取。检查摄像头是否被遮挡，清洁镜头，或联系任天堂支持。";
						break;
					case 1540:
						meaning = "红外摄像头硬件错误";
						causeAndSolution = "红外摄像头硬件故障。联系任天堂支持进行维修。";
						break;
				}
				break;

			case 155: // nn::account (账户服务)
				switch (descriptionId)
				{
					case 8006:
						meaning = "无法链接任天堂账户";
						causeAndSolution = "网络问题或服务中断。检查网络连接，稍后重试，或查看任天堂网络状态页面。";
						break;
				}
				break;

			case 160: // nn::online (在线服务)
				switch (descriptionId)
				{
					case 103:
						meaning = "无法加入在线匹配";
						causeAndSolution = "网络不稳定。重启 Switch，检查网络连接，或稍后重试。";
						break;
					case 202:
						meaning = "无法连接到在线服务";
						causeAndSolution = "服务可能正在维护。查看任天堂网络状态页面，稍后重试。";
						break;
					case 8006:
						meaning = "连接测试失败";
						causeAndSolution = "网络问题。重启路由器，检查网络设置，或尝试其他网络。";
						break;
				}
				break;

			case 162: // nn::application (应用程序)
				switch (descriptionId)
				{
					case 2:
						meaning = "软件崩溃";
						causeAndSolution = "软件崩溃，可能是盗版内容或固件问题。删除盗版内容，更新系统，或重新安装软件。";
						break;
					case 101:
						meaning = "软件需要更新";
						causeAndSolution = "游戏或软件需要更新。检查软件更新并安装。";
						break;
				}
				break;

			case 168: // nn::sys (系统)
				switch (descriptionId)
				{
					case 0:
						meaning = "需要软件更新";
						causeAndSolution = "软件需要更新。更新游戏或系统固件。";
						break;
					case 2:
						meaning = "系统崩溃";
						causeAndSolution = "系统崩溃，可能是硬件问题。重启 Switch，若无效联系任天堂支持。";
						break;
				}
				break;

			case 205: // nn::camera (摄像头)
				switch (descriptionId)
				{
					case 123:
						meaning = "摄像头读取错误";
						causeAndSolution = "摄像头无法读取。检查摄像头是否被遮挡，清洁镜头，或联系任天堂支持。";
						break;
				}
				break;

			case 306: // nn::ngc (网络游戏连接)
				switch (descriptionId)
				{
					case 501:
						meaning = "无法加入在线匹配";
						causeAndSolution = "网络连接中断。重启 Switch，检查网络连接，或稍后重试。";
						break;
					case 502:
						meaning = "匹配过程失败";
						causeAndSolution = "网络不稳定。测试网络连接，重启路由器，或联系网络提供商。";
						break;
					case 820:
						meaning = "在线服务不可用";
						causeAndSolution = "服务可能正在维护。查看任天堂网络状态页面，稍后重试。";
						break;
				}
				break;

			case 613: // nn::eShop (eShop)
				switch (descriptionId)
				{
					case 1400:
						meaning = "无法使用信用卡购买";
						causeAndSolution = "信用卡信息错误或 eShop 服务问题。检查信用卡信息，稍后重试，或联系任天堂支持。";
						break;
					case 6838:
						meaning = "eShop 连接失败";
						causeAndSolution = "网络问题或 eShop 维护。检查网络连接，查看任天堂网络状态页面，稍后重试。";
						break;
				}
				break;

			case 618: // nn::ngc (网络游戏连接)
				switch (descriptionId)
				{
					case 6:
						meaning = "无法加入在线匹配";
						causeAndSolution = "网络不稳定。重启 Switch，检查网络连接，或稍后重试。";
						break;
					case 201:
						meaning = "匹配已满";
						causeAndSolution = "尝试加入的匹配已满。稍后重试，或加入其他匹配。";
						break;
					case 501:
						meaning = "匹配过程失败";
						causeAndSolution = "网络连接中断。重启 Switch，检查网络连接，或稍后重试。";
						break;
				}
				break;

			case 801: // nn::sns (社交网络服务)
				switch (descriptionId)
				{
					case 7002:
						meaning = "无法上传截图到 Twitter";
						causeAndSolution = "服务可能正在维护。查看任天堂网络状态页面，稍后重试。";
						break;
					case 7199:
						meaning = "无法上传照片到 Facebook";
						causeAndSolution = "服务可能正在维护。查看任天堂网络状态页面，稍后重试。";
						break;
				}
				break;

			case 810: // nn::account (账户服务)
				switch (descriptionId)
				{
					case 1224:
						meaning = "无法登录任天堂账户";
						causeAndSolution = "网络问题或服务中断。检查网络连接，稍后重试，或查看任天堂网络状态页面。";
						break;
					case 1500:
						meaning = "无法登录 Facebook 账户";
						causeAndSolution = "服务可能正在维护。重启 Switch，稍后重试。";
						break;
				}
				break;

			case 811: // nn::account (账户服务)
				switch (descriptionId)
				{
					case 1006:
						meaning = "无法链接任天堂账户";
						causeAndSolution = "网络问题或 DNS 错误。检查网络连接，尝试更换 DNS（如 8.8.8.8），或稍后重试。";
						break;
					case 5001:
						meaning = "无法访问 eShop";
						causeAndSolution = "eShop 服务中断。查看任天堂网络状态页面，稍后重试。";
						break;
				}
				break;

			case 813: // nn::eShop (eShop)
				switch (descriptionId)
				{
					case 0:
						meaning = "eShop 访问失败";
						causeAndSolution = "eShop 服务中断。查看任天堂网络状态页面，稍后重试。";
						break;
					case 2470:
						meaning = "交易处理失败";
						causeAndSolution = "信用卡信息错误或 eShop 服务问题。检查信用卡信息，稍后重试，或联系任天堂支持。";
						break;
				}
				break;

			case 819: // nn::online (在线服务)
				switch (descriptionId)
				{
					case 3:
						meaning = "软件被暂停";
						causeAndSolution = "同一账户在另一台设备上使用。关闭其他设备上的软件，或使用不同账户。";
						break;
				}
				break;

			default:
				meaning = "未知模块";
				causeAndSolution = "未识别的模块 ID，请检查日志或联系任天堂支持。";
				break;
		}

		return $"错误码: {errorCode}\n含义: {meaning}\n可能原因及解决办法: {causeAndSolution}";
	}
}
