using AxibugProtobuf;
using static AxibugEmuOnline.Web.Controllers.ApiController;

namespace AxibugEmuOnline.Web.Common
{
    public class Helper
    {
        const string NoImgUri = "images/NoImg.jpg";
        public static void CheckDefaultImg(Resp_RomInfo info)
        {
            if(string.IsNullOrEmpty(info.imgUrl))
                info.imgUrl = NoImgUri;
        }

        public static bool TryDecrypToken(string tokenStr, out Protobuf_Token_Struct tokenData)
        {
            if (string.IsNullOrEmpty(tokenStr) || string.IsNullOrEmpty(tokenStr.Trim()))
            {
                tokenData = null;
                return false;
            }
            try
            {
                byte[] encryptData = Convert.FromBase64String(tokenStr.Trim());
                byte[] decryptData = AESHelper.Decrypt(encryptData);
                tokenData = ProtoBufHelper.DeSerizlize<Protobuf_Token_Struct>(decryptData);
                return true;
            }
            catch
            {
                tokenData = null;
                return false;
            }
        }
    }
}
