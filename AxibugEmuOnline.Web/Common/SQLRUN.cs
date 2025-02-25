using MySql.Data.MySqlClient;

namespace AxibugEmuOnline.Web.Common
{
    public static class SQLRUN
    {
        // 移除自定义队列和状态跟踪字典
        private static Dictionary<string, long> _DicSqlRunFunNum = new Dictionary<string, long>();
        private static Dictionary<string, long> _DicTimeOutSqlRunFunNum = new Dictionary<string, long>();

        const int DefaultCount = 1;
        const int MaxLimit = 10;
        static readonly object _sync = new object();
        static MySqlConnectionStringBuilder connBuilder;

        public static void InitConnMgr()
        {
            // 配置 MySQL 内置连接池
            connBuilder = new MySqlConnectionStringBuilder
            {
                Database = Config.cfg.DBName,
                Server = Config.cfg.DBIp,
                UserID = Config.cfg.DBUname,
                Password = Config.cfg.DBPwd,
                Port = Config.cfg.DBPort,
                Pooling = true,               // 启用内置连接池
                MinimumPoolSize = DefaultCount,   // 最小连接数
                MaximumPoolSize = MaxLimit        // 最大连接数
            };

            // 初始化时不手动创建连接，依赖连接池自动管理
            Console.WriteLine("SQLPool初始化完成，连接池参数已配置");
        }

        public static MySqlConnection GetConn(string FuncStr)
        {
            lock (_sync)
            {
                IncrementFuncCall(FuncStr);
                // 直接使用 MySQL 内置连接池获取连接
                var conn = new MySqlConnection(connBuilder.ConnectionString);
                conn.Open();
                return conn;
            }
        }

        public static void GetPoolState()
        {
            Console.WriteLine("-----------------查询统计-----------------");
            foreach (var entry in _DicSqlRunFunNum)
            {
                Console.WriteLine($"函数 {entry.Key} 调用次数: {entry.Value}");
            }
            Console.WriteLine("-----------------超时统计-----------------");
            foreach (var entry in _DicTimeOutSqlRunFunNum)
            {
                Console.WriteLine($"函数 {entry.Key} 超时次数: {entry.Value}");
            }
            Console.WriteLine("------------------------------------------");
        }

        #region 私有方法（辅助统计逻辑）
        private static void IncrementFuncCall(string funcStr)
        {
            _DicSqlRunFunNum[funcStr] = _DicSqlRunFunNum.ContainsKey(funcStr) ? _DicSqlRunFunNum[funcStr] + 1 : 1;
        }

        #endregion

    }

}