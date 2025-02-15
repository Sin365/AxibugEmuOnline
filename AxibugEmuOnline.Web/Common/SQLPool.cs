﻿using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;

namespace AxibugEmuOnline.Web.Common
{
    public static class SQLPool
    {
        static Queue<MySqlConnection> _ConQueue = new Queue<MySqlConnection>();
        static Dictionary<MySqlConnection, Haoyue_PoolTime> _OutOfSQLPool = new Dictionary<MySqlConnection, Haoyue_PoolTime>();
        static Dictionary<string, long> _DicSqlRunFunNum = new Dictionary<string, long>();
        static Dictionary<string, long> _DicTimeOutSqlRunFunNum = new Dictionary<string, long>();
        const int DefaultCount = 1;
        const int MaxLimit = 5;
        static readonly object _sync = new object();
        static MySqlConnectionStringBuilder connBuilder;

        public static void InitConnMgr()
        {
            connBuilder = new MySqlConnectionStringBuilder();
            connBuilder.Database = Config.cfg.DBName;
            connBuilder.Server = Config.cfg.DBIp;
            connBuilder.UserID = Config.cfg.DBUname;
            connBuilder.Password = Config.cfg.DBPwd;
            connBuilder.Port = Config.cfg.DBPort;
            //connBuilder.MinimumPoolSize = 40u;
            //connBuilder.MaximumPoolSize = 100u;
            connBuilder.Pooling = true;


            Console.WriteLine("SQLPool连接初始化....");
            for (int i = 0; i < DefaultCount; i++)
            {
                MySqlConnection _conn = conn();
                _conn.Open();
                _ConQueue.Enqueue(_conn);
            }
            Console.WriteLine("SQLPool初始化完毕,连接数" + _ConQueue.Count);
        }
        public static MySqlConnection conn()
        {
            return new MySqlConnection(connBuilder.ConnectionString);
        }

        public static MySqlConnection DequeueSQLConn(string FuncStr)
        {
            lock (_sync)
            {
                if (_DicSqlRunFunNum.ContainsKey(FuncStr))
                {
                    _DicSqlRunFunNum[FuncStr]++;
                }
                else
                {
                    _DicSqlRunFunNum[FuncStr] = 1L;
                }
                MySqlConnection _conn = null;
                if (_ConQueue.Count < 1)
                {
                    Console.WriteLine("[DequeueSQLConn]创建新的SQLPool.Count>" + _ConQueue.Count);
                    _conn = conn();
                    _conn.Open();
                }
                else
                {
                    MySqlConnection temp = null;
                    while (_ConQueue.Count > 0)
                    {
                        Console.WriteLine("[DequeueSQLConn]取出一个SQLCount.Count>" + _ConQueue.Count);
                        temp = _ConQueue.Dequeue();
                        if (temp.State == System.Data.ConnectionState.Closed)
                        {
                            Console.WriteLine("[DequeueSQLConn]已经断开SQLCount.Count>" + _ConQueue.Count);
                            temp.Dispose();
                            temp = null;
                            continue;
                        }
                    }

                    if (temp != null)
                    {
                        _conn = temp;
                    }
                    else
                    {
                        Console.WriteLine("[DequeueSQLConn]连接池全部已断开，重新创建连接");
                        _conn = conn();
                        _conn.Open();
                    }
                }

                _OutOfSQLPool.Add(_conn, new Haoyue_PoolTime
                {
                    time = time(),
                    FuncStr = FuncStr
                });

                return _conn;
            }
        }
        public static void EnqueueSQLConn(MySqlConnection BackConn)
        {
            lock (_sync)
            {
                if (_OutOfSQLPool.ContainsKey(BackConn))
                {
                    _OutOfSQLPool.Remove(BackConn);
                }
                else
                {
                    Console.WriteLine("出队遗漏的数据出现了！");
                }
                if (_ConQueue.Count > MaxLimit)
                {
                    Console.WriteLine("已经不需要回收了,多余了,SQLPool.Count>" + _ConQueue.Count);
                    BackConn.Close();
                    BackConn.Dispose();
                    BackConn = null;
                }
                else
                {
                    _ConQueue.Enqueue(BackConn);
                    Console.WriteLine("回收SQLPool.Count>" + _ConQueue.Count);
                }
            }
        }

        public static void CheckPoolTimeOut()
        {
            lock (_sync)
            {
                long now = time();
                List<MySqlConnection> removeTemp = new List<MySqlConnection>();
                foreach (KeyValuePair<MySqlConnection, Haoyue_PoolTime> o2 in _OutOfSQLPool)
                {
                    if (now - o2.Value.time >= 120)
                    {
                        if (_DicTimeOutSqlRunFunNum.ContainsKey(o2.Value.FuncStr))
                        {
                            _DicTimeOutSqlRunFunNum[o2.Value.FuncStr]++;
                        }
                        else
                        {
                            _DicTimeOutSqlRunFunNum[o2.Value.FuncStr] = 1L;
                        }
                        if (_ConQueue.Count > MaxLimit)
                        {
                            Console.WriteLine("[超时回收]" + o2.Value.FuncStr + "已经不需要回收了,多余了,SQLPool.Count>" + _ConQueue.Count);
                            o2.Key.Close();
                        }
                        else
                        {
                            Console.WriteLine("[超时回收]" + o2.Value.FuncStr + "回收SQLPool.Count>" + _ConQueue.Count);
                            _ConQueue.Enqueue(o2.Key);
                        }
                        removeTemp.Add(o2.Key);
                    }
                }
                if (removeTemp.Count() <= 0)
                {
                    return;
                }
                foreach (MySqlConnection o in removeTemp)
                {
                    if (_OutOfSQLPool.ContainsKey(o))
                    {
                        _OutOfSQLPool.Remove(o);
                        Console.WriteLine("[超时回收]_OutOfSQLPool清理");
                    }
                    else
                    {
                        Console.WriteLine("[超时回收]_OutOfSQLPool清理异常？？？？？？？");
                    }
                }
                Console.WriteLine("[超时回收]处理结束SQLPool.Count>" + _ConQueue.Count);
            }
        }
        public static long time()
        {
            return Convert.ToInt64((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
        }
        public static void GetPoolState()
        {
            Console.WriteLine("-----------------查询统计-----------------");
            foreach (KeyValuePair<string, long> dic2 in _DicSqlRunFunNum)
            {
                Console.WriteLine(dic2.Key + ":" + dic2.Value);
            }
            Console.WriteLine("-----------------超时统计-----------------");
            foreach (KeyValuePair<string, long> dic in _DicTimeOutSqlRunFunNum)
            {
                Console.WriteLine(dic.Key + ":" + dic.Value);
            }
            Console.WriteLine("------------------------------------------");
        }
    }

    public class Haoyue_PoolTime
    {
        public long time { get; set; }
        public string FuncStr { get; set; }
    }
}
