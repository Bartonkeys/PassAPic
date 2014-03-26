using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Text;

namespace PassAPic.Core.AnimatedGif

{
    public static class MyGlobals
    {
        public const int ImageWidth = 512;
        public const int ImageHeight = 512;
    }

    public class TextToImageConversion
    {
        public static Bitmap CreateBitmapImage(string sImageText)
        {
            Bitmap objBmpImage = new Bitmap(1, 1);

            // Create the Font object for the image text drawing.
            Font objFont = new Font("Arial", 48, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
   
            // Create a graphics object to measure the text's width and height.
            Graphics objGraphics = Graphics.FromImage(objBmpImage);
       
            // This is where the bitmap size is determined.
            int intWordWidth = (int)objGraphics.MeasureString(sImageText, objFont).Width;
            int intWordHeight = (int)objGraphics.MeasureString(sImageText, objFont).Height;

            int trailingSpace = (MyGlobals.ImageWidth/2 - intWordWidth/2);
            int trailingHeight = (MyGlobals.ImageHeight / 2 - intWordHeight / 2);

            // Create the bmpImage again with the correct size for the text and font.
            objBmpImage = new Bitmap(objBmpImage, new Size(MyGlobals.ImageWidth, MyGlobals.ImageHeight));
   
            // Add the colors to the new bitmap.
            objGraphics = Graphics.FromImage(objBmpImage);
    
            // Set Background color
            objGraphics.Clear(Color.White);
            objGraphics.SmoothingMode = SmoothingMode.AntiAlias;
            objGraphics.TextRenderingHint = TextRenderingHint.AntiAlias;
            objGraphics.DrawString(sImageText, objFont, new SolidBrush(Color.FromArgb(102, 102, 102)), trailingSpace, trailingHeight);
            objGraphics.Flush();
    
            return (objBmpImage);
        }

    }
}
