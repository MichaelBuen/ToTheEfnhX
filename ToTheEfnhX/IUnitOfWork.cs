using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ienablemuch.ToTheEfnhX
{
    public interface IUnitOfWork : IDisposable
    {        
        void Commit();        
    }

    public interface IUnitOfWorkFactory
    {
        IUnitOfWork BeginTransaction();
    }
}
