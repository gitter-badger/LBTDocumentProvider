using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Linq.Expressions;

using LBTUnityT4Linq2SqlOrm;

using LBT_Repository_1909;

namespace LBTDocumentProvider.UnitOfWork
{
    public class LBTLinq2SqlOrmRepository : IRepository
    {
        readonly DataContext _context;
        public LBTLinq2SqlOrmRepository(DataContext lbtDataModelDataContext)
        {
            _context = lbtDataModelDataContext;
        }

        #region IRepository Members

        public IQueryable<T> GetQuery<T>() where T : class,new()
        {

            return _context.GetTable<T>().AsQueryable<T>();
        }

        public void Insert<T>(T entity) where T : class,new()
        {
            _context.GetTable<T>().InsertOnSubmit(entity);
        }

        public void Delete<T>(T entity) where T : class,new()
        {
            _context.GetTable<T>().DeleteOnSubmit(entity);
        }

        public void Update<T>(T entity) where T : class,new()
        {
            _context.GetTable<T>().Attach(entity);
            _context.Refresh(System.Data.Linq.RefreshMode.KeepCurrentValues, entity);
        }

        public T Single<T>(System.Linq.Expressions.Expression<Func<T, bool>> predicate) where T : class,new()
        {
            return _context.GetTable<T>().SingleOrDefault<T>(predicate);
        }

        public T First<T>(System.Linq.Expressions.Expression<Func<T, bool>> predicate) where T : class,new()
        {
            return _context.GetTable<T>().FirstOrDefault<T>(predicate);
        }

        public IQueryable GetAll(Type type)
        {
            return _context.GetTable(type).AsQueryable();

        }

        public IEnumerable<string> GetCustomQuery(string query, params object[] parameters)
        {
            return _context.ExecuteQuery<string>(query, parameters);
        }

        #endregion
    }
}