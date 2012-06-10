using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;

namespace Ienablemuch.ToTheEfnhX.EntityFramework
{
    public class TransactionBound : ITransactionBound
    {
        TransactionScope txScope = null;
        public TransactionBound()
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


    public class TransactionBoundFactory : ITransactionBoundFactory
    {
        public ITransactionBound BeginTransaction()
        {
            return new TransactionBound();
        }
    }
}
