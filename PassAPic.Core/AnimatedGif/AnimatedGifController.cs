using System;

using System.Drawing;


namespace PassAPic.Core.AnimatedGif
{
    public class AnimatedGifController
    {
        
		/* create Gif */
		//you should replace filepath
        //private static String filePathLocal = "C:\\YerMA\\Pass-a-pic\\Server\\Gif\\Example\\Res\\";
        //private static String [] imageFilePaths = new String[]{filePathLocal+"01.png",filePathLocal+"02.png",filePathLocal+"03.png"}; 
		//private static readonly String outputFilePath = filePathLocal+"test.gif";
        private static readonly AnimatedGifEncoder e = new AnimatedGifEncoder();

       

        public static Image CreateAnimatedGifFromBitmapArray(String [] imageFilePaths, Boolean setRepeat, String outputFilePath) 
        {
            try
            {
                e.Start(outputFilePath);
                e.SetDelay(1000);
                //-1:no repeat,0:always repeat
                e.SetRepeat(setRepeat ? 0 : -1);
                for (int i = 0, count = imageFilePaths.Length; i < count; i++)
                {
                    e.AddFrame(Image.FromFile(imageFilePaths[i]));
                }
                e.Finish();
                return new Bitmap(outputFilePath);
            }

            catch (Exception e)
            {
                return null;
            }
            

            
        }
		
        public static Image[] ExtractBitmapsFromAnimatedGif(String pathToAnimatedGif)
        {
            Image[] frames;

            /* extract Gif */
            //string outputPath = "c:\\";
            GifDecoder gifDecoder = new GifDecoder();
            gifDecoder.Read(pathToAnimatedGif);
            frames = new Image[gifDecoder.GetFrameCount()];
            for (int i = 0, count = frames.Length; i < count; i++)
            {
                Image frame = gifDecoder.GetFrame(i);  // frame i
                frames[i] = frame;
                //frame.Save(outputPath + Guid.NewGuid().ToString() + ".png", ImageFormat.Png);
            }

            return frames;
        }
		
		
		
    }
}
