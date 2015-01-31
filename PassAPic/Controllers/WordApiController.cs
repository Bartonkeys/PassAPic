using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Ninject;
using PassAPic.Contracts;
using PassAPic.Core.AnimatedGif;
using PassAPic.Core.CloudImage;
using PassAPic.Core.PushRegistration;
using PassAPic.Core.Services;
using PassAPic.Core.WordManager;
using PassAPic.Data;
using PassAPic.Models.Models;
using PassAPic.Models.Models.Models;

namespace PassAPic.Controllers
{
    /// <summary>
    /// Handles word stuff
    /// </summary>
    [RoutePrefix("api/Word")]
    public class WordApiController : BaseController
    {
        protected IWordManager WordManager;
        
        [Inject]
        public WordApiController(IDataContext dataContext, IWordManager wordManager)
        {
            DataContext = dataContext;
            EasyWords = DataContext.EasyWord.Select(x => x.Word).ToList();
            WordManager = wordManager;
            WordManager.DataContext = DataContext;
        }



        // POST /api/word/cleanup
        /// <summary>
        /// Cleanup the words in the DB
        /// </summary>
        /// <returns></returns>
        [Route("Cleanup/{password}")]
        public async Task<HttpResponseMessage> GetCleanup(string password)
        {
            if (password != "Y)rm91234")
            {
                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "You are not authorised to run this");
            }
            try
            {
                var normalWords = DataContext.Word;

                foreach (var normalWord in normalWords)
                {
                    normalWord.word = normalWord.word.Trim();
                    var noOfTimesUsed =
                        DataContext.Game.Count(g => g.StartingWord.Trim().ToLower().Equals(normalWord.word.ToLower()));
                    normalWord.games = noOfTimesUsed;
                }


                var easyWords = DataContext.EasyWord;

                foreach (var easyWord in easyWords)
                {
                    easyWord.Word = easyWord.Word.Trim();
                    var noOfTimesUsed =
                        DataContext.Game.Count(g => g.StartingWord.Trim().ToLower().Equals(easyWord.Word.ToLower()));
                    easyWord.games = noOfTimesUsed;
                }

                DataContext.Commit();

                return Request.CreateResponse(HttpStatusCode.OK, "Cleanup successful");
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }


        // Get /api/word/GetWordViewModels/password/mode/orderByExchanges
        /// <summary>
        ///Get words in DB
        /// </summary>
        /// <returns></returns>
        [Route("GetWordViewModels/{password}/{mode}/{orderByExchanges}")]
        public async Task<HttpResponseMessage> GetWordViewModels(string password, Mode mode, bool orderByExchanges)
        {
            if (password.Equals("Y)rm91234"))
            {

                try
                {
                    var wordViewModels = new List<WordViewModel>();

                    switch (mode)
                    {
                        case Mode.Normal:

                            wordViewModels.AddRange(DataContext.Word.Select(wordItem => new WordViewModel()
                            {
                                Word = wordItem.word,
                                Games = wordItem.games,
                                Exchanges = wordItem.exchanges,
                                Mode = Mode.Normal
                            }));

                            break;
                        case Mode.Easy:

                            wordViewModels.AddRange(DataContext.EasyWord.Select(wordItem => new WordViewModel()
                            {
                                Word = wordItem.Word,
                                Games = wordItem.games,
                                Exchanges = wordItem.exchanges,
                                Mode = Mode.Easy
                            }));

                            break;
                    }
                    if (orderByExchanges)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, wordViewModels.OrderByDescending(w => w.Exchanges));
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, wordViewModels.OrderByDescending(w => w.Games));
                    }
                }

                catch (Exception ex)
                {
                    _log.Error(ex);
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
                }
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "You are not authorised to run this command");
            }
            
        }
    }
}
