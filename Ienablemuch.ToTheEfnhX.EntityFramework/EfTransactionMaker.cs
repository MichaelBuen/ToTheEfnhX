using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;

namespace Ienablemuch.ToTheEfnhX.EntityFramework
{
    public class EfTransactionBound : ITransactionBound
    {
        TransactionScope txScope = null;
        public EfTransactionBound()
        {
            txScope = new TransactionScope();
        }

        public void Complete()
        {
            txScope.Complete();
        }

        public void Dispose()
        {
            txScope.Dispose();
        }
    }


    public class EfTransactionBoundFactory : ITransactionBoundFactory
    {
        public ITransactionBound BeginTransaction()
        {
            return new EfTransactionBound();
        }
    }
}
