using AxibugEmuOnline.Client.ClientCore;
using AxibugProtobuf;

namespace AxibugEmuOnline.Client.Manager
{
    public class UserDataBase
    {
        public long UID { get; set; }
        public string Account { get; set; }
    }

    public class MainUserDataBase : UserDataBase
    {
        public bool IsLoggedIn { get; set; } = false;
    }

    public class UserDataManager
    {
        public UserDataManager()
        {
            //注册重连成功事件，以便后续自动登录
            AppAxibugEmuOnline.networkHelper.OnReConnected += OnReConnected;
        }
        public MainUserDataBase userdata { get;private set; } = new MainUserDataBase();
        public bool IsLoggedIn => userdata.IsLoggedIn;

        public void InitMainUserData(string UName,long UID)
        {
            userdata.Account = UName;
            userdata.IsLoggedIn = true;
            userdata.UID = UID;
            //以及其他数据初始化
            //...
        }

        /// <summary>
        /// 登出
        /// </summary>
        public void LoginOutData()
        {
            userdata.IsLoggedIn = false;
            //以及其他数据清理
            //...
        }

        /// <summary>
        /// 当重连成功
        /// </summary>
        public void OnReConnected()
        {
            //如果之前已登录，则重新登录
            if (userdata.IsLoggedIn)
            {
                AppAxibugEmuOnline.login.Login();
            }
        }
    }
}
