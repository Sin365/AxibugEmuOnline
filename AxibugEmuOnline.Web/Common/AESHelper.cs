using System.Security.Cryptography;
using System.Text;

namespace AxibugEmuOnline.Web.Common
{
    public static class AESHelper
    {
        static byte[] currKey;
        static byte[] currIV;

        public static void LoadKeyIVCfg(string key, string vi)
        {
            try
            {
                currKey = CommaSeparatedStringToByteArray(key);
                currIV = CommaSeparatedStringToByteArray(key);
            }
            catch (Exception ex)
            {
                Console.WriteLine("aeskeyvi 配置错误"+ex.Message);
            }
        }

        public static void LoadKeyIVCfg(byte[] key, byte[] vi)
        {
            currKey = key;
            currIV = key;
        }

        public static void GenAesKeyIV()
        {
            Aes aes = Aes.Create();
            aes.KeySize = 128;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.GenerateKey();
            aes.GenerateIV();

            string key = ByteArrayToCommaSeparatedString(aes.Key);
            Console.WriteLine("key:");
            Console.WriteLine(key);
            string vi = ByteArrayToCommaSeparatedString(aes.IV);
            Console.WriteLine("iv:");
            Console.WriteLine(vi);

            byte[] temp = new byte[255];
            for (byte i = 0; i < temp.Length; i++)
                temp[i] = i;
            byte[] EncodeData = Encrypt(temp, aes.Key, aes.IV);
            byte[] DecodeData = Decrypt(EncodeData, aes.Key, aes.IV);

            bool bOk = true;
            if (temp.Length != DecodeData.Length)
            {
                bOk = false;
            }
            else
            {
                for (int i = 0; i < temp.Length; i++)
                {
                    if (temp[i] != DecodeData[i])
                    { 
                        bOk = false;
                        break;
                    }
                }
            }

            Console.WriteLine($"密钥和填充加解密验证：{bOk}");
        }

        public static string ByteArrayToCommaSeparatedString(byte[] byteArray)
        {
            if (byteArray == null)
                throw new ArgumentNullException(nameof(byteArray));

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < byteArray.Length; i++)
            {
                sb.Append(byteArray[i]);

                // 不是最后一个元素时，添加逗号
                if (i < byteArray.Length - 1)
                {
                    sb.Append(",");
                }
            }

            return sb.ToString();
        }

        public static byte[] CommaSeparatedStringToByteArray(string commaSeparatedString)
        {
            if (string.IsNullOrEmpty(commaSeparatedString))
                throw new ArgumentNullException(nameof(commaSeparatedString));

            // 去除字符串两端的空格，并按逗号分割
            string[] byteStrings = commaSeparatedString.Trim().Split(',');

            // 将每个字符串转换成byte，并存储到数组中
            byte[] byteArray = byteStrings.Select(byteString =>
            {
                if (!byte.TryParse(byteString, out byte result))
                    throw new FormatException($"无法将字符串 '{byteString}' 解析为字节。");
                return result;
            }).ToArray();

            return byteArray;
        }

        public static byte[] Encrypt(byte[] toEncryptArray)
        {
            return Encrypt(toEncryptArray, currKey, currIV);
        }

        public static byte[] Encrypt(byte[] toEncryptArray, byte[] keyArray, byte[] ivArray)
        {
            Aes rDel = Aes.Create();
            rDel.Key = keyArray;
            rDel.IV = ivArray;
            rDel.Mode = CipherMode.CBC;
            rDel.Padding = PaddingMode.PKCS7;
            ICryptoTransform cTransform = rDel.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            return resultArray;
        }

        public static byte[] Decrypt(byte[] toDecrypt)
        {
            return Decrypt(toDecrypt, currKey, currIV);
        }

        public static byte[] Decrypt(byte[] toDecrypt, byte[] keyArray, byte[] ivArray)
        {
            Aes rDel = Aes.Create();
            rDel.Key = keyArray;
            rDel.IV = ivArray;
            rDel.Mode = CipherMode.CBC;
            rDel.Padding = PaddingMode.PKCS7;
            ICryptoTransform cTransform = rDel.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toDecrypt, 0, toDecrypt.Length);
            return resultArray;
        }

    }
}