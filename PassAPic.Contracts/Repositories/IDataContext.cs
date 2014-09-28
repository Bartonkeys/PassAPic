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
        void Commit();
    }
}