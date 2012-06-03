using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ienablemuch.ToTheEfnhX
{

    public interface ITransactionBound : IDisposable
    {
        void Complete();
    }

    public interface ITransactionBoundFactory
    {
        ITransactionBound BeginTransaction();   
    }
}
