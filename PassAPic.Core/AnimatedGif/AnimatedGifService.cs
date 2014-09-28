using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using ImageMagick;
using PassAPic.Contracts;
using PassAPic.Core.CloudImage;
using PassAPic.Data;


namespace PassAPic.Core.AnimatedGif
{
    public class AnimatedGifService
    {
        private readonly CloudImageService _cloudImageService;
        private readonly IDataContext _dataContext;

        public AnimatedGifService(CloudImageService cloudImageService, IDataContext dataContext)
        {
            _cloudImageService = cloudImageService;
            _dataContext = dataContext;
        }     

        public string CreateAnimatedGif(int gameId, string tempAnimatedGif)
        {
            try
            {
                var game = _dataContext.Game.FirstOrDefault(x => x.Id == gameId);

                using (var magickImageCollection = new MagickImageCollection())
                {
                    var startingWordImage = TextToImageConversion.CreateBitmapImage("Start: " + game.StartingWord);
                    var startingMagickImage = new MagickImage(startingWordImage) {AnimationDelay = 300};
                    magickImageCollection.Add(startingMagickImage);

                    var count = 1;
                    foreach (var guess in game.Guesses)
                    {
                        if (guess is WordGuess)
                        {
                            var wordGuess = (WordGuess)guess;

                            String word;
                            if (game.Guesses.Count == count)
                                word = "Final: " + wordGuess.Word;
                            else
                                word = count + ": " + wordGuess.Word;

                            var magickWordImage = new MagickImage(TextToImageConversion.CreateBitmapImage(word))
                            {
                                AnimationDelay = 300
                            };
                            magickImageCollection.Add(magickWordImage);
                        }
                        else if (guess is ImageGuess)
                        {
                            var imageGuess = (ImageGuess)guess;
                            Image image;
                            using (var webClient = new WebClient())
                            {
                                image = Image.FromStream(webClient.OpenRead(imageGuess.Image));
                            }

                            var magickImage = new MagickImage((Bitmap) image)
                            {
                                AnimationDelay = 300
                            };
                            magickImageCollection.Add(magickImage);
                        }
                        count++;
                    }

                    var settings = new QuantizeSettings {Colors = 256};
                    foreach (var magickImage in magickImageCollection)
                    {
                        MagickGeometry geometry = new MagickGeometry(1024, 1024);
                        geometry.IgnoreAspectRatio = true; 
                        magickImage.Resize(geometry);

                        //magickImage.Resize(new MagickGeometry("1024x1024!"));
                    }
                    magickImageCollection.Quantize(settings);

                    //We can't optimize if the images are not all the same dimensions
                    magickImageCollection.Optimize();

                    magickImageCollection.Write(tempAnimatedGif);
                }

                game.AnimatedResult  = _cloudImageService.SaveImageToCloud(tempAnimatedGif, gameId.ToString()+".gif");
                _dataContext.Commit();
                return game.AnimatedResult;
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}
