using AxibugEmuOnline.Web.Common;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Mysqlx.Crud;

namespace AxibugEmuOnline.Web.Controllers
{
    public class ApiController : Controller
    {
        private readonly ILogger<ApiController> _logger;

        public ApiController(ILogger<ApiController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public JsonResult CheckStandInfo(int platform, string version)
        {
            Resp_CheckStandInfo resp = new Resp_CheckStandInfo()
            {
                needUpdateClient = 0,
                clientVersion = Config.cfg.ClientVersion,
                serverIp = Config.cfg.ServerIp,
                serverPort = Config.cfg.ServerPort,
                downLoadUrl = ""
            };
            return new JsonResult(resp);
        }

        [HttpGet]
        public JsonResult RomList(string SearchKey, int Ptype, int GType, int Page, int PageSize)
        {
            string searchPattern = $"%{SearchKey}%";
            Resp_GameList resp = new Resp_GameList();
            resp.gameList = new List<Resp_RomInfo>();
            MySqlConnection conn = Haoyue_SQLPoolManager.DequeueSQLConn("RomList");
            {
                string platformCond = "";
                if (GType > 0)
                {
                    platformCond = $" and PlatformType = '{Ptype}' ";
                }

                GameType SearchGType = (GameType)GType;
                string GameTypeCond = "";
                switch (SearchGType)
                {
                    case GameType.NONE:
                        GameTypeCond = string.Empty;
                        break;
                    case GameType.ALLINONE:
                        GameTypeCond = $" and GameType = '合卡' ";
                        break;
                    default:
                        GameTypeCond = $" and GameType = '{SearchGType.ToString()}' ";
                        break;
                }

                string query = "SELECT count(id) FROM romlist where `Name` like ?searchPattern " + platformCond + GameTypeCond;
                using (var command = new MySqlCommand(query, conn))
                {
                    // 设置参数值  
                    command.Parameters.AddWithValue("?searchPattern", searchPattern);
                    // 执行查询并处理结果  
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            resp.resultAllCount = reader.GetInt32(0);
                            resp.page = Page;
                            resp.maxPage = (int)Math.Ceiling((float)resp.resultAllCount / PageSize);
                        }
                    }
                }


                string HotOrderBy = "ORDER BY playcount DESC, id ASC";

                query = $"SELECT id,`Name`,GameType,Note,RomUrl,ImgUrl,`Hash`,`playcount`,`stars`,`PlatformType` FROM romlist where `Name` like ?searchPattern {platformCond} {GameTypeCond} {HotOrderBy} LIMIT ?offset, ?pageSize;";
                using (var command = new MySqlCommand(query, conn))
                {
                    // 设置参数值  
                    command.Parameters.AddWithValue("?searchPattern", searchPattern);
                    command.Parameters.AddWithValue("?offset", Page * PageSize);
                    command.Parameters.AddWithValue("?pageSize", PageSize);
                    // 执行查询并处理结果  
                    using (var reader = command.ExecuteReader())
                    {
                        int orderIndex = Page * PageSize;
                        while (reader.Read())
                        {
                            resp.gameList.Add(new Resp_RomInfo()
                            {
                                orderid = orderIndex++,
                                id = reader.GetInt32(0),
                                romName = !reader.IsDBNull(1) ? reader.GetString(1) : string.Empty,
                                gType = !reader.IsDBNull(2) ? reader.GetString(2) : string.Empty,
                                desc = !reader.IsDBNull(3) ? reader.GetString(3) : string.Empty,
                                url = !reader.IsDBNull(4) ? reader.GetString(4) : string.Empty,
                                imgUrl = !reader.IsDBNull(5) ? reader.GetString(5) : string.Empty,
                                hash = !reader.IsDBNull(6) ? reader.GetString(6) : string.Empty,
                                playcount = reader.GetInt32(7),
                                stars = reader.GetInt32(8),
                                ptype = reader.GetInt32(9),
                            });
                        }
                    }
                }
                Haoyue_SQLPoolManager.EnqueueSQLConn(conn);
            }
            return new JsonResult(resp);
        }

        [HttpGet]
        public JsonResult RomInfo(int Ptype, int RomID)
        {
            string searchPattern = $"%{RomInfo}%";
            Resp_RomInfo resp = new Resp_RomInfo();
            MySqlConnection conn = Haoyue_SQLPoolManager.DequeueSQLConn("NesRomList");
            {
                string query = $"SELECT id,`Name`,GameType,Note,RomUrl,ImgUrl,`Hash`,`playcount`,`stars`,`PlatformType` FROM romlist where id = ?romid;";
                using (var command = new MySqlCommand(query, conn))
                {
                    // 设置参数值  
                    command.Parameters.AddWithValue("?romid", RomID);
                    // 执行查询并处理结果  
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            resp.id = reader.GetInt32(0);
                            resp.romName = !reader.IsDBNull(1) ? reader.GetString(1) : string.Empty;
                            resp.gType = !reader.IsDBNull(2) ? reader.GetString(2) : string.Empty;
                            resp.desc = !reader.IsDBNull(3) ? reader.GetString(3) : string.Empty;
                            resp.url = !reader.IsDBNull(4) ? reader.GetString(4) : string.Empty;
                            resp.imgUrl = !reader.IsDBNull(5) ? reader.GetString(5) : string.Empty;
                            resp.hash = !reader.IsDBNull(6) ? reader.GetString(6) : string.Empty;
                            resp.playcount = reader.GetInt32(7);
                            resp.stars = reader.GetInt32(8);
                            resp.ptype = reader.GetInt32(9);
                        }
                    }
                }
                Haoyue_SQLPoolManager.EnqueueSQLConn(conn);
            }
            return new JsonResult(resp);
        }


        enum GameType : byte
        {
            NONE = 0,
            ACT,
            ARPG,
            AVG,
            ETC,
            FTG,
            PUZ,
            RAC,
            RPG,
            SLG,
            SPG,
            SRPG,
            STG,
            TAB,
            /// <summary>
            /// 合卡
            /// </summary>
            ALLINONE,
        }

        class Resp_CheckStandInfo
        {
            public int needUpdateClient { get; set; }
            public string serverIp { get; set; }
            public ushort serverPort { get; set; }
            public string clientVersion { get; set; }
            public string downLoadUrl { get; set; }
        }


        class Resp_GameList
        {
            public int page { get; set; }
            public int maxPage { get; set; }
            public int resultAllCount { get; set; }
            public List<Resp_RomInfo> gameList { get; set; }
        }

        public class Resp_RomInfo
        {
            public int orderid { get; set; }
            public int id { get; set; }
            public int ptype { get; set; }
            public string romName { get; set; }
            public string gType { get; set; }
            public string desc { get; set; }
            public string url { get; set; }
            public string imgUrl { get; set; }
            public string hash { get; set; }
            public int stars { get; set; }
            public int playcount { get; set; }
        }
    }
}
