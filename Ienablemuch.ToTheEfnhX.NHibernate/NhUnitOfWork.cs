using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;

namespace Ienablemuch.ToTheEfnhX.NHibernate
{
    public class NhUnitOfWork : IUnitOfWork
    {
        NhSmartSession _ness;
        internal NhUnitOfWork(NhSmartSession session)
        {
            _ness.BeginTransaction();
        }



        public void Commit()
        {
            _ness.Commit();
        }

        public void Dispose()
        {
            if (_ness.TxNestCount > 0)
            {
                _ness.Rollback();
            }

            
        }
    }


    public class NhUnitOfWorkFactory : IUnitOfWorkFactory
    {
        NhSmartSession _ness;
        public NhUnitOfWorkFactory(NhSmartSession session)
        {
            _ness = session;
        }

        internal ISession Session { get { return _ness.Session; } }

        public IUnitOfWork BeginTransaction()
        {
            return new NhUnitOfWork(_ness);
        }
    }
}
