using System;
using System.IO;
using System.Linq;
using JPassAPic.Core.Repositories.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PassAPic.Core.AnimatedGif;
using PassAPic.Core.CloudImage;
using PassAPic.Core.Repositories;
using PassAPic.Repositories.Helpers;

namespace PassAPic.Tests
{
    [TestClass]
    public class AnimatedGifServiceShouldBeAbleTo
    {
        [TestMethod]
        public void CreateAnimaitedGif()
        {
            var unitOfWork = new EFUnitOfWork(new RepositoryProvider(new RepositoryFactories()));

            var latestFinishedGame =
                unitOfWork.Game.SearchFor(x => x.GameOverMan == true).OrderByDescending(x => x.DateCompleted).First();

            var animatedGifService = new AnimatedGifService(new CloudImageService(new CloudImageProviderCloudinary()), unitOfWork);

            var testGifPath = Path.Combine(@"C:\YerMA\PassAPic\PassAPic.Tests\GifResults", latestFinishedGame.Id + ".gif");
            animatedGifService.CreateAnimatedGif(latestFinishedGame.Id, testGifPath);
        }
    }
}
