using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;

namespace Ienablemuch.ToTheEfnhX.NHibernate
{


    public class TransactionBound : ITransactionBound
    {
        ISession _session = null;
        public TransactionBound(ISession session)
        {
            _session = session;
            _session.Transaction.Begin();
        }

        public void Complete()
        {
            _session.Transaction.Commit();
        }

        public void Dispose()
        {
            if (!_session.Transaction.WasCommitted && _session.Transaction.IsActive)
                _session.Transaction.Rollback();            
        }
    }

    public class TransactionBoundFactory : ITransactionBoundFactory
    {

        ISession _session = null;
        public TransactionBoundFactory(ISession session)
        {
            _session = session;
        }

        public ITransactionBound BeginTransaction()
        {
            return new TransactionBound(_session);
        }
    }
}
