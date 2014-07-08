using System;
using PassAPic.Data;

namespace PassAPic.Contracts
{
    public interface IUserRepository: IRepository<User>
    {
        int GetUserId(string name);
        User GetUser(string userName);
    }
}