using System;
using System.Linq;
using System.Linq.Expressions;

namespace PassAPic.Contracts
{
    public interface IRepository<TEntity>
    {
        void Insert(TEntity entity);
        void Delete(TEntity entity);
        void Update(TEntity entity);
        IQueryable<TEntity> SearchFor(Expression<Func<TEntity, bool>> predicate);
        IQueryable<TEntity> GetAll();
        TEntity GetById(object id);
        void Delete(int id);
    }
}