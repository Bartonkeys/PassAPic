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
        private readonly IUnitOfWork _unitOfWork;

        public AnimatedGifService(CloudImageService cloudImageService, IUnitOfWork unitOfWork)
        {
            _cloudImageService = cloudImageService;
            _unitOfWork = unitOfWork;
        }     

        public string CreateAnimatedGif(int gameId, string tempAnimatedGif)
        {
            try
            {
                var game = _unitOfWork.Game.SearchFor(x => x.Id == gameId).FirstOrDefault();

                using (var magickImageCollection = new MagickImageCollection())
                {
                    var startingWordImage = TextToImageConversion.CreateBitmapImage("Start: " + game.StartingWord);
                    var startingMagickImage = new MagickImage(startingWordImage) {AnimationDelay = 300};
                    //startingMagickImage.Resize(1024, 1024);
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
                            //magickWordImage.Resize(1024, 1024);
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
                            //magickImage.Resize(1024,1024);
                            magickImageCollection.Add(magickImage);
                        }
                        count++;
                    }

                    var settings = new QuantizeSettings {Colors = 256};
                    magickImageCollection.Quantize(settings);

                    //magickImageCollection.Optimize();

                    magickImageCollection.Write(tempAnimatedGif);
                }

                game.AnimatedResult  = _cloudImageService.SaveImageToCloud(tempAnimatedGif, gameId.ToString());
                _unitOfWork.Commit();
                return game.AnimatedResult;
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}
