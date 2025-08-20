using Microsoft.AspNetCore.Mvc;

namespace AxibugEmuOnline.Web.Controllers
{
    public class UserController : Controller
    {
        private readonly ILogger<UserController> _logger;

        public UserController(ILogger<UserController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        //[HttpPost]
        //public JsonResult CheckTokenState(string tokenStr,int type)
        //{
        //    long UID = 0;
        //    if (Helper.TryDecrypToken(tokenStr, out Protobuf_Token_Struct tokenData))
        //        UID = tokenData.UID;

        //    if (UID < 0)
        //        return new JsonResult(new CheckTokenState_Resp(){ code = -1,msg = "token错误"});
        //    string query;
        //    using (MySqlConnection conn = SQLRUN.GetConn("RomList"))
        //    {
        //        //设置默认名字
        //        query = "select uid,account,mail from users where uid = ?uid ";
        //        using (var command = new MySqlCommand(query, conn))
        //        {
        //            // 设置参数值
        //            command.Parameters.AddWithValue("?uid", UID);// 执行查询并处理结果  
        //            using (var reader = command.ExecuteReader())
        //            {
        //                while (reader.Read())
        //                {
        //                    uid = reader.GetInt64(0);
        //                    Account = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
        //                    mail = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
        //                }
        //            }
        //        }
        //    }
        //    if (type == 0)
        //    {
        //    }
        //}

        class CheckTokenState_Resp
        {
            public int code { get; set; }
            public string msg { get; set; }
            public string uri { get; set; }
        }
    }
}
