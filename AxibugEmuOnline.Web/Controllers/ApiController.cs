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
        public JsonResult NesRomList(string SearchKey,int Page, int PageSize)
        {
            string searchPattern = $"%{SearchKey}%";
            Resp_GameList resp = new Resp_GameList();
            resp.GameList = new List<Resp_RomInfo>();
            using (MySqlConnection conn = Haoyue_SQLPoolManager.DequeueSQLConn("NesRomList"))
            {
                string query = "SELECT count(id) FROM romlist_nes where `Name` like ?searchPattern ";
                using (var command = new MySqlCommand(query, conn))
                {
                    // 设置参数值  
                    command.Parameters.AddWithValue("?searchPattern", searchPattern);
                    // 执行查询并处理结果  
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            resp.ResultAllCount = reader.GetInt32(0);
                            resp.Page = Page;
                            resp.MaxPage = resp.ResultAllCount / PageSize;
                        }
                    }
                }

                query = "SELECT id,`Name`,RomUrl,ImgUrl FROM romlist_nes where `Name` like ?searchPattern LIMIT ?offset, ?pageSize;";
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
                            resp.GameList.Add(new Resp_RomInfo() {
                                ID = reader.GetInt32(0),
                                RomName = reader.GetString(1),
                                Hash = string.Empty,
                                ImgUrl = reader.GetString(2),
                                Url = reader.GetString(3),
                            });
                        }
                    }
                }
                Haoyue_SQLPoolManager.EnqueueSQLConn(conn);
            }
            return new JsonResult(resp);
        }


        class Resp_GameList
        {
            public int Page { get; set; }
            public int MaxPage { get; set; }
            public int ResultAllCount { get; set; }
            public List<Resp_RomInfo> GameList { get; set; }
        }

        public class Resp_RomInfo
        {
            public int ID { get; set; }
            public string Hash { get; set; }
            public string RomName { get; set;}
            public string Url { get; set; }
            public string ImgUrl { get; set; }
        }
    }
}
