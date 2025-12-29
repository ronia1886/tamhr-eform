using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;

namespace TAMHR.ESS.Infrastructure.Web.Helpers
{
    public static class ImageHelper
    {
        private static object _locker = new object();

        public static Image Resize(Image image, int sourceWidth, int sourceHeight = 0, int resolutionWidth = 0, int resolutionHeight = 0)
        {
            var width = image.Width;
            var height = image.Height;
            var ratioX = sourceWidth > 0 ? (double)sourceWidth / width : 0;
            var ratioY = sourceHeight > 0 ? (double)sourceHeight / height : 0;
            var posX = 0f;
            var posY = 0f;

            ratioX = ratioX == 0 ? ratioY : ratioX;
            ratioY = ratioY == 0 ? ratioX : ratioY;

            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(width * ratio);
            var newHeight = (int)(height * ratio);

            var canvasWidth = resolutionWidth > newWidth ? resolutionWidth : newWidth;
            var canvasHeight = resolutionHeight > newHeight ? resolutionHeight : newHeight;

            if (newWidth < canvasWidth)
            {
                posX = Math.Abs(((float)canvasWidth - newWidth) / 2);
            }

            if (newWidth < canvasWidth)
            {
                posY = Math.Abs(((float)canvasHeight - newHeight) / 2);
            }

            var canvas = new Bitmap(canvasWidth, canvasHeight);
            var graphic = Graphics.FromImage(canvas);

            graphic.CompositingQuality = CompositingQuality.HighSpeed;
            graphic.SmoothingMode = SmoothingMode.HighSpeed;
            graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;

            graphic.Clear(Color.White);
            graphic.DrawImage(image, posX, posY, newWidth, newHeight);

            return canvas;
        } 
    }
}
