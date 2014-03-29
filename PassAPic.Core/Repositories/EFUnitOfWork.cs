using System;
using System.Collections.Generic;
using System.Data.Entity;
using PassAPic.Contracts;
using PassAPic.Data;
using PassAPic.Repositories.Helpers;

namespace PassAPic.Core.Repositories
{
    public class EFUnitOfWork: IUnitOfWork, IDisposable
    {
        public DbContext PassAPicModel { get; set; }

        public IUserRepository User { get { return GetRepo<IUserRepository>(); } }
        public IRepository<Game> Game { get { return GetStandardRepository<Game>(); } }
        public IRepository<Guess> Guess { get { return GetStandardRepository<Guess>(); } }
        public IRepository<Word> Word { get { return GetStandardRepository<Word>(); } }
        public IRepository<PushRegister> PushRegister { get { return GetStandardRepository<PushRegister>(); } }

        private readonly IRepositoryProvider _repositoryProvider;

        public EFUnitOfWork(IRepositoryProvider repositoryProvider)
        {
            CreateDbContext();

            _repositoryProvider = repositoryProvider;
            repositoryProvider.DbContext = PassAPicModel;
        }

        private void CreateDbContext()
        {
            PassAPicModel = new PassAPicModelContainer();
            PassAPicModel.Configuration.LazyLoadingEnabled = true;
        }


        private IRepository<T> GetStandardRepository<T>() where T : class
        {
            return _repositoryProvider.GetRepositoryForEntityType<T>();
        }

        private T GetRepo<T>() where T : class
        {
            return _repositoryProvider.GetRepository<T>();
        }

        public void Commit()
        {
            PassAPicModel.SaveChanges();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (PassAPicModel != null)
                {
                    PassAPicModel.Dispose();
                }
            }
        }
    }
}
