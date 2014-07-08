using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using PassAPic.Contracts;
using PassAPic.Data;

namespace PassAPic.Core.Repositories
{
    class EFUserRepository: EFRepository<User>, IUserRepository
    {
        public EFUserRepository(DbContext dbContext) : base(dbContext)
        {
        }

        public int GetUserId(string name)
        {
            throw new NotImplementedException();
        }

        public User GetUser(string userName)
        {
            throw new NotImplementedException();
        }
    }
}
