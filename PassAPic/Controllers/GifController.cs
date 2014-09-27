using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Ninject;
using PassAPic.Contracts;
using PassAPic.Core.AnimatedGif;
using PassAPic.Core.CloudImage;
using PassAPic.Core.PushRegistration;

namespace PassAPic.Controllers
{
    [RoutePrefix("api/gif")]
    public class GifController : BaseController
    {

        protected CloudImageService CloudImageService;
        protected AnimatedGifService AnimatedGifService;

        [Inject]
        public GifController(IDataContext dataContext, ICloudImageProvider cloudImageProvider)
        {
            CloudImageService = new CloudImageService(cloudImageProvider);
            AnimatedGifService = new AnimatedGifService(CloudImageService, dataContext);
            DataContext = dataContext;
        }

        /// GET /api/gif
        /// <summary>
        /// Create animated Gif for game and return url
        /// </summary>
        /// <returns></returns>
        [Route("{gameId}")]
        public async Task<HttpResponseMessage> GetAnimatedGif(int gameId)
        {
            try
            {
                var game = DataContext.Game.Find(gameId);
                if (game.AnimatedResult != null && !game.AnimatedResult.Equals("")) return Request.CreateResponse(HttpStatusCode.OK, game.AnimatedResult);

                var tempAnimatedGif = HttpContext.Current.Server.MapPath("~/App_Data/" + gameId + ".gif");
                var animatedGifPath = await Task.Run(() => AnimatedGifService.CreateAnimatedGif(gameId, tempAnimatedGif));
                
                return Request.CreateResponse(HttpStatusCode.OK, animatedGifPath);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
        }
}
