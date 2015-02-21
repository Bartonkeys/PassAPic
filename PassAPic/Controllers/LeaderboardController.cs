using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using PassAPic.Data;
using PassAPic.Models.Models;

namespace PassAPic.Controllers
{
    public class LeaderboardController : BaseMvcController
    {
        // GET: Leaderboard
        public async Task<ActionResult> Index()
        {
            var client = new HttpClient();
            var response = await client.GetAsync(BaseUrl + "/api/game/getLeaderboard/overall");
            
            var leaderboardModels = await response.Content.ReadAsAsync<List<LeaderboardModel>>();
            
            return View(leaderboardModels);
        }

        // GET: Leaderboard
        public async Task<ActionResult> Split()
        {
            var client = new HttpClient();
            var response = await client.GetAsync(BaseUrl + "/api/game/getLeaderboard/thisweek");

            var leaderboardModels = await response.Content.ReadAsAsync<List<LeaderboardModel>>();

            return View(leaderboardModels);
        }
    }
}