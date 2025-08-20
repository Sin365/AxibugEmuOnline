using System;

namespace MAME.Core
{
    partial class Video
    {
        //public delegate Bitmap drawcrosshairdelegate(Bitmap bm1);
        //public static drawcrosshairdelegate drawcrosshair;


        public delegate int[] drawcrosshairdelegate(int[] bm1);
        public static drawcrosshairdelegate drawcrosshair;

        //public static Bitmap drawcrosshair_null(Bitmap bm1)
        //{
        //    Bitmap bm2 = bm1;
        //    return bm2;
        //}

        public static int[] drawcrosshair_null(int[] bm1)
        {
            return bm1;
        }
        //public static Bitmap drawcrosshair_opwolf(Bitmap bm1)
        //{
        //    Bitmap bm2 = bm1;
        //    Graphics g = Graphics.FromImage(bm2);
        //    g.DrawImage(MultiplyAlpha(Crosshair.global.bitmap[0], (float)Crosshair.global.fade / 0xff), new Rectangle(Crosshair.global.x[0] - 10, Crosshair.global.y[0] - 10, 20, 20), new Rectangle(0, 0, 100, 100), GraphicsUnit.Pixel);
        //    g.Dispose();
        //    return bm2;
        //}

        public static int[] drawcrosshair_opwolf(int[] bm1)
        {
            int[] bm2 = bm1;
            //TODO
            return bm2;
        }

        /* 替换代码
         * public static Bitmap MultiplyAlpha(Bitmap bitmap, float factor)
        {
            Bitmap result = new Bitmap(bitmap.Width, bitmap.Height);
            using (Graphics graphics = Graphics.FromImage(result))
            {
                ColorMatrix colorMatrix = new ColorMatrix();
                colorMatrix.Matrix33 = factor;
                ImageAttributes imageAttributes = new ImageAttributes();
                imageAttributes.SetColorMatrix(colorMatrix);
                graphics.DrawImage(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height), 0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel, imageAttributes);
            }
            return result;
        }*/


        ///// <summary>
        /////
        ///// </summary>
        ///// <param name="bitmap"></param>
        ///// <param name="factor"></param>
        ///// <returns></returns>
        //public static int[] MultiplyAlpha(int[] bitmap, float factor)
        //{
        //    int[] result = (int[])bitmap.Clone();
        //    for (int i = 0; i < result.Length; i++)
        //    {
        //        Color originalColor = AxiColor.FromArgb(result[i]);
        //        byte newAlpha = (byte)Math.Min(255, (int)(originalColor.a * factor));
        //        Color newColor = new Color
        //        {
        //            r = originalColor.r,
        //            g = originalColor.g,
        //            b = originalColor.b,
        //            a = newAlpha
        //        };
        //        result[i] = AxiColor.ToArgb(newColor);
        //    }
        //    return result;
        //}

        //public static void GDIDraw()
        //{
        //    try
        //    {
        //        bitmapData = bitmapGDI.LockBits(new Rectangle(0, 0, Video.fullwidth, Video.fullheight), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
        //        Marshal.Copy(Video.bitmapcolor, 0, bitmapData.Scan0, Video.fullwidth * Video.fullheight);
        //        bitmapGDI.UnlockBits(bitmapData);
        //        if (Wintime.osd_ticks() < popup_text_end)
        //        {
        //            Machine.mainMotion.tsslStatus = sDrawText;
        //        }
        //        else
        //        {
        //            popup_text_end = 0;
        //            if (Mame.paused)
        //            {
        //                Machine.mainMotion.tsslStatus = "pause";
        //            }
        //            else
        //            {
        //                switch (Mame.playState)
        //                {
        //                    case Mame.PlayState.PLAY_RECORDRUNNING:
        //                        Machine.mainMotion.tsslStatus = "record";
        //                        break;
        //                    case Mame.PlayState.PLAY_REPLAYRUNNING:
        //                        Machine.mainMotion.tsslStatus = "replay";
        //                        break;
        //                    default:
        //                        Machine.mainMotion.tsslStatus = "run";
        //                        break;
        //                }
        //            }
        //        }
        //        //bbmp[iMode] = drawcrosshair((Bitmap)bitmapGDI.Clone(new Rectangle(offsetx, offsety, width, height), PixelFormat.Format32bppArgb));
        //        bbmp[iMode] = drawcrosshair(bitmapGDI.Clone(new Rectangle(offsetx, offsety, width, height)));
        //        switch (Machine.sDirection)
        //        {
        //            case "":
        //                break;
        //            case "90":
        //                bbmp[iMode].RotateFlip(RotateFlipType.Rotate90FlipNone);
        //                break;
        //            case "180":
        //                bbmp[iMode].RotateFlip(RotateFlipType.Rotate180FlipNone);
        //                break;
        //            case "270":
        //                bbmp[iMode].RotateFlip(RotateFlipType.Rotate270FlipNone);
        //                break;
        //        }
        //        //Machine.mainMotion.pictureBox1.Image = bbmp[iMode];
        //        SubmitVideo(bbmp[iMode]);
        //    }
        //    catch
        //    {

        //    }
        //}


        public static void GDIDraw()
        {
            try
            {
                //TODO
                //int[] TempData = AxiBitmapEx.CloneIntColorArr(Video.bitmapcolor, Video.fullwidth, Video.fullheight, new Rectangle(offsetx, offsety, width, height));
                //drawcrosshair(TempData);
                //bbmp[iMode] = drawcrosshair(bitmapGDI.Clone(new Rectangle(offsetx, offsety, width, height)));
                //switch (Machine.sDirection)
                //{
                //    case "":
                //        break;
                //    case "90":
                //        bbmp[iMode].RotateFlip(RotateFlipType.Rotate90FlipNone);
                //        break;
                //    case "180":
                //        bbmp[iMode].RotateFlip(RotateFlipType.Rotate180FlipNone);
                //        break;
                //    case "270":
                //        bbmp[iMode].RotateFlip(RotateFlipType.Rotate270FlipNone);
                //        break;
                //}
                //Machine.mainMotion.pictureBox1.Image = bbmp[iMode];

                //AxiBitmapEx.CloneIntColorArr(Video.bitmapcolor,Video.bitmapcolorRect, Video.fullwidth, Video.fullheight, new Rectangle(offsetx, offsety, width, height));
                SubmitVideo(Video.bitmapcolorRect, Video.screenstate.frame_number);
                //SubmitVideo(Video.bitmapcolor);
            }
            catch (Exception ex)
            {

            }
        }


    }
}