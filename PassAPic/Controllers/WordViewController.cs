using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using PassAPic.Models.Models;
using PassAPic.Models.Models.Models;

namespace PassAPic.Controllers
{
    public class WordViewController : BaseMvcController
    {
        // GET: Exchanges
        public async Task<ActionResult> Index()
        {
            
            return View(await GetWordsViewModels(Mode.Normal, true));
        }

        public async Task<ActionResult> NormalByExchanges()
        {
            return View("Index", await GetWordsViewModels(Mode.Normal, true));
        }

        public async Task<ActionResult> EasyByExchanges()
        {

            return View("Index", await GetWordsViewModels(Mode.Easy, true));
        }

        public async Task<ActionResult> NormalByGames()
        {
            return View("Index", await GetWordsViewModels(Mode.Normal, false));
        }

        public async Task<ActionResult> EasyByGames()
        {

            return View("Index", await GetWordsViewModels(Mode.Easy, false));
        }

        private async Task<IEnumerable<WordViewModel>> GetWordsViewModels(Mode mode, bool orderByExchanges)
        {
            string url = BaseUrl;
            switch (mode)
            {
                case Mode.Normal:
                    url += "/api/word/GetWordViewModels/Y)rm91234/Normal/" + orderByExchanges;
                    break;
                case Mode.Easy:
                    url += "/api/word/GetWordViewModels/Y)rm91234/Easy/" + orderByExchanges;
                    break;
            }

            var client = new HttpClient();
            var response = await client.GetAsync(url);

            return (await response.Content.ReadAsAsync<IEnumerable<WordViewModel>>());
        }
    }

    
}