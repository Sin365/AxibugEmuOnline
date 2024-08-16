using AxibugEmuOnline.Web.Common;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

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
        public JsonResult NesRomList(string SearchKey,int Ptype,int GType,int Page, int PageSize)
        {
            string searchPattern = $"%{SearchKey}%";
            Resp_GameList resp = new Resp_GameList();
            resp.gameList = new List<Resp_RomInfo>();
            MySqlConnection conn = Haoyue_SQLPoolManager.DequeueSQLConn("NesRomList");
            {

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


                string query = "SELECT count(id) FROM romlist_nes where `Name` like ?searchPattern " + GameTypeCond;
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

                query = $"SELECT id,`Name`,GameType,Note,RomUrl,ImgUrl,`Hash` FROM romlist_nes where `Name` like ?searchPattern {GameTypeCond} LIMIT ?offset, ?pageSize;";
                using (var command = new MySqlCommand(query, conn))
                {
                    // 设置参数值  
                    command.Parameters.AddWithValue("?searchPattern", searchPattern);
                    command.Parameters.AddWithValue("?offset", Page * PageSize);
                    command.Parameters.AddWithValue("?pageSize", PageSize);
                    // 执行查询并处理结果  
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            resp.gameList.Add(new Resp_RomInfo()
                            {
                                id = reader.GetInt32(0),
                                romName = !reader.IsDBNull(1) ? reader.GetString(1) : string.Empty,
                                gType = !reader.IsDBNull(2) ? reader.GetString(2) : string.Empty,
                                desc = !reader.IsDBNull(3) ? reader.GetString(3) : string.Empty,
                                url = !reader.IsDBNull(4) ? reader.GetString(4) : string.Empty,
                                imgUrl = !reader.IsDBNull(5) ? reader.GetString(5) : string.Empty,
                                hash = !reader.IsDBNull(6) ? reader.GetString(6) : string.Empty,
                                stars = 0,
                            });
                        }
                    }
                }
                Haoyue_SQLPoolManager.EnqueueSQLConn(conn);
            }
            return new JsonResult(resp);
        }

        enum PlatformType : byte
        {
            All = 0,
            Nes,
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

        class Resp_GameList
        {
            public int page { get; set; }
            public int maxPage { get; set; }
            public int resultAllCount { get; set; }
            public List<Resp_RomInfo> gameList { get; set; }
        }

        public class Resp_RomInfo
        {
            public int id { get; set; }
            public string romName { get; set;}
            public string gType { get; set; }
            public string desc { get; set; }
            public string url { get; set; }
            public string imgUrl { get; set; }
            public string hash { get; set; }
            public int stars { get; set; }
        }
    }
}
