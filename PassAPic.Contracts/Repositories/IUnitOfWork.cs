using System.Data.Entity;
using PassAPic.Data;

namespace PassAPic.Contracts
{
    public interface IUnitOfWork
    {
        DbContext PassAPicModel { get; set; }

        IUserRepository User { get; }
        IRepository<Game> Game { get; }
        IRepository<Guess> Guess { get; }
        IRepository<Word> Word { get; }
        IRepository<PushRegister> PushRegister { get; }
        void Commit();
    }
}