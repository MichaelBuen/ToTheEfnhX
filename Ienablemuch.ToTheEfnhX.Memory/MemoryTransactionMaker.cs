using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ienablemuch.ToTheEfnhX.Memory
{
    public class MemoryTransactionBound : ITransactionBound
    {

        public MemoryTransactionBound()
        {
        }

        public void Complete()
        {
        }

        public void Dispose()
        {
        }
    }


    public class MemoryTransactionBoundFactory : ITransactionBoundFactory
    {
        public ITransactionBound BeginTransaction()
        {
            return new MemoryTransactionBound();
        }
    }
}
