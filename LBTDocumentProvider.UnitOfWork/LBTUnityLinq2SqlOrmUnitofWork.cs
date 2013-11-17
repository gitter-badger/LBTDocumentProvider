
using System;

using LBTUnityT4Linq2SqlOrm;

using LBT_Repository_1909;

namespace LBTDocumentProvider.UnitOfWork
{
    public class LBTUnityLinq2SqlOrmUnitofWork : IUnitOfWork
    {
        #region IUnitOfWork Members
        LBTDataModelDataContext _context;
        private IRepository _repository;
        public LBTUnityLinq2SqlOrmUnitofWork()
        {
            _context = new LBTDataModelDataContext(@"Data Source=LBTMSQL;Initial Catalog=LBTUnity1;Integrated Security=True;MultipleActiveResultSets=True;")
            {
                ObjectTrackingEnabled = false
            };
        }

        public IRepository Repository
        {
            get { return _repository ?? (_repository = new LBTLinq2SqlOrmRepository(_context)); }
        }

        public void SaveChanges()
        {
            _context.SubmitChanges();
        }

        #endregion




        #region IDisposable Members
        ~LBTUnityLinq2SqlOrmUnitofWork()
        {
            Dispose(false);
        }

        private Boolean disposed;

        private void Dispose(Boolean disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                if (_context != null)
                {
                    _context.Dispose();
                    _repository = null;
                    _context = null;
                }
            }

            disposed = true;
        }

        public void Dispose()
        {

            this.Dispose(true);

            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
