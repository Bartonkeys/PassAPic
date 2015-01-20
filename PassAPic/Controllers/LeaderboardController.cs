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
            var response = await client.GetAsync(BaseUrl + "/api/game/getLeaderboard");
            var leaderboardItems = await response.Content.ReadAsAsync<List<Leaderboard>>();

            var leaderboardModels = new List<LeaderboardModel>();
            foreach (var leaderboardItem in leaderboardItems)
            {
                if (leaderboardItem.TotalScore != null)
                    leaderboardModels.Add(new LeaderboardModel()
                        {
                            UserName = leaderboardItem.Username,
                            TotalScore = (int) leaderboardItem.TotalScore

                        });
            }
            return View(leaderboardModels.OrderByDescending(l => l.TotalScore));
        }
    }
}