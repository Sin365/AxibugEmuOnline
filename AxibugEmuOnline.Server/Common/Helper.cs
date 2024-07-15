namespace AxibugEmuOnline.Server.Common
{
    public static class Helper
    {
        public static long GetNowTimeStamp()
        {
            return GetTimeStamp(DateTime.Now);
        }

        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        public static long GetTimeStamp(DateTime dt)
        {
            TimeSpan ts = dt - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds);
        }
    }
}
