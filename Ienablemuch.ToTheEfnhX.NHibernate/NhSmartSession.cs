using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;

namespace Ienablemuch.ToTheEfnhX.NHibernate
{
    
    public class NhSmartSession 
    {
        ISession _sess;
        ITransaction _tx;

        internal int TxNestCount { get; set; }

        public NhSmartSession(ISession sess)
        {
            _sess = sess;
        }

        public ISession Session
        {
            get
            {
                return _sess;
            }
        }

        public ITransaction BeginTransaction()
        {
            if (TxNestCount == 0)
                _tx = _sess.BeginTransaction();

            ++TxNestCount;

            return _tx;            
        }

        public void Commit()
        {
            if (TxNestCount == 0)
                throw new Exception("Commit is not balance with Transaction count");

            --TxNestCount;

            if (TxNestCount == 0)
                _tx.Commit();

        }

        public void Rollback()
        {
            if (TxNestCount == 0)
                throw new Exception("Rollback is not balance with Transaction count");

            --TxNestCount;

            if (TxNestCount == 0)
                _tx.Rollback();

        }
    }
}
