using System.Data.Entity;
using PassAPic.Data;

namespace PassAPic.Contracts
{
    public interface IDataContext
    {
        IDbSet<User> User { get; }
        IDbSet<Game> Game { get; }
        IDbSet<Guess> Guess { get; }
        IDbSet<Word> Word { get; }
        IDbSet<EasyWord> EasyWord { get; }
        IDbSet<PushRegister> PushRegister { get; }
        IDbSet<Game_Comments> Comment { get; }
        IDbSet<Game_Scoring> Score { get; }
        IDbSet<Leaderboard> Leaderboard { get; }
        IDbSet<LeaderboardSplit> LeaderboardSplit { get; } 

        void Commit();
    }
}