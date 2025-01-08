namespace AxibugEmuOnline.GameScreenConvert
{
    public static class ScreenConvert
    {
        /// <summary>
        /// 转换
        /// </summary>
        /// <param name="platform">
        /// enum RomPlatformType
        ///{
        ///     Invalid = 0;
        ///     Nes = 1;
        ///     Master_System = 2;
        ///	    Game_Gear = 3;
        ///	    Game_Boy = 4;
        ///	    Game_Boy_Color = 5;
        ///	    Coleco_Vision = 6;
        ///	    SC_3000 = 7;
        ///	    SG_1000 = 8;
        ///	    All = 999;
        ///}</param>
        /// <param name="srcData"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="Quality">品质0~100</param>
        /// <param name="JpegData"></param>
        /// <returns></returns>
        public static bool Convert(int platform, byte[] srcData, out byte[] imageData)
        {
            IScreenConvert convert = default;
            switch (platform)
            {
                case 1://nes
                    //
                    break;
                default:
                    convert = new SampleScreenConvert();
                    break;
            }
            return convert.ScreenDataToRGBA32Data(platform, srcData, out imageData);
        }
    }

    public interface IScreenConvert
    {

        bool ScreenDataToRGBA32Data(int platform, byte[] srcData, out byte[] imageData);
    }

    public class SampleScreenConvert : IScreenConvert
    {
        public bool ScreenDataToRGBA32Data(int platform, byte[] srcData, out byte[] imageData)
        {
            imageData = null;
            //TODO 这里加上自己从原始数据中的颜色处理 比如颜色查找表
            //System.Span<T> 也是可用的
            //统一处理成RGBA32的通道顺序
            return imageData != null;
        }
    }
}
