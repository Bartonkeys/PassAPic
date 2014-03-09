using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Objects;
using System.Linq;
using System.Text;
using PassAPic.Contracts;

namespace PassAPic.Core.Repositories
{
    public class EFRepository<T>: IRepository<T> where T: class 
    {
        protected IDbSet<T> DbSet { get; set; }
        protected DbContext DbContext { get; set; }

        public EFRepository(DbContext dbContext)
        {
            if (dbContext == null)
                throw new ArgumentNullException("dbContext");
            DbContext = dbContext;
            DbSet = DbContext.Set<T>();
        }

        public virtual void Insert(T entity)
        {
            DbSet.Add(entity);
        }

        public virtual void Delete(T entity)
        {
            DbSet.Remove(entity);
        }

        public virtual void Delete(int id)
        {
            var entityToDelete = DbSet.Find(id);
            DbSet.Remove(entityToDelete);
        }


        public virtual void Update(T entity)
        {
            DbSet.Attach(entity);
            DbContext.Entry(entity).State = EntityState.Modified;
        }

        public virtual IQueryable<T> SearchFor(System.Linq.Expressions.Expression<Func<T, bool>> predicate)
        {
            return DbSet.Where(predicate);
        }

        public virtual IQueryable<T> GetAll()
        {
            return DbSet.AsQueryable();
        }

        public virtual T GetById(object id)
        {
            return DbSet.Find(id);
        }
    }
}
