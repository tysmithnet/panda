﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Panda.Client
{
    public class IconHelper
    {
        public static ImageSource IconFromFilePath(string filePath)
        {
            var img = Imaging.CreateBitmapSourceFromHIcon(
                Icon.ExtractAssociatedIcon(filePath).Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            img.Freeze();
            return img;
        }
    }
}
