using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Web;
using PassAPic.Core.CloudImage;
using PassAPic.Data;


namespace PassAPic.Core.AnimatedGif
{
    public class AnimatedGifController
    {
        
		/* create Gif */
		//you should replace filepath
        //private static String filePathLocal = "C:\\YerMA\\Pass-a-pic\\Server\\Gif\\Example\\Res\\";
        //private static String [] imageFilePaths = new String[]{filePathLocal+"01.png",filePathLocal+"02.png",filePathLocal+"03.png"}; 
		//private static readonly String outputFilePath = filePathLocal+"test.gif";
        private static readonly AnimatedGifEncoder _animatedGifEncoder = new AnimatedGifEncoder();
        private CloudImageService _cloudImageService;

        public AnimatedGifController(CloudImageService cloudImageService)
        {
            _cloudImageService = cloudImageService;
        }
       

        public static Image CreateAnimatedGifFromBitmapArray(String [] imageFilePaths, Boolean setRepeat, String outputFilePath) 
        {
            try
            {
                _animatedGifEncoder.Start(outputFilePath);
                _animatedGifEncoder.SetDelay(1000);
                //-1:no repeat,0:always repeat
                _animatedGifEncoder.SetRepeat(setRepeat ? 0 : -1);
                for (int i = 0, count = imageFilePaths.Length; i < count; i++)
                {
                    _animatedGifEncoder.AddFrame(Image.FromFile(imageFilePaths[i]));
                }
                _animatedGifEncoder.Finish();
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


        public String CreateAnimatedGif(Game game, string tempAnimatedGif)
        {
            try
            {
             
                _animatedGifEncoder.Start(tempAnimatedGif);
                _animatedGifEncoder.SetDelay(3000);
                _animatedGifEncoder.SetRepeat(0);

                Image startingWordImage = TextToImageConversion.CreateBitmapImage("Start: " + game.StartingWord);
                _animatedGifEncoder.AddFrame(startingWordImage);

                var count = 1;
                foreach (var guess in game.Guesses)
                {
                    if (guess is WordGuess)
                    {
                        var wordGuess = (WordGuess) guess;

                        String word;
                        if (game.Guesses.Count == count)
                            word = "Final: " + wordGuess.Word;
                        else
                            word = wordGuess.Word;

                        Image wordImage = TextToImageConversion.CreateBitmapImage(word);
                        _animatedGifEncoder.AddFrame(wordImage);
                    }
                    else if (guess is ImageGuess)
                    {
                        var imageGuess = (ImageGuess) guess;
                        Image image;
                        using (var webClient = new WebClient())
                        {
                            image = Image.FromStream(webClient.OpenRead(imageGuess.Image));
                        }
                        _animatedGifEncoder.AddFrame(image);
                    }
                    count++;
                }

                _animatedGifEncoder.Finish();

                var gifUrl = _cloudImageService.SaveImageToCloud(tempAnimatedGif);
                File.Delete(tempAnimatedGif);
                return gifUrl;
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}
