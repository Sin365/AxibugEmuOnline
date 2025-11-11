namespace MAME.Core
{
    /// <summary>
    /// 原依赖Form的内容
    /// </summary>
    public unsafe class Motion
    {
        private static uint UI_FILLCOLOR = Palette.make_argb(0xe0, 0x10, 0x10, 0x30);
        public delegate void motion_delegate();
        public static motion_delegate motion_handler_callback, motion_update_callback;
        public static bool single_step;
        public static void ui_update_and_render()
        {
            motion_update_callback();
            motion_handler_callback();
        }
        //public static void ui_updateC()
        //{
        //    //不再填充完整画布
        //    //{
        //    //    int i;
        //    //    int red, green, blue;
        //    //    if (single_step || Mame.paused)
        //    //    {
        //    //        byte bright = 0xa7;
        //    //        for (i = 0; i < Video.fullwidth * Video.fullheight; i++)
        //    //        {
        //    //            red = (int)(((Palette.entry_color[Video.bitmapbase_Ptrs[Video.curbitmap][i]] & 0xff0000) >> 16) * bright / 0xff);
        //    //            green = (int)(((Palette.entry_color[Video.bitmapbase_Ptrs[Video.curbitmap][i]] & 0xff00) >> 8) * bright / 0xff);
        //    //            blue = (int)((Palette.entry_color[Video.bitmapbase_Ptrs[Video.curbitmap][i]] & 0xff) * bright / 0xff);
        //    //            Video.bitmapcolor[i] = (int)Palette.make_argb(0xff, red, green, blue);
        //    //        }
        //    //    }
        //    //    else
        //    //    {
        //    //        for (i = 0; i < Video.fullwidth * Video.fullheight; i++)
        //    //        {
        //    //            Video.bitmapcolor[i] = (int)Palette.entry_color[Video.bitmapbase_Ptrs[Video.curbitmap][i]];
        //    //        }
        //    //    }
        //    //}

        //    {
        //        int i;
        //        int target_i = 0;
        //        int x, y;
        //        int red, green, blue;

        //        int startX = Video.offsetx;
        //        int endX = Video.offsetx + Video.width;
        //        int startY = Video.offsety;
        //        int endY = Video.offsety + Video.height;

        //        if (single_step || Mame.paused)
        //        {
        //            byte bright = 0xa7;
        //            for (y = startY; y < endY; y++)
        //            {
        //                int stepIndex = y * Video.fullwidth;
        //                for (x = startX; x < endX; x++, target_i++)
        //                {
        //                    //i = y * Video.fullwidth + x;
        //                    i = stepIndex + x;
        //                    red = (int)(((Palette.entry_color[Video.bitmapbase_Ptrs[Video.curbitmap][i]] & 0xff0000) >> 16) * bright / 0xff);
        //                    green = (int)(((Palette.entry_color[Video.bitmapbase_Ptrs[Video.curbitmap][i]] & 0xff00) >> 8) * bright / 0xff);
        //                    blue = (int)((Palette.entry_color[Video.bitmapbase_Ptrs[Video.curbitmap][i]] & 0xff) * bright / 0xff);
        //                    Video.bitmapcolorRect_Ptrunsafe[target_i] = (int)Palette.make_argb(0xff, red, green, blue);
        //                }
        //            }
        //        }
        //        else
        //        {

        //            for (y = startY; y < endY; y++)
        //            {
        //                int stepIndex = y * Video.fullwidth;
        //                for (x = startX; x < endX; x++, target_i++)
        //                {
        //                    //i = y * Video.fullwidth + x;
        //                    i = stepIndex + x;
        //                    Video.bitmapcolorRect_Ptrunsafe[target_i] = (int)Palette.entry_color[Video.bitmapbase_Ptrs[Video.curbitmap][i]];
        //                }
        //            }
        //        }
        //    }
        //}
        public unsafe static void ui_updateC()
        {
            //fixed (ushort* curbitmapPtr = &Video.bitmapbase_Ptrs[Video.curbitmap][0])
            //fixed (uint* entry_colorPtr = &Palette.entry_color[0])
            //fixed (int* bitmapcolorRectPtr = &Video.bitmapcolorRect_Ptrunsafe[0])
            {
                //ushort* curbitmap = curbitmapPtr;
                ushort* curbitmap = (ushort*)Video.bitmapbase_Ptrs[Video.curbitmap];
                //uint* entry_color = entry_colorPtr;
                uint* entry_color = (uint*)Palette.entry_color;
                //int* bitmapcolorRect = bitmapcolorRectPtr;
                int* bitmapcolorRect = (int*)Video.bitmapcolorRect_Ptr;

                /*
            //不再填充完整画布
            //{
            //    int i;
            //    int red, green, blue;
            //    if (single_step || Mame.paused)
            //    {
            //        byte bright = 0xa7;
            //        for (i = 0; i < Video.fullwidth * Video.fullheight; i++)
            //        {
            //            red = (int)(((Palette.entry_color[Video.bitmapbase_Ptrs[Video.curbitmap][i]] & 0xff0000) >> 16) * bright / 0xff);
            //            green = (int)(((Palette.entry_color[Video.bitmapbase_Ptrs[Video.curbitmap][i]] & 0xff00) >> 8) * bright / 0xff);
            //            blue = (int)((Palette.entry_color[Video.bitmapbase_Ptrs[Video.curbitmap][i]] & 0xff) * bright / 0xff);
            //            Video.bitmapcolor[i] = (int)Palette.make_argb(0xff, red, green, blue);
            //        }
            //    }
            //    else
            //    {
            //        for (i = 0; i < Video.fullwidth * Video.fullheight; i++)
            //        {
            //            Video.bitmapcolor[i] = (int)Palette.entry_color[Video.bitmapbase_Ptrs[Video.curbitmap][i]];
            //        }
            //    }
            //}
                */
                {
                    int i;
                    int target_i = 0;
                    int x, y;
                    int red, green, blue;

                    int startX = Video.offsetx;
                    int endX = Video.offsetx + Video.width;
                    int startY = Video.offsety;
                    int endY = Video.offsety + Video.height;

                    if (single_step || Mame.paused)
                    {
                        byte bright = 0xa7;
                        for (y = startY; y < endY; y++)
                        {
                            int stepIndex = y * Video.fullwidth;
                            for (x = startX; x < endX; x++, target_i++)
                            {
                                i = stepIndex + x;
                                red = (int)(((entry_color[curbitmap[i]] & 0xff0000) >> 16) * bright / 0xff);
                                green = (int)(((entry_color[curbitmap[i]] & 0xff00) >> 8) * bright / 0xff);
                                blue = (int)((entry_color[curbitmap[i]] & 0xff) * bright / 0xff);
                                bitmapcolorRect[target_i] = (int)Palette.make_argb(0xff, red, green, blue);
                            }
                        }
                    }
                    else
                    {

                        for (y = startY; y < endY; y++)
                        {
                            int stepIndex = y * Video.fullwidth;
                            for (x = startX; x < endX; x++, target_i++)
                            {
                                i = stepIndex + x;
                                bitmapcolorRect[target_i] = (int)entry_color[curbitmap[i]];
                            }
                        }

                        //for (y = startY; y < endY; y++)
                        //{
                        //    int stepIndex = y * Video.fullwidth;

                        //    for (x = startX; x < endX; x++, target_i++)
                        //    {
                        //        i = stepIndex + x;
                        //        bitmapcolorRect[target_i] = (int)entry_color[curbitmap[i]];
                        //    }

                        //    // 使用Marshal.Copy进行内存拷贝
                        //    Marshal.Copy(Palette.entry_color_Ptr,, Video.bitmapcolorRect_Ptr, endX - startX);
                        //}
                    }
                }
            }
        }
        public static void ui_updateTehkan()
        {
            //不再填充完整画布
            //{
            //    int i;
            //    int red, green, blue;
            //    if (single_step || Mame.paused)
            //    {
            //        byte bright = 0xa7;
            //        for (i = 0; i < Video.fullwidth * Video.fullheight; i++)
            //        {
            //            if (Video.bitmapbase_Ptrs[Video.curbitmap][i] < 0x100)
            //            {
            //                red = (int)(((Palette.entry_color[Video.bitmapbase_Ptrs[Video.curbitmap][i]] & 0xff0000) >> 16) * bright / 0xff);
            //                green = (int)(((Palette.entry_color[Video.bitmapbase_Ptrs[Video.curbitmap][i]] & 0xff00) >> 8) * bright / 0xff);
            //                blue = (int)((Palette.entry_color[Video.bitmapbase_Ptrs[Video.curbitmap][i]] & 0xff) * bright / 0xff);
            //                Video.bitmapcolor[i] = (int)Palette.make_argb(0xff, red, green, blue);
            //            }
            //            else
            //            {
            //                int i1 = 1;
            //            }
            //        }
            //    }
            //    else
            //    {
            //        for (i = 0; i < Video.fullwidth * Video.fullheight; i++)
            //        {
            //            if (Video.bitmapbase_Ptrs[Video.curbitmap][i] < 0x100)
            //            {
            //                Video.bitmapcolor[i] = (int)Palette.entry_color[Video.bitmapbase_Ptrs[Video.curbitmap][i]];
            //            }
            //            else
            //            {
            //                Video.bitmapcolor[i] = (int)Palette.entry_color[0];
            //            }
            //        }
            //    }
            //}

            {
                int i;
                int target_i = 0;
                int x, y;
                int red, green, blue;

                int startX = Video.offsetx;
                int endX = Video.offsetx + Video.width;
                int startY = Video.offsety;
                int endY = Video.offsety + Video.height;

                if (single_step || Mame.paused)
                {
                    byte bright = 0xa7;
                    for (y = startY; y < endY; y++)
                    {
                        int stepIndex = y * Video.fullwidth;
                        for (x = startX; x < endX; x++, target_i++)
                        {
                            //i = y * Video.fullwidth + x;
                            i = stepIndex + x;
                            if (Video.bitmapbase_Ptrs[Video.curbitmap][i] < 0x100)
                            {
                                red = (int)(((Palette.entry_color[Video.bitmapbase_Ptrs[Video.curbitmap][i]] & 0xff0000) >> 16) * bright / 0xff);
                                green = (int)(((Palette.entry_color[Video.bitmapbase_Ptrs[Video.curbitmap][i]] & 0xff00) >> 8) * bright / 0xff);
                                blue = (int)((Palette.entry_color[Video.bitmapbase_Ptrs[Video.curbitmap][i]] & 0xff) * bright / 0xff);
                                Video.bitmapcolorRect_Ptrunsafe[target_i] = (int)Palette.make_argb(0xff, red, green, blue);
                            }
                            else
                            {
                                int i1 = 1;
                            }
                        }
                    }
                }
                else
                {
                    for (y = startY; y < endY; y++)
                    {
                        int stepIndex = y * Video.fullwidth;
                        for (x = startX; x < endX; x++, target_i++)
                        {
                            //i = y * Video.fullwidth + x;
                            i = stepIndex + x;
                            if (Video.bitmapbase_Ptrs[Video.curbitmap][i] < 0x100)
                            {
                                Video.bitmapcolorRect_Ptrunsafe[target_i] = (int)Palette.entry_color[Video.bitmapbase_Ptrs[Video.curbitmap][i]];
                            }
                            else
                            {
                                Video.bitmapcolorRect_Ptrunsafe[target_i] = (int)Palette.entry_color[0];
                            }
                        }
                    }
                }
            }
        }
        public unsafe static void ui_updateN()
        {
            //不再填充完整画布
            //{
            //    int i;
            //    int red, green, blue;
            //    if (single_step || Mame.paused)
            //    {
            //        byte bright = 0xa7;
            //        for (i = 0; i < Video.fullwidth * Video.fullheight; i++)
            //        {
            //            red = ((Video.bitmapbaseN_Ptrs[Video.curbitmap][i] & 0xff0000) >> 16) * bright / 0xff;
            //            green = ((Video.bitmapbaseN_Ptrs[Video.curbitmap][i] & 0xff00) >> 8) * bright / 0xff;
            //            blue = (Video.bitmapbaseN_Ptrs[Video.curbitmap][i] & 0xff) * bright / 0xff;
            //            Video.bitmapcolor[i] = (int)Palette.make_argb(0xff, red, green, blue);
            //        }
            //    }
            //    else
            //    {
            //        for (i = 0; i < Video.fullwidth * Video.fullheight; i++)
            //        {
            //            Video.bitmapcolor[i] = (int)(0xff000000 | (uint)Video.bitmapbaseN_Ptrs[Video.curbitmap][i]);
            //        }
            //    }
            //}

            {
                int i;
                int target_i = 0;
                int x, y;
                int red, green, blue;

                int startX = Video.offsetx;
                int endX = Video.offsetx + Video.width;
                int startY = Video.offsety;
                int endY = Video.offsety + Video.height;
                if (single_step || Mame.paused)
                {
                    byte bright = 0xa7;
                    for (y = startY; y < endY; y++)
                    {
                        int stepIndex = y * Video.fullwidth;
                        for (x = startX; x < endX; x++, target_i++)
                        {
                            //i = y * Video.fullwidth + x;
                            i = stepIndex + x;
                            red = ((Video.bitmapbaseN_Ptrs[Video.curbitmap][i] & 0xff0000) >> 16) * bright / 0xff;
                            green = ((Video.bitmapbaseN_Ptrs[Video.curbitmap][i] & 0xff00) >> 8) * bright / 0xff;
                            blue = (Video.bitmapbaseN_Ptrs[Video.curbitmap][i] & 0xff) * bright / 0xff;
                            //Video.bitmapcolorRect_Ptrunsafe[target_i] = (int)Palette.make_argb(0xff, red, green, blue);
                            Video.bitmapcolorRect_Ptrunsafe[target_i] = (int)((((uint)(0xff) & 0xff) << 24) | (((uint)(blue) & 0xff) << 16) | (((uint)(green) & 0xff) << 8) | ((uint)(red) & 0xff));
                        }
                    }
                }
                else
                {
                    for (y = startY; y < endY; y++)
                    {
                        int stepIndex = y * Video.fullwidth;
                        for (x = startX; x < endX; x++, target_i++)
                        {
                            //i = y * Video.fullwidth + x;
                            i = stepIndex + x;
                            Video.bitmapcolorRect_Ptrunsafe[target_i] = (int)(0xff000000 | (uint)Video.bitmapbaseN_Ptrs[Video.curbitmap][i]);
                        }
                    }
                }
            }
        }
        public static void ui_updateNa()
        {
            //不再填充完整画布
            //{
            //    int i;
            //    int red, green, blue;
            //    if (single_step || Mame.paused)
            //    {
            //        byte bright = 0xa7;
            //        for (i = 0; i < Video.fullwidth * Video.fullheight; i++)
            //        {
            //            red = (int)(((Palette.entry_color[Video.bitmapbase_Ptrs[Video.curbitmap][i]] & 0xff0000) >> 16) * bright / 0xff);
            //            green = (int)(((Palette.entry_color[Video.bitmapbase_Ptrs[Video.curbitmap][i]] & 0xff00) >> 8) * bright / 0xff);
            //            blue = (int)((Palette.entry_color[Video.bitmapbase_Ptrs[Video.curbitmap][i]] & 0xff) * bright / 0xff);
            //            Video.bitmapcolor[i] = (int)Palette.make_argb(0xff, red, green, blue);
            //        }
            //    }
            //    else
            //    {
            //        for (i = 0; i < Video.fullwidth * Video.fullheight; i++)
            //        {
            //            Video.bitmapcolor[i] = (int)Palette.entry_color[Video.bitmapbase_Ptrs[Video.curbitmap][i]];
            //        }
            //    }
            //}

            {
                int i;
                int target_i = 0;
                int x, y;
                int red, green, blue;

                int startX = Video.offsetx;
                int endX = Video.offsetx + Video.width;
                int startY = Video.offsety;
                int endY = Video.offsety + Video.height;
                if (single_step || Mame.paused)
                {
                    byte bright = 0xa7;
                    for (y = startY; y < endY; y++)
                    {
                        int stepIndex = y * Video.fullwidth;
                        for (x = startX; x < endX; x++, target_i++)
                        {
                            //i = y * Video.fullwidth + x;
                            i = stepIndex + x;
                            red = (int)(((Palette.entry_color[Video.bitmapbase_Ptrs[Video.curbitmap][i]] & 0xff0000) >> 16) * bright / 0xff);
                            green = (int)(((Palette.entry_color[Video.bitmapbase_Ptrs[Video.curbitmap][i]] & 0xff00) >> 8) * bright / 0xff);
                            blue = (int)((Palette.entry_color[Video.bitmapbase_Ptrs[Video.curbitmap][i]] & 0xff) * bright / 0xff);
                            Video.bitmapcolorRect_Ptrunsafe[target_i] = (int)Palette.make_argb(0xff, red, green, blue);
                        }
                    }
                }
                else
                {
                    for (y = startY; y < endY; y++)
                    {
                        int stepIndex = y * Video.fullwidth;
                        for (x = startX; x < endX; x++, target_i++)
                        {
                            //i = y * Video.fullwidth + x;
                            i = stepIndex + x;
                            Video.bitmapcolorRect_Ptrunsafe[target_i] = (int)Palette.entry_color[Video.bitmapbase_Ptrs[Video.curbitmap][i]];
                        }
                    }
                }
            }
        }
        public static void ui_updateIGS011()
        {
            //不再填充完整画布
            //{
            //    int i;
            //    int red, green, blue;
            //    if (single_step || Mame.paused)
            //    {
            //        byte bright = 0xa7;
            //        for (i = 0; i < Video.fullwidth * Video.fullheight; i++)
            //        {
            //            red = (int)(((Palette.entry_color[Video.bitmapbase_Ptrs[Video.curbitmap][i]] & 0xff0000) >> 16) * bright / 0xff);
            //            green = (int)(((Palette.entry_color[Video.bitmapbase_Ptrs[Video.curbitmap][i]] & 0xff00) >> 8) * bright / 0xff);
            //            blue = (int)((Palette.entry_color[Video.bitmapbase_Ptrs[Video.curbitmap][i]] & 0xff) * bright / 0xff);
            //            Video.bitmapcolor[i] = (int)Palette.make_argb(0xff, red, green, blue);
            //        }
            //    }
            //    else
            //    {
            //        for (i = 0; i < Video.fullwidth * Video.fullheight; i++)
            //        {
            //            Video.bitmapcolor[i] = (int)Palette.entry_color[Video.bitmapbase_Ptrs[Video.curbitmap][i]];
            //        }
            //    }
            //}

            {
                int i;
                int target_i = 0;
                int x, y;
                int red, green, blue;

                int startX = Video.offsetx;
                int endX = Video.offsetx + Video.width;
                int startY = Video.offsety;
                int endY = Video.offsety + Video.height;

                if (single_step || Mame.paused)
                {
                    byte bright = 0xa7;
                    for (y = startY; y < endY; y++)
                    {
                        int stepIndex = y * Video.fullwidth;
                        for (x = startX; x < endX; x++, target_i++)
                        {
                            //i = y * Video.fullwidth + x;
                            i = stepIndex + x;
                            red = (int)(((Palette.entry_color[Video.bitmapbase_Ptrs[Video.curbitmap][i]] & 0xff0000) >> 16) * bright / 0xff);
                            green = (int)(((Palette.entry_color[Video.bitmapbase_Ptrs[Video.curbitmap][i]] & 0xff00) >> 8) * bright / 0xff);
                            blue = (int)((Palette.entry_color[Video.bitmapbase_Ptrs[Video.curbitmap][i]] & 0xff) * bright / 0xff);
                            Video.bitmapcolorRect_Ptrunsafe[target_i] = (int)Palette.make_argb(0xff, red, green, blue);
                        }
                    }
                }
                else
                {
                    for (y = startY; y < endY; y++)
                    {
                        int stepIndex = y * Video.fullwidth;
                        for (x = startX; x < endX; x++, target_i++)
                        {
                            //i = y * Video.fullwidth + x;
                            i = stepIndex + x;
                            Video.bitmapcolorRect_Ptrunsafe[target_i] = (int)Palette.entry_color[Video.bitmapbase_Ptrs[Video.curbitmap][i]];
                        }
                    }
                }
            }
        }
        public static void ui_updatePGM()
        {
            //不再填充完整画布
            //{
            //    int i;
            //    int red, green, blue;
            //    if (single_step || Mame.paused)
            //    {
            //        byte bright = 0xa7;
            //        for (i = 0; i < Video.fullwidth * Video.fullheight; i++)
            //        {
            //            red = (int)(((Palette.entry_color[Video.bitmapbase_Ptrs[Video.curbitmap][i]] & 0xff0000) >> 16) * bright / 0xff);
            //            green = (int)(((Palette.entry_color[Video.bitmapbase_Ptrs[Video.curbitmap][i]] & 0xff00) >> 8) * bright / 0xff);
            //            blue = (int)((Palette.entry_color[Video.bitmapbase_Ptrs[Video.curbitmap][i]] & 0xff) * bright / 0xff);
            //            Video.bitmapcolor[i] = (int)Palette.make_argb(0xff, red, green, blue);
            //        }
            //    }
            //    else
            //    {
            //        for (i = 0; i < Video.fullwidth * Video.fullheight; i++)
            //        {
            //            Video.bitmapcolor[i] = (int)Palette.entry_color[Video.bitmapbase_Ptrs[Video.curbitmap][i]];
            //        }
            //    }
            //}

            {
                int i;
                int target_i = 0;
                int x, y;
                int red, green, blue;

                int startX = Video.offsetx;
                int endX = Video.offsetx + Video.width;
                int startY = Video.offsety;
                int endY = Video.offsety + Video.height;

                if (single_step || Mame.paused)
                {
                    byte bright = 0xa7;
                    for (y = startY; y < endY; y++)
                    {
                        int stepIndex = y * Video.fullwidth;
                        for (x = startX; x < endX; x++, target_i++)
                        {
                            //i = y * Video.fullwidth + x;
                            i = stepIndex + x;
                            red = (int)(((Palette.entry_color[Video.bitmapbase_Ptrs[Video.curbitmap][i]] & 0xff0000) >> 16) * bright / 0xff);
                            green = (int)(((Palette.entry_color[Video.bitmapbase_Ptrs[Video.curbitmap][i]] & 0xff00) >> 8) * bright / 0xff);
                            blue = (int)((Palette.entry_color[Video.bitmapbase_Ptrs[Video.curbitmap][i]] & 0xff) * bright / 0xff);
                            Video.bitmapcolorRect_Ptrunsafe[target_i] = (int)Palette.make_argb(0xff, red, green, blue);
                        }
                    }
                }
                else
                {
                    for (y = startY; y < endY; y++)
                    {
                        int stepIndex = y * Video.fullwidth;
                        for (x = startX; x < endX; x++, target_i++)
                        {
                            //i = y * Video.fullwidth + x;
                            i = stepIndex + x;
                            Video.bitmapcolorRect_Ptrunsafe[target_i] = (int)Palette.entry_color[Video.bitmapbase_Ptrs[Video.curbitmap][i]];
                        }
                    }
                }
            }
        }
        public static void handler_ingame()
        {
            Mame.is_foreground = true;
            bool is_paused = Mame.mame_is_paused();
            if (single_step)
            {
                Mame.mame_pause(true);
                single_step = false;
            }
            if (Mame.is_foreground)
            {
                if (Keyboard.IsPressed(MotionKey.F3))
                {
                    cpurun();
                    Mame.playState = Mame.PlayState.PLAY_RESET;
                }
                if (Keyboard.IsTriggered(MotionKey.F7))
                {
                    cpurun();
                    if (Keyboard.IsPressed(MotionKey.LeftShift) || Keyboard.IsPressed(MotionKey.RightShift))
                    {
                        Mame.playState = Mame.PlayState.PLAY_SAVE;
                    }
                    else
                    {
                        Mame.playState = Mame.PlayState.PLAY_LOAD;
                    }
                    return;
                }
                if (Keyboard.IsTriggered(MotionKey.F8))
                {
                    cpurun();
                    if (Keyboard.IsPressed(MotionKey.LeftShift) || Keyboard.IsPressed(MotionKey.RightShift))
                    {
                        if (Mame.playState == Mame.PlayState.PLAY_RECORDRUNNING)
                        {
                            Mame.playState = Mame.PlayState.PLAY_RECORDEND;
                        }
                        else
                        {
                            Mame.playState = Mame.PlayState.PLAY_RECORDSTART;
                        }
                    }
                    else
                    {
                        Mame.playState = Mame.PlayState.PLAY_REPLAYSTART;
                    }
                    return;
                }
                if (Keyboard.IsTriggered(MotionKey.EMU_PAUSED))
                {
                    if (is_paused && (Keyboard.IsPressed(MotionKey.LeftShift) || Keyboard.IsPressed(MotionKey.RightShift)))
                    {
                        single_step = true;
                        Mame.mame_pause(false);
                    }
                    else
                    {
                        Mame.mame_pause(!Mame.mame_is_paused());
                    }
                }
                if (Keyboard.IsTriggered(MotionKey.F10))
                {
                    Keyboard.bF10 = true;
                    bool b1 = Video.global_throttle;
                    Video.global_throttle = !b1;
                }
            }
        }
        public static void cpurun()
        {
        }
        private static double ui_get_line_height()
        {
            int raw_font_pixel_height = 0x0b;
            int target_pixel_height = 0xff;
            double one_to_one_line_height;
            double scale_factor;
            one_to_one_line_height = (double)raw_font_pixel_height / (double)target_pixel_height;
            scale_factor = 1.0;
            return scale_factor * one_to_one_line_height;
        }
    }
}
