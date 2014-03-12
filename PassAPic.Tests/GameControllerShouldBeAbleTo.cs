using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PassAPic.Data;
using PassAPic.Models;

namespace PassAPic.Tests
{
    /// <summary>
    /// Summary description for GameControllerShouldBeAbleTo
    /// </summary>
    [TestClass]
    public class GameControllerShouldBeAbleTo
    {
        public GameControllerShouldBeAbleTo()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public async Task PlayGame()
        {
            var firstWordDown = new WordModel(); 
            var firstImageDownList = new List<GameBaseModel>();
            var secondWordDownList = new List<GameBaseModel>();
            var secondImageDownList = new List<GameBaseModel>();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://passapic.apphb.com/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                //Game Setup and First Word

                var gameSetup = new GameSetupModel
                {
                    NumberOfPlayers = 4,
                    UserId = 1
                };

                var response = await client.PostAsJsonAsync("api/game/start", gameSetup);
                if (response.IsSuccessStatusCode)
                {
                    firstWordDown = await response.Content.ReadAsAsync<WordModel>();
                }
                else Assert.Fail();

                //First Image

                var firstImageUp = new ImageModel
                {
                    GameId = firstWordDown.GameId,
                    NextUserId = 2,
                    Image = "First Image",
                    UserId = firstWordDown.UserId
                };
                response = await client.PostAsJsonAsync("api/game/guessimage", firstImageUp);
                if (!response.IsSuccessStatusCode) Assert.Fail();

                response = await client.GetAsync("api/game/guesses/" + firstImageUp.NextUserId);
                if (response.IsSuccessStatusCode)
                {
                    firstImageDownList = await response.Content.ReadAsAsync<List<GameBaseModel>>();
                }
                else Assert.Fail();

                //Second Word

                var secondWordUp = new WordModel()
                {
                    GameId = firstImageDownList.Where(x => x.GameId == firstWordDown.GameId).Select(y => y.GameId).FirstOrDefault(),
                    NextUserId = 3,
                    Word = "First Word Up",
                    UserId = firstImageDownList.Where(x => x.GameId == firstWordDown.GameId).Select(y => y.UserId).FirstOrDefault()
                };
                response = await client.PostAsJsonAsync("api/game/guessword", secondWordUp);
                if (!response.IsSuccessStatusCode) Assert.Fail();

                response = await client.GetAsync("api/game/guesses/" + secondWordUp.NextUserId);
                if (response.IsSuccessStatusCode)
                {
                    secondWordDownList = await response.Content.ReadAsAsync<List<GameBaseModel>>();
                }
                else Assert.Fail();

                //Second Image

                var secondImageUp = new ImageModel
                {
                    GameId = secondWordDownList.Where(x => x.GameId == firstWordDown.GameId).Select(y => y.GameId).FirstOrDefault(),
                    NextUserId = 4,
                    Image = "Second Image",
                    UserId = secondWordDownList.Where(x => x.GameId == firstWordDown.GameId).Select(y => y.UserId).FirstOrDefault()
                };
                response = await client.PostAsJsonAsync("api/game/guessimage", secondImageUp);
                if (!response.IsSuccessStatusCode) Assert.Fail();

                response = await client.GetAsync("api/game/guesses/" + secondImageUp.NextUserId);
                if (response.IsSuccessStatusCode)
                {
                    secondImageDownList = await response.Content.ReadAsAsync<List<GameBaseModel>>();
                }
                else Assert.Fail();

                //Final Word

                var finalWordUp = new WordModel()
                {
                    GameId = secondImageDownList.Where(x => x.GameId == firstWordDown.GameId).Select(y => y.GameId).FirstOrDefault(),
                    NextUserId = 455,
                    Word = "final word",
                    UserId = secondImageDownList.Where(x => x.GameId == firstWordDown.GameId).Select(y => y.UserId).FirstOrDefault()
                };
                response = await client.PostAsJsonAsync("api/game/guessword", finalWordUp);
                if (!response.IsSuccessStatusCode) Assert.Fail();

                //Get Results
                response = await client.GetAsync("api/game/results/1");
                if (response.IsSuccessStatusCode)
                {
                    var games = await response.Content.ReadAsAsync<List<ResultsModel>>();
                }


                Assert.IsTrue(true);

            }
        }
    }
}
