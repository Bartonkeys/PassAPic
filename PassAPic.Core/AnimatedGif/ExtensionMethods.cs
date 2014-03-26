using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassAPic.Core.AnimatedGif
{
    public static class ExtensionMethods
    {
        public static Bitmap Base64StringToBitmap(this string base64String)
        {
            Bitmap bmpReturn = null;

            byte[] byteBuffer = Convert.FromBase64String(base64String);
            MemoryStream memoryStream = new MemoryStream(byteBuffer);

            memoryStream.Position = 0;

            bmpReturn = (Bitmap)Bitmap.FromStream(memoryStream);

            memoryStream.Close();
           
            return bmpReturn;
        }
    }
}
