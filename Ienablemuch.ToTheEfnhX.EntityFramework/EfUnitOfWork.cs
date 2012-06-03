using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;



namespace Ienablemuch.ToTheEfnhX.EntityFramework
{
    public class EfUnitOfWork : IUnitOfWork
    {

        TransactionScope scope;

        internal EfUnitOfWork()
        {            
            scope = new TransactionScope();
        }


        public void Commit()
        {
            scope.Complete();
        }


        public void Dispose()
        {
            scope.Dispose();
        }
    }

    public class EfUnitOfWorkFactory : IUnitOfWorkFactory
    {
        public IUnitOfWork BeginTransaction()
        {
            return new EfUnitOfWork();
        }
    }
}
